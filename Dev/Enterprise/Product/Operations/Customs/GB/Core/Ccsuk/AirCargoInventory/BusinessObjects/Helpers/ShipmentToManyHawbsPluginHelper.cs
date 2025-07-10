using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class ShipmentToManyHawbsPluginHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShipmentToManyHawbsPluginHelper(ForwardingShipment shipment, ForwardingConsol relevantConsol) : base(shipment.Factory)
		{
			this.shipment = shipment;
			this.consol = relevantConsol;
		}

		CusHAWBCollectionNonDependent hawbs;
		public CusHAWBCollectionNonDependent Hawbs
		{
			get
			{
				if (hawbs == null)
				{
					hawbs = new CusHAWBCollectionNonDependent(shipment.Factory);
					if (consol != null)
					{
						var mawbHelper = new ConsolToManyMawbsPluginHelper(consol);
						var hawbQuery = new ZQuery(CusHAWBSchema.CS_CM, (from CusMAWB m in mawbHelper.Mawbs select m.PK));
						var hawbNumberOrJs = new ZQuery(CusHAWBSchema.CS_JS, shipment.PK);
						hawbNumberOrJs.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_HAWB, CcsukUtilities.LeftPadWithZeros(shipment.JS_HouseBill));
						hawbQuery.AddToFilter(hawbNumberOrJs);
						hawbQuery.AddToFilter(CusHAWBSchema.CS_IsActive, true);
						hawbs.Load(hawbQuery);
					}
				}
				return hawbs;
			}
		}

		public int ResetReloadAndCount()
		{
			hawbs = null;
			return Hawbs.Count;
		}

		public void MakeNewHawbAndPurgeCache(CusMAWB onMawb)
		{
			new CusHAWB.Loader(consol.Factory).CreateNewOnMawbLinkedToShipment(onMawb, shipment);
			ResetReloadAndCount();
		}

		public void RegisterHawbsToShipment()
		{
			foreach (CusHAWB h in Hawbs)
			{
				shipment.RegisterEditableChildObject(h);
				h.CS_JS = shipment.PK;
			}
		}

		public void SetConsol(ForwardingConsol newlyAttachedConsol)
		{
			this.consol = newlyAttachedConsol; // update to handle the case when this plugin is loaded but hidden upon first opening the shipment form, and then the consol is attached (thus showing the plugin). Without this we incorrectly don't find a hawb on the shipment (actually we don't even look because the consol is null). 
		}

		readonly ForwardingShipment shipment;
		ForwardingConsol consol;
	}
}
