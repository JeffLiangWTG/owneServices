using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.FR;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
		, IDepartureMovementHeader, IHarbourJob, IFRMessagesOwner, ICorrelationIDProvider, IAllowPermitProcessing
	{
		public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : EU.NCTS.Business.NctsDepartureMovementHeader.Schema
		{
			public const string ChargePaymentOrDestinationID = "ChargePaymentOrDestinationID";
			public const string CustomsOffice = "CustomsOffice";
			public const string BarrierPort = "BarrierPort";
			public const string CorrelationID = "CorrelationID";
			public const int CorrelationMaxLength = 10;
		}

		#region CorrelationID

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		public ZString CorrelationID
		{
			get { return Header.CorrelationID; }
			set { Header.CorrelationID = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => Header.CorrelationIDEntryNumber.CE_EntryNumInfo); } }

		public ZString CorrelationIDPrefix => ZString.Empty;

		#endregion

		#region IHarbourJob Implementation

		public ZDateTime ValuationDate => ZDateTime.Today;

		public ZString HarbourType => Customs.Common.EU.EUJobMessageTypeList.Codes.Import;

		public ZString DataGrouping => DefaultDataGroupingCode;

		public ZString ContainerMode => Header.DepartureHeaderContainers.Cast<FRNctsDepartureHeaderContainer>().FirstOrDefault()?.BC_Mode ?? ZString.Empty;

		public ZString CustomsOffice => IsPhase5 ? DepartureCustomsOffice?.OfficeCode ?? ZString.Empty : Header.DepartureCustomsOffice?.OfficeCode ?? ZString.Empty;

		public ZString BarrierPort => FRLookups.BarrierPort;

		ZBool IHarbourJob.IsDCN => true;

		#endregion

		public override ZString BM_InBondEntryType
		{
			get => base.BM_InBondEntryType;
			set
			{
				if (BM_InBondEntryType != value)
				{
					base.BM_InBondEntryType = value;
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						Header?.FRNctsHeader.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(FRLookups) + "." + nameof(IFRNctsDepartureMovementHeaderLookups.AuthorizedLocationOfGoodsCodeList))]
		public override ZString BM_LocationOfGoodsCode
		{
			get => base.BM_LocationOfGoodsCode;
			set
			{
				if (base.BM_LocationOfGoodsCode != value)
				{
					base.BM_LocationOfGoodsCode = value;
					if (isSaving)
					{
						var exceptionMessage = $"Value of BM_LocationOfGoodsCode unexpectedly changed during saving, IsInDataBase: {(IsInDatabase ? (NoResString)"True" : (NoResString)"False")}.";
						ExceptionReporter.Instance.ReportDeveloperException(exceptionMessage, new InvalidOperationException(exceptionMessage));
					}
				}
			}
		}

		[RelatedBusinessObject("PortOfPresentation")]
		[List(nameof(FRLookups) + "." + nameof(NctsDepartureMovementHeaderPhase5Lookups.PortOfPresentationList))]
		public override ZString BM_RL_NKPortOfPresentation
		{
			get
			{
				return base.BM_RL_NKPortOfPresentation;
			}
			set
			{
				base.BM_RL_NKPortOfPresentation = value;
			}
		}

		public override ZString BM_TypeOfSecurity
		{
			get => base.BM_TypeOfSecurity;
			set
			{
				base.BM_TypeOfSecurity = value;
				if (!IsMarkingAsNeedingValidationSuspended)
				{
					Header?.FRNctsHeader.MarkAsNeedingValidation();
				}
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[] { Schema.BM_EntryDate, Schema.BM_ValuationDate };
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedMoveHeader = base.CloneInternal(args);
			clonedMoveHeader.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ChargePaymentOrDestinationIDType, ChargePaymentOrDestinationID);
			return clonedMoveHeader;
		}

		[MaxLength(3)]
		[ResourceStringData("FR.Business.NctsDepartureMovementHeader|ChargePaymentOrDestinationID", Caption = "Payment/Destination")]
		[List(nameof(FRLookups) + "." + nameof(NctsDepartureMovementHeaderPhase4Lookups.PaymentDestinationList))]
		public ZString ChargePaymentOrDestinationID
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.ChargePaymentOrDestinationIDType);
			set
			{
				CheckMaximumLength(ChargePaymentOrDestinationIDInfo, value);
				var oldValue = ChargePaymentOrDestinationID;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ChargePaymentOrDestinationIDType, value);

				if (oldValue != ChargePaymentOrDestinationID)
				{
					new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(this);
				}

				if (!IsValidationSuspended)
				{
					if (IsPhase5)
					{
						(Validation as NctsDepartureMovementHeaderPhase5Validation)?.ValidateChargePaymentOrDestinationID();
					}
					else
					{
						(Validation as NctsDepartureMovementHeaderPhase4Validation)?.ValidateChargePaymentOrDestinationID();
					}
				}

				ChargePaymentOrDestinationIDInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ChargePaymentOrDestinationIDInfo => GetZPropertyInfo(Schema.ChargePaymentOrDestinationID);

		public new INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

		protected override INctsCommonCargoDescCollection<NctsCommonCargoDesc> CreateGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		public new NctsHeader Header => (NctsHeader)base.Header;

		FREDIMessageCollection IFRMessagesOwner.Messages => (FREDIMessageCollection)base.Messages;

		protected override EDIMessageCollection GetNewMessageCollection() => new FREDIMessageCollection(this);

		protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation GetNewPhase4Validation() => new NctsDepartureMovementHeaderPhase4Validation(this);

		protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderPhase5Validation(this);

		public new EU.Business.ICusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsDepartureMovementHeader> CusAuthorizationUsages => (CusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsDepartureMovementHeader>)base.CusAuthorizationUsages;

		protected override EU.Business.ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsDepartureMovementHeader> GetCusAuthorizationUsages()
			=> new CusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsDepartureMovementHeader>(this);

		public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<FRNctsGuarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<FRNctsGuarantee>(this);

		protected override ZBool GetPreLodgedForAgreedLocationOfGoodsCode() => Header.IsPrelodgedMovement;

		protected override void SetPreLodgedForAgreedLocationOfGoodsCode(bool value)
		{
			Header.IsPrelodgedMovement = value;
		}

		protected override ZBool IsContainerised_Phase5 => Header.Bills.Any(bill => bill.GoodsItems.Any(item => item.Packages.Any(package => package.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>().Any(container => container.ContainerSelected && !container.ContainerNumber.IsEmpty))));

		public IFRNctsDepartureMovementHeaderLookups FRLookups => (IFRNctsDepartureMovementHeaderLookups)Lookups;

		protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderPhase5Lookups(this);

		protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderPhase4Lookups(this);

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new NctsDepartureHeaderMovementHeaderValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		NctsDepartureHeaderMovementHeaderValueSetStrategy NctsDepartureHeaderMovementHeaderValueSetStrategy => (NctsDepartureHeaderMovementHeaderValueSetStrategy)GetValueSetStrategy();

		internal void ChangeValueInBMLocationOfGoodsCode() => NctsDepartureHeaderMovementHeaderValueSetStrategy.ChangeValueInBMLocationOfGoodsCode();

		protected override Type CusInBondCargoDescTypeCore => typeof(NctsDepartureCargoDesc);

		public ZString LastNonIntermediateStatus => Header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code)).Where(x => IsNonIntermediateStatus(x.SL_Reference)).OrderByDescending(x => x.SL_EventTime).FirstOrDefault()?.SL_Reference.Left(3) ?? ZString.Empty;

		ZBool IsNonIntermediateStatus(ZString status) => NonIntermediateStatusList.Contains(status);

		public ImmutableArray<string> NonIntermediateStatusList => (nonIntermediateStatusList ?? (nonIntermediateStatusList = new CachedValue<ImmutableArray<string>>(() => ImmutableArray.Create(
		NctsTransitStatusList.Codes.DeclarationRejected,
		NctsTransitStatusList.Codes.DeclarationAccepted,
		NctsTransitStatusList.Codes.DeclarationMrnAllocated,
		NctsTransitStatusList.Codes.DeclarationCancelled,
		NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
		NctsTransitStatusList.Codes.GoodsNotReleasedForTransit,
		NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture,
		NctsTransitStatusList.Codes.DeclarationCancelled,
		NctsTransitStatusList.Codes.ReadyForAmendment)))).Value;

		CachedValue<ImmutableArray<string>> nonIntermediateStatusList;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_InBondEntryType = CusEntryNumberTypes.EU.T1;
			BM_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
		}

		public override void OnSaving()
		{
			using (new DisposableAction(() => isSaving = true, () => isSaving = false))
			{
				base.OnSaving();
#if DEBUG
				ActionDuringSaving();
#endif
			}
		}

		bool isSaving;

#if DEBUG
		protected virtual void ActionDuringSaving()
		{
		}

#endif

		[ChildEditable(true)]
		public new NctsFrOfficeCodeCollection CustomsOffices => (NctsFrOfficeCodeCollection)base.CustomsOffices;

		protected override NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsFrOfficeCodeCollection(this);

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.OfficeCode, typeof(NctsFrOfficeCode) },
			};
		}

		public bool IsDepartureCancellationAllowed
		{
			get
			{
				var departureStatus = BM_CustomsStatus;
				return departureStatus == NctsTransitStatusList.Codes.DeclarationAccepted
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid
						|| departureStatus == NctsTransitStatusList.Codes.DeclarationMrnAllocated
						|| departureStatus == NctsTransitStatusList.Codes.GoodsNotReleasedForTransit
						|| departureStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture
						|| departureStatus == NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival
						|| departureStatus == NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
			}
		}

		public OrgHeader ProcedureHolder => Representative?.Organisation ?? Header.Principal?.Organisation;

		public ZString GetPermitReference() => BM_PaperlessInbondNum;

		public ZInt GetPermitReferenceNumberLine() => Header.GetPermitReferenceNumberLine();

		public IList<PermitRecord> GetPermitRecords() => Header.GetPermitRecords();

		public ZString GetPermitComment(PermitRecord permitRecord) => Header.GetPermitComment(permitRecord);

		public ZInt PermitValueDecimalPlaceCount => Header.PermitValueDecimalPlaceCount;

		public ZInt PermitQuantityDecimalPlaceCount => Header.PermitQuantityDecimalPlaceCount;

		public ZInt PackageCount => (Header as IAllowPermitProcessing).PackageCount;
	}
}
