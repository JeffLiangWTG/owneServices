using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRate))]
	sealed class CACTaxRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTaxTypes()
		{
			//TaxType values is in db constraint
			AssertEquals("GST", DutyAndTaxTypes.Codes.GST, CACTaxRate.TaxType.GST);
			AssertEquals("Excise Tax", DutyAndTaxTypes.Codes.ExciseTax, CACTaxRate.TaxType.Excise);
		}

		public void TestLoader()
		{
			var taxRate = Factory.New<CACTaxRate>();
			taxRate.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			taxRate.ZH_TaxRefNumber = "123";
			taxRate.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			taxRate.ZH_ExpiryDate = ZDateTime.Today.AddDays(+1);

			taxRate = Factory.New<CACTaxRate>();
			taxRate.ZH_TaxType = DutyAndTaxTypes.Codes.GST;
			taxRate.ZH_TaxRefNumber = "123";
			taxRate.ZH_EffectiveDate = ZDateTime.Today.AddDays(-2);
			taxRate.ZH_ExpiryDate = ZDateTime.Today.AddDays(+1);

			taxRate = Factory.New<CACTaxRate>();
			taxRate.ZH_TaxType = DutyAndTaxTypes.Codes.GST;
			taxRate.ZH_TaxRefNumber = "123";
			taxRate.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			taxRate.ZH_ExpiryDate = ZDateTime.Today.AddDays(+1);

			AssertEquals("CACTaxRate", taxRate, CACTaxRate.Load(Factory, "123", DutyAndTaxTypes.Codes.GST, ZDateTime.Today));

			AssertEquals("CACTaxRate", taxRate, CACTaxRate.Load(Factory, "123", DutyAndTaxTypes.Codes.GST, new ZDateTime(" ")));
			string expectedErrorMessage = "You should input a valid effectiveDate";
			AssertContains("An error should be thrown", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("CACTaxRate", taxRate, CACTaxRate.Load(Factory, "123", DutyAndTaxTypes.Codes.GST, ZDateTime.Empty));
			AssertContains("An error should be thrown", expectedErrorMessage, ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Should not report error.", 0, ErrorReporter.TotalErrorCount);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.New<CACTaxRate>();
			rate.ZH_TaxType = CACTaxRate.TaxType.GST;
			return rate;
		}
	}
}
