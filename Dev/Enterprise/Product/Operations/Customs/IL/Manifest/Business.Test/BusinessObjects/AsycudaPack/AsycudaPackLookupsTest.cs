using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public sealed class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PKG");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.PackageTypes, "KG", "KG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.PackageTypes, "TN", "TN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.PackUQList;
			CombineAssertions(() =>
			{
				AssertNotEquals("List is not RefPackTypeCollection", typeof(RefPackTypeCollection), list.GetType());
				AssertEquals("KG, TN", list.CodesAsString);
			});
		}
	}
}
