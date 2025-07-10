using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public abstract class JCDActionStrategyTestBase : ServiceTaskTestCase<JobCostingDataPopulationServiceTask>
	{
		public void TestDBExceptionRetryAndLogging()
		{
			var logger = new TestServiceLogger();
			var strategy = new JCDActionStrategyToRaiseDBError_ForTest(TestConnection, logger, DbErrorType.LockTimeoutExpired);
			strategy.Process();
			AssertEquals("Immediate retry is not required, as DB may be too busy", false, strategy.ShouldRetry.Value);
			AssertEquals("Do not throw Exception. Immediate retry is not required, as DB may be too busy", false, strategy.ThrowOriginalException.Value);
			AssertContains("Information|Forced DB Lock Error", logger.ToString());

			strategy = new JCDActionStrategyToRaiseDBError_ForTest(TestConnection, logger, DbErrorType.TimeoutExpired);
			strategy.Process();
			AssertEquals("Immediately retry", true, strategy.ShouldRetry.Value);
			AssertEquals("Throw Exception", false, strategy.ThrowOriginalException.Value);
			AssertContains("Information|Forced Time Out Error", logger.ToString());

			strategy = new JCDActionStrategyToRaiseDBError_ForTest(TestConnection, logger, DbErrorType.GhostRecordsBeingDeleted);
			strategy.Process();
			AssertEquals("Immediately retry", false, strategy.ShouldRetry.Value);
			AssertEquals("Throw Exception", true, strategy.ThrowOriginalException.Value);
			AssertContains("Error|Forced GhostRecordsBeingDeleted error", logger.ToString());
		}

		#region Assertion Functions
		protected void AssertDBObjectExist(bool expectToExist)
		{
			AssertEquals("RptDtUnprocessedAccTransactionLines: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedAccTransactionLines"));
			AssertEquals("RptDtUnprocessedReversedAL: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedReversedAL"));
			AssertEquals("RptDt_TG_AccTransactionLines_InsertToReversedLinesTable: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDt_TG_AccTransactionLines_InsertToReversedLinesTable"));
			AssertEquals("RptDtTransformAccTransactionLineToJobCostingQueueRecord: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtTransformAccTransactionLineToJobCostingQueueRecord"));
			AssertEquals("RptDtJobCostingData: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtJobCostingData"));
			AssertEquals("RptDtPopulateJobCostingDataFromQueue: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtPopulateJobCostingDataFromQueue"));
			AssertEquals("RptDt_Report_GlobalJobProfitSummaryByJob: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDt_Report_GlobalJobProfitSummaryByJob"));
		}

		protected void AssertTempDBObjectExist(bool expectToExist)
		{
			AssertEquals("RptDtUnprocessedAccTransactionLines: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedAccTransactionLines"));
			AssertEquals("RptDtUnprocessedReversedAL: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedReversedAL"));
			AssertEquals("RptDt_TG_AccTransactionLines_InsertToReversedLinesTable: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDt_TG_AccTransactionLines_InsertToReversedLinesTable"));
			AssertEquals("RptDtTransformAccTransactionLineToJobCostingQueueRecord: Exist", expectToExist,
				DataUtils.ObjectExists(Helper.Connection, "RptDtTransformAccTransactionLineToJobCostingQueueRecord"));
		}

		// This is a temporary table. We do not have a schema object for this.
		protected void AssertTempALPKTableData(ZGuid[] expectedUnprocessedAccTransactionLines,
			ZGuid[] expectedUnprocessedReversedAccTransactionLines)
		{
			bool isExist = DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedAccTransactionLines");
			if (isExist)
			{
				var dt = DataUtils.GetDataTableFromQuery(Helper.Connection,
					"SELECT UL_ALPK FROM RptDtUnprocessedAccTransactionLines");

				if (dt != null)
				{
					AssertEquals("UnprocessedAccTransactionLines Line Count", expectedUnprocessedAccTransactionLines.Length,
						dt.Rows.Count);
					foreach (DataRow row in dt.Rows)
					{
						var linePK =
							new Guid(row["UL_ALPK"]
								.ToString());
						Assert($"UnprocessedAccTransactionLines Line should have this AL_PK : {linePK}",
							expectedUnprocessedAccTransactionLines.Any(x => x == linePK));
					}
				}
			}
			else
			{
				AssertEquals("No Row should be expected as table is deleted", expectedUnprocessedAccTransactionLines.Length, 0);
			}

			isExist = DataUtils.ObjectExists(Helper.Connection, "RptDtUnprocessedReversedAL");
			if (isExist)
			{
				var dt = DataUtils.GetDataTableFromQuery(Helper.Connection, "SELECT URL_ALPK FROM RptDtUnprocessedReversedAL");
				if (dt != null)
				{
					AssertEquals("UnprocessedAccTransactionLinesReversedAfterUpgrade Line Count",
						expectedUnprocessedReversedAccTransactionLines.Length, dt.Rows.Count);
					foreach (DataRow row in dt.Rows)
					{
						var linePK =
							new Guid(row["URL_ALPK"]
								.ToString());
						Assert($"UnprocessedAccTransactionLinesReversedAfterUpgrade Line should have this AL_PK : {linePK}",
							expectedUnprocessedReversedAccTransactionLines.Any(x => x == linePK));
					}
				}
			}
			else
			{
				AssertEquals("No Row should be expected as table is deleted",
					expectedUnprocessedReversedAccTransactionLines.Length, 0);
			}
		}

		// We do not have a schema object for this.
		protected void AssertQueueTableData(List<Tuple<Guid, DateTime, DateTime, Guid>> expectedQueuedValues)
		{
			var queueTable = DataUtils.GetDataTableFromQuery(Helper.Connection,
				"SELECT JCQ_ALPK, JCQ_PostDate, JCQ_ReverseDate, JCQ_GCPK FROM dbo.JobCostingDataQueue");
			if (queueTable != null)
			{
				AssertEquals("Transaction Line Count", expectedQueuedValues.Count, queueTable.Rows.Count);
				foreach (DataRow row in queueTable.Rows)
				{
					var linePK =
						new Guid(row["JCQ_ALPK"].ToString());
					var postDate = row["JCQ_PostDate"] == DBNull.Value
						? DateTime.MinValue
						: Convert.ToDateTime(row["JCQ_PostDate"]);
					var reverseDate = row["JCQ_ReverseDate"] == DBNull.Value
						? DateTime.MinValue
						: Convert.ToDateTime(
							row["JCQ_ReverseDate"]);
					var companyPK =
						new Guid(row["JCQ_GCPK"].ToString());

					Assert("Transaction Line should Exist", expectedQueuedValues.Any(x => x.Item1 == linePK
																						  && x.Item2.Date == postDate.Date
																						  && x.Item3.Date == reverseDate.Date
																						  && x.Item4 == companyPK));
				}
			}
		}

		// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
		protected IDisposable TrackJCDBObjectCheckerQueryCall(int expectedNumberOfCall, string explanation)
		{
			IDisposable commandTracker = null;
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			return new DisposableAction(() => commandTracker = TestConnection.TrackExecutedCommands()
										, () =>
										{
											var numberOfCall = TestConnection.ExecutedCommandsAndQueryPlans?.Count(t => t.Item1.Contains("--This is the query for loading JCD DB Object Info"));
											commandTracker?.Dispose();
											AssertEquals(explanation, expectedNumberOfCall, numberOfCall);
										});
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			AccountingConfigurationRegistry.Instance.JobCostingQueueDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
			AccountingConfigurationRegistry.Instance.JobCostingReportRelatedDBObjectVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
		}

		protected const string JCDDBObjectCheckerExplanationForSTR = @"Goes to the database four times -
1.	Before creating database objects required for processing existing AL records (JCDActionStrategy. SynchronizeDBObjectsCore())
2.	While changing JCDServiceTaskController registry status to ‘CDB’ validation process invokes a call to JCDDBObjectChecker (JCDStartActionStrategy.UpdateControllerRegistryStatusAfterDBSync())
3.	While creating partition keys (JCDActionStrategy .SynchronizePartitionKeysWithAccountingPeriods())
4.	After populating required database tables with existing AL records so that they can be transformed to report data, JCDServiceTaskController registry status is updated to ‘POR and validation process invokes a call to JCDDBObjectChecker (JCDActionStrategy. PopulateTempTables())";

		protected const string JCDDBObjectCheckerExplanationForPOR = @"Goes to the database two times -
1.	Once all existing AL records have been transformed to report data record, JCDServiceTaskController registry status is updated to ‘CUP’ and validation process invokes a call to JCDDBObjectChecker (JCDActionStrategy. PopulateTempTables())
2.	While dropping temporary JCD related database objects, JCD checks DB state which invokes a call to JCDDBObjectChecker (JCDDefaultActionStrategy.PopulateQueueFromOldALRecords())";

		protected const string JCDDBObjectCheckerExplanationForCUP = "JCDDBObjectChecker should not go to the database when JCDServiceTaskController registry status is ‘CUP’, as registry status reflects database state";

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccTransactionLinesSchema.Constants.TableName,
						null),
				};
			}
		}

		#region Properties

		protected JobCostingReportDataTestHelper Helper
		{
			get { return helper ?? (helper = new JobCostingReportDataTestHelper(ObjectCreator)); }
		}

		JobCostingReportDataTestHelper helper;

		protected TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator objectCreator;

		#endregion

		#region Inner Classes

		public class JobCostingDataPopulationServiceTask_ForTest : JobCostingDataPopulationServiceTask, IDisposable
		{
			public JobCostingDataPopulationServiceTask_ForTest(DbConnection connection)
			{
				if (!ObjectFactory.HasBeenSubstituted(nameof(IServiceTaskNudger)))
				{
					ObjectFactory.Substitute<IServiceTaskNudger>(new ServiceTaskNudger_ForTest(connection));
				}
				DisposableLeakListener.Instance.RegisterDisposable(this);
				this.connection = connection;
			}
			readonly DbConnection connection;

			protected override void RunTaskCore()
			{
				InitializeAndExecuteStrategey(connection);
			}

			public void Dispose()
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		public class ServiceTaskNudger_ForTest : IServiceTaskNudger
		{
			public ServiceTaskNudger_ForTest(DbConnection connection)
				: base()
			{
				this.connection = connection;
			}
			readonly DbConnection connection;

			public void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
			{
				if (throwError)
				{
					throw new AggregateException(new Exception("Forced to fail Nudging"));
				}
				else if (allowNudging)
				{
					QueuedServiceTask.Enqueue(new JobCostingDataPopulationServiceTask_ForTest(connection));
				}
			}

			public Queue<JobCostingDataPopulationServiceTask_ForTest> QueuedServiceTask
			{
				get
				{
					if (queuedServiceTask == null)
					{
						queuedServiceTask = new Queue<JobCostingDataPopulationServiceTask_ForTest>();
					}

					return queuedServiceTask;
				}
			}
			Queue<JobCostingDataPopulationServiceTask_ForTest> queuedServiceTask;

			public DisposableAction AllowNudging()
			{
				allowNudging = true;
				return new DisposableAction(() => allowNudging = false);
			}
			bool allowNudging;

			public DisposableAction RaiseExceptionWhileNudging()
			{
				throwError = true;
				return new DisposableAction(() => throwError = false);
			}
			bool throwError;

			public static ServiceTaskNudger_ForTest GetTestInstance()
			{
				return ObjectFactory.Get<IServiceTaskNudger>() as ServiceTaskNudger_ForTest;
			}
		}

		public class JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest : JCDDefaultActionStrategy
		{
			public JCDDefaultActionStrategy_ProcessOldRecordOnly_ForTest(DbConnection connection, ILogger logger, int? throwErrorAtRow = null)
				: base(connection, logger)
			{
				this.throwErrorAtRow = throwErrorAtRow;
			}
			readonly int? throwErrorAtRow;

			protected override long PopulateJobCostingQueue(DbConnection connection, long startRowNumber, long endRowNumber)
			{
				if (throwErrorAtRow.HasValue && throwErrorAtRow.Value == startRowNumber)
				{
					throw new OutOfMemoryException("Forced OutOfMemoeryException");
				}

				return base.PopulateJobCostingQueue(connection, startRowNumber, endRowNumber);
			}

			protected override void PopulateReportDataTable()
			{
				// Do Nothing. This will stop Queue table from being cleared and we will be able to assert queue table info in unit test functions.
			}

			protected override bool IsNudgingRequired => false;
		}

		public class JCDActionStrategyToRaiseDBError_ForTest : JCDActionStrategy
		{
			public JCDActionStrategyToRaiseDBError_ForTest(DbConnection connection, ILogger logger, DbErrorType errorType)
				: base(connection, logger)
			{
				ErrorType = errorType;
			}

			DbErrorType ErrorType { get; }

			internal bool? ShouldRetry { get; private set; }

			internal bool? ThrowOriginalException { get; private set; }

			protected override bool CanPerform() => true;

			protected override string GetCannotPerformMessage() => string.Empty;

			protected override void ProcessData()
			{
				switch (ErrorType)
				{
					case DbErrorType.LockTimeoutExpired:
						RaiseDBLockRequestTimeOutError();
						break;
					case DbErrorType.TimeoutExpired:
						RaiseTimeOutExpiredError();
						break;
					case DbErrorType.GhostRecordsBeingDeleted:
						RaiseGhostRecordsBeingDeleted();
						break;
					default:
						break;
				}
			}

			void RaiseDBLockRequestTimeOutError()
			{
				var sql = "UPDATE dbo.RefCountry SET RN_IsActive = 0 WHERE RN_Code = @Code";
				connection.ExecuteNonQuery(sql, cmd => cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU"));

				try
				{
					sql = @"SELECT RN_IsActive FROM dbo.RefCountry WITH (UPDLOCK) WHERE RN_Code = @Code;";
					using (var conn = Db.NewExtraConnectionToMainDb())
					using (var temp = conn.TemporarySetLockTimeout(0))
					using (var cmd = conn.Command(sql))
					{
						cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU");
						cmd.ExecuteNonQuery();
					}
				}
				catch (System.Data.Common.DbException ex)
				{
					(ShouldRetry, ThrowOriginalException) = CanRetryTheAction(ex, "Forced DB Lock Error");
				}
			}

			void RaiseTimeOutExpiredError()
			{
				using (var anotherConnection = Db.NewAdminConnection())
				{
					var sqlText = "UPDATE dbo.RefCountry SET RN_IsActive = 0 WHERE RN_Code = @Code";
					connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@Code", SqlDbType.Char, 2, "AU"));
					try
					{
						var command = anotherConnection.Command("SELECT RN_IsActive FROM dbo.RefCountry WITH (UPDLOCK) WHERE RN_Code = @Code", 1);
						command.AddParameter("@Code", SqlDbType.Char, 2, "AU");
						command.ExecuteNonQuery();
					}
					catch (System.Data.Common.DbException ex)
					{
						(ShouldRetry, ThrowOriginalException) = CanRetryTheAction(ex, "Forced Time Out Error");
					}
				}
			}

			void RaiseGhostRecordsBeingDeleted()
			{
				try
				{
					var error = SqlExceptionBuilder.CreateSqlError(3948, 1, 1, Db.Connection.ServerName, "Transaction Terminated", "", 1);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					throw SqlExceptionBuilder.CreateSqlException(errors);
				}
				catch (System.Data.Common.DbException ex)
				{
					(ShouldRetry, ThrowOriginalException) = CanRetryTheAction(ex, "Forced GhostRecordsBeingDeleted error");
				}
			}
		}

		#endregion
	}
}
