using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506")]
	public class FreightConsolWrapper : Customs.Business.FreightConsolWrapper
	{
		public FreightConsolWrapper(ForwardingConsol consol)
			: base(consol)
		{
		}

		public FreightConsolWrapper(ForwardingConsol consol, EDIMessage responseMessage)
			: base(consol)
		{
			this.responseMessage = responseMessage;
		}

		readonly EDIMessage responseMessage;

		public void ReloadAllChildObjects()
		{
			Consol.Factory.ClearQueryCache();
			Consol.Reload();
			Consol.Messages.Load();
			foreach (var declaration in Declarations)
			{
				declaration.Reload();
				declaration.Messages.Load();
			}

			var mAWB = CusMAWB.Load(Consol);
			if (mAWB != null)
			{
				mAWB.Reload();
				mAWB.Messages.Load();
			}

			var ocean = CusSCAOceanBill.Load(Consol);
			if (ocean != null)
			{
				ocean.Reload();
				ocean.MessageCollection.Load();
			}

			foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
			{
				shipment.Reload();
				shipment.Messages.Load();
				var hAWB = CusHAWB.Load(shipment);
				if (hAWB != null)
				{
					hAWB.Reload();
					hAWB.Messages.Load();
				}
				var house = CusSCAHouse.Load(shipment.Factory, shipment.JS_UniqueConsignRef);
				if (house != null)
				{
					house.Reload();
					house.Messages.Load();
				}
			}
		}

		public bool AreAnyCusSCAHousesWaitingForAResponse
		{
			get
			{
				var result = false;
				var oceanBill = CusSCAOceanBill.Load(Consol);
				if (oceanBill != null)
				{
					foreach (var house in oceanBill.HouseBills.Cast<CusSCAHouse>())
					{
						if (house.Messages.IsWaitingForAResponse)
						{
							result = true;
							break;
						}
					}
					if (oceanBill.MessageCollection.IsWaitingForAResponse)
					{
						result = true;
					}
				}
				return result;
			}
		}

		public bool AreAnyHAWBsWaitingForAResponse
		{
			get
			{
				var result = false;
				foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
				{
					var hAWB = CusHAWB.Load(shipment);
					if (hAWB != null && hAWB.Messages.IsWaitingForAResponse)
					{
						result = true;
						break;
					}
				}
				var mAWB = CusMAWB.Load(Consol);
				if (mAWB != null && mAWB.Messages.IsWaitingForAResponse)
				{
					result = true;
				}

				return result;
			}
		}

		public bool AreAnyDeclarationsWaitingForAResponse
		{
			get
			{
				var result = false;
				foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
				{
					var filter = new ZQuery();
					filter.AddToFilter(JobDeclarationSchema.JE_JS, shipment.PK);
					var declaration = Consol.Factory.LoadTop1<JobDeclaration>(filter);
					if (declaration != null && (declaration.IsWaitingForExportResponse || CMRImportMessageStatusList.IsAwaitingResponse(declaration.JE_MessageStatus) || CMRExportOtherMessageStatusList.IsAwaitingResponse(declaration.JE_MessageStatus)))
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool AreAnyCusUnderbondsWaitingForAResponse
		{
			get
			{
				var result = false;
				foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
				{
					var hAWB = CusHAWB.Load(shipment);
					if (hAWB != null && ((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AreAnyUnderbondsWaitingForAResponse)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool HasAnyDeclarationFailed
		{
			get
			{
				var result = false;
				foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
				{
					var declarations = new FreightShipmentWrapper(shipment, Consol).GetJobDeclaration();
					foreach (var declaration in declarations)
					{
						if (CMRImportMessageStatusList.IsFailedResponse(declaration.JE_MessageStatus))
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public JobDeclaration LoadArbitraryJobDeclaration()
		{
			JobDeclaration result = null;
			var filter = new ZQuery(JobDeclarationSchema.JE_JS, Consol.Shipments.GetPKs());
			foreach (var declaration in Consol.Factory.Load(typeof(JobDeclaration), filter).Cast<JobDeclaration>())
			{
				if (declaration.Branch != null && declaration.Branch.Company != null && declaration.Branch.Company.PK == GlbCompany.CurrentCompany.PK)
				{
					result = declaration;
					break;
				}
			}
			return result;
		}

		public ZString ShortDescription
		{
			get { return "Consol #: " + Consol.JK_UniqueConsignRef; }
		}

		public ZString Details
		{
			get
			{
				var resultBuilder = new StringBuilder();
				if (!Consol.JK_UniqueConsignRef.IsEmpty)
				{
					resultBuilder.Append("Consol #: " + Consol.JK_UniqueConsignRef + "\r\n");
				}

				if (!Consol.JK_TransportMode.IsEmpty)
				{
					resultBuilder.Append("Mode: " + Consol.JK_TransportMode + "\r\n");
				}

				if (!Consol.JK_RL_NKLoadPort.IsEmpty)
				{
					resultBuilder.Append("Port of 1st Load: " + Consol.JK_RL_NKLoadPort + "\r\n");
				}

				if (!Consol.JK_JX_JA_RL_NKPortOfLoading.IsEmpty)
				{
					resultBuilder.Append("Port of Loading: " + Consol.JK_JX_JA_RL_NKPortOfLoading + "\r\n");
				}

				if (Consol.MostInterestingTransportPortOfDischarge != null && !Consol.MostInterestingTransportPortOfDischarge.RL_Code.IsEmpty)
				{
					resultBuilder.Append("Port of Discharge: " + Consol.MostInterestingTransportPortOfDischarge.RL_Code + "\r\n");
				}

				if (!Consol.JK_JX_JB_RL_NKPortOfDischarge.IsEmpty)
				{
					resultBuilder.Append("Port of Final Discharge: " + Consol.JK_RL_NKDischargePort + "\r\n");
				}

				if (!Consol.JK_JX_JV_NKVessel.IsEmpty)
				{
					resultBuilder.Append("Vessel #: " + Consol.JK_JX_JV_NKVessel + "\r\n");
				}

				if (!Consol.JK_JX_JV_VoyageFlight.IsEmpty)
				{
					resultBuilder.Append((Consol.IsAir ? "Flight" : "Voyage") + " #: " + Consol.JK_JX_JV_VoyageFlight + "\r\n");
				}

				if (!Consol.JK_JX_JA_E_DEP.IsEmpty)
				{
					resultBuilder.Append("Estimated Departure Date: " + Consol.JK_JX_JA_E_DEP.ToString("D") + "\r\n");
				}

				if (!Consol.JK_JX_JA_A_DEP.IsEmpty)
				{
					resultBuilder.Append("Departure Date: " + Consol.JK_JX_JA_A_DEP.ToString("D") + "\r\n");
				}

				if (!Consol.JK_MasterBillNum.IsEmpty)
				{
					resultBuilder.Append("Masterbill #: " + Consol.JK_MasterBillNum + "\r\n");
				}

				return resultBuilder.ToString();
			}
		}

		public ZString GetCMRStatus(ZString messageType)
		{
			var lastMessage = Consol.Messages.GetLastMessage(EDIInterchange.ApplicationCodes.CMR, messageType);
			return GetStatus(lastMessage);
		}

		protected AUCusEntryNumber GetEntryNumberByCode(ZString code)
		{
			var sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, code);
			if (Consol != null)
			{
				sQLFilter.AddToFilter(CusEntryNumSchema.CE_ParentID, Consol.PK);
			}

			sQLFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			sQLFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, Freight.Common.Business.AutoJobConsol.Schema.TableName);
			return Consol == null ? null : Consol.Factory.LoadTop1<AUCusEntryNumber>(sQLFilter);
		}

		public AUCusEntryNumber GetPermit()
		{
			return GetEntryNumberByCode(CusEntryNumberTypes.Australia.CRN);
		}

		#region ESM Change Message Methods

		public void UpdatePreliminaryLinesToManifestedLines()
		{
			foreach (var manifestSequence in AllConsolManifestLineSequences)
			{
				if (manifestSequence.IsPreliminaryNumberType)
				{
					manifestSequence.UpdatePreliminaryLineNumberToManifestedLineNumber();
				}
			}
		}

		public void RemovePreliminaryLineNumbers()
		{
			foreach (var manifestSequence in AllConsolManifestLineSequences)
			{
				if (manifestSequence.IsPreliminaryDeletedLine)
				{
					manifestSequence.ResetPreliminaryDeleteLineToManifested();
				}
				else if (manifestSequence.IsPreliminaryNumberType)
				{
					manifestSequence.Delete();
				}
			}
		}

		public void RemoveAllESMLineNumbers()
		{
			foreach (var manifestSequence in AllConsolManifestLineSequences)
			{
				manifestSequence.Delete();
			}
		}

		public void UpdateDeletedManifestLines()
		{
			foreach (var manifestSequence in AllConsolManifestLineSequences)
			{
				if (manifestSequence.IsPreliminaryDeletedLine)
				{
					manifestSequence.UpdatePreliminaryDeletedLineNumberToDeletedLine();
				}
			}
		}

		public Common.MessageBuilders.MessageSubTypes ExportManifestMessageType
		{
			get
			{
				var result = Common.MessageBuilders.MessageSubTypes.Create;
				var permit = GetPermit();
				if (permit != null && !permit.CE_EntryNum.IsEmpty)
				{
					result = HasSameShipments ? Common.MessageBuilders.MessageSubTypes.Replace : Common.MessageBuilders.MessageSubTypes.Change;
				}

				return result;
			}
		}

		bool CalculateHasSameShipments()
		{
			var result = true;
			if (LastClearedESM != null)
			{
				if (LastClearedESM.EM_MessageSubType == CMRMessage.MessageSubTypes.Change)
				{
					result = false;
				}
				else
				{
					var allConsignmentLines = LineShipmentConsignments.Count + LineHVLVConsignments.Count;
					if (allConsignmentLines > 0)
					{
						result = ConsolShipmentsAndHVLVConsignmentCount == allConsignmentLines;

						if (result)
						{
							var consolForwardingShipments = Consol.Shipments.Cast<ForwardingShipment>();
							if (consolForwardingShipments.Any(shipment => !shipment.IsHighVolumeLowValue && !shipment.IsHighVolumeLowValueLegacy &&
								!LineShipmentConsignments.ContainsValue(shipment.PK)))
							{
								result = false;
							}
							else
							{
								var currentLines = consolForwardingShipments.SelectMany(shipment => shipment.GetHVLVConsignmentLines().Select(line => line.PK)).ToHashSet();
								var prevLines = LineHVLVConsignments.Select(consignment => consignment.Value).ToHashSet();
								if (!currentLines.SetEquals(prevLines))
								{
									result = false;
								}
							}
						}
					}
				}
			}
			return result;
		}

		public bool HasSameShipments
		{
			get
			{
				if (!hasSameShipments.HasValue)
				{
					hasSameShipments = CalculateHasSameShipments();
				}
				return hasSameShipments.Value;
			}
		}

		bool? hasSameShipments;

		int ConsolShipmentsAndHVLVConsignmentCount
		{
			get
			{
				return Consol.Shipments.Cast<ForwardingShipment>().Sum(x => x.IsHighVolumeLowValueLegacy || x.IsHighVolumeLowValue ? x.GetHVLVConsignmentLines().Count() : 1);
			}
		}

		EDIMessage LastClearedESM
		{
			get
			{
				if (fLastClearedESM == null)
				{
					var lastClearedResponse = Consol.Messages.GetLastClearReceivedMessage(CMRMessage.CMRMessageTypes.ESM);
					if (lastClearedResponse != null)
					{
						fLastClearedESM = GetMatchingOutgoingMessage(lastClearedResponse.EM_SystemCreateTimeUtc);
					}
				}

				return fLastClearedESM;
			}
		}
		EDIMessage fLastClearedESM;

		EDIMessage GetMatchingOutgoingMessage(ZDateTime responseMessageCreateTime)
		{
			EDIMessage outgoingMessage = null;
			Consol.Messages.Sort(AutoEDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
			foreach (var message in Consol.Messages.Cast<EDIMessage>())
			{
				if (message.EM_ApplicationCode == EDIInterchange.ApplicationCodes.CMR
					&& message.EM_MessageType == CMRMessage.CMRMessageTypes.ESM
					&& message.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit
					&& message.EM_SystemCreateTimeUtc < responseMessageCreateTime)
				{
					outgoingMessage = message;
					break;
				}
			}

			return outgoingMessage;
		}

		EDIMessage MatchingOutgoingMessageForResponse
		{
			get
			{
				if (fMatchingOutgoingMessageForResponse == null && responseMessage != null)
				{
					fMatchingOutgoingMessageForResponse = GetMatchingOutgoingMessage(responseMessage.EM_SystemCreateTimeUtc);
				}

				return fMatchingOutgoingMessageForResponse;
			}
		}
		EDIMessage fMatchingOutgoingMessageForResponse;

		public string MessageTypeBeingRespondedTo
		{
			get
			{
				return MatchingOutgoingMessageForResponse != null ? MatchingOutgoingMessageForResponse.EM_MessageSubType : ZString.Empty;
			}
		}

		public bool IsWithdrawalResponse
		{
			get { return MessageTypeBeingRespondedTo == CMRMessage.MessageSubTypes.Withdraw; }
		}

		public bool IsReplacementResponse
		{
			get { return MessageTypeBeingRespondedTo == CMRMessage.MessageSubTypes.Amendment; }
		}

		public bool IsChangeResponse
		{
			get { return MessageTypeBeingRespondedTo == CMRMessage.MessageSubTypes.Change; }
		}

		public bool IsOriginalResponse
		{
			get { return MessageTypeBeingRespondedTo == CMRMessage.MessageSubTypes.Original; }
		}

		CustomsManifestLineSequence[] ConsolMlnManifestLineSequences()
		{
			// TODO: Refactor this code to make CY_ParentID = Consol.JK_PK and CY_Data = Other reference, there is no index for this current logic
			var manifestLinesForConsolQuery = new ZQuery(CusCodeDataSchema.CY_Data, Consol.JK_UniqueConsignRef);
			manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CustomsManifestLineSequence);
			manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Code, CustomsManifestLineSequence.DataCodes.ManifestLineNumber);
			manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Order, SQLComparisonOperator.GreaterThan, consignmentNotUsed);
			return Consol.Factory.Load<CustomsManifestLineSequence>(manifestLinesForConsolQuery);
		}
		readonly ZShort consignmentNotUsed = 0;

		CustomsManifestLineSequence[] AllConsolManifestLineSequences
		{
			get
			{
				// TODO: Refactor this code to make CY_ParentID = Consol.JK_PK and CY_Data = Other reference, there is no index for this current logic
				var manifestLinesForConsolQuery = new ZQuery(CusCodeDataSchema.CY_Data, Consol.JK_UniqueConsignRef);
				manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CustomsManifestLineSequence);
				return Consol.Factory.Load<CustomsManifestLineSequence>(manifestLinesForConsolQuery);
			}
		}

#if DEBUG
		public
#endif
 Dictionary<ZInt, ZGuid> LineShipmentConsignments
		{
			/*
			 *	These are the last set of consigment references that Customs will have recorded. (i.e. the CNI details for each Consignment)
			 *	This is needed to determine the Line Number that Customs have stored against the consignment.
			 *	Subsequent Change messages must match the consignment line numbers exactly with those recorded at Customs for the last successful message.
			 *	The array is obtained by finding all the manifest line number records generated from the last clear response for the reference sent.
			 */
			get
			{
				var lineConsignmentsAtLastAcceptedExportManifest = new Dictionary<ZInt, ZGuid>();

				if (LastClearedESM != null)
				{
					foreach (var manifestSequence in ConsolMlnManifestLineSequences())
					{
						if (manifestSequence.CY_ParentTableCode == JobShipmentSchema.Constants.Prefix)
						{
							var shipment = Consol.Factory.Load<CommonShipment>(manifestSequence.CY_ParentID);
							var shipmentPK = shipment != null ? shipment.PK : ZGuid.Empty;
							lineConsignmentsAtLastAcceptedExportManifest.Add(manifestSequence.CY_Order, shipmentPK);
						}
					}
				}

				return lineConsignmentsAtLastAcceptedExportManifest;
			}
		}

		Dictionary<ZInt, ZGuid> LineHVLVConsignments => Consol.Factory.GetValue(ref lineHVLVConsignmentsCached, delegate
		{
			var result = new Dictionary<ZInt, ZGuid>();
			if (LastClearedESM != null)
			{
				foreach (var manifestSequence in ConsolMlnManifestLineSequences())
				{
					if (manifestSequence.CY_ParentTableCode == HVLVConsignmentSchema.Constants.Prefix)
					{
						var consignment = Consol.Factory.Load<IHVLVConsignment>(manifestSequence.CY_ParentID);
						var consignmentPK = consignment?.PK ?? ZGuid.Empty;
						result.Add(manifestSequence.CY_Order, consignmentPK);
					}
				}
			}

			return result;
		});

		CachedProperty<Dictionary<ZInt, ZGuid>> lineHVLVConsignmentsCached;

		Dictionary<ZInt, ZGuid> LineShipmentAndHVLVConsignments => Consol.Factory.GetValue(ref lineShipmentAndHVLVConsignmentsCached, delegate
		{
			var result = new Dictionary<ZInt, ZGuid>();
			foreach (var manifestSequence in ConsolMlnManifestLineSequences())
			{
				if (manifestSequence.CY_ParentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					var shipment = Consol.Factory.Load<CommonShipment>(manifestSequence.CY_ParentID);
					var shipmentPK = shipment != null ? shipment.PK : ZGuid.Empty;
					result.Add(manifestSequence.CY_Order, shipmentPK);
				}
				else if (manifestSequence.CY_ParentTableCode == HVLVConsignmentSchema.Constants.Prefix)
				{
					var consignment = Consol.Factory.Load<IHVLVConsignment>(manifestSequence.CY_ParentID);
					var consignmentPK = consignment?.PK ?? ZGuid.Empty;
					result.Add(manifestSequence.CY_Order, consignmentPK);
				}
			}

			return result;
		});

		CachedProperty<Dictionary<ZInt, ZGuid>> lineShipmentAndHVLVConsignmentsCached;

		public string GetLineActionFor(ZGuid shipmentPK)
		{
			var result = LineAction.Insert;
			foreach (var consignment in LineShipmentConsignments)
			{
				if (consignment.Value == shipmentPK)
				{
					result = LineAction.Amend;
					break;
				}
			}

			return result;
		}

		public string GetLineActionForConsignment(ZGuid consignmentPK)
		{
			var result = LineAction.Insert;
			foreach (var consignment in LineHVLVConsignments)
			{
				if (consignment.Value == consignmentPK)
				{
					result = LineAction.Amend;
					break;
				}
			}

			return result;
		}

		public IEnumerable<ZInt> GetRemovedConsignmentsLineNo()
		{
			foreach (var consignmentToDelete in RemovedConsignmentsLineNo)
			{
				yield return consignmentToDelete.Key;
			}
		}

		public Dictionary<ZInt, ZGuid> RemovedConsignmentsLineNo => Consol.Factory.GetValue(ref deletedLineConsignmentsCached, delegate
		{
			var consignmentsDeleted = new Dictionary<ZInt, ZGuid>();
			var consignmentPKs = new List<ZGuid>();
			var nonHVLVShipmentPKs = new List<ZGuid>();
			foreach (var shipment in Consol.Shipments.Cast<ForwardingShipment>())
			{
				if (shipment.IsHighVolumeLowValueLegacy || shipment.IsHighVolumeLowValue)
				{
					consignmentPKs.AddRange(shipment.GetHVLVConsignmentLines().Select(line => line.PK));
				}
				else
				{
					nonHVLVShipmentPKs.Add(shipment.PK);
				}
			}

			foreach (var consignment in LineShipmentConsignments)
			{
				if (!nonHVLVShipmentPKs.Contains(consignment.Value))
				{
					consignmentsDeleted.Add(consignment.Key, consignment.Value);
				}
			}

			foreach (var consignment in LineHVLVConsignments)
			{
				if (!consignmentPKs.Contains(consignment.Value))
				{
					consignmentsDeleted.Add(consignment.Key, consignment.Value);
				}
			}

			return consignmentsDeleted;
		});
		CachedProperty<Dictionary<ZInt, ZGuid>> deletedLineConsignmentsCached;

		public ZInt LastLineNoUsed
		{
			get
			{
				if (!lastLineNoUsed.HasValue)
				{
					lastLineNoUsed = AllManifestedLines.Values.Count > 0 ? AllManifestedLines.Keys.Max() : ZInt.Zero;
				}

				return lastLineNoUsed.Value;
			}
		}
		ZInt? lastLineNoUsed;

		protected Dictionary<ZInt, ZGuid> AllManifestedLines
		{
			get
			{
				if (fAllManifestedLines == null)
				{
					fAllManifestedLines = LineShipmentAndHVLVConsignments;
					foreach (var deletedRef in DeletedManifestLineConsignments)
					{
						fAllManifestedLines.Add(deletedRef.Key, deletedRef.Value);
					}
				}

				return fAllManifestedLines;
			}
		}
		Dictionary<ZInt, ZGuid> fAllManifestedLines;

		public Dictionary<ZInt, ZGuid> DeletedManifestLineConsignments
		{
			get
			{
				var manifestConsignmentsDeleted = new Dictionary<ZInt, ZGuid>();
				foreach (var manifestSequence in ConsolManifestLinesDeleted)
				{
					var shipment = Consol.Factory.Load<CommonShipment>(manifestSequence.CY_ParentID);
					var shipmentPK = shipment != null ? shipment.PK : ZGuid.Empty;
					manifestConsignmentsDeleted.Add(manifestSequence.CY_Order, shipmentPK);
				}

				return manifestConsignmentsDeleted;
			}
		}

		Customs.Business.CusCodeData[] ConsolManifestLinesDeleted
		{
			get
			{
				if (fConsolManifestLinesDeleted == null)
				{
					// TODO: Refactor this code to make CY_ParentID = Consol.JK_PK and CY_Data = Other reference, there is no index for this current logic
					var manifestLinesForConsolQuery = new ZQuery(CusCodeDataSchema.CY_Data, Consol.JK_UniqueConsignRef);
					manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CustomsManifestLineSequence);
					manifestLinesForConsolQuery.AddToFilter(CusCodeDataSchema.CY_Code, CustomsManifestLineSequence.DataCodes.DeletedLineNumber);
					fConsolManifestLinesDeleted = Consol.Factory.Load<CustomsManifestLineSequence>(manifestLinesForConsolQuery);
				}

				return fConsolManifestLinesDeleted;
			}
		}
		Customs.Business.CusCodeData[] fConsolManifestLinesDeleted;

		#endregion

		public AUCusEntryNumber CreateCusEntryNumber()
		{
			var cusEntryNumber = (AUCusEntryNumber)Consol.Factory.New(typeof(AUCusEntryNumber));
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.CRN;
			cusEntryNumber.CE_ParentID = Consol.PK;
			cusEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNumber.CE_ParentTable = Freight.Common.Business.AutoJobConsol.Schema.TableName;
			return cusEntryNumber;
		}

		public AUCusEntryNumber GetMainManifestNumber()
		{
			return GetEntryNumberByCode(CusEntryNumberTypes.Australia.MMN);
		}

		public AUCusEntryNumber CreateMainManifestNumber()
		{
			var cusEntryNumber = (AUCusEntryNumber)Consol.Factory.New(typeof(AUCusEntryNumber));
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.MMN;
			cusEntryNumber.CE_ParentID = Consol.PK;
			cusEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNumber.CE_ParentTable = Freight.Common.Business.AutoJobConsol.Schema.TableName;
			return cusEntryNumber;
		}

		public ZString CAN
		{
			get
			{
				var entryNum = GetPermit()
					?? GetMainManifestNumber();

				return entryNum == null ? ZString.Empty : entryNum.CE_EntryNum;
			}
		}

		public ZString PackDepotEstablishmentID
		{
			get
			{
				if (Consol.PackDepotAddress != null)
				{
					return Consol.PackDepotAddress.LocalControlledPremisesID;
				}
				return ZString.Empty;
			}
		}

		public ZString DepartureCTOEstablishmentID
		{
			get
			{
				if (Consol.DepartureCTOAddress != null)
				{
					return Consol.DepartureCTOAddress.LocalControlledPremisesID;
				}
				return ZString.Empty;
			}
		}

		public AUCusEntryNumber CreateCustomsAuthorityNumber()
		{
			var cusEntryNumber = (AUCusEntryNumber)Consol.Factory.New(typeof(AUCusEntryNumber));
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			cusEntryNumber.CE_ParentID = Consol.PK;
			cusEntryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNumber.CE_ParentTable = Freight.Common.Business.AutoJobConsol.Schema.TableName;
			return cusEntryNumber;
		}

		public ZDateTime GetActualOrEstimatedDateOfDeparture()
		{
			if (Consol.JK_JX_JA_A_DEP.IsValid)
			{
				return Consol.JK_JX_JA_A_DEP;
			}
			else if (Consol.JK_JX_JA_E_DEP.IsValid)
			{
				return Consol.JK_JX_JA_E_DEP;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		public string GetActualOrEstimatedDateOfDeparture(string format)
		{
			var actualOrEstimatedDateOfDeparture = GetActualOrEstimatedDateOfDeparture();
			if (actualOrEstimatedDateOfDeparture.IsEmpty)
			{
				return "";
			}
			else
			{
				return actualOrEstimatedDateOfDeparture.ToString(format);
			}
		}

		public bool ShouldBeSentViaCMR
		{
			get
			{
				return !HasExitEntryNumber;
			}
		}

		public JobDeclaration[] Declarations
		{
			get
			{
				var result = new ArrayList();
				foreach (var shipment in Consol.Shipments.Cast<CommonShipment>())
				{
					var shipmentWrapper = new FreightShipmentWrapper(shipment, Consol);
					var declarations = shipmentWrapper.GetJobDeclaration();
					foreach (var declaration in declarations)
					{
						result.Add(declaration);
					}
				}
				return (JobDeclaration[])result.ToArray(typeof(JobDeclaration));
			}
		}

		public ESMStatus ESMStatus
		{
			get
			{
				if (fESMStatus == null)
				{
					fESMStatus = new ESMStatus(Consol);
				}
				return fESMStatus;
			}
		}
		protected ESMStatus fESMStatus;

		public EMMStatus EMMStatus
		{
			get
			{
				if (fEMMStatus == null)
				{
					fEMMStatus = new EMMStatus(Consol);
				}
				return fEMMStatus;
			}
		}
		protected EMMStatus fEMMStatus;

		[MaxLength(14)]
		public ZString ContingencyCAN
		{
			get
			{
				var entryNum = GetEntryNumber(CANType.ContingencyCustomsAuthorityNumber.Code);
				return entryNum == null ? ZString.Empty : entryNum.CE_EntryNum;
			}
			set
			{
				if (ContingencyCAN != value)
				{
					var entryNum = GetOrCreateEntryNumber(CANType.ContingencyCustomsAuthorityNumber.Code);
					if (value.IsEmpty)
					{
						entryNum.Delete();
					}
					else
					{
						entryNum.CE_EntryNum = value;
					}

					Consol.HasChanges = true;
				}
			}
		}

		public bool HasExitEntryNumber
		{
			get
			{
				var permit = GetPermit();
				return permit != null && permit.CE_EntryNum.Length == 14;
			}
		}

		#region Implementation

		protected AUCusEntryNumber GetEntryNumber(ZString entryNumberType)
		{
			var filter = new ZQuery();
			filter.AddToFilter(CusEntryNumSchema.CE_ParentID, Consol.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryNumberType);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			return Consol.Factory.LoadTop1<AUCusEntryNumber>(filter);
		}

		protected AUCusEntryNumber GetOrCreateEntryNumber(ZString entryNumberType)
		{
			var result = GetEntryNumber(entryNumberType);
			if (result == null)
			{
				result = (AUCusEntryNumber)Consol.Factory.New(typeof(AUCusEntryNumber));
				result.CE_ParentTable = Consol.TableName;
				result.CE_ParentID = Consol.PK;
				result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				result.CE_EntryType = entryNumberType;
			}
			return result;
		}

		protected ZString GetStatus(EDIMessage lastMessage)
		{
			ZString result = Constants.CMRConsolStatus.NotSent;
			if (lastMessage != null)
			{
				if (lastMessage.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit)
				{
					result = Constants.CMRConsolStatus.WaitingForResponse;
				}
				else if (lastMessage.EM_ReceiveTransmit == EDIInterchange.Direction.Receive)
				{
					switch (lastMessage.EM_MessageSubType)
					{
						case CMRMessage.ManifestResponseSubTypes.Clear:
							result = Constants.CMRConsolStatus.Clear;
							break;
						case CMRMessage.ManifestResponseSubTypes.Error:
							result = Constants.CMRConsolStatus.Errors;
							break;
						case CMRMessage.ManifestResponseSubTypes.Rejected:
							result = Constants.CMRConsolStatus.Rejected;
							break;
						case CMRMessage.ManifestResponseSubTypes.Revoked:
							result = Constants.CMRConsolStatus.Revoked;
							break;
						case CMRMessage.ManifestResponseSubTypes.Withdrawn:
							result = Constants.CMRConsolStatus.Withdrawn;
							break;
					}
				}
			}
			return result;
		}

		#endregion

	}
}
