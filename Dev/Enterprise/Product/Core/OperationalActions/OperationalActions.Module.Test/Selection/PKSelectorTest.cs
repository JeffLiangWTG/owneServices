using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class PKSelectorTest : TestCaseWithFactory
	{
		public void TestGetSelectedRecordPKs()
		{
			var bizo1 = Factory.New<DummyChildBusinessObject>();
			var bizo2 = Factory.New<DummyChildBusinessObjectExcludable>();
			var bizo3 = Factory.New<DummyChildBusinessObjectExcludable>();

			var bizos = new[] { bizo1, bizo2, bizo3 };

			ITargetRecordSelection selection = new BusinessObjectSelector(bizos);

			var selectedRecords = selection.GetSelectedRecords();

			AssertEquals(false, selectedRecords.AutoSelectedAllKeys);
			AssertContainsExactElementsInAnyOrder(bizos.Select(bizo => bizo.PK), selectedRecords.PrimaryKeys);

			bizo2.ShouldExclude = true;

			selectedRecords = selection.GetSelectedRecords();

			AssertEquals(false, selectedRecords.AutoSelectedAllKeys);
			AssertContainsExactElementsInAnyOrder(new[] { bizo1.PK, bizo3.PK }, selectedRecords.PrimaryKeys);
			AssertEquals(bizo2.ReasonForExclusion, selection.ExclusionReasons.Single());
		}
	}
}
