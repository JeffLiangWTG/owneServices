using System;

namespace Enterprise.Integration.Accounting
{
	public struct AutoRateOptions
	{
		public AutoRateOptions
			(
				bool autoRateCost = false,
				bool autoRateRevenue = false,
				bool excludeConsolLevelChargesOnCosting = false,
				bool standaloneShipmentOnly = false,
				bool enableAutoRateExplorer = false,
				AutoRateTriggerSource triggerSource = AutoRateTriggerSource.Unspecified,
				BillingType billingType = BillingType.Default
			)
			{
				AutoRateCost = autoRateCost;
				AutoRateRevenue = autoRateRevenue;
				ExcludeConsolLevelChargesOnCosting = excludeConsolLevelChargesOnCosting;
				StandaloneShipmentOnly = standaloneShipmentOnly;
				EnableAutoRateExplorer = enableAutoRateExplorer;
				TriggerSource = triggerSource;
				BillingType = billingType;

				currentAutoratingProcess = null;
			}

		public bool AutoRateCost { get; }
		public bool AutoRateRevenue { get; }
		public bool ExcludeConsolLevelChargesOnCosting { get; }
		public bool StandaloneShipmentOnly { get; }
		public bool EnableAutoRateExplorer { get; }
		public AutoRateTriggerSource TriggerSource { get; }
		public BillingType BillingType { get; }

		public CostSell AutoratingProcess
		{
			get
			{
				if (currentAutoratingProcess.HasValue)
				{
					return currentAutoratingProcess.Value;
				}

				if (AutoRateCost != AutoRateRevenue)
				{
					return AutoRateCost ? CostSell.Cost : CostSell.Revenue;
				}

				throw new NotSupportedException("AutoratingProcess hasn't specified yet!");
			}
			private set
			{
				currentAutoratingProcess = value;
			}
		}
		CostSell? currentAutoratingProcess;

		public static AutoRateOptions AutorateCosts =>
			new AutoRateOptions
			(
				autoRateCost: true
			);

		public static AutoRateOptions AutorateRevenue =>
			new AutoRateOptions
			(
				autoRateRevenue: true
			);

		public static AutoRateOptions AutorateCostsRevenue =>
			new AutoRateOptions
			(
				autoRateCost: true,
				autoRateRevenue: true
			);

		public AutoRateOptions With
			(
				bool? autoRateCost = null,
				bool? autoRateRevenue = null,
				bool? excludeConsolLevelChargesOnCosting = null,
				bool? standaloneShipmentOnly = null,
				bool? enableAutoRateExplorer = null,
				AutoRateTriggerSource? triggerSource = null,
				BillingType? billingType = null
			) =>
			new AutoRateOptions
			(
				autoRateCost: autoRateCost ?? AutoRateCost,
				autoRateRevenue: autoRateRevenue ?? AutoRateRevenue,
				excludeConsolLevelChargesOnCosting: excludeConsolLevelChargesOnCosting ?? ExcludeConsolLevelChargesOnCosting,
				standaloneShipmentOnly: standaloneShipmentOnly ?? StandaloneShipmentOnly,
				enableAutoRateExplorer: enableAutoRateExplorer ?? EnableAutoRateExplorer,
				triggerSource: triggerSource ?? TriggerSource,
				billingType: billingType ?? BillingType
			);

		public AutoRateOptions ToExecuteAutoratingCosts()
		{
			var newOptions = (AutoRateOptions)MemberwiseClone();
			newOptions.AutoratingProcess = CostSell.Cost;
			return newOptions;
		}

		public AutoRateOptions ToExecuteAutoratingRevenue()
		{
			var newOptions = (AutoRateOptions)MemberwiseClone();
			newOptions.AutoratingProcess = CostSell.Revenue;
			return newOptions;
		}
	}
}
