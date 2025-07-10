using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(GLJournalApprovalFilterBusinessObject))]
	public class GLJournalApprovalFilterBusinessObjectTest : TransactionApprovalFilterBusinessObjectTest
	{
		public void TestCurrentUserSecurityFilter()
		{
			var approval_CurrentUser = Factory.NewWithValidTestData<GenApprovalRequest>();
			approval_CurrentUser.XP_ParentID = ZGuid.NewZGuid();
			var approval_AnotherUser = Factory.NewWithValidTestData<GenApprovalRequest>();
			approval_AnotherUser.XP_ParentID = ZGuid.NewZGuid();
			var expectedUserCode = "XXX";
			approval_AnotherUser.XP_SystemCreateUser = expectedUserCode;
			Factory.Save();

			AssertEquals("Precondition: approval_CurrentUser.XP_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, approval_CurrentUser.XP_SystemCreateUser);
			AssertEquals("Precondition: approval_AnotherUser.XP_SystemCreateUser", expectedUserCode, approval_AnotherUser.XP_SystemCreateUser);

			var filterBO = (GLJournalApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filterCollection = new ActiveBusinessObjectCollection<GenApprovalRequest>(Factory);
			filterCollection.AdditionalFilter = filterBO.Filter;

			AssertCollectionContains("User with rights: approval_CurrentUser", approval_CurrentUser, filterCollection);
			AssertCollectionContains("User with rights: approval_AnotherUser", approval_AnotherUser, filterCollection);

			Env.Security.GLJournalApprovalShowAllUsersRequests.IsAllowed = false;
			filterCollection.AdditionalFilter = filterBO.Filter;

			AssertCollectionContains("User with rights: approval_CurrentUser", approval_CurrentUser, filterCollection);
			AssertCollectionNotContains("User with rights: approval_AnotherUser", approval_AnotherUser, filterCollection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GLJournalApprovalFilterBusinessObject();
		}
	}
}
