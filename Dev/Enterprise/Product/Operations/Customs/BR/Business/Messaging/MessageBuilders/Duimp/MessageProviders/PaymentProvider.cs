using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class PaymentProvider : IPayment
	{
		PaymentProvider(IEnumerable<CusEntryLineFee> fees)
		{
			this.fees = Argument.NotNull(fees, nameof(fees));
		}

		public static PaymentProvider New(IEnumerable<CusEntryLineFee> fees) => fees == null ? null : new PaymentProvider(fees);

		readonly IEnumerable<CusEntryLineFee> fees;

		public double TaxAmount => (double)fees.Sum(s => s.CF_ChargeAmount).Round(2);

		public string TaxType => MapChargeType(fees.FirstOrDefault().CF_ChargeType);

		string MapChargeType(string type)
		{
			switch (type)
			{
				case ChargeTypesList.Codes.DTY:
					return "II";
				case Constants.RateTypes.IPI:
					return "IPI";
				case Constants.RateTypes.PIS:
					return "PIS";
				case Constants.RateTypes.Cofins:
					return "COFINS";
				case Constants.RateTypes.Antidumping:
					return "ANTIDUMPING";
				default:
					return null;
			}
		}
	}
}
