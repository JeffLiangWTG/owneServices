using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class InvoiceLineApportionedChargeWrapper : IInvoiceLineChargeWrapper
{
	public InvoiceLineApportionedChargeWrapper(BaseInvoiceLineApportionedCharge apportionedCharge)
	{
		Argument.NotNull(apportionedCharge, nameof(apportionedCharge));
		this.apportionedCharge = apportionedCharge;
	}

	#region IInvoiceLineChargeWrapper

	ZString IInvoiceLineChargeWrapper.ChargeCode => apportionedCharge.J7_ChargeType;
	ZDecimal IInvoiceLineChargeWrapper.Amount => apportionedCharge.J7_Amount;
	Money IInvoiceLineChargeWrapper.MoneyInLocalCurrency => apportionedCharge.MoneyInLocalCurrency;
	RefCurrency IInvoiceLineChargeWrapper.Currency => apportionedCharge.Currency;
	ZBool IInvoiceLineChargeWrapper.IsDutiable => apportionedCharge.J7_IsDutiable;
	ZBool IInvoiceLineChargeWrapper.IsIncludedInLine => apportionedCharge.J7_IsIncludedInITOT;
	CurrencyConverter IInvoiceLineChargeWrapper.CurrencyConverter => apportionedCharge.CurrencyConverter;
	ZGuid IInvoiceLineChargeWrapper.LineId => apportionedCharge.J7_ParentID;

	#endregion

	readonly BaseInvoiceLineApportionedCharge apportionedCharge;
}
