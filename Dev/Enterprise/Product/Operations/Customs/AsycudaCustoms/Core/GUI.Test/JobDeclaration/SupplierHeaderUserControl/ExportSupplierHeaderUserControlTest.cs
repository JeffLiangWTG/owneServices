using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class ExportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestColumnsCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (ExportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var expectCaption = "Add to Customs Value";
				AssertEquals("J7_IsDutiable caption For InvoiceChargesGrid", expectCaption, supplierHeaderUserControl.InvoiceChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Caption);
				AssertEquals("J7_IsDutiable caption For ApportionedChargesGrid", expectCaption, supplierHeaderUserControl.ApportionedChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Caption);
				AssertEquals("J7_IsDutiable caption For BaseGroupChargesGrid", expectCaption, supplierHeaderUserControl.BaseGroupChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Caption);
			}
		}

		public void TestColumnsRemoval()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (ExportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				Assert("Remove J7_IsGSTApplicable from InvoiceChargesGrid", supplierHeaderUserControl.InvoiceChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsGSTApplicable).IsUnavailable);
				Assert("Remove J7_IsGSTApplicable from ApportionedChargesGrid", supplierHeaderUserControl.ApportionedChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsGSTApplicable).IsUnavailable);
				Assert("Remove J7_IsGSTApplicable from BaseGroupChargesGrid", supplierHeaderUserControl.BaseGroupChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsGSTApplicable).IsUnavailable);
			}
		}

		public void TestColumnsLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (ExportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var expectWidth = 120;
				AssertEquals("J7_IsDutiable width For InvoiceChargesGrid", expectWidth, supplierHeaderUserControl.InvoiceChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Width);
				AssertEquals("J7_IsDutiable width For ApportionedChargesGrid", expectWidth, supplierHeaderUserControl.ApportionedChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Width);
				AssertEquals("J7_IsDutiable width For BaseGroupChargesGrid", expectWidth, supplierHeaderUserControl.BaseGroupChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == InvoiceCharge.Schema.J7_IsDutiable).Width);
			}
		}
	}
}
