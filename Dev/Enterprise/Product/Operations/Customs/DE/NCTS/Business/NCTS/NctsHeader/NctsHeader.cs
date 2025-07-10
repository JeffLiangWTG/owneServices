using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.NCTS.Business
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.DENctsHeader)]
	public class NctsHeader : EU.NCTS.Business.NctsHeader
		, Integration.Customs.DE.ICusInBondHeader
		, IRelatedJob
		, IDepartureCustomsOfficeCodeProvider
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

		public new NonPersistentItineraryCountryCollection Itinerary => (NonPersistentItineraryCountryCollection)base.Itinerary;

		public new NctsHeaderValidation Validation => (NctsHeaderValidation)base.Validation;

		public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

		public event CancelEventHandler OnPreviousProcedureMasterCSI_ProcedureAboutToChange;

		public void PreviousProcedureMasterCSI_ProcedureAboutToChange(object sender, CancelEventArgs args) => OnPreviousProcedureMasterCSI_ProcedureAboutToChange?.Invoke(sender, args);

		protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new EU.NCTS.Business.NctsGuaranteeCollection<Guarantee>(this);

		protected override EU.NCTS.Business.NonPersistentItineraryCountryCollection GetItineraryCore() => new NonPersistentItineraryCountryCollection(this);

		public new UnloadingRemarkAddInfo UnloadingRemark => (UnloadingRemarkAddInfo)base.UnloadingRemark;

		protected override CusAddInfoCollection<EU.NCTS.Business.UnloadingRemarkAddInfo> GetUnloadingRemarkAddInfoCollectionCore() => new UnloadingRemarkAddInfoCollection(this);

		public new EU.NCTS.Business.INctsBillCollection<NctsBill> Bills => (EU.NCTS.Business.INctsBillCollection<NctsBill>)base.Bills;
		protected override EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new EU.NCTS.Business.NctsBillCollection<NctsBill>(this);

		public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments => (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

		protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

		public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;

		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		protected override ZBool IsBrokerNeededCore => false;

		protected override CusInBondHeaderValidation GetNewPhase5Validation() => IsArrivalMovement ? new ArrivalNctsHeaderValidation(this) : new NctsHeaderValidation(this);

		protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

		public new EU.NCTS.Business.SealCollection<Seal> Seals => (EU.NCTS.Business.SealCollection<Seal>)base.Seals;

		protected override EU.NCTS.Business.SealCollection GetNewSealCollection() => new EU.NCTS.Business.SealCollection<Seal>(this);

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[EU.NCTS.Business.CusCodeDataTypeList.Codes.Seal] = typeof(Seal);
			return result;
		}

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type PreviousDocumentType => typeof(CommonPreviousDocument);

		protected override Dictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = base.GetCusAddInfoTypes();
			result[CusAddInfoTypeAttribute.Codes.EuNctsUnloadingRemark] = typeof(CusAddInfoUnloadingRemarkAddInfo);
			return result;
		}

		protected override ZBool CanOverrideDestinationTraderAddress => false;

		protected override ZString DefaultApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;

		#region Properties

		[ReadOnlyMember(nameof(BH_ExportFlag_ReadOnly))]
		public override ZString BH_ExportFlag
		{
			get => base.BH_ExportFlag;
			set
			{
				var oldValue = BH_ExportFlag;
				base.BH_ExportFlag = value;
				if (!IsCopying && oldValue != BH_ExportFlag)
				{
					var isArrivalEventNotAvailable = !IsArrivalEventAvailable;
					EnRouteIncidents.SetReadOnlyIncludingChildren(isArrivalEventNotAvailable);
					EnRouteTransshipments.SetReadOnlyIncludingChildren(isArrivalEventNotAvailable);
					EnRouteSeals.SetReadOnlyIncludingChildren(isArrivalEventNotAvailable);
				}
			}
		}

		ZBool BH_ExportFlag_ReadOnly => BH_ExportFlag == EU.NCTS.Business.EventFlagList.Codes.Cancelled && (EventFlagYesLog?.IsCancelled ?? ZBool.False);

		public StmALog EventFlagYesLog => Logs.Find(x => x.SL_SE_NKEvent == Events.MiscellaneousEvent.Code && x.SL_Reference == EventFlagYesLogReference).SingleOrDefault();

		const string EventFlagYesLogReference = "EventFlag=Y";

		public void LogEventFlagYes()
		{
			if (BH_ExportFlag == EU.NCTS.Business.EventFlagList.Codes.Yes)
			{
				Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.MiscellaneousEvent, eventTime: ZDateTimeOffset.Now, reference: EventFlagYesLogReference), EventFlagYesLog);
			}
		}

		public void CancelEventFlagYesLog()
		{
			if (BH_ExportFlag == EU.NCTS.Business.EventFlagList.Codes.Cancelled)
			{
				EventFlagYesLog?.Cancel();
			}
		}

		[ResourceStringData("58b5b04a-ed49-4602-b077-97aedf5b8a45", Caption = "Reason")]
		[ReadOnlyMember(nameof(EventCancellationReasonReadOnly))]
		public ZString EventCancellationReason
		{
			get => Notes.GetNoteText(PredefinedNoteTypes.Instance.NCTSEventCancellationReason.Description);
			set
			{
				var oldValue = EventCancellationReason;
				Notes.SetNoteText(this, EventCancellationReasonInfo, PredefinedNoteTypes.Instance.NCTSEventCancellationReason.Description, value);
				if (!IsCopying && oldValue != EventCancellationReason)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateEventCancellationReason();
					}
				}
			}
		}

		public ZPropertyInfo EventCancellationReasonInfo => GetZPropertyInfo(nameof(EventCancellationReason));

		protected ZBool EventCancellationReasonReadOnly => BH_ExportFlag != EU.NCTS.Business.EventFlagList.Codes.Cancelled;

		[ResourceStringData("DENctsHeader.ArrivalMrnFromUser", Caption = "MRN", FullDescription = "Movement Reference Number")]
		[MaxLength(18)]
		public override ZString ArrivalMrnFromUser
		{
			get => base.ArrivalMrnFromUser;
			set => base.ArrivalMrnFromUser = value;
		}

		[LightValidationTestExempt]
		public override ZString BH_HeaderType
		{
			get => base.BH_HeaderType;
			set
			{
				base.BH_HeaderType = value;
				if (IsArrivalMovement)
				{
					BH_ExportFlag = EU.NCTS.Business.EventFlagList.Codes.No;

					if (GlbBranch.CurrentBranch.OrgProxy is OrgHeader orgHeader)
					{
						DestinationTrader.E2_OA_Address = orgHeader.MainAddress.PK;
					}
				}
			}
		}

		public override ZString PlaceOfUnloading
		{
			get
			{
				var result = base.PlaceOfUnloading;
				if (IsArrivalMovementAllowed && ArrivalMovementHeader.Lookups.PortsOfUnloadingList is CodeDescriptionPairList list)
				{
					result = list.GetDescriptionFromCode(PlaceOfUnloadingCode);
				}
				return result;
			}
		}

		#endregion

		protected override ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore
			=> new NctsCommonGoodsItemsIntegrator(this);

		protected override void ValidateDestinationTraderForSpecificCountry()
		{
			if (!IsValidationSuspended)
			{
				Validation.CheckArrivalDestinationTraderAPIRegistrationNumber();
				Validation.CheckDestinationTraderContact(DestinationTrader.OrganisationPKInfo, DestinationTrader.Contact);
			}
		}

		protected override void ValidateContactForSpecificCountry()
		{
			var destinationTrader = DestinationTrader;
			Validation.CheckDestinationTraderContact(destinationTrader.E2_ContactInfo, destinationTrader.Contact);

			destinationTrader.Validation.ValidateOrganisationPK();
		}

		protected override void ValidatePrincipalTrader(JobDocAddressValidation validation)
		{
			base.ValidatePrincipalTrader(validation);
			if (IsDepartureMovement)
			{
				var principalOrgHeader = Principal?.Address?.Header;
				if (principalOrgHeader != null)
				{
					if (MovementHeader.IsSimplifiedNctsProcedure && !principalOrgHeader.HasConsignorTransitAuthorization())
					{
						Principal.OrganisationPKInfo.AddMessageError(Res.GetString("0A51BED0-F5BB-4A20-A66F-F3B7D46617F0", "Principal must have an authorization of type 'ACR' to use Simplified Procedure."));
					}
				}
			}
		}

		protected override void ApportionedAmountToGuaranteesLiabilityAmountCore()
		{
			var guarantees = MovementHeader.Guarantees;
			if (guarantees.Cast<EU.NCTS.Business.NctsGuarantee>().Any(x => x.PW_Override == false))
			{
				if (ApportionedAmount == 0)
				{
					var totalCustomsValue = DepartureGoodsItems.Sum(x => x.BY_MonetaryValue);
					var sharedCustomsValue = Utilities.Round(totalCustomsValue * BondAmountApportionmentRatio, 0);
					guarantees.Cast<EU.NCTS.Business.NctsGuarantee>().Where(x => !x.PW_Override).ForEach(x => x.PW_BondAmount = sharedCustomsValue);
				}
				else
				{
					base.ApportionedAmountToGuaranteesLiabilityAmountCore();
				}
			}
		}

		protected override ZString[] GetRolesForDestinationOfficeLookupCore() => Array.Empty<ZString>();

		public override ZBool IsArrivalEventAvailable =>
			BH_ExportFlag == EU.NCTS.Business.EventFlagList.Codes.Yes &&
			(IsInPhase5TransitionPeriod || EnRouteIncidents.Count > 0);

		protected override bool IsArrivalDetailsReadOnlyCore => IsArrivalMovement && ArrivalMovementHeader.BM_CustomsStatus.In<ZString>(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);

		#region IRelatedJob Members

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EU.NctsMovementController;

		#endregion

		const decimal BondAmountApportionmentRatio = 0.25m;

		protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

		protected override EU.NCTS.Business.NctsGuaranteeRefresher GetNewGuaranteeRefresher()
		{
			return new NctsGuaranteeRefresher(this);
		}

		class NctsGuaranteeRefresher : EU.NCTS.Business.NctsGuaranteeRefresher
		{
			readonly NctsHeader nctsHeader;

			public NctsGuaranteeRefresher(NctsHeader nctsHeader) : base(nctsHeader)
			{
				this.nctsHeader = nctsHeader;
			}

			protected override void CreateNctsGuaranteeFromPrincipalGuarantee(EU.Business.CusGuaranteeHeader guarantee)
			{
				var guarantees = nctsHeader.MovementHeader.Guarantees;
				var nctsGuarantee = guarantees.Cast<EU.NCTS.Business.NctsGuarantee>().FirstOrDefault(x => x.PW_BondNumber == guarantee.CPH_Number)
					?? guarantees.AddNew();
				nctsGuarantee.PW_BondType = guarantee.CPH_SubType;
				nctsGuarantee.PW_BondNumber = guarantee.CPH_Number;
			}
		}

		protected override void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
		{
			base.CustomsOfficesForDeparture_ListChanged(sender, e);

			if (MovementHeader is NctsDepartureMovementHeader departureMovement && departureMovement.IsSimplifiedNctsProcedure)
			{
				UpdateGoodsLocationAdditionalIdentifier(departureMovement.GoodsLocation, departureMovement.GoodsLocationDescriptionInfo);
			}
		}

		public override ZBool FallBackIsActive => DENctsCustomsDataRegistry.NctsFallbackIsActive;

		public void UpdateGoodsLocationAdditionalIdentifier(CusGoodsLocation goodsLocation, ZPropertyInfo goodsLocationDescriptionInfo)
		{
			if (goodsLocation != null)
			{
				var list = goodsLocation.Lookups.AdditionalIdentifierList;
				if (list.Count == 1)
				{
					goodsLocation.CGL_AdditionalIdentifier = list[0].Code;
				}
				goodsLocationDescriptionInfo.RefreshBinding();
			}
		}

		protected override Type BillTypeCore => typeof(NctsBill);
	}
}
