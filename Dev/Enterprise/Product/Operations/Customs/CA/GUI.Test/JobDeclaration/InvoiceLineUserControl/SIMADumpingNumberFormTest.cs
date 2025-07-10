using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(SIMADumpingNumberForm))]
	sealed class SIMADumpingNumberFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SIMAMeasures.AddNew("AD1234", "IAN TEST");
			return new SIMADumpingNumberForm(invoiceLine.SIMAMeasures, "");
		}
	}
}
