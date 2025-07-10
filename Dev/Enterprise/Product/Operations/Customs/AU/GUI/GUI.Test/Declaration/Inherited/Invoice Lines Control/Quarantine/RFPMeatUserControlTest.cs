using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing.Declaration.Inherited.Invoice_Lines_Control.Quarantine
{
	sealed class RFPMeatUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			using (var testForm = new ZForm(invoiceLine))
			using (var control = new RFPMeatUserControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				var meatGroupBox = control.FindSingle<ZGroupBox>("MeatGroupBox");
				AssertEquals("Meat", meatGroupBox.Text);
			}
		}

		public void TestLabelApprovalNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_LabelApprovalNumber = "11";

			using (var testForm = new ZForm(invoiceLine))
			{
				using (var control = new RFPMeatUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var labelApprovalNumberTextBox = control.FindSingle<ZTextBox>("QL_LabelApprovalNumberTextBox");
					AssertEquals("11", labelApprovalNumberTextBox.Text);
				}
			}
		}
	}
}
