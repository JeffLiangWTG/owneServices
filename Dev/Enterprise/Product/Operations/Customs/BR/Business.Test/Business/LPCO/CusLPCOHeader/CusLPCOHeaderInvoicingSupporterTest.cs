using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusLPCOHeaderInvoicingSupporter))]
	sealed class CusLPCOHeaderInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestGetJobInvoicingSecurityCore()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			AssertEquals("SecurityCheckpointToSendWithMessageError", Env.Security.BRLPCOJobInvoicing, lpcoHeader.InvoicingSupporter.JobInvoicingSecurity);
		}

		public void TestConsumerType()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.BRLPCO, lpcoHeader.InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusLPCOHeader>();
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Brazil; }
		}
	}
}
