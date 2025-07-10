using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.
namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Info class for ZMultiControlColumnStyle required by column designer
	/// </summary>
	public class ZMultiControlColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfoWithModuleID, IMultiTypeColumnStyleInfo
	{
		public ZMultiControlColumnStyleInfo()
		{
		}

		public ZMultiControlColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(INumericZType))]
		public string BindToDecimalPlaces { get; set; }

		[ZColumnBindingMemberType(typeof(ZString))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[ZColumnBindingMemberType(typeof(ZString))]
		public string FieldTypeColumnName
		{
			get { return fieldTypeColumnName; }
			set { fieldTypeColumnName = value; }
		}

		[DefaultValue("")]
		[ZColumnBindingMemberType(typeof(IList))]
		public string BindToList
		{
			get { return bindToList; }
			set { bindToList = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZMultiControlColumnStyle); }
		}

		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
			set { moduleID = value; }
		}

		[DefaultValue(false)]
		[ZColumnBindingMemberType(typeof(ZBool))]
		public bool DataFieldsOnly { get; set; } = false;

		#region Implementation

		string fieldTypeColumnName = "";
		string bindToList = "";
		ModuleIdentifier moduleID = ModuleIDs.NotAssigned;

		bool ShouldSerializeModuleID()
		{
			return (ModuleID != ModuleIDs.NotAssigned);
		}

		#endregion
	}

	/// <summary>
	/// A ColumnStyle that binds to a string field and will return different edit control depending on field type.
	/// </summary>
	public class ZMultiControlColumnStyle : ZCustomControlColumnStyle, IZColumn, ICustomKeyHandlingGridColumn
	{
		public ZMultiControlColumnStyle(ZMultiControlColumnStyleInfo columnInfo)
			: this(() => new ZMultiCombinationControl(), columnInfo)
		{
		}

		public ZMultiControlColumnStyle(Func<ZMultiCombinationControl> combinationControl, ZMultiControlColumnStyleInfo columnInfo)
			: base(combinationControl, columnInfo)
		{
			dataFieldsOnly = columnInfo.DataFieldsOnly;
			allowMultipleMacroses = columnInfo.AllowMultipleMacroses;
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var comboControl = (ZMultiCombinationControl)control;
			comboControl.BindTo = ColumnInfo.ColumnName;
			comboControl.BindToList = ColumnInfo.BindToList;
			comboControl.BindToDecimalPlaces = ColumnInfo.BindToDecimalPlaces;
			comboControl.ModuleID = ColumnInfo.ModuleID;
			comboControl.CharacterCasing = ColumnInfo.CharacterCasing;
			comboControl.PasswordChar = ColumnInfo.PasswordChar;
			comboControl.FindBoxModuleShowing += new EventHandler<ModuleShowingEventArgs>(EditControl_FindBoxModuleShowing);
			comboControl.CheckBoxCheckedChanged += new EventHandler(EditControl_CheckBoxCheckedChanged);
			comboControl.GuidFindBoxPopupSelected += EditControl_GuidFindBoxPopupSelected;
		}

		protected internal new ZMultiControlColumnStyleInfo ColumnInfo => (ZMultiControlColumnStyleInfo)base.ColumnInfo;

		void EditControl_GuidFindBoxPopupSelected(object sender, EventArgs e)
		{
			IsEditing = true;
		}

		void EditControl_CheckBoxCheckedChanged(object sender, EventArgs e)
		{
			var oldValue = ColumnTextAtRow(sourceData, EditingRowNum);
			if (!EditValue.Equals(oldValue))
			{
				IsEditing = true;
				Commit(sourceData, EditingRowNum);
				ColumnStartedEditing((Control)sender);
			}
		}

		void EditControl_FindBoxModuleShowing(object sender, ModuleShowingEventArgs e)
		{
			var parentGrid = parentDataGrid as ZGrid;
			if (parentGrid != null)
			{
				parentGrid.OnFindBoxColumnModuleShowing(this, e);
			}
		}

		protected override object EditValue
		{
			get { return EditControl.GetValue(sourceData.GetCurrent() as BusinessObject, MappingName); }
		}

		const int ArbitraryMultiLineTextBoxHeight = 200;
#if DEBUG
		internal
#endif
		protected override Rectangle GetEditControlBounds(Rectangle bounds)
		{
			return (EditControl.ControlType == FieldType.TextMultiLine)
				? ControlDpiScalingHelper.NewScaledRectangle(bounds.X, bounds.Y, bounds.Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(ArbitraryMultiLineTextBoxHeight), false)
				: base.GetEditControlBounds(bounds);
		}

		protected internal override void HideEditControl()
		{
			if (editBusinessObject != null)
			{
				editBusinessObject.PropertyUpdated -= BusinessObjectUpdated;
				editBusinessObject = null;
				editRowNumber = -1;
				editRowBounds = default(Rectangle);
			}

			base.HideEditControl();
		}
#if DEBUG
		internal void EditExposed(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			Edit(source, rowNum, bounds, readOnly);
		}

		internal void CommitExposed(CurrencyManager currency, int rowNum)
		{
			Commit(currency, rowNum);
		}

		internal void EditExposed(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}
#endif
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if (CanEdit)
			{
				var bizObj = source.List[rowNum] as BusinessObject;
				if (bizObj != null)
				{
					sourceData = source;

					var bizObjWithNotifications = bizObj as INotifyPropertyUpdated;
					if (bizObjWithNotifications != null)
					{
						editRowNumber = rowNum;
						editRowBounds = bounds;

						bizObjWithNotifications.PropertyUpdated += BusinessObjectUpdated;
					}

					if (!ParentZGrid.IsCellEdited)
					{
						UpdateControlType(source, rowNum, bounds);
					}
				}
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		INotifyPropertyUpdated editBusinessObject;
		int editRowNumber = -1;
		Rectangle editRowBounds;
		readonly ZBool dataFieldsOnly = false;
		readonly ZBool allowMultipleMacroses = true;

		protected override void Dispose(bool disposing)
		{
			if (disposing && core != null)
			{
				core.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override void SetDataGrid(DataGrid value)
		{
			base.SetDataGrid(value);
			ParentZGrid = value as ZGrid;
		}

		protected override void SetDataGridInColumn(DataGrid value)
		{
			base.SetDataGridInColumn(value);
			ParentZGrid = value as ZGrid;
		}
		internal ZGrid ParentZGrid { get; private set; }

		#region ZCalcEditCore

		protected internal ZCalcEditCore Core
		{
			get { return core ?? (core = GetNewCalcEditCore()); }
		}
		ZCalcEditCore core;

		protected virtual ZCalcEditCore GetNewCalcEditCore()
		{
			return new ZCalcEditCore(TextBox);
		}

		protected int GetDecimalPlacesForCurrent(BusinessObject current)
		{
			if (ColumnIsInteger)
			{
				return 0;
			}

			var result = -1;
			if (current != null && ColumnProperty != null)
			{
				result = MetaData.GetDecimalPlaces(current, ColumnProperty);
			}
			if (result < 0 && ColumnSchema != null)
			{
				result = ColumnSchema.Scale;
			}
			if (result < 0)
			{
				result = Core.Decimals;
			}
			if (current != null && !current.IsDeleted && ColumnInfo != null && !string.IsNullOrEmpty(ColumnInfo.BindToDecimalPlaces))
			{
				result = ((INumericZType)current[ColumnInfo.BindToDecimalPlaces]).ToZInt();
			}

			return result;
		}

		#endregion

		void CacheValues()
		{
			if (!cached && ParentZGrid != null)
			{
				var keyCalculator = new ResourceStringKeyCalculator(ParentZGrid, MappingName);
				if (!string.IsNullOrEmpty(keyCalculator.FinalPropertyTableName))
				{
					var column = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(keyCalculator.PropertyName, keyCalculator.FinalPropertyTableName);
					columnSchema = column as SchemaDecimalColumn;
					columnIsInteger = columnSchema == null && column is SchemaNumericColumn;
				}

				columnProperty = ZCustomTypeDescriptor.GetProperties(ParentZGrid.ElementTypeFromCollection)[MappingName];
				if (!columnIsInteger && columnProperty != null)
				{
					columnIsInteger = typeof(INumericZType).IsAssignableFrom(columnProperty.PropertyType) && columnProperty.PropertyType != typeof(ZDecimal);
				}

				cached = true;
			}
		}
		bool cached;

		SchemaDecimalColumn ColumnSchema
		{
			get
			{
				CacheValues();
				return columnSchema;
			}
		}
		SchemaDecimalColumn columnSchema;

		PropertyDescriptor ColumnProperty
		{
			get
			{
				CacheValues();
				return columnProperty;
			}
		}
		PropertyDescriptor columnProperty;

		bool ColumnIsInteger
		{
			get
			{
				CacheValues();
				return columnIsInteger;
			}
		}
		bool columnIsInteger;

		void BusinessObjectUpdated(object sender, PropertyUpdatedEventArgs e)
		{
			var fieldTypePropertyName = ColumnInfo.FieldTypeColumnName;

			if (e.PropertyName == fieldTypePropertyName)
			{
				UpdateControlType(sourceData, editRowNumber, editRowBounds);
			}
		}

		void UpdateControlType(CurrencyManager source, int rowNum, [DpiState(DpiState.ScaledVariant)] Rectangle bounds)
		{
			var bizObj = source.List[rowNum] as BusinessObject;
			if (bizObj != null)
			{
				EditControl.ControlType = ControlTypeForBizObj(bizObj);
				EditControl.Edit(source, MappingName);

				GridControl.Text = ColumnTextAtRow(source, rowNum);
				IsEditing = false; //not until the field actually changes!

				if (EditControl.ControlType == FieldType.TextMultiLine)
				{
					TextBox.Multiline = true;
					TextBox.ScrollBars = ScrollBars.Both;
					TextBox.Bounds = ControlDpiScalingHelper.NewScaledRectangle(bounds.X, bounds.Y, bounds.Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(ArbitraryMultiLineTextBoxHeight), false);
				}
				else
				{
					TextBox.Multiline = false;
					TextBox.ScrollBars = ScrollBars.None;
					TextBox.Bounds = bounds;
				}

				var controlType = EditControl.ControlType;

				if (controlType == FieldType.AntlrMacro)
				{
					if (EditControl.GetControlForControlType(FieldType.AntlrMacro) is ZMacrosFindBox findBox)
					{
						findBox.Current = source.List[rowNum] as BusinessObject;
						findBox.PropertyDescriptor = PropertyDescriptor;
					}
				}
				else if (controlType == FieldType.TextMacro)
				{
					if (EditControl.GetControlForControlType(FieldType.TextMacro) is ZMacrosFindBox macrosFindBox)
					{
						macrosFindBox.DataFieldsOnly = dataFieldsOnly;
						macrosFindBox.AllowMultipleMacroses = allowMultipleMacroses;

						object parentJob = null;

						if (GetTopLevelForm() is ZForm form)
						{
							parentJob = form.DataSource;
						}

						macrosFindBox.PrepareControl(source.List[rowNum], parentJob, PropertyDescriptor);
						macrosFindBox.PrepareContextMenu(source, MappingName);
					}
				}
				if (EditControl.ControlType == FieldType.GuidDropEdit)
				{
					var dropEdit = EditControl.GetControlForControlType(FieldType.GuidDropEdit) as ZGuidDropEdit;
					dropEdit.ShowDescriptionBox = false;
				}
			}
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(
				source,
				rowNum,
				(EditControl.CurrentEditor is ZTextBox)
					? ControlDpiScalingHelper.NewScaledRectangle(bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), TextBox.Bounds.Y, bounds.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), TextBox.Bounds.Height, false)
					: bounds,
				readOnly);
		}
#if DEBUG
		internal
#endif
		protected override void InitialiseEditControlForNewPosition(BusinessObject bizObj, bool readOnly)
		{
			base.InitialiseEditControlForNewPosition(bizObj, readOnly);

			if (EditControl.ControlType == FieldType.TextDropEdit)
			{
				var editor = (ZGridDropEdit)EditControl.CurrentEditor;
				editor.CurrentItem = bizObj;
				editor.DataPropertyName = MappingName;
				editor.InvalidateList();
			}
		}

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			return base.ShouldColumnHandleKey(keyData) || EditControl.ShouldHandleKey(keyData);
		}

		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			if (EditControl.CurrentEditor is ZDateEdit dateEdit)
			{
				dateEdit.PushValue();
			}

			return base.Commit(source, rowNum);
		}

		protected override void Abort(int rowNum)
		{
			if (sourceData != null && rowNum < sourceData.Count)
			{
				GridControl.Text = ColumnTextAtRow(sourceData, rowNum);
			}
			else
			{
				GridControl.Text = "";
			}
		}

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			return IsCurrentCellReadOnly ? base.ColumnTextAtRow(source, rowNum) : base.GetColumnValueAtRow(source, rowNum);
		}

		public virtual void LoadModuleId(BusinessObject bizo, ZListUserControl listUserControl) { }

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			var bizObj = source as BusinessObject;
			var controlTypes = ControlTypeForBizObj(bizObj);

			switch (controlTypes)
			{
				case FieldType.OrganisationGuid:
				case FieldType.Guid:
				case FieldType.GuidDropEdit:
					var listControl = (ZListUserControl)EditControl.GetControlForControlType(controlTypes);

					var oldList = listControl.List;
					var oldCurrentItem = listControl.CurrentItem;

					try
					{
						if (ZGuid.TryParse(propertyValue, out var pk) && pk.IsValid)
						{
							listControl.CurrentItem = bizObj;
							listControl.DataPropertyName = MappingName;
							listControl.PullList();
							LoadModuleId(bizObj, listControl);

							var dropEdit = listControl as ZGridGuidDropEdit;
							return (dropEdit != null) ?
								dropEdit.GetCode(pk) :
								((IFindBox)listControl).ListProvider.CodeFromPrimaryKey(pk);
						}
						else
						{
							return FieldInvalidTextMemory.GetInvalidText(source, PropertyDescriptor.Name);
						}
					}
					finally
					{
						listControl.List = oldList;
						listControl.CurrentItem = oldCurrentItem;
					}
				case FieldType.Boolean:
					return CoerceToBool(propertyValue) ? Constants.BooleanTrueString : Constants.BooleanFalseString;
				case FieldType.Byte:
				case FieldType.Decimal:
				case FieldType.Integer:
					if (propertyValue == null || Equals(string.Empty, propertyValue))
					{
						propertyValue = ZInt.Zero;
					}
					if (bizObj == null)
					{
						return propertyValue.ToString();
					}

					return Core.Format(propertyValue, GetDecimalPlacesForCurrent(bizObj));
				case FieldType.LinkLabel:
					if (propertyValue is ZGuid && (ZGuid)propertyValue == UNDGConstants.UNDGDataItem.GridGuidForMany)
					{
						return Res.GetString("269E9FE0-9779-458E-ACCF-8E69D9AA39E6", "Many");
					}
					return base.FormatValueObjectCore(source, propertyValue);
				default:
					return base.FormatValueObjectCore(source, propertyValue);
			}
		}

		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			if (EditControl.ControlType == FieldType.Guid || EditControl.ControlType == FieldType.OrganisationGuid || EditControl.ControlType == FieldType.GuidDropEdit)
			{
				var guid = new ZGuid(value);
				if (!guid.IsValid)
				{
					var code = EditControl.ControlType == FieldType.GuidDropEdit ? EditControl.CurrentEditor.Text : ((IFindBox)EditControl.CurrentEditor).Code;
					FieldInvalidTextMemory.SetInvalidText(source.GetCurrent(), PropertyDescriptor.Name, code);
				}

				if (PropertyDescriptor.PropertyType == typeof(ZGuid))
				{
					base.SetColumnValueAtRow(source, rowNum, guid);
					return;
				}
			}

			base.SetColumnValueAtRow(source, rowNum, value);
		}

		internal new ZMultiCombinationControl EditControl
		{
			get { return (ZMultiCombinationControl)base.EditControl; }
		}

		internal FieldType ControlTypeForBizObj(BusinessObject bizObj)
		{
			if (bizObj == null)
			{
				return FieldType.Text;
			}

			var customColumnPropertyInfo = ZPropertyInfoRetriever.GetZPropertyInfo(this, bizObj);

			if (PropertyDescriptor is ZCustomPropertyDescriptor)
			{
				if (customColumnPropertyInfo == null)
				{
					return FieldType.Text;
				}

				if (customColumnPropertyInfo.BizObj != bizObj)
				{
					if (customColumnPropertyInfo.BizObj is CustomBusinessObject customBusinessObject)
					{
						var metaDataTypeFullName = customColumnPropertyInfo.Name + PropertyDescriptorCollectionWithMetaData.MetaDataPropertyNameSeparator + MetaDataTypes.ListDataSource;
						var listDataSourceMetaData = customBusinessObject.GetMetaData(metaDataTypeFullName);

						return listDataSourceMetaData == null ? FieldType.Text : FieldType.TextDropEdit;
					}

					return FieldType.Text;
				}
			}

			var fieldTypeRequiredString = ColumnInfo != null && !string.IsNullOrEmpty(ColumnInfo.FieldTypeColumnName) ? bizObj[ColumnInfo.FieldTypeColumnName]?.ToString() : null;

			if (!string.IsNullOrEmpty(fieldTypeRequiredString))
			{
				return (FieldType)Enum.Parse(typeof(FieldType), fieldTypeRequiredString, true);
			}

			return FieldType.Text;
		}

		#region ICustomKeyHandlingGridColumn Members

		#if !WINZOR

		public override bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			if (EditControl.CurrentEditor is ZGridFindBox)
			{
				return ReadOnly && keyData == Keys.F3;
			}

			return base.ShouldProcessCmdKey(ref m, keyData) || (
					(EditControl.ControlType == FieldType.TextMultiLine) &&
					MultiLineTextBoxGridHelper.ShouldProcessCmdKey(ref m, keyData) &&
					!IsCellReadOnly(sourceData, EditingRowNum));
		}

		public override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			if (EditControl.CurrentEditor is ZGridFindBox)
			{
				return ((ZGridFindBox)EditControl.CurrentEditor).GetCodeBox().HandleKey(keyData);
			}

			return MultiLineTextBoxGridHelper.ProcessCmdKey((DataGridTextBox)TextBox, ref m, keyData) || base.ProcessCmdKey(ref m, keyData);
		}

#endif

		#endregion
	}
}
