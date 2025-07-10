using System;
using System.Collections.Generic;
using System.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZRowSaveOrderSorterTest : TestCase
	{
		public void TestSort_DeleteUpdate()
		{
			var dummy1 = CreateSimpleDummyRow("AAA");
			var dummy2 = CreateSimpleDummyRow("BBB");
			dataSet.AcceptChanges();

			dummy1.Delete();
			dummy2[DummyBizoSchema.Constants.Z0_Code] = "CCC";

			AssertSortRandomizedRows(new List<DataRow> { dummy1, dummy2 }, 5);
		}

		public void TestSort_UpdateInsert()
		{
			var dummy1 = CreateSimpleDummyRow("BBB");
			dataSet.AcceptChanges();

			var dummy2 = CreateSimpleDummyRow("AAA");
			dummy1[DummyBizoSchema.Constants.Z0_Code] = "CCC";

			AssertSortRandomizedRows(new List<DataRow> { dummy1, dummy2 }, 5);
		}

		public void TestSort_DeleteInsert()
		{
			var dummy1 = CreateSimpleDummyRow("BBB");
			dataSet.AcceptChanges();

			var dummy2 = CreateSimpleDummyRow("AAA");
			dummy1.Delete();

			AssertSortRandomizedRows(new List<DataRow> { dummy1, dummy2 }, 5);
		}

		public void TestSort_DeleteUpdateInsert()
		{
			var dummy1 = CreateSimpleDummyRow("AAA");
			var dummy2 = CreateSimpleDummyRow("BBB");
			dataSet.AcceptChanges();

			dummy1.Delete();
			dummy2[DummyBizoSchema.Constants.Z0_Code] = "CCC";
			var dummy3 = CreateSimpleDummyRow("DDD");

			AssertSortRandomizedRows(new List<DataRow> { dummy1, dummy2, dummy3 }, 10);
		}

		public void TestSort_RelatedRows()
		{
			var dummy1 = CreateSimpleDummyRow("AAA");
			var dummy2 = CreateSimpleDummyRow("BBB");
			var dependent1 = CreateSimpleDependentRow("111", (Guid)dummy1[DummyBizoSchema.Constants.PK]);
			var dependent2 = CreateSimpleDependentRow("222", (Guid)dummy2[DummyBizoSchema.Constants.PK]);
			dataSet.AcceptChanges();

			var dummy3 = CreateSimpleDummyRow("AAA");
			dummy1.Delete();
			dependent1[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummy2[DummyBizoSchema.Constants.PK];
			dependent2[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummy3[DummyBizoSchema.Constants.PK];

			AssertSortRandomizedRows(new List<DataRow> { dependent1, dummy1, dummy3, dependent2 }, 20);
		}

		#region Implementation

		void AssertSortRandomizedRows(IList<DataRow> rowsInExpectedOrder, int times)
		{
			var randomizedRows = new List<DataRow>(rowsInExpectedOrder);
			var randomizer = new Random(randomizedRows.Count);

			for (int i = 0; i < times; i++)
			{
				for (int j = 0; j < randomizedRows.Count * 2; j++)
				{
					int x = randomizer.Next(randomizedRows.Count - 1);
					int y;
					do
					{
						y = randomizer.Next(randomizedRows.Count - 1);
					} while (x != y);

					var t = randomizedRows[x];
					randomizedRows[x] = randomizedRows[y];
					randomizedRows[y] = t;
				}

				AssertSortRows(rowsInExpectedOrder, randomizedRows);
			}
		}

		void AssertSortRows(IList<DataRow> expectedOrder, IList<DataRow> randomizedRows)
		{
			AssertEquals("Precondition", expectedOrder.Count, randomizedRows.Count);

			new ZRowSaveOrderSorter().Sort(randomizedRows);

			AssertEquals("Sort should leave all rows in the list", expectedOrder.Count, randomizedRows.Count);

			for (int i = 0; i < expectedOrder.Count; i++)
			{
				AssertSame(expectedOrder[i], randomizedRows[i]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareDataSet();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void PrepareDataSet()
		{
			dataSet = new DataSet();

			dummies = dataSet.Tables.Add(DummyBizoSchema.Constants.TableName);
			dummies.Columns.Add(DummyBizoSchema.Constants.PK, typeof(Guid));
			dummies.Columns.Add(DummyBizoSchema.Constants.Z0_Code, typeof(string));
			dummies.PrimaryKey = new[] { dummies.Columns[DummyBizoSchema.Constants.PK] };

			dependents = dataSet.Tables.Add(DummyDependentBizoSchema.Constants.TableName);
			dependents.Columns.Add(DummyDependentBizoSchema.Constants.PK, typeof(Guid));
			dependents.Columns.Add(DummyDependentBizoSchema.Constants.ZD1_Code, typeof(string));
			dependents.Columns.Add(DummyDependentBizoSchema.Constants.ZD1_Z0, typeof(Guid));
			dependents.PrimaryKey = new[] { dependents.Columns[DummyDependentBizoSchema.Constants.PK] };
		}

		DataRow CreateSimpleDummyRow(string code)
		{
			var dummy = dummies.NewRow();
			dummy[DummyBizoSchema.Constants.PK] = Guid.NewGuid();
			dummy[DummyBizoSchema.Constants.Z0_Code] = code;
			dummies.Rows.Add(dummy);
			return dummy;
		}

		DataRow CreateSimpleDependentRow(string code, Guid dummyFK)
		{
			var dependent = dependents.NewRow();
			dependent[DummyDependentBizoSchema.Constants.PK] = Guid.NewGuid();
			dependent[DummyDependentBizoSchema.Constants.ZD1_Code] = code;
			dependent[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummyFK;
			dependents.Rows.Add(dependent);
			return dependent;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataSet dataSet;
		DataTable dummies;
		DataTable dependents;

		#endregion
	}
}
