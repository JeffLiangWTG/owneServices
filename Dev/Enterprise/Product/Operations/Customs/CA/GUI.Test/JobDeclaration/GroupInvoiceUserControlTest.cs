using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class GroupInvoiceUserControlTest : TestCaseWithFactory
	{
		public void TestChangeGSTApplicableText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceGroupingTabPage;
				var userControl = (GroupInvoiceUserControl)form.CustomsBrokerageUserControl.InvoiceGroupUserControl;
				AssertEquals("Charges grid GST Applicable column text", CACustomsSupplierHeaderUserControl.IsCIFComponent, userControl.GroupChargeGrid.Columns[JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name].ColumnStyle.HeaderText);
			}
		}
	}
}
