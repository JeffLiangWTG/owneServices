using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public interface INettingTransactionReference
	{
		ZString Type { get; set; }
		ZString Reference { get; set; }
		ZGuid NettingTransactionPK { get; }
		ZGuid NettingPeriodPK { get; }
	}
}
