using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	public class MTDDeclarationFormTest : TransactionCreatorBaseTest
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			DataCollection.PopulateDataFromReport();

			using (MTDDeclarationForm form = new MTDDeclarationForm(DataCollection))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestConfirmButton()
		{
			using (MTDDeclarationForm form = new MTDDeclarationForm(DataCollection))
			{
				form.Show();

				DataCollection.Declaration = false;
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);

				DataCollection.Declaration = true;
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (MTDDeclarationForm form = new MTDDeclarationForm(DataCollection))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestDeclarationCheckBoxText()
		{
			var vatRegistration = Report.Company.OrgProxy.CustomsCodes.AddNew();
			vatRegistration.OK_CodeType = "VAT";
			vatRegistration.OK_CustomsRegNo = "12345678";
			AssertEquals("12345678", DataCollection.GSTRegNo);

			using (MTDDeclarationForm form = new MTDDeclarationForm(DataCollection))
			{
				form.Show();

				var declarationLabel = form.Controls.Find("declarationCheckBoxText1", true).FirstOrDefault() as ZArchitecture.ZLabel;
				AssertNotNull("Declaration Label", declarationLabel);
				var expectedText = "Declaration to HMRC by CargoWise Support for Eagle Datamation International (VRN - 12345678):";
				AssertEquals("Declaration Label Text", expectedText, declarationLabel.Text);

				var declarationCheckBox = form.Controls.Find("declarationCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull("Declaration Checkbox", declarationCheckBox);
				expectedText = "When you submit this VAT information you are making a legal declaration that the information is true and complete. A false declaration can result in prosecution.";
				AssertEquals("Declaration Checkbox Text", expectedText, declarationCheckBox.Text);
			}
		}

		public void TestErrorLabelText()
		{
			var vatRegistration = Report.Company.OrgProxy.CustomsCodes.AddNew();
			vatRegistration.OK_CodeType = "VAT";
			vatRegistration.OK_CustomsRegNo = "12345678";
			AssertEquals("12345678", DataCollection.GSTRegNo);

			using (var form = new MTDDeclarationForm(DataCollection))
			{
				form.Show();

				var errorLabel = form.Controls.Find("errorLabel", true).FirstOrDefault() as ZArchitecture.ZLabel;
				AssertNotNull("Error Label", errorLabel);
				var expectedText = "You need to accept the declaration before you can submit your VAT return.";
				AssertEquals("Error Label Text", expectedText, errorLabel.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			DataCollection = new MTDSubmissionDataColumns(Factory, Report);
			DataCollection.PopulateDataFromReport();
		}

		AccComplianceReport Report;
		MTDSubmissionDataColumns DataCollection;
	}
}
