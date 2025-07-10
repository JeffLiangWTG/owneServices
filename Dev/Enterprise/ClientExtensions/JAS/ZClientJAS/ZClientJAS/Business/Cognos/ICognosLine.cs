
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public interface ICognosLine
	{
		ZString AccountName { get; }
		ZString AccountCode { get; }
		ZString CounterCompany { get; }
		ZString Mode { get; }
		ZString Branch { get; }
		ZString Business { get; }
		ZDecimal Amount { get; }
		ZString TransactionCurrency { get; }
		ZDecimal TransactionAmount { get; }
		ZString Geographical { get; }
	}
}
