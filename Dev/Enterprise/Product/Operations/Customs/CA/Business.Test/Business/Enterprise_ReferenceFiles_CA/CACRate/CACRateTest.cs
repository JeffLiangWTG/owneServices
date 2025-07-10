using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACRate))]
	sealed class CACRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var rate = Factory.New<CACRate>();
			AssertEquals("RateLines", typeof(CACRateLineCollection), rate.RateLines.GetType());
		}

		public void TestParent()
		{
			var rateHeader = Factory.New<CACRateHeader>();
			rateHeader.ZB_UnitOfMeasure = "KG";
			var rate = rateHeader.Rates.AddNew();
			AssertEquals("Parent", rateHeader, rate.Parent);
			AssertEquals("EffectiveUnitOfMeasure from Parent", "KG", rate.EffectiveUnitOfMeasure);

			var tariffHeader = Factory.New<CACTariffHeader>();
			rate = tariffHeader.Rates.AddNew();
			AssertEquals("Parent", tariffHeader, rate.Parent);
			AssertEquals("EffectiveUnitOfMeasure from Parent", "", rate.EffectiveUnitOfMeasure);
		}

		public void TestLoader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_EffectiveDate = ZDateTime.Now.AddDays(-10);
			classHeader.ZA_ExpiryDate = ZDateTime.Now.AddDays(10);
			classHeader.ZA_ClassificationNumber = "1212121212";
			classHeader.ZA_AreaCode = "XXX";

			var rateHeader = classHeader.ClassRates.AddNew();
			rateHeader.ZB_EffectiveDate = ZDateTime.Now.AddDays(-5);
			rateHeader.ZB_ExpiryDate = ZDateTime.Now.AddDays(5);

			var rate = rateHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = "12";

			rate = rateHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = "22";

			Factory.Save();

			AssertEquals("CACRate", rate, CACRate.Load(rateHeader, "22"));

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertNull(CACRate.Load(invoiceLine));

			invoiceLine.JI_Tariff = "12.12.12.12.12";
			AssertNull(CACRate.Load(invoiceLine));

			invoiceLine.CA_TreatmentCode = "33";
			AssertNull(CACRate.Load(invoiceLine));

			invoiceLine.CA_TreatmentCode = ZString.Empty;
			declaration.Invoices[0].CA_TreatmentCode = "12";
			AssertNotNull(CACRate.Load(invoiceLine));

			invoiceLine.CA_TreatmentCode = "22";
			AssertNotNull(CACRate.Load(invoiceLine));

			declaration.CA_EstReleaseDate = ZDateTime.Now.AddDays(-4);
			AssertNotNull(CACRate.Load(invoiceLine));

			declaration.CA_EstReleaseDate = ZDateTime.Now.AddDays(-6);
			AssertNull(CACRate.Load(invoiceLine));

			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-4);
			var cacRate01 = CACRate.Load(invoiceLine);
			AssertNotNull(cacRate01);

			var cacRate02 = CACRate.Load(invoiceLine.JI_Tariff, invoiceLine.EffectiveTreatmentCode, invoiceLine.EffectiveDateForDutyRate, invoiceLine.Factory);
			AssertSame(cacRate01, cacRate02);

			var newFactory = new BusinessObjectFactory();
			var cacRate03 = CACRate.Load(invoiceLine.JI_Tariff, invoiceLine.EffectiveTreatmentCode, invoiceLine.EffectiveDateForDutyRate, newFactory);
			AssertNotSame(cacRate01, cacRate03);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(-6);
			AssertNull(CACRate.Load(invoiceLine));
		}

		public void TestLoaderLatestWhenMoreThanOneRateHeader()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_EffectiveDate = ZDateTime.Now.AddDays(-10);
			classHeader.ZA_ExpiryDate = ZDateTime.Now.AddDays(10);
			classHeader.ZA_ClassificationNumber = "1212121212";
			classHeader.ZA_AreaCode = "XXX";

			var rateHeader = classHeader.ClassRates.AddNew();
			rateHeader.ZB_EffectiveDate = ZDateTime.Now.AddDays(-5);
			rateHeader.ZB_ExpiryDate = ZDateTime.Now.AddDays(5);

			var rateHeader2 = classHeader.ClassRates.AddNew();
			rateHeader2.ZB_EffectiveDate = ZDateTime.Now.AddDays(-1);
			rateHeader2.ZB_ExpiryDate = ZDateTime.Now.AddDays(5);

			var rate = rateHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = "12";

			var rate2 = rateHeader2.Rates.AddNew();
			rate2.ZC_TreatmentCode = "22";

			Factory.Save();

			AssertEquals("CACRate", rate, CACRate.Load("1212121212", "12", ZDateTime.Today, Factory));
			AssertEquals("CACRate", rate2, CACRate.Load("1212121212", "22", ZDateTime.Today, Factory));
		}
	}
}
