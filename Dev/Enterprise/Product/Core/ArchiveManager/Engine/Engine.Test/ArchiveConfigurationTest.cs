using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine.Test
{
	public class ArchiveConfigurationTest : TestCaseWithFactory
	{
		public void TestGetConfigurationLogs()
		{
			var descriptor = new DummySimpleArchiveSystemDescriptor();
			var system = new ArchiveSystem(descriptor);
			var configuration = new ArchiveConfiguration(new ZDateTime(2023, 1, 15), 10, ZDateTime.Now, false, true);

			var logs = configuration.GetConfigurationLogs(system).ToList();

			AssertCommonConfigurationLogs(logs, 4, descriptor);
		}

		public void TestGetConfigurationLogs_WhenUseOnOrBeforeDateWhenWatermarkReset()
		{
			var descriptor = new PDOPurgeSystemDescriptor();
			var system = new ArchiveSystem(descriptor);
			var configuration = new ArchiveConfiguration(new ZDateTime(2023, 1, 15), 10, ZDateTime.Now, false, true, useOnOrBeforeDateWhenWatermarkReset: true);

			var logs = configuration.GetConfigurationLogs(system).ToList();

			CombineAssertions(() =>
			{
				AssertCommonConfigurationLogs(logs, 5, descriptor);
				AssertEquals("Use 'On Or Before' Date When Watermark Is Reset: Yes", logs[4]);
			});

			configuration = new ArchiveConfiguration(new ZDateTime(2023, 1, 15), 10, ZDateTime.Now, false, true, useOnOrBeforeDateWhenWatermarkReset: false);
			logs = configuration.GetConfigurationLogs(system).ToList();

			CombineAssertions(() =>
			{
				AssertCommonConfigurationLogs(logs, 5, descriptor);
				AssertEquals("Use 'On Or Before' Date When Watermark Is Reset: No", logs[4]);
			});
		}

		public void TestGetConfigurationLogs_WhenDateParameterIsJobOpenDate()
		{
			var descriptor = new DummyOPSArchiveSystemDescriptor();
			var system = new ArchiveSystem(descriptor);
			var configuration = new ArchiveConfiguration(new ZDateTime(2023, 1, 15), 10, ZDateTime.Now, false, true);
			configuration.SetIsFilteringByJobOpenDate(true);

			var logs = configuration.GetConfigurationLogs(system).ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Date Parameter: Job Open Date", logs[0]);
				AssertCommonConfigurationLogs(logs, 5, descriptor);
			});
		}

		public void AssertCommonConfigurationLogs(List<string> logs, int expectedLogCount, IArchiveSystemDescriptor descriptor)
		{
			AssertEquals(expectedLogCount, logs.Count);

			AssertCollectionContains("On or Before Log", $"{descriptor.PresentTenseVerb} Records on or Before: 15-Jan-2023", logs);
			AssertCollectionContains("Max Run Duration Log", "Max Run Duration: 10 minutes", logs);
			AssertCollectionContains("Verbose Logging Log", "Verbose Logging: No", logs);
			AssertCollectionContains("Incl. Customs Log", "Incl. Customs: Yes", logs);
		}
	}
}
