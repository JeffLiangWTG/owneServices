using System;
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RowWrapperTest : TestCaseWithDummy
	{
		public void TestCancelChanges()
		{
			var dataTable = Dummy.Row.Table;
			DataRow row = dataTable.NewRow();
			row["Z0_PK"] = Guid.NewGuid();
			dataTable.Rows.Add(row);
			row.AcceptChanges(); // pretend Row is in the DB

			DataRow notInDBRow = row.Table.NewRow();
			row.Table.Rows.Add(notInDBRow);

			ConcreteRowWrapper testRowWrapper = new ConcreteRowWrapper(row);
			ConcreteRowWrapper testRowWrapper2 = new ConcreteRowWrapper(notInDBRow);
			row.SetModified();

			Assert(row.RowState == DataRowState.Modified);
			Assert(notInDBRow.RowState == DataRowState.Added);

			testRowWrapper.CancelChanges();
			testRowWrapper2.CancelChanges();

			Assert(row.RowState == DataRowState.Unchanged);
			Assert(notInDBRow.RowState == DataRowState.Detached);
		}

		public void TestDelete()
		{
			var dataTable = Dummy.Row.Table;
			DataRow row = dataTable.NewRow();
			row["Z0_PK"] = Guid.NewGuid();
			dataTable.Rows.Add(row);
			row.AcceptChanges(); // pretend Row is in the DB

			DataRow notInDBRow = row.Table.NewRow();
			row.Table.Rows.Add(notInDBRow);

			ConcreteRowWrapper testRowWrapper = new ConcreteRowWrapper(row);
			ConcreteRowWrapper testRowWrapper2 = new ConcreteRowWrapper(notInDBRow);

			row.AcceptChanges();
			Assert(row.RowState == DataRowState.Unchanged);
			Assert(notInDBRow.RowState == DataRowState.Added);

			testRowWrapper.Delete();
			testRowWrapper2.Delete();

			Assert(row.RowState == DataRowState.Deleted);
			Assert(notInDBRow.RowState == DataRowState.Detached);
		}

		public void TestIsRowDeletedOrDetachedOrNull()
		{
			var table = Dummy.Row.Table;
			var row = table.NewRow();
			row["Z0_PK"] = Guid.NewGuid();
			table.Rows.Add(row);
			row.AcceptChanges();

			var testRowWrapper = new ConcreteRowWrapper(row);
			Assert(!testRowWrapper.IsRowDeletedOrDetachedOrNull);
			testRowWrapper.Row = null;
			Assert(testRowWrapper.IsRowDeletedOrDetachedOrNull);

			testRowWrapper = new ConcreteRowWrapper(row);
			Assert(row.RowState == DataRowState.Unchanged);
			Assert(!testRowWrapper.IsRowDeletedOrDetachedOrNull);
			testRowWrapper.Delete();
			Assert(row.RowState == DataRowState.Deleted);
			Assert(testRowWrapper.IsRowDeletedOrDetachedOrNull);

			row = row.Table.NewRow();
			row.Table.Rows.Add(row);
			testRowWrapper = new ConcreteRowWrapper(row);
			Assert(row.RowState == DataRowState.Added);
			Assert(!testRowWrapper.IsRowDeletedOrDetachedOrNull);
			testRowWrapper.Delete();
			Assert(row.RowState == DataRowState.Detached);
			Assert(testRowWrapper.IsRowDeletedOrDetachedOrNull);
		}

		public void TestTableName()
		{
			AssertEquals(DummyBusinessObject.Schema.TableName,
				new BusinessObjectFactory().New(typeof(DummyBusinessObject)).TableName);
		}

		public void TestCancelChangesClearsLightValidationIsValidSet()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			((ILightValidationInternals)dummy).IsValid = true;
			Factory.Save();

			Assert(dummy.LightValidationIsValid);

			dummy.MarkAsNeedingValidation();
			Assert(!dummy.LightValidationIsValid);

			dummy.CancelChanges();
			Assert(dummy.LightValidationIsValid);
		}

		class ConcreteRowWrapper : DummyBusinessObject
		{
			public ConcreteRowWrapper(DataRow row)
				: base(new BusinessObjectFactory(), row)
			{
			}
		}
	}
}
