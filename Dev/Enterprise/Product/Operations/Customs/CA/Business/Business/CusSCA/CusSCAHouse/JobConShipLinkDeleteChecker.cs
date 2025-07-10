using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAJobConShipLinkDeleteChecker : DeleteChecker
	{
		#region Constants
		internal static string HouseBillInProgress
		{
			get { return Res.GetString("d2d370cc-7cdd-44e4-aa2e-98ef54bb6d00", "The Shipment/Consol cannot be detached as the ACI job on the shipment has messages in progress."); }
		}
		internal static string HouseBillAcknowledged
		{
			get { return Res.GetString("6ded17bb-7104-41a6-894e-8d3cbcb5c792", "The Shipment/Consol cannot be detached as the ACI job on the shipment has been acknowledged. The ACI job needs to be canceled."); }
		}
		#endregion

		public override DeleteDetails DeleteDetails(BusinessObject businessObjectBeingDeleted)
		{
			DeleteDetails result = null;
			CusSCAHouse houseBill = GetHouseBillFromConsolJobPivot(businessObjectBeingDeleted);

			if (houseBill != null && !businessObjectBeingDeleted.IsRefreshingByDataRefreshBus)
			{
				if (StatusCalculator.IsAwaitingReply(houseBill.CA_MessageStatus))
				{
					result = new DeleteDetails.Disallow(HouseBillInProgress);
				}
				else if (StatusCalculator.IsLodged(houseBill.CA_ShipmentStatus))
				{
					result = new DeleteDetails.Disallow(HouseBillAcknowledged);
				}
			}

			if (result == null)
			{
				result = new DeleteDetails.Allow();
			}

			return result;
		}

		SupplementaryCargoReportStatusCalculator StatusCalculator
		{
			get
			{
				if (statusCalculator == null)
				{
					statusCalculator = new SupplementaryCargoReportStatusCalculator();
				}
				return statusCalculator;
			}
		}
		SupplementaryCargoReportStatusCalculator statusCalculator;

		public override void BeforeSuccessfulDelete(BusinessObject businessObjectToBeDeleted)
		{
			base.BeforeSuccessfulDelete(businessObjectToBeDeleted);

			CusSCAHouse houseBill = GetHouseBillFromConsolJobPivot(businessObjectToBeDeleted);
			if (houseBill != null)
			{
				houseBill.PackLines.DeleteAll();
				houseBill.CA_CB = ZGuid.Empty;
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
