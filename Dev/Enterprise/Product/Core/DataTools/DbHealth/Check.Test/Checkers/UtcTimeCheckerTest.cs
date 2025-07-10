using System;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(UtcTimeChecker))]
	sealed class UtcTimeCheckerTest : CheckerTestCaseBase
	{
		public void TestCheck()
		{
			utcTimeCheckerMock.Setup(u => u.GetReferenceUtc(It.IsAny<DbHealthWarningList>(), It.IsAny<ILogger>())).Returns(() => DateTime.UtcNow);

			var warningList = new DbHealthWarningList();
			utcTimeChecker.Check(Db.Connection, warningList, null);

			AssertEquals("No warnings expected", 0, warningList.Count);

			utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddMinutes(-13));
			utcTimeChecker.Check(Db.Connection, warningList, null);
			var expectedDescription = "Database Server UTC Date/Time is incorrect [";

			AssertEquals("Should be 1 warning", 1, warningList.Count);
			AssertStartsWith("Description", expectedDescription, warningList[0].Description);
			AssertMultilineASCIIEquals("Action",
				"Ensure the database server time is synchronized with the correct NTP servers and its time zone/daylight saving settings are configured correctly.\r\n" +
				$"Note: these are operating system settings (no action required in {BrandingFactory.Instance.ProductName}).",
				warningList[0].Action);

			warningList.Clear();
			utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddMinutes(-4));
			utcTimeChecker.Check(Db.Connection, warningList, null);

			AssertEquals("No warnings expected", 0, warningList.Count);
		}

		[UseSnapshotProtection]
		public void TestCheck_WhenRegistryNtpServersUsed()
		{
			var testServers = new[] { "pool.time.wtg.ws", "1.time.wtg.ws", "2.time.wtg.ws", "3.time.wtg.ws" };
			SystemDataRegistry.Instance.NtpTimeServers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testServers);

			var timeChecker = new UtcTimeChecker();
			var warningList = new DbHealthWarningList();

			var time = timeChecker.GetReferenceUtc(warningList, null);

			AssertNotNull("Time should not be null.", time);
			AssertNotEquals("Time should not be DateTime.MinValue.", DateTime.MinValue, time);
		}

		[UseSnapshotProtection]
		public void TestCheck_WhenNoRegistryNtpServersUsed()
		{
			var timeChecker = new UtcTimeChecker();
			var warningList = new DbHealthWarningList();

			var time = timeChecker.GetReferenceUtc(warningList, null);

			AssertNotNull("Time should not be null.", time);
			AssertNotEquals("Time should not be DateTime.MinValue.", DateTime.MinValue, time);
		}

		public void TestIgnoreTooBigDateDifference()
		{
			var warningList = new DbHealthWarningList();
			utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddMinutes(-13));
			utcTimeCheckerMock.Setup(u => u.GetReferenceUtc(It.IsAny<DbHealthWarningList>(), It.IsAny<ILogger>())).Returns(() => DateTime.UtcNow);

			utcTimeChecker.Check(Db.Connection, warningList, null);
			var expectedDescription = "Database Server UTC Date/Time is incorrect [";

			AssertEquals("Should be 1 warning", 1, warningList.Count);
			AssertStartsWith("Description", expectedDescription, warningList[0].Description);
			AssertMultilineASCIIEquals("Action",
				"Ensure the database server time is synchronized with the correct NTP servers and its time zone/daylight saving settings are configured correctly.\r\n" +
				$"Note: these are operating system settings (no action required in {BrandingFactory.Instance.ProductName}).",
				warningList[0].Action);

			warningList.Clear();

			utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddDays(-7));
			utcTimeChecker.Check(Db.Connection, warningList, null);
			AssertEquals("No warnings expected", 0, warningList.Count);
		}

		public void TestTimeoutMessageOnlyShowsAfterThreeFailures_SelfHosted()
		{
			TestTimeoutMessageOnlyShowsAfterThreeFailures_Core(false, "Please contact your IT administrator to ensure OS time is accurate. If NTP is required, ensure UDP traffic on port 123 is allowed and correct registry item is overwritten.");
		}

		public void TestTimeoutMessageOnlyShowsAfterThreeFailures_Hosted()
		{
			TestTimeoutMessageOnlyShowsAfterThreeFailures_Core(true, "Please contact your IT administrator If NTP is required, ensure UDP traffic on port 123 is allowed and correct registry item is overwritten.");
		}

		void TestTimeoutMessageOnlyShowsAfterThreeFailures_Core(bool isHosted, string expectedMessage)
		{
			try
			{
				var logger = new Mock<ILogger>();
				var warningList = new DbHealthWarningList();
				utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddMinutes(-13));
				utcTimeCheckerMock.Setup(u => u.GetUtcFromNTPService()).Throws(new TimeoutException("did not properly respond after a period of time"));
				utcTimeCheckerMock.SetupGet(u => u.IsHosted).Returns(isHosted);

				utcTimeChecker.GetReferenceUtc(warningList, logger.Object);
				AssertEquals("No warnings expected", 0, warningList.Count);
				logger.Verify(l => l.Log(LogType.Warning, It.IsAny<string>(), It.Is<Exception>(ex => ex.Message == "did not properly respond after a period of time")), Times.Once);
				logger.Invocations.Clear();

				utcTimeChecker.GetReferenceUtc(warningList, logger.Object);
				AssertEquals("No warnings expected", 0, warningList.Count);
				logger.Verify(l => l.Log(LogType.Warning, It.IsAny<string>(), It.Is<Exception>(ex => ex.Message == "did not properly respond after a period of time")), Times.Once);
				logger.Invocations.Clear();

				utcTimeChecker.GetReferenceUtc(warningList, logger.Object);
				AssertEquals("We Should get the timeout warning now", 1, warningList.Count);
				AssertEquals(expectedMessage, warningList[0].Action);
				logger.Verify(l => l.Log(LogType.Warning, It.IsAny<string>(), It.Is<Exception>(ex => ex.Message == "did not properly respond after a period of time")), Times.Once);
			}
			finally
			{
				UtcTimeChecker.ResetTimeout();
			}
		}

		public void TestNonTimeoutMessageShowsImmediately()
		{
			var logger = new Mock<ILogger>();
			var warningList = new DbHealthWarningList();
			utcTimeCheckerMock.Setup(u => u.GetDbServerUtc()).Returns(DateTime.UtcNow.AddMinutes(-13));
			utcTimeCheckerMock.Setup(u => u.GetUtcFromNTPService()).Throws(new Exception("Some random message"));

			utcTimeChecker.GetReferenceUtc(warningList, logger.Object);
			AssertEquals("We Should get warning on first failure", 1, warningList.Count);
			logger.Verify(l => l.Log(LogType.Warning, It.IsAny<string>(), It.Is<Exception>(ex => ex.Message == "Some random message")), Times.Once);
		}

		[ExpectNoExceptions]
		[SnailTest]
		public void TestRealGetReferenceUtc()
		{
			var timeChecker = new UtcTimeChecker();
			timeChecker.GetUtcFromNTPService();
		}

		public void TestRealGetReferenceUtcWithCheck()
		{
			var time1 = new DateTime(2011, 11, 11, 11, 11, 12);
			utcTimeCheckerMock.Setup(u => u.GetUtcFromNTPService()).Returns(time1);

			AssertEquals(time1, utcTimeChecker.GetReferenceUtc(new DbHealthWarningList(), Mock.Of<ILogger>()));
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return utcTimeChecker;
		}

		protected override void SetUp()
		{
			base.SetUp();

			utcTimeCheckerMock = new Mock<UtcTimeChecker> { CallBase = true };
			utcTimeChecker = utcTimeCheckerMock.Object;
		}

		Mock<UtcTimeChecker> utcTimeCheckerMock;
		UtcTimeChecker utcTimeChecker;
	}
}
