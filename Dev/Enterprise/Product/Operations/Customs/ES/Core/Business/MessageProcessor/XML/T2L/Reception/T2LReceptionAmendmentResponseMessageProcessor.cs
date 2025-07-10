using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionModificaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class T2LReceptionAmendmentResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LrecepcionModificaV1Sal>
	{
		public T2LReceptionAmendmentResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LrecepcionModificaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LReceptionAmendmentMessagePrettyFormatter(response);

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Reception Amendment Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LrecepcionModificaV1Sal;

		protected override ZBool ShouldChangeEntryStatus => false;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lReceptionAmendment };

		const string XsdSchemaNameT2LrecepcionModificaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LrecepcionModificaV1Sal.xsd";
	}
}
