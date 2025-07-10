using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestBusinessContext()
		{
			AssertEquals("Business Context", BusinessContext.JobInvoicingJob, DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Constants.DataContext.JobInvoicingJob is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.JobInvoicingJob)));
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.JobInvoicingJob, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("Customisation Security Checkpoint", Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Job = Factory.NewJobForTesting<Job>();
			JobDocumentPrinter docPrinter = new JobDocumentPrinter(Factory);
			JobDocumentPrintItem docPrintItem = new JobDocumentPrintItem(docPrinter, Job, Factory);
			AssertNotNull("Job should not be null", Job);

			DocumentSupporter = JobDocumentSupporter.New(docPrintItem);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
		}

		JobDocumentSupporter DocumentSupporter;
		Job Job;
	}
}
