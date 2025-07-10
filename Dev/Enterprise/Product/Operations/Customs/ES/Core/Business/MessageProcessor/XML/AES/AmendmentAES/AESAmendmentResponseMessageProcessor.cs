using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC513C_v514.CC513CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class AESAmendmentResponseMessageProcessor : AESCommonResponseMessageProcessor<Cc513Cv1Sal>
	{
		public AESAmendmentResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export Amendment Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC513CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportAmendmentUcc6 };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc513Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new AESAmendmentMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc513Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			ProcessDocuments(entryHeader);

			return ZString.Empty;
		}

		const string XsdSchemaNameCC513CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC513CV1Sal.xsd";
	}
}
