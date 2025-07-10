using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal class ZDataRow : DataRow, IColumnIndexer
	{
		public ZDataRow(DataRowBuilder builder)
			: base(builder)
		{ }

		public bool HasSource(string columnName)
		{
			return valueSources != null && LazyLoading.LoadRequired(this[columnName]) && valueSources.ContainsKey(columnName);
		}

		public IStreamSource GetStreamSource(string columnName)
		{
			return GetSource<IStreamSource>(columnName);
		}

		public ITextReaderSource GetReaderSource(string columnName)
		{
			return GetSource<ITextReaderSource>(columnName);
		}

		public void ClearSources()
		{
			if (valueSources != null)
			{
				var sources = valueSources.Values.OfType<IDisposable>();
				foreach (var source in sources)
				{
					source.Dispose();
				}

				valueSources.Clear();
			}
		}

		T GetSource<T>(string columnName)
		{
			T source = default(T);
			if (valueSources != null && LazyLoading.LoadRequired(this[columnName]) && valueSources.ContainsKey(columnName))
			{
				source = (T)valueSources[columnName];
			}
			return source;
		}

		public void SetReaderSource(IStreamSource source, string columnName)
		{
			SetSource(columnName, source, LazyLoading.BinaryPlaceholder);
		}

		public void SetReaderSource(ITextReaderSource source, string columnName)
		{
			SetSource(columnName, source, LazyLoading.TextPlacehoder);
		}

		void SetSource(string columnName, object source, object placeholder)
		{
			if (valueSources == null)
			{
				valueSources = new Dictionary<string, object>();
			}
			valueSources[columnName] = source;
			this[columnName] = placeholder;
			if (this.RowState == DataRowState.Unchanged)
			{
				this.SetModified();
			}
		}

		Dictionary<string, object> valueSources;

		#region IColumnIndexer Members

		object IIndexer.this[string propertyName]
		{
			get { return this[propertyName]; }
			set
			{
				var zTypeValue = value as IZTypeInternals;
				if (zTypeValue != null)
				{
					value = zTypeValue.GetValueForLogicalDataLayer(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(propertyName, Table.TableName).IsNullable);
				}
				this[propertyName] = value;
			}
		}

		string IColumnIndexer.TableName
		{
			get { return Table.TableName; }
		}

		#endregion
	}

	internal static class DataRowExtensions
	{
		public static bool IsValueChanged(this DataRow row, DataColumn column)
		{
			return ZDataUtils.IsRowValueChanged(row, column);
		}

		public static void ClearSources(this DataRow row)
		{
			if (row is ZDataRow)
			{
				((ZDataRow)row).ClearSources();
			}
		}
	}
}
