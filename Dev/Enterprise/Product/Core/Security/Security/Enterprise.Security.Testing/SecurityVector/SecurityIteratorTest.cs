using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.Testing
{
	sealed class SecurityIteratorTest : TestCaseWithFactory
	{
		#region TestSecurityIterator

		public void TestSecurityIterator()
		{
			var securities = new GlbSecurityCollection(Factory);
			securities.Load();

			var securityCore = new SecurityCore(securities, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			securityCore.LoadRegistrySecurityCheckPoints();

			var staffs = Enumerable.Range(0, 1).Select(i => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			var vector = new SecurityVector();
			vector.Initialise(securityCore);

			var securityCoreForIterator = new SecurityCore(securities, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			var iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.SetupNoInitialize(staffs);
			var summaries = iterator.GetSummaries(securities, securityCoreForIterator);

			AssertContainsExactElementsInAnyOrder(summaries.Select(x => x.Checkpoint.Code + x.Checkpoint.ItemGuid.ToString()),
				vector.Select(x => x.Checkpoint.Code + x.Checkpoint.ItemGuid.ToString()));
		}

		#endregion

		#region TestGetSummariesWithMultipleGroups

		public void TestGetSummariesWithMultipleGroupsWithDifferentLevelSecurities()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staff.GS_LoginName = staff.GS_FullName = "SS";
			staff.Groups.Add(group2);
			foreach (GlbGroup group in staff.Groups)
			{
				new SecurityTestHelper(Factory).CreateSecurity(Env.Security.OrgDetailsModify, group, null, null, null, false);
			}

			staff.Groups.Add(group1);
			new SecurityTestHelper(Factory).CreateSecurity(Env.Security.OrganisationModify, group1, null, null, null, true);

			Factory.Save();

			SecurityIterator<string> iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrgDetailsModify.Code));

			List<ISecuritySummary<string>> summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be granted to staff with group1", StringSecuritySummaryGenerator.Granted, summary.Summary);
			}

			GlbStaff staffGranted = Factory.NewWithValidTestData<FakeStaffForGroupForm>();
			staffGranted.Groups.Add(group1);

			GlbStaff staffDenied = Factory.NewWithValidTestData<FakeStaffForGroupForm>();
			staffDenied.Groups.Add(group2);

			iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staffGranted, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrganisationModify.Code));

			summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be granted to staff with group1", StringSecuritySummaryGenerator.Granted, summary.Summary);
			}

			iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staffDenied, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrganisationModify.Code));

			summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be deneid to staff with group2", StringSecuritySummaryGenerator.Denied, summary.Summary);
			}

			group1.GG_IsActive = false;
			var newFactory = new BusinessObjectFactory();
			newFactory.Load<GlbGroup>(group1.PK).GG_IsActive = false;
			newFactory.Save();
			iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrgDetailsModify.Code));

			summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be denied since group1 is inactive", StringSecuritySummaryGenerator.Denied, summary.Summary);
			}

			group1.GG_IsActive = true;
			group1.GG_IsSales = true;
			newFactory = new BusinessObjectFactory();
			var groupInNewFactory = newFactory.Load<GlbGroup>(group1.PK);
			groupInNewFactory.GG_IsActive = true;
			groupInNewFactory.GG_IsSales = true;
			newFactory.Save();
			iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrgDetailsModify.Code));

			summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be denied since group1 is a Sales Team", StringSecuritySummaryGenerator.Denied, summary.Summary);
			}
		}

		public void TestGetSummariesWithMultipleGroupsAndDefaultRights()
		{
			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			new SecurityTestHelper(Factory).CreateSecurity(Env.Security.OrganisationModify, group1, null, null, null, false);

			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.SecurityPermissions.RemoveAndDeleteAll();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staff.GS_LoginName = staff.GS_FullName = "SS";
			staff.Groups.Add(group1);
			staff.Groups.Add(group2);

			Factory.Save();

			SecurityIterator<string> iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrganisationModify.Code));

			List<ISecuritySummary<string>> summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("All securities should be granted (defaulted) from Group 2", StringSecuritySummaryGenerator.Granted, summary.Summary);
			}
		}

		public void TestGetSummariesWithGroupAndDefaultRights()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "G1";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staff.GS_LoginName = staff.GS_FullName = "SS";
			staff.Groups.Add(group);

			Factory.Save();

			var iterator = new SecurityIterator<string>(new BusinessObjectFactory(), new StringSecuritySummaryGenerator());
			iterator.Setup(new GlbGroup[] { group }, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.Operations.Code));

			var summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			AssertEquals("Operations security should be denied (defaulted) from Group 1",
				StringSecuritySummaryGenerator.Denied, summaries.First(s => s.Checkpoint.Code == Env.Security.Operations.Code && s.Staff.GS_Code == staff.GS_Code).Summary);
		}

		public void TestGetSummariesForNonOperationalUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsOperational = false;
			staff.GS_FullName = "Bob";

			Factory.Save();

			SecurityIterator<string> iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.OrganisationModify.Code));

			List<ISecuritySummary<string>> summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("Staff should have no access to the module since he is non-operational", StringSecuritySummaryGenerator.Denied, summary.Summary);
			}

			iterator = new SecurityIterator<string>(Factory, new StringSecuritySummaryGenerator());
			iterator.Setup(staff, new ZArchitecture.Modules.CheckpointLookupKey(Env.Security.ActiveUser.Code)); //this is a SecurityCheckpointNonOperationalAllowed

			summaries = iterator.GetSummaries().ToList();
			Assert(summaries.Count > 0);
			foreach (ISecuritySummary<string> summary in summaries)
			{
				AssertEquals("Staff should have access to the module since the security check points are SecurityCheckpointNonOperationalAllowed", StringSecuritySummaryGenerator.Granted, summary.Summary);
			}
		}

		#endregion
	}
}
