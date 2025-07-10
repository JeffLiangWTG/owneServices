using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class InvoiceLineChargeWrapper : IInvoiceLineChargeWrapper
{
	public InvoiceLineChargeWrapper(BaseInvoiceLineCharge charge)
	{
		Argument.NotNull(charge, nameof(charge));
		this.charge = charge;
	}

	#region IInvoiceLineChargeWrapper

	ZString IInvoiceLineChargeWrapper.ChargeCode => charge.J7_ChargeType;
	ZDecimal IInvoiceLineChargeWrapper.Amount => charge.J7_Amount;
	Money IInvoiceLineChargeWrapper.MoneyInLocalCurrency => charge.MoneyInLocalCurrency;
	RefCurrency IInvoiceLineChargeWrapper.Currency => charge.Currency;
	ZBool IInvoiceLineChargeWrapper.IsDutiable => charge.J7_IsDutiable;
	ZBool IInvoiceLineChargeWrapper.IsIncludedInLine => charge.J7_IsIncludedInITOT;
	CurrencyConverter IInvoiceLineChargeWrapper.CurrencyConverter => charge.CurrencyConverter;
	ZGuid IInvoiceLineChargeWrapper.LineId => charge.J7_ParentID;

	#endregion

	readonly BaseInvoiceLineCharge charge;
}
