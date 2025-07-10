using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobDocumentSupporter : DocumentSupporter
	{
		protected JobDocumentSupporter(JobDocumentPrintItem jobPrinter)
			: base(jobPrinter)
		{
		}

		public static JobDocumentSupporter New(JobDocumentPrintItem jobPrinter)
		{
			return new JobDocumentSupporter(jobPrinter);
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobInvoicingJob; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.JobInvoicingJob, JobDocPrinter) };
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			if (fSupportedDataContexts == null)
			{
				fSupportedDataContexts = new Constants.DataContext[] { Constants.DataContext.JobInvoicingJob };
			}

			return fSupportedDataContexts;
		}

		Constants.DataContext[] fSupportedDataContexts;

		protected virtual JobDocumentPrintItem JobDocPrinter
		{
			get { return (JobDocumentPrintItem)BusinessObject; }
		}
	}
}
