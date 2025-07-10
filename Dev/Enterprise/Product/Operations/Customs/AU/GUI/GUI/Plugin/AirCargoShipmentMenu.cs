using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoShipmentMenu : CMRMessageManagementMenu
	{
		#region Construction

		protected AirCargoShipmentMenu(CusHAWBMessageManager manager) : base(manager)
		{
			this.Text = "&Air Cargo";
		}

		protected delegate AirCargoShipmentMenu NewDelegate(CusHAWBMessageManager manager);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public static AirCargoShipmentMenu New(CusHAWBMessageManager manager)
		{
			AirCargoShipmentMenu result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				result = new AirCargoShipmentMenu(manager);
			}
			else
			{
				result = overridden(manager);
			}
			return result;
		}

		#endregion

		#region Visibility

		void SetMenuVisibility()
		{
			RefreshAirCargoDataMenuItem.Visible = true;
			sendMessages.Visible = true;
			withdrawMessages.Visible = true;
			resetToOriginal.Visible = true;
			CreateContingencyMenuItem.Visible = true;
			if (ReleaseConsignmentsFromBondStore != null)
			{
				ReleaseConsignmentsFromBondStore.Visible = true;
			}
		}

		#endregion

		#region Items and Text

		public MenuItem RefreshAirCargoDataMenuItem;
		public MenuItem CreateContingencyMenuItem;
		public MenuItem ReleaseConsignmentsFromBondStore;

		public const string RefreshAirCargoDataMenuItemString = "Refresh &AirCargo Data";
		public const string CreateContingencyDataMenuItemString = "Create Contingency Data";
		public const string ReleaseConsignmentsFromBondStoreString = "Release Cleared Consignment from Bond Store";
		protected const string NullHouseBillError = "Please create a house bill or select one you want to act on";

		#endregion

		#region InitializeMenu / OnPopup

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			RefreshAirCargoDataMenuItem = new ZMenuItem(RefreshAirCargoDataMenuItemString);

			CreateContingencyMenuItem = new ZMenuItem(CreateContingencyDataMenuItemString);
			CreateContingencyMenuItem.Click += CreateContingencyMenuItem_Click;
			MenuItems.Add(CreateContingencyMenuItem);

			if (Env.Security.ReleaseConsignmentsFromBondStore.Visible)
			{
				ReleaseConsignmentsFromBondStore = new ZMenuItem(ReleaseConsignmentsFromBondStoreString);
				ReleaseConsignmentsFromBondStore.Click += ReleaseConsignmentsFromBondStore_Click;
				MenuItems.Add(ReleaseConsignmentsFromBondStore);
			}
		}

		void ReleaseConsignmentsFromBondStore_Click(object sender, EventArgs e)
		{
			var licence = Env.Security.ReleaseConsignmentsFromBondStore;
			if (licence.IsAllowed)
			{
				var result = HouseBill.ReleaseFromBondStore();
				Globals.Message.ShowInformation("Releasing Cleared Consignment from Bond Store completed.");
				if (result)
				{
					MainForm.FireSaveButton();
				}
			}
			else
			{
				licence.ShowError();
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			AddExtraMenus();
			if (HouseBill != null)
			{
				SetMenuVisibility();
			}
		}

		void AddExtraMenus()
		{
			if (HouseBill != null && !HouseBill.IsDeleted && HouseBill.Shipment != null)
			{
				if (!MenuItems.Contains(RefreshAirCargoDataMenuItem))
				{
					MenuItems.Add(RefreshAirCargoDataMenuItem);
					RefreshAirCargoDataMenuItem.Click += new EventHandler(RefreshAirCargoDataMenuItem_Click);
				}
			}
		}

		#endregion

		#region Click Handlers

		void CreateContingencyMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (HouseBill != null)
				{
					new CMRExportForm(MainForm, new AirReportHAWBExporter(HouseBill)).Export();
				}
				else
				{
					throw new AirCargoException(NullHouseBillError);
				}
			}
			catch (AirCargoException ex)
			{
				Globals.Message.Show(ex.Message, "AirCargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void RefreshAirCargoDataMenuItem_Click(object sender, EventArgs e)
		{
			string error = null;
			if (HouseBill != null && HouseBill.MAWB != null)
			{
				if (!HouseBill.CS_IsResponsePending)
				{
					HouseBill.SynchroniseData();
					HouseBill.MAWB.SynchroniseData();

					var info = !HouseBill.HasChanges ? "No change has been made."
							 : HouseBill.MAWB.ReadOnly ? "AirCargo Data has been updated except for the master details. Please click Save to commit this change."
							 : "AirCargo Data has been updated. Please click Save to commit this change.";
					Globals.Message.ShowInformation(info, "AirCargo Data");
				}
				else
				{
					error = "Response has not come back from the Customs yet. You can't perform this action without the Customs response.";
				}
			}
			else
			{
				error = NullHouseBillError;
			}

			if (error != null)
			{
				Globals.Message.Show(error, "AirCargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		protected override bool SendMessagesClickCore(object sender)
		{
			var shipment = HouseBill?.Shipment;
			if (shipment != null && shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
			{
				Globals.Message.ShowWarning(Res.GetString("fa8b6113-44d4-42b4-8fbf-0354696fc182", "In order to create the AirCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV AirCargo Report"));
			}

			return base.SendMessagesClickCore(sender);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (RefreshAirCargoDataMenuItem != null)
			{
				RefreshAirCargoDataMenuItem.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		CusHAWB HouseBill
		{
			get { return Manager.HAWB; }
		}

		protected CusHAWBMessageManager Manager
		{
			get { return (CusHAWBMessageManager)manager; }
		}

		#endregion
	}
}
