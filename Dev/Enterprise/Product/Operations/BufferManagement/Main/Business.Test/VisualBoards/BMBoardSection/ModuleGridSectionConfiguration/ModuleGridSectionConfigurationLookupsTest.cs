using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	class ModuleGridSectionConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDisplaySelectedPanels()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig1 = new ModuleGridSectionPanelConfiguration(section);
			panelConfig1.Sequence = 3;
			panelConfig1.ModuleName = ModuleIDs.WorkItem.Name;

			var panelConfig2 = new ModuleGridSectionPanelConfiguration(section);
			panelConfig2.Sequence = 2;
			panelConfig2.ModuleName = ModuleIDs.ProcessHeader.Name;
			panelConfig2.SectionNameIsOverridden = true;
			panelConfig2.SectionNameOverride = "Panel2";

			var panelConfig3 = new ModuleGridSectionPanelConfiguration(section);
			panelConfig3.Sequence = 1;
			panelConfig3.ModuleName = ModuleIDs.ProcessHeader.Name;

			config.PanelConfigurations.Add(panelConfig1);
			config.PanelConfigurations.Add(panelConfig2);
			config.PanelConfigurations.Add(panelConfig3);

			var lookups = new ModuleGridSectionConfigurationLookups(config);

			var panels = lookups.AllPanels;

			CombineAssertions(() =>
			{
				AssertEquals(3, panels.Count);
				AssertEquals("1", panels[0].Code);
				AssertEquals(ModuleIDs.ProcessHeader.Description, panels[0].Description);
				AssertEquals("2", panels[1].Code);
				AssertEquals("Panel2", panels[1].Description);
				AssertEquals("3", panels[2].Code);
				AssertEquals(ModuleIDs.WorkItem.Description, panels[2].Description);
			});
		}
	}
}
