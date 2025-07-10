using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHJobConShipLinkDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectBeingDeleted)
		{
			DeleteDetails result = new DeleteDetails.Allow();

			var jobConShipLink = businessObjectBeingDeleted as JobConShipLink;
			var consol = jobConShipLink?.Consol;
			if (consol != null && !consol.Factory.CanadianCarrierCode().IsEmpty && consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.Canada))
			{
				var cusCAeMHHouse = GetHouseBillFromConsolJobPivot(businessObjectBeingDeleted);
				var masterBill = cusCAeMHHouse?.MasterBill;
				if (masterBill != null && masterBill.BP_ParentID == consol.PK && (cusCAeMHHouse.IsCustomsStatusActive || cusCAeMHHouse.IsMessageStatusAwaiting || cusCAeMHHouse.IsCloseReported))
				{
					result = new DeleteDetails.Disallow(Res.GetString("a062ff38-dbbc-4eca-821e-27cbdcdb3367", "The shipment has a Canadian House Bill reported and has not been canceled. Please send a withdrawal message and try again."));
				}
			}

			return result;
		}

		public override void BeforeSuccessfulDelete(BusinessObject businessObjectToBeDeleted)
		{
			base.BeforeSuccessfulDelete(businessObjectToBeDeleted);

			var cusCAeMHHouse = GetHouseBillFromConsolJobPivot(businessObjectToBeDeleted);
			if (cusCAeMHHouse != null)
			{
				var masterbill = cusCAeMHHouse.MasterBill;
				if (masterbill != null)
				{
					masterbill.HouseBills.Delete(cusCAeMHHouse);
				}
			}
		}

		CusCAeMHHouse GetHouseBillFromConsolJobPivot(BusinessObject businessObject)
		{
			CusCAeMHHouse result = null;

			if (businessObject is JobConShipLink jobConShipLink)
			{
				var query = new ZDBOnlyQuery(typeof(CusCAeMHHouse));
				query.AddToFilter(CusCAeMHHouseSchema.BW_ParentID, jobConShipLink.JN_JS);
				query.FetchOnlyFromLocalCache = !jobConShipLink.IsInDatabase;
				query.ReLoadExistingRows = true;

				var subQuery = new ZDBOnlySubQuery(typeof(CusCAeMHMaster), CusCAeMHMasterSchema.PK);
				subQuery.AddToFilter(CusCAeMHMasterSchema.BP_ParentID, jobConShipLink.JN_JK);
				subQuery.FetchOnlyFromLocalCache = !jobConShipLink.IsInDatabase;
				subQuery.ReLoadExistingRows = true;

				query.AddSubQuery(CusCAeMHHouseSchema.BW_BP_Master, subQuery, JoinCondition.And);

				result = jobConShipLink.Factory.LoadTop1<CusCAeMHHouse>(query);
			}

			return result;
		}
	}
}
