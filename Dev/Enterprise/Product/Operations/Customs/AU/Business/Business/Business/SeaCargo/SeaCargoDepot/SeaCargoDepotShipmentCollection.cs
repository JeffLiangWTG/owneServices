using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for SeaCargoDepotShipmentCollection.
	/// </summary>
	public class SeaCargoDepotShipmentCollection : NonPersistentBusinessObjectCollection<SeaCargoDepotShipment>
	{
		public SeaCargoDepotShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(SeaCargoDepotShipment);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public void RemoveRelated(CommonShipment cFSShipmentToRemove)
		{
			for (int i = Count - 1; i > 0; i--)
			{
				SeaCargoDepotShipment shipment = (SeaCargoDepotShipment)Elements[i];
				if (shipment.WrappedBusinessObject.PK == cFSShipmentToRemove.PK)
				{
					Remove(shipment.PK);
					break;
				}
			}
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			CFSShipment toKeepCollectionTestHappyShipment = Factory.New<CFSShipment>();
			return SeaCargoDepotShipment.Load(toKeepCollectionTestHappyShipment);
		}
	}
}
