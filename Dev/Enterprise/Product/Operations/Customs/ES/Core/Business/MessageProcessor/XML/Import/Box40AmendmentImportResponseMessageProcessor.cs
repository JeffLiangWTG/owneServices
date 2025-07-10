using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ModificacionPdcCas40V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class Box40AmendmentImportResponseMessageProcessor : ImportGenericResponseMessageProcessor<ModificacionPdcCas40V1Sal>
	{
		public Box40AmendmentImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Import Amendment Box 40 Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameModificacionPdcCas40V1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportAmendmentBox40 };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ModificacionPdcCas40V1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ImportCommonMessagePrettyFormatter<ModificacionPdcCas40V1Sal>(response, entryHeader);

		protected override ZString ProcessAcceptedDeclaration(ModificacionPdcCas40V1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var pdaRegisteredOperationCodes = ImmutableHashSet.Create<ZString>(RegisteredOperationCodeDSPAccepted, RegisteredOperationCodePDSAccepted, RegisteredOperationCodeDSPAcceptedByComplementary);

			SetCircuitIfNeeded(response, entryHeader, pdaRegisteredOperationCodes);
			var entryStatus = GetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes);
			SetAcceptedDeclarationData(response, entryHeader, entryStatus, isSimplified: false);
			SetEntryStatus(response, entryHeader, message, pdaRegisteredOperationCodes, entryStatus);

			ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessImportEntryLinePreviousDocuments();
			}

			return ZString.Empty;
		}

		protected override Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(ModificacionPdcCas40V1Sal response) => ((IImportCommon)response).GRNGuaranteesCan;

		const string XsdSchemaNameModificacionPdcCas40V1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.ModificacionPdcCas40V1Sal.xsd";
	}
}
