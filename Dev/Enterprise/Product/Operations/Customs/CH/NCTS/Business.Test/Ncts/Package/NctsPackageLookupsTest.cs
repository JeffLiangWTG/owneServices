using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageLookups))]
sealed class NctsPackageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestUnloadedStatesList() => CombineAssertions(() =>
	{
		Package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("CodesAsString", "DEC, DIF, MIS, NEW", Package.Lookups.UnloadedStates.CodesAsString);

		Package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		AssertEquals("CodesAsString", "DEC, DIF, MIS", Package.Lookups.UnloadedStates.CodesAsString);

		var list = Package.Lookups.UnloadedStates;
		Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_False_Y", out CodeDescriptionPairList cachedList);

		AssertSame("Cached", list, cachedList);
	});

	NctsHeader NctsHeader => nctsHeader ??= CreateNctsHeader();
	NctsHeader nctsHeader;

	NctsPackage Package => package ??= NctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();
	NctsPackage package;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader;
	}
}
