using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoConsolMenu : CMRMessageManagementMenu
	{
		public SeaCargoConsolMenu(SeaCargoConsolPlugIn plugin, CusSCAOceanBillMessageManager manager) : base(manager)
		{
			this.plugin = plugin;
		}

		#region InitializeMenu / HookClickEvents

		protected override void InitializeMenu()
		{
			menuItemSendCMRUnderbondRequests = new ZMenuItem("Send &Underbond Requests");
			MenuItems.Add(menuItemSendCMRUnderbondRequests);

			base.InitializeMenu();

			scheduleOriginalSendingMenuItem = new ZMenuItem("Schedule Out-of-Hours Original Message Sending", ScheduleOriginalSendingMenuItem_Click);
			MenuItems.Add(scheduleOriginalSendingMenuItem);

			menuItemRefreshSeaCargoData = new ZMenuItem("&Refresh Sea Cargo Data");
			MenuItems.Add(menuItemRefreshSeaCargoData);

			seaCargoReportContingencyMenuItem = new ZMenuItem("&Create Contingency Data");
			seaCargoReportContingencyMenuItem.Click += new EventHandler(SeaCargoReportContingencyMenuItem_Click);
			MenuItems.Add(seaCargoReportContingencyMenuItem);

			exportSeaCargoContainersDataMenuItem = new ZMenuItem("&Export Sea Cargo Containers Data");
			MenuItems.Add(exportSeaCargoContainersDataMenuItem);
		}

		protected override void HookClickEvents()
		{
			base.HookClickEvents();
			menuItemRefreshSeaCargoData.Click += new EventHandler(MenuItemRefreshSeaCargoData_Click);
			menuItemSendCMRUnderbondRequests.Click += new EventHandler(MenuItemSendCMRUnderbondRequests_Click);
			exportSeaCargoContainersDataMenuItem.Click += new EventHandler(ExportSeaCargoContainersDataMenuItem_Click);
		}

		#endregion

		#region Click Handlers

		void SeaCargoReportContingencyMenuItem_Click(object sender, EventArgs e)
		{
			if (plugin.OceanBill != null)
			{
				new CMRExportForm(MainForm, new CusSCAOceanBillExporter(plugin.OceanBill)).Export();
			}
		}

		void ExportSeaCargoContainersDataMenuItem_Click(object sender, EventArgs e)
		{
			if (plugin.OceanBill != null)
			{
				if (plugin.OceanBill.HasChanges)
				{
					var mainMenu = this.GetMainMenu();
					var form = mainMenu != null ? mainMenu.GetForm() as ZForm : null;
					if (Globals.Message.Show(Declaration.GUI.Res.GetString("CEA20709-A070-47DD-BC0E-2FF156C4C763", "There are changes on this form. Do you want to save changes first?"), Declaration.GUI.Res.GetString("0D6FD809-AD04-446A-8883-FFB66FE16DEE", "Save"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
					{
						if (form.FireSaveButton() == ContinueWithSave.Yes)
						{
							ExportSeaCargoContainerData();
						}
					}
				}
				else
				{
					ExportSeaCargoContainerData();
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ExportSeaCargoContainerData()
		{
			if (plugin.OceanBill != null)
			{
				string exportFileName = String.Format(CultureInfo.InvariantCulture, "ExportOceanBill_{0}_{1}.csv", plugin.OceanBill.CB_OceanBill, ZDateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture));

				using (var saveFileDialog = new ZFolderBrowserDialog())
				{
					saveFileDialog.CreateDirectory = false;
					saveFileDialog.RequireMappablePath = true;
					saveFileDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
					saveFileDialog.ShowNewFolderButton = true;
					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						try
						{
							var exporter = new SeaCargoContainersExporter(plugin.OceanBill);
							exporter.Generate();
							string exportFilePath = Path.Combine(saveFileDialog.UnmappedSelectedPath, exportFileName);
							using (var stream = exporter.GetStream(exportFilePath))
							{
								if (stream != Stream.Null)
								{
									exporter.SaveToFile(stream);
									Globals.Message.ShowInformation("The data has been exported to  " + exportFilePath);
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Globals.Message.ShowError(ex.Message);
						}
					}
				}
			}
		}

		void MenuItemRefreshSeaCargoData_Click(object sender, EventArgs e)
		{
			if (!IsMessagingSuppressed)
			{
				Cursor oldCursor = Cursor.Current;
				try
				{
					if (plugin.OceanBill == null)
					{
						// Create the Ocean Bill by invoking the Tab Page.
						plugin.SelectTabPage();
					}
					if (plugin.OceanBill != null)
					{
						Cursor.Current = Cursors.WaitCursor;
						if (plugin.SeaCargoInfo != null && plugin.SeaCargoInfo.SeaCargoSynchroniser != null)
						{
							if (plugin.SeaCargoInfo.SeaCargoSynchroniser.OceanBill == plugin.OceanBill)
							{
								if (plugin.SeaCargoInfo.CanSynchronise || RunSynchroniseForRemaining)
								{
									plugin.SeaCargoInfo.SeaCargoSynchroniser.SynchroniseOceanBill();
								}
							}
						}
					}
				}
				finally
				{
					Cursor.Current = oldCursor;
				}
			}
			else
			{
				Globals.Message.ShowError("One or more shipments of this consol are open on other forms. Please close these shipment forms before Synchronizing.", Declaration.GUI.Res.GetString("B0F767DE-1998-42CB-8537-EDBDAC982642", "Synchronizing not allowed"));
			}
		}

		bool RunSynchroniseForRemaining
		{
			get
			{
				if (plugin.SeaCargoInfo.NoSynchronisingWillOccur)
				{
					Globals.Message.ShowWarning(plugin.SeaCargoInfo.SynchroniseFailureMessage + "\r\nNo House Bills are suitable for synchronising.");
					return false;
				}
				else
				{
					return Globals.Message.Show(plugin.SeaCargoInfo.SynchroniseFailureMessage + "\r\nSynchronise remaining House Bills?", "Continue", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
				}
			}
		}

		void MenuItemSendCMRUnderbondRequests_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed)
			{
				Cursor oldCursor = Cursor.Current;
				try
				{
					if (plugin.OceanBill == null)
					{
						plugin.SelectTabPage();
					}
					if (plugin.OceanBill != null)
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
				}
				finally
				{
					Cursor.Current = oldCursor;
				}
			}
		}

		protected void ScheduleOriginalSendingMenuItem_Click(object sender, EventArgs e)
		{
			if (Manager != null)
			{
				var mutex = GetMutex();
				if (mutex != null && mutex.IsLocked)
				{
					DisplayLockedMessage(mutex.GetLockInfo());
					return;
				}

				SeaCargoMenuHelper.ScheduleOriginalSending(plugin.OceanBill, plugin.Consol);
			}
			else
			{
				Globals.Message.Show("The Sea Cargo job has NOT been created. Please click on the Sea Cargo tab before scheduling message sending.", "No Sea Cargo", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region Implementation

		internal MenuItem menuItemRefreshSeaCargoData;
		internal MenuItem menuItemSendCMRUnderbondRequests;
		MenuItem scheduleOriginalSendingMenuItem;
		internal MenuItem seaCargoReportContingencyMenuItem;
		internal MenuItem exportSeaCargoContainersDataMenuItem;
		readonly SeaCargoConsolPlugIn plugin;

		CusSCAOceanBillMessageManager Manager
		{
			get { return (CusSCAOceanBillMessageManager)manager; }
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			bool isCMR = Manager.OceanBill != null && Manager.OceanBill is CusSCAOceanBill;
			SetVisibility(isCMR);
		}

		void SetVisibility(bool isCMR)
		{
			sendMessages.Visible = isCMR;
			withdrawMessages.Visible = isCMR;
			resetToOriginal.Visible = isCMR;
			menuItemSendCMRUnderbondRequests.Visible = isCMR;

			menuItemRefreshSeaCargoData.Visible = true;
		}

		protected override bool IsMessagingSuppressed
		{
			get
			{
				Freight.Forwarding.GUI.ConsolForm consolForm = ParentForm as Freight.Forwarding.GUI.ConsolForm;
				return consolForm != null && consolForm.IsAnyShipmentOpenForEdit;
			}
		}

		protected override ZString ReasonMessagingIsSuppressed
		{
			get { return ReasonMessagingIsSuppressedText; }
		}
		internal const string ReasonMessagingIsSuppressedText = "You have one or more shipments of this consol open on other forms. Please close these shipment forms before doing any customs messaging.";

		internal ZForm ParentForm { private get; set; }

		protected override ZArchitecture.Data.Mutex.ZGlobalMutex GetMutex()
		{
			return plugin.OceanBill != null ? plugin.OceanBill.SendSEACRMutex : null;
		}

		#endregion
	}
}
