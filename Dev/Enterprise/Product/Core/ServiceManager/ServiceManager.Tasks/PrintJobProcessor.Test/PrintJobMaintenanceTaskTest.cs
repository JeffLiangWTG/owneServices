using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(PrintJobMaintenanceTask))]
	sealed class PrintJobMaintenanceTaskTest : ServiceTaskTestCase<PrintJobMaintenanceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "PJM", hostedServiceAttribute.Code);
				AssertEquals("Description", "Print Job Maintenance", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestPurgeOldJobs()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_RunDateTime = ZDateTime.Now.AddDays(-15);
			printJob1.SP_SB_DeliveryGroup = deliveryGroup.PK;

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_RunDateTime = ZDateTime.Now.AddDays(-16);
			printJob2.SP_SB_DeliveryGroup = deliveryGroup.PK;

			Factory.Save();
			AssertEquals(2, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroup.PK)));

			using (var documentPurgeTask = new PrintJobMaintenanceTask())
			{
				var log = InitialiseAndRunTaskSchedule(documentPurgeTask);
				AssertEquals(0, Factory.GetDatabaseCount(typeof(StmPrintJob), new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, deliveryGroup.PK)));
				AssertEquals(1, log.Count);
				AssertEquals("Information|Available printers have been updated.", log[0]);
			}
		}

		public void TestPurgeOldJobs_CleanupOldUnprocessedStmDeliveryGroup()
		{
			const string insertStmDeliveryGroup = "INSERT INTO dbo.StmDeliveryGroup (SB_PK, SB_IsProcessed, SB_SystemCreateTimeUtc) VALUES ('{0}', {1}, {2})";
			const string insertStmPrintJob = "INSERT INTO dbo.StmPrintJob (SP_PK, SP_SB_DeliveryGroup) VALUES (NEWID(), '{0}')";

			var group_Processed_WithPrintJob_New = Guid.NewGuid();
			var group_Processed_WithPrintJob_Old = Guid.NewGuid();
			var group_Processed_WithoutPrintJob_New = Guid.NewGuid();
			var group_Processed_WithoutPrintJob_Old = Guid.NewGuid();
			var group_NotProcessed_WithPrintJob_New = Guid.NewGuid();
			var group_NotProcessed_WithPrintJob_Old = Guid.NewGuid();
			var group_NotProcessed_WithoutPrintJob_New = Guid.NewGuid();
			var group_NotProcessed_WithoutPrintJob_Old = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_Processed_WithPrintJob_New, "1", "getutcdate()"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_Processed_WithPrintJob_Old, "1", "dateadd(day, -2, getutcdate())"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_Processed_WithoutPrintJob_New, "1", "getutcdate()"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_Processed_WithoutPrintJob_Old, "1", "dateadd(day, -2, getutcdate())"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_NotProcessed_WithPrintJob_New, "0", "getutcdate()"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_NotProcessed_WithPrintJob_Old, "0", "dateadd(day, -2, getutcdate())"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_NotProcessed_WithoutPrintJob_New, "0", "getutcdate()"));
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, group_NotProcessed_WithoutPrintJob_Old, "0", "dateadd(day, -2, getutcdate())"));

			TestConnection.ExecuteNonQuery(string.Format(insertStmPrintJob, group_Processed_WithPrintJob_New));
			TestConnection.ExecuteNonQuery(string.Format(insertStmPrintJob, group_Processed_WithPrintJob_Old));
			TestConnection.ExecuteNonQuery(string.Format(insertStmPrintJob, group_NotProcessed_WithPrintJob_New));
			TestConnection.ExecuteNonQuery(string.Format(insertStmPrintJob, group_NotProcessed_WithPrintJob_Old));

			using (var documentJobTask = new DocumentJobTask())
			{
				using (var printJobTask = new PrintJobTask())
				{
					using (var maintenanceTask = new PrintJobMaintenanceTask())
					{
						var logger = new TestServiceLogger();

						maintenanceTask.ServiceLogger = logger;
						printJobTask.ServiceLogger = logger;
						documentJobTask.ServiceLogger = logger;

						printJobTask.RunTask();
						documentJobTask.RunTask();
						maintenanceTask.RunTask();

						AssertEquals(true, StmDeliveryGroupExists(group_Processed_WithPrintJob_New));
						AssertEquals(true, StmDeliveryGroupExists(group_Processed_WithPrintJob_Old));
						AssertEquals(true, StmDeliveryGroupExists(group_Processed_WithoutPrintJob_New));
						AssertEquals(true, StmDeliveryGroupExists(group_Processed_WithoutPrintJob_Old));
						AssertEquals(true, StmDeliveryGroupExists(group_NotProcessed_WithPrintJob_New));
						AssertEquals(true, StmDeliveryGroupExists(group_NotProcessed_WithPrintJob_Old));
						AssertEquals(true, StmDeliveryGroupExists(group_NotProcessed_WithoutPrintJob_New));
						AssertEquals(false, StmDeliveryGroupExists(group_NotProcessed_WithoutPrintJob_Old));
					}
				}
			}
		}

		public void TestPurgeOldJobs_CleanupOldUnprocessedStmDeliveryGroup_DeleteConflict()
		{
			const string insertStmDeliveryGroup = "INSERT INTO dbo.StmDeliveryGroup (SB_PK, SB_IsProcessed, SB_SystemCreateTimeUtc) VALUES ('{0}', {1}, dateadd(day, -2, getutcdate()))";

			var sB_PK = Guid.NewGuid().ToString();
			TestConnection.ExecuteNonQuery(string.Format(insertStmDeliveryGroup, sB_PK, "0"));

			using (var documentPurgeTask = new PrintJobMaintenanceTask())
			{
				Exception e = null;
				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						Insert(sB_PK, ref e, TestConnection);
					}
				});
				task.Start();

				var log = InitialiseAndRunTaskSchedule(documentPurgeTask);

				task.Wait();

				AssertEquals("Clear up StmDeliveryGroup failed.", 1, log.Count);
				AssertNotNull("Insert into dbo.StmPrintJob failed.", e);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void Insert(string sB_PK, ref Exception e, DbConnection connection)
		{
			var insertStmPrintJob = string.Format(CultureInfo.InvariantCulture, "INSERT dbo.StmPrintJob (SP_PK, SP_SB_DeliveryGroup) VALUES (NEWID(), '{0}')", sB_PK);

			try
			{
				connection.ExecuteNonQuery(insertStmPrintJob, 1);
			}
			catch (Exception ex)
			{
				e = ex;
			}
		}

		bool StmDeliveryGroupExists(Guid groupPk)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, "FROM dbo.StmDeliveryGroup WHERE SB_PK = '{0}'", groupPk.ToString());

			return TestConnection.Exists(sql);
		}
	}
}
