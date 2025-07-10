using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[UseSnapshotProtection]
	sealed class PerformanceStatisticsPersisterTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		protected override void SetUp()
		{
			base.SetUp();
			Assert("Precondition: Run against the proper DAT database", Factory.GetDatabaseCount(typeof(StmUsage)) == 0);
		}

		[TestDate(2012, 1, 1)]
		public void TestRecord()
		{
			var startDateUTC = ZDateTime.UtcNow.ToDateTime();
			var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);

			var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
			TestDateAttribute.Date = endDateUTC;
			Thread.Sleep(1000);
			token.Stop();

			AssertRecord(token, forceWrite: true, expectSaveAttempt: true);

			var usage = Factory.LoadTop1<StmUsage>(new ZQuery());
			var action = usage.UsageActionsSettings.First();
			CombineAssertions(delegate
			{
				AssertEquals("usage.XW_MachineName", System.Environment.MachineName, usage.XW_MachineName);
				AssertEquals("usage.XW_CompanyCode", "EDI", usage.XW_CompanyCode);
				AssertEquals("usage.XW_GB_NKBranchCode", "BNE", usage.XW_GB_NKBranchCode);
				AssertEquals("usage.XW_GS_NKStaffCode", "E", usage.XW_GS_NKStaffCode);
				AssertEquals("usage.XW_ExeVersion", ReleaseInfo.Instance.VersionNumber.ToString(), usage.XW_ExeVersion);
				AssertEquals("usage.XW_StartTimeUtc", startDateUTC, usage.XW_StartTimeUtc);
				AssertEquals("usage.XW_EndTimeUtc", endDateUTC, usage.XW_EndTimeUtc);

				AssertEquals("action.XO_Name", "name", action.Name);
				AssertEquals("action.XO_SubName", "This is a sexy test", action.SubName);
				AssertEquals("action.XO_ActionCount", 1, action.ActionCount);
				AssertEquals("action.XO_StartTimeUtc", startDateUTC, action.StartTimeUTC);
				AssertEquals("action.XO_EndTimeUtc", endDateUTC, action.EndTimeUTC);
				AssertEquals("action.XO_ElapsedWithoutChildrenSeconds >= 0.250 ?", true, action.ElapsedWithoutChildrenSeconds >= 0.250);
				AssertEquals("action.XO_ElapsedWithChildrenSeconds >= 0.250 ?", true, action.ElapsedWithChildrenSeconds >= 0.250);
			});
		}

		[TestDate(2012, 1, 1)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestRecordWithTemporaryUserContext()
		{
			ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = nameof(EnabledState.Detailed);
			TestPerformanceStatisticsCollector.ResetStatisticMode();
			PerformanceStatisticsCollector.ResetInstance();
			var userContext = new Mock<IUserContext>();
			var branch = new Mock<IBranch>();
			branch.Setup(br => br.Code).Returns("NBC");
			userContext.Setup(uc => uc.Branch).Returns(branch.Object);
			var company = new Mock<ICompany>();
			company.Setup(cp => cp.Code).Returns("NCC");
			userContext.Setup(uc => uc.Company).Returns(company.Object);

			var licence = new Mock<ILicenceProxy>();
			var language = String.Empty;
			licence.Setup(l => l.UseLanguageLicense(ref It.Ref<string>.IsAny, It.IsAny<LanguageUsageType>())).Returns((IDisposable)null);
			userContext.Setup(uc => uc.Licence).Returns(licence.Object);

			var user = Factory.New<IGlbStaff>();
			user.GS_Code = "NUI";
			userContext.Setup(uc => uc.User).Returns(user);

			var startDateUTC = ZDateTime.UtcNow.ToDateTime();
			var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
			var env = EnvProxy.Instance;
			using (env.SetTemporaryUserContext(userContext.Object))
			{
				AssertEquals("current branch changed", "NBC", env.CurrentBranch.Code);
				AssertEquals("current company changed", "NCC", env.CurrentCompany.Code);
				AssertEquals("currently logged in staff member changed", "NUI", env.CurrentUser.Initials);
				Assert("Precondition: Run against the proper DAT database", Factory.GetDatabaseCount(typeof(StmUsage)) == 0);

				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(1000);
				token.Stop();

				using (env.TemporaryServiceTaskContext("AUI", true))
				{
					AssertRecord(token, forceWrite: true, expectSaveAttempt: true);
				}
			}

			var usage = Factory.LoadTop1<StmUsage>(new ZQuery());
			var action = usage.UsageActionsSettings.First();
			CombineAssertions(delegate
			{
				AssertEquals("usage.XW_MachineName", System.Environment.MachineName, usage.XW_MachineName);
				AssertEquals("usage.XW_CompanyCode", "NCC", usage.XW_CompanyCode);
				AssertEquals("usage.XW_GB_NKBranchCode", "NBC", usage.XW_GB_NKBranchCode);
				AssertEquals("usage.XW_GS_NKStaffCode", "NUI", usage.XW_GS_NKStaffCode);
				AssertEquals("usage.XW_ExeVersion", ReleaseInfo.Instance.VersionNumber.ToString(), usage.XW_ExeVersion);
				AssertEquals("usage.XW_StartTimeUtc", startDateUTC, usage.XW_StartTimeUtc);
				AssertEquals("usage.XW_EndTimeUtc", endDateUTC, usage.XW_EndTimeUtc);

				AssertEquals("action.XO_Name", "name", action.Name);
				AssertEquals("action.XO_SubName", "This is a sexy test", action.SubName);
				AssertEquals("action.XO_ActionCount", 1, action.ActionCount);
				AssertEquals("action.XO_StartTimeUtc", startDateUTC, action.StartTimeUTC);
				AssertEquals("action.XO_EndTimeUtc", endDateUTC, action.EndTimeUTC);
				AssertEquals("action.XO_ElapsedWithoutChildrenSeconds >= 0.250 ?", true, action.ElapsedWithoutChildrenSeconds >= 0.250);
				AssertEquals("action.XO_ElapsedWithChildrenSeconds >= 0.250 ?", true, action.ElapsedWithChildrenSeconds >= 0.250);
			});
		}

		[TestDate(2015, 1, 1)]
		public void TestDoNotRecordIfNumberOfElementsLessThanThresholdAndNotForcing()
		{
			// Arrange
			var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
			var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
			TestDateAttribute.Date = endDateUTC;
			Thread.Sleep(1);
			token.Stop();

			// Act
			AssertRecord(token, forceWrite: false, expectSaveAttempt: false);
		}

		[TestDate(2015, 1, 1)]
		public void TestDatabaseUpgradeInProgressExceptionIsCorrectlyHandled()
		{
			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				var persister = new PerformanceStatisticsPersister { ExceptionThrownForTest = new DatabaseUpgradeInProgressException(), };

				var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(1);
				token.Stop();

				persister.Record(new[] { token, }, true);

				AssertNoExceptionThrown("DatabaseUpgradeInProgressException should not be left unhandled while processing", () => persister.ProcessingTask.Wait());
				AssertEquals("Token(s) to be processed should come back to the queue in case of DatabaseUpgradeInProgressException", 1, persister.QueueLength);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestDatabaseUpgradedExceptionIsCorrectlyHandled()
		{
			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				var processingFinishedEvent = new ManualResetEvent(false);
				var persister = new PerformanceStatisticsPersister { ExceptionThrownForTest = new DatabaseUpgradedException(), };

				var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(1);
				token.Stop();

				persister.Record(new[] { token, }, true);

				AssertNoExceptionThrown("DatabaseUpgradedException should not be left unhandled while processing", () => persister.ProcessingTask.Wait());
				AssertEquals("Token(s) to be processed should not come back to the queue as they are not processable in case of DatabaseUpgradedException", 0, persister.QueueLength);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestAdHocExceptionsAreCorrectlyHandled()
		{
			using (new DisposableAction(() => ErrorReporter.Clear()))
			{
				var processingFinishedEvent = new ManualResetEvent(false);
				var persister = new PerformanceStatisticsPersister { ExceptionThrownForTest = new InvalidOperationException("SomeInvalidOp"), };

				var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(1);
				token.Stop();

				persister.Record(new[] { token, }, true);

				AssertNoExceptionThrown("Ad-hoc non-critical exception should not be left unhandled while processing", () => persister.ProcessingTask.Wait());
				AssertEquals("ErrorReport should be generated by ad-hoc non-critical exception", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("ErrorReport should have the exception thrown", "SomeInvalidOp", ErrorReporter.LastExceptionReported.Message);
				AssertEquals("Token(s) to be processed should come back to the queue in case of ad-hoc non-critical exception", 1, persister.QueueLength);

				ErrorReporter.Clear();

				persister.ExceptionThrownForTest = new OutOfMemoryException();
				persister.Record(Array.Empty<IPerformanceStatisticCollectorToken>(), true);

				AssertNoExceptionThrown("Ad-hoc critical exception should not be left unhandled while processing", () => persister.ProcessingTask.Wait());
				AssertEquals("ErrorReport should not be generated by ad-hoc non-critical exception", 0, ErrorReporter.TotalErrorCount);
				AssertEquals("Token(s) to be processed should not come back to the queue in case of ad-hoc critical exception", 0, persister.QueueLength);
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestAggregatesElementsBeforeRecording()
		{
			// Arrange
			var persister = new PerformanceStatisticsPersister();
			var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
			List<PerformanceStatisticCollectorToken> tokens = new List<PerformanceStatisticCollectorToken>();

			for (int i = 0; i < 50; i++)
			{
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(1);
				token.Stop();
				tokens.Add(token);
			}

			AssertRecord(persister, tokens.ToArray(), forceWrite: false, expectSaveAttempt: false);

			var tokenToExceedThreshold = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
			TestDateAttribute.Date = endDateUTC;
			Thread.Sleep(1);
			tokenToExceedThreshold.Stop();

			// Act
			AssertRecord(persister, new[] { tokenToExceedThreshold }, forceWrite: false, expectSaveAttempt: true);

			// Assert
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmUsage)));
			var usage = Factory.LoadTop1<StmUsage>(new ZQuery());
			AssertEquals(51, usage.UsageActionsSettings.Count());
		}

		[TestDate(2012, 1, 1)]
		public void TestRecord_HideException()
		{
			ErrorReporter.Clear();

			var token = new Mock<IPerformanceStatisticCollectorToken>();
			token.Setup(t => t.ElapsedIncludingChildren).Throws(new InvalidOperationException());
			token.Setup(t => t.TimeStartedUtc).Returns(ZDateTime.UtcNow);
			token.Setup(t => t.TimeEndedUtc).Returns(ZDateTime.UtcNow);
			AssertRecord(token.Object, forceWrite: true, expectSaveAttempt: true);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		[TestDate(2015, 1, 1)]
		public void TestRecordTakesMaxMinTimeFromElementListWhenGroupSaving()
		{
			// Arrange
			var persister = new PerformanceStatisticsPersister();
			var endDateUTC = TestDateAttribute.Date.AddMilliseconds(250);
			List<PerformanceStatisticCollectorToken> tokens = new List<PerformanceStatisticCollectorToken>();

			for (int i = 0; i < 50; i++)
			{
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				TestDateAttribute.Date = endDateUTC;
				Thread.Sleep(10);
				token.Stop();
				tokens.Add(token);
			}

			var minStartTime = tokens.Min(t => t.TimeStartedUtc);
			var maxEndTime = tokens.Max(t => t.TimeEndedUtc);

			AssertRecord(persister, tokens.ToArray(), forceWrite: false, expectSaveAttempt: false);

			var tokenToExceedThreshold = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
			TestDateAttribute.Date = endDateUTC;
			Thread.Sleep(1);
			tokenToExceedThreshold.Stop();

			// Act
			AssertRecord(persister, new[] { tokenToExceedThreshold }, forceWrite: false, expectSaveAttempt: true);

			// Assert
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StmUsage)));
			var usage = Factory.LoadTop1<StmUsage>(new ZQuery());
			AssertEquals(minStartTime, usage.XW_StartTimeUtc);
			AssertEquals(maxEndTime, usage.XW_EndTimeUtc);
		}

		[TestDate(2017, 3, 1)]
		public void TestSqlExceptionTempdbOutOfSpace()
		{
			var performanceStatistics = new PerformanceStatisticsPersister();
			performanceStatistics.ExceptionThrownForTest = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(3958, 0, 0, string.Empty, "Transaction aborted when accessing versioned row in table 'dbo.StmUsage' in database 'Odyssey'. Requested versioned row was not found. Your tempdb is probably out of space. Please refer to BOL on how to configure tempdb for versioning.", string.Empty, 0)));
			var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
			token.Stop();
			AssertRecord(token, forceWrite: true, expectSaveAttempt: true);
			AssertEquals("Should not report the exception", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestSaveAlerterIsOverriddenOnSave()
		{
			var persister = new PerformanceStatisticsPersister();
			var waiter = new ManualResetEvent(false);
			persister.HookEndSave((_) => waiter.Set());

			using (new FactorySaveAlerter(() => "test", "is bad"))
			{
				var token = new PerformanceStatisticCollectorToken(action: "name", actionCount: 1);
				token.Stop();
				persister.Record(new[] { token }, true);
			}

			waiter.WaitOne();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		void AssertRecord(PerformanceStatisticsPersister persister, IPerformanceStatisticCollectorToken[] tokens, bool forceWrite, bool expectSaveAttempt)
		{
			var currentSaves = persister.SuccessfullSaves + persister.FailedSaves;
			persister.Record(tokens, forceWrite);
			// wait for async insert
			for (int i = 0; i < 10 && persister.SuccessfullSaves + persister.FailedSaves == currentSaves; ++i)
			{
				Thread.Sleep(100);
			}

			AssertEquals(expectSaveAttempt ? currentSaves + 1 : currentSaves, persister.SuccessfullSaves + persister.FailedSaves);
			if (!expectSaveAttempt)
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(StmUsage)));
			}
		}

		void AssertRecord(IPerformanceStatisticCollectorToken token, bool forceWrite, bool expectSaveAttempt)
		{
			AssertRecord(new PerformanceStatisticsPersister(), new[] { token }, forceWrite, expectSaveAttempt);
		}
	}
}
