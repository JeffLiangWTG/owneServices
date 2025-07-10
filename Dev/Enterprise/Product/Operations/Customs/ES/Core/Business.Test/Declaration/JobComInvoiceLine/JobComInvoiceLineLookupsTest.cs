using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestStateIslandCodesList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
			helper.CreateCusCodeListTerritory(countryCode, "02", "Test 2");
			helper.CreateCusCodeListTerritory(countryCode, "03", "Test 3");
			helper.CreateCusCodeListTerritory(countryCode, "04", "Test 4");
			helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "05", "Test 5");
			helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "06", "Test 6");
			helper.CreateCusCodeListCanaryIsland(countryCode, "07", "Test 7");
			helper.CreateCusCodeListCanaryIsland(countryCode, "08", "Test 8");
			helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "09", "Test 9", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var lookups = invoiceLine.Lookups.StateIslandCodesList;
				AssertEquals("Number of codes in list", 8, lookups.Count);
				Assert(lookups.ContainsCode("01"));
				Assert(lookups.ContainsCode("02"));
				Assert(lookups.ContainsCode("03"));
				Assert(lookups.ContainsCode("04"));
				Assert(lookups.ContainsCode("05"));
				Assert(lookups.ContainsCode("06"));
				Assert(lookups.ContainsCode("07"));
				Assert(lookups.ContainsCode("08"));
				AssertEquals("Test 1", lookups.GetDescriptionFromCode("01"));

				AssertSame("Should be cached", lookups, invoiceLine.Lookups.StateIslandCodesList);
			}
		}

		public void TestTaxOrFeeCodeList()
		{
			var countryCode = Core.Constants.CountryCodes.Spain;
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("IV1", 0.21, countryCode);
			helper.CreateTaxOrFee("IV2", 0.21, countryCode);
			helper.CreateTaxOrFee("IG3", 0.21, countryCode);
			var tariffType = helper.CreateTariffType(countryCode, "IMP");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV1");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV2");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IG3");
			Factory.Save();

			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			CombineAssertions(() =>
			{
				AssertEquals("IG3, EX", invoiceLine.Lookups.TaxOrFeeCodeList.CodesAsString);

				declaration.ZG_DestinationState = "ZZ";
				AssertEquals("IV1, IV2, EX", invoiceLine.Lookups.TaxOrFeeCodeList.CodesAsString);

				invoiceLine.JI_Tariff = ZString.Empty;
				AssertEquals("EX", invoiceLine.Lookups.TaxOrFeeCodeList.CodesAsString);

				invoiceLine.JI_Tariff = "2222";
				AssertEquals("EX", invoiceLine.Lookups.TaxOrFeeCodeList.CodesAsString);
			});
		}
	}
}
