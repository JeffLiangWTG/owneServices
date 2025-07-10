using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.Testing
{
	class BaseMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestAddNotInEDIMessageQueueStateQuery()
		{
			var message1 = CreateEDIMessage("1", Factory);
			var message2 = CreateEDIMessage("2", Factory, "T1T");
			var message3 = CreateEDIMessage("Test child fields aren't shortened".PadRight(EDIMessage.Schema.EM_MessageNumMaxLength), Factory);
			var message4 = CreateEDIMessage("4", Factory);
			Factory.Save();
			using (var cmd = Db.Connection.Command(@"INSERT EDIMessageQueueState (EQS_PK, EQS_ApplicationCode, EQS_EM, EQS_Keys, EQS_Status, EQS_ChainID, EQS_ParentMessageNumber, EQS_ParentSystemCreateTimeUtc, EQS_SystemCreateTimeUtc, EQS_SystemCreateUser, EQS_SystemLastEditTimeUtc, EQS_SystemLastEditUser)
VALUES (NEWID(), @ApplicationCode, @MessagePK, 'DEFAULT', 'PKE', '00000000-0000-0000-0000-000000000000', @MessageNum, @ParentSystemCreateTimeUtc, GETUTCDATE(), 'T', GETUTCDATE(), 'T')"))
			{
				cmd.AddParameter("@ApplicationCode", SqlDbType.VarChar, message3.EM_ApplicationCode.ToString());
				cmd.AddParameter("@MessagePK", SqlDbType.UniqueIdentifier, message3.PK.ToGuid());
				cmd.AddParameter("@MessageNum", SqlDbType.VarChar, message3.EM_MessageNum.ToString());
				cmd.AddParameter("@ParentSystemCreateTimeUtc", SqlDbType.DateTime, message3.EM_SystemCreateTimeUtc.ToDateTime());
				cmd.ExecuteNonQuery();
			}

			var newFactory = new BusinessObjectFactory();
			var query = BaseMessageProcessor.AddNotInEDIMessageQueueStateQuery(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "TST"));
			var messages = newFactory.Load<EDIMessage>(query);
			AssertEquals("messages.Length", 2, messages.Length);
			var messageA = messages[0];
			var messageB = messages[1];
			if (messageB.PK == message1.PK)
			{
				messageB = messages[0];
				messageA = messages[1];
			}
			AssertEquals("messageA.PK", message1.PK, messageA.PK);
			AssertEquals("messageB.PK", message4.PK, messageB.PK);
		}

		EDIMessage CreateEDIMessage(string messageNumber, BusinessObjectFactory factory, string applicationCode = "TST")
		{
			var message = factory.New<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = messageNumber;
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return message;
		}
	}
}
