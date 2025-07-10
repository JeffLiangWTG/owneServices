using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[CodeProperty(CusMAWB.Schema.CM_MAWB)]
	[PreventDelete(false)]
	[DescriptionProperty(CusHAWB.Schema.ReferenceNumberWithShed)]
	public partial class CusMAWB : Customs.Business.CusMAWB
		, ICcsukCusAwb
		, Integration.Customs.GB.CCSUK.ICusMAWB
		, IDocManagerSupport
		, IAgentBadgeValidationProvider
		, IWorkflowProvider
		, IMessageManageableBizObj
		, IWorkflowProviderCore
	{
		new class Schema : Customs.Business.CusMAWB.Schema
		{
			// All of these wrap members on MasterLevelHouseHelper.
			public const string Weight = "Weight";
			public const string WeightCode = "WeightCode";
			public const string DescriptionOfGoods = "DescriptionOfGoods";
			public const string NumberOfPiecesReceived = "NumberOfPiecesReceived";
			public const string NumberOfPiecesExpected = "NumberOfPiecesExpected";
			public const string CargoTerminalOperator = "CargoTerminalOperator";
			public const string CargoTerminalOperatorAirport = "CargoTerminalOperatorAirport";
			public const string CargoTerminalOperatorAirportAndShed = "CargoTerminalOperatorAirportAndShed";
			public const string AgentBadge = "AgentBadge";
			public const string ShipmentDescriptionCode = "ShipmentDescriptionCode";
			public const string ConsignmentOrEntryType = "ConsignmentOrEntryType";
			public const string Profile = "Profile";
			public const string PresenceOnNetworkStatus = "PresenceOnNetworkStatus";
			public const string NumberOfPiecesReleasedSoFarCumulative = "NumberOfPiecesReleasedSoFarCumulative";
			public const string Status1Date = "Status1Date";
			public const string IataStatus = "IataStatus";
			public const string ReferenceNumberWithShed = "ReferenceNumberWithShed";
			public const string TemporaryStorageEndDate = "TemporaryStorageEndDate";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CusMAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			TouchChildObjectsToCreateThemWhenMakingABrandNewMawbSoThatTheirHasChangesIsSetCorrectly();
		}

		// For change logs
		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var relatedObjects = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					relatedObjects.Add(MasterLevelHouseHelper);
				}
				if (HasSplits)
				{
					relatedObjects.AddRange(Splits);
				}
				return (BusinessObject[])relatedObjects.ToArray(typeof(BusinessObject));
			}
		}

		public void DeactivateByWtg()
		{
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				CM_IsActive = false;
				MasterLevelHouseHelper.DeactivateByWtg();
				foreach (CusHAWB h in ChildBills)
				{
					h.DeactivateByWtg();
				}
				Logs.AddNew(Events.SetToInactive);
			}
		}

		public override bool CanDelete
		{
			get { return AwbCanDeleteHelper.ProcessCanDeleteRequestAndSetReason(this, ref reasonForNotAbleToDelete); }
		}

		MultilingualString reasonForNotAbleToDelete;
		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return reasonForNotAbleToDelete; }
		}

		public ZBool IsLinkedToJobConsol
		{
			get { return !CM_JK.IsEmpty; }
		}

		EnterpriseBusinessObject ICcsukCusAwb.ForwardingParent => Factory.Load<ForwardingConsol>(CM_JK);

		void TouchChildObjectsToCreateThemWhenMakingABrandNewMawbSoThatTheirHasChangesIsSetCorrectly()
		{
			if (!IsInDatabase)
			{
				var throwAway = MasterLevelHouseHelper;
			}
		}

		public override void Delete()
		{
			MasterLevelHouseHelper.Delete();
			base.Delete();
		}

		bool isSettingDefaultValues;
		protected override void SetDefaultValues()
		{
			isSettingDefaultValues = true;
			base.SetDefaultValues();
			CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			SetProfileIfOnlyOneInRegistry();
			Status2Granted = true;
			isSettingDefaultValues = false;
		}

		void SetProfileIfOnlyOneInRegistry()
		{
			ZString profile = MawbExportAddInfo.GetBestOrOnlyPima(MasterLevelHouseHelper.Lookups.ProfilesList);
			if (!profile.IsEmpty)
			{
				Profile = profile;
			}
		}

		protected override Customs.Business.CusMAWBValidation GetNewValidation()
		{
			return new CusMAWBValidation(this);
		}

		public new CusMAWBLookups Lookups
		{
			get { return (CusMAWBLookups)base.Lookups; }
		}

		protected override Customs.Business.CusMAWBLookups GetNewLookups()
		{
			return new CusMAWBLookups(this);
		}

		public CusUnderbondCollection<TranshipmentRemoval> TSRs
		{
			get { return MasterLevelHouseHelper.TSRs; }
		}

		public CusUnderbondCollection<InterAirportRemoval> IARs
		{
			get { return MasterLevelHouseHelper.IARs; }
		}

		public CusUnderbondCollection<InterShedRemoval> ISRs
		{
			get { return MasterLevelHouseHelper.ISRs; }
		}

		public CusUnderbondCollection<Fallback> FBKs
		{
			get { return MasterLevelHouseHelper.FBKs; }
		}

		[ReadOnlyMember(nameof(CM_FlightNoReadOnly))]
		public override ZString CM_FlightNo
		{
			get { return base.CM_FlightNo; }
			set { base.CM_FlightNo = value; }
		}
		bool CM_FlightNoReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.FlightNumber); }
		}

		[ReadOnlyMember(nameof(CM_ArrivalDateReadOnly))]
		public override ZDateTime CM_ArrivalDate
		{
			get { return base.CM_ArrivalDate; }
			set { base.CM_ArrivalDate = value; }
		}
		bool CM_ArrivalDateReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.FlightDate); }
		}

		[ReadOnlyMember(nameof(CM_MAWBReadOnly))]
		public override ZString CM_MAWB
		{
			get { return base.CM_MAWB; }
			set
			{
				var oldValue = base.CM_MAWB;
				base.CM_MAWB = value.Replace(" ", "").Replace("-", "").SubstringSafe(0, CusMAWB.Schema.CM_MAWBMaxLength); // remove hyphen and space from "123-1234 5678".
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, CM_MAWB);
			}
		}
		bool CM_MAWBReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AwbNumber); }
		}

		public ZString CM_MAWB_Formatted
		{
			get { return CM_MAWB.IsEmpty ? ZString.Empty : ZString.Format("{0}-{1}", CM_MAWB.Left(3), CM_MAWB.Right(8)); }
		}

		public ZString CargoTerminalOperator //SHED
		{
			get { return MasterLevelHouseHelper.CargoTerminalOperator; }
			set { MasterLevelHouseHelper.CargoTerminalOperator = value; }
		}
		public ZPropertyInfo CargoTerminalOperatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CargoTerminalOperator, x => MasterLevelHouseHelper.CargoTerminalOperatorInfo); }
		}

		public ZString CargoTerminalOperatorAirport
		{
			get { return MasterLevelHouseHelper.CargoTerminalOperatorAirport; }
			set { MasterLevelHouseHelper.CargoTerminalOperatorAirport = value; }
		}
		public ZPropertyInfo CargoTerminalOperatorAirportInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CargoTerminalOperatorAirport, x => MasterLevelHouseHelper.CargoTerminalOperatorAirportInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.ShedsList))]
		[MaxLength(6)]
		[ReadOnlyMember(nameof(CargoTerminalOperatorReadOnly))]
		public ZString CargoTerminalOperatorAirportAndShed //SHED
		{
			get { return MasterLevelHouseHelper.CargoTerminalOperatorAirportAndShed; }
			set { MasterLevelHouseHelper.CargoTerminalOperatorAirportAndShed = value; }
		}
		public ZPropertyInfo CargoTerminalOperatorAirportAndShedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CargoTerminalOperatorAirportAndShed, x => MasterLevelHouseHelper.CargoTerminalOperatorAirportAndShedInfo); }
		}
		bool CargoTerminalOperatorReadOnly
		{
			get { return IsProfileAShedOrHasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Agent); }
		}

		bool IsProfileAnAgentOrHasSplits
		{
			get { return HasSplits || MasterLevelHouseHelper.IsProfileAnAgent; }
		}
		bool IsProfileAShedOrHasSplits
		{
			get { return HasSplits || MasterLevelHouseHelper.IsProfileAShed; }
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.AgentsList))]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		public ZString AgentBadge
		{
			get { return MasterLevelHouseHelper.AgentBadge; }
			set { MasterLevelHouseHelper.AgentBadge = value; }
		}
		public ZPropertyInfo AgentBadgeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AgentBadge, x => MasterLevelHouseHelper.CS_ResponsiblePartyIDInfo); }
		}
		public bool AgentBadgeReadOnly
		{
			get { return IsProfileAnAgentOrHasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Agent); }
		}

		public event PimaChangedEventHandler OnPimaChanged;
		public delegate void PimaChangedEventHandler(ICcsukCusAwb awb);

		[List(nameof(MasterLevelHouseHelper) + "." + nameof(CusMAWB.MasterLevelHouseHelper.Lookups) + "." + nameof(CusHAWBLookups.ProfilesList))]
		[MaxLength(CcsukConstants.PimaMaxLength)]
		[ReadOnly(false)]
		public ZString Profile
		{
			get { return MasterLevelHouseHelper.Profile; }
			set
			{
				SetProfileOnHousesIfTheyAreBlank(value);
				MasterLevelHouseHelper.Profile = value;
				if (OnPimaChanged != null)
				{
					OnPimaChanged(this);
				}
				CargoTerminalOperatorAirportAndShedInfo.RefreshBinding();
				AgentBadgeInfo.RefreshBinding();
				CusHAWB.MakeOutTurnsReadOnlyIfNprReadOnly(OutTurns, NumberOfPiecesReceivedReadOnly);
				if (!IsInDatabase && IsUFO)
				{
					CM_MAWB = GetUfoMawbNumber();
				}

				MasterLevelHouseHelper.ISRs.RefreshBinding();
				MasterLevelHouseHelper.TSRs.RefreshBinding();
				MasterLevelHouseHelper.IARs.RefreshBinding();
				MasterLevelHouseHelper.FBKs.RefreshBinding();
			}
		}
		public ZPropertyInfo ProfileInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Profile, x => MasterLevelHouseHelper.ProfileInfo); }
		}

		void SetProfileOnHousesIfTheyAreBlank(ZString newProfile)
		{
			// Needed for when creating a mawb from a consol.  Without this, the houses would be created without a profile and then one cannot save anything.
			foreach (CusHAWB house in ChildBills)
			{
				if (house.Profile.IsEmpty)
				{
					house.Profile = newProfile;
				}
			}
		}

		[List(nameof(MasterLevelHouseHelper) + "." + nameof(CusMAWB.MasterLevelHouseHelper.Lookups) + "." + nameof(CusHAWBLookups.ShipmentDescriptionCodeList))]
		[MaxLength(1)]
		[ReadOnlyMember(nameof(ShipmentDescriptionCodeReadOnly))]
		public ZString ShipmentDescriptionCode
		{
			get { return MasterLevelHouseHelper.ShipmentDescriptionCode; }
			set
			{
				MasterLevelHouseHelper.ShipmentDescriptionCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCM_RL_NKLoadPort();
				}
				ShipmentDescriptionCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ShipmentDescriptionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipmentDescriptionCode, x => MasterLevelHouseHelper.ShipmentDescriptionCodeInfo); }
		}
		bool ShipmentDescriptionCodeReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Sdc); }
		}

		[List(nameof(MasterLevelHouseHelper) + "." + nameof(CusMAWB.MasterLevelHouseHelper.Lookups) + "." + nameof(CusHAWBLookups.ConsignmentOrEntryTypesList))]
		[MaxLength(2)]
		[ReadOnly(true)]
		public ZString ConsignmentOrEntryType
		{
			get { return MasterLevelHouseHelper.ConsignmentOrEntryType; }
			set { MasterLevelHouseHelper.ConsignmentOrEntryType = value; }
		}
		public ZPropertyInfo ConsignmentOrEntryTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsignmentOrEntryType, x => MasterLevelHouseHelper.ConsignmentOrEntryTypeInfo); }
		}

		[ReadOnlyMember(nameof(NumberOfPiecesExpectedReadOnly))]
		public ZShort NumberOfPiecesExpected
		{
			get { return MasterLevelHouseHelper.CS_PiecesManifested; }
			set
			{
				MasterLevelHouseHelper.CS_PiecesManifested = value;
				MarkMasterAsCompleteUponChangingPieceCountIfAllChildrenAreAlreadyComplete();
			}
		}

		public ZPropertyInfo NumberOfPiecesExpectedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NumberOfPiecesExpected, x => MasterLevelHouseHelper.CS_PiecesManifestedInfo); }
		}
		bool NumberOfPiecesExpectedReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npx); }
		}

		void MarkMasterAsCompleteUponChangingPieceCountIfAllChildrenAreAlreadyComplete()
		{
			// Mark master as COM if all its houses are already COM, regardless of whether mawb has its own status 1.
			if (!IsBasic)
			{
				CcsukUtilities.CompleteParentIfLastChildIsNowComplete(MasterLevelHouseHelper, this, ChildBills.Cast<ICcsukCusAwb>());
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.WeightUnitList))]
		[MaxLength(2)]
		[ReadOnlyMember(nameof(WeightReadOnly))]
		public ZString WeightCode
		{
			get { return MasterLevelHouseHelper.CS_WeightUQ; }
			set { MasterLevelHouseHelper.CS_WeightUQ = value; }
		}
		public ZPropertyInfo WeightCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WeightCode, x => MasterLevelHouseHelper.CS_WeightUQInfo); }
		}

		[ReadOnlyMember(nameof(WeightReadOnly))]
		public ZDecimal Weight
		{
			get { return MasterLevelHouseHelper.CS_Weight; }
			set { MasterLevelHouseHelper.CS_Weight = value; }
		}
		public ZPropertyInfo WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Weight, x => MasterLevelHouseHelper.CS_WeightInfo); }
		}
		bool WeightReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass); }
		}

		[ReadOnlyMember(nameof(DescriptionOfGoodsReadOnly))]
		public ZString DescriptionOfGoods
		{
			get { return MasterLevelHouseHelper.CS_GoodsDescription; }
			set
			{
				var oldValue = MasterLevelHouseHelper.CS_GoodsDescription;
				MasterLevelHouseHelper.CS_GoodsDescription = value;
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		public ZPropertyInfo DescriptionOfGoodsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DescriptionOfGoods, x => MasterLevelHouseHelper.CS_GoodsDescriptionInfo); }
		}
		bool DescriptionOfGoodsReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Description); }
		}

		[ReadOnlyMember(nameof(NumberOfPiecesReceivedReadOnly))]
		public ZShort NumberOfPiecesReceived
		{
			get { return MasterLevelHouseHelper.CS_PiecesLanded; }
			set
			{
				if (MasterLevelHouseHelper.CS_PiecesLanded != value)
				{
					MasterLevelHouseHelper.CS_PiecesLanded = value;
					MarkMasterAsCompleteUponChangingPieceCountIfAllChildrenAreAlreadyComplete();
					NumberOfPiecesReceivedInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo NumberOfPiecesReceivedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NumberOfPiecesReceived, x => MasterLevelHouseHelper.CS_PiecesLandedInfo); }
		}
		public bool NumberOfPiecesReceivedReadOnly
		{
			get { return ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npr) || !(LicenceAndPimaHelper.IsFullShed(this) || LicenceAndPimaHelper.IsFallbackShed(this)) || ProfileInfo.HasErrors(); }
		}

		[ReadOnly(true)]
		public ZDateTime Status1Date
		{
			get { return MasterLevelHouseHelper.Status1Date; }
			set { MasterLevelHouseHelper.Status1Date = value; }
		}

		public ZPropertyInfo Status1DateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.Status1Date, x => MasterLevelHouseHelper.Status1DateInfo); }
		}

		[ReadOnly(true)]
		public ZDateTime TemporaryStorageEndDate
		{
			get { return MasterLevelHouseHelper.TemporaryStorageEndDate; }
			set { MasterLevelHouseHelper.TemporaryStorageEndDate = value; }
		}

		public ZPropertyInfo TemporaryStorageEndDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TemporaryStorageEndDate, x => MasterLevelHouseHelper.TemporaryStorageEndDateInfo); }
		}

		public ZBool HasSplits
		{
			get { return Splits.Count > 0; }
		}

		ZString ICcsukCusAwb.SplitReference { get; }

		public void Split(List<ICuscarLine> fcsLinesHowToSplit)
		{
			new ConsignmentSplitter(this).Split(fcsLinesHowToSplit);
		}

		SplitCollection splits;
		[ChildEditable]
		public SplitCollection Splits
		{
			get
			{
				if (splits == null)
				{
					splits = new SplitCollection(this);
					splits.CountChanged += delegate
					{ FireDelegatedSplitCountChangedHandler(); };
					splits.Load();
					RegisterEditableChildObject(splits); // Only applicable to setting EC Status on a spliut using the split grid context menu (not by opening the split in its own form and using its own menu), a process which is probably not applicable (splitting EC jobs seems forbidden)
					splits.SetReadOnlyIncludingChildren(true);
				}
				return splits;
			}
		}

		void FireDelegatedSplitCountChangedHandler()
		{
			if (OnSplitsCountChanged != null)
			{
				OnSplitsCountChanged(this);
			}
		}

		public event SplitsCountChangedEventHandler OnSplitsCountChanged;
		public delegate void SplitsCountChangedEventHandler(ICcsukCusAwb awb);

		[ReadOnlyMember(nameof(Status2GrantedReadOnly))]
		public ZBool Status2Granted
		{
			get { return MasterLevelHouseHelper.Status2Granted; }
			set
			{
				MasterLevelHouseHelper.Status2Granted = value;
				if (!isSettingDefaultValues) // otherwise, if we touch the ChildBills collection too early, the ForwardingConsol's DocManagerInfo can't load the mawb's hawbs as related objects. Pfff.
				{
					foreach (CusHAWB house in ChildBills)
					{
						house.Status2Granted = value;
					}
				}
			}
		}
		public ZPropertyInfo Status2GrantedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Status2Granted), x => MasterLevelHouseHelper.Status2GrantedInfo); }
		}
		bool Status2GrantedReadOnly
		{
			get { return !MasterLevelHouseHelper.IsProfileAShed; }
		}

		[ChildEditable(true)]
		public new CusHAWBDependentCollection ChildBills
		{
			get { return (CusHAWBDependentCollection)base.ChildBills; }
		}

		protected override Customs.Business.CusHAWBDependentCollection GetNewChildBillsCollection()
		{
			return new CusHAWBDependentCollection(this);
		}

		public  // for binding, boooo hiss
		CusHAWB MasterLevelHouseHelper
		{
			get
			{
				if (masterLevelHouseHelper == null || (masterLevelHouseHelper.IsDeleted && !IsDeleted))
				{
					ZQuery q = new ZQuery(CusHAWBSchema.CS_CM, PK);
					q.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
					masterLevelHouseHelper = Factory.LoadTop1<CusHAWB>(q);
					if (masterLevelHouseHelper == null)
					{
						masterLevelHouseHelper = Factory.New<CusHAWB>();
						masterLevelHouseHelper.CS_CM = PK;
						masterLevelHouseHelper.CS_IsMasterHouse = true;
					}
					RegisterEditableChildObject(masterLevelHouseHelper);
				}
				return masterLevelHouseHelper;
			}
		}
		CusHAWB masterLevelHouseHelper;

		public EDIMessageCollection Messages
		{
			get { return MasterLevelHouseHelper.Messages; }
		}

		public ZString MasterBill
		{
			get { return CM_MAWB_Formatted; }
		}
		/// <summary>
		///  For humans only
		/// </summary>
		public ZString ReferenceNumber
		{
			get { return CM_MAWB_Formatted; }
		}

		public ZString ReferenceNumberWithShed
		{
			get { return string.Format("{0}{1}-{2}", CargoTerminalOperatorAirport, CargoTerminalOperator, ReferenceNumber); }
		}

		ZString ICcsukCusAwb.ChiefMasterUCRReferenceSuffix
		{
			get { return ReferenceNumber.KeepAlphanumericCharacters(); }
		}

		[ReadOnlyMember(nameof(CM_RL_NKDischargePortReadOnly))]
		public override ZString CM_RL_NKDischargePort
		{
			get { return base.CM_RL_NKDischargePort; }
			set
			{
				base.CM_RL_NKDischargePort = value;
				DefaultOtherAirports(value, AirportOfArrivalInfo, CargoTerminalOperatorAirportInfo);
				InitialiseShipmentDescriptionCode(ShipmentDescriptionCodeInfo, this, Factory);
				MasterLevelHouseHelper.AirportOfDestination = value; // For underbonds
			}
		}
		bool CM_RL_NKDischargePortReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination); }
		}

		internal static void DefaultOtherAirports(ZString newValue, ZPropertyInfo otherPortInfo, ZPropertyInfo cargoTerminalOperatorAirportInfo)
		{
			if (otherPortInfo.Value.IsEmpty && !newValue.IsEmpty)
			{
				otherPortInfo.Value = newValue;
			}
			if (cargoTerminalOperatorAirportInfo.Value.IsEmpty && !newValue.IsEmpty)
			{
				cargoTerminalOperatorAirportInfo.Value = newValue.Right(3);
				cargoTerminalOperatorAirportInfo.RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(CM_RL_NKFirstArrivalPortReadOnly))]
		public override ZString CM_RL_NKFirstArrivalPort
		{
			get { return base.CM_RL_NKFirstArrivalPort; }
			set
			{
				base.CM_RL_NKFirstArrivalPort = value;
				MasterLevelHouseHelper.AirportOfArrival = value; // For underbonds
				DefaultOtherAirports(value, AirportOfDestinationInfo, CargoTerminalOperatorAirportInfo);
			}
		}
		bool CM_RL_NKFirstArrivalPortReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination); }
		}

		internal static void InitialiseShipmentDescriptionCode(ZPropertyInfo sdcProperty, ICcsukCusAwb awb, BusinessObjectFactory factory)
		{
			var portsInEU = CusMAWBValidation.ArePortsInEU(awb.AirportOfOrigin, awb.AirportOfDestination, factory);
			bool wipeEcStatus = false;
			switch (portsInEU)
			{
				case CusMAWBValidation.PortsInEU.BothInEU:
					sdcProperty.Value = (ZString)ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport; //C
					break;
				default:
					if (!awb.AirportOfDestination.IsEmpty)
					{
						sdcProperty.Value = (ZString)ShipmentDescriptionCodes.Codes.TotalConsignmentManifested; //T
						wipeEcStatus = true;
					}
					break;
			}

			if (wipeEcStatus && awb.CustomsActionCode == CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease)
			{
				awb.SetEcStatusRelease(false);
			}
		}

		protected override void SetPKAndDefaults()
		{
			base.SetPKAndDefaults();
			WeightCode = "KG";
		}

		#region Load / Create

		public static CusMAWB CreateNew(ForwardingConsol consol)
		{
			CusMAWB result = consol.Factory.New<CusMAWB>();
			result.CM_JK = consol.PK;
			result.SynchroniseData();
			return result;
		}

		public void SynchroniseData()
		{
			if (ShouldSynchronise)
			{
				var synchroniser = new CusMawbToConsolSynchoniser(this);
				if (!IsDeleted && Consol != null)
				{
					synchroniser.SynchroniseFromConsol(Consol);
				}
			}
		}

		bool ShouldSynchronise
		{
			get { return Messages.Count == 0; }
		}

		public IFSR GetAwbToFsrProvider(CcsukTransmissionMessageFunction how)
		{
			return new CusAwbToFsrProvider(this, how.MessageSubType);
		}

		public ZBool IsThroughAwb
		{
			get { return !AirportOfDestination.IsEmpty && AirportOfDestination != AirportOfArrival; }
		}

		public ControllerID ModuleControllerId
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventory; }
		}

		#endregion

		public ICuscar GetCuscarWrapper()
		{
			return IsUFO ? new CuscarWrapperFromCusMawbUFO(this) : new CuscarWrapperFromCusMawb(this);
		}

		protected override DocumentSupporter GetNewDocumentSupporter()
		{
			return new CcsukDocumentSupporter(this);
		}

		public ZBool IsPrearrival
		{
			get { return NumberOfPiecesReceived == 0 && CM_ArrivalDate.IsEmpty; }
		}

		public ZDateTime LocalCreationDate
		{
			get { return CM_SystemCreateTimeUtc; }
			set { CM_SystemCreateTimeUtc = value; }
		}

		public ZDateTime LastEditTime
		{
			get { return CM_SystemLastEditTimeUtc; }
		}

		public ZBool IsLodgedAtCcsuk
		{
			get { return PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.OnCommDb; }
		}

		public ZBool IsLodgedOrAssumedAtCcsuk
		{
			get { return (PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.OnCommDb || PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection); }
		}

		public ZBool IsProfileAShed
		{
			get { return MasterLevelHouseHelper.IsProfileAShed; }
		}

		public ZBool IsProfileAnAgent
		{
			get { return MasterLevelHouseHelper.IsProfileAnAgent; }
		}

		public bool UpdateStatusToCacIfAllowed(ZString customsActionCode, ZDateTime customsActionDate, ZString agentReference, ZString customsActionText)
		{
			return new ConsignmentStatusUpdater(this).Update(customsActionCode, customsActionDate, agentReference, customsActionText);
		}

		[ReadOnly(true)]
		public ZString LatestCustomsActionText
		{
			get { return MasterLevelHouseHelper.LatestCustomsActionText; }
			set { MasterLevelHouseHelper.LatestCustomsActionText = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.CustomsActionsCodes))]
		[MaxLength(4)]
		public ZString CustomsActionCode
		{
			get { return MasterLevelHouseHelper.CustomsActionCode; }
		}

		public void SetCustomsActionCode(ZString code, ZDateTime date)
		{
			MasterLevelHouseHelper.SetCustomsActionCode(code, date);
			if (!Status1Date.IsEmpty && CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(CustomsActionCode))
			{
				((ICcsukCusAwb)this).CompleteOnCcsuk();
			}
		}

		public ZDateTime CustomsActionDate
		{
			get { return MasterLevelHouseHelper.CustomsActionDate; }
		}

		protected override void ReloadCore()
		{
			base.ReloadCore();
			MasterLevelHouseHelper.Reload();
			outTurns = null;  //see comment in CusHAWB
		}

		public ZString ReasonForNotAllowSplit
		{
			get { return IsBasic ? string.Empty : "A consolidation cannot be split, instead split the houses"; }
		}

		protected override ZString HumanReadableNameCore => IsBasic
			? Res.GetString("843B84C9-12AD-4796-931E-CE3D0704084B", "CCS-UK Basic Air Waybill {0}", ReferenceNumber)
			: Res.GetString("58F37990-346C-4D89-BBC7-390380EE7FE1", "CCS-UK Master Air Waybill {0}", ReferenceNumber);

		public GlbStaff UserInChargeOfJob
		{
			get { return masterLevelHouseHelper.UserInChargeOfJob; }
		}

		public ZBool IsBasic
		{
			get
			{
				var hasSplits = Splits.Count > 0;
				return !ChildBillsExcludingUnCommitted.Any(h => h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDbDeleted &&
					(h.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.NotOnCommDb || !hasSplits));
			}
		}

		IEnumerable<CusHAWB> ChildBillsExcludingUnCommitted
		{
			get
			{
				foreach (CusHAWB item in ChildBills)
				{
					if (!((IBusinessObjectInternals)item).IsUnCommittedRow)
					{
						yield return item;
					}
				}
			}
		}

		protected override DocManagerInfo GetNewDocManagerInfo()
		{
			return new CusMawbDocManagerInfo(this);
		}

		[ReadOnlyMember(nameof(CM_RL_NKLoadPortReadOnly))]
		public override ZString CM_RL_NKLoadPort
		{
			get { return base.CM_RL_NKLoadPort; }
			set
			{
				base.CM_RL_NKLoadPort = value;
				InitialiseShipmentDescriptionCode(ShipmentDescriptionCodeInfo, this, Factory);
				MasterLevelHouseHelper.AirportOfOrigin = value; // For underbonds
			}
		}
		bool CM_RL_NKLoadPortReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfOrigin); }
		}

		[List(nameof(MasterLevelHouseHelper) + "." + nameof(CusMAWB.MasterLevelHouseHelper.Lookups) + "." + nameof(CusHAWBLookups.NonUkAirports))]
		public ZString AirportOfOrigin
		{
			get { return CM_RL_NKLoadPort; }
			set
			{
				var oldValue = CM_RL_NKLoadPort;
				CM_RL_NKLoadPort = value;
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		public ZPropertyInfo AirportOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CM_RL_NKLoadPort, x => CM_RL_NKLoadPortInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.UkInventoryControlledAirportsList))]
		[MaxLength(3)] //IATA
		public ZString AirportOfArrival
		{
			get { return CM_RL_NKFirstArrivalPort; }
			set { CM_RL_NKFirstArrivalPort = value; }
		}
		public ZPropertyInfo AirportOfArrivalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CM_RL_NKFirstArrivalPort, x => CM_RL_NKFirstArrivalPortInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusMAWBLookups.UkInventoryControlledAirportsList))]
		[ReadOnlyMember(nameof(CM_RL_NKDischargePortReadOnly))]
		[MaxLength(3)] //IATA
		public ZString AirportOfDestination
		{
			get { return CM_RL_NKDischargePort; }
			set
			{
				var oldValue = CM_RL_NKDischargePort;
				CM_RL_NKDischargePort = value;
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		public ZPropertyInfo AirportOfDestinationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CM_RL_NKDischargePort, x => CM_RL_NKDischargePortInfo); }
		}

		CusOutTurnList ICcsukCusAwb.OutTurns
		{
			get
			{
				var list = new CusOutTurnList();
				if (OutTurns != null)
				{
					list.AddRange(OutTurns.ToArray<CusOutTurn>());
				}
				return list;
			}
		}

		BusinessObjectCollection ICcsukCusAwb.OutTurnsCollection
		{
			get { return OutTurns; }
		}

#if DEBUG
		protected
#endif
		CusOutTurnCollection outTurns;
		[ChildEditable]
		public CusOutTurnCollection OutTurns
		{
			get
			{
				if (IsBasic && outTurns == null)
				{
					outTurns = new CusOutTurnCollection(MasterLevelHouseHelper);
					RegisterEditableChildObject(outTurns);
					outTurns.Load();
				}
				return outTurns;
			}
		}

		[List(nameof(MasterLevelHouseHelper) + "." + nameof(CusMAWB.MasterLevelHouseHelper.Lookups) + "." + nameof(CusHAWBLookups.PresenceOnNetworkList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString PresenceOnNetworkStatus
		{
			get { return MasterLevelHouseHelper.PresenceOnNetworkStatus; }
			set
			{
				var oldValue = MasterLevelHouseHelper.PresenceOnNetworkStatus;
				if (oldValue != value)
				{
					MasterLevelHouseHelper.PresenceOnNetworkStatus = value;
				}
			}
		}
		public ZPropertyInfo PresenceOnNetworkStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PresenceOnNetworkStatus, x => MasterLevelHouseHelper.PresenceOnNetworkStatusInfo); }
		}

		public ZBool IsUFO
		{
			get { return CM_HasProhibitedPackaging; }
			set { CM_HasProhibitedPackaging = value; }
		}

		public void InitialiseUFO()
		{
			IsUFO = true;
			CM_ArrivalDate = ZDateTime.Now;
			var predicate = (from CodeDescriptionPair pima in MasterLevelHouseHelper.Lookups.ProfilesList where pima.Code.StartsWith("CUKAIR98") select pima);
			if (predicate.Count() == 1)
			{
				Profile = predicate.First().Code;
			}
			CM_MAWB = GetUfoMawbNumber();
		}

		internal string GetUfoMawbNumber()
		{
			return CargoTerminalOperator.PadRight(3) + ZDateTime.Now.ToString("MMddHHmm");
		}

		public WhsLocationCollection WarehouseLocations
		{
			get { return CusOutTurn.GetWarehouseLocations(this); }
		}

		// Simple access to the first OutTurn's SSL
		[List(nameof(WarehouseLocations))]
		public ZGuid UfoShedStorageLocation
		{
			get => UfoFirstOutTurn?.WarehouseLocationID ?? ZGuid.Empty;
			set
			{
				if (UfoFirstOutTurn != null)
				{
					UfoFirstOutTurn.WarehouseLocationID = value;
					UfoShedStorageLocationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UfoShedStorageLocationInfo
		{
			get { return GetZPropertyInfo(nameof(UfoShedStorageLocation)); }
		}

		// Simple access to the first OutTurn's NoP
		public ZInt UfoPiecesReceived
		{
			get => UfoFirstOutTurn?.C5_PackagesOutturned ?? ZInt.Zero;
			set
			{
				if (UfoFirstOutTurn != null)
				{
					UfoFirstOutTurn.C5_PackagesOutturned = value;
					NumberOfPiecesReceived = (ZShort)value;
					UfoPiecesReceivedInfo.RefreshBinding();
					NumberOfPiecesReceivedInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UfoPiecesReceivedInfo
		{
			get { return GetZPropertyInfo(nameof(UfoPiecesReceived)); }
		}

		CusOutTurn UfoFirstOutTurn
		{
			get
			{
				CusOutTurn outTurn = null;
				if (OutTurns != null)
				{
					if (OutTurns.Count == 0)
					{
						OutTurns.AddNew();
					}
					outTurn = OutTurns[0];
				}
				return outTurn;
			}
		}

		ZBool ICcsukCusAwb.IsCompleteOnCcsuk
		{
			get { return ((ICcsukCusAwb)MasterLevelHouseHelper).IsCompleteOnCcsuk; }
		}

		ZBool ICcsukCusAwb.IsArchivedOnCcsuk
		{
			get { return ((ICcsukCusAwb)MasterLevelHouseHelper).IsArchivedOnCcsuk; }
		}

		ZBool ICcsukCusAwb.IsEntryCancelled
		{
			get
			{
				var isEntryCancelled = false;
				if (CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestCancelled && MasterLevelHouseHelper?.Declaration != null && MasterLevelHouseHelper.Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
				{
					var entry = MasterLevelHouseHelper.Declaration.CustomsEntryHeaders.FirstOrDefault();
					isEntryCancelled = entry?.IsCancelledWithCustoms ?? false;
				}
				return isEntryCancelled;
			}
		}

		void ICcsukCusAwb.ArchiveOnCcsuk(ReasonForArchiving why)
		{
			((ICcsukCusAwb)MasterLevelHouseHelper).ArchiveOnCcsuk(why);
			Logs.AddNew(Events.DocumentDeleted, CcsukUtilities.GetEnumDescription(why), ZDateTimeOffset.Now);
		}

		void ICcsukCusAwb.CompleteOnCcsuk(bool irrelevant)
		{
			((ICcsukCusAwb)MasterLevelHouseHelper).CompleteOnCcsuk(false);
			Logs.AddNew(Events.TaskCompleted, "Completed on CCSUK", ZDateTimeOffset.Now);
		}

		void ICcsukCusAwb.UncompleteOnCcsuk(bool irrelevant)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Logs.AddNew(Events.TaskCompleted, "Revoked completion on CCSUK", ZDateTimeOffset.Now);
		}

		CusAwbIsReadOnlyHelper readOnlyAndPermissionHelper;
		public CusAwbIsReadOnlyHelper ReadOnlyAndPermissionHelper
		{
			get { return readOnlyAndPermissionHelper ?? (readOnlyAndPermissionHelper = new CusAwbIsReadOnlyHelper(this)); }
		}

		public ZInt NumberOfPiecesReleasedSoFarCumulative(Event eventType)
		{
			return MasterLevelHouseHelper.NumberOfPiecesReleasedSoFarCumulative(eventType);
		}

		public void ReleaseThisNumberOfPieces(ZInt pieces, Event type)
		{
			MasterLevelHouseHelper.ReleaseThisNumberOfPieces(pieces, type);
		}

		JobDeclaration ICcsukCusAwb.CreateNewStandaloneCDSDeclaration()
		{
			JobDeclaration declaration = null;
			if (IsBasic && !MasterLevelHouseHelper.HasDeclaration && !IsLinkedToJobConsol && ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateDeclaration))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_MasterBill = CM_MAWB;
				declaration.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
				declaration.JE_DateOfArrival = CM_ArrivalDate;
				declaration.JE_EntrySubStyle = IsPrearrival ?
					GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived :
					GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
				declaration.JE_VoyageFlightNo = CM_FlightNo;
				MasterLevelHouseHelper.SynchroniseToDeclaration(declaration, true, true);
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
				Factory.Save();
				ChiefDeclarationUCRInfo.RefreshBinding();
			}
			return declaration;
		}

		public ZString ChiefDeclarationUCR
		{
			get { return IsBasic && MasterLevelHouseHelper.HasDeclaration ? MasterLevelHouseHelper.ChiefDeclarationUCR : ZString.Empty; }
		}

		public ZPropertyInfo ChiefDeclarationUCRInfo
		{
			get { return GetZPropertyInfo(nameof(ChiefDeclarationUCR)); }
		}

		bool ICcsukCusAwb.HasEntryWithLodgedOrPrelodgedWithCustoms
		{
			get { return MasterLevelHouseHelper.HasEntryWithLodgedOrPrelodgedWithCustoms; }
		}

		void ICcsukCusAwb.SetEcStatusRelease(bool isSetting)
		{
			AwbEcStatusReleaseSetUnsetHelper.SetEcStatusRelease(isSetting, this, this);
		}

		void MaybeWipeClearanceDetailsIfCacIsCu(ZString oldValue, ZString newValue)
		{
			CusHAWB.MaybeWipeClearanceDetailsIfCacIsCuShared(this, oldValue, newValue, IsProfileAShed);
		}

		public CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodes
		{
			get { return MasterLevelHouseHelper.CommunityHandlingCodes; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			CalculateNprFromReceiptsIfNecessary();
		}

		#region Suspend Calculate NPR

		public bool IsCalculateNprFromReceiptsSuspended => calculateNprFromReceiptsSuspenderIndex > 0;

		int calculateNprFromReceiptsSuspenderIndex;

		public IDisposable SuspendCalculateNprFromReceipts() => SuspendCalculateNprFromReceiptsCore();

		IDisposable SuspendCalculateNprFromReceiptsCore()
		{
			calculateNprFromReceiptsSuspenderIndex++;
			return new DisposableAction(() => calculateNprFromReceiptsSuspenderIndex--);
		}

		#endregion

		public void CalculateNprFromReceiptsIfNecessary()
		{
			if (!IsCalculateNprFromReceiptsSuspended || GBCustomsDataRegistry.Instance.ForceRecalculationOfNPR.Value)
			{
				if (!NumberOfPiecesReceivedReadOnly)
				{
					if (IsBasic && OutTurns != null)
					{
						NumberOfPiecesReceived = (ZShort)(from CusOutTurn cot in OutTurns where !cot.IsDeleted select (int)cot.C5_PackagesOutturned).Sum();
						if (NumberOfPiecesExpected == NumberOfPiecesReceived && Status1Date.IsEmpty && NumberOfPiecesReceived > 0)
						{
							Status1Date = ZDateTime.Now;
						}
						NumberOfPiecesReceivedInfo.RefreshBinding();
						if (HasSplits)
						{
							foreach (SplitConsignment split in Splits)
							{
								split.CalculateNprFromReceiptsIfNecessary();
							}
						}
					}
					else
					{
						// Update my NPR from my child houses' NPRs
						NumberOfPiecesReceived = (ZShort)(from CusHAWB h in ChildBills where !h.IsDeleted select (int)h.CS_PiecesLanded).Sum();
						if (NumberOfPiecesExpected == NumberOfPiecesReceived && Status1Date.IsEmpty && NumberOfPiecesReceived > 0)
						{
							Status1Date = ZDateTime.Now;
						}
					}
				}
			}
		}

		#region Workflow

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusMAWBProcessTaskCollection()
		{
			return new CusMAWBProcessTaskCollection(this);
		}
		#endregion

		ZInt ICcsukCusAwb.NumberOfPiecesDelivered
		{
			get { return OutTurns != null ? OutTurns.TotalDelivered : 0; }
		}

		void IAgentBadgeValidationProvider.ValidateAgentBadge()
		{
			MasterLevelHouseHelper.Validation.ValidateCS_ResponsiblePartyID();
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusAWBMultiMessageManagerForFRC<CusMAWB>(delegate
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

		CusOutTurn ICcsukCusAwb.CreateNewOutTurn()
		{
			return OutTurns.AddNew();
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

		public ZString DisplayTextForCustomsCargoStatusColumn
		{
			get { return MasterLevelHouseHelper.DisplayTextForCustomsCargoStatusColumn; }
		}

		public string CheckInAllChildPieces(NonPersistentCheckInAllChildPieces nonPersistentCheckInAllChildPiecesData)
		{
			var result = new ZStringBuilder();

			foreach (CusHAWB hawb in ChildBills)
			{
				if (hawb.Status1Date.IsEmpty && !hawb.Splits.Any())
				{
					hawb.OutTurns.DeleteAll();
					MakeNewOutturnForHawbOrSplit(hawb, null, nonPersistentCheckInAllChildPiecesData);
				}
				else if (!hawb.Status1Date.IsEmpty)
				{
					result.Append(@$"HAWB {hawb.CS_HAWB}: status 1 already set; skipping");
				}
				else if (hawb.HasSplits && hawb.Splits.All(split => ((SplitConsignment)split).NumberOfPiecesReceived == 0))
				{
					hawb.OutTurns.DeleteAll();
					foreach (SplitConsignment split in hawb.Splits)
					{
						MakeNewOutturnForHawbOrSplit(hawb, split, nonPersistentCheckInAllChildPiecesData);
					}
				}
				else if (hawb.HasSplits && hawb.Splits.Any(split => ((SplitConsignment)split).NumberOfPiecesReceived > 0))
				{
					result.Append(@$"HAWB {hawb.CS_HAWB}: checked-in splits exist already set; skipping");
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		void MakeNewOutturnForHawbOrSplit(CusHAWB hawb, SplitConsignment split, NonPersistentCheckInAllChildPieces nonPersistentCheckInAllChildPiecesData)
		{
			var newOutturn = hawb.OutTurns.AddNew();

			newOutturn.C5_PackagesUnits = nonPersistentCheckInAllChildPiecesData.PackagesUnits;
			newOutturn.C5_CargoReceiptDate = nonPersistentCheckInAllChildPiecesData.ReceivedDate;
			newOutturn.C5_MarksAndNumbers = nonPersistentCheckInAllChildPiecesData.MarksAndNumbers;
			newOutturn.WarehouseLocationID = nonPersistentCheckInAllChildPiecesData.ShedStorageLocationId;
			newOutturn.IsBeingReleasedNow = nonPersistentCheckInAllChildPiecesData.IsBeingReleasedNow;
			newOutturn.C5_GoodsDescription = nonPersistentCheckInAllChildPiecesData.GoodsDescription;
			newOutturn.C5_ContainerNumber = nonPersistentCheckInAllChildPiecesData.ContainerNumber;
			newOutturn.C5_ContainerSeal = nonPersistentCheckInAllChildPiecesData.ContainerSeal;
			newOutturn.C5_DamageIndicator = nonPersistentCheckInAllChildPiecesData.IsDamaged;

			if (split != null)
			{
				newOutturn.C5_PackagesOutturned = split.NumberOfPiecesExpected;
				newOutturn.SplitReferenceToWhichThisPertains = split.SplitReference;
			}
			else
			{
				newOutturn.C5_PackagesOutturned = hawb.CS_PiecesManifested;
			}
		}
	}
}
