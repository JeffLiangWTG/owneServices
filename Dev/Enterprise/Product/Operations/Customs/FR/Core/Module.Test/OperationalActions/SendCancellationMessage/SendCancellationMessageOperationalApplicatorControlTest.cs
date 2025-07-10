using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	public class SendCancellationMessageOperationalApplicatorControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var zForm = new ZForm())
			using (var applicatorControl = new SendCancellationMessageOperationalApplicatorControl())
			{
				zForm.Controls.Add(applicatorControl);
				zForm.Show();
				var invalidationMotivationDropEditWithFixedWidth = applicatorControl.Controls.Find("invalidationMotivationDropEditWithFixedWidth", true).SingleOrDefault() as ZDropEditWithFixedWidth;
				var invalidationReasonTextBox = applicatorControl.Controls.Find("invalidationReasonTextBox", true).SingleOrDefault() as ZTextBox;

				AssertionWithHtml.CombineAssertions(delegate
				{
					Assertion.AssertNotNull("invalidationMotivationDropEditWithFixedWidth should NOT be null.", invalidationMotivationDropEditWithFixedWidth);
					Assertion.AssertNotNull("invalidationReasonTextBox should NOT be null.", invalidationReasonTextBox);
				});
			}
		}
	}
}
