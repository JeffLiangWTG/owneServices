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
	[TestedType(typeof(CreatedTagLinks))]
	[TestDate(2019, 7, 6, 10, 0, 0)]
	class CreatedTagLinksTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var company1 = factory.New<GlbCompany>();
			var company2 = factory.New<GlbCompany>();
			company1.GC_Code = "CM1";
			company2.GC_Code = "CM2";

			var branch1 = factory.New<GlbBranch>();
			var branch2 = factory.New<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch2.GB_Code = "BR2";
			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;

			var department1 = factory.NewWithValidTestData<GlbDepartment>();
			var department2 = factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			department2.GE_Code = "DP2";

			var staff1 = GlbStaff.New(factory);
			var staff2 = GlbStaff.New(factory);
			var staff3 = GlbStaff.New(factory);
			var staff4 = GlbStaff.New(factory);
			staff1.GS_Code = "ST1";
			staff2.GS_Code = "ST2";
			staff3.GS_Code = "ST3";
			staff4.GS_Code = "ST4";
			staff1.GS_LoginName = "One";
			staff2.GS_LoginName = "Two";
			staff3.GS_LoginName = "Three";
			staff4.GS_LoginName = "Four";

			var tagGroup = helper.CreateTagDefinition(factory, "TAG");
			var tagMagnitude = helper.CreateTagMagnitude(tagGroup, "TG1");

			var jobHeader = helper.CreateJobHeader<OrgHeader>(factory, false);

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow2");
			var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow3");
			var workflow4 = helper.CreateWorkflow(jobHeader, "Workflow4");
			var workflow5 = helper.CreateWorkflow(jobHeader, "Workflow5");

			factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddYears(1);

			using (EnvProxy.Instance.SetTemporaryUserContext(staff1.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				jobHeader.AddTag(tagMagnitude);
				workflow1.AddTag(tagMagnitude);
				factory.Save();
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			using (EnvProxy.Instance.SetTemporaryUserContext(staff2.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				workflow2.AddTag(tagMagnitude);
				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				workflow3.AddTag(tagMagnitude);
				factory.Save();

				workflow3.RemoveTag(tagMagnitude);
				factory.Save();

				workflow3.AddTag(tagMagnitude);
				factory.Save();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staff4.GS_LoginName, Guid.Empty, Guid.Empty))
			{
				workflow4.AddTag(tagMagnitude);
				factory.Save();
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMonths(-1);
			using (EnvProxy.Instance.SetTemporaryUserContext(staff3.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				workflow5.AddTag(tagMagnitude);
				factory.Save();
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());

			AssertRow(transactions, 0, "CM1", "BR1", "ST1", 2);
			AssertRow(transactions, 2, "CM2", "BR2", "ST2", 1);
			AssertRow(transactions, 2, "CM2", "BR2", "ST3", 2);
			AssertRow(transactions, 3, null, null, "ST4", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedBranchCode, string expectedUserCode, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.GetCompanyCode() == expectedCompanyCode && t.GetBranchCode() == expectedBranchCode && t.ClientStaffCode == expectedUserCode);

			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "BranchCode", expectedBranchCode, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2020, 7, 1), transaction.ServiceOccuredUTC);
			AssertEquals(assertPrefix + "TransactionGuidReference", "B63C3401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2020, 7);

		public void TestScriptShouldUseDateTimeParameters()
		{
			var script = new CreatedTagLinks();
			var parameters = ((IStlScript)new RefStlScriptRetriever(script)).GetInputParameters(AusydMonthRange.New(2020, 7));
			AssertEquals(true, parameters.All(x => x.SchemaColumn.SqlDbType == SqlDbType.SmallDateTime));
		}
	}
}
