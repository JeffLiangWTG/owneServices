using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class RecommendEnableAutoJRJInfoControlTest : TestCaseWithFactory
	{
		public void TestLearnMore()
		{
			using (var form = new ZForm())
			using (var userControl = new RecommendEnableAutoJRJInfoControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var buttonLearnMore = form.Controls.Find("LearnMoreButton", true)[0] as ZButton;
				buttonLearnMore.PerformClick();
				AssertEquals("http://www.cargowise.com/Documents/UpdateNotes/CargoWiseOneUpdateNote20190923.pdf", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestDialogDefaultContext()
		{
			var context = RecommendEnableAutoJRJInfoControl.DialogDefaultContext;
			Assert(context.ShowCheckboxOnly);
			AssertEquals("Do not show this message again", context.CheckBoxCaption.Caption);
			AssertEquals("Auto Job Revenue Journal setting", context.Caption);
			AssertEquals(ZMessageBoxIcon.Information, context.Icon);
		}
	}
}
