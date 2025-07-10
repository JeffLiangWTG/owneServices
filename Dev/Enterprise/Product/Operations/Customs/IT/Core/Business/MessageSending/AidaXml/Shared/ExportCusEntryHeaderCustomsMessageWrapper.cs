using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

sealed class ExportCusEntryHeaderCustomsMessageWrapper : CusEntryHeaderCustomsMessageWrapper, ICusEntryHeaderCustomsMessageWrapper
{
	public ExportCusEntryHeaderCustomsMessageWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		var invoiceHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));

		InitializeLazy(entryHeader, invoiceHeader);
	}

	ITermsOfDelivery ICusEntryHeaderCustomsMessageWrapper.TermOfDelivery => lazyTermOfDelivery.Value;
	Lazy<ITermsOfDelivery> lazyTermOfDelivery;

	ZString ICusEntryHeaderCustomsMessageWrapper.DeferredPayment => lazyDeferredPayment.Value;
	Lazy<ZString> lazyDeferredPayment;

	void InitializeLazy(CusEntryHeader entryHeader, JobComInvoiceHeader invoiceHeader)
	{
		lazyTermOfDelivery = new Lazy<ITermsOfDelivery>(() => ExportTermsOfDeliveryWrapper.NewOrNull(entryHeader, invoiceHeader));
		lazyDeferredPayment = new Lazy<ZString>(() => GetDeferredPayment(entryHeader));
	}

	ZString GetDeferredPayment(CusEntryHeader entryHeader) => FeesHaveRequiredMethodOfPayment(entryHeader) ? base.GetDeferredPayment() : ZString.Empty;

	bool FeesHaveRequiredMethodOfPayment(CusEntryHeader entryHeader) =>
		entryHeader.MergedLines.Cast<CusEntryLine>()
		.Any(ml => ml.Fees.Cast<CusEntryLineFee>()
			.Any(elf => requiredMethodOfPaymentList.Contains(elf.CF_MethodOfPayment)));

	static readonly ImmutableArray<ZString> requiredMethodOfPaymentList = new ZString[]
	{
		UniversalReferenceConstants.DutyMethodOfPayment.OthersD,
		UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE,
		UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG
	}.ToImmutableArray();
}
