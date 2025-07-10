using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	public class AccPayableOrderHeaderDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestBusinessContext()
		{
			AssertEquals("Business Context", BusinessContext.PayableOrder, DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Constants.DataContext.PayableOrder is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.PayableOrder)));
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrapperArray = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.PayableOrder, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals("Customisation Security Checkpoint", Env.Security.OrderTrackingCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			DocumentSupporter = new AccPayableOrderHeaderDocumentSupporter(Order);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
		}

		AccPayableOrderHeaderDocumentSupporter DocumentSupporter;
		AccPayableOrderHeader Order;
	}
}
