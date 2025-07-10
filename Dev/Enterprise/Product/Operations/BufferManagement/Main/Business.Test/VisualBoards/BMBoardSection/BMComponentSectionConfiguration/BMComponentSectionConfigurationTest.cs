using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentSectionConfiguration))]
	class BMComponentSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		#region Filters

		public void TestSectionWithFilterDoesNotCauseDuplicateKeyConstraintFailure()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			AssertNotNull(section.WorkflowFilter);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var filter = factory2.Load<StmModuleFilter>(section.WorkflowFilter.PK);

			AssertNotNull(filter);

			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var loadedSection = factory3.Load<BMBoardSection>(section.PK);

			AssertNotNull(loadedSection.WorkflowFilter);
			AssertEquals("WFL", loadedSection.WorkflowFilter.S9_FilterName);

			factory3.Save();

			var factory4 = new BusinessObjectFactory();
			var reloadedLoadedSection = factory4.Load<BMBoardSection>(section.PK);

			AssertEquals("Still have not set changes on filter", false, reloadedLoadedSection.WorkflowFilter.HasChanges);

			reloadedLoadedSection.SectionConfiguration.Delete();

			factory4.Save();

			var factory5 = new BusinessObjectFactory();
			var loadedWithoutSectionConfigurationSection = factory5.Load<BMBoardSection>(section.PK);
			loadedWithoutSectionConfigurationSection.WorkflowFilter.HasChanges = true;

			AssertNotNull(loadedWithoutSectionConfigurationSection.WorkflowFilter);

			AssertNoExceptionThrown(() => { factory5.Save(); });

			var factory6 = new BusinessObjectFactory();
			var finalSection = factory6.Load<BMBoardSection>(section.PK);

			filter = factory6.Load<StmModuleFilter>(finalSection.WorkflowFilter.PK);

			AssertNotNull(filter);
		}

		public void TestSectionCreatesNewFilterUponCreation()
		{
			var pair = BMSTestHelper.CreateSystemAndBuffer(Factory);
			var board = pair.Item1.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(pair.Item1);
			var section = BMSTestHelper.CreateBoardSection(pair.Item2, board);
			section.MS_SectionType = BMConstants.ComponentSectionType;

			AssertEquals(ModuleIDs.BMFilterRule.Name, section.WorkflowFilter.S9_ModuleID);
		}

		public void TestFilterReloadsFromDatabase()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			Factory.ChildFactories.Add(factory1);
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			var configuration = section.SectionConfiguration;

			AssertNotNull(configuration.WorkflowFilter);

			var blob1 = ZBlob.FromAscii("filter");

			var filter = configuration.WorkflowFilter;

			filter.S9_FilterData = blob1;
			filter.S9_IsPublished = true;

			factory1.Save();

			filter = Factory.Load<StmModuleFilter>(configuration.WorkflowFilter.PK);

			const string insertSql = @"UPDATE dbo.StmModuleFilter SET S9_FilterData = @S9_FilterData WHERE S9_PK = @S9_PK";

			using (var command = Db.Connection.Command(insertSql))
			{
				command.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, filter.PK.ToGuid());
				command.AddParameter("@S9_FilterData", SqlDbType.VarBinary, Compressor.Compress(System.Text.Encoding.ASCII.GetBytes("changed data")));
				command.ExecuteNonQuery();
			}

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			Factory.ChildFactories.Add(factory2);
			var system2 = factory2.Load<BMSystem>(system.PK);
			var board2 = system2.Boards[0];
			var section2 = board2.Sections[0];

			var configuration3 = section2.Configuration as BMComponentSectionConfiguration;

			AssertNotNull(configuration3);

			var filter2 = configuration3.WorkflowFilter;
			AssertNotNull(filter2);
			AssertEquals(ZBlob.FromAscii("changed data"), filter2.S9_FilterData);
		}

		public void TestFilterConcurrencyPolicyIsStrict()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			var configuration = section.SectionConfiguration;

			AssertNotNull(configuration.WorkflowFilter);

			AssertEquals(ConcurrencyPolicy.Strict, configuration.WorkflowFilter.S9_FilterDataInfo.ConcurrencyPolicy);
		}

		public void TestFilterForNewSection_DbHits()
		{
			Factory.SuspendValidation(); // Stop validation hitting the db. We're just testing StmModuleFilter stuff.

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionConfig = config.BufferSection.SectionConfiguration;

			Factory.ResumeValidation();

			AssertNotNull(sectionConfig.WorkflowFilter);
			AssertNotNull(sectionConfig.TaskFilter);

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
			}, Factory);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSectionConfig = newFactory.Load<BMBoardSection>(sectionConfig.Section.PK).SectionConfiguration;

			AssertNotNull(loadedSectionConfig.WorkflowFilter);
			AssertNotNull(loadedSectionConfig.TaskFilter);

			AssertDbHits(new Dictionary<string, int>
			{
				{ BMBoardSectionSchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 2 },
			}, newFactory);
		}

		#endregion

		#region BO Properties

		public void TestSetDefaultValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;
			section.MS_FC_Component = bucket.PK;

			var sectionConfiguration = section.SectionConfiguration;

			CombineAssertions("Default values for section configuration.", () =>
			{
				AssertEquals("FlowDirection", FlowDirectionList.Codes.Down, sectionConfiguration.FlowDirection);
				AssertEquals("Subsections", 1, sectionConfiguration.Subsections);
				AssertEquals("CellsPerSubsection", 1, sectionConfiguration.CellsPerSubsection);
				AssertEquals("ChannelBy", ChannelTypeList.Codes.NotChanneled, sectionConfiguration.ChannelBy);
				AssertEquals("ChannelSecondaryBy", BMConstants.ChannelByTimeCode, sectionConfiguration.ChannelSecondaryBy);
				AssertEquals("TimeProgressionMode", TimeProgressionModeList.Codes.Age, sectionConfiguration.TimeProgressionMode);
				AssertEquals("TimeField", TimeProgressionFieldList.Codes.TransferTime, sectionConfiguration.TimeField);
				AssertEquals("ShowZones", false, sectionConfiguration.ShowZones);
				AssertEquals("CardType", CardTypeList.Codes.Task, sectionConfiguration.CardType);
				AssertEquals("CountdownTargetBorderColor", Color.Red.Name, sectionConfiguration.CountdownTargetBorderColor);
				AssertEquals("CountdownTargetBorderStyle", new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true).ToString(), sectionConfiguration.CountdownTargetBorderStyle);
				AssertEquals("CountdownStartableBorderColor", Color.Blue.Name, sectionConfiguration.CountdownStartableBorderColor);
				AssertEquals("CountdownStartableBorderStyle", new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true).ToString(), sectionConfiguration.CountdownStartableBorderStyle);
				AssertEquals("EnableShowCurrentItemsFilterByDefault", false, sectionConfiguration.EnableShowCurrentItemsFilterByDefault);
				AssertEquals("ShowChildComponentZones", false, sectionConfiguration.ShowChildComponentZones);
				AssertEquals("PanelLayoutStyle", PanelLayoutTypeList.Codes.Stacked, sectionConfiguration.PanelLayoutStyle);
			});
		}

		public void TestSetDefaultValues_Buffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;
			section.MS_FC_Component = buffer.PK;

			var sectionConfiguration = section.SectionConfiguration;

			CombineAssertions("Default values for buffer section configuration", () =>
			{
				AssertEquals("CellsPerSubsection", 10, sectionConfiguration.CellsPerSubsection);
				AssertEquals("FadeBackgroundPercentage", 80, sectionConfiguration.FadeBackgroundAtPercentage);
				AssertEquals("FlowDirection", FlowDirectionList.Codes.Up, sectionConfiguration.FlowDirection);
				AssertEquals("PanelLayoutStyle", PanelLayoutTypeList.Codes.Staggered, sectionConfiguration.PanelLayoutStyle);
				AssertEquals("ShowZones", true, sectionConfiguration.ShowZones);
				AssertEquals("TimeProgressionMode", TimeProgressionModeList.Codes.Age, sectionConfiguration.TimeProgressionMode);
			});
		}

		public void TestSetDefaultValues_ChangeBufferToBuffer_DoesNotOverrideConfiguration()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;
			section.MS_FC_Component = buffer1.PK;

			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.CellsPerSubsection = 11;
			sectionConfiguration.FadeBackgroundAtPercentage = 90;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			sectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			section.MS_FC_Component = buffer2.PK;

			CombineAssertions("Changing component from a buffer to another buffer should not override configuration.", () =>
			{
				AssertEquals("CellsPerSubsection", 11, sectionConfiguration.CellsPerSubsection);
				AssertEquals("FadeBackgroundPercentage", 90, sectionConfiguration.FadeBackgroundAtPercentage);
				AssertEquals("FlowDirection", FlowDirectionList.Codes.Down, sectionConfiguration.FlowDirection);
				AssertEquals("PanelLayoutStyle", PanelLayoutTypeList.Codes.Stacked, sectionConfiguration.PanelLayoutStyle);
				AssertEquals("ShowZones", false, sectionConfiguration.ShowZones);
				AssertEquals("TimeProgressionMode", TimeProgressionModeList.Codes.Due, sectionConfiguration.TimeProgressionMode);
			});
		}

		public void TestSetDefaultValues_ChangeBucketToBuffer_OverridesConfiguration()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;
			section.MS_FC_Component = bucket.PK;

			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.CellsPerSubsection = 11;
			sectionConfiguration.FadeBackgroundAtPercentage = 90;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			sectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;

			section.MS_FC_Component = buffer.PK;

			CombineAssertions("Changing bucket component to a buffer should override configuration.", () =>
			{
				AssertEquals("CellsPerSubsection", 10, sectionConfiguration.CellsPerSubsection);
				AssertEquals("FadeBackgroundPercentage", 80, sectionConfiguration.FadeBackgroundAtPercentage);
				AssertEquals("FlowDirection", FlowDirectionList.Codes.Up, sectionConfiguration.FlowDirection);
				AssertEquals("PanelLayoutStyle", PanelLayoutTypeList.Codes.Staggered, sectionConfiguration.PanelLayoutStyle);
				AssertEquals("ShowZones", true, sectionConfiguration.ShowZones);
				AssertEquals("TimeProgressionMode", TimeProgressionModeList.Codes.Age, sectionConfiguration.TimeProgressionMode);
			});
		}

		public void TestSetDefaultValues_ChangeToBufferToBucket_OverridesConfiguration()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ComponentSectionType;
			section.MS_FC_Component = buffer.PK;

			var sectionConfiguration = section.SectionConfiguration;

			sectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			sectionConfiguration.ShowZones = true;

			section.MS_FC_Component = bucket.PK;

			CombineAssertions("Changing buffer component to a bucket should override configuration.", () =>
			{
				AssertEquals("PanelLayoutStyle", PanelLayoutTypeList.Codes.Stacked, sectionConfiguration.PanelLayoutStyle);
				AssertEquals("ShowZones", false, sectionConfiguration.ShowZones);
			});
		}

		public void TestMinimumBufferTimePerCell()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 12);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			AssertEquals("TimePerCell should be readonly, since it is a calculated property when used in conjunction with a buffer section", true, section.SectionConfiguration.TimePerCellInfo.ReadOnly);

			section.SectionConfiguration.CellsPerSubsection = 30;
			AssertEquals("TimePerCell should not be less than one minute", 1.0d, section.SectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan());

			section.SectionConfiguration.CellsPerSubsection = 6;
			AssertEquals(4.0d, section.SectionConfiguration.TimePerCell.GetMinutesFromDateTimeSpan());
		}

		#endregion

		#region SectionName

		public void TestSectionName()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			var config = (BMComponentSectionConfiguration)section.Configuration;
			AssertEquals("buffer", config.SectionName);

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "I will override everything!";
			AssertEquals("I will override everything!", config.SectionName);

			config.SectionNameIsOverridden = false;
			AssertEquals("buffer", config.SectionName);
		}

		#endregion

		#region Channels

		public void TestOrderedPrimaryAxisChannels_WhenSortingAlphabetically_ShouldIgnoreMSC_Sequence()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AD", "Adam");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BAD", "Badam");
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionConfig = config.BufferSection.SectionConfiguration;

			sectionConfig.SortPrimaryChannels = true;
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", staff2.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", staff1.PK);

			AssertEquals("Precondition: Sequence numbers are set such that the channels are not in alphabetical order if sorting by sequence.", 1, channel1.MSC_Sequence);
			AssertEquals("Precondition: Sequence numbers are set such that the channels are not in alphabetical order if sorting by sequence.", 2, channel2.MSC_Sequence);

			var orderedChannels = sectionConfig.OrderedPrimaryAxisChannels;

			AssertSequencesEqual("MSC_Sequence should be ignored when SortPrimaryChannels is true. SAD!", new[] { "Adam", "Badam" }, orderedChannels.Select(x => x.BizoDescription.ToString()));
		}

		public void TestOrderedSecondaryAxisChannels_WhenSortingAlphabetically_ShouldIgnoreMSC_Sequence()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AD", "Adam");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BAD", "Badam");
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionConfig = config.BufferSection.SectionConfiguration;

			sectionConfig.SortSecondaryChannels = true;
			var channel1 = BMSTestHelper.CreateSecondaryChannelForSection(config.BufferSection, "RES", staff2.PK);
			var channel2 = BMSTestHelper.CreateSecondaryChannelForSection(config.BufferSection, "RES", staff1.PK);

			AssertEquals("Precondition: Sequence numbers are set such that the channels are not in alphabetical order if sorting by sequence.", 1, channel1.MSC_Sequence);
			AssertEquals("Precondition: Sequence numbers are set such that the channels are not in alphabetical order if sorting by sequence.", 2, channel2.MSC_Sequence);

			var orderedChannels = sectionConfig.OrderedSecondaryAxisChannels;

			AssertSequencesEqual("MSC_Sequence should be ignored when SortSecondaryChannels is true. SAD!", new[] { "Adam", "Badam" }, orderedChannels.Select(x => x.BizoDescription.ToString()));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			return new BMComponentSectionConfiguration(section);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "AcceptabilityBands";
				yield return "BufferZone0Color";
				yield return "BufferZone1Color";
				yield return "BufferZone2Color";
				yield return "BufferZone3Color";
				yield return "CellsPerSubsection";
				yield return "ChannelBy";
				yield return "ChannelSecondaryBy";
				yield return "CountdownStartableBorderColor";
				yield return "CountdownStartableBorderStyle";
				yield return "CountdownTargetBorderColor";
				yield return "CountdownTargetBorderStyle";
				yield return "EnableShowCurrentItemsFilterByDefault";
				yield return "FadeBackgroundAtPercentage";
				yield return "FlowDirection";
				yield return "HideCapabilityTasksFromResourceChannels";
				yield return "HideResourceTasksFromCapabilityChannels";
				yield return "IsReleaseScheduler";
				yield return "LastCell";
				yield return "MaxOverdueSlots";
				yield return "OverdueBackgroundColor";
				yield return "OverdueForegroundColor";
				yield return "OverrideChannels";
				yield return "OverrideSecondaryChannels";
				yield return "ReleaseGroupPK";
				yield return "ShowSecondaryUnchanneled";
				yield return "ShowUnchanneled";
				yield return "CardType";
				yield return "ShowWorkInReleaseGroupOnly";
				yield return "Subsections";
				yield return "TimeField";
				yield return "TimePerCell";
				yield return "TimeProgressionMode";
				yield return "ShowZones";
				yield return "PanelLayoutStyle";
				yield return "SortPrimaryChannels";
				yield return "SortSecondaryChannels";
				yield return "SectionNameOverride";
				yield return "SectionNameIsOverridden";
				yield return "ShowChildComponentZones";
			}
		}

		#endregion
	}
}
