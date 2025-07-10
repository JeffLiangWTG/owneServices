using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Moq;

namespace Enterprise.Accounting.Business.Presentation.Testing
{
	public class AccountingPresentationProviderFactoryTest : TestCaseWithFactory
	{
		public void TestGetJobInvoicePrintingControlPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetJobInvoicePrintingControlPresentationProvider());
		}

		public void TestGetTransactionFilterStripControlPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionFilterStripControlPresentationProvider());
		}

		public void TestGetTransactionModuleStripPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionModuleStripPresentationProvider());
		}

		public void TestGetInvoiceFormPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoiceFormPresentationProvider());
		}

		public void TestGetEnquiryFilterControlPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetEnquiryFilterControlPresentationProvider());
		}

		public void TestGetInvoicePrintingControlPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoicePrintingControlPresentationProvider());
		}

		public void TestGetAPInvoicePrintingUserControlPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetAPJobInvoicePrintingUserControlPresentationProvider());
		}

		public void TestGetInvoicingPluginToFreightPresentationProvider_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoicingPluginToFreightPresentationProvider(new Mock<IClosedJobReopener>().Object, new Mock<IJobRevRecognitionDataRetriever>().Object));
		}
	}
}
