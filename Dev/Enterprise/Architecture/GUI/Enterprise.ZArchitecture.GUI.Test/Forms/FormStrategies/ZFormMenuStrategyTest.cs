using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Core.GUI.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZFormMenuStrategyTest : TestCaseWithFactory
	{
		const string URL_ENT_SERVICES = "https://some.domain/Services";
#if WINZOR
		const string URL_SB_REDIRECT = "https://sessionBroker.domain/link";
#endif

		public void TestAddOperationalActionsMenuItemAfterFormIsLoaded()
		{
			using (var form = new ZForm((BusinessObject)Factory.New<Forwarding.IForwardingShipment>()) { ControllerID = ControllerIDs.JobShipment })
			{
				AssertNull("Operational Actions menu item should not added yet", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Operational Actions"));

				form.Show();
				AssertNotNull("Operational Actions menu item should be added", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Operational Actions"));
			}
		}

		public void TestHasZEditMenu()
		{
			using (var form = new ZForm())
			{
				var item = form.Menu.MenuItems.FindByText("&Edit");
				AssertNotNull("ZEditMenuNotFound", item);
				AssertEquals(typeof(ZEditMenuItem), item.GetType());
			}
		}

		public void TestFileNewMenuItem_Click_NullButton()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileNewMenuItemName].PerformClick();
				AssertExceptionReported("fApplyButton(FileNewMenuItem)");
			}
		}

		public void TestFileSaveMenuItem_Click_NullButton()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName].PerformClick();
				AssertExceptionReported("fApplyButton(FileSaveMenuItem)");
			}
		}

		public void TestFileSaveAndCloseMenuItem_Click_NullButton()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveAndCloseMenuItemName].PerformClick();
				AssertExceptionReported("fPostButton(FileSaveAndCloseMenuItem)");
			}
		}

		public void TestFileCloseMenuItem_Click_NullButton()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileCloseMenuItemName].PerformClick();
				AssertExceptionReported("fCancelButton");
			}
		}

		public void TestFileDeleteMenuItem_Click_NullButton()
		{
			using (var form = new ZForm())
			{
				form.Menu.MenuItems.FindByText(ZFormMenuStrategy.GetFileDeleteMenuItemText(form), true).PerformClick();
				AssertExceptionReported("fPostButton(FileDeleteMenuItem)");
			}
		}

		public void TestFileDeleteMenuItem_ShowsCustomCaption()
		{
			using (var form = new CustomDeleteTextForm())
			{
				var menuItem = form.Menu.MenuItems.FindByText(ZFormMenuStrategy.GetFileDeleteMenuItemText(form), true);
				AssertEquals(form.DeleteButtonText, menuItem.Text);
			}
		}

		#region CustomDeleteTextForm

		class CustomDeleteTextForm : ZForm, IButtonDeleteTextOverride
		{
			public string DeleteButtonText
			{
				get { return "Custom"; }
			}
		}

		#endregion

		public void TestFavoriteMenuItems()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZForm(dummyBizO))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				var actionMenuItemsProvider = (IFileMenuItemsProvider)testForm;

				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;

				var addMenuItem = actionsMenuItem.MenuItems.FindByText("Add to Favorites");
				AssertNotNull("Has Add to Favorites button", addMenuItem);
				AssertEquals("Add to Favorites menu item should be named correctly", "Actions.AddToFavoriteMenuItem", addMenuItem.Name);

				var removeMenuItem = actionsMenuItem.MenuItems.FindByText("Remove from Favorites");
				AssertNotNull("Has Remove from Favorites button", removeMenuItem);
				AssertEquals("Remove from Favorites menu item should be named correctly", "Actions.DeleteFromFavoriteMenuItem", removeMenuItem.Name);

				actionsMenuItem.ShowPopupMenu();
				AssertEquals(true, addMenuItem.Visible);
				AssertEquals(false, addMenuItem.Enabled);
				AssertEquals(false, removeMenuItem.Visible);
				AssertEquals(false, removeMenuItem.Enabled);

				// does nothing because object is not saved, but should not crash with exception
				addMenuItem.PerformClick();
				removeMenuItem.PerformClick();

				Factory.Save();

				actionsMenuItem.ShowPopupMenu();
				AssertEquals(true, addMenuItem.Visible);
				AssertEquals(true, addMenuItem.Enabled);
				AssertEquals(false, removeMenuItem.Visible);
				AssertEquals(false, removeMenuItem.Enabled);

				// item added to favourites
				addMenuItem.PerformClick();

				actionsMenuItem.ShowPopupMenu();
				AssertEquals(false, addMenuItem.Visible);
				AssertEquals(false, addMenuItem.Enabled);
				AssertEquals(true, removeMenuItem.Visible);
				AssertEquals(true, removeMenuItem.Enabled);

				// item removed from favourites
				removeMenuItem.PerformClick();

				actionsMenuItem.ShowPopupMenu();
				AssertEquals(true, addMenuItem.Visible);
				AssertEquals(true, addMenuItem.Enabled);
				AssertEquals(false, removeMenuItem.Visible);
				AssertEquals(false, removeMenuItem.Enabled);
			}
		}

		public void TestReloadFormMenuItem_ShouldExist()
		{
			var dummyBizo = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(dummyBizo))
			{
				form.Show();
				var menuItems = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName];
				AssertNotNull("We should be able to find this button and click it, and yet...", menuItems.MenuItems[ZFormMenuStrategy.FileReloadMenuItemName]);
			}
		}

		[DeveloperOnlyTest]
		public void TestActionMenu_CopyHyperlinkToClipboard_NativeHandler()
		{
			var bizo = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, AutoEvents.EditedARecord.Code));

			DataRegistry.Instance.WebHyperlinksEnabled = false;

			using (var testForm = new ZForm(bizo))
			{
				testForm.ControllerID = ControllerIDs.Events;

				var actionMenuItemsProvider = (IFileMenuItemsProvider)testForm;
				var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
				actionsMenu.OnPopup(EventArgs.Empty);

				var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyHyperlinkToClipboardName);
				AssertEquals(Shortcut.CtrlH, menuItem.Shortcut);
				AssertEquals(true, menuItem.ShowShortcut);
				menuItem.PerformClick();

				var text = (string)SafeClipboard.GetData(DataFormats.Text);
				AssertEquals("hyperlink should be job code or fallback (back-compat only)", "Event", text);
				AssertNotContains("hyperlink should not contain markdown", "](", text);

				text = (string)SafeClipboard.GetData(DataFormats.Rtf);
				AssertContains("hyperlink should be RTF text", @"\rtf", text);
				AssertContains("hyperlink should be for CW1 native URL handler.", "edient:", text);

				text = (string)SafeClipboard.GetData(DataFormats.Html);
				AssertContains("hyperlink should be HTML", "<a href=", text);
				AssertContains("hyperlink should be for CW1 native URL handler.", "edient:", text);
			}

			SafeClipboard.Clear();
		}

		[DeveloperOnlyTest]
		public void TestActionMenu_CopyHyperlinkToClipboard_WebHyperlinks()
		{
			var bizo = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, AutoEvents.EditedARecord.Code));

			DataRegistry.Instance.WebHyperlinksEnabled = true;
			DataRegistry.Instance.WebVersionLaunchUrl = null;
			WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://cargowise.one/services/");

			using (var testForm = new ZForm(bizo))
			{
				testForm.ControllerID = ControllerIDs.Events;

				var actionMenuItemsProvider = (IFileMenuItemsProvider)testForm;
				var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
				actionsMenu.OnPopup(EventArgs.Empty);

				var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyHyperlinkToClipboardName);

				CombineAssertions("Test Copy HyperLink menu item", () => {
					AssertEquals(Shortcut.CtrlH, menuItem.Shortcut);
					AssertEquals(true, menuItem.ShowShortcut);
					menuItem.PerformClick();

					var text = (string)SafeClipboard.GetData(DataFormats.Text);
					var expectedHyperlink = $"https://cargowise.one/services/link/ShowEditForm/Events/{bizo.PK}?LicenceCode={Env.CurrentCompany.LicenceKeyIdentifier}";
					AssertEquals("hyperlink should be plain text link for Enterprise Service web trampoline.", expectedHyperlink, text);

					text = (string)SafeClipboard.GetData(DataFormats.Rtf);
					AssertContains("hyperlink should be RTF text", @"\rtf", text);
					AssertContains("hyperlink should be for Enterprise Service web trampoline.", expectedHyperlink, text);

					text = (string)SafeClipboard.GetData(DataFormats.Html);
					AssertContains("hyperlink should be HTML text", @"<a href=", text);
					AssertContains("hyperlink should be for Enterprise Services web trampoline.", expectedHyperlink, text);
				});
			}

			SafeClipboard.Clear();
		}

		public void TestDoesUseWebHyperlinks()
		{
			foreach (var (webHyperlinksEnabled, webVersionLaunchUrl, rootServicesUri) in GetAllHyperlinksRegistrySettingsCombinations())
			{
				DataRegistry.Instance.WebHyperlinksEnabled = webHyperlinksEnabled;
				DataRegistry.Instance.WebVersionLaunchUrl = webVersionLaunchUrl;
				WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rootServicesUri);
				var doesUseWebHyperlinks = ShortcutCreator.DoesUseWebHyperlinks();

#if WINZOR
				AssertEquals(doesUseWebHyperlinks, !webVersionLaunchUrl.IsNullOrEmpty() || !rootServicesUri.IsNullOrEmpty());
#else
				AssertEquals(doesUseWebHyperlinks, webHyperlinksEnabled && !rootServicesUri.IsNullOrEmpty());
#endif
			}
		}

		static IEnumerable<(bool webHyperlinksEnabled, string webVersionLaunchUrl, string rootServicesUri)> GetAllHyperlinksRegistrySettingsCombinations()
		{
			bool[] webHyperlinksEnabledValues = [true, false];
			string[] webVersionLaunchUrlValues = ["", "https://cargowise.one/services/"];
			string[] rootServicesUriValues = ["", "https://cargowise.one/services/"];
			foreach (var webHyperlinksEnabledValue in webHyperlinksEnabledValues)
			{
				foreach (var webVersionLaunchUrlValue in webVersionLaunchUrlValues)
				{
					foreach (var rootServicesUriValue in rootServicesUriValues)
					{
						yield return (webHyperlinksEnabledValue, webVersionLaunchUrlValue, rootServicesUriValue);
					}
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestActionMenu_HumanReadableNameToClipboard()
		{
			var bizoWithCode = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, AutoEvents.EditedARecord.Code));
			var bizoNoCode = Factory.New<GenPivot>();
			Factory.Save();

			AssertExceptionThrown(typeof(NoCodePropertyException), () => CodePropertyAttribute.CodePropertyNameFromType(bizoNoCode.GetType()));

			using (var testForm = new ZForm(bizoWithCode))
			{
				var actionMenuItemsProvider = testForm as IFileMenuItemsProvider;
				var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
				actionsMenu.OnPopup(EventArgs.Empty);

				var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyHumanReadableNameToClipboardName);

				CombineAssertions("Checking Menu item 'Copy Name To Clipboard' with Code.", delegate
				{
					AssertEquals("Shortcut Key", Shortcut.CtrlShiftJ, menuItem.Shortcut);
					AssertEquals("Shortcut Visible",true, menuItem.ShowShortcut);
					AssertEquals("Menu Visible",true, menuItem.Visible);

					menuItem.PerformClick();
					AssertEquals("Click functionality", "Event", (string)SafeClipboard.GetData(DataFormats.Text));
				});
			}

			SafeClipboard.Clear();

			using (var testForm = new ZForm(bizoNoCode))
			{
				var actionMenuItemsProvider = testForm as IFileMenuItemsProvider;
				var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
				actionsMenu.OnPopup(EventArgs.Empty);
				var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyHumanReadableNameToClipboardName);

				CombineAssertions("Checking Menu item 'Copy Name To Clipboard' with No Code.", delegate
				{
					AssertEquals("Shortcut Key", Shortcut.CtrlShiftJ, menuItem.Shortcut);
					AssertEquals("Shortcut Visible", true, menuItem.ShowShortcut);
					AssertEquals("Menu Visible", true, menuItem.Visible);

					menuItem.PerformClick();
					AssertEquals("Click functionality", "GenPivot", (string)SafeClipboard.GetData(DataFormats.Text));
				});
			}
		}

		public void TestAddActionMenuItem()
		{
			using (var testForm = new TestZForm())
			{
				var previousCount = testForm.ExposedActionMenuItem.MenuItems.Count;

				var newActionsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(testForm, "TestMenuItem", delegate { });
				AssertNotNull("AddActions", newActionsMenuItem);
				AssertEquals("Failed To Add Menu Item", 1 + previousCount, testForm.ExposedActionMenuItem.MenuItems.Count);
				AssertEquals("Actions Menu Item should be enabled", true, testForm.ExposedActionMenuItem.Enabled);

				ZFormMenuStrategy.AddActionsMenuItem(testForm, new ZMenuItem());
				AssertEquals("Failed To Add Menu Item", 2 + previousCount, testForm.ExposedActionMenuItem.MenuItems.Count);
				AssertEquals("Actions Menu Item should be enabled", true, testForm.ExposedActionMenuItem.Enabled);
			}
		}

		public void TestAddActionsMenuItemSetsParentModuleOnController()
		{
			var dummyTemplateBizo = Factory.New<DummyTemplateRecord>();
			using (var form = new ZForm(dummyTemplateBizo))
			{
				var controller = new DummyControllerWithTemplateModule();
				Assert("Precondition: Controller should disallow deletion for normal records, so we know that allowing a deletion for template records indicates the ParentModule was set correctly.",
					!controller.CheckPointForDeleteExposedForTest.IsAllowed);

				form.ControllerID = controller.ID;
				form.DisplayMode = ODisplayMode.Edit;

				var formModule = form.GetModule() as DummyFilterGridModuleWithTemplates;
				formModule.SupportTemplateRecordsForTest = true;
				AssertNotNull("Precondition: Form's module should be template module.", formModule);
				Assert("Precondition: Form's module should allow template records.", (formModule as IZFilterGridModule).AllowTemplateRecords);

				var actionsMenuItem = (form as IFileMenuItemsProvider)?.ActionsMenuItem;
				AssertNotNull("Precondition: Actions menu item should exist.", actionsMenuItem);

				var makeInactiveMenuItem = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				AssertNotNull("Precondition: Actions menu item should contain Make Inactive menu item.", makeInactiveMenuItem);

				makeInactiveMenuItem.PerformClick();
				AssertEquals("Form should be switched to Delete mode, and no security rights errors should have occurred.", ODisplayMode.Delete, form.DisplayMode);
			}
		}

		public void TestMainMeunItemShouldBeClearedAfterZFormDisposed()
		{
			var testForm = new TestZForm();
			var mainMenu = testForm.Menu;

			AssertNotNull(testForm.Menu);
			AssertEquals(4, mainMenu.MenuItems.Count);

			testForm.Dispose();

			AssertNull(testForm.Menu);
			AssertEquals(0, mainMenu.MenuItems.Count);
		}

		[TestDate(2015, 10, 21)]
		public void TestAddInterfaceConnectorCSVImportMenuItem_WithNewItem()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var item = new ZMenuItem("From CSV file", delegate { });

			using (var form = new TestZForm())
			{
				var previousCount = form.ExposedActionMenuItem.MenuItems.Count;

				ZFormMenuStrategy.AddInterfaceConnectorMenuItem(form, item);

				AssertEquals("should have added menu item", 1 + previousCount, form.ExposedActionMenuItem.MenuItems.Count);
			}

			using (var form = new TestZForm())
			{
				var previousCount = form.ExposedActionMenuItem.MenuItems.Count;

				ZFormMenuStrategy.AddInterfaceConnectorMenuItem(form, item, true);

				AssertEquals("should have added menu item", previousCount + 1, form.ExposedActionMenuItem.MenuItems.Count);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var form = new TestZForm())
			{
				var previousCount = form.ExposedActionMenuItem.MenuItems.Count;

				ZFormMenuStrategy.AddInterfaceConnectorMenuItem(form, item);

				AssertEquals("should not have added menu item", previousCount, form.ExposedActionMenuItem.MenuItems.Count);
			}

			using (var form = new TestZForm())
			{
				var previousCount = form.ExposedActionMenuItem.MenuItems.Count;

				ZFormMenuStrategy.AddInterfaceConnectorMenuItem(form, item, true);

				AssertEquals("should have added menu item", 1 + previousCount, form.ExposedActionMenuItem.MenuItems.Count);
			}
		}

		public void TestActionsMenuItemContainsActivatingDeactivatingItems()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			var dummyCancellable = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			var dummyCancellableWhichCanNotBeCancelled = Factory.New<ZFormTest.DummyCancellableWhichCanNotBeCancelled>();
			var dummyCancellableWhichCanNotBeReactivated = Factory.New<ZFormTest.DummyCancellableWhichCanNotBeReactivated>();

			using (var zForm = new ZForm(dummyBizO))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNull("Should not contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));
			}

			using (var zForm = new ZForm(dummyCancellable))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				makeInactive.PerformClick();
				AssertEquals("Form should be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO shuold be marked as Cancelled", true, dummyCancellable.IsCancelled);
			}

			using (var zForm = new ZForm(dummyCancellableWhichCanNotBeCancelled))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should NOT contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				makeInactive.PerformClick();

				AssertEquals("Should be message tht BizO can't be cancelled", "This Dummy BizO can't be cancelled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("Form should NOT be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO shuold NOT be marked as Cancelled", false, dummyCancellableWhichCanNotBeCancelled.IsCancelled);
			}

			dummyCancellableWhichCanNotBeReactivated.IsCancelled = true;
			using (var zForm = new ZForm(dummyCancellableWhichCanNotBeReactivated))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNotNull("Should contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNull("Should NOT contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeActive = actionsMenuItem.MenuItems.FindByText("Make Active");
				makeActive.PerformClick();

				AssertEquals("Should be message that BizO can't be cancelled", "This Dummy BizO can't be reactivated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("Form should NOT be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO shuold be marked as Cancelled", true, dummyCancellableWhichCanNotBeReactivated.IsCancelled);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestAddInterfaceConnectorMenuItem()
		{
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText, true, true);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText, true, false);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText, false, true);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText, false, false);

			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText, true, true);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText, true, false);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText, false, true);
			AssertAddInterfaceConnectorMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText, false, false);
		}

		public void AssertAddInterfaceConnectorMenuItem(string textForItem, bool temporarilyAllowed, bool enableInterfaceConnector)
		{
			using (var testForm = new ZTestForm())
			{
				var numberOfActionMenuItems = testForm.ActionsMenuItem.MenuItems.Count;

				var item = new ZMenuItem(textForItem);

				InterfaceConnectorTemporarilyEnabledUntil dateValue;
				if (enableInterfaceConnector)
				{
					dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(2) };
					eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);
				}
				else
				{
					dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
					eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);
				}

				ZFormMenuStrategy.AddInterfaceConnectorMenuItem(testForm, item, temporarilyAllowed);

				if (!temporarilyAllowed)
				{
					if (enableInterfaceConnector)
					{
						AssertNotNull(testForm.ActionsMenuItem.MenuItems.FindByText(item.Text).Text);
						AssertEquals("1 menu item should have been added.", numberOfActionMenuItems + 1, testForm.ActionsMenuItem.MenuItems.Count);
					}
					else
					{
						AssertEquals("No menu items should have been added.", numberOfActionMenuItems, testForm.ActionsMenuItem.MenuItems.Count);
					}
				}
				else
				{
					AssertNotNull(testForm.ActionsMenuItem.MenuItems.FindByText(item.Text).Text);
					AssertEquals("1 menu item should have been added.", numberOfActionMenuItems + 1, testForm.ActionsMenuItem.MenuItems.Count);
				}
			}

			var reset = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, reset);
		}

		public void TestAddInterfaceConnectorMenuItems_TemporarilyAllowed()
		{
			using (var testForm = new ZTestForm())
			{
				var numberOfActionMenuItems = testForm.ActionsMenuItem.MenuItems.Count;

				var dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(2) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);

				var items = new List<MenuItem>();
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.NativeMenuItemText));

				ZFormMenuStrategy.AddInterfaceConnectorMenuItems(testForm, items, true);

				AssertNotNull(testForm.ActionsMenuItem.MenuItems.FindByText(ExportXmlMenuItemHelper.VerboseMenuItemText));
				AssertEquals("3 new menu items should have been added.", numberOfActionMenuItems + 3, testForm.ActionsMenuItem.MenuItems.Count);

				dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);
			}
		}

		public void TestAddInterfaceConnectorMenuItems_NotTemporarilyAllowedAndInterfaceConnectorTrue()
		{
			using (var testForm = new ZTestForm())
			{
				var numberOfActionMenuItems = testForm.ActionsMenuItem.MenuItems.Count;

				var dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(2) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);

				var items = new List<MenuItem>();
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.NativeMenuItemText));

				ZFormMenuStrategy.AddInterfaceConnectorMenuItems(testForm, items);

				AssertNotNull(testForm.ActionsMenuItem.MenuItems.FindByText(ExportXmlMenuItemHelper.VerboseMenuItemText));
				AssertEquals("3 new menu items should have been added.", numberOfActionMenuItems + 3, testForm.ActionsMenuItem.MenuItems.Count);

				dateValue = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);
			}
		}

		public void TestAddInterfaceConnectorMenuItems_NotTemporarilyAllowedAndInterfaceConnectorFalse()
		{
			using (var testForm = new ZTestForm())
			{
				var numberOfActionMenuItems = testForm.ActionsMenuItem.MenuItems.Count;

				var items = new List<MenuItem>();
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.VerboseMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.LightWeightMenuItemText));
				items.Add(new ZMenuItem(ExportXmlMenuItemHelper.NativeMenuItemText));

				ZFormMenuStrategy.AddInterfaceConnectorMenuItems(testForm, items);

				AssertEquals("No new menu items should have been added.", numberOfActionMenuItems, testForm.ActionsMenuItem.MenuItems.Count);
			}
		}

		public void TestActivatingDeactivatingMenuItemIsControlledByDeleteSecurityCheckpoint()
		{
			var dummyCancellable = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			var dummyCancellable2 = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();

			using (var zForm = new ZForm(dummyCancellable))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				var controllerYes = new DummyControllerWhichAllowsDelete();
				zForm.ControllerID = controllerYes.ID;
				zForm.DisplayMode = ODisplayMode.Edit;

				makeInactive.PerformClick();
				AssertEquals("Form should be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO should be marked as Cancelled", true, dummyCancellable.IsCancelled);
			}

			using (var zForm = new ZForm(dummyCancellable2))
			{
				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				var controllerNo = new DummyControllerWhichDoesNotAllowDelete();
				zForm.ControllerID = controllerNo.ID;
				zForm.DisplayMode = ODisplayMode.Edit;

				makeInactive.PerformClick();
				AssertEquals("Form should NOT be switched to Delete mode", ODisplayMode.Edit, zForm.DisplayMode);
				AssertEquals("BizO should NOT be marked as Cancelled", false, dummyCancellable2.IsCancelled);

				AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMarkInactiveCallsOnBusinessObjectIsCancelledChanged()
		{
			var dummyCancellable = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();

			using (var zForm = new ZForm(dummyCancellable))
			{
				zForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				var dummyPlugin = zForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1) as DummyPlugIn1;
				AssertNotNull(dummyPlugin);
				Assert(!dummyPlugin.IsCancelledCurrentValue);

				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				var controllerYes = new DummyControllerWhichAllowsDelete();
				zForm.ControllerID = controllerYes.ID;
				zForm.DisplayMode = ODisplayMode.Edit;

				makeInactive.PerformClick();
				AssertEquals("Form should be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO should be marked as Cancelled", true, dummyCancellable.IsCancelled);
				Assert(dummyPlugin.IsCancelledCurrentValue);
			}
		}

		public void TestMarkActiveCallsOnBusinessObjectCallsMarkAsNeededValidation()
		{
			var dummyCancellable = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			dummyCancellable.IsCancelled = true;

			using (var zForm = new ZForm(dummyCancellable))
			{
				zForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				var dummyPlugin = zForm.PlugIns.GetPlugIn(DummyControllerIDs.Dummy1) as DummyPlugIn1;
				AssertNotNull(dummyPlugin);

				IFileMenuItemsProvider menuItemsProvider = zForm;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;
				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNotNull("Should contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNull("Should NOT contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeActive = actionsMenuItem.MenuItems.FindByText("Make Active");

				var controllerYes = new DummyControllerWhichAllowsDelete();
				zForm.ControllerID = controllerYes.ID;
				zForm.DisplayMode = ODisplayMode.Edit;

				dummyCancellable.MarkLightValidationAsValidForTesting();
				AssertEquals("BIzO should be marked as NOT Needed Validation", true, dummyCancellable.LightValidationIsValid);

				makeActive.PerformClick();
				AssertEquals("Form should be switched to Delete mode", ODisplayMode.Delete, zForm.DisplayMode);
				AssertEquals("BIzO should be marked as Not Cancelled", false, dummyCancellable.IsCancelled);
				AssertEquals("BIzO should be marked as Needed Validation", false, dummyCancellable.LightValidationIsValid);
			}
		}

		public void TestDoDisplayModeNew()
		{
			using (var form = new ZForm())
			{
				ZFormMenuStrategy.DoDisplayModeNew(form);

				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName].Visible);
				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName].Enabled);
				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveAndCloseMenuItemName].Visible);
				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveAndCloseMenuItemName].Enabled);
			}
		}

		public void TestDoDisplayModeNewSaved()
		{
			using (var form = new ZForm())
			{
				ZFormMenuStrategy.DoDisplayModeNewSaved(form);

				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName].Visible);
				Assert(!form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName].Enabled);
				Assert(form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveAndCloseMenuItemName].Visible);
				Assert(!form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveAndCloseMenuItemName].Enabled);
			}
		}

		#region TestControlSDoesNotOpenNewFormIfNoChanges

		public void TestControlSDoesNotOpenNewFormIfNoChanges()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var postingButtonsUserControl = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, postingButtonsUserControl);
				ZFormPostingButtonsStrategy.DoDisplayModeBrowse(form);
				form.ControllerID = new DummyControllerWhichDoesNotAllowDelete().ID;

				IPostingButtonsProvider buttonsProvider = form;
				AssertNotNull(buttonsProvider.CommandButtonApply);

				var buttonSave = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileSaveMenuItemName];
				Assert(buttonSave.Visible);
				Assert(!buttonSave.Enabled);

				var buttonNew = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileNewMenuItemName];
				Assert(buttonNew.Visible);
				Assert(buttonNew.Enabled);

				var toolStrip = (ZToolStrip)postingButtonsUserControl.Controls.Find("toolStrip", true).First();
				var realButtonNew = toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Text == "&New");
				AssertNotNull(realButtonNew);

				realButtonNew.Click += realButtonNew_Click;
				AssertEquals(0, realButtonNewClickCounter);
				buttonSave.PerformClick();
				AssertEquals("Button should not be pressed", 0, realButtonNewClickCounter);

				form.BusinessEntity.HasChanges = true;
				AssertEquals("&Save", postingButtonsUserControl.SaveButton.Text);
				postingButtonsUserControl.SaveButton.Text = "&Post";

				buttonSave.PerformClick();
				AssertEquals("Button should be pressed when button text is not 'New'", 1, realButtonNewClickCounter);
			}
		}

		int realButtonNewClickCounter;
		void realButtonNew_Click(object sender, EventArgs e)
		{
			realButtonNewClickCounter++;
		}

		#endregion

		#region InsertRange

		public void TestInsertRange_IndexIsLessThenZero_ThrowArgumentOutOfRangeException()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			var itemsToInsert = new List<MenuItem> { new MenuItem() };

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => { collectionToTest.InsertRange(-1, itemsToInsert); });
		}

		public void TestInsertRange_IndexIsGreaterThenCollectionCount_ThrowArgumentOutOfRangeException()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());

			var itemsToInsert = new List<MenuItem> { new MenuItem() };

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => { collectionToTest.InsertRange(5, itemsToInsert); });
		}

		public void TestInsertRange_ItemsToAddCollectionIsNull_DoNotModifyCollection()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());

			collectionToTest.InsertRange(2, null);
			AssertEquals("Items count should be the same", 4, collectionToTest.Count);
		}

		public void TestInsertRange_IndexIsInRangeOfCollection_InsertItemsAtSpecifiedIndex()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());

			var itemsToInsert = new MenuItem[] { new MenuItem("McLaren"), new MenuItem("Man Utd") };
			collectionToTest.InsertRange(2, itemsToInsert);

			AssertEquals("Items count should be increased", 6, collectionToTest.Count);
			AssertArrayEqualsByElements("Items are inserted at specified position", collectionToTest.Cast<MenuItem>().Skip(2).Take(2).ToArray(), itemsToInsert);
		}

		public void TestInsertRange_IndexEqualsCollectionSize_AddItemsAtTheEnd()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());

			var itemsToInsert = new MenuItem[] { new MenuItem("McLaren"), new MenuItem("Man Utd") };
			collectionToTest.InsertRange(4, itemsToInsert);

			AssertEquals("Items count should be increased", 6, collectionToTest.Count);
			AssertArrayEqualsByElements("Items are inserted at specified position", collectionToTest.Cast<MenuItem>().Skip(4).Take(2).ToArray(), itemsToInsert);
		}

		#endregion

		#region Replace

		public void TestReplace_ItemToReplaceIsNull_ThrowArgumentNullException()
		{
			var menu = new ZMainMenu();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			var itemsToReplaceWith = new List<MenuItem> { new MenuItem() };

			AssertExceptionThrown<ArgumentNullException>(() => { collectionToTest.Replace(null, itemsToReplaceWith); });
		}

		public void TestReplace_ItemsToReplaceWithCollectionIsNull_RemoveItemToReplace()
		{
			var menu = new ZMainMenu();

			var itemToReplace = new MenuItem();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(itemToReplace);
			collectionToTest.Add(new MenuItem());

			collectionToTest.Replace(itemToReplace, null);

			AssertEquals("Items count should be decreased", 2, collectionToTest.Count);
			AssertCollectionNotContains("ItemToReplace is removed", itemToReplace, collectionToTest);
		}

		public void TestReplace_ItemsToReplaceNotExist_DoNotModifyCollection()
		{
			var menu = new ZMainMenu();

			var itemToReplace = new MenuItem();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(new MenuItem());

			collectionToTest.Replace(itemToReplace, null);

			AssertEquals("Items count should be decreased", 2, collectionToTest.Count);
		}

		public void TestReplace_ItemsToReplaceWithAreSpecified_ProceedReplace()
		{
			var menu = new ZMainMenu();

			var itemToReplace = new MenuItem();

			var collectionToTest = new Menu.MenuItemCollection(menu);
			collectionToTest.Add(new MenuItem());
			collectionToTest.Add(itemToReplace);
			collectionToTest.Add(new MenuItem());

			var itemsToReplaceWith = new MenuItem[] { new MenuItem("McLaren"), new MenuItem("Man Utd") };

			collectionToTest.Replace(itemToReplace, itemsToReplaceWith);

			AssertEquals("Items count should be increased", 4, collectionToTest.Count);
			AssertCollectionNotContains("ItemToReplace is removed", itemToReplace, collectionToTest);
			AssertArrayEqualsByElements("Items are inserted instead at expected position", collectionToTest.Cast<MenuItem>().Skip(1).Take(2).ToArray(), itemsToReplaceWith);
		}

		#endregion

		#region InitialiseChildItemsOnIdle

		public void TestInitialiseChildItemsOnIdle_ParentMenuItemIsNull_ThrowArgumentNullException()
		{
			using (var testForm = new TestZForm())
			{
				ZFormMenuStrategy.MenuItemsProvider childInitialiser = () =>
				{
					return new List<MenuItem>
					{
						new MenuItem("McLaren"),
						new MenuItem("MU"),
					};
				};

				AssertExceptionThrown<ArgumentNullException>(() => { ZFormMenuStrategy.AddDeferredMenuItems(null, testForm, childInitialiser); });
			}
		}

		public void TestInitialiseChildItemsOnIdle_InitialiserIsNull_ThrowArgumentNullException()
		{
			using (var testForm = new TestZForm())
			{
				var actionMenu = ((IFileMenuItemsProvider)testForm).ActionsMenuItem;

				AssertExceptionThrown<ArgumentNullException>(() => { actionMenu.AddDeferredMenuItems(testForm, null); });
			}
		}

		public void TestInitialiseChildItemsOnIdle_OwnerIsNull_ThrowArgumentNullException()
		{
			using (var testForm = new TestZForm())
			{
				var actionMenu = ((IFileMenuItemsProvider)testForm).ActionsMenuItem;

				ZFormMenuStrategy.MenuItemsProvider childInitialiser = () =>
				{
					return new List<MenuItem>
					{
						new MenuItem("McLaren"),
						new MenuItem("MU"),
					};
				};

				AssertExceptionThrown<ArgumentNullException>(() => { actionMenu.AddDeferredMenuItems(null, childInitialiser); });
			}
		}

		public void TestInitialiseChildItemsOnIdle_InitialiserInitialisesSomeMenus_AddMenusToParent()
		{
			using (var testForm = new TestZForm())
			{
				var actionMenu = ((IFileMenuItemsProvider)testForm).ActionsMenuItem;

				ZFormMenuStrategy.MenuItemsProvider deferedItems1 = () =>
				{
					return new List<MenuItem>
					{
						new MenuItem("McLaren"),
						new MenuItem("MU"),
					};
				};

				ZFormMenuStrategy.MenuItemsProvider deferedItems2 = () =>
				{
					return new List<MenuItem>
					{
						new MenuItem("Alex Ferguson")
					};
				};

				actionMenu.MenuItems.Clear();
				actionMenu.MenuItems.Add("Button");
				actionMenu.MenuItems.Add("Perez");
				actionMenu.AddDeferredMenuItems(testForm, deferedItems1);
				actionMenu.MenuItems.Add("Rooney");
				actionMenu.MenuItems.Add("Giggs");
				actionMenu.AddDeferredMenuItems(testForm, deferedItems2);

				actionMenu.OnPopup(EventArgs.Empty);

				var mclarenItem = actionMenu.MenuItems.FindByText("McLaren");
				var muItem = actionMenu.MenuItems.FindByText("MU");
				var sirItem = actionMenu.MenuItems.FindByText("Alex Ferguson");

				AssertNotNull("Menu item exists", mclarenItem);
				AssertEquals("Menu is inserted on expected position", 2, actionMenu.MenuItems.IndexOf(mclarenItem));

				AssertNotNull("Menu item exists", muItem);
				AssertEquals("Menu is inserted on expected position", 3, actionMenu.MenuItems.IndexOf(muItem));

				AssertNotNull("Menu item exists", sirItem);
				AssertEquals("Menu is inserted on expected position", 6, actionMenu.MenuItems.IndexOf(sirItem));
			}
		}

		#region Incident CS00241641

		[ExpectNoExceptions]
		public void TestInitialiseChildItemsOnIdle_UserIdleWorkerIsDisabled_ShouldNotThrowExceptionOnFormDispose()
		{
			var userIdleWorkerEnabled = SystemDataRegistryForTest.Get().UserIdleWorkerEnabled;
			SystemDataRegistryForTest.Get().UserIdleWorkerEnabled = false;

			try
			{
				using (var testForm = new TestZForm())
				{
					var actionMenu = ((IFileMenuItemsProvider)testForm).ActionsMenuItem;

					ZFormMenuStrategy.MenuItemsProvider deferedItems = () =>
					{
						return new List<MenuItem>
						{
							new MenuItem("McLaren"),
							new MenuItem("MU"),
						};
					};

					actionMenu.MenuItems.Clear();
					actionMenu.MenuItems.Add("Button");
					actionMenu.MenuItems.Add("Magnussen");
					actionMenu.AddDeferredMenuItems(testForm, deferedItems);

					actionMenu.OnPopup(EventArgs.Empty);
				}
			}
			finally
			{
				SystemDataRegistryForTest.Get().UserIdleWorkerEnabled = userIdleWorkerEnabled;
			}
		}

		#endregion

		#endregion

		#region TestAddUniversalCopyMenuAfterFormIsLoaded

		public void TestAddUniversalCopyMenuAfterFormIsLoaded()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()) { ControllerID = DummyControllerIDs.Dummy })
			{
				AssertNull("Not added yet", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Universal Copy"));

				form.Show();
				AssertNotNull("Should be added added", ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Universal Copy"));
			}
		}

		#endregion

		public void TestDefaultActionMenuItemNames()
		{
			var names = ZFormMenuStrategy.DefaultActionMenuItemNames;
			AssertEquals(5, names.Count());
			AssertEquals(true, names.Any(n => n == ZFormMenuStrategy.CopyHyperlinkToClipboardName));
			AssertEquals(true, names.Any(n => n == ZFormMenuStrategy.CreateDesktopShortcutName));
			AssertEquals(true, names.Any(n => n == ZFormMenuStrategy.CopyFormToClipboardMenuItemName));
			AssertEquals(true, names.Any(n => n == ZFormMenuStrategy.ResetFormSizeToDefaultName));
			AssertEquals(true, names.Any(n => n == ZFormMenuStrategy.CopyIdToClipboardName));
		}

		public void TestIsDefaultActionMenuItem()
		{
			AssertEquals(true, ZFormMenuStrategy.IsDefaultActionMenuItemName(ZFormMenuStrategy.CopyHyperlinkToClipboardName));
			AssertEquals(true, ZFormMenuStrategy.IsDefaultActionMenuItemName(ZFormMenuStrategy.CreateDesktopShortcutName));
			AssertEquals(true, ZFormMenuStrategy.IsDefaultActionMenuItemName(ZFormMenuStrategy.CopyFormToClipboardMenuItemName));
			AssertEquals(true, ZFormMenuStrategy.IsDefaultActionMenuItemName(ZFormMenuStrategy.ResetFormSizeToDefaultName));
			AssertEquals(true, ZFormMenuStrategy.IsDefaultActionMenuItemName(ZFormMenuStrategy.CopyIdToClipboardName));
			AssertEquals(false, ZFormMenuStrategy.IsDefaultActionMenuItemName("foo"));
			AssertEquals(false, ZFormMenuStrategy.IsDefaultActionMenuItemName(""));
		}

		public void TestDisableActionMenuItemsExcludingDefaults()
		{
			using (var form = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				form.ActionsMenuItem.MenuItems.Add(new ZMenuItem("dummy action 1"));
				form.ActionsMenuItem.MenuItems.Add(new ZMenuItem("dummy action 2"));
				form.ActionsMenuItem.MenuItems.Add(new ZMenuItem("dummy action 3"));
				form.DisplayMode = ODisplayMode.ReadOnly;

				form.Show();

				var validActionMenuItems = form.ActionsMenuItem.MenuItems.OfType<ZMenuItem>();

				AssertEquals(true, validActionMenuItems.FindByText("dummy action 1").Enabled);
				AssertEquals(true, validActionMenuItems.FindByText("dummy action 2").Enabled);
				AssertEquals(true, validActionMenuItems.FindByText("dummy action 3").Enabled);

				ZFormMenuStrategy.DisableActionMenuItemsExcludingDefaultsInViewMode(form);

				AssertEquals("Actions enabled and not default", 0, validActionMenuItems.Count((m) => m.Enabled && !ZFormMenuStrategy.IsDefaultActionMenuItemName(m.Name)));
				AssertEquals("Actions enabled and default", 5, validActionMenuItems.Count((m) => m.Enabled && ZFormMenuStrategy.IsDefaultActionMenuItemName(m.Name)));
				AssertEquals(false, validActionMenuItems.FindByText("dummy action 1").Enabled);
				AssertEquals(false, validActionMenuItems.FindByText("dummy action 2").Enabled);
				AssertEquals(false, validActionMenuItems.FindByText("dummy action 3").Enabled);
			}
		}

#if !WINZOR
		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboard_NonWinzor_NotWebLinks_ExpectEdient()
		{
			using (RawDataRegistry.Instance.WebHyperlinksEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				// Act
				ZFormMenuStrategy.CopyHyperlinkToClipboard(testForm);

				// Asserts
				var valueText = (string)SafeClipboard.GetData(DataFormats.Text);
				var valueHtml = (string)SafeClipboard.GetData(DataFormats.Html);
				var valueRtf = (string)SafeClipboard.GetData(DataFormats.Rtf);

				SafeClipboard.Clear();

				AssertEquals("DummyBizo", valueText);

				AssertContains("href", valueHtml);
				AssertContains("edient:", valueHtml);

				AssertContains("rtf1", valueRtf);
				AssertContains("edient:", valueRtf);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboard_NonWinzor_WebLinks_ExpectHttpsEnterprise()
		{
			using (RawDataRegistry.Instance.WebHyperlinksEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, URL_ENT_SERVICES))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				// Act
				ZFormMenuStrategy.CopyHyperlinkToClipboard(testForm);

				// Asserts
				var valueText = (string)SafeClipboard.GetData(DataFormats.Text);
				var valueHtml = (string)SafeClipboard.GetData(DataFormats.Html);
				var valueRtf = (string)SafeClipboard.GetData(DataFormats.Rtf);

				SafeClipboard.Clear();

				AssertStartsWith("Should start with https","https://", valueText);

				AssertContains("href", valueHtml);
				AssertContains("https://", valueHtml);

				AssertContains("rtf1", valueRtf);
				AssertContains("https://", valueRtf);
			}
		}
#endif

#if WINZOR

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboard_Winzor_WebVersionLaunchUrl_ExpectHttpsSessionBroker()
		{
			using (RawDataRegistry.Instance.WebHyperlinksEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, URL_SB_REDIRECT))
			using (WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, URL_ENT_SERVICES))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				// Act
				SafeClipboard.Clear();
				ZFormMenuStrategy.CopyHyperlinkToClipboard(testForm);

				// Asserts
				var valueText = (string)SafeClipboard.GetData(DataFormats.Text);
				var valueHtml = (string)SafeClipboard.GetData(DataFormats.Html);
				var valueRtf = (string)SafeClipboard.GetData(DataFormats.Rtf);

				SafeClipboard.Clear();

				AssertStartsWith("Should contain Session Broker URL", URL_SB_REDIRECT, valueText);
				AssertContains("href", valueHtml);
				AssertContains(URL_SB_REDIRECT, valueHtml);
				AssertNullOrEmpty(valueRtf);    // No RTF for Winzor
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboard_Winzor_RootServicesUri_ExpectHttpsEnterprise()
		{
			using (RawDataRegistry.Instance.WebHyperlinksEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, URL_ENT_SERVICES))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				// Act
				SafeClipboard.Clear();
				ZFormMenuStrategy.CopyHyperlinkToClipboard(testForm);

				// Asserts
				var valueText = (string)SafeClipboard.GetData(DataFormats.Text);
				var valueHtml = (string)SafeClipboard.GetData(DataFormats.Html);
				var valueRtf = (string)SafeClipboard.GetData(DataFormats.Rtf);

				SafeClipboard.Clear();

				AssertStartsWith("Should start with Enterprise Services URL", URL_ENT_SERVICES, valueText);
				AssertContains("href", valueHtml);
				AssertContains(URL_ENT_SERVICES, valueHtml);
				AssertNullOrEmpty(valueRtf);	// No RTF for Winzor
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyHyperlinkToClipboard_Winzor_NoUrlsSet_ExpectEntent()
		{
			using (RawDataRegistry.Instance.WebHyperlinksEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (WebDataRegistry.Instance.RootServicesUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				testForm.ControllerID = DummyControllerIDs.Dummy;

				// Act
				SafeClipboard.Clear();
				ZFormMenuStrategy.CopyHyperlinkToClipboard(testForm);

				// Asserts
				var valueText = (string)SafeClipboard.GetData(DataFormats.Text);
				var valueHtml = (string)SafeClipboard.GetData(DataFormats.Html);
				var valueRtf = (string)SafeClipboard.GetData(DataFormats.Rtf);

				SafeClipboard.Clear();

				AssertEquals("DummyBizo", valueText);
				AssertContains("href", valueHtml);
				AssertContains("edient:", valueHtml);

				AssertNullOrEmpty(valueRtf);    // No RTF for Winzor
			}
		}
#endif

		[DeveloperOnlyTest]
		public void TestCopyHumanReadableNameToClipboard()
		{
			//using (var testForm = new ZForm(Factory.New<DummyBusinessObject>()))
			using (var testForm = new ZTestForm(Factory.New<DummyBusinessObject>()))
			{
				ZFormMenuStrategy.CopyHumanReadableNameToClipboard(testForm);
				var dataObject = SafeClipboard.GetDataObject();
				var value = dataObject != null ? (string)dataObject.GetData(typeof(string)) : null;
				AssertEquals("DummyBizo", value);
			}
		}

		#region PromptForInactivation

		public void TestPromptForInactivation_SetToInactive()
		{
			var bizo = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();

			using (var form = new RequireInactivationPromptStubForm(bizo))
			{
				IFileMenuItemsProvider menuItemsProvider = form;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;

				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNull("Should not contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNotNull("Should contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");
				makeInactive.PerformClick();

				Assert("Stub.PromptForInvalidation() should have been hit", form.WasHit);
			}
		}

		public void TestPromptForInactivation_SetToActive()
		{
			var bizo = Factory.New<ZFormTest.DummyCancellableWhichCanBeCancelled>();
			bizo.IsCancelled = true;

			using (var form = new RequireInactivationPromptStubForm(bizo))
			{
				IFileMenuItemsProvider menuItemsProvider = form;
				var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;

				AssertNotNull("Precondition: Actions menu item should exist:", actionsMenuItem);
				AssertNotNull("Should contain Make Active menu item", actionsMenuItem.MenuItems.FindByText("Make Active"));
				AssertNull("Should not contain Make Inactive menu item", actionsMenuItem.MenuItems.FindByText("Make Inactive"));

				var makeActive = actionsMenuItem.MenuItems.FindByText("Make Active");
				makeActive.PerformClick();

				Assert("Stub.PromptForInvalidation() shouldn't have been hit", !form.WasHit);
			}
		}

		#endregion

		#region Implementation

		void AssertExceptionReported(string expectedButtonName)
		{
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(expectedButtonName + " was null when menu item click was raised. Check your SetupPostingButton logic. Form: Enterprise.ZArchitecture.GUI.ZForm. Display mode: Undefined", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public static void AssertActionsMenuItemsNotAvailableInViewMode(ZForm form)
		{
			using (form)
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				actionsMenu.OnPopup(EventArgs.Empty);

				var anyNonDefaultActionsEnabled = actionsMenu.MenuItems
					.Cast<MenuItem>()
					.Any((m) => m.Enabled && !ZFormMenuStrategy.IsDefaultActionMenuItemName(m.Name) && !ZFormMenuStrategy.IsAlwaysEnabledActionMenuItemName(m.Name));

				AssertEquals("Only default actions available in View mode", false, anyNonDefaultActionsEnabled);
			}
		}

		class RequireInactivationPromptStubForm : ZForm, IRequireInactivationPrompt
		{
			public RequireInactivationPromptStubForm(BusinessObject bizo) : base(bizo)
			{
			}

			public void PromptForInactivation()
			{
				WasHit = true;
			}

			public bool WasHit { get; private set; }
		}

		#endregion
	}
}
