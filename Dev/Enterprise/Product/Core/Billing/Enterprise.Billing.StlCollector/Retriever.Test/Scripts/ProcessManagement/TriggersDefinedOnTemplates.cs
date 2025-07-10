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
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.ProcessManagement
{
	[TestedType(typeof(TriggersDefinedOnTemplates))]
	[TestDate(2022, 12, 08, 10, 0, 0)]
	class TriggersDefinedOnTemplatesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int templateTriggers_ByStaff_FFF_InCompany1_AAA_WithinMonth = 10;
		const int universalTriggers_ByStaff_GGG_InCompany1_AAA_WithinMonth = 7;
		const int universalTriggers_ByStaff_GGG_InCompany1_AAA_OutsideMonth = 5;
		const int normalTriggersOnUniveralTemplates_ByStaff_GGG_InCompany1_AAA_WithinMonth = 3;
		const int templateTriggers_ByStaff_EEE_InCompany1_BBB_WithinMonth = 4;
		const int universalTriggersOnNormalTemplate_ByStaff_EEE_InCompany1_BBB_WithinMonth = 4;
		const int templateTriggers_ByStaff_EEE_InCompany1_BBB_OutsideMonth = 2;

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

			factory.Save();

			ProcessTaskTemplate template;
			ProcessTaskTemplate universalTemplate;

			IProcessJobHeader jobHeader1;
			IProcessJobHeader jobHeader2;
			IProcessHeader workflow1;

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				EnvProxy.Instance.Security.WorkflowTaskTemplatesNew.IsAllowed = true;

				template = (ProcessTaskTemplate)helper.CreateWorkflowTemplate(factory, "INQ");
				template.GlobalTemplate = true;

				var templateWorkflow1 = helper.CreateWorkflow(template, "Template Workflow");
				helper.CreateTask(template, templateWorkflow1, description: "Template task 1");

				for (int i = 0; i < templateTriggers_ByStaff_FFF_InCompany1_AAA_WithinMonth; i++)
				{
					template.WorkflowItems.Triggers.AddNew();
				}

				template.WorkflowItems.Milestones.AddNew();
				template.WorkflowItems.Exceptions.AddNew();
				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				universalTemplate = (ProcessTaskTemplate)helper.CreateWorkflowTemplate(factory, "ORG");
				universalTemplate.GlobalTemplate = true;
				universalTemplate.P0_IsUniversal = true;
				universalTemplate.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

				for (int i = 0; i < universalTriggers_ByStaff_GGG_InCompany1_AAA_WithinMonth; i++)
				{
					var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
					universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
					universalTrigger.Description = $"Universal trigger 1_{i}";
				}

				// emulating normal triggers added before the template became universal
				for (int i = 0; i < normalTriggersOnUniveralTemplates_ByStaff_GGG_InCompany1_AAA_WithinMonth; i++)
				{
					universalTemplate.WorkflowItems.Triggers.AddNew();
				}

				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);
				jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);
				jobHeader2 = helper.CreateJobHeader<OrgHeader>(factory, false);

				workflow1 = helper.CreateWorkflow(jobHeader1, "Workflow1");
				helper.CreateTask(workflow1, staff1.GS_Code);

				var jobTrigger1 = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Triggers.AddNew();
				jobTrigger1.P9_Description = "Job Trigger 1";

				var jobMilestone1 = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				var jobException1 = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Exceptions.AddNew();

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				EnvProxy.Instance.Security.WorkflowTaskTemplatesNew.IsAllowed = true;

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(15);
				for (int i = 0; i < templateTriggers_ByStaff_EEE_InCompany1_BBB_WithinMonth; i++)
				{
					template.WorkflowItems.Triggers.AddNew();
				}

				// emulating universal triggers before the template because non-universal again
				for (int i = 0; i < universalTriggersOnNormalTemplate_ByStaff_EEE_InCompany1_BBB_WithinMonth; i++)
				{
					var universalTrigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
					universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
					universalTrigger.Description = $"Universal trigger {i}";
				}

				var jobTrigger2 = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Triggers.AddNew();
				jobTrigger2.P9_Description = "Job Trigger 2";

				helper.CreateTask(workflow1, staff1.GS_Code);

				factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(20);
				for (int i = 0; i < templateTriggers_ByStaff_EEE_InCompany1_BBB_OutsideMonth; i++)
				{
					template.WorkflowItems.Triggers.AddNew();
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				for (int i = 0; i < universalTriggers_ByStaff_GGG_InCompany1_AAA_OutsideMonth; i++)
				{
					var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
					universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
					universalTrigger.Description = $"Universal trigger 2_{i}";
				}
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertRow(transactions, 0, "BBB", "DDD", "FFF", "Defined on a non-universal template", "Non-universal trigger", templateTriggers_ByStaff_FFF_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 1, "AAA", "CCC", "GGG", "Defined on a universal template", "Universal trigger", universalTriggers_ByStaff_GGG_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 2, "AAA", "CCC", "GGG", "Defined on a universal template", "Non-universal trigger", normalTriggersOnUniveralTemplates_ByStaff_GGG_InCompany1_AAA_WithinMonth);
			AssertRow(transactions, 3, "AAA", "CCC", "EEE", "Defined on a non-universal template", "Non-universal trigger", templateTriggers_ByStaff_EEE_InCompany1_BBB_WithinMonth);
			AssertRow(transactions, 4, "AAA", "CCC", "EEE", "Defined on a non-universal template", "Universal trigger", universalTriggersOnNormalTemplate_ByStaff_EEE_InCompany1_BBB_WithinMonth);

			AssertEquals("Number of Transactions", 5, transactions.Count());
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, string expectedDefinedOnUniversalTemplate, string expectedIsUniversalTrigger, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode && t.Reference1 == expectedDefinedOnUniversalTemplate && t.Reference2 == expectedIsUniversalTrigger);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2022, 12, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "CA8C3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 12);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedWorkflows();
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
