using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC013C_v515.CC013CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class AmendmentNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc013Cv1Sal, AmendmentNCTSMessagePrettyFormatter>
	{
		public AmendmentNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Amendment Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC013CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment };

		protected override ZBool SetPhaseStatusTo015 => true;

		protected override AmendmentNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc013Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new AmendmentNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc013Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			nctsHeader.BH_ReleaseStatus = "0";
			return ZString.Empty;
		}

		const string XsdSchemaNameCC013CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC013CV1Sal.xsd";
	}
}
