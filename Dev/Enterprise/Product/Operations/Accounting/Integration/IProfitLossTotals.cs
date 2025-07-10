using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public interface IProfitLossTotals
	{
		ZDecimal TotalAccrual { get; }
		ZPropertyInfo TotalAccrualInfo { get; }
		ZDecimal TotalAccrualRecognized { get; }
		ZPropertyInfo TotalAccrualRecognizedInfo { get; }
		ZDecimal TotalAccrualNotRecognized { get; }
		ZPropertyInfo TotalAccrualNotRecognizedInfo { get; }

		ZDecimal TotalWIP { get; }
		ZPropertyInfo TotalWIPInfo { get; }
		ZDecimal TotalWIPRecognized { get; }
		ZPropertyInfo TotalWIPRecognizedInfo { get; }
		ZDecimal TotalWIPNotRecognized { get; }
		ZPropertyInfo TotalWIPNotRecognizedInfo { get; }

		ZDecimal TotalCost { get; }
		ZPropertyInfo TotalCostInfo { get; }
		ZDecimal TotalCostRecognized { get; }
		ZPropertyInfo TotalCostRecognizedInfo { get; }
		ZDecimal TotalCostNotRecognized { get; }
		ZPropertyInfo TotalCostNotRecognizedInfo { get; }

		ZDecimal TotalRevenue { get; }
		ZPropertyInfo TotalRevenueInfo { get; }
		ZDecimal TotalRevenueRecognized { get; }
		ZPropertyInfo TotalRevenueRecognizedInfo { get; }
		ZDecimal TotalRevenueNotRecognized { get; }
		ZPropertyInfo TotalRevenueNotRecognizedInfo { get; }

		ZDecimal TotalLineAmount { get; }
		ZPropertyInfo TotalLineAmountInfo { get; }
		ZDecimal TotalLineAmountRecognized { get; }
		ZPropertyInfo TotalLineAmountRecognizedInfo { get; }
		ZDecimal TotalLineAmountNotRecognized { get; }
		ZPropertyInfo TotalLineAmountNotRecognizedInfo { get; }

		ZString MarginProfitRev { get; }
		ZPropertyInfo MarginProfitRevInfo { get; }
		ZString MarginProfitRevRecognized { get; }
		ZPropertyInfo MarginProfitRevRecognizedInfo { get; }
		ZString MarginProfitRevNotRecognized { get; }
		ZPropertyInfo MarginProfitRevNotRecognizedInfo { get; }

		ZString MarginProfitCost { get; }
		ZPropertyInfo MarginProfitCostInfo { get; }
		ZString MarginProfitCostRecognized { get; }
		ZPropertyInfo MarginProfitCostRecognizedInfo { get; }
		ZString MarginProfitCostNotRecognized { get; }
		ZPropertyInfo MarginProfitCostNotRecognizedInfo { get; }

		ZDecimal TotalTaxExpenseRevenue { get; }
		ZPropertyInfo TotalTaxExpenseRevenueInfo { get; }
		ZDecimal TotalTaxExpenseCost { get; }
		ZPropertyInfo TotalTaxExpenseCostInfo { get; }
	}
}
