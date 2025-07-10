using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class PreDUAIncompleteImportResponseMessageProcessor : XMLResponseMessageProcessor<PreDeclaIncompletaV1Sal, IMessagePrettyFormatter>
	{
		public PreDUAIncompleteImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Incomplete Pre SAD Import Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNamePreDeclaIncompletaV1Sal;

		protected override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration };

		protected override ZString ProcessAcceptedDeclaration(PreDeclaIncompletaV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.IncompletePreDeclaration;

			return ZString.Empty;
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(PreDeclaIncompletaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new PreDUAIncompleteMessagePrettyFormatter(response, entryHeader);

		const string XsdSchemaNamePreDeclaIncompletaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.PreDeclaIncompletaV1Sal.xsd";
	}
}
