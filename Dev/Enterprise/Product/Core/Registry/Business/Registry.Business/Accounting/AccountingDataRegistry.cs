using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class AccountingDataRegistry : RegistryItemSet
	{
		#region Construction

		AccountingDataRegistry() { }

		public static AccountingDataRegistry Instance
		{
			get { return instance ?? (instance = new AccountingDataRegistry()); }
		}
		[ThreadStatic]
		static AccountingDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Accounting_GeneralLedgerDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("440EE890-54A9-4EDF-9B78-FCE6C96983C3", "General Ledger Defaults")); } }
		}

		#endregion

		#region MinimumIntervalSubLedgerTakeUp

		public MinimumIntervalSubLedgerTakeUpRegistryItem MinimumIntervalSubLedgerTakeUp
		{
			get
			{
				return GetItem("MinimumIntervalSubLedgerTakeUp", delegate
				{
					var result = new MinimumIntervalSubLedgerTakeUpRegistryItem(
							"MinimumIntervalSubLedgerTakeUp",
							Categories.Accounting_GeneralLedgerDefaults,
							(NoResString)"Minimum Interval for Sub Ledger Take Up",
							(NoResString)@"This registry controls the minimum interval in minutes for 'Allow Scheduling Automatic Sub-Ledger (A/R & A/P) Take up' system registry.

By default, this registy will be set to 15 minutes.
This value should only be adjusted if the client has raised an incident that this affects their operation.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							new MinimumIntervalSubLedgerTakeUp()
							);
					result.OnBuildLogReference += BuildMinimumIntervalSubLedgerTakeUpLogReference;

					return result;
				});
			}
		}

		string BuildMinimumIntervalSubLedgerTakeUpLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var config = args.NewValue as MinimumIntervalSubLedgerTakeUp;
			return Res.GetString("A22DE320-DA9B-4A29-97AE-8D2C43436DA2", "The interval has been updated to {0} {1}", config.Interval, config.IntervalType);
		}

		#endregion
	}
}
