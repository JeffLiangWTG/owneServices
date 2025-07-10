using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class ExportOperationProvider : IExportOperation
{
	readonly CusEntryHeader cusEntryHeader;
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;
	readonly ExportEntryMessageSendingAction messageSendingAction;

	public ExportOperationProvider(ExportEntryMessageSendingAction messageSendingAction)
	{
		this.messageSendingAction = Argument.NotNull(messageSendingAction, nameof(messageSendingAction));
		this.cusEntryHeader = Argument.NotNull(messageSendingAction.Header, $"{nameof(messageSendingAction)}.{nameof(ExportEntryMessageSendingAction.Header)}");
		this.declaration = Argument.NotNull(cusEntryHeader.Declaration, $"{nameof(messageSendingAction)}.{nameof(cusEntryHeader)}.{nameof(CusEntryHeader.Declaration)}");
		this.entryInstruction = cusEntryHeader.EntryInstruction;
	}

	public string LRN => cusEntryHeader.CH_BGMReference;

	public string MRN => cusEntryHeader.MovementReferenceNumber;

	public DateTime InvalidationRequestDateTime => MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind();

	public string InvalidationReason => (messageSendingAction.TypeOfEntry == BEExportEntryTypeList.Codes.CancellationRequest) ? messageSendingAction.Justification : string.Empty;

	public string DeclarationType => declaration.JE_MessageSubType;

	public string AdditionalDeclarationType => entryInstruction?.CEI_SubStyle;

	public DateTime? PresentationOfTheGoodsDateAndTime =>
		declaration.ZG_PresentationStartDate.IsValid ? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(declaration.ZG_PresentationStartDate, removeMillisecond: true) : null;

	public string Security => MessageProviderHelper.ReturnNullIfEmpty(messageSendingAction.SecurityType);

	public string SpecificCircumstanceIndicator => CachedValueHelper.GetValue(ref specificCircumstanceIndicator, () => MessageProviderHelper.ReturnNullIfEmpty(declaration.ZG_SpecificCircumstanceIndicator));
	CachedValue<string> specificCircumstanceIndicator;

	public decimal? TotalAmountInvoiced => CachedValueHelper.GetValue(ref totalAmountInvoiced, () =>
	{
		return MessageProviderHelper.ReturnNullIfEmpty(GetTotalAmountInvoiced());
	});
	CachedValue<decimal?> totalAmountInvoiced;

	public string InvoiceCurrency => CachedValueHelper.GetValue(ref invoiceCurrency, () => !GetTotalAmountInvoiced().IsEmpty ? MessageProviderHelper.ReturnNullIfEmpty(cusEntryHeader.InvoiceCurrency) : null);
	CachedValue<string> invoiceCurrency;

	ZDecimal GetTotalAmountInvoiced()
	{
		const int allowedLengthOfDecimals = 2;
		ZDecimal result = cusEntryHeader.InvoiceHeaders.Sum(invoiceHeader => invoiceHeader.JZ_InvoiceAmount);
		return result.Round(allowedLengthOfDecimals);
	}
}
