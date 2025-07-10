using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AccountingDataRegistry))]
	sealed class AccountingDataRegistryTest : RegistryItemSetTestCaseWithFactory<AccountingDataRegistry>
	{
		public void TestMinimumIntervalSubLedgerTakeUp()
		{
			var registry = ItemSet.MinimumIntervalSubLedgerTakeUp;

			AssertEquals("MinimumIntervalSubLedgerTakeUp", registry.Name);
			AssertEquals("Accounting/General Ledger Defaults", registry.Category);
			AssertEquals("Minimum Interval for Sub Ledger Take Up", registry.Caption);
			AssertEquals(@"This registry controls the minimum interval in minutes for 'Allow Scheduling Automatic Sub-Ledger (A/R & A/P) Take up' system registry.

By default, this registy will be set to 15 minutes.
This value should only be adjusted if the client has raised an incident that this affects their operation.",
				registry.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, registry.Storage);
			AssertEquals(15, registry.DefaultValue.Interval);
			AssertEquals("MINUTES", registry.DefaultValue.IntervalType);

			var oldValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var newValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			newValue.Interval = 12;
			newValue.IntervalType = MinimumIntervalSubLedgerTakeUp.IntervalTypes.Hours;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var result = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.OnBuildLogReference(args);
			AssertEquals($"The interval has been updated to {newValue.Interval} {newValue.IntervalType}", result);
		}
	}
}
