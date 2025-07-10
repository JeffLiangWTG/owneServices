using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CustomsAndExciseReportInboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestProcessCustomsAndExciseReportInterchange()
		{
			var sessionGuid = ZGuid.BrettsGuid;
			var outgoingMessage = CustomsAndExciseReportInterchangeProcessorTestHelper.CreateOutgoingMessage(Factory, CustomsAndExciseReportTypeList.Codes.PSR);
			var outgoingInterchange = CustomsAndExciseReportInterchangeProcessorTestHelper.CreateOutgoingPSRInterchange(Factory, sessionGuid, outgoingMessage);
			var incomingInterchange = CustomsAndExciseReportInterchangeProcessorTestHelper.CreateIncomingPSRInterchange(Factory, sessionGuid);
			Factory.Save();

			new MessageRetrievingProcessor().ExecuteBatch();

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);

				var processedMessage = (EDIMessage)processedInterchange.ContainedMessages.First();
				AssertNotNull("Processed EDIMessage should exist.", processedMessage);
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertContains("EM_MessageText should contain json response", "{\"timestamp\":\"1660211772103\"", processedMessage.EM_MessageText);
				AssertEquals("Processed message should be linked to outgoing message", outgoingMessage.PK, processedMessage.EM_LinkUniqueID);
				AssertEquals("Processed message should be linked to outgoing message", outgoingMessage.TableName, processedMessage.EM_LinkTable);
			});
		}
	}
}
