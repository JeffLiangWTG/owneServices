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
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.ProcessManagement
{
	[TestedType(typeof(AchievedMilestones))]
	[TestDate(2023, 01, 10, 10, 0, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	class AchievedMilestonesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_NotAchieved = 5;
		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedLastMonth = 2;
		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedCurrentMonth = 4;
		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedNextMonth = 3;

		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_NotAchieved = 9;
		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth = 7;
		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_AchievedNextMonth = 1;

		const int MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth_AchievedNextMonth = 5;

		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_NotAchieved = 11;
		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedLastMonth = 8;
		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedCurrentMonth = 3;
		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedNextMonth = 7;

		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_NotAchieved = 5;
		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_AchievedCurrentMonth = 4;
		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_AchievedNextMonth = 2;

		const int MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedNextMonth_AchievedNextMonth = 6;

		const int MilestonesCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth = 3;

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
			staff1.GS_GB_HomeBranch = branch1.PK;
			staff2.GS_GB_HomeBranch = branch2.PK;
			staff3.GS_GB_HomeBranch = branch1.PK;

			factory.Save();

			var jobHeader1 = helper.CreateJobHeader<SalesEnquiry>(factory, false);
			var job = (BusinessObject)jobHeader1.Parent;

			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertNotNull(unloco);

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = lastMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_NotAchieved; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedLastMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, lastMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedCurrentMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, currentMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();

				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_NotAchieved; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, currentMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();

				TestDateAttribute.Date = nextMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedNextMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = lastMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_NotAchieved; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedLastMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, lastMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedCurrentMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, currentMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();

				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_NotAchieved; i++)
				{
					((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
				}

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_AchievedCurrentMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, currentMonth.ToDateTimeOffset(unloco));
				}

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();

				TestDateAttribute.Date = nextMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedNextMonth_AchievedNextMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, nextMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();
			}

			// a different user but the same company - let's check how the resulting query aggregates data
			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestDateAttribute.Date = currentMonth.ToDateTime();

				for (int i = 0; i < MilestonesCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth; i++)
				{
					var milestone = ((IWorkflowProvider)jobHeader1.Parent).WorkflowItems.Milestones.AddNew();
					milestone.TrySetActualDateForEvent(null, job, currentMonth.ToDateTimeOffset(unloco));
				}

				factory.Save();
			}
		}
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			AssertRow(transactions, 0, "AAA", "CCC", "EEE", MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedLastMonth_AchievedCurrentMonth + MilestonesCreatedByStaff_EEE_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth);
			AssertRow(transactions, 1, "BBB", "DDD", "FFF", MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedLastMonth_AchievedCurrentMonth + MilestonesCreatedByStaff_FFF_InCompany_BBB_CreatedCurrentMonth_AchievedCurrentMonth);
			AssertRow(transactions, 2, "AAA", "CCC", "GGG", MilestonesCreatedByStaff_GGG_InCompany_AAA_CreatedCurrentMonth_AchievedCurrentMonth);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2023, 1, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "2F8D3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 1);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedTagLinks();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2023, 1));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}

		public void TestQuery_ShouldNotJoinOnStmALog()
		{
			var sqlText = GetSqlText();
			AssertNotContains("StmALog", sqlText);
		}
	}
}
