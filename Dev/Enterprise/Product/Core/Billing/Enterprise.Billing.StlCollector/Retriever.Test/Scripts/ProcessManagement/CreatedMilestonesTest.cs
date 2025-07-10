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
	[TestedType(typeof(CreatedMilestones))]
	[TestDate(2022, 12, 08, 10, 0, 0)]
	class CreatedMilestonesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int milestonesCreatedByStaff_GGG_InCompany1_AAA_WithinMonth = 8;
		const int milestonesCreatedByStaff_FFF_InCompany1_BBB_WithinMonth = 5;
		const int milestonesCreatedByStaff_FFF_InCompany1_BBB_OutsideMonth = 4;
		const int milestonesCreatedByStaff_EEE_InCompany1_AAA_WithinMonth = 2;

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

			var template = (ProcessTaskTemplate)helper.CreateWorkflowTemplate(factory, "INQ");
			template.GlobalTemplate = true;
			var templateWorkflow = helper.CreateWorkflow(template, "Template Workflow");
			helper.CreateTask(template, templateWorkflow, description: "Template task 1");
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Template milestone 1";

			factory.Save();

			IProcessJobHeader jobHeader1;

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);

				var workflow1 = helper.CreateWorkflow(jobHeader1, "Workflow1");
				helper.CreateTask(workflow1, staff1.GS_Code);
				helper.CreateTask(workflow1, staff1.GS_Code);

				factory.Save();

				var milestone1 = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.Cast<ProcessTask>().SingleOrDefault(m => m.P9_Description == "Template milestone 1");
				AssertNotNull(milestone1);

				for (int i = 0; i < milestonesCreatedByStaff_GGG_InCompany1_AAA_WithinMonth - 1; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Triggers.AddNew();
				((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Exceptions.AddNew();

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				for (int i = 0; i < milestonesCreatedByStaff_EEE_InCompany1_AAA_WithinMonth; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(14);
				for (int i = 0; i < milestonesCreatedByStaff_FFF_InCompany1_BBB_WithinMonth; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}
				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(20);
				for (int i = 0; i < milestonesCreatedByStaff_FFF_InCompany1_BBB_OutsideMonth; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}
				factory.Save();
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			AssertRow(transactions, 0, "AAA", "CCC", "GGG", milestonesCreatedByStaff_GGG_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 1, "BBB", "DDD", "FFF", milestonesCreatedByStaff_FFF_InCompany1_BBB_WithinMonth);
			AssertRow(transactions, 2, "AAA", "CCC", "EEE", milestonesCreatedByStaff_EEE_InCompany1_AAA_WithinMonth);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2022, 12, 1), transaction.ServiceOccuredUTC);
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
