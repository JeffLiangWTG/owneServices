using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CACustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestChangeGSTApplicableText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var userControl = (CACustomsSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				AssertEquals("InvoiceChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.InvoiceChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
				AssertEquals("ApportionedChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.ApportionedChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
				AssertEquals("BaseGroupChargesGrid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.BaseGroupChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name).Caption);
			}
		}
	}
}
