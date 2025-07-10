using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class SuspensionDrawbackUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new SuspensionDrawbackUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestTariffColumnStyle()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var header = jobDeclaration.Invoices.AddNew();
			header.JZ_ValuationDateOverride = new ZDateTime(2021, 04, 30);
			var line = header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.SuspensionDrawbackTab;
					var suspensionDrawbackUserControl = invoiceLineUserControl.SuspensionDrawbackUserControl;
					AssertNotNull(suspensionDrawbackUserControl);

					var grid = suspensionDrawbackUserControl.SuspensionDrawbackGrid;
					AssertType<Universal.GUI.TariffColumnStyleInfo>(grid.GetColumnStyle(AutoCusSupportingInfo.Schema.CSI_Tariff));
					AssertEquals("EffectiveDate", new ZDateTime(2021, 04, 30), suspensionDrawbackUserControl.GetEffectiveDate());
				}
			}
		}
	}
}
