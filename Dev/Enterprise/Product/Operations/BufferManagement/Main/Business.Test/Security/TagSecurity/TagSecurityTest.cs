using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test.Security.TagSecurity
{
	sealed class TagSecurityTest : BMSecurityTestCase
	{
		#region Helpers

		#region Codes

		const string systemTagForWorkQueuesPK = "4D479031-0142-4153-96DB-2FF982F47CDA";

		static string GetTagAddSecurityCheckPointCode(ZGuid tagPK)
		{
			return "TagAdd" + tagPK;
		}

		static string GetTagRemoveSecurityCheckPointCode(ZGuid tagPK)
		{
			return "TagRemove" + tagPK;
		}

		#endregion

		#region Checkpoints

		static SecurityCheckpoint GetAddTagCheckpoint(SecurityCore securityInstance)
		{
			return securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.TagAdd.Code));
		}

		static SecurityCheckpoint GetRemoveTagCheckpoint(SecurityCore securityInstance)
		{
			return securityInstance.FindCheckPoint(new CheckpointLookupKey(Env.Security.TagRemove.Code));
		}

		#endregion

		TagMagnitude CreateTagMagnitude(string code, string description, string usageScope)
		{
			var tagDefinition = Factory.New<TagDefinition>();
			tagDefinition.TGD_UsageScope = usageScope;
			tagDefinition.TGD_Code = code;

			var tagMagnitude = Factory.New<TagMagnitude>();
			tagMagnitude.TGM_TGD_Tag = tagDefinition.PK;
			tagMagnitude.TGM_Code = code;
			tagMagnitude.TGM_Description = description;

			return tagMagnitude;
		}

		#endregion

		public void TestAddTagPresentInSecurityTree()
		{
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			AssertNotNull(addTagMagnitudeCheckpoint);
			AssertEquals("Add Tag", addTagMagnitudeCheckpoint.DisplayText);
		}

		public void TestRemoveTagPresentInSecurityTree()
		{
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			AssertNotNull(removeTagMagnitudeCheckpoint);
			AssertEquals("Remove Tag", removeTagMagnitudeCheckpoint.DisplayText);
		}

		public void TestAddTagGroupsOnTGMGGPopulated()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();
			tagMagnitude1.TGM_GG_OwnerGroup = group1.PK;
			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GCC");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a tag should have an add checkpoint.",
				addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(group1.PK)));
			AssertNull("A group that is not an owner group for a tag should not have an add checkpoint.",
				addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(group2.PK)));
		}

		public void TestRemoveTagGroupsOnTGMGGPopulated()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();
			tagMagnitude1.TGM_GG_OwnerGroup = group1.PK;
			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GCC");

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a tag should have a remove checkpoint.",
				removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(group1.PK)));
			AssertNull("A group that is not an owner group for a tag should not have a remove checkpoint.",
				removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(group2.PK)));
		}

		public void TestAddTagHasNoGroupSpecifiedNode()
		{
			var tagWithNoOwnerGroup = Factory.NewWithValidTestData<TagMagnitude>();

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a tag should have an add checkpoint.",
				addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(ZGuid.Empty)));
		}

		public void TestRemoveTagHasNoGroupSpecifiedNode()
		{
			var tagWithNoOwnerGroup = Factory.NewWithValidTestData<TagMagnitude>();

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			AssertNotNull("A group that is an ownergroup for a tag should have a remove checkpoint.",
				removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(ZGuid.Empty)));
		}

		public void TestAddTagHasTagMagnitudesUnderOwnerGroupNode()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();
			tagMagnitude1.TGM_GG_OwnerGroup = group1.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			var group1Checkpoint = addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(group1.PK));
			AssertNotNull("group1 checkpoint should contain the child of the tag checkpoint, as group1 is the tags owner group.",
				group1Checkpoint.FindChild(GetTagAddSecurityCheckPointCode(tagMagnitude1.PK)));
		}

		public void TestRemoveTagHasTagMagnitudesUnderOwnerGroupNode()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();
			tagMagnitude1.TGM_GG_OwnerGroup = group1.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			var group1Checkpoint = removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(group1.PK));
			AssertNotNull("group1 checkpoint should contain the child of the tag checkpoint, as group1 is the tags owner group.",
				group1Checkpoint.FindChild(GetTagRemoveSecurityCheckPointCode(tagMagnitude1.PK)));
		}

		public void TestAddTag_TagsUnderUnspecifiedGroup()
		{
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			var unspecifiedGroupCheckpoint = addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(ZGuid.Empty));
			AssertNotNull("tags without an owner group should be found under the unspecified group checkpoint",
				unspecifiedGroupCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(tagMagnitude1.PK)));
		}

		public void TestRemoveTag_TagsUnderUnspecifiedGroup()
		{
			var tagMagnitude1 = Factory.NewWithValidTestData<TagMagnitude>();

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			var unspecifiedGroupCheckpoint = removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(ZGuid.Empty));
			AssertNotNull("tags without an owner group should be found under the unspecified group checkpoint",
				unspecifiedGroupCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(tagMagnitude1.PK)));
		}

		public void TestAddTag_OnlyHasTagMagnitudes_WithALLOrUSRCode()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagWithAllScope = Factory.NewWithValidTestData<TagMagnitude>();
			tagWithAllScope.TGM_GG_OwnerGroup = group1.PK;

			var tagWithUsrScope = CreateTagMagnitude("TBB", "tag 2", "USR");
			tagWithUsrScope.TGM_GG_OwnerGroup = group1.PK;

			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GBB");
			var tagWithRulScope = CreateTagMagnitude("TCC", "tag 3", "RUL");
			tagWithRulScope.TGM_GG_OwnerGroup = group2.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);

			var group1Checkpoint = addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(group1.PK));
			AssertNotNull(group1Checkpoint.FindChild(GetTagAddSecurityCheckPointCode(tagWithAllScope.PK)));
			AssertNotNull(group1Checkpoint.FindChild(GetTagAddSecurityCheckPointCode(tagWithUsrScope.PK)));

			AssertNull(addTagMagnitudeCheckpoint.FindChild(GetTagAddSecurityCheckPointCode(group2.PK)));
		}

		public void TestRemoveTag_OnlyHasTagMagnitudes_WithALLOrUSRCode()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tagWithAllScope = Factory.NewWithValidTestData<TagMagnitude>();
			tagWithAllScope.TGM_GG_OwnerGroup = group1.PK;

			var tagWithUsrScope = CreateTagMagnitude("TBB", "tag 2", "USR");
			tagWithUsrScope.TGM_GG_OwnerGroup = group1.PK;

			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GBB");
			var tagWithRulScope = CreateTagMagnitude("TCC", "tag 3", "RUL");
			tagWithRulScope.TGM_GG_OwnerGroup = group2.PK;

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
			var removeTagMagnitudeCheckpoint = GetRemoveTagCheckpoint(securityInstance);

			var group1Checkpoint = removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(group1.PK));
			AssertNotNull(group1Checkpoint.FindChild(GetTagRemoveSecurityCheckPointCode(tagWithAllScope.PK)));
			AssertNotNull(group1Checkpoint.FindChild(GetTagRemoveSecurityCheckPointCode(tagWithUsrScope.PK)));

			AssertNull(removeTagMagnitudeCheckpoint.FindChild(GetTagRemoveSecurityCheckPointCode(group2.PK)));
		}

		public void TestAddTag_TagDefintions_DontHaveWorkQueues()
		{
			var tag1 = Factory.NewWithValidTestData<TagMagnitude>();
			tag1.TGM_TGD_Tag = new ZGuid(systemTagForWorkQueuesPK);

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);

			AssertNull("Work queue tag should be in the workqueue section, not here.",
				securityInstance.FindCheckPoint(GetTagAddSecurityCheckPointCode(tag1.PK)));
		}

		public void TestRemoveTag_TagDefintions_DontHaveWorkQueues()
		{
			var tag1 = Factory.NewWithValidTestData<TagMagnitude>();
			tag1.TGM_TGD_Tag = new ZGuid(systemTagForWorkQueuesPK);

			Factory.Save();
			var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);

			AssertNull("Work queue tag should be in the workqueue section, not here.",
				securityInstance.FindCheckPoint(GetTagRemoveSecurityCheckPointCode(tag1.PK)));
		}

		public void TestTag_DbHits()
		{
			var group1 = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var tag1 = CreateTagMagnitude("TBA", "tag 1", "USR");
			tag1.TGM_GG_OwnerGroup = group1.PK;

			var group2 = BMSecurityTestHelper.CreateGroup(Factory, "GAB");
			var tag2 = CreateTagMagnitude("TBB", "tag 2", "USR");
			tag2.TGM_GG_OwnerGroup = group2.PK;

			var group3 = BMSecurityTestHelper.CreateGroup(Factory, "GAC");
			var tag3 = CreateTagMagnitude("TBC", "tag 3", "USR");
			tag3.TGM_GG_OwnerGroup = group3.PK;

			Factory.Save();

			var allowedHits = new Dictionary<string, int>
			{
				{ StmPrintQueueSchema.Constants.TableName, 1 },
				{ TagDefinitionSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 2 }
			};

			using (AssertDbHitsForAllFactories(allowedHits, useOnlyNewFactories: true, ignoreUnspecified: true, thresholdForUnspecified: 99))
			{
				var securityInstance = BMSecurityTestHelper.CreateSecurity(SecurityCollection, Staff);
				var addTagMagnitudeCheckpoint = GetAddTagCheckpoint(securityInstance);
			}
		}
	}
}
