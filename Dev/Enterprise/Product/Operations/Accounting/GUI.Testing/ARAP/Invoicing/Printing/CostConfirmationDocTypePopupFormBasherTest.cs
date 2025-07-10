using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CostConfirmationDocTypePopupForm))]
	public class CostConfirmationDocTypePopupFormBasherTest : ZFormBasherTest
	{
		public void TestDontCloseFormWithValidationErrors()
		{
			using (ZForm testForm = (ZForm)GetFormToBashCore())
			{
				testForm.Show();
				CostConfirmationDocTypeBizo docTypeBizo = (CostConfirmationDocTypeBizo)testForm.BusinessEntity;
				docTypeBizo.CostConfirmationDocType = "";
				testForm.DialogResult = DialogResult.OK;
				testForm.Close();
				AssertEquals("The form must not be closed with validation errors", true, testForm.Visible);

				docTypeBizo.CostConfirmationDocType = AccountingConstants.CostConfirmationDocumentSettingsCodes.Both;
				testForm.Close();
				AssertEquals("The form must not be closed with validation errors", false, testForm.Visible);
			}
			using (ZForm testForm = (ZForm)GetFormToBashCore())
			{
				testForm.Show();
				CostConfirmationDocTypeBizo docTypeBizo = (CostConfirmationDocTypeBizo)testForm.BusinessEntity;
				docTypeBizo.CostConfirmationDocType = "";
				testForm.DialogResult = DialogResult.Cancel;
				testForm.Close();
				AssertEquals("The form can be closed with validation errors if user cancel an action.", false, testForm.Visible);
			}
		}

		public void TestAcceptCancelButtons()
		{
			using (ZForm testForm = (ZForm)GetFormToBashCore())
			{
				AssertNotNull("AcceptButton must be set.", testForm.AcceptButton);
				AssertEquals("AcceptButton DialogResult must be set to OK.", DialogResult.OK, ((Button)testForm.AcceptButton).DialogResult);
				AssertNotNull("CancelButton must be set.", testForm.CancelButton);
				AssertEquals("CancelButton DialogResult must be set to Cancel.", DialogResult.Cancel, ((Button)testForm.CancelButton).DialogResult);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CostConfirmationDocTypePopupForm(new CostConfirmationDocTypeBizo());
		}

		#endregion
	}
}
