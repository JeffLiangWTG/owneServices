using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class DataImporterBusinessObjectWithResultReporter : DataImporterBusinessObject
	{
		public DataImporterBusinessObjectWithResultReporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IDataTransferResultReporter DataTransferResultReporter
		{
			get;
			set;
		}

		protected override ZString ProgressMessageForFatalError
		{
			get { return DataTransferResultReporter.WasTheLastDataTransferSuccessful ? ProgressMessageForSuccessfulImport : base.ProgressMessageForFatalError; }
		}

		protected ZString ProgressMessageForFatalErrorBase
		{
			get { return base.ProgressMessageForFatalError; }
		}
	}
}