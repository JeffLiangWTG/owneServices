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
	public class SCDContainerPlugIn : SCDPlugIn
	{
		public SCDContainerPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		public override bool CanDelete
		{
			get { return SCDContainer.State == DepotState.Unknown || SCDContainer.State == DepotState.ImpendingCargoCancelled; }
		}

		protected internal override MultiMessageManager GetManager()
		{
			if (IsCMR)
			{
				ICusUnderbondUnionCollectionParent collectionParent = BusinessEntity as ICusUnderbondUnionCollectionParent;
				if (collectionParent != null)
				{
					return new SeaCargoDepotMultiMessageManager(collectionParent);
				}
			}
			return null;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotContainer == null)
			{
				fSeaCargoDepotContainer = SeaCargoDepotContainer.Load(CFSContainer);
			}
			return fSeaCargoDepotContainer;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			return SeaCargoDepotContainer.Load(CFSContainer);
		}

		public SeaCargoDepotContainer SCDContainer
		{
			get { return BusinessEntity as SeaCargoDepotContainer; }
		}

		public CFSContainer CFSContainer
		{
			get { return HostBusinessEntity as CFSContainer; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return CFSContainer != null && CFSContainer.Consol != null && CFSContainer.Consol.JK_TransportMode == Core.Constants.TransportModes.Sea;
		}

		#region Implementation

		SeaCargoDepotContainer fSeaCargoDepotContainer;

		void ForceImpendingArrival_Click(object sender, EventArgs e)
		{
			if (SCDContainer != null)
			{
				SCDContainer.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
			}
		}

		void ForceReadyForDelivery_Click(object sender, EventArgs e)
		{
			if (SCDContainer != null)
			{
				SCDContainer.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.CargoArrivedAcknowledged);
			}
		}

		protected override Control GetNewUserControl()
		{
			if (IsCMR)
			{
				CMRDepotShipmentUserControl result = new CMRDepotShipmentUserControl(ModuleIDs.Customs.AU.CusSCADepotContainer);
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

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem item = base.GetNewTopLevelMenu();
			if (IsCMR && Manager != null)
			{
				item.MenuItems.Add(new CMRMessageManagementMenu(Manager));
			}
			return item;
		}

		protected internal override bool ForceCMR
		{
			get
			{
				if (CFSContainer != null && CFSContainer.Consol != null)
				{
					return ConsolHasCMRUnderbonds(CFSContainer.Consol);
				}
				return false;
			}
		}

		protected internal override bool ForceLegacy
		{
			get
			{
				if (CFSContainer != null && CFSContainer.Consol != null)
				{
					return ConsolHasLegacyMessages(CFSContainer.Consol);
				}
				return false;
			}
		}

		protected override ZDateTime DateOfFirstArrival
		{
			get
			{
				ZDateTime result = ZDateTime.Now;
				if (CFSContainer != null && CFSContainer.Consol != null)
				{
					if (!CFSContainer.Consol.JK_DatePortOfFirstArrival.IsEmpty)
					{
						result = CFSContainer.Consol.JK_DatePortOfFirstArrival;
					}
					else if (!CFSContainer.Consol.JK_JX_JB_E_ARV.IsEmpty)
					{
						result = CFSContainer.Consol.JK_JX_JB_E_ARV;
					}
				}
				return result;
			}
		}

		#endregion

	}
}

