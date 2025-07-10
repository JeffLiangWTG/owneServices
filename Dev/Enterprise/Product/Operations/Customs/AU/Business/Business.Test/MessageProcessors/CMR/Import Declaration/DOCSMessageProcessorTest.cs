using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DOCSMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.DOCS;

		protected override ZString GetExpectedMessageName() => "Assessment Processing for Import Declarations (DOCS)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new DOCSMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRDOCSMessage);
	}
}
