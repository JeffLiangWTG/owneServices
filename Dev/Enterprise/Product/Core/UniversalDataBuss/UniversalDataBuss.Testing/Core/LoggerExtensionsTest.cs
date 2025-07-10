using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class LoggerExtensionsTest : TestCaseWithUniversalObjectFactory
	{
		public void TestLogVerboseOnly()
		{
			var logger = new LoggingMatchResultForTesting();
			logger.Log(LogType.Information, "Howdy!");
			logger.LogVerboseOnly(LogType.Warning, "Registry Item is not on");
			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			logger.LogVerboseOnly(LogType.Warning, "Registry item is on!");
			AssertMultilineASCIIEquals("logger.Logs", @"
Howdy!
Warning - Registry item is on!
".Trim(), string.Join("\r\n", logger.GetMatchingLogsForTesting()));
		}

		class LoggingMatchResultForTesting : LoggingMatchResult<DummyBusinessObject>
		{
		}
	}
}
