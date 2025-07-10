using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class BranchCustomsApplicationTypeMessageProcessorTestHelper : BranchCustomsApplicationTypeMessageProcessor
	{
		public BranchCustomsApplicationTypeMessageProcessorTestHelper(LoggingInformation logger, string applicationCode = "APP")
			: base(logger)
		{
			this.applicationCode = applicationCode;
		}

		protected override string MessageFriendlyNameCore => "Test Processor";

		protected override string ApplicationCodeCore => applicationCode;

		readonly string applicationCode;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.Received;
		}

		protected override IReadOnlyList<ZString> MessageTypesToExcludeCore => MessageTypesToExcludeExposed ?? System.Array.Empty<ZString>();
		public IReadOnlyList<ZString> MessageTypesToExcludeExposed;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => MessageTypesToIncludeExposed ?? System.Array.Empty<ZString>();
		public IReadOnlyList<ZString> MessageTypesToIncludeExposed;
	}
}
