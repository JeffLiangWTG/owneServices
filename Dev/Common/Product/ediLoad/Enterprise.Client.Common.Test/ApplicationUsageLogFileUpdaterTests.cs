using System;
using System.Collections.Generic;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.Common.Testing
{
	public class ApplicationUsageLogFileUpdaterTests : TestCase
	{
		public void TestApplicationUsageLogFileUpdated()
		{
			var logs = new List<ApplicationUsageLog>();
			var applicationUsageLogFile = new Mock<ApplicationUsageLogFile>();
			applicationUsageLogFile.Setup(x => x.LogUsage(It.IsAny<ApplicationUsageLog>()))
				.Callback<ApplicationUsageLog>(x => logs.Add(x));
			applicationUsageLogFile.Setup(x => x.RetrieveAllTheLogs()).Returns(logs);

			AssertEquals("There should not be any logs in the file before starting the test", 0, applicationUsageLogFile.Object.RetrieveAllTheLogs().Count);

			var results = new InstallationResultCollection();
			new ApplicationUsageLogFileUpdater(null, "TestServer", "TestDatabaase", applicationUsageLogFile.Object).Install(results);

			AssertEquals(expected: true, results[0].IsOK);
			AssertEquals("There should be a log in the file after the task runs", 1, applicationUsageLogFile.Object.RetrieveAllTheLogs().Count);
		}

		public void TestSuccessWhenFailedLog()
		{
			var applicationUsageLogFile = new Mock<ApplicationUsageLogFile>();
			applicationUsageLogFile.Setup(x => x.LogUsage(It.IsAny<ApplicationUsageLog>())).Throws(new Exception());

			var results = new InstallationResultCollection();
			AssertNoExceptionThrown(() => new ApplicationUsageLogFileUpdater(null, "TestServer", "TestDatabaase", applicationUsageLogFile.Object).Install(results));

			AssertEquals(expected: true, results[0].IsOK);
		}
	}
}
