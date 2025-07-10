using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class HousebillsInInterfaceFileList
	{
		protected HousebillsInInterfaceFileList()
		{
		}

		public static HousebillsInInterfaceFileList Instance
		{
			get { return fInstance ?? (fInstance = new HousebillsInInterfaceFileList()); }
		}
		[ThreadStatic]
		static HousebillsInInterfaceFileList fInstance;

		public void Add(string consolRef, ZGuid shipmentPK)
		{
			List<ZGuid> shipmentPKs = (List<ZGuid>)ConsolList[consolRef];
			if (shipmentPKs == null)
			{
				shipmentPKs = new List<ZGuid>();
				ConsolList.Add(consolRef, shipmentPKs);
			}
			if (!shipmentPKs.Contains(shipmentPK))
			{
				shipmentPKs.Add(shipmentPK);
			}
		}

		public void Clear()
		{
			ConsolList.Clear();
		}

		public bool Contains(string consolRef, ZGuid shipmentPK)
		{
			bool result = false;
			List<ZGuid> shipmentPKs = (List<ZGuid>)ConsolList[consolRef];
			if (shipmentPKs != null)
			{
				result = shipmentPKs.Contains(shipmentPK);
			}
			return result;
		}

		Hashtable ConsolList
		{
			get
			{
				if (fConsolList == null)
				{
					fConsolList = new Hashtable();
				}
				return fConsolList;
			}
		}
		Hashtable fConsolList;

		internal int NoOfShipmentLinkedToConsol(string consolRef)
		{
			int result = 0;
			List<ZGuid> shipmentPKs = (List<ZGuid>)ConsolList[consolRef];
			if (shipmentPKs != null)
			{
				result = shipmentPKs.Count;
			}
			return result;
		}
	}
}
