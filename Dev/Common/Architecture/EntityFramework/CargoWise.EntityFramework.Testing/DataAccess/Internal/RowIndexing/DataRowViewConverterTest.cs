using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DataRowViewConverterTest : TestCase
	{
		public void TestConvertFiltersRowWithComplexQuery()
		{
			RowFactory rowFactory = new RowFactory();
			DataTable table = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowMatching = rowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
			rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching[DummyBizoSchema.Z0_Number.Name] = 1;
			rowMatching.Table.Rows.Add(rowMatching);

			DataRow rowNotMatching = rowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowNotMatching);
			rowNotMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowNotMatching[DummyBizoSchema.Z0_Number.Name] = 2;
			rowNotMatching.Table.Rows.Add(rowNotMatching);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);
			DataRowViewConverter converter = new DataRowViewConverter(filter, rowFactory);

			DataView dataView = new DataView(table, "", DummyBizoSchema.Z0_Code.Name, DataViewRowState.CurrentRows);
			DataRowView[] dataRowViews = dataView.FindRows("123");
			AssertEquals(2, dataRowViews.Length);

			DataRow[] rows = converter.Convert(dataRowViews);
			AssertEquals(1, rows.Length);
			AssertEquals(rowMatching, rows[0]);
		}

		public void TestConvertFiltersRowWithSimpleQuery()
		{
			RowFactory rowFactory = new RowFactory();
			DataTable table = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowMatching = rowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
			rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching[DummyBizoSchema.Z0_Number.Name] = 1;
			rowMatching.Table.Rows.Add(rowMatching);

			DataRow rowMatching2 = rowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching2);
			rowMatching2[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching2[DummyBizoSchema.Z0_Number.Name] = 2;
			rowMatching2.Table.Rows.Add(rowMatching2);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			DataRowViewConverter converter = new DataRowViewConverter(filter, rowFactory);

			DataView dataView = new DataView(table, "", DummyBizoSchema.Z0_Code.Name, DataViewRowState.CurrentRows);
			DataRowView[] dataRowViews = dataView.FindRows("123");
			DataRow[] rows = converter.Convert(dataRowViews);
			AssertEquals(2, rows.Length);
		}

		public void TestConvertFiltersRowWithMultipleExclusivePrimaryKeyFilters()
		{
			ZGuid[] guidList = new ZGuid[] { ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid() };

			ZQuery query = new ZQuery(DummyBizoSchema.PK, guidList[0]);
			query.AddToFilter(DummyBizoSchema.PK, new ZGuid[] { guidList[1], guidList[2] });

			RowFactory rowFactory = new RowFactory();
			DataTable table = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			foreach (var guid in guidList)
			{
				DataRow row = rowFactory.New(DummyBizoSchema.Constants.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(row);
				row[DummyBizoSchema.PK.Name] = guid.ToGuid();
				row[DummyBizoSchema.Z0_Code.Name] = "123";
				row.Table.Rows.Add(row);
			}

			DataRowViewConverter converter = new DataRowViewConverter(query, rowFactory);

			DataView dataView = new DataView(table, "", DummyBizoSchema.Z0_Code.Name, DataViewRowState.CurrentRows);
			DataRowView[] dataRowViews = dataView.FindRows("123");
			AssertEquals(3, dataRowViews.Length);

			DataRow[] rows = converter.Convert(dataRowViews);
			AssertEquals(0, rows.Length);
		}

		public void TestRowFilterComparerCacheKeyShouldUniqueWithDifferentInstanceOfTable()
		{
			var rowFactory1 = new RowFactory();
			var table1 = rowFactory1.GetTable(DummyBizoSchema.Constants.TableName);
			for (var i = 0; i < 2; i++)
			{
				var rowMatching = rowFactory1.New(DummyBizoSchema.Constants.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
				rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
				rowMatching[DummyBizoSchema.Z0_Number.Name] = i + 1;
				rowMatching.Table.Rows.Add(rowMatching);
			}

			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			var converter = new DataRowViewConverter(filter, rowFactory1);
			var dataView = new DataView(table1, filter.LiteralTextADO, DummyBizoSchema.Z0_Code.Name, DataViewRowState.CurrentRows);
			var dataRowViews = dataView.FindRows("123");
			var rows = converter.Convert(dataRowViews);
			AssertEquals(2, rows.Length);

			var table2 = new RowFactory().GetTable(DummyBizoSchema.Constants.TableName);
			foreach (DataRow row in table1.Rows)
			{
				var row2 = table2.NewRow();
				row2.ItemArray = row.ItemArray;
				table2.Rows.Add(row2);
				row2.AcceptChanges();
			}
			var rowNew = table2.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(rowNew);
			rowNew[DummyBizoSchema.Z0_Code.Name] = "123";
			rowNew[DummyBizoSchema.Z0_Number.Name] = 0;
			table2.Rows.InsertAt(rowNew, 0);

			dataView = new DataView(table2, filter.LiteralTextADO, DummyBizoSchema.Z0_Code.Name, DataViewRowState.CurrentRows);
			dataRowViews = dataView.FindRows("123");
			rows = converter.Convert(dataRowViews);
			AssertEquals(3, rows.Length);
		}
	}
}
