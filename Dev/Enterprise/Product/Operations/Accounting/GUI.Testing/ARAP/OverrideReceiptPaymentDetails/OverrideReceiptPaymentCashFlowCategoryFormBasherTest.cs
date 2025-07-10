using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(OverrideReceiptPaymentCashFlowCategoryForm))]
	public class OverrideReceiptPaymentCashFlowCategoryFormBasherTest : OverrideReceiptPaymentDetailsFormTest
	{
		#region Implementation

		OverrideReceiptPaymentCashFlowCategoryHelper bo;

		protected override Form GetFormToBashCore()
		{
			bo = new OverrideReceiptPaymentCashFlowCategoryHelper(Factory, Factory.New<ARReceipt>().PK);
			return new OverrideReceiptPaymentCashFlowCategoryForm(bo);
		}

		protected override void AssertFormDisplayMode(OverrideReceiptPaymentDetailsForm testForm)
		{
			AssertEquals("DisplayMode", ODisplayMode.NewSaved, testForm.DisplayMode);
			bo.WrappedObjects[0].AH_TransactionCategory = "O01";
			AssertEquals("DisplayMode", ODisplayMode.Edit, testForm.DisplayMode);
			ContinueWithSave saveResult = testForm.FireSaveButton();
			AssertEquals("Precondition: sorm should be correctly", ContinueWithSave.Yes, saveResult);
			AssertEquals("DisableNewAction", ODisplayMode.NewSaved, testForm.DisplayMode);
		}

		#endregion
	}
}
