using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ProcessManagement.Module.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedIncidentsOfIncidentFilter))]
	class RelatedIncidentOfIncidentFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedIncidentsOfIncidentFilter, SupportIncident, SupportIncident>
	{
		protected override string DescriptionFilterStripName => "Problem Description";
		protected override ModuleIdentifier RelatedModuleID => ClientModuleRegistration.SupportIncident;
		protected override string RelatedEntityFilterStripName => "Related Incidents";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new SupportIncidentCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject()
		{
			var filter = new SupportIncidentFilterBusinessObject();
			var extraFilter = (ModuleTextFilter)filter["Product"];
			extraFilter.IsActive = true;
			extraFilter.Property = "CW1";
			extraFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			return filter;
		}
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
		protected override RelatedIncidentsOfIncidentFilter GetNewModuleFilter()
		{
			var filter = new RelatedIncidentsOfIncidentFilter("moo", () => CreateRelatedEntityCollection());
			filter.Category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
			return filter;
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(SupportIncident mainBizo)
		{
			return mainBizo.IM_DescriptionInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(SupportIncident relatedBizo)
		{
			return relatedBizo.IM_DescriptionInfo;
		}

		protected override void SetUp()
		{
			base.SetUp();
			mainBizo1.IM_Product = "CW1";
			mainBizo2.IM_Product = "CW1";
		}
		protected override void AssertResults(params SupportIncident[] expectedResults)
		{
			var expectedDescriptions = expectedResults.Select(b => GetMainBizoDescriptionProperty(b).Value.ToString());
			if (mainBizo3 != null)
			{
				mainBizo3.IM_Product = "CW1";
				Factory.Save();
			}
			else
			{
				var extraFilter = (ModuleTextFilter)mainFilterBizo["Product"];
				extraFilter.IsActive = false;
			}
			var results = Factory.Load<SupportIncident>(mainFilterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(expectedDescriptions, results.Select(b => GetMainBizoDescriptionProperty(b).Value.ToString()));
		}
	}
}
