using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionsAndDeductionWrapper : IAdditionsAndDeductions
	{
		AdditionsAndDeductionWrapper(string chargeType, decimal chargeAmount, string chargeCurrency)
		{
			this.chargeType = Argument.NotNull(chargeType, nameof(chargeType));
			this.chargeAmount = Argument.NotNull(chargeAmount, nameof(chargeAmount));
			this.chargeCurrency = Argument.NotNull(chargeCurrency, nameof(chargeCurrency));
		}
		readonly string chargeType;
		readonly decimal chargeAmount;
		readonly string chargeCurrency;

		public string Code => code ?? (code = chargeType);
		string code;

		public double Amount => amount != 0 ? amount : (amount = (double)chargeAmount);
		double amount;

		public string Currency => currency ?? (currency = chargeCurrency);
		string currency;

		public static AdditionsAndDeductionWrapper New(string chargeType, decimal chargeAmount, string chargeCurrency) => chargeType.IsNullOrEmpty() ? null : new AdditionsAndDeductionWrapper(chargeType, chargeAmount, chargeCurrency);
	}
}
