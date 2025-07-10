using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobConShipLinkDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectBeingDeleted)
		{
			DeleteDetails result = new DeleteDetails.Allow();
			var houseBill = GetHouseBillFromConsolJobPivot(businessObjectBeingDeleted);

			if (houseBill != null && !businessObjectBeingDeleted.IsRefreshingByDataRefreshBus)
			{
				if (!houseBill.CanDelete)
				{
					result = new DeleteDetails.Disallow(CannotBeDetached);
				}
			}
			return result;
		}

		internal const string CannotBeDetached = "This shipment has an Australian SeaCargo record with messages associated with it. Please send a withdrawal message to AU Customs and try again.";

		public override void BeforeSuccessfulDelete(BusinessObject businessObjectToBeDeleted)
		{
			base.BeforeSuccessfulDelete(businessObjectToBeDeleted);

			CusSCAHouse houseBill = GetHouseBillFromConsolJobPivot(businessObjectToBeDeleted);
			if (houseBill != null)
			{
				houseBill.Delete();
			}
		}

		CusSCAHouse GetHouseBillFromConsolJobPivot(BusinessObject businessObjectToBeDeleted)
		{
			var consolShipmentPivot = (JobConShipLink)businessObjectToBeDeleted;
			foreach (var house in businessObjectToBeDeleted.Factory.Load<BaseCusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, consolShipmentPivot.JN_JS)))
			{
				var oceanBillQuery = new ZQuery(CusSCAOceanBillSchema.PK, house.CA_CB);
				oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
				oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ParentId, consolShipmentPivot.JN_JK);
				oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				if (businessObjectToBeDeleted.Factory.LoadTop1<CusSCAOceanBill>(oceanBillQuery) != null)
				{
					return (CusSCAHouse)house;
				}
			}
			return null;
		}
	}
}
