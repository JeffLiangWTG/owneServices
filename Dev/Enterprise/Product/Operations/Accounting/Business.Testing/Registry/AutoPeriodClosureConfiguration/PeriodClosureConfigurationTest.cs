using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(PeriodClosureConfiguration))]
	class PeriodClosureConfigurationTest : RegistryBusinessObjectTemplateTestCase<PeriodClosureConfiguration>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		public void TestDefualtValue()
		{
			var periodClosureConfiguration = new PeriodClosureConfiguration();
			AssertNullOrEmpty(periodClosureConfiguration.IntervalType);
			AssertEquals(0, periodClosureConfiguration.SubLedgerInterval);
			AssertEquals(0, periodClosureConfiguration.GeneralLedgerInterval);
			AssertEquals(0, periodClosureConfiguration.AdjustmentLedgerInterval);

			periodClosureConfiguration = new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes);
			AssertEquals(PeriodClosureConfigurationIntervalType.Minutes, periodClosureConfiguration.IntervalType);
			AssertEquals(0, periodClosureConfiguration.SubLedgerInterval);
			AssertEquals(0, periodClosureConfiguration.GeneralLedgerInterval);
			AssertEquals(0, periodClosureConfiguration.AdjustmentLedgerInterval);
		}

		protected override PeriodClosureConfiguration GetBusinessObjectToClone()
		{
			return new PeriodClosureConfiguration(PeriodClosureConfigurationIntervalType.Minutes, 1, 2, 3);
		}

		protected override PeriodClosureConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
