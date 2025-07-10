using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeResponseMessageProcessor : G3CommonResponseMessageProcessor<G3RevokeV1Sal, G3RevokeMessagePrettyFormatter>
	{
		public G3RevokeResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"G3 Revoke Response Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.G3.Incoming.G3RevokeV1Sal.xsd";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.G3RevocationOfGoods };

		protected override G3RevokeMessagePrettyFormatter GetNewMessagePrettyFormatter(G3RevokeV1Sal response, EDIMessage message, AsycudaManifestHeader header) => new G3RevokeMessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(G3RevokeV1Sal response, EDIMessage message, AsycudaManifestHeader header)
		{
			base.ProcessAcceptedResponse(response, message, header);
			foreach (var bill in GetBillsByG3Lrn(message.Factory, response.Accepted.Lrn, false))
			{
				bill.ABL_BillStatus = AISEntryStatusList.Codes.Prelodged;
				bill.G3RevokedMovementReferenceNumber = response.Accepted.Mrn;
			}
		}
	}
}
