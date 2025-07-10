using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.CompactGLAggragate;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.CompactGLAggregate.Testing
{
	[TestedType(typeof(CompactGLAggragateServiceTask))]
	public class CompactGLAggregateServiceTaskTest : ServiceTaskTestCase<CompactGLAggragateServiceTask>
	{
		#region CompactingTests

		public void TestCompactGLAggragateServiceTask_CompactsRows()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			CreateAggregateEntries(creator.GLHeader1.PK, periodHelper.CurrentPeriodInt, 5);
			Factory.Save();

			AssertCompactions(new [] {
			  new AssertCompaction {
				  Message = "Should compact",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = periodHelper.CurrentPeriodInt,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1 }
			});
		}

		public void TestCompactGLAggragateServiceTask_SkipsRowsIfDoesNotMeetThreshold()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			 
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0.51m);

			CreateAggregateEntries(creator.GLHeader1.PK, periodHelper.CurrentPeriodInt, 2);

			Factory.Save();

			AssertCompactions(new [] {
			  new AssertCompaction {
				  Message = "A ratio of 0.5 should not compact with threshold of 0.51",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = periodHelper.CurrentPeriodInt,
				  ExpectedCountBefore = 2,
				  ExpectedCountAfter = 2
			  }
			});
		}

		public void TestCompactGLAggragateServiceTask_CompactsRowsIfOnThreshold()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0.50m);

			CreateAggregateEntries(creator.GLHeader1.PK, periodHelper.CurrentPeriodInt, 2);

			Factory.Save();

			AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "A ratio of 0.5 should compact with threshold of 0.50",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = periodHelper.CurrentPeriodInt,
				  ExpectedCountBefore = 2,
				  ExpectedCountAfter = 1
			  }
			});
		}

		public void TestCompactGLAggragateServiceTask_AssertLogs()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			CreateAggregateEntries(creator.GLHeader2.PK, periodHelper.PreviousOpenPeriodInt, 3);
			CreateAggregateEntries(creator.GLHeader1.PK, periodHelper.CurrentPeriodInt, 5);
			Factory.Save();

			var mockStopwatch = new Mock<IStopwatch>();
			mockStopwatch.SetupGet(s => s.ElapsedMilliseconds).Returns(7L);
			ObjectFactory.Substitute(mockStopwatch.Object);

			var logger = AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "Precondition: Should compact",
				  GLHeaderPk = creator.GLHeader2.PK,
				  Period = periodHelper.PreviousOpenPeriodInt,
				  ExpectedCountBefore = 3,
				  ExpectedCountAfter = 1
			  },
			  new AssertCompaction {
				  Message = "Precondition: Should compact",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = periodHelper.CurrentPeriodInt,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1
			  }
			});

			var log = logger.ToString();
			AssertEqualsIgnoreLineBreaks("Log should output expected format:", $@"
Debug|
Generating ring took 7ms

Debug|
Compacting company: {GlbCompany.CurrentCompany.PK} {GlbCompany.CurrentCompany.GC_Code}, period: {periodHelper.PreviousOpenPeriodInt}
Calculation of compacting took 7ms
Total records: 3, Compacted records: 1, Ratio: {2 / 3d}

Debug|
Compacting took 7ms

Debug|
Compacting company: {GlbCompany.CurrentCompany.PK} {GlbCompany.CurrentCompany.GC_Code}, period: {periodHelper.CurrentPeriodInt}
Calculation of compacting took 7ms
Total records: 5, Compacted records: 1, Ratio: {4 / 5d}

Debug|
Compacting took 7ms

Information|
6 records compacted over 2 company/periods
Company/periods skipped: 3

Debug|
Service task finished in 7ms
Average compacting time: 7ms", log);
		}

		#endregion

		#region RingTests

		public void TestCompactGLAggragateServiceTask_InvalidFormatCompanyPeriodResetsRingPosition()
		{
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid");
			AssertRingPositionReset();
		}

		public void TestCompactGLAggragateServiceTask_InvalidGuidIntCompanyPeriodResetsRingPosition()
		{
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "bad guid|bad int");
			AssertRingPositionReset();
		}

		public void TestCompactGLAggragateServiceTask_NullCompanyPeriodResetsRingPosition()
		{
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertRingPositionReset();
		}

		void AssertRingPositionReset()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();
			var nextPeriod = companyPeriods.ElementAt(0);
			var nextNextPeriod = companyPeriods.ElementAt(1);

			CreateAggregateEntries(creator.GLHeader1.PK, nextPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, nextNextPeriod, 5);
			Factory.Save();

			// Setup so only one compaction occurs so we can see where it starts in the ring
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateRunDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);

			AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "Should compact first in ring",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = nextPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1
			  },
			  new AssertCompaction {
				  Message = "Should not compact any other company/period",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = nextNextPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 5
			  }
			});

			AssertEquals("Service task should set last company/period registry",
				$"{GlbCompany.CurrentCompany.PK}|{nextPeriod}", AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value);
		}

		public void TestCompactGLAggragateServiceTask_CompanyPeriodSetsRingStartingPosition()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();
			var lastPeriod = companyPeriods.ElementAt(companyPeriods.Count() / 2);
			var nextPeriod = companyPeriods.ElementAt(companyPeriods.Count() / 2 + 1);
			var nextNextPeriod = companyPeriods.ElementAt(companyPeriods.Count() / 2 + 2);

			SetCompanyPeriodRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), lastPeriod);

			CreateAggregateEntries(creator.GLHeader1.PK, lastPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, nextPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, nextNextPeriod, 5);
			Factory.Save();

			// Setup so only one compaction occurs so we can see where it starts in the ring
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateRunDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);

			AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "Should compact first iteration",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = nextPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1
			  },
			  new AssertCompaction {
				  Message = "Should not compact any other company/period",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = nextNextPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 5
			  },
			  new AssertCompaction {
				  Message = "Should not compact any other company/period",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = lastPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 5
			  }
			});

			AssertEquals("Service task should set last company/period registry",
				$"{GlbCompany.CurrentCompany.PK}|{nextPeriod}", AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value);
		}

		public void TestCompactGLAggragateServiceTask_RingCirclesBackToStartAndEndsAtFirstCompanyPeriod()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();
			var lastPeriod = companyPeriods.ElementAt(companyPeriods.Count() / 2);

			SetCompanyPeriodRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), lastPeriod);

			CreateAggregateEntries(creator.GLHeader1.PK, lastPeriod, 5);
			Factory.Save();

			AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "Should cycle through entire ring and compact last company/period",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = lastPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1
			  }
			});

			AssertEquals("Service task should set last company/period registry",
				$"{GlbCompany.CurrentCompany.PK}|{lastPeriod}", AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value);
		}

		#endregion

		#region ExceptionTests

		public void TestCompactGLAggragateServiceTask_NoCompanyPeriods()
		{
			var serviceTask = new CompactGLAggragateServiceTask();
			AssertNoExceptionThrown(() => { InitialiseAndRunTaskSchedule(serviceTask); });
		}

		public void TestCompactGLAggragateServiceTask_CancelCompacting()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();

			var lastPeriod = companyPeriods.ElementAt(0);
			var firstPeriod = companyPeriods.ElementAt(1);
			var nextPeriod = companyPeriods.ElementAt(2);

			SetCompanyPeriodRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), lastPeriod);

			CreateAggregateEntries(creator.GLHeader1.PK, firstPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, nextPeriod, 5);
			Factory.Save();

			var serviceTask = new CompactGLAggragateServiceTask();
			AssertExceptionThrown<OperationCanceledException>("Should throw operation cancelled exception", () => { InitialiseAndRunTaskSchedule(serviceTask, new CancellationToken(true)); });

			var (_, firstCount) = GetTotalAmountAndCountForAccountAndPeriod(creator.GLHeader1.PK, firstPeriod);
			AssertEquals("Should check token after transaction", 1, firstCount);
			var (_, nextCount) = GetTotalAmountAndCountForAccountAndPeriod(creator.GLHeader1.PK, nextPeriod);
			AssertEquals("No compaction should occur", 5, nextCount);

			AssertEquals("Service task should set last company/period registry to last before exception",
				$"{GlbCompany.CurrentCompany.PK}|{firstPeriod}", AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value);
		}

		public void TestCompactGLAggragateServiceTask_TimeoutStops()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();
			var lastPeriod = companyPeriods.ElementAt(0);
			var firstPeriod = companyPeriods.ElementAt(1);
			var nextPeriod = companyPeriods.ElementAt(2);

			CreateAggregateEntries(creator.GLHeader1.PK, firstPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, nextPeriod, 5);
			Factory.Save();

			var mockStopwatch = new Mock<IStopwatch>();
			mockStopwatch.SetupGet(s => s.ElapsedMilliseconds).Returns(7L);
			ObjectFactory.Substitute(mockStopwatch.Object);

			var error = SqlExceptionBuilder.CreateSqlError(-2, 1, 1, "Server", "Timeout", "Procedure", 0);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);

			var serviceTask = new CompactGLAggragateServiceTaskForTest(exception);
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			var (_, afterCountCurrent) = GetTotalAmountAndCountForAccountAndPeriod(creator.GLHeader1.PK, firstPeriod);
			var (_, afterCountPrevious) = GetTotalAmountAndCountForAccountAndPeriod(creator.GLHeader1.PK, nextPeriod);
			AssertEquals("Should rollback and exit without compacting", 5, afterCountCurrent);
			AssertEquals("Should rollback and exit without compacting", 5, afterCountPrevious);

			AssertEquals("Timeout should break loop", 1, serviceTask.CompactionTimesCalled);

			AssertEquals("Service task should set last company/period registry to last before exception",
				$"{GlbCompany.CurrentCompany.PK}|{lastPeriod}", AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.Value);

			AssertContains("SQL timeout occured after 7ms", logger.ToString());
		}

		public void TestCompactGLAggragateServiceTask_TotalDurationExceeded()
		{
			var creator = new TestObjectCreator(Factory);
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var companyPeriods = FetchPeriods();
			var firstPeriod = companyPeriods.ElementAt(0);
			var secondPeriod = companyPeriods.ElementAt(1);

			SetCompanyPeriodRegistry(Guid.Empty, 0);

			CreateAggregateEntries(creator.GLHeader1.PK, firstPeriod, 5);
			CreateAggregateEntries(creator.GLHeader1.PK, secondPeriod, 5);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateRunDuration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1m);
			var mockStopwatch = new Mock<IStopwatch>();
			mockStopwatch.SetupGet(s => s.ElapsedMilliseconds).Returns((long)TimeSpan.FromHours(1).TotalMilliseconds);
			ObjectFactory.Substitute(mockStopwatch.Object);

			var logger = AssertCompactions(new[] {
			  new AssertCompaction {
				  Message = "Should compact first iteration",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = firstPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 1
			  },
			  new AssertCompaction {
				  Message = "Should not compact as duration exceeded",
				  GLHeaderPk = creator.GLHeader1.PK,
				  Period = secondPeriod,
				  ExpectedCountBefore = 5,
				  ExpectedCountAfter = 5
			  }
			});

			AssertContains("Should contain timeout warning log", "Warning|Exit due to time out (3600000ms). If the duration is too small, please check registry Accounting -> Compact General Ledger Aggregate Service Task -> Maximum run time (in hours) of Compact General Ledger Aggregate Service Task", logger.ToString());
		}

		#endregion

		#region Helpers

		class AssertCompaction
		{
			public ZString Message { get; set; }
			public ZGuid GLHeaderPk { get; set; }
			public ZInt Period { get; set; }
			public ZInt ExpectedCountBefore { get; set; }
			public ZInt ExpectedCountAfter { get; set; }
		}

		TestServiceLogger AssertCompactions(AssertCompaction[] assertions)
		{
			var beforeTotals = new decimal[assertions.Length];

			for (var i = 0; i < assertions.Length; i++)
			{
				int beforeCount;
				(beforeTotals[i], beforeCount) = GetTotalAmountAndCountForAccountAndPeriod(assertions[i].GLHeaderPk, assertions[i].Period);
				AssertEquals("Precondition", assertions[i].ExpectedCountBefore, beforeCount);
			}

			var serviceTask = new CompactGLAggragateServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			for (var i = 0; i < assertions.Length; i++)
			{
				var (afterTotal, afterCount) = GetTotalAmountAndCountForAccountAndPeriod(assertions[i].GLHeaderPk, assertions[i].Period);
				AssertEquals(assertions[i].Message, assertions[i].ExpectedCountAfter, afterCount);
				AssertEquals("Total must be the same", beforeTotals[i], afterTotal);
			}

			return logger;
		}

		void CreateAggregateEntries(ZGuid glAccount, int period, int count, decimal amount = 100m, string transactionCategory = "", ZGuid? company = null, ZGuid? branch = null, ZGuid? department = null)
		{
			for (var i = 0; i < count; i++)
			{
				var aggregate = Factory.New<AccGLAggregate>();
				aggregate.AA_AG = glAccount;
				aggregate.AA_Period = period;
				aggregate.AA_GB = branch ?? GlbBranch.CurrentBranch.PK;
				aggregate.AA_GC = company ?? GlbCompany.CurrentCompany.PK;
				aggregate.AA_GE = department ?? GlbDepartment.CurrentDepartment.PK;
				aggregate.AA_Amount = amount;
				aggregate.AA_TransactionCategory = transactionCategory;
			}
		}

		(decimal amount, int count) GetTotalAmountAndCountForAccountAndPeriod(ZGuid glAccount, ZInt period)
		{
			var sql = "SELECT ISNULL(SUM(AA_Amount), 0), COUNT(1) FROM AccGLAggregate WHERE AA_AG = @AG AND AA_Period = @Period";
			var cmd = Db.Connection.Command(sql);
			cmd.AddParameter("@AG", SqlDbType.UniqueIdentifier, glAccount.ToGuid());
			cmd.AddParameter("@Period", SqlDbType.Int, (int)period);
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				return ((decimal)reader[0], (int)reader[1]);
			}
		}

		IEnumerable<int> FetchPeriods()
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = $@"
SELECT {AccPeriodManagementSchema.AM_GC_Company.Name}, {AccPeriodManagementSchema.AM_Period.Name}
FROM {AccPeriodManagementSchema.Constants.TableName}
JOIN {GlbCompanySchema.Constants.TableName} ON {GlbCompanySchema.PK.Name} = {AccPeriodManagementSchema.AM_GC_Company.Name}
ORDER BY {GlbCompanySchema.GC_Code.Name}, {AccPeriodManagementSchema.AM_Period.Name}";
			result.Load(sql);

			var companies = result.Select(r => Guid.Parse(r[AccPeriodManagementSchema.AM_GC_Company.Name].ToString())).Distinct();
			AssertEquals("Should only be one company testing with", 1, companies.Count());
			AssertEquals("Should only be one company testing with", GlbCompany.CurrentCompany.PK.ToGuid(), companies.First());

			var periods = result.Select(r => int.Parse(r[AccPeriodManagementSchema.AM_Period.Name].ToString()));

			return periods;
		}

		void SetCompanyPeriodRegistry(Guid company, int period)
		{
			AccountingConfigurationRegistry.Instance.AutoCompactAccGLAggregateLastCompanyPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, $"{company}|{period}");
		}

		class CompactGLAggragateServiceTaskForTest : CompactGLAggragateServiceTask
		{
			public CompactGLAggragateServiceTaskForTest(System.Data.Common.DbException exception)
			{
				CompactionTimesCalled = 0;
				ExceptionToThrow = exception;
			}

			readonly System.Data.Common.DbException ExceptionToThrow;
			public int CompactionTimesCalled;

			override protected void CompactCompanyPeriod(DbConnection connection, Guid companyPK, int period)
			{
				CompactionTimesCalled++;
				throw ExceptionToThrow;
			}
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
