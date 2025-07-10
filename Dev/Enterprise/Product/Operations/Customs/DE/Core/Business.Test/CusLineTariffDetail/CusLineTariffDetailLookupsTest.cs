using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, "EXC");
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tariffDetail = invLine.CusLineTariffDetails.AddNew();

			var tariffTypeList = tariffDetail.Lookups.TariffTypeList;
			AssertEquals("EXC", string.Join(",", tariffTypeList.Cast<RefCusTariffType>().Select(x => x.ZZI_TariffType).OrderBy(x => x)));
		}

		public void TestQuantityUnitList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euGrouping);

			const string customsUq = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ;
			helper.CreateCusCodeType(customsUq, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, customsUq, "UM1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, customsUq, "UM2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, customsUq, "UM3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, customsUq, "UM4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var quantityUnitList = tariffDetail.Lookups.QuantityUnitList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List should only contain DE CUSUQ codes", new[] { "UM2", "UM3", "UM4" }, ((CodeDescriptionPairList)quantityUnitList).GetAllCodes());
				AssertSame("Cached", quantityUnitList, tariffDetail.Lookups.QuantityUnitList);
			});
		}

		public void TestTariffCollection()
		{
			var de = Core.Constants.CountryCodes.Germany;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(de, "Germany", euGrouping);
			Factory.Save();

			var tariffTypeEXC = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Excise);
			var tariffTypeIMP = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var tariffTypeRandom = helper.CreateTariffType(de, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Levies);

			Factory.Save();

			helper.CreateTariff(de, tariffTypeEXC.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff1 = helper.CreateTariff(de, tariffTypeEXC.PK, "1111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(de, tariffTypeEXC.PK, "2222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff3 = helper.CreateTariff(de, tariffTypeEXC.PK, "3333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff4 = helper.CreateTariff(de, tariffTypeRandom.PK, "4444", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff5 = helper.CreateTariff(de, tariffTypeEXC.PK, "5555", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			helper.CreateTariffRelationship(tariff1.PK, tariffTypeIMP.PK, "08091998");
			helper.CreateTariffRelationship(tariff2.PK, tariffTypeIMP.PK, "0809");
			helper.CreateTariffRelationship(tariff3.PK, tariffTypeIMP.PK, "5454");
			helper.CreateTariffRelationship(tariff4.PK, tariffTypeIMP.PK, "08091998");
			helper.CreateTariffRelationship(tariff5.PK, tariffTypeRandom.PK, "08091998");

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var tariffDetail = invLine.CusLineTariffDetails.AddNew();

			var collection = tariffDetail.Lookups.TariffCollection;
			collection.Load();
			AssertEquals(0, collection.Count);

			invLine.JI_Tariff = "08091998";
			collection = tariffDetail.Lookups.TariffCollection;
			var filter = collection.CompleteFilter;
			AssertEquals("Tariff 1111 should be a part of the collection, as it has the whole tariff code", true, tariff1.MatchesFilter(filter));
			AssertEquals("Tariff 2222 should be a part of the collection, as it has part of the tariff code", true, tariff2.MatchesFilter(filter));
			AssertEquals("Tariff 3333 shouldn't be a part of the collection, as it does not have the correct tariff code ('5454')", false, tariff3.MatchesFilter(filter));
			AssertEquals("Tariff 4444 shouldn't be a part of the collection, as the relatedTariff is not of type 'EXC'", false, tariff4.MatchesFilter(filter));
			AssertEquals("Tariff 5555 shouldn't be a part of the collection, as the tariffType is not of type 'IMP'", false, tariff5.MatchesFilter(filter));
		}
	}
}
