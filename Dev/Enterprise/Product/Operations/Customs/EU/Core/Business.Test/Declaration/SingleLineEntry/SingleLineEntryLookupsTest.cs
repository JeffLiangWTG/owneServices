using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class SingleLineEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var singleLineEntry = new SingleLineEntry(declaration);

			var newCur = Factory.NewWithValidTestData<RefCurrency>();
			newCur.RX_Code = "ABC";

			AssertEquals(expected: true, singleLineEntry.Lookups.Currencies.Any(x => x.RX_Code == "ABC"));
		}

		public void TestTariffsShouldReferUniversalTariffCollection()
		{
			var testTariffCode = "ABC";
			var declaration = Factory.New<JobDeclaration>();
			var singleLineEntry = new SingleLineEntry(declaration);
			var dataGrouping = declaration.GetDefaultDataGroupingCode();
			var tariffType = TariffFormatter.GetTariffType(declaration.IsExport);

			var testTariffExists = singleLineEntry.Lookups.Tariffs.Any(x => ((TariffView)x).ZZ1_TariffCode == testTariffCode);
			AssertEquals($"Tariff code {testTariffCode} should not exists.", expected: false, testTariffExists);

			var sourceTariffViewCollection = TariffViewCollection.GetCachedCollection(Factory, dataGrouping, tariffType, ZDateTime.Today);

			var tariffView1 = sourceTariffViewCollection.AddNew();
			tariffView1.ZZ1_TariffCode = testTariffCode;

			testTariffExists = singleLineEntry.Lookups.Tariffs.Any(x => ((TariffView)x).ZZ1_TariffCode == testTariffCode);
			AssertEquals($"Tariff code {testTariffCode} should exists.", expected: true, testTariffExists);
		}

		public void TestCPCListShouldBeCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			var singleLineEntry = new SingleLineEntry(declaration);

			var firstCPCList = singleLineEntry.Lookups.CPCList;
			var secondCPCList = singleLineEntry.Lookups.CPCList;

			AssertEquals("First list should be the same as in Lookups", firstCPCList, singleLineEntry.Lookups.CPCList);
			AssertEquals("First list should be the same as the second list", firstCPCList, secondCPCList);
		}
	}
}
