using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class T2LReceptionResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LrecepcionV1Sal>
	{
		public T2LReceptionResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LrecepcionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LReceptionMessagePrettyFormatter(response);

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Reception Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LrecepcionV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lReception };
		protected override ZBool CanTriggerAnnexSending => true;

		protected override ZString ProcessAcceptedDeclaration(T2LrecepcionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			base.ProcessAcceptedDeclaration(response, message, entryHeader);

			SetAcceptanceDate(response, entryHeader);

			bool sentAnnexesCorrectly = TriggerMessageSendingCommon(message, ((IESMessageInfoProvider)entryHeader).Broker.GS_Code, entryHeader.Declaration.CusAgent, ShouldTriggerAnnexes(entryHeader), entryHeader, SendAnnex);

			if (!sentAnnexesCorrectly)
			{
				SetCHStatusAsReceived(entryHeader);
			}

			return ZString.Empty;
		}

		const string XsdSchemaNameT2LrecepcionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LrecepcionV1Sal.xsd";
	}
}
