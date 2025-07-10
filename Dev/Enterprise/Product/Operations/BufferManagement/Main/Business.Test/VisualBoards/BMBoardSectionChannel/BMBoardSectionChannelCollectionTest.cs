using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionChannelCollection))]
	class BMBoardSectionChannelCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardSectionChannelCollection>
	{
		public void TestAddToCollectionActuallyWorks()
		{
			var collection = GetCollectionToTest();

			AssertEquals(0, collection.Count);

			var channel = collection.AddNew();

			AssertEquals(1, collection.Count);
			AssertEquals(ChannelAxisCodeList.Codes.Primary, channel.MSC_Axis);
		}

		public void TestSequenceNumber()
		{
			var section = Factory.New<BMSystem>().Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(section);

			AssertEquals(1, channel1.MSC_Sequence);
			AssertEquals(2, channel2.MSC_Sequence);
			AssertEquals(3, channel3.MSC_Sequence);
		}

		[TestDate(2014, 1, 02)]
		public void TestOnChannelUpdateBoardLastEditTimeChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "systemname";
			var section = system.Boards.AddNew().Sections.AddNew();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);

			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, channel1.Section.Board.MB_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			channel1.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel1.MSC_ParentID = Factory.NewWithValidTestData<GlbStaff>().PK;
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, channel1.Section.Board.MB_SystemLastEditTimeUtc);
		}

		#region Implementation

		protected override BMBoardSectionChannelCollection GetCollectionToTest()
		{
			return Section.SectionConfiguration.PrimaryAxisChannels;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Section.SectionConfiguration.PrimaryAxisChannels.AddNew();
		}

		BMBoardSection Section
		{
			get
			{
				if (section == null)
				{
					var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

					section = config.BufferSection;
				}

				return section;
			}
		}

		BMBoardSection section;

		#endregion
	}
}
