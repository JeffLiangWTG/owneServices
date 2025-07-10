using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class AFRStatusHelper : Integration.Customs.JP.AFR.IAFRStatusHelper
	{
		public const string MultipleStatusCode = "Multiple";

		public static string MultipleBillsWithDifferentStatuses
		{
			get { return ResString.GetMultilingualString("2C9C4427-7515-4FEA-A712-BFC86068F051", "There are multiple bills with different statuses"); }
		}

		#region IAFRStatusHelper Members

		public ZString GetAFRBillStatus(Integration.Customs.JP.AFR.IJPAFRHeader headerSource)
		{
			var header = headerSource as JPAFRHeader;
			var result = ZString.Empty;
			if (header != null && !header.IsDeleted)
			{
				foreach (JPAFRBills bill in header.Bills)
				{
					var billStatus = bill.JPB_ReleaseStatus;
					billStatus = billStatus.IsEmpty ? (ZString)AFRBillCustomsStatusList.Codes.NotRegistered : billStatus;
					if (billStatus != result)
					{
						if (result.IsEmpty)
						{
							result = billStatus;
						}
						else
						{
							result = AFRStatusHelper.MultipleStatusCode;
							break;
						}
					}
				}
			}
			return result;
		}

		public ZString GetAFRBillStatus(Integration.Forwarding.IForwardingConsol consolSource)
		{
			ZString result = ZString.Empty;
			var consol = consolSource as ForwardingConsol;
			if (consol.IsEligibleForJPAFR())
			{
				var header = consol.GetAFRHeader();
				if (header != null)
				{
					result = GetAFRBillStatus(header);
				}
			}
			return result;
		}

		public ZString GetAFRBillStatus(Integration.Forwarding.IForwardingShipment shipmentSource)
		{
			ZString result = ZString.Empty;
			var shipment = shipmentSource as ForwardingShipment;
			if (shipment != null && !shipment.IsDeleted)
			{
				var bills = LocateBillsForShipment(shipment.Factory, shipment);
				if (bills != null)
				{
					result = AggregateBillStatus(bills);
				}
			}
			return result;
		}

		public ZString GetAFRBillStatusDescription(BusinessObjectFactory factory, ZString status)
		{
			ZString result;
			if (status == MultipleStatusCode)
			{
				result = MultipleBillsWithDifferentStatuses;
			}
			else
			{
				result = factory.GetCachedValue<AFRBillCustomsStatusList>().GetDescriptionFromCode(status);
			}
			return result;
		}

		#endregion

		#region Implementation

		IEnumerable<JPAFRBills> LocateBillsForShipment(BusinessObjectFactory factory, ForwardingShipment shipment)
		{
			List<JPAFRBills> result = new List<JPAFRBills>();
			var relatedConsols = shipment.Consols;
			if (relatedConsols == null || relatedConsols.Count == 0)
			{
				result = null;
			}
			else
			{
				var shipmentBill = shipment.JS_HouseBill;
				var relatedAFRHeaders = from relatedConsol in relatedConsols.OfType<ForwardingConsol>().Where(consol => consol.IsEligibleForJPAFR())
										select (relatedConsol.GetAFRHeader());

				foreach (var header in relatedAFRHeaders.OfType<JPAFRHeader>().Where(header => { return !header.IsDeleted; }))
				{
					var foundBills = header.Bills.Find(bill => { return (bill.JPB_BillNumber == shipmentBill) || (bill.JPB_BillNumber.Length > 3 && bill.JPB_BillNumber.SubstringSafe(4) == shipmentBill); });
					result.AddRange(foundBills);
				}
			}
			return result;
		}

		ZString AggregateBillStatus(IEnumerable<JPAFRBills> bills)
		{
			var result = ZString.Empty;
			foreach (var bill in bills)
			{
				var billStatus = bill.JPB_ReleaseStatus;
				if (billStatus.IsEmpty)
				{
					billStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
				}
				if (result.IsEmpty)
				{
					result = billStatus;
				}
				else if (result != billStatus)
				{
					result = MultipleStatusCode;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
