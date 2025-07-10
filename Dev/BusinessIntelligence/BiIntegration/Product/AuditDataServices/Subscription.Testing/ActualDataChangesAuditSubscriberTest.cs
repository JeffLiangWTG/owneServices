namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Data;
	using System.Linq;
	using CargoWise.Bi.ConfigLoader;
	using Enterprise.AuditDataServices.Subscription.Common;
	using NUnit.Framework;
	using NUnit.Framework.TestHelper;

	[TestsSubclassesOf(typeof(ActualDataChangesAuditSubscriber))]
	public abstract class ActualDataChangesAuditSubscriberTest : AuditSubscriberTest
	{
		public abstract void TestCustomFilter();

		protected abstract DataTable GetTestDataTable();

		public void TestCustomFilterHandlesDbNullColumnValue()
		{
			if (TestDataChangeSubscriber.CustomFilter != null)
			{
				var table = GetTestDataTable();
				AssertNotNull("Precondition : GetTestDataTable", table);

				var row = table.NewRow();
				table.Rows.Add(row);
				table.AcceptChanges();
				AssertNoExceptionThrown(() =>
				{
					RunCustomFilter(row, TestDataChangeSubscriber);
					table.AcceptChanges();
				});
			}
			Assert(true);
		}

		public void TestTableIsAudited()
		{
			AssertNotNull("Table", TestDataChangeSubscriber.Table);
			AssertEquals("Is subscriber table [" + TestDataChangeSubscriber.Table.SqlSchemaName + "].[" + TestDataChangeSubscriber.Table.TableName + "] audited?",
				true, BiAutomationConfigLoader.AuditTableExists(TestDataChangeSubscriber.Table.SqlSchemaName, TestDataChangeSubscriber.Table.TableName));
		}

		public void TestSpecificColumnsHasNoNullElements()
		{
			AssertEquals("SpecificColumns has null elements.", false, TestDataChangeSubscriber.SpecificColumns != null && TestDataChangeSubscriber.SpecificColumns.Any(c => c == null));
		}

		public void TestSpecificColumnsBelongToSubscribedTable()
		{
			if (TestDataChangeSubscriber.SpecificColumns == null)
			{
				Assert("No specific columns - nothing to test", true);
			}
			else
			{
				var invaldColumns =
					from c in TestDataChangeSubscriber.SpecificColumns
					where
						c != null
						&& !c.TableSchema.GetType().Equals(TestDataChangeSubscriber.Table.GetType())
					select c.Name;

				Assert(
					"The following columns do not belong to table [" + TestDataChangeSubscriber.Table.SqlSchemaName + "].[" + TestDataChangeSubscriber.Table.TableName + "]:\r\n\r\n" + string.Join("\r\n", invaldColumns) + "\r\n",
					!invaldColumns.Any());
			}
		}

		public void TestSpecificColumnsAreAudited()
		{
			if (TestDataChangeSubscriber.SpecificColumns == null)
			{
				Assert("No specific columns - nothing to test", true);
			}
			else
			{
				var nonAuditedColumns =
					from c in TestDataChangeSubscriber.SpecificColumns
					where
						c != null
						&& !BiAutomationConfigLoader.ColumnInAuditTableExists(c.TableSchema.SqlSchemaName, c.TableName, c.Name)
					select c.Name;

				Assert(
					"The following columns are not Audited:\r\n\r\n" + string.Join("\r\n", nonAuditedColumns) + "\r\n",
					!nonAuditedColumns.Any());
			}
		}

		public void TestAtLeastOneOperationIsSelected()
		{
			AssertEquals(
				"Is at least one of insert/update/delete operations selected?",
				true,
				TestDataChangeSubscriber.NotifyInsert
				|| TestDataChangeSubscriber.NotifyUpdate
				|| TestDataChangeSubscriber.NotifyDelete);
		}

		public void TestNoDuplicatedColumns()
		{
			if (TestDataChangeSubscriber.SpecificColumns == null)
			{
				Assert("No specific columns - nothing to test", true);
			}
			else
			{
				var duplicateColumns =
					from c in TestDataChangeSubscriber.SpecificColumns
					where c != null
					group c by c.Name into grp
					where grp.Count() > 1
					select grp.Key;

				Assert(
					"The following columns are duplicated:\r\n\r\n" + string.Join("\r\n", duplicateColumns) + "\r\n",
					!duplicateColumns.Any());
			}
		}

		#region TestSubscriber

		protected ActualDataChangesAuditSubscriber TestDataChangeSubscriber => testDataChangeSubscriber ?? (testDataChangeSubscriber = NewDataChangeSubscriber());
		ActualDataChangesAuditSubscriber testDataChangeSubscriber;

		protected override sealed IAuditSubscriber TestSubscriber => TestDataChangeSubscriber;

		protected ActualDataChangesAuditSubscriber NewDataChangeSubscriber() =>
			(ActualDataChangesAuditSubscriber)Activator.CreateInstance
			(
				TestedTypeHelper.GetTestedType(GetType())
			);

		protected void RunCustomFilter(DataRow row, ActualDataChangesAuditSubscriber subscriber)
		{
			if (subscriber.CustomFilter != null)
			{
				subscriber.CustomFilter(row);
			}
		}
		#endregion
	}
}
