using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FeatureRequestRelatedFeatureRequestsCollection : WorkTaskRelatedItemGenPivotCollection<SupportIncident>
	{
		public FeatureRequestRelatedFeatureRequestsCollection(SupportIncident associatedObject)
			: base(associatedObject, RelatedLinkType.MasterAlwaysParent)
		{
		}

		protected override bool ShouldAddToCollection(BusinessObject relatedItem)
		{
			bool result = base.ShouldAddToCollection(relatedItem);
			if (result)
			{
				result = ((SupportIncident)relatedItem).IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest;
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((SupportIncident)child).SetupForNewCreatedFeatureRequest();
		}
	}
}

