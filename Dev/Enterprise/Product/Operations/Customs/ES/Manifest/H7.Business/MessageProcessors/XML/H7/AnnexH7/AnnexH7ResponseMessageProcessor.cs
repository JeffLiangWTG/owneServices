using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AnnexH7ResponseMessageProcessor : H7CommonResponseMessageProcessor<EnvioDeDocumentosV1Sal, AnnexH7MessagePrettyFormatter>
	{
		public AnnexH7ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;

		protected override string MessageFriendlyNameCore => (NoResString)"Annex H7 Response Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.H7Annexes };

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.Incoming.EnvioDeDocumentosV1Sal.xsd";

		protected override AnnexH7MessagePrettyFormatter GetNewMessagePrettyFormatter(EnvioDeDocumentosV1Sal response, EDIMessage message, AsycudaBill bill) => new AnnexH7MessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(EnvioDeDocumentosV1Sal response, EDIMessage message, AsycudaBill bill)
		{
			base.ProcessAcceptedResponse(response, message, bill);
			SendH7QueryMessageIfMrnNotEmpty(bill.H7MovementReferenceNumber, message, bill);
		}

		protected override void ProcessRejectedResponse(EnvioDeDocumentosV1Sal response, EDIMessage message, AsycudaBill bill)
		{
			bill.ABL_MessageStatus = LogicalStatusList.Codes.Error;
		}
	}
}
