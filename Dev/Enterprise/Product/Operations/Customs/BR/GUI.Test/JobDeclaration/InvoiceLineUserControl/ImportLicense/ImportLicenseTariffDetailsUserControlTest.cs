using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseTariffDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new ImportTariffDetailsUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestTariffDetach_Button_Click()
		{
			var declartion = Factory.NewWithValidTestData<JobDeclaration>();
			declartion.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ImportLicenseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.BRTariffDetailsTab;
				var importTariffDetails = invoiceLineUserControl.ImportTariffDetailsUserControl;
				var tariffDetachLayout = importTariffDetails.TariffDetachLayout;
				var tariffDetachButton = tariffDetachLayout.TariffDetachButton;
				tariffDetachButton.PerformClick();
				AssertType<TariffDetachCollectionForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
