using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Module.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedWorkItemsOfIncidentFilter))]
	class RelatedWorkItemsOfIncidentFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedWorkItemsOfIncidentFilter, SupportIncident, NewWorkItem>
	{
		protected override string DescriptionFilterStripName => "Summary";
		protected override ModuleIdentifier RelatedModuleID => ModuleIDs.WorkItem;
		protected override string RelatedEntityFilterStripName => "Related Work Items";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new NewWorkItemCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new EDIWorkItemFilterBusinessObject();
		protected override RelatedWorkItemsOfIncidentFilter GetNewModuleFilter()
		{
			return new RelatedWorkItemsOfIncidentFilter("moo", () => CreateRelatedEntityCollection());
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(SupportIncident mainBizo)
		{
			return mainBizo.IM_DescriptionInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(NewWorkItem relatedBizo)
		{
			return relatedBizo.WKI_SummaryInfo;
		}

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
