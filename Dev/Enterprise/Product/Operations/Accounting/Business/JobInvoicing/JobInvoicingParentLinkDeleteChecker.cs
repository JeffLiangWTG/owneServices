using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobInvoicingParentLinkDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectToBeDeleted)
		{
			if (IsJobHeaderExist(businessObjectToBeDeleted))
			{
				var message = Res.GetString("d1aae24d-4da2-41c5-842c-1ce54104ca0b", "{0} cannot be deleted. An Invoicing Job Header has been created for this.", ((IJobHeaderParent)businessObjectToBeDeleted).JobNumber);
				return new DeleteDetails.Disallow(message);
			}
			return new DeleteDetails.Allow();
		}

		bool IsJobHeaderExist(BusinessObject businessObjectToBeDeleted)
		{
			if (businessObjectToBeDeleted is IJobHeaderParentCore)
			{
				var zQuery = new ZQuery(JobHeaderSchema.JH_ParentID, businessObjectToBeDeleted.PK);
				return businessObjectToBeDeleted.Factory.Exists(typeof(JobHeader), zQuery, false);
			}
			return false;
		}
	}
}
