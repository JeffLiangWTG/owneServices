using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AdditionalTariffLookupsTest : TestCaseWithFactory
	{
		public void TestTariffTypeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var invoiceLine = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var parent = invoiceLine.AdditionalTariffs.AddNew();
			var tariffTypeList = parent.Lookups.TariffTypeList;
			CombineAssertions("Subject is empty", () =>
			{
				AssertEquals(6, tariffTypeList.Count);
				AssertEquals("LEBIT, LETEC, COVID, BIT, BK, IPI", tariffTypeList.CodesAsString);
			});

			parent.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			tariffTypeList = parent.Lookups.TariffTypeList;
			CombineAssertions("Subject = 1", () =>
			{
				AssertEquals(5, tariffTypeList.Count);
				AssertEquals("LEBIT, LETEC, COVID, BIT, BK", tariffTypeList.CodesAsString);
			});

			parent.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			tariffTypeList = parent.Lookups.TariffTypeList;
			CombineAssertions("Subject = 2", () =>
			{
				AssertEquals(1, tariffTypeList.Count);
				AssertEquals("IPI", tariffTypeList.CodesAsString);
			});

			parent.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			tariffTypeList = parent.Lookups.TariffTypeList;
			CombineAssertions("Subject = 3", () =>
			{
				AssertEquals(4, tariffTypeList.Count);
				AssertEquals("AR99, ASGPC, CO99, MX99", tariffTypeList.CodesAsString);
			});
		}

		public void TestChildTariffs()
		{
			var invoiceLine = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var collection = invoiceLine.AdditionalTariffs;
			var parent = collection.AddNew();

			AssertEquals("Type of ChildTariff should be", typeof(ChildTariffViewCollection), parent.Lookups.ChildTariffs.GetType());

			var filterCode = parent.Lookups.ChildTariffs.FilterBusinessObjectDefaults["Tariff Code" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull("filter: Tariff Code added", filterCode);

			ZInt instances = 0;
			foreach (var filterType in parent.Lookups.ChildTariffs.FilterBusinessObjectDefaults)
			{
				var filter = ((FilterBusinessObjectDefault)filterType).FilterName;
				if (filter.Contains("Tariff Type"))
				{
					instances++;
				}
			}

			AssertEquals("Should have more than 2 Tariff Type", true, instances > 2);

			invoiceLine.JI_Tariff = "09022000";
			parent.TariffType = ChildTariffTypeList.Codes.LEBIT;

			var filterType1 = parent.Lookups.ChildTariffs.FilterBusinessObjectDefaults["Tariff Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"];
			AssertNotNull("filter: Tariff Type added", filterType1);

			var filterType2 = parent.Lookups.ChildTariffs.FilterBusinessObjectDefaults["Tariff Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"];
			AssertNotNull("filter: Tariff Type added", filterType2);
		}

		public void TestLegalActSubjectList()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var additionalTariff1 = invoiceLine.AdditionalTariffs.AddNew();
			AssertEquals("The codes should be", "1, 2, 3", additionalTariff1.Lookups.LegalActSubjectList.CodesAsString);

			additionalTariff1.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			AssertEquals("The codes should be", "1, 2, 3", additionalTariff1.Lookups.LegalActSubjectList.CodesAsString);

			var additionalTariff2 = invoiceLine.AdditionalTariffs.AddNew();
			AssertEquals("The codes should be", "1, 2", additionalTariff2.Lookups.LegalActSubjectList.CodesAsString);

			var pivot = Factory.New<CusClassPartPivot>();
			var additionalTariff3 = pivot.AdditionalTariffs.AddNew();
			AssertEquals("The codes should be", "1, 2", additionalTariff3.Lookups.LegalActSubjectList.CodesAsString);
		}
	}
}
