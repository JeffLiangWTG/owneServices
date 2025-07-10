using System.Data;

namespace Enterprise.Integration.Billing
{
	public interface IStlTransactionFactory
	{
		IStlTransaction CreateTransaction(IStlScript script, DataRow dataRow);
	}
}
