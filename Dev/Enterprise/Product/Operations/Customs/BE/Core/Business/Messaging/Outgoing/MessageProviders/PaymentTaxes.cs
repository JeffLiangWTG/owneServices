using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class PaymentTaxes : ITPaymentTaxes
{
	public PaymentTaxes(CusEntryHeader entry)
	{
		Argument.NotNull(entry, CusEntryHeader.Schema.TableName);
		this.entry = entry;
	}

	readonly CusEntryHeader entry;

	public ZString DeferredPayment { get => entry.Declaration.JE_DefermentAccountNumber; }
	public ZString DeferredPaymentAccountHolder
	{
		get
		{
			return EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14.Equals(PaymentMethodTaxes) ?
				entry.RepresentativeOrDeclarantEoriOfMainOffice
				: entry.ImporterEoriOfMainOffice;
		}
	}
	public ZString PaymentMethodTaxes { get => entry.Declaration.JE_PaymentMethod; }
}
