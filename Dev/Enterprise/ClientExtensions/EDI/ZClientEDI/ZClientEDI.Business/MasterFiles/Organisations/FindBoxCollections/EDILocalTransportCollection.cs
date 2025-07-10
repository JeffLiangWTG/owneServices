using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Organisations.OrgHeader.FindBoxCollections
{
	public class EDILocalTransportCollection : LocalTransportCollection
	{
		public EDILocalTransportCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
		public EDILocalTransportCollection(BusinessObjectFactory factory, OrganisationDefaults organisationDefaults) : base(factory, organisationDefaults)
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

