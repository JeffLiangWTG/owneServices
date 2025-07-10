using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	sealed class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			var unpackCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "NE", "Unpack code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(unpackCode.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "1");
			var bulkCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VQ", "Unpack code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(bulkCode.PK, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "1");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.PackUQList;
			CombineAssertions(() =>
			{
				AssertNotEquals("List is not RefPackTypeCollection", typeof(RefPackTypeCollection), list.GetType());
				Assert("List contains unpack code", list.ContainsCode("NE"));
				Assert("List contains bulk code", list.ContainsCode("VQ"));
			});
		}
	}
}
