using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ResourceStrings.Grammar;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public static class MenuBuilderHelper
	{
		public delegate void CreateManifestMessage(AsycudaManifestHeader header, string messageSubType);
		public delegate void CreateMessageFromSelectedItems(AsycudaManifestHeader header, string messageSubType, IList<IMessageParent> messageParents, MessageChooser messageChooser);

		#region Bill Level MenuItems

		public static void AddBillLevelMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header, CreateMessageFromSelectedItems createMessageFromSelectedItems, string messageLabel, bool validateManifest = true)
		{
			AddBillLevelMenuItem(mainForm, menuItems, header, header.MessageStatusProvider, createMessageFromSelectedItems, messageLabel, validateManifest);
		}

		public static void AddBillLevelMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header, MessageStatusProvider messageStatusProvider, CreateMessageFromSelectedItems createMessageFromSelectedItems, string messageLabel, bool validateManifest = true)
		{
			var billCountries = header.Bills.OfType<IMessageParent>().ToArray();
			var itemLabel = Res.GetString("F386CE29-0FC7-4E73-836E-8B0E52A17BBA", "Bill");

			AddSelectionItemsMenuItem(mainForm, menuItems, header, messageStatusProvider, billCountries, createMessageFromSelectedItems, messageLabel, itemLabel, validateManifest);
		}

		#endregion

		#region Pack Level MenuItems

		public static void AddPackLevelMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header, CreateMessageFromSelectedItems createMessageFromSelectedItems, string messageLabel)
		{
			var packedItems = header.IsOnePackedItemRelationship ? GetReloadedPackedItems(header) : Array.Empty<AsycudaPackedItem>();
			var itemLabel = Res.GetString("0FACA573-052C-4EA4-B5BE-0601B95619C6", "Bill"); // User selects through Bills

			AddSelectionItemsMenuItem(mainForm, menuItems, header, header.MessageStatusProvider, packedItems, createMessageFromSelectedItems, messageLabel, itemLabel);
		}

		static AsycudaPackedItem[] GetReloadedPackedItems(AsycudaManifestHeader header)
		{
			var packedItems = header.Bills.OfType<AsycudaBill>().SelectMany(bill => bill.Packs.OfType<AsycudaPack>()).Select(x => x.PackedItem).OfType<AsycudaPackedItem>().ToArray();
			header.Factory.ReloadAllSafe(packedItems);
			return packedItems;
		}

		#endregion

		#region SelectionItems MenuItems

		public static void AddSelectionItemsMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header, MessageStatusProvider messageStatusProvider, IMessageParent[] messageParents, CreateMessageFromSelectedItems createMessageFromSelectedItems, string messageLabel, string itemLabel, bool validateManifest = true)
		{
			messageStatusProvider = messageStatusProvider ?? header.MessageStatusProvider;
			bool hasManifestMenuItem = false;
			var newMessageParents = messageParents
				.Where(messageStatusProvider.AllowOriginalMessage)
				.ToArray();

			var amendableMessageParents = messageParents
				.Where(messageStatusProvider.AllowModificationMessage)
				.ToArray();

			var cancellableMessageParents = messageParents
				.Where(messageStatusProvider.AllowCancellationMessage)
				.ToArray();

			if (newMessageParents.Length > 0)
			{
				var caption = ResString.GetMultilingualString("AsycudaMenu|SendManifest", "Send &{0}", messageLabel);
				AddSelectionItemsMenuItem(mainForm, menuItems, caption, header, newMessageParents, createMessageFromSelectedItems, itemLabel, messageStatusProvider.GetMessageFunctionSubTypeForSend(header), validateManifest: validateManifest);
				hasManifestMenuItem = true;
			}

			if (amendableMessageParents.Length > 0)
			{
				var caption = ResString.GetMultilingualString("AsycudaMenu|ModifyItems", "&Amend {0}", Grammar.Instance.Pluralize(itemLabel));
				AddSelectionItemsMenuItem(mainForm, menuItems, caption, header, amendableMessageParents, createMessageFromSelectedItems, itemLabel, messageStatusProvider.GetMessageFunctionSubTypeForAmend(header), validateManifest: validateManifest);
				hasManifestMenuItem = true;
			}

			if (cancellableMessageParents.Length > 0)
			{
				var caption = ResString.GetMultilingualString("AsycudaMenu|CancelItems", "&Cancel {0}", Grammar.Instance.Pluralize(itemLabel));
				AddSelectionItemsMenuItem(mainForm, menuItems, caption, header, cancellableMessageParents, createMessageFromSelectedItems, itemLabel, messageStatusProvider.GetMessageFunctionSubTypeForCancel(header), validateManifest: validateManifest);

				var manifestCancellableMessageParents = messageParents
					.Where(messageStatusProvider.AllowManifestCancellationMessage)
					.ToArray();

				if (manifestCancellableMessageParents.Any())
				{
					caption = ResString.GetMultilingualString("AsycudaMenu|CancelManifest", "Cancel {0}", messageLabel);
					AddSelectionItemsMenuItem(mainForm, menuItems, caption, header, manifestCancellableMessageParents, createMessageFromSelectedItems, itemLabel, MessageSubTypeCodes.Codes.CancelManifest, false, validateManifest: validateManifest);
				}
				hasManifestMenuItem = true;
			}

			if (!hasManifestMenuItem)
			{
				var menu = AsycudaMenu.AddNoManifestToSendMenuItem(header, messageLabel);
				menuItems.Add(menu);
			}
		}

		static void AddSelectionItemsMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, ResourceString caption, AsycudaManifestHeader header, IList<IMessageParent> messageParents, CreateMessageFromSelectedItems createMessageFromSelectedItems, string itemLabel, string messageSubType, bool withSelection = true, bool validateManifest = true)
		{
			AddMenuItem(mainForm, menuItems, caption, header, () => SendSelectionItems(header, messageSubType, messageParents, withSelection, createMessageFromSelectedItems, itemLabel), validateManifest);
		}

		static void SendSelectionItems(AsycudaManifestHeader header, string messageSubType, IList<IMessageParent> messageParents, bool withSelection, CreateMessageFromSelectedItems createMessageFromSelectedItems, string itemLabel)
		{
			var dialogResult = DialogResult.Abort;
			MessageChooser messageChooser = null;

			bool packLevel = header.IsPackedItemLevelManifestType;
			IEnumerable<IMessageParent> billsToShow = messageParents;
			if (packLevel)
			{
				billsToShow = messageParents.OfType<AsycudaPackedItem>().Select(x => x.Pack.Bill).Distinct();
			}

			using (var mutexManager = new MutexManager(billsToShow.Cast<AsycudaBill>(), (x) => x.SendToManifestMutex))
			{
				if (mutexManager.HasAquiredLockForAllBills)
				{
					if (withSelection)
					{
						messageChooser = header.GetNewMessageChooser(billsToShow.Select(x => x.SelectionItem), messageSubType, messageSubType != header.MessageFunctionSubTypeForCancel);

						using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, itemLabel, messageSubType))
						{
							dialogResult = ZFormModaliser.ShowDialogWithoutDispose(dlg);
							var selectedPks = dlg.GetSelectedItems().Select(x => x.PK).ToList();

							if (dialogResult == DialogResult.OK && !selectedPks.Any())
							{
								Globals.Message.ShowError(Res.GetString("0E868EB3-9767-48C1-872A-D0867A196503", "At least one {0} must be selected when sending a {0}-level manifest message.", itemLabel));
								return;
							}

							if (packLevel)
							{
								var selectedBills = billsToShow.Where(x => selectedPks.Contains(x.SelectionItem.PK));
								var billCountries = selectedBills.Cast<AsycudaBill>().Where(x => x != null);
								messageParents = messageParents.OfType<AsycudaPackedItem>().Where(x => billCountries.Contains(x.Pack.Bill)).ToArray();
							}
							else
							{
								messageParents = billsToShow.Where(x => selectedPks.Contains(x.SelectionItem.PK)).ToArray();
							}
						}
					}
					else
					{
						dialogResult = DialogResult.OK;
					}
				}
				else
				{
					Globals.Message.Show(string.Format(CultureInfo.CurrentCulture, "Messages are currently being generated and sent by {0}, please try again later.", mutexManager.GetMutexLockByInfo()), "Try later", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
			}

			if (dialogResult == DialogResult.OK)
			{
				createMessageFromSelectedItems(header, messageSubType, messageParents, messageChooser);
			}
		}

		#endregion

		#region Manifest Level MenuItems

		public static void AddSendManifestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header, CreateManifestMessage createManifestMessage, string messageLabel, bool validateManifest = true)
		{
			var messageStatusProvider = header?.MessageStatusProvider;
			if (messageStatusProvider != null)
			{
				bool hasManifestMenuItem = false;

				if (messageStatusProvider.AllowOriginalMessage(header))
				{
					var caption = ResString.GetMultilingualString("AsycudaMenu|SendManifest", "Send &{0}", messageLabel);
					AddMenuItem(mainForm, menuItems, caption, header, () => SendManifest(header, createManifestMessage, MessageSubTypeCodes.Codes.Original), validateManifest);
					hasManifestMenuItem = true;
				}

				if (messageStatusProvider.AllowModificationMessage(header))
				{
					var caption = ResString.GetMultilingualString("AsycudaMenu|Amend", "&Amend {0}", messageLabel);
					AddMenuItem(mainForm, menuItems, caption, header, () => SendManifest(header, createManifestMessage, header.MessageFunctionSubTypeForAmend), validateManifest);
					hasManifestMenuItem = true;
				}

				if (messageStatusProvider.AllowCancellationMessage(header))
				{
					var caption = ResString.GetMultilingualString("AsycudaMenu|Cancel", "&Cancel {0}", messageLabel);
					AddMenuItem(mainForm, menuItems, caption, header, () => SendManifest(header, createManifestMessage, header.MessageFunctionSubTypeForCancel), validateManifest);
					hasManifestMenuItem = true;
				}

				if (!hasManifestMenuItem)
				{
					var menuItem = AsycudaMenu.AddNoManifestToSendMenuItem(header, messageLabel);
					menuItems.Add(menuItem);
				}
			}
		}

		static void SendManifest(AsycudaManifestHeader header, CreateManifestMessage createManifestMessage, string messageSubType)
		{
			var securityCheckPoint = Env.Security.GlobalManifestSendWithMessageErrors;
			if (!securityCheckPoint.IsAllowed && header.HasMessageErrors)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				createManifestMessage(header, messageSubType);
			}
		}

		#endregion

		public static ZMenuItem AddMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, ResourceString caption, AsycudaManifestHeader header, Action send, bool validateManifest = true, Func<bool> preValidationDelegate = null)
		{
			var menuItem = new ZMenuItem(caption);
			menuItem.Click += delegate
			{
				bool preValidationPassed = preValidationDelegate?.Invoke() ?? true;

				if (preValidationPassed && ((validateManifest && ValidateManifest(header, mainForm)) || !validateManifest))
				{
					send();
				}
			};
			menuItems.Add(menuItem);

			return menuItem;
		}

		static bool ValidateManifest(AsycudaManifestHeader header, ZForm mainForm)
		{
			var result = false;
			if (header != null)
			{
				if (SaveDataFirst.Confirm(header, mainForm))
				{
					var messageSendingNotification = header.MessageSendingNotificationHelper.GetNotifications();
					if (!messageSendingNotification.IsEmpty)
					{
						Globals.Message.ShowWarning(messageSendingNotification);
					}
					else
					{
						var messageConfirmation = header.MessageSendingNotificationHelper.GetConfirmations();
						var couldContinue = true;
						foreach (var message in messageConfirmation)
						{
							couldContinue = Globals.Message.Show(message,
								Res.GetString("421A608D-47ED-4A18-880F-6DE131861B41", "Continue sending?"),
								ZMessageBoxButtons.YesNo, ZDialogResult.No) == ZDialogResult.Yes;
							if (!couldContinue)
							{
								break;
							}
						}

						if (couldContinue)
						{
							var errorCollector = header.ValidateEverythingAndSummariseProblems();
							result = errorCollector.IsEmpty || AskToProceedDespiteErrors(errorCollector);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("caea039e-c16a-4ce1-a434-271fed6d283c", "No valid manifest header exists. Please enter the Manifest tab to create a manifest header."));
			}
			return result;
		}

		static bool AskToProceedDespiteErrors(string problems)
		{
			var answerOnItsOwnLineForDebugging = Globals.Message.Show(problems, Res.GetString("7d8ae321-bd87-451b-8d4b-c1628661e625", "Proceed with errors?"), MessageBoxButtons.YesNo, DialogResult.Yes);
			return answerOnItsOwnLineForDebugging == DialogResult.Yes;
		}
	}
}
