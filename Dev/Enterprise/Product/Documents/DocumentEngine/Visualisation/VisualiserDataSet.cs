using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserDataSet : DataSet, ICloneable // need DS for resilising to XML
	{
		public const string MainBusinessObjectTableName = "MainBizObject";

		public VisualiserDataSet()
			: base()
		{
			UnTouchedTableNames = new List<string>();
			Tables.CollectionChanged += new CollectionChangeEventHandler(Tables_CollectionChanged);
			fMainTable = new DataTable(MainBusinessObjectTableName);
			Tables.Add(fMainTable);
			fMainTable.Rows.Add(Array.Empty<object>());
			DataSetName = "VisualiserDataSet";
			UnTouchedTableNames.Remove(MainBusinessObjectTableName);
		}

		#region MainTable
		DataTable fMainTable;
		public DataTable MainTable
		{
			get
			{
				return fMainTable;
			}
		}

		public DataRow MainRow
		{
			get
			{
				return MainTable.Rows[0];
			}
		}
		#endregion

		#region ChangeTracking
#if DEBUG
		protected
#endif
 List<string> UnTouchedTableNames;
		List<string> ChangedFieldNames;

		void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
		{
			OnRowDeleted(e.Row);
		}

		void OnRowDeleted(DataRow dataRow)
		{
			if (!fSuspendChangeTracking)
			{
				if (dataRow.Table.TableName != MainBusinessObjectTableName)
				{
					UnTouchedTableNames.Remove(dataRow.Table.TableName);
				}
			}
		}

		void Table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			OnColumnChanged(e.Column);
		}

		void OnColumnChanged(DataColumn col)
		{
			if (!fSuspendChangeTracking)
			{
				if (col.Table.TableName != MainBusinessObjectTableName)
				{
					UnTouchedTableNames.Remove(col.Table.TableName);
				}
				else
				{
					ChangedFieldNames.Add(col.ColumnName);
				}
			}
		}

		public IDisposable SuspendChangeTracking
		{
			get
			{
				return new ChangeTrackingSuspender(this);
			}
		}

		class ChangeTrackingSuspender : IDisposable
		{
			readonly VisualiserDataSet Parent;
			public ChangeTrackingSuspender(VisualiserDataSet parent)
			{
				this.Parent = parent;
				parent.fSuspendChangeTracking = true;
			}

			#region IDisposable Members

			public void Dispose()
			{
				Parent.fSuspendChangeTracking = false;
			}

			#endregion
		}
		#endregion

		#region Serialisation

		internal bool fSuspendChangeTracking;
		public void DeSerialise(string xml)
		{
			Tables.Clear();
			Clear();

			using (TextReader reader = new StringReader(xml))
			{
				ReadXmlSchema(reader);  // ReadXml is supposed to read schema and data, but it just reads data.
			}
			using (TextReader reader = new StringReader(xml))
			{
				ReadXml(reader);
			}
			fMainTable = Tables[MainBusinessObjectTableName];

			if (fMainTable == null)
			{
				fMainTable = new DataTable(MainBusinessObjectTableName);
				Tables.Add(fMainTable);
				fMainTable.Rows.Add(Array.Empty<object>());
			}

			foreach (DataTable table in Tables)
			{
				if (table == fMainTable)
				{
					foreach (DataColumn col in table.Columns)
					{
						OnColumnChanged(col);
					}
				}
				else
				{
					if (table.Columns.Count > 0)
					{
						OnColumnChanged(table.Columns[0]);
					}
				}
			}
		}

		void Tables_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
		{
			if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
			{
				DataTable table = e.Element as DataTable;
				table.ColumnChanged += new DataColumnChangeEventHandler(Table_ColumnChanged);
				table.RowDeleted += new DataRowChangeEventHandler(Table_RowDeleted);
				UnTouchedTableNames.Add(table.TableName);
				if (table.TableName == MainBusinessObjectTableName)
				{
					ChangedFieldNames = new List<string>();
				}
			}
			else if (e.Action == System.ComponentModel.CollectionChangeAction.Remove)
			{
				DataTable table = e.Element as DataTable;
				table.ColumnChanged -= new DataColumnChangeEventHandler(Table_ColumnChanged);
				table.RowDeleted -= new DataRowChangeEventHandler(Table_RowDeleted);
			}
		}

		DataSet GetDataSetWithUnchangedMainFieldsAndChildTablesRemoved() // need DS for resilising to XML
		{
			DataSet dS = Copy();// need DS for resilising to XML
			foreach (string tableNameToRemove in UnTouchedTableNames)
			{
				if (tableNameToRemove != MainBusinessObjectTableName)
				{
					dS.Tables.Remove(tableNameToRemove);
				}
			}

			DataTable mainTable = dS.Tables[MainBusinessObjectTableName];
			for (int columnNumber = 0; columnNumber < mainTable.Columns.Count;)
			{
				if (ChangedFieldNames.Contains(mainTable.Columns[columnNumber].ColumnName))
				{
					columnNumber++;
				}
				else
				{
					mainTable.Columns.RemoveAt(columnNumber);
				}
			}
			if (mainTable.Columns.Count == 0)
			{
				dS.Tables.Remove(MainBusinessObjectTableName);
			}

			foreach (DataTable dataTable in dS.Tables)
			{
				foreach (DataColumn dataColumn in dataTable.Columns)
				{
					if (dataColumn.DataType == typeof(string))
					{
						foreach (DataRow dataRow in dataTable.Rows)
						{
							ZString value = new ZString(dataRow[dataColumn]);
							dataRow[dataColumn] = value.ExcludeNonValidXMLCharacters().ToString();
						}
					}
				}
			}

			return dS;
		}

		public byte[] SerialiseToByteArray()
		{
			DataSet dS = GetDataSetWithUnchangedMainFieldsAndChildTablesRemoved();// need DS for resilising to XML
			using (MemoryStream stream = new MemoryStream())
			{
				dS.WriteXml(stream, XmlWriteMode.WriteSchema);
				stream.Position = 0;
				return stream.ToArray();
			}
		}

		#endregion

		public static string GetTableName(string collectionName)
		{
			return TableNamePrefix + collectionName.Replace(".", "_");
		}

		const string TableNamePrefix = "VisualiserTable_";

		internal static string GetCollectionName(string tableName)
		{
			if (tableName.StartsWith(TableNamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				tableName = tableName.Remove(0, TableNamePrefix.Length);
			}

			return tableName;
		}

		#region ICloneable Members
		public new VisualiserDataSet Clone()
		{
			VisualiserDataSet result = Copy() as VisualiserDataSet;
			result.UnTouchedTableNames.Clear();
			result.UnTouchedTableNames.AddRange(UnTouchedTableNames);
			result.ChangedFieldNames.Clear();
			result.ChangedFieldNames.AddRange(ChangedFieldNames);
			result.fMainTable = result.Tables[MainBusinessObjectTableName];
			result.VisualiserGridStyles.AddRange(VisualiserGridStyles);
			return result;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
		#endregion

		internal void AddColumnToDataTableIfNotAddedAlreadyWithStyle(string dataTableName, Area areaContainingFields, int bodyRowCount, string columnDataSource, CellFormat format, Size cellSize)
		{
			var columnDataType = GetColumnDataSourceType(areaContainingFields, columnDataSource);
			var columnName = GetColumnName(columnDataSource, GetCollectionName(dataTableName));

			if (Tables.Contains(dataTableName) && !string.IsNullOrEmpty(columnName))
			{
				var dataTable = Tables[dataTableName];

				if (!dataTable.Columns.Contains(columnName))
				{
					using (SuspendChangeTracking)
					{
						dataTable.Columns.Add(columnName, columnDataType);

						for (int currentBodyRow = 0; currentBodyRow < bodyRowCount && currentBodyRow < dataTable.Rows.Count; currentBodyRow++)
						{
							object columnValue;
							if (ModifiableField.MacroRegex.IsMatch(columnDataSource))
							{
								columnValue = areaContainingFields.ReplaceMacros(columnDataSource);
							}
							else
							{
								columnValue = areaContainingFields.GetColumnValue(currentBodyRow, columnDataSource);
							}

							if (columnValue is ZBool)
							{
								columnValue = (bool)(ZBool)columnValue;
							}
							else if (columnValue is ZInt)
							{
								columnValue = (int)(ZInt)columnValue;
							}
							else if (columnValue is ZDecimal)
							{
								columnValue = (decimal)(ZDecimal)columnValue;
							}
							else if (columnValue is ZLong)
							{
								columnValue = (long)(ZLong)columnValue;
							}
							else if (columnValue is ZDateTime)
							{
								columnValue = GetDateTimeColumnValue(columnValue, col => ((ZDateTime)col).ToDateTime());
							}
							else if (columnValue is ZDateTimeOffset)
							{
								columnValue = GetDateTimeColumnValue(columnValue, col => ((ZDateTimeOffset)col).ToDateTime());
							}

							if (columnValue != null)
							{
								dataTable.Rows[currentBodyRow][columnName] = columnValue;
							}
						}
					}
				}
			}

			AddStyleToVisualiserGridStyles(dataTableName, columnName, false, format, cellSize);
		}

		object GetDateTimeColumnValue(object columnValue, Func<object, DateTime> func)
		{
			var timeValue = (IZType)columnValue;
			if (timeValue.IsValid)
			{
				columnValue = func(columnValue);
			}
			else
			{
				columnValue = null;
			}
			return columnValue;
		}

		internal static Type GetColumnDataSourceType(Area areaContainingFields, string columnDataSource)
		{
			Type columnDataType = typeof(string);
			BusinessObjectDataProvider boDataProvider = areaContainingFields.ParentReport.DataProvider as BusinessObjectDataProvider;
			if (boDataProvider != null)
			{
				var topLevelDataSource = BODocDataProvider.GetObject(boDataProvider.TopLevelDataSources.PrimaryDataProvider);
				MethodInfoChainLink[] methodInfoChainLink = new BusinessObjectReflector().GetMethodInfoChain(topLevelDataSource.GetType(), topLevelDataSource, columnDataSource);
				if (methodInfoChainLink != null &&
					 methodInfoChainLink.Length > 0)
				{
					MethodInfoChainLink lastLink = methodInfoChainLink[methodInfoChainLink.Length - 1];
					columnDataType = lastLink.MethodInfo.ReturnType;
					if (columnDataType == typeof(ZDecimal))
					{
						columnDataType = typeof(decimal);
					}
					else if (columnDataType == typeof(ZBool))
					{
						columnDataType = typeof(bool);
					}
					else if (columnDataType == typeof(ZInt))
					{
						columnDataType = typeof(int);
					}
					else if (columnDataType == typeof(ZLong))
					{
						columnDataType = typeof(long);
					}
					else if (columnDataType == typeof(ZDateTime) || columnDataType == typeof(ZDateTimeOffset))
					{
						columnDataType = typeof(DateTime);
					}
					else
					{
						columnDataType = typeof(string);
					}
				}
			}

			return columnDataType;
		}

		#region Visualiser Grid Styles
		void AddStyleToVisualiserGridStyles(string dataTableName, string dataColumnName, bool useFlexCelNumericFormatting, CellFormat cellFormat, Size cellSize)
		{
			if (FindStyleFromTableAndField(dataTableName, dataColumnName) == null)
			{
				VisualiserGridStyles.Add(new VisualiserGridStyle(dataTableName, dataColumnName, useFlexCelNumericFormatting, cellFormat, cellSize));
			}
		}

		public VisualiserGridStyle FindStyleFromTableAndField(string dataTableName, string dataColumnName)
		{
			return VisualiserGridStyles.Find(delegate(VisualiserGridStyle gridStyle)
			{ return gridStyle.TableName == dataTableName && gridStyle.ColumnName == dataColumnName; });
		}

#if DEBUG
		public
#endif
 List<VisualiserGridStyle> VisualiserGridStyles = new List<VisualiserGridStyle>();

		public class VisualiserGridStyle
		{
			public VisualiserGridStyle(string tableName, string columnName, bool useFlexCelNumericFormatting, CellFormat cellFormat, Size cellSize)
			{
				TableName = tableName;
				ColumnName = columnName;
				UseFlexCelNumericFormatting = useFlexCelNumericFormatting;
				CellFormat = cellFormat;
				CellSize = cellSize;
			}

			public readonly string TableName;
			public readonly string ColumnName;
			public readonly bool UseFlexCelNumericFormatting;
			public readonly CellFormat CellFormat;
			public readonly Size CellSize;
		}

		internal static bool UsesFlexCelNumericFormatting(Type columnDataType)
		{
			return columnDataType == typeof(decimal);
		}

		internal static string GetColumnName(string macroText, string tableName)
		{
			var result = macroText.Trim(new char[] { ' ', '.' });

			var formatExtractor = FormatFunctionExtractor.ParseAndExtract(result);
			if (formatExtractor != null)
			{
				result = Regex.Replace(result, @"Format\(.*?\)", formatExtractor.PropertyLabel, RegexOptions.IgnoreCase);
			}

			var totalExtractor = TotalFunctionExtractor.ParseAndExtract(result);
			if (totalExtractor != null)
			{
				return totalExtractor.PropertyLabel;
			}

			var docDataExtractor = DocDataFunctionExtractor.ParseAndExtract(result);
			if (docDataExtractor != null)
			{
				return docDataExtractor.PropertyLabel;
			}

			if (result.StartsWith(tableName + ".", StringComparison.OrdinalIgnoreCase))
			{
				result = result.Remove(0, tableName.Length + 1);
			}

			return result.Replace(".", "");
		}

		#endregion
	}
}
