using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardSectionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponents_ShouldNotIncludeChildComponents()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var childComponent = component.ChildComponents.AddNew();

			var section = system.Boards.AddNew().Sections.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { component }, section.Lookups.Components);
		}

		public void TestTimeFieldList()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var section = system.Boards.AddNew().Sections.AddNew();
			section.MS_FC_Component = component.PK;

			var list = section.SectionConfiguration.Lookups.TimeFieldList;
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.AgreedDeliveryDate));
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.CreateTime));
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.DoNotStartBeforeDate));
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.TransferTime));
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.WorkingTimeSinceStartable));

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			list = section.SectionConfiguration.Lookups.TimeFieldList;
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.TransferTime));
			Assert(list.ContainsCode(TimeProgressionFieldList.Codes.WorkingTimeSinceStartable));
		}

		public void TestChannelByList()
		{
			var section = Factory.New<BMBoardSection>();
			var list = section.SectionConfiguration.Lookups.ChannelByList;
			AssertEquals(DefaultChannelsProvider.SupportedChannelTypes.Count(), list.Count);

			foreach (var code in DefaultChannelsProvider.SupportedChannelTypes)
			{
				Assert(list.ContainsCode(code));
			}
		}

		public void TestChannelSecondaryByList()
		{
			var section = Factory.New<BMBoardSection>();
			var list = section.SectionConfiguration.Lookups.ChannelSecondaryByList;
			AssertEquals(DefaultChannelsProvider.SupportedChannelTypes.Count() + 1, list.Count);

			foreach (var code in DefaultChannelsProvider.SupportedChannelTypes)
			{
				Assert(list.ContainsCode(code));
			}
			Assert(list.ContainsCode(BMConstants.ChannelByTimeCode));
		}

		public void TestReleaseGroups()
		{
			var system = Factory.New<BMSystem>();
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "CCC";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "BBB";
			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "AAA";

			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group2.PK;
			var releaseGroup3 = system.ReleaseGroups.AddNew();
			releaseGroup3.FSG_GG_Group = group3.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var groups = section.SectionConfiguration.Lookups.SystemReleaseGroups;
			AssertEquals(8, groups.Count);
			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);
		}

		public void TestLastCellList()
		{
			var sectionConfiguration = Factory.New<BMBoardSection>().SectionConfiguration;

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			AssertLastCellItems(sectionConfiguration.Lookups.LastCellList, LastCellList.Codes.Left, LastCellList.Codes.Right);
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			AssertLastCellItems(sectionConfiguration.Lookups.LastCellList, LastCellList.Codes.Left, LastCellList.Codes.Right);

			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			AssertLastCellItems(sectionConfiguration.Lookups.LastCellList, LastCellList.Codes.Top, LastCellList.Codes.Bottom);
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
			AssertLastCellItems(sectionConfiguration.Lookups.LastCellList, LastCellList.Codes.Top, LastCellList.Codes.Bottom);
		}

		static void AssertLastCellItems(CodeDescriptionPairList list, params string[] expectedLastCellCodes)
		{
			AssertEquals(string.Format("List should contain {0} items", expectedLastCellCodes.Length), expectedLastCellCodes.Length, list.Count);
			foreach (var code in expectedLastCellCodes)
			{
				Assert("List should contain code " + code, list.ContainsCode(code));
			}
		}

		public void TestColorList()
		{
			var section = Factory.New<BMBoardSection>();
			Assert("Should contain a bunch of colors", section.Lookups.ColorList.Count > 20);
			Assert("Should have 'black' in it", section.Lookups.ColorList.OfType<ICodeDescription>().Any(pair => pair.Code == "Black"));
		}

		public void TestOrientationList()
		{
			var section = Factory.New<BMBoardSection>();
			AssertEquals(2, section.Lookups.OrientationList.Count);
		}

		public void TestComponents()
		{
			var lookups = new BMBoardSectionLookups(Factory.New<BMBoardSection>());
			AssertNotNull(lookups.Components);

			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			component1.FC_Name = "component1";

			var otherSystem = Factory.New<BMSystem>();
			var otherComponent = otherSystem.Components.AddNew();
			otherComponent.FC_Name = "otherComponent";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();

			AssertEquals("Should return all components since no component is selected in section", 2, section.Lookups.Components.Count);

			AssertCollectionContains(component1, section.Lookups.Components);
			AssertCollectionContains(otherComponent, section.Lookups.Components);

			section.MS_FC_Component = ZGuid.Empty;
			var defaults = section.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted with 'system', even though a component has not been selected", system.PK, defaults.First().Value);

			section.MS_FC_Component = component1.PK;
			defaults = section.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted with 'system'", system.PK, defaults.First().Value);

			section.MS_FC_Component = otherComponent.PK;
			defaults = section.Lookups.Components.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>();
			AssertEquals(1, defaults.Count());
			AssertEquals("BMSystem filter should be defaulted with 'otherSystem'", otherSystem.PK, defaults.First().Value);
		}

		public void TestSectionTypeDIAForWinzor()
		{
			using (Globals.SetIsWinzorForTest(true))
			{
				var lookups = new BMBoardSectionLookups(Factory.New<BMBoardSection>());
				var sectionTypes = lookups.SectionTypes;
				Assert(!sectionTypes.ContainsCode(BMConstants.NetworkDiagramSectionType));
			}
		}

		public void TestSectionTypeDIAForCW1()
		{
			using (Globals.SetIsWinzorForTest(false))
			{
				var lookups = new BMBoardSectionLookups(Factory.New<BMBoardSection>());
				var sectionTypes = lookups.SectionTypes;
				Assert(sectionTypes.ContainsCode(BMConstants.NetworkDiagramSectionType));
			}
		}
	}
}
