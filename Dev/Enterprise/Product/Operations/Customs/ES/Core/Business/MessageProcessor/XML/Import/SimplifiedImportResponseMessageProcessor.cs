using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DeclaSimpliImporV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class SimplifiedImportResponseMessageProcessor : ImportGenericResponseMessageProcessor<DeclaSimpliImporV1Sal>
	{
		public SimplifiedImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Import Simplified Pre-Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameImportacionCompletaV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(DeclaSimpliImporV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ImportCommonMessagePrettyFormatter<DeclaSimpliImporV1Sal>(response, entryHeader);

		protected override ZString ProcessAcceptedDeclaration(DeclaSimpliImporV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var pdaRegisteredOperationCodes = ImmutableHashSet.Create<ZString>(RegisteredOperationCodePDSAccepted, RegisteredOperationCodePDSModification);

			SetCircuitIfNeeded(response, entryHeader, pdaRegisteredOperationCodes);
			var entryStatus = GetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes);
			SetAcceptedDeclarationData(response, entryHeader, entryStatus, isSimplified: true);
			SetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes, entryStatus);
			TriggerDocument031CaptureInGreenCircuitWithNoCsvClearance(entryHeader, message);

			ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessImportEntryLineSupportingDocuments();
			}

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(DeclaSimpliImporV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		protected override ZString GetEntryStatus(IImportCommon response, CusEntryHeader entryHeader, EDIMessage message, ImmutableHashSet<ZString> pdaRegisteredOperationCodes)
			=> pdaRegisteredOperationCodes.Contains(response.RegisteredOperationCode)
						? EntryStatusCodes.PreDeclarationAccepted
						: entryHeader.MovementReferenceNumberEntryStatus == CircuitCodeList.Codes.YELLOW
									? EntryStatusCodes.ClearedWithPendingDocuments
									: HasCSVClearance(response)
										? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations
										: EntryStatusCodes.CustomsDeclarationAccepted;

		protected override Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(DeclaSimpliImporV1Sal response) => ((IImportCommon)response).GRNGuaranteesCan;

		const string XsdSchemaNameImportacionCompletaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.ImportacionCompletaV1Sal.xsd";
	}
}
