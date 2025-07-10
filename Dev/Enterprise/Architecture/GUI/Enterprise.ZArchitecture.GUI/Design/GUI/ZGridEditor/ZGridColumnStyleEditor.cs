using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Design
{
	internal class ZGridColumnStyleEditor : CollectionEditor
	{
		public ZGridColumnStyleEditor()
			: base(typeof(ArrayList))
		{
		}

		protected override Type[] CreateNewItemTypes()
		{
			if (AvailableColumns != null && AvailableColumns.SelectedItem is IAvailableColumnItem)
			{
				return ((IAvailableColumnItem)AvailableColumns.SelectedItem).AvailableColumnStyles;
			}
			else
			{
				return new EmptyColumnItem().AvailableColumnStyles;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		protected override CollectionForm CreateCollectionForm()
		{
			EditorForm = base.CreateCollectionForm();

			var tableFlowControl = EditorForm.Controls[0];
			for (var i = tableFlowControl.Controls.Count - 1; i >= 0; i--)
			{
				EditorForm.Controls.Add(tableFlowControl.Controls[i]);
			}
			EditorForm.Controls.Remove(tableFlowControl);
			tableFlowControl.Dispose();

			AvailableColumns = new ListBox();
			ControlDpiScalingHelper.SetWidth(ref AvailableColumns, 200, true);
			var availableColumnsRequiredSpace = AvailableColumns.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			ControlDpiScalingHelper.SetWidth(EditorForm, EditorForm.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(100) + availableColumnsRequiredSpace, false);
			ControlDpiScalingHelper.SetHeight(ref AvailableColumns, GetControlByName("listbox").Height, false);
			AvailableColumns.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.ScaleToCurrentDpiX(10), GetControlByName("listbox").Top, false);

			var availableColumnsLabel = new Label();
			availableColumnsLabel.Text = "Available Columns:";
			availableColumnsLabel.Location = ControlDpiScalingHelper.NewScaledPoint(AvailableColumns.Left, GetControlByName("membersLabel").Top, false);

			EditorForm.Controls.Add(AvailableColumns);
			EditorForm.Controls.Add(availableColumnsLabel);

			GetControlByName("listbox").Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetWidth(GetControlByName("listbox"), 260, true);
			ControlDpiScalingHelper.SetLeft(GetControlByName("listbox"), GetControlByName("listbox").Left + availableColumnsRequiredSpace, false);

			ControlDpiScalingHelper.SetLeft(GetControlByName("membersLabel"), GetControlByName("membersLabel").Left + availableColumnsRequiredSpace, false);
			GetControlByName("membersLabel").Anchor = AnchorStyles.Top | AnchorStyles.Left;

			ControlDpiScalingHelper.SetLeft(GetControlByName("propertiesLabel"), GetControlByName("propertiesLabel").Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(50) + availableColumnsRequiredSpace, false);
			GetControlByName("propertiesLabel").Anchor = AnchorStyles.Top | AnchorStyles.Left;
			ControlDpiScalingHelper.SetLeft(GetControlByName("propertyBrowser"), GetControlByName("propertyBrowser").Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(50) + availableColumnsRequiredSpace, false);
			ControlDpiScalingHelper.SetWidth(GetControlByName("propertyBrowser"), EditorForm.ClientSize.Width - GetControlByName("propertyBrowser").Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(20), false);
			ControlDpiScalingHelper.SetHeight(GetControlByName("propertyBrowser"), GetControlByName("listbox").Height, false);

			ControlDpiScalingHelper.SetLeft(GetControlByName("upButton"), GetControlByName("upButton").Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(70) + availableColumnsRequiredSpace, false);
			ControlDpiScalingHelper.SetLeft(GetControlByName("downButton"), GetControlByName("upButton").Left, false);
			ControlDpiScalingHelper.SetTop(GetControlByName("downButton"), GetControlByName("upButton").Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);

			GetControlByName("upButton").Anchor = AnchorStyles.Top | AnchorStyles.Left;
			GetControlByName("downButton").Anchor = AnchorStyles.Top | AnchorStyles.Left;

			var addRemoveParentControl = GetControlByName("addButton").Parent;
			EditorForm.Controls.Add(GetControlByName("addButton"));
			EditorForm.Controls.Add(GetControlByName("removeButton"));
			EditorForm.Controls.Remove(addRemoveParentControl);

			GetControlByName("addButton").Text = "Add ZTextBoxColumnStyle";
			ControlDpiScalingHelper.SetLeft(GetControlByName("addButton"), AvailableColumns.Left, false);
			GetControlByName("addButton").Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			ControlDpiScalingHelper.SetWidth(GetControlByName("addButton"), AvailableColumns.Width, false);
			ControlDpiScalingHelper.SetTop(GetControlByName("addButton"), AvailableColumns.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			GetControlByName("addButton").Click += new EventHandler(GridColumnStyleEditor_Click);

			ControlDpiScalingHelper.SetLeft(GetControlByName("removeButton"), GetControlByName("listbox").Left, false);
			ControlDpiScalingHelper.SetWidth(GetControlByName("removeButton"), GetControlByName("listbox").Width, false);
			ControlDpiScalingHelper.SetTop(GetControlByName("removeButton"), GetControlByName("listbox").Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			GetControlByName("removeButton").Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			GetControlByName("removeButton").Click += new EventHandler(GridColumnStyleEditor_Click);

			EditorForm.Font = OFont.GetFont();
			EditorForm.Closed += new EventHandler(RefreshGrid);
			EditorForm.Text = "Customize ZGrid ColumnStyles";

			AvailableColumns.Items.Add(new EmptyColumnItem());
			AvailableColumns.DisplayMember = "DisplayName";
			AvailableColumns.SelectedIndexChanged += new EventHandler(AvailableColumns_SelectedIndexChanged);
			AvailableColumns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			AvailableColumns.IntegralHeight = false;
			AvailableColumns.Sorted = true;

			GetControlByName("okButton").Parent.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			EditorForm.Load += new EventHandler(EditorForm_Load);

			return EditorForm;
		}

		protected override bool CanSelectMultipleInstances()
		{
			return true;
		}

		protected override Type CreateCollectionItemType()
		{
			return typeof(ZTextBoxColumnStyleInfo);
		}

		protected override object CreateInstance(Type itemType)
		{
			var info = (ZGridColumnInfo)base.CreateInstance(itemType);
			if (SelectedItem != null)
			{
				info.ColumnName = SelectedItem.ColumnName;
			}

			return info;
		}

		#region Event Handlers

		void GridColumnStyleEditor_Click(object sender, EventArgs e)
		{
			PopulateList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		void AvailableColumns_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (SelectedItem != null)
			{
				var types = SelectedItem.AvailableColumnStyles;

				typeof(CollectionEditor).GetField("newItemTypes", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, CreateNewItemTypes());
				GetControlByName("addButton").Text = "Add " + types[0].Name.Replace("Info", "");

				var menuItemType = EditorForm.GetType().GetNestedType("TypeMenuItem", BindingFlags.NonPublic | BindingFlags.Instance);

				var dropDown = (ToolStripDropDown)GetControlByType(typeof(ToolStripDropDown));
				var button = (Button)GetControlByName("addButton");
				button.ContextMenuStrip.Items.Clear();
				foreach (var type in types)
				{
					var typeMenuItem = (ToolStripMenuItem)Activator.CreateInstance(menuItemType, new object[] { type, new EventHandler(AddType_Click) });
					button.ContextMenuStrip.Items.Add(typeMenuItem);
				}
			}
		}

		void AddType_Click(object sender, EventArgs e)
		{
			var addMethod = EditorForm.GetType().GetMethod("AddDownMenu_click", BindingFlags.Instance | BindingFlags.NonPublic);
			addMethod.Invoke(EditorForm, new object[] { sender, e });
			PopulateList();
		}

		void EditorForm_Load(object sender, EventArgs e)
		{
			EditorForm.BeginInvoke(new EventHandler(EditorForm_LoadDeferred), new object[] { sender, e });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		void EditorForm_LoadDeferred(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(GridBindingMember))
				{
					var host = (IDesignerHost)Site.GetService(typeof(IDesignerHost));
					var dataSourceTypeHolder = host == null ? null : host.RootComponent as ITopLevelDataSourceType;
					DataSource = dataSourceTypeHolder == null ? null : dataSourceTypeHolder.DataSourceType;

					if (DataSource != null)
					{
						PopulateList();
					}
					else
					{
						ShowMessage("You need to select the Top Level BusinessObject for your Form. Set the DataSourceType property on the BindingSource component in the component tray.", "DataSource not set.", MessageBoxButtons.OK);
					}
				}
				else
				{
					ShowMessage("Please set your BindingMember!  \r\nIt's possible that our sucky form designer has removed a line similar to the following from your code, please restore it. \r\n\tBindingSource.SetBindingMember(grid, \".\");");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ShowMessage(ex.ToString(), ex.Message, MessageBoxButtons.OK);
			}
		}

		#endregion

		#region Implementation

		CollectionForm EditorForm;
		ListBox AvailableColumns;
		Type DataSource;

		void RefreshGrid(object sender, EventArgs e)
		{
			if (Context.Instance != null && Context.Instance is ZGrid)
			{
				((ZGrid)Context.Instance).Invalidate();
			}
		}

		ISite Site
		{
			get
			{
				ISite result = null;
				var control = (Control)Context.Instance;
				while (result == null && control != null)
				{
					control = control.Parent;
					result = control.Site;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		void PopulateList()
		{
			if (DataSource != null)
			{
				AvailableColumns.Items.Clear();
				AvailableColumns.Items.Add(new EmptyColumnItem());
				AvailableColumns.SelectedIndex = 0;

				var finalType = FinalTypeRetriever.Retrieve(DataSource, GridBindingMember);
				if (finalType != null)
				{
					foreach (PropertyDescriptor property in ZCustomTypeDescriptor.GetProperties(finalType))
					{
						if (IsAvailableColumn(property))
						{
							AvailableColumns.Items.Add(new AvailableColumnItem(property));
						}
					}
				}
				else
				{
					if (!HasNotifiedDataSourceNotFound)
					{
						ShowMessage(GridBindingMember + " not found on DataSource " + DataSource.Name);
						HasNotifiedDataSourceNotFound = true;
					}
				}
			}
		}

		string GridBindingMember
		{
			get { return Grid.Parent is IButtonGrid ? Grid.Parent.GetBindingMember() : Grid.GetBindingMember(); }
		}

		bool HasNotifiedDataSourceNotFound;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		bool IsAvailableColumn(PropertyDescriptor property)
		{
			if (TypeUtilities.DoesTypeImplementInterface(property.PropertyType, typeof(IZType)) && !ExcludedProperties.Contains(property.Name))
			{
				var isFound = false;

				foreach (var item in ((ListBox)GetControlByName("listbox")).Items)
				{
					var info = (ZGridColumnInfo)item.GetType().GetProperty("Value").GetValue(item, null);
					if (info.ColumnName == property.Name)
					{
						isFound = true;
						break;
					}
				}

				return !isFound;
			}
			else
			{
				return false;
			}
		}

		StringCollection ExcludedProperties
		{
			get
			{
				var result = new StringCollection();
				result.Add("ShowRelatedNotes");
				result.Add("PK");
				result.Add("HumanReadableName");
				result.Add("InstatiationTime");

				return result;
			}
		}

		protected Control GetControlByName(string controlName)
		{
			return GetControlByName(controlName, EditorForm);
		}

		Control GetControlByName(string controlName, Control parentControl)
		{
			Control controlByName = null;

			foreach (Control currentControl in parentControl.Controls)
			{
				if (currentControl.Name == controlName)
				{
					controlByName = currentControl;
					break;
				}

				var internalControl = GetControlByName(controlName, currentControl);
				if (internalControl != null)
				{
					controlByName = internalControl;
					break;
				}
			}

			return controlByName;
		}

		Control GetControlByType(Type controlType)
		{
			return GetControlByType(controlType, EditorForm);
		}

		Control GetControlByType(Type controlType, Control parentControl)
		{
			Control controlByType = null;

			foreach (Control currentControl in parentControl.Controls)
			{
				if (currentControl.GetType() == controlType)
				{
					controlByType = currentControl;
					break;
				}

				var internalControl = GetControlByType(controlType, currentControl);
				if (internalControl != null)
				{
					controlByType = internalControl;
					break;
				}
			}

			return controlByType;
		}

		ZGrid Grid
		{
			get
			{
				ZGrid grid = null;
				if (Context.Instance is IButtonGrid)
				{
					grid = ((IButtonGrid)Context.Instance).InnerGrid;
				}
				else
				{
					grid = Context.Instance as ZGrid;
				}

				return grid;
			}
		}

		IAvailableColumnItem SelectedItem
		{
			get { return ((IAvailableColumnItem)AvailableColumns.SelectedItem); }
		}

		void ShowMessage(string message)
		{
			System.Windows.Forms.MessageBox.Show(message); // Not Production Code, Designer Tool only.
		}

		DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons)
		{
			return System.Windows.Forms.MessageBox.Show(message, caption, buttons); // Not Production Code, Designer Tool only.
		}

		#endregion
	}

	#region AvailableColumnItem

	public interface IAvailableColumnItem
	{
		string DisplayName { get; }
		string ColumnName { get; }
		string Caption { get; }
		Type[] AvailableColumnStyles { get; }
	}

	public class EmptyColumnItem : IAvailableColumnItem
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public virtual string DisplayName
		{
			get { return "<Other Property>"; }
		}

		public virtual string ColumnName
		{
			get { return ""; }
		}

		public virtual string Caption
		{
			get { return ""; }
		}

		public virtual Type[] AvailableColumnStyles
		{
			get
			{
				var types = new List<Type>
					{
						typeof(ZTextBoxColumnStyleInfo),
						typeof(ZCalcEditColumnStyleInfo),
						typeof(ZCheckBoxColumnStyleInfo),
						typeof(ZCodeFindBoxColumnStyleInfo),
						typeof(ZDateEditColumnStyleInfo),
						typeof(ZDateTimeOffsetEditColumnStyleInfo),
						typeof(ZDropEditColumnStyleInfo),
						typeof(ZDynamicMultilineTextBoxColumnStyleInfo),
						typeof(ZGuidDropEditColumnStyleInfo),
						typeof(ZGuidFindBoxColumnStyleInfo),
						typeof(ZMultiControlColumnStyleInfo),
						typeof(ZMultiLineTextBoxColumnInfo),
						typeof(ZTimeEditExColumnStyleInfo),
						typeof(ZCodeFindBoxWithSelectedEventColumnStyleInfo)
					};
				try
				{
					types.Add(ObjectFactory.GetTypeDesignerSafe("ZOrganisationFindBoxColumnStyleInfo"));
					types.Add(ObjectFactory.GetTypeDesignerSafe<IZAddressDropEditColumnStyleInfo>());
				}
				catch (CannotLoadObjectTypeException)
				{
				}
				return types.ToArray();
			}
		}
	}

	public class AvailableColumnItem : EmptyColumnItem
	{
		public AvailableColumnItem(PropertyDescriptor property)
		{
			this.Property = property;
		}

		public override string DisplayName
		{
			get { return ColumnName; }
		}

		public override string ColumnName
		{
			get { return Property.Name; }
		}

		public override string Caption
		{
			get
			{
				var result = ColumnName;

				if (result.Length > 3 && result[2] == '_')
				{
					result = result.Substring(3);
				}
				else if (result.Length > 4 && result[3] == '_')
				{
					result = result.Substring(4);
				}

				for (var i = result.Length - 1; i >= 0; i--)
				{
					if (result[i].ToString() == result[i].ToString().ToUpper() && i != 0)
					{
						result = result.Insert(i, " ");
					}
				}

				return result;
			}
		}

		public override Type[] AvailableColumnStyles
		{
			get
			{
				if (fAvailableColumnStyles == null)
				{
					PopulateAvailableColumnStyles();
				}

				return fAvailableColumnStyles;
			}
		}

		#region Implementation

		Type[] fAvailableColumnStyles;
		readonly PropertyDescriptor Property;

		void PopulateAvailableColumnStyles()
		{
			if (IsType(Property.PropertyType, typeof(ZInt)) || IsType(Property.PropertyType, typeof(ZDecimal)) || IsType(Property.PropertyType, typeof(ZShort)) || IsType(Property.PropertyType, typeof(ZByte)))
			{
				fAvailableColumnStyles = new Type[] { typeof(ZCalcEditColumnStyleInfo) };
			}
			else if (IsType(Property.PropertyType, typeof(ZString)))
			{
				fAvailableColumnStyles = new Type[] { typeof(ZTextBoxColumnStyleInfo), typeof(ZCodeFindBoxColumnStyleInfo), typeof(ZDropEditColumnStyleInfo), typeof(ZMultiLineTextBoxColumnInfo), typeof(ZMultiControlColumnStyleInfo), typeof(ZDynamicMultilineTextBoxColumnStyleInfo) };
			}
			else if (IsType(Property.PropertyType, typeof(ZDateTime)))
			{
				fAvailableColumnStyles = new Type[] { typeof(ZDateEditColumnStyleInfo), typeof(ZTimeEditExColumnStyleInfo) };
			}
			else if (IsType(Property.PropertyType, typeof(ZDateTimeOffset)))
			{
				fAvailableColumnStyles = new Type[] { typeof(ZDateTimeOffsetEditColumnStyleInfo) };
			}
			else if (IsType(Property.PropertyType, typeof(ZGuid)))
			{
				var columnName = string.IsNullOrEmpty(ColumnName) ? "" : ColumnName;
				if (columnName.Contains("_" + OrgHeaderSchema.Constants.Prefix, StringComparison.InvariantCulture))
				{
					fAvailableColumnStyles = new Type[] { ZOrganisationFindBoxColStyleInfoType, typeof(ZGuidFindBoxColumnStyleInfo) };
				}
				else if (columnName.Contains("_" + OrgAddressSchema.Constants.Prefix, StringComparison.InvariantCulture))
				{
					fAvailableColumnStyles = new Type[] { ZAddressDropEditColumnStyleInfoType, typeof(ZGuidDropEditColumnStyleInfo) };
				}
				else
				{
					fAvailableColumnStyles = new Type[] { typeof(ZGuidFindBoxColumnStyleInfo), ZOrganisationFindBoxColStyleInfoType, ZAddressDropEditColumnStyleInfoType, typeof(ZGuidDropEditColumnStyleInfo) };
				}
			}
			else if (IsType(Property.PropertyType, typeof(ZBool)))
			{
				fAvailableColumnStyles = new Type[] { typeof(ZCheckBoxColumnStyleInfo) };
			}
			else
			{
				fAvailableColumnStyles = base.AvailableColumnStyles;
			}
		}

		bool IsType(Type type1, Type type2)
		{
			return type1.FullName == type2.FullName;
		}

		static Type ZOrganisationFindBoxColStyleInfoType
		{
			get { return ObjectFactory.GetTypeDesignerSafe("ZOrganisationFindBoxColumnStyleInfo"); }
		}

		static Type ZAddressDropEditColumnStyleInfoType
		{
			get { return ObjectFactory.GetTypeDesignerSafe<IZAddressDropEditColumnStyleInfo>(); }
		}

		#endregion
	}

	#endregion
}
