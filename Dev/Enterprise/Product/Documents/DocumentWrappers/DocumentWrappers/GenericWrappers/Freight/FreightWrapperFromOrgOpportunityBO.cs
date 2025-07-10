using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromOrgOpportunityBO : FreightWrapper
	{
		#region Constructors
		public FreightWrapperFromOrgOpportunityBO(OrgOpportunity orgOpportunityBO, BusinessObjectFactory factory)
			: base(orgOpportunityBO, factory)
		{
			Argument.NotNull(factory, "factory");
			OrgOpportunityBO = orgOpportunityBO;
		}
		#endregion

		readonly OrgOpportunity OrgOpportunityBO;

		protected override DocOrgOpportunity GetOrgOpportunityDocument()
		{
			return (DocOrgOpportunity)DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.OrgOpportunity, OrgOpportunityBO);
		}

		protected override SalesRelationsWrapper GetSalesRelationsWrapper()
		{
			if (!OrgOpportunityBO.RelatedChildActivityPivotCollection.IsNullOrEmpty())
			{
				return new SalesRelationsWrapper(OrgOpportunityBO, Factory);
			}
			else
			{
				return base.GetSalesRelationsWrapper();
			}
		}

		#region Implementation

		protected override ZString GetJobNumber()
		{
			return OrgOpportunityBO.P8_OpportunityID;
		}

		protected override OrganisationWrapper GetClient()
		{
			var org = Factory.Load<OrgHeader>(OrgOpportunityBO.P8_OH);
			var contact = Factory.Load<OrgContact>(OrgOpportunityBO.P8_OC);
			return new OrganisationWrapper(OrganisationUsageType.Client, org, contact, Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor, OrgOpportunityBO.AssignedOrg, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee, OrgOpportunityBO.AssignedOrg, ContactType.All, Factory);
		}
		#endregion
	}
}
