using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class CANPreDUAImportResponseMessageProcessor : XMLResponseMessageProcessor<AnulaImportacionV1Sal, IMessagePrettyFormatter>
	{
		public CANPreDUAImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const string TransactionCommentPrefix = "IMP ";

		protected override string MessageFriendlyNameCore => (NoResString)"Cancel Pre SAD Import Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameAnulaImportacionV1Sal;

		protected override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;
		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation };

		protected override ZString ProcessAcceptedDeclaration(AnulaImportacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
			ResetGuaranteesAmountAndAddTransactionsForCancellation(entryHeader, TransactionCommentPrefix);

			return ZString.Empty;
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(AnulaImportacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new CANPreDUAMessagePrettyFormatter(response);

		const string XsdSchemaNameAnulaImportacionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.AnulaImportacionV1Sal.xsd";
	}
}
