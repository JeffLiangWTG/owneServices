using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[SystemDefinedValues]
	[CodeProperty(Enterprise.Customs.Business.CusPartShip.Schema.CG_MessageReference)] // Was SplitReference.... but that doesn't allow ListValidation on CommunityHandlingCode to work well
	[DescriptionProperty("HumanReadableNameConcrete")]
	public abstract class SplitConsignment : CusPartShip, ICcsukCusAwb, IDocManagerSupport, IDocumentSupportable, IAgentBadgeValidationProvider, IMessageManageableBizObj
	{
		public SplitConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusPartShip.Schema
		{
			public const string HandlingInformation = "HandlingInformation";
			public const string LatestCustomsActionText = "LatestCustomsActionText";
			public const string TemporaryStorageEndDate = "TemporaryStorageEndDate";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!IsInDatabase)
			{
				LocalCreationDate = ZDateTime.Now;
			}
		}

		void ICcsukCusAwb.DeactivateByWtg()
		{ }

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.PresenceOnNetworkList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString PresenceOnNetworkStatus
		{
			get { return CG_Status; }
			set
			{
				CG_Status = value;
				if (value == PresenceOnNetworkList.Codes.OnCommDb && AWB != null)
				{
					CcsukUtilities.UpdatePresenceToYesIfCurrentlyTransientOrNegative(AWB);
				}
			}
		}

		public ZDecimal Weight
		{
			get { return CG_GrossWeight; }
			set { CG_GrossWeight = value; }
		}
		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(Schema.CG_GrossWeight, "Weight"); }
		}

		[MaxLength(2)]
		public ZString WeightCode
		{
			get { return CG_GrossWeightUQ; }
			set { CG_GrossWeightUQ = value.Left(2); }
		}
		public ZPropertyInfo WeightCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CG_GrossWeightUQ, "Weight Unit"); }
		}

		[MaxLength(2)]
		public ZString SplitReference
		{
			get { return CG_MessageReference; }
			set { CG_MessageReference = value; }
		}
		public ZPropertyInfo SplitReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SplitReference), x => CG_MessageReferenceInfo); }
		}

		protected abstract ICcsukCusAwb AwbCore { get; set; }
		public ICcsukCusAwb AWB
		{
			get { return AwbCore; }
			set { AwbCore = value; }
		}

		public EDIMessageCollection Messages
		{
			get { return AWB != null ? AWB.Messages : new EDIMessageCollection(this); }
		}

		public ZString CargoTerminalOperatorAirport
		{
			get { return AWB != null ? AWB.CargoTerminalOperatorAirport : ZString.Empty; }
		}

		public ZString CargoTerminalOperator
		{
			get { return AWB != null ? AWB.CargoTerminalOperator : ZString.Empty; }
		}

		public ZString CargoTerminalOperatorAirportAndShed
		{
			get { return AWB != null ? AWB.CargoTerminalOperatorAirportAndShed : ZString.Empty; }
		}

		public ZString MasterBill
		{
			get { return AWB != null ? AWB.MasterBill : ZString.Empty; }
		}

		public ZBool HasSplits
		{
			get { return false; }
		}

		public ZString DescriptionOfGoods
		{
			get { return AWB != null ? AWB.DescriptionOfGoods : ZString.Empty; }
		}

		[MaxLength(3)]
		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		public ZString AgentBadge
		{
			get { return CG_FlightNo; }
			set
			{
				if (CG_FlightNo != value)
				{
					CheckMaximumLength(AgentBadgeInfo, value); // you are likely missing this line!
					CG_FlightNo = value;
					AgentBadgeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AgentBadgeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CG_FlightNo, x => CG_FlightNoInfo); }
		}

		[MaxLength(3)]
		public override ZString CG_FlightNo
		{
			get { return base.CG_FlightNo; }
			set { base.CG_FlightNo = value; }
		}

		public override ZPropertyInfo CG_FlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.CG_FlightNo, "Nominated Agent"); }
		}

		public bool AgentBadgeReadOnly
		{
			get { return true; }
		}

		public ZString ReferenceNumber
		{
			get { return string.Format("{0}/{1}", AWB != null ? AWB.ReferenceNumber : ZString.Empty, SplitReference); }
		}

		public GlbBranch Branch
		{
			get { return AWB != null ? AWB.Branch : null; }
		}

		public ICuscar GetCuscarWrapper()
		{
			return GetCuscarWrapperCore();
		}

		protected virtual ICuscar GetCuscarWrapperCore() { return null; }

		public ZString ReasonForNotAllowSplit
		{
			get { return "A split cannot be split, instead extend the parent consignment's split"; }
		}

		public CusUnderbondCollection<InterAirportRemoval> IARs
		{
			get { return IARsCore; }
		}

		public CusUnderbondCollection<InterShedRemoval> ISRs
		{
			get { return ISRsCore; }
		}

		public CusUnderbondCollection<TranshipmentRemoval> TSRs
		{
			get { return TSRsCore; }
		}

		public CusUnderbondCollection<Fallback> FBKs
		{
			get { return FBKsCore; }
		}

		public bool UpdateStatusToCacIfAllowed(ZString customsActionCode, ZDateTime customsActionDate, ZString agentReference, ZString customsActionText)
		{
			return new ConsignmentStatusUpdater(this).Update(customsActionCode, customsActionDate, agentReference, customsActionText);
		}

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString LatestCustomsActionText
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.CustomsActionTextAddOnTypeCode);
			set
			{
				var oldValue = LatestCustomsActionText;
				CheckMaximumLength(LatestCustomsActionTextInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.CustomsActionTextAddOnTypeCode, value);
				LatestCustomsActionTextInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LatestCustomsActionTextInfo => GetZPropertyInfo(Schema.LatestCustomsActionText);

		[List(nameof(CACList))]
		[MaxLength(2)]
		public ZString CustomsActionCode
		{
			get { return CG_CustomsStatus; }
		}

		EnterpriseBusinessObject ICcsukCusAwb.ForwardingParent => AWB?.ForwardingParent;

		public void SetCustomsActionCode(ZString code, ZDateTime date)
		{
			CG_CustomsStatus = code;
			CusHAWB.LogCustomsActionCodeEvent(code, date.ToOffset(), Logs, PK);
			if (AWB != null && !code.IsEmpty)
			{
				AWB.SetCustomsActionCode(CustomsStatusCodes.Codes._CargoWise_ConsignmentHasSplits, ZDateTime.Empty);
			}
			if (!Status1Date.IsEmpty && CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(CustomsActionCode))
			{
				((ICcsukCusAwb)this).CompleteOnCcsuk();
			}
		}

		public ZDateTime CustomsActionDate
		{
			get { return CusHAWB.GetCustomsActionDate(CustomsActionCode, PK, Logs); }
		}

		public void Split(List<ICuscarLine> fcsLinesHowToSplit)
		{
			throw new System.NotSupportedException(ReasonForNotAllowSplit);
		}

		public SplitCollection Splits
		{
			get { return new SplitCollection(Factory); } // will be empty
		}

		public IFSR GetAwbToFsrProvider(CcsukTransmissionMessageFunction how)
		{
			return new SplitToFsrProvider(this, how.MessageSubType);
		}

		public GlbStaff UserInChargeOfJob
		{
			get { return AWB != null ? AWB.UserInChargeOfJob : null; }
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;

		public ZString Profile
		{
			get { return AWB != null ? AWB.Profile : null; }
		}

		protected abstract CusUnderbondCollection<InterAirportRemoval> IARsCore { get; }
		protected abstract CusUnderbondCollection<TranshipmentRemoval> TSRsCore { get; }
		protected abstract CusUnderbondCollection<InterShedRemoval> ISRsCore { get; }
		protected abstract CusUnderbondCollection<Fallback> FBKsCore { get; }

		public ZBool IsThroughAwb
		{
			get { return AWB.IsThroughAwb; }
		}

		public ZShort NumberOfPiecesExpected
		{
			get { return CG_PiecesManifested; }
			set
			{
				if (CG_PiecesManifested != value)
				{
					CG_PiecesManifested = value;
					if (value > 0 && value == CG_PiecesLanded)
					{
						if (Status1Date.IsEmpty && !NumberOfPiecesReceivedReadOnly)
						{
							Status1Date = ZDateTime.Now;
						}
					}
					else
					{
						CusHAWB.WipeStatus1DateAndUncomplete(out status1Date, Logs, Status1DateInfo, this);
					}
				}
			}
		}

		public ZPropertyInfo NumberOfPiecesExpectedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CG_PiecesManifested, x => CG_PiecesManifestedInfo); }
		}
		public override ZPropertyInfo CG_PiecesManifestedInfo  // we have to override the persistent member's info to rename the wrapped member
		{
			get { return GetZPropertyInfo(Schema.CG_PiecesManifested, "NPX"); }
		}

		[ReadOnlyMember(nameof(NumberOfPiecesReceivedReadOnly))]
		public ZShort NumberOfPiecesReceived
		{
			get { return CG_PiecesLanded; }
			set
			{
				if (CG_PiecesLanded != value)
				{
					CG_PiecesLanded = value;
					if (value > 0 && value == CG_PiecesManifested)
					{
						if (Status1Date.IsEmpty && !NumberOfPiecesReceivedReadOnly)
						{
							Status1Date = ZDateTime.Now;
						}
					}
					else
					{
						CusHAWB.WipeStatus1DateAndUncomplete(out status1Date, Logs, Status1DateInfo, this);
					}
				}
			}
		}

		public ZPropertyInfo NumberOfPiecesReceivedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CG_PiecesLanded, x => CG_PiecesLandedInfo); }
		}
		public override ZPropertyInfo CG_PiecesLandedInfo  // we have to override the persistent member's info to rename the wrapped member
		{
			get { return GetZPropertyInfo(Schema.CG_PiecesLanded, "NPR"); }
		}

		public bool NumberOfPiecesReceivedReadOnly
		{
			get { return !(LicenceAndPimaHelper.IsFullShed(this) || LicenceAndPimaHelper.IsFallbackShed(this)); }  // Do not worry about whether the parent Awb has red errors on Pima filed - user cannot open the split form when there are unsaved changes, and cannot save with red errors on that field, so cannot open the split wehn parent has pima errors
		}

		public ZString AirportOfOrigin
		{
			get { return AWB.AirportOfOrigin; }
			set { AWB.AirportOfOrigin = value; }
		}
		ZPropertyInfo Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.AirportOfOriginInfo
		{
			get { return AWB.AirportOfOriginInfo; }
		}

		public ZString AirportOfArrival
		{
			get { return AWB.AirportOfArrival; }
			set { AWB.AirportOfArrival = value; }
		}
		ZPropertyInfo Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.AirportOfArrivalInfo
		{
			get { return AWB.AirportOfArrivalInfo; }
		}

		public ZString AirportOfDestination
		{
			get { return AWB.AirportOfDestination; }
			set { AWB.AirportOfDestination = value; }
		}
		ZPropertyInfo Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.AirportOfDestinationInfo
		{
			get { return AWB.AirportOfDestinationInfo; }
		}

		public ZString ShipmentDescriptionCode
		{
			get { return AWB.ShipmentDescriptionCode; }
			set { AWB.ShipmentDescriptionCode = value; }
		}

		public ZString ConsignmentOrEntryType
		{
			get { return AWB.ConsignmentOrEntryType; }
			set { AWB.ConsignmentOrEntryType = value; }
		}

		protected override ZString HumanReadableNameCore => NumberOfPiecesExpected != 1
			? Res.GetString("0DADD418-4F80-45F8-BA3F-1F6BE796C46A", "{0} ({1} pieces)", ReferenceNumber, NumberOfPiecesExpected)
			: Res.GetString("A4FE7E26-9B4D-4118-846D-24D90441E5FF", "{0} (1 piece)", ReferenceNumber);

		public ZString HumanReadableNameConcrete
		{
			get { return HumanReadableNameCore; }
		}

		public ZString HumanReadableNameForFormCaption => Res.GetString("733F96E5-B7FD-4C81-949B-AE898C169978", "Split {0} {1}", ConsignmentParentName, HumanReadableNameCore);

		public ZString ReferenceNumberWithShed
		{
			get { return string.Format("{0}/{1}", AWB != null ? AWB.ReferenceNumberWithShed : ZString.Empty, SplitReference); }
		}

		public CodeDescriptionPairList CACList
		{
			get { return new CustomsStatusCodes(); }
		}

		public ControllerID ModuleControllerId
		{
			get { return ModuleControllerIdCore; }
		}

		protected abstract ControllerID ModuleControllerIdCore { get; }

		protected abstract ZString ConsignmentParentName { get; }

		BusinessObjectCollection ICcsukCusAwb.OutTurnsCollection
		{
			get { return new CusOutTurnCollectionForSplit(this); }
		}

		CusOutTurnList ICcsukCusAwb.OutTurns
		{
			get
			{
				var list = new CusOutTurnList();
				if (AWB != null && AWB.OutTurns != null)
				{
					list.AddRange((from CusOutTurn cot in AWB.OutTurns where cot.SplitReferenceToWhichThisPertains == this.SplitReference select cot));
				}
				return list;
			}
		}

		public ZBool IsLodgedAtCcsuk
		{
			get { return AWB.IsLodgedAtCcsuk && this.IsInDatabase; }
		}

		public ZBool IsLodgedOrAssumedAtCcsuk
		{
			get { return (AWB.IsLodgedOrAssumedAtCcsuk) && this.IsInDatabase; }
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new CcsukDocumentSupporter(this); }
		}

		DocManagerInfo docManagerInfo;
		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new SplitConsignmentDocManagerInfo(this)); }
		}

		public ZString HandlingInformationForBinding
		{
			get
			{
				var detailedHandlingInfomation = ZString.Empty;
				if (((ICcsukCusAwb)this).OutTurns.Count > 0)
				{
					var sb = new ZStringBuilder();
					foreach (var ot in ((ICcsukCusAwb)this).OutTurns) // already filters by split#
					{
						var piecesAndType = string.Format("{0}{1}", ot.C5_PackagesOutturned, ot.C5_PackagesUnits);
						var where = ot.ShedStorageLocationForRraDocument.IsEmpty ? "" : " in " + ot.ShedStorageLocationForRraDocument;
						var marked = ot.C5_MarksAndNumbers.IsEmpty ? "" : " marked " + ot.C5_MarksAndNumbers;
						var overall = string.Format("{0} {1} {2}", piecesAndType.Trim(), marked.Trim(), where.Trim());
						overall = overall.Replace("  ", " ");
						sb.Append(overall.Trim());
					}
					detailedHandlingInfomation = sb.Length > 1 ? sb.ToStringWithDelimiterBetweenAppends("...\r\n") : sb.ToString();
				}
				return detailedHandlingInfomation.IsEmpty ? HandlingInformation : detailedHandlingInfomation;
			}
		}

		public ZPropertyInfo HandlingInformationForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(HandlingInformationForBinding)); }
		}

		[MaxLength(38)]
		[ReadOnly(true)]
		public ZString HandlingInformation
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.HandlingInformationCodeType);
			set
			{
				var oldValue = HandlingInformation;
				CheckMaximumLength(HandlingInformationInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.HandlingInformationCodeType, value);
				HandlingInformationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo HandlingInformationInfo => GetZPropertyInfo(Schema.HandlingInformation);

		public bool IsHandlingInformationReadOnly
		{
			get { return AWB == null || LicenceAndPimaHelper.IsSimpleAgentProfile(AWB); }
		}

		ZBool ICcsukCusAwb.IsCompleteOnCcsuk
		{
			get { return PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.CompletedOnCcsUk; }
		}

		ZBool ICcsukCusAwb.IsArchivedOnCcsuk
		{
			get { return PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.ArchivedOnCcsuk; }
		}

		ZBool ICcsukCusAwb.IsEntryCancelled
		{
			get
			{
				var isEntryCancelled = false;
				if (CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestCancelled && OwnDeclaration != null && OwnDeclaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
				{
					var entry = OwnDeclaration.CustomsEntryHeaders.FirstOrDefault();
					isEntryCancelled = entry?.IsCancelledWithCustoms ?? false;
				}
				return isEntryCancelled;
			}
		}

		void ICcsukCusAwb.ArchiveOnCcsuk(ReasonForArchiving why)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ArchivedOnCcsuk;
			Logs.AddNew(Events.DocumentDeleted, CcsukUtilities.GetEnumDescription(why), ZDateTimeOffset.Now);
			if (why != ReasonForArchiving.LastChildRecordWasArchived)
			{
				CcsukUtilities.ArchiveParentIfLastChildIsNowArchived(this, AWB, AWB.Splits.Cast<ICcsukCusAwb>());
			}
		}

		void ICcsukCusAwb.CompleteOnCcsuk(bool propagateToParent)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			Logs.AddNew(Events.TaskCompleted, "Completed on CCSUK", ZDateTimeOffset.Now);
			if (propagateToParent)
			{
				CcsukUtilities.CompleteParentIfLastChildIsNowComplete(this, AWB, AWB.Splits.Cast<ICcsukCusAwb>());
			}
		}

		void ICcsukCusAwb.UncompleteOnCcsuk(bool propagateToParent)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Logs.AddNew(Events.TaskCompleted, "Revoked completion on CCSUK", ZDateTimeOffset.Now);
			if (propagateToParent)
			{
				if (AWB.IsCompleteOnCcsuk)
				{
					AWB.UncompleteOnCcsuk();
				}
			}
		}

		ZDateTime status1Date;
		[ReadOnly(true)]
		[BusinessObjectTestExclude] // See SplitHouseTests.TestStatus1() and SplitBasicTests.TestStatus1() for thorough - and proper - testing.  Base tests expect us to record invalid dates. 
		public ZDateTime Status1Date
		{
			get
			{
				if (status1Date.IsEmpty)
				{
					status1Date = CusHAWB.GetStatusDate("ST1", Logs);
				}
				return status1Date;
			}
			set
			{
				CusHAWB.SetStatus1Date(value.ToOffset(), Logs, Status1DateInfo, out status1Date, this);
			}
		}

		public ZPropertyInfo Status1DateInfo
		{
			get { return GetZPropertyInfo(nameof(Status1Date)); }
		}

		[ReadOnly(true)]
		public ZDateTime TemporaryStorageEndDate
		{
			get => this.GetSystemDefinedValue<ZDateTime>(Schema.TemporaryStorageEndDate);
			set
			{
				var oldValue = TemporaryStorageEndDate;
				this.SetSystemDefinedValue(Schema.TemporaryStorageEndDate, value);
				TemporaryStorageEndDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TemporaryStorageEndDateInfo
		{
			get => GetZPropertyInfo(Schema.TemporaryStorageEndDate);
		}

		CusAwbIsReadOnlyHelper readOnlyAndPermissionHelper;
		public CusAwbIsReadOnlyHelper ReadOnlyAndPermissionHelper
		{
			get { return readOnlyAndPermissionHelper ?? (readOnlyAndPermissionHelper = new CusAwbIsReadOnlyHelper(this)); }
		}

		public ZInt NumberOfPiecesReleasedSoFarCumulative(Event eventType)
		{
			return NumberOfPiecesReleasedHelper.GetTotalPiecesReleased(this, eventType);
		}

		public void ReleaseThisNumberOfPieces(ZInt pieces, Event type)
		{
			NumberOfPiecesReleasedHelper.ReleaseMorePieces(this, pieces, type);
		}

		public ZBool IsPrearrival
		{
			get { return AWB != null && AWB.IsPrearrival && NumberOfPiecesReceived == 0; }
		}

		public ZDateTime LocalCreationDate
		{
			get { return this.CG_ArrivalDate; }
			set { CG_ArrivalDate = value; }
		}

		public ZDateTime LastEditTime
		{
			get { return ZDateTime.Empty; }
		}

		public ZString ChiefDeclarationUCR
		{
			get { return HasOwnDeclaration ? OwnDeclaration.JE_UCR : ZString.Empty; }
		}

		bool ICcsukCusAwb.HasEntryWithLodgedOrPrelodgedWithCustoms
		{
			get
			{
				var hasEntryWithLodgedOrPrelodgedWithCustoms = false;
				if (HasOwnDeclaration && OwnDeclaration.CustomsEntryHeaders.Count > 0)
				{
					var entry = OwnDeclaration.CustomsEntryHeaders[0];
					if (OwnDeclaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF)
					{
						hasEntryWithLodgedOrPrelodgedWithCustoms = !entry.EntryNumber.IsEmpty;
					}
					else if (OwnDeclaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
					{
						hasEntryWithLodgedOrPrelodgedWithCustoms = !(entry.CH_EntryStatus.IsEmpty || entry.CH_EntryStatus == CDS.Constants.ThreeCharFunctionCodes.MessageRejected);
					}
				}
				return hasEntryWithLodgedOrPrelodgedWithCustoms;
			}
		}

		public ZPropertyInfo ChiefDeclarationUCRInfo
		{
			get { return GetZPropertyInfo(nameof(ChiefDeclarationUCR)); }
		}

		public ZBool HasOwnDeclaration
		{
			get { return OwnDeclaration != null; }
		}

		JobDeclaration ownDeclaration;
		public JobDeclaration OwnDeclaration
		{
			get
			{
				if (ownDeclaration == null)
				{
					var query = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
					var pivot = Factory.LoadTop1<GenPivot>(query);
					if (pivot != null)
					{
						ownDeclaration = Factory.Load<JobDeclaration>(pivot.XX_Relation2ID);
					}
				}
				return ownDeclaration;
			}
		}

		protected abstract bool IsParentLinkedToForwardingJob { get; }

		JobDeclaration ICcsukCusAwb.CreateNewStandaloneCDSDeclaration()
		{
			JobDeclaration declaration = null;
			if (!HasOwnDeclaration && !IsParentLinkedToForwardingJob && ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateDeclaration))
			{
				declaration = GetNewDeclarationAndMakeLinkPivot();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_MasterBill = AWB.MasterBill;
				declaration.ZG_HouseSplitReference = SplitReference;
				SetNewChiefDeclarationProperties(declaration);
				declaration.JE_TotalNoOfPacks = NumberOfPiecesExpected;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				Factory.Save();
				ChiefDeclarationUCRInfo.RefreshBinding();
			}
			return declaration;
		}

		JobDeclaration GetNewDeclarationAndMakeLinkPivot()
		{
			JobDeclaration gbDeclaration = null;
			foreach (var brotherSplit in (from SplitConsignment s in AWB.Splits where s.PK != PK orderby s.SplitReference select s))
			{
				if (brotherSplit.HasOwnDeclaration)
				{
					var baseDec = brotherSplit.OwnDeclaration.GetNewRelatedDeclaration(Factory);
					gbDeclaration = Factory.Load<JobDeclaration>(baseDec.PK);
					break;
				}
			}
			if (gbDeclaration == null)
			{
				gbDeclaration = Factory.New<JobDeclaration>();
			}

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = PK;
			pivot.XX_Relation2ID = gbDeclaration.PK;
			return gbDeclaration;
		}

		protected abstract void SetNewChiefDeclarationProperties(JobDeclaration declaration);

		ZString ICcsukCusAwb.ChiefMasterUCRReferenceSuffix
		{
			get { return ChiefMasterUCRReferenceSuffixCore; }
		}

		protected abstract ZString ChiefMasterUCRReferenceSuffixCore { get; }

		void ICcsukCusAwb.SetEcStatusRelease(bool isSetting)
		{
			AwbEcStatusReleaseSetUnsetHelper.SetEcStatusRelease(isSetting, this, this);
		}

		public ZString GetMasterUCRReference()
		{
			var masterUCRCalculator = new MasterUCRCalculator();
			return masterUCRCalculator.GetAirportPrefixFromSixCharShedCode(CargoTerminalOperatorAirportAndShed, Factory) + CargoTerminalOperator + ((ICcsukCusAwb)this).ChiefMasterUCRReferenceSuffix;
		}

		protected abstract CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodesCore { get; }

		CusAddInfoCollection<CommunityHandlingCode> ICcsukCusAwb.CommunityHandlingCodes
		{
			get { return CommunityHandlingCodesCore; }
		}

		public ZBool Status2Granted
		{
			get { return AWB.Status2Granted; }
		}

		public ZInt NumberOfPiecesDelivered
		{
			get { return ((ICcsukCusAwb)this).OutTurns.TotalDelivered; }
		}

		protected override Customs.Business.CusPartShipValidation GetNewValidation()
		{
			return new CusPartShipValidation(this);
		}

		public new CusPartShipValidation Validation
		{
			get { return (CusPartShipValidation)base.Validation; }
		}

		void IAgentBadgeValidationProvider.ValidateAgentBadge()
		{
			Validation.ValidateCG_FlightNo();
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusAWBMultiMessageManagerForFRC<SplitConsignment>(delegate
			{ return this; });
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			//UpdateNprIfNeeded();  // TODO - after NPR overhaul shelf
			return Customs.Business.ContinueWithDetection.Yes;
		}

		ZPropertyInfo ICcsukCusAwb.ProfileInfo
		{
			get { return AWB.ProfileInfo; }
		}

		public void CalculateNprFromReceiptsIfNecessary()
		{
			var allOutTurns = ((ICcsukCusAwb)this).OutTurns;
			if (allOutTurns != null)
			{
				NumberOfPiecesReceived = (ZShort)(from CusOutTurn cot in allOutTurns where !cot.IsDeleted select (int)cot.C5_PackagesOutturned).Sum();
				if (NumberOfPiecesExpected == NumberOfPiecesReceived && Status1Date.IsEmpty && NumberOfPiecesReceived > 0)
				{
					Status1Date = ZDateTime.Now;
				}
				NumberOfPiecesReceivedInfo.RefreshBinding();
			}
		}

		CusOutTurn ICcsukCusAwb.CreateNewOutTurn()
		{
			throw new System.NotSupportedException("Do this at the parent level");
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				foreach (DocManagerInfo docManagerInfo in eDocsManagersQueuedForSaving)
				{
					docManagerInfo.Save();
				}
			}
		}

		void IEDocsDelayedSaver.QueueForSaving(DocManagerInfo docManagerInfo)
		{
			eDocsManagersQueuedForSaving.Add(docManagerInfo);
		}

		readonly List<DocManagerInfo> eDocsManagersQueuedForSaving = new List<DocManagerInfo>();

		public override void Delete()
		{
			if (HasOwnDeclaration)
			{
				var query = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
				Factory.LoadTop1<GenPivot>(query)?.Delete();
			}
			if (AWB != null)
			{
				((ICcsukCusAwb)this).CommunityHandlingCodes?.RemoveAndDeleteAll();
			}
			base.Delete();
		}
	}
}
