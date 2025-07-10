using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class DV1UserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			using (var control = GetNewDV1UserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("DV1TabControl", control.FindSingle<ZTabControl>("DV1TabControl"));
					AssertNotNull("DV1DetailsTabPage", control.FindSingle<ZTabPage>("DV1DetailsTabPage"));
					AssertNotNull("DynamicDV1DetailsLayout", control.FindSingle<DynamicLayoutPanel>("DynamicDV1DetailsPanel"));
					AssertEquals("DV1GridUserControl", ExpectedGridUserControl(), control.FindSingle<ZDynamicControlCreationUserControl>("DV1GridUserControl").UserControlType);
				});
			}
		}

		public void TestGetDv1DetailsLayout()
		{
			using (var control = GetNewDV1UserControl())
			{
				AssertEquals(ExpectedDynamicDV1DetailsLayout(), control.GetDv1DetailsLayout().GetType());
			}
		}

		public void TestDynamicDV1DetailsPanel()
		{
			using (var control = GetNewDV1UserControl())
			{
				var panel = control.FindSingle<DynamicLayoutPanel>("DynamicDV1DetailsPanel");
				AssertNoExceptionThrown(() => panel.PerformLayout());
			}
		}

		protected virtual Type ExpectedGridUserControl() => typeof(DV1GridUserControl);

		protected virtual DV1UserControl GetNewDV1UserControl() => new DV1UserControl();

		protected virtual Type ExpectedDynamicDV1DetailsLayout() => typeof(DV1DetailsLayout);
	}
}
