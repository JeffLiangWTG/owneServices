using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class RefCusPackListProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomsPackList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILManifestPackageTypes, "IL Manifest Package Type List", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "MPKG", "1A", "Drum steel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "MPKG", "BE", "Bundle", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "MPKG", "CC", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL Package Type List", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "PKG", "1A", "Drum steel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "PKG", "BE", "Bundle", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "PKG", "AA", "Bundle AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetCustomsPackList(factory, ZString.Empty, Core.Constants.CountryCodes.Israel);
			AssertEquals(4, list.Count);
			AssertEquals("list Sort By Description And Combine (MPKG,PKG) If Same Code", "BE, AA, CC, 1A", list.CodesAsString);
		}
	}
}
