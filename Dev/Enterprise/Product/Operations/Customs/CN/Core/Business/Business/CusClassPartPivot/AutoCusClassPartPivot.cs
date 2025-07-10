using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public abstract partial class AutoCusClassPartPivot : Customs.Business.BaseCusClassPartPivot
	{
		protected AutoCusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new partial class Schema : Customs.Business.BaseCusClassPartPivot.Schema
		{
			public const string CNC_CIQTariff = CusCNClassificationSchema.Constants.CNC_CIQTariff;
			public const string CNC_ExpiryDate = CusCNClassificationSchema.Constants.CNC_ExpiryDate;
			public const string CNC_QualityGuaranteePeriod = CusCNClassificationSchema.Constants.CNC_QualityGuaranteePeriod;
			public const string CNC_Brand = CusCNClassificationSchema.Constants.CNC_Brand;
			public const string CNC_Model = CusCNClassificationSchema.Constants.CNC_Model;
			public const string CNC_EndUse = CusCNClassificationSchema.Constants.CNC_EndUse;
			public const string CNC_OriginState = CusCNClassificationSchema.Constants.CNC_OriginState;
			public const string CNC_DestinationDistrict = CusCNClassificationSchema.Constants.CNC_DestinationDistrict;
			public const string CNC_OriginDistrict = CusCNClassificationSchema.Constants.CNC_OriginDistrict;
			public const string CNC_DestinationRegion = CusCNClassificationSchema.Constants.CNC_DestinationRegion;
			public const string CNC_OriginRegion = CusCNClassificationSchema.Constants.CNC_OriginRegion;
			public const string CNC_TradeUnitQty = CusCNClassificationSchema.Constants.CNC_TradeUnitQty;
			public const string CNC_OA_ManufacturerAddress = CusCNClassificationSchema.Constants.CNC_OA_ManufacturerAddress;
			public const string CNC_UNPackageMarking = CusCNClassificationSchema.Constants.CNC_UNPackageMarking;
			public const string CNC_NonDangerousChemicalFlag = CusCNClassificationSchema.Constants.CNC_NonDangerousChemicalFlag;
			public const string CNC_NameOfGoods = CusCNClassificationSchema.Constants.CNC_NameOfGoods;
			public const string CNC_GoodsSpecModel = CusCNClassificationSchema.Constants.CNC_GoodsSpecModel;
		}
		#endregion

		#region Details Properties

		#region CNC_CIQTariff

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.CIQTariffList))]
		public virtual ZString CNC_CIQTariff
		{
			get => Details.CNC_CIQTariff;
			set
			{
				Details.CNC_CIQTariff = value;
				CNC_CIQTariffInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_CIQTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_CIQTariff, x => Details.CNC_CIQTariffInfo); }
		}

		#endregion

		#region CNC_ExpiryDate

		public virtual ZDateTime CNC_ExpiryDate
		{
			get => Details.CNC_ExpiryDate;
			set => Details.CNC_ExpiryDate = value;
		}

		public virtual ZPropertyInfo CNC_ExpiryDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_ExpiryDate, x => Details.CNC_ExpiryDateInfo); }
		}

		#endregion

		#region CNC_QualityGuaranteePeriod

		public virtual ZInt CNC_QualityGuaranteePeriod
		{
			get => Details.CNC_QualityGuaranteePeriod;
			set => Details.CNC_QualityGuaranteePeriod = value;
		}

		public virtual ZPropertyInfo CNC_QualityGuaranteePeriodInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_QualityGuaranteePeriod, x => Details.CNC_QualityGuaranteePeriodInfo); }
		}

		#endregion

		#region CNC_Brand

		public virtual ZString CNC_Brand
		{
			get => Details.CNC_Brand;
			set => Details.CNC_Brand = value;
		}

		public virtual ZPropertyInfo CNC_BrandInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_Brand, x => Details.CNC_BrandInfo); }
		}

		#endregion

		#region CNC_Model

		public virtual ZString CNC_Model
		{
			get => Details.CNC_Model;
			set => Details.CNC_Model = value;
		}

		public virtual ZPropertyInfo CNC_ModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_Model, x => Details.CNC_ModelInfo); }
		}

		#endregion

		#region CNC_EndUse

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.EndUseList))]
		public virtual ZString CNC_EndUse
		{
			get => Details.CNC_EndUse;
			set
			{
				Details.CNC_EndUse = value;
				CNC_EndUseInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_EndUseInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_EndUse, x => Details.CNC_EndUseInfo); }
		}

		#endregion

		#region CNC_OriginState

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.CIQOriginStateList))]
		public virtual ZString CNC_OriginState
		{
			get => Details.CNC_OriginState;
			set
			{
				Details.CNC_OriginState = value;
				CNC_OriginStateInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_OriginStateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_OriginState, x => Details.CNC_OriginStateInfo); }
		}

		#endregion

		#region CNC_DestinationDistrict

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.DestDistrictList))]
		public virtual ZString CNC_DestinationDistrict
		{
			get => Details.CNC_DestinationDistrict;
			set
			{
				Details.CNC_DestinationDistrict = value;
				CNC_DestinationDistrictInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_DestinationDistrictInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_DestinationDistrict, x => Details.CNC_DestinationDistrictInfo); }
		}

		#endregion

		#region CNC_OriginDistrict

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.OrigDistrictList))]
		public virtual ZString CNC_OriginDistrict
		{
			get => Details.CNC_OriginDistrict;
			set
			{
				Details.CNC_OriginDistrict = value;
				CNC_OriginDistrictInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_OriginDistrictInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_OriginDistrict, x => Details.CNC_OriginDistrictInfo); }
		}

		#endregion

		#region CNC_DestinationRegion

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.DestRegionList))]
		public virtual ZString CNC_DestinationRegion
		{
			get => Details.CNC_DestinationRegion;
			set
			{
				Details.CNC_DestinationRegion = value;
				CNC_DestinationRegionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_DestinationRegionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_DestinationRegion, x => Details.CNC_DestinationRegionInfo); }
		}

		#endregion

		#region CNC_OriginRegion

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.OrigRegionList))]
		public virtual ZString CNC_OriginRegion
		{
			get => Details.CNC_OriginRegion;
			set
			{
				Details.CNC_OriginRegion = value;
				CNC_OriginRegionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_OriginRegionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_OriginRegion, x => Details.CNC_OriginRegionInfo); }
		}

		#endregion

		#region CNC_TradeUnitQty

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.TradeUnitQtyList))]
		public virtual ZString CNC_TradeUnitQty
		{
			get => Details.CNC_TradeUnitQty;
			set
			{
				Details.CNC_TradeUnitQty = value;
				CNC_TradeUnitQtyInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_TradeUnitQtyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_TradeUnitQty, x => Details.CNC_TradeUnitQtyInfo); }
		}

		#endregion

		#region CNC_OA_ManufacturerAddress

		public virtual ZGuid CNC_OA_ManufacturerAddress
		{
			get => Details.CNC_OA_ManufacturerAddress;
			set => Details.CNC_OA_ManufacturerAddress = value;
		}

		public virtual ZPropertyInfo CNC_OA_ManufacturerAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_OA_ManufacturerAddress, x => Details.CNC_OA_ManufacturerAddressInfo); }
		}

		#endregion

		#region CNC_UNPackageMarking

		[List(nameof(CNClassificationLookups) + "." + nameof(CusCNClassificationLookups.UNDGPackageTypes))]
		public virtual ZString CNC_UNPackageMarking
		{
			get => Details.CNC_UNPackageMarking;
			set
			{
				Details.CNC_UNPackageMarking = value;
				CNC_UNPackageMarkingInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_UNPackageMarkingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_UNPackageMarking, x => Details.CNC_UNPackageMarkingInfo); }
		}

		#endregion

		#region CNC_NonDangerousChemicalFlag

		public virtual ZBool CNC_NonDangerousChemicalFlag
		{
			get => Details.CNC_NonDangerousChemicalFlag;
			set => Details.CNC_NonDangerousChemicalFlag = value;
		}

		public virtual ZPropertyInfo CNC_NonDangerousChemicalFlagInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_NonDangerousChemicalFlag, x => Details.CNC_NonDangerousChemicalFlagInfo); }
		}

		#endregion

		#region CNC_NameOfGoods

		public virtual ZString CNC_NameOfGoods
		{
			get => Details.CNC_NameOfGoods;
			set
			{
				Details.CNC_NameOfGoods = value;
				CNC_NameOfGoodsInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_NameOfGoodsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_NameOfGoods, x => Details.CNC_NameOfGoodsInfo); }
		}

		#endregion

		#region CNC_GoodsSpecModel

		public virtual ZString CNC_GoodsSpecModel
		{
			get => Details.CNC_GoodsSpecModel;
			set
			{
				Details.CNC_GoodsSpecModel = value;
				CNC_GoodsSpecModelInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CNC_GoodsSpecModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CNC_GoodsSpecModel, x => Details.CNC_GoodsSpecModelInfo); }
		}

		#endregion

		#region CNC_TradeUnitPrice

		public virtual ZDecimal CNC_TradeUnitPrice
		{
			get => Details.CNC_TradeUnitPrice;
			set
			{
				Details.CNC_TradeUnitPrice = value;
				CNC_TradeUnitPriceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CNC_TradeUnitPriceInfo => GetWrappedZPropertyInfo(nameof(CNC_TradeUnitPrice), x => Details.CNC_TradeUnitPriceInfo);

		#endregion

		#region CNC_RX_NKTradeUnitPriceCurrency

		public virtual ZString CNC_RX_NKTradeUnitPriceCurrency
		{
			get => Details.CNC_RX_NKTradeUnitPriceCurrency;
			set
			{
				Details.CNC_RX_NKTradeUnitPriceCurrency = value;
				CNC_RX_NKTradeUnitPriceCurrencyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CNC_RX_NKTradeUnitPriceCurrencyInfo => GetWrappedZPropertyInfo(nameof(CNC_RX_NKTradeUnitPriceCurrency), x => Details.CNC_RX_NKTradeUnitPriceCurrencyInfo);

		#endregion

		#endregion

		#region Details object/Validation and Lookups objects

		#endregion

		#region ClassificationDetails

		public CusCNClassificationLookups CNClassificationLookups => Details.Lookups;

		internal CusCNClassification Details
		{
			get
			{
				if (fDetails == null || fDetails.IsDeleted)
				{
					var query = new ZQuery(CusCNClassificationSchema.CNC_CI, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					fDetails = Factory.LoadTop1<CusCNClassification>(query);
					if (fDetails == null && !IsDeleted)
					{
						fDetails = Factory.New<CusCNClassification>();
						fDetails.CNC_CI = PK;
					}
					RegisterEditableChildObject(fDetails);
				}
				return fDetails;
			}
		}
		CusCNClassification fDetails;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Details.Delete();
			}
			base.Delete();
		}
		#endregion
	}
}
