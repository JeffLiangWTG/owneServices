using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Organisations.OrgHeader.FindBoxCollections
{
	public class EDIBrokerCollection : BrokerCollection
	{
		public EDIBrokerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public EDIBrokerCollection(BusinessObjectFactory factory, OrganisationDefaults orgDefaults) : base(factory, orgDefaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZQuery(OrgHeaderSchema.OH_IsCompetitor, true);
			query.AddToFilter(base.CreateAdditionalFilter(), JoinCondition.Or);
			return query;
		}
	}
}





