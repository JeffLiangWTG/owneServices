using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class PaymentVAT : ITPaymentVat
{
	public PaymentVAT(CusEntryHeader entry)
	{
		Argument.NotNull(entry, CusEntryHeader.Schema.TableName);
		this.entry = entry;
	}

	readonly CusEntryHeader entry;

	public ZString DeferredPaymentVat { get => entry.Declaration.ZG_VATDeferType; }
	public ZString PaymentMethodVat { get => entry.Declaration.ZG_VATDeferNumber; }
}
