using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(EntryHeaderFilterLookups))]
sealed class EntryHeaderFilterLookupsTest : TestCaseWithFactory
{
	public void TestMessageStatusList()
	{
		AssertContainsExactElementsInAnyOrder("MessageStatusList", new[] { "ACC", "ACK", "INV", "NOT", "FAL", "SNT" }, Lookups.MessageStatusList().GetAllCodes());
	}

	public void TestPhaseStatusList()
	{
		AssertContainsExactElementsInAnyOrder("PhaseStatusList", new[] { "013", "014", "015", "016", "069", "123", "130" }, Lookups.PhaseStatusList.GetAllCodes());
	}

	public void TestEComplaintStatusList()
	{
		AssertContainsExactElementsInAnyOrder("EComplaintStatusList", new[] { "ACC", "CLS", "RCV", "REJ", "SNT" }, Lookups.EComplaintStatusList.GetAllCodes());
	}

	public void TestSelectionResultList()
	{
		RefCusCodeTestHelper.CreateSelectionResultList(Factory);

		AssertContainsExactElementsInAnyOrder("SelectionResultList", new[] { "1", "2", "3" }, Lookups.SelectionResultList.GetAllCodes());
	}

	EntryHeaderFilterLookups Lookups => lookups ??= new EntryHeaderFilterLookups(new EntryHeaderFilterBusinessObject());
	EntryHeaderFilterLookups lookups;
}
