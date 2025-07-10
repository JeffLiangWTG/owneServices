using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class JobComInvocieLineBoundLongTextControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyIsCorrectForSeededAndNonSeededLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.Invoices.AddNew();
			var invLine1 = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			invLine1.CA_IsSeeded = true;
			var invLine2 = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			invLine2.CA_IsSeeded = false;

			using (var form = new ZForm(declaration))
			{
				using (var longTextBoxControl = new JobComInvocieLineBoundLongTextControl())
				{
					form.Controls.Add(longTextBoxControl);
					longTextBoxControl.SetDataBinding(declaration, "Invoices.AsAccountForFilteredInvoiceLines.JI_Description");
					form.Show();
					var manager = longTextBoxControl.BindingContext[declaration, "Invoices.AsAccountForFilteredInvoiceLines"] as CurrencyManager;
					manager.Position = 0;
					var isReadOnly = longTextBoxControl.ReadOnly;
					manager.Position = 1;
					AssertEquals("ReadOnly state changes as long text control move binding from non-seeded line to seeded line (or vice versa)", isReadOnly, !longTextBoxControl.ReadOnly);
				}
			}
		}
	}
}
