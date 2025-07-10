using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ProcessManagement.Module.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedProjectsOfIncidentFilter))]
	class RelatedProjectsOfIncidentFilterTest : GenPivotDualDirectionRelatedEntityFilterTestCase<RelatedProjectsOfIncidentFilter, SupportIncident, EDIProject>
	{
		protected override string DescriptionFilterStripName => "Summary";
		protected override ModuleIdentifier RelatedModuleID => ModuleIDs.Project;
		protected override string RelatedEntityFilterStripName => "Related Projects";
		protected override IBusinessObjectCollection CreateRelatedEntityCollection() => new ProjectCollection(Factory);
		protected override FilterStripBusinessObject CreateMainFilterBusinessObject() => new SupportIncidentFilterBusinessObject();
		protected override FilterStripBusinessObject CreateRelatedFilterBusinessObject() => new EDIProjectFilterBusinessObject();
		protected override RelatedProjectsOfIncidentFilter GetNewModuleFilter()
		{
			return new RelatedProjectsOfIncidentFilter("moo", () => CreateRelatedEntityCollection());
		}

		protected override ZPropertyInfo GetMainBizoDescriptionProperty(SupportIncident mainBizo)
		{
			return mainBizo.IM_DescriptionInfo;
		}

		protected override ZPropertyInfo GetRelatedBizoDescriptionProperty(EDIProject relatedBizo)
		{
			return relatedBizo.WKP_SummaryInfo;
		}
	}
}
