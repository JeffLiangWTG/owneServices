using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class ConsolJobDocumentSupporter : JobDocumentSupporter
	{
		protected ConsolJobDocumentSupporter(ConsolJobDocumentPrintItem jobPrinter)
			: base(jobPrinter)
		{
		}

		public static ConsolJobDocumentSupporter New(ConsolJobDocumentPrintItem jobPrinter)
		{
			return new ConsolJobDocumentSupporter(jobPrinter);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Consol; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, JobDocPrinter) };
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			if (fSupportedDataContexts == null)
			{
				fSupportedDataContexts = new Constants.DataContext[] { Constants.DataContext.ForwardingConsol };
			}

			return fSupportedDataContexts;
		}

		Constants.DataContext[] fSupportedDataContexts;

		protected override JobDocumentPrintItem JobDocPrinter
		{
			get { return (ConsolJobDocumentPrintItem)BusinessObject; }
		}
	}
}