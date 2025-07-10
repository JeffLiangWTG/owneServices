using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(CalloutCharge))]
	internal class CalloutChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		public void TestAmount()
		{
			Charge.TaxableAmount = 150m;
			Charge.NonTaxableAmount = 200m;
			AssertEquals(350m, Charge.Amount);
		}

		public void TestNonTaxableAmount()
		{
			Charge.JR_OSCostAmt = 20m;
			AssertEquals(20m, Charge.NonTaxableAmount);
			Charge.NonTaxableAmount = 25m;
			AssertEquals(25m, Charge.NonTaxableAmount);
			AssertEquals(25m, Charge.JR_OSCostAmt);
			AssertEquals(Charge.JR_OSCostAmtInfo, ((ZWrappedPropertyInfo)Charge.NonTaxableAmountInfo).InnerInfo);
		}

		public void TestTaxableAmount()
		{
			Charge.JR_OSSellAmt = 20m;
			AssertEquals(20m, Charge.TaxableAmount);
			Charge.TaxableAmount = 25m;
			AssertEquals(25m, Charge.TaxableAmount);
			AssertEquals(25m, Charge.JR_OSSellAmt);
			AssertEquals(Charge.JR_OSSellAmtInfo, ((ZWrappedPropertyInfo)Charge.TaxableAmountInfo).InnerInfo);
		}

		public void TestDiscount()
		{
			Charge.JR_LocalCostAmt = 20m;
			AssertEquals(20m, Charge.Discount);
			Charge.Discount = 25m;
			AssertEquals(25m, Charge.Discount);
			AssertEquals(25m, Charge.JR_LocalCostAmt);
			AssertEquals(Charge.JR_LocalCostAmtInfo, ((ZWrappedPropertyInfo)Charge.DiscountInfo).InnerInfo);
		}

		public void TestGSTAmount()
		{
			var auTaxRecord = GetTaxRateByCountry(Core.Constants.CountryCodes.Australia);
			var auRate = auTaxRecord.GetRate_ForTestOnly() / 100m;

			var sgTaxRecord = GetTaxRateByCountry(Core.Constants.CountryCodes.Singapore);
			var sgRate = sgTaxRecord.GetRate_ForTestOnly() / 100m;

			Charge.TaxableAmount = 100;
			AssertEquals("GST Amount", auRate * 100m, Charge.GSTAmount);
			Charge.TaxableAmount = 260;
			AssertEquals("GST Amount", auRate * 260m, Charge.GSTAmount);

			Charge.JR_SellTaxDate = ZDate.Today.AddDays(-1);
			auRate = auTaxRecord.GetRate(Charge.JR_SellTaxDate) / 100m;
			AssertEquals("GST Amount", auRate * 260, Charge.GSTAmount);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				Charge.JR_SellTaxDate = ZDate.Today; //this will trigger the call to ResetRate()
				Charge.TaxableAmount = 100;
				AssertEquals("GST Amount", sgRate * 100m, Charge.GSTAmount);

				Charge.TaxableAmount = 260;
				AssertEquals("GST Amount", sgRate * 260m, Charge.GSTAmount);

				Charge.JR_SellTaxDate = ZDate.Today.AddDays(-1);
				sgRate = sgTaxRecord.GetRate(Charge.JR_SellTaxDate) / 100m;
				AssertEquals("GST Amount", sgRate * 260, Charge.GSTAmount);
			}
		}

		public void TestNettAmount()
		{
			Charge.JR_LocalSellAmt = 20m;
			AssertEquals(20m, Charge.NettAmount);
			Charge.NettAmount = 25m;
			AssertEquals(25m, Charge.NettAmount);
			AssertEquals(25m, Charge.JR_LocalSellAmt);
			AssertEquals(Charge.JR_LocalSellAmtInfo, ((ZWrappedPropertyInfo)Charge.NettAmountInfo).InnerInfo);
		}

		AccTaxRate GetTaxRateByCountry(ZString countryCode)
		{
			var taxFilter = new ZQuery(AccTaxRateSchema.AT_Code, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			taxFilter.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, countryCode);
			return Factory.LoadTop1<AccTaxRate>(taxFilter);
		}
		CalloutCharge Charge
		{
			get
			{
				if (fCharge == null)
				{
					fCharge = Factory.New<CalloutCharge>();
				}

				return fCharge;
			}
		}

		CalloutCharge fCharge;
	}
}
