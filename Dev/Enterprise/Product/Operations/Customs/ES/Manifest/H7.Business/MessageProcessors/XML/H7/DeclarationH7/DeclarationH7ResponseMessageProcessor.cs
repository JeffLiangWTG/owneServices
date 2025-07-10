using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class DeclarationH7ResponseMessageProcessor : H7CommonResponseMessageProcessor<AltaH7V1Sal, DeclarationH7MessagePrettyFormatter>
	{
		public DeclarationH7ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Declaration H7 Response Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.H7Declaration };

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.H7.Incoming.AltaH7V1Sal.xsd";

		protected override DeclarationH7MessagePrettyFormatter GetNewMessagePrettyFormatter(AltaH7V1Sal response, EDIMessage message, AsycudaBill bill) => new DeclarationH7MessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(AltaH7V1Sal response, EDIMessage message, AsycudaBill bill)
		{
			base.ProcessAcceptedResponse(response, message, bill);

			bill.DocumentationRequired = response.DocumentationRequired;
			SetMovementReferenceNumber(response, bill);
			SetReleaseDate(response, bill);
			SetClearanceCSV(response, bill, message);

			SendH7QueryMessageIfMrnNotEmpty(response.Mrn, message, bill);
		}
	}
}
