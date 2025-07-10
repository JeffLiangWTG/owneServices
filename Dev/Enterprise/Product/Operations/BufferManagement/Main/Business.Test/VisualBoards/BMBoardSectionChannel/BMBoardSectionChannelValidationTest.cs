using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionChannelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestChannelTypeCode_WhenShowingUnchanneled_ShouldAllowOneTypeOnly()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var primaryChannel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			primaryChannel1.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			primaryChannel1.IsUnChanneled = true;
			var primaryChannel2 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			primaryChannel2.MSC_ChannelType = ChannelTypeList.Codes.Capability;

			var secondaryChannel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			secondaryChannel1.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			secondaryChannel1.IsUnChanneled = true;
			var secondaryChannel2 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			secondaryChannel2.MSC_ChannelType = ChannelTypeList.Codes.Capability;

			primaryChannel1.Validation.ValidateAll();
			primaryChannel2.Validation.ValidateAll();
			secondaryChannel1.Validation.ValidateAll();
			secondaryChannel2.Validation.ValidateAll();
			AssertNoErrors(primaryChannel1.MSC_ChannelTypeInfo);
			AssertNoErrors(primaryChannel2.MSC_ChannelTypeInfo);
			AssertNoErrors(secondaryChannel1.MSC_ChannelTypeInfo);
			AssertNoErrors(secondaryChannel2.MSC_ChannelTypeInfo);

			primaryChannel2.MSC_ChannelType = ChannelTypeList.Codes.Group;
			primaryChannel1.Validation.ValidateAll();
			primaryChannel2.Validation.ValidateAll();
			secondaryChannel1.Validation.ValidateAll();
			secondaryChannel2.Validation.ValidateAll();
			AssertHasError(primaryChannel2.MSC_ChannelTypeInfo, "When showing un-channeled work, only one type of channel is allowed.");
			AssertNoErrors(primaryChannel1.MSC_ChannelTypeInfo);
			AssertNoErrors(secondaryChannel1.MSC_ChannelTypeInfo);
			AssertNoErrors(secondaryChannel2.MSC_ChannelTypeInfo);
		}

		public void TestChannelKeys_Unchanneled_ShouldNotBeEntered()
		{
			var channel = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration.PrimaryAxisChannels.AddNew();

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			channel.Validation.ValidateAll();
			AssertHasError(channel.MSC_ParentIDInfo, "Please enter an Entity.");

			channel.IsUnChanneled = true;
			channel.Validation.ValidateAll();
			AssertNoErrors(channel.MSC_ParentIDInfo);
		}

		public void TestChannelTypeCode()
		{
			var channel = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration.PrimaryAxisChannels.AddNew();
			channel.Validation.ValidateAll();
			AssertHasError(channel.MSC_ChannelTypeInfo, "Please enter a Channel Type.");

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			AssertNoErrors(channel.MSC_ChannelTypeInfo);

			channel.MSC_ChannelType = "boo";
			AssertHasError(channel.MSC_ChannelTypeInfo, "Enter a valid Channel Type.");
		}

		public void TestUniqueChannelValidation_Resource()
		{
			var staff = Factory.New<GlbStaff>();

			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(section);
			channel1.MSC_ParentID = staff.PK;
			channel2.MSC_ParentID = staff.PK;

			AssertHasError(channel2.MSC_ParentIDInfo, "Cannot have the same channel more than once on one board section.");
		}

		public void TestUniqueChannelValidation_Group()
		{
			var group = Factory.New<GlbGroup>();

			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(section);
			channel1.MSC_ParentID = group.PK;
			channel2.MSC_ParentID = group.PK;

			AssertHasError(channel2.MSC_ParentIDInfo, "Cannot have the same channel more than once on one board section.");
		}

		public void TestUniqueChannelValidation_Capability()
		{
			var capability = Factory.New<GlbCapability>();

			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(section);
			channel1.MSC_ParentID = capability.PK;
			channel2.MSC_ParentID = capability.PK;

			AssertHasError(channel2.MSC_ParentIDInfo, "Cannot have the same channel more than once on one board section.");
		}

		public void TestUniqueChannelValidation_Tag()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(section);

			channel1.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			channel2.MSC_ChannelType = ChannelTypeList.Codes.Tag;

			channel1.MSC_ParentID = config.PrincessCelestiaTag.PK;
			channel2.MSC_ParentID = config.PrincessCelestiaTag.PK;

			AssertHasError(channel2.MSC_ParentIDInfo, "Cannot have the same channel more than once on one board section.");

			channel2.MSC_ParentID = config.DerpyHoovesTag.PK;
			AssertNoErrors(channel2.MSC_ParentIDInfo);
		}

		public void TestUniqueChannelValidation_CurrentUser()
		{
			var capability = Factory.New<GlbCapability>();

			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(section);
			channel1.MSC_ChannelType = ChannelTypeList.Codes.CurrentUser;
			channel2.MSC_ChannelType = ChannelTypeList.Codes.CurrentUser;

			AssertHasError(channel2.MSC_ChannelTypeInfo, "Cannot have the same channel more than once on one board section.");
		}

		public void TestValidation_ShouldAllowOnlyOneChannelObjectSelection()
		{
			const string noChannelMessage = "Please enter an Entity.";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var tag1 = Factory.NewWithValidTestData<TagMagnitude>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket1.PK;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.Validation.ValidateAll();
			AssertHasError(channel.MSC_ParentIDInfo, noChannelMessage);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Tag;
			channel.MSC_ParentID = tag1.PK;
			AssertNoErrors(channel.MSC_ParentIDInfo);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel.MSC_ParentID = staff1.PK;
			AssertNoErrors(channel.MSC_ParentIDInfo);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Group;
			channel.MSC_ParentID = group1.PK;
			AssertNoErrors(channel.MSC_ParentIDInfo);

			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			channel.MSC_ParentID = capability1.PK;
			AssertNoErrors(channel.MSC_ParentIDInfo);
		}

		public void TestValidation_InvalidKeys()
		{
			var channel = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration.PrimaryAxisChannels.AddNew();
			channel.MSC_ParentID = ZGuid.NewZGuid();
			AssertHasError(channel.MSC_ParentIDInfo, "Enter a valid Entity.");
			channel.MSC_ParentID = ZGuid.NewZGuid();
			AssertHasError(channel.MSC_ParentIDInfo, "Enter a valid Entity.");
			channel.MSC_ParentID = ZGuid.NewZGuid();
			AssertHasError(channel.MSC_ParentIDInfo, "Enter a valid Entity.");
			channel.MSC_ParentID = ZGuid.NewZGuid();
			AssertHasError(channel.MSC_ParentIDInfo, "Enter a valid Entity.");
		}

		public void TestCurrentUserChannelType()
		{
			var channel = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew().SectionConfiguration.PrimaryAxisChannels.AddNew();

			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel.Validation.ValidateAll();
			AssertHasError(channel.MSC_ParentIDInfo, "Please enter an Entity.");

			channel.MSC_ChannelType = ChannelTypeList.Codes.CurrentUser;
			channel.Validation.ValidateAll();
			AssertNoErrors(channel.MSC_ParentIDInfo);
		}
	}
}
