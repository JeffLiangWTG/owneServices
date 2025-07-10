using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportTaxTreatmentsProviderTest : TestCaseWithFactory
	{
		[TestDate(2024, 12, 28)]
		public void TestProperties()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("BR", "105")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2024, 12, 26);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;

			var provider = new ImportTaxTreatmentsProvider(invoiceLine);

			AssertValues(provider, 105, new DateTime(2024, 12, 26));

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Cameroon;
			invoiceHeader.ExchangeRateDate = ZDateTime.Empty;
			provider = new ImportTaxTreatmentsProvider(invoiceLine);

			AssertValues(provider, 0, new DateTime(2024, 12, 28));
		}

		void AssertValues(ImportTaxTreatmentsProvider provider, int countryCode, DateTime date)
		{
			CombineAssertions(() =>
			{
				AssertEquals("NCM should be", "01010101", provider.Ncm);
				AssertEquals("CountryCodeshould be", countryCode, provider.CountryCode);
				AssertEquals("TaxEventDateshould be", date, provider.TaxEventDate);
				AssertEquals("OperationType should be", Constants.OperationType.Import, provider.OperationType);
				AssertNull(provider.OptionalLegalBasisList);
			});
		}
	}
}
