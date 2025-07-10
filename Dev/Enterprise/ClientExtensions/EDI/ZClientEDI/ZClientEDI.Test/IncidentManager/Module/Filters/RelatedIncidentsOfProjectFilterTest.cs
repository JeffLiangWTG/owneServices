using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Module.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedIncidentsOfProjectFilter))]
	class RelatedIncidentsOfProjectFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedIncidentsOfProjectFilter, EDIProject, SupportIncident>
	{
		protected override string DescriptionFilterStripName => "Problem Description";
		protected override ModuleIdentifier RelatedModuleID => Modules.ClientModuleRegistration.SupportIncident;
		protected override string RelatedEntityFilterStripName => "Related Incidents";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new SupportIncidentCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new EDIProjectFilterBusinessObject();
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override RelatedIncidentsOfProjectFilter GetNewModuleFilter()
		{
			return new RelatedIncidentsOfProjectFilter("moo", () => CreateRelatedEntityCollection());
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(EDIProject mainBizo)
		{
			return mainBizo.WKP_SummaryInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(SupportIncident relatedBizo)
		{
			return relatedBizo.IM_DescriptionInfo;
		}
	}
}
