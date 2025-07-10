using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IWriteOff
	{
		ZDecimal QuantityQuantity { get; }
		ZDecimal AmountAmount { get; }
		ZString QuantityQuantityUnitCode { get; }
	}

	class WriteOffWrapper : IWriteOff
	{
		WriteOffWrapper(ZDecimal quantityQuantity, ZDecimal amountAmount, ZString quantityQuantityUnitCode)
		{
			this.quantityQuantity = quantityQuantity;
			this.amountAmount = amountAmount;
			this.quantityQuantityUnitCode = quantityQuantityUnitCode;
		}

		public static WriteOffWrapper New(ZDecimal quantityQuantity, ZDecimal amountAmount, ZString quantityQuantityUnitCode)
		{
			return new WriteOffWrapper(quantityQuantity, amountAmount, quantityQuantityUnitCode);
		}

		ZDecimal IWriteOff.QuantityQuantity => quantityQuantity;

		ZDecimal IWriteOff.AmountAmount => amountAmount;

		ZString IWriteOff.QuantityQuantityUnitCode => quantityQuantityUnitCode;

		readonly ZDecimal quantityQuantity;
		readonly ZDecimal amountAmount;
		readonly ZString quantityQuantityUnitCode;
	}
}
