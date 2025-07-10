using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaControlesCCEV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationCceControlAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationCceControlAESMessagePrettyFormatter(ComunicaControlesCcev1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaControlesCcev1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("8C3F9B41-E350-4090-B68E-F70CF6FC7481", "CONCCE - Control needed at CCE");

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(InboxCommunicationText);

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMessageType(messageDetails, MessageTypeDescription);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, (ZDateTime)response.PreparationDateAndTime);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				messageDetails.Append(blankLine);
				AppendDateValueWithddMMyyyFormatInNewTableIfNotEmpty(messageDetails, correctResponseData.FechaNotificacionControl, ControlNotificationDateText);
				AppendDataInNewTableIfNotEmpty(messageDetails, NotificationTypeText, GetNotificationTypeDescription(correctResponseData.TipoNotificacion));
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
				AppendControlTypeData(messageDetails);
				AppendRequiredDocumentsData(messageDetails);
			}

			return messageDetails.ToString();
		}

		ZString GetNotificationTypeDescription(string notificationTypeCode)
		{
			var description = notificationTypeCode switch
			{
				AESInboxNotificationTypeCodeList.Codes.DecissionToControlAndRequestedDocumentsIfNeeded => AESInboxNotificationTypeCodeList.Descriptions.DecissionToControlAndRequestedDocumentsIfNeeded,
				AESInboxNotificationTypeCodeList.Codes.AdditionalDocumentsRequest => AESInboxNotificationTypeCodeList.Descriptions.AdditionalDocumentsRequest,
				AESInboxNotificationTypeCodeList.Codes.IntentionToControl => AESInboxNotificationTypeCodeList.Descriptions.IntentionToControl,
				_ => string.Empty
			};
			return notificationTypeCode + " - " + description;
		}

		void AppendControlTypeData(StringBuilder messageDetails)
		{
			var controlTypeDataList = response.TipoDeControl;
			if (controlTypeDataList != null && controlTypeDataList.Any())
			{
				messageDetails.Append(TypeOfControlText);

				var tableCreator = CreateNewListTable();

				foreach (var controlTypeData in controlTypeDataList)
				{
					tableCreator.WriteRow(controlTypeData.NumeroSecuencia, GetControlTypeDescription(controlTypeData.Tipo), controlTypeData.Texto);
				}

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		ZString GetControlTypeDescription(string controlTypeCode)
		{
			var description = controlTypeCode switch
			{
				AESInboxControlTypeCodeList.Codes.DocumentaryControls => AESInboxControlTypeCodeList.Descriptions.DocumentaryControls,
				AESInboxControlTypeCodeList.Codes.NuclearRadioactiveMaterialCheck => AESInboxControlTypeCodeList.Descriptions.NuclearRadioactiveMaterialCheck,
				AESInboxControlTypeCodeList.Codes.NonIntrusiveInspection => AESInboxControlTypeCodeList.Descriptions.NonIntrusiveInspection,
				AESInboxControlTypeCodeList.Codes.PhysicalControls => AESInboxControlTypeCodeList.Descriptions.PhysicalControls,
				AESInboxControlTypeCodeList.Codes.IdentificationOfConsignmentAndSeals => AESInboxControlTypeCodeList.Descriptions.IdentificationOfConsignmentAndSeals,
				AESInboxControlTypeCodeList.Codes.IntrusiveInspection => AESInboxControlTypeCodeList.Descriptions.IntrusiveInspection,
				AESInboxControlTypeCodeList.Codes.QuantityControlPartialOrTotal => AESInboxControlTypeCodeList.Descriptions.QuantityControlPartialOrTotal,
				AESInboxControlTypeCodeList.Codes.NatureAndCharacteristicsOfTheGoods => AESInboxControlTypeCodeList.Descriptions.NatureAndCharacteristicsOfTheGoods,
				AESInboxControlTypeCodeList.Codes.Sampling => AESInboxControlTypeCodeList.Descriptions.Sampling,
				AESInboxControlTypeCodeList.Codes.Other => AESInboxControlTypeCodeList.Descriptions.Other,
				_ => string.Empty
			};
			return controlTypeCode + " - " + description;
		}

		void AppendRequiredDocumentsData(StringBuilder messageDetails)
		{
			var documentList = response.DocumentoSolicitado;
			if (documentList != null && documentList.Any())
			{
				messageDetails.Append(RequiredDocumentsText);

				var tableCreator = CreateNewListTable();

				foreach (var doc in documentList)
				{
					tableCreator.WriteRow(doc.NumeroSecuencia, doc.TipoDocumento, doc.Descripcion);
				}

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		HtmlTableCreator CreateNewListTable()
		{
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ItemColumnText, TypeColumnText, DescriptionColumnText);
			return tableCreator;
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;

		string ControlNotificationDateText => ResString.GetMultilingualString("7F0A7B57-33B0-4D1C-8046-B8F3DBDD7D38", "Control Notification Date:");
		string NotificationTypeText => ResString.GetMultilingualString("BCEF9862-C35C-4048-B54B-9575C5A67B79", "Notification Type:");
		string TypeOfControlText => GetH4Text(ResString.GetMultilingualString("71F0E471-4F7E-458B-BB9F-7221EB7D44A7", "Type of Control"));
		string RequiredDocumentsText => GetH4Text(ResString.GetMultilingualString("67E3EA52-157A-426F-ACF6-D6C18BAB03F1", "Required Documents"));
	}
}
