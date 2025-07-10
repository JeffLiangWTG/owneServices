using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("SupportIncident")]
	public class SupportIncidentCollection : BusinessObjectCollection<SupportIncident>
	{
		public SupportIncidentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public SupportIncidentCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new SupportIncidentCollectionFetchStrategy(this);
		}

		#region Relationship Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			return query;
		}

		#endregion
	}
}

