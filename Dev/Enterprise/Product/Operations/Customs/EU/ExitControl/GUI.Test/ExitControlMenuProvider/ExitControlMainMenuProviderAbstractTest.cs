using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

public abstract class ExitControlMainMenuProviderAbstractTest : TestCaseWithFactory
{
	public void TestAdditionalMainMenuItems()
	{
		var header = Factory.New<CusExitHeader>();
		IExitControlMainMenuProvider provider = new ExitControlMainMenuProvider(header);
		var additionalMainMenuItems = provider.AdditionalMainMenuItems;
		CombineAssertions(() =>
		{
			AssertEquals(2, additionalMainMenuItems.Count);
			AssertEquals("CreateExitReportMenuItem", additionalMainMenuItems[0].Name);
			AssertEquals("SendToCustomsMenuItem", additionalMainMenuItems[1].Name);
		});
	}
}
