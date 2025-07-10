using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMControlCustomisationFilterBusinessObject))]
	class BMControlCustomisationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMControlCustomisationFilterBusinessObject();
		}

		public void TestIndexSearchFiltersOfBMControlCustomisation()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.BMControlCustomisation))
			using (var mocker = new GlowIndexQueryEngineMock(
				mock =>
				{
					_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection());
					_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IBMControlCustomisation" });
				}))
			{
				var controlTypeFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["ControlType"];
				AssertNotNull(controlTypeFilter);
				AssertEquals(FilterCategories.TextSearch, controlTypeFilter.Category);
				AssertEquals("Control Type", controlTypeFilter.MultilingualDescription.ToString());

				var jobTypeFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["JobType"];
				AssertNotNull(jobTypeFilter);
				AssertEquals(FilterCategories.TextSearch, jobTypeFilter.Category);
				AssertEquals("Job Type", jobTypeFilter.MultilingualDescription.ToString());

				var nameFilter = (IndexSearchModuleTextFilter)module.FilterBusinessObject["Name"];
				AssertNotNull(nameFilter);
				AssertEquals(FilterCategories.TextSearch, nameFilter.Category);
				AssertEquals("Name", nameFilter.MultilingualDescription.ToString());

				var systemWideFilter = (IndexSearchModuleFlagsFilter)module.FilterBusinessObject["SystemWide"];
				AssertNotNull(systemWideFilter);
				AssertEquals(FilterCategories.StatusAndFlags, systemWideFilter.Category);
				AssertEquals("System Wide", systemWideFilter.MultilingualDescription.ToString());
			}
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("ControlType", "Control Type");
			var field2 = SearchField.Create("JobType", "Job Type");
			var field3 = SearchField.Create("SystemWide", "System Wide", typeof(bool));
			var field4 = SearchField.Create("Name", "Name");
			var ret = new SearchFieldCollection("IBMControlCustomisation", new SearchField[] { field1, field2, field3, field4 });
			return ret;
		}
	}
}
