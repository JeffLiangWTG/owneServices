using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class CoreFunctionalityTest : NUnit.Framework.TestCase
	{
		public void TestGetActiveFiltersByDescription()
		{
			var filterBizO = new TestFilterBusinessObject();
			foreach (var filter in filterBizO.ModuleFilters)
			{
				filter.IsActive = true;
			}

			AssertEquals(1, filterBizO.GetActiveFiltersByDescription("new filter").Count());
			AssertEquals(0, filterBizO.GetActiveFiltersByDescription("new").Count());
		}

		public void TestGetActiveFiltersByDescriptionDoNotCauseStackOverflow()
		{
			TestFilterBusinessObject filterBizO = new TestFilterBusinessObject();
			AssertNotNull("Should not cause stack overflow exception here", filterBizO.ActiveModuleFilters);
		}

		class TestFilterBusinessObject : IncidentMainFilterBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				ModuleFilterCollection result = base.GetModuleFiltersCore();
				result.AddTextFilter("new filter", IncidentMainSchema.IM_Status, FilterList);
				return result;
			}

			CodeDescriptionPairList FilterList
			{
				get
				{
					GetActiveFiltersByDescription("new filter").GetEnumerator().MoveNext();
					return new CodeDescriptionPairList();
				}
			}

			protected override string IncidentType
			{
				get
				{
					return "";
				}
			}

			protected override IncidentMainLookups GetNewLookups()
			{
				return null;
			}
		}
	}
}
