using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationAccrualFilterTypesProvider
	{
		IEnumerable<APReconciliationAccrualFilterTypes> GetFilterTypes();
	}

	public class APReconciliationAccrualFilterTypesProvider : IAPReconciliationAccrualFilterTypesProvider
	{
		IEnumerable<APReconciliationAccrualFilterTypes> IAPReconciliationAccrualFilterTypesProvider.GetFilterTypes()
		{
			return Enum.GetValues(typeof(APReconciliationAccrualFilterTypes))
				.OfType<APReconciliationAccrualFilterTypes>()
				.Where(IsEnabled)
				.OrderBy(f => f);
		}

		bool IsEnabled(APReconciliationAccrualFilterTypes filterType)
		{
			return filterType switch
			{
				APReconciliationAccrualFilterTypes.SettlementGroupCreditors => AccountingConfigurationRegistry.Instance
					.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value,
				APReconciliationAccrualFilterTypes.AllOtherCreditors => AccountingConfigurationRegistry.Instance
					.IncludeChargesForAllOtherCreditors.Value,
				_ => true
			};
		}
	}
}
