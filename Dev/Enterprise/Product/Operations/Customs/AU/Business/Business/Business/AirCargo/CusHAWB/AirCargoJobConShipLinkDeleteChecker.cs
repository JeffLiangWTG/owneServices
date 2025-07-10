
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoJobConShipLinkDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectBeingDeleted)
		{
			CusHAWB houseBill = GetHouseBillFromJobConShipLink(businessObjectBeingDeleted);
			DeleteDetails result = new DeleteDetails.Allow();
			if (houseBill != null && !businessObjectBeingDeleted.IsRefreshingByDataRefreshBus && !houseBill.CanDeleteWhenShipmentIsDetached)
			{
				result = new DeleteDetails.Disallow(HAWBCannotBeDeleted);
			}
			return result;
		}

		internal const string HAWBCannotBeDeleted = "This shipment has an Australian AirCargo HAWB record with messages associated with it, or is linked to an outturn. Please send a withdrawal message to AU Customs, or remove from the outturn, and try again.";

		public override void BeforeSuccessfulDelete(BusinessObject businessObjectToBeDeleted)
		{
			base.BeforeSuccessfulDelete(businessObjectToBeDeleted);

			CusHAWB houseBill = GetHouseBillFromJobConShipLink(businessObjectToBeDeleted);
			if (houseBill != null)
			{
				var masterBill = houseBill.MAWB;
				masterBill.ChildBills.Remove(houseBill);
				masterBill.FilteredChildBills.Rebuild();
			}
		}

		#region Implementation

		protected CusHAWB GetHouseBillFromJobConShipLink(BusinessObject pivotToBeDeleted)
		{
			JobConShipLink consolShipmentPivot = (JobConShipLink)pivotToBeDeleted;
			BusinessObjectFactory factory = consolShipmentPivot.Factory;
			CusMAWB cusMAWB = null;
			foreach (Customs.Business.CusMAWB baseCusMAWB in new CusMAWBBase.Loader(factory).FindMatchingMAWBs(consolShipmentPivot.JN_JK, false))
			{
				if (baseCusMAWB is CusMAWB)
				{
					cusMAWB = (CusMAWB)baseCusMAWB;
					break;
				}
			}

			CusHAWB result = null;
			if (cusMAWB != null)
			{
				ZQuery hawbQuery = new ZQuery(CusHAWBSchema.CS_JS, consolShipmentPivot.JN_JS);
				hawbQuery.AddToFilter(JoinCondition.And, CusHAWBSchema.CS_CM, SQLComparisonOperator.Equal, cusMAWB.PK);
				result = factory.LoadTop1<CusHAWB>(hawbQuery);
			}
			return result;
		}

		#endregion
	}
}
