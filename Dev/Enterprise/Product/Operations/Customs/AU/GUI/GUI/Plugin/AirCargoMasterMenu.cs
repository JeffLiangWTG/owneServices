using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoMasterMenu : CMRMessageManagementMenu
	{
		#region Construction

		protected internal AirCargoMasterMenu(CusMAWB masterBill, CusMAWBMessageManager manager)
			: base(manager)
		{
			this.Text = "&Air Cargo";
			this.fMasterBill = masterBill;
		}

		protected internal AirCargoMasterMenu(ForwardingConsol consol, CusMAWBMessageManager manager)
			: base(manager)
		{
			this.Text = "&Air Cargo";
			this.consol = consol;
		}

		#endregion

		#region Factory Methods

		protected delegate AirCargoMasterMenu NewMAWBDelegate(CusMAWB masterBill, CusMAWBMessageManager manager);
		protected static readonly Overridable<NewMAWBDelegate> OverridableNewMAWBDelegate = new Overridable<NewMAWBDelegate>();

		public static AirCargoMasterMenu New(CusMAWB masterBill, CusMAWBMessageManager manager)
		{
			AirCargoMasterMenu result;
			var overridden = OverridableNewMAWBDelegate.Value;
			if (overridden == null)
			{
				result = new AirCargoMasterMenu(masterBill, manager);
			}
			else
			{
				result = overridden(masterBill, manager);
			}

			return result;
		}

		protected delegate AirCargoMasterMenu NewConsolDelegate(ForwardingConsol consol, CusMAWBMessageManager manager);
		protected static readonly Overridable<NewConsolDelegate> OverridableNewConsolDelegate = new Overridable<NewConsolDelegate>();

		public static AirCargoMasterMenu New(ForwardingConsol consol, CusMAWBMessageManager manager)
		{
			AirCargoMasterMenu result;
			var overridden = OverridableNewConsolDelegate.Value;
			if (overridden == null)
			{
				result = new AirCargoMasterMenu(consol, manager);
			}
			else
			{
				result = overridden(consol, manager);
			}

			return result;
		}

		#endregion

		#region InitializeMenu / OnPopup

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			if (consol != null)
			{
				SetupMenuItems();
			}
			else if (CurrentCheckPoint.IsAllowed)
			{
				SetupMenuItems();
				houseMenu = AirCargoShipmentMenu.New(new CusHAWBMessageManager(() => MasterBill.CurrentHouseBill));
				MenuItems.Add(houseMenu);
			}
			else
			{
				ConfigureMenuForLicenceOrSecurityDenied();
			}
		}

		internal void OnPopupInternal(EventArgs e) => OnPopup(e);

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			SetMenuVisibility();
			if (houseMenu != null)
			{
				if (MasterBill != null && MasterBill.CurrentHouseBill != null && !MasterBill.CurrentHouseBill.IsDeleted)
				{
					houseMenu.Text = "House Bill:" + MasterBill.CurrentHouseBill.CS_HAWB;
				}
				else
				{
					houseMenu.Text = "HAWB Level";
				}
			}
		}

		void SetupMenuItems()
		{
			scheduleOriginalSendingMenuItem = new ZMenuItem("Schedule Out-of-Hours Original Message Sending", ScheduleOriginalSendingMenuItem_Click);
			airCargoReportContingencyMenuItem = new ZMenuItem("&Create Contingency Data", AirCargoReportContingencyMenuItem_Click);
			refreshAirCargoDataMenuItem = new ZMenuItem("Refresh &AirCargo Data", RefreshAirCargoDataMenuItem_Click);

			MenuItems.Add(scheduleOriginalSendingMenuItem);
			MenuItems.Add(airCargoReportContingencyMenuItem);
			MenuItems.Add(refreshAirCargoDataMenuItem);
		}

		void ScheduleOriginalSending()
		{
			if (MasterBill != null)
			{
				if (MasterBill.HasDeferredScheduledMessageLog(CusMAWBBase.AirCargoReportLogReference) && Globals.Message.Show(
							string.Format("Deferred sending of messages has already been scheduled ({0})\r\nDo you wish to reschedule them?", MasterBill.DeferredScheduledDateForDisplayInCanberraTime),
							"Already Scheduled", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
				{
					return;
				}

				var notifications = Customs.Business.MessageSendingValidation.New(consol == null ? MasterBill : consol, null).CheckBusinessObjectLevelValidation();
				if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
				{
					Globals.Message.Show("Please enter a Customs Registration Number before sending CMR messages", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (notifications.ErrorCount > 0)
				{
					Globals.Message.Show("There are errors.\r\n\r\n" + notifications.NotificationsAsString(10) + "\r\nPlease fix these errors before scheduling original message sending", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (GetConfirmation(notifications.NotificationsAsString(10)) == DialogResult.No)
				{
					return;
				}

				MasterBill.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
				try
				{
					MasterBill.Factory.Save();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(e);
				}

				Globals.Message.Show(string.Format("Message sending scheduled {0}.", MasterBill.DeferredScheduledMessagesDateTimeString), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			return;
		}

		DialogResult GetConfirmation(string warningMessage)
		{
			bool hasMessageErrors = !string.IsNullOrEmpty(warningMessage);
			string caption = hasMessageErrors ? "Message Errors" : "Message Sending";
			string displaytext = hasMessageErrors ? "There are message errors.\r\n\r\n" + warningMessage + "\r\n\r\n" : string.Empty;
			displaytext += MasterBill.ScheduledMessagesConfirmationText;
			if (displaytext.StartsWith("Original cargo reports are within the late cargo reporting time frame"))
			{
				displaytext += "\r\n\r\nDo you wish to continue with sending these messages NOW?";
			}
			else
			{
				displaytext += "\r\n\r\nDo you wish to continue with the background scheduling of messages?";
			}

			return Globals.Message.Show(displaytext, caption, MessageBoxButtons.YesNo, DialogResult.No);
		}

		#endregion

		#region Click Handlers

		protected override bool SendMessagesClickCore(object sender)
		{
			bool result = false;
			if (MasterBill == null)
			{
				result = base.SendMessagesClickCore(sender);
			}
			else
			{
				var mutex = GetMutex();
				if (mutex != null && mutex.IsLocked)
				{
					DisplayLockedMessage(mutex.GetLockInfo());
					return result;
				}

				if (AUCustomsDataRegistry.Instance.SendAirCargoMessagesInBackGroundDefault.Value)
				{
					if (Globals.Message.Show("Do you wish to schedule the generation and sending of cargo reports for this job in the background?", "Schedule Message Sending", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
					{
						ScheduleOriginalSending();
						return result;
					}
				}

				result = base.SendMessagesClickCore(sender);
				if (result)
				{
					if (MasterBill.HasDeferredScheduledMessageLog())
					{
						MasterBill.CancelDeferredScheduledMessageLogs();
						SaveFactory();
					}
				}
			}

			return result;
		}

		protected override ZArchitecture.Data.Mutex.ZGlobalMutex GetMutex()
		{
			return MasterBill != null ? MasterBill.SendAIRCRMutex : null;
		}

		void AirCargoReportContingencyMenuItem_Click(object sender, EventArgs e)
		{
			if (MasterBill != null)
			{
				new CMRExportForm(MainForm, new AirReportMAWBExporter(MasterBill)).Export();
			}
		}

		void RefreshAirCargoDataMenuItem_Click(object sender, EventArgs e)
		{
			if (MasterBill != null)
			{
				bool changeMade = false;
				foreach (var bill in MasterBill.ChildBills.Cast<CusHAWB>().Where(bill => !bill.CS_IsResponsePending))
				{
					bill.SynchroniseData();
					changeMade |= bill.HasChanges;
				}

				MasterBill.SynchroniseData();

				var message = !changeMade ? "No change has been made."
							: MasterBill.ReadOnly ? "AirCargo Data has been updated except for the master details. Please click Save to commit this change."
							: "AirCargo Data has been updated. Please click Save to commit this change.";
				Globals.Message.ShowInformation(message, "AirCargo Data");
			}
			else
			{
				Globals.Message.Show("Please create a master bill first.", "AirCargo Automation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		void ScheduleOriginalSendingMenuItem_Click(object sender, EventArgs e)
		{
			if (MasterBill == null)
			{
				Globals.Message.Show("The Air Cargo job has NOT been created. Please click on the Air Cargo tab before scheduling message sending.", "No Air Cargo", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				var mutex = GetMutex();
				if (mutex != null && mutex.IsLocked)
				{
					DisplayLockedMessage(mutex.GetLockInfo());
					return;
				}

				ScheduleOriginalSending();
			}
		}

		#endregion // Click Handlers

		#region MasterBill

		protected CusMAWB MasterBill
		{
			get
			{
				if (fMasterBill == null)
				{
					fMasterBill = Manager.MAWB;
				}

				return fMasterBill;
			}
		}
		CusMAWB fMasterBill;

		#endregion

		#region Visibility

		void SetMenuVisibility()
		{
			sendMessages.Visible = CurrentCheckPoint.IsAllowed;
			withdrawMessages.Visible = CurrentCheckPoint.IsAllowed;
			resetToOriginal.Visible = CurrentCheckPoint.IsAllowed;
			scheduleOriginalSendingMenuItem.Visible = CurrentCheckPoint.IsAllowed;
			airCargoReportContingencyMenuItem.Visible = CurrentCheckPoint.IsAllowed;
			refreshAirCargoDataMenuItem.Visible = MasterBill != null && MasterBill.Consol != null;
		}

		#endregion

		#region Security

		SecurityCheckpoint CurrentCheckPoint
		{
			get { return Env.Security.ACASendMessage; }
		}

		void ConfigureMenuForLicenceOrSecurityDenied()
		{
			MenuItems.Clear();
			if (accessDeniedMenuItem == null)
			{
				accessDeniedMenuItem = new ZMenuItem("Access Denied, click this menu for detail.", new EventHandler(ShowLicenceOrSecurityError));
			}

			MenuItems.Add(accessDeniedMenuItem);
		}
		MenuItem accessDeniedMenuItem;

		void ShowLicenceOrSecurityError(object sender, EventArgs e)
		{
			CurrentCheckPoint.ShowError();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (houseMenu != null)
			{
				houseMenu.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		readonly ForwardingConsol consol;
		internal MenuItem airCargoReportContingencyMenuItem;
		internal MenuItem scheduleOriginalSendingMenuItem;
		internal MenuItem refreshAirCargoDataMenuItem;
		protected internal AirCargoShipmentMenu houseMenu;

		CusMAWBMessageManager Manager => (CusMAWBMessageManager)manager;

		protected override bool IsMessagingSuppressed
		{
			get
			{
				var consolForm = ParentForm as Freight.Forwarding.GUI.ConsolForm;
				return consolForm != null && consolForm.IsAnyShipmentOpenForEdit;
			}
		}

		protected override ZString ReasonMessagingIsSuppressed
		{
			get { return SeaCargo.GUI.SeaCargoConsolMenu.ReasonMessagingIsSuppressedText; }
		}

		internal ZForm ParentForm { private get; set; }

		#endregion
	}
}
