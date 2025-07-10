using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	class ZGridImportCollectionInfo : IImportCollectionInfo
	{
		public ZGridImportCollectionInfo(ZGrid grid)
		{
			this.grid = grid;
		}

		public IBusinessObjectCollection Collection
		{
			get { return grid.ListManager == null ? null : grid.ListManager.List as IBusinessObjectCollection; }
		}

		public IEnumerable<IImportPropertyInfo> Properties
		{
			get
			{
				if (properties == null)
				{
					properties = new List<IImportPropertyInfo>();
					foreach (var column in grid.Columns)
					{
						if (column != null &&
							column.IsVisible &&
							column.ColumnStyle != null &&
							column.ColumnStyle.PropertyDescriptor != null &&
							column.ColumnStyle.PropertyDescriptor.PropertyType != null)
						{
							properties.Add(new ZGridImportPropertyInfo(column));
						}
					}
				}

				return properties;
			}
		}

		List<IImportPropertyInfo> properties;
		readonly ZGrid grid;

		#region IImportCollectionInfo Members

		bool IImportCollectionInfo.ValidateAndSave { get { return false; } }

		void IImportCollectionInfo.OnImportStarted()
		{ }

		void IImportCollectionInfo.OnImportCompleted(bool success)
		{
			grid.Focus();
		}

		#endregion
	}

#if DEBUG
	internal
#endif
 class ZGridImportPropertyInfo : IImportPropertyInfo
	{
		public ZGridImportPropertyInfo(ZGridColumn gridColumn, bool isMandatory = false)
		{
			this.gridColumn = gridColumn;
			IsMandatory = isMandatory;
		}

		public string HeaderText
		{
			get { return gridColumn.ColumnStyle.HeaderText; }
		}

		public string MappingName
		{
			get { return gridColumn.ColumnStyle.MappingName; }
		}

		public bool IsMandatory { get; }

		public Type PropertyType
		{
			get
			{
				if (gridColumn.ColumnStyle.PropertyDescriptor.PropertyType != typeof(IZType))
				{
					return gridColumn.ColumnStyle.PropertyDescriptor.PropertyType;
				}
				else
				{
					return gridColumn.ColumnStyle.GetType() == typeof(ZTextBoxColumnStyle) ? typeof(ZString) :
						gridColumn.ColumnStyle.GetType() == typeof(ZCalcEditColumnStyle) ? typeof(ZDecimal) :
						typeof(IZType);
				}
			}
		}

		public Type ComponentType
		{
			get { return gridColumn.ColumnStyle.PropertyDescriptor.ComponentType; }
		}

		public int ColumnWidth
		{
			get { return gridColumn.ColumnStyle.Width; }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList GetBindToList(BusinessObject bizObj)
		{
			var result = CargoWise.ComponentModel.MetaData.GetListDataSource(bizObj, gridColumn.ColumnStyle.PropertyDescriptor) as IList;
			if (result != null)
			{
				return result;
			}

			var propertyType = gridColumn.ColumnStyle.PropertyDescriptor.PropertyType;
			if (ImportWizard.IsType(propertyType, typeof(ZGuid)) || ImportWizard.IsType(propertyType, typeof(ZString)))
			{
				string bindToList = null;
				Control editControl = null;

				var findBoxStyle = gridColumn.ColumnStyle as ZBaseFindBoxColumnStyle;
				if (findBoxStyle != null)
				{
					var findBox = (ZGridFindBox)findBoxStyle.EditControl;
					bindToList = findBox.BindToList;
					editControl = findBox;
				}

				if (bindToList == null)
				{
					var multiColumnStyle = gridColumn.ColumnStyle as ZMultiControlColumnStyle;
					if (multiColumnStyle != null)
					{
						var multiControl = multiColumnStyle.EditControl;
						bindToList = multiControl.BindToList;
						editControl = multiControl;
					}
				}

				if (bindToList == null)
				{
					var dropEditColumnStyle = gridColumn.ColumnStyle as ZDropEditColumnStyle;
					if (dropEditColumnStyle != null)
					{
						var dropEdit = (ZDropEdit)dropEditColumnStyle.EditControl;
						bindToList = dropEdit.BindToList;
						editControl = dropEdit;
					}
				}

				if (!String.IsNullOrEmpty(bindToList))
				{
					result = ZListUserControl.GetList(bizObj, bindToList, editControl, "");
					if (result == null)
					{
						var lastDot = bindToList.LastIndexOf('.');
						if (lastDot != -1)
						{
							bindToList = bindToList.Substring(0, lastDot) + "+" + bindToList.Substring(lastDot + 1);
							result = ZListUserControl.GetList(bizObj, bindToList, editControl, "");
						}
					}
					return result;
				}
			}

			return null;
		}

		public ModuleIdentifier GetModuleID(BusinessObject bizObj)
		{
			var propertyType = gridColumn.ColumnStyle.PropertyDescriptor.PropertyType;
			ModuleIdentifier result = null;
			if (ImportWizard.IsType(propertyType, typeof(ZGuid)) || ImportWizard.IsType(propertyType, typeof(ZString)))
			{
				var findBoxStyle = gridColumn.ColumnStyle as ZBaseFindBoxColumnStyle;
				if (findBoxStyle != null)
				{
					var findBox = (ZGridFindBox)findBoxStyle.EditControl;
					result = findBox.ModuleID;
				}

				if (result == null || result == ModuleIDs.NotAssigned)
				{
					var multiColumnStyle = gridColumn.ColumnStyle as ZMultiControlColumnStyle;
					if (multiColumnStyle != null)
					{
						var multiControl = multiColumnStyle.EditControl;
						result = multiControl.ModuleID;
					}
				}

				if (result == null || result == ModuleIDs.NotAssigned)
				{
					var list = GetBindToList(bizObj);
					if (list != null)
					{
						result = ZMetaData.GetModuleId(list);
					}
				}
			}

			return result;
		}

		public bool IsMultiControl
		{
			get { return gridColumn.ColumnStyle is ZMultiControlColumnStyle; }
		}

		public Type GetExpectedTypeForMultiControl(BusinessObject bizObj)
		{
			return FieldTypeToZType(((ZMultiControlColumnStyle)gridColumn.ColumnStyle).ControlTypeForBizObj(bizObj));
		}

		public static Type FieldTypeToZType(FieldType fieldType)
		{
			switch (fieldType)
			{
				case FieldType.Guid:
				case FieldType.OrganisationGuid:
				case FieldType.GuidDropEdit:
					return typeof(ZGuid);

				case FieldType.DateTime:
				case FieldType.Date:
					return typeof(ZDateTime);

				case FieldType.DateTimeOffset:
					return typeof(ZDateTimeOffset);

				case FieldType.Geography:
					return typeof(ZGeography);

				case FieldType.Time:
					return typeof(ZTime);

				case FieldType.Decimal:
					return typeof(ZDecimal);

				case FieldType.Byte:
					return typeof(ZByte);

				case FieldType.Integer:
					return typeof(ZInt);

				case FieldType.Boolean:
					return typeof(ZBool);

				case FieldType.Text:
				case FieldType.TextDropEdit:
				case FieldType.TextCodeFindBox:
				case FieldType.TextMultiLine:
				case FieldType.TextMacro:
				case FieldType.AntlrMacro:
				case FieldType.LinkLabel:
					return typeof(ZString);

				//case FieldType.AnyType:
				//case FieldType.ThatIsNotSupposed:
				//case FieldType.ToBeOnAGrid:
				//case FieldType.WithDataImportFeature:
				//	throw new NotSupportedException();

				default:
					throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, "Mapping of this field type to ZType is not defined.");
			}
		}

		public ZCharacterCasing CharacterCasing
		{
			get
			{
				var textboxColumnStyle = gridColumn.ColumnStyle as ZTextBoxColumnStyle;
				if (textboxColumnStyle != null)
				{
					return (ZCharacterCasing)textboxColumnStyle.CharacterCasing;
				}

				return ZCharacterCasing.Normal;
			}
		}

		public bool ProperCaseConvertable
		{
			get { return true; }
		}

		public bool IsReadOnly
		{
			get { return gridColumn.ColumnStyle.ReadOnly; }
		}

		public DataGridColumnStyle ColumnStyle
		{
			get
			{
				return gridColumn.ColumnStyle;
			}
		}

		public string FieldTypeColumnName
		{
			get
			{
				var multiControlColumnStyle = gridColumn.ColumnStyle as ZMultiControlColumnStyle;
				if (multiControlColumnStyle != null)
				{
					return multiControlColumnStyle.ColumnInfo.FieldTypeColumnName;
				}
				return string.Empty;
			}
		}

		readonly ZGridColumn gridColumn;
	}
}
