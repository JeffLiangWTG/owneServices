using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IDebitCreditAmounts
	{
		ZDecimal OSUnsignedLineAmount { get; set; }
		ZDecimal LocalUnsignedLineAmount { get; set; }
		ZString DebitCreditSign { get; set; }
	}
}
