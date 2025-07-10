using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Core.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	[TestedType(typeof(MoveDatabasesToNewEnterprisePopupForm))]
	public class MoveDatabasesToNewEnterprisePopupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = new MoveDatabasesToNewEnterpriseBizObj(Factory, Enumerable.Empty<ZGuid>(), new LoggerForTest());
			return new MoveDatabasesToNewEnterprisePopupForm(bizO);
		}

		public void TestMessageBoxes()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			Factory.Save();

			var bizObj = new MoveDatabasesToNewEnterpriseBizObj(Factory, Enumerable.Empty<ZGuid>(), new MoveDatabasesToNewEnterpriseLogger());
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new MoveDatabasesToNewEnterprisePopupForm(bizObj))
			{
				form.Show();
				Application.DoEvents();
				var okButton = form.Controls.Find("ButtonOk", true).OfType<ZButton>().Single();

				AssertEquals("", bizObj.LicenceEnterpriseID);
				bizObj.RegistrationStatus = "xxx";
				okButton.PerformClick();
				AssertEquals("Enterprise: Please enter an Enterprise.\nRegistration: Enter a valid Registration.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				bizObj.LicenceEnterpriseID = licHeader.Database.EnterpriseID;
				bizObj.RegistrationStatus = "REG";
				okButton.PerformClick();
				var dialog = (UserConfirmationDialog)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("SET", dialog.ExpectedString);
				AssertEquals("You are about to set Enterprise ID/Code. Are you sure you want to proceed?", dialog.MessageMultilingual.ToString());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				okButton.PerformClick();
				AssertEquals("Invalid Default Enterprise (Registry -> Product Registration WebAPI Default Enterprise ID)", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Close();
			}
		}
	}
}
