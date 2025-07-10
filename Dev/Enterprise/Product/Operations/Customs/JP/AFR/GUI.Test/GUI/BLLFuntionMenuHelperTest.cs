using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class BLLFuntionMenuHelperTest : TestCaseWithFactory
	{
		public void TestBLLFuntionMenuHelper()
		{
			AssertNoExceptionThrown(() =>
			{
				var helper = new BLLFuntionMenuHelper(() => null, () => null);
				helper.RegisterSplitMenuItem.PerformClick();
				helper.RegisterSwitchMenuItem.PerformClick();
				helper.RegisterMergeMenuItem.PerformClick();
				helper.CancelSplitMenuItem.PerformClick();
				helper.CancelSwitchMenuItem.PerformClick();
				helper.CancelMergeMenuItem.PerformClick();
			});

			AssertNoExceptionThrown(() =>
			{
				var helper = new BLLFuntionMenuHelper(() => Factory.New<JPAFRHeader>(), () => null);
				helper.RegisterSplitMenuItem.PerformClick();
				helper.RegisterSwitchMenuItem.PerformClick();
				helper.RegisterMergeMenuItem.PerformClick();
				helper.CancelSplitMenuItem.PerformClick();
				helper.CancelSwitchMenuItem.PerformClick();
				helper.CancelMergeMenuItem.PerformClick();
			});

			AssertNoExceptionThrown(() =>
			{
				var header = Factory.New<JPAFRHeader>();
				var helper = new BLLFuntionMenuHelper(() => header, () => header.Bills.AddNew());
				helper.RegisterSplitMenuItem.PerformClick();
				helper.RegisterSwitchMenuItem.PerformClick();
				helper.RegisterMergeMenuItem.PerformClick();
				helper.CancelSplitMenuItem.PerformClick();
				helper.CancelSwitchMenuItem.PerformClick();
				helper.CancelMergeMenuItem.PerformClick();
			});
		}
	}
}
