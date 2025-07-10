using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Transformation.Testing
{
	public class UpdateScheduledReportTemplateForJobHistoryReportTest : TestCaseWithFactory
	{
		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory() { RefreshEnabled = false };
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			var scheduleReportPK = Guid.NewGuid();

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(scheduleReportPK, dateRangeFilterDefinition, org.PK.ToString());
			}

			SetupTestTable();

			var originalScheduleTask = factoryForPreparingTestData.Load<ReportScheduleTask>(scheduleReportPK);
			AssertFieldValue(originalScheduleTask, "DateField", "Precondition: Field should be populated.");

			var helper = new UpdateScheduledReportTemplateForJobHistoryReport();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				helper.Run(new Action<string>((s) => { }), CancellationToken.None);
			}

			var task = Factory.Load<ReportScheduleTask>(scheduleReportPK);
			AssertFieldValue(task, string.Empty, "Field should be transformed.");
			AssertNotEquals("Report changed, so audit info modificatied.", originalScheduleTask.S5_SystemLastEditUser, task.S5_SystemLastEditUser);
			AssertNotEquals("Report changed, so audit info modificatied.", originalScheduleTask.S5_SystemLastEditTimeUtc, task.S5_SystemLastEditTimeUtc);

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_TableDoesNotExist()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory();
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			var scheduleReportPK = Guid.NewGuid();

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(scheduleReportPK, dateRangeFilterDefinition, org.PK.ToString());
			}

			var originalScheduleTask = factoryForPreparingTestData.Load<ReportScheduleTask>(scheduleReportPK);

			var helper = new UpdateScheduledReportTemplateForJobHistoryReport();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				helper.Run(new Action<string>((s) => { }), CancellationToken.None);
			}

			var task = Factory.Load<ReportScheduleTask>(scheduleReportPK);
			AssertFieldValue(task, "DateField", "Field should still be populated.");
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditUser, task.S5_SystemLastEditUser);
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditTimeUtc, task.S5_SystemLastEditTimeUtc);

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_NotIncludedInUpdateTable()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory();
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			var scheduleReportPK = Guid.NewGuid();

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(scheduleReportPK, dateRangeFilterDefinition, org.PK.ToString());
			}

			var sql = $@"
DROP TABLE IF EXISTS dbo.{TempTableName}

CREATE TABLE dbo.{TempTableName}
(
	JHR_S5_PK UNIQUEIDENTIFIER NOT NULL,
	JHR_FailedBatch bit NOT NULL DEFAULT ((0)),
	JHR_FailedIndividually bit NOT NULL DEFAULT ((0))
)
		";
			Db.Connection.ExecuteNonQuery(sql);

			var originalScheduleTask = factoryForPreparingTestData.Load<ReportScheduleTask>(scheduleReportPK);

			var helper = new UpdateScheduledReportTemplateForJobHistoryReport();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				helper.Run(new Action<string>((s) => { }), CancellationToken.None);
			}

			var task = Factory.Load<ReportScheduleTask>(scheduleReportPK);
			AssertFieldValue(task, "DateField", "Field should still be populated.");
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditUser, task.S5_SystemLastEditUser);
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditTimeUtc, task.S5_SystemLastEditTimeUtc);

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_FieldNotDefinedOnFilter()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory();
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			var scheduleReportPK = Guid.NewGuid();

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(scheduleReportPK, dateRangeFilterDefinition, org.PK.ToString());
			}

			SetupTestTable();

			var originalScheduleTask = factoryForPreparingTestData.Load<ReportScheduleTask>(scheduleReportPK);

			var helper = new UpdateScheduledReportTemplateForJobHistoryReport();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				helper.Run(new Action<string>((s) => { }), CancellationToken.None);
			}

			var task = Factory.Load<ReportScheduleTask>(scheduleReportPK);
			AssertFieldValue(task, string.Empty, "Field should still be empty.");
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditUser, task.S5_SystemLastEditUser);
			AssertEquals("Report unchanged, so no modifications to audit details.", originalScheduleTask.S5_SystemLastEditTimeUtc, task.S5_SystemLastEditTimeUtc);

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_MultipleBatchesReports()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory() { RefreshEnabled = false };
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			const int countForTest = 350;

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: true);
				for (var count = 1; count < countForTest; count++)
				{
					PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: false);
				}
			}

			SetupTestTable();

			var logTracker = new StringBuilder();

			var helper = new UpdateScheduledReportTemplateForJobHistoryReport();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				helper.Run(new Action<string>((s) => { logTracker.AppendLine(s); }), CancellationToken.None);
			}

			var tasks = Factory.Load<ReportScheduleTask>(new ZQuery());
			foreach (var task in tasks)
			{
				AssertFieldValue(task, string.Empty, "Field should be transformed.");
			}

			AssertEquals(@"Processed 100 records.
Processed 100 records.
Processed 100 records.
Processed 50 records.
Completed processing all Report Schedule Task batch records.
Successfully processed all Report Schedule Tasks.
", logTracker.ToString());

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_BatchFailed()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory() { RefreshEnabled = false };
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			const int countForTest = 103;

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: true);
				for (var count = 1; count < countForTest; count++)
				{
					PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: false);
				}
			}

			SetupTestTable();

			var logTracker = new StringBuilder();

			var provider = new BusinessObjectFactoryProviderWithFailureOnCount(Factory, org, countToFailOn: 2);
			var transform = new UpdateScheduledReportTemplateForJobHistoryReport(provider);

			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				transform.Run(new Action<string>((s) => { logTracker.AppendLine(s); }), CancellationToken.None);
			}

			var tasks = Factory.Load<ReportScheduleTask>(new ZQuery());
			foreach (var task in tasks)
			{
				AssertFieldValue(task, string.Empty, "Field should be transformed.");
			}

			AssertEquals(@"Processed 100 records.
Processed 3 records.
Completed processing all Report Schedule Task batch records.
3 records could not be processed in Batch. Will attempt to process individually.
Individual Record Processed. 2 record(s) remaining.
Individual Record Processed. 1 record(s) remaining.
Individual Record Processed. 0 record(s) remaining.
Successfully processed all Report Schedule Tasks.
", logTracker.ToString());

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		[ExpectNoExceptions("Service Task: ODT accesses environment current branch without setting the environment first.\r\nDirect access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.")]
		public void TestUpdateScheduledReportTemplateForJobHistoryReport_EverySaveFailed()
		{
			var factoryForPreparingTestData = new BusinessObjectFactory() { RefreshEnabled = false };
			var org = factoryForPreparingTestData.NewWithValidTestData<OrgHeader>();
			factoryForPreparingTestData.Save();

			var tempSwitchResult = ScheduledReportTestHelper.SwitchToNewBranch(factoryForPreparingTestData);
			TestCaseHelper.ClearTable(ReportScheduleTask.Schema.TableName);

			const int countForTest = 3;

			using (tempSwitchResult.TempUserContext)
			{
				org.OH_IsCreditor = true;
				factoryForPreparingTestData.Save();

				var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

				PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: true);
				for (var count = 1; count < countForTest; count++)
				{
					PrepareSampleJobHistoryReport(Guid.NewGuid(), dateRangeFilterDefinition, org.PK.ToString(), createNewTemplate: false);
				}
			}

			SetupTestTable();

			var logTracker = new StringBuilder();

			var provider = new BusinessObjectFactoryProviderWithFailureOnCount(Factory, org, failAllSaves: true);
			var transform = new UpdateScheduledReportTemplateForJobHistoryReport(provider);

			using (EnvProxy.Instance.TemporaryServiceTaskContext("ODT", true))
			{
				transform.Run(new Action<string>((s) => { logTracker.AppendLine(s); }), CancellationToken.None);
			}

			var tasks = Factory.Load<ReportScheduleTask>(new ZQuery());
			foreach (var task in tasks)
			{
				AssertFieldValue(task, "DateField", "Field should be transformed.");
			}

			AssertEquals(@"Processed 3 records.
Completed processing all Report Schedule Task batch records.
3 records could not be processed in Batch. Will attempt to process individually.
Individual Record Processed. 2 record(s) remaining.
Individual Record Processed. 1 record(s) remaining.
Individual Record Processed. 0 record(s) remaining.
3 Report Schedule Task failed to be processed.
", logTracker.ToString());

			AssertContains("Error processing scheduled report. The following ReportScheduleTask PK's could not be processed:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, TempTableName));
		}

		void AssertFieldValue(ReportScheduleTask task, string expectedDateField, string assertionMessage)
		{
			var bytes = ZCompressor.GetUncompressedVersion(task.S5_ScheduleState, "S5_ScheduleState");
			var reportInfo = ScheduledReportHelper.DeserializeStreamToReportSerializationInfo(new ZBlob(bytes));

			var dateRangeFilter = reportInfo.Report.FilterCollection.Single(r => r is DateRangeField);
			AssertEquals(assertionMessage, expectedDateField, ((DateRangeField)dateRangeFilter).FieldName);
		}

		void PrepareSampleJobHistoryReport(Guid scheduleReportPK, string dateRangeFilter, string creditorPk, bool createNewTemplate = true)
		{
			var templateContent = new Dictionary<string, string>();
			templateContent.Add("Template", @"
{A}-[#Config]
{A}-[#EndOfReport]");

			var placeHolderFilters = @"
{A}-[Orgs] {B}-[Type] {C}-[Organisation MultipleSelectionLookup]
{B}-[SerialisedByPK]
{B}-[Field] {C}-[OH_PK]

{A}-[Creditor] {B}-[Type] {C}-[creditor MultipleSelectionLookup]
{B}-[SerialisedByPK]
{A}-[#End]";

			templateContent.Add("Filters", string.Format(@"
{0}
{1}
", dateRangeFilter, placeHolderFilters));

			var stmMenuItemPK = new Guid("CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC");

			var testDocumentCreationHelper = new ScheduledReportTestHelper();

			if (createNewTemplate)
			{
				var templatePK = Guid.NewGuid();
				testDocumentCreationHelper.CreateTemplate(templatePK, "MultipleSelectionLookupSerialiseByCode", templateContent);
			}

			var scheduledReportContent = testDocumentCreationHelper.PrepareScheduledReport(templateContent, stmMenuItemPK, creditorPk);
			testDocumentCreationHelper.CreateScheduleTask(scheduleReportPK, stmMenuItemPK, "M", scheduledReportContent);
		}

		const string TempTableName = "ClientTransformTable_WI00769394";
		void SetupTestTable()
		{
			var sql = $@"
DROP TABLE IF EXISTS dbo.{TempTableName}

CREATE TABLE dbo.{TempTableName}
(
	JHR_S5_PK UNIQUEIDENTIFIER NOT NULL,
	JHR_FailedBatch bit NOT NULL DEFAULT ((0)),
	JHR_FailedIndividually bit NOT NULL DEFAULT ((0))
)

BEGIN
	INSERT INTO dbo.{TempTableName} (JHR_S5_PK)
	SELECT DISTINCT S5_PK
	FROM dbo.StmScheduleTask
END
		";
			Db.Connection.ExecuteNonQuery(sql);
		}

		public void PrepareValidTestDataForTransform(Guid creditorPK)
		{
			var scheduleReportPK = Guid.NewGuid();

			var dateRangeFilterDefinition = @"{A}-[Date Range] {B}-[Type] {C}-[Date range]
{B}-[Field] {C}-[DateField]
{B}-[DefaultTo] {C}-[<Now>]
{B}-[Required]
{B}-[Date Format] {C}-[long]";

			PrepareSampleJobHistoryReport(scheduleReportPK, dateRangeFilterDefinition, creditorPK.ToString());

			Factory.Save();
		}

		#region BusinessObjectFactoryProviderForTest

		class BusinessObjectFactoryProviderWithFailureOnCount : BusinessObjectFactoryProvider
		{
			public BusinessObjectFactoryProviderWithFailureOnCount(BusinessObjectFactory factory, BusinessObject someObject, int countToFailOn)
				: base(factory)
			{
				FailOnFactoryCount = countToFailOn;

				ArbitraryObjectForRow = someObject;
			}

			public BusinessObjectFactoryProviderWithFailureOnCount(BusinessObjectFactory factory, BusinessObject someObject, bool failAllSaves)
				: base(factory)
			{
				FailAllSaves = failAllSaves;

				ArbitraryObjectForRow = someObject;
			}

			BusinessObject ArbitraryObjectForRow { get; }

			int FailOnFactoryCount { get; }
			bool FailAllSaves { get; }

			protected override BusinessObjectFactory CreateNew(bool reclaimMemory)
			{
				FactoryCount++;

				var newFactory = base.CreateNew(reclaimMemory);
				if (FailAllSaves || FactoryCount == FailOnFactoryCount)
				{
					// simulate error of some sort to ensure it prevents save
					newFactory.Saving += (factory) => throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)ArbitraryObjectForRow).Row, Db.Connection), factory);
				}

				return newFactory;
			}
			int FactoryCount { get; set; }
		}

		#endregion
	}
}

