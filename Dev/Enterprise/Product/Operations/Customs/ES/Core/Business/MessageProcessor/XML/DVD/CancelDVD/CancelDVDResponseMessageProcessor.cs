using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class CancelDVDResponseMessageProcessor : XMLResponseMessageProcessor<AnulaPdcVinculacionV1Sal, CancelDVDMessagePrettyFormatter>
	{
		public CancelDVDResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const string TransactionCommentPrefix = "DVD ";

		protected override string MessageFriendlyNameCore => (NoResString)"DVD (H2) Cancellation Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameAnulaPDCVinculacionV1Sal;

		protected override ZString AcceptedResponseCode => nameof(CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT.TdRespuesta.A);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2Cancellation };

		protected override CancelDVDMessagePrettyFormatter GetNewMessagePrettyFormatter(AnulaPdcVinculacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new CancelDVDMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(AnulaPdcVinculacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
			ResetGuaranteesAmountAndAddTransactionsForCancellation(entryHeader, TransactionCommentPrefix);

			return ZString.Empty;
		}

		const string XsdSchemaNameAnulaPDCVinculacionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming.AnulaPDCVinculacionV1Sal.xsd";
	}
}
