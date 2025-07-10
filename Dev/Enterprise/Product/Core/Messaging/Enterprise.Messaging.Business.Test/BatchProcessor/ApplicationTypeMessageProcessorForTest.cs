using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.Testing
{
	class ApplicationTypeMessageProcessorForTest : ApplicationTypeMessageProcessor
	{
		public ApplicationTypeMessageProcessorForTest(ZGuid[] messagePKsToFailProcessingOn)
			: base(new LoggingInformation())
		{
			this.messagePKsToFailProcessingOn = messagePKsToFailProcessingOn;
		}

		readonly ZGuid[] messagePKsToFailProcessingOn;
		public int MessagesProcessed;
		public int MessageFilterAccesses;
		public int PostProcessOnExceptionCount;

		protected override string ApplicationCodeCore => "TST";

		protected override string MessageFriendlyNameCore => "test";

		protected override ZQuery MessageFilterCore
		{
			get
			{
				MessageFilterAccesses++;
				ZQuery result = base.MessageFilterCore;
				result.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, "XZX");
				return result;
			}
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			MessagesProcessed++;

			if (messagePKsToFailProcessingOn?.Contains(message.PK) ?? false)
			{
				//// Create EDIInterchange in Factory without essential fields EI_To and EI_From.
				message.Factory.New<EDIInterchange>();
				//// Will cause an exception only when this Factory is next saved.
			}

			if (message.EM_MessageText == TestConstants.HasLinkedObject && message.EM_LinkedObject == null)
			{
				var bizo = message.Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, message.EM_MessageNum));
				message.EM_LinkedObject = bizo[0];
			}

			message.EM_Status = EDIMessage.Status.Received;
		}

		protected override void PostProcessOnExceptionCore(EDIMessage message)
		{
			PostProcessOnExceptionCount++;
		}
	}
}
