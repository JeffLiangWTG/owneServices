using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class RFPPackagesUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			using (var testForm = new ZForm(invoiceLine))
			{
				using (var control = new RFPPackagesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					var packagesGroupBox = control.FindSingle<ZGroupBox>("PackagesGroupBox");
					AssertEquals("Packages", packagesGroupBox.Text);
					var weightGroupBox = control.FindSingle<ZGroupBox>("WeightGroupBox");
					AssertEquals("Weights", weightGroupBox.Text);
					var groupBoxShippingMarks = control.FindSingle<ZGroupBox>("GroupBoxShippingMarks");
					AssertEquals("Shipping Marks and Batch Code", groupBoxShippingMarks.Text);
				}
			}
		}

		public void TestShippingMarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_ShippingMarks = "MARKS";

			using (var testForm = new ZForm(invoiceLine))
			{
				using (var control = new RFPPackagesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var textBoxShippingMarks = control.FindSingle<ZTextBox>("TextBoxQL_ShippingMarks");
					AssertEquals("MARKS", textBoxShippingMarks.Text);
				}
			}
		}
	}
}
