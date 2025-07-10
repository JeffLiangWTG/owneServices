using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ShipmentWrapper.
	/// </summary>
	public class ShipmentWrapper : NonPersistentBusinessObject, ISeaCargoShipmentInfo, IObsoleteValidation
	{
		public ShipmentWrapper(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}
		readonly ForwardingShipment shipment;

		public void StartSynchronising()
		{
			if (SeaCargoSynchroniser != null)
			{
				SeaCargoSynchroniser.GetHouseBill(shipment);
			}
		}

		public IManifestProvider ManifestProvider
		{
			get { return shipment; }
		}

		public CusSCAOceanBill OceanBill
		{
			get
			{
				CusSCAOceanBill result = null;
				if (ArrivalConsol != null)
				{
					#pragma warning disable IDE0001 // Prevent simplification to base class
					result = (CusSCAOceanBill)new CusSCAOceanBill.Loader(ArrivalConsol.Factory).LoadFromConsolAndApplicationCode(ArrivalConsol, CusSCAOceanBill.ApplicationCodes);
					#pragma warning restore IDE0001 // Prevent simplification to base class
				}
				return result;
			}
		}

		public CusSCAHouse HouseBill
		{
			get
			{
				#pragma warning disable IDE0001 // Prevent simplification to base class
				return houseBill ?? (houseBill = (CusSCAHouse)new CusSCAHouse.Loader(shipment.Factory).LoadFromShipmentAndApplicationCode(shipment.PK, CusSCAOceanBill.ApplicationCodes, reloadQuery: true));
				#pragma warning restore IDE0001 // Prevent simplification to base class
			}
		}
		CusSCAHouse houseBill;

		public CusSCAHouse GetHouseBill
		{
			get { return SeaCargoSynchroniser.GetHouseBill(shipment); }
		}

		public ForwardingConsol ArrivalConsol
		{
			get
			{
				if (fArrivalConsol == null)
				{
					fArrivalConsol = FindConsolArrivingInAustralia();
				}
				return fArrivalConsol;
			}
		}
		ForwardingConsol fArrivalConsol;

		BusinessObject ISeaCargoShipmentInfo.TopLevelObject
		{
			get { return shipment; }
		}

		protected ForwardingConsol FindConsolArrivingInAustralia()
		{
			ForwardingConsol result = null;
			if (shipment != null)
			{
				foreach (ForwardingConsol consol in shipment.Consols)
				{
					if (consol.JK_JX_JB_RL_NKPortOfDischarge.StartsWith(Enterprise.Core.Constants.CountryCodes.Australia))
					{
						result = consol;
					}
				}
			}
			return result;
		}

		public bool IsShipmentCMRShipment
		{
			get
			{
				ForwardingConsol arrivalConsol = shipment.ArrivalConsol;
				return arrivalConsol != null && new CMRUtilities().ShouldImportMessageBeSentCMR(ZDateTime.Now, arrivalConsol.JK_DatePortOfFirstArrival, false, false);
			}
		}

		public SeaCargoSynchroniser SeaCargoSynchroniser
		{
			get
			{
				if (fSeaCargoSynchroniser == null && shipment != null && ArrivalConsol != null)
				{
					fSeaCargoSynchroniser = new CMRSeaCargoSynchroniser(ArrivalConsol, synchroniseConsol: false);
				}
				return fSeaCargoSynchroniser;
			}
		}
		SeaCargoSynchroniser fSeaCargoSynchroniser;

		public bool RegisterTopLevelBusinessObjectAsEditable
		{
			get { return true; }
		}

		public ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					if (ArrivalConsol != null)
					{
						fMutex = CusSCAOceanBill.CreateMutexForConsol(ArrivalConsol.PK, Core.Constants.CountryCodes.Australia);
					}
				}
				return fMutex;
			}
		}
		ZGlobalMutex fMutex;

		public void UnlockMutexIfNeeded()
		{
			((IDisposable)fMutex)?.Dispose();
			fMutex = null;
		}

		public bool IsVisible
		{
			get
			{
				return shipment.JS_TransportMode == Enterprise.Core.Constants.TransportModes.Sea
					&& shipment.HasConsolsDischargingInCurrentCountry
					&& !shipment.HasConsolsLoadingInCurrentCountry
					&& IsShipmentCMRShipment;
			}
		}

		public void SynchroniseIfWeCan()
		{
			if (HouseBill != null && shipment != null && ArrivalConsol != null && !shipment.IsDeleted && !HouseBill.IsDeleted)
			{
				SeaCargoSynchroniser?.SynchroniseHouse(HouseBill, shipment);
			}
		}

		public bool CanSynchronise
		{
			get { return SynchroniseFailureMessage.IsEmpty; }
		}

		public bool NoSynchronisingWillOccur { get; private set; }

		public ZString SynchroniseFailureMessage
		{
			get
			{
				NoSynchronisingWillOccur = true;
				ZStringBuilder sb = new ZStringBuilder();
				if (OceanBill != null)
				{
					foreach (CusSCAHouse house in OceanBill.HouseBills)
					{
						if (!CMRStatusHelper.CanDelete(house.CA_MessageStatus))
						{
							sb.Append("House Bill " + house.CA_HouseBill + " will not be synchronised, is currently " + new CMRBaseStatuses().GetDescriptionFromCode(house.CA_MessageStatus));
						}
						else
						{
							NoSynchronisingWillOccur = false;
						}
					}
				}
				return sb.ToStringWithNewLineBetweenAppends();
			}
		}
	}
}
