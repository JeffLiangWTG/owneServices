using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using AccGenericConsol = Enterprise.Accounting.Business.GenericConsol.GenericConsol;

namespace Enterprise.Accounting.Business.Billing
{
	public static class AccBillingHandler
	{
		public static void CreateGSHBillingAuditLog(BusinessObjectFactory factory, ZDateTime eventTimeUtc, string eventUser)
		{
			var billingCode = AccBillingCodes.GatewayBilling;
			var events = AccBillingEventCollector.GetInstance(factory).GetEvents(AccBillingCodes.GatewayBilling);
			if (events != null)
			{
				foreach (var eventInfo in events)
				{
					var jobCostingPlugin = AccGenericConsol.LoadConsolBOFromParentIdAndCode(factory, eventInfo.BilledBizOPK, eventInfo.BilledBizOTableCode);
					var shipments = jobCostingPlugin?.CostSupporter.ShipmentsList;
					if (jobCostingPlugin is CommonConsol consol && shipments != null)
					{
						var summary = GetBillingSummary(factory, billingCode, consol.PK, eventInfo.BilledBizOTableCode);
						if (summary.BilledItemCount < shipments.Length)
						{
							var increment = shipments.Length - summary.BilledItemCount;
							var billingHeader = CreateAccBillingHeader(consol.Factory, AccBillingCodes.GatewayBilling, consol.PK, consol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix, eventInfo.EventCode, eventTimeUtc, eventUser, increment);
							foreach (CommonShipment shipment in shipments.OfType<CommonShipment>())
							{
								AddBillingItem(billingHeader, shipment.PK, JobShipmentSchema.Constants.Prefix, shipment.JS_UniqueConsignRef);
							}
						}
					}
					AccBillingEventCollector.GetInstance(factory).ClearEvents(billingCode, eventInfo.BilledBizOPK, eventInfo.BilledBizOTableCode);
				}
			}
		}

		public static AccBillingSummary GetBillingSummary(BusinessObjectFactory factory, string billingCode, ZGuid parentId, string parentTableCode)
		{
			var query = new ZQuery(AccBillingHeaderSchema.ABH_ParentId, parentId);
			query.AddToFilter(AccBillingHeaderSchema.ABH_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccBillingHeaderSchema.ABH_BillingCode, billingCode);
			var headers = factory.Load<AccBillingHeader>(query);
			return new AccBillingSummary(headers);
		}

		static AccBillingHeader CreateAccBillingHeader(BusinessObjectFactory factory, ZString billingCode, ZGuid billedBizOPK, ZString billedBizOReferenceNumber, ZString billedBizOTableCode, string billingEventCode, ZDateTime billingEventTimeUtc, string billingEventUser, int billingCounter)
		{
			var billingHeader = factory.New<AccBillingHeader>();
			billingHeader.ABH_ParentId = billedBizOPK;
			billingHeader.ABH_ParentTableCode = billedBizOTableCode;
			billingHeader.ABH_BillingCode = billingCode;
			billingHeader.ABH_EventType = billingEventCode;
			billingHeader.ABH_GS_NKEventUser = billingEventUser;
			billingHeader.ABH_EventTimeUtc = billingEventTimeUtc;
			billingHeader.ABH_BillingCounter = billingCounter;
			billingHeader.ABH_ParentReferenceNumber = billedBizOReferenceNumber;
			return billingHeader;
		}

		static void AddBillingItem(AccBillingHeader billingHeader, ZGuid billingItemBizOPK, ZString billingItemBizOTableCode, ZString billingItemBizOReference)
		{
			var billingItem = billingHeader.BillingItems.AddNew();
			billingItem.ABI_ParentId = billingItemBizOPK;
			billingItem.ABI_ParentReferenceNumber = billingItemBizOReference;
			billingItem.ABI_ParentTableCode = billingItemBizOTableCode;
		}
	}
}
