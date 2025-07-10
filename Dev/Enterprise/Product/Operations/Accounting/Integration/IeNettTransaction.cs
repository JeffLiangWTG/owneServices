using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IeNettTransaction
	{
		ZGuid AH_GB { get; }
		ZGuid AH_GE { get; }
		ZGuid PK { get; }
		ZDateTime PostDate { get; }
		ZDecimal LocalExTaxAmount { get; }
		ZDecimal LocalTaxAmount { get; }
		ZDecimal ExchangeRate { get; }
		AccBankAccount BankAccount { get; }
		ZString AH_Desc { get; }
		ZString AH_TransactionNum { get; }
		ZDecimal AH_OSTotalAmount { get; }
		RefCurrency Currency { get; }
		OrgHeader Header { get; }
		Logs Logs { get; }
	}
}
