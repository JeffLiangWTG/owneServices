using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	public class FrDeclarationSendPrelodgeAmendmentActionApplicatorControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var zForm = new ZForm())
			using (var applicatorControl = new FrDeclarationSendPrelodgeAmendmentActionApplicatorControl())
			{
				zForm.Controls.Add(applicatorControl);
				zForm.Show();
				var sendMessagesEvenWithMessageErrorsCheckBox = applicatorControl.Controls.Find("sendMessagesEvenWithMessageErrorsCheckBox", true).SingleOrDefault() as ZCheckBox;
				AssertNotNull(sendMessagesEvenWithMessageErrorsCheckBox);
			}
		}
	}
}
