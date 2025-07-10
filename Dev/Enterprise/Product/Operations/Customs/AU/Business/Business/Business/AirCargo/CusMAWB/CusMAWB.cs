using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusMAWB.Schema.CM_MAWB), DescriptionProperty(CusMAWB.Schema.CM_Description)]
	public class CusMAWB : CusMAWBBase,
		IDocManagerSupport,
		ICMRMessageRespondee,
		ICusUnderbondParent,
		ICusMAWBProvider,
		IOutturnableLine,
		IUnderbondMovementRequestHeaderProvider,
		IAirOutturnReportHeaderInformationProvider,
		IUnderbondDefaultValueProvider,
		ICusUnderbondNilUnderbondPerformer,
		IJobInvoicingPlugIn,
		IWorkflowProviderIncludingRelated,
		IJobHeaderParent,
		ICusUnderbondDependentCollectionParent,
		Customs.Business.IMessageManageableBizObj,
		IAUCusMAWB,
		IDataExportCSVFileNameProvider,
		IWorkflowTriggerEventSource,
		IDocManagerSupportIncudingRelatedObjects,
		IValidateForCustomsMessagingSupporter,
		IScanMasterBillProvider,
		ITriggerActionMessagingSupporter,
		ITriggerActionMessagingSupporterProvider,
		IControllerIDProvider,
		ITransitWarehouseSyncDataParent
	{
		#region Schema

		public abstract new class Schema : CusMAWBBase.Schema
		{
			public const string CM_Description = "CM_Description";
		}

		#endregion

		public CusMAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			if (CM_IsCTOMAWB)
			{
				ErrorReporter.ReportOnce("Attempting to load a CusMAWB from a record that is flagged CM_IsCTOMAWB", "Attempting to load a CusMAWB from a record that is flagged CM_IsCTOMAWB"); // Column names are in a string, which is okay
			}
			base.OnLoaded();
		}

		#region Load / Create

		public static CusMAWB CreateNew(ForwardingConsol consol)
		{
			var result = consol.Factory.New<CusMAWB>();
			result.CM_JK = consol.PK;
			result.SynchroniseData();
			return result;
		}

		public static CusMAWB Load(ForwardingConsol consol)
		{
			return consol.AUCusMAWB as CusMAWB;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			PartShips.RemoveAndDeleteAll();
			ChildBills.RemoveAndDeleteAll();
			JobHeader.DeleteAllJobs(this);
			base.Delete();
		}

		#endregion

		#region Properties

		protected override ZAddress GetNewCM_OA_UnpackDepotAddress_ZAddress()
		{
			var result = base.GetNewCM_OA_UnpackDepotAddress_ZAddress();
			result.GetDefaultAddress = (x) => { var org = x as OrgHeader; return org != null ? org.MainAddress.PK : ZGuid.Empty; };
			return result;
		}

		public ZString DischargeCTOID;

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZDateTime CM_ArrivalDate
		{
			get { return base.CM_ArrivalDate; }
			set
			{
				HasMAWBChangesOnly |= base.CM_ArrivalDate != value;
				base.CM_ArrivalDate = value;
			}
		}

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZString CM_FlightNo
		{
			get { return base.CM_FlightNo; }
			set
			{
				HasMAWBChangesOnly |= base.CM_FlightNo != value;
				var oldCarrier = CM_FlightNo.Left(2);
				base.CM_FlightNo = value;
				if (!IsCopying && oldCarrier != CM_FlightNo.Left(2))
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public override ZString CM_ApplicationCode
		{
			get { return base.CM_ApplicationCode; }
			set
			{
				bool isDiff = base.CM_ApplicationCode != value;
				base.CM_ApplicationCode = value;
				if (isDiff)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZString CM_MAWB
		{
			get { return base.CM_MAWB; }
			set
			{
				ZString originalValue = base.CM_MAWB;
				base.CM_MAWB = value;
				HasMAWBChangesOnly |= base.CM_MAWB != originalValue;
				if (originalValue != value)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZString CM_RL_NKDischargePort
		{
			get { return base.CM_RL_NKDischargePort; }
			set
			{
				HasMAWBChangesOnly |= base.CM_RL_NKDischargePort != value;
				var oldValue = CM_RL_NKDischargePort;
				base.CM_RL_NKDischargePort = value;
				if (!IsCopying && oldValue != CM_RL_NKDischargePort)
				{
					ResetCustomBusinessObject();
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZString CM_RL_NKLoadPort
		{
			get { return base.CM_RL_NKLoadPort; }
			set
			{
				HasMAWBChangesOnly |= base.CM_RL_NKLoadPort != value;
				var oldValue = CM_RL_NKLoadPort;
				base.CM_RL_NKLoadPort = value;
				if (!IsCopying && oldValue != CM_RL_NKLoadPort)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public override ZBool CM_IsBureau
		{
			get { return base.CM_IsBureau; }
			set
			{
				base.CM_IsBureau = value;
				foreach (CusUnderbond underbond in AllUnderbonds)
				{
					if (!underbond.AcknowledgedByCustoms)
					{
						underbond.C4_IsBureau = value;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(CM_ResponsiblePartyIDReadOnly))]
		public override ZString CM_ResponsiblePartyID
		{
			get
			{
				return base.CM_ResponsiblePartyID;
			}
			set
			{
				base.CM_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required by test - TestCM_OH_ResponsibleParty in CusMAWBTest.cs")]
		bool CM_ResponsiblePartyIDReadOnly
		{
			get { return !CM_OH_ResponsibleParty.IsEmpty; }
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			if (NeedUpdateFlightDetailFromCARST)
			{
				UpdateFlightDetailFromCARST(this);
				NeedUpdateFlightDetailFromCARST = false;
			}
			base.OnSaving();
		}

		internal bool NeedUpdateFlightDetailFromCARST;

		void UpdateFlightDetailFromCARST(CusMAWB targetBO)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			query.AddToFilter(CusHAWBSchema.CS_HAWB, targetBO.CM_MasterHouseBill);

			var subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			subQuery.AddToFilter(CusMAWBSchema.CM_MAWB, targetBO.CM_MAWB);
			subQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
			subQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, false);
			query.AddSubQuery(CusHAWBSchema.CS_CM, subQuery, JoinCondition.And);

			var hawb = Factory.LoadTop1<CusHAWB>(query);
			if (hawb != null)
			{
				var mawb = hawb.MAWB;
				var lastMessage = hawb.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.CARST, EDIMessage.Direction.Receive, EDIMessage.Status.Received);
				if (lastMessage != null)
				{
					var lastCASTMessage = lastMessage as CMRCARSTMessage;
					new AirCargoRecordLoaderAndCreator(lastCASTMessage.Factory).SetMAWBInfo(
						targetBO,
						mawb.CM_FlightNo,
						mawb.CM_ArrivalDate,
						mawb.CM_MAWB,
						mawb.CM_RL_NKDischargePort,
						mawb.DischargeCTOID,
						lastCASTMessage.EM_MessageNum);
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				HasMAWBChangesOnly = false;
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		#endregion

		#region Notes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.I;
				return result;
			}
		}

		#endregion

		#region Flags

		public void UpdateHouseMessageIsSent()
		{
			base.CM_HouseMessageIsSent = ChildBills.HasChildResponsePendingOrPrealerted;
			ReadOnly = CM_HouseMessageIsSent;
		}

		protected bool fHasMAWBChangesOnly;
		public bool HasMAWBChangesOnly
		{
			get { return fHasMAWBChangesOnly; }
			set { fHasMAWBChangesOnly = value; }
		}

		public override bool CanDelete
		{
			get { return !CM_HouseMessageIsSent && ChildBills.CanDelete; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("963B32B2-31A4-4D9B-B910-24587F6F7F23", "This Cargo Report Master may not be deleted because messages have been sent, or it has child bills for which messages have been sent."); }
		}

		public bool MasterBillNumChanged
		{
			get { return IsInDatabase && CM_MAWB != (ZString)CM_MAWBInfo.OriginalValue; }
		}

		public void SetReadOnly(bool propagateToChildren)
		{
			this.propagateToChildren = propagateToChildren;
			SetReadOnlyIncludingChildren(true);
		}
		bool propagateToChildren = true;

		protected override void UpdateChildReadOnlyWhenRegistering(IBusiness child)
		{
			if (propagateToChildren)
			{
				base.UpdateChildReadOnlyWhenRegistering(child);
			}
		}

		#endregion

		#region PlugIns

		/// <summary>
		/// This collection has only one CusHAWB that corresponds to Current Shipment in PlugIn situation
		/// </summary>
		public CusHAWBCollectionWithOneBill fCurrentHouseBills;
		public CusHAWBCollectionWithOneBill CurrentHouseBills
		{
			get
			{
				if (fCurrentHouseBills == null)
				{
					fCurrentHouseBills = new CusHAWBCollectionWithOneBill(this, Factory);
					fCurrentHouseBills.HouseBill = CurrentHouseBill;//this will trigger Load()
					fCurrentHouseBills.IsManagedForDataRefresh = true;
				}
				return fCurrentHouseBills;
			}
		}

		protected CusHAWB fCurrentHouseBill;
		public CusHAWB CurrentHouseBill
		{
			get { return fCurrentHouseBill; }
			set
			{
				fCurrentHouseBill = value;
				CurrentHouseBills.HouseBill = value;
			}
		}

		public void SynchroniseData()
		{
			var bridge = new MAWBToConsolBridge(this);

			if (!IsDeleted && Consol != null && !CM_HouseMessageIsSent && bridge.ShouldWeSynchronise)
			{
				if (CM_ArrivalDate.IsEmpty)
				{
					CM_ArrivalDate = Consol.JK_ArrivalForLastImportTransport;
				}

				if (CM_DepartureDate.IsEmpty)
				{
					CM_DepartureDate = Consol.JK_DepartureForTheFirstInternationalLeg;
				}

				if (CM_FlightNo.IsEmpty)
				{
					CM_FlightNo = Consol.JK_VoyageFlightForLastImportTransport;
				}

				if (CM_MAWB.IsEmpty)
				{
					CM_MAWB = Consol.JK_MasterBillNum.Left(CM_MAWBInfo.MaxLength);
				}

				if (CM_MasterHouseBill.IsEmpty)
				{
					CM_MasterHouseBill = Consol.IsCoLoad ? Consol.JK_CoLoadMasterBill : ZString.Empty;
				}

				if (CM_RL_NKDischargePort.IsEmpty || !IsInDatabase || !CM_RL_NKDischargePortInfo.HasChanges)
				{
					CM_RL_NKDischargePort = Consol.JK_RL_NKDiscForFirstImportTransport;
				}

				if (CM_RL_NKLoadPort.IsEmpty)
				{
					CM_RL_NKLoadPort = Consol.JK_RL_NKLoadForFirstImportTransport;
				}

				if (CM_OA_UnpackDepotAddress.IsEmpty)
				{
					CM_OA_UnpackDepotAddress = Consol.JK_OA_UnpackDepotAddress;
				}
			}
		}

		public ForwardingShipment[] GetShipmentsNotReferenceByHAWB()
		{
			ForwardingShipment[] result = null;
			var consol = Consol;
			if (consol != null)
			{
				var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				var jobConShipLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				jobConShipLinkQuery.AddToFilter(JobConShipLinkSchema.JN_JK, consol.PK);
				var cusHawbSubQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_JS, true);
				cusHawbSubQuery.AddToFilter(CusHAWBSchema.CS_CM, PK);
				cusHawbSubQuery.AddToFilter(CusHAWBSchema.CS_JS, SQLComparisonOperator.NotEqual, DBNull.Value);
				shipmentQuery.AddSubQuery(jobConShipLinkQuery, JoinCondition.And);
				shipmentQuery.AddSubQuery(cusHawbSubQuery, JoinCondition.And);
				result = Factory.Load<ForwardingShipment>(shipmentQuery);
			}
			return result;
		}

		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public new CusHAWBCollection ChildBills
		{
			get { return (CusHAWBCollection)base.ChildBills; }
		}

		protected override Customs.Business.CusHAWBDependentCollection GetNewChildBillsCollection()
		{
			return new CusHAWBCollection(this, Factory);
		}

		public override GlbBranch Branch
		{
			get
			{
				var result = base.Branch;
				if (result == null && CM_GB.IsEmpty)
				{
					result = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				}
				return base.Branch;
			}
		}

		public Transport Transport
		{
			get
			{
				Transport matchingTransport = null;

				foreach (Transport transport in Consol.Transports)
				{
					if (transport.JW_TransportMode == Core.Constants.TransportModes.Air
						&& transport.JW_VoyageFlight == CM_FlightNo)
					{
						matchingTransport = transport;
						break;
					}
				}

				if (matchingTransport == null)
				{
					matchingTransport = Consol.Transports[0];
				}

				return matchingTransport;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList bizORL = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				bizORL.AddRange(ChildBills.ToArray(ChildBills.TypeOfElements));
				return (BusinessObject[])bizORL.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region FilteredChildBills Collection

		public new CusHAWBFilteredCollection FilteredChildBills
		{
			get
			{
				return (CusHAWBFilteredCollection)base.FilteredChildBills;
			}
		}

		protected override Customs.Business.CusHAWBFilteredCollection GetNewFilteredChildBillsCollection()
		{
			return new CusHAWBFilteredCollection(this);
		}

		#endregion // FilteredChildBills Collection

		#region New Properties

		public override ZBool IsAltPartShipModelActive
		{
			get { return AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.Value; }
		}

		public CusUnderbond[] UnderbondsForDCL
		{
			get
			{
				if (underbondsForDCL == null)
				{
					underbondsForDCL = Underbonds.Cast<CusUnderbond>().Where(x => x.GetOutturnStatus() == OutturnStatus.Sent).ToArray();
				}
				return underbondsForDCL;
			}
		}
		CusUnderbond[] underbondsForDCL;

		internal void ResetUnderbondsForDCL()
		{
			underbondsForDCL = null;
			isDCLOutturnComplete = null;
		}

#if DEBUG
		public void ResetDCLUnderbondCacheForTest()
		{
			ResetUnderbondsForDCL();
			if (fUnderbonds != null)
			{
				fUnderbonds.CountChanged -= new CollectionCountChangedEventHandler(fUnderbonds_CountChanged);
				fUnderbonds = null;
			}
		}
#endif

		public bool IsDCLOutturnComplete
		{
			get
			{
				if (!isDCLOutturnComplete.HasValue)
				{
					bool result = ChildBills.Count > 0;
					foreach (CusHAWB child in ChildBills)
					{
						if (child.RemainingDCLOutturnQuantity > 0)
						{
							result = false;
							break;
						}
					}
					isDCLOutturnComplete = result;
				}
				return isDCLOutturnComplete.Value;
			}
		}
		bool? isDCLOutturnComplete;

		public string MessageErrorsString
		{
			get { return Notifications.GetMessageErrors().ToUniqueMessageListString(); }
		}

		public ZString CM_Description
		{
			get { return CM_FlightNo + " : " + CM_ArrivalDate.ToShortDateString(); }
		}

		public void GetHouseBillsToZerolandAndAmend(out StringBuilder houseBillsToZeroLand, out StringBuilder houseBillsToAmend)
		{
			houseBillsToZeroLand = new StringBuilder();
			houseBillsToAmend = new StringBuilder();
			foreach (CusHAWB houseBill in ChildBills)
			{
				if (houseBill.CS_IsPrealerted)
				{
					if (houseBill.HouseBillNumberChanged)
					{
						houseBillsToZeroLand.Append(houseBill.CS_HAWB + ",");
					}
					else if (houseBill.HasMessageChanges || houseBill.IsMasterFlagOrMasterHouseBillDifferent)
					{
						houseBillsToAmend.Append(houseBill.CS_HAWB + ",");
					}
				}
			}
		}

		public ZString Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("MASTER BILL DETAILS:\r\n");
				if (!CM_MAWB.IsEmpty)
				{
					builder.Append("MAWB: " + CM_MAWB + "\r\n");
				}

				if (LoadPort != null)
				{
					builder.Append("Load Port: " + LoadPort.Code + "\r\n");
				}

				if (DischargePort != null)
				{
					builder.Append("Discharge Port: " + DischargePort.Code + "\r\n");
				}

				return builder.ToString();
			}
		}

		public ZInt PiecesManifestedForAllHAWBs
		{
			get
			{
				int result = 0;
				foreach (CusHAWB hAWB in ChildBills)
				{
					result += hAWB.CS_PiecesManifested;
				}
				return result;
			}
		}

		public bool IsReportedToCustomsPreFlight
		{
			get
			{
				if (!isReportedToCustomsPreFlight)
				{
					isReportedToCustomsPreFlight = Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.ReportedToCustomsPreFlight.Code);
				}
				return isReportedToCustomsPreFlight;
			}
		}
		bool isReportedToCustomsPreFlight;

		public void ReportToCustomsPreFlight()
		{
			if (!IsReportedToCustomsPreFlight)
			{
				Logs.AddNew(Events.ReportedToCustomsPreFlight);
				isReportedToCustomsPreFlight = true;
			}
		}

		#endregion

		#region Property Overrides

		[RelatedBusinessObject(nameof(Consol))]
		public override ZGuid CM_JK
		{
			get { return base.CM_JK; }
			set
			{
				if (base.CM_JK != value)
				{
					fConsol = null;
					ChildBills.MarkAsNeedingValidation();
				}
				base.CM_JK = value;
			}
		}

		public override ZGuid CM_OH_ResponsibleParty
		{
			get { return base.CM_OH_ResponsibleParty; }
			set
			{
				base.CM_OH_ResponsibleParty = value;
				if (ResponsibleParty != null)
				{
					CM_ResponsiblePartyID = (!ResponsibleParty.PrimaryRegistrationNumber.Number.IsEmpty ? ResponsibleParty.PrimaryRegistrationNumber.Number : ResponsibleParty.GetCustomsClientID()).Replace(" ", "").Left(CM_ResponsiblePartyIDInfo.MaxLength);
				}
				else
				{
					CM_ResponsiblePartyID = ZString.Empty;
				}
			}
		}

		protected virtual bool ShouldFieldsBeReadonly
		{
			get { return CM_HouseMessageIsSent; }
		}

		[ReadOnlyMember(nameof(ShouldFieldsBeReadonly))]
		public override ZString CM_MasterHouseBill
		{
			get { return base.CM_MasterHouseBill; }
			set
			{
				bool isDiff = base.CM_MasterHouseBill != value;
				base.CM_MasterHouseBill = value;
				if (isDiff)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region Validation

		public void RunMAWBValidations()
		{
			Validation.ValidateCM_ArrivalDate();
			Validation.ValidateCM_FlightNo();
			Validation.ValidateCM_MAWB();
			Validation.ValidateCM_RL_NKDischargePort();
			Validation.ValidateCM_RL_NKLoadPort();
		}

		#endregion

		#region BulkAllocateReferenceNumbersForChildBills

		public void BulkAllocateReferenceNumbersForChildBills()
		{
			var query = new ZQuery(CusHAWBSchema.CS_CM, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var childBills = Factory.Load<CusHAWB>(query).Where(x =>
					!x.IsDeleted && !x.IsDeleting && !x.IsInDatabase && x.CS_MessageReference.IsEmpty &&
					x.Shipment == null)
				.ToArray();
			if (childBills.Length > 0)
			{
				var messageReferences = Env.NumberFountains.AUAirCargoJobNumber.GetNextsFormatted(Factory, childBills.Length);
				for (int index = 0; index < messageReferences.Length; index++)
				{
					childBills[index].CS_MessageReference = messageReferences[index];
				}
			}
		}

		#endregion

		#region Implementation

		public override void DefaultFromConsol()
		{
			SynchroniseData();
		}

		protected override OrgHeader GetEffectiveResponsibleParty()
		{
			var result = base.GetEffectiveResponsibleParty();
			if (result == null && !CM_ResponsiblePartyID.IsEmpty)
			{
				var allOrgCusCodes = GetOrgCusCodesFromABN();
				if (allOrgCusCodes.Length == 1)
				{
					result = allOrgCusCodes[0].Header;
				}
			}
			return result;
		}

		protected override ZString GetReasonEffectiveResponsiblePartyIsUnavailable()
		{
			var result = ZString.Empty;
			if (CM_ResponsiblePartyID.IsEmpty)
			{
				result = base.GetReasonEffectiveResponsiblePartyIsUnavailable();
			}
			else
			{
				var allOrgCusCodes = GetOrgCusCodesFromABN();
				if (allOrgCusCodes.Length == 0)
				{
					result = Res.GetString("536f802a-a559-459e-bdef-97764117df2a", "Cannot find any organization with ABN [{0}]", CM_ResponsiblePartyID);
				}
				else if (allOrgCusCodes.Length > 1)
				{
					result = Res.GetString("4288cf0b-9207-4ea4-afc3-2d667a63e640", "There are multiple organizations with the same ABN [{0}]", CM_ResponsiblePartyID);
				}
			}
			return result;
		}

		OrgCusCode[] GetOrgCusCodesFromABN()
		{
			var loader = new OrgCusCode.Loader(Factory);
			return loader.Load(countryCode: Core.Constants.CountryCodes.Australia,
							   codeType: OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
							   regoNumber: CM_ResponsiblePartyID);
		}

		protected override OrgHeader GetDeConsolidator()
		{
			OrgHeader result = null;
			var premiseID = GetPremiseID();
			if (!premiseID.IsEmpty)
			{
				var loader = new OrgCusCode.Loader(Factory);
				result = loader.Load(countryCode: Core.Constants.CountryCodes.Australia,
									 codeType: OrgCusCode.CodeTypes.ControlledPremisesID,
									 regoNumber: premiseID)
							   .Select(orgCusCode => orgCusCode.Header)
							   .FirstOrDefault();
			}
			return result;
		}

		ZString GetPremiseID()
		{
			var result = Underbonds.Cast<CusUnderbond>()
								   .Where(underbond => underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination)
								   .Select(underbond => underbond.C4_DestinationPremiseID)
								   .FirstOrDefault();
			if (result.IsEmpty)
			{
				var depotAddress = UnpackDepotAddress;
				if (depotAddress != null)
				{
					result = depotAddress.LocalControlledPremisesID;
				}
			}
			if (result.IsEmpty)
			{
				result = AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs.GetPremiseIDMatchingDischargePort(CM_FlightNo, CM_RL_NKDischargePort);
			}
			if (result.IsEmpty)
			{
				var hawbs = ChildBills.Cast<CusHAWB>();
				if (!hawbs.IsNullOrEmpty())
				{
					var destination = hawbs.First().CS_RL_NKDestination;
					if (hawbs.All(hawb => hawb.CS_RL_NKDestination == destination))
					{
						result = AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs.GetPremiseIDMatchingDestinationPort(CM_FlightNo, destination);
					}
				}
			}

			return result;
		}

		protected MessageValidation fMessageValidation;
		protected MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(this);
				}
				return fMessageValidation;
			}
		}

		protected override Customs.Business.CusMAWBValidation GetNewValidation()
		{
			return new CMRCusMAWBValidation(this);
		}

		protected override bool IsStandAloneCore
		{
			get { return Consol == null; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (AUCustomsDataRegistry.Instance.DefaultAirConsolResponsibleParty.Value && GlbCompany.CurrentCompany.OrgProxy != null && !GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.IsEmpty)
			{
				CM_OH_ResponsibleParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			}
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusMAWBUnderbondMovementRequestHeader(underbond, this);
		}

		public bool IsBureau
		{
			get { return CM_IsBureau; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent

		[ChildEditable(false)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.CountChanged += new CollectionCountChangedEventHandler(fUnderbonds_CountChanged);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds
		{
			get { return Underbonds; }
		}

		void fUnderbonds_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CusUnderbond underbond = e.BizObject as CusUnderbond;
			if (e.ItemAdded && underbond != null)
			{
				underbond.C4_IsBureau = CM_IsBureau;
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get
			{
				CMREdiMessageFunctions messageFunctions = new CMREdiMessageFunctions();
				return messageFunctions.DoMessagesContainAnyCARSTs(Messages);
			}
		}

		public ZString UnderbondHumanReadableName
		{
			get { return "MasterBill" + (CM_MAWB.IsEmpty ? "" : (" " + CM_MAWB)); }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				IOutturnableLine[] result = new IOutturnableLine[ChildBills.Count + 1];
				result[0] = this;
				int index = 1;
				foreach (CusHAWB hAWB in ChildBills)
				{
					result[index++] = hAWB;
				}
				return result;
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return true; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return CM_RL_NKDischargePort; }
		}

		#endregion

		#region IAirOutturnReportHeaderInformationProvider Members

		IAirOutturnReportHeaderInformation IAirOutturnReportHeaderInformationProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusMAWBOutturnReportHeaderInformation(this, underbond);
		}

		#endregion

		#region GetAllPossibleCollectionProvidersCore

		protected override ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore()
		{
			ArrayList result = new ArrayList();
			result.Add(this);
			result.AddRange(PartShips);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		#endregion

		#region ICusMAWBProvider

		CusMAWB ICusMAWBProvider.MAWB
		{
			get { return this; }
		}

		#endregion

		#region IUnderbondDefaultValueProvider Members

		void IUnderbondDefaultValueProvider.SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_FlightNo = CM_FlightNo;
			underbond.C4_IsBureau = CM_IsBureau;
			ZShort packageCount = 0;
			foreach (CusHAWB hAWB in ChildBills)
			{
				if ((int)packageCount + (int)hAWB.CS_PiecesManifested > short.MaxValue)
				{
					underbond.C4_PiecesManifested = 0;
					return;
				}

				if (hAWB.CS_MasterHouseBill.IsEmpty)
				{
					packageCount += hAWB.CS_PiecesManifested;
				}
			}

			underbond.C4_PiecesManifested = packageCount;
			underbond.C4_OriginPremiseID = Consol != null && Consol.ArrivalCTOAddress != null ? Consol.ArrivalCTOAddress.LocalControlledPremisesID : ZString.Empty;

			var ccpCode = UnpackDepotAddress != null ? UnpackDepotAddress.LocalControlledPremisesID : ZString.Empty;
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			underbond.C4_DestinationPremiseID = ccpCode.IsEmpty ? (orgProxy != null ? orgProxy.MainAddress.LocalControlledPremisesID : ZString.Empty) : ccpCode;
		}

		#endregion

		#region ICusUnderbondNilUnderbondPerformer Members

		ZString ICusUnderbondNilUnderbondPerformer.PerformNilUnderbond(Customs.Business.CusUnderbond underbond)
		{
			ZString result = ZString.Empty;
			if (!underbond.IsDeleted)
			{
				CusUnderbond currentUnderbond = (CusUnderbond)underbond;
				underbond.C4_Outurned = ZDateTime.Now;

				foreach (CusHAWB hAWB in ChildBills)
				{
					if (!OutturnLineAlreadyExists(hAWB.PK, currentUnderbond))
					{
						CusOutturn outturn = currentUnderbond.Outturns.AddNew();
						outturn.Parent = hAWB;
						outturn.C5_OuterPacks = hAWB.CS_PiecesManifested;
						outturn.C5_PackagesOutturned = hAWB.CS_PiecesManifested;
						outturn.C5_GoodsDescription = hAWB.CS_GoodsDescription;
						SetOutturnResultType(hAWB, outturn);
					}
				}
			}
			return result;
		}

		protected virtual void SetOutturnResultType(CusHAWB hAWB, CusOutturn outturn)
		{
		}

		bool OutturnLineAlreadyExists(ZGuid parentId, CusUnderbond underbond)
		{
			foreach (CusOutturn outturnLine in underbond.Outturns)
			{
				if (outturnLine.C5_ParentID == parentId)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return "M" + CM_MAWB; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CusMAWBInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CusMAWBInvoicingSupporter(this)); }
		}

		#endregion

		#region IWorkflowProvider Members

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusMAWBProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>(this);
		}

		public IEnumerable<IWorkflowProvider> RelatedIWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();

				result.AddRange(AllUnderbonds.OfType<IWorkflowProvider>());

				result.AddRange(ChildBills.OfType<IWorkflowProvider>());

				return result;
			}
		}
		protected override IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			ColumnValueRanker result = this.GetJobRelatedTemplateSelectionCriteria();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, CM_FlightNo.SubstringSafe(0, 2), ZString.Empty);
			return result;
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusMAWBMessageManager(delegate
			{ return this; });
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return CM_MAWB; }
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo GetNewDocManagerInfo()
		{
			return new DocManagerIncludingRelatedObjectsInfo(this, Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster);
		}

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference
		{
			get { return this; }
		}

		IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var bill in ChildBills)
			{
				yield return bill;
			}
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "AirCargo Report";

				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty("MAWB: ", FormattedMAWB);
				parameters.AppendIfNotEmpty("MHB: ", CM_MasterHouseBill);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}

				return result;
			}
		}

		public ZString FormattedMAWB
		{
			get { return CM_MAWB.Length <= 3 ? CM_MAWB : new ZString(CM_MAWB.Left(3) + "-" + CM_MAWB.Substring(3)); }
		}

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				var branch = Branch;
				return branch == null ? null : branch.Company;
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var consol = Consol;
				if (consol != null)
				{
					list.Add(consol);
				}
				return list;
			}
		}

		#endregion

		#region IScanMasterBillProvider

		ZString IScanMasterBillProvider.MasterBill
		{
			get { return CM_MAWB; }
		}

		ZString IScanMasterBillProvider.MasterHouseBill
		{
			get { return CM_MasterHouseBill; }
		}

		ZBool IScanMasterBillProvider.IsStandAlone
		{
			get { return this.IsStandAlone(); }
		}

		IEnumerable<IScanHouseBillProvider> IScanMasterBillProvider.GetChildBills(CusUnderbond underbond)
		{
			return ChildBills.Cast<IScanHouseBillProvider>();
		}

		IEnumerable<CusUnderbond> IScanMasterBillProvider.Underbonds
		{
			get { return Underbonds.Cast<CusUnderbond>(); }
		}

		void IScanMasterBillProvider.CreateSurplusConsignment(BusinessObjectFactory factory, OutturnLine outturn)
		{
			var house = factory.New<CusHAWB>();
			ChildBills.Add(house);
			house.IsSurplus = true;
			house.CS_HAWB = outturn.ConsignmentRef;
			house.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			house.CS_GoodsDescription = "SURPLUS GOODS";
			outturn.HouseBill = house;
			outturn.ManifestInfo = house;
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion

		#region ITriggerActionMessagingSupporter

		void ITriggerActionMessagingSupporter.SendMessage(INotifications notifications, ZString queuedUserNK, ZString triggerAction)
		{
			AddDeferredScheduledMessagesEvent(triggerAction);
		}

		#endregion

		#region ITriggerActionMessagingSupporterProvider

		ITriggerActionMessagingSupporter ITriggerActionMessagingSupporterProvider.GetSupporter(string triggerAction)
		{
			return this;
		}

		#endregion

		#region IControllerIDProvider

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.AU.AirCargo; }
		}

		#endregion

		public ZString GetOutturnsReadyForSendingMessage()
		{
			if (FilteredChildBills.Count == 0)
			{
				return "There are no House Bills";
			}

			var allOutturns = AllUnderbonds.Cast<CusUnderbond>().SelectMany(u => u.Outturns.Cast<CusOutturn>()).ToArray();
			foreach (CusHAWB bill in FilteredChildBills)
			{
				if (!allOutturns.Any(o => o.C5_ParentID == bill.PK))
				{
					return $"HouseBill {bill.CS_HAWB} doesn't have matching outturn record in Underbond";
				}
			}

			return ZString.Empty;
		}

		public void LogOutturnsReadyForSendingEvent(IXmlImportLogger logger)
		{
			var errorMessage = GetOutturnsReadyForSendingMessage();
			if (errorMessage.IsEmpty)
			{
				Logs.AddNew(ZArchitecture.Business.Events.AUOutturnReadyForSending);
				logger.Log(LogType.Information, Res.GetString("4EC78929-3CCB-4D5B-AAAF-70701E82DB52", "Added event 'AUT' for Air Cargo {0}.", CM_MAWB));
			}
		}

		#region IAUCusMAWB

		public ZString GetOutturnsReconciliationMessage()
		{
			var allOutturns = AllUnderbonds.Cast<CusUnderbond>().SelectMany(u => u.Outturns.Cast<CusOutturn>()).ToArray();
			foreach (CusHAWB bill in FilteredChildBills)
			{
				if (!allOutturns.Any(o => o.C5_ParentID == bill.PK && o.C5_PackagesOutturned == bill.CS_PiecesManifested))
				{
					return $"HouseBill {bill.CS_HAWB} (packages manifested {bill.CS_PiecesManifested}) doesn't match packages outturned in Underbond";
				}
			}

			return ZString.Empty;
		}

		#endregion
	}
}
