using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class TaxItemProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(TaxItemProvider.New(null));
			AssertType<TaxItemProvider>(TaxItemProvider.New(Factory.New<DuimpTaxRegime>()));
		}

		public void TestProperties()
		{
			var codes = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("DTY", "II") };
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.RateType, "BR Rate Types", codes);
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "12345678";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;

			var profile = invoiceLine.GetRequiredTTProfiles().First(s => s.LegalCode == "P01");
			var taxRegime = invoiceLine.DuimpTaxRegimes.AddNew(profile);

			var dataProvider = TaxItemProvider.New(taxRegime);
			CombineAssertions(() =>
			{
				AssertEquals("TaxCode", "II", dataProvider.TaxCode);
				AssertEquals("TaxRegimeCode", 1, dataProvider.TaxRegimeCode);
				AssertEquals("LegalBasisCode", 1, dataProvider.LegalBasisCode);
			});
		}

		public void TestAttributes()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "12345678";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				invoiceLine.DuimpLegalBase = "P01";
				invoiceLine.AddDuimpTaxRegimes();
				invoiceLine.DuimpLegalBase = "P06";
				invoiceLine.AddDuimpTaxRegimes();

				var dataProvider = TaxItemProvider.New(invoiceLine.DuimpTaxRegimes[0]);
				AssertContainsExactElementsInAnyOrder(["ATT1", "ATT1_1"], dataProvider.Attributes.Select(x => x.Code));

				dataProvider = TaxItemProvider.New(invoiceLine.DuimpTaxRegimes[1]);
				AssertContainsExactElementsInAnyOrder(["ATT10", "ATT10_1", "ATT10_2", "ATT11", "ATT11_1", "ATT11_11"], dataProvider.Attributes.Select(x => x.Code));
			}
		}
	}
}
