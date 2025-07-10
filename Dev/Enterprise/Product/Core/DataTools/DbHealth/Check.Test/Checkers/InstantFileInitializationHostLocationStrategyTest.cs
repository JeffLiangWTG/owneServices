using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	sealed class InstantFileInitializationHostLocationStrategyTest : TestCase
	{
		readonly TestServiceLogger logger = new TestServiceLogger();

		public void TestIsInstantFileInitilizationEnabledReturnsTrueForWiseCloudHost()
		{
			var strategy = new InstantFileInitializationHostLocationStrategy();
			EnvProxy.SetHostedLocationForTest("WiseCloud");

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals("Should be true when WiseCloud hosted", result, true);
		}

		public void TestIsInstantFileInitilizationEnabledReturnsUndecidedForNonWiseCloudHost()
		{
			var strategy = new InstantFileInitializationHostLocationStrategy();
			EnvProxy.SetHostedLocationForTest(Constants.LicenceConstants.NotHostedWithCargoWise);

			var result = strategy.IsInstantFileInitializationEnabled(logger);

			AssertEquals("Should be undecided when not WiseCloud hosted", result, null);
		}
	}
}
