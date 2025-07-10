using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestShipperCountriesCodeList()
		{
			CountryListSetUp();

			var codeList = (ZZRefCusCodeListCombinedCollection)bill.Lookups.ShipperCountries;

			AssertCountryList(codeList);
		}

		public void TestConsigneeCountriesCodeList()
		{
			CountryListSetUp();

			var codeList = (ZZRefCusCodeListCombinedCollection)bill.Lookups.ConsigneeCountries;

			AssertCountryList(codeList);
		}

		AsycudaBill bill;

		void CountryListSetUp()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI008", "AU", "Australia", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI008", "IE", "Ireland", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC40I", "325", "Proforma invoice", yesterday, tomorrow);
			Factory.Save();
		}

		void AssertCountryList(ZZRefCusCodeListCombinedCollection codeList)
		{
			codeList.Load();
			var codes = codeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
			CombineAssertions(() =>
			{
				AssertEquals("There are two countries in the list", 2, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "AU", "IE" }, codes);
			});
		}

		public void TestSubStyleList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.SubStyleList;

			CombineAssertions(() =>
			{
				AssertEquals("There are two elements in the list", 2, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "A", "D" }, codeList.GetAllCodes());
			});
		}

		public void TestImporterIdentificationTypeList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.ImporterIdentificationTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("There are four elements in the list", 4, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "CGT", "EOR", "ITX", "PYE" }, codeList.GetAllCodes());
			});
		}

		public void TestBillStatusList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.BillStatusList;

			CombineAssertions(() =>
			{
			AssertEquals("There are eighteen elements in the list", 18, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ACC", "AMR", "AMA", "SUP", "CAR", "CAN", "CON", "GEN", "INS", "INV", "NOT", "PRE", "RAA", "RAJ", "RAR", "REG", "REJ", "REL" }, codeList.GetAllCodes());
			});
		}

		public void TestMessageStatusList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.MessageStatusList;

			CombineAssertions(() =>
			{
				AssertEquals("There are six elements in the list", 6, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ACC", "ACK", "ERR", "FAL", "INV", "SNT" }, codeList.GetAllCodes());
			});
		}
	}
}
