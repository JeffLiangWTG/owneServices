using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.GUI;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;
using JobDeclarationUniversalMessagingHelper = Enterprise.Customs.DataTransfer.Universal.JobDeclarationUniversalMessagingHelper;
using MessageSending = Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.GUI
{
	public class EDIMenu : EU.GUI.EDIMenu
	{
		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = Declaration;
			GenerateEntriesMenuItem.Visible = declaration != null && !declaration.ShowSubmitMenuItem;
			sendCINMessageMenuItem.Visible = declaration != null && declaration.IsAir && declaration.IsExport;
			CreateEntryRegularizationMessagingMenuItemInfos();

			sendDeltaGMessageMenuItem.Visible = declaration != null && !declaration.IsUCC6 && !declaration.ShowSubmitMenuItem;
			sendDeltaIEMessageMenuItem.Visible = declaration != null && declaration.IsUCC6 && !declaration.ShowSubmitMenuItem;

			NewEntryInstructionMenuItem.Visible = false;
		}

		protected override Customs.GUI.AmendmentSnapshotManagementMenuItemsCreator CreateNewAmendmentSnapshotManagementMenuItemsCreator() => new AmendmentSnapshotManagementMenuItemsCreator(this);

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return true; }
		}

		protected override string GenerateEntriesMenuOptionText
		{
			get { return Res.GetString("47601F45-720C-4D70-ABED-EC998F7E725B", "Generate Entries (Merge)"); }
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public JobDeclaration CurrentEntry
		{
			get { return CurrentEntry; }
			set { CurrentEntry = value; }
		}

		protected override JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			return new MessageSending.JobDeclarationUniversalMessagingHelper(wrapper);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			sendDeltaGMessageMenuItem = new ZMenuItem(LabelMenuSendDeltaG, SendDeltaGMessage_Click);
			sendDeltaGMessageMenuItem.Name = nameof(sendDeltaGMessageMenuItem);
			MenuItems.Add(sendDeltaGMessageMenuItem);

			sendDeltaIEMessageMenuItem = new ZMenuItem(LabelMenuSendDeltaIE, SendDeltaIEMessage_Click);
			sendDeltaIEMessageMenuItem.Name = nameof(sendDeltaIEMessageMenuItem);
			MenuItems.Add(sendDeltaIEMessageMenuItem);

			sendCINMessageMenuItem = new ZMenuItem(LabelMenuSendCIN);
			MenuItems.Add(sendCINMessageMenuItem);
			sendCIN745MenuItem = new ZMenuItem(LabelMenuSendCIN745, SendCIN745Message_Click);
			sendCINMessageMenuItem.MenuItems.Add(sendCIN745MenuItem);
			sendCIN755MenuItem = new ZMenuItem(LabelMenuSendCIN755, SendCIN755Message_Click);
			sendCINMessageMenuItem.MenuItems.Add(sendCIN755MenuItem);
			portMessagingMenuItem = new ZMenuItem(LabelPortMessaging);
			MenuItems.Add(portMessagingMenuItem);
		}

		ZBool IsMessagingSecurityAllowed()
		{
			if (Declaration.IsImport && !Env.Security.ImportMessaging.IsAllowed)
			{
				Env.Security.ImportMessaging.ShowError();
				return false;
			}

			if (Declaration.IsExport && !Env.Security.ExportMessaging.IsAllowed)
			{
				Env.Security.ExportMessaging.ShowError();
				return false;
			}

			return true;
		}

		void SendDeltaGMessage_Click(object sender, EventArgs e)
		{
			SendDeltaMessage();
		}

		void SendDeltaIEMessage_Click(object sender, EventArgs e)
		{
			SendDeltaMessage();
		}

		void SendCIN745Message_Click(object sender, EventArgs e)
		{
			SendCINMessage(Customs.FR.Business.EntryActionCodeList.Codes.CIN745);
		}

		void SendCIN755Message_Click(object sender, EventArgs e)
		{
			SendCINMessage(Customs.FR.Business.EntryActionCodeList.Codes.CIN755);
		}

		void ManageComplementarySubStyle(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			var entryInstruction = objectToSend.Header.EntryInstruction;
			var messageType = objectToSend.MessageType;
			if ((entryInstruction.IsPrelodgedSubstyle && messageType == Business.EntryActionCodeList.Codes.VAL)
				|| (entryInstruction.IsLodgedSubstyle && messageType == Business.EntryActionCodeList.Codes.ANT))
			{
				entryInstruction.CEI_SubStyle = CusEntryInstruction.GetComplementarySubstyle(entryInstruction.CEI_SubStyle);
			}
		}

		void SendDeltaMessage()
		{
			if (IsMessagingSecurityAllowed() && PreSaveDeclaration(Declaration))
			{
				bool continueWithSend = DeclarationHasEntry(Declaration);
				{
					if (continueWithSend)
					{
						if (Declaration.IsUCC6)
						{
							SendDeltaIEMessage();
						}
						else
						{
							SendDeltaGMessage();
						}
					}
				}
			}
		}

		void SendDeltaGMessage()
		{
			bool continueWithSend = true;

			if (Declaration.DeltaGFallbackAnnouncedButNotActive)
			{
				continueWithSend = Globals.Message.Show(Res.GetString("B2530DC8-11E6-4C89-B2FB-4E7771876F51", "Delta G fallback has been announced but you have not yet activated it in the CW1 registry.  You are advised to view this eLearning material and activate Delta G fallback as appropriate: 1BFR041"),
				Res.GetString("8114F4D4-AB61-4CE5-9369-865AFFD43952", "Continue with send?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
			}
			if (continueWithSend)
			{
				var decWrapper = new DeltaGJobDeclarationMessageSendingObjectParent(Declaration);

				using (var form = GetMessageSendingForm(decWrapper))
				{
					continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
				}

				if (continueWithSend)
				{
					var hasManualRegularisationEntry = decWrapper.SendingObjectsCollection.Cast<DeltaGJobDeclarationMessageSendingObject>().Any(x => x.ShouldSend && x.Header.IsDeltaGFallbackInactiveAndNotRegularised);
					if (hasManualRegularisationEntry)
					{
						continueWithSend = Globals.Message.Show(Res.GetString("B407D30D-3D84-4E69-9E83-F4B6E9173ED2", "There are some entries which will be regularized automatically around 22:00, are you sure you want to regularize them now?"),
							Res.GetString("F01BC320-F887-4A75-AEF1-1C07F3CDAD43", "Regularize now?"), MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
					}

					if (continueWithSend)
					{
						foreach (DeltaGJobDeclarationMessageSendingObject objectToSend in decWrapper.ObjectsToSend)
						{
							SendDeltaGMessageWithBondedWarehouse(objectToSend);
						}
					}
				}
			}
		}

		bool SendDeltaGMessage(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			var sent = true;
			if (objectToSend.ShouldSend)
			{
				var entryInstruction = objectToSend.Header.EntryInstruction;
				var oldSubStyle = ZString.Empty;
				if (entryInstruction != null)
				{
					oldSubStyle = entryInstruction.CEI_SubStyle;

					if (!objectToSend.DoNotRecalculateEntrySubstyle)
					{
						ManageComplementarySubStyle(objectToSend);
					}
				}

				var errorCollector = new EU.Business.ErrorCollector();
				string result;
				var fallbackSpecialMentionCreated = objectToSend.Header.AddFallbackSpecialMention();
				var sender = new DeltaGMessageSender(objectToSend, errorCollector);
				using (ObjectFactory.New<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
				{
					result = sender.Send();
				}

				if (errorCollector.ErrorCount == 0)
				{
					if (result == DeltaGMessageSender.MessageSendSuccessful)
					{
						Globals.Message.Show(result);
					}
				}
				else
				{
					objectToSend.Header.RollbackFallbackSpecialMention(fallbackSpecialMentionCreated);
					sent = false;
					if (entryInstruction != null)
					{
						entryInstruction.CEI_SubStyle = oldSubStyle;
					}
					Globals.Message.Show(errorCollector.GetErrorsAsString());
				}
			}
			return sent;
		}

		void SendDeltaIEMessage()
		{
			bool continueWithSend = true;

			var decWrapper = new DeltaIEJobDeclarationMessageSendingObjectParent(Declaration);

			using (var form = GetDeltaIEMessageSendingForm(decWrapper))
			{
				continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}

			if (continueWithSend)
			{
				var errorCollector = new EU.Business.ErrorCollector();
				var result = ZString.Empty;
				foreach (DeltaIEJobDeclarationMessageSendingObject objectToSend in decWrapper.SendingObjectsCollection)
				{
					var messageSender = new DeltaIEMessageSender(objectToSend, errorCollector);
					result += messageSender.Send();
				}

				if (errorCollector.ErrorCount == 0)
				{
					Globals.Message.Show(result);
				}
				else
				{
					Globals.Message.Show(errorCollector.GetErrorsAsString());
				}
			}
		}

		void SendDeltaGMessageWithBondedWarehouse(DeltaGJobDeclarationMessageSendingObject objectToSend)
		{
			var entry = objectToSend.Header;
			var isBondedWarehousingEnabled = !entry.IsBondedWarehousingDisabled;

			if (isBondedWarehousingEnabled && (objectToSend.MessageType == EntryActionCodeList.Codes.ANT || objectToSend.MessageType == EntryActionCodeList.Codes.VAL))
			{
				Declaration.SendMessageWithBondedWarehouseAutomation(entry, entry.GetInventoryAutomationAction(), () => SendDeltaGMessage(objectToSend), MessageAction.Original);
			}
			else if (isBondedWarehousingEnabled && (objectToSend.MessageType == EntryActionCodeList.Codes.MAP || objectToSend.MessageType == EntryActionCodeList.Codes.REC))
			{
				Declaration.SendMessageWithBondedWarehouseAutomation(entry, entry.GetInventoryAutomationAction(), () => SendDeltaGMessage(objectToSend), MessageAction.Amendment);
			}
			else if (isBondedWarehousingEnabled && objectToSend.MessageType == EntryActionCodeList.Codes.INV)
			{
				Declaration.SendMessageWithBondedWarehouseAutomation(entry, entry.GetInventoryAutomationAction(), () => SendDeltaGMessage(objectToSend), MessageAction.Withdrawal);
			}
			else
			{
				SendDeltaGMessage(objectToSend);
			}
		}

		protected override BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return new EntryBondedWarehouseOperationDeterminer((CusEntryHeader)supporter);
		}

		void SendCINMessage(string messageType) => SendCINMessage(Declaration, messageType);

		protected void SendCINMessage(JobDeclaration declaration, string messageType)
		{
			if (PreSaveDeclaration(declaration))
			{
				bool continueWithSend = DeclarationHasEntry(declaration);

				if (continueWithSend)
				{
					var errorCollector = new EU.Business.ErrorCollector();
					var manager = new CINExportMessageBuilderManager(errorCollector);
					var (action, result) = manager.SendMessage(declaration, messageType);
					if (action != CINExportMessageBuilderManager.SendMessageResult.NoAction)
					{
						Globals.Message.Show(result);
					}
				}
			}
		}

		bool DeclarationHasEntry(JobDeclaration declaration)
		{
			bool result = true;
			if (declaration.CustomsEntryHeaders.Count == 0)
			{
				Globals.Message.Show(Res.GetString("359E1E13-9F6F-4D12-907E-3BCA25E7D98A", "Declaration {0} has no entry – Please generate entries before attempting to send a message.", declaration.JE_DeclarationReference));
				result = false;
			}
			return result;
		}

		protected virtual DeltaGMessageSendingForm GetMessageSendingForm(DeltaGJobDeclarationMessageSendingObjectParent decWrapper) => new DeltaGMessageSendingForm(decWrapper);
		protected virtual DeltaIEMessageSendingForm GetDeltaIEMessageSendingForm(DeltaIEJobDeclarationMessageSendingObjectParent decWrapper) => new DeltaIEMessageSendingForm(decWrapper);

		protected ZMenuItem sendDeltaGMessageMenuItem;
		public static ZString LabelMenuSendDeltaG => FRCustomsDataRegistry.DeltaGFallbackIsActive ? ResString.GetMultilingualString("A15BAF18-9C90-4F80-8154-154C859143A6", "Send Fallback")
														: ResString.GetMultilingualString("F9EEEB43-DF49-42B8-8F4F-37EDE6B50185", "Send Delta");

		protected ZMenuItem sendDeltaIEMessageMenuItem;
		public static ZString LabelMenuSendDeltaIE => ResString.GetMultilingualString("F2E1A8F7-0155-42D8-81CB-FF4BC849EFCA", "Send Delta");

		protected ZMenuItem sendCINMessageMenuItem;
		public static ZString LabelMenuSendCIN => ResString.GetMultilingualString("A9589239-BEAD-4B47-A16E-4CE03A27983A", "Send CIN");

		protected ZMenuItem sendCIN745MenuItem;
		public static ZString LabelMenuSendCIN745 => ResString.GetMultilingualString("F03F01EE-D581-43BA-8753-96F2FBAEB84E", "Send 745");

		protected ZMenuItem sendCIN755MenuItem;
		public static ZString LabelMenuSendCIN755 => ResString.GetMultilingualString("B5BD9828-1419-47BE-939B-44CE7A15B1F9", "Send 755");

		protected ZMenuItem portMessagingMenuItem;
		public static ZString LabelPortMessaging => ResString.GetMultilingualString("E9DC733B-50D2-48DD-9CBD-94DC4DC4E40F", "Port Messaging");

		public static ZString LabelMenuTRC => ResString.GetMultilingualString("36D54A82-301F-48CE-917C-3320493B8E3B", "Tracing Request(TRC) (FR)");

		void CreateEntryRegularizationMessagingMenuItemInfos()
		{
			portMessagingMenuItem.MenuItems.Clear();
			if (Declaration?.CustomsEntryHeaders.Count > 0)
			{
				portMessagingMenuItem.Visible = true;
				foreach (CusEntryHeader entry in Declaration.CustomsEntryHeaders)
				{
					MenuItem menuEntry = new ZMenuItem(entry.CH_BGMReference);
					var trcMenuItemnew = new ZMenuItem(LabelMenuTRC, SendTRCMessage_Click(entry));
					menuEntry.MenuItems.Add(trcMenuItemnew);

					trcMenuItemnew.Visible = CanSendTrc(Declaration, entry);

					menuEntry.AddFormsMenuItems(entry, ZArchitecture.Modules.ModuleIDs.Customs.EntryHeader, CreateRegularizationMessagingMenuItemInfos());
					portMessagingMenuItem.MenuItems.Add(menuEntry);
				}
			}
			else
			{
				portMessagingMenuItem.Visible = false;
			}
		}

		bool CanSendTrc(JobDeclaration declaration, CusEntryHeader entry)
		{
			var listOfCountries = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.ToList();
			return declaration.JE_TransportMode == "SEA"
				&& ((listOfCountries.Contains(declaration.JE_RL_NKPortOfArrival.SubstringSafe(0, 2)) && declaration.IsImport) || (listOfCountries.Contains(declaration.JE_RL_NKPortOfLoading.SubstringSafe(0, 2)) && declaration.IsExport))
				&& entry.Containers.Length != 0;
		}

		IEnumerable<IMenuItemInfo> CreateRegularizationMessagingMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = new ZGuid("f12cdff4-842d-4cb8-8bba-e8cb28904efd")  // PK of CAED document menu
			};
		}

		EventHandler SendTRCMessage_Click(CusEntryHeader entry)
		{
			return (s, e) =>
			{
				SendTrcMessage(entry);
			};
		}

		protected void SendTrcMessage(CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			if (PreSaveDeclaration(declaration))
			{
				var containers = entry.Containers;

				if (containers.Length == 1)
				{
					var confirmationDialogResult = Globals.Message.Show(Res.GetString("ED410D37-B218-4EFD-AE49-5A083F7E03B3", "Are you sure you want to send the TRC container tracing message to the port system?"), Res.GetString("71D7580D-C73E-4C9B-947A-DCB551E5C2C9", "Tracing Request (TRC)"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (confirmationDialogResult == DialogResult.No)
					{
						return;
					}
				}

				var direction = entry.IsImport ? DemandeDeTracingDirection.Import : DemandeDeTracingDirection.Export;
				var res = DemandeDeTracingMessageSender.SendMessage(entry.TRCDetailsProvider, direction, out var notifications);

				var message = string.Empty;

				if (res)
				{
					message = Res.GetString("7476A001-67FE-44FD-9C16-77F7174904C9", "Tracing Request (TRC) has been sent.");
				}
				else if (notifications.Count > 0)
				{
					message = string.Join(System.Environment.NewLine, notifications.Select(n => n.Message));
				}

				if (!string.IsNullOrWhiteSpace(message))
				{
					var caption = Res.GetString("8002798D-4E16-45B3-9ECC-BAC3321A62D0", "Information");
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}
	}
}
