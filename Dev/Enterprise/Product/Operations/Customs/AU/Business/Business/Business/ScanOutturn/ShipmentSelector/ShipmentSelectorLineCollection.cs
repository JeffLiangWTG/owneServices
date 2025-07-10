using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ShipmentSelectorLineCollection : NonPersistentBusinessObjectCollection<ShipmentSelectorLine>
	{
		public ShipmentSelectorLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public IEnumerable<ShipmentSelectorLine> GetSelectedLines()
		{
			foreach (ShipmentSelectorLine shipment in this)
			{
				if (shipment.IncludeInScan)
				{
					yield return shipment;
				}
			}
		}

		public IEnumerable<ShipmentSelectorLine> GetHLSShipmentIncludeInScan()
		{
			foreach (ShipmentSelectorLine shipment in this)
			{
				if (shipment.IncludeInScan && shipment.Type == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
				{
					yield return shipment;
				}
			}
		}
	}
}
