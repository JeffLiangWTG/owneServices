using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsUQListForSupplement()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Ireland))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
				helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
				helper.CreateCusCodeList("IE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
				helper.CreateCusCodeList("IE", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
				Factory.Save();

				var header = Factory.New<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				var packedItem = bill.PackedItems.AddNew();
				var list = packedItem.Lookups.CustomsUQListForSupplement;

				CombineAssertions(() =>
				{
					AssertEquals(2, list.Count);
					AssertEquals("CodesAsString", "DEF, GHI", list.CodesAsString);
				});
			}
		}

		public void TestPackTypeListCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			AssertContainsExactElementsInAnyOrder(expectedPackTypeListCodes, packedItem.Lookups.PackTypeList.GetAllCodes());
		}

		readonly string[] expectedPackTypeListCodes = new[] { "BAG", "BBG", "BBK", "BLC", "BLU", "BND", "BOT", "BOX", "BSK", "CAS", "CC", "CF", "CI", "CM", "CM2", "COI", "CRD", "CRT", "CTN",
			"CY", "CYL", "D3", "DOZ", "DRM", "DT", "ENV", "F2", "FT", "G", "GA", "GI", "GRS", "HG", "I2", "IN", "KEG", "KG", "KM", "KT", "L", "LA", "LB", "LT", "M", "M2", "M3", "MC", "MG",
			"MI", "MIX", "ML", "MM", "MM2", "NMB", "NO", "OT", "OZ", "PAI", "PCE", "PKG", "PLT", "REL", "RLL", "ROR", "SHT", "SKD", "SPL", "T", "TE", "TL", "TN", "TOT", "TUB", "UNT", "Y2", "YD" };
	}
}
