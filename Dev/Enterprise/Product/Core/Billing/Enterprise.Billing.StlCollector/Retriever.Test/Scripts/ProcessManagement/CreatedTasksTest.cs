using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors.ProcessManagement;
using CargoWise.EntityFramework;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration.Billing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.ProcessManagement
{
	[TestedType(typeof(CreatedTasks))]
	[TestDate(2022, 12, 08, 10, 0, 0)]
	class CreatedTasksTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int templateTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth = 7;
		const int jobTasksCreatedByStaff_FFF_InCompany1_AAA_WithinMonth = 2;
		const int jobTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth = 15;
		const int jobTasksCreatedByStaff_EEE_InCompany1_BBB_WithinMonth = 8;
		const int jobTasksCreatedByStaff_EEE_InCompany1_BBB_OutsideMonth = 6;

		protected override void PrepareTestData()
		{
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
			staff1.GS_GB_HomeBranch = branch1.PK;
			staff2.GS_GB_HomeBranch = branch2.PK;
			staff3.GS_GB_HomeBranch = branch1.PK;

			helper.EnableBMSInRegistry();
			helper.CreateSystem(factory, "INQ", "ORG");

			var template = helper.CreateWorkflowTemplate(factory, "INQ");
			((ProcessTaskTemplate)template).GlobalTemplate = true;
			var templateWorkflow = helper.CreateWorkflow(template, "Template Workflow");
			for (int i = 0; i < templateTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth; i++)
			{
				helper.CreateTask(template, templateWorkflow);
			}

			factory.Save();

			IProcessJobHeader jobHeader1;
			IProcessJobHeader jobHeader2;

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);
				jobHeader2 = helper.CreateJobHeader<OrgHeader>(factory, false);

				var workflow1 = helper.CreateWorkflow(jobHeader1, "Workflow1");
				var workflow2 = helper.CreateWorkflow(jobHeader1, "Workflow2");
				var workflow3 = helper.CreateWorkflow(jobHeader1, "Workflow3");

				for (int i = 0; i < jobTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth - 2; i++)
				{
					helper.CreateTask(workflow1, staff1.GS_Code);
				}
				helper.CreateTask(workflow2, staff1.GS_Code);

				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);
				helper.CreateTask(workflow3, staff1.GS_Code);

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var workflow4 = helper.CreateWorkflow(jobHeader2, "Workflow4");

				for (int i = 0; i < jobTasksCreatedByStaff_FFF_InCompany1_AAA_WithinMonth; i++)
				{
					helper.CreateTask(workflow4, staff2.GS_Code);
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				var workflow5 = helper.CreateWorkflow(jobHeader2, "Workflow5");
				var workflow6 = helper.CreateWorkflow(jobHeader2, "Workflow6");
				var workflow7 = helper.CreateWorkflow(jobHeader2, "Workflow7");

				for (int i = 0; i < jobTasksCreatedByStaff_EEE_InCompany1_BBB_WithinMonth - 2; i++)
				{
					helper.CreateTask(workflow5, staff2.GS_Code);
				}

				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);
				helper.CreateTask(workflow6, staff2.GS_Code);
				helper.CreateTask(workflow7, staff2.GS_Code);

				((IWorkflowProvider)workflow5.Parent).WorkflowItems.Milestones.AddNew();
				((IWorkflowProvider)workflow6.Parent).WorkflowItems.Triggers.AddNew();
				((IWorkflowProvider)workflow7.Parent).WorkflowItems.Exceptions.AddNew();

				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(20);
				for (int i = 0; i < jobTasksCreatedByStaff_EEE_InCompany1_BBB_OutsideMonth; i++)
				{
					helper.CreateTask(workflow7, staff2.GS_Code);
				}

				factory.Save();
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			AssertRow(transactions, 0, "AAA", "CCC", "GGG", "Linked to a template", templateTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 0, "BBB", "DDD", "FFF", "Not linked to a template", jobTasksCreatedByStaff_FFF_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 1, "AAA", "CCC", "GGG", "Not linked to a template", jobTasksCreatedByStaff_GGG_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 2, "BBB", "CCC", "EEE", "Not linked to a template", jobTasksCreatedByStaff_EEE_InCompany1_BBB_WithinMonth);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, string expectedLinkedToTemplate, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode && t.Reference1 == expectedLinkedToTemplate);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2022, 12, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "LinkedToTemplate", expectedLinkedToTemplate, transaction.Reference1);
			AssertEquals(assertPrefix + "TransactionGuidReference", "CA8C3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 12);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedTagLinks();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2022, 12));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}

		public void TestQuery_ShouldNotJoinOnStmALog()
		{
			var sqlText = GetSqlText();
			AssertNotContains("StmALog", sqlText);
		}
	}
}
