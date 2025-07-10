using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NctsHeader = Enterprise.Customs.EU.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5NctsMessagingMenuItemTest : TestCaseWithFactory
	{
		public void TestAddMenuItems_Default()
		{
			using (var menu = new Phase5NctsMessagingMenuItem())
			{
				menu.NctsHeader = Factory.New<NctsHeader>();
				AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "Make Arrival Notification for this Departure", "-", "Inventory Management", "TS Register Management", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item", "Lock Customs Declaration", "Unlock Customs Declaration" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestAddMenuItems()
		{
			var header = Factory.New<NctsHeader>();
			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(header)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var menu = new Phase5NctsMessagingMenuItem())
			{
				menu.NctsHeader = header;
				AssertContainsExactElementsInExactOrder(new[] { "Test Menu Item", "Invisible Menu Item", "-", "Inventory Management", "TS Register Management", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item", "Lock Customs Declaration", "Unlock Customs Declaration" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestOnPopupWhenHeaderIsNull()
		{
			using (var menu = new Phase5NctsMessagingMenuItem())
			{
				menu.NctsHeader = null;
				AssertNoExceptionThrown(() => menu.ShowPopupMenu());
			}
		}

		public void TestOnPopup()
		{
			var header = Factory.New<NctsHeader>();
			var nctsMessagingMenuProviders = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, new TestObjectHandle(new Phase5MessagingMenuProviderForTest(header)) }
			};

			using (ObjectFactory.Substitute("NCTSMessagingMenuProviders", nctsMessagingMenuProviders))
			using (var menu = new Phase5NctsMessagingMenuItem())
			{
				menu.NctsHeader = header;
				menu.ShowPopupMenu();
				AssertMultilineASCIIEquals("&NCTS\r\n   Test Menu Item\r\n   -\r\n   Inventory Management\r\n   TS Register Management\r\n   -\r\n   Import Entry Lines\r\n   Import Invoice Lines\r\n   &Copy Previous Goods Item\r\n   Lock Customs Declaration\r\n   Unlock Customs Declaration\r\n", menu.GetVisibleMenuItemsCaptions());
			}
		}

		public void TestImportInvoiceLinesMessagePopup()
		{
			var message = (UnitTestUserNotification)Globals.Message;

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var nctsMovementForm = CreateMovementForm(header))
			{
				var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest("Import Invoice Lines");
				message.ClearMessages();
				menuItem.PerformClick();

				AssertEquals("Message is shown when bill count = 0", "In order to import Invoice Lines, please first add at least one House Consignment.", message.LastMessage.Text);
			}
		}

		public void TestResendMessagePopup()
		{
			var message = (UnitTestUserNotification)Globals.Message;
			var variations = new List<(string movementType, string transactionStatus, bool expectResendPopup)>();
			foreach (var transactionStatus in new NctsMovementHeaderTransactionStatusList().GetAllCodes())
			{
				bool expectSuccess;
				switch (transactionStatus)
				{
					case NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent:
					case NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent:
						expectSuccess = true;
						break;
					default:
						expectSuccess = false;
						break;
				}

				variations.Add((NctsMovementType.Codes.Departure, transactionStatus, expectSuccess));
				variations.Add((NctsMovementType.Codes.Arrival, transactionStatus, expectSuccess));
			}

			CombineAssertions(() =>
			{
				foreach (var variation in variations)
				{
					var header = Factory.New<NctsHeader>();
					header.SetMovementType(variation.movementType);

					if (variation.movementType == NctsMovementType.Codes.Departure)
					{
						header.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
						header.MovementHeader.BM_Phase = variation.transactionStatus;
					}
					else
					{
						header.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
						header.ArrivalMovementHeader.BM_Phase = variation.transactionStatus;
					}

					Factory.Save();

					using (var nctsMovementForm = CreateMovementForm(header))
					{
						var sendMenuItem = GetSendToCustomsMenuItem(nctsMovementForm);
						message.ClearMessages();
						sendMenuItem.PerformClick();

						if (variation.expectResendPopup)
						{
							AssertEquals($"For MovementType {variation.movementType} with TransactionStatus {variation.transactionStatus}: Question Message isn't shown", true, message.LastMessage.WasQuestion);
							AssertEquals($"For MovementType {variation.movementType} with TransactionStatus {variation.transactionStatus}: Question of message is incorrect", "Are you sure you want to resend this message?", message.LastMessage.Text);
							AssertEquals($"For MovementType {variation.movementType} with TransactionStatus {variation.transactionStatus}: Confirmation string of message is incorrect", "yes", message.LastConfirmationStringShown);
						}
						else
						{
							AssertNotEquals($"For MovementType {variation.movementType} with TransactionStatus {variation.transactionStatus}: resend popup shouldn't be shown", "Are you sure you want to resend this message?", message.LastMessage.Text);
						}
					}
				}
			});
		}

		ZTemplateForm CreateMovementForm(NctsHeader header)
		{
			return header.BH_HeaderType == NctsMovementType.Codes.Departure ? new Phase5DepartureMovementForm(header) : new Phase5ArrivalMovementForm(header);
		}

		ZMenuItem GetSendToCustomsMenuItem(ZTemplateForm nctsMovementForm)
		{
			const string menuItemCaption = "Send to Customs";
			var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(menuItemCaption);
			AssertNotNull($@"Menu item ""{menuItemCaption}"" not found", menuItem);
			return menuItem;
		}

		sealed class Phase5MessagingMenuProviderForTest : Phase5MessagingMenuProvider
		{
			public Phase5MessagingMenuProviderForTest(NctsHeader header)
				: base(header)
			{
			}

			protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
			{
				yield return new ZMenuItem("Test Menu Item");
				yield return invisibleMenuItem = new ZMenuItem("Invisible Menu Item");
			}
			ZMenuItem invisibleMenuItem;

			public override void RefreshMenu()
			{
				SetMenuItemVisibility(invisibleMenuItem, () => false);
			}
		}
	}
}
