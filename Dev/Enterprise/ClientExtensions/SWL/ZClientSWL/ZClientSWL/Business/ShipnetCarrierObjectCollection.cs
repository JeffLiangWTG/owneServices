using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.SWL.Business
{
	public class ShipnetCarrierObjectCollection : IEnumerable<ShipnetCarrierObject>
	{
		public ShipnetCarrierObjectCollection(INotifications notify)
		{
			if (notify == null)
			{
				throw new ArgumentNullException(nameof(notify));
			}
			this.Notify = notify;
			Collection = new List<ShipnetCarrierObject>();
		}

		public int Count
		{
			get { return Collection.Count; }
		}

		public ShipnetCarrierObject this[OrgHeader carrier]
		{
			get
			{
				ShipnetCarrierObject result = null;
				foreach (ShipnetCarrierObject item in Collection)
				{
					if (item.Carrier.PK == carrier.PK)
					{
						result = item;
						break;
					}
				}
				return result;
			}
		}

		public void Add(ShipnetARInvoice header)
		{
			ShipnetCarrierObject shipnetCarrier = this[header.Carrier];
			if (shipnetCarrier == null)
			{
				shipnetCarrier = ShipnetCarrierObject.New(header.Carrier, Notify);
				Collection.Add(shipnetCarrier);
			}
			shipnetCarrier.Add(header);
		}

		public void Export()
		{
			foreach (ShipnetCarrierObject item in Collection)
			{
				item.Export();
			}
		}

		public IEnumerator<ShipnetCarrierObject> GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		readonly List<ShipnetCarrierObject> Collection;
		readonly INotifications Notify;
	}
}
