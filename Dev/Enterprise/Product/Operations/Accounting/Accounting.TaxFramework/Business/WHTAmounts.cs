using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IWithheldTaxAmounts
	{
		ZGuid TransactionPK { get; }

		ZDecimal NotionalAmount { get; }

		ZDecimal RealizedAmount { get; }
	}

	public class WithheldTaxAmounts : IWithheldTaxAmounts
	{
		public ZGuid TransactionPK { get; set; }

		public ZDecimal NotionalAmount { get; set; }

		public ZDecimal RealizedAmount { get; set; }
	}
}
