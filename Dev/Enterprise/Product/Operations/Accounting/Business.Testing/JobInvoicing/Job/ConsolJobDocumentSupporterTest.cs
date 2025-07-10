using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ConsolJobDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestBusinessContext()
		{
			AssertEquals("Business Context", BusinessContext.Consol, DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Constants.DataContext.ForwardingConsol is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ForwardingConsol)));
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.ForwardingConsol, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("Customisation Security Checkpoint", Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			ConsolJobDocumentPrinter docPrinter = new ConsolJobDocumentPrinter(Factory);
			ConsolJobDocumentPrintItem docPrintItem = new ConsolJobDocumentPrintItem(docPrinter, Consol, Factory);
			AssertNotNull("DocPrinter should not be null", docPrintItem);

			DocumentSupporter = ConsolJobDocumentSupporter.New(docPrintItem);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
		}

		ConsolJobDocumentSupporter DocumentSupporter;
		ForwardingConsol Consol;
	}
}
