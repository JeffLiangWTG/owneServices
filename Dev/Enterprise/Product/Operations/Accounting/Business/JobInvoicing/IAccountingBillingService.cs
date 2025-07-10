using System;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class BillingActionResult
	{
		public string ErrorMessage { get; private set; }
		public bool InputArgumentsHadError { get; private set; }
		public BillingActionResult(string errorMessage, bool inputArgumentsHadError)
		{
			ErrorMessage = errorMessage;
			InputArgumentsHadError = inputArgumentsHadError;
		}
	}

	public interface IAccountingBillingService
	{
		BillingActionResult CreateJobHeader(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk, Guid localClientAddressPk);

		BillingActionResult PostRevenue(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk);

		BillingActionResult PostCost(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk);

		BillingActionResult PostOverseasAgentCharges(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk);

		BillingActionResult SplitApportionAmount(Guid jobConsolCostPK, Guid staffPk, Guid branchPk, Guid departmentPk);
	}
}
