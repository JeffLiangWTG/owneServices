using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	public class BaseMessageProcessorNonTransactionedTest : TestCase
	{
		public void TestTransactionIsPresentRollsBackAnythingThatHappensOnTheCurrentConnection()
		{
			var factory = new BusinessObjectFactory();

			var message = factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "1901291";
			message.EM_ApplicationCode = "TST";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;

			AssertEquals("Precondition: GetCountOfMatchingMailItems()", 0, GetCountOfMatchingMailItems());

			factory.Saving += delegate
			{ throw new Exception(); };

			var messageProcessor = GetNewMessageProcessorForTestSendingAnEmail(factory);
			messageProcessor.ExecuteBatch();

			AssertEquals("GetCountOfMatchingMailItems()", 0, GetCountOfMatchingMailItems());
		}

		int GetCountOfMatchingMailItems()
		{
			return (int)Db.Connection.ExecuteScalar("select count(*) from " + MailDBItemsSchema.Constants.SqlSchemaName + "." + MailDBItemsSchema.Constants.TableName + " where " + MailDBItemsSchema.Constants.MI_Subject + " = '" + MessageProcessorForTestSendingAnEmail.UniqueString + "'");
		}

		protected virtual BaseMessageProcessor<EDIMessage> GetNewMessageProcessorForTestSendingAnEmail(BusinessObjectFactory overridingFactory)
		{
			return new MessageProcessorForTestSendingAnEmail(overridingFactory);
		}

		class MessageProcessorForTestSendingAnEmail : MessageProcessorForTest
		{
			public MessageProcessorForTestSendingAnEmail(BusinessObjectFactory overridingFactory)
			{
				this.overridingFactory = overridingFactory;
			}

			readonly BusinessObjectFactory overridingFactory;

			protected override BusinessObjectFactory GetNewFactoryCore() => overridingFactory;

			protected override void ProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
			{
				base.ProcessMessageCore(processor, message);
				var email = new EmailDef();
				email.AddRecipientForUserCommunication("really@unique.com");
				email.Subject = UniqueString;
				email.Body = "Unique Stuff";

				Env.Instance.OutgoingMailManager.Create(message.Factory, email);
			}

			public const string UniqueString = "{8200317A-9883-4ee9-805C-BD40AE3AFD2D}";
		}
	}
}
