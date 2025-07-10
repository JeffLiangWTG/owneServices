using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// IWorkTaskRelatedItemSource to NewWorkItem only,
	/// i.e., ProfessionalServicesQuote and HelpErrorLog.
	/// </summary>
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[ModuleID("WorkItem")]
	public class NewWorkItemRelatedCollection : WorkTaskRelatedItemGenPivotCollection<NewWorkItem>
	{
		public NewWorkItemRelatedCollection(IWorkTaskRelatedItemSource master, RelatedLinkType relatedLinkType = RelatedLinkType.TwoWay)
			: base(master, relatedLinkType)
		{
		}
	}

	public static class EDIWorkTaskRelatedItemTypes
	{
		public const string ProfessionalServiceQuote = "Quote";
		public const string Defect = SupportIncidentCategoriesList.Descriptions.Defect;
		public const string FeatureRequest = SupportIncidentCategoriesList.Descriptions.FeatureRequest;
		public const string ComplianceRequirement = SupportIncidentCategoriesList.Descriptions.ComplianceRequirement;
		public const string CustomerServiceRequest = SupportIncidentCategoriesList.Descriptions.CustomerServiceRequest;
		public const string ContentDevelopment = SupportIncidentCategoriesList.Descriptions.ContentDevelopment;
		public const string SupportIncident = "Support Incident";
		public const string Issue = "Issue";
	}
}

