using System;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettInboundTransactionProcessorTransactionedTest : TransactionedTestCase
	{
		public void TestWebServiceUsesCorrectUrl()
		{
			eNettInboundTransactionProcessorForTesting processor = new eNettInboundTransactionProcessorForTesting { UseRealWebService = true };
			AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.microsoft.com/");
			AssertEquals("Should have referenced http://www.microsoft.com/", "http://www.microsoft.com/", processor.GetNewWebService_Exposed().Url);
			AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/");
			AssertEquals("Should have referenced http://www.cargowise.com/ now", "http://www.cargowise.com/", processor.GetNewWebService_Exposed().Url);
		}
	}

	public class eNettInboundTransactionProcessorForTesting : eNettInboundTransactionProcessor
	{
		public eNettInboundTransactionProcessorForTesting()
		{
		}

		public eNettInboundTransactionProcessorForTesting(string integrator)
			: base(integrator)
		{
		}

		protected override IeNettWebServiceClient GetNewWebService()
		{
			if (Globals.IsTest && !UseRealWebService)
			{
				return MockENettWebService.Instance.WebService;
			}
			else
			{
				return base.GetNewWebService();
			}
		}

		public IeNettWebServiceClient GetNewWebService_Exposed()
		{
			return GetNewWebService();
		}

		public bool UseRealWebService { get; set; }
	}
}
