using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Aga.Controls.Tree;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal static class ZFormTranslatableBasherTest
	{
		internal delegate Form GetFormToBashDelegate();

		static public string BashTranslatable(BasherTest basherTest, GetFormToBashDelegate getFormToBashDelegate)
		{
			if (basherTest != null && basherTest.BashType.GetCustomAttributes(typeof(SuppressFormsLocalizedTestAttribute), true).Length > 0)
			{
				return null;
			}

			var markerResource = new ResourceStringData("", "", "", BasherTestMarkerCaption, BasherTestMarkerCaption);

			using (var cache = Res.UseMockData())
			{
				cache.SetResourceGetter((key) => markerResource);

				ZLabelCaptionCache.Instance.ClearCache();
				try
				{
					using (var testForm = getFormToBashDelegate())
					{
						if (IsExempt(testForm) || (basherTest != null && IsExempt(basherTest)))
						{
							return null;
						}

						if (testForm is ICaptionRenderingSupport && ((ICaptionRenderingSupport)testForm).CaptionRenderingEnabled != true) // It is Nullable<System.Boolean>-type value with three states, so it can't be just !value
						{
							return string.Format("Form {0} does not have CaptionRenderingEnabled set to true.", testForm.GetType().Name);
						}

						var nonTranslatableComponents = new List<string>();

						if (basherTest == null || !basherTest.AllowUntranslatableFormTitle())
						{
							if (!string.IsNullOrEmpty(testForm.Text) && !testForm.Text.Contains(BasherTestMarkerCaption))
							{
								nonTranslatableComponents.Add(FormatComponentResolution("Form", testForm.Name, testForm.Text, "Text set in code", testForm));
							}
							else
							{
								var zForm = testForm as ZForm;
								if (zForm != null)
								{
									if (!string.IsNullOrEmpty(zForm.FormHeading) && !zForm.FormHeading.Contains(BasherTestMarkerCaption, StringComparison.OrdinalIgnoreCase))
									{
										nonTranslatableComponents.Add(FormatComponentResolution("ZForm", testForm.Name, zForm.FormHeading, "FormHeading set in code", zForm));
									}
									if (!string.IsNullOrEmpty(zForm.FormCaption) && !zForm.FormCaption.Contains(BasherTestMarkerCaption, StringComparison.OrdinalIgnoreCase))
									{
										nonTranslatableComponents.Add(FormatComponentResolution("ZForm", testForm.Name, zForm.FormCaption, "FormCaption set in code", zForm));
									}
									if (!string.IsNullOrEmpty(zForm.FormVerb) && !zForm.FormVerb.Contains(BasherTestMarkerCaption, StringComparison.OrdinalIgnoreCase))
									{
										nonTranslatableComponents.Add(FormatComponentResolution("ZForm", testForm.Name, zForm.FormVerb, "FormVerb set in code", zForm));
									}
								}
							}
						}

						CheckControlIsTranslatable(testForm, nonTranslatableComponents);
						if (testForm.Menu != null)
						{
							CheckMenuItemsAreTranslatable(testForm, testForm.Menu.MenuItems, nonTranslatableComponents);
						}
						if (testForm.MainMenuStrip != null)
						{
							CheckToolStripItemsAreTranslatable(testForm, testForm.MainMenuStrip.Items, nonTranslatableComponents);
						}

						if (nonTranslatableComponents.Count > 0)
						{
							return
								string.Format(
									"Form {1} has nontranslatable controls:{0}{0}{2}{0}{0}{0}For detail see Wiki: <A href='{3}'>{3}</A>{0}",
									System.Environment.NewLine,
									testForm.GetType().FullName,
									string.Join(System.Environment.NewLine + System.Environment.NewLine, nonTranslatableComponents.ToArray()),
									"https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx"
									).Replace(System.Environment.NewLine, "<br/>");
						}
					}
				}
				finally
				{
					ZLabelCaptionCache.Instance.ClearCache();
				}
			}

			return null;
		}

		public static bool IsExempt(Object controlOrTest)
		{
			return IsExempt(controlOrTest.GetType());
		}

		public static bool IsExempt(Type type)
		{
			return new[] { "Enterprise.Customs.AU", "Enterprise.Customs.US",
				"Enterprise.Customs.CustomsWare",
				"Enterprise.Customs.US.ACEManifest", "AMS",
				"Enterprise.Client.", "ZClient", "Enterprise.Messaging." }.Any(exempt => type.FullName.Contains(exempt));
		}

		#region CheckControlIsTranslatable

		static void CheckControlIsTranslatable(Control control, ICollection<string> nonTranslatableComponents)
		{
			if (IsExempt(control) || ShouldSkipComponent(control))
			{
				return;
			}

			var zControl = control as ZUserControl;
			if (zControl != null && zControl.CaptionRenderingEnabled != true) // It is Nullable<System.Boolean>-type value with three states, so it can't be just !value
			{
				nonTranslatableComponents.Add(FormatComponentResolution(
					"Control",
					!string.IsNullOrEmpty(control.Name) ? control.Name + " - " + control.GetType().Name : string.Format("[{0}]", control.GetType().Name),
					"", "CaptionRenderingEnabled is not set to true", control));
				return;
			}

			var resolution = string.Empty;
			var controlCaption = control.Text;

			var labelCaptionRenderer = control.GetExtension<LabelCaptionRenderer>();
			if (labelCaptionRenderer != null && labelCaptionRenderer.Visible)
			{
				controlCaption = GetControlsCaption(control, labelCaptionRenderer);

				var captionContainsBasherTestMarkerCaption = controlCaption.EndsWith("...", StringComparison.Ordinal) ? BasherTestMarkerCaption.Contains(controlCaption.Substring(0, controlCaption.Length - 3)) : controlCaption.Contains(BasherTestMarkerCaption);
				if (labelCaptionRenderer.IsCaptionOverridden && IsCaptionReadable(controlCaption) && !captionContainsBasherTestMarkerCaption)
				{
					resolution += "Caption is set in code. ";
				}
			}

			if (UnsupportedControls.Contains(control.GetType()))
			{
				resolution += "Control type is not supported for resource strings - use an appropriate control from the " + typeof(ZCheckBox).Assembly.GetName().Name + " assembly. ";
			}

			if (!string.IsNullOrEmpty(resolution))
			{
				nonTranslatableComponents.Add(FormatComponentResolution(
					"Control",
					!string.IsNullOrEmpty(control.Name) ? control.Name : string.Format("[{0}]", control.GetType().Name),
					controlCaption, resolution, control));
			}

			if (control.ContextMenu != null)
			{
				CheckMenuItemsAreTranslatable(control, control.ContextMenu.MenuItems, nonTranslatableComponents);
			}

			var grid = control as ZGrid;
			if (grid != null)
			{
				CheckColumnsAreTranslatable(grid, nonTranslatableComponents);
				return;
			}

			var treeView = control as TreeViewAdv;
			if (treeView != null)
			{
				CheckTreeColumnsAreTranslatable(treeView, nonTranslatableComponents);
				return;
			}

			var tabControl = control as TabControl;
			if (tabControl != null)
			{
				foreach (TabPage tabPage in tabControl.TabPages)
				{
					CheckControlIsTranslatable(tabPage, nonTranslatableComponents);
				}
			}
			else
			{
				foreach (Control subcontrol in control.Controls)
				{
					CheckControlIsTranslatable(subcontrol, nonTranslatableComponents);
				}
			}

			var toolStrip = control as ToolStrip;
			if (toolStrip != null)
			{
				CheckToolStripItemsAreTranslatable(control, toolStrip.Items, nonTranslatableComponents);
			}

			if (control.ContextMenuStrip != null)
			{
				CheckToolStripItemsAreTranslatable(control.ContextMenuStrip, control.ContextMenuStrip.Items, nonTranslatableComponents);
			}

			var dropEdit = control as ZDropEdit;
			if (dropEdit != null && string.IsNullOrEmpty(dropEdit.BindToForDescription)
				&& dropEdit.List != null && !(dropEdit.List is UntranslatableCodeDescriptionPairList)
				&& !(dropEdit.List is IBusinessObjectCollection) && !(dropEdit.List is ZAddressList)
				&& !(dropEdit.List.GetType().Name == "DateRangePairList") && !MissingResourceStringChecker.IsExcludedFromTest(dropEdit))
			{
				foreach (var listItem in dropEdit.List)
				{
					var codeDescription = listItem as ICodeDescription;
					var multilingualDescription = codeDescription.GetMultilingualDescription();
					if (codeDescription != null && (multilingualDescription is ResourceString || multilingualDescription is NoResString))
					{
						var displayedText = dropEdit.ShowDescriptionInDropDown && !string.IsNullOrEmpty(codeDescription.Description) ? codeDescription.Description : codeDescription.GetMultilingualCode();
						if (!displayedText.Contains(BasherTestMarkerCaption) && displayedText.Count(c => char.IsLetter(c)) > 1)
						{
							nonTranslatableComponents.Add(FormatComponentResolution("Drop Edit Item",
								!string.IsNullOrEmpty(control.Name) ? control.Name : string.Format("[{0}]", control.GetType().Name), displayedText, "Drop Down item display is not translatable, use ResString.GetMultilingualString() when creating the related CodePairDescriptionList", control));
						}
					}
				}
			}
		}

		#endregion

		#region CheckColumnsAreTranslatable

		static void CheckColumnsAreTranslatable(ZGrid grid, ICollection<string> nonTranslatableComponents)
		{
			foreach (var column in grid.Columns)
			{
				foreach (var item in grid.ColumnStyles)
				{
					var info = item as ZGridColumnInfo;
					if (info != null && info.ColumnName == column.ColumnStyle.MappingName && ShouldSkipComponent(info))
					{
						return;
					}
				}

				var resolution = string.Empty;

				if (!string.IsNullOrEmpty(column.ColumnStyle.HeaderText) && !column.ColumnStyle.HeaderText.Contains(BasherTestMarkerCaption))
				{
					resolution += "Column is not translatable, set the ColumnID or CaptionResourceString.";
				}

				if (!string.IsNullOrEmpty(resolution))
				{
					nonTranslatableComponents.Add(FormatComponentResolution("Column", column.ColumnStyle.MappingName, column.ColumnStyle.HeaderText, resolution, grid));
				}
			}
		}

		#endregion

		#region CheckTreeColumnsAreTranslatable

		static void CheckTreeColumnsAreTranslatable(TreeViewAdv treeView, ICollection<string> nonTranslatableComponents)
		{
			foreach (var column in treeView.Columns)
			{
				var resolution = string.Empty;

				if (!string.IsNullOrEmpty(column.Header) && !column.Header.Contains(BasherTestMarkerCaption))
				{
					resolution += "Tree Column is not translatable, set the Column Header with a Resource String.";
				}

				if (!string.IsNullOrEmpty(resolution))
				{
					nonTranslatableComponents.Add(FormatComponentResolution("Column", column.Header, column.Header, resolution, treeView));
				}
			}
		}

		#endregion

		#region CheckMenuItemsAreTranslatable

		static void CheckMenuItemsAreTranslatable(Control parent, Menu.MenuItemCollection menuItems, ICollection<string> nonTranslatableComponents)
		{
			foreach (MenuItem item in menuItems)
			{
				if (ShouldSkipComponent(item) || SpecialMenuItems.Contains(item.Text))
				{
					return;
				}

				if (IsCaptionReadable(item.Text))
				{
					if (!item.Text.Contains(BasherTestMarkerCaption))
					{
						nonTranslatableComponents.Add(FormatComponentResolution("Menu", item.Name, item.Text, "Res.GetString is not used. ", parent));
					}
					else if (!(item is ZMenuItem zItem) || (zItem.Caption == null && zItem.CaptionResourceString == null))
					{
#if WINZOR
						var eventInfo = item.GetType().GetEvent("Click");
						var fieldInfo = eventInfo.DeclaringType.GetField("Click", BindingFlags.Instance | BindingFlags.NonPublic);
						var onClick = (EventHandler)fieldInfo.GetValue(item);
#else
						#if NETFRAMEWORK
						const string dataFieldName = "data";
						const string onClickFieldName = "onClick";
						#else
						const string dataFieldName = "_data";
						const string onClickFieldName = "_onClick";
#endif
						var itemData = typeof(MenuItem).GetField(dataFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(item);
						var onClick = (EventHandler)itemData.GetType().GetField(onClickFieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(itemData);
#endif
						nonTranslatableComponents.Add(FormatComponentResolution("Menu", item.Name, onClick != null ? "onClick=" + onClick.Method.DeclaringType.FullName + "." + onClick.Method.Name : "?", "Use ZMenuItem and set the Caption = ResString.GetMultilingualString() instead of .Text = Res.GetString()", parent));
					}
				}

				if (item.MenuItems.Count > 0)
				{
					CheckMenuItemsAreTranslatable(parent, item.MenuItems, nonTranslatableComponents);
				}
			}
		}

		static List<string> SpecialMenuItems
		{
			get
			{
				return
					specialMenuItems ??
					(specialMenuItems =
					 new List<string>
						{
							"Audit Classification Lookup",
							"<placeholder for popup event>",
							"Placeholder; menu items added OnPopup",
						});
			}
		}
		static List<string> specialMenuItems;

#endregion

		#region CheckToolStripItemsAreTranslatable

		static void CheckToolStripItemsAreTranslatable(Control parent, ToolStripItemCollection toolStripItems, ICollection<string> nonTranslatableComponents)
		{
			foreach (ToolStripItem item in toolStripItems)
			{
				if (ShouldSkipComponent(item) || SpecialMenuItems.Contains(item.Text))
				{
					return;
				}

				if (!item.Text.Contains(BasherTestMarkerCaption) && IsCaptionReadable(item.Text))
				{
					nonTranslatableComponents.Add(FormatComponentResolution("ToolStripItem", item.Name, item.Text, "Item is not translatable, use ResourceStringCaption to set the text", parent));
				}

				var dropDownItem = item as ToolStripDropDownItem;
				if (dropDownItem != null)
				{
					CheckToolStripItemsAreTranslatable(parent, dropDownItem.DropDownItems, nonTranslatableComponents);
				}
			}
		}

		#endregion

		#region Implementation

		static bool ShouldSkipComponent(object component)
		{
			if (component.GetType().GetCustomAttributes(typeof(SuppressFormsLocalizedTestAttribute), true).Length > 0)
			{
				return true;
			}

			if (TypeDescriptor.GetAttributes(component)[typeof(SuppressFormsLocalizedTestAttribute)] != null)
			{
				return true;
			}

			return false;
		}

		internal const string BasherTestMarkerCaption = "@#$_Basher_Test";

		static string GetControlsCaption(Control control, ILabelCaptionRenderer labelCaptionRenderer)
		{
			var controlCaption = control.Text;
			var labelCaption = labelCaptionRenderer.Caption;

			return
				string.IsNullOrEmpty(labelCaption) || labelCaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(controlCaption)
					? controlCaption
					: labelCaption;
		}

		static bool IsCaptionReadable(string caption)
		{
			return !string.IsNullOrEmpty(caption) && ReadableCaptionRegex.IsMatch(caption);
		}
		static readonly Regex ReadableCaptionRegex = new Regex("[a-zA-Z]+", RegexOptions.Compiled);

		static List<Type> UnsupportedControls
		{
			get
			{
				return unsupportedControls ?? (unsupportedControls =
					new List<Type>
						{
							typeof(Label),
							typeof(CheckBox),
							typeof(RadioButton),
							typeof(GroupBox),
							typeof(Button),
						});
			}
		}
		static List<Type> unsupportedControls;

		static string FormatComponentResolution(string componentType, string name, string text, string resolution, Control locationControl)
		{
			return string.Format(
				"{1}:\t\t{2}{0}Text:\t\t{3}{0}Error:\t\t{4}{0}Path:\t\t{5}",
				System.Environment.NewLine,
				componentType,
				name,
				text.Replace("<", "&lt;").Replace(">", "&gt;"),
				resolution,
				ControlDescription.GetControlPath(locationControl)
			);
		}

		#endregion
	}
}
