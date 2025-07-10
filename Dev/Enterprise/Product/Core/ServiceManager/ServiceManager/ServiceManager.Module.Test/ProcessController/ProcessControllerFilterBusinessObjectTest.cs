using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(ProcessControllerFilterBusinessObject))]
	class ProcessControllerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessControllerFilterBusinessObject();
		}

		public void TestFiltersMatch_ShouldBeDisabledForSupportAuditUserFilters()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessController))
			{
				var filterBizo = module.FilterBusinessObject;
				var filtersThatActuallySupportFiltersMatch = filterBizo.ModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.AllowedComparisonOperators.Contains(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));

				AssertContainsExactElementsInAnyOrder("Filters match doesn't work in the Process Controller module because of a bunch of complicated things that happen when a search is performed, so this option should be disabled until someone fixes that."
													+ "If you remove DisableFiltersMatchForAuditFilters and run the ZModuleBasherTest.TestFiltersMatchOperator_ShouldGenerateValidQueries for this module, you will see the errors that would need to be fixed.",
					Array.Empty<string>(), filtersThatActuallySupportFiltersMatch.Select(x => x.Description));

				var creatingUserFilter = filterBizo.ModuleFilters[FilterDescriptions.CreatingUser] as ModuleNkFilter;
				var lastEditUserFilter = filterBizo.ModuleFilters[FilterDescriptions.LastEditUser] as ModuleNkFilter;

				AssertEquals("Creating User does not supports Filters Match operator", false, creatingUserFilter.SupportsFiltersMatchComparisonOperator);
				AssertEquals("Last Edit User does not supports Filters Match operator", false, lastEditUserFilter.SupportsFiltersMatchComparisonOperator);
			}
		}

		#endregion
	}
}

