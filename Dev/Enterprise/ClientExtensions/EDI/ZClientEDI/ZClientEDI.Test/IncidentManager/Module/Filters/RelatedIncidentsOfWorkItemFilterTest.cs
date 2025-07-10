using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.ProcessManagement.Module.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedIncidentsOfWorkItemFilter))]
	class RelatedIncidentsOfWorkItemFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedIncidentsOfWorkItemFilter, NewWorkItem, SupportIncident>
	{
		protected override string DescriptionFilterStripName => "Problem Description";
		protected override ModuleIdentifier RelatedModuleID => Modules.ClientModuleRegistration.SupportIncident;
		protected override string RelatedEntityFilterStripName => "Related Incidents";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new SupportIncidentCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new EDIWorkItemFilterBusinessObject();
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override RelatedIncidentsOfWorkItemFilter GetNewModuleFilter()
		{
			return new RelatedIncidentsOfWorkItemFilter("moo", () => CreateRelatedEntityCollection());
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(NewWorkItem mainBizo)
		{
			return mainBizo.WKI_SummaryInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(SupportIncident relatedBizo)
		{
			return relatedBizo.IM_DescriptionInfo;
		}

		protected override FilterCategory ExpectedDefaultCategory => WorkItemFilterBusinessObject.RelatedItemsFilterCategory;

		protected override void CreateRelationship(IWorkTaskRelatedItemSource fromBizo, IWorkTaskRelatedItemSource toBizo)
		{
			if (fromBizo.GetType() == typeof(NewWorkItem) && toBizo.GetType() == typeof(SupportIncident))
			{
				toBizo.RelatedItems.Add((BusinessObject)fromBizo);
			}
			else
			{
				fromBizo.RelatedItems.Add((BusinessObject)toBizo);
			}
		}
	}
}
