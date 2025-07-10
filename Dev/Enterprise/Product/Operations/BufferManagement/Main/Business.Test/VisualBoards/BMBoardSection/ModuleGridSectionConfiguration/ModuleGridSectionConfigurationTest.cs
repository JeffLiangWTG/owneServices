using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ModuleGridSectionConfiguration))]
	class ModuleGridSectionConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCopyConfigurationPropertiesToNewSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;
			config.PanelConfigurations.Add(panelConfig);

			Factory.Save();

			var clonedSection = (BMBoardSection)section.Clone();

			AssertEquals(BMConstants.ModuleGridSectionType, clonedSection.MS_SectionType);
			AssertEquals(clonedSection, ((ModuleGridSectionConfiguration)clonedSection.Configuration).Section);
			AssertEquals(ModuleIDs.ProcessHeader.Name, (((ModuleGridSectionConfiguration)clonedSection.Configuration).PanelConfigurations.First() as ModuleGridSectionPanelConfiguration).ModuleName);
		}

		public void TestSectionName()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var config = (ModuleGridSectionConfiguration)section.Configuration;

			AssertEquals(string.Empty, config.SectionName);

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "I will override everything!";
			AssertEquals("I will override everything!", config.SectionName);

			config.SectionNameIsOverridden = false;
			AssertEquals(string.Empty, config.SectionName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var section = Factory.NewWithValidTestData<BMBoardSection>();
			return new ModuleGridSectionConfiguration(section);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "SectionNameOverride";
				yield return "SectionNameIsOverridden";
				yield return "PanelConfigurations";
			}
		}

		#endregion
	}
}
