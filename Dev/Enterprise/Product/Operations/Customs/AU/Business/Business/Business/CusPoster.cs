using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPoster : ICusPoster<ForwardingConsol>, Integration.Customs.AU.ICusPoster
	{
		public void PostAndSetSACFlag(ForwardingConsol consol)
		{
			if (consol != null && consol.IsImport())
			{
				if (consol.IsAir)
				{
					ProcessAirCargo(consol);
				}
				else if (consol.IsSea)
				{
					ProcessSeaCargo(consol);
				}
			}
		}

		bool IsSAC(ForwardingShipment shipment)
		{
			StmALog log = shipment.Logs.MostRecentLogByEventTime(Events.DataImport, "AU Declaration Style: SAC");
			return log != null && !log.IsInDatabase;
		}

		void ProcessAirCargo(ForwardingConsol consol)
		{
			using (var processor = new AirCargoProcessorJobForConsol(consol))
			{
				if (processor.MasterBill != null)
				{
					foreach (CusHAWB current in processor.MasterBill.ChildBills)
					{
						if (current.Shipment != null && IsSAC(current.Shipment))
						{
							current.CS_IsSelfAssessedClearance = true;
						}
					}
				}
			}
		}

		void ProcessSeaCargo(ForwardingConsol consol)
		{
			ISeaCargoConsolInfo seaCargoConsolInfo = new SeaCargo.FreightConsolWrapper(consol);

			if (seaCargoConsolInfo != null && seaCargoConsolInfo.Mutex.Lock())
			{
				try
				{
					seaCargoConsolInfo.SeaCargoSynchroniser.SynchroniseOceanBill();

					if (seaCargoConsolInfo.OceanBill != null)
					{
						foreach (CusSCAHouse cusSCAHouse in seaCargoConsolInfo.OceanBill.HouseBills)
						{
							if (cusSCAHouse.Shipment != null && IsSAC(cusSCAHouse.Shipment))
							{
								foreach (CusSCAPivot pivot in cusSCAHouse.Pivot)
								{
									pivot.CV_IsSAC = true;
								}
							}
						}
					}
				}
				finally
				{
					seaCargoConsolInfo.Mutex.Unlock();
				}
			}
		}
	}
}
