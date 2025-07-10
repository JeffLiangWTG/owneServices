using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDLoadListPlugIn : SCDPlugIn
	{
		public SCDLoadListPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
		}

		public override bool CanDelete
		{
			get { return true; }
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

		protected override Control GetNewUserControl()
		{
			if (IsCMR)
			{
				return new CMRDepotUnderbondUserControl();
			}
			else
			{
				return new LoadListDepotUserControl();
			}
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotLoadList == null)
			{
				fSeaCargoDepotLoadList = SeaCargoDepotLoadList.Load(CFSLoadList);
			}
			return fSeaCargoDepotLoadList;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotLoadList == null)
			{
				fSeaCargoDepotLoadList = CFSLoadListConsolWrapper.Load(CFSLoadList);
			}
			return fSeaCargoDepotLoadList;
		}

		public SeaCargoDepotLoadList SeaCargoDepotLoadList
		{
			get { return fSeaCargoDepotLoadList; }
		}

		public CFSLoadListConsol CFSLoadList
		{
			get { return HostBusinessEntity as CFSLoadListConsol; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return CFSLoadList != null && CFSLoadList.JK_TransportMode == Core.Constants.TransportModes.Sea;
		}

		#region Implementation

		protected MenuItem containerMenuItem;
		protected MenuItem shipmentMenuItem;

		protected internal MenuItem SendContainerArrivalMessage;
		protected internal MenuItem SendNilOutturnContainerMessage;
		protected internal MenuItem SendOutturnContainerMessage;
		protected internal MenuItem SendContainerDeliveryMessage;

		protected internal MenuItem SendShipmentArrivalMessage;
		protected internal MenuItem SendShipmentOutturnMessage;
		protected internal MenuItem SendShipmentDeliveredMessage;

		protected internal MenuItem CoLoadWizardMenuItem;
#if DEBUG
		protected internal MenuItem CMRDepotMessageSenderMenuItem;
		protected internal MenuItem DebugMessageSenderMenuItem;
#endif

		SeaCargoDepotLoadList fSeaCargoDepotLoadList;

		protected CFSLoadListConsol Consol
		{
			get
			{
				return HostBusinessEntity as CFSLoadListConsol;
			}
		}

		protected virtual CommonContainer CurrentContainer
		{
			get
			{
				CommonContainer result = null;
				if (Consol.Containers.Count == 1)
				{
					result = Consol.Containers[0];
				}
				return result;
			}
		}

		protected virtual CommonShipment CurrentShipment
		{
			get
			{
				CommonShipment result = null;
				if (Consol.Shipments.Count == 1)
				{
					result = Consol.Shipments[0];
				}
				return result;
			}
		}

		protected bool ConfirmSendNilOutturnMessage()
		{
			bool result = Globals.Message.Show("Declare outturn of container matches manifest and no pilliged or damaged items found", "Confirm Send Nil Outturn Message", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
			return result;
		}

		protected void NilOutturnContainer(CommonContainer container)
		{
			foreach (PackLine packLine in container.PackLines)
			{
				packLine.JL_Outturn = packLine.JL_PackageCount;
				packLine.JL_Damaged = 0;
				packLine.JL_Pillaged = 0;
			}
		}

		void CoLoadWizardMenuItem_Click(object sender, EventArgs e)
		{
			CFSLoadListConsolForm parentForm = ((MenuItem)sender).GetMainMenu().GetForm() as CFSLoadListConsolForm;
			if (parentForm != null)
			{
				CommonShipment[] shipments = parentForm.GetSelectedShipments();
				if (shipments.Length == 0)
				{
					Globals.Message.ShowInformation("Please select a Co-Load Master Shipment to run this wizard", "Select a shipment");
				}
				else if (shipments.Length == 1)
				{
					if (shipments[0].HasChanges)
					{
						Globals.Message.ShowInformation("Please save the current records before running the wizard", "Select a shipment");
					}
					else if (!shipments[0].IsCoLoadMaster && !shipments[0].IsBlindCoLoadMaster)
					{
						Globals.Message.ShowInformation("The selected shipment is no a co-load Master", "Select a shipment");
					}
					else
					{
						RunCoLoadWizard(shipments[0].PK);
					}
				}
				else if (shipments.Length > 1)
				{
					Globals.Message.ShowInformation("Please select only one Co-Load shipment to run this wizard", "Select a shipment");
				}
			}
		}

		protected void SynchroniseEventClick(object sender, EventArgs e)
		{
			if (Synchroniser != null)
			{
				Synchroniser.Synchronise(Customs.Business.SynchroniseAction.Force);
			}
		}

		protected internal override bool ForceCMR
		{
			get
			{
				if (CFSLoadList != null)
				{
					return ConsolHasCMRUnderbonds(CFSLoadList);
				}
				return false;
			}
		}

		protected internal override bool ForceLegacy
		{
			get
			{
				if (CFSLoadList != null)
				{
					return ConsolHasLegacyMessages(CFSLoadList);
				}
				return false;
			}
		}

		protected override ZDateTime DateOfFirstArrival
		{
			get
			{
				ZDateTime result = ZDateTime.Now;
				if (CFSLoadList != null)
				{
					if (!CFSLoadList.JK_DatePortOfFirstArrival.IsEmpty)
					{
						result = CFSLoadList.JK_DatePortOfFirstArrival;
					}
					else if (!CFSLoadList.JK_JX_JB_E_ARV.IsEmpty)
					{
						result = CFSLoadList.JK_JX_JB_E_ARV;
					}
				}
				return result;
			}
		}

		void RunCoLoadWizard(ZGuid shipmentPK)
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			PackUnpackShipment coLoadMaster = (PackUnpackShipment)factory2.Load(typeof(PackUnpackShipment), shipmentPK);
			CoLoadWizardShipment coloadShipments = new CoLoadWizardShipment(new BusinessObjectFactory(), coLoadMaster);
			if (ZFormModaliser.ShowDialogAndDispose(new CoLoadWizardForm(coloadShipments)) == DialogResult.OK)
			{
				try
				{
					coloadShipments.Factory.Save();
					HostBusinessEntity.HasChanges = true;
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem item = base.GetNewTopLevelMenu();
			if (IsCMR)
			{
				item.MenuItems.Add(new CMRMessageManagementMenu(Manager));
				item.MenuItems.Add(new ZMenuItem("Synchronise Outturns", new EventHandler(SynchroniseEventClick)));
			}
			item.MenuItems.Add("-");
			CoLoadWizardMenuItem = new ZMenuItem("Co-Load Wizard", new EventHandler(CoLoadWizardMenuItem_Click));
			item.MenuItems.Add(CoLoadWizardMenuItem);
			return item;
		}

		#endregion

	}
}
