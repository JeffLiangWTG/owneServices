using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class EntryFee
	{
		public EntryFee() { }

		public EntryFee(ZString description, ZString chargeType, ZDecimal chargeAmount)
		{
			Description = description;
			ChargeType = chargeType;
			ChargeAmount = chargeAmount;
		}

		public EntryFee(ZString description, ZString chargeType, ZDecimal chargeAmount, ZDecimal baseValue, ZString methodOfCalculation, ZDecimal rate) : this(description, chargeType, chargeAmount)
		{
			BaseValue = baseValue;
			MethodOfCalculation = methodOfCalculation;
			Rate = rate;
		}

		public ZString Description { get; set; }
		public ZString ChargeType { get; set; }
		public ZDecimal BaseValue { get; set; }
		public ZString MethodOfCalculation { get; set; }
		public ZDecimal Rate { get; set; }
		public ZDecimal ChargeAmount { get; set; }
	}
}
