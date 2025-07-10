using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SCDTallyPlugIn : SCDPlugIn
	{
		public SCDTallyPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
		}

		public override bool CanDelete
		{
			get { return SCDTallyContainer.State == DepotState.Unknown || SCDTallyContainer.State == DepotState.ImpendingCargoCancelled; }
		}

		protected internal override MultiMessageManager GetManager()
		{
			return null;//For CMR, this plugin is disposed in SeaCargoDepotStandAloneController
		}

		protected override IBusiness GetBusinessEntityForPlugIn_Legacy()
		{
			if (HostBusinessEntity != null && fSeaCargoDepotTally == null)
			{
				fSeaCargoDepotTally = SeaCargoDepotTally.Load((TallyContainer)HostBusinessEntity);
			}
			return fSeaCargoDepotTally;
		}

		protected override IBusiness GetBusinessEntityForPlugIn_CMR()
		{
			return SeaCargoDepotTally.Load((TallyContainer)HostBusinessEntity);
		}

		public SeaCargoDepotTally SCDTallyContainer
		{
			get { return (SeaCargoDepotTally)BusinessEntity; }
		}

		public TallyContainer CFSContainer
		{
			get { return HostBusinessEntity as TallyContainer; }
		}

		#region Implemenatation

		SeaCargoDepotTally fSeaCargoDepotTally;
		protected internal MenuItem CoLoadShipmentWizardMenuItem;

		protected override MenuItem GetNewTopLevelMenu()
		{
			MenuItem item = base.GetNewTopLevelMenu();
			if (!IsCMR)
			{
				item.MenuItems.Add(new ZMenuItem("-"));

				CoLoadShipmentWizardMenuItem = new ZMenuItem("Co-Load Shipment Wizard", new EventHandler(CoLoadShipmentWizard_Click));
				item.MenuItems.Add(CoLoadShipmentWizardMenuItem);
			}
			return item;
		}

		void CoLoadShipmentWizard_Click(object sender, EventArgs e)
		{
			ManifestTallyForm parentForm = ((MenuItem)sender).GetMainMenu().GetForm() as ManifestTallyForm;
			if (parentForm != null)
			{
				System.Reflection.FieldInfo packagesUserControlFieldInfo = typeof(ManifestTallyForm).GetField("PackagesUserControl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				PackagesUserControl userControl = (PackagesUserControl)packagesUserControlFieldInfo.GetValue(parentForm);

				System.Reflection.FieldInfo shipmentGridProperty = userControl.GetType().GetField("ShipmentsGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				ZGrid shipmentsGrid = (ZGrid)shipmentGridProperty.GetValue(userControl);
				if (shipmentsGrid.CurrentRowIndex != -1)
				{
					CFSShipment shipment = ((CFSContainer)parentForm.BusinessEntity).PackUnpackShipments[shipmentsGrid.CurrentRowIndex];
					if (shipment != null && (shipment.IsCoLoadMaster && shipment.IsBlindCoLoadMaster))
					{
						CoLoadWizardShipment coloadShipments = new CoLoadWizardShipment(new BusinessObjectFactory(), shipment);
						if (ZFormModaliser.ShowDialogAndDispose(new CoLoadWizardForm(coloadShipments)) == DialogResult.OK)
						{
							coloadShipments.Factory.Save();
							foreach (PackUnpackShipment addedShipment in coloadShipments.CoLoadShipments)
							{
								((TallyContainer)parentForm.BusinessEntity).PackUnpackShipments.Add(addedShipment);
								((TallyContainer)parentForm.BusinessEntity).HasChanges = true;
								shipment.ArrivalConsol.Shipments.Add(addedShipment);
								shipment.CoLoadShipments.Add(addedShipment);
							}
							((TallyContainer)parentForm.BusinessEntity).PackUnpackShipments.Load();
						}
					}
					else
					{
						Globals.Message.ShowInformation("Please select a Co-Load shipment to run this wizard", "Select a shipment");
					}
				}
				else
				{
					Globals.Message.ShowInformation("Please create a Co-Load shipment to run this wizard", "Select a shipment");
				}
			}
		}

		void ForceContainerArrived_Click(object sender, EventArgs e)
		{
			if (SCDTallyContainer != null)
			{
				SCDTallyContainer.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.CargoArrived);
			}
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

