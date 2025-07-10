using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationAccrualSource
	{
		IEnumerable<ZGuid> GetJobParentPKs();

		IEnumerable<ZGuid> GetConsolPKs();

		IEnumerable<Charge> GetCharges(ZGuid jobParentId);

		IEnumerable<AccrualsForAPReconciliation<Charge>> GetChargeAccruals();

		IEnumerable<JobConsolCost> GetConsolCosts(ZGuid consolId);

		IEnumerable<AccrualsForAPReconciliation<JobConsolCost>> GetConsolCostAccruals();
	}
}
