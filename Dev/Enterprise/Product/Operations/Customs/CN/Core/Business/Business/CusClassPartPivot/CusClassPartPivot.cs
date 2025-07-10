using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusClassPartPivot : AutoCusClassPartPivot, ICusCodeDataTypeSupporter, IAdditionalInformationWrapperParent
	{
		#region Schema

		public new class Schema : AutoCusClassPartPivot.Schema
		{
			public const string CargoAttributesAsString = "CargoAttributesAsString";
			public const string CIQIngredient = "CIQIngredient";
			public const string DangerousGoodsDGSubs = "DangerousGoodsDGSubs";
		}

		#endregion

		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			if (AdditionalInformationCodes.Count == 0)
			{
				var nameOfGoods = Details.CNC_NameOfGoods;
				if (!nameOfGoods.IsEmpty)
				{
					AdditionalInformationCodes.SetNameOfGoods(nameOfGoods);
				}
				var goodsSpecModel = Details.CNC_GoodsSpecModel;
				if (!goodsSpecModel.IsEmpty)
				{
					AdditionalInformationCodes.SetGoodsSpecModel(UniversalTariff, IsEnteringOrExiting, goodsSpecModel);
				}
			}
			else
			{
				var nameOfGoods = AdditionalInformationCodes.GetNameOfGoods();
				if (nameOfGoods != Details.CNC_NameOfGoods)
				{
					Details.CNC_NameOfGoods = nameOfGoods.Left(CusCNClassification.Schema.CNC_NameOfGoodsMaxLength);
				}
				var goodsSpecModel = AdditionalInformationCodes.GetGoodsSpecModel(UniversalTariff, IsEnteringOrExiting);
				if (goodsSpecModel != Details.CNC_GoodsSpecModel)
				{
					Details.CNC_GoodsSpecModel = goodsSpecModel.Left(CusCNClassification.Schema.CNC_GoodsSpecModelMaxLength);
				}
			}
		}

		#region Override Properties

		[MaxLength(10)]
		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set => base.CI_TariffNum = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.OriginStateList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CI_RW_NKOriginState", Caption = "Origin State")]
		public override ZString CI_RW_NKOriginState
		{
			get => base.CI_RW_NKOriginState;
			set
			{
				var oldValue = CI_RW_NKOriginState;
				base.CI_RW_NKOriginState = value;
				if (!IsCopying && oldValue != value)
				{
					DefaultCIQOriginState(value);
				}
			}
		}

		void DefaultCIQOriginState(ZString ciOriginState)
		{
			if (!CI_RN_NKCountryOfOrigin.IsEmpty)
			{
				var ciqState = CNRefCusMapper.MapCW1StateCodeToCustomsCode(Factory, CI_RN_NKCountryOfOrigin + ciOriginState);
				CNC_OriginState = ciqState.IsEmpty ? CountryOfOrigin.RN_IsoNumericUNM49Code : ciqState;
			}
		}

		public override ZString CI_ChildType
		{
			get => base.CI_ChildType;
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value;

				if (!IsCopying && oldValue != value)
				{
					if (IsImportClassification)
					{
						CNC_OriginDistrict = CNC_OriginRegion = ZString.Empty;
					}
					else if (IsExportClassification)
					{
						CI_RW_NKOriginState = CNC_OriginState = CNC_DestinationDistrict = CNC_DestinationRegion = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_OriginState", Caption = "Origin State")]
		public override ZString CNC_OriginState
		{
			get => base.CNC_OriginState;
			set => base.CNC_OriginState = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_CIQTariff", Caption = "CIQ Tariff")]
		public override ZString CNC_CIQTariff
		{
			get => base.CNC_CIQTariff;
			set => base.CNC_CIQTariff = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.TradeUnitQtyList))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_TradeUnitQty", Caption = "Trade Quantity Unit")]
		public override ZString CNC_TradeUnitQty { get => base.CNC_TradeUnitQty; set => base.CNC_TradeUnitQty = value; }

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CI_RN_NKCountryOfOrigin", Caption = "Goods Origin")]
		public override ZString CI_RN_NKCountryOfOrigin
		{
			get => base.CI_RN_NKCountryOfOrigin;
			set => base.CI_RN_NKCountryOfOrigin = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CI_RN_NKCountryOfExport", Caption = "Final Destination")]
		public override ZString CI_RN_NKCountryOfExport
		{
			get => base.CI_RN_NKCountryOfExport;
			set => base.CI_RN_NKCountryOfExport = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_OriginDistrict", Caption = "Domestic Origin")]
		public override ZString CNC_OriginDistrict
		{
			get => base.CNC_OriginDistrict;
			set => base.CNC_OriginDistrict = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_OriginRegion", Caption = "Domestic Origin")]
		public override ZString CNC_OriginRegion
		{
			get => base.CNC_OriginRegion;
			set => base.CNC_OriginRegion = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_DestinationDistrict", Caption = "Domestic Destination", ShortCaption = "Domestic Dest.")]
		public override ZString CNC_DestinationDistrict
		{
			get => base.CNC_DestinationDistrict;
			set => base.CNC_DestinationDistrict = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_DestinationRegion", Caption = "Domestic Destination", ShortCaption = "Domestic Dest.")]
		public override ZString CNC_DestinationRegion
		{
			get => base.CNC_DestinationRegion;
			set => base.CNC_DestinationRegion = value;
		}

		#region Manufacturer Address

		[RelatedBusinessObject("ManufacturerAddress")]
		[List(nameof(CNC_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_OA_ManufacturerAddress", Caption = "Manufacturer")]
		public override ZGuid CNC_OA_ManufacturerAddress
		{
			get => base.CNC_OA_ManufacturerAddress;
			set
			{
				base.CNC_OA_ManufacturerAddress = value;
				fCNC_OA_Manufacturer_ZAddress = null;
			}
		}

		public OrgAddress ManufacturerAddress => Factory.Load<OrgAddress>(CNC_OA_ManufacturerAddress);

		public ZAddress CNC_OA_ManufacturerAddress_ZAddress
		{
			get
			{
				if (fCNC_OA_Manufacturer_ZAddress == null)
				{
					fCNC_OA_Manufacturer_ZAddress = new ZAddress(CNC_OA_ManufacturerAddressInfo)
					{
						IsOrgVisible = true,
						DefaultAddressType = AddressType.NoDefault,
						GetDefaultAddress = header => header?.MainAddress.PK ?? ZGuid.Empty
					};
					RegisterEditableChildObject(fCNC_OA_Manufacturer_ZAddress);
				}
				return fCNC_OA_Manufacturer_ZAddress;
			}
		}
		ZAddress fCNC_OA_Manufacturer_ZAddress;

		#endregion

		[MaxLength(100)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CI_NDescription", Caption = "Specification")]
		public override ZString CI_NDescription
		{
			get => base.CI_NDescription;
			set => base.CI_NDescription = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_Model", Caption = "Model")]
		public override ZString CNC_Model
		{
			get => base.CNC_Model;
			set => base.CNC_Model = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_Brand", Caption = "Brand")]
		public override ZString CNC_Brand
		{
			get => base.CNC_Brand;
			set => base.CNC_Brand = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_EndUse", Caption = "End Use")]
		public override ZString CNC_EndUse
		{
			get => base.CNC_EndUse;
			set => base.CNC_EndUse = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_TradeUnitPrice", Caption = "Trade Unit Price")]
		public override ZDecimal CNC_TradeUnitPrice
		{
			get => base.CNC_TradeUnitPrice;
			set => base.CNC_TradeUnitPrice = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_RX_NKTradeUnitPriceCurrency", Caption = "Currency")]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.TradeUnitPriceCurrencies))]
		public override ZString CNC_RX_NKTradeUnitPriceCurrency
		{
			get => base.CNC_RX_NKTradeUnitPriceCurrency;
			set => base.CNC_RX_NKTradeUnitPriceCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CNC_NonDangerousChemicalFlag", Caption = "Non Dangerous Goods")]
		public override ZBool CNC_NonDangerousChemicalFlag { get => base.CNC_NonDangerousChemicalFlag; set => base.CNC_NonDangerousChemicalFlag = value; }

		#endregion

		#region Dangerous Goods

		public UNDGDataItem DangerousGoods => Part?.UNDGs?.FirstItemForBinding[0];

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.UNDGSubs))]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|DangerousGoodsDGSubs", Caption = "UNDG")]
		public ZGuid DangerousGoodsDGSubs
		{
			get
			{
				return (dangerousGoodsDGSubsCached
						?? (dangerousGoodsDGSubsCached =
							new CachedProperty<ZGuid>(Factory, () => GetUNDGValue(undg => undg.DI_DG)))).Value;
			}
			set
			{
				SetUNDGValue(x => x.DI_DG = value);
				DangerousGoodsDGSubsInfo.RefreshBinding();
			}
		}

		T GetUNDGValue<T>(Func<UNDGDataItem, T> valueGetter)
		{
			if (Part?.UNDGs == null)
			{
				return default;
			}
			return Part.UNDGs.Count > 1 ? default : Part.UNDGs.Select(valueGetter).FirstOrDefault();
		}

		void SetUNDGValue(Action<UNDGDataItem> valueSetter)
		{
			if (Part?.UNDGs != null)
			{
				if (Part.UNDGs.Count <= 1)
				{
					var undg = (Part.UNDGs.FirstOrDefault() ?? Part.UNDGs.AddNew());
					var oldDI_DGValue = undg.DI_DG;
					valueSetter(undg);
					if (undg.DI_DG.IsEmpty && undg.DI_OC_DGContact.IsEmpty)
					{
						undg.Delete();
					}
					else
					{
						var dgClass = undg.UNDGSubstance?.DG_Class ?? ZString.Empty;
						if (undg.DI_IMOClass != dgClass)
						{
							undg.DI_IMOClass = dgClass;
						}

						if (!IsCopying && oldDI_DGValue.IsEmpty)
						{
							CNC_NonDangerousChemicalFlag = !undg.DI_DG.IsEmpty && !IsDangerousChemical;
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDangerousGoodsDGSubs();
				}
			}
		}

		CachedProperty<ZGuid> dangerousGoodsDGSubsCached;

		public ZPropertyInfo DangerousGoodsDGSubsInfo => GetZPropertyInfo(Schema.DangerousGoodsDGSubs);

		public bool IsDangerousChemical => DangerousGoodsHelper.IsDangerousChemical(Factory, AdditionalInformationHelper, ZDateTime.Now);

		#endregion

		#region Cargo Attributes

		[ChildEditable(true)]
		public CargoAttributeCollection CargoAttributes
		{
			get
			{
				if (cargoAttributes == null)
				{
					cargoAttributes = new CargoAttributeCollection(this);
					cargoAttributes.Load();
					RegisterEditableChildObject(cargoAttributes);
				}
				return cargoAttributes;
			}
		}
		CargoAttributeCollection cargoAttributes;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CargoAttributesAsString", Caption = "Cargo Attributes")]
		public ZString CargoAttributesAsString => CargoAttributes.GetSelectedOptionDescAsString();

		public ZPropertyInfo CargoAttributesAsStringInfo => GetZPropertyInfo(Schema.CargoAttributesAsString);

		public ZBool NonDangerousChemicalFlagVisible => CargoAttributes.IsAnyDangerousGoodsAttributeSelected() || !DangerousGoodsDGSubs.IsEmpty;

		#endregion

		#region notes

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|CIQIngredient", Caption = "Customs Quarantine Ingredient", ShortCaption = "Ingredient")]
		public ZString CIQIngredient
		{
			get => CustomsQuarantineIngredientNote.Text;
			set => CustomsQuarantineIngredientNote.SetNoteText(this, CIQIngredientInfo, value);
		}

		HiddenTextNote CustomsQuarantineIngredientNote => customsQuarantineIngredientNote ?? (customsQuarantineIngredientNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsQuarantineIngredient.Description));
		HiddenTextNote customsQuarantineIngredientNote;

		public ZPropertyInfo CIQIngredientInfo => GetZPropertyInfo(Schema.CIQIngredient);

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Constants.CusCodeDataTypes.Codes.AdditionalInformation, typeof(AdditionalInformation));
			result.Add(Constants.CusCodeDataTypes.Codes.CargoAttribute, typeof(CargoAttribute));
			return result;
		}

		#endregion

		#region Additional Information

		[ChildEditable(true)]
		public AdditionalInformationCollection AdditionalInformationCodes
		{
			get
			{
				if (additionalInformationCodes == null)
				{
					additionalInformationCodes = new AdditionalInformationCollection(this);
					additionalInformationCodes.Load();
					RegisterEditableChildObject(additionalInformationCodes);
				}
				return additionalInformationCodes;
			}
		}
		AdditionalInformationCollection additionalInformationCodes;

		public EnteringOrExiting IsEnteringOrExiting
		{
			get
			{
				var result = EnteringOrExiting.Both;
				if (!IsHTB)
				{
					if (IsImportClassification)
					{
						result = EnteringOrExiting.Entering;
					}
					else if (IsExportClassification)
					{
						result = EnteringOrExiting.Exiting;
					}
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|NameOfGoods", Caption = "Name of Goods")]
		public override ZString CNC_NameOfGoods
		{
			get => base.CNC_NameOfGoods;
			set
			{
				base.CNC_NameOfGoods = value;
				AdditionalInformationCodes.SetNameOfGoods(value);
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.CN.Business.CusClassPartPivot|GoodsSpecModel", Caption = "Specification & Model", ShortCaption = "Spec & Model")]
		public override ZString CNC_GoodsSpecModel
		{
			get => base.CNC_GoodsSpecModel;
			set
			{
				base.CNC_GoodsSpecModel = value;
				AdditionalInformationCodes.SetGoodsSpecModel(UniversalTariff, IsEnteringOrExiting, value);
			}
		}

		public AdditionalInformationHelper AdditionalInformationHelper => fAdditionalInformationHelper ?? (fAdditionalInformationHelper = new AdditionalInformationHelper(this, CNC_NameOfGoodsInfo, CNC_GoodsSpecModelInfo, () => IsEnteringOrExiting));
		AdditionalInformationHelper fAdditionalInformationHelper;

		#region Implementation of IAdditionalInformationWrapperParent

		TariffView IAdditionalInformationWrapperParent.UniversalTariff => UniversalTariff;

		ZString IAdditionalInformationWrapperParent.CountryOfOrigin => CI_RN_NKCountryOfOrigin;

		bool IAdditionalInformationWrapperParent.ElementValueAllowEmpty => true;

		ValidationModes IValidationModeProvider.ValidationMode => ValidationModes.Full;

		#endregion

		#endregion

		#region CloneFunction
		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusClassPartPivot)base.CloneInternal(args);

			using (result.GetValidationSuspender())
			{
				foreach (CargoAttribute cargoAttribute in CargoAttributes)
				{
					result.CargoAttributes.Add((CargoAttribute)cargoAttribute.Clone(new BusinessObjectCloneArgs(new[] { CusCodeData.Schema.CY_ParentID })));
				}

				foreach (AdditionalInformation additionalInformation in AdditionalInformationCodes)
				{
					result.AdditionalInformationCodes.Add((AdditionalInformation)additionalInformation.Clone(new BusinessObjectCloneArgs(new[] { CusCodeData.Schema.CY_ParentID })));
				}

				result.CIQIngredient = this.CIQIngredient;
				result.Details.CopyPersistentValuesFrom(Details, new BusinessObjectCloneArgs(new[] { CusCNClassification.Schema.CNC_CI, CusCNClassification.Schema.CNC_UNPackageMarking, CusCNClassification.Schema.CNC_NonDangerousChemicalFlag }));
			}

			return result;
		}
		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			AdditionalInformationCodes.CleanByTariff(UniversalTariff, IsEnteringOrExiting);
		}
	}
}
