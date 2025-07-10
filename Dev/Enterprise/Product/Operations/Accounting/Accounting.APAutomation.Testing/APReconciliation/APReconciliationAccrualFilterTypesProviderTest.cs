using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.Testing.APReconciliation.Testing
{
	public class APReconciliationAccrualFilterTypesProviderTest : TestCaseWithFactory
	{
		public void TestGetFilterTypes_MandatoryOnly()
		{
			IAPReconciliationAccrualFilterTypesProvider provider = new APReconciliationAccrualFilterTypesProvider();
			using (AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var filterTypes = provider.GetFilterTypes().ToArray();

				AssertEquals(2, filterTypes.Length);
				AssertEquals("1-MatchingCreditor", APReconciliationAccrualFilterTypes.MatchingCreditor, filterTypes[0]);
				AssertEquals("2-EmptyCreditor", APReconciliationAccrualFilterTypes.EmptyCreditor, filterTypes[1]);
			}
		}

		public void TestGetFilterTypes_All()
		{
			IAPReconciliationAccrualFilterTypesProvider provider = new APReconciliationAccrualFilterTypesProvider();
			using (AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterTypes = provider.GetFilterTypes().ToArray();

				AssertEquals(4, filterTypes.Length);
				AssertEquals("1-MatchingCreditor", APReconciliationAccrualFilterTypes.MatchingCreditor, filterTypes[0]);
				AssertEquals("2-EmptyCreditor", APReconciliationAccrualFilterTypes.EmptyCreditor, filterTypes[1]);
				AssertEquals("3-SettlementGroupCreditors", APReconciliationAccrualFilterTypes.SettlementGroupCreditors, filterTypes[2]);
				AssertEquals("4-AllOtherCreditors", APReconciliationAccrualFilterTypes.AllOtherCreditors, filterTypes[3]);
			}
		}
	}
}
