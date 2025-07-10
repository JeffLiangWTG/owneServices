using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromOrgSalesCallBO : FreightWrapper
	{
		#region Constructors

		public FreightWrapperFromOrgSalesCallBO(OrgSalesCall orgSalesCallBO, BusinessObjectFactory factory)
			: base(orgSalesCallBO, factory)
		{
			Argument.NotNull(factory, "factory");
			OrgSalesCallBO = orgSalesCallBO;
		}

		#endregion

		readonly OrgSalesCall OrgSalesCallBO;

		protected override DocSalesCall GetOrgSalesCallDocument()
		{
			return (DocSalesCall)DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.SalesCall, OrgSalesCallBO);
		}

		#region Implementation

		protected override ZString GetJobNumber()
		{
			return OrgSalesCallBO.OQ_CommunicationID;
		}

		protected override OrganisationWrapper GetClient()
		{
			var org = Factory.Load<OrgHeader>(OrgSalesCallBO.OQ_OH);
			var contact = Factory.Load<OrgContact>(OrgSalesCallBO.OQ_OC);
			return new OrganisationWrapper(OrganisationUsageType.Client, org, contact, Factory);
		}

		#endregion
	}
}
