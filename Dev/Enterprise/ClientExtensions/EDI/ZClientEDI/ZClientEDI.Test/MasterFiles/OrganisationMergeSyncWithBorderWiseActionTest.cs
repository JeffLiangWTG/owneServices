using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace ZClientEDI.Test.MasterFiles
{
	class OrganisationMergeSyncWithBorderWiseActionTest : TransactionedTestCase
	{
		public void TestMerge()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			AssertLastEditUser(org1.PK, OrganisationMergeSyncWithBorderWiseAction.OrgMergeUserCode, false);

			IMergeAction mergeAction = new OrganisationMergeSyncWithBorderWiseAction();

			mergeAction.Merge(org1, org2, OrganisationMergerActionOnSave.DeleteOnly);
			AssertLastEditUser(org1.PK, OrganisationMergeSyncWithBorderWiseAction.OrgMergeUserCode, false);

			mergeAction.Merge(org1, org2, OrganisationMergerActionOnSave.MergeOnly);
			AssertLastEditUser(org1.PK, OrganisationMergeSyncWithBorderWiseAction.OrgMergeUserCode, true);

			// Reset last edit user
			org1.Reload();
			org1.OH_IsForwarder = !org1.OH_IsForwarder;
			factory.Save();
			AssertLastEditUser(org1.PK, OrganisationMergeSyncWithBorderWiseAction.OrgMergeUserCode, false);

			mergeAction.Merge(org1, org2, OrganisationMergerActionOnSave.MergeAndDelete);
			AssertLastEditUser(org1.PK, OrganisationMergeSyncWithBorderWiseAction.OrgMergeUserCode, true);
		}

		void AssertLastEditUser(ZGuid orgPk, string userCode, bool expect)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var reloadedOrg = newFactory.Load<OrgHeader>(orgPk);
			if (expect)
			{
				AssertEquals(userCode, reloadedOrg.OH_SystemLastEditUser);
			}
			else
			{
				AssertNotEquals(userCode, reloadedOrg.OH_SystemLastEditUser);
			}
		}
	}
}
