using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Messaging.Business.Testing
{
	class MessageProcessorForTest : BaseMessageProcessor, IMessageProcessorForTest
	{
		public MessageProcessorForTest(ZGuid[] messagePKsToFailOn = null)
		{
			this.messagePKsToFailOn = messagePKsToFailOn;
		}

		public List<ApplicationTypeMessageProcessor> BaseGetMessageProcessorsForTest()
		{
			return base.GetMessageProcessors();
		}

		readonly ZGuid[] messagePKsToFailOn;

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = new List<ApplicationTypeMessageProcessor>();
			result.Add(new ApplicationTypeMessageProcessorForTest(messagePKsToFailOn));
			result.Add(new UDMMessageProcessorForTest(messagePKsToFailOn));
			return result;
		}

		protected override BusinessObjectFactory GetNewFactoryCore()
		{
			BusinessObjectFactory result = base.GetNewFactoryCore();
			result.Saved += new BusinessObjectFactory.SavedEventHandler((factory, saved) => FactorySaveCount++);
			return result;
		}

		public int FactorySaveCount { get; private set; }

		public new void Execute() => base.Execute(CancellationToken.None);

		public bool MessageShouldBeProcessedInASeparateFactoryExposed
		{
			get { return base.MessageShouldBeProcessedInASeparateFactory; }
		}

		public int MessagesProcessed
		{
			get { return ApplicationTypeProcessor.MessagesProcessed; }
			set { ApplicationTypeProcessor.MessagesProcessed = 0; }
		}

		public int ApplicationTypeProcessorFindCount => ApplicationTypeProcessor.MessageFilterAccesses;

		public ApplicationTypeMessageProcessorForTest ApplicationTypeProcessor => (ApplicationTypeMessageProcessorForTest)MessageProcessors[0];

		class UDMMessageProcessorForTest : ApplicationTypeMessageProcessorForTest
		{
			public UDMMessageProcessorForTest(ZGuid[] messagePKsToFailOn)
				: base(messagePKsToFailOn)
			{
			}

			protected override string ApplicationCodeCore => "UDM";
		}
	}
}
