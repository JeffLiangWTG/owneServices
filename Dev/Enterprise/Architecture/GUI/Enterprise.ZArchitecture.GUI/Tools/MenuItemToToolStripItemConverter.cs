using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Converts a legacy MenuItem object into a new ToolStripItem object.
	/// Copies text, event handlers, child items, visibility and icons.
	/// </summary>
	public static class MenuItemToToolStripItemConverter
	{
		public static ToolStripItem ConvertMenuItemToToolStripItem(MenuItem legacyMenuItem)
		{
			return ConvertMenuItemToToolStripItem(legacyMenuItem, null, false);
		}

		public static ToolStripItem ConvertMenuItemToToolStripItem(MenuItem legacyMenuItem, ZEmbeddedModule module)
		{
			return ConvertMenuItemToToolStripItem(legacyMenuItem, module, true);
		}

		public static ToolStripItem ConvertMenuItemToToolStripItem(MenuItem legacyMenuItem, ZEmbeddedModule module, bool isTopLevel)
		{
			ToolStripItem toolstripItem = null;

			if (legacyMenuItem != null)
			{
#if NET || WINZOR // Winzor is net core, but this makes it explicit that we have considered how Winzor handles this section
				const string clickEventHandlerName = "_onClick";
				const string popupEventHandlerName = "_onPopup";

#else
				const string clickEventHandlerName = "onClick";
				const string popupEventHandlerName = "onPopup";
#endif

				var clickHandler = GetEventHandlerFromMenuItem(legacyMenuItem, clickEventHandlerName);

				if (legacyMenuItem.MenuItems.Count > 0)
				{
					if (isTopLevel)
					{
						toolstripItem = new ZToolStripSplitButton() { Text = legacyMenuItem.Text };
						if (clickHandler != null)
						{
							((ToolStripSplitButton)toolstripItem).ButtonClick += clickHandler;
						}
						else
						{
							((ToolStripSplitButton)toolstripItem).ButtonClick += delegate
							{
								((ToolStripSplitButton)toolstripItem).ShowDropDown();
							};
						}
					}
					else
					{
						toolstripItem = new ZToolStripMenuItem() { Text = legacyMenuItem.Text, ShowShortcutKeys = true, ShortcutKeys = (Keys)legacyMenuItem.Shortcut, Tag = legacyMenuItem.Tag };
#if WINZOR
						toolstripItem.Font = legacyMenuItem.Font;
#endif
						toolstripItem.Click += clickHandler;
					}

					// Copy PopUp event and also reset items after drop-down opening event fired
					var popupHandler = GetEventHandlerFromMenuItem(legacyMenuItem, popupEventHandlerName);
					((ToolStripDropDownItem)toolstripItem).DropDownOpening += popupHandler;

					if (popupHandler != null)
					{
						((ToolStripDropDownItem)toolstripItem).DropDownOpening +=
							delegate
							{
								if (legacyMenuItem.MenuItems.Count == 1 && String.IsNullOrEmpty(legacyMenuItem.MenuItems[0].Text))
								{
									return;
								}

								for (var i = ((ToolStripDropDownItem)toolstripItem).DropDown.Items.Count - 1; i >= 0; i--)
								{
									((ToolStripDropDownItem)toolstripItem).DropDown.Items[i].Dispose();
								}

								foreach (MenuItem addedItem in legacyMenuItem.MenuItems)
								{
									var childItem = ConvertMenuItemToToolStripItem(addedItem, module, false);
									((ToolStripDropDownItem)toolstripItem).DropDown.Items.Add(childItem);
								}
							};
					}

					foreach (MenuItem legacyChildItem in legacyMenuItem.MenuItems)
					{
						var childItem = ConvertMenuItemToToolStripItem(legacyChildItem, module, false);
						childItem.Text = KMenuItem.StripAcceleratorKeysButKeepAmpersandInText(childItem.Text);
						((ToolStripDropDownItem)toolstripItem).DropDown.Items.Add(childItem);
					}
				}
				else if (legacyMenuItem.Text == "-")
				{
					toolstripItem = new ToolStripSeparator();
				}
				else
				{
					if (isTopLevel)
					{
						toolstripItem = new ZToolStripButton() { Text = legacyMenuItem.Text };
						toolstripItem.Click += clickHandler;
					}
					else
					{
						toolstripItem = new ZToolStripMenuItem() { Text = legacyMenuItem.Text, ShowShortcutKeys = true, ShortcutKeys = (Keys)legacyMenuItem.Shortcut, Tag = legacyMenuItem.Tag };
						toolstripItem.Click += clickHandler;
					}
				}

				SetNewItemImageAndVisibility(toolstripItem, legacyMenuItem, module);

				toolstripItem.Enabled = legacyMenuItem.Enabled;
				if (toolstripItem is IConvertedFromMenuItem)
				{
					((IConvertedFromMenuItem)toolstripItem).SourceMenuItem = legacyMenuItem;
				}
			}

			return toolstripItem;
		}

		#region Implementation

		static void SetNewItemImageAndVisibility(ToolStripItem toolStripItem, MenuItem legacyMenuItem, ZEmbeddedModule module)
		{
			if (module != null)
			{
				var button = (ZToolBarButton)module.ToolBarButtons.FindByText(toolStripItem.Text);
				if (button != null)
				{
					toolStripItem.Image = Icons.ImageList.Images[button.ImageIndex];
					if (button.ActiveIconType != IconTypes.None)
					{
						toolStripItem.MouseEnter +=
							delegate
							{
								if (!toolStripItem.IsOnOverflow)
								{
									using (new DisposableAction(toolStripItem.Owner.SuspendLayout, toolStripItem.Owner.ResumeLayout))
									{
										toolStripItem.Image = Icons.GetImage(button.ActiveIconType);
									}
								}
							};
						toolStripItem.MouseLeave +=
							delegate
							{
								if (!toolStripItem.IsOnOverflow)
								{
									using (new DisposableAction(toolStripItem.Owner.SuspendLayout, toolStripItem.Owner.ResumeLayout))
									{
										toolStripItem.Image = Icons.ImageList.Images[button.ImageIndex];
									}
								}
							};
					}
					var cache = legacyMenuItem as IResCaptionedControl;
					if (toolStripItem is IResCaptionedControl && cache != null && cache.CaptionResourceString != null)
					{
						((IResCaptionedControl)toolStripItem).CaptionResourceString = cache.CaptionResourceString;
					}
					else
					{
						toolStripItem.Text = KMenuItem.StripAcceleratorKeys(button.Text);
						toolStripItem.ToolTipText = button.ToolTipText;
					}
					button.OnVisibleChanged += delegate
					{
						toolStripItem.Visible = button.Visible;
						legacyMenuItem.Visible = button.Visible;
					};
					button.OnActiveIconTypeChanged += (s, e) => toolStripItem.Image = Icons.GetImage(button.ActiveIconType);
				}

				if (!(legacyMenuItem.Parent is MenuItem)) // ie a top level item
				{
					toolStripItem.Visible = button != null && button.Visible;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant field names")]
		static EventHandler GetEventHandlerFromMenuItem(MenuItem menuItem, string eventHandlerName)
		{
#if !WINZOR
#if NETFRAMEWORK
			const string dataFieldName = "data";
#else
			const string dataFieldName = "_data";
#endif
			var menuItemData = typeof(MenuItem).GetField(dataFieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menuItem);
			return (EventHandler)menuItemData.GetType().GetField(eventHandlerName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menuItemData);

#else

			switch (eventHandlerName)
			{
				case "onClick":
				case "_onClick":
					return menuItem.HandlesClick ? new EventHandler((o, e) => menuItem.CallOnClick(e)) : null;

				case "onPopup":
				case "_onPopup":
					return menuItem.HandlesPopup ? new EventHandler((o, e) => menuItem.CallOnPopup(e)) : null;

				default:
					throw new NotSupportedException(eventHandlerName);
			}

#endif
		}

		#endregion
	}
}
