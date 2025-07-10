using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDShipmentPlugIn : SCDPlugIn
	{
		public SCDShipmentPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		public override bool CanDelete
		{
			get { return SCDShipment.State == DepotState.Unknown || SCDShipment.State == DepotState.ImpendingCargoCancelled; }
		}

		protected internal override MultiMessageManager GetManager()
		{
			ICusUnderbondUnionCollectionParent collectionParent = BusinessEntity as ICusUnderbondUnionCollectionParent;
			if (collectionParent != null)
			{
				return new SeaCargoDepotMultiMessageManager(collectionParent);
			}
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			if (CFSShipment != null && fSeaCargoDepotShipment == null)
			{
				fSeaCargoDepotShipment = SeaCargoDepotShipment.Load(CFSShipment);
			}
			return fSeaCargoDepotShipment;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			if (CFSShipment != null && fSeaCargoDepotShipment == null)
			{
				fSeaCargoDepotShipment = CFSShipmentWrapper.Load(CFSShipment);
			}
			return fSeaCargoDepotShipment;
		}

		public CFSShipment CFSShipment
		{
			get { return HostBusinessEntity as CFSShipment; }
		}

		public SeaCargoDepotShipment SCDShipment
		{
			get { return (SeaCargoDepotShipment)BusinessEntity; }
		}

		protected override Control GetNewUserControl()
		{
			if (IsCMR)
			{
				CMRDepotShipmentUserControl result = new CMRDepotShipmentUserControl(ModuleIDs.Customs.AU.CusSCADepotHouse);
				CMRDepotUnderbondUserControl underbondControl = new CMRDepotUnderbondUserControl();
				result.Controls.Add(underbondControl);
				underbondControl.Dock = DockStyle.Fill;
				underbondControl.BringToFront();
				return result;
			}
			else
			{
				return new SCDUserControl();
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return CFSShipment != null && CFSShipment.ArrivalConsol != null && CFSShipment.ArrivalConsol.JK_TransportMode == Core.Constants.TransportModes.Sea;
		}

		#region Implementation

		SeaCargoDepotShipment fSeaCargoDepotShipment;

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem item = base.GetNewTopLevelMenu();
			if (IsCMR)
			{
				item.MenuItems.Add(new CMRMessageManagementMenu(Manager));
			}
			return item;
		}

		public void ForceImpendingArrival()
		{
			ForceImpendingArrival_Click(this, EventArgs.Empty);
		}

		void ForceImpendingArrival_Click(object sender, EventArgs e)
		{
			if (SCDShipment != null)
			{
				SCDShipment.Shipment.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
			}
		}

		public void ForceReadyForDelivery()
		{
			ForceReadyForDelivery_Click(this, EventArgs.Empty);
		}

		void ForceReadyForDelivery_Click(object sender, EventArgs e)
		{
			if (SCDShipment != null)
			{
				SCDShipment.Shipment.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.CargoDeliveredError);
			}
		}

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
