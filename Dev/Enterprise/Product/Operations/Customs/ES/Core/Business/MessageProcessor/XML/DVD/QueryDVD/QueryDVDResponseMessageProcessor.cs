using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using TdRespuesta = CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Sal.TdRespuesta;

namespace Enterprise.Customs.ES.Business
{
	public class QueryDVDResponseMessageProcessor : DVDCommonResponseMessageProcessor<ConsultaDvdh2V1Sal, QueryDVDMessagePrettyFormatter>
	{
		public QueryDVDResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"DVD (H2) Query Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameConsultaDVDH2V1Sal;

		protected override ZString AcceptedResponseCode => nameof(CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT.TdRespuesta.A);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.DvdH2Query };

		protected override QueryDVDMessagePrettyFormatter GetNewMessagePrettyFormatter(ConsultaDvdh2V1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new QueryDVDMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ConsultaDvdh2V1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				SetEntryStatusAndTriggerInboxRequestIfNeeded(message, correctResponseData.SituacionDelDocumento, correctResponseData.CsvLevante, entryHeader);

				SetAcceptanceAndReleaseData(correctResponseData.FechaAdmision + correctResponseData.HoraAdmision, correctResponseData.FechaLevante + correctResponseData.HoraLevante, correctResponseData.CsvLevante, entryHeader, message);

				SetCircuitOrCircuitCan(correctResponseData, entryHeader);

				ResetGuaranteesAmountAndAddTransactions(response, entryHeader);
			}

			return ZString.Empty;
		}
		void SetEntryStatusAndTriggerInboxRequestIfNeeded(EDIMessage message, string status, string csvClearance, CusEntryHeader entryHeader)
		{
			switch (status)
			{
				case DVDResponseClearanceStatusCodeList.Codes.Incomplete:
					entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
					break;
				case DVDResponseClearanceStatusCodeList.Codes.PreDeclarationDvdH2:
					entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

					TriggerDVDInboxRequest(message, entryHeader);
					break;
				default:
					SetEntryHeaderCommon(csvClearance, entryHeader);
					break;
			}
		}

		void SetCircuitOrCircuitCan(TdRespuesta correctResponseData, CusEntryHeader entryHeader)
		{
			if (correctResponseData.Administracion == TdAdministracion.Atc)
			{
				entryHeader.SetCircuitCan(GetCircuitCode(correctResponseData.Circuito));
			}
			else
			{
				entryHeader.SetMovementReferenceNumberEntryStatus(GetCircuitCode(correctResponseData.Circuito));
			}
		}

		protected override List<TdGarantiaGrNutilizada> GetGuaranteesList(ConsultaDvdh2V1Sal response) => (response.Respuesta.GarantiaGrNutilizada ?? new Collection<TdGarantiaGrNutilizada>()).ToList();

		const string XsdSchemaNameConsultaDVDH2V1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.Incoming.ConsultaDVDH2V1Sal.xsd";

		protected sealed override ConsultaDvdh2V1Sal DererializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			try
			{
				return GetMessageProviderWithOrWithoutValidation(message, xsdSchemaEmbeddedResourceName, true);
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				if (ex.ToString().Contains(DocumentIdTag))
				{
					return GetMessageProviderWithOrWithoutValidation(message, xsdSchemaEmbeddedResourceName, false);
				}
				else
				{
					throw;
				}
			}
		}

		ConsultaDvdh2V1Sal GetMessageProviderWithOrWithoutValidation(EDIMessage message, string xsdSchemaEmbeddedResourceName, bool useValidation)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				if (useValidation)
				{
					return ESXmlObjectSerializer.DeserializeWithValidation<ConsultaDvdh2V1Sal>(xsdSchemaEmbeddedResourceName, bodyTextReader);
				}
				else
				{
					return ESXmlObjectSerializer.DeserializeWithoutValidation<ConsultaDvdh2V1Sal>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: false);
				}
			}
		}

		const string DocumentIdTag = "IdentificadorDocumento1";
	}
}
