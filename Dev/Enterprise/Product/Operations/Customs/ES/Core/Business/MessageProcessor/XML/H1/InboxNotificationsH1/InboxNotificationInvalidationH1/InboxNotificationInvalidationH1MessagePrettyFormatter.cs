using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ComunicaAnulacionV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business;

public class InboxNotificationInvalidationH1MessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
{
	public InboxNotificationInvalidationH1MessagePrettyFormatter(ComunicaAnulacionV1Sal response)
	{
		this.response = Argument.NotNull(response, nameof(response));
	}
	readonly ComunicaAnulacionV1Sal response;

	public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
	{
		var messageDetails = new StringBuilder();

		messageDetails.Append(InboxCommunicationText);

		var message = response.Message;
		var comunicaAnulacion = response.ComunicaAnulacion;
		var correctResponseData = comunicaAnulacion.ImportOperation;
		if (correctResponseData != null)
		{
			messageDetails.Append(blankLine);
			AppendDateddMMyyyyHHmmssWithDash(messageDetails, PreparationDateText, message.PreparationDateAndTime);
			AppendDataInNewTableIfNotEmpty(messageDetails, correctResponseData.Mrn.IsNullOrEmpty() ? RegistrationNumberText : ReferenceText
														 , correctResponseData.Mrn.IsNullOrEmpty() ? correctResponseData.CustomsRegistrationNumber : correctResponseData.Mrn);
			messageDetails.Append(blankLine);
			AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationByCustomsText, correctResponseData.InvalidationInitiatedByCustoms.Equals("1") ? InvalidationByCustomsYesText : InvalidationByCustomsNoText);
			AppendInvalidationDate(messageDetails, correctResponseData.InvalidationDecisionDateAndTime);
			AppendDateddMMyyyyHHmmssWithDash(messageDetails, InvalidationRequestedDateText, correctResponseData.InvalidationRequestDateAndTime);
			AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationReasonText, correctResponseData.InvalidationJustification);
		}

		return messageDetails.ToString();
	}

	void AppendInvalidationDate(StringBuilder messageDetails, ZString fieldData)
	{
		var tableCreator = GetNewNonVisibleTableCreator();
		DateTime.TryParse(fieldData, out var dateTime);
		AppendDateWithddMMyyyyFormatIfNotEmpty((ZDateTime)dateTime, tableCreator, InvalidationDateText);
		messageDetails.Append(tableCreator.ToHtml());
	}

	void AppendDateddMMyyyyHHmmssWithDash(StringBuilder messageDetails, ZString rowName, ZString fieldData)
	{
		DateTime.TryParse(fieldData, out var dateTime);
		AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, (ZDateTime)dateTime, rowName);
	}

	public ZString CreateMessageDetailsRejected() => ZString.Empty;

	protected string PreparationDateText => ResString.GetMultilingualString("2CA20B1F-68A7-4A82-A0EA-A42C5B56D389", "Preparation Date:");
	protected string RegistrationNumberText => ResString.GetMultilingualString("E64F5BF7-412D-4948-ABF2-D206BCFC43AE", "Register (CRN):");
	protected string InvalidationByCustomsText => ResString.GetMultilingualString("33BA3624-5895-4468-9E63-B47CF3C868D2", "Invalidation by Customs:");
}
