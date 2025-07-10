using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class T2LExpeditionAmendmentResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LexpedicionModificaV1Sal>
	{
		public T2LExpeditionAmendmentResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Expedition Amendment Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LexpedicionModificaV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lExpeditionAmendment };

		protected override ZBool ShouldChangeEntryStatus => false;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LexpedicionModificaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LExpeditionAmendmentMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(T2LexpedicionModificaV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			base.ProcessAcceptedDeclaration(response, message, entryHeader);

			SetCSVClearance(response, message, entryHeader);

			return ZString.Empty;
		}

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictT2LExpeditionAmendment(mrn, oldCSVClearance);

		const string XsdSchemaNameT2LexpedicionModificaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LexpedicionModificaV1Sal.xsd";
	}
}
