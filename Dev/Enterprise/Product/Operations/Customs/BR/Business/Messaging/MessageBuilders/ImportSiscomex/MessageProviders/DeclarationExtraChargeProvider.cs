using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationExtraChargeProvider : IDeclarationExtraCharge
	{
		public DeclarationExtraChargeProvider(IEnumerable<JobComInvCharge> charges)
		{
			this.charges = Argument.NotNull(charges, nameof(charges));
			Argument.GreaterThanZero(charges.Count(), nameof(charges));

			randomCharge = this.charges.First();
		}
		readonly IEnumerable<JobComInvCharge> charges;
		readonly JobComInvCharge randomCharge;

		public static DeclarationExtraChargeProvider New(IEnumerable<JobComInvCharge> charges)
		{
			return charges == null || !charges.Any() ? null : new DeclarationExtraChargeProvider(charges);
		}

		public string ChargeCode => ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(randomCharge.J7_ChargeType);

		public string CurrencyCode => BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(randomCharge.Factory, randomCharge.J7_RX_NKCurrency);

		public decimal Amount => charges.Sum(charge => charge.Money.Amount);

		public decimal AmountInLocalCurrency => charges.Sum(charge => charge.MoneyInLocalCurrency.Amount);
	}
}
