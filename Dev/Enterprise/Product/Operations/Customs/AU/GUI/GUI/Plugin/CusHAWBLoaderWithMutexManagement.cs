using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class CusHAWBLoaderWithMutexManagement
	{
		public CusHAWBLoaderWithMutexManagement(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public CusHAWB Load(CusMAWB mawb, bool reloadExistingRows = false)
		{
			var result = CusHAWB.Load(shipment, reloadExistingRows);

			if (result != null)
			{
				AfterCusHAWBIsLoadedOrCreated(result, mawb);
			}

			return result;
		}

		public CusHAWB CreateWithMutexLock(CusMAWB mawb)
		{
			CusHAWB result = null;

			if (MutexForShipment.Lock())
			{
				result = CusHAWB.CreateNew(mawb, shipment);
				shipment.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}

			if (result != null)
			{
				AfterCusHAWBIsLoadedOrCreated(result, mawb);
			}

			return result;
		}

		void AfterCusHAWBIsLoadedOrCreated(CusHAWB hawb, CusMAWB mawb)
		{
			if (shipment.AirCargoSynchroniser == null)
			{
				shipment.AirCargoSynchroniser = new HAWBToShipmentBridge(hawb);
			}

			if (mawb != null)
			{
				if (!mawb.ChildBills.Contains(hawb))
				{
					mawb.ChildBills.Add(hawb);
				}

				mawb.FilteredChildBills.Rebuild();
			}
		}

		public CusHAWB LoadOrCreate(CusMAWB mawb)
			=> Load(mawb)
				?? CreateWithMutexLock(mawb);

		ZGlobalMutex MutexForShipment
		{
			get { return fMutexForShipment ?? (fMutexForShipment = Customs.Business.CusHAWB.CreateMutexForShipment(shipment.PK, Core.Constants.CountryCodes.Australia)); }
		}
		ZGlobalMutex fMutexForShipment;

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfNecessary();
				shipment.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		internal void UnlockMutexIfNecessary()
		{
			if (fMutexForShipment != null)
			{
				if (MutexForShipment.HasLock)
				{
					MutexForShipment.Unlock();
				}

				((IDisposable)MutexForShipment).Dispose();

				fMutexForShipment = null;
			}
		}
	}
}
