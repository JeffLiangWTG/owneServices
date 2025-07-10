using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CancellationH7ResponseMessageProcessor : H7CommonResponseMessageProcessor<AnulaPreH7V1Sal, CancellationH7MessagePrettyFormatter>
	{
		public CancellationH7ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.H7.Incoming.AnulaPreH7V1Sal.xsd";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.H7Cancellation };

		protected override string MessageFriendlyNameCore => (NoResString)"H7 Cancellation Response Message Processor";

		protected override CancellationH7MessagePrettyFormatter GetNewMessagePrettyFormatter(AnulaPreH7V1Sal response, EDIMessage message, AsycudaBill bill) => new CancellationH7MessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(AnulaPreH7V1Sal response, EDIMessage message, AsycudaBill bill)
		{
			base.ProcessAcceptedResponse(response, message, bill);
			bill.ABL_BillStatus = AISEntryStatusList.Codes.Cancelled;
		}
	}
}
