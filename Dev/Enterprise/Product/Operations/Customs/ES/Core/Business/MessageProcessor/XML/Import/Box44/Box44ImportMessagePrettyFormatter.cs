using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class Box44ImportMessagePrettyFormatter : ImportGenericCommonMessagePrettyFormatter<IBox44Import>, IMessagePrettyFormatter
	{
		public Box44ImportMessagePrettyFormatter(IBox44Import response, CusEntryHeader entryHeader) : base(response, entryHeader)
		{
		}

		protected override ZString CreateMessageDetailsAcceptedCore(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			AppendDeclarationData(messageDetails);
			AppendTaxesTitle(messageDetails);
			AppendManagementData(messageDetails);
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendTotalAmountToPayAndGuaranteedTotal(messageDetails, tableCreator);
			AppendATCAmountToPayAndGuaranteedTotal(messageDetails, tableCreator);
			AppendVATData(messageDetails, tableCreator);
			messageDetails.Append(tableCreator.ToHtml());
			var tableCreatorPayment = GetNewNonVisibleTableCreator();
			AppendPaymentInfoBox44(messageDetails, tableCreatorPayment);
			AppendPaymentInfoATCBox44(messageDetails, tableCreatorPayment);
			messageDetails.Append(tableCreatorPayment.ToHtml());
			messageDetails.Append(blankLine);
			AppendGuarantees(messageDetails, response.GRNGuarantees, response.GRNGuaranteesCan);
			AppendFees(messageDetails, true);
			AppendCertificates(messageDetails);

			return messageDetails.ToString();
		}

		void AppendManagementData(StringBuilder messageDetails)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			if (!response.ClearanceStatus.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, AEATDispatchStatusText, (response.ClearanceStatus + " - " + response.ClearanceStatusDescription));
			}
			if (!response.ClearanceStatusCan.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, ATCDispatchStatusText, (response.ClearanceStatusCan + " - " + response.ClearanceStatusDescriptionCan));
			}
			if (!response.AccountingStatus.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, AEATAccountingStatusText, (response.AccountingStatus + " - " + response.AccountingStatusDescription));
			}
			if (!response.AccountingStatusCan.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, ATCAccountingStatusText, (response.AccountingStatus + " - " + response.AccountingStatusDescription));
			}
			if (!response.UnfinishedPendencies.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, AEATUnfinishedPendenciesText, response.UnfinishedPendencies);
			}
			if (!response.UnfinishedPendenciesCan.IsEmpty())
			{
				WriteRowIfNotEmpty(tableCreator, ATCUnfinishedPendenciesText, response.UnfinishedPendenciesCan);
			}
			messageDetails.Append(tableCreator.ToHtml());
			messageDetails.Append(blankLine);
		}

		protected void AppendPaymentInfoBox44(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			messageDetails.Append(blankLine + PaymentInformationText + blankLine);
			if (!response.LimitPaymentDate.IsEmpty())
			{
				var limitPaymentDateCorrect = ZDateTime.TryParseExact(response.LimitPaymentDate, out var limitPaymentDate, CustomsDateTimeExtension.DateFormat);
				var limitDate = limitPaymentDateCorrect && !limitPaymentDate.IsEmpty ? limitPaymentDate : ZDateTime.MaxSmallDateTime;
				WriteRowIfNotEmpty(tableCreatorExternal, PaymentDateLimitText, limitDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected void AppendPaymentInfoATCBox44(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!response.LimitPaymentDateCan.IsEmpty())
			{
				var limitPaymentCanDateCorrect = ZDateTime.TryParseExact(response.LimitPaymentDateCan, out var limitPaymentCanDate, CustomsDateTimeExtension.DateFormat);
				var aTCLimitDate = limitPaymentCanDateCorrect && !limitPaymentCanDate.IsEmpty ? limitPaymentCanDate : ZDateTime.MaxSmallDateTime;
				WriteRowIfNotEmpty(tableCreatorExternal, ATCPaymentDateLimitText, aTCLimitDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected override void AppendAcceptanceDataIfNotEmpty(StringBuilder messageDetails, ZString serviceSegmentText, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(serviceSegmentText))
			{
				ZDateTime.TryParseExact(serviceSegmentText, out var acceptanceDate, CustomsDateTimeExtension.DateFormat);

				WriteRowIfNotEmpty(tableCreatorExternal, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}
		protected override ZDecimal GetTotalAmountToPayCan() => response.TotalAmountToPayCan;
		protected override ZDecimal GetTotalGuaranteedCan() => response.TotalGuaranteedCan;
	}
}
