using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class T2LExpeditionResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LexpedicionV2Sal>
	{
		public T2LExpeditionResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Expedition Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LexpedicionV2Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lExpedition };
		protected override ZBool CanTriggerAnnexSending => true;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LexpedicionV2Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LExpeditionMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(T2LexpedicionV2Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			base.ProcessAcceptedDeclaration(response, message, entryHeader);

			SetCSVClearance(response, message, entryHeader);
			SetCircuit(response, entryHeader);
			SetAcceptanceDate(response, entryHeader);

			bool sentAnnexesCorrectly = TriggerMessageSendingCommon(message, ((IESMessageInfoProvider)entryHeader).Broker.GS_Code, entryHeader.Declaration.CusAgent, ShouldTriggerAnnexes(entryHeader), entryHeader, SendAnnex);

			if (!sentAnnexesCorrectly)
			{
				SetCHStatusAsReceived(entryHeader);
			}

			return ZString.Empty;
		}

		const string XsdSchemaNameT2LexpedicionV2Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LexpedicionV2Sal.xsd";
	}
}
