using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Licensing.GUI.Test
{
	[TestedType(typeof(LicenseAgreementWebAcceptanceUserControl))]
	public class LicenseAgreementWebAcceptanceUserControlTest : TestCaseWithFactory
	{
		ZForm GetFormToBashCore(out System.Threading.Tasks.TaskCompletionSource<bool> tcs)
		{
			tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
			var form = new ZForm();
			form.Controls.Add(new LicenseAgreementWebAcceptanceUserControl("https://myaccount/UserAgreement.aspx", tcs));
			return form;
		}

		[TestDate(2024, 7, 4)]
		public void TestRecheckLicenseStatus_ClickShouldCloseFormIfAcceptedViaUserPortalClient()
		{
			var licenseAgreement = Factory.NewWithValidTestData<LicenseAgreement>();
			licenseAgreement.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			licenseAgreement.LAG_Type = LicenseAgreementTypeList.Codes.CargoWiseNext;
			Factory.Save();

			using (var form = GetFormToBashCore(out var tcs))
			{
				form.Show();

				var recheckButton = form.FindAll<ZButton>().Single(n => n.Name.StartsWith("recheckStatusButton"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				using (LicenseAcceptanceGuarantorTestHelper.WithNotAccepted())
				{
					recheckButton.PerformClick();
					Application.DoEvents();

					AssertEquals("Should show message that agreement has not been signed", "The agreement has not been signed. Please follow the link, sign the agreement, then try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not have tried to close", false, tcs.Task.IsCompleted);

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					recheckButton.PerformClick();

					AssertEquals("Should show message that agreement has not been signed", "The agreement has not been signed. Please follow the link, sign the agreement, then try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not have tried to close", false, tcs.Task.IsCompleted);
				}

				using (LicenseAcceptanceGuarantorTestHelper.WithAccepted())
				{
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					recheckButton.PerformClick();

					AssertEquals("Should show message that agreement has been signed", "The agreement has been signed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should close form", true, tcs.Task.IsCompleted);
				}
			}
		}
	}
}
