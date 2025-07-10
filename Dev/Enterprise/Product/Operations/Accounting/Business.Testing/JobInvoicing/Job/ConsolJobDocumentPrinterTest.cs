using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ConsolJobDocumentPrinter))]
	public class ConsolJobDocumentPrinterTest : JobDocumentPrinterTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDocPrinter = new ConsolJobDocumentPrinter(Factory);
			return JobDocPrinter;
		}

		#endregion
	}
}
