using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class BranchMessageProcessorNonTransactionedTest : BaseMessageProcessorNonTransactionedTest
	{
		protected override BaseMessageProcessor<EDIMessage> GetNewMessageProcessorForTestSendingAnEmail(BusinessObjectFactory overridingFactory)
		{
			return new BranchMessageProcessorForTestSendingAnEmail(overridingFactory);
		}

		class BranchMessageProcessorForTestSendingAnEmail : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorForTestSendingAnEmail(BusinessObjectFactory overridingFactory)
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

			public const string UniqueString = "{617613D7-615D-4AC5-8F32-792C2A798D2C}";
		}
	}
}
