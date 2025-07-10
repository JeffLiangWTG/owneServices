using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors.ProcessManagement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration.Billing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.ProcessManagement
{
	[TestedType(typeof(CompletedTasks))]
	[TestDate(2023, 01, 17, 10, 0, 0)]
	class CompletedTasksTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_Closed = 3;

		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Open = 5;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Assigned = 4;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Working = 2;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Suspended = 6;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Cancelled = 7;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen = 9;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_OperationalAction = 2;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_StatusControlButtons = 4;
		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times_TaskMenu_TaskTabGrid = 3;

		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth_Closed = 6;

		const int TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopenedCurrentMonth_ThenClosedAndReopenedNextMonth_TaskMenu_TaskTabGrid = 2;

		const int TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Open = 1;
		const int TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_EditTaskScreen = 4;
		const int TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_OperationalAction = 5;
		const int TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_StatusControlButtons = 8;

		const int TasksCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen = 3;

		protected override void PrepareTestData()
		{
			ZDateTime currentMonth = ZDateTime.UtcNow;
			ZDateTime lastMonth = currentMonth.AddMonths(-1);
			ZDateTime nextMonth = currentMonth.AddMonths(1);

			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var company1 = factory.New<GlbCompany>();
			var company2 = factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			company2.GC_Code = "BBB";

			var branch1 = factory.New<GlbBranch>();
			var branch2 = factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch2.GB_Code = "DDD";
			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;

			var department = factory.NewWithValidTestData<GlbDepartment>();

			var staff1 = GlbStaff.New(factory);
			var staff2 = GlbStaff.New(factory);
			var staff3 = GlbStaff.New(factory);
			staff1.GS_Code = "EEE";
			staff2.GS_Code = "FFF";
			staff3.GS_Code = "GGG";
			staff1.GS_LoginName = "Squanchy";
			staff2.GS_LoginName = "Schwifty";
			staff3.GS_LoginName = "The Creator";

			helper.EnableBMSInRegistry();
			helper.CreateSystem(factory, "INQ", "ORG");

			var template = (ProcessTaskTemplate)helper.CreateWorkflowTemplate(factory, "INQ");
			template.GlobalTemplate = true;
			var templateWorkflow = helper.CreateWorkflow(template, "Template Workflow");
			helper.CreateTask(template, templateWorkflow, description: "Template task");

			factory.Save();

			var jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				// Last month

				TestDateAttribute.Date = lastMonth.ToDateTime();

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_Closed; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				factory.Save();

				// Curret month

				TestDateAttribute.Date = currentMonth.ToDateTime();

				// Non closed tasks

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Open; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Assigned; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Working; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Suspended; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Cancelled; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
					}
				}

				// Closed tasks with various modes

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_OperationalAction; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_StatusControlButtons; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				factory.Save();

				// Closed and reopened the same month

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times_TaskMenu_TaskTabGrid; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
					factory.Save();

					TestDateAttribute.AddMinutes(1);

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.Other))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
					factory.Save();

					TestDateAttribute.AddMinutes(1);

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
					factory.Save();

					TestDateAttribute.AddMinutes(1);

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.Other))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
					factory.Save();
				}

				// Next month

				TestDateAttribute.Date = nextMonth.ToDateTime();

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth_Closed; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				factory.Save();

				// Closed and reopened diferent months

				for (int i = 0; i < TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopenedCurrentMonth_ThenClosedAndReopenedNextMonth_TaskMenu_TaskTabGrid; i++)
				{
					TestDateAttribute.Date = currentMonth.ToDateTime();

					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
					factory.Save();

					TestDateAttribute.AddMinutes(1);

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.Other))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
					factory.Save();

					TestDateAttribute.Date = nextMonth.ToDateTime();

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
					factory.Save();

					TestDateAttribute.AddMinutes(1);

					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.Other))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
					factory.Save();
				}
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = currentMonth.ToDateTime();

				// Non closed tasks

				for (int i = 0; i < TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Open; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
					}
				}

				// Closed tasks with various modes

				for (int i = 0; i < TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_EditTaskScreen; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_OperationalAction; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				for (int i = 0; i < TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_StatusControlButtons; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = currentMonth.ToDateTime();

				// Closed tasks with various modes

				for (int i = 0; i < TasksCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen; i++)
				{
					var task = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Tasks.AddNew();
					using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen))
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}

				factory.Save();
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 9, transactions.Count());

			AssertRow(transactions, 0, "AAA", "CCC", "EEE", ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen, TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen);
			AssertRow(transactions, 1, "AAA", "CCC", "EEE", ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction, TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_OperationalAction);
			AssertRow(transactions, 2, "AAA", "CCC", "EEE", ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons, TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_Closed_StatusControlButtons);
			AssertRow(transactions, 3, "AAA", "CCC", "EEE", ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu, TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times_TaskMenu_TaskTabGrid + TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopenedCurrentMonth_ThenClosedAndReopenedNextMonth_TaskMenu_TaskTabGrid);
			AssertRow(transactions, 4, "AAA", "CCC", "EEE", ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid, TasksCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times_TaskMenu_TaskTabGrid);
			AssertRow(transactions, 5, "BBB", "DDD", "FFF", ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen, TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_EditTaskScreen);
			AssertRow(transactions, 6, "BBB", "DDD", "FFF", ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction, TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_OperationalAction);
			AssertRow(transactions, 7, "BBB", "DDD", "FFF", ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons, TasksCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_Closed_StatusControlButtons);
			AssertRow(transactions, 8, "AAA", "CCC", "GGG", ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen, TasksCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_Closed_EditTaskScreen);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, string expectedStatusChangeMode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.ClientStaffCode == expectedUserCode && t.Reference1 == expectedStatusChangeMode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2023, 1, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "StatusChangeMode", expectedStatusChangeMode, transaction.Reference1);
			AssertEquals(assertPrefix + "TransactionGuidReference", "2F8D3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 1);
	}
}
