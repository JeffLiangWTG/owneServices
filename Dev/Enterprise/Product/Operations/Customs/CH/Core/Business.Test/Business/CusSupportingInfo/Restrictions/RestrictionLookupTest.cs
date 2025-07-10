using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(RestrictionLookups))]
sealed class RestrictionLookupTest : TestCaseWithFactory
{
	public void TestPermitOwnerList()
	{
		AssertType<OrganisationsFindBoxCollection>(Lookups.PermitOwnerList);
	}

	public void TestCodeList()
	{
		RefCusCodeTestHelper.CreateRestrictionCodeLists(Factory);
		AssertContainsExactElementsInAnyOrder(new[] { RefCusCodeTestHelper.RestrictionCode1, RefCusCodeTestHelper.RestrictionCode2, RefCusCodeTestHelper.RestrictionCodeWithoutException }, ((CodeDescriptionPairList)Lookups.CodeList).GetAllCodes());
	}

	public void TestExceptionReasonList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRestrictionCodeLists(Factory);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCode1;
		AssertContainsExactElementsInAnyOrder("Multiple codes", new[] { RefCusCodeTestHelper.RestrictionExceptionCodeFor1, RefCusCodeTestHelper.RestrictionExceptionCodeFor1And2 }, Lookups.ExceptionReasonList.GetAllCodes());

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCode2;
		AssertContainsExactElementsInAnyOrder("Single code", new[] { RefCusCodeTestHelper.RestrictionExceptionCodeFor1And2, }, Lookups.ExceptionReasonList.GetAllCodes());

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithoutException;
		AssertEquals("No codes", 0, Lookups.ExceptionReasonList.Count);
	});

	Restriction Restriction => restriction ??= Factory.New<Restriction>();
	Restriction restriction;

	RestrictionLookups Lookups => Restriction.Lookups;
}
