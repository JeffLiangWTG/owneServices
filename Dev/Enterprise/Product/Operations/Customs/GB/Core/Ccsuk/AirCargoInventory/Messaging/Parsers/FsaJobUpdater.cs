using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	class FsaJobUpdater
	{
		public FsaJobUpdater(FsaResponseMessage report, EDIMessage outboundMessage, EDIMessage incomingMessage)
		{
			this.report = report;
			this.outboundMessage = outboundMessage;
			this.incomingMessage = incomingMessage;
		}

		internal FsaProcessingResult UpdatePropertiesOfConsignmentUsingGospelCcsukData(bool alsoSetNprOnRecords = true)
		{
			this.alsoSetNprOnRecords = alsoSetNprOnRecords;
			var houseOrWorker = outboundMessage != null ? outboundMessage.EM_LinkedObject as CusHAWB : incomingMessage != null ? incomingMessage.EM_LinkedObject as CusHAWB : null;
			foreach (var consign in report.ChildConsignments)
			{
				if (houseOrWorker != null)
				{
					if (consign.IndicatorErrorText.IsEmpty)
					{
						if (houseOrWorker.CS_IsMasterHouse && new List<string> { FsaDocumentTypes.Codes.MasterAwb, FsaDocumentTypes.Codes.AirWaybill }.Contains(consign.ConsignmentReferenceNumberType))
						{
							ProcessMawb(houseOrWorker.MAWB, consign);
						}
						else if (!houseOrWorker.CS_IsMasterHouse && FsaDocumentTypes.Codes.HouseWaybill == consign.ConsignmentReferenceNumberType)
						{
							ProcessHawb(houseOrWorker, consign);
						}
						else if (FsaDocumentTypes.Codes.SplitReference == consign.ConsignmentReferenceNumberType)
						{
							ProcessSplit(houseOrWorker, consign);
						}
					}
					else if (ConsignmentErrorTextSaysNoRecordFound(consign))
					{
						ICcsukCusAwb awbThatIsNotFound = houseOrWorker;
						if (consign.ConsignmentReferenceNumberType == FsaDocumentTypes.Codes.SplitReference)
						{
							SplitConsignment splitExisting;
							SetSplitAndSplitsParent(houseOrWorker, consign, out awbThatIsNotFound, out splitExisting);
							if (splitExisting != null)
							{
								awbThatIsNotFound = splitExisting;
							}
						}
						awbThatIsNotFound.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
						processingResult = FsaProcessingResult.NotProcessed;
						QueryConsolWithFsrIfThisIsLastHouse(houseOrWorker);
						break;
					}
				}
			}
			RemoveBogusLocalSplits(houseOrWorker);
			return processingResult;
		}

		void ProcessSplit(CusHAWB houseOrWorker, FsaChildConsignment splitInFsa)
		{
			var consignmentToUseToDetmineOwner = splitInFsa.InwardLeg != null && splitInFsa.InwardLeg.Locations != null && splitInFsa.InwardLeg.Locations.AirportOfArrival != null && !splitInFsa.AgentCode.IsEmpty
													? splitInFsa : report.ChildConsignments[0];
			if (JobCanBeUpdatedAccordingToRegistryCredentials(houseOrWorker, consignmentToUseToDetmineOwner))
			{
				ICcsukCusAwb awbParentOfSplits;
				SplitConsignment splitExisting;
				SetSplitAndSplitsParent(houseOrWorker, splitInFsa, out awbParentOfSplits, out splitExisting);
				if (splitExisting == null)
				{
					splitExisting = awbParentOfSplits.Splits.AddNew();
					splitExisting.SplitReference = splitInFsa.ConsignmentReferenceNumber;
					splitExisting.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				}
				if (report.Header_SplitReference.IsEmpty)
				{
					// i.e. if the response is to a parent enquiry (without this check, we repond to the split-level FSA with a further enquiry). 
					SendFsrFsuQueryForChildConsignmentIfRegoEnabled(splitExisting);
				}
				else
				{
					// i.e. response is to this specific split, so we will have all details
					if (report.IsReportP5Insert)
					{
						ProcessMawb(houseOrWorker.MAWB, splitInFsa);
						ProcessHawb(houseOrWorker, splitInFsa);
					}
					if (alsoSetNprOnRecords)
					{
						splitExisting.NumberOfPiecesReceived = (ZShort)splitInFsa.NPR;
					}
					splitExisting.NumberOfPiecesExpected = (ZShort)splitInFsa.NPX;
					splitExisting.Weight = splitInFsa.Weight;
					splitExisting.WeightCode = splitInFsa.WeightCode;
					splitExisting.Status1Date = splitInFsa.Status1Date;
					splitExisting.TemporaryStorageEndDate = splitInFsa.TemporaryStorageEndDate;
					splitExisting.AgentBadge = splitInFsa.AgentCode;
					splitExisting.UpdateStatusToCacIfAllowed(splitInFsa.CustomsActionCode, splitInFsa.DateOfCustomsAction, splitInFsa.AgentsReferenceNumber, splitInFsa.CustomsActionText);
					if (report.IsReportP5Insert)
					{
						houseOrWorker.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
						houseOrWorker.MAWB.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
						splitExisting.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
						// CHCs cannot pertain to a split
					}
				}
			}
			else
			{
				processingResult = FsaProcessingResult.ProcessedWithoutUpdate;
			}
		}

		static void SetSplitAndSplitsParent(CusHAWB houseOrWorker, FsaChildConsignment splitInFsa, out ICcsukCusAwb awbParentOfSplits, out SplitConsignment splitExisting)
		{
			awbParentOfSplits = houseOrWorker.CS_IsMasterHouse ? houseOrWorker.MAWB : houseOrWorker;
			splitExisting = awbParentOfSplits.Splits.Find(s => s.SplitReference == splitInFsa.ConsignmentReferenceNumber).FirstOrDefault();
		}

		void SendFsrFsuQueryForChildConsignmentIfRegoEnabled(ICcsukCusAwb awb)
		{
			if (GBCustomsDataRegistry.Instance.CcsukQueryChildObjectsWhenMentionedInFsa.Value)
			{
				SendFsaWithUpdateForAwbSilently(awb);
			}
		}

		public static bool SendFsaWithUpdateForAwbSilently(ICcsukCusAwb awb)
		{
			var silentInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sender = new CcsukInventoryMessageManager(awb, new CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate(), silentInitiator);
			return sender.SendToCommunity();
		}

		void QueryConsolWithFsrIfThisIsLastHouse(CusHAWB hawb)
		{
			if (!hawb.CS_IsMasterHouse && hawb.IsLastHouseOnConsol && GBCustomsDataRegistry.Instance.CcsukAlsoSendFsrAfterFrx.Value)
			{
				SendFsaWithUpdateForAwbSilently(hawb.MAWB);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal void ProcessMawb(CusMAWB mawb, FsaChildConsignment consign)
		{
			if (JobCanBeUpdatedAccordingToRegistryCredentials(mawb, consign))
			{
				var inwardLegLocations = consign.InwardLeg?.Locations;
				mawb.CM_FlightNo = consign.InwardLeg.Carrier + consign.InboundFlightNumber;
				mawb.CM_ArrivalDate = consign.DateOfArrival;
				if ((inwardLegLocations.AirportOfOrigin?.LocationCode ?? ZString.Empty) != ZString.Empty)
				{
					mawb.AirportOfOrigin = PortConverter.IataToUnloco(inwardLegLocations.AirportOfOrigin.LocationCode, mawb.Factory);
				}
				mawb.AirportOfDestination = inwardLegLocations?.AirportOfDestination?.LocationCode ?? mawb.AirportOfDestination;
				mawb.AirportOfArrival = inwardLegLocations?.AirportOfArrival?.LocationCode ?? mawb.AirportOfArrival;
				mawb.CargoTerminalOperator = inwardLegLocations?.AirportOfArrival?.ShedOperator ?? mawb.CargoTerminalOperator;
				if ((inwardLegLocations?.AirportOfArrival?.LocationCode ?? ZString.Empty) != ZString.Empty)
				{
					mawb.CargoTerminalOperatorAirport = PortConverter.IataToChief(inwardLegLocations.AirportOfArrival.LocationCode, mawb.Factory).Left(mawb.CargoTerminalOperatorAirportInfo.MaxLength);
				}
				mawb.ShipmentDescriptionCode = consign.ShipmentDescriptionCode;
				mawb.ConsignmentOrEntryType = consign.ConsignmentType;
				mawb.AgentBadge = consign.AgentCode;
				mawb.NumberOfPiecesExpected = (ZShort)consign.NPX;
				if (alsoSetNprOnRecords)
				{
					mawb.NumberOfPiecesReceived = (ZShort)consign.NPR;
				}
				mawb.Weight = consign.Weight;
				mawb.WeightCode = consign.WeightCode.Left(2);
				mawb.DescriptionOfGoods = consign.DescriptionOfGoods;
				mawb.Status1Date = consign.Status1Date;
				mawb.TemporaryStorageEndDate = consign.TemporaryStorageEndDate;
				processingResult = FsaProcessingResult.ProcessedAndUpdated;
				if (!consign.CustomsActionText.IsEmpty && consign.CustomsActionText != mawb.LatestCustomsActionText)
				{
					mawb.LatestCustomsActionText = consign.CustomsActionText;
				}
				if (!consign.CustomsActionCode.IsEmpty && consign.CustomsActionCode != mawb.CustomsActionCode)
				{
					mawb.SetCustomsActionCode(consign.CustomsActionCode, consign.DateOfCustomsAction);
				}
				MaybeAddCHCsToIsrdRecordFromP5(mawb, consign);
			}
			else
			{
				processingResult = FsaProcessingResult.ProcessedWithoutUpdate;
			}
			CcsukUtilities.UpdatePresenceToYesIfNotCurrentlyTerminal(mawb);
		}

		void MaybeAddCHCsToIsrdRecordFromP5(ICcsukCusAwb awb, FsaChildConsignment consign)
		{
			if (GBCustomsDataRegistry.Instance.CcsukPutChcsFromInboundP5ReportOntoNewJob.Value)
			{
				CuscarFrcConsignmentUpdater.SynchroniseCommunityHandlingCodes(consign.CommunityHandlingCodes, awb.CommunityHandlingCodes, null, "");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessHawb(CusHAWB hawb, FsaChildConsignment consign)
		{
			if (JobCanBeUpdatedAccordingToRegistryCredentials(hawb, consign))
			{
				var inwardLegLocations = consign.InwardLeg?.Locations;
				hawb.MAWB.CM_FlightNo = consign.InwardLeg.Carrier + consign.InboundFlightNumber;
				hawb.MAWB.CM_ArrivalDate = consign.DateOfArrival;
				if ((inwardLegLocations?.AirportOfOrigin?.LocationCode ?? ZString.Empty) != ZString.Empty)
				{
					hawb.AirportOfOrigin = PortConverter.IataToUnloco(inwardLegLocations.AirportOfOrigin.LocationCode, hawb.Factory);
				}
				hawb.AirportOfDestination = inwardLegLocations?.AirportOfDestination?.LocationCode ?? hawb.AirportOfDestination;
				hawb.AirportOfArrival = inwardLegLocations?.AirportOfArrival?.LocationCode ?? hawb.AirportOfArrival;
				hawb.CargoTerminalOperator = inwardLegLocations.AirportOfArrival.ShedOperator;
				if ((inwardLegLocations?.AirportOfArrival?.LocationCode ?? ZString.Empty) != ZString.Empty)
				{
					hawb.CargoTerminalOperatorAirport = PortConverter.IataToChief(inwardLegLocations.AirportOfArrival.LocationCode, hawb.Factory).Left(hawb.CargoTerminalOperatorAirportInfo.MaxLength);
				}
				hawb.ShipmentDescriptionCode = consign.ShipmentDescriptionCode;
				hawb.ConsignmentOrEntryType = consign.ConsignmentType;
				hawb.AgentBadge = consign.AgentCode;
				if (alsoSetNprOnRecords)
				{
					hawb.CS_PiecesLanded = (ZShort)consign.NPR;
				}
				hawb.CS_PiecesManifested = (ZShort)consign.NPX;
				hawb.CS_Weight = consign.Weight;
				hawb.CS_WeightUQ = consign.WeightCode.Left(2);
				hawb.CS_GoodsDescription = consign.DescriptionOfGoods;
				hawb.Status1Date = consign.Status1Date;
				hawb.TemporaryStorageEndDate = consign.TemporaryStorageEndDate;
				processingResult = FsaProcessingResult.ProcessedAndUpdated;
				if (!consign.CustomsActionText.IsEmpty && consign.CustomsActionText != hawb.LatestCustomsActionText)
				{
					hawb.LatestCustomsActionText = consign.CustomsActionText;
				}
				if (!consign.CustomsActionCode.IsEmpty && consign.CustomsActionCode != hawb.CustomsActionCode)
				{
					hawb.SetCustomsActionCode(consign.CustomsActionCode, consign.DateOfCustomsAction);
				}
				if (report.IsReportP5Insert)
				{
					hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
					hawb.MAWB.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
				}
				MaybeAddCHCsToIsrdRecordFromP5(hawb, consign);
			}
			else
			{
				processingResult = FsaProcessingResult.ProcessedWithoutUpdate;
			}
			CcsukUtilities.UpdatePresenceToYesIfNotCurrentlyTerminal(hawb);
		}

		internal void UpdatePresenceOnNetworkForSimpleNonUpdatingFsrCycle()
		{
			var hawb = outboundMessage.EM_LinkedObject as CusHAWB;
			if (hawb != null)
			{
				var presence = PresenceOnNetworkList.Codes.OnCommDb;
				foreach (var consignment in report.ChildConsignments)
				{
					if (FsaJobUpdater.ConsignmentErrorTextSaysNoRecordFound(consignment))
					{
						presence = PresenceOnNetworkList.Codes.NotOnCommDb;
						QueryConsolWithFsrIfThisIsLastHouse(hawb);
						break;
					}
				}
				if (CcsukUtilities.PresenceOnNetworkIsNotTerminal(hawb.PresenceOnNetworkStatus))
				{
					hawb.PresenceOnNetworkStatus = presence;
				}
			}
		}

		bool JobCanBeUpdatedAccordingToRegistryCredentials(ICcsukCusAwb awbInLocalDatabase, FsaChildConsignment consign)
		{
			var airportAndShedInMessage = consign.InwardLeg.Locations.AirportOfArrival.LocationCode + consign.InwardLeg.Locations.AirportOfArrival.ShedOperator;
			var agentInMessage = consign.AgentCode;
			return JobCanBeUpdatedAccordingToRegistryCredentials(awbInLocalDatabase, airportAndShedInMessage, agentInMessage, incomingMessage);
		}

		internal static bool JobCanBeUpdatedAccordingToRegistryCredentials(ICcsukCusAwb awbInLocalDatabase, ZString airportAndShedInMessage, ZString agentInMessage, EDIMessage incomingMessage)
		{
			bool result = false;
			if (InboundMessageIsAddressedToAgentInAirlineFallback(awbInLocalDatabase, incomingMessage)) // Result describes a agent who is not us, but the FSA isa addressed to an airline-agent-in-fallback
			{
				result = true;
			}
			else
			{
				foreach (CredentialsSetting credential in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(awbInLocalDatabase.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if ((credential.IsCcskShed && credential.PIMA.EndsWith(airportAndShedInMessage, System.StringComparison.OrdinalIgnoreCase)) // Result describes a shed, and that shed is ours
					||
					(credential.IsCcskAgent && credential.Company == agentInMessage))// Result describes an agent, and that agent is ours
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		internal static bool IsOurShedAccordingToRegistryCredentials(ZString airportAndShed, GlbBranch branchForRegistry)
		{
			var isOurShed = false;
			foreach (CredentialsSetting credential in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(branchForRegistry.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (credential.IsCcskShed && credential.PIMA.EndsWith(airportAndShed, System.StringComparison.OrdinalIgnoreCase)) // Result describes a shed, and that shed is ours
				{
					isOurShed = true;
					break;
				}
			}
			return isOurShed;
		}

		static bool InboundMessageIsAddressedToAgentInAirlineFallback(ICcsukCusAwb awbInLocalDatabase, EDIMessage incomingMessage)
		{
			var isFallback = false;
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(awbInLocalDatabase.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			if (incomingMessage != null && incomingMessage.Interchange != null && credentials != null)
			{
				isFallback = (from CredentialsSetting c in credentials where c.PIMA == incomingMessage.Interchange.EI_To && !c.FallbackForShed.IsEmpty select c).Any();
			}
			return isFallback;
		}

		internal static bool ConsignmentErrorTextSaysNoRecordFound(FsaChildConsignment consign)
		{
			return consign.IndicatorErrorText.ToUpper().Contains(noRecordFound);
		}

		void RemoveBogusLocalSplits(CusHAWB houseOrWorker)
		{
			if (!report.Header_ReportText.ToUpper().Contains("WHICH HAS SPLITS", System.StringComparison.Ordinal) && houseOrWorker != null && report.Header_SplitReference.IsEmpty)
			{
				var localRecord = houseOrWorker.CS_IsMasterHouse ? houseOrWorker.MAWB : (ICcsukCusAwb)houseOrWorker;
				if (localRecord.HasSplits)
				{
					// our record has splits locally but the FSA reports none
					var anySplitHasStatus3 = false;
					foreach (SplitConsignment s in localRecord.Splits)
					{
						if (!s.CustomsActionCode.IsEmpty && CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(s.CustomsActionCode))
						{
							anySplitHasStatus3 = true;
							break;
						}
					}
					if (!anySplitHasStatus3)
					{
						localRecord.Splits.RemoveAndDeleteAll();
						localRecord.SetCustomsActionCode("", ZDateTime.Empty);  // Wipe the "--" status once all splits are gone
					}
				}
			}
		}

		const string noRecordFound = "NO MATCHING CONSIGNMENT RECORD FOUND";

		readonly FsaResponseMessage report;
		readonly EDIMessage outboundMessage;
		readonly EDIMessage incomingMessage;
		FsaProcessingResult processingResult;
		bool alsoSetNprOnRecords;
	}
}
