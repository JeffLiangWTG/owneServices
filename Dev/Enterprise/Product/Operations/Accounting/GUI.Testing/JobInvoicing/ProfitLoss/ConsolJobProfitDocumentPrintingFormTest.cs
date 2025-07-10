using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ConsolJobProfitDocumentPrintingForm))]
	public class ConsolJobProfitDocumentPrintingFormTest : JobProfitDocumentPrintingFormTest
	{
		protected override Form GetFormToBashCore()
		{
			ConsolJobDocumentPrinter printer = new ConsolJobDocumentPrinter(Factory);
			Factory.Save();
			return new ConsolJobProfitDocumentPrintingForm(printer);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
