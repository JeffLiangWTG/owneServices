using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Export.NotifPreDUAV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationExportMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter//IPrettyMessageFactoryExportNotif
	{
		public InboxNotificationExportMessagePrettyFormatter(NotifPreDuav1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly NotifPreDuav1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => CreateMessageDetailsTransactionDate();

		ZString CreateMessageDetailsTransactionDate()
		{
			ZDateTime.TryParseExact(response.FechaOperacion, out var transactionDate, CustomsDateTimeExtension.DateFormat);

			return TransactionDateText(transactionDate.ToCustomsFormatDateStringddMMyyyyWithDash()) + blankLine;
		}

		public ZString CreateMessageDetailsRejected()
		{
			if (!response.Rechazo.IsNullOrEmpty())
			{
				return SetMessageDetailsForRejectedMessage();
			}
			else if (response.Anulacion != null)
			{
				return SetMessageDetailsForCancelledMessage();
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString SetMessageDetailsForRejectedMessage()
		{
			var transactionDateDetails = CreateMessageDetailsTransactionDate();
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorErrorColumnText, ErrorLocationAndDescriptionColumnText);

			foreach (var errorText in response.Rechazo)
			{
				tableCreator.WriteRow(errorText.Codigo, errorText.Localizacion + ". " + errorText.Descripcion);
			}

			return transactionDateDetails + tableCreator.ToHtml();
		}

		ZString SetMessageDetailsForCancelledMessage()
		{
			var messageDetailsStringBuilder = new ZStringBuilder();
			messageDetailsStringBuilder.Append(CreateMessageDetailsTransactionDate());
			messageDetailsStringBuilder.Append(PreSADText);

			ZDateTime.TryParseExact(response.Anulacion.FechaAnulacion, out var cancellationDate, CustomsDateTimeExtension.DateFormat);

			messageDetailsStringBuilder.Append(DateText(cancellationDate.ToCustomsFormatDateStringddMMyyyyWithDash()));
			messageDetailsStringBuilder.Append(MotiveText(response.Anulacion.MotivoAnulacion));

			return messageDetailsStringBuilder.ToString();
		}

		string TransactionDateText(ZString transactionDate) => GetH4Text(ResString.GetMultilingualString("75199E7A-2604-49B1-9FBC-122F12F80322", "Transaction Date = {0}", transactionDate));
		string PreSADText => GetH3Text(ResString.GetMultilingualString("158BE00A-4C75-45C1-A863-39BC9069FD41", "Pre-SAD cancellation notification"));
		string DateText(ZString cancellationDate) => GetH3Text(ResString.GetMultilingualString("16FA71E7-544B-4B12-BB82-2153135B462F", "Date: {0}", cancellationDate));
		string MotiveText(ZString cancellationReason) => GetH3Text(ResString.GetMultilingualString("7A1C747C-C9B9-421E-8E90-51AE0F9E677D", "Motive: {0}", cancellationReason));
	}
}
