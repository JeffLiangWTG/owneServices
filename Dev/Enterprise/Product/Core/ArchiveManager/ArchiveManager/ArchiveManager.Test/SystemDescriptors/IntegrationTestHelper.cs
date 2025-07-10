using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public static class IntegrationTestHelper
	{
		public static void RunArchiveStagesSortByMainDateFilterColumn(TestConfiguration configuration)
		{
			var system = TestHelpers.GetArchiveSystem(configuration.ArchiveSystemCodeToTest);
			var archiveBeforeDate = ZDateTime.Now;
			var configWithoutCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var configWithCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: true);

			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithCustoms);
			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithoutCustoms);
		}

		public static void RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfiguration configuration, BusinessObjectFactory factory, TestArchiveLogger archiveLogger)
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(configuration.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, true, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(factory, configuration.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(configuration.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule, new CancellationToken());

			var logLinesWithStageNames = archiveLogger.ListOfMessages.Where(line => line.StartsWith($"Information|{configuration.ArchiveSystemCodeToTest}|Executing: ")).ToList();

			Assertion.AssertEquals(configuration.ListOfStageNames.Count, logLinesWithStageNames.Count);

			for (var i = 0; i < configuration.ListOfStageNames.Count; i++)
			{
				var expectedLine = $"Information|{configuration.ArchiveSystemCodeToTest}|Executing: {configuration.ListOfStageNames[i]}";

				Assertion.AssertEquals(expectedLine, logLinesWithStageNames[i]);
			}
		}
	}
}
