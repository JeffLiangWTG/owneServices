using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutCharge : AutoJobCharge
	{
		#region Schema

		public new class Schema : AutoJobCharge.Schema
		{
			public const string Amount = "Amount";
			public const string NonTaxableAmount = "NonTaxableAmount";
			public const string TaxableAmount = "TaxableAmount";
			public const string Discount = "Discount";
			public const string NettAmount = "NettAmount";
			public const string GSTAmount = "GSTAmount";
		}

		#endregion

		public CalloutCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal Amount
		{
			get { return TaxableAmount + NonTaxableAmount; }
		}

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		public ZDecimal NonTaxableAmount
		{
			get { return JR_OSCostAmt; }
			set { JR_OSCostAmt = value; }
		}

		public ZPropertyInfo NonTaxableAmountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NonTaxableAmount), x => JR_OSCostAmtInfo); }
		}

		public ZDecimal TaxableAmount
		{
			get { return JR_OSSellAmt; }
			set { JR_OSSellAmt = value; }
		}

		public ZPropertyInfo TaxableAmountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TaxableAmount), x => JR_OSSellAmtInfo); }
		}

		public ZDate TaxDate
		{
			get { return JR_SellTaxDate; }
			set { JR_SellTaxDate = value; }
		}

		public ZPropertyInfo TaxDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TaxDateInfo), x => JR_SellTaxDateInfo); }
		}

		public ZDecimal Discount
		{
			get { return JR_LocalCostAmt; }
			set { JR_LocalCostAmt = value; }
		}

		public ZPropertyInfo DiscountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(Discount), x => JR_LocalCostAmtInfo); }
		}

		public ZDecimal NettAmount
		{
			get { return JR_LocalSellAmt; }
			set { JR_LocalSellAmt = value; }
		}

		public ZPropertyInfo NettAmountInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NettAmount), x => JR_LocalSellAmtInfo); }
		}

		public ZDecimal GSTAmount
		{
			get { return TaxableAmount * TaxRate; }
		}

		public ZPropertyInfo GSTAmountInfo
		{
			get { return GetZPropertyInfo(nameof(GSTAmount)); }
		}

		ZDecimal TaxRate
		{
			get
			{
				if (fTaxRate.IsEmpty)
				{
					ZQuery taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
					taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.Country.Code);
					AccTaxRate gSTTaxRate = Factory.LoadTop1<AccTaxRate>(taxFilter);
					fTaxRate = gSTTaxRate.GetRate(TaxDate) / 100m;
					JR_SellTaxDateInfo.ValueChanged += (sender, e) => ResetRate();
				}
				return fTaxRate;
			}
		}

		ZDecimal fTaxRate;

		void ResetRate() => fTaxRate = ZDecimal.Zero;

		public JobConsolCost ParentConsolCost
		{
			get { return Factory.Load<JobConsolCost>(JR_E6); }
		}

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlacesOfSupply))]
		public override ZString JR_CostPlaceOfSupply { get => base.JR_CostPlaceOfSupply; set => base.JR_CostPlaceOfSupply = value; }

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlaceOfSupplyTypes))]
		public override ZString JR_CostPlaceOfSupplyType { get => base.JR_CostPlaceOfSupplyType; set => base.JR_CostPlaceOfSupplyType = value; }

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlacesOfSupply))]
		public override ZString JR_SellPlaceOfSupply { get => base.JR_SellPlaceOfSupply; set => base.JR_SellPlaceOfSupply = value; }

		[List(nameof(Lookups) + "." + nameof(JobChargeLookups.PlaceOfSupplyTypes))]
		public override ZString JR_SellPlaceOfSupplyType { get => base.JR_SellPlaceOfSupplyType; set => base.JR_SellPlaceOfSupplyType = value; }
	}
}
