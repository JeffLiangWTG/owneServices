using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Utilities
{
	public class DataGridExcelExportHelper
	{
		public DataGridExcelExportHelper(DataGrid gridToExport)
		{
			collection = gridToExport.DataSource as IBusinessObjectCollection;
			columns = gridToExport.Columns;
		}

		public DataGridExcelExportHelper(IBusinessObjectCollection collectionToExport, DataGridColumnCollection dataGridColumnsToExport)
		{
			collection = collectionToExport;
			columns = dataGridColumnsToExport;
		}

		readonly IBusinessObjectCollection collection;
		readonly DataGridColumnCollection columns;

		public List<ExcelExportColumnBase> GetExcelExportColumns()
		{
			List<ExcelExportColumnBase> result = new List<ExcelExportColumnBase>();
			if (CanContinueWithExport && columns != null)
			{
				AddExcelExportColumns(result, columns);
			}
			return result;
		}

		protected void AddExcelExportColumns(List<ExcelExportColumnBase> excelColumns, IEnumerable columns)
		{
			if (columns != null)
			{
				foreach (DataGridColumn column in columns)
				{
					AddExcelExportColumns(excelColumns, column);
				}
			}
		}

		protected void AddExcelExportColumns(List<ExcelExportColumnBase> excelColumns, DataGridColumn column)
		{
			if (column is ZGroupColumn)
			{
				AddExcelExportColumns(excelColumns, ((ZGroupColumn)column).GroupMembers);
			}
			else
			{
				if (ColumnIsVisible(column) && (IsColumnBoundToProperty(column) || IsColumnBoundToCollection(column)))
				{
					ExcelExportColumnBase exportColumn = GetNewExcelExportColumn(column);
					excelColumns.Add(exportColumn);
				}
			}
		}

		public bool CanContinueWithExport
		{
			get { return collection != null && collection.Count > 0 && columns != null && columns.Count > 0; }
		}

		public static bool ColumnIsVisible(DataGridColumn gridColumn)
		{
			ZNewRowColumn newRowColumn = gridColumn as ZNewRowColumn;

			return newRowColumn != null ? (newRowColumn.Visible && !newRowColumn.Collapsable) : gridColumn.Visible;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1081:DoNotUseBusienssObjectCollectionIsAssignableFrom", Justification = "Baseline")]
		bool IsColumnBoundToCollection(DataGridColumn gridColumn)
		{
			Type boundType = null;

			var templateColumn = gridColumn as ZTemplateColumn;
			if (templateColumn != null && !string.IsNullOrEmpty(templateColumn.BindTo))
			{
				boundType = GetBoundZType(templateColumn.BindTo);
			}

			return boundType != null &&
				(boundType.IsSubclassOf(typeof(BusinessObjectCollection)) || boundType.IsAssignableFrom(typeof(IBusinessObjectCollection)));
		}

		bool IsColumnBoundToProperty(DataGridColumn gridColumn)
		{
			var templateColumn = gridColumn as ZTemplateColumn;
			var buttonColumn = gridColumn as ZButtonColumn;
			var dropDownColumn = gridColumn as ZDropDownListColumn;

			Type boundType = null;

			if (templateColumn != null && !string.IsNullOrEmpty(templateColumn.BindTo))
			{
				boundType = GetBoundZType(templateColumn.BindTo);
			}
			else if (buttonColumn != null)
			{
				boundType = GetBoundZType(buttonColumn.DataTextField);
			}

			if (!string.IsNullOrEmpty(dropDownColumn?.BindToList) && !ZPropertyAccessor.TryGet(collection[0], dropDownColumn.BindToList, out var _))
			{
				return false;
			}

			return boundType != null && boundType.GetInterface(typeof(IZType).FullName) != null;
		}

		Type GetBoundZType(string bindTo)
		{
			if (string.IsNullOrEmpty(bindTo))
			{
				return null;
			}

			var descriptor = TypeDescriptor.GetProperties(collection[0]).Find(bindTo, true);
			if (descriptor != null)
			{
				return descriptor.PropertyType;
			}

			foreach (var item in collection)
			{
				if (ZPropertyAccessor.TryGet(item, bindTo, out var propertyValue))
				{
					return propertyValue.GetType();
				}
			}
			return null;
		}

		ExcelExportColumnBase GetNewExcelExportColumn(DataGridColumn column)
		{
			ExcelExportColumnBase result;

			SchemaColumn schemaColumn = null;
			ZTemplateColumn templateColumn = column as ZTemplateColumn;
			ZButtonColumn buttonColumn = column as ZButtonColumn;
			ZHyperLinksColumn hyperLinksColumn = column as ZHyperLinksColumn;

			if (templateColumn != null && hyperLinksColumn == null)
			{
				schemaColumn = GetSchemaColumnForExcelExport(templateColumn.BindTo);
			}
			else if (buttonColumn != null)
			{
				schemaColumn = GetSchemaColumnForExcelExport(buttonColumn.DataTextField);
			}

			string description = column.HeaderText;
			if (description.Length > 1 && char.IsControl(description[description.Length - 1])) // remove bad chars at the end of the string
			{
				description = description.Remove(description.Length - 2, 2);
			}

			if (hyperLinksColumn != null)
			{
				result = new ExcelExportCollectionColumn(description, hyperLinksColumn.BindTo, hyperLinksColumn.BindToField);
			}
			else
			{
				IExcelExportCustomFunction customFunction = column as IExcelExportCustomFunction;
				IExcelExportCustomValue valueColumn = column as IExcelExportCustomValue;
				if (valueColumn != null)
				{
					result = ExcelExportCustomValueColumn.New(valueColumn, customFunction);
				}
				else
				{
					result = ExcelExportColumn.New(schemaColumn, description, customFunction);

					if (schemaColumn.ColumnType == SchemaColumnType.DateTime)
					{
						ZDateTimeColumn dateTimeColumn = column as ZDateTimeColumn;
						if (dateTimeColumn != null)
						{
							((ExcelExportDateTimeColumn)result).DateTimeFormat = dateTimeColumn.DateTimeFormat;
						}
					}

					if (schemaColumn.ColumnType == SchemaColumnType.Guid)
					{
						ZFindBoxColumn findBoxColumn = column as ZFindBoxColumn;
						if (findBoxColumn != null)
						{
							((ExcelExportGuidColumn)result).DisplayStyle = findBoxColumn.DisplayStyle;
						}
					}
				}
			}

			const int defaultColumnWidth = 80;
			int editorWidth = (templateColumn != null && templateColumn.EditorWidth > 0) ? templateColumn.EditorWidth : defaultColumnWidth;
			result.Width = editorWidth * 48;

			return result;
		}

		SchemaColumn GetSchemaColumnForExcelExport(string columnBindTo)
		{
			SchemaColumn result = null;

			if (TableSchema != null)
			{
				result = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(columnBindTo, TableSchema.TableName);
			}

			if (result == null)
			{
				Type boundZType = GetBoundZType(columnBindTo);

				if (boundZType == typeof(ZDateTime))
				{
					result = new SchemaDateTimeColumn(TableSchema, columnBindTo, 0, SqlDbType.DateTime, null, false);
				}
				else if (boundZType == typeof(ZDecimal))
				{
					result = new SchemaDecimalColumn(TableSchema, columnBindTo, 0, SqlDbType.Decimal, 0m, false, 0, 0);
				}
				else if (boundZType == typeof(ZGuid))
				{
					result = new SchemaGuidColumn(TableSchema, columnBindTo, 0, null, true);
				}
				else
				{
					result = new SchemaStringColumn(TableSchema, columnBindTo, 0, SqlDbType.VarChar, "", false, 10);
				}
			}

			return result;
		}

		ITableSchema TableSchema
		{
			get
			{
				if (tableSchema == null)
				{
					if (ElementTypeFromCollection.IsClass && BusinessObjectFactory.HasTableName(ElementTypeFromCollection))
					{
						string tableName = BusinessObjectFactory.GetTableNameFromType(ElementTypeFromCollection);
						tableSchema = EnterpriseSchema.GetTableSchema(tableName);
					}
					else
					{
						tableSchema = CargoWise.Schema.Schema.GenericTableSchema; // eg. non-persistant bizo that is not auto-generated from xml
					}
				}

				return tableSchema;
			}
		}
		ITableSchema tableSchema;

		protected Type ElementTypeFromCollection
		{
			get { return collection.TypeOfElements; }
		}
	}
}
