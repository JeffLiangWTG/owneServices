using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostParentLinkDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectToBeDeleted)
		{
			if (IsJobConsolCostExist(businessObjectToBeDeleted))
			{
				var message = Res.GetString("f9dd1984-0578-48cb-a8f8-e8dbf8052373", "{0} cannot be deleted. A Consol Cost has been created for this.", ((IJobCostingPlugIn)businessObjectToBeDeleted).JK_UniqueConsignRef);
				return new DeleteDetails.Disallow(message);
			}
			return new DeleteDetails.Allow();
		}

		bool IsJobConsolCostExist(BusinessObject businessObjectToBeDeleted)
		{
			if (businessObjectToBeDeleted is IGenericJobCostPlugInBase)
			{
				var query = new ZQuery(JobConsolCostSchema.E6_ParentID, businessObjectToBeDeleted.PK);
				return businessObjectToBeDeleted.Factory.Exists(typeof(JobConsolCost),query, false);
			}

			return false;
		}
	}
}
