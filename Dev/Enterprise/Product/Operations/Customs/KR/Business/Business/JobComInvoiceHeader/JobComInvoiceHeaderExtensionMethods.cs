using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public static class JobComInvoiceHeaderExtensionMethods
	{
		public static ZDecimal GetTotalChargesAmount(this JobComInvoiceHeader invoice, ZString chargeCode)
		{
			var result = new Money(0, invoice.LocalCurrency);
			result = (from BaseJobComInvHeaderCharge charge in invoice.Charges.Cast<BaseJobComInvHeaderCharge>().Concat(invoice.GroupCharges.Cast<BaseJobComInvHeaderCharge>())
								where charge.J7_ChargeType == chargeCode
								select charge).Aggregate(result, (Money current, BaseJobComInvHeaderCharge charge) => invoice.CurrencyConverter.Add(current, charge.Money));

			return invoice.CurrencyConverter.ConvertExact(result, invoice.LocalCurrency).Amount;
		}
	}
}
