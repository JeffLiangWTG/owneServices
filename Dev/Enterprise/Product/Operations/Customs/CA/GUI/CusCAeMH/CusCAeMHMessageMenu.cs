using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class CusCAeMHMessageMenu : MessageManagementMenu
	{
		public CusCAeMHMessageMenu(ACIHouseBillMultiMessageManager messageManager)
			: base(messageManager)
		{
		}

		protected CusCAeMHMaster Master
		{
			get { return Manager.Master; }
		}

		protected ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		MenuItem sendCloseMessages;

		protected override void InitializeMenu()
		{
			base.InitializeMenu();

			sendCloseMessages = new ZMenuItem(ResString.GetMultilingualString("480254E8-83CC-4582-8392-A618BCEB0543", "Send &Close Message(s)"));
			sendCloseMessages.Click += new EventHandler(SendCloseMessages_Click);
			var posOfSend = MenuItems.IndexOf(sendMessages);
			MenuItems.Add(posOfSend > -1 ? ++posOfSend : 0, sendCloseMessages);

			var forcedMessagesMenu = new ZMenuItem(ResString.GetMultilingualString("39442966-D692-42A6-890F-80F92C71C2CF", "Forced Messages"));
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("D3933229-EE1E-4D9E-A130-84F3A45FA1C6", "Send Original Messages"), SendOriginalMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("C5592E16-4673-4A01-B6D5-1BD157338CD0", "Send Change Messages"), SendChangeMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("4705F04B-CA22-49B3-B5FA-EFE1E59FF99A", "Send Post-arrival Change Messages"), SendChangeRequestMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("A4D2E9B7-F10C-451E-A00F-36E3B74D3ECC", "Send Original Close Messages"), SendOriginalCloseMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("EFD14207-D535-49C6-9449-30CFDA69DAA2", "Send Change Close Messages"), SendChangeCloseMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("F3523C6F-9DC1-47B1-B4B2-FC7523BEB7CF", "Send Post Arrival Change Close Messages"), SendPostArrivalChangeCloseMessages_Click);
			forcedMessagesMenu.MenuItems.Add(ResString.GetMultilingualString("B169F0E0-FB9C-46B9-A683-2AEDDDE11181", "Withdraw Close Messages"), WithdrawCloseMessages_Click);
			MenuItems.Add(forcedMessagesMenu);

			if (Master != null && Master.Consol != null)
			{
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("47F7F773-88D7-4C28-BE10-E53520C610BA", "Refresh eManifest Data"), RefreshAll_Click));
			}
		}

		void SendCloseMessages_Click(object sender, EventArgs e)
		{
			if (IsMessagingAllowed && CheckErrorAndFireSaveButtonIfNeeded() && Manager.SendCloseMessages(SenderWithoutChooser))
			{
				SaveFactory();
			}
		}

		ISendsMessagesToCustoms SenderWithoutChooser
		{
			get { return fSenderWithoutChooser ?? (fSenderWithoutChooser = new SendsMessagesToCustomsGUI()); }
		}
		ISendsMessagesToCustoms fSenderWithoutChooser;

		void RefreshAll_Click(object sender, EventArgs e)
		{
			var master = Master;
			var consol = master.Consol;
			if (master != null && !master.IsDeleted && consol != null && !consol.IsDeleted)
			{
				Manager.RefreshAll(MessageAction);
			}
		}

		MessagingActionsController MessageAction
		{
			get { return fMessageAction ?? (fMessageAction = new MessagingActionsController()); }
		}
		MessagingActionsController fMessageAction;

		void SendOriginalMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Create;
				SendForcedOriginalMessages(sender);
			}
		}

		void SendChangeMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Change;
				SendForcedOriginalMessages(sender);
			}
		}

		void SendChangeRequestMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Request;
				SendForcedOriginalMessages(sender);
			}
		}

		void SendForcedOriginalMessages(object sender)
		{
			SendForcedMessages(sender, SendMessagesClickCore);
		}

		void SendOriginalCloseMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Create;
				SendForcedCloseMessages(sender);
			}
		}

		void SendChangeCloseMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Change;
				SendForcedCloseMessages(sender);
			}
		}

		void SendPostArrivalChangeCloseMessages_Click(object sender, EventArgs eventArgs)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded())
			{
				forcedMessageSubType = MessageSubTypes.Request;
				SendForcedAmendCloseMessages(sender);
			}
		}

		void SendForcedCloseMessages(object sender)
		{
			SendForcedMessages(sender, o =>
			{
				Manager.SendCloseMessages(Sender, true);
				SaveFactory();
				return true;
			});
		}

		void SendForcedAmendCloseMessages(object sender)
		{
			SendForcedMessages(sender, o =>
			{
				Manager.SendCloseMessages(Sender, true, true);
				SaveFactory();
				return true;
			});
		}

		void WithdrawCloseMessages_Click(object sender, EventArgs e)
		{
			if (CheckErrorAndFireSaveButtonIfNeeded() && Manager.WithdrawCloseMessages(SenderWithoutChooser))
			{
				SaveFactory();
			}
		}

		protected ACIHouseBillMultiMessageManager Manager
		{
			get { return (ACIHouseBillMultiMessageManager)base.manager; }
		}

		void SendForcedMessages(object sender, Func<object, bool> sendMessages)
		{
			try
			{
				Manager.OnMessagesSendingEvent += OnMessagesSending;
				sendMessages(sender);
			}
			finally
			{
				Manager.OnMessagesSendingEvent -= OnMessagesSending;
				if (releaseSingleManagerHooks != null)
				{
					releaseSingleManagerHooks(this, EventArgs.Empty);
				}

				// TODO:  how do I dispose/clear releaseSingleManagerHooks at this point?
			}
		}
		event EventHandler releaseSingleManagerHooks;

		void OnMessagesSending(SingleMessageManager[] singleMessageManagers)
		{
			foreach (CAEManifestForwarderMessageManager singleManager in singleMessageManagers)
			{
				singleManager.DefineActionCodeEvent += DefineActionCodeEvent;
				releaseSingleManagerHooks += delegate
				{ singleManager.DefineActionCodeEvent -= DefineActionCodeEvent; };
			}
		}

		void DefineActionCodeEvent(ref MessageSubTypes actionCode)
		{
			actionCode = forcedMessageSubType;
		}

		MessageSubTypes forcedMessageSubType;

		protected override ISendsMessagesToCustoms Sender
		{
			get { return new CASendsMessagesToCustomsGUI(); }
		}

		// TODO: CLOSE message funcionality to be added (not in this current check-in)
	}
}
