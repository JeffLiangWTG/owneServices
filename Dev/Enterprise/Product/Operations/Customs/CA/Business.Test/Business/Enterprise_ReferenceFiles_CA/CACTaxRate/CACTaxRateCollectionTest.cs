using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRateCollection))]
	sealed class CACTaxRateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadCollectionWithEffectiveDate()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1234567890";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_ExciseTaxRefNumber = "XXX";
			var refNum2 = refNumHeader.RefNumbers.AddNew();
			refNum2.ZE_ExciseTaxRefNumber = "YYY";

			var rate1 = Factory.New<CACTaxRate>();
			rate1.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate1.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate1.ZH_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate1.ZH_TaxRefNumber = "XXX";
			rate1.ZH_RateType = RateTypes.Codes.AdValorem;
			rate1.ZH_Rate = 10;

			var rate2 = Factory.New<CACTaxRate>();
			rate2.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate2.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate2.ZH_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate2.ZH_TaxRefNumber = "YYY";
			rate2.ZH_RateType = RateTypes.Codes.Specific;
			rate2.ZH_Rate = 100;

			var rate3 = Factory.New<CACTaxRate>();
			rate3.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate3.ZH_EffectiveDate = ZDateTime.Today.AddDays(-2);
			rate3.ZH_ExpiryDate = ZDateTime.Today.AddDays(-1);
			rate3.ZH_TaxRefNumber = "ZZZ";
			rate3.ZH_RateType = RateTypes.Codes.Specific;
			rate3.ZH_Rate = 100;

			var result = new CACTaxRateCollection(refNumHeader, ZDateTime.Now, CACTaxRate.TaxType.Excise);
			result.Load();
			AssertEquals(2, result.Count);
			Assert(result.OfType<CACTaxRate>().Any(x => x.ZH_TaxRefNumber == "XXX"));
			Assert(result.OfType<CACTaxRate>().Any(x => x.ZH_TaxRefNumber == "YYY"));
			Assert(!result.OfType<CACTaxRate>().Any(x => x.ZH_TaxRefNumber == "ZZZ"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1234567890";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);

			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			return new CACTaxRateCollection(refNumHeader, ZDateTime.Now, CACTaxRate.TaxType.Excise);
		}
	}
}
