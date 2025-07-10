using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Statistics;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ParameterSuffixerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCharacterBoolean()
		{
			new ParameterSuffixer().GetParameterSuffix(DateTime.UtcNow, StmALogSchema.SL_IsCancelled, true);
		}

		enum MyEnum { Yes, No }

		[ExpectNoExceptions]
		public void TestEnumToString()
		{
			new ParameterSuffixer().GetParameterSuffix(DateTime.UtcNow, StmALogSchema.SL_SE_NKEvent, MyEnum.Yes);
		}

		[UseSnapshotProtection]
		public void TestTimeoutException()
		{
			using (Db.DisposableActionForDbConnection())
			{
				// Timeout in trigger will rollback transaction on database level
				Db.Connection.ExecuteNonQuery("CREATE TRIGGER _TRG ON DummyBizo AFTER INSERT AS WAITFOR DELAY '00:01:00'");

				using (var connection = Db.Connection)
				{
					var factory = new BusinessObjectFactory(connection);
					var dummy = factory.New<DummyBusinessObject>();

					var defaultTimeout = Db.Connection.DefaultCommandTimeOutInSeconds;
					try
					{
						Db.Connection.DefaultCommandTimeOutInSeconds = 1;
						using (ParameterSuffixer.Instance.TemporaryUseNewCache_ForTest())
						{
							AssertExceptionThrown<ZSaveException>(factory.Save);
							AssertEquals(null, ErrorReporter.LastExceptionReported);
						}
					}
					finally
					{
						Db.Connection.DefaultCommandTimeOutInSeconds = defaultTimeout;
					}
				}
			}
		}

		public void TestException_CallsErrorReporter_ReportDeveloperExceptionOrHandleSilently()
		{
			var mockPersister = new Mock<IStatisticsPersister>();
			SqlHistogram[] histograms = null;
			var exception = new SystemException("something failed");
			mockPersister.Setup(x => x.TryRetrieve(It.IsAny<string>(), It.IsAny<string>(), out histograms))
				.Throws(exception);

			var mockErrorReporter = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(mockErrorReporter.Object))
			using (ObjectFactory.Substitute(mockPersister.Object))
			{
				var suffixer = new ParameterSuffixer();
				var actual = suffixer.GetParameterSuffix(DateTime.MinValue, WorkItemSchema.WKI_Priority, "UDF");
				AssertEquals(SqlHistogram.Constants.BUCKETFAIL, actual);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				mockErrorReporter.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), "BUCKETFAIL:WKI_Priority", exception), Times.Once);
				mockErrorReporter.VerifyNoOtherCalls();
			}
		}

		public void TestConnectionPoolTimeoutException_IsNotReported()
		{
			var mockPersister = new Mock<IStatisticsPersister>();
			SqlHistogram[] histograms = null;
			mockPersister.Setup(x => x.TryRetrieve(It.IsAny<string>(), It.IsAny<string>(), out histograms))
				.Throws(new InvalidOperationException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool."));
			using (ObjectFactory.Substitute(mockPersister.Object))
			{
				var suffixer = new ParameterSuffixer();
				var actual = suffixer.GetParameterSuffix(DateTime.MinValue, WorkItemSchema.WKI_Priority, "UDF");
				AssertEquals(SqlHistogram.Constants.BUCKETFAIL, actual);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestGetParameterSuffix()
		{
			TestGetParameterSuffixCore(
				column: JobPackLinesSchema.JL_JS,
				expectedSchema: JobPackLinesSchema.Constants.SqlSchemaName,
				expectedTable: JobPackLinesSchema.Constants.TableName,
				expectedColumn: JobPackLinesSchema.Constants.JL_JS);
		}

		public void TestGetParameterSuffix_View_UsesBackingColumnsTable()
		{
			TestGetParameterSuffixCore(
				column: WhsInventoryViewSchema.WI_OP,
				expectedSchema: WhsDocketLineSchema.Constants.SqlSchemaName,
				expectedTable: WhsDocketLineSchema.Constants.TableName,
				expectedColumn: WhsDocketLineSchema.Constants.WE_OP);
		}

		public void TestGetParameterSuffix_View_NoBackingColumn()
		{
			TestGetParameterSuffixCore(
				column: ProductionRuleScheduleTaskViewSchema.PRT_RunOnFriday,
				expectedSchema: ProductionRuleScheduleTaskViewSchema.Constants.SqlSchemaName,
				expectedTable: ProductionRuleScheduleTaskViewSchema.Constants.TableName,
				expectedColumn: ProductionRuleScheduleTaskViewSchema.Constants.PRT_RunOnFriday);
		}

		public void TestGetParameterSuffix_IndexedViewOverrride()
		{
			TestGetParameterSuffixCore(
				column: WhsLocationViewSchema.WLV_WW_Whs,
				expectedSchema: WhsLocationViewSchema.Constants.SqlSchemaName,
				expectedTable: "WhsLocationView_DoNotUse",
				expectedColumn: WhsLocationViewSchema.Constants.WLV_WW_Whs);
		}

		public void TestGetParameterSuffix_View_BackedByAnotherView()
		{
			TestGetParameterSuffixCore(
				column: WhsPickFaceViewSchema.WPV_WW_Whs,
				expectedSchema: WhsLocationViewSchema.Constants.SqlSchemaName,
				expectedTable: "WhsLocationView_DoNotUse",
				expectedColumn: WhsLocationViewSchema.Constants.WLV_WW_Whs);
		}

		void TestGetParameterSuffixCore(SchemaColumn column, string expectedSchema, string expectedTable, string expectedColumn)
		{
			var step2Guid = Guid.Parse("BBBBBBBB-0000-0000-0000-000000000000");
			var step1 = new SqlHistogramStep(Guid.Parse("AAAAAAAA-0000-0000-0000-000000000000"), 100, 10000);
			var step2 = new SqlHistogramStep(step2Guid, 100, 10000);
			var step3 = new SqlHistogramStep(Guid.Parse("CCCCCCCC-0000-0000-0000-000000000000"), 100, 10000);
			var histogram = new SqlHistogram(expectedSchema, expectedTable, expectedColumn, new[] { step1, step2, step3 });
			var histograms = new SqlHistogram[] { histogram };

			var mockPersister = new Mock<IStatisticsPersister>();
			mockPersister.Setup(p => p.TryRetrieve(expectedSchema, expectedTable, out histograms)).Returns(true);

			using (ObjectFactory.Substitute(mockPersister.Object))
			{
				var suffixer = new ParameterSuffixer();
				AssertEquals("Should return bucket 2.", "2", suffixer.GetParameterSuffix(DateTime.UtcNow, column, step2Guid));
			}

			mockPersister.Verify(p => p.TryRetrieve(expectedSchema, expectedTable, out histograms));
			mockPersister.VerifyNoOtherCalls();
		}
	}
}
