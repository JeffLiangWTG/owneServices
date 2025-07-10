using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase5LookupsTest : BusinessObjectValidationTestCase
	{
		public void TestExciseCodeList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var expTariffType = helper.CreateTariffType(countryCode, "EXP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var zTariffType = helper.CreateTariffType(countryCode, "ZZZZ");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var tariffExcise2 = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "1PL", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var tariffExcise3 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A7", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc3");
			var tariffExcise4 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc4");
			var tariffExcise5 = helper.LoadOrCreateNewTariff(countryCode, zTariffType.PK, "NoExcise", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc5");
			var tariffExcise6 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "1CF", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc6");

			helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExcise2.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExcise3.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExcise4.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExcise5.PK, expTariffType.PK, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariffExcise6.PK, expTariffType.PK, tariff.ZZ1_TariffCode);

			var tariffWithoutExcises = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "33332222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();

			nctsHeader.MovementHeader.CustomsOfficesForDeparture.RemoveAndDeleteAll();
			var customsOfficeForDeparture = nctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
			customsOfficeForDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOfficeForDeparture.CY_Data = "ES0035";

			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = tariff.ZZ1_TariffCode;
				var list = goodsItem.ESDepartureCargoDescLookups.ExciseCodeList;
				AssertEquals("ExciseCodeList Contains codes 0A7 and 0A3 for tariff and canary island in customs office departure", "0A7 - desc3\r\n0A3 - desc4", list.ElementsAsString);

				goodsItem.BY_HarmonisedTariff = ZString.Empty;
				AssertEquals("Empty ExciseCodeList when the tariff is empty", ZString.Empty, goodsItem.ESDepartureCargoDescLookups.ExciseCodeList.ElementsAsString);

				goodsItem.BY_HarmonisedTariff = tariffWithoutExcises.ZZ1_TariffCode;
				AssertEquals("Empty ExciseCodeList when InvoiceLine Tariff has not child Excise Tariff", ZString.Empty, goodsItem.ESDepartureCargoDescLookups.ExciseCodeList.ElementsAsString);

				goodsItem.BY_HarmonisedTariff = tariff.ZZ1_TariffCode;
				customsOfficeForDeparture.CY_Data = "ES0008";
				AssertEquals("ExciseCodeList Contains codes 0A0 for tariff and not canary island in customs office departure", "0A0 - desc1", goodsItem.ESDepartureCargoDescLookups.ExciseCodeList.ElementsAsString);
			});
		}
	}
}
