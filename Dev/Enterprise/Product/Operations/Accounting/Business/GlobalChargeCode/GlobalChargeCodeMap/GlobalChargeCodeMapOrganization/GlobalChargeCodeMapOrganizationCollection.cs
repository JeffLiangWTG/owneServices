using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	[ModuleID(ModuleId.GlobalChargeCodeOrganization)]
	public class GlobalChargeCodeMapOrganizationCollection : BusinessObjectCollection<GlobalChargeCodeMapOrganization>
	{
		public GlobalChargeCodeMapOrganizationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlobalChargeCodeMapOrganizationCollection(BusinessObjectFactory factory, ZGuid organisationPK)
			: base(factory)
		{
			this.OrganisationPK = organisationPK;
		}

		readonly ZGuid OrganisationPK;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapSchema.YG_OH, SQLComparisonOperator.NotEqual, null));
			if (!OrganisationPK.IsEmpty && OrganisationPK.IsValid)
			{
				filter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapSchema.YG_OH, OrganisationPK));
			}
			return filter;
		}
	}
}

