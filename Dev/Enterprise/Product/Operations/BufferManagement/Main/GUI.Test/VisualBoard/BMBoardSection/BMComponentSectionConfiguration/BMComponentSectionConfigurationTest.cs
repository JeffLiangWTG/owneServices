using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMComponentSectionConfiguration))]
	class BMComponentSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		#region Clone

		public void TestCopyConfigurationPropertiesToNewSection()
		{
			var pair = BMSTestHelper.CreateSystemAndBuffer(Factory);
			var board = pair.Item1.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(pair.Item1);
			var section = BMSTestHelper.CreateBoardSection(pair.Item2, board);
			section.MS_SectionType = BMConstants.ComponentSectionType;
			var additionalComponent = Factory.NewWithValidTestData<BMBoardSectionAdditionalComponent>();
			additionalComponent.BSA_MS_Section = section.PK;
			additionalComponent.BSA_FC_Component = bucket.PK;
			((BMComponentSectionConfiguration)section.Configuration).AdditionalComponents.Add(additionalComponent);

			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, "Completion Statement", "Tra la la");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, "Description", "Tra la la");

			var newSection = BMSTestHelper.CreateBoardSection(pair.Item2, board);
			newSection.MS_SectionType = BMConstants.ComponentSectionType;

			section.SectionConfiguration.CopyConfigurationPropertiesToNewSection(newSection);

			AssertEquals(1, newSection.SectionConfiguration.AdditionalComponents.Count);

			var originalWorkflowQuery = RelatedModuleFiltersHelper.GetFilterQuery(section.WorkflowFilter);
			var originalTaskQuery = section.SectionConfiguration.GetQuery(ModuleIDs.ProcessTasks, section.SectionConfiguration.TaskFilter).LiteralTextSqlFormatted;

			var newWorkflowQuery = RelatedModuleFiltersHelper.GetFilterQuery(newSection.WorkflowFilter);
			var newTaskQuery = newSection.SectionConfiguration.GetQuery(ModuleIDs.ProcessTasks, section.SectionConfiguration.TaskFilter).LiteralTextSqlFormatted;

			AssertEquals(originalWorkflowQuery, newWorkflowQuery);
			AssertEquals(originalTaskQuery, newTaskQuery);
		}

		#endregion

		#region Filters
		public void TestFilters_ShouldBePersisted()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var workflowFilterPk = section.WorkflowFilter.PK;
			var taskFilterPk = section.TaskFilter.PK;

			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, "Completion Statement", "These strips must be added...");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, "Description", "Or the StmModuleFilters will be deleted.");

			Factory.Save();

			var loadedSection = new BusinessObjectFactory().Load<BMBoardSection>(section.PK);
			AssertEquals(workflowFilterPk, loadedSection.WorkflowFilter.PK);
			AssertEquals(taskFilterPk, loadedSection.TaskFilter.PK);
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
