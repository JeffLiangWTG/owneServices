using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatLine : AutoCusIntrastatLine
		, IClusterKeyWorker
		, ITariffFormatProvider
		, Integration.Customs.EU.ICusIntrastatLine
	{
		public CusIntrastatLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusIntrastatLine.Schema
		{
			public const string CIL_FormattedTariff = nameof(CusIntrastatLine.CIL_FormattedTariff);
			public const string CIL_MassInKilogramsUnit = nameof(CusIntrastatLine.CIL_MassInKilogramsUnit);
		}

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid CIL_CIH_Header
		{
			get => base.CIL_CIH_Header;
			set => base.CIL_CIH_Header = value;
		}

		[ResourceStringData("0213b646-7c91-426f-9911-4f7e6c62bce3", Caption = "Goods Description")]
		public override ZString CIL_DescriptionOfGoods
		{
			get => base.CIL_DescriptionOfGoods;
			set => base.CIL_DescriptionOfGoods = value;
		}

		[ResourceStringData("d8f38953-d5a9-4a68-9059-09079a66d544", Caption = "Invoice Amount")]
		public override ZDecimal CIL_InvoiceValue
		{
			get => base.CIL_InvoiceValue;
			set => base.CIL_InvoiceValue = value;
		}

		[ResourceStringData("ee821073-f39d-4077-b3f2-38ac4e439f79", Caption = "Statistical Value")]
		public override ZDecimal CIL_StatisticalValue
		{
			get => base.CIL_StatisticalValue;
			set => base.CIL_StatisticalValue = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("29b548e8-0769-4ebc-acfd-c98cee0111f3", Caption = "Currency")]
		public override ZString CIL_RX_NKCurrency
		{
			get => base.CIL_RX_NKCurrency;
			set => base.CIL_RX_NKCurrency = value;
		}

		[ResourceStringData("b0fdf860-5105-4b30-824d-d994a2882197", Caption = "Mass")]
		public override ZInt CIL_MassInKilograms
		{
			get => base.CIL_MassInKilograms;
			set => base.CIL_MassInKilograms = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusIntrastatLineLookups.MassInKilogramsUnits))]
		[ResourceStringData("2d6c4f52-9c1e-4c1c-a219-db6ad1204ad8", Caption = "Mass UQ")]
		public ZString CIL_MassInKilogramsUnit => Core.Constants.Weight.Kilograms;

		public ZPropertyInfo CIL_MassInKilogramsUnitInfo => GetZPropertyInfo(nameof(CIL_MassInKilogramsUnit));

		[ResourceStringData("31f95796-1b1b-4c7c-aec6-06f59087e4d1", Caption = "Country of Origin", ShortCaption = "Origin")]
		public override ZString CIL_RN_NKCountryOfOrigin
		{
			get => base.CIL_RN_NKCountryOfOrigin;
			set => base.CIL_RN_NKCountryOfOrigin = value;
		}

		[ResourceStringData("2f682c3a-dca8-4ef5-b229-18660e03047d", Caption = "Supplementary Quantity", ShortCaption = "Supp. Qty.")]
		public override ZDecimal CIL_SupplementaryQuantity
		{
			get => base.CIL_SupplementaryQuantity;
			set => base.CIL_SupplementaryQuantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusIntrastatLineLookups.CustomsUQList))]
		[ResourceStringData("9229f46c-efbe-4755-a0ba-193b024ba7a6", Caption = "Supplementary Quantity Unit", ShortCaption = "Supp. UQ")]
		public override ZString CIL_SupplementaryQuantityUnit
		{
			get => base.CIL_SupplementaryQuantityUnit;
			set => base.CIL_SupplementaryQuantityUnit = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusIntrastatLineLookups.Regions))]
		[ResourceStringData("75d97788-1dba-4457-b096-a14d40ced7f1", Caption = "Region")]
		public override ZString CIL_Region
		{
			get => base.CIL_Region;
			set => base.CIL_Region = value;
		}

		public CusIntrastatHeader Header => header ??= Factory.Load<CusIntrastatHeader>(CIL_CIH_Header);
		CusIntrastatHeader header;

		[BusinessObjectTestExclude]
		[ResourceStringData("44338eb1-1e5e-408c-83ed-c5d06f26ad9a", Caption = "Tariff")]
		public virtual ZString CIL_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(CIL_Tariff); }
			set { CIL_Tariff = value; }
		}

		public ZPropertyInfo CIL_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CIL_FormattedTariff, x => CIL_TariffInfo); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("861d41ba-86ad-4a91-8f49-98e4cad87a9e", Caption = "Tariff")]
		public override ZString CIL_Tariff
		{
			get => base.CIL_Tariff;
			set => base.CIL_Tariff = TariffFormatter.Format(value).Left(CIL_TariffInfo.MaxLength);
		}

		public TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ??= GetNewTariffFormatter(); }
		}
		TariffFormatter tariffFormatter;

		protected virtual TariffFormatter GetNewTariffFormatter() => EU.Business.TariffFormatter.New(Header.CountryCode);

		public ZBool IsImport => Header.IsImport;

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CIL_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(CusIntrastatHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CIL_CIH_HeaderInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		#region ITariffFormatProvider

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#endregion

	}
}
