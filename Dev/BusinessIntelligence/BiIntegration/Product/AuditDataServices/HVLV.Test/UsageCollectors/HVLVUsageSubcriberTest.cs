using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestsSubclassesOf(typeof(HVLVUsageSubscriber))]
	public abstract class HVLVUsageSubcriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestGetBranchCode_ShouldUseHomeBranchAsFallBack_WhenStaffBranchIsNull()
		{
			var factory = new BusinessObjectFactory();

			var editHomeBranch = factory.NewWithValidTestData<GlbBranch>();
			editHomeBranch.GB_Code = "XIV";

			var createHomeBranch = factory.NewWithValidTestData<GlbBranch>();
			createHomeBranch.GB_Code = "GNB";

			var editingStaff = factory.NewWithValidTestData<GlbStaff>();
			editingStaff.GS_GB_HomeBranch = editHomeBranch.PK;
			editingStaff.GS_GB_LastLogonBranch = new ZGuid();
			editingStaff.GS_Code = "BLM";

			var creatingStaff = factory.NewWithValidTestData<GlbStaff>();
			creatingStaff.GS_GB_HomeBranch = createHomeBranch.PK;
			creatingStaff.GS_Code = "RDM";

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_SystemCreateUser = creatingStaff.GS_Code;

			var item = consignment.Items.AddNew();
			item.HVI_SystemCreateUser = creatingStaff.GS_Code;
			factory.Save();

			var editingBranch = factory.Load<GlbBranch>(editingStaff.GS_GB_HomeBranch);
			AssertNotNull("Precondition: Branch is not null", editingBranch);

			var lastLogOnBranch = factory.Load<GlbBranch>(editingStaff.GS_GB_LastLogonBranch);
			AssertNull("Precondition: Branch is null - not found", lastLogOnBranch);

			var branch = TestHVLVUsageSubscriber.GetBranchFromStaffCode(creatingStaff.GS_Code, DataFactory);

			AssertEquals("GNB", branch.GB_Code);

			var expectedMessageBeginning = "Cannot find Last Logon Branch, using Home Branch instead.";

			AssertContains(expectedMessageBeginning, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected int GetRecordCountFromHXUTable()
		{
			return TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.HVLVUsage");
		}

		protected string GetUserFromHXUTable()
		{
			return TestConnection.ExecuteScalar<string>("SELECT HXU_GS_NKUser FROM dbo.HVLVUsage");
		}

		protected string GetUsageCodeFromHXUTable()
		{
			return TestConnection.ExecuteScalar<string>("SELECT HXU_Code from dbo.HVLVUsage");
		}

		#endregion

		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;

		protected HVLVUsageSubscriber TestHVLVUsageSubscriber => testHVLVUsageSubscriber ?? (testHVLVUsageSubscriber = NewHVlVUsageSubscriber());
		HVLVUsageSubscriber testHVLVUsageSubscriber;

		protected HVLVUsageSubscriber NewHVlVUsageSubscriber() =>
			(HVLVUsageSubscriber)Activator.CreateInstance
			(
				TestedTypeHelper.GetTestedType(GetType())
			);
	}
}
