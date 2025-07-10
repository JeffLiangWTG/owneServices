using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ModuleGridSectionPanelConfiguration))]
	class ModuleGridSectionPanelConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var board = BMSTestHelper.CreateBoard(system);
			board.MB_IsPublished = false;
			board.MB_GG_ReleaseGroup = ZGuid.Empty;

			var modSection = BMSTestHelper.CreateBoardSection(buffer);
			modSection.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(modSection);
			((ModuleGridSectionConfiguration)modSection.Configuration).PanelConfigurations.Add(panelConfig);

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, panelConfig.ModuleName);
				AssertEquals(1, panelConfig.Sequence);
				AssertEquals(false, panelConfig.SectionNameIsOverridden);
				AssertEquals(string.Empty, panelConfig.SectionNameOverride);
				AssertEquals(ZGuid.Empty, panelConfig.FilterLayout);
				AssertEquals(true, panelConfig.ShowFilters);
				AssertEquals(true, panelConfig.AllowFilterToggle);
				AssertEquals(true, panelConfig.AllowOpenModule);
				AssertEquals(true, panelConfig.AllowFilterEdit);
			});

			panelConfig.Sequence = 11;
			var panelConfigNew = new ModuleGridSectionPanelConfiguration(modSection);
			((ModuleGridSectionConfiguration)modSection.Configuration).PanelConfigurations.Add(panelConfigNew);

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, panelConfigNew.ModuleName);
				AssertEquals(12, panelConfigNew.Sequence);
				AssertEquals(false, panelConfigNew.SectionNameIsOverridden);
				AssertEquals(string.Empty, panelConfigNew.SectionNameOverride);
				AssertEquals(ZGuid.Empty, panelConfigNew.FilterLayout);
				AssertEquals(true, panelConfigNew.ShowFilters);
				AssertEquals(true, panelConfigNew.AllowFilterToggle);
				AssertEquals(true, panelConfigNew.AllowOpenModule);
				AssertEquals(true, panelConfigNew.AllowFilterEdit);
			});
		}

		public void TestPanelName()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;
			config.PanelConfigurations.Add(panelConfig);
			AssertEquals("Job Workflows", panelConfig.PanelName);

			var filter = Factory.New<StmModuleFilter>();
			filter.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter.S9_FilterName = "I will be a panel name!";
			panelConfig.FilterLayout = filter.PK;
			AssertEquals("I will be a panel name!", panelConfig.PanelName);

			panelConfig.SectionNameIsOverridden = true;
			panelConfig.SectionNameOverride = "I will override everything!";
			AssertEquals("I will override everything!", panelConfig.PanelName);

			panelConfig.SectionNameIsOverridden = false;
			AssertEquals("I will be a panel name!", panelConfig.PanelName);
		}

		public void TestFilterLayout_ResetAfterModuleNameChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;
			config.PanelConfigurations.Add(panelConfig);
			AssertEquals(ModuleIDs.ProcessHeader.Description, panelConfig.PanelName);

			var filter = Factory.New<StmModuleFilter>();
			filter.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter.S9_FilterName = "I will be a section name!";
			panelConfig.FilterLayout = filter.PK;
			AssertEquals("I will be a section name!", panelConfig.PanelName);

			panelConfig.ModuleName = ModuleIDs.WorkItem.Name;

			AssertEquals(ZGuid.Empty, panelConfig.FilterLayout);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			return new ModuleGridSectionPanelConfiguration(section);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "ModuleName";
				yield return "Sequence";
				yield return "SectionNameIsOverridden";
				yield return "SectionNameOverride";
				yield return "FilterLayout";
				yield return "ShowFilters";
				yield return "AllowFilterToggle";
				yield return "AllowOpenModule";
				yield return "AllowFilterEdit";
			}
		}

		#endregion
	}
}
