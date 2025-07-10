using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Helper = Enterprise.Customs.DE.Business.CusLineTariffDetailHelper;

namespace Enterprise.Customs.DE.Business
{
	public class CusLineTariffDetail : AutoCusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This would resolve to an abstract class which is undesirable")]
		public new partial class Schema : EU.Business.CusLineTariffDetail.Schema
		{
			public const string BZ_PercentAlcohol = nameof(CusLineTariffDetail.BZ_PercentAlcohol);
			public const string BZ_TobaccoRetailPrice = nameof(CusLineTariffDetail.BZ_TobaccoRetailPrice);
			public const string ExciseValue = nameof(CusLineTariffDetail.ExciseValue);
		}

		#region Properties

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new CusLineTariffDetailLookups Lookups => (CusLineTariffDetailLookups)base.Lookups;

		public new CusLineTariffDetailValidation Validation => (CusLineTariffDetailValidation)base.Validation;

		[ReadOnly(true)]
		[ResourceStringData("08EBBA5B-504A-42A7-BEE6-B75398C8B72F", Caption = "Part")]
		public override ZString BZ_Type
		{
			get => base.BZ_Type;
			set => base.BZ_Type = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffCollection))]
		[ResourceStringData("7BAF6AEF-A097-4216-AB81-A76652887216", Caption = "Code")]
		public override ZString BZ_Tariff
		{
			get => base.BZ_Tariff;
			set
			{
				var oldValue = BZ_Tariff;
				base.BZ_Tariff = value;
				if (!IsCopying && BZ_Tariff != oldValue)
				{
					SetTobaccoRetailPrice();
					if (!Helper.IsPercentAlcoholMandatory(this))
					{
						BZ_PercentAlcohol = 0m;
					}
					BZ_PercentAlcoholInfo.RefreshBinding();
					InvoiceLine?.JI_TobaccoStampInfo.RefreshBinding();
					BZ_UQ1 = universalTariff?.UnitsOfMeasure.Count == 1 ? universalTariff.UnitsOfMeasure[0].ZZ8_UOM : ZString.Empty;
				}
			}
		}

		[DecimalPlaces(3)]
		[ResourceStringData("25632048-6548-4924-9795-869A41F0025B", Caption = "Quantity")]
		public override ZDecimal BZ_Qty1
		{
			get => base.BZ_Qty1;
			set => base.BZ_Qty1 = value;
		}

		[ResourceStringData("03F28B75-82C4-41C4-B4FE-59F2D96506EF", Caption = "UOM", FullDescription = "Unit Of Measurement")]
		public override ZString BZ_UQ1
		{
			get => base.BZ_UQ1;
			set => base.BZ_UQ1 = value;
		}

		[ReadOnlyMember(nameof(BZ_PercentAlcohol_ReadOnly))]
		[ResourceStringData("4B94F0B2-87D5-4AAE-8531-451FEB077BA7", Caption = "Degree Percentage")]
		public ZDecimal BZ_PercentAlcohol
		{
			get => AddInfo.ZG_AlcoholicStrength;
			set => AddInfo.ZG_AlcoholicStrength = value;
		}

		public ZPropertyInfo BZ_PercentAlcoholInfo => GetWrappedZPropertyInfo(Schema.BZ_PercentAlcohol, x => AddInfo.ZG_AlcoholicStrengthInfo);

		[DecimalPlaces(6)]
		[ReadOnlyMember(nameof(BZ_TobaccoRetailPrice_ReadOnly))]
		[ResourceStringData("FFFB31AA-6E80-4AED-A3EA-59DCC7D23030", Caption = "Retail Price")]
		public ZDecimal BZ_TobaccoRetailPrice
		{
			get => base.BZ_Value / 100;
			set => base.BZ_Value = value * 100;
		}

		public ZPropertyInfo BZ_TobaccoRetailPriceInfo => GetWrappedZPropertyInfo(Schema.BZ_TobaccoRetailPrice, x => BZ_ValueInfo);

		bool BZ_TobaccoRetailPrice_ReadOnly => universalTariff != null && !IsTobaccoRelatedTariff;

		[DecimalPlaces(2)]
		[ResourceStringData("1DE52ACC-4FB3-4F9A-A832-845BE3A09550", Caption = "Excise Value")]
		public ZDecimal ExciseValue => ZArchitecture.Core.Utilities.Round(BZ_Qty1 * BZ_TobaccoRetailPrice, 2);

		public ZPropertyInfo ExciseValueInfo => GetZPropertyInfo(Schema.ExciseValue);
		#endregion

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups() => new CusLineTariffDetailLookups(this);

		protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation() => new CusLineTariffDetailValidation(this);

		protected bool BZ_PercentAlcohol_ReadOnly => universalTariff != null && !Helper.IsPercentAlcoholMandatory(this);

		public bool IsTobaccoRelatedTariff => Factory.GetValue(ref isTobaccoRelatedTariffCached, () => universalTariff?.HasAttribute(Helper.AttributeName, Helper.ExciseTypes._10) ?? false);
		CachedProperty<bool> isTobaccoRelatedTariffCached;

		void SetTobaccoRetailPrice() => BZ_TobaccoRetailPrice = IsTobaccoRelatedTariff ? Factory.GetTobaccoRetailSellingPriceOrZero(universalTariff.ZZ1_ZZ8_UQ1) : ZDecimal.Zero;

		TariffView universalTariff => Factory.GetValue(ref universalTariffCached, () => UniversalTariff);
		CachedProperty<TariffView> universalTariffCached;
	}
}
