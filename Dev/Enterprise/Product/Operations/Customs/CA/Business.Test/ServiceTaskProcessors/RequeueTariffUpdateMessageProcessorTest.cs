using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RequeueTariffUpdateMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			using (var helper = new RefDbExtendedPropertyHelper())
			{
				var connection = helper.Connection;
				DataUtils.DropDbExtendedProperty(connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName);
				DataUtils.AddDbExtendedProperty(connection, RefDbExtendedPropertyHelper.LastUpdateDatePropertyName, "20160710");
				DataUtils.DropDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName);
				DataUtils.AddDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName, "Y");

				var messages = new EDIMessage[]
				{
				CreateMessage("CSF", new ZDateTime(2016, 7, 6)),
				CreateMessage("EXC", new ZDateTime(2016, 7, 7)),
				CreateMessage("GST", new ZDateTime(2016, 7, 8)),
				CreateMessage("TRF", new ZDateTime(2016, 7, 9)),
				CreateMessage("EXF", new ZDateTime(2016, 7, 10))
				};
				Factory.Save();

				var logger = new DummyLogger();
				var processor = new RequeueTariffUpdateMessageProcessor(logger);
				processor.Process();
				messages.ForEach(m => m.Reload());
				AssertEquals("No messages found, Refresh Required extended property should NOT be clear", "Y", DataUtils.LoadDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName));
				AssertEquals("messages[0] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[0].EM_Status);
				AssertEquals("messages[1] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[1].EM_Status);
				AssertEquals("messages[2] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[2].EM_Status);
				AssertEquals("messages[3] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[3].EM_Status);
				AssertEquals("messages[4] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[4].EM_Status);

				messages[3].EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 10);
				Factory.Save();
				processor.Process();
				messages.ForEach(m => m.Reload());
				AssertEquals("Messages found, Refresh Required extended property should be clear", "N", DataUtils.LoadDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName));
				AssertEquals("messages[0] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[0].EM_Status);
				AssertEquals("messages[1] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[1].EM_Status);
				AssertEquals("messages[2] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[2].EM_Status);
				AssertEquals("messages[3] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[3].EM_Status);
				AssertEquals("messages[4] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[4].EM_Status);

				messages[0].EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 10, 12, 0, 0);
				messages[1].EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 11);
				messages[2].EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 11);
				Factory.Save();
				processor.Process();
				messages.ForEach(m => m.Reload());
				AssertEquals("messages[0] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[0].EM_Status);
				AssertEquals("messages[1] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[1].EM_Status);
				AssertEquals("messages[2] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[2].EM_Status);
				AssertEquals("messages[3] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[3].EM_Status);
				AssertEquals("messages[4] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[4].EM_Status);

				DataUtils.SaveDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName, "Y");
				processor.Process();
				messages.ForEach(m => m.Reload());
				AssertEquals("messages[0] EM_Status should be changed", EDIMessage.Status.ProcessedOK, messages[0].EM_Status);
				AssertEquals("messages[1] EM_Status should be changed", EDIMessage.Status.Queued, messages[1].EM_Status);
				AssertEquals("messages[2] EM_Status should be changed", EDIMessage.Status.Queued, messages[2].EM_Status);
				AssertEquals("messages[3] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[3].EM_Status);
				AssertEquals("messages[4] EM_Status should NOT be changed", EDIMessage.Status.ProcessedOK, messages[4].EM_Status);
				AssertEquals("Messages found, Refresh Required extended property should be clear", "N", DataUtils.LoadDbExtendedProperty(connection, RefDbExtendedPropertyHelper.RefreshRequiredPropertyName));
				AssertEquals("Information - 2 tariff update messages were re-queued.", logger.Last());
			}
		}

		EDIMessage CreateMessage(ZString messageSubType, ZDateTime createTime)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = MessageTypeList.Codes.Query;
			message.EM_MessageSubType = messageSubType;
			message.EM_SystemCreateTimeUtc = createTime;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			return message;
		}
	}
}
