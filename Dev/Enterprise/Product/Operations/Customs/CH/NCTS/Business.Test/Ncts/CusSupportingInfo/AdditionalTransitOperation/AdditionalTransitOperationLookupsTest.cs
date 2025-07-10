using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using CHRefCusCodeList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class AdditionalTransitOperationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestIssuerTypeList() => CombineAssertions(() =>
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateCodeList(CHRefCusCodeList.PassarTypes.N1150)
			.CreateCode("C01")
			.CreateCode("C02");
		Factory.Save();

		AssertEquals("C01, C02", Lookups.IssuerTypeList.CodesAsString);
		AssertSame("cached", Lookups.IssuerTypeList, Lookups.IssuerTypeList);
	});

	public void TestPackTypeList() => CombineAssertions(() =>
	{
		var refDataHelper = new RefDataTestHelper(Factory);
		refDataHelper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, RefDataGrouping.Codes.UnitedNationsRecommendations)
			.CreateCode("P01")
			.CreateCode("P02");
		Factory.Save();

		AssertEquals("P01, P02", Lookups.PackTypeList.CodesAsString);
		AssertSame("cached", Lookups.PackTypeList, Lookups.PackTypeList);
	});

	public void TestStateOfSealsValidList() => CombineAssertions(() =>
	{
		AssertEquals("N, Y", Lookups.StateOfSealsValidList.CodesAsString);
		AssertSame("Cached", Lookups.StateOfSealsValidList, Lookups.StateOfSealsValidList);
	});

	AdditionalTransitOperation AdditionalTransitOperation => additionalTransitOperation ?? (additionalTransitOperation = CreateSupernumeraryGoods());
	AdditionalTransitOperation additionalTransitOperation;

	AdditionalTransitOperation CreateSupernumeraryGoods()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
	}

	AdditionalTransitOperationLookups Lookups => AdditionalTransitOperation.Lookups;
}
