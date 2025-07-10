using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZDataTableTest : TestCase
	{
		public void TestGetDataColumnFromSchemaColumn()
		{
			ZDataTable table = new ZDataTable();
			foreach (SchemaColumn column in GlbCompanySchema.All)
			{
				table.Columns.Add(new DataColumn(column.Name));
			}
			AssertEquals("Initially associating a cache key with a column", "GC_Address1", table.GetDataColumnFromSchemaColumn(GlbCompanySchema.GC_Address1).ColumnName);
			AssertEquals("Initially associating a cache key with a column", "GC_Address2", table.GetDataColumnFromSchemaColumn(GlbCompanySchema.GC_Address2).ColumnName);
			AssertEquals("Using the cache key to return the column", "GC_Address1", table.GetDataColumnFromSchemaColumn(GlbCompanySchema.GC_Address1).ColumnName);
			AssertEquals("Using the cache key to return the column", "GC_Address2", table.GetDataColumnFromSchemaColumn(GlbCompanySchema.GC_Address2).ColumnName);
		}

		[ExpectException(typeof(ConstraintException))]
		public void TestUniqueIndex()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject bizO = factory.New<DummyBusinessObject>();
			DataRow row = bizO.Table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Constants.PK] = bizO.PK.ToGuid();
			bizO.Table.Rows.Add(row);
		}

		public void TestGetRowIncludingDeleted()
		{
			ZDataTable table = new ZDataTable();
			table.Columns.Add("PK", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			Guid pK1 = Guid.NewGuid();
			Guid pK2 = Guid.NewGuid();
			Guid pK3 = Guid.NewGuid();

			table.Rows.Add(new object[] { pK1 });
			table.Rows.Add(new object[] { pK2 });
			table.AcceptChanges();
			table.Rows.Add(new object[] { pK3 });
			table.Rows[1].Delete();

			DataRow row1 = table.GetRowIncludingDeleted(pK1);
			AssertNotNull("Should find an unchanged row", row1);
			DataRow row2 = table.GetRowIncludingDeleted(pK2);
			AssertNotNull("Should find a deleted row", row2);
			DataRow row3 = table.GetRowIncludingDeleted(pK3);
			AssertNotNull("Show find an added row", row3);
			DataRow bodgeRow = table.GetRowIncludingDeleted(Guid.NewGuid());
			Assert("Should not find a non-existant PK", bodgeRow == null);
		}

		public void TestBogusValue()
		{
			var dt = new ZDataTable("HelloTable");
			var dc = new DataColumn("HelloColumn", typeof(DateTime));
			dt.Columns.Add(dc);

			dt.Rows.Add();
			var row = dt.Rows[0];

			// valid
			row[dc] = new DateTime(2000, 1, 1);
			AssertEquals(new DateTime(2000, 1, 1), row[dc]);

			row[dc] = new DateTime(1899, 1, 1);
			AssertEquals(new DateTime(1899, 1, 1), row[dc]);

			Assert("No error notifications on bogus dates", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestHasDeletedRowWithPK_ReturnsTrueForDeletedRow()
		{
			// Arrange
			var guid = new Guid();
			var dt = new ZDateTableExposed();
			var dc = new DataColumn("PK Column", typeof(Guid));
			dt.Columns.Add(dc);
			dt.Rows.Add();
			var row = dt.Rows[0];
			row[0] = guid;
			dt.PrimaryKey = new[] { dc };

			// Act
			dt.OnRowDeletingExposed(new DataRowChangeEventArgs(row, DataRowAction.Delete));
			dt.OnRowDeletedExposed(new DataRowChangeEventArgs(row, DataRowAction.Delete));

			// Assert
			AssertEquals("Has deleted row", true, dt.HasDeletedRowWithPK(guid));
		}

		public void TestHasDeletedRowWithPK_ReturnsFalseForExistingRow()
		{
			// Arrange
			var guid = new Guid();
			var dt = new ZDateTableExposed();
			var dc = new DataColumn("PK Column", typeof(Guid));
			dt.Columns.Add(dc);
			dt.Rows.Add();
			var row = dt.Rows[0];
			row[0] = guid;
			dt.PrimaryKey = new[] { dc };

			// Assert
			AssertEquals("Does not have deleted row", false, dt.HasDeletedRowWithPK(guid));
		}
	}

	class ZDateTableExposed : ZDataTable
	{
		public void OnRowDeletingExposed(DataRowChangeEventArgs e)
		{
			base.OnRowDeleting(e);
		}
		public void OnRowDeletedExposed(DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
		}
	}
}
