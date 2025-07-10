using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3PresV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3DeclarationResponseMessageProcessor : G3CommonResponseMessageProcessor<G3PresV1Sal, G3DeclarationMessagePrettyFormatter>
	{
		public G3DeclarationResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"G3 Declaration Response Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.G3.Incoming.G3PresV1Sal.xsd";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.G3DeclarationOfGoods };

		protected override G3DeclarationMessagePrettyFormatter GetNewMessagePrettyFormatter(G3PresV1Sal response, EDIMessage message, AsycudaManifestHeader header) => new G3DeclarationMessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(G3PresV1Sal response, EDIMessage message, AsycudaManifestHeader header)
		{
			base.ProcessAcceptedResponse(response, message, header);
			foreach (var bill in GetBillsByG3Lrn(message.Factory, response.Accepted.Header.Lrn, false))
			{
				bill.G3MovementReferenceNumber = response.Accepted.Mrn;
			}
		}
	}
}
