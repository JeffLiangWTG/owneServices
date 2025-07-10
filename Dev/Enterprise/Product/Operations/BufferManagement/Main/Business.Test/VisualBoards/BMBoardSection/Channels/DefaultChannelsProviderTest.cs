using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class DefaultChannelsProviderTest : BMSTestCaseWithFactory
	{
		public void TestSupportedChannelTypes()
		{
			var supportedTypes = DefaultChannelsProvider.SupportedChannelTypes.ToArray();

			AssertEquals(3, supportedTypes.Length);
			AssertEquals(ChannelTypeList.Codes.Resource, supportedTypes[0]);
			AssertEquals(ChannelTypeList.Codes.NotChanneled, supportedTypes[1]);
			AssertEquals(ChannelTypeList.Codes.ReleaseSchedulerChannels, supportedTypes[2]);
		}

		public void TestAddMissingDefaultChannels_Resource()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(new[] { staff1, staff2, staff3 });

			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.MSC_ParentID = staff1.PK;
			AssertEquals(1, sectionConfiguration.PrimaryAxisChannels.Count);

			DefaultChannelsProvider.AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.PrimaryAxisChannels, ChannelTypeList.Codes.Resource, false);
			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff1.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));

			DefaultChannelsProvider.AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.SecondaryAxisChannels, ChannelTypeList.Codes.Resource, false);
			AssertEquals(3, sectionConfiguration.SecondaryAxisChannels.Count);
			Assert(sectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff1.PK));
			Assert(sectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));
		}

		public void TestRemoveChannelsOnDeletedResource()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(new[] { staff1, staff2, staff3, staff4 });

			staff4.GS_IsActive = false;

			Factory.Save();

			var system = Factory.New<BMSystem>();
			system.FS_Name = "Tim";
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff1.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));

			sectionConfiguration.OverrideChannels = true;
			staff1.Delete();
			Factory.Save();

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, section.Factory);

			AssertEquals(2, sectionConfiguration.PrimaryAxisChannels.Count);

			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff2.PK));
			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));

			staff2.GS_IsActive = false;
			Factory.Save();

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, section.Factory);

			AssertEquals(1, sectionConfiguration.PrimaryAxisChannels.Count);

			Assert(sectionConfiguration.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ParentID == staff3.PK));
		}

		public void TestRemoveChannels_SectionContainingUnChanneledChannel_WhenSectionReLoaded()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			AssertEquals(1, section.SectionConfiguration.PrimaryAxisChannels.Count);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			AssertEquals(1, loadedSection.SectionConfiguration.PrimaryAxisChannels.Count);

			DefaultChannelsProvider.RefreshChannels(loadedSection.SectionConfiguration, newFactory);

			AssertEquals(1, loadedSection.SectionConfiguration.PrimaryAxisChannels.Count);
		}

		public void TestAddMissingDefaultChannels_NotChanneled()
		{
			var group = Factory.New<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			group.Staff.AddRange(new[] { staff1, staff2, staff3 });

			var system = Factory.New<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.MSC_ParentID = staff1.PK;
			AssertEquals(1, sectionConfiguration.PrimaryAxisChannels.Count);

			DefaultChannelsProvider.AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.PrimaryAxisChannels, ChannelTypeList.Codes.NotChanneled, false);
			AssertEquals(1, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(staff1.PK, sectionConfiguration.PrimaryAxisChannels[0].MSC_ParentID);

			DefaultChannelsProvider.AddMissingDefaultChannels(sectionConfiguration, sectionConfiguration.SecondaryAxisChannels, ChannelTypeList.Codes.NotChanneled, false);
			AssertEquals(0, sectionConfiguration.SecondaryAxisChannels.Count);
		}

		public void TestRemoveChannels_ResourceRemovedFromReleaseGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(new[] { staff1, staff2, staff3 });

			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = CreateBoardSection(CreateBucket(system));
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(staff1.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory).EntityPK);
			AssertEquals(staff2.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[1], section.Factory).EntityPK);
			AssertEquals(staff3.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[2], section.Factory).EntityPK);

			group.Staff.Remove(staff1);
			Factory.Save();

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, Factory);

			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(2, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(staff2.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory).EntityPK);
			AssertEquals(staff3.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[1], section.Factory).EntityPK);
		}

		public void TestPurgeIgnoresUnchanneledChannels()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(new[] { staff1, staff2, staff3 });

			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = CreateBoardSection(CreateBucket(system));
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(false, sectionConfiguration.ShowUnchanneled);
			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(staff1.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory).EntityPK);
			AssertEquals(staff2.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[1], section.Factory).EntityPK);
			AssertEquals(staff3.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[2], section.Factory).EntityPK);

			sectionConfiguration.ShowUnchanneled = true;

			DefaultChannelsProvider.RefreshChannels(sectionConfiguration, Factory);

			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(4, sectionConfiguration.PrimaryAxisChannels.Count);
			AssertEquals(staff1.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory).EntityPK);
			AssertEquals(staff2.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[1], section.Factory).EntityPK);
			AssertEquals(staff3.PK, viewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[2], section.Factory).EntityPK);
			AssertEquals(true, sectionConfiguration.PrimaryAxisChannels[3].IsUnChanneled);
		}

		public void TestShouldHitDBOnceAndLoadAllStaffPhoto_WhenRefreshChannels()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.ProfileImage = new Bitmap(1024, 768);
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.ProfileImage = new Bitmap(800, 600);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);

			var system = CreateSystem("ORG");
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(CreateBucket(system));
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			group.Staff.Add(staff2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var sectionConfiguration = newFactory.Load<BMBoardSection>(section.PK).SectionConfiguration;

			var expectedHits = new Dictionary<string, int>()
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ BMBoardSectionChannelSchema.Constants.TableName, 2 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ GlbGroupLinkSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsForAllFactories(expectedHits, includeFactoryPredicate: f => f == newFactory))
			using (TestConnection.TrackExecutedCommands())
			{
				DefaultChannelsProvider.RefreshChannels(sectionConfiguration, newFactory);

				var staffs = sectionConfiguration.ReleaseGroup.Staff.Cast<GlbStaff>().ToArray();

				AssertNotNull(staffs[0].ProfileImage);
				AssertNotNull(staffs[1].ProfileImage);

				var holidayDBHits = Factory.GetTableHitCount(GlbHolidaySchema.Constants.TableName);
				var staffQueries = TestConnection.ExecutedCommands.Where(q => q.Contains("FROM dbo.GlbStaff") || q.Contains("FROM [GlbStaff]")).ToArray();

				AssertEquals(1, staffQueries.Length);
			}
		}
	}
}
