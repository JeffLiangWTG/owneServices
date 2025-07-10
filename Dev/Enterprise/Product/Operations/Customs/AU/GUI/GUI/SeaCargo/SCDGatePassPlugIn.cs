using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDGatePassPlugIn : SCDPlugIn
	{
		public SCDGatePassPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		public override bool CanDelete
		{
			get { return SCDGatePassShipment == null || SCDGatePassShipment.State == DepotState.Unknown || SCDGatePassShipment.State == DepotState.ImpendingCargoCancelled; }
		}

		protected internal override MultiMessageManager GetManager()
		{
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotGatePass == null)
			{
				fSeaCargoDepotGatePass = SeaCargoDepotGatePass.Load((GatePassShipment)HostBusinessEntity);
			}
			return fSeaCargoDepotGatePass;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotGatePass == null)
			{
				fSeaCargoDepotGatePass = CFSShipmentWrapper.Load(HostBusinessEntity as CFSShipment);
			}
			return fSeaCargoDepotGatePass;
		}

		public SeaCargoDepotBusinessObject SCDGatePassShipment
		{
			get
			{
				return (SeaCargoDepotBusinessObject)BusinessEntity;
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return CFSShipment != null && CFSShipment.ArrivalConsol != null && CFSShipment.ArrivalConsol.JK_TransportMode == Core.Constants.TransportModes.Sea;
		}

		public GatePassShipment CFSShipment
		{
			get { return HostBusinessEntity as GatePassShipment; }
		}

		#region Implementation

		SeaCargoDepotBusinessObject fSeaCargoDepotGatePass;

		protected internal override bool ForceCMR
		{
			get
			{
				if (CFSShipment != null && CFSShipment.ArrivalConsol != null)
				{
					return ConsolHasCMRUnderbonds((CFSLoadListConsol)CFSShipment.ArrivalConsol);
				}
				return false;
			}
		}

		protected internal override bool ForceLegacy
		{
			get
			{
				if (CFSShipment != null && CFSShipment.ArrivalConsol != null)
				{
					return ConsolHasLegacyMessages((CFSLoadListConsol)CFSShipment.ArrivalConsol);
				}
				return false;
			}
		}

		protected override ZDateTime DateOfFirstArrival
		{
			get
			{
				ZDateTime result = ZDateTime.Now;
				if (CFSShipment != null && CFSShipment.ArrivalConsol != null)
				{
					if (!CFSShipment.ArrivalConsol.JK_DatePortOfFirstArrival.IsEmpty)
					{
						result = CFSShipment.ArrivalConsol.JK_DatePortOfFirstArrival;
					}
					else if (!CFSShipment.ArrivalConsol.JK_JX_JB_E_ARV.IsEmpty)
					{
						result = CFSShipment.ArrivalConsol.JK_JX_JB_E_ARV;
					}
				}
				return result;
			}
		}

		#endregion

	}
}
