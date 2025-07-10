using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	#region Interfaces

	public interface IFileMenuItemsProvider
	{
		MenuItem FileMenuItem { get; set; }
		MenuItem EditMenuItem { get; set; }
		MenuItem ActionsMenuItem { get; set; }
		MenuItem HelpClientSpecificMenuItem { get; set; }
		MenuItem HelpMenuItem { get; set; }

		MainMenu MainMenu { get; set; }
	}

	#endregion

	public static class ZMenuStrategyHelper
	{
		public static ShortcutCreator ShortcutCreator
		{
			get { return shortcutCreator ??= new ShortcutCreator(); }
		}

		[ThreadStatic]
		static ShortcutCreator shortcutCreator;

		public static void CopyIdToClipboard(BusinessObject bizo)
		{
			if (bizo == null)
			{
				return;
			}

			try
			{
				string id = CodePropertyAttribute.CodeFromBusinessObject(bizo);
				ShortcutCreator.CopyTextToClipboard(id);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
		}

		public static void CopyHyperlinkToClipboard(string humanReadableName, string url)
		{
			var useWebHyperlinks = ShortcutCreator.DoesUseWebHyperlinks();
			if (useWebHyperlinks)
			{
				ShortcutCreator.CopyWebHyperlinkToClipboard(humanReadableName, url);
			}
			else
			{
				ShortcutCreator.CopyHyperlinkToClipboard(humanReadableName, url);
			}
		}
	}

	public static class ZFormMenuStrategy
	{
		#region Menu items captions

		public const string FileMenuItemName = "FileMenuItem";
		public const string FileNewMenuItemName = "FileNewMenuItem";
		public const string FileSaveMenuItemName = "FileSaveMenuItem";
		public const string FileSaveAndCloseMenuItemName = "FileSaveAndCloseMenuItem";
		public const string FileDeleteMenuItemName = "FileDeleteMenuItem";
		public const string FileReloadMenuItemName = "FileReloadMenuItem";
		public const string FileCloseMenuItemName = "FileCloseMenuItem";
		public const string ActionsMenuItemName = "ActionsMenuItem";
		public const string HelpMenuItemName = "HelpMenuItem";
		public const string HelpIndexMenuItemName = "HelpIndexMenuItem";
		public const string HelpContentsMenuItemName = "HelpContentsMenuItem";
		public const string HelpAboutMenuItemName = "HelpAboutMenuItem";
		public const string ValidateMenuItemName = "ValidateMenuItem";

		public const string FileSeperator1MenuItemName = "FileSeperator1MenuItem";
		public const string FileSeperator2MenuItemName = "FileSeperator2MenuItem";
		public const string FileSeperator3MenuItemName = "FileSeperator3MenuItem";

		public const string CopyFormToClipboardMenuItemName = "CopyFormToClipboardMenuItem";
		public const string CopyHyperlinkToClipboardName = "CopyHyperlinkToClipboard";
		public const string CopyIdToClipboardName = "CopyIdToClipboard";
		public const string CopyHumanReadableNameToClipboardName = "CopyHumanReadableNameToClipboard";
		public const string CreateDesktopShortcutName = "CreateDesktopShortcut";
		public const string ResetFormSizeToDefaultName = "ResetFormSizeToDefault";
		public const string MakeInactiveName = "MakeInactiveName";
		public const string MakeActiveName = "MakeActiveName";

		#endregion

		public static void AddAdornments(ZForm form)
		{
			InitialiseMainMenu(form);
			InitializeCopyToClipboardActionsMenuItem(form);
			InitializeFavoriteActionsMenuItem(form);
		}

		#region Initialize main menu

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Design mode placeholder only, Developer Exception Text")]
		static void InitialiseMainMenu(ZForm form)
		{
			var mainMenu = new ZMainMenu();
			form.Menu = mainMenu;

			MenuItem fileMenuItem = new ZMenuItem(ResString.GetMultilingualString("File", "&File"));
			MenuItem fileNewMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.New", "&New"));
			MenuItem fileSeperator1MenuItem = new ZMenuItem();
			MenuItem fileSaveMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.Save", "&Save"));
			MenuItem fileSaveAndCloseMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.SaveClose", "S&ave && Close"));
			MenuItem fileSeperator2MenuItem = new ZMenuItem();
			MenuItem fileDeleteMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.Delete", "&Delete"));
			MenuItem fileReloadMenuItem = CreateReloadMenuItem(form);
			MenuItem fileSeperator3MenuItem = new ZMenuItem();
			MenuItem fileCloseMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.Close", "&Close"));

			MenuItem editMenuItem = new ZEditMenuItem(form);
			MenuItem actionsMenuItem = new ZMenuItem(ResString.GetMultilingualString("Actions", "Actio&ns"));
			var helpMenuItem = DesignModeFinder.IsDesigning ? new ZMenuItem("Help Menu PlaceHolder") : new ZHelpMenu(form);

			var menuItemsProvider = form as IFileMenuItemsProvider;
			if (menuItemsProvider != null)
			{
				menuItemsProvider.MainMenu = mainMenu;

				menuItemsProvider.FileMenuItem = fileMenuItem;
				menuItemsProvider.EditMenuItem = editMenuItem;
				menuItemsProvider.ActionsMenuItem = actionsMenuItem;
				menuItemsProvider.HelpMenuItem = helpMenuItem;
			}

			form.Menu.MenuItems.AddRange(new[] { fileMenuItem, editMenuItem, actionsMenuItem, helpMenuItem });

			fileMenuItem.Index = 0;
			fileMenuItem.MenuItems.AddRange(
				new[]
					{
						fileNewMenuItem, fileSeperator1MenuItem, fileSaveMenuItem, fileSaveAndCloseMenuItem, fileSeperator2MenuItem,
						fileDeleteMenuItem, fileReloadMenuItem, fileSeperator3MenuItem, fileCloseMenuItem
					});
			fileMenuItem.Name = FileMenuItemName;

			var postingButtonsProvider = form as IPostingButtonsProvider;

			fileNewMenuItem.Index = 0;
			fileNewMenuItem.Shortcut = Shortcut.CtrlN;
			fileNewMenuItem.Name = FileNewMenuItemName;
			fileNewMenuItem.Enabled = false;
			if (postingButtonsProvider != null)
			{
				fileNewMenuItem.Click += ((sender, e) => RaiseButtonClick(form, postingButtonsProvider.CommandButtonApply, "fApplyButton(FileNewMenuItem)"));
			}

			fileSeperator1MenuItem.Index = 1;
			fileSeperator1MenuItem.Text = "-";
			fileSeperator1MenuItem.Name = FileSeperator1MenuItemName;

			fileSaveMenuItem.Index = 2;
			fileSaveMenuItem.Shortcut = Shortcut.CtrlS;
			fileSaveMenuItem.Name = FileSaveMenuItemName;
			fileSaveMenuItem.Enabled = false;
			if (postingButtonsProvider != null)
			{
				fileSaveMenuItem.Click += delegate
				{
					if (postingButtonsProvider.CommandButtonApply == null || (postingButtonsProvider.CommandButtonApply != null && postingButtonsProvider.CommandButtonApply.Text != fileNewMenuItem.Text))
					{
						RaiseButtonClick(form, postingButtonsProvider.CommandButtonApply, "fApplyButton(FileSaveMenuItem)");
					}
				};
			}

			fileSaveAndCloseMenuItem.Index = 3;
			fileSaveAndCloseMenuItem.Shortcut = Shortcut.CtrlShiftS;
			fileSaveAndCloseMenuItem.Name = FileSaveAndCloseMenuItemName;
			fileSaveAndCloseMenuItem.Enabled = false;
			if (postingButtonsProvider != null)
			{
				fileSaveAndCloseMenuItem.Click += ((sender, e) => RaiseButtonClick(form, postingButtonsProvider.CommandButtonPost, "fPostButton(FileSaveAndCloseMenuItem)"));
			}

			fileSeperator2MenuItem.Index = 4;
			fileSeperator2MenuItem.Text = "-";
			fileSeperator2MenuItem.Name = FileSeperator2MenuItemName;

			fileDeleteMenuItem.Index = 5;
			fileDeleteMenuItem.Text = GetFileDeleteMenuItemText(form);
			fileDeleteMenuItem.Name = FileDeleteMenuItemName;
			fileDeleteMenuItem.Enabled = false;
			if (postingButtonsProvider != null)
			{
				fileDeleteMenuItem.Click += ((sender, e) => RaiseButtonClick(form, postingButtonsProvider.CommandButtonPost, "fPostButton(FileDeleteMenuItem)"));
			}

			fileReloadMenuItem.Index = 6;

			fileSeperator3MenuItem.Index = 7;
			fileSeperator3MenuItem.Text = "-";
			fileSeperator3MenuItem.Name = FileSeperator3MenuItemName;

			fileCloseMenuItem.Index = 8;
			fileCloseMenuItem.Shortcut = Shortcut.CtrlQ;
			fileCloseMenuItem.Name = FileCloseMenuItemName;
			fileCloseMenuItem.Enabled = false;
			if (postingButtonsProvider != null)
			{
				fileCloseMenuItem.Click += ((sender, e) => RaiseButtonClick(form, postingButtonsProvider.CommandButtonCancel, "fCancelButton"));
			}

			editMenuItem.Index = 1;

			actionsMenuItem.Enabled = true;
			actionsMenuItem.Index = 2;
			actionsMenuItem.Name = ActionsMenuItemName;

			helpMenuItem.Index = 3;
			helpMenuItem.Name = HelpMenuItemName;
		}

		public static string GetFileDeleteMenuItemText(Form form)
		{
			var deleteTextOverride = form as IButtonDeleteTextOverride;
			return (deleteTextOverride != null && !string.IsNullOrEmpty(deleteTextOverride.DeleteButtonText))
					? deleteTextOverride.DeleteButtonText
					: Res.GetString("File.Delete", "&Delete");
		}

		#region Add Actions Menu Item

		public static MenuItem AddActionsMenuItem(IFileMenuItemsProvider form, string text, EventHandler handler)
		{
			return AddActionsMenuItem(form.ActionsMenuItem, text, handler);
		}

		static MenuItem AddActionsMenuItem(MenuItem actionsMenu, string text, EventHandler handler)
		{
			return AddActionsMenuItem(actionsMenu, new ZMenuItem(text, handler));
		}

		public static MenuItem AddActionsMenuItem(IFileMenuItemsProvider form, MultilingualString text, EventHandler handler)
		{
			return AddActionsMenuItem(form.ActionsMenuItem, text, handler);
		}

		static MenuItem AddActionsMenuItem(MenuItem actionsMenu, MultilingualString text, EventHandler handler)
		{
			return AddActionsMenuItem(actionsMenu, new ZMenuItem(text, handler));
		}

		public static MenuItem AddActionsMenuItem(IFileMenuItemsProvider form, MenuItem newItem)
		{
			return AddActionsMenuItem(form.ActionsMenuItem, newItem);
		}

		public static void AddActionsMenuItem(IFileMenuItemsProvider form, IEnumerable<MenuItem> menuItems)
		{
			foreach (var menuItem in menuItems)
			{
				AddActionsMenuItem(form.ActionsMenuItem, menuItem);
			}
		}

		static MenuItem AddActionsMenuItem(MenuItem actionsMenu, MenuItem newItem)
		{
			if (actionsMenu != null)
			{
				actionsMenu.MenuItems.Add(newItem);
			}
			return newItem;
		}

		public static void AddActionsMenuItem(Form form)
		{
			var menuItemsProvider = form as IFileMenuItemsProvider;
			var actionsMenuItem = menuItemsProvider != null ? menuItemsProvider.ActionsMenuItem : null;

			if (actionsMenuItem == null)
			{
				return;
			}

			AddResetFormSizeMenuItem(form, actionsMenuItem);
			actionsMenuItem.MenuItems.Add("-");

			var zform = form as ZForm;
			if (zform == null)
			{
				return;
			}

			AddDataMenuItem(zform, actionsMenuItem);
			AddActiveMenuItem(zform, actionsMenuItem);
			AddRelatableActivityMenuItems(zform);
			actionsMenuItem.Popup += delegate
			{
				InitializeExportToXmlActionMenuItem(form);
			};
		}

		public static void AddActionsMenuAfterFormIsLoaded(ZForm form)
		{
			if (form.BusinessEntity != null && form.ControllerID != null && ((IFileMenuItemsProvider)form).ActionsMenuItem != null)
			{
				if (!(form is ZChildForm))
				{
					AddUniversalCopyMenuItem(form);
				}

				AddOperationalActionsMenuItem(form);
			}
			AddSendEmailMenuItem(form);
		}

		static void AddUniversalCopyMenuItem(ZForm form)
		{
			var copyManager = ObjectFactory.New<IFormUniversalCopyManager>(form);
			if (copyManager.AllowsUniversalCopy)
			{
				copyManager.AddMenuItems();
			}
		}

		static void AddOperationalActionsMenuItem(ZForm form)
		{
			ObjectFactory.Get<IModuleOperationalActionsHelper>().AddOperationalActionsIntoActionsMenu(form);
		}

		static void AddDataMenuItem(ZForm zform, MenuItem actionsMenuItem)
		{
			if (!zform.AllowActionDataMenuItem)
			{
				return;
			}

			var dataMenu = ActionDataMenuItem.New(zform);
			if (dataMenu == null)
			{
				return;
			}

			actionsMenuItem.MenuItems.Add(dataMenu);
			actionsMenuItem.Popup += delegate
			{
				dataMenu.Populate();
				if (dataMenu.MenuItems.Count > 0)
				{
					actionsMenuItem.MenuItems.Add(dataMenu);
				}
				else
				{
					dataMenu.Dispose();
				}
			};
		}

		static void AddResetFormSizeMenuItem(Form form, MenuItem actionsMenuItem)
		{
			actionsMenuItem.MenuItems.Add(GetResetFormSizeMenuItem(form));
		}

		public static ZMenuItem GetResetFormSizeMenuItem(Form form)
		{
			return new ZMenuItem(ResString.GetMultilingualString("Actions.ResetFormSize", "Reset Form Size and Layout to Default"),
										delegate
										{
											((WinFormsEnvironment)EnvProxy.Instance).FormRegistry.ClearFormLocationAndSize(form.Name);
											var splitContainerLayoutProvider = form as ISplitterLayoutProvider;
											if (splitContainerLayoutProvider != null)
											{
												foreach (var key in splitContainerLayoutProvider.Splitters.Keys)
												{
													((IWinFormsEnvironment)EnvProxy.Instance).SplitterLayoutRegistry.ClearSplitterLayout(key);
												}
												splitContainerLayoutProvider.RememberSplitterLayout = false;
											}
											Globals.Message.ShowInformation(Res.GetString("665fd774-13fd-40d4-9adb-bda65e517735", "After you close and re-open this form, default form layout will be restored."), Res.GetString("0dcab1d9-1865-4525-b38c-1d1c614a2344", "Form Layout"));
										})
			{ Name = ResetFormSizeToDefaultName };
		}

		static void AddActiveMenuItem(ZForm zform, MenuItem actionsMenuItem)
		{
			var cancellable = zform.GetICancellable(zform.BusinessEntityForValidation);

			if (cancellable == null || !PreventDeleteAttribute.IsTrue(cancellable.GetType()) || cancellable.IsCancelledHasChanged)
			{
				return;
			}

			var menuItemText = (cancellable.IsCancelled) ? ResString.GetMultilingualString("4920b4d3-dfaf-4087-b1e5-903792ab3078", "Make Active") : ResString.GetMultilingualString("c2e36eee-16ad-47dc-8f40-de08099a216d", "Make Inactive");
			var menuItmeName = (cancellable.IsCancelled) ? MakeActiveName : MakeInactiveName;
			actionsMenuItem.MenuItems.Add(new ZMenuItem(menuItemText,
											delegate
											{
												var controller = (zform.ControllerID != null) ? ZControllerFactory.Create(zform.ControllerID) : null;
												if (controller != null)
												{
													controller.ParentModule = zform.GetModule();
												}

												var checkpoint = controller?.GetCheckPointForDelete(zform.BusinessEntity as BusinessObject);

												if (checkpoint == null || checkpoint.IsAllowed)
												{
													var isCancelled = cancellable.IsCancelled;
													var canCancel = cancellable.CanCancel();
													var canReactivate = cancellable.CanReactivate();
													if (!isCancelled && string.IsNullOrEmpty(canCancel) ||
														isCancelled && string.IsNullOrEmpty(canReactivate))
													{
														isCancelled = !isCancelled;
														cancellable.IsCancelled = isCancelled;
														if (!isCancelled)
														{
															(cancellable as BusinessObject).MarkAsNeedingValidation();
														}
														else if (zform is IRequireInactivationPrompt formRequiringPrompt)
														{
															formRequiringPrompt.PromptForInactivation();
														}

														if (zform.PlugIns != null)
														{
															zform.PlugIns.OnBusinessObjectIsCancelledChanged(isCancelled);
														}
														zform.DisplayMode = ODisplayMode.Delete;
														ZFormPostingButtonsStrategy.UpdateSaveButtonsBasedOnHasChanges(zform);
													}
													else
													{
														if (isCancelled)
														{
															Globals.Message.Show(canReactivate, Res.GetString("78216213-293c-4ecb-9eb4-c52dba28137a", "Cannot reactivate"), MessageBoxButtons.OK, DialogResult.OK);
														}
														else
														{
															Globals.Message.Show(canCancel, Res.GetString("88216213-293c-4ecb-9eb4-c52dba28137a", "Cannot cancel/deactivate"), MessageBoxButtons.OK, DialogResult.OK);
														}
													}
												}
												else
												{
													checkpoint.ShowError();
												}
											})
			{
				Name = menuItmeName,
			});
		}

		static void AddRelatableActivityMenuItems(ZForm form)
		{
			ObjectFactory.Get<IRelatableActivityActionMenuStrategy>().AddRelatableActivityMenuItemsIfApplicable(form);
		}

		static void AddSendEmailMenuItem(Form form)
		{
			ObjectFactory.Get<ISendEmailActionMenuStrategy>().AddSendEmailActionMenuIfApplicable(form);
		}

		public static void AddInterfaceConnectorMenuItems(IFileMenuItemsProvider form, List<MenuItem> menuItems, bool temporarilyAllow = false)
		{
			foreach (var item in menuItems)
			{
				AddInterfaceConnectorMenuItem(form, item, temporarilyAllow);
			}
		}

		public static void AddInterfaceConnectorMenuItem(List<MenuItem> menuItems, MenuItem item, bool temporarilyAllow = false)
		{
			if (!temporarilyAllow)
			{
				if (HasInterfaceConnector)
				{
					menuItems.Add(item);
				}
			}
			else
			{
				menuItems.Add(item);
			}
		}

		public static void AddInterfaceConnectorMenuItem(MenuItem parentItem, MenuItem itemToBeAdded, bool temporarilyAllow = false)
		{
			if (!temporarilyAllow)
			{
				if (HasInterfaceConnector)
				{
					parentItem.MenuItems.Add(itemToBeAdded);
				}
			}
			else
			{
				parentItem.MenuItems.Add(itemToBeAdded);
			}
		}

		public static void AddInterfaceConnectorMenuItem(IFileMenuItemsProvider form, MenuItem item, bool temporarilyAllow = false)
		{
			if (!temporarilyAllow)
			{
				if (HasInterfaceConnector)
				{
					AddActionsMenuItem(form, item);
				}
			}
			else
			{
				AddActionsMenuItem(form, item);
			}
		}

		public static bool HasInterfaceConnector
		{
			get { return Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}

		#endregion

		#region Add Validate Menu Item

		public static void AddValidateMenuItem(Form form)
		{
			var validateMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.ValidateAll", "&Validate All"));
			validateMenuItem.Name = ValidateMenuItemName;
			var menuItem2 = new ZMenuItem("-");
			var fileMenuItem = form.Menu.MenuItems[FileMenuItemName];
			fileMenuItem.MenuItems.Add(2, validateMenuItem);
			fileMenuItem.MenuItems.Add(3, menuItem2);

			var zform = form as ZForm;
			if (zform != null)
			{
				validateMenuItem.Click +=
					delegate
					{
						zform.RunActionWithProcessBox(ZForm.LoadingValidationCodeText, () => { zform.ValidateAll(ValidationType.Full); });
					};
			}
		}

		#endregion

		#region Add Reload Menu Item

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "The reloadMenuItem will be disposed with the form")]
		public static ZMenuItem CreateReloadMenuItem(ZForm form)
		{
			var reloadMenuItem = new ZMenuItem(ResString.GetMultilingualString("File.ReloadForm", "&Reload this form"));
			reloadMenuItem.Name = FileReloadMenuItemName;
			reloadMenuItem.Shortcut = Shortcut.ShiftF5;
			reloadMenuItem.Enabled = false;
			reloadMenuItem.Click += (o, e) => ZFormUtilities.ReloadCurrentForm(form);

			return reloadMenuItem;
		}

		#endregion

		#endregion

		#region Copying to Clipboard

		static void InitializeCopyToClipboardActionsMenuItem(ZForm form)
		{
			var menuItemsProvider = form as IFileMenuItemsProvider;
			if (menuItemsProvider != null && menuItemsProvider.ActionsMenuItem != null)
			{
				AddEntityHyperlinkActionMenuItems(form);
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Clipboard.CopyForm", "Copy Form to Clipboard"), delegate { CopyFormToClipboard(form); }) { Name = CopyFormToClipboardMenuItemName });
				menuItemsProvider.ActionsMenuItem.MenuItems.Add("-");
			}
		}

		static void AddEntityHyperlinkActionMenuItems(ZForm form)
		{
			var menuItemsProvider = form as IFileMenuItemsProvider;
			if (menuItemsProvider != null)
			{
				var copyLinkToClipboardItem = new ZMenuItem(ResString.GetMultilingualString("Clipboard.CopyHyperlink", "Copy Hyperlink to Clipboard"), delegate
				{ CopyHyperlinkToClipboard(form); })
				{
					Name = CopyHyperlinkToClipboardName,
					Shortcut = Shortcut.CtrlH,
				};
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(copyLinkToClipboardItem);

				var copyIdToClipboardItem = new ZMenuItem(
					ResString.GetMultilingualString("Clipboard.CopyID", "Copy ID to Clipboard"),
					delegate
					{
						ZMenuStrategyHelper.CopyIdToClipboard(ZFormUtilities.GetBusiness(form) as BusinessObject);
					})
				{
					Name = CopyIdToClipboardName
				};
				copyIdToClipboardItem.Shortcut = Shortcut.CtrlJ;
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(copyIdToClipboardItem);

				var copyHumanReadableNameToClipboardItem = new ZMenuItem(
					ResString.GetMultilingualString("Clipboard.HumanReadableName", "Copy Name to Clipboard"),
					delegate
					{
						CopyHumanReadableNameToClipboard(form);
					})
				{
					Name = CopyHumanReadableNameToClipboardName
				};
				copyHumanReadableNameToClipboardItem.Shortcut = Shortcut.CtrlShiftJ;
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(copyHumanReadableNameToClipboardItem);

				var createShortcutItem = new ZMenuItem(
#if WINZOR
					ResString.GetMultilingualString("Actions.DownloadShortcut", "Download Shortcut"),
#else
							ResString.GetMultilingualString("Actions.CreateDesktopShortcut", "Create Desktop Shortcut"),
#endif

					delegate
					{
						var shortcutUrl = ZFormUtilities.BusinessEntityShortcutUrl(form, useWebHyperlinks: false);
						ZMenuStrategyHelper.ShortcutCreator.CreateDesktopShortcut(ZFormUtilities.GetBusiness(form).HumanReadableName, shortcutUrl);
					})
				{
					Name = CreateDesktopShortcutName
				};

				menuItemsProvider.ActionsMenuItem.MenuItems.Add(createShortcutItem);

				var zform = form as IZForm;

				menuItemsProvider.ActionsMenuItem.Popup +=
					delegate
					{
						var businessObject = ZFormUtilities.GetBusiness(form) as BusinessObject;
						var allowLinkingToBusinessObject = (zform == null || zform.ControllerID != null) && businessObject != null && businessObject.IsInDatabase;
						createShortcutItem.Visible = allowLinkingToBusinessObject;
						copyLinkToClipboardItem.Visible = allowLinkingToBusinessObject;
						copyIdToClipboardItem.Visible = !string.IsNullOrEmpty(SafeCodeFromBusinessObject(businessObject));
					};
			}
		}

		static string SafeCodeFromBusinessObject(BusinessObject bizo)
		{
			string result = null;
			if (bizo != null)
			{
				try
				{
					result = CodePropertyAttribute.CodeFromBusinessObject(bizo);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}

			return result;
		}

		public static void CopyHyperlinkToClipboard(ZForm form, IBusiness business = null)
		{
			var humanReadableName = GetHumanReadableName(form, business);
			var url = ZFormUtilities.BusinessEntityShortcutUrl(form, ShortcutCreator.DoesUseWebHyperlinks());
			if (string.IsNullOrEmpty(url))
			{
				return;
			}

			ZMenuStrategyHelper.CopyHyperlinkToClipboard(humanReadableName, url);
		}

		public static void CopyHumanReadableNameToClipboard(ZForm form, IBusiness business = null)
		{
			var humanReadableName = GetHumanReadableName(form, business);
			ShortcutCreator.CopyTextToClipboard(humanReadableName);
		}

		static string GetHumanReadableName(ZForm form, IBusiness business = null)
		{
			business ??= ZFormUtilities.GetBusiness(form);
			return business.HumanReadableName;
		}

		static void CopyFormToClipboard(Form form)
		{
			try
			{
#if WINZOR
				form.InvokeRenderDispatcher(async () =>
				{
					await form.CargoWiseClientServices?.WindowService.ScreenShotAsync(
					new CargoWise.Blazor.Client.Integration.Messaging.ScreenShotSetting()
					{
						Action = CargoWise.Blazor.Client.Integration.Messaging.ScreenShotAction.CopyToClipboard
					});
				});
#else
				var image = ZScreenShotGrabber.Capture(form);
				if (!SafeClipboard.SetDataObject(image))
				{
					Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
				}
#endif
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		#endregion

		#region Initialize Favorites Menu Item
		[SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		static void InitializeFavoriteActionsMenuItem(ZForm form)
		{
			var menuItemsProvider = form as IFileMenuItemsProvider;
			if (menuItemsProvider != null)
			{
				var addToFavoritesItem = new ZMenuItem(ResString.GetMultilingualString("Actions.AddToFavorite", "Add to Favorites"), (s, e) =>
				{
					var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
					if (linkWrapper != null)
					{
						ObjectFactory.Get<IFavoriteProvider>().AddToFavorites(linkWrapper);
					}
				});
				addToFavoritesItem.Name = "Actions.AddToFavoriteMenuItem";

				var removeFromFavoritesItem = new ZMenuItem(ResString.GetMultilingualString("Actions.DeleteFromFavorite", "Remove from Favorites"), (s, e) =>
				{
					var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
					if (linkWrapper != null)
					{
						ObjectFactory.Get<IFavoriteProvider>().DeleteFromFavorites(linkWrapper);
					}
				});
				removeFromFavoritesItem.Name = "Actions.DeleteFromFavoriteMenuItem";

				menuItemsProvider.ActionsMenuItem.MenuItems.Add(addToFavoritesItem);
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(removeFromFavoritesItem);
				menuItemsProvider.ActionsMenuItem.MenuItems.Add(new MenuItem { Text = "-" });

				menuItemsProvider.ActionsMenuItem.Popup += (s, e) =>
				{
					var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
					if (linkWrapper == null)
					{
						addToFavoritesItem.Visible = true;
						addToFavoritesItem.Enabled = false;
						removeFromFavoritesItem.Visible = false;
						removeFromFavoritesItem.Enabled = false;
					}
					else
					{
						var inFavorites = ObjectFactory.Get<IFavoriteProvider>().IsInFavorites(linkWrapper);
						addToFavoritesItem.Visible = !inFavorites;
						addToFavoritesItem.Enabled = !inFavorites;
						removeFromFavoritesItem.Visible = inFavorites;
						removeFromFavoritesItem.Enabled = inFavorites;
					}
				};
			}
		}

		#endregion

		#region Export To Xml

		static void InitializeExportToXmlActionMenuItem(Form form)
		{
			var menuItemsProvider = form as IFileMenuItemsProvider;
			if (menuItemsProvider != null && menuItemsProvider.ActionsMenuItem != null)
			{
				AddExportToNativeXmlActionMenuItem(form);
			}
		}

		static void AddExportToNativeXmlActionMenuItem(Form form)
		{
			var nativeXmlActionMenuStrategy = new ZFormNativeXMLActionMenuStrategy();
			nativeXmlActionMenuStrategy.Exporter = ObjectFactory.GetDesignerSafe<IExportService>("NativeXmlExportService");
			nativeXmlActionMenuStrategy.ExportValidator = ObjectFactory.GetDesignerSafe<IExportValidator>("NativeXmlExportValidator");
			nativeXmlActionMenuStrategy.AddAdornments(form);
		}

		#endregion

		#region Implementation

		static void RaiseButtonClick(Form form, IButton button, string fieldName)
		{
			if (button != null)
			{
				button.PerformClick();
			}
			else
			{
				var zform = form as ZForm;
				var displayMode = zform != null ? zform.DisplayMode.ToString() : string.Empty;
				Globals.Message.ShowDeveloperErrorOnce("Null" + fieldName + "Button." + form.GetType().FullName, fieldName + " was null when menu item click was raised. Check your SetupPostingButton logic. Form: " + form.GetType().FullName + ". Display mode: " + displayMode, ""); // Developer Exception Text
			}
		}

		#endregion

		#region Do Display Mode

		static void SetEnableMenuItem(Menu menu, string menuItemKey, string subMenuItemKey, bool isEnabled)
		{
			var menuItem = menu.MenuItems[menuItemKey];
			if (menuItem != null)
			{
				menuItem = menuItem.MenuItems[subMenuItemKey];
				if (menuItem != null)
				{
					menuItem.Enabled = isEnabled;
				}
			}
		}

		internal static void DoDisplayModeNew(Form form)
		{
			if (form.Menu != null)
			{
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileCloseMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileNewMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileDeleteMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, false);
			}
		}

		internal static void DoDisplayModeNewSaved(Form form)
		{
			if (form.Menu != null)
			{
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileCloseMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileNewMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileDeleteMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, true);
			}
		}

		internal static void DoDisplayModeBrowse(Form form)
		{
			DoDisplayModeNew(form);

			if (form.Menu != null)
			{
				var buttonsProvider = form as IPostingButtonsProvider;
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileNewMenuItemName, buttonsProvider == null || buttonsProvider.AllowNew);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, true);
			}
		}

		internal static void DoDisplayModeEdit(Form form)
		{
			DoDisplayModeNew(form);

			if (form.Menu != null)
			{
				var buttonsProvider = form as IPostingButtonsProvider;
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, buttonsProvider == null || !buttonsProvider.IsPostOnly && buttonsProvider.CommandButtonApply != null);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, buttonsProvider == null || buttonsProvider.CommandButtonPost != null);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, true);
			}
		}

		internal static void DoDisplayModeDelete(Form form)
		{
			if (form.Menu != null)
			{
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileCloseMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileNewMenuItemName, false);
				var buttonsProvider = form as IPostingButtonsProvider;
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileDeleteMenuItemName, buttonsProvider == null || buttonsProvider.CommandButtonPost != null);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, true);
			}
		}

		internal static void DoDisplayModeReadOnly(Form form)
		{
			if (form.Menu != null)
			{
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileSaveAndCloseMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileCloseMenuItemName, true);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileNewMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileDeleteMenuItemName, false);
				SetEnableMenuItem(form.Menu, FileMenuItemName, FileReloadMenuItemName, true);
			}
		}

		public static void DisableActionMenuItemsExcludingDefaultsInViewMode(ZForm form)
		{
			if (form.DisplayMode == ODisplayMode.ReadOnly)
			{
				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];

				if (actionsMenu != null)
				{
					foreach (MenuItem menuItem in actionsMenu.MenuItems)
					{
						if (!IsDefaultActionMenuItemName(menuItem.Name) && !IsAlwaysEnabledActionMenuItemName(menuItem.Name))
						{
							menuItem.Enabled = false;
						}
					}
				}
			}
		}

		public static IEnumerable<string> DefaultActionMenuItemNames
		{
			get
			{
				yield return CopyHyperlinkToClipboardName;
				yield return CreateDesktopShortcutName;
				yield return CopyFormToClipboardMenuItemName;
				yield return ResetFormSizeToDefaultName;
				yield return CopyIdToClipboardName;
			}
		}

		public static bool IsDefaultActionMenuItemName(string name)
		{
			return DefaultActionMenuItemNames.Any(n => n == name);
		}

		public static void FlagActionMenuAsAlwaysEnabled(string name)
		{
			AlwaysEnabledActionMenuItemNames.Add(name);
		}

		static HashSet<string> AlwaysEnabledActionMenuItemNames
		{
			get { return alwaysEnabledActionMenuItemNames.Value; }
		}
		static readonly Lazy<HashSet<string>> alwaysEnabledActionMenuItemNames = new Lazy<HashSet<string>>(() => new HashSet<string>());

		public static bool IsAlwaysEnabledActionMenuItemName(string name)
		{
			return AlwaysEnabledActionMenuItemNames.Contains(name);
		}

		#endregion

		#region MenuItem Popup

		public static void SetMainMenuItemsPopupEventHandler(Form form)
		{
			if (form.Menu != null)
			{
				foreach (MenuItem menuItem in form.Menu.MenuItems)
				{
					menuItem.Popup += delegate
					{ ZFormUtilities.EnsureSelectedControlValueCommitted(form); };
				}
			}
		}

		#endregion

		#region Find Menu Item

		static public void SetMenuItemText(Form form, string menuItemName, string text)
		{
			var menuItem = form.Menu.MenuItems.FindByName(menuItemName, true);
			if (menuItem != null)
			{
				menuItem.Text = text;
			}
		}

		static public void SetMenuItemText(Form form, string menuItemName, MultilingualString text)
		{
			var menuItem = form.Menu.MenuItems.FindByName(menuItemName, true) as ZMenuItem;
			if (menuItem != null)
			{
				menuItem.Caption = text;
			}
		}

		static public void SetMenuItemVisible(Form form, string menuItemName, bool vidible)
		{
			var menuItem = form.Menu.MenuItems.FindByName(menuItemName, true);
			if (menuItem != null)
			{
				menuItem.Visible = vidible;
			}
		}

		static public void SetMenuItemEnabled(Form form, string menuItemName, bool enabled)
		{
			var menuItem = form.Menu.MenuItems.FindByName(menuItemName, true);
			if (menuItem != null)
			{
				menuItem.Enabled = enabled;
			}
		}

		#endregion

		#region MenuItems Deferred Loading

		public delegate IEnumerable<MenuItem> MenuItemsProvider();

		/// <summary>
		///		Inserts items into the <paramref name="itemsCollection"/> at the specified index.
		/// </summary>
		/// <param name="itemsCollection">
		///		The collection into which to insert items.
		/// </param>
		/// <param name="index">
		///		The zero-based index at which items should be inserted.
		/// </param>
		/// <param name="itemsToAdd">
		///		Items to insert.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="itemsCollection"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<paramref name="index"/> is less then zero or greater then <see cref="Menu.MenuItemCollection.Count"/>.
		/// </exception>
		public static void InsertRange(this Menu.MenuItemCollection itemsCollection, int index, IEnumerable<MenuItem> itemsToAdd)
		{
			Argument.NotNull(itemsCollection, "itemsCollection");

			if (index < 0 || index > itemsCollection.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "The index should not exceed range of the collection where items should be inserted");
			}

			if (itemsToAdd != null)
			{
				foreach (var menuItem in itemsToAdd)
				{
					itemsCollection.Add(index, menuItem);
					index++;
				}
			}
		}

		/// <summary>
		///		Replaces the <paramref name="itemToReplace"/> with <paramref name="itemsToReplaceWith"/>.
		/// </summary>
		/// <param name="itemsCollection">
		///		The collection in which to do replace.
		/// </param>
		/// <param name="itemToReplace">
		///		The item to replace.
		/// </param>
		/// <param name="itemsToReplaceWith">
		///		Items to replace with. If collection is <c>null</c> or empty, the <paramref name="itemToReplace"/> will be just deleted from the <paramref name="itemsCollection"/>.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="itemsCollection"/> is <c>null</c> -OR-
		///		<paramref name="itemToReplace"/> is <c>null</c>.		
		/// </exception>
		public static void Replace(this Menu.MenuItemCollection itemsCollection, MenuItem itemToReplace, IEnumerable<MenuItem> itemsToReplaceWith)
		{
			Argument.NotNull(itemsCollection, "itemsCollection");
			Argument.NotNull(itemToReplace, "itemToReplace");

			var index = itemsCollection.IndexOf(itemToReplace);
			if (index != -1)
			{
				itemsCollection.RemoveAt(index);
				itemsCollection.InsertRange(index, itemsToReplaceWith);
			}
		}

		/// <summary>
		///		Initializes menu items provided by <paramref name="childItemsProvider"/> on idle or on demand.
		/// </summary>
		/// <param name="parentMenuItem">
		///		The parent (usually top-level menu item), where the items should be initialized.
		/// </param>
		/// <param name="owner">
		///		The parent control for <paramref name="parentMenuItem"/>.
		/// </param>
		/// <param name="childItemsProvider">
		///		The provider which actually initializes menu items.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> instance, if the items from <paramref name="childItemsProvider"/> where scheduled for adding. Please call 
		///		<see cref="IDisposable.Dispose"/> for cancelling adding these items; <c>null</c>, if the deffered items were added synchronously.
		/// </returns>
		/// <remarks>
		///		Note, if the <paramref name="owner"/> is disposed, all deferred menu items associated with it will be canceled automatically. 
		///		So, you don't need to cancel them manually.
		/// </remarks>
		public static IDisposable AddDeferredMenuItems(this MenuItem parentMenuItem, Control owner, MenuItemsProvider childItemsProvider)
		{
			Argument.NotNull(parentMenuItem, "parentMenuItem");
			Argument.NotNull(owner, "owner");
			Argument.NotNull(childItemsProvider, "childItemsProvider");
			IDisposable result = null;

			var placeHolder = new ZMenuItem();
			parentMenuItem.MenuItems.Add(placeHolder);

			var idleWorkerInfo = new IdleWorkerInfo();
			idleWorkerInfo.PlaceHolder = placeHolder;
			idleWorkerInfo.ParentMenuItem = parentMenuItem;
			idleWorkerInfo.Owner = owner;
			idleWorkerInfo.InitialiseChildItems = childItemsProvider;
			idleWorkerInfo.IdleWorker = UserIdleWorker.QueueWorkItem(owner, () => InitialiseChildItems(idleWorkerInfo));

			if (idleWorkerInfo.IdleWorker != null)
			{
				ScheduleWorker(idleWorkerInfo);

				result = new DisposableAction(() =>
				{
					FinaliseWorker(idleWorkerInfo);
				});
			}

			return result;
		}

		static void OwnerDisposed(object sender, EventArgs e)
		{
			var owner = (Control)sender;
			owner.Disposed -= OwnerDisposed;

			var relatedWorkers = IdleWorkers.Where(w => w.Owner == owner).ToList();
			foreach (var worker in relatedWorkers)
			{
				FinaliseWorker(worker);
			}
		}

		static void ParentMenuPopup(object sender, EventArgs e)
		{
			var parentMenuItem = (MenuItem)sender;
			var relatedWorkers = IdleWorkers.Where(p => p.ParentMenuItem == parentMenuItem).ToList();

			foreach (var worker in relatedWorkers)
			{
				InitialiseChildItems(worker);
			}
		}

		static void InitialiseChildItems(IdleWorkerInfo worker)
		{
			var childItems = worker.InitialiseChildItems();
			worker.ParentMenuItem.MenuItems.Replace(worker.PlaceHolder, childItems);

			if (worker.IdleWorker != null)
			{
				FinaliseWorker(worker);
			}
		}

		static void ScheduleWorker(IdleWorkerInfo worker)
		{
			if (!IdleWorkers.Any(w => w.Owner == worker.Owner))
			{
				worker.Owner.Disposed += OwnerDisposed;
			}

			if (!IdleWorkers.Any(w => w.ParentMenuItem == worker.ParentMenuItem))
			{
				worker.ParentMenuItem.Popup += ParentMenuPopup;
			}

			IdleWorkers.Add(worker);
		}

		static void FinaliseWorker(IdleWorkerInfo worker)
		{
			IdleWorkers.Remove(worker);

			worker.IdleWorker.Dispose();

			if (!worker.Owner.Disposing)
			{
				if (!IdleWorkers.Any(p => p.Owner == worker.Owner))
				{
					worker.Owner.Disposed -= OwnerDisposed;
				}

				if (!IdleWorkers.Any(p => p.ParentMenuItem == worker.ParentMenuItem))
				{
					worker.ParentMenuItem.Popup -= ParentMenuPopup;
				}
			}
		}

		[ThreadStatic]
		static List<IdleWorkerInfo> idleWorkers;

		static List<IdleWorkerInfo> IdleWorkers => idleWorkers ?? (idleWorkers = new List<IdleWorkerInfo>());

		class IdleWorkerInfo
		{
			public Control Owner
			{
				get;
				set;
			}

			public MenuItem PlaceHolder
			{
				get;
				set;
			}

			public MenuItem ParentMenuItem
			{
				get;
				set;
			}

			public IDisposable IdleWorker
			{
				get;
				set;
			}

			public MenuItemsProvider InitialiseChildItems
			{
				get;
				set;
			}
		}

		#endregion
	}
}
