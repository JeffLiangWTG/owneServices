using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing.Declaration.Inherited.Invoice_Lines_Control.Quarantine
{
	sealed class RFPStatementsUserControlTest : TestCaseWithFactory
	{
		public void TestStatementText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;
			quarantineExDocLine.QL_StatementText = "THIS IS A STATEMENT.";

			using (var testForm = new ZForm(invoiceLine))
			{
				using (var control = new RFPStatementsUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();

					var statementTextTextBox = control.FindSingle<ZTextBox>("QL_StatementTextTextBox");
					AssertEquals("THIS IS A STATEMENT.", statementTextTextBox.Text);
				}
			}
		}
	}
}
