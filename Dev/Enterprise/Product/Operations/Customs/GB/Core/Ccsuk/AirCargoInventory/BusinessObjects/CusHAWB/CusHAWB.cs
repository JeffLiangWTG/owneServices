using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(CusMAWB), "ChildBills")]
	[CodeProperty(CusHAWB.Schema.CS_HAWB)]
	[DescriptionProperty(CusHAWB.Schema.ReferenceNumberWithShed)]
	public partial class CusHAWB : Customs.Business.CusHAWB
		, ICcsukCusAwb
		, Integration.Customs.GB.CCSUK.ICusHAWB
		, IDocumentSupportable
		, IDocManagerSupport
		, IAgentBadgeValidationProvider
		, IWorkflowProvider
		, IMessageManageableBizObj
		, Integration.Customs.ICusAddInfoTypeSupporter
		, IWorkflowProviderCore
		, IWorkflowTriggerEventSource  // To ensure that when St1 is set on worker house, master-level triggers for St1 are fired. Otherwise only worker house's trigger is fired. 
	{
		public new class Schema : AutoCusHAWB.Schema
		{
			public const string IsLinkedToForwardingJob = "IsLinkedToForwardingJob";
			public const string IsLinkedToJobShipment = "IsLinkedToJobShipment";
			public const string ReferenceNumber = "ReferenceNumber";
			public const string ReferenceNumberWithShed = "ReferenceNumberWithShed";
			public const string SplitReferencesForAllAwbsModuleGrid = "SplitReferencesForAllAwbsModuleGrid";
			public const string LatestCustomsActionText = "LatestCustomsActionText";
			public const string TemporaryStorageEndDate = "TemporaryStorageEndDate";
		}

		public CusHAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CS_MsgStatus), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CS_Weight), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CS_PiecesLanded), ConcurrencyPolicy.Ignore);
		}

		[ReadOnlyMember(nameof(CS_HAWBReadOnly))]
		public override ZString CS_HAWB
		{
			get { return base.CS_HAWB; }
			set
			{
				var oldValue = CS_HAWB;
				base.CS_HAWB = CcsukUtilities.LeftPadWithZeros(value).ToUpper();
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		bool CS_HAWBReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AwbNumber); }
		}

		public void DeactivateByWtg()
		{
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				CS_IsActive = false;
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

		public ZBool IsLodgedAtCcsuk
		{
			get { return MAWB != null && MAWB.IsLodgedAtCcsuk && PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.OnCommDb; }
		}

		public ZBool IsLodgedOrAssumedAtCcsuk
		{
			get { return MAWB != null && MAWB.IsLodgedOrAssumedAtCcsuk && (PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.OnCommDb || PresenceOnNetworkStatus == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection); }
		}

		public ZBool IsLinkedToJobShipment
		{
			get { return !CS_JS.IsEmpty; }
		}

		public ZString ForwardingNumber
		{
			get
			{
				return IsLinkedToJobShipment
							? Shipment.JS_UniqueConsignRef
							: ZString.Empty;
				// No need to show a consol number, this is only used on a MAWB's HAWBs grid. 
			}
		}

		public ZBool IsLinkedToForwardingJob
		{
			get { return CS_IsMasterHouse && MAWB != null ? MAWB.IsLinkedToJobConsol : IsLinkedToJobShipment; }
		}

		EnterpriseBusinessObject ICcsukCusAwb.ForwardingParent => CS_IsMasterHouse ? Factory.Load<ForwardingConsol>(MAWB.CM_JK) : Factory.Load<ForwardingShipment>(CS_JS);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
			CS_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
		}

		public override ZGuid CS_JS
		{
			get { return base.CS_JS; }
			set
			{
				base.CS_JS = value;
				if (!isSettingJsOrJe && !value.IsEmpty && CS_JE_CustomsFormalEntry.IsEmpty && Shipment != null)
				{
					var gbDeclaration = (from BaseJobDeclaration baseDec
										 in Shipment.Declarations
										 where baseDec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom
										 select baseDec
										).FirstOrDefault();
					if (gbDeclaration != null)
					{
						isSettingJsOrJe = true;
						CS_JE_CustomsFormalEntry = gbDeclaration.PK;
						isSettingJsOrJe = false;
					}
				}
			}
		}

		bool isSettingJsOrJe;

		public override ZGuid CS_JE_CustomsFormalEntry
		{
			get { return base.CS_JE_CustomsFormalEntry; }
			set
			{
				base.CS_JE_CustomsFormalEntry = value;
				if (!isSettingJsOrJe && !value.IsEmpty && CS_JS.IsEmpty)
				{
					if (Declaration != null && !Declaration.JE_JS.IsEmpty)
					{
						isSettingJsOrJe = true;
						CS_JS = Declaration.JE_JS;
						isSettingJsOrJe = false;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.PresenceOnNetworkList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString PresenceOnNetworkStatus
		{
			get { return CS_MsgStatus; }
			set
			{
				CS_MsgStatus = value;
				var mawb = MAWB;
				if (value == PresenceOnNetworkList.Codes.OnCommDb && !CS_IsMasterHouse && mawb != null)
				{
					CcsukUtilities.UpdatePresenceToYesIfCurrentlyTransientOrNegative(mawb);
				}
			}
		}

		public override ZString CS_MsgStatus
		{
			get => base.CS_MsgStatus;
			set
			{
				base.CS_MsgStatus = value;
				var mawb = MAWB;
				if (mawb != null)
				{
					mawb.MarkAsNeedingValidation();
				}
			}
		}

		public ZPropertyInfo PresenceOnNetworkStatusInfo
		{
			get
			{
				var result = GetZPropertyInfo(nameof(CS_MsgStatus), "Presence Status");
				return result;
			}
		}

		/// <summary>
		/// Set by TCP/IP sender/receiver after successfully uploading a message for this HAWB. 
		/// Only updates the status if it's in a transient/crappy status. Will not clobber a firm status.
		/// </summary>
		public void SetPresenceOnNetworkStatusAfterMessageUpload()
		{
			var oldValue = PresenceOnNetworkStatus;
			if (oldValue == PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask || oldValue == PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck)
			{
				PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			}
		}

		[ReadOnly(true)]
		public override ZString CS_TranshipmentEntryNum
		{
			get { return base.CS_TranshipmentEntryNum; }
			set
			{
				base.CS_TranshipmentEntryNum = value;
				if (Shipment != null)
				{
					var existingTsrNum = (from CusEntryNumber cen in Shipment.CusEntryNumbers where cen.CE_EntryType == "TSN" select cen).FirstOrDefault(); // No core constant for TSN - transhipment number. 
					if (existingTsrNum == null)
					{
						existingTsrNum = Shipment.CusEntryNumbers.AddNew();
						existingTsrNum.CE_ParentID = Shipment.PK;
						existingTsrNum.CE_ParentTable = JobShipmentSchema.Constants.TableName;
						existingTsrNum.CE_EntryIsSystemGenerated = false;
						existingTsrNum.CE_EntryType = "TSN";
					}
					existingTsrNum.CE_EntryNum = value;
					Shipment.Logs.AddNew(Events.IntermediateTranshipmentArrival, value, ZDateTimeOffset.Now, false);
				}
				Logs.AddNew(Events.IntermediateTranshipmentArrival, value, ZDateTimeOffset.Now, false);
			}
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;

		public ZBool IsPrearrival
		{
			get { return MAWB != null && MAWB.IsPrearrival && CS_PiecesLanded == 0; }
		}

		public ZDateTime LocalCreationDate
		{
			get { return CS_SystemCreateTimeUtc; }
			set { CS_SystemCreateTimeUtc = value; }
		}

		public ZDateTime LastEditTime
		{
			get { return CS_SystemLastEditTimeUtc; }
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new CcsukDocumentSupporter(this); }
		}

		DocManagerInfo docManagerInfo;
		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusHawbDocManagerInfo(this)); }
		}

		protected override Customs.Business.CusHAWBValidation GetNewValidation()
		{
			return new CusHAWBValidation(this);
		}

		public new CusHAWBValidation Validation
		{
			get { return (CusHAWBValidation)base.Validation; }
		}

		public new CusHAWBLookups Lookups
		{
			get { return (CusHAWBLookups)base.Lookups; }
		}

		protected override Customs.Business.CusHAWBLookups GetNewLookups()
		{
			return new CusHAWBLookups(this);
		}

		[MaxLength(15)]
		[ReadOnlyMember(nameof(CS_GoodsDescriptionReadOnly))]
		public override ZString CS_GoodsDescription
		{
			get { return base.CS_GoodsDescription; }
			set
			{
				var oldValue = CS_GoodsDescription;
				base.CS_GoodsDescription = new UkCharSet().RemoveIllegalCharacters(value.ToUpper());
				MarkMawbForValidation();
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		bool CS_GoodsDescriptionReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Description); }
		}

		public ZString DescriptionOfGoods
		{
			get { return CS_GoodsDescription; }
		}

		[ReadOnlyMember(nameof(NumberOfPiecesReceivedReadOnly))]
		public override ZShort CS_PiecesLanded
		{
			get { return base.CS_PiecesLanded; }
			set
			{
				var isChanging = base.CS_PiecesLanded != value;
				base.CS_PiecesLanded = value;
				if (isChanging)
				{
					if (value > 0 && value == CS_PiecesManifested)
					{
						if (Status1Date.IsEmpty)
						{
							if (!NumberOfPiecesReceivedReadOnly)
							{
								Status1Date = ZDateTime.Now;
							}
						}
					}
					else
					{
						WipeStatus1DateAndUncomplete(out status1Date, Logs, Status1DateInfo, this);
					}
				}
				MarkMawbForValidation();
			}
		}
		public bool NumberOfPiecesReceivedReadOnly
		{
			get { return ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npr) || !(LicenceAndPimaHelper.IsFullShed(this) || LicenceAndPimaHelper.IsFallbackShed(this)) || ProfileInfo.HasErrors(); }
		}

		[MaxLength(3)]
		public ZString CargoTerminalOperator
		{
			get { return CS_WarehouseLocation.PadRight(6).Right(3).Trim(); }
			set
			{
				ZString airport = CS_WarehouseLocation.Left(3);
				CargoTerminalOperatorAirportAndShed = airport.PadRight(3) + value.PadRight(3);
				CargoTerminalOperatorInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CargoTerminalOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(CargoTerminalOperator)); }
		}

		[MaxLength(3)]
		public ZString CargoTerminalOperatorAirport
		{
			get { return CS_WarehouseLocation.PadRight(6).Left(3).Trim(); }
			set
			{
				ZString shed = CS_WarehouseLocation.PadRight(6).Right(3);
				CargoTerminalOperatorAirportAndShed = value.PadLeft(3) + shed.PadRight(3);
				CargoTerminalOperatorAirportInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo CargoTerminalOperatorAirportInfo
		{
			get { return GetZPropertyInfo(nameof(CargoTerminalOperatorAirport)); }
		}

		bool CargoTerminalOperatorReadOnly
		{
			get { return IsProfileAShedOrHasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Agent); }
		}

		[MaxLength(6)]
		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ShedsList))]
		[ReadOnlyMember(nameof(CargoTerminalOperatorReadOnly))]
		public ZString CargoTerminalOperatorAirportAndShed
		{
			get { return CS_WarehouseLocation.Trim(); }
			set
			{
				CheckMaximumLength(CargoTerminalOperatorAirportAndShedInfo, value);
				CS_WarehouseLocation = value.PadRight(6);
				CargoTerminalOperatorAirportAndShedInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCargoTerminalOperatorAirportAndShed();
				}
			}
		}
		public ZPropertyInfo CargoTerminalOperatorAirportAndShedInfo
		{
			get { return GetZPropertyInfo(nameof(CargoTerminalOperatorAirportAndShed)); }
		}

		public override ZString CS_WarehouseLocation
		{
			get { return base.CS_WarehouseLocation; }
			set
			{
				base.CS_WarehouseLocation = value;
				MarkMawbForValidation();
			}
		}

		[MaxLength(2)]
		[ReadOnlyMember(nameof(CS_WeightReadOnly))]
		public override ZString CS_WeightUQ
		{
			get { return base.CS_WeightUQ; }
			set
			{
				base.CS_WeightUQ = value;
				MarkMawbForValidation();
			}
		}

		ZDecimal Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.Weight
		{
			get { return CS_Weight; }
		}

		ZString Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.WeightCode
		{
			get { return CS_WeightUQ; }
		}

		[ReadOnlyMember(nameof(CS_WeightReadOnly))]
		public override ZDecimal CS_Weight
		{
			get { return base.CS_Weight; }
			set
			{
				base.CS_Weight = value;
				MarkMawbForValidation();
			}
		}
		bool CS_WeightReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass); }
		}

		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		[List(nameof(MAWB) + "." + nameof(CusHAWB.MAWB.Lookups) + "." + nameof(CusMAWBLookups.AgentsList))]
		[MaxLength(3)]
		public ZString AgentBadge
		{
			get { return CS_ResponsiblePartyID; }
			set
			{
				CheckMaximumLength(AgentBadgeInfo, value);
				CS_ResponsiblePartyID = value;
				CS_ResponsiblePartyIDInfo.RefreshBinding();
				AgentBadgeInfo.RefreshBinding();
			}
		}
		public bool AgentBadgeReadOnly
		{
			get { return IsProfileAnAgentOrHasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Agent); }
		}
		public ZPropertyInfo AgentBadgeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CS_ResponsiblePartyID, x => CS_ResponsiblePartyIDInfo); }
		}

		[ReadOnlyMember(nameof(AgentBadgeReadOnly))]
		public override ZString CS_ResponsiblePartyID  // This is needed because AgentBadgeInfo now wraps CS_ResponsiblePartyIDInfo (instead of giving a brand-new propertyInfo), so without this looking at AgentBadgeInfo.ReadOnly gives bad results.
		{
			get { return base.CS_ResponsiblePartyID; }
			set { base.CS_ResponsiblePartyID = value; }
		}

		[ReadOnlyMember(nameof(CS_PiecesManifestedReadOnly))]
		public override ZShort CS_PiecesManifested
		{
			get { return base.CS_PiecesManifested; }
			set
			{
				var isChanging = base.CS_PiecesManifested != value;
				base.CS_PiecesManifested = value;
				if (isChanging)
				{
					if (value > 0 && value == CS_PiecesLanded)
					{
						if (!NumberOfPiecesReceivedReadOnly)
						{
							Status1Date = ZDateTime.Now;
						}
					}
					else if (ShouldCascadeBasicMawbStatus1ToNewHouses)
					{
						CascadeBasicMawbStatus1ToNewHouses(value);
					}
					else
					{
						WipeStatus1DateAndUncomplete(out status1Date, Logs, Status1DateInfo, this);
					}
				}
				MarkMawbForValidation();
			}
		}

		bool CS_PiecesManifestedReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npx); }
		}

		void CascadeBasicMawbStatus1ToNewHouses(ZShort newNpxOnHouse)
		{
			var masterOutturn = (CusOutTurn)(MAWB.OutTurns?.FirstOrDefault());
			if (masterOutturn != null)
			{
				var newHouseOutturn = (CusOutTurn)(OutTurns.FirstOrDefault() ?? OutTurns.AddNew());
				newHouseOutturn.C5_PackagesOutturned = newNpxOnHouse;
				newHouseOutturn.C5_PackagesUnits = masterOutturn.C5_PackagesUnits;
				newHouseOutturn.C5_MarksAndNumbers = masterOutturn.C5_MarksAndNumbers;
				newHouseOutturn.C5_GoodsDescription = masterOutturn.C5_GoodsDescription;
				newHouseOutturn.C5_CargoReceiptDate = masterOutturn.C5_CargoReceiptDate;
				newHouseOutturn.C5_CargoUnpackDate = masterOutturn.C5_CargoUnpackDate;
				newHouseOutturn.C5_ContainerNumber = masterOutturn.C5_ContainerNumber;
				newHouseOutturn.C5_ContainerSeal = masterOutturn.C5_ContainerSeal;
				newHouseOutturn.C5_DamageIndicator = masterOutturn.C5_DamageIndicator;
				newHouseOutturn.WarehouseLocationID = masterOutturn.WarehouseLocationID;
				CS_PiecesLanded = newNpxOnHouse; // It will be calculated on save, but this gives a preview
			}
			else
			{
				// Problem - try using a new factory
			}
		}

		bool ShouldCascadeBasicMawbStatus1ToNewHouses
		{
			get
			{
				// Look in the DB, because at the point of setting NPX on a house the mawb will of course have a house (albeit not yet saved)
				if (MAWB != null)
				{
					var newFactory = new BusinessObjectFactory();
					var parentmawbInNewFactory = newFactory.Load<CusMAWB>(MAWB.PK);
					return !IsInDatabase
							&& GBCustomsDataRegistry.Instance.CcsukCascadeStatus1FromBasicToNewHouse.Value
							&& parentmawbInNewFactory != null
							&& parentmawbInNewFactory.IsBasic
							&& LicenceAndPimaHelper.IsFullShed(parentmawbInNewFactory)
							&& LicenceAndPimaHelper.IsFullShed(MAWB)
							&& LicenceAndPimaHelper.IsFullShed(this)
							&& !parentmawbInNewFactory.Status1Date.IsEmpty
							&& parentmawbInNewFactory.OutTurns.Count == 1;
				}
				else
				{
					return false;
				}
			}
		}

		public override ZGuid CS_CM
		{
			get { return base.CS_CM; }
			set
			{
				base.CS_CM = value;
				MarkMawbForValidation();
				if (MAWB != null && !IsInDatabase && !CS_IsMasterHouse)
				{
					Status2Granted = MAWB.Status2Granted;
				}
			}
		}

		[BusinessObjectTestExclude] // See CusHAWBTests.TestStatus1() for thorough - and proper - testing.  Base tests expect us to record invalid dates. 
		[ReadOnly(true)]
		public ZDateTime Status1Date
		{
			get
			{
				if (status1Date.IsEmpty)
				{
					status1Date = GetStatusDate("ST1", Logs);
				}
				return status1Date;
			}
			set
			{
				SetStatus1Date(value.ToOffset(), Logs, Status1DateInfo, out status1Date, this);
			}
		}

		internal static void SetStatus1Date(ZDateTimeOffset newValue, Logs logs, ZPropertyInfo status1DateInfo, out ZDateTime localStatus1Date, ICcsukCusAwb awb)
		{
			WipeStatus1DateAndUncomplete(out localStatus1Date, logs, status1DateInfo, awb);
			if (!newValue.IsEmpty)
			{
				logs.AddNew(Events.StatusChange, "ST1", newValue);
				status1DateInfo.RefreshBinding();
				if (CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(awb.CustomsActionCode))
				{
					awb.CompleteOnCcsuk();
				}
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

		[ReadOnlyMember(nameof(Status2GrantedReadOnly))]
		public ZBool Status2Granted
		{
			get { return CS_IsSurplus; }
			set
			{
				CS_IsSurplus = value;
				Status2GrantedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Status2GrantedInfo
		{
			get { return GetZPropertyInfo(nameof(Status2Granted)); }
		}

		bool Status2GrantedReadOnly
		{
			get { return !IsProfileAShed; }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ShipmentDescriptionCodeList))]
		[MaxLength(1)]
		[ReadOnlyMember(nameof(ShipmentDescriptionCodeReadOnly))]
		public ZString ShipmentDescriptionCode
		{
			get { return CS_ShipmentType.PadRight(3).Left(1).Trim(); }
			set
			{
				CheckMaximumLength(ShipmentDescriptionCodeInfo, value);
				var consignmentType = CS_ShipmentType.PadRight(3).Right(2);
				CS_ShipmentType = value.PadLeft(1) + consignmentType.PadRight(2);
				MarkMawbForValidation();
				if (!IsValidationSuspended)
				{
					Validation.ValidateShipmentDescriptionCode();
				}
				ShipmentDescriptionCodeInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ShipmentDescriptionCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentDescriptionCode)); }
		}
		bool ShipmentDescriptionCodeReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Sdc); }
		}

		public override ZString CS_ShipmentType
		{
			get { return base.CS_ShipmentType; }
			set
			{
				base.CS_ShipmentType = value;
				MarkMawbForValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ConsignmentOrEntryTypesList))]
		[MaxLength(2)]
		[ReadOnly(true)]
		public ZString ConsignmentOrEntryType
		{
			get { return CS_ShipmentType.PadRight(3).Right(2).Trim(); }
			set
			{
				CheckMaximumLength(ConsignmentOrEntryTypeInfo, value);
				var sdc = CS_ShipmentType.Left(1);
				CS_ShipmentType = sdc.PadRight(1) + value.PadRight(2);
				MarkMawbForValidation();
				ConsignmentOrEntryTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsignmentOrEntryType();
				}
			}
		}
		public ZPropertyInfo ConsignmentOrEntryTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignmentOrEntryType)); }
		}

		internal static void WipeStatus1DateAndUncomplete(out ZDateTime localStatus1Date, Logs logs, ZPropertyInfo st1Info, ICcsukCusAwb awb)
		{
			localStatus1Date = ZDateTime.Empty;
			var found = (from StmALog log in logs.GetAllLogs()
						 where log.SL_SE_NKEvent == Events.StatusChange.Code && log.SL_Reference == "ST1"
						 select log).FirstOrDefault();

			if (found != null)
			{
#if DEBUG
				using (found.LockForUpdatingKeyFieldsForTesting())
#endif
				{
					found.SL_Reference = "Old Status 1";
				}
				// Un-complete
				if (awb.IsCompleteOnCcsuk)
				{
					awb.UncompleteOnCcsuk();
				}
				st1Info.RefreshBinding();
			}
		}

		internal static ZDateTime GetStatusDate(string eventCode, Logs logs)
		{
			return (from StmALog log in logs.GetAllLogs()
					where log.SL_SE_NKEvent == Events.StatusChange.Code && log.SL_Reference == eventCode
					select log.SL_EventTime).FirstOrDefault();
		}
		public ZString MasterBill
		{
			get { return MAWB.CM_MAWB_Formatted; }
		}

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
					RegisterEditableChildObject(splits);
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

		public event CusMAWB.SplitsCountChangedEventHandler OnSplitsCountChanged;

		ZString ICcsukCusAwb.SplitReference { get; }

		public ZString SplitReferencesForAllAwbsModuleGrid
		{
			get
			{
				var splitsList = new List<SplitConsignment>();
				if (CS_IsMasterHouse && MAWB.HasSplits)
				{
					splitsList.AddRange(MAWB.Splits.ToArray<SplitConsignment>());
				}
				else if (HasSplits)
				{
					splitsList.AddRange(Splits.ToArray<SplitConsignment>());
				}
				var sb = new ZStringBuilder();
				foreach (SplitConsignment split in splitsList)
				{
					sb.Append("SRF-" + split.SplitReference + " " + split.LatestCustomsActionText);
				}
				return sb.ToStringWithDelimiterBetweenAppends("; ");
			}
		}

		public ZBool HasSplits
		{
			get { return CS_IsMasterHouse && MAWB != null ? (bool)MAWB.HasSplits : Splits.Count > 0; }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ProfilesList))]
		[MaxLength(CcsukConstants.PimaMaxLength)]
		public override ZString CS_FolioReference
		{
			get { return base.CS_FolioReference; }
			set
			{
				base.CS_FolioReference = value;
				if (CanUpdateShedAndAgentUsingPima)
				{
					if (IsProfileAnAgent)  // Also serves agent-airline-in-fallback
					{
						AgentBadge = value.Right(3);
					}
					else if (IsProfileAShed)
					{
						CargoTerminalOperatorAirportAndShed = value.SubstringSafe(8, 6);
						SetPreferredNominatedAgentIfAgentIsBlank();
					}
					else // likely fallback
					{
						if (Branch != null)
						{
							var foundFallbackProfile = false;
							foreach (CredentialsSetting cred in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
							{
								if (cred.PIMA == value && !cred.FallbackForShed.IsEmpty)
								{
									CargoTerminalOperator = cred.FallbackForShed;
									foundFallbackProfile = true;
									break;
								}
							}
							if (foundFallbackProfile)
							{
								AgentBadge = value.Right(3);
								CargoTerminalOperatorAirport = value.SubstringSafe(8, 3);
							}
						}
					}
				}

				CargoTerminalOperatorAirportAndShedInfo.RefreshBinding();
				CS_ResponsiblePartyIDInfo.RefreshBinding();
				CS_FolioReferenceInfo.RefreshBinding();
				MarkMawbForValidation();
			}
		}

		public override ZPropertyInfo CS_FolioReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(CS_FolioReference), "Profile/PIMA"); }
		}

		public event CusMAWB.PimaChangedEventHandler OnPimaChanged;
		[MaxLength(CcsukConstants.PimaMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.ProfilesList))]
		public ZString Profile
		{
			get { return CS_FolioReference; }
			set
			{
				CS_FolioReference = value;
				if (OnPimaChanged != null)
				{
					OnPimaChanged(this);
				}
				ProfileInfo.RefreshBinding();
				MakeOutTurnsReadOnlyIfNprReadOnly(OutTurns, NumberOfPiecesReceivedReadOnly);
			}
		}

		public ZPropertyInfo ProfileInfo => CS_FolioReferenceInfo;

		// To stop agents fiddling with outturns
		internal static void MakeOutTurnsReadOnlyIfNprReadOnly(CusOutTurnCollection outTurns, bool isNprReadOnly)
		{
			if (outTurns != null)
			{
				outTurns.SetReadOnlyIncludingChildren(isNprReadOnly);
				foreach (var outTurn in outTurns)
				{
					outTurn.ReadOnly = isNprReadOnly;  // Can probably go away if you can make the Receipts grid appear/disappear based on Pima.
				}
			}
		}

		void SetPreferredNominatedAgentIfAgentIsBlank()
		{
			var profile = Profile;
			if (AgentBadge.IsEmpty)
			{
				foreach (CredentialsSetting cred in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if (cred.PIMA == profile && !cred.PreferredAgent.IsEmpty)
					{
						AgentBadge = cred.PreferredAgent;
						break;
					}
				}
			}
		}

		internal bool CanUpdateShedAndAgentUsingPima
		{
			get
			{
				return
					PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.OnCommDb
					&&
					PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.ArchivedOnCcsuk
					&&
					PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection
					&&
					PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.CompletedOnCcsUk
					&&
					!CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(CustomsActionCode);
			}
		}

		public bool IsProfileAShed
		{
			get { return LicenceAndPimaHelper.IsFullShed(this); }
		}

		internal bool IsProfileAShedOrHasSplits
		{
			get { return IsProfileAShed || HasSplits; }
		}

		public bool IsProfileAnAgent
		{
			get { return LicenceAndPimaHelper.IsSimpleAgentProfile(this); }
		}

		internal bool IsProfileAnAgentOrHasSplits
		{
			get { return IsProfileAnAgent || HasSplits; }
		}

		[ReadOnly(true)]
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

		public ZPropertyInfo LatestCustomsActionTextInfo
		{
			get => GetZPropertyInfo(Schema.LatestCustomsActionText);
		}

		public new CusMAWB MAWB
		{
			get { return (CusMAWB)base.MAWB; }
		}

		/// <summary>
		///  For humans only
		/// </summary>
		public ZString ReferenceNumber
		{
			get
			{
				if (CS_IsMasterHouse && MAWB != null)
				{
					return MAWB.CM_MAWB_Formatted;
				}

				if (MAWB != null)
				{
					return ZString.Format("{0}-{1}", MAWB.CM_MAWB_Formatted, CS_HAWB);
				}
				else
				{
					return CS_HAWB;
				}
			}
		}

		public ZString ReferenceNumberWithShed
		{
			get { return string.Format("{0}-{1}", CS_WarehouseLocation, ReferenceNumber); }
		}

		ZString ICcsukCusAwb.ChiefMasterUCRReferenceSuffix
		{
			get { return ReferenceNumber.KeepAlphanumericCharacters(); }
		}

		#region Underbond collections
		[ChildEditable]
		public CusUnderbondCollection<TranshipmentRemoval> TSRs
		{
			get { return tsrCollection ?? (tsrCollection = MakeAndLoadAndRegister<TranshipmentRemoval>()); }
		}
		CusUnderbondCollection<TranshipmentRemoval> tsrCollection;

		[ChildEditable]
		public CusUnderbondCollection<InterAirportRemoval> IARs
		{
			get { return iarCollection ?? (iarCollection = MakeAndLoadAndRegister<InterAirportRemoval>()); }
		}
		CusUnderbondCollection<InterAirportRemoval> iarCollection;

		[ChildEditable]
		public CusUnderbondCollection<InterShedRemoval> ISRs
		{
			get { return isrCollection ?? (isrCollection = MakeAndLoadAndRegister<InterShedRemoval>()); }
		}
		CusUnderbondCollection<InterShedRemoval> isrCollection;

		[ChildEditable]
		public CusUnderbondCollection<Fallback> FBKs
		{
			get { return fbkCollection ?? (fbkCollection = MakeAndLoadAndRegister<Fallback>()); }
		}
		CusUnderbondCollection<Fallback> fbkCollection;

		CusUnderbondCollection<T> MakeAndLoadAndRegister<T>() where T : CusUnderbond
		{
			var coll = new CusUnderbondCollection<T>(this);
			coll.Load();
			RegisterEditableChildObject(coll);
			return coll;
		}

		#endregion

		public GlbBranch Branch
		{
			get { return MAWB != null ? MAWB.Branch : GlbBranch.CurrentBranch; }
		}

		void MarkMawbForValidation()
		{
			if (MAWB != null)
			{
				MAWB.MarkAsNeedingValidation();
			}
		}

		public ICuscar GetCuscarWrapper()
		{
			return new CuscarWrapperFromCusHawb(this);
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.CustomsActionsCodes))]
		[MaxLength(4)]
		public ZString CustomsActionCode
		{
			get { return CS_CustomsStatus; }
		}
		public void SetCustomsActionCode(ZString code, ZDateTime date)
		{
			CS_CustomsStatus = code;
			if (!date.IsEmpty)
			{
				LogCustomsActionCodeEvent(code, date.ToOffset(), Logs, PK);
			}
			if (!Status1Date.IsEmpty && CusAwbIsReadOnlyHelper.FinalisedCustomsStatusCodes.Contains(CustomsActionCode))
			{
				((ICcsukCusAwb)this).CompleteOnCcsuk();
			}
		}

		internal static void LogCustomsActionCodeEvent(ZString code, ZDateTimeOffset date, Logs logs, ZGuid pk)
		{
			var existingLog = logs.Find(
					log =>
					{
						return log.SL_SE_NKEvent == Events.CustomsEntryStatus.Code
						&& log.SL_Reference == code
						&& log.SL_Parent == pk
						&& log.SL_EventTimeOffset == date; // Makes sure we do not find an old/first log record if we receive e.g. CB-CX-CB
					}).FirstOrDefault();
			if (existingLog == null)
			{
				if (date.IsEmpty && !code.IsEmpty)
				{
					date = ZDateTimeOffset.Now;
				}

				logs.AddNew(Events.CustomsEntryStatus, code, date);
			}
		}

		public ZDateTime CustomsActionDate
		{
			get { return GetCustomsActionDate(CustomsActionCode, PK, Logs); }
		}

		internal static ZDateTime GetCustomsActionDate(ZString cac, ZGuid pkOfParentOfLogs, Logs logs)
		{
			if (cac.IsEmpty)
			{
				return ZDateTime.Empty;
			}
			var query = new ZQuery(StmALogSchema.SL_Reference, cac);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, pkOfParentOfLogs);
			query.OrderBy = StmALogSchema.SL_EventTime.Name;
			var existingLog = logs.Find(query).LastOrDefault();
			return existingLog != null ? existingLog.SL_EventTime : ZDateTime.Empty;
		}

		public override ZString CS_CustomsStatus
		{
			get { return base.CS_CustomsStatus; }
			set
			{
				base.CS_CustomsStatus = value;
				MarkMawbForValidation();
			}
		}

		public bool UpdateStatusToCacIfAllowed(ZString customsActionCode, ZDateTime customsActionDate, ZString agentReference, ZString customsActionText)
		{
			return new ConsignmentStatusUpdater(this).Update(customsActionCode, customsActionDate, agentReference, customsActionText);
		}

		ZDateTime status1Date;

		/// <summary>
		///   Has declaration, wtih entry headers, with entry number
		/// </summary>
		public bool HasEntryWithLodgedOrPrelodgedWithCustoms
		{
			get
			{
				var hasEntryWithLodgedOrPrelodgedWithCustoms = false;
				if (HasDeclaration && Declaration.CustomsEntryHeaders.Count > 0)
				{
					var entry = Declaration.CustomsEntryHeaders[0];
					if (Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF)
					{
						hasEntryWithLodgedOrPrelodgedWithCustoms = !entry.EntryNumber.IsEmpty;
					}
					else if (Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
					{
						hasEntryWithLodgedOrPrelodgedWithCustoms = !(entry.CH_EntryStatus.IsEmpty || entry.CH_EntryStatus == CDS.Constants.ThreeCharFunctionCodes.MessageRejected);
					}
				}
				return hasEntryWithLodgedOrPrelodgedWithCustoms;
			}
		}

		public ZString ChiefDeclarationUCR
		{
			get { return HasDeclaration ? Declaration.JE_UCR : ZString.Empty; }
		}

		public ZPropertyInfo ChiefDeclarationUCRInfo
		{
			get { return GetZPropertyInfo(nameof(ChiefDeclarationUCR)); }
		}

		/// <summary>
		/// CS_JE is set. 
		/// </summary>
		public bool HasDeclaration
		{
			get { return Declaration != null; }
		}

		public ZString ReasonForNotAllowSplit => ZString.Empty;

		public void SynchroniseFromShipment(ForwardingShipment shipment)
		{
			if (ShouldSynchronise)
			{
				var syncher = new CusHawbToShipmentSynchroniser(this);
				syncher.SynchroniseFromShipment(shipment);
			}
		}

		bool ShouldSynchronise
		{
			get { return Messages.Count == 0; }
		}

		protected override ZString HumanReadableNameCore => CS_IsMasterHouse
			? Res.GetString("9F6EEB18-1350-4CD9-B731-B0539B1F6FC0", "{0} (helper)", MAWB.HumanReadableName)
			: Res.GetString("8CD2DD43-69A7-4AAE-9045-1767AD74BC14", "CCS-UK House Bill {0}", ReferenceNumber);

		JobDeclaration declaration;
		public JobDeclaration Declaration
		{
			get
			{
				if (declaration == null || declaration.IsDeleted)
				{
					if (!CS_JE_CustomsFormalEntry.IsEmpty)
					{
						declaration = Factory.Load<JobDeclaration>(CS_JE_CustomsFormalEntry);
					}
				}
				return declaration;
			}
		}

		public GlbStaff UserInChargeOfJob
		{
			get
			{
				if (!CS_IsMasterHouse && Declaration != null && !Declaration.JE_GS_NKCusAgent.IsEmpty)
				{
					return Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, Declaration.JE_GS_NKCusAgent));
				}
				else if (Messages != null && Messages.Count > 0 && Messages.LastOutgoingNonSystemAndNonNullUserMessage != null)
				{
					return Messages.LastOutgoingNonSystemAndNonNullUserMessage.UserWhoQueuedThisRecord;
				}
				return null;
			}
		}

		public IFSR GetAwbToFsrProvider(CcsukTransmissionMessageFunction how)
		{
			return new CusHawbToFsrProvider(this, how.MessageSubType);
		}

		public ZBool IsThroughAwb
		{
			get { return !AirportOfDestination.IsEmpty && AirportOfDestination != AirportOfArrival; }
		}

		public override ZBool CS_IsMasterHouse
		{
			get { return base.CS_IsMasterHouse; }
			set
			{
				base.CS_IsMasterHouse = value;
				if (MAWB != null)
				{
					MAWB.MarkAsNeedingValidation();
				}
			}
		}

		ZShort Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.NumberOfPiecesExpected
		{
			get { return CS_PiecesManifested; }
			set { CS_PiecesManifested = value; }
		}

		ZShort Integration.Customs.GB.CCSUK.ICcsukCusAwbBase.NumberOfPiecesReceived
		{
			get { return CS_PiecesLanded; }
			set { CS_PiecesLanded = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.NonUkAirports))]
		[ReadOnlyMember(nameof(CS_RL_NKLoadPortReadOnly))]
		public ZString AirportOfOrigin
		{
			get { return CS_RL_NKLoadPort; }
			set
			{
				var oldValue = CS_RL_NKLoadPort;
				CS_RL_NKLoadPort = value;
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		public ZPropertyInfo AirportOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CS_RL_NKLoadPort, x => CS_RL_NKLoadPortInfo); }
		}
		[ReadOnlyMember(nameof(CS_RL_NKLoadPortReadOnly))]
		public override ZString CS_RL_NKLoadPort
		{
			get { return base.CS_RL_NKLoadPort; }
			set
			{
				base.CS_RL_NKLoadPort = value;
				CusMAWB.InitialiseShipmentDescriptionCode(ShipmentDescriptionCodeInfo, this, Factory);
				MarkMawbForValidation();
			}
		}
		bool CS_RL_NKLoadPortReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfOrigin); }
		}

		[ReadOnlyMember(nameof(CS_RL_NKDestinationReadOnly))]
		public override ZString CS_RL_NKDestination
		{
			get { return base.CS_RL_NKDestination; }
			set
			{
				base.CS_RL_NKDestination = value;
				CusMAWB.DefaultOtherAirports(value, AirportOfDestinationInfo, CargoTerminalOperatorAirportInfo);
			}
		}
		bool CS_RL_NKDestinationReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination); }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.UkInventoryControlledAirportsList))]
		[MaxLength(3)] //IATA
		public ZString AirportOfArrival
		{
			get { return CS_RL_NKDestination; }
			set { CS_RL_NKDestination = value; }
		}
		public ZPropertyInfo AirportOfArrivalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CS_RL_NKDestination, x => CS_RL_NKDestinationInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.UkInventoryControlledAirportsList))]
		[MaxLength(3)] //IATA
		public ZString AirportOfDestination
		{
			get { return CS_RL_NKDischargePort; }
			set
			{
				var oldValue = CS_RL_NKDischargePort;
				CS_RL_NKDischargePort = value;
				MaybeWipeClearanceDetailsIfCacIsCu(oldValue, value);
			}
		}
		public ZPropertyInfo AirportOfDestinationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CS_RL_NKDischargePort, x => CS_RL_NKDischargePortInfo); }
		}

		[ReadOnlyMember(nameof(CS_RL_NKDischargePortReadOnly))]
		public override ZString CS_RL_NKDischargePort
		{
			get { return base.CS_RL_NKDischargePort; }
			set
			{
				base.CS_RL_NKDischargePort = value;
				CusMAWB.DefaultOtherAirports(value, AirportOfArrivalInfo, CargoTerminalOperatorAirportInfo);
				CusMAWB.InitialiseShipmentDescriptionCode(ShipmentDescriptionCodeInfo, this, Factory);
			}
		}
		bool CS_RL_NKDischargePortReadOnly
		{
			get { return HasSplits || ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination); }
		}

		public ControllerID ModuleControllerId
		{
			get { return ControllerIDs.Customs.GB.CcsukAirInventoryHouse; }
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

		CusOutTurnCollection outTurns;
		[ChildEditable]
		public CusOutTurnCollection OutTurns
		{
			get
			{
				if (outTurns == null)
				{
					outTurns = new CusOutTurnCollection(this);
					RegisterEditableChildObject(outTurns);
					outTurns.Load();
				}
				return outTurns;
			}
		}

		AllCusUnderbondsCollection allCusUnderbonds;
		public AllCusUnderbondsCollection AllCusUnderbonds
		{
			get
			{
				if (allCusUnderbonds == null)
				{
					allCusUnderbonds = new AllCusUnderbondsCollection(this);
					allCusUnderbonds.Load();
				}
				return allCusUnderbonds;
			}
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
				if (CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestCancelled && Declaration != null && Declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
				{
					var entry = Declaration.CustomsEntryHeaders.FirstOrDefault();
					isEntryCancelled = entry?.IsCancelledWithCustoms ?? false;
				}
				return isEntryCancelled;
			}
		}

		void ICcsukCusAwb.ArchiveOnCcsuk(ReasonForArchiving why)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ArchivedOnCcsuk;
			Logs.AddNew(Events.DocumentDeleted, CcsukUtilities.GetEnumDescription(why), ZDateTimeOffset.Now);
			if (why != ReasonForArchiving.LastChildRecordWasArchived || !CS_IsMasterHouse)
			{
				CcsukUtilities.ArchiveParentIfLastChildIsNowArchived(this, MAWB, MAWB.ChildBills.Cast<ICcsukCusAwb>());
			}
		}

		void ICcsukCusAwb.CompleteOnCcsuk(bool propagateToParent)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			Logs.AddNew(Events.TaskCompleted, "Completed on CCSUK", ZDateTimeOffset.Now);
			if (propagateToParent)
			{
				CcsukUtilities.CompleteParentIfLastChildIsNowComplete(this, MAWB, MAWB.ChildBills.Cast<ICcsukCusAwb>());
			}
		}

		void ICcsukCusAwb.UncompleteOnCcsuk(bool propagateToParent)
		{
			PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Logs.AddNew(Events.TaskCompleted, "Revoked completion on CCSUK", ZDateTimeOffset.Now);
			if (propagateToParent)
			{
				if (((ICcsukCusAwb)MAWB).IsCompleteOnCcsuk)
				{
					((ICcsukCusAwb)MAWB).UncompleteOnCcsuk();
				}
			}
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

		JobDeclaration ICcsukCusAwb.CreateNewStandaloneCDSDeclaration()
		{
			JobDeclaration declaration = null;
			if (!HasDeclaration && !IsLinkedToJobShipment && ReadOnlyAndPermissionHelper.IsActionAllowed(Actions.CW_CanCreateDeclaration))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_HouseBill = CS_HAWB;
				declaration.JE_MasterBill = MAWB.CM_MAWB;
				declaration.ZG_ShipmentType = ShipmentTypeList.Codes.HouseConsignment;
				declaration.JE_DateOfArrival = MAWB.CM_ArrivalDate;
				declaration.JE_EntrySubStyle = IsPrearrival ?
					GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullDeclarationGoodsNotArrived :
					GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
				declaration.JE_VoyageFlightNo = MAWB.CM_FlightNo;
				SynchroniseToDeclaration(declaration, true, true);
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
				Factory.Save();
				ChiefDeclarationUCRInfo.RefreshBinding();
			}
			return declaration;
		}

		void ICcsukCusAwb.SetEcStatusRelease(bool isSetting)
		{
			AwbEcStatusReleaseSetUnsetHelper.SetEcStatusRelease(isSetting, this, this);
		}

		public bool IsLastHouseOnConsol
		{
			get { return !CS_IsMasterHouse && (MAWB.ChildBills.Count == 1 || !HasBrothersProbablyOnNetwork); }
		}

		public bool HasBrothersProbablyOnNetwork
		{
			get
			{
				return (from CusHAWB brother in MAWB.ChildBills
						where brother.PK != PK
						&& new ZString[] { PresenceOnNetworkList.Codes.OnCommDb,
										PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection,
										PresenceOnNetworkList.Codes.CompletedOnCcsUk,
										PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask
									}.Contains(brother.PresenceOnNetworkStatus)
						select brother).Any();
			}
		}

		void MaybeWipeClearanceDetailsIfCacIsCu(ZString oldValue, ZString newValue)
		{
			MaybeWipeClearanceDetailsIfCacIsCuShared(this, oldValue, newValue, IsProfileAShed);
		}

		internal static void MaybeWipeClearanceDetailsIfCacIsCuShared(ICcsukCusAwb awb, ZString oldValue, ZString newValue, bool isProfileAShed)
		{
			if (oldValue != newValue
				&& awb.CustomsActionCode == CustomsStatusCodes.Codes.ThroughAirWaybillReleased
				&& isProfileAShed
				&& LicenceAndPimaHelper.ShedEnabled)
			{
				awb.LatestCustomsActionText = "(CAT wiped on edit, send FRC & await new FSN)";
				awb.SetCustomsActionCode("", ZDateTime.Empty);
			}
		}

		#region public CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodes
		[ChildEditable(true)]
		public CusAddInfoCollection<CommunityHandlingCode> CommunityHandlingCodes
		{
			get { return communityHandlingCodes ?? (communityHandlingCodes = GetCommunityHandlingCodes()); }
		}
		CusAddInfoCollection<CommunityHandlingCode> communityHandlingCodes;

		CusAddInfoCollection<CommunityHandlingCode> GetCommunityHandlingCodes()
		{
			var result = new CusAddInfoCollection<CommunityHandlingCode>(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}
		#endregion

		#region ERTS Licence

		public event LicenceLoginEventHandler CcsukLicenceLoginHandler;

		internal bool CanRaiseCcsukLicenceLogin(LicenceAndPimaHelper.CcsukLicenceType type)
		{
			return CcsukLicenceLoginHandler != null && (type == LicenceAndPimaHelper.CcsukLicenceType.Agent || (type == LicenceAndPimaHelper.CcsukLicenceType.ErtsShed && Environment.Env.Security.AirCcsukShed.IsAllowed));
		}

		public void RaiseCcsukLicenceLogin(LicenceAndPimaHelper.CcsukLicenceLoginEventArgs e)
		{
			if (CcsukLicenceLoginHandler != null)
			{
				CcsukLicenceLoginHandler(this, e);
			}
		}

		#endregion

		#region Workflow

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusHAWBProcessTaskCollection()
		{
			return new CusHAWBProcessTaskCollection(this);
		}

		#region IWorkflowTriggerEventSource Members		

		public IGlbCompany JobHeaderCompany
		{
			get { return Branch != null ? Branch.Company : null; }
		}

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				if (CS_IsMasterHouse)
				{
					list.Add(MAWB);
				}
				return list;
			}
		}

		#endregion

		#endregion

		/// <param name="alsoLinkPhysicalFreightProperties">Tells us whether to also set things like descripotion, mass, pieces, etc.  If linking via a shipment, pass FALSE as the declaration/shipment synchroniser will do all that </param>
		public void SynchroniseToDeclaration(JobDeclaration declaration, bool alsoLinkPhysicalFreightProperties = false, bool setCS_JEOnHawb = true)
		{
			var awb = this as ICcsukCusAwb;
			if (setCS_JEOnHawb)
			{
				CS_JE_CustomsFormalEntry = declaration.PK;
			}
			declaration.JE_CustomsProfile = AgentBadge;
			if (!CargoTerminalOperatorAirport.IsEmpty)
			{
				declaration.JE_LocationOfGoods = PortConverter.IataToChief(CargoTerminalOperatorAirport, Factory).Left(declaration.JE_LocationOfGoodsInfo.MaxLength);
			}
			declaration.SubLocation = CargoTerminalOperator;
			if (alsoLinkPhysicalFreightProperties)
			{
				declaration.JE_TotalNoOfPacks = awb.NumberOfPiecesExpected;
				RefUNLOCO portOfArrival = null;
				if (!AirportOfArrival.IsEmpty)
				{
					portOfArrival = RefUNLOCO.LoadFromIATA(Factory, AirportOfArrival);
				}
				declaration.JE_RL_NKPortOfArrival = portOfArrival == null ? AirportOfArrival : portOfArrival.RL_Code;
				RefUNLOCO portOfFinalDestination = null;
				if (!AirportOfDestination.IsEmpty)
				{
					portOfFinalDestination = RefUNLOCO.LoadFromIATA(Factory, AirportOfDestination);
				}
				declaration.JE_RL_NKFinalDestination = portOfFinalDestination == null ? AirportOfDestination : portOfFinalDestination.RL_Code;
				declaration.JE_RL_NKOrigin = AirportOfOrigin;
				declaration.JE_GoodsDescription = DescriptionOfGoods.Left(ForwardingShipment.Schema.JS_GoodsDescriptionMaxLength);
				declaration.JE_TotalWeight = awb.Weight;
				declaration.JE_TotalWeightUnit = awb.WeightCode;
			}
			declaration.Logs.AddNew(Events.StatusUpdated, string.Format("Synchronised from CCSUK AWB. {0} {1} {2}", CS_WarehouseLocation, MAWB.CM_MAWB, CS_HAWB), ZDateTimeOffset.Now);
		}

		public ZString DisplayTextForCustomsCargoStatusColumn
		{
			get
			{
				var result = ZString.Empty;
				if (LatestCustomsActionText.IsEmpty)
				{
					result = string.Format("Presence {0} @ {1}", PresenceOnNetworkStatus, CS_WarehouseLocation);
				}
				else
				{
					result = string.Format("{0} @ {1}", LatestCustomsActionText, CS_WarehouseLocation);
				}
				if (Shipment != null && Shipment.JS_HouseBill != CS_HAWB)
				{
					result = string.Format("{0} {1}", CS_HAWB, result);
				}
				if (!Status1Date.IsEmpty)
				{
					result = string.Format("{0} St1", result);
				}
				return result;
			}
		}

		public ZInt NumberOfPiecesDelivered
		{
			get { return OutTurns.TotalDelivered; }
		}

		void IAgentBadgeValidationProvider.ValidateAgentBadge()
		{
			Validation.ValidateCS_ResponsiblePartyID();
		}

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusAWBMultiMessageManagerForFRC<CusHAWB>(delegate
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
				if (!CS_IsMasterHouse && OutTurns != null && !NumberOfPiecesReceivedReadOnly)
				{
					var newNpr = (ZShort)(from CusOutTurn cot in OutTurns where !cot.IsDeleted select (int)cot.C5_PackagesOutturned).Sum();
					CS_PiecesLanded = newNpr;
					if (CS_PiecesLanded == CS_PiecesManifested && Status1Date.IsEmpty && CS_PiecesLanded > 0)
					{
						Status1Date = ZDateTime.Now;
					}
					CS_PiecesLandedInfo.RefreshBinding();
					if (HasSplits)
					{
						foreach (SplitConsignment split in Splits)
						{
							split.CalculateNprFromReceiptsIfNecessary();
						}
					}
					if (MAWB != null)
					{
						MAWB.CalculateNprFromReceiptsIfNecessary();
					}
				}
			}
		}

		protected override void ReloadCore()
		{
			base.ReloadCore();
			outTurns = null;  //otherwise: when the back-end uploads this hawb's interchange, reloads the hawb, updates this hawb's presensce and finally saves, it will see the original collection.  The reload does not refresh the collection of ourturns. Force it here. 
		}

		CusOutTurn ICcsukCusAwb.CreateNewOutTurn()
		{
			return OutTurns.AddNew();
		}

		#region Stuff for expunging release notes 

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
		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.GbCcsukSpecialHandling, typeof(CusAddInfo<CommunityHandlingCode>));
			return result;
		}

		#endregion

		public override bool ReadOnly
		{
			get { return base.ReadOnly || !Env.Security.AirCcsukHouse.IsAllowed; }
			set { base.ReadOnly = value; }
		}

		public bool IsNonStandardHawbNumberSoDoNotSynch { get; set; }

		public bool IsSpent => !CustomsActionCode.IsEmpty && CustomsActionCode != CustomsStatusCodes.Codes.EntryOrRequestCancelled;
	}
}
