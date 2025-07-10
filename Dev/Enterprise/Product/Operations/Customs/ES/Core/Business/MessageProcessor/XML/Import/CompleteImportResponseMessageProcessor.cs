using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class CompleteImportResponseMessageProcessor : ImportGenericResponseMessageProcessor<ImportacionCompletaV1Sal>
	{
		public CompleteImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Import Complete Pre-Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameImportacionCompletaV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ImportacionCompletaV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ImportCommonMessagePrettyFormatter<ImportacionCompletaV1Sal>(response, entryHeader);

		protected override ZString ProcessAcceptedDeclaration(ImportacionCompletaV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			UpdateEntryInstructionSubStyleFromCToY(entryHeader, message);

			SetCircuitIfNeeded(response, entryHeader, pdaRegisteredOperationCodes);
			var entryStatus = GetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes);
			SetAcceptedDeclarationData(response, entryHeader, entryStatus, isSimplified: false);
			SetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes, entryStatus);
			TriggerDocument031CaptureInGreenCircuitWithNoCsvClearance(entryHeader, message);

			ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessImportEntryLineSupportingDocuments();
				entryLine.ProcessImportEntryLinePreviousDocuments();
			}

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(ImportacionCompletaV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		protected override Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(ImportacionCompletaV1Sal response) => ((IImportCommon)response).GRNGuaranteesCan;

		static readonly ImmutableHashSet<ZString> pdaRegisteredOperationCodes = ImmutableHashSet.Create<ZString>
			(
				RegisteredOperationCodeDSPAccepted,
				RegisteredOperationCodePDSAccepted,
				RegisteredOperationCodeDSPAcceptedByComplementary
			);

		const string XsdSchemaNameImportacionCompletaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.ImportacionCompletaV1Sal.xsd";
	}
}
