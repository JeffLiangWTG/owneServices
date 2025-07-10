using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Service.Test
{
	public abstract class ComponentSectionConfigurationBuilderTestBase : TestCaseWithFactory
	{
		public void TestGetConfiguration_ShouldReturnCorrectComponentSectionConfigurationDTOs()
		{
			var board = CreateBoardForConfiguration();
			var sectionConfigDTOs = Service.GetConfiguration(board.PK.ToGuid()).Sections.ToList();

			AssertNotNull(sectionConfigDTOs);
			AssertEquals(board.Sections.Count, sectionConfigDTOs.Count);

			foreach (var componentSectionConfigDTO in sectionConfigDTOs)
			{
				var section = board.Sections.First(b => b.PK == componentSectionConfigDTO.SectionPK);

				AssertComponentSectionConfigurationValues(section, componentSectionConfigDTO as ComponentSectionConfigurationDTO);
			}
		}

		public void TestGetConfiguration_ShouldReturnCorrectBackgroundColor()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";

			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var bufferSection = board.Sections.AddNew();

			bufferSection.MS_FC_Component = buffer.PK;
			bufferSection.BackgroundColor = Color.Blue.Name;

			Factory.Save();

			var componentSectionConfigurationDTO = Service.GetConfiguration(boardPK).Sections.First() as ComponentSectionConfigurationDTO;

			AssertEquals(Color.Blue.ToHex(), componentSectionConfigurationDTO.BackgroundColor);
		}

		public void TestGetStaffName_ShouldReturnCorrectName()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var bufferSection = board.Sections.AddNew();

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "staff1 fullName");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABA", "staff2 fullName");
			staff2.GS_FriendlyName = "staff2 friendlyName";

			bufferSection.MS_FC_Component = buffer.PK;

			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff2.PK);

			Factory.Save();

			var componentSectionConfigurationDTO = Service.GetConfiguration(boardPK).Sections.First() as ComponentSectionConfigurationDTO;

			var staff1Channel = componentSectionConfigurationDTO.PrimaryChannels.Channels.FirstOrDefault(channel => channel.EntityPK == staff1.PK);
			var staff2Channel = componentSectionConfigurationDTO.PrimaryChannels.Channels.FirstOrDefault(channel => channel.EntityPK == staff2.PK);

			AssertEquals("StaffName Displayed is the full name", staff1Channel?.Name, staff1.GS_FullName);
			AssertEquals("StaffName Displayed is the friendly name", staff2Channel?.Name, staff2.GS_FriendlyName);
		}

		public void TestGetConfiguration_ShouldReturnChannel_WhenResoureWasAddedInReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Resource 2");
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(resource1);
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var bufferSection = board.Sections.AddNew();

			bufferSection.MS_FC_Component = buffer.PK;
			bufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;
			bufferSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			Service = new BoardService();
			var componentSectionConfigurationDTO = Service.GetConfiguration(boardPK).Sections.First() as ComponentSectionConfigurationDTO;

			AssertEquals("Precondition", 1, componentSectionConfigurationDTO.PrimaryChannels.Channels.Count());
			AssertEquals("Precondition", resource1.PK.ToGuid(), componentSectionConfigurationDTO.PrimaryChannels.Channels.First().EntityPK);

			group.Staff.Add(resource2);

			Factory.Save();

			Service = new BoardService();
			componentSectionConfigurationDTO = Service.GetConfiguration(boardPK).Sections.First() as ComponentSectionConfigurationDTO;

			AssertEquals(2, componentSectionConfigurationDTO.PrimaryChannels.Channels.Count());
			var channelEntityPKs = componentSectionConfigurationDTO.PrimaryChannels.Channels.Select(channel => channel.EntityPK).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { resource1.PK.ToGuid(), resource2.PK.ToGuid() }, channelEntityPKs);
		}

		public void TestGetConfiguration_ShouldReturnChannel_WhenChannelByNonChanneled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var group = Factory.New<GlbGroup>();

			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var bufferSection = board.Sections.AddNew();

			bufferSection.MS_FC_Component = buffer.PK;
			bufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;
			bufferSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			Factory.Save();

			var componentSectionConfigurationDTO = Service.GetConfiguration(boardPK).Sections.First() as ComponentSectionConfigurationDTO;

			AssertEquals("Should return 1 channel when section channel by is not channeled", 1, componentSectionConfigurationDTO.PrimaryChannels.Channels.Count());
			AssertEquals(Guid.Empty, componentSectionConfigurationDTO.PrimaryChannels.Channels.First().EntityPK);
			AssertEquals("", componentSectionConfigurationDTO.PrimaryChannels.Channels.First().Name);
		}

		#region Active Additional Components

		public void TestGetConfiguration_WhenSectionHasMixOfInactiveAndActiveAdditionalComponents_ShouldReturnSectionWithActiveComponentsOnly()
		{
			AssertGetConfiguration_SectionActiveAdditionalComponents(mainComponentIsActive: true, additionalComponent1IsActive: true, additionalComponent2IsActive: false);
		}

		public void TestGetConfiguration_WhenSectionHasNoActiveAdditionalComponents_ShouldReturnSectionWithNoAdditionalComponents()
		{
			AssertGetConfiguration_SectionActiveAdditionalComponents(mainComponentIsActive: true, additionalComponent1IsActive: false, additionalComponent2IsActive: false);
		}

		public void TestGetConfiguration_WhenSectionHasMainComponentInactive_ShouldNotReturnSection()
		{
			AssertGetConfiguration_SectionActiveAdditionalComponents(mainComponentIsActive: false, additionalComponent1IsActive: true, additionalComponent2IsActive: true);
		}

		void AssertGetConfiguration_SectionActiveAdditionalComponents(bool mainComponentIsActive, bool additionalComponent1IsActive, bool additionalComponent2IsActive)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer 1, the main component!");
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			buffer.FC_IsActive = mainComponentIsActive;

			var anotherBuffer = BMSTestHelper.CreateBuffer(system, "Buffer 2, the other buffer!");
			var anotherBufferComponent = BMSTestHelper.CreateAdditionalComponent(section, anotherBuffer);
			anotherBuffer.FC_IsActive = additionalComponent1IsActive;

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket 1, lonely bucket.");
			var bucketComponent = BMSTestHelper.CreateAdditionalComponent(section, bucket);
			bucket.FC_IsActive = additionalComponent2IsActive;

			Factory.Save();

			AssertEquals("Precondition - The main component buffer should have correct active status.", mainComponentIsActive, section.Component.FC_IsActive);
			AssertEquals("Precondition - The additional component buffer should have correct active status.", additionalComponent1IsActive, anotherBufferComponent.Component.FC_IsActive);
			AssertEquals("Precondition - The additional component bucket should have correct active status.", additionalComponent2IsActive, bucketComponent.Component.FC_IsActive);

			var components = Service.GetConfiguration(board.PK.ToGuid()).Sections.Cast<ComponentSectionConfigurationDTO>().FirstOrDefault()?.Components ?? Enumerable.Empty<ComponentDTO>();
			var expectedComponentDTOs = new[] {
					new { Include = mainComponentIsActive, ComponentDTOPK = section.Component.PK.ToGuid() },
					new { Include = mainComponentIsActive && additionalComponent1IsActive, ComponentDTOPK = anotherBufferComponent.Component.PK.ToGuid() },
					new { Include = mainComponentIsActive && additionalComponent2IsActive, ComponentDTOPK = bucketComponent.Component.PK.ToGuid() }
				}
			.Where(l => l.Include)
			.Select(l => l.ComponentDTOPK);

			AssertContainsExactElementsInAnyOrder("Only correct active components should be returned.", expectedComponentDTOs, components.Select(l => l.PK));
		}

		#endregion

		#region CreateBoardForConfiguration

		protected BMBoard CreateBoardForConfiguration()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var board = system.Boards.AddNew();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "GROUP";
			var capability = BMSTestHelper.CreateCapability(Factory, description: "Capability");
			var tagMag = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "FLP"), "TAG", description: "Tag Magnitude");

			CreateBufferSection(board, buffer, GlbStaff.CurrentUser, group, capability, tagMag);
			CreateBucketSection(board, bucket, GlbStaff.CurrentUser, group, capability, tagMag);

			Factory.Save();

			return board;
		}

		protected void CreateBufferSection(BMBoard board, BMComponent buffer, IGlbStaff staff, GlbGroup group, GlbCapability capability, TagMagnitude tagMag, string additionalBuffer = "additionalBuffer", bool addReleaseScheduleSection = true, bool addAcceptabilityBands = true)
		{
			var bufferSection = BMSTestHelper.CreateBoardSection(buffer, board, row: 0, col: 0, rowHeightPercent: 90, colWidthPercent: 80, cellsPerSubsection: 10);

			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK, displaySequence: 5);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Group, group.PK, displaySequence: 4);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Capability, capability.PK, displaySequence: 3);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Tag, tagMag.PK, displaySequence: 2);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.NotChanneled, Guid.Empty, displaySequence: 1);

			BMSTestHelper.CreateAdditionalComponent(bufferSection, BMSTestHelper.CreateBuffer(board.System, additionalBuffer));
			var bufferSectionConfig = bufferSection.SectionConfiguration;

			bufferSectionConfig.SectionNameIsOverridden = true;
			bufferSectionConfig.SectionNameOverride = "Other Buffer Name";
			bufferSectionConfig.Subsections = 1;
			bufferSectionConfig.FlowDirection = FlowDirectionList.Codes.Left;
			bufferSectionConfig.FadeBackgroundAtPercentage = 30;
			bufferSectionConfig.ShowZones = true;
			bufferSectionConfig.BufferZone0Color = ColorList.NameFromColor(Color.Blue);
			bufferSectionConfig.BufferZone1Color = ColorList.NameFromColor(Color.Red);
			bufferSectionConfig.BufferZone2Color = ColorList.NameFromColor(Color.Yellow);

			if (addAcceptabilityBands)
			{
				CreateAcceptabilityBands(bufferSection);
			}

			if (addReleaseScheduleSection)
			{
				var bufferReleaseSchedulerSection = BMSTestHelper.CreateBoardSection(buffer, board, row: 1, col: 0, rowHeightPercent: 70, colWidthPercent: 60);

				bufferReleaseSchedulerSection.ColSpan = 2;

				var bufferReleaseSchedulerSectionConfig = bufferReleaseSchedulerSection.SectionConfiguration;
				bufferReleaseSchedulerSectionConfig.IsReleaseScheduler = true;
				bufferReleaseSchedulerSectionConfig.EnableShowCurrentItemsFilterByDefault = true;
				bufferReleaseSchedulerSectionConfig.HideCapabilityTasksFromResourceChannels = true;
				bufferReleaseSchedulerSectionConfig.HideResourceTasksFromCapabilityChannels = true;
			}
		}

		protected void CreateBucketSection(BMBoard board, BMComponent bucket, IGlbStaff staff, GlbGroup group, GlbCapability capability, TagMagnitude tagMag, string additionalBucket = "additionalBucket")
		{
			var bucketSection = BMSTestHelper.CreateBoardSection(bucket, board, row: 0, col: 1, rowHeightPercent: 50, colWidthPercent: 40, cellsPerSubsection: 4);

			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Resource, staff.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Group, group.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Capability, capability.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Tag, tagMag.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.NotChanneled, Guid.Empty);

			BMSTestHelper.CreateAdditionalComponent(bucketSection, BMSTestHelper.CreateBucket(bucket.System, additionalBucket));

			var bucketSectionConfig = bucketSection.SectionConfiguration;

			bucketSectionConfig.Subsections = 1;
			bucketSectionConfig.FlowDirection = FlowDirectionList.Codes.Down;
			bucketSectionConfig.LastCell = LastCellList.Codes.Right;
			bucketSectionConfig.MaxOverdueSlots = 2;
			bucketSectionConfig.FadeBackgroundAtPercentage = 50;
			bucketSectionConfig.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			bucketSectionConfig.TimePerCell = TimeSpan.FromMinutes(30);
			bucketSectionConfig.TimeField = TimeProgressionFieldList.Codes.CreateTime;
			bucketSectionConfig.EnableShowCurrentItemsFilterByDefault = true;
		}

		#endregion

		#region Assertions

		void AssertComponentSectionConfigurationValues(BMBoardSection section, ComponentSectionConfigurationDTO componentSectionConfigDTO)
		{
			var sectionConfig = section.Configuration as BMComponentSectionConfiguration;

			AssertNotNull(sectionConfig);
			AssertNotNull(componentSectionConfigDTO);
			AssertEquals(section.PK, componentSectionConfigDTO.SectionPK);
			AssertEquals(SectionType.Component, componentSectionConfigDTO.Type);
			AssertEquals(section.SectionName, componentSectionConfigDTO.Name);

			if (sectionConfig.IsBuffer)
			{
				AssertEquals(ComponentType.Buffer, componentSectionConfigDTO.ComponentType);
				AssertBufferZoneColorsValues(componentSectionConfigDTO.BufferZonesColors);
				AssertNull(componentSectionConfigDTO.BackgroundColor);
			}

			if (sectionConfig.IsBucket)
			{
				AssertEquals(ComponentType.Bucket, componentSectionConfigDTO.ComponentType);
				AssertNull(componentSectionConfigDTO.BufferZonesColors);
				AssertNull(componentSectionConfigDTO.BackgroundColor);
			}

			AssertLayoutValues(section, componentSectionConfigDTO.Layout);
			AssertComponentsValues(section, sectionConfig, componentSectionConfigDTO.Components.ToList());
			AssertFlowDirectionValue(sectionConfig.FlowDirection, componentSectionConfigDTO.FlowDirection);
			AssertLastCellValue(sectionConfig.LastCell, componentSectionConfigDTO.LastCellPosition);
			AssertEquals(sectionConfig.MaxOverdueSlots, componentSectionConfigDTO.MaxOverdueSlots);
			AssertEquals(sectionConfig.FadeBackgroundAtPercentage, componentSectionConfigDTO.FadeBackgroundAtPercentage);
			AssertEquals(sectionConfig.IsReleaseScheduler, componentSectionConfigDTO.IsReleaseScheduler);
			AssertEquals(sectionConfig.EnableShowCurrentItemsFilterByDefault, componentSectionConfigDTO.EnableShowCurrentItemsFilterByDefault);
			AssertChannelsValues(sectionConfig.OrderedPrimaryAxisChannels.ToArray(), sectionConfig.OverrideChannels, sectionConfig.ChannelBy, componentSectionConfigDTO.PrimaryChannels);
			AssertChannelsValues(sectionConfig.OrderedSecondaryAxisChannels.ToArray(), sectionConfig.OverrideSecondaryChannels, sectionConfig.ChannelSecondaryBy, componentSectionConfigDTO.SecondaryChannels);
			AssertCellsValues(section, sectionConfig, sectionConfig.PrimaryAxisChannels, sectionConfig.SecondaryAxisChannels, componentSectionConfigDTO.Cells);
			AssertAcceptabilityBandValues(section.SectionConfiguration.AcceptabilityBands, componentSectionConfigDTO.AcceptabilityBands.ToArray());
		}

		void CreateAcceptabilityBands(BMBoardSection section)
		{
			var band1 = BMSTestHelper.CreateAcceptabilityBand(
				Factory,
				cautionMin: 1,
				goodMin: 2,
				excellentMin: 3,
				excellentMax: 4,
				goodMax: 5,
				cautionMax: 6,
				"Acceptability Band 1"
			);
			var band1InSection = BMSTestHelper.AddAcceptabilityBandToSection(section, band1);
			band1InSection.DisplaySequence = 1;

			var band2 = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(
				section.Component,
				cautionMin: 10,
				goodMin: 20,
				excellentMin: 30,
				excellentMax: 40,
				goodMax: 50,
				cautionMax: 60,
				"Acceptability Band 2 with num workflows"
			);
			var band2InSection = BMSTestHelper.AddAcceptabilityBandToSection(section, band2);
			band2InSection.DisplaySequence = 2;
			band2InSection.DisplayUnits = "bla";

			var band3 = BMSTestHelper.CreateAcceptabilityBand(
				section.Component,
				cautionMin: 10,
				goodMin: 20,
				excellentMin: 30,
				excellentMax: 40,
				goodMax: 50,
				cautionMax: 60,
				"Acceptability Band 3 overriden"
			);
			var band3InSection = BMSTestHelper.AddAcceptabilityBandToSection(section, band3);
			band3InSection.DisplaySequence = 3;
			band3InSection.AreBoundaryValuesOverridden = true;
			band3InSection.CautionMinOverride = 5;
			band3InSection.GoodMinOverride = 10;
			band3InSection.CautionMaxOverride = 70;

			var band4 = BMSTestHelper.CreateAcceptabilityBand(
				section.Component,
				cautionMin: 10,
				goodMin: 20,
				excellentMin: 30,
				excellentMax: 40,
				goodMax: 50,
				cautionMax: 60,
				"They don't want to show me :("
			);
			var band4InSection = BMSTestHelper.AddAcceptabilityBandToSection(section, band4);
			band4InSection.DisplaySequence = 4;
			band4InSection.SelectedShowOnOption = AcceptabilityBandShowOnOption.Heading;
		}

		void AssertAcceptabilityBandValues(BoardSectionAcceptabilityBandCollection sectionAcceptabilityBands, AcceptabilityBandDTO[] acceptabilityBandDTOs)
		{
			var sectionBands = sectionAcceptabilityBands.Cast<BoardSectionAcceptabilityBand>();
			var headingsBands = sectionBands.Where(band => band.SelectedShowOnOption == AcceptabilityBandShowOnOption.Heading).ToArray();

			AssertEquals("Should not have Headings Bands", acceptabilityBandDTOs.Any(band => headingsBands.Any(headingBand => headingBand.AcceptabilityBandPK == band.PK)), false);

			var notHeadingsBands = sectionBands.Where(band => band.SelectedShowOnOption != AcceptabilityBandShowOnOption.Heading).ToArray();

			for (int i = 0; i < notHeadingsBands.Length; i++)
			{
				var sectionAcceptabilityBand = notHeadingsBands[i];
				var acceptabilityBandDTO = acceptabilityBandDTOs[i];
				var dtoBoundaryValues = acceptabilityBandDTO.BoundaryValues.ToArray();

				AssertEquals("Precondition - should be 6 boundary values in the DTO.", 6, dtoBoundaryValues.Length);
				var acceptabilityBandBoundaryValues = new AcceptabilityBandBoundaryValues(
					cautionMin: dtoBoundaryValues[0],
					goodMin: dtoBoundaryValues[1],
					excellentMin: dtoBoundaryValues[2],
					excellentMax: dtoBoundaryValues[3],
					goodMax: dtoBoundaryValues[4],
					cautionMax: dtoBoundaryValues[5]
				);

				CombineAssertions("The section acceptability band values should be the same in the DTO.", () =>
				{
					AssertEquals("PK", sectionAcceptabilityBand.AcceptabilityBandPK.ToGuid(), acceptabilityBandDTO.PK);
					AssertEquals("Display Name", sectionAcceptabilityBand.DisplayName, acceptabilityBandDTO.DisplayName);
					AssertEquals("Display Units", sectionAcceptabilityBand.DisplayUnits, acceptabilityBandDTO.DisplayUnits);
					AssertEquals("Boundary Values", sectionAcceptabilityBand.BoundaryValues, acceptabilityBandBoundaryValues);
				});
			}
		}

		void AssertLayoutValues(BMBoardSection section, SectionLayoutDTO sectionLayoutDTO)
		{
			AssertEquals(section.Row, sectionLayoutDTO.Row);
			AssertEquals(section.Column, sectionLayoutDTO.Column);
			AssertEquals(section.RowSpan, sectionLayoutDTO.RowSpan);
			AssertEquals(section.ColSpan, sectionLayoutDTO.ColSpan);
			AssertEquals(section.RowHeightPercent, sectionLayoutDTO.RowHeightPercent);
			AssertEquals(section.ColWidthPercent, sectionLayoutDTO.ColWidthPercent);
		}

		void AssertComponentsValues(BMBoardSection section, BMComponentSectionConfiguration sectionConfig, IList<ComponentDTO> componentsDTO)
		{
			var mainComponentDTO = componentsDTO.FirstOrDefault();

			AssertNotNull(mainComponentDTO);
			AssertEquals(section.Component.PK.ToGuid(), mainComponentDTO.PK);
			AssertEquals(section.Component.FC_Name, mainComponentDTO.Name);

			for (int i = 0; i < sectionConfig.AdditionalComponents.Count; i++)
			{
				var component = sectionConfig.AdditionalComponents[i].Component;
				var additionalComponentDTO = componentsDTO[i + 1];
				AssertEquals(component.PK.ToGuid(), additionalComponentDTO.PK);
				AssertEquals(component.FC_Name, additionalComponentDTO.Name);
			}
		}

		void AssertFlowDirectionValue(string flowDirection, FlowDirection flowDirectionDTO)
		{
			FlowDirection expected;

			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Left:
					expected = FlowDirection.Left;
					break;
				case FlowDirectionList.Codes.Right:
					expected = flowDirectionDTO;
					break;
				case FlowDirectionList.Codes.Up:
					expected = flowDirectionDTO;
					break;
				case FlowDirectionList.Codes.Down:
				default:
					expected = flowDirectionDTO;
					break;
			}

			AssertEquals(expected, flowDirectionDTO);
		}

		void AssertLastCellValue(string lastCell, LastCellPosition? lastCellPositionDTO)
		{
			LastCellPosition? expected = null;

			switch (lastCell)
			{
				case LastCellList.Codes.Bottom:
					expected = LastCellPosition.Bottom;
					break;
				case LastCellList.Codes.Left:
					expected = LastCellPosition.Left;
					break;
				case LastCellList.Codes.Right:
					expected = LastCellPosition.Right;
					break;
				case LastCellList.Codes.Top:
					expected = LastCellPosition.Top;
					break;
				default:
					expected = null;
					break;
			}

			AssertEquals(expected, lastCellPositionDTO);
		}

		void AssertBufferZoneColorsValues(Dictionary<int, string> bufferZonesColorsDTOs)
		{
			AssertEquals(4, bufferZonesColorsDTOs.Count);

			AssertEquals(Color.Blue.ToHex(), bufferZonesColorsDTOs[0]);
			AssertEquals(Color.Red.ToHex(), bufferZonesColorsDTOs[1]);
			AssertEquals(Color.Yellow.ToHex(), bufferZonesColorsDTOs[2]);
			AssertNull(bufferZonesColorsDTOs[3]);
		}

		#region Channels

		void AssertChannelsValues(BMBoardSectionChannel[] sectionChannels, bool overrideChannels, string channelBy, SectionChannelsConfigurationDTO sectionChannelsConfigurationDTO)
		{
			var channelsDTO = sectionChannelsConfigurationDTO.Channels.ToList();

			AssertChannelTypeValue(channelBy, overrideChannels, sectionChannelsConfigurationDTO.ChannelBy);

			if (channelBy == ChannelTypeList.Codes.NotChanneled)
			{
				var notChanneledChannel = channelsDTO.Single();

				AssertEquals(Guid.Empty, notChanneledChannel.EntityPK);
				AssertChannelTypeValue(ChannelTypeList.Codes.NotChanneled, false, notChanneledChannel.Type);
				AssertEquals(string.Empty, notChanneledChannel.Name);

				return;
			}

			var entityChannelDTO = channelsDTO
				.Where(c => c.Type != ChannelType.Time && c.Type != ChannelType.ReleaseScheduler)
				.ToArray();

			AssertEquals(sectionChannels.Length, entityChannelDTO.Length);

			foreach (var sectionChannel in sectionChannels)
			{
				var channelDTO = entityChannelDTO.First(c => c.EntityPK == sectionChannel.EntityPK);
				var parentID = sectionChannel.MSC_ParentID.IsEmpty ? Guid.Empty : sectionChannel.MSC_ParentID.ToGuid();

				var bizo = sectionChannel.GetChannelBusinessObject(Factory);
				var staff = bizo as GlbStaff;

				if (staff != null)
				{
					AssertEquals(staff.GS_Code, channelDTO.Code);
				}

				AssertEquals(parentID, channelDTO.EntityPK);
				AssertChannelTypeValue(sectionChannel.MSC_ChannelType, false, channelDTO.Type);
				AssertChannelName(sectionChannel, channelDTO.Name);
			}
		}

		void AssertChannelTypeValue(string channelType, bool overrideChannels, ChannelType channelTypeDTO)
		{
			var expected = ChannelType.NotChanneled;

			if (overrideChannels)
			{
				expected = ChannelType.Override;
			}
			else
			{
				switch (channelType)
				{
					case ChannelTypeList.Codes.Capability:
						expected = ChannelType.Capability;
						break;
					case ChannelTypeList.Codes.Group:
						expected = ChannelType.Group;
						break;
					case ChannelTypeList.Codes.Resource:
						expected = ChannelType.Resource;
						break;
					case ChannelTypeList.Codes.Tag:
						expected = ChannelType.Tag;
						break;
					case ChannelTypeList.Codes.ReleaseSchedulerChannels:
						expected = ChannelType.ReleaseScheduler;
						break;
					case BMConstants.ChannelByTimeCode:
						expected = ChannelType.Time;
						break;
					case ChannelTypeList.Codes.NotChanneled:
					default:
						expected = ChannelType.NotChanneled;
						break;
				}
			}

			AssertEquals(expected, channelTypeDTO);
		}

		void AssertChannelName(BMBoardSectionChannel sectionChannel, string name)
		{
			string expected = null;

			if (sectionChannel.MSC_ChannelType == ChannelTypeList.Codes.NotChanneled || sectionChannel.MSC_ParentID.IsEmpty)
			{
				expected = BMConstants.UnchanneledDisplayName;
			}
			else
			{
				var parentEntity = Factory.Load(sectionChannel.MSC_ParentTableCode, sectionChannel.MSC_ParentID);

				switch (parentEntity)
				{
					case GlbStaff staff:
						expected = staff.GS_FullName;
						break;
					case GlbGroup group:
						expected = group.GG_Desc;
						break;
					case GlbCapability capability:
						expected = capability.G4_Description;
						break;
					case TagMagnitude tag:
						expected = tag.DisplayText;
						break;
				}
			}

			AssertEquals(expected, name);
		}

		#endregion

		void AssertCellsValues(BMBoardSection section, BMComponentSectionConfiguration sectionConfig, IEnumerable<IChannel> primaryChannels, IEnumerable<IChannel> secondaryChannels, IEnumerable<CellDTO> cellsDTO)
		{
			CellDTO[] expectedValues = null;
			CellDTO[] cells = cellsDTO?.ToArray();

			if (sectionConfig.IsBuffer && !sectionConfig.IsReleaseScheduler)
			{
				expectedValues = new CellDTO[]
				{
					new CellDTO { Name = "100 %+", IndexLowerBoundary = 100.0m, Zone = 0 },
					new CellDTO { Name = "100 %", IndexLowerBoundary = 88.9m, Zone = 1 },
					new CellDTO { Name = "88.9 %", IndexLowerBoundary = 77.8m, Zone = 1 },
					new CellDTO { Name = "77.8 %", IndexLowerBoundary = 66.7m, Zone = 1 },
					new CellDTO { Name = "66.7 %", IndexLowerBoundary = 55.6m, Zone = 2 },
					new CellDTO { Name = "55.6 %", IndexLowerBoundary = 44.4m, Zone = 2 },
					new CellDTO { Name = "44.4 %", IndexLowerBoundary = 33.3m, Zone = 2 },
					new CellDTO { Name = "33.3 %", IndexLowerBoundary = 22.2m, Zone = 3 },
					new CellDTO { Name = "22.2 %", IndexLowerBoundary = 11.1m, Zone = 3 },
					new CellDTO { Name = "11.1 %", IndexLowerBoundary = 0.0m, Zone = 3 }
				};
			}
			if (sectionConfig.IsBucket)
			{
				expectedValues = new CellDTO[]
				{
					new CellDTO { Name = "30m+", IndexLowerBoundary = 1m, Zone = null },
					new CellDTO { Name = "30m", IndexLowerBoundary = 0m, Zone = null },
					new CellDTO { Name = "-30m", IndexLowerBoundary = -1m, Zone = null },
					new CellDTO { Name = "-60m", IndexLowerBoundary = -2m, Zone = null }
				};
			}

			if (expectedValues == null)
			{
				AssertNull(cellsDTO);
				return;
			}

			CombineAssertions(() =>
			{
				AssertEquals(expectedValues.Length, cellsDTO.Count());

				for (int i = 0; i < expectedValues.Length; i++)
				{
					var expectedValue = expectedValues[i];
					var cell = cells[i];

					AssertCellValue(expectedValue, cell);
				}
			});
		}

		void AssertCellValue(CellDTO expectedValue, CellDTO cell)
		{
			AssertEquals(expectedValue.Name, cell.Name);
			AssertEquals(expectedValue.IndexLowerBoundary, cell.IndexLowerBoundary);
			AssertEquals(expectedValue.Zone, cell.Zone);
		}

		#endregion

		#region Helpers

		string[] GetCodesValues<T>()
		{
			return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
			.Select(constant => constant.GetRawConstantValue().ToString())
			.OrderBy(value => value)
			.ToArray();
		}

		#endregion

		#region Enum Values

		public void TestSectionTypeValues()
		{
			var supportedSectionsType = SectionDescriptorProvider.GetSupportedSectionTypes()
				.OrderBy(type => type)
				.ToArray();

			AssertArrayEqualsByElements(supportedSectionsType, BMSTestHelper.GetEnumMemberAttributeValues<SectionType>().Except(new[] { "UNK" }).ToArray());
		}

		public void TestComponentTypeValues()
		{
			AssertArrayEqualsByElements(new string[] { "BUC", "BUF" }, BMSTestHelper.GetEnumMemberAttributeValues<ComponentType>().ToArray());
		}

		public void TestLastCellPositionValues()
		{
			AssertArrayEqualsByElements(GetCodesValues<LastCellList.Codes>(), BMSTestHelper.GetEnumMemberAttributeValues<LastCellPosition>().ToArray());
		}

		public void TestFlowDirectionValues()
		{
			AssertArrayEqualsByElements(GetCodesValues<FlowDirectionList.Codes>(), BMSTestHelper.GetEnumMemberAttributeValues<FlowDirection>().ToArray());
		}

		public void TestChannelTypeValues()
		{
			var channelTypes = new string[] { BMConstants.ChannelByTimeCode, "OVE" }
			.Concat(GetCodesValues<ChannelTypeList.Codes>())
			.OrderBy(type => type)
			.ToArray();

			AssertArrayEqualsByElements(channelTypes, BMSTestHelper.GetEnumMemberAttributeValues<ChannelType>().ToArray());
		}

		#endregion

		#region SetUp

		protected BoardService Service;

		protected override void SetUp()
		{
			base.SetUp();

			Globals.IsWebService = true;

			Service = new BoardService();
			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Globals.IsWebService = false;
		}

		#endregion
	}
}
