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
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsBill : CusInBondBill
		, IUnloadedStatusSupporter
		, Integration.Customs.EU.NCTS.ICusInBondBill
		, IShortSequenceNumberLine
		, IDocAddresses
		, ICanBeImportOrExport
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, ISequenceNumberHeader
		, ICusInBondCargoDescTypeProvider
		, ICusReferenceTypeSupporter
		, INctsCusInBondCargoDescMaster
		, ICanSupportPhase5
		, IDepartureTransportMeansProvider
		, ISupportMultipleResourceStringData
	{
		public NctsBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new NctsHeader Header => (NctsHeader)base.Header;

		public ZString DataGroupingCode => Header?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public INctsBillValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<INctsBillValidationDecider> validationDeciderCached;

		INctsBillValidationDecider GetValidationDecider() => Header?.Configuration.BillConfiguration.GetValidationDecider(Header);

		[ChildEditable(true)]
		public INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = GetSupportingDocuments();
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		INctsSupportingDocumentCollection<NctsSupportingDocument> supportingDocuments;

		protected virtual INctsSupportingDocumentCollection<NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newBill = (NctsBill)base.CloneInternal(args);
			newBill.SequenceNumber = SequenceNumber;
			newBill.TransportTypeAtDeparture = TransportTypeAtDeparture;
			newBill.FirstDepartureTransportMeansID = FirstDepartureTransportMeansID;
			newBill.FirstDepartureTransportMeansNationality = FirstDepartureTransportMeansNationality;
			newBill.SecondDepartureTransportMeansID = SecondDepartureTransportMeansID;
			newBill.SecondDepartureTransportMeansNationality = SecondDepartureTransportMeansNationality;
			newBill.ThirdDepartureTransportMeansID = ThirdDepartureTransportMeansID;
			newBill.ThirdDepartureTransportMeansNationality = ThirdDepartureTransportMeansNationality;
			return newBill;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("36871F3F-46EE-46C2-92DB-79F377334EE5", "House Consignment");

		[ChildEditable(true)]
		public ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = GetPreviousDocuments();
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}
				return previousDocuments;
			}
		}
		ICommonPreviousDocumentCollection<CommonPreviousDocument> previousDocuments;

		protected virtual ICommonPreviousDocumentCollection<CommonPreviousDocument> GetPreviousDocuments() => new CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

		public ZInt NCTSPreviousDocumentsCount => Factory.GetCached(ref nctsPreviousDocumentsCount, () => PreviousDocuments.Count(x => x.IsNCTSPreviousDocument));
		CachedProperty<ZInt> nctsPreviousDocumentsCount;

		public ZInt MaxCountNCTSPreviousDocuments => Factory.GetCached(ref maxNCTSPreviousDocumentsCount, () => NCTSPreviousDocumentsCount + GoodsItems.MaxOrDefault(x => x.NCTSPreviousDocumentsCount));
		CachedProperty<ZInt> maxNCTSPreviousDocumentsCount;

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = GetCusSupplyChainActorReferencesCore();
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}
				return cusSupplyChainActorReferences;
			}
		}
		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> GetCusSupplyChainActorReferencesCore() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		[ChildEditable(true)]
		public INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> AdditionalDocuments
		{
			get
			{
				if (additionalDocuments == null)
				{
					additionalDocuments = GetAdditionalDocuments();
					additionalDocuments.Load();
					RegisterEditableChildObject(additionalDocuments);
				}
				return additionalDocuments;
			}
		}
		INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> additionalDocuments;

		protected virtual INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> GetAdditionalDocuments() => new NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(this);

		public bool Has30600AdditionalInformation => Factory.GetValue(ref has30600AdditionalInformation, AdditionalDocuments.Has30600AdditionalInformation);
		CachedProperty<bool> has30600AdditionalInformation;

		[ReadOnlyMember(nameof(IsPhase5Arrival))]
		[ResourceStringData("480a2bb0-cac5-4888-9ead-5700796ab40d", Caption = "Security", FullDescription = "Security Indicator from Export Declaration")]
		public override ZBool B0_SecurityIndicatorFromExport
		{
			get => base.B0_SecurityIndicatorFromExport;
			set => base.B0_SecurityIndicatorFromExport = value;
		}

		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.CountryList))]
		[ResourceStringData("75BF5D46-F119-4050-B76A-4EA862EBAAE4", Caption = "Country/Region of Dispatch", MediumCaption = "Dispatch Ctry./Rgn.", ShortCaption = "Disp. Ctry./Rgn.")]
		public override ZString B0_RN_NKCountryOfExport
		{
			get => base.B0_RN_NKCountryOfExport;
			set
			{
				var oldValue = B0_RN_NKCountryOfExport;
				base.B0_RN_NKCountryOfExport = value;
				if (!IsCopying && oldValue != B0_RN_NKCountryOfExport)
				{
					GoodsItems.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("B7776E0B-3AC5-407D-825E-7CF164294962", Caption = "Total Gross Weight", MediumCaption = "Gross Weight", ShortCaption = "Gross Wgt.")]
		[ReadOnlyMember(nameof(IsWeightReadOnly))]
		public override ZDecimal B0_Weight
		{
			get => base.B0_Weight;
			set => base.B0_Weight = value;
		}

		bool IsWeightReadOnly => IsPhase5Arrival
			&& ((MovementDetail is CusInBondMoveDetail moveDetail
					&& moveDetail.B9_B9_InBondMoveDetail.IsEmpty
					&& NctsHelper.IsUnloadedStateAccepted(moveDetail.B9_UnloadedState))
				|| IsUnloadingRemarksReadOnly);

		public bool HasDifference => MovementDetail?.DifferenceMoveDetail != null && (MovementDetail?.B9_UnloadedState ?? string.Empty) == NctsUnloadedStateList.Codes.DIF;

		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.WeightUnitList))]
		[ReadOnlyMember(nameof(IsWeightUQReadOnly))]
		[ResourceStringData("66f48d7d-4cb1-422a-95ff-ea4c7c7c8c23", Caption = "Units")]
		public override ZString B0_WeightUQ
		{
			get => base.B0_WeightUQ;
			set => base.B0_WeightUQ = value;
		}

		public ZWeight GrossWeight => new(B0_Weight, B0_WeightUQ);

		public ZDecimal GrossWeightInKilograms => GrossWeight.InKilogramsSafe;

		bool IsWeightUQReadOnly => !IsPhase5 || IsPhase5Departure && IsWeightUQReadOnlyCore || IsWeightReadOnly;

		protected virtual bool IsWeightUQReadOnlyCore => true;

		protected int MaxLengthOfReferenceID => Header.IsPhase5Departure ? 35 : AutoCusInBondBill.Schema.B0_ReferenceIDMaxLength;

		[ResourceStringData("7814C7EA-7C16-4AB6-BAA2-3F363AD86A8E", Caption = "Reference Number UCR", MediumCaption = "Ref. No. UCR", ShortCaption = "UCR", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("2A5AB447-972B-47D2-AD66-4E017DECC331", Caption = "House Consignment ID", MediumCaption = "House Consignment", ShortCaption = "House Ref.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ResourceStringData("D5BBADCB-502B-4BC5-A28C-935839937465", Caption = "Reference Number / UCR", FullDescription = "Indicate the Reference Number / Unique Consignment Reference (UCR)", ShortCaption = "Ref. No. / UCR", MultipleKey = NctsHeader.Phase5DepartureCaptionKey)]
		[MaxLength(nameof(MaxLengthOfReferenceID))]
		[ReadOnlyMember(nameof(IsPhase5Arrival))]
		public override ZString B0_ReferenceID
		{
			get => base.B0_ReferenceID;
			set
			{
				var oldValue = B0_ReferenceID;
				base.B0_ReferenceID = value;

				if (!IsCopying && oldValue != B0_ReferenceID)
				{
					if (!IsValidationSuspended)
					{
						ValidateGoodsItemsMandatoryCommercialReferenceNumber();
					}
				}
			}
		}

		internal void ValidateGoodsItemsMandatoryCommercialReferenceNumber()
		{
			foreach (var item in GoodsItems)
			{
				if (item.Validation is NctsDepartureCargoDescPhase5Validation validation)
				{
					using (validation.SuspendBY_CommercialReferenceNumberUniqueValidation())
					{
						item.MarkAsNeedingValidation();
						validation.ValidateBY_CommercialReferenceNumber();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(B0_TransportPaymentMethodReadOnly))]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportPaymentMethodList))]
		[ResourceStringData("E82558A6-2F81-4BA4-A72B-53C8CB019691", Caption = "Transport MoP", MediumCaption = "Transp. MoP", ShortCaption = "MoP")]
		public override ZString B0_TransportPaymentMethod
		{
			get => base.B0_TransportPaymentMethod;
			set
			{
				var oldValue = B0_TransportPaymentMethod;
				base.B0_TransportPaymentMethod = value;

				if (!IsCopying && oldValue != B0_TransportPaymentMethod)
				{
					if (IsRuleC0337Applied)
					{
						GoodsItems.ForEach(goodsItem => goodsItem.BY_TransportChargesMethodOfPayment = ZString.Empty);
					}
					GoodsItems.RefreshBinding();
				}
			}
		}

		protected virtual bool B0_TransportPaymentMethodReadOnly
		{
			get
			{
				var header = Header;
				return header.IsDepartureMovement && header.MovementHeader.IsRuleC0186Applied;
			}
		}

		public override ZGuid B0_BH
		{
			get => base.B0_BH;
			set
			{
				var oldValue = B0_BH;
				base.B0_BH = value;
				if (!IsCopying && oldValue != B0_BH)
				{
					GoodsItems.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.CountryList))]
		[ResourceStringData("249B8321-72B3-4B1A-9E59-0DFB0268DBA8", Caption = "Country/Region of Destination", MediumCaption = "Destination Ctry./Rgn.", ShortCaption = "Destin. Ctry./Rgn.")]
		public override ZString B0_RN_NKCountryOfDestination
		{
			get => base.B0_RN_NKCountryOfDestination;
			set
			{
				var oldValue = B0_RN_NKCountryOfDestination;
				base.B0_RN_NKCountryOfDestination = value;
				if (!IsCopying && oldValue != B0_RN_NKCountryOfDestination)
				{
					GoodsItems.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(GrossWeightUnloadedReadOnly))]
		public override ZDecimal B0_GrossWeightUnloaded { get => base.B0_GrossWeightUnloaded; set => base.B0_GrossWeightUnloaded = value; }

		public ZWeight GrossWeightUnloaded => new(B0_GrossWeightUnloaded, B0_WeightUQ);

		public ZDecimal GrossWeightUnloadedInKilograms => GrossWeightUnloaded.InKilogramsSafe;

		public bool GrossWeightUnloadedReadOnly => IsUnloadingRemarksReadOnly || !HasDifference;

		[ResourceStringData("05049DE7-47D9-4F04-841E-FC328AA3D9FD", Caption = "Line Price Currency", MediumCaption = "Line Price Curr.", ShortCaption = "Curr.")]
		public override ZString B0_RX_NKLinePriceCurrency
		{
			get => base.B0_RX_NKLinePriceCurrency;
			set => base.B0_RX_NKLinePriceCurrency = value;
		}

		[ResourceStringData("73A7A109-BE65-40A3-B655-878ED3489895", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq. No.")]
		[ReadOnly(true)]
		public ZShort SequenceNumber
		{
			get { return sequenceNumber; }
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(SequenceNumberInfo, ref sequenceNumber, ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value));
					if (IsPhase5)
					{
						MovementDetail.B9_SeqNo = value.ToString();
					}
				}
			}
		}
		ZShort sequenceNumber;

		public ZPropertyInfo SequenceNumberInfo => GetZPropertyInfo(nameof(SequenceNumber));

		[ChildEditable]
		public INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems
		{
			get
			{
				if (goodsItems == null)
				{
					goodsItems = GetNewGoodsItems();
					goodsItems.CopyLastGoodsItemToNewLines = AlwaysCopyFromPreviousLine;
					RegisterEditableChildObject(goodsItems);
				}
				return goodsItems;
			}
		}
		INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> goodsItems;

		protected virtual INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GetNewGoodsItems() => new NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);

		[ChildEditable]
		public INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> ArrivalGoodsItems
		{
			get
			{
				if (arrivalGoodsItems == null)
				{
					arrivalGoodsItems = GetNewArrivalGoodsItems();
					RegisterEditableChildObject(arrivalGoodsItems);
				}
				return arrivalGoodsItems;
			}
		}
		INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> arrivalGoodsItems;

		protected virtual INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> GetNewArrivalGoodsItems() => new NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(this);

		[ChildEditable]
		public IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos
		{
			get
			{
				if (arrivalTransportInfos == null)
				{
					arrivalTransportInfos = GetNewArrivalTransportInfos();
					arrivalTransportInfos.Load();
					RegisterEditableChildObject(arrivalTransportInfos);
				}

				return arrivalTransportInfos;
			}
		}
		IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportInfos;

		protected virtual IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

		public void SetArrivalTransportInfosReadOnly() => ArrivalTransportInfos.SetReadOnlyIncludingChildren(ShouldSetArrivalTransportInfosReadOnlyCore);

		protected virtual bool ShouldSetArrivalTransportInfosReadOnlyCore => IsInPhase5TransitionPeriod;

		[ChildEditable]
		public IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> DepartureTransportInfos
		{
			get
			{
				if (departureTransportInfos == null)
				{
					departureTransportInfos = GetNewDepartureTransportInfos();
					departureTransportInfos.Load();
					RegisterEditableChildObject(departureTransportInfos);
				}

				return departureTransportInfos;
			}
		}
		IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> departureTransportInfos;

		protected virtual IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> GetNewDepartureTransportInfos() => new DepartureCusTransportMeansCollection<DepartureCusTransportMeans>(this);

		public DepartureCusTransportMeansFilteredCollection TransportDepartureAdditionalWagonNumbers
		{
			get
			{
				if (transportDepartureAdditionalWagonNumbers == null)
				{
					transportDepartureAdditionalWagonNumbers = GetNewTransportDepartureAdditionalWagonNumbers();
				}
				return transportDepartureAdditionalWagonNumbers;
			}
		}
		DepartureCusTransportMeansFilteredCollection transportDepartureAdditionalWagonNumbers;

		protected virtual DepartureCusTransportMeansFilteredCollection GetNewTransportDepartureAdditionalWagonNumbers() => new DepartureCusTransportMeansFilteredCollection(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			B0_WeightUQ = Core.Constants.Weight.Kilograms;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (B0_WeightUQ.IsEmpty)
			{
				B0_WeightUQ = Core.Constants.Weight.Kilograms;
			}
		}

		public override void Delete()
		{
			NctsHeader header = null;
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				header = Header;
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				this.DeleteChildren<NctsCommonCargoDesc>(CusInBondCargoDescSchema.BY_ParentID, additionalQuery: new ZQuery(CusInBondCargoDescSchema.BY_ParentTableCode, TablePrefix));
			}

			if (IsInDatabase && GoodsItems.Any())
			{
				Header.ResetGoodsItemNumbersWhenPhase5();
			}

			base.Delete();

			header?.Validation.ValidateMaxGoodsItemsForAllBills();
		}

		public new NctsBillLookups Lookups => (NctsBillLookups)base.Lookups;

		protected override CusInBondBillLookups GetNewLookups() => new NctsBillLookups(this);

		public new NctsBillValidation Validation => (NctsBillValidation)base.Validation;

		protected override CusInBondBillValidation GetNewValidation() => new NctsBillValidation(this);

		protected override CusInBondMoveHeader GetMoveHeader(CusInBondHeader header)
		{
			return header switch
			{
				NctsHeader nctsHeader when nctsHeader.IsArrivalMovement => nctsHeader.ArrivalMovementHeader,
				NctsHeader nctsHeader when nctsHeader.IsDepartureMovement => nctsHeader.MovementHeader,
				_ => base.GetMoveHeader(header)
			};
		}

		protected override Type MovementDetailType => typeof(CusInBondMoveDetail);

		protected override bool IsMovementDetailSupported => IsPhase5Arrival || IsPhase5Departure;

		public new CusInBondMoveDetail MovementDetail => (CusInBondMoveDetail)base.MovementDetail;

		public override ZQuery MovementDetailQuery
		{
			get
			{
				var result = base.MovementDetailQuery;
				if (!result.IsNoResultQuery)
				{
					result.AddToFilter(CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, DBNull.Value);
				}
				return result;
			}
		}

		public bool IsPhase5 => Header?.IsPhase5 ?? false;

		public bool IsPhase5Arrival => Header?.IsPhase5Arrival ?? false;

		public bool IsPhase5Departure => Header?.IsPhase5Departure ?? false;

		public bool IsUnloadingRemarksReadOnly => IsPhase5Arrival && Header.ArrivalMovementHeader.IsUnloadingRemarksReadOnly;

		public bool AreUnloadingRemarksFullyAccepted => IsPhase5Arrival && Header.ArrivalMovementHeader.AreUnloadingRemarksFullyAccepted;

		public ZBool B0_CopyLastGoodsLineToNewLines => goodsItems != null ? GoodsItems.CopyLastGoodsItemToNewLines : AlwaysCopyFromPreviousLine;

		bool AlwaysCopyFromPreviousLine => CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public override bool CanDelete
		{
			get
			{
				if (MovementDetail is CusInBondMoveDetail moveDetail)
				{
					return IsPhase5Arrival ? MovementDetail.B9_UnloadedStateInfo.ReadOnly : base.CanDelete;
				}
				return base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("D4C7C205-1A0F-4E1E-AAEB-E4938FF1E112", "Cannot delete House Consignment from Customs.");

		internal ZBool IsRuleC0337Applied
		{
			get
			{
				var header = Header;
				return !B0_TransportPaymentMethod.IsEmpty && header.IsDepartureMovement && header.Configuration.ValidationRuleConfiguration.IsRuleC0337Active;
			}
		}

		public bool AreAllDepartureTransportMeansFieldsEmpty => DepartureTransportMeansFields.All(f => f.IsEmpty);

		ZString[] DepartureTransportMeansFields
		{
			get
			{
				switch (InlandTransportModeAtDeparture)
				{
					case ModeOfTransportList.Codes._1_SeaTransport:
						return [TransportTypeAtDeparture, VesselNameAtDeparture, VesselCountryAtDeparture];
					case ModeOfTransportList.Codes._2_RailTransport:
					case ModeOfTransportList.Codes._4_AirTransport:
					case ModeOfTransportList.Codes._7_FixedTransportInstallations:
					case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
					case ModeOfTransportList.Codes._9_OwnPropulsion:
						return [TransportTypeAtDeparture, TransportAtDeparture, TransportCountryAtDeparture];
					case ModeOfTransportList.Codes._3_RoadTransport:
						return [TransportTypeAtDeparture, TransportAtDeparture, TransportCountryAtDeparture, Trailer1IDAtDeparture, Trailer1NationalityAtDeparture, Trailer2IDAtDeparture, Trailer2NationalityAtDeparture];
					default:
						return [];
				}
			}
		}

		#region FirstDepartureTransportMeans

		[MaxLength(35)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.Vessels))]
		public ZString FirstDepartureTransportMeansID
		{
			get => LoadFirstDepartureTransportMeans()?.TPM_IdentificationNumber ?? ZString.Empty;
			set
			{
				LoadOrCreateFirstDepartureTransportMeans().TPM_IdentificationNumber = value;
				if (FirstDepartureTransportMeansID.IsEmpty && FirstDepartureTransportMeansNationality.IsEmpty)
				{
					firstTransportMeans.Delete();
					firstTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateFirstDepartureTransportMeansID();
				}
				FirstDepartureTransportMeansIDInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo FirstDepartureTransportMeansIDInfo => GetZPropertyInfo(nameof(FirstDepartureTransportMeansID));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("C3E1E30B-11E8-46E7-8B03-D9A822FCB108", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		public ZString FirstDepartureTransportMeansNationality
		{
			get => LoadFirstDepartureTransportMeans()?.TPM_RN_NKTransportNationality ?? ZString.Empty;
			set
			{
				LoadOrCreateFirstDepartureTransportMeans().TPM_RN_NKTransportNationality = value;
				if (FirstDepartureTransportMeansID.IsEmpty && FirstDepartureTransportMeansNationality.IsEmpty)
				{
					firstTransportMeans.Delete();
					firstTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateFirstDepartureTransportMeansNationality();
				}
				FirstDepartureTransportMeansNationalityInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo FirstDepartureTransportMeansNationalityInfo => GetZPropertyInfo(nameof(FirstDepartureTransportMeansNationality));

		DepartureCusTransportMeans LoadFirstDepartureTransportMeans()
		{
			if (firstTransportMeans == null || firstTransportMeans.IsDeleted
				|| firstTransportMeans.TPM_ParentID != PK || firstTransportMeans.TPM_ParentTableCode != TablePrefix
				|| firstTransportMeans.TPM_SequenceNumber != 1)
			{
				firstTransportMeans = DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 1);
			}
			return firstTransportMeans;
		}
		DepartureCusTransportMeans firstTransportMeans;

		DepartureCusTransportMeans LoadOrCreateFirstDepartureTransportMeans()
		{
			LoadFirstDepartureTransportMeans();
			if (firstTransportMeans == null)
			{
				firstTransportMeans = DepartureTransportInfos.AddNew();
				firstTransportMeans.TPM_SequenceNumber = 1;
			}
			return firstTransportMeans;
		}

		#endregion FirstDepartureTransportMeans

		#region SecondDepartureTransportMeans

		[MaxLength(35)]
		public ZString SecondDepartureTransportMeansID
		{
			get => LoadSecondDepartureTransportMeans()?.TPM_IdentificationNumber ?? ZString.Empty;
			set
			{
				LoadOrCreateSecondDepartureTransportMeans().TPM_IdentificationNumber = value;
				if (SecondDepartureTransportMeansID.IsEmpty && SecondDepartureTransportMeansNationality.IsEmpty)
				{
					secondTransportMeans.Delete();
					secondTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondDepartureTransportMeansID();
				}
				SecondDepartureTransportMeansIDInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SecondDepartureTransportMeansIDInfo => GetZPropertyInfo(nameof(SecondDepartureTransportMeansID));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("BFFB0668-657A-4F89-85F8-7B370154B902", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		public ZString SecondDepartureTransportMeansNationality
		{
			get => LoadSecondDepartureTransportMeans()?.TPM_RN_NKTransportNationality ?? ZString.Empty;
			set
			{
				LoadOrCreateSecondDepartureTransportMeans().TPM_RN_NKTransportNationality = value;
				if (SecondDepartureTransportMeansID.IsEmpty && SecondDepartureTransportMeansNationality.IsEmpty)
				{
					secondTransportMeans.Delete();
					secondTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondDepartureTransportMeansNationality();
				}
				SecondDepartureTransportMeansNationalityInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SecondDepartureTransportMeansNationalityInfo => GetZPropertyInfo(nameof(SecondDepartureTransportMeansNationality));

		DepartureCusTransportMeans LoadSecondDepartureTransportMeans()
		{
			if (secondTransportMeans == null || secondTransportMeans.IsDeleted
				|| secondTransportMeans.TPM_ParentID != PK || secondTransportMeans.TPM_ParentTableCode != TablePrefix
				|| secondTransportMeans.TPM_SequenceNumber != 2)
			{
				secondTransportMeans = DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 2);
			}
			return secondTransportMeans;
		}
		DepartureCusTransportMeans secondTransportMeans;

		DepartureCusTransportMeans LoadOrCreateSecondDepartureTransportMeans()
		{
			LoadSecondDepartureTransportMeans();
			if (secondTransportMeans == null)
			{
				secondTransportMeans = DepartureTransportInfos.AddNew();
				secondTransportMeans.TPM_SequenceNumber = 2;
			}
			return secondTransportMeans;
		}

		#endregion SecondDepartureTransportMeans

		#region ThirdDepartureTransportMeans

		[MaxLength(35)]
		public ZString ThirdDepartureTransportMeansID
		{
			get => LoadThirdDepartureTransportMeans()?.TPM_IdentificationNumber ?? ZString.Empty;
			set
			{
				LoadOrCreateThirdDepartureTransportMeans().TPM_IdentificationNumber = value;
				if (ThirdDepartureTransportMeansID.IsEmpty && ThirdDepartureTransportMeansNationality.IsEmpty)
				{
					thirdTransportMeans.Delete();
					thirdTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateThirdDepartureTransportMeansID();
				}
				ThirdDepartureTransportMeansIDInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ThirdDepartureTransportMeansIDInfo => GetZPropertyInfo(nameof(ThirdDepartureTransportMeansID));

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("4A6ADC5E-8AA4-47D3-9A19-435638B6926D", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		public ZString ThirdDepartureTransportMeansNationality
		{
			get => LoadThirdDepartureTransportMeans()?.TPM_RN_NKTransportNationality ?? ZString.Empty;
			set
			{
				LoadOrCreateThirdDepartureTransportMeans().TPM_RN_NKTransportNationality = value;
				if (ThirdDepartureTransportMeansID.IsEmpty && ThirdDepartureTransportMeansNationality.IsEmpty)
				{
					thirdTransportMeans.Delete();
					thirdTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateThirdDepartureTransportMeansNationality();
				}
				ThirdDepartureTransportMeansNationalityInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ThirdDepartureTransportMeansNationalityInfo => GetZPropertyInfo(nameof(ThirdDepartureTransportMeansNationality));

		DepartureCusTransportMeans LoadThirdDepartureTransportMeans()
		{
			if (thirdTransportMeans == null || thirdTransportMeans.IsDeleted
				|| thirdTransportMeans.TPM_ParentID != PK || thirdTransportMeans.TPM_ParentTableCode != TablePrefix
				|| thirdTransportMeans.TPM_SequenceNumber != 3)
			{
				thirdTransportMeans = DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 3);
			}
			return thirdTransportMeans;
		}
		DepartureCusTransportMeans thirdTransportMeans;

		DepartureCusTransportMeans LoadOrCreateThirdDepartureTransportMeans()
		{
			LoadThirdDepartureTransportMeans();
			if (thirdTransportMeans == null)
			{
				thirdTransportMeans = DepartureTransportInfos.AddNew();
				thirdTransportMeans.TPM_SequenceNumber = 3;
			}
			return thirdTransportMeans;
		}

		#endregion ThirdDepartureTransportMeans

		#region Consignor

		public JobDocAddress Consignor
		{
			get
			{
				if (consignorJobDocAddress == null || consignorJobDocAddress.IsDeleted)
				{
					if (consignorJobDocAddress != null)
					{
						consignorJobDocAddress.DocAddressChanged -= new EventHandler(ConsignorJobDocAddressChanged);
					}
					consignorJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorJobDocAddressRequirement);
					consignorJobDocAddress.DocAddressChanged += new EventHandler(ConsignorJobDocAddressChanged);
					consignorJobDocAddress.AdditionalValidation = GetConsignorJobDocAddressAdditionalValidation(consignorJobDocAddress);
				}
				return consignorJobDocAddress;
			}
		}
		JobDocAddress consignorJobDocAddress;

		protected virtual ZValidation GetConsignorJobDocAddressAdditionalValidation(JobDocAddress consignorJobDocAddress) => null;

		public JobDocAddressRequirement ConsignorJobDocAddressRequirement
		{
			get
			{
				if (consignorJobDocAddressRequirement == null)
				{
					consignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.NoContactType)
					{
						CanOverride = ConsignorJobDocAddressCanOverride,
						ValidateOrganisationPK = ValidateConsignor,
					};
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(consignorJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(consignorJobDocAddressRequirement);
					JobDocAddressManager.AddRequirement(consignorJobDocAddressRequirement);
				}
				return consignorJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consignorJobDocAddressRequirement;

		protected virtual bool ConsignorJobDocAddressCanOverride => false;

		protected virtual void ValidateConsignor(JobDocAddressValidation validation)
		{
			var consignor = Consignor;
			var header = Header;
			var targetInfo = consignor.OrganisationPKInfo;
			var houseResString = Res.GetString("D9969613-3B32-48B3-84C5-6878224926DB", "House");
			var consignorDescription = ConsignorDescription;
			CheckRuleR0506ForOrg(consignor.OrganisationPK, targetInfo, consignorDescription, x => x.Consignor.OrganisationPK);
			header.CheckConditionC0505(targetInfo, consignor);
			CheckRuleE1301ForOrg(consignor.OrganisationPK, targetInfo, consignorDescription);
			CheckConsignorNR0068(consignor);

			var ruleC0542_1Validation = new NctsRuleC0542_1Validation(header);
			ruleC0542_1Validation.ValidateConsignor(targetInfo, consignor, houseResString);
			ruleC0542_1Validation.ValidateHouseConsignor(targetInfo, consignor);

			new NctsRuleG0123_1Validation(header).ValidateConsignor(targetInfo, consignor, houseResString);
		}

		void ConsignorJobDocAddressChanged(object sender, EventArgs e)
		{
			OnChangedConsignorJobDocAddressRequirement();
		}

		protected virtual void OnChangedConsignorJobDocAddressRequirement()
		{
		}

		#endregion

		#region Consignee

		public JobDocAddress Consignee
		{
			get
			{
				if (consigneeJobDocAddress == null || consigneeJobDocAddress.IsDeleted)
				{
					if (consigneeJobDocAddress != null)
					{
						consigneeJobDocAddress.DocAddressChanged -= new EventHandler(ConsigneeJobDocAddressChanged);
					}
					consigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeJobDocAddressRequirement);
					consigneeJobDocAddress.DocAddressChanged += new EventHandler(ConsigneeJobDocAddressChanged);
					consigneeJobDocAddress.AdditionalValidation = GetConsigneeJobDocAddressAdditionalValidation(consigneeJobDocAddress);
				}
				return consigneeJobDocAddress;
			}
		}
		JobDocAddress consigneeJobDocAddress;

		protected virtual ZValidation GetConsigneeJobDocAddressAdditionalValidation(JobDocAddress consigneeJobDocAddress) => new NctsBillConsigneeJobDocAddressValidation(consigneeJobDocAddress, Header);

		public JobDocAddressRequirement ConsigneeJobDocAddressRequirement
		{
			get
			{
				if (consigneeJobDocAddressRequirement == null)
				{
					consigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.NoContactType)
					{
						CanOverride = ConsigneeJobDocAddressCanOverride,
						ValidateOrganisationPK = ValidateConsignee,
					};
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(consigneeJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(consigneeJobDocAddressRequirement);
					JobDocAddressManager.AddRequirement(consigneeJobDocAddressRequirement);
				}
				return consigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consigneeJobDocAddressRequirement;

		protected virtual bool ConsigneeJobDocAddressCanOverride => false;

		protected virtual void ValidateConsignee(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var organisationPKInfo = parent.OrganisationPKInfo;
			CheckRuleR0506ForOrg(parent.OrganisationPK, organisationPKInfo, ConsigneeDescription, x => x.Consignee.OrganisationPK);
			CheckConsignee_C0001_1Rule(organisationPKInfo);
			CheckConsignee_C0001_2Rule(organisationPKInfo);
			CheckConsignee_C0001_4Rule(organisationPKInfo);
			CheckConsignee_C0001_6Rule(organisationPKInfo);
			CheckConsignee_C0001_7Rule(organisationPKInfo);
			CheckConsignee_G0001_1Rule(organisationPKInfo);
			Header.CheckConditionC0505(Consignee.OrganisationPKInfo, Consignee);
			CheckRuleE1301ForOrg(parent.OrganisationPK, organisationPKInfo, ConsigneeDescription);
			CheckConsignee_NR0069(Consignee);
		}

		void ConsigneeJobDocAddressChanged(object sender, EventArgs e)
		{
			if (this.IsPhase5Departure)
			{
				this.Header.Consignee.Validation.ValidateOrganisationPK();
			}
			OnChangedConsigneeJobDocAddressRequirement();
		}

		protected virtual void OnChangedConsigneeJobDocAddressRequirement()
		{
		}

		#endregion

		public ZBool IsContainerised
		{
			get
			{
				foreach (var item in GoodsItems)
				{
					foreach (var package in item.Packages)
					{
						foreach (var container in package.ContainersPivot.Containers)
						{
							if (!container.BC_ContainerNum.IsEmpty)
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		JobDocAddressManager JobDocAddressManager => jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager());
		JobDocAddressManager jobDocAddressManager;

		void CheckRuleE1301ForOrg(ZGuid orgPk, ZPropertyInfo orgPkInfo, string orgDescriptor)
		{
			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleE1301Active
				&& IsInPhase5TransitionPeriod && !orgPk.IsEmpty)
			{
				orgPkInfo.AddMessageError(NctsBillValidationHelper.GetMessageErrorForRuleE1301(orgDescriptor));
			}
		}

		void CheckRuleR0506ForOrg(ZGuid orgPk, ZPropertyInfo orgPkInfo, string orgDescriptor, Func<NctsBill, ZGuid> orgProvider)
		{
			if (ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleR0506Active
				&& !orgPk.IsEmpty && IsOrgSameForAllBills())
			{
				orgPkInfo.AddMessageError(NctsBillValidationHelper.GetMessageErrorR0506MustBeDifferent(orgDescriptor));
			}

			bool IsOrgSameForAllBills() => Header.Bills.Count > 1 && Header.Bills.AllSame(orgProvider);
		}

		void CheckConsignee_C0001_1Rule(ZPropertyInfo info)
		{
			var nctsHeader = Header;
			var movementHeader = nctsHeader.MovementHeader;

			var validationDecider = nctsHeader.ValidationDecider as INctsHeaderDeparturePhase5ValidationDecider;
			if (validationDecider is null)
			{
				return;
			}

			var isRuleB1823Applicable = validationDecider.IsRuleB1823Active
				&& IsInPhase5TransitionPeriod;

			var isRuleC0001_1Applicable = validationDecider.IsRuleC0001_1Active
				&& !IsInPhase5TransitionPeriod
				&& !isRuleB1823Applicable;

			if (!isRuleC0001_1Applicable || !nctsHeader.Consignee.IsEmpty)
			{
				return;
			}

			var codeListC0009 = nctsHeader.GetC0009CountryCodes();
			var destinationIsC0009Code = codeListC0009.Contains(movementHeader.BM_RL_NKDestinationPort) || codeListC0009.Contains(B0_RN_NKCountryOfDestination);
			if (destinationIsC0009Code || (movementHeader.IsCombinedWithExit && !movementHeader.Has30600AdditionalInformation))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info, messagePrefix: $"[{ValidationRuleCodeConstants.C0001_1}] ");
			}
		}

		void CheckConsignee_C0001_2Rule(ZPropertyInfo info)
		{
			var validationRuleConfiguration = Header.Configuration?.ValidationRuleConfiguration;

			if ((validationRuleConfiguration?.IsRuleC0001_2Active ?? false) && Header.IsPhase5Departure)
			{
				if (!Header.Consignee.IsEmpty && !Consignee.IsEmpty)
				{
					info.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.C0001_2aMessage);
				}

				if (Header.Consignee.IsEmpty && Consignee.IsEmpty && (!Header.MovementHeader.IsSecurityTypeENTOrNON || RefCountry.LoadFromCountryCode(Factory, Header.MovementHeader.BM_RL_NKDestinationPort).IsACountryEligibleToACommonTransitProcedure()))
				{
					info.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.C0001_2bMessage);
				}
			}
		}

		void CheckConsignee_C0001_4Rule(ZPropertyInfo organisationPKInfo)
		{
			var nctsHeader = Header;
			var movementHeader = nctsHeader.MovementHeader;

			var isRuleC0001_4Applicable = ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleC0001_4Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleC0001_4Applicable || Consignee.IsEmpty)
			{
				return;
			}

			var destinationIsC0009Code = nctsHeader.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort);

			if ((!nctsHeader.Consignee.IsEmpty
				&& (destinationIsC0009Code || movementHeader.IsSecurityTypeENTOrNON))
				|| Has30600AdditionalInformation
				|| Header.Has30600AdditionalInformation
				|| !nctsHeader.Consignee.IsEmpty)
			{
				organisationPKInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.C0001_4Message);
			}
		}

		void CheckConsignee_C0001_6Rule(ZPropertyInfo organisationPKInfo)
		{
			var nctsHeader = Header;
			var movementHeader = nctsHeader.MovementHeader;

			var isRuleC0001_6Applicable = ValidationDecider is INctsBillDeparturePhase5ValidationDecider validation && validation.IsRuleC0001_6Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleC0001_6Applicable)
			{
				return;
			}

			var isDestinationPortExcludedFromC0009 = !nctsHeader.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort);

			if (isDestinationPortExcludedFromC0009)
			{
				if (movementHeader.IsSecurityTypeNONOrENT)
				{
					return;
				}

				if (Has30600AdditionalInformationAtHeaderOrBills())
				{
					if (!Consignee.IsEmpty)
					{
						organisationPKInfo.AddWarning(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.C0001_6Message);
					}
					return;
				}
			}

			new HeaderOrLineValueValidator<ZGuid>(
			headerValueProvider: () => nctsHeader.Consignee.OrganisationPK,
			lineValuesProvider: () => nctsHeader.Bills.Select(x => x.Consignee.OrganisationPK),
			ruleCode: nctsHeader.Configuration.ValidationRuleConfiguration.Messages.C0001_6RuleCode)
			{
				IsEmptyFunc = x => x.IsEmpty,
				EmptyHeaderAndLinesMessageProvider = () => ValidationCaptions.EmptyHeaderAndHouseValue,
			}.ValidateLine(organisationPKInfo, Consignee.OrganisationPK);

			return;

			bool Has30600AdditionalInformationAtHeaderOrBills() => Has30600AdditionalInformation || Header.Has30600AdditionalInformation;
		}

		void CheckConsignee_C0001_7Rule(ZPropertyInfo organisationPKInfo)
		{
			var nctsHeader = Header;
			var movementHeader = nctsHeader.MovementHeader;

			var isRuleC0001_7Applicable = ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleC0001_7Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleC0001_7Applicable)
			{
				return;
			}

			if (!nctsHeader.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort) &&
				!movementHeader.IsSecurityTypeENTOrNON &&
				!nctsHeader.Has30600AdditionalInformation &&
				!Has30600AdditionalInformation &&
				nctsHeader.Bills.Any(x => x.Has30600AdditionalInformation) &&
				Consignee.IsEmpty &&
				nctsHeader.Consignee.IsEmpty)
			{
				organisationPKInfo.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.C0001_7Message);
			}
		}

		void CheckConsignorNR0068(JobDocAddress consignor)
		{
			if (!consignor.IsEmpty && ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0068Active && consignor.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY022AtHouseLevel = AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.ConsignorAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY022AtHouseLevel)
				{
					consignor.OrganisationPKInfo.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.NR0068Message);
				}
			}
		}

		void CheckConsignee_NR0069(JobDocAddress consignee)
		{
			if (!consignee.IsEmpty && ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider && validationDecider.IsRuleNR0069Active && consignee.Organisation.GetEORI().IsEmpty)
			{
				var hasAdditionalReferenceWithCodeY023AtHouseLevel = AdditionalDocuments.Any(a => a.IsAnAdditionalReference && a.CSI_Code == NctsConstants.AdditionalInfoCodes.ConsigneeAEOCertificateNumber);

				if (hasAdditionalReferenceWithCodeY023AtHouseLevel)
				{
					consignee.OrganisationPKInfo.AddMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.NR0069Message);
				}
			}
		}

		void CheckConsignee_G0001_1Rule(ZPropertyInfo info)
		{
			var nctsHeader = Header;
			var isRuleG0001_1Applicable = ValidationDecider is INctsBillDeparturePhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleG0001_1Active
				&& !IsInPhase5TransitionPeriod;

			if (!isRuleG0001_1Applicable || Consignee.IsEmpty)
			{
				return;
			}

			if (Has30600AdditionalInformation)
			{
				info.AddMessageError(nctsHeader.Configuration.ValidationRuleConfiguration.Messages.G0001_1Message);
			}
		}

		string ConsignorDescription => Res.GetString("2B0FD65B-B269-4AE2-8CFE-19905E725E79", "Consignor");
		string ConsigneeDescription => Res.GetString("7C311C14-3BFD-4EAA-A4C9-7F4F7CEF7205", "Consignee");

		#region  IShortSequenceNumberLine Implementation

		ZGuid ISequenceNumberLine.FKToHeader => B0_BH;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => !IsDeleted && MovementDetail is CusInBondMoveDetail moveDetail ? ZShort.ParseSafe(moveDetail.B9_SeqNo, ZShort.Zero) : SequenceNumber;
			set
			{
				if (MovementDetail is CusInBondMoveDetail moveDetail)
				{
					SequenceNumber = value;
					moveDetail.B9_SeqNo = value.ToString();
				}
			}
		}

		#endregion

		#region IDocAddresses Implementation

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
				{
					jobDocAddressDependentCollection = new JobDocAddressDependentCollection(this);
					jobDocAddressDependentCollection.Load();
					RegisterEditableChildObject(jobDocAddressDependentCollection);
				}
				return jobDocAddressDependentCollection;
			}
		}
		JobDocAddressDependentCollection jobDocAddressDependentCollection;

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => true;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Environment.Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorJobDocAddressRequirement;
				case DocAddressType.ConsigneeAddress:
					return ConsigneeJobDocAddressRequirement;
				default:
					return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return new OrgHeaderCollection(Factory);
		}

		ZString IDocAddresses.HumanReadableName => ZString.Empty;

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => GetJobDocAddressValidation(addressToValidate);

		protected virtual ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[]
		{
			DocAddressType.ConsignorDocumentaryAddress,
			DocAddressType.ConsigneeAddress
		};

		#endregion

		#region Warehouse Integration

		public ZBool IsOutwardOrderImported
		{
			get
			{
				return Factory.GetValue(ref isOutwardOrderImported, () => this.GetSystemDefinedValue<ZBool>(nameof(IsOutwardOrderImported)));
			}
			set
			{
				if (value != IsOutwardOrderImported)
				{
					this.SetSystemDefinedValue(nameof(IsOutwardOrderImported), value);
				}
			}
		}

		CachedProperty<ZBool> isOutwardOrderImported;

		#endregion

		#region  ICanBeImportOrExport Implementation

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		ZBool ICanBeImportOrExport.IsExport => false;

		ZBool ICanBeImportOrExport.IsImport => false;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => DataGroupingCode;

		string ICanBeImportOrExport.DataGroupingCode => DataGroupingCode;

		#endregion

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(NctsSupportingDocument) },
			{ CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(CommonPreviousDocument) },
			{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(NctsBillAdditionalDocument) }
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ISequenceNumberHeader Implementation

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => IsPhase5Arrival ? new TypedEnumerable<ISequenceNumberLine>(ArrivalGoodsItems) : new TypedEnumerable<ISequenceNumberLine>(GoodsItems);

		#endregion

		#region ICusInBondCargoDescTypeProvider Implementation

		Type ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => Header is NctsHeader header ?
			header.IsPhase5 && header.IsArrivalMovement ? NctsTypeDecider.GetNctsArrivalCargoDescType(header.Factory, header.CountryCode) : NctsTypeDecider.GetNctsDepartureCargoDescType(header.Factory, header.CountryCode)
			: null;

		#endregion

		#region INctsCusInBondCargoDescMaster Implementation

		public ShortSequenceNumberGenerator LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new ShortSequenceNumberGenerator(this));

		ShortSequenceNumberGenerator lineNumberGenerator;

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		#endregion

		#region IDepartureTransportMeansProvider

		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.ModeOfTransportList))]
		[ResourceStringData("3F449829-DD76-492D-9CBE-3D74921A6FEF", Caption = "Inland M. O. T.")]
		public ZString InlandTransportModeAtDeparture => Header?.MovementHeader?.BM_InlandTransportMode ?? ZString.Empty;

		public ZPropertyInfo InlandTransportModeAtDepartureInfo => GetZPropertyInfo(nameof(InlandTransportModeAtDeparture));

		[MaxLength(35)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.Vessels))]
		[ResourceStringData("EB5B6304-E9CC-47C2-AEA4-C6BDFEC19D35", ShortCaption = "Vessel", MediumCaption = "Vessel", Caption = "Vessel Name")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString VesselNameAtDeparture
		{
			get => FirstDepartureTransportMeansID;
			set => FirstDepartureTransportMeansID = value;
		}
		public ZPropertyInfo VesselNameAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselNameAtDeparture), x => FirstDepartureTransportMeansIDInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("938F3A9E-711E-4B81-A9F8-51124451FC71", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString VesselCountryAtDeparture
		{
			get => FirstDepartureTransportMeansNationality;
			set => FirstDepartureTransportMeansNationality = value;
		}
		public ZPropertyInfo VesselCountryAtDepartureInfo => GetWrappedZPropertyInfo(nameof(VesselCountryAtDeparture), x => FirstDepartureTransportMeansNationalityInfo);

		public IBusinessObjectCollection<IAdditionalWagonProvider> AdditionalWagons => TransportDepartureAdditionalWagonNumbers;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportAtDepartureTypeOfIdList))]
		[ResourceStringData("D426DAAE-C802-4D47-89B8-9E5A68AD49BC", Caption = "Type of Identification", ShortCaption = "Type of ID")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString TransportTypeAtDeparture
		{
			get => LoadFirstDepartureTransportMeans()?.TPM_TypeOfIdentification ?? ZString.Empty;
			set
			{
				LoadOrCreateFirstDepartureTransportMeans().TPM_TypeOfIdentification = value;
				if (TransportTypeAtDeparture.IsEmpty && FirstDepartureTransportMeansID.IsEmpty && FirstDepartureTransportMeansNationality.IsEmpty)
				{
					firstTransportMeans.Delete();
					firstTransportMeans = null;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransportTypeAtDeparture();
				}
				TransportTypeAtDepartureInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo TransportTypeAtDepartureInfo => GetZPropertyInfo(nameof(TransportTypeAtDeparture));

		[ResourceStringData("B3EC2F6C-E3C1-4D7E-B88F-65568AE24FEF", Caption = "Transport Identification", ShortCaption = "Transport ID")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString TransportAtDeparture
		{
			get => FirstDepartureTransportMeansID;
			set => FirstDepartureTransportMeansID = value;
		}
		public ZPropertyInfo TransportAtDepartureInfo => GetWrappedZPropertyInfo(nameof(TransportAtDeparture), x => FirstDepartureTransportMeansIDInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("33DC30D9-A470-416E-8F8F-3FFBDA8FF297", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString TransportCountryAtDeparture
		{
			get => FirstDepartureTransportMeansNationality;
			set => FirstDepartureTransportMeansNationality = value;
		}
		public ZPropertyInfo TransportCountryAtDepartureInfo => GetWrappedZPropertyInfo(nameof(TransportCountryAtDeparture), x => FirstDepartureTransportMeansNationalityInfo);

		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		[ResourceStringData("6C65DD4A-B511-40E6-83CD-2C4A0F655DBC", Caption = "Trailer 1 ID", MediumCaption = "Trailer 1", ShortCaption = "TRLR 1")]
		public ZString Trailer1IDAtDeparture
		{
			get => SecondDepartureTransportMeansID;
			set => SecondDepartureTransportMeansID = value;
		}
		public ZPropertyInfo Trailer1IDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer1IDAtDeparture), x => SecondDepartureTransportMeansIDInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("26F698F4-C81A-49EA-BCFB-E7C081BC2207", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString Trailer1NationalityAtDeparture
		{
			get => SecondDepartureTransportMeansNationality;
			set => SecondDepartureTransportMeansNationality = value;
		}
		public ZPropertyInfo Trailer1NationalityAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer1NationalityAtDeparture), x => SecondDepartureTransportMeansNationalityInfo);

		[ResourceStringData("FFAF4CB8-2107-4E72-9055-8AE981211258", Caption = "Trailer 2 ID", MediumCaption = "Trailer 2", ShortCaption = "TRLR 2")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString Trailer2IDAtDeparture
		{
			get => ThirdDepartureTransportMeansID;
			set => ThirdDepartureTransportMeansID = value;
		}
		public ZPropertyInfo Trailer2IDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer2IDAtDeparture), x => ThirdDepartureTransportMeansIDInfo);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(NctsBillLookups.TransportNationalityList))]
		[ResourceStringData("39121A88-A069-42EA-9A6B-B4C9981CCA4D", Caption = "Nationality", MediumCaption = "Nationality", ShortCaption = "Nat.")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString Trailer2NationalityAtDeparture
		{
			get => ThirdDepartureTransportMeansNationality;
			set => ThirdDepartureTransportMeansNationality = value;
		}
		public ZPropertyInfo Trailer2NationalityAtDepartureInfo => GetWrappedZPropertyInfo(nameof(Trailer2NationalityAtDeparture), x => ThirdDepartureTransportMeansNationalityInfo);

		//not used in EU
		[ResourceStringData("0FC0010E-ECD5-4758-A565-71600E0DE81D", Caption = "Flight Number", ShortCaption = "Flight ID")]
		[ReadOnlyMember(nameof(IsTransportDepartureReadOnly))]
		public ZString AircraftIDAtDeparture
		{
			get => SecondDepartureTransportMeansID;
			set => SecondDepartureTransportMeansID = value;
		}
		public ZPropertyInfo AircraftIDAtDepartureInfo => GetWrappedZPropertyInfo(nameof(AircraftIDAtDeparture), x => SecondDepartureTransportMeansIDInfo);

		ZBool IDepartureTransportMeansProvider.IsInPhase5TransitionPeriod => IsInPhase5TransitionPeriod;

		#endregion IDepartureTransportMeansProvider

		#region ReadOnly

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var propertyName = property.Name;
			return (propertyName == nameof(B0_Weight) && IsWeightReadOnly)
				|| (propertyName == nameof(B0_WeightUQ) && IsWeightUQReadOnly)
				|| base.GetShouldPropertiesBeReadOnly(property);
		}

		public bool IsTransportDepartureReadOnly => Factory.GetCached(ref isTransportDepartureReadOnlyCached, () => IsTransportDepartureReadOnlyCore);
		CachedProperty<bool> isTransportDepartureReadOnlyCached;

		protected virtual bool IsTransportDepartureReadOnlyCore => IsPhase5Departure && IsInPhase5TransitionPeriod;

		#endregion

		#region ISupportMultipleResourceStringData

		public IReadOnlyList<string> MultipleKeysToUse => Header?.MultipleKeysToUse ?? Array.Empty<string>();

		public ZString UnloadedStatus
		{
			get => MovementDetail?.B9_UnloadedState ?? ZString.Empty;
			set => MovementDetail.B9_UnloadedState = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => SupportingDocuments.Cast<IUnloadedStatusSupporter>()
					.Union(AdditionalDocuments)
					.Union(PreviousDocuments)
					.Union(ArrivalGoodsItems);
		#endregion

		public INctsCustomsEntryIntegrator GetCustomsEntryIntegrator() => GetCustomsEntryIntegratorCore();

		protected virtual INctsCustomsEntryIntegrator GetCustomsEntryIntegratorCore() => new NctsBillCustomsEntryIntegrator(this);

		public ZBool IsInPhase5TransitionPeriod => Header?.IsInPhase5TransitionPeriod ?? false;
	}
}
