#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class DataImporterBusinessObjectWithResultReporter
	{
		public ZString ProgressMessageForFatalErrorBase_ForTestOnly => ProgressMessageForFatalErrorBase;

		public ZString ProgressMessageForFatalError_ForTestOnly => ProgressMessageForFatalError;

		public ZString ProgressMessageForSuccessfulImport_ForTestOnly => ProgressMessageForSuccessfulImport;

		public ZString ProgressMessageForFatalError_ForTestOnlyBase_ForTestOnly => ProgressMessageForFatalError;
	}
}

#endif
