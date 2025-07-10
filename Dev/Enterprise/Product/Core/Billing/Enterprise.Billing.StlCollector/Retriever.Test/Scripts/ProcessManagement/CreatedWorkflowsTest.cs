using System;
using System.Collections.Generic;
using System.Data;
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
	[TestedType(typeof(CreatedWorkflows))]
	[TestDate(2023, 02, 14, 10, 0, 0)]
	class CreatedWorkflowsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth = 3;

		const int WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth = 5;
		const int WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times = 8;

		const int WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth = 6;

		const int WorkflowsCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth = 1;

		const int WorkflowsCreatedByStaff_GGG_InCompany_BBB_CreatedCurrentMonth = 2;

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

			var template = helper.CreateWorkflowTemplate(factory, "INQ");
			((ProcessTaskTemplate)template).GlobalTemplate = true;
			var templateWorkflow = helper.CreateWorkflow(template, "Template Workflow");
			helper.CreateTask(template, templateWorkflow);

			factory.Save();

			var jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				// Last month

				TestDateAttribute.Date = lastMonth.ToDateTime();

				for (int i = 0; i < WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"A{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
				}
				factory.Save();

				// Current month

				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"B{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
				}
				factory.Save();

				// Closed and reopened the same month - should count just once

				for (int i = 0; i < WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"D{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
					factory.Save();

					TestDateAttribute.AddMinutes(1);
					task.P9_Status = "ASN";
					factory.Save();

					TestDateAttribute.AddMinutes(1);
					task.P9_Status = "CLS";
					factory.Save();
				}

				// Next month

				TestDateAttribute.Date = nextMonth.ToDateTime();

				for (int i = 0; i < WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"E{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < WorkflowsCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"F{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
				}
				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < WorkflowsCreatedByStaff_GGG_InCompany_BBB_CreatedCurrentMonth; i++)
				{
					var workflow = helper.CreateWorkflow(jobHeader1, $"G{i}");
					var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
				}
				factory.Save();
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			AssertRow(transactions, 0, "AAA", "CCC", "EEE", WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth + WorkflowsCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_ClosedAndReopened2Times);
			AssertRow(transactions, 1, "BBB", "DDD", "FFF", WorkflowsCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth);
			AssertRow(transactions, 1, "BBB", "DDD", "GGG", WorkflowsCreatedByStaff_GGG_InCompany_BBB_CreatedCurrentMonth);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2023, 2, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "F3AF3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 2);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedWorkflows();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2023, 2));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}
	}
}
