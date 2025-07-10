using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ComplXDVDResponseMessageProcessor : DVDCommonResponseMessageProcessor<DeclaComplemVinculV2Sal, ComplXDVDMessagePrettyFormatter>
	{
		public ComplXDVDResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}
		const string AcceptedCode = "0";

		protected override string MessageFriendlyNameCore => (NoResString)"DVD (H2) Compl. X Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameDeclaComplemVinculV2Sal;

		protected override ZString AcceptedResponseCode => AcceptedCode;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.TypeXDvdH2 };

		protected override ComplXDVDMessagePrettyFormatter GetNewMessagePrettyFormatter(DeclaComplemVinculV2Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ComplXDVDMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(DeclaComplemVinculV2Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

			LoggerTSGuarantee(entryHeader);
			ProcessDocuments(entryHeader);

			return ZString.Empty;
		}

		void ProcessDocuments(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessDVD5018EntryLineSupportingDocuments();
			}
		}

		protected override void ProcessRejectedDeclaration(DeclaComplemVinculV2Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		const string XsdSchemaNameDeclaComplemVinculV2Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming.DeclaComplemVinculV2Sal.xsd";
	}
}
