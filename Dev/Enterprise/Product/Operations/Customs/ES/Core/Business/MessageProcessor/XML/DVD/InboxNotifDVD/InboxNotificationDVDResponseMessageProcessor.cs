using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ActivaPDCVinculacionV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationDVDResponseMessageProcessor : DVDCommonResponseMessageProcessor<ActivaPdcVinculacionV1Sal, InboxNotificationDVDMessagePrettyFormatter>
	{
		public InboxNotificationDVDResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification DVD (H2) Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameActivaPDCVinculacionV1Sal;

		protected override ZString AcceptedResponseCode => nameof(CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT.TdRespuesta.A);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 };

		protected override ZBool IsInboxDeclaration => true;

		protected override InboxNotificationDVDMessagePrettyFormatter GetNewMessagePrettyFormatter(ActivaPdcVinculacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationDVDMessagePrettyFormatter(response);

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			return MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<ActivaPdcVinculacionV1Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
		}

		protected override ZString ProcessAcceptedDeclaration(ActivaPdcVinculacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				SetEntryStatusAndTriggerInboxRequestIfNeededCommon(message, correctResponseData.CodigoOperacion, correctResponseData.CsvLevante, entryHeader);

				SetAcceptanceAndReleaseDataAndCircuitCan(entryHeader, correctResponseData.FechaAdmision + correctResponseData.HoraAdmision, correctResponseData.FechaLevante + correctResponseData.HoraLevante, correctResponseData.CsvLevante, message, correctResponseData.Circuito, correctResponseData.CircuitoAtc);

				ResetGuaranteesAmountAndAddTransactions(response, entryHeader);
			}

			return ZString.Empty;
		}

		protected override List<TdGarantiaGrNutilizada> GetGuaranteesList(ActivaPdcVinculacionV1Sal response)
		{
			var guarantees = (response.Respuesta.GarantiaGrNutilizada ?? new Collection<TdGarantiaGrNutilizada>()).ToList();
			var guaranteesCan = (response.Respuesta.GarantiaGrNutilizadaAtc ?? new Collection<TdGarantiaGrNutilizada>()).ToList();
			guarantees.AddRange(guaranteesCan);
			return guarantees;
		}

		const string XsdSchemaNameActivaPDCVinculacionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming.ActivaPDCVinculacionV1Sal.xsd";
	}
}
