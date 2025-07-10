using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZDataUtilsTest : TestCaseWithDummy
	{
		public void DoesUpdateRowHavePersistentChangesFromOriginal()
		{
			var resolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			Dummy.Row.AcceptChanges();
			Dummy.Z0_Description = "comrade";
			Assert("No changes", !ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, Dummy.Row));

			Dummy.Z0_Description = "noodle woodle";
			Assert("Valid changes", ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, Dummy.Row));

			Dummy.Z0_Description = "comrade";
			Assert("No real changes", !ZDataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, Dummy.Row));
		}

		public void TestGetPK()
		{
			DataTable table = new DataTable();
			table.Columns.Add("PK", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			Guid pK = Guid.NewGuid();
			DataRow row = table.NewRow();
			row["PK"] = pK;
			AssertEquals("Can get PK", pK, ZDataUtils.GetPK(row));

			table.Rows.Add(row);
			AssertEquals("Can get PK", pK, ZDataUtils.GetPK(row));
			row.AcceptChanges();

			row.Delete();
			AssertEquals("Can get PK", pK, ZDataUtils.GetPK(row));
		}

		[Obsolete]
		public void TestCountOfAllThatMatchQueryInDB()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "ZZZ");
			AssertEquals("No matches", 0, ZDataUtils.GetCountOfAllThatMatchQueryInDB(typeof(DummyBusinessObject), query));

			Factory.New(typeof(DummyBusinessObject)); // doesn't match
			for (int i = 0; i < 10; i++) // do match
			{
				((DummyBusinessObject)Factory.New(typeof(DummyBusinessObject))).Z0_Code = "ZZZ";
			}

			AssertEquals("No matches", 0, ZDataUtils.GetCountOfAllThatMatchQueryInDB(typeof(DummyBusinessObject), query));

			Factory.Save();
			AssertEquals("Found saved matches", 10, ZDataUtils.GetCountOfAllThatMatchQueryInDB(typeof(DummyBusinessObject), query));
		}

		public void TestShouldRowBeSaved()
		{
			BusinessObject dummy2 = Factory.New(typeof(DummyBusinessObject));

			Assert(ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)Dummy).Row));
			Assert(ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)dummy2).Row));

			ZDataUtils.SetShouldRowBeSaved(((IBusinessObjectInternals)dummy2).Row, false);

			Assert(ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)Dummy).Row));
			Assert(!ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)dummy2).Row));

			ZDataUtils.SetShouldRowBeSaved(((IBusinessObjectInternals)dummy2).Row, true);

			Assert(ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)Dummy).Row));
			Assert(ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)dummy2).Row));
		}

		public void TestTopN()
		{
			DataTable table = new DataTable();
			table.Columns.Add();

			DataRow[] rows = new DataRow[3];
			rows[0] = table.NewRow();
			rows[1] = table.NewRow();
			rows[2] = table.NewRow();

			AssertEquals(0, ZDataUtils.TopN(rows, 0).Length);
			AssertEquals(rows[0], ZDataUtils.TopN(rows, 1)[0]);
			AssertEquals(2, ZDataUtils.TopN(rows, 2).Length);
			AssertEquals(3, ZDataUtils.TopN(rows, 6).Length);
			AssertEquals(rows[2], ZDataUtils.TopN(rows, 3)[2]);
		}

		[Obsolete]
		public void TestFindRowByPKIncludingDeleted()
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

			DataRow row1 = ZDataUtils.FindRowByPKIncludingDeleted(table, pK1);
			AssertNotNull("Should find an unchanged row", row1);
			DataRow row2 = ZDataUtils.FindRowByPKIncludingDeleted(table, pK2);
			AssertNotNull("Should find a deleted row", row2);
			DataRow row3 = ZDataUtils.FindRowByPKIncludingDeleted(table, pK3);
			AssertNotNull("Show find an added row", row3);
			DataRow bodgeRow = ZDataUtils.FindRowByPKIncludingDeleted(table, Guid.NewGuid());
			Assert("Should not find a non-existant PK", bodgeRow == null);
		}

		public void TestGetFKNameFromDependentTableAndPKColumnName_ReturnSameInstanceForSameText()
		{
			var table1 = new DataTable();
			table1.Columns.Add("XY_PK", typeof(Guid));
			table1.PrimaryKey = new[] { table1.Columns[0] };

			var table2 = new DataTable();
			table2.Columns.Add("XY_PK", typeof(Guid));
			table2.PrimaryKey = new[] { table2.Columns[0] };

			var value1 = ZDataUtils.GetFKNameFromDependentTableAndPKColumnName(table1, "YZ_PK");
			var value2 = ZDataUtils.GetFKNameFromDependentTableAndPKColumnName(table2, "YZ_PK");

			AssertSame("Should return same string instance", value1, value2);
		}
	}
}
