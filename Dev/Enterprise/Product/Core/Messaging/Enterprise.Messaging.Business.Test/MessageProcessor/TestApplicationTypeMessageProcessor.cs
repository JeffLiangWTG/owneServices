using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	sealed class TestApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public TestApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore
		{
			get { return "Test Processor"; }
		}

		protected override string ApplicationCodeCore => "APP";

		protected override void ProcessMessageCore(EDIMessage message)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToExcludeCore => MessageTypesToExcludeExposed;
		public IReadOnlyList<ZString> MessageTypesToExcludeExposed;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => MessageTypesToIncludeExposed;
		public IReadOnlyList<ZString> MessageTypesToIncludeExposed;

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => MessageSubTypesToIncludeExposed;
		public IReadOnlyList<ZString> MessageSubTypesToIncludeExposed;
	}
}
