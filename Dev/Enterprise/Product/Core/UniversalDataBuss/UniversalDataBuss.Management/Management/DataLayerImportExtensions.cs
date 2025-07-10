using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class DataLayerImportExtensions
	{
		public static void DeleteAll(this IEnumerable<DataRow> dataRows)
		{
			if (dataRows != null)
			{
				foreach (var row in dataRows)
				{
					row.Delete();
				}
			}
		}

		public static void DeleteAllJobDocAddresses(this IColumnIndexer docAddressParent, RowFactory rowFactory, SchemaPKColumn pkColumn)
		{
			var jobDocAddressQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, docAddressParent[pkColumn.Name]);
			rowFactory.LoadAndDeleteRows(JobDocAddressSchema.Constants.TableName, jobDocAddressQuery);
		}

		public static void LoadAndDeleteRows(this RowFactory rowFactory, string tableName, ZQuery filter)
		{
			rowFactory.Load(tableName, filter).DeleteAll();
		}

		public static IColumnIndexer NewRowWithPK(this RowFactory factory, ITableSchema schema)
		{
			var newRow = factory.New(schema.TableName);
			newRow[schema.PK.Name] = Guid.NewGuid();
			newRow.Table.Rows.Add(newRow);
			return newRow as IColumnIndexer;
		}
	}
}
