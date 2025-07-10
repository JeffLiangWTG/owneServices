using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	[CodeProperty(AsycudaBill.Schema.ABL_BillNumber), DescriptionProperty(AsycudaBill.Schema.ABL_BillNumber)]
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter,
		ICusStorageDocPivotTypeSupporter,
		ICusReferenceTypeSupporter,
		IAsycudaTransportMeansProvider
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string BuyerPersonType = nameof(BuyerPersonType);
			public const string SellerPersonType = nameof(SellerPersonType);
			public const string ShipperPersonType = nameof(ShipperPersonType);
			public const string ConsigneePersonType = nameof(ConsigneePersonType);
			public const string NotifyPartyPersonType = nameof(NotifyPartyPersonType);
			public const string TransportDocumentType = nameof(TransportDocumentType);
			public const string ReceptacleId = nameof(ReceptacleId);
			public const int ReceptacleIdMaxLength = 35;
			public const int PrepaidCollectMaxLength = 1;
		}

		protected override void OnFactorySaving()
		{
			CleanDisabledData();

			base.OnFactorySaving();
		}

		public new AsycudaManifestHeader Header => base.Header as AsycudaManifestHeader;
		public new AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		public new ManifestBase.IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (ManifestBase.IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override ManifestBase.IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new ManifestBase.AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);
		public new ManifestBase.IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill> BillScreenings => (ManifestBase.IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill>)base.BillScreenings;
		protected override ManifestBase.IAsycudaBillScreeningCollection<ASYCUDA.Business.AsycudaBillScreening, ASYCUDA.Business.AsycudaBill> CreateNewAsycudaBillScreeningCollection() => new ManifestBase.AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill>(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		protected override Type GetAsycudaBillScreeningTypeCore() => typeof(AsycudaBillScreening);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.EuropeanUnion;
		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill) => ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

		public override ZString[] ShipperRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };
		public override ZString[] ConsigneeRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };
		public override ZString[] NotifyPartyRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };
		public override ZString[] SellerRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };
		public override ZString[] BuyerRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };

		protected override (ZString RegNumber, ZString RegNumberType) GetPartyOrgAddressRegNoAndType(OrgAddress org, ManifestBase.AsycudaBillAddress.AddressType addressType, ZString[] regNoTypes)
		{
			var countryListCL010 = Universal.RefCusCodeListTypes.GetCachedList(org.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today);
			foreach (var regNoType in regNoTypes)
			{
				var orgCusCodes = org?.Header?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(regNoType).Where(x => countryListCL010.ContainsCode(x.OK_RN_NKCodeCountry)).ToArray();
				if (orgCusCodes is { Length: 1 })
				{
					var orgCusCode = orgCusCodes[0];
					return ($"{orgCusCode.OK_RN_NKCodeCountry}{orgCusCode.OK_CustomsRegNo}", regNoType);
				}

				if (orgCusCodes is { Length: > 1 })
				{
					return (RegNumberValueOnMultiple, regNoType);
				}
			}
			return (ZString.Empty, ZString.Empty);
		}

		public ZString RegNumberValueOnMultiple { get; } =
			Res.GetString("d09f0455-401f-47a4-9682-368ac4b54e9b", "* multiple found");

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TransportDocumentTypeList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.TransportDocumentType", Caption = "Transport Document Type", ShortCaption = "Trans. Doc. Type")]
		[MaxLength(4)]
		public ZString TransportDocumentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransportDocumentType);
			set
			{
				var oldValue = TransportDocumentType;

				if (oldValue != value)
				{
					CheckMaximumLength(TransportDocumentTypeInfo, value);
					this.SetSystemDefinedValue(Schema.TransportDocumentType, value);
					if (!IsValidationSuspended)
					{
						(Validation as AsycudaBillValidationForMasterChild)?.ValidateTransportDocumentType();
						(Validation as AsycudaBillValidationForRegularBill)?.ValidateTransportDocumentType();
					}
					TransportDocumentTypeInfo.RefreshBinding();
					Header?.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TransportDocumentTypeInfo => GetZPropertyInfo(Schema.TransportDocumentType);

		[DecimalPlaces(2)]
		[DecimalPrecision(16)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.ABL_FreightValue", Caption = "Postal Charges")]
		public override ZDecimal ABL_FreightValue
		{
			get => base.ABL_FreightValue;
			set => base.ABL_FreightValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.ABL_RX_NKFreightValueCurrency", Caption = "Currency")]
		public override ZString ABL_RX_NKFreightValueCurrency
		{
			get => base.ABL_RX_NKFreightValueCurrency;
			set => base.ABL_RX_NKFreightValueCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.PaymentMethod", Caption = "Method of Payment")]
		[MaxLength(Schema.PrepaidCollectMaxLength)]
		public override ZString ABL_PrepaidCollect
		{
			get => base.ABL_PrepaidCollect;
			set => base.ABL_PrepaidCollect = value;
		}

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) },
				{ CusSupportingInfoTypeList.Codes.ScreeningMethod, typeof(ScreeningMethod) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region Supporting Documents

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}

		SupportingDocumentCollection supportingDocuments;

		#endregion

		#region TranssportMeans

		[ChildEditable(true)]
		public AsycudaTransportMeansCollection AsycudaTransportMeans
		{
			get
			{
				if (asycudaTransportMeans == null)
				{
					asycudaTransportMeans = new AsycudaTransportMeansCollection(this);
					asycudaTransportMeans.Load();
					RegisterEditableChildObject(asycudaTransportMeans);
				}
				return asycudaTransportMeans;
			}
		}
		AsycudaTransportMeansCollection asycudaTransportMeans;

		#endregion

		#region Additional Infos

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new AdditionalInfoCollection(this);
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}

		AdditionalInfoCollection additionalInfos;

		public bool HasAdditionalInfoWithCode10600 => Factory.GetValue(ref hasAdditionalInfoWithCode10600Cached, () => AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == EUICS2AdditionalInfoTypes.Codes.CL701_10600));
		CachedProperty<bool> hasAdditionalInfoWithCode10600Cached;

		#endregion

		#region Cus Supply Chain Actor Reference

		[ChildEditable(true)]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}
				return cusSupplyChainActorReferences;
			}
		}

		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		#endregion

		#region Additional Fiscal Reference

		[ChildEditable(true)]
		public AdditionalFiscalReferenceCollection AdditionalFiscalReferences
		{
			get
			{
				if (additionalFiscalReferences == null)
				{
					additionalFiscalReferences = new AdditionalFiscalReferenceCollection(this);
					additionalFiscalReferences.Load();
					RegisterEditableChildObject(additionalFiscalReferences);
				}
				return additionalFiscalReferences;
			}
		}

		AdditionalFiscalReferenceCollection additionalFiscalReferences;

		#endregion

		#region Screening Method

		[ChildEditable(true)]
		public ScreeningMethodCollection ScreeningMethods
		{
			get
			{
				if (screeningMethods == null)
				{
					screeningMethods = new ScreeningMethodCollection(this);
					screeningMethods.Load();
					RegisterEditableChildObject(screeningMethods);
				}
				return screeningMethods;
			}
		}
		ScreeningMethodCollection screeningMethods;

		#endregion

		#region Person Type Fields

		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				base.ABL_OA_Shipper = value;
				if (ABL_OA_Shipper.IsValid && !IsCopying)
				{
					ShipperPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
				}

				if (value.IsEmpty)
				{
					ShipperOrgPK = ZGuid.Empty;
				}
				ABL_OA_ShipperInfo.RefreshBinding();
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.ShipperPersonType", Caption = "Person Type")]
		public ZString ShipperPersonType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.ShipperPersonType); }
			set
			{
				var oldValue = ShipperPersonType;
				if (oldValue != value)
				{
					CheckMaximumLength(ShipperPersonTypeInfo, value);
					this.SetSystemDefinedValue(Schema.ShipperPersonType, value);
					ShipperPersonTypeInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						(Validation as AsycudaBillValidationForRegularBill)?.ValidateShipperPersonType();
						(Validation as AsycudaBillValidationForMasterChild)?.ValidateShipperPersonType();
					}
				}
			}
		}

		public ZPropertyInfo ShipperPersonTypeInfo => GetZPropertyInfo(Schema.ShipperPersonType);

		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				base.ABL_OA_Consignee = value;
				if (ABL_OA_Consignee.IsValid && !IsCopying)
				{
					ConsigneePersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
				}

				if (value.IsEmpty)
				{
					ConsigneeOrgPK = ZGuid.Empty;
				}
				ABL_OA_ConsigneeInfo.RefreshBinding();
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.ConsigneePersonType", Caption = "Person Type")]
		public ZString ConsigneePersonType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.ConsigneePersonType); }
			set
			{
				var oldValue = ConsigneePersonType;
				if (oldValue != value)
				{
					CheckMaximumLength(ConsigneePersonTypeInfo, value);
					this.SetSystemDefinedValue(Schema.ConsigneePersonType, value);
					ConsigneePersonTypeInfo.RefreshBinding();
				}
			}
		}

		public override bool CanDelete
		{
			get => base.CanDelete && !(Header?.ShouldSynchroniseWithConsol ?? false);
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get => !(Header?.ShouldSynchroniseWithConsol ?? false) ? base.ReasonForNotAbleToDelete : ResString.GetMultilingualString("36e24288-d460-4a19-83d0-6eab4fe257c5", "Cannot delete Bills when defaulting values from Consol.");
		}

		public ZPropertyInfo ConsigneePersonTypeInfo => GetZPropertyInfo(Schema.ConsigneePersonType);

		public override ZGuid ABL_OA_NotifyParty
		{
			get => base.ABL_OA_NotifyParty;
			set
			{
				base.ABL_OA_NotifyParty = value;
				if (ABL_OA_NotifyParty.IsValid && !IsCopying)
				{
					NotifyPartyPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
				}

				if (value.IsEmpty)
				{
					NotifyPartyOrgPK = ZGuid.Empty;
				}
				ABL_OA_NotifyPartyInfo.RefreshBinding();
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.NotifyPartyPersonType", Caption = "Person Type")]
		public ZString NotifyPartyPersonType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.NotifyPartyPersonType); }
			set
			{
				var oldValue = NotifyPartyPersonType;
				if (oldValue != value)
				{
					CheckMaximumLength(NotifyPartyPersonTypeInfo, value);
					this.SetSystemDefinedValue(Schema.NotifyPartyPersonType, value);
					NotifyPartyPersonTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo NotifyPartyPersonTypeInfo => GetZPropertyInfo(Schema.NotifyPartyPersonType);

		#endregion

		#region ICusCodeDataTypeSupporter

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = base.SupportedCusCodeDataTypes;
				result.Add(CusCodeDataTypeList.Codes.EUICS2SupplementaryDeclarant, typeof(SupplementaryDeclarant));
				result.Add(CusCodeDataTypeList.Codes.EUICS2Receptacle, typeof(Receptacle));
				return result;
			}
		}

		#endregion

		#region Receptacle

		[ResourceStringData("FE46A649-5F4B-41F4-805D-9DDC2EF08407", ShortCaption = "Rec. ID", Caption = "Receptacle ID")]
		[MaxLength(Schema.ReceptacleIdMaxLength)]
		public ZString ReceptacleId
		{
			get { return (Receptacle != null) ? Receptacle.CY_Data : ZString.Empty; }
			set => ReceptacleHelper.LoadOrCreate(value, this, 1, ReceptacleIdInfo, Receptacle);
		}

		public ZPropertyInfo ReceptacleIdInfo
		{
			get { return Receptacle != null ? GetWrappedZPropertyInfo(nameof(ReceptacleId), x => Receptacle.CY_DataInfo) : GetZPropertyInfo(nameof(ReceptacleId)); }
		}

		public Receptacle Receptacle
		{
			get
			{
				if (receptacle == null || receptacle.IsDeleted)
				{
					var loader = new Receptacle.Loader(Factory);
					receptacle = loader.Load(this, 1);
					if (receptacle != null)
					{
						RegisterEditableChildObject(receptacle);
					}
				}
				return receptacle;
			}
		}
		Receptacle receptacle;

		public bool IsReceptacleEnabled => Header != null && Header.IsForwarderManifest && Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44;

		#endregion

		#region Supplementary Declarant

		[ChildEditable(true)]
		public SupplementaryDeclarantCollection SupplementaryDeclarants
		{
			get
			{
				if (supplementaryDeclarants == null)
				{
					supplementaryDeclarants = new SupplementaryDeclarantCollection(this);
					supplementaryDeclarants.Load();
					RegisterEditableChildObject(supplementaryDeclarants);
				}
				return supplementaryDeclarants;
			}
		}
		SupplementaryDeclarantCollection supplementaryDeclarants;

		#endregion

		#region ICusStorageDocPivotTypeSupporter

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

		public void ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		public Type CusStorageDocPivotType => typeof(CusStorageDocPivot);

		public IEnumerable<IStorageDocsBaseCollection> EDocCollections => EDocsHelper.GetEDocCollections(this.Header);

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		#endregion

		#region ICusReferenceTypeSupporter

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, typeof(CusSupplyChainActorReference) },
			{ CusReferenceTypeList.Codes.FiscalReference, typeof(AdditionalFiscalReference) },
		};

		#endregion

		[ResourceStringData("8316F47F-734F-4C17-AF78-45365E835360", Caption = "Buyer")]
		public override ZGuid ABL_OA_Buyer
		{
			get => base.ABL_OA_Buyer;
			set
			{
				var oldValue = ABL_OA_Buyer;
				base.ABL_OA_Buyer = value;
				if (!IsCopying && oldValue != ABL_OA_Buyer)
				{
					if (ABL_OA_Buyer.IsValid)
					{
						BuyerPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
					}
				}
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.BuyerPersonType", Caption = "Person Type")]
		public ZString BuyerPersonType
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.BuyerPersonType); }
			set
			{
				var oldValue = BuyerPersonType;
				if (oldValue != value)
				{
					CheckMaximumLength(BuyerPersonTypeInfo, value);
					this.SetUserDefinedValue(Schema.BuyerPersonType, value);
					BuyerPersonTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BuyerPersonTypeInfo => GetZPropertyInfo(Schema.BuyerPersonType);

		[ResourceStringData("C3C91C5D-D213-4E1C-B2A2-11224CAA22C4", Caption = "Buyer Name")]
		public override ZString ABL_BuyerName { get => base.ABL_BuyerName; set => base.ABL_BuyerName = value; }

		[ResourceStringData("B4128646-D7D5-4C01-B182-B63CA6AAFA1B", Caption = "Buyer Street 1")]
		public override ZString ABL_BuyerStreet1 { get => base.ABL_BuyerStreet1; set => base.ABL_BuyerStreet1 = value; }

		[ResourceStringData("CDD35AE1-125B-4C4F-9333-21F46067BE28", Caption = "Buyer Street 2")]
		public override ZString ABL_BuyerStreet2 { get => base.ABL_BuyerStreet2; set => base.ABL_BuyerStreet2 = value; }

		[ResourceStringData("BA66B500-280E-4FDF-AA62-6048F9A0FB24", Caption = "Buyer City")]
		public override ZString ABL_BuyerCity { get => base.ABL_BuyerCity; set => base.ABL_BuyerCity = value; }

		[ResourceStringData("55BBDF32-C520-4CC5-BA14-1A3200EE9832", Caption = "Buyer State")]
		public override ZString ABL_BuyerState { get => base.ABL_BuyerState; set => base.ABL_BuyerState = value; }

		[ResourceStringData("7740DC36-B524-4772-B317-3B82EE664EBB", Caption = "Buyer Postcode")]
		public override ZString ABL_BuyerPostcode { get => base.ABL_BuyerPostcode; set => base.ABL_BuyerPostcode = value; }

		[ResourceStringData("7C5EBF9C-0C2C-4823-B1EE-6687C16C3978", Caption = "Buyer Phone")]
		public override ZString ABL_BuyerPhone { get => base.ABL_BuyerPhone; set => base.ABL_BuyerPhone = value; }

		[ResourceStringData("8FFA5DE0-A190-4D34-B0D5-E649302482D2", Caption = "Buyer Country / Region")]
		public override ZString ABL_RN_NKBuyerCountry { get => base.ABL_RN_NKBuyerCountry; set => base.ABL_RN_NKBuyerCountry = value; }

		[MaxLength(Schema.ABL_BuyerRegNoTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BuyerRegistrationNoTypeList))]
		public override ZString ABL_BuyerRegNoType { get => base.ABL_BuyerRegNoType; set => base.ABL_BuyerRegNoType = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PackUQList))]
		public override ZString ABL_ManifestUQ { get => base.ABL_ManifestUQ; set => base.ABL_ManifestUQ = value; }

		[List(nameof(ABL_OA_Seller_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("778D88FA-6741-438E-94CD-DF735489A81E", Caption = "Seller")]
		public override ZGuid ABL_OA_Seller
		{
			get => base.ABL_OA_Seller;
			set
			{
				var oldValue = ABL_OA_Seller;
				base.ABL_OA_Seller = value;
				if (!IsCopying && oldValue != ABL_OA_Seller)
				{
					ClearValueIfNeeded(!ABL_OA_Seller.IsEmpty, ABL_SellerNameInfo, ABL_SellerStreet1Info, ABL_SellerStreet2Info, ABL_SellerCityInfo, ABL_SellerStateInfo, ABL_SellerPostcodeInfo, ABL_RN_NKSellerCountryInfo, ABL_SellerRegNoInfo, ABL_SellerRegNoTypeInfo, ABL_SellerPhoneInfo);
					DefaultPartyOrgAddressDetails(Seller, ManifestBase.AsycudaBillAddress.AddressType.Seller);
					if (ABL_OA_Seller.IsValid)
					{
						SellerPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
					}
				}
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ScreeningAuthorizedPersonTypes))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill.SellerPersonType", Caption = "Person Type")]
		public ZString SellerPersonType
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.SellerPersonType); }
			set
			{
				var oldValue = SellerPersonType;
				if (oldValue != value)
				{
					CheckMaximumLength(SellerPersonTypeInfo, value);
					this.SetUserDefinedValue(Schema.SellerPersonType, value);
					SellerPersonTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SellerPersonTypeInfo => GetZPropertyInfo(Schema.SellerPersonType);

		[ResourceStringData("8BC8DE64-C798-4D34-A9D3-52416A6A9918", Caption = "Seller Name")]
		public override ZString ABL_SellerName { get => base.ABL_SellerName; set => base.ABL_SellerName = value; }

		[ResourceStringData("201F8AEF-F952-4557-A88D-7D4E60B1A721", Caption = "Seller Street 1")]
		public override ZString ABL_SellerStreet1 { get => base.ABL_SellerStreet1; set => base.ABL_SellerStreet1 = value; }

		[ResourceStringData("AA676821-313B-451F-8D9A-E9BF27E98719", Caption = "Seller Street 2")]
		public override ZString ABL_SellerStreet2 { get => base.ABL_SellerStreet2; set => base.ABL_SellerStreet2 = value; }

		[ResourceStringData("4969027C-8874-493D-ACF3-33A3A94B6E3F", Caption = "Seller City")]
		public override ZString ABL_SellerCity { get => base.ABL_SellerCity; set => base.ABL_SellerCity = value; }

		[ResourceStringData("7AA698C5-E867-4E7A-99AA-C5E04FC2F8DB", Caption = "Seller State")]
		public override ZString ABL_SellerState { get => base.ABL_SellerState; set => base.ABL_SellerState = value; }

		[ResourceStringData("C17055D6-762C-433B-B0AE-649A60325728", Caption = "Seller Postcode")]
		public override ZString ABL_SellerPostcode { get => base.ABL_SellerPostcode; set => base.ABL_SellerPostcode = value; }

		[ResourceStringData("AD4626F6-4947-4B8D-8E34-00F4E67AE406", Caption = "Seller Phone")]
		public override ZString ABL_SellerPhone { get => base.ABL_SellerPhone; set => base.ABL_SellerPhone = value; }

		[ResourceStringData("77EC246B-F2F9-4C39-B0A6-36254F0D535B", Caption = "Seller Country / Region")]
		public override ZString ABL_RN_NKSellerCountry { get => base.ABL_RN_NKSellerCountry; set => base.ABL_RN_NKSellerCountry = value; }

		protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;

		public ZDecimal GrossWeightInKG => new ZWeight(ABL_GrossWeight, ABL_GrossWeightUQ).InKilogramsSafe;

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => base.GetShouldPropertiesBeReadOnly(property) || !(Header?.CanBeAmended(property.Name) ?? true);

		void CleanDisabledData()
		{
			if (!IsReceptacleEnabled && Receptacle != null)
			{
				Receptacle.Delete();
			}
		}
	}
}
