using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using testBizO = Enterprise.Client.EDI.MasterFiles.Business.Test.MoveLicencesToNewEnterpriseIDBizOTest;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	[TestedType(typeof(MoveLicencesToNewEnterpriseIDPopupForm))]
	public class MoveLicencesToNewEnterpriseIDPopupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			MoveLicencesToNewEnterpriseIDBizO bizO = new MoveLicencesToNewEnterpriseIDBizO(licEnt, org, new MoveLicencesToNewEnterpriseIDBizOCallbacks());
			return new MoveLicencesToNewEnterpriseIDPopupForm(bizO);
		}

		public void TestButtonOk_Click()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			licEnt.LE_EnterpriseID = "QWE";
			LicenceEnterprise anotherLicEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			anotherLicEnt.LE_EnterpriseID = "RTY";
			Factory.Save();
			testBizO.MoveLicencesToNewEnterpriseIDBizOForTest bizO = new testBizO.MoveLicencesToNewEnterpriseIDBizOForTest(licEnt, org);
			bizO.MoveLicencesToNewEnterpriseID_CallBase = false;
			using (MoveLicencesToNewEnterpriseIDPopupFormForTest form = new MoveLicencesToNewEnterpriseIDPopupFormForTest(bizO))
			{
				try
				{
					ZCodeFindBox formFindBox = (ZCodeFindBox)(form.Controls.Find("LicenceEnterpriseIDFindBox", false)[0]);
					ZButton okButton = (ZButton)(form.Controls.Find("ButtonOk", false)[0]);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.ShowConfirmationDialog_Result = false;
					form.Show();
					formFindBox.CodeBox.Text = "";
					AssertEquals("Precondition from MoveLicencesToNewEnterpriseIDBizO", null, bizO.fLicEnterpriseAccessor);
					okButton.PerformClick();
					Application.DoEvents();
					AssertEquals("Should show error message if user enters empty enterprise ID", OkButtonErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not run MoveLicencesToNewEnterpriseID() if user does not enter acceptable enterprise ID", false, bizO.MoveLicencesToNewEnterpriseID_WasCalled);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.ShowConfirmationDialog_Result = false;
					formFindBox.CodeBox.Text = "RTY";
					okButton.Focus();
					Application.DoEvents();
					AssertEquals("Precondition from MoveLicencesToNewEnterpriseIDBizO", anotherLicEnt, bizO.fLicEnterpriseAccessor);
					okButton.PerformClick();
					Application.DoEvents();
					AssertEquals("Should not run MoveLicencesToNewEnterpriseID() if user enters acceptable enterprise ID but does not enter 'move' word in confirmation popup window", false, bizO.MoveLicencesToNewEnterpriseID_WasCalled);

					form.ShowConfirmationDialog_Result = true;
					okButton.PerformClick();
					Application.DoEvents();
					AssertEquals("Should run MoveLicencesToNewEnterpriseID() if user enters acceptable enterprise ID and enters 'move' word in confirmation popup window", true, bizO.MoveLicencesToNewEnterpriseID_WasCalled);
				}
				finally
				{
					form.Hide();
					form.Dispose();
				}
			}
		}

		#region Implementation

		ZString OkButtonErrorMessage
		{
			get { return "Please enter existing enterprise ID or press Cancel button to close this window"; }
		}

		class MoveLicencesToNewEnterpriseIDPopupFormForTest : MoveLicencesToNewEnterpriseIDPopupForm
		{
			public MoveLicencesToNewEnterpriseIDPopupFormForTest()
				: base()
			{
			}

			public MoveLicencesToNewEnterpriseIDPopupFormForTest(MoveLicencesToNewEnterpriseIDBizO moveLicEntBizO)
				: base(moveLicEntBizO)
			{
			}

			protected override bool ShowConfirmationDialog()
			{
				return ShowConfirmationDialog_Result;
			}

			public bool ShowConfirmationDialog_Result
			{
				get;
				set;
			}
		}

		#endregion
	}
}
