using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface ITaxProcessorCrossAssembly
	{
		(ZDate TaxExpenseDate, ZDecimal TaxExpenseAmount)[] GetTaxExpenses(BusinessObjectFactory factory, ZGuid linePK);
	}
}
