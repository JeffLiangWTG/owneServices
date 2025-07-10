using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DataRowSorterTest : TestCase
	{
		public void TestIsSortableOnString()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRowSorter sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Code);
			Assert(sorter.IsSortable);
		}

		public void TestIsSortableOnEmptyString()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRowSorter sorter = new DataRowSorter(table, "");
			Assert(!sorter.IsSortable);
		}

		public void TestSort()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row1 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			table.Rows.Add(row1);
			row1[DummyBizoSchema.Constants.Z0_Code] = "3";

			DataRow row2 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			table.Rows.Add(row2);
			row2[DummyBizoSchema.Constants.Z0_Code] = "2";

			DataRow row3 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row3);
			table.Rows.Add(row3);
			row3[DummyBizoSchema.Constants.Z0_Code] = "1";

			DataRowSorter sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Code);
			List<DataRow> rows = new List<DataRow>();
			foreach (DataRow row in table.Rows)
			{
				rows.Add(row);
			}
			sorter.Sort(rows);

			AssertEquals(row3, rows[0]);
			AssertEquals(row2, rows[1]);
			AssertEquals(row1, rows[2]);
		}

		public void TestRowOrderingForNullFirst()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row1 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			table.Rows.Add(row1);

			DataRow row2 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			table.Rows.Add(row2);

			row1[DummyBizoSchema.Z0_Date.Name] = DBNull.Value;
			row2[DummyBizoSchema.Z0_Date.Name] = ZDateTime.Now.ToDateTime();

			List<DataRow> rows = new List<DataRow>();
			foreach (DataRow row in table.Rows)
			{
				rows.Add(row);
			}

			DataRowSorter sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date);
			sorter.Sort(rows);
			AssertEquals(row1, rows[0]);
			AssertEquals(row2, rows[1]);

			sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date + " desc");
			sorter.Sort(rows);
			AssertEquals(row2, rows[0]);
			AssertEquals(row1, rows[1]);
		}

		public void TestRowOrderingForNullSecond()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row1 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			table.Rows.Add(row1);

			DataRow row2 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			table.Rows.Add(row2);

			row1[DummyBizoSchema.Z0_Date.Name] = ZDateTime.Now.ToDateTime();
			row2[DummyBizoSchema.Z0_Date.Name] = DBNull.Value;

			List<DataRow> rows = new List<DataRow>();
			foreach (DataRow row in table.Rows)
			{
				rows.Add(row);
			}

			DataRowSorter sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date);
			sorter.Sort(rows);
			AssertEquals(row2, rows[0]);
			AssertEquals(row1, rows[1]);

			sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date + " desc");
			sorter.Sort(rows);
			AssertEquals(row1, rows[0]);
			AssertEquals(row2, rows[1]);
		}

		[ExpectNoExceptions]
		public void TestRowOrderingForNullBoth()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row1 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			table.Rows.Add(row1);

			DataRow row2 = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			table.Rows.Add(row2);

			row1[DummyBizoSchema.Z0_Date.Name] = DBNull.Value;
			row2[DummyBizoSchema.Z0_Date.Name] = DBNull.Value;

			List<DataRow> rows = new List<DataRow>();
			foreach (DataRow row in table.Rows)
			{
				rows.Add(row);
			}

			DataRowSorter sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date);
			sorter.Sort(rows);

			sorter = new DataRowSorter(table, DummyBizoSchema.Constants.Z0_Date + " desc");
			sorter.Sort(rows);
		}
	}
}
