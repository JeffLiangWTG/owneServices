using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarFrcConsignmentUpdater
	{
		public CuscarFrcConsignmentUpdater(EDIMessage inboundEdiMessageForAuditing, ILogger serviceLogger, CuscarWithFlagsToShowWhatsSet flagableCuscar)
		{
			this.inboundMsg = inboundEdiMessageForAuditing;
			this.serviceLogger = serviceLogger;
			this.flagableCuscar = flagableCuscar;
		}

		internal bool HandleUpdateOfConsignment()
		{
			var awb = CuscarParserAndProcessor.GetExistingAwbFromInboundCuscarReferenceNumbers(flagableCuscar, inboundMsg.Factory);
			inboundMsg.EM_MessageSubType = CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode;
			if (awb != null)
			{
				inboundMsg.EM_Status = EDIMessage.Status.Received;
				bool isNewNotUpdated = false;
				if (!(awb is SplitConsignment) && flagableCuscar.SplitReferenceSet && !flagableCuscar.SplitReference.IsEmpty)
				{
					//  Awb found is a whole awb but the message refers to a split.  This can happen if the agent was nominated for the whole awb, then the shed changed to another agent (FRC received advising of this), then the shed split the AWB (no info 
					// to first agent), then the shed nominated just one split back to first agent (FRC at split level recevied now).   We need to ensure that we add a new split.
					if (awb is CusMAWB mawb && mawb.ChildBills.Count > 0)
					{
						serviceLogger.Log(LogType.Error, delegate
						{ return string.Format("Inbound FRC message #{1}, cannot add split to MAWB {0} which has child bills", awb.ReferenceNumber, inboundMsg.EM_MessageNum); });
						return false;
					}
					else
					{
						awb = InsertAndUpdateSplitForNewlyNominatedAgent(awb, flagableCuscar, inboundMsg);
						isNewNotUpdated = true;
						serviceLogger.Log(LogType.Information, delegate
						{ return string.Format("Inbound FRC message #{1}, found parent consignment and added split {0}, about to update it", awb.ReferenceNumber, inboundMsg.EM_MessageNum); });
					}
				}
				else
				{
					serviceLogger.Log(LogType.Information, delegate
					{ return string.Format("Inbound FRC message #{1}, found consignment {2}{3}{0}, about to update it", awb.ReferenceNumber, inboundMsg.EM_MessageNum, awb.CargoTerminalOperator, awb.CargoTerminalOperatorAirport); });
				}
				string messageInterpretation = null;
				var piecesReceivedBeforeUpdate = awb.NumberOfPiecesReceived;
				var nominatedAgentBeforeUpdate = awb.AgentBadge;
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				if (awb is CusMAWB)
				{
					messageInterpretation = UpdateUsingCuscarData_Mawb(awb as CusMAWB, flagableCuscar, ColumnsToShowInInterpretation.FieldNameAndBothValues, false);
				}
				else if (awb is CusHAWB)
				{
					messageInterpretation = UpdateUsingCuscarData_Hawb(awb as CusHAWB, flagableCuscar, ColumnsToShowInInterpretation.FieldNameAndBothValues, false);
				}
				else if (awb is SplitConsignment)
				{
					inboundMsg.EM_ApplicationReference = flagableCuscar.SplitReference;
					messageInterpretation = UpdateUsingCuscarData_Split(awb as SplitConsignment, flagableCuscar, isNewNotUpdated ? ColumnsToShowInInterpretation.FieldNameAndPersistentValue : ColumnsToShowInInterpretation.FieldNameAndBothValues, isNewNotUpdated);
				}
				MaybeSetNprOnSplits(awb);
				awb.Messages.Add(inboundMsg);
				inboundMsg.EM_GB = awb.Branch.PK;
				inboundMsg.EM_MessageInterpretation = string.Format("{0} <h3>Record {4}{5} {1} {2} using community data</h3> {3}", MessagePrettierCss.CSS, awb.ReferenceNumber, (isNewNotUpdated ? "inserted" : "updated"), messageInterpretation, awb.CargoTerminalOperatorAirport, awb.CargoTerminalOperator);
				MaybeAutomaticallyPrintC1FromFrc(awb, piecesReceivedBeforeUpdate);
				MaybeArchiveIfNoLongerOurConsignment(awb, nominatedAgentBeforeUpdate, inboundMsg);
			}
			else
			{
				serviceLogger.Log(LogType.Information, delegate
				{ return string.Format("Inbound FRC message #{1}, consignment {2}{3}{0} not found, inserting new record instead", flagableCuscar.AirWaybillSerialNumber + flagableCuscar.HouseAirWaybillNumber, inboundMsg.EM_MessageNum, flagableCuscar.AirportOfArrival, flagableCuscar.CargoTerminalOperator); });
				var insertedAwb = new CuscarFriConsignmentInserter(inboundMsg, serviceLogger, flagableCuscar, CcsukTransmissionMessageFunction.CUSCAR.FRC.Subcode).HandleInsertionOfConsignment();
				InsertAndUpdateSplitForNewlyNominatedAgent(insertedAwb, flagableCuscar, inboundMsg);
			}
			return true;  // we understood the message even if it talked of unknown consignments
		}

		void MaybeArchiveIfNoLongerOurConsignment(ICcsukCusAwb awb, ZString nominatedAgentBeforeUpdate, EDIMessage inboundMessage)
		{
			var isStillNominatedToOneOfOurBadgesOrAtOneOfOurSheds = FsaJobUpdater.JobCanBeUpdatedAccordingToRegistryCredentials(awb, awb.CargoTerminalOperatorAirport + awb.CargoTerminalOperator, awb.AgentBadge, inboundMsg);
			if (nominatedAgentBeforeUpdate != awb.AgentBadge) //agent has changed, but it maay have changed to another of our badges. 
			{
				if (!isStillNominatedToOneOfOurBadgesOrAtOneOfOurSheds && GBCustomsDataRegistry.Instance.CcsukMarkAsArchivedWhenNoLongerOurConsignment.Value)
				{
					// The FRC advised that the job now belongs to another badge and that badge is not one of ours
					awb.ArchiveOnCcsuk(ReasonForArchiving.NoLongOurConsignment);
					inboundMsg.EM_MessageInterpretation += "<p>This consignment no longer belongs to any of your badges and so has been marked as 'archived'</p>";
				}
			}
			else
			{
				// Agent unchanged by message but need to ensure the PIMA is correct if still our job, so long as the current shed in the mesage is not our shed
				if (inboundMsg.Interchange != null)
				{
					var recipientPimaOfMessage = inboundMsg.Interchange.EI_To.Replace("/", "");
					if (isStillNominatedToOneOfOurBadgesOrAtOneOfOurSheds && awb.Profile != recipientPimaOfMessage && recipientPimaOfMessage.EndsWith(awb.AgentBadge))
					{
						if (!FsaJobUpdater.IsOurShedAccordingToRegistryCredentials(awb.CargoTerminalOperatorAirport + awb.CargoTerminalOperator, awb.Branch))
						{
							awb.ProfileInfo.Value = recipientPimaOfMessage;
						}
					}
				}
			}
		}

		ICcsukCusAwb InsertAndUpdateSplitForNewlyNominatedAgent(ICcsukCusAwb insertedAwb, CuscarWithFlagsToShowWhatsSet flagableCuscar, EDIMessage inboundMsg)
		{
			if (flagableCuscar.SplitReferenceSet)
			{
				var newSplit = insertedAwb.Splits.AddNew();
				newSplit.SplitReference = flagableCuscar.SplitReference;
				newSplit.AgentBadge = flagableCuscar.AgentBrokerConsolidatorCode;  // set explicitly, do not inherit from parent
				newSplit.NumberOfPiecesExpected = flagableCuscar.NumberOfPiecesExpected;
				newSplit.NumberOfPiecesReceived = flagableCuscar.NumberOfPiecesReceived;
				newSplit.Weight = flagableCuscar.Weight;
				if (flagableCuscar.Status1DateSet)
				{
					newSplit.Status1Date = flagableCuscar.Status1Date;
				}
				inboundMsg.EM_MessageInterpretation = "<h3>Split " + flagableCuscar.SplitReference + "</h3>" + inboundMsg.EM_MessageInterpretation;
				IncrementExistingParentAwbsPieceCountToMatchItsSplits(insertedAwb);
				return newSplit;
			}
			return null;
		}

		static void IncrementExistingParentAwbsPieceCountToMatchItsSplits(ICcsukCusAwb existingParentAwb)
		{
			if (((BusinessObject)existingParentAwb).IsInDatabase)
			{
				ZShort npr = 0;
				ZShort npx = 0;
				foreach (SplitConsignment s in existingParentAwb.Splits)
				{
					npr += s.NumberOfPiecesReceived;
					npx += s.NumberOfPiecesExpected;
				}
				existingParentAwb.NumberOfPiecesExpected = npx;
				existingParentAwb.NumberOfPiecesReceived = npr;
			}
		}

		void MaybeSetNprOnSplits(ICcsukCusAwb wholeAwb)
		{
			if (wholeAwb.HasSplits)
			{
				if (wholeAwb.NumberOfPiecesExpected == wholeAwb.NumberOfPiecesReceived)
				{
					if (GBCustomsDataRegistry.Instance.CcsukAllocationOfNPR.Value)
					{
						foreach (SplitConsignment split in wholeAwb.Splits)
						{
							split.NumberOfPiecesReceived = split.NumberOfPiecesExpected;
						}
					}
				}
			}
		}

		internal static string UpdateUsingCuscarData_Mawb(CusMAWB mawb, CuscarWithFlagsToShowWhatsSet newData, ColumnsToShowInInterpretation columnStyle, bool isNewNotUpdated, bool sendEmailToo = true)
		{
			var mapOfProperties = GetMawbMembers(mawb, newData, isNewNotUpdated);
			var listOfPropertiesUpdated = UpdateProperties(mapOfProperties, mawb, newData.CommunityHandlingCodes);
			return FormatChangedPropertiesAndCreateMessageInterpretationAndEmailUser(listOfPropertiesUpdated, columnStyle, isNewNotUpdated, mawb, sendEmailToo);
		}

		internal static Dictionary<ZPropertyInfo, IZType> GetMawbMembers(CusMAWB mawb, CuscarWithFlagsToShowWhatsSet newData, bool isNew)
		{
			var mapOfProperties = new Dictionary<ZPropertyInfo, IZType>();
			if (isNew && newData.AirWaybillSerialNumberSet)
			{
				mapOfProperties.Add(mawb.CM_MAWBInfo, new ZString(newData.AirlinePrefix + newData.AirWaybillSerialNumber));
			}

			if (newData.AgentBrokerConsolidatorCodeSet)
			{
				mapOfProperties.Add(mawb.AgentBadgeInfo, newData.AgentBrokerConsolidatorCode);
			}

			if (newData.AirportOfArrivalSet)
			{
				mapOfProperties.Add(mawb.AirportOfArrivalInfo, newData.AirportOfArrival);
			}

			if (newData.AirportOfDestinationSet)
			{
				mapOfProperties.Add(mawb.AirportOfDestinationInfo, newData.AirportOfDestination);
			}

			if (newData.AirportOfOriginSet)
			{
				mapOfProperties.Add(mawb.AirportOfOriginInfo, PortConverter.IataToUnloco(newData.AirportOfOrigin, mawb.Factory));
			}

			if (newData.DateOfFlightArrivalSet)
			{
				mapOfProperties.Add(mawb.CM_ArrivalDateInfo, newData.DateOfFlightArrival);
			}

			if (newData.CargoTerminalOperatorSet)
			{
				mapOfProperties.Add(mawb.CargoTerminalOperatorInfo, newData.CargoTerminalOperator);
			}

			if (newData.DescriptionOfGoodsSet)
			{
				mapOfProperties.Add(mawb.DescriptionOfGoodsInfo, newData.DescriptionOfGoods);
			}

			if (newData.FlightNumberSet || newData.CarrierCodeSet)
			{
				mapOfProperties.Add(mawb.CM_FlightNoInfo, new ZString(newData.CarrierCode + newData.FlightNumber));
			}

			if (newData.WeightSet)
			{
				mapOfProperties.Add(mawb.WeightInfo, newData.Weight);
			}

			if (newData.WeightCodeSet)
			{
				mapOfProperties.Add(mawb.WeightCodeInfo, newData.WeightCode);
			}

			if (newData.NumberOfPiecesExpectedSet)
			{
				mapOfProperties.Add(mawb.NumberOfPiecesExpectedInfo, newData.NumberOfPiecesExpected);
			}

			if (newData.NumberOfPiecesReceivedSet)
			{
				mapOfProperties.Add(mawb.NumberOfPiecesReceivedInfo, newData.NumberOfPiecesReceived);
			}

			if (newData.ShipmentDescriptionCodeSet)
			{
				mapOfProperties.Add(mawb.ShipmentDescriptionCodeInfo, newData.ShipmentDescriptionCode);
			}

			if (newData.Status2IndicatorSet)
			{
				mapOfProperties.Add(mawb.Status2GrantedInfo, newData.Status2Indicator);
			}

			if (newData.Status1DateSet)
			{
				mapOfProperties.Add(mawb.Status1DateInfo, newData.Status1Date);
			}

			return mapOfProperties;
		}

		static Dictionary<ZPropertyInfo, IZType> UpdateProperties(Dictionary<ZPropertyInfo, IZType> mapOfProperties, ICcsukCusAwb awb, List<ZString> communityHandlingCodes)
		{
			var listOfPropertiesUpdated = new Dictionary<ZPropertyInfo, IZType>();
			foreach (var propertyInfo in mapOfProperties.Keys)
			{
				var newValue = mapOfProperties[propertyInfo];
				var oldValue = propertyInfo.Value;
				listOfPropertiesUpdated.Add(propertyInfo, oldValue);
				try
				{
					propertyInfo.Value = newValue;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Property we were tyring to set was bad, e.g. too long, KGM --> KG field. Ignore the exception and continue to set other members. Report.
					ErrorReporter.ReportOnce("BGB-CUK-FRC",
											string.Format("Could not set property on CCSUK CusAwb using inbound data from community. Ignoring and continuing, but please check data. Old value='{0}'; New (bad) value='{1}'; PropertyInfo.Name='{2}'.",
											oldValue, newValue, propertyInfo.Name)
											, ex);
				}
			}

			// Set new CHCs here....
			SynchroniseCommunityHandlingCodes(communityHandlingCodes, awb.CommunityHandlingCodes, listOfPropertiesUpdated, awb.SplitReference);

			return listOfPropertiesUpdated;
		}

		internal static void SynchroniseCommunityHandlingCodes(List<ZString> communityHandlingCodesToSet, CusAddInfoCollection<CommunityHandlingCode> handlingCodesAlreadyExisting, Dictionary<ZPropertyInfo, IZType> listOfPropertiesUpdated, ZString splitReferenceForAssigningAgainstNewChc)
		{
			foreach (CusAddInfo<CommunityHandlingCode> existing in handlingCodesAlreadyExisting.ToArray())
			{
				var existingCode = existing.Data.C4_CommunityHandlingCode;
				if (!communityHandlingCodesToSet.Contains(existingCode))
				{
					existing.Data.C4_CommunityHandlingCodeInfo.Value = ZString.Empty;
					if (listOfPropertiesUpdated != null)
					{
						listOfPropertiesUpdated.Add(existing.Data.C4_CommunityHandlingCodeInfo, existingCode);
					}
					existing.Delete();
				}
				else if (listOfPropertiesUpdated != null)
				{
					// match
					listOfPropertiesUpdated.Add(existing.Data.C4_CommunityHandlingCodeInfo, existingCode);
				}
			}
			if (communityHandlingCodesToSet != null)
			{
				foreach (var toSet in communityHandlingCodesToSet)
				{
					if (!handlingCodesAlreadyExisting.OfType<CusAddInfo<CommunityHandlingCode>>().Any(x => x.Data.C4_CommunityHandlingCode == toSet))
					{
						var newPersistentItem = handlingCodesAlreadyExisting.AddNew();
						newPersistentItem.Data.C4_CommunityHandlingCodeInfo.Value = ZString.Empty;
						newPersistentItem.Data.C4_SplitReferenceToWhichThisPertains = splitReferenceForAssigningAgainstNewChc;
						newPersistentItem.Data.C4_CommunityHandlingCodeInfo.Value = toSet;
						if (listOfPropertiesUpdated != null)
						{
							listOfPropertiesUpdated.Add(newPersistentItem.Data.C4_CommunityHandlingCodeInfo, ZString.Empty);
						}
					}
				}
			}
		}

		internal static string UpdateUsingCuscarData_Split(SplitConsignment split, CuscarWithFlagsToShowWhatsSet newData, ColumnsToShowInInterpretation columnsStyle, bool isNewNotUpdated)
		{
			var mapOfProperties = GetSplitMembers(split, newData, isNewNotUpdated);
			var listOfPropertiesUpdated = UpdateProperties(mapOfProperties, split, newData.CommunityHandlingCodes);
			return FormatChangedPropertiesAndCreateMessageInterpretationAndEmailUser(listOfPropertiesUpdated, columnsStyle, isNewNotUpdated, split);
		}

		internal static string UpdateUsingCuscarData_Hawb(CusHAWB hawb, CuscarWithFlagsToShowWhatsSet newData, ColumnsToShowInInterpretation columnsStyle, bool isNewNotUpdated)
		{
			var mapOfProperties = GetHawbMembers(hawb, newData, isNewNotUpdated);
			var listOfPropertiesUpdated = UpdateProperties(mapOfProperties, hawb, newData.CommunityHandlingCodes);
			return FormatChangedPropertiesAndCreateMessageInterpretationAndEmailUser(listOfPropertiesUpdated, columnsStyle, isNewNotUpdated, hawb);
		}

		internal static Dictionary<ZPropertyInfo, IZType> GetSplitMembers(SplitConsignment split, CuscarWithFlagsToShowWhatsSet newData, bool isNew)
		{
			var mapOfProperties = new Dictionary<ZPropertyInfo, IZType>();
			if (newData.NumberOfPiecesExpectedSet)
			{
				mapOfProperties.Add(split.NumberOfPiecesExpectedInfo, newData.NumberOfPiecesExpected);
			}

			if (newData.NumberOfPiecesReceivedSet)
			{
				mapOfProperties.Add(split.NumberOfPiecesReceivedInfo, newData.NumberOfPiecesReceived);
			}

			if (newData.WeightSet)
			{
				mapOfProperties.Add(split.WeightInfo, newData.Weight);
			}

			if (newData.WeightCodeSet)
			{
				mapOfProperties.Add(split.WeightCodeInfo, newData.WeightCode);
			}

			if (newData.AgentBrokerConsolidatorCodeSet)
			{
				mapOfProperties.Add(split.AgentBadgeInfo, newData.AgentBrokerConsolidatorCode);
			}

			if (newData.Status1DateSet)
			{
				mapOfProperties.Add(split.Status1DateInfo, newData.Status1Date);
			}

			return mapOfProperties;
		}

		internal static Dictionary<ZPropertyInfo, IZType> GetHawbMembers(CusHAWB hawb, CuscarWithFlagsToShowWhatsSet newData, bool isNew)
		{
			var mapOfProperties = new Dictionary<ZPropertyInfo, IZType>();
			if (isNew && newData.HouseAirWaybillNumberSet)
			{
				mapOfProperties.Add(hawb.CS_HAWBInfo, newData.HouseAirWaybillNumber);
			}

			if (newData.AgentBrokerConsolidatorCodeSet)
			{
				mapOfProperties.Add(hawb.CS_ResponsiblePartyIDInfo, newData.AgentBrokerConsolidatorCode);
			}

			if (newData.AirportOfArrivalSet)
			{
				mapOfProperties.Add(hawb.AirportOfArrivalInfo, newData.AirportOfArrival);
			}

			if (newData.AirportOfDestinationSet)
			{
				mapOfProperties.Add(hawb.AirportOfDestinationInfo, newData.AirportOfDestination);
			}

			if (newData.AirportOfOriginSet)
			{
				mapOfProperties.Add(hawb.AirportOfOriginInfo, PortConverter.IataToUnloco(newData.AirportOfOrigin, hawb.Factory));
			}

			if (newData.CargoTerminalOperatorSet)
			{
				mapOfProperties.Add(hawb.CargoTerminalOperatorInfo, newData.CargoTerminalOperator);
			}

			if (newData.DescriptionOfGoodsSet)
			{
				mapOfProperties.Add(hawb.CS_GoodsDescriptionInfo, newData.DescriptionOfGoods);
			}

			if (newData.ShipmentDescriptionCodeSet)
			{
				mapOfProperties.Add(hawb.ShipmentDescriptionCodeInfo, newData.ShipmentDescriptionCode);
			}

			if (newData.NumberOfPiecesExpectedSet)
			{
				mapOfProperties.Add(hawb.CS_PiecesManifestedInfo, newData.NumberOfPiecesExpected);
			}

			if (newData.NumberOfPiecesReceivedSet)
			{
				mapOfProperties.Add(hawb.CS_PiecesLandedInfo, newData.NumberOfPiecesReceived);
			}

			if (newData.WeightSet)
			{
				mapOfProperties.Add(hawb.CS_WeightInfo, newData.Weight);
			}

			if (newData.WeightCodeSet)
			{
				mapOfProperties.Add(hawb.CS_WeightUQInfo, newData.WeightCode);
			}

			if (isNew && (newData.FlightNumberSet || newData.CarrierCodeSet))
			{
				mapOfProperties.Add(hawb.MAWB.CM_FlightNoInfo, new ZString(newData.CarrierCode + newData.FlightNumber));
			}

			if (isNew && newData.DateOfFlightArrivalSet)
			{
				mapOfProperties.Add(hawb.MAWB.CM_ArrivalDateInfo, newData.DateOfFlightArrival);
			}

			if (newData.Status2IndicatorSet)
			{
				mapOfProperties.Add(hawb.Status2GrantedInfo, newData.Status2Indicator);
			}

			if (newData.Status1DateSet)
			{
				mapOfProperties.Add(hawb.Status1DateInfo, newData.Status1Date);
			}

			return mapOfProperties;
		}

		static string FormatChangedPropertiesAndCreateMessageInterpretationAndEmailUser(Dictionary<ZPropertyInfo, IZType> propertiesUpdatedAndOldValueHash, ColumnsToShowInInterpretation columnStyle, bool isNew, ICcsukCusAwb job, bool sendEmailToo = true)
		{
			var html = MakeHtmlTableFromListOfProperties(propertiesUpdatedAndOldValueHash, columnStyle);
			if (sendEmailToo)
			{
				var createdOrUpdated = isNew ? "created" : "updated";
				var subject = string.Format("{0} {1} using community data", job.HumanReadableName, createdOrUpdated);
				new CcsukEmailSender(job.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)job, job.UserInChargeOfJob).SendEmail(subject, html,
										GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrc, "", job.Branch.Company.PK.ToGuid(), job.Branch.PK.ToGuid(), Guid.Empty);
			}
			return html;
		}

		internal static string MakeHtmlTableFromListOfProperties(Dictionary<ZPropertyInfo, IZType> propertiesUpdatedAndTransientValueHash, ColumnsToShowInInterpretation columnStyle, List<ZString> communityHandlingCodes = null)
		{
			HtmlTableCreator table = null;
			switch (columnStyle)
			{
				case ColumnsToShowInInterpretation.FieldNameAndProcessingValue:
				case ColumnsToShowInInterpretation.FieldNameAndPersistentValue:
					table = new HtmlTableCreator(new string[] { "Field", "Value" });
					break;
				case ColumnsToShowInInterpretation.FieldNameAndBothValues:
					table = new HtmlTableCreator(new string[] { "Field", "Old Value", "New Value" });
					break;
			}
			foreach (var propertyInfo in propertiesUpdatedAndTransientValueHash.Keys)
			{
				switch (columnStyle)
				{
					case ColumnsToShowInInterpretation.FieldNameAndProcessingValue:
						table.WriteRow(propertyInfo.HumanReadableName, propertiesUpdatedAndTransientValueHash[propertyInfo]);
						break;

					case ColumnsToShowInInterpretation.FieldNameAndPersistentValue:
						if (ShouldWriteDeletedLocalCHCInColour(propertyInfo))
						{
							WriteTableRowForOldAndNewValueHighlightingChanges(table, propertyInfo, propertiesUpdatedAndTransientValueHash, writeOnlyOldDeletedValue: true);
						}
						else
						{
							table.WriteRow(propertyInfo.HumanReadableName, propertyInfo.Value);
						}
						break;

					case ColumnsToShowInInterpretation.FieldNameAndBothValues:
						WriteTableRowForOldAndNewValueHighlightingChanges(table, propertyInfo, propertiesUpdatedAndTransientValueHash);
						break;
				}
			}

			if (communityHandlingCodes != null)
			{
				var chcPairs = new Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList();
				foreach (var chc in communityHandlingCodes)
				{
					table.WriteRow("Community Handling Code", chc + " - " + chcPairs.GetDescriptionFromCode(chc));
				}
			}

			return table.ToHtml();
		}

		static bool ShouldWriteDeletedLocalCHCInColour(ZPropertyInfo propertyInfo)
		{
			return propertyInfo.BizObj.IsDeleted && propertyInfo.HumanReadableName.Contains("Handling Code", StringComparison.OrdinalIgnoreCase);
		}

		void MaybeAutomaticallyPrintC1FromFrc(ICcsukCusAwb awb, ZShort piecesReceivedBeforeUpdate)
		{
			if (!awb.HasSplits)
			{
				new PrintFromFrcProvider(awb, piecesReceivedBeforeUpdate, inboundMsg, serviceLogger).DoPrinting();
			}
		}

		static void WriteTableRowForOldAndNewValueHighlightingChanges(HtmlTableCreator table, ZPropertyInfo propertyInfo, Dictionary<ZPropertyInfo, IZType> propertiesUpdatedAndTransientValueHash, bool writeOnlyOldDeletedValue = false)
		{
			var newValue = propertyInfo.Value;
			var oldValue = propertiesUpdatedAndTransientValueHash[propertyInfo];
			var attributes = new NameValueCollection();
			var parentBizO = propertyInfo.BizObj;
			if (parentBizO.IsInDatabase && !parentBizO.IsDeleted)
			{
				var compare = oldValue.CompareTo(newValue);
				if (compare < 0)
				{
					attributes.Add("style", "background-color: Gold");
				}
				else if (compare > 0)
				{
					attributes.Add("style", "background-color: Yellow");
				}
			}
			else if (parentBizO.IsInDatabase && parentBizO.IsDeleted)
			{
				attributes.Add("style", "background-color: FireBrick");
			}
			else if (!parentBizO.IsInDatabase)
			{
				attributes.Add("style", "background-color: YellowGreen");
			}

			if (writeOnlyOldDeletedValue)
			{
				table.WriteRow(attributes, propertyInfo.HumanReadableName, oldValue + " (deleted)");
			}
			else
			{
				table.WriteRow(attributes, propertyInfo.HumanReadableName, oldValue, newValue);
			}
		}

		readonly EDIMessage inboundMsg;
		readonly ILogger serviceLogger;
		readonly CuscarWithFlagsToShowWhatsSet flagableCuscar;
	}

	public enum ColumnsToShowInInterpretation
	{
		FieldNameAndPersistentValue,
		FieldNameAndBothValues,
		FieldNameAndProcessingValue
	}
}
