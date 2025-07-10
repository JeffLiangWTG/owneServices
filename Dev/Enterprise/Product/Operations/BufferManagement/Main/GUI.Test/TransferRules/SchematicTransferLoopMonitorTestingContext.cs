using System;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public abstract class SchematicTransferLoopMonitorTestingContext : Assertion, IDisposable
	{
		protected readonly BusinessObjectFactory factory;

		public SchematicTransferLoopMonitorTestingContext(BusinessObjectFactory factory)
		{
			this.factory = factory;
			Arrange();
		}

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		void Arrange()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);

			Assert("Test requires TestDate attribute", TestDateAttribute.IsActive);
			Assert("Test requires TestDateIncremental attribute", TestDateIncrementalAttribute.IsActive);

			TestDateAttribute.Date = new DateTime(2015, 7, 14);
			TestDateIncrementalAttribute.Span = TimeSpan.FromSeconds(1);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(factory, Env.CurrentDepartmentPK);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			SetupRegistry();
			SetupStaffAndNotificationGroup();
			SetupSystemAndWorkflows();
			SetupSchematic();
			factory.Save();
			TestCaseHelper.ClearTable(StmALogSchema.Constants.TableName);

			RunLooping();
		}

		protected abstract void RunLooping();

		protected void SetupRegistry()
		{
			BMSRegistry.Instance.WorkflowLoopingDetectionLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			BMSRegistry.Instance.WorkflowLoopingDetectionDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
		}

		public void SetupStaffAndNotificationGroup()
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "king.arthur@avalon.com";

			var bmsNotificationGroup = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		protected BMSystem System;
		public OrgHeader OrgHeader;
		public ProcessJobHeader JobHeader;
		public ProcessHeader Workflow;
		public ProcessHeader WorkflowToDeleteBeforeScan;
		public ProcessHeader WorkflowToDeactivateBeforeScan;
		protected TagMagnitude Tag;

		protected void SetupSystemAndWorkflows()
		{
			VisualBoardsTestCase.SetupAndClearTables();

			System = BMSTestHelper.CreateSystem(factory, "ORG");
			System.FS_Name = "TestSystem";

			OrgHeader = factory.NewWithValidTestData<OrgHeader>();
			JobHeader = ProcessJobHeader.GetForParent(OrgHeader, factory, true);
			Workflow = JobHeader.ProcessHeaders[0];
			Workflow.FH_CompletionStatement = "Number one";
			BMSTestHelper.CreateTask(Workflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 1);

			WorkflowToDeleteBeforeScan = JobHeader.ProcessHeaders.AddNew();
			WorkflowToDeleteBeforeScan.FH_CompletionStatement = "Number two";
			BMSTestHelper.CreateTask(WorkflowToDeleteBeforeScan, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 1);

			WorkflowToDeactivateBeforeScan = JobHeader.ProcessHeaders.AddNew();
			WorkflowToDeactivateBeforeScan.FH_CompletionStatement = "Number three";
			BMSTestHelper.CreateTask(WorkflowToDeactivateBeforeScan, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 1);

			var tagGroup = BMSTestHelper.CreateTagDefinition(factory, "BIM");
			Tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "BOM");
		}

		protected abstract void SetupSchematic();

		protected void MakeManualChangesToWorkflowsByApplyingTag()
		{
			//We save after each change to get three records in StmALog. Otherwise, there will be one record only due to a bug.
			BMSTestHelper.CreateTagLink(Workflow, Tag);
			factory.Save();
			BMSTestHelper.CreateTagLink(WorkflowToDeactivateBeforeScan, Tag);
			factory.Save();
			BMSTestHelper.CreateTagLink(WorkflowToDeleteBeforeScan, Tag);
			factory.Save();
		}

		protected void PreconditionAssertLogContainsManualChangesToWorkflows()
		{
			var xfrLogs = GetXFRRecords();
			var fromTime = xfrLogs.First().SL_EventTime;
			var toTime = xfrLogs.Last().SL_EventTime;
			var query = new ZQuery(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, serviceTaskUserCode);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.TagWasAddedOrRemovedCode); //TAG
			query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, fromTime);
			query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThanOrEqualTo, toTime);
			var logs = factory.CreateNewFactory().Load<StmALog>(query);
			AssertEquals("Precondition: manual changes should be logged in StmALog", 1, logs.Count(x => x.SL_Parent == Workflow.PK));
			AssertEquals("Precondition: manual changes should be logged in StmALog", 1, logs.Count(x => x.SL_Parent == WorkflowToDeactivateBeforeScan.PK));
			AssertEquals("Precondition: manual changes should be logged in StmALog", 1, logs.Count(x => x.SL_Parent == WorkflowToDeleteBeforeScan.PK));
		}

		protected void MakeManualChangesToWorkflowsByChangingProperties()
		{
			var log = factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_SE_NKEvent = Events.EditedARecordCode; //EDT
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				log.SL_Table = "OrgHeader";
				log.SL_Parent = OrgHeader.PK;
			}
			factory.Save();
		}

		protected void PreconditionAssertLogContainsManualChangesToJob()
		{
			var xfrLogs = GetXFRRecords();
			var fromTime = xfrLogs.First().SL_EventTime;
			var toTime = xfrLogs.Last().SL_EventTime;
			var query = new ZQuery(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, serviceTaskUserCode);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecordCode); //EDT
			query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, fromTime);
			query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThanOrEqualTo, toTime);
			var logs = factory.CreateNewFactory().Load<StmALog>(query);
			AssertEquals("Precondition: manual changes should be logged in StmALog", 1, logs.Count(x => x.SL_Parent == OrgHeader.PK));
		}

		protected void AlterWorkflowsBeforeScan()
		{
			using (WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(factory).SuppressWorkflowChangeLog())
			{
				WorkflowToDeleteBeforeScan.Delete();
			}
			WorkflowToDeactivateBeforeScan.FH_IsActive = false;
			factory.Save();
		}

		TestTransferRuleRunner transferRuleRunner;
		public LoggerForTest LoggerForTransferRuleRunner;
		ReleaseGateDirector releaseGateDirector;
		LoggerForTest LoggerForReleaseGateDirector;

		protected void RunTransferRuleRunner(int times = 1)
		{
			if (transferRuleRunner == null)
			{
				LoggerForTransferRuleRunner = new LoggerForTest();
				transferRuleRunner = new TestTransferRuleRunner(System, LoggerForTransferRuleRunner);
			}

			for (int i = 0; i < times; i++)
			{
				transferRuleRunner.Process_ForTest();
			}
		}

		protected void RunReleaseGateDirector(int times = 1)
		{
			if (releaseGateDirector == null)
			{
				LoggerForReleaseGateDirector = new LoggerForTest();
				var failureLogService = factory.ServiceContainer.GetService<ReleaseGateFailureLogService>();
				var releaseGateLogger = new ReleaseGateLoggerWithFailureServices(failureLogService);
				releaseGateDirector = new ReleaseGateDirector(System, LoggerForReleaseGateDirector, releaseGateLogger);
			}

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			{
				for (int i = 0; i < times; i++)
				{
					releaseGateDirector.Process();
				}
			}
		}

		protected string serviceTaskUserLoginName => "CWService";
		protected string serviceTaskUserCode => "~BP";

		public StmALog[] GetXFRRecords()
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "XFR");
			query.AddToFilter(StmALogSchema.SL_GS_NKUser, serviceTaskUserCode);
			return factory.Load<StmALog>(query);
		}
	}

	public class BMSMultipleRunsTestingContext : SchematicTransferLoopMonitorTestingContext
	{
		public BMSMultipleRunsTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected BMComponent Bucket1;
		protected BMComponent Bucket2;
		protected BMComponent Bucket3;

		protected override void SetupSchematic()
		{
			var start = BMSTestHelper.CreateBucket(System, "start", sequence: 0);
			Bucket1 = BMSTestHelper.CreateBucket(System, "looped bucket 1", sequence: 1);
			Bucket2 = BMSTestHelper.CreateBucket(System, "looped bucket 2", sequence: 3);
			Bucket3 = BMSTestHelper.CreateBucket(System, "looped bucket 3", sequence: 2);

			BMSTestHelper.LinkComponents(start, Bucket1);
			var link1_2 = BMSTestHelper.LinkComponents(Bucket1, Bucket2);
			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var link2_3 = BMSTestHelper.LinkComponents(Bucket2, Bucket3);
			FilterStripsTestHelper.AddFilterStrips(link2_3.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var link3_1 = BMSTestHelper.LinkComponents(Bucket3, Bucket1);
			FilterStripsTestHelper.AddFilterStrips(link3_1.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			Workflow.MoveToComponent(Bucket1);
			WorkflowToDeleteBeforeScan.MoveToComponent(Bucket1);
			WorkflowToDeactivateBeforeScan.MoveToComponent(Bucket1);
		}

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				//a workflow makes a full loop in two runs of the BMS service task
				RunTransferRuleRunner(times: BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value * 2);
				PreconditionAssertLogContainsTransfers();
				AlterWorkflowsBeforeScan();
			}
		}

		protected void PreconditionAssertLogContainsTransfers()
		{
			int loopsCount = BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value;
			int movesCount = loopsCount * 3;

			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number one") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 1 to looped bucket 2")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number one") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 2 to looped bucket 3")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number one") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 3 to looped bucket 1")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number two") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 1 to looped bucket 2")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number two") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 2 to looped bucket 3")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number two") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 3 to looped bucket 1")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number three") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 1 to looped bucket 2")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number three") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 2 to looped bucket 3")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number three") && x.Contains("Organization (XVBQP68SIYXQ)) from component looped bucket 3 to looped bucket 1")));

			var logs = GetXFRRecords();
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == Workflow.PK));
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == WorkflowToDeactivateBeforeScan.PK));
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == WorkflowToDeleteBeforeScan.PK));
		}
	}

	public class BMSMultipleRunsWithUserChangesToWorkflowsTestingContext : BMSMultipleRunsTestingContext
	{
		public BMSMultipleRunsWithUserChangesToWorkflowsTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunTransferRuleRunner();
			}

			MakeManualChangesToWorkflowsByApplyingTag();

			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunTransferRuleRunner(times: BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value * 2 - 1);
			}

			PreconditionAssertLogContainsTransfers();
			PreconditionAssertLogContainsManualChangesToWorkflows();
			AlterWorkflowsBeforeScan();
		}
	}

	public class BMSMultipleRunsWithUserChangesToJobTestingContext : BMSMultipleRunsTestingContext
	{
		public BMSMultipleRunsWithUserChangesToJobTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunTransferRuleRunner();
			}

			MakeManualChangesToWorkflowsByChangingProperties();

			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunTransferRuleRunner(times: BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value * 2 - 1);
			}

			PreconditionAssertLogContainsTransfers();
			PreconditionAssertLogContainsManualChangesToJob();
			AlterWorkflowsBeforeScan();
		}
	}

	public class LoopingBetweenBMSAndBMGTestingContext : SchematicTransferLoopMonitorTestingContext
	{
		public LoopingBetweenBMSAndBMGTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected BMComponent JealousBucket;
		protected BMComponent DecentBuffer;
		protected override void SetupSchematic()
		{
			var start = BMSTestHelper.CreateBucket(System, "start", sequence: 0);
			JealousBucket = BMSTestHelper.CreateBucket(System, "jealous bucket", sequence: 0);
			DecentBuffer = BMSTestHelper.CreateBuffer(System, "decent buffer");

			BMSTestHelper.LinkComponents(start, JealousBucket);
			var linkBucketToBuffer = BMSTestHelper.LinkComponents(JealousBucket, DecentBuffer, isReleaseGate: true);
			var linkBufferToBucket = BMSTestHelper.LinkComponents(DecentBuffer, JealousBucket);

			FilterStripsTestHelper.AddFilterStrips(linkBufferToBucket.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			Workflow.MoveToComponent(JealousBucket);
			WorkflowToDeleteBeforeScan.MoveToComponent(JealousBucket);
			WorkflowToDeactivateBeforeScan.MoveToComponent(JealousBucket);
		}

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				for (int i = 0; i < BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value; i++)
				{
					RunReleaseGateDirector();
					RunTransferRuleRunner();
				}
			}

			PreconditionAssertLogContainsTransfers();
			AlterWorkflowsBeforeScan();
		}

		protected void PreconditionAssertLogContainsTransfers()
		{
			int loopsCount = BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value;
			int movesCount = loopsCount * 2;

			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number one") && x.Contains("Job = Organization (XVBQP68SIYXQ)) from component decent buffer to jealous bucket")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number two") && x.Contains("Job = Organization (XVBQP68SIYXQ)) from component decent buffer to jealous bucket")));
			AssertEquals("Precondition: transfers should be logged", loopsCount, LoggerForTransferRuleRunner.LogEntries.Count(x => x.StartsWith("TestSystem: Moved workflow Number three") && x.Contains("Job = Organization (XVBQP68SIYXQ)) from component decent buffer to jealous bucket")));

			var logs = GetXFRRecords();
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == Workflow.PK));
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == WorkflowToDeactivateBeforeScan.PK));
			AssertEquals("Precondition: changes should be logged in StmALog", movesCount, logs.Count(x => x.SL_Parent == WorkflowToDeleteBeforeScan.PK));
		}
	}

	public class LoopingBetweenBMSAndBMGWithUserChangesToWorkflowsTestingContext : LoopingBetweenBMSAndBMGTestingContext
	{
		public LoopingBetweenBMSAndBMGWithUserChangesToWorkflowsTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunReleaseGateDirector();
				RunTransferRuleRunner();
			}

			MakeManualChangesToWorkflowsByApplyingTag();

			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				for (int i = 0; i < BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value - 1; i++)
				{
					RunReleaseGateDirector();
					RunTransferRuleRunner();
				}
			}

			PreconditionAssertLogContainsTransfers();
			var xfrLogs = GetXFRRecords();
			PreconditionAssertLogContainsManualChangesToWorkflows();
			AlterWorkflowsBeforeScan();
		}
	}

	public class LoopingBetweenBMSAndBMGWithUserChangesToJobTestingContext : LoopingBetweenBMSAndBMGTestingContext
	{
		public LoopingBetweenBMSAndBMGWithUserChangesToJobTestingContext(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void RunLooping()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				RunReleaseGateDirector();
				RunTransferRuleRunner();
			}

			MakeManualChangesToWorkflowsByChangingProperties();

			using (EnvProxy.Instance.SetTemporaryUserContext(serviceTaskUserLoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				for (int i = 0; i < BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value - 1; i++)
				{
					RunReleaseGateDirector();
					RunTransferRuleRunner();
				}
			}

			PreconditionAssertLogContainsTransfers();
			PreconditionAssertLogContainsManualChangesToJob();
			AlterWorkflowsBeforeScan();
		}
	}
}
