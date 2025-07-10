using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.XTCredentialManagement.Inbound;

namespace ZClientEDI.Test.ServiceTasks.XTCredentialManagement.Inbound
{
	class XTCredentialManagementInboundProcessorTest : TestCaseWithFactory
	{
		public void TestQueryFilterAssociatedWithIndexNR_RX__EI_ReceiveTransmit_EI_ApplicationCode_EI_From_EI_SystemCreateTimeUtc_XTTRCVTRX()
		{
			var query = new ZQuery();
			ProcessorForTest.AddFilterExpose(query);

			AssertEquals("If you change the condition then please make sure to update index NR_RX__EI_ReceiveTransmit_EI_Status_EI_TransportType_EI_ApplicationCode_EI_From_EI_SystemCreateTimeUtc_XTTRCVTRX designed to optimize this query.",
				"EI_Status = 'QUE' and EI_IsActive = 1 and EI_ReceiveTransmit = 'RCV' and EI_ApplicationCode = 'XMS' and EI_From = 'XH' and (EI_TransportType = 'XTT' or EI_TransportType = 'TXT')",
				query.LiteralTextADO);
			AssertEquals("If you change the 'order by' then please make sure to update index NR_RX__EI_ReceiveTransmit_EI_Status_EI_TransportType_EI_ApplicationCode_EI_From_EI_SystemCreateTimeUtc_XTTRCVTRX designed to optimize this query.",
				"EI_SystemCreateTimeUtc",
				query.OrderBy);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ProcessorForTest = new XTCredentialManagementInboundProcessorForTest(Factory);
		}

		XTCredentialManagementInboundProcessorForTest ProcessorForTest;

		sealed class XTCredentialManagementInboundProcessorForTest : XTCredentialManagementInboundProcessor
		{
			public XTCredentialManagementInboundProcessorForTest(BusinessObjectFactory businessObjectFactory)
				: base(businessObjectFactory)
			{
			}

			public void AddFilterExpose(ZQuery filter) => AddFilter(filter);
		}
	}
}
