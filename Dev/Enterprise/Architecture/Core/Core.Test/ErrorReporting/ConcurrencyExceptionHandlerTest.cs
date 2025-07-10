using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ConcurrencyExceptionHandlerTest : TransactionedTestCase
	{
		public void TestConcurrencyForZGeography()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			dummy.Z0_Description = "New Description";
			dummy.Z0_Geography = new ZGeography("POINT (136.89 35.161)");

			var hookOnce = true;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(onSavingFactory =>
			{
				if (hookOnce)
				{
					hookOnce = false;
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var dummyReload = newFactory.Load<DummyBusinessObject>(dummy.PK);
					dummyReload.Z0_Description = "Changed Description";
					newFactory.Save();
				}
			});

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory.Save());
			var handler = new ConcurrencyExceptionHandler(exception);
			_ = handler.Info;
			AssertEquals("Should be only one column changed in DB.", 1, handler.ColumnsDBChanged.Count);
			Assert("Z0_Description", handler.ColumnsDBChanged.ContainsKey("Z0_Description"));
		}

		public void TestStmALogAddsExtraColumn()
		{
			var datatable = DataUtils.GetDataTableFromQuery(Db.Connection, "select top 0 * from dbo.StmALog");
			datatable.TableName = "StmALog";

			var parentRow = datatable.NewRow();
			datatable.Rows.Add(parentRow);

			var row = datatable.NewRow();
			row.SetParentRow(parentRow);
			datatable.Rows.Add(row);

			var ex = new DBConcurrencyException();
			row[StmALogSchema.Constants.SL_Parent] = Guid.Empty;
			ex.Row = row;
			var handler = new ConcurrencyExceptionHandler(ex);
			// this triggers the method.
			DataRow randomRow = handler.RowOnDatabase;
			var lastQuery = ObjectFactory.Get<IUserEventTracker>().LastSqlQuery;
			Assert("Query must contain SL_Parent to avoid table scan", lastQuery.Contains("SL_Parent"));
		}
	}
}
