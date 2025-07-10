using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class DeclarationDVDResponseMessageProcessor : DVDCommonResponseMessageProcessor<Dvdh2V1Sal, DeclarationDVDMessagePrettyFormatter>
	{
		public DeclarationDVDResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"DVD (H2) Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameDVDH2V1Sal;

		protected override ZString AcceptedResponseCode => nameof(CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT.TdRespuesta.A);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2 };

		protected override DeclarationDVDMessagePrettyFormatter GetNewMessagePrettyFormatter(Dvdh2V1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new DeclarationDVDMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Dvdh2V1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				SetAcceptanceAndReleaseDataAndCircuitCan(entryHeader, correctResponseData.FechaAdmision + correctResponseData.HoraAdmision, correctResponseData.FechaLevante + correctResponseData.HoraLevante, correctResponseData.CsvLevante, message, correctResponseData.Circuito, correctResponseData.CircuitoAtc);

				SetEntryStatusAndTriggerInboxRequestIfNeededCommon(message, correctResponseData.CodigoOperacion, correctResponseData.CsvLevante, entryHeader);

				ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

				ProcessDocuments(entryHeader);
			}

			return ZString.Empty;
		}

		void ProcessDocuments(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessImportEntryLineSupportingDocuments();
			}
		}

		protected override List<TdGarantiaGrNutilizada> GetGuaranteesList(Dvdh2V1Sal response)
		{
			var guarantees = (response.Respuesta.GarantiaGrNutilizada ?? new Collection<TdGarantiaGrNutilizada>()).ToList();
			var guaranteesCan = (response.Respuesta.GarantiaGrNutilizadaAtc ?? new Collection<TdGarantiaGrNutilizada>()).ToList();
			guarantees.AddRange(guaranteesCan);
			return guarantees;
		}

		protected override void ProcessRejectedDeclaration(Dvdh2V1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		const string XsdSchemaNameDVDH2V1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming.DVDH2V1Sal.xsd";
	}
}
