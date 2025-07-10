using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business.SeaCargo
{
	public class FreightConsolWrapper : NonPersistentBusinessObject, ISeaCargoConsolInfo, IObsoleteValidation
	{
		public FreightConsolWrapper(CommonConsol consol)
		{
			this.consol = consol;
		}
		readonly CommonConsol consol;

		public BusinessObject TopLevelObject
		{
			get { return consol; }
		}

		public CusSCAOceanBill OceanBill
		{
			#pragma warning disable IDE0001 // Prevent simplification to base class
			get { return (CusSCAOceanBill)new CusSCAOceanBill.Loader(consol.Factory).LoadFromConsolAndApplicationCode(consol, CusSCAOceanBill.ApplicationCodes); }
			#pragma warning restore IDE0001 // Prevent simplification to base class
		}

		public bool IsVisible
		{
			get
			{
				return consol.JK_RL_NKDischargePort.StartsWith("AU")
					&& !consol.JK_RL_NKLoadPort.StartsWith("AU")
					&& consol.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Sea
					&& !consol.IsCharter;
			}
		}

		public ZGlobalMutex Mutex
		{
			get
			{
				if (fMutex == null)
				{
					fMutex = CusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia);
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

		protected bool ForceCMR
		{
			get
			{
				return OceanBill != null && OceanBill is CusSCAOceanBill;
			}
		}

		public SeaCargoSynchroniser SeaCargoSynchroniser
		{
			get
			{
				if (seaCargoSynchroniser == null)
				{
					seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: true);
				}
				return seaCargoSynchroniser;
			}
		}
		SeaCargoSynchroniser seaCargoSynchroniser;

		public IManifestProvider ManifestProvider
		{
			get { return consol; }
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
				var sb = new ZStringBuilder();
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
