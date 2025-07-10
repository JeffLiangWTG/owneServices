using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	[SuppressFormsLocalizedTest]
	public class SeaCargoShipmentMenu : CMRMessageManagementMenu
	{
		#region Constants

		const string RefreshSeaCargoDataMenuItemString = "&Refresh SeaCargo Data";
		const string MessagingManagementMenuItemString = "&Messaging Admin";
		const string AcceptCurrentAsAcknowledgedMenuItemString = "&Accept Current as Acknowledged";
		const string UserChangeCustomsStatusWarning = @"In manually changing the Sea Cargo Status you must have authority to do so from Customs and your companys management. Consider the reasons and consequence of overriding the status or release of customs controlled cargo they are important and have consequences. Changing the status manually as incorrect or inappropriate changes to customs status could result in fines and or other civil or criminal penalties.

All status changes are logged with your name, time, and action taken.

Please enter a reason (must be more that 20 characters long):";

		#endregion

		public SeaCargoShipmentMenu(SeaCargoShipmentPlugIn plugin, CusSCAHouseMessageManager manager)
			: base(manager)
		{
			this.Text = "Sea Ca&rgo";
			this.plugin = plugin;
		}

		#region Click Handlers

		void SeaCargoReportContingencyMenuItem_Click(object sender, EventArgs e)
		{
			if (plugin.OceanBill != null)
			{
				new CMRExportForm(MainForm, new CusSCAOceanBillExporter(plugin.OceanBill)).Export();
			}
		}

		void RefreshSeaCargoDataMenuItem_Click(object sender, EventArgs e)
		{
			Cursor oldCursor = Cursor.Current;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (plugin.SeaCargoShipmentInfo.CanSynchronise || RunSynchroniseForRemaining)
				{
					plugin.SeaCargoShipmentInfo.SynchroniseIfWeCan();
				}
			}
			finally
			{
				Cursor.Current = oldCursor;
			}
		}

		bool RunSynchroniseForRemaining
		{
			get
			{
				if (plugin.SeaCargoShipmentInfo.NoSynchronisingWillOccur)
				{
					Globals.Message.ShowWarning(plugin.SeaCargoShipmentInfo.SynchroniseFailureMessage + "\r\nNo House Bills are suitable for synchronising.");
					return false;
				}
				else
				{
					return Globals.Message.Show(plugin.SeaCargoShipmentInfo.SynchroniseFailureMessage + "\r\nSynchronise remaining House Bills?", "Continue", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
				}
			}
		}

		internal void AcceptCurrentAsAcknowledgedMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult resetSeaCargoJobDialogResult = Globals.Message.ShowConfirmation("Are you sure you want to mark the current state as accepted?", "Continue?", "YES", MessageBoxIcon.Exclamation);
			if (resetSeaCargoJobDialogResult == DialogResult.OK)
			{
				if (plugin.HouseBill != null)
				{
					plugin.HouseBill.RunPreSaveValidation();
					if (plugin.HouseBill.HasMessageErrors || plugin.HouseBill.HasErrors)
					{
						Globals.Message.ShowError(plugin.HouseBill.NotificationsIncludingChildren.GetErrors() + "\r\n" + plugin.HouseBill.NotificationsIncludingChildren.GetMessageErrors());
					}
					else
					{
						string queryUserResponse = Globals.Message.QueryUserResponse(UserChangeCustomsStatusWarning, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
						if (queryUserResponse.Length > 20)
						{
							if (plugin.HouseBill != null)
							{
								plugin.HouseBill.AcceptCurrentAsAcknowledged(queryUserResponse);
							}
						}
					}
				}
			}
		}

		internal void MenuItemPreAlertCMRHouseBill_Click(object sender, EventArgs e)
		{
			Cursor oldCursor = Cursor.Current;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (Manager != null)
				{
					SendOriginalCargoReportMessageAction action = new SendOriginalCargoReportMessageAction();
					if (Manager.SendOriginalMessages(action).Any())
					{
						Globals.Message.Show(action.LastMessage);
						try
						{
							Manager.Factory.Save();
						}
						catch (ZSaveException e1)
						{
							ZExceptionReporting.HandleSaveException(e1);
						}
					}
					else
					{
						if (!action.InvalidOperationText.IsEmpty)
						{
							Globals.Message.ShowError(action.InvalidOperationText, "Pre-Alert Consol Error");
						}
						else
						{
							Globals.Message.Show("No new messages generated.");
						}
					}
				}
			}
			finally
			{
				Cursor.Current = oldCursor;
			}
		}

		internal void MenuItemSendCMRUnderbondRequests_Click(object sender, EventArgs e)
		{
			Cursor oldCursor = Cursor.Current;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (Manager != null)
				{
					SendOrignalUnderbondMessageAction action = new SendOrignalUnderbondMessageAction();
					if (Manager.SendOriginalMessages(action).Any())
					{
						Globals.Message.Show(action.LastMessage);
						try
						{
							Manager.Factory.Save();
						}
						catch (ZSaveException e1)
						{
							ZExceptionReporting.HandleSaveException(e1);
						}
					}
					else
					{
						if (!action.InvalidOperationText.IsEmpty)
						{
							Globals.Message.ShowError(action.InvalidOperationText, "Underbond Request Error");
						}
						else
						{
							Globals.Message.Show("No new messages generated.");
						}
					}
				}
			}
			finally
			{
				Cursor.Current = oldCursor;
			}
		}

		protected override bool SendMessagesClickCore(object sender)
		{
			var shipmentType = Manager.HouseBill.Shipment.JS_ShipmentType;
			if (shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
			{
				Globals.Message.ShowWarning(Declaration.GUI.Res.GetString("a1bae91d-874a-49af-866a-9ac246f8aa81", "In order to create the SeaCargo Report for the HVLV House Bill’s, go to HVLV > Create HVLV SeaCargo Report"));
			}

			return base.SendMessagesClickCore(sender);
		}

		#endregion

		#region Implementation

		protected override void InitializeMenu()
		{
			menuItemPreAlertCMRHouseBill = new ZMenuItem("&Pre-Alert House");
			menuItemPreAlertCMRHouseBill.Click += new EventHandler(MenuItemPreAlertCMRHouseBill_Click);
			MenuItems.Add(menuItemPreAlertCMRHouseBill);

			menuItemSendCMRUnderbondRequests = new ZMenuItem("Send &Underbond Requests");
			menuItemSendCMRUnderbondRequests.Click += new EventHandler(MenuItemSendCMRUnderbondRequests_Click);
			MenuItems.Add(menuItemSendCMRUnderbondRequests);

			base.InitializeMenu();

			refreshSeaCargoDataMenuItem = new ZMenuItem(RefreshSeaCargoDataMenuItemString);
			refreshSeaCargoDataMenuItem.Click += new EventHandler(RefreshSeaCargoDataMenuItem_Click);

			this.MenuItems.Add("-");
			this.MenuItems.Add(refreshSeaCargoDataMenuItem);
			if (GlbStaff.CurrentUser.GS_IsController || Env.Security.AUCustomsSCAImportMessagingAdmin.IsAllowed)
			{
				messagingManagementMenuItem = new ZMenuItem(MessagingManagementMenuItemString);
				acceptCurrentAsAcknowledgedMenuItem = new ZMenuItem(AcceptCurrentAsAcknowledgedMenuItemString);
				acceptCurrentAsAcknowledgedMenuItem.Click += new EventHandler(AcceptCurrentAsAcknowledgedMenuItem_Click);
				this.MenuItems.Add("-");
				this.MenuItems.Add(messagingManagementMenuItem);
				messagingManagementMenuItem.MenuItems.Add(acceptCurrentAsAcknowledgedMenuItem);
			}

			seaCargoReportContingencyMenuItem = new ZMenuItem("&Create Contingency Data");
			seaCargoReportContingencyMenuItem.Click += new EventHandler(SeaCargoReportContingencyMenuItem_Click);
			MenuItems.Add(seaCargoReportContingencyMenuItem);
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			ChangeVisibility();
		}

		void ChangeVisibility()
		{
			var isCMR = Manager.OceanBill is CusSCAOceanBill;
			foreach (MenuItem menuItem in MenuItems)
			{
				menuItem.Enabled = isCMR;
			}
		}

		CusSCAHouseMessageManager Manager
		{
			get { return (CusSCAHouseMessageManager)manager; }
		}

		readonly SeaCargoShipmentPlugIn plugin;
		MenuItem refreshSeaCargoDataMenuItem;
		MenuItem messagingManagementMenuItem;
		MenuItem acceptCurrentAsAcknowledgedMenuItem;
		MenuItem menuItemPreAlertCMRHouseBill;
		MenuItem menuItemSendCMRUnderbondRequests;
		internal MenuItem seaCargoReportContingencyMenuItem;

		#endregion
	}
}
