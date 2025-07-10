using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationFilterLookups))]
sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
{
	public void TestEntryStatusList()
	{
		AssertContainsExactElementsInExactOrder(CommonLookups.CustomsStatusList(Factory), Lookups.EntryStatusList());
	}

	public void TestMessageStatusList()
	{
		AssertContainsExactElementsInAnyOrder("MessageStatusList", new[] { "ACC", "ACK", "INV", "NOT", "FAL", "SNT" }, Lookups.MessageStatusList().GetAllCodes());
	}

	public void TestPhaseStatusList()
	{
		AssertContainsExactElementsInAnyOrder("PhaseStatusList", new[] { "013", "014", "015", "016", "069", "123", "130" }, Lookups.PhaseStatusList.GetAllCodes());
	}

	public void TestSelectionResultList()
	{
		RefCusCodeTestHelper.CreateSelectionResultList(Factory);

		AssertContainsExactElementsInAnyOrder("SelectionResultList", new[] { "1", "2", "3" }, Lookups.SelectionResultList().GetAllCodes());
	}

	JobDeclarationFilterLookups Lookups => lookups ??= new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
	JobDeclarationFilterLookups lookups;
}
