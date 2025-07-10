using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientInvoiceDeliveryCollection : ActiveBusinessObjectCollection<ClientInvoiceDelivery>
	{
		public ClientInvoiceDeliveryCollection(LicenceCompany master)
			: base(master.Factory, master, new ZQuery(), ClientInvoiceDeliverySchema.L9_LC)
		{
			Master = master;
		}

		readonly LicenceCompany Master;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(ClientInvoiceDelivery newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.L9_LC = Master.PK;
		}

		#endregion

		public ClientInvoiceDelivery FindByServerAndSystem(string serverCode, string billingSystemCode)
		{
			return FindByServerAndSystem(this, serverCode, billingSystemCode);
		}

		static public ClientInvoiceDelivery FindByServerAndSystem(IEnumerable<ClientInvoiceDelivery> deliveries, string serverCode, string billingSystemCode)
		{
			ClientInvoiceDelivery systemMatch = null;
			ClientInvoiceDelivery serverMatch = null;
			ClientInvoiceDelivery allMatch = null;
			foreach (ClientInvoiceDelivery item in deliveries)
			{
				if (item.L9_SystemCode == billingSystemCode && item.L9_ServerCode == serverCode)
				{
					// exact match
					return item;
				}
				else if (item.L9_ServerCode.IsEmpty && item.L9_SystemCode == billingSystemCode)
				{
					systemMatch = item;
				}
				else if (item.L9_ServerCode == serverCode && item.L9_SystemCode == BillingConstants.BillingSystem.All)
				{
					serverMatch = item;
				}
				else if (item.L9_ServerCode.IsEmpty && item.L9_SystemCode == BillingConstants.BillingSystem.All)
				{
					allMatch = item;
				}
			}
			return systemMatch ?? serverMatch ?? allMatch;
		}
	}
}

