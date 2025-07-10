using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaPackLookups))]
sealed class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
{
	[ExpectNoExceptions]
	public void TestPackageTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("PKG", "PKG");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedArabEmirates, "PKG", "T", "Bag, super bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedArabEmirates);
		Factory.Save();

		var pack = Factory.New<AsycudaPack>();
		var cachedList = pack.Lookups.PackUQList;
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(cachedList.GetAllCodes(), Is.EquivalentTo(new[] { "T" }));
			NUnit.Framework.Assert.That(pack.Lookups.PackUQList, Is.SameAs(cachedList), "List cached");
		});
	}
}
