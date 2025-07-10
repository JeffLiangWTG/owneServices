#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class SingleInvoiceDataImporterBusinessObject
	{
		public ZString ProgressMessageForFatalError_ForTestOnly => ProgressMessageForFatalError;
	}
}

#endif
