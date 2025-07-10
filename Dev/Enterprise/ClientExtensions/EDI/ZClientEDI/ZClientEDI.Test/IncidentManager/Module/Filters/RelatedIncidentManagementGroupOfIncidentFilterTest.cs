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
	[TestedType(typeof(RelatedIncidentManagementGroupOfIncidentFilter))]

	class RelatedIncidentManagementGroupOfIncidentFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedIncidentManagementGroupOfIncidentFilter, SupportIncident, IncidentManagementGroup>
	{
		protected override string DescriptionFilterStripName => "Description";
		protected override ModuleIdentifier RelatedModuleID => ClientModuleRegistration.IncidentManagementGroup;
		protected override string RelatedEntityFilterStripName => "Related Incident Management Groups";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new IncidentManagementGroupCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new IncidentManagementGroupFilterBusinessObject();
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
		protected override RelatedIncidentManagementGroupOfIncidentFilter GetNewModuleFilter()
		{
			var filter = new RelatedIncidentManagementGroupOfIncidentFilter("moo", () => CreateRelatedEntityCollection());
			filter.Category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
			return filter;
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(SupportIncident mainBizo)
		{
			return mainBizo.IM_DescriptionInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(IncidentManagementGroup relatedBizo)
		{
			return relatedBizo.ING_DescriptionInfo;
		}
	}
}
