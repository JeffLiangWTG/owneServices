using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test.Security;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	sealed class WorkQueueSecurityTest : BMSecurityTestCase
	{
		#region Helpers

		#region Checkpoint Codes

		static string GetAddToQueueSecurityCheckPointCode(ZGuid groupPK)
		{
			return "AddTo-Q" + groupPK;
		}

		static string GetRemoveFromQueueSecurityCheckPointCode(ZGuid groupPK)
		{
			return "RemoveFrom-Q" + groupPK;
		}

		static string GetResequenceQueueSecurityCheckPointCode(ZGuid groupPK)
		{
			return "Resequence-Q" + groupPK;
		}

		#endregion

		#region checkpoints

		static SecurityCheckpoint GetAddToQueueCheckpoint(SecurityCore securityInstance)
		{
			return securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesAddToQueue.Code));
		}

		static SecurityCheckpoint GetRemoveFromQueueCheckpoint(SecurityCore securityInstance)
		{
			return securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesRemoveFromQueue.Code));
		}

		static SecurityCheckpoint GetResequenceQueueCheckpoint(SecurityCore securityInstance)
		{
			return securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.WorkQueuesResequenceQueue.Code));
		}

		#endregion

		#endregion

		public void TestAddToQueuePresentInSecurityTree()
		{
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addToQueueCheckpoint = GetAddToQueueCheckpoint(securityInstance);

			AssertNotNull(addToQueueCheckpoint);
			AssertEquals("Add To Queue", addToQueueCheckpoint.DisplayText);
		}

		public void TestRemoveFromQueuePresentInSecurityTree()
		{
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeFromQueueCheckpoint = GetRemoveFromQueueCheckpoint(securityInstance);

			AssertNotNull(removeFromQueueCheckpoint);
			AssertEquals("Remove From Queue", removeFromQueueCheckpoint.DisplayText);
		}

		public void TestReSequenceQueuePresentInSecurityTree()
		{
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var resequenceQueueCheckpoint = GetResequenceQueueCheckpoint(securityInstance);

			AssertNotNull(resequenceQueueCheckpoint);
			AssertEquals("Re-sequence Queue", resequenceQueueCheckpoint.DisplayText);
		}

		public void TestAddToQueueGroupsPopulated()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TA1", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;
			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GR2");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addToQueueCheckpoint = GetAddToQueueCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a workQueue should have a add to queue checkpoint.",
				addToQueueCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(group1.PK)));
			AssertNull("A group that is not an owner group for a workQueue should not have a add to queue checkpoint.",
				addToQueueCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(group2.PK)));
		}

		public void TestRemoveFromQueueGroupsPopulated()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TA1", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;
			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GR2");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeFromQueueCheckpoint = GetRemoveFromQueueCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a workQueue should have a remove from queue checkpoint.",
				removeFromQueueCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(group1.PK)));
			AssertNull("A group that is not an owner group for a workQueue should not have a remove from queue checkpoint.",
				removeFromQueueCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(group2.PK)));
		}

		public void TestResequenceQueueGroupsPopulated()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TA1", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;
			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GR2");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var resequenceQueueCheckpoint = GetResequenceQueueCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a workQueue should have a resequence queue checkpoint.",
				resequenceQueueCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(group1.PK)));
			AssertNull("A group that is not an owner group for a workQueue should not have a resequence queue checkpoint.",
				resequenceQueueCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(group2.PK)));
		}

		public void TestAddToQueue_UnderUnspecifiedGroup()
		{
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addToQueueCheckpoint = GetAddToQueueCheckpoint(securityInstance);

			var unspecifiedGroupCheckpoint = addToQueueCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(ZGuid.Empty));
			AssertNotNull("tags without an owner group should be found under the unspecified group checkpoint",
				unspecifiedGroupCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(workQueue1.PK)));
		}

		public void TestRemoveFromQueue_UnderUnspecifiedGroup()
		{
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeFromQueueCheckpoint = GetRemoveFromQueueCheckpoint(securityInstance);

			var unspecifiedGroupCheckpoint = removeFromQueueCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(ZGuid.Empty));
			AssertNotNull("tags without an owner group should be found under the unspecified group checkpoint",
				unspecifiedGroupCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(workQueue1.PK)));
		}

		public void TestResequenceQueue_UnderUnspecifiedGroup()
		{
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var resequenceQueueCheckpoint = GetResequenceQueueCheckpoint(securityInstance);

			var unspecifiedGroupCheckpoint = resequenceQueueCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(ZGuid.Empty));
			AssertNotNull("tags without an owner group should be found under the unspecified group checkpoint",
				unspecifiedGroupCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(workQueue1.PK)));
		}

		public void TestAddToQueue_UnderGroup()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addToQueueCheckpoint = GetAddToQueueCheckpoint(securityInstance);

			var groupCheckpoint = addToQueueCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(group1.PK));
			AssertNotNull("Tags with an owner group should be found under their owner group's checkpoint.",
				groupCheckpoint.FindChild(GetAddToQueueSecurityCheckPointCode(workQueue1.PK)));
		}

		public void TestRemoveFromQueue_UnderGroup()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeFromQueueCheckpoint = GetRemoveFromQueueCheckpoint(securityInstance);

			var groupCheckpoint = removeFromQueueCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(group1.PK));
			AssertNotNull("Tags with an owner group should be found under their owner group's checkpoint.",
				groupCheckpoint.FindChild(GetRemoveFromQueueSecurityCheckPointCode(workQueue1.PK)));
		}

		public void TestResequenceQueue_UnderGroup()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GR1");
			var workQueue1 = BMSTestHelper.CreateWorkQueue(Factory, "TAA", "tag 1");
			workQueue1.TGM_GG_OwnerGroup = group1.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var resequenceQueueCheckpoint = GetResequenceQueueCheckpoint(securityInstance);

			var groupCheckpoint = resequenceQueueCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(group1.PK));
			AssertNotNull("Tags with an owner group should be found under their owner group's checkpoint.",
				groupCheckpoint.FindChild(GetResequenceQueueSecurityCheckPointCode(workQueue1.PK)));
		}
	}
}
