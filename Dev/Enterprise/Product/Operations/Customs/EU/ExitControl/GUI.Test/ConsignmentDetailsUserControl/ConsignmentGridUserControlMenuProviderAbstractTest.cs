using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

public abstract class ConsignmentGridUserControlMenuProviderAbstractTest : TestCaseWithFactory
{
	public void TestIConsignmentsGridUserControlMenuProvider_AdditionalConsignmentsGridMenuItems()
	{
		var header = Factory.New<CusExitHeader>();
		using (var gridProvider = new ConsignmentsGridUserControlProviderForTesting(header))
		{
			IConsignmentsGridUserControlMenuProvider provider = new ConsignmentsGridUserControlMenuProvider(gridProvider);
			var additionalConsignmentsGridMenuItems = provider.AdditionalConsignmentsGridMenuItems;
			CombineAssertions(() =>
			{
				AssertEquals(1, additionalConsignmentsGridMenuItems.Count);
				AssertEquals("CreateExitReportMenuItem", additionalConsignmentsGridMenuItems[0].Name);
			});
		}
	}

	public void TestIConsignmentsGridUserControlMenuProvider_AdditionalMenuItemsForMainForm()
	{
		var header = Factory.New<CusExitHeader>();
		using (var gridProvider = new ConsignmentsGridUserControlProviderForTesting(header))
		{
			IConsignmentsGridUserControlMenuProvider provider = new ConsignmentsGridUserControlMenuProvider(gridProvider);
			var additionalMenuItemsForMainForm = provider.AdditionalMenuItemsForMainForm;
			CombineAssertions(() =>
			{
				AssertEquals(1, additionalMenuItemsForMainForm.Count);
				AssertEquals("CreateExitReportMenuItem", additionalMenuItemsForMainForm[0].Name);
			});
		}
	}
}
