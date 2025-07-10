using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common.Enumeration;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used soon, once we've implemented enough Hotkeys using the HotkeyRegister.")]
	public partial class AvailableHotkeysForm : ZChildForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1111:DoNotUseSystemWindowsFormsTabDrawModeOwnerDrawFixed", Justification = "We want the tabs to be displayed horizontally. We have to draw them ourselves.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "We want the tabs to be displayed horizontally. We have to draw them ourselves.")]
		public AvailableHotkeysForm(Control currentlyFocusedElement)
		{
			InitializeComponent();
			BackColor = PageBackColor;

			tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;

#if WINZOR
			tabControl.TabControlNavigationAdditionalStyles = "width: 108px; height: 100px;";
			tabControl.TabControlAlignmentStyle = "vertical";
			tabControl.TabControlButtonAdditionalStyles = "color:white; top: 5px; height: 25px; --custom-outline: 1.5px dotted #866f8a; --custom-outline-offset: -4px; --custom-left-offset:3px; --custom-padding-left-active: 2px; --custom-padding-right-active: 2px; --custom-box-sizing: content-box; --default-color:#0099CC; --inactive-hover-color:#0099CC; --active-color:#8fd400;";
			var itemSize = new Size(100, 25);
			tabControl.ItemSize = itemSize;
			tabControl.TabPageAdditionalStyles = "top:0px; left: 108px;";
#endif

#if !WINZOR
			tabControl.DrawItem += drawItemsOnTheRight;
#endif

			AddTabPage(Res.GetString("03BDDFB0-DF06-41FC-ACD3-CBD83FDE24E1", "Global"), ToCodeDescriptionPair(GlobalHotkeys.Descriptions));

			var children = ZEnumerable.Iterate(currentlyFocusedElement, control => control?.Parent, null).Reverse();
			foreach (var control in children)
			{
				if (control is IHotkeyProvider hotkeyProvider)
				{
					AddHotkeyProvider(hotkeyProvider, includeShortcuts: true);

					if (control is IAdditionalHotkeyProviders otherProviders)
					{
						foreach (var provider in otherProviders.OtherProviders)
						{
							AddHotkeyProvider(provider, includeShortcuts: false);
						}
					}
				}
			}
		}

		void AddHotkeyProvider(IHotkeyProvider provider, bool includeShortcuts)
		{
			var descriptions = includeShortcuts ?
				GetAllShortcuts((Control)provider) :
				ToCodeDescriptionPair(provider.Hotkeys.Descriptions);

			if (descriptions.Count > 0)
			{
				AddTabPage(Res.GetString("B0DE743B-ACDA-4767-9308-FD608F547E11", "This {0}", provider.TypeNameForDisplay), descriptions);
			}
		}

		CodeDescriptionPairList GetAllShortcuts(Control control)
		{
			var hotkeys = ToCodeDescriptionPair(((IHotkeyProvider)control).Hotkeys.Descriptions);

			if (control.ContextMenu != null)
			{
				AddMenuShortcuts(hotkeys, control.ContextMenu.MenuItems);
			}

			var menu = (control as Form)?.Menu;
			if (menu != null)
			{
				AddMenuShortcuts(hotkeys, menu.MenuItems);
			}

			return hotkeys;
		}

		Point HeaderLabelLocation => ControlDpiScalingHelper.NewScaledPoint(5, 5);

		const int gridVerticalDrop = 10;

		void AddTabPage(string name, CodeDescriptionPairList hotkeys)
		{
			var page = new ZTabPage { BackColor = PageBackColor };
			page.GetExtension<ILabelCaptionRenderer>().Caption = name;

			var topLabel = new ZLabel { Location = HeaderLabelLocation, AutoSize = true };
			topLabel.GetExtension<ILabelCaptionRenderer>().Caption = name;

			var defaultFont = topLabel.Font;
			topLabel.Font = new Font(defaultFont.FontFamily, defaultFont.Size * 1.2f, FontStyle.Bold);

			page.Controls.Add(topLabel);

			var grid = new CodeDescriptionListEditControl(true, true, CharacterCasing.Normal, CharacterCasing.Normal, Res.GetString("E64C7A72-F942-4F55-B8DD-895F7B642091", "Shortcut"), Res.GetString("CA4C3FA9-B7A4-4A9A-AEA4-7970F1E02167", "Action"));
			grid.ReadOnly = true;
			grid.Location = ControlDpiScalingHelper.NewScaledPoint(0, topLabel.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiX(gridVerticalDrop), false);
			grid.Size = ControlDpiScalingHelper.NewScaledSize(page.Width, page.Height - grid.Top, false);
			grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

			grid.FieldValue = hotkeys.ToXMLByteArray();

			page.Controls.Add(grid);

			tabControl.TabPages.Add(page);
		}

		void AddMenuShortcuts(CodeDescriptionPairList list, Menu.MenuItemCollection menuItems)
		{
			foreach (MenuItem menuItem in menuItems)
			{
				if (menuItem.IsParent && menuItem.Visible)
				{
					AddMenuShortcuts(list, menuItem.MenuItems);
				}
				else if (menuItem.Shortcut != Shortcut.None && menuItem.Visible)
				{
					list.AddPair(FormatShortcut(menuItem.Shortcut), StripAmpersands(menuItem.Text));
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		string StripAmpersands(string s)
		{
			return Regex.Replace(s, "&(?!&)", ""); //Ampersands not proceeded by another ampersand. "My &Cat && Dog" => "My Cat & Dog"
		}

		string FormatShortcut(Shortcut shortcut)
		{
			const string upperCaseLetter = "[A-Z]";
			const string digitNotPreceededByAnotherDigitOrF = @"(?<![\dF])\d";
			const string regex = upperCaseLetter + "|" + digitNotPreceededByAnotherDigitOrF;

			return Regex.Replace(shortcut.ToString(), regex, @" + $0").TrimStart(new char[] { ' ', '+' }); //CtrlShiftA => Ctrl + Shift + A, CtrlShift0 => Ctrl + Shift + 0
		}

		#endregion

		CodeDescriptionPairList ToCodeDescriptionPair(IEnumerable<KeyValuePair<string, string>> descriptions)
		{
			var list = new CodeDescriptionPairList();
			foreach (var kvp in descriptions)
			{
				list.AddPair(kvp.Key, kvp.Value);
			}

			return list;
		}

#if !WINZOR

		void drawItemsOnTheRight(object sender, DrawItemEventArgs e)
		{
			var graphics = e.Graphics;
			var tabPage = (ZTabPage)tabControl.TabPages[e.Index];
			var tabBounds = tabControl.GetTabRect(e.Index);

			var tabBrush = e.State == DrawItemState.Selected ? selectedTabBrush : unselectedTabBrush;
			graphics.FillRectangle(tabBrush, tabBounds);

			using (var format = new StringFormat())
			{
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;

				TextRendererHelper.DrawText(graphics, tabPage.Text, tabPage.Font, tabBounds, tabTextBrush, format, TextRendererType.GDIPlus);
			}
		}

#endif

		Color PageBackColor => SystemDataRegistry.Instance.ColorTheme.MainFormBackgroundColor;

		#if !WINZOR
		readonly Brush unselectedTabBrush = BrushProvider.FromColor(SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1);
		readonly Brush selectedTabBrush = BrushProvider.FromColor(SystemDataRegistry.Instance.ColorTheme.NavBarGroupSelected1);
		readonly Brush tabTextBrush = BrushProvider.FromColor(SystemDataRegistry.Instance.ColorTheme.NavBarTextColor);
		#endif

		internal ZTabControl TabControl => tabControl;
	}
}
