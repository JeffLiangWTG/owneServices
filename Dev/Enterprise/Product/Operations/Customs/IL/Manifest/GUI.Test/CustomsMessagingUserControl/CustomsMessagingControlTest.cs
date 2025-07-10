using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class CustomsMessagingControlTest : TestCaseWithFactory
	{
		public void TestCustomsMessagingControl()
		{
			AssertEquals(typeof(IL.GUI.CustomsMessagingControl), typeof(CustomsMessagingControl).BaseType);
			using var control = new CustomsMessagingControl();

			var additionalTabPage = control as IAdditionalTabPage;
			AssertNotNull(additionalTabPage);
			AssertEquals(control, additionalTabPage.AdditionalTabPageUserControl);
			AssertEquals(6, additionalTabPage.TabPageSequence);
			AssertEquals("Messages", additionalTabPage.AdditionalTabPageCaption.Caption);
			AssertEquals(true, additionalTabPage.AdditionalControlVisibility.isVisible(null));
		}
	}
}
