using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	public class AdditionalProcedureUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAdditionalProcedureCodesEditButton()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new EUH7BillFieldsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;

				var additionalProcedureCodesEditButton = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");

				var button = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");
				button.PerformClick();
				AssertEquals(typeof(AdditionalProcedureCodeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestAdditionalProcedureCodesEditButton_NoExceptionThrown_WhenCurrentDataItemIsNull()
		{
			using (var form = new ZForm(null))
			using (var control = new EUH7BillFieldsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;

				var additionalProcedureCodesEditButton = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");

				var button = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");
				AssertNoExceptionThrown(button.PerformClick);
			}
		}

		[RequiresSTA]
		public void TestAdditionalProcedureCodesTextBox()
		{
			using (var control = new EUH7BillFieldsUserControl())
			{
				control.Show();

				var additionalProcedureCodesAsStringTextBox = control.FindSingle<ZTextBox>("AdditionalProcedureCodesAsStringTextBox");

				AssertEquals("BindingMember", "AdditionalProcedureCodesAsString", additionalProcedureCodesAsStringTextBox.GetBindingMember());
			}
		}
	}
}
