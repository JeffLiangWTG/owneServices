using System;
using System.Collections.Specialized;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[UseSnapshotProtection]
	public abstract class NumberFountainUniqueIndexFailureHandlingTest : TestCase
	{
		public virtual void TestNumberFountainFix()
		{
			var testFountain = NumberFountainToTest;

			long initialValue = testFountain.PeekPreliminary(Factory);
			string expectedColumnThatUsesNumberFountainValue1 = testFountain.PeekPreliminaryFormatted(Factory);

			BusinessObject testBizO1 = GetNewTestBizO();

			Factory.Save();

			AssertEquals("Value on ColumnThatUsesNumberFountain (BizO 1)", expectedColumnThatUsesNumberFountainValue1, testBizO1[ColumnThatUsesNumberFountain.Name].ToString());
			AssertEquals("Fountain value after saving 1st BizO", initialValue + 1, testFountain.PeekPreliminary(Factory));

			// Insert a record on the BizOTypeToTest table with the same value in the ColumnThatUsesNumberFountain as the next value from the Fountain
			string nextFountainValue = testFountain.PeekPreliminaryFormatted(Factory);
			string columnList = ColumnThatUsesNumberFountain.Name;
			string valueList = String.Format("'{0}'", nextFountainValue);

			foreach (string key in AdditionalInsertValues.Keys)
			{
				columnList += ", " + key;
				valueList += ", " + AdditionalInsertValues[key];
			}

			var auditColumns = new[]
			{
				new { Name = $"{ColumnThatUsesNumberFountain.ColumnPrefix}_SystemCreateTimeUtc", Value = $"'{DateTime.UtcNow.ToString("s")}'" },
				new { Name = $"{ColumnThatUsesNumberFountain.ColumnPrefix}_SystemCreateUser", Value = $"'A'" },
				new { Name = $"{ColumnThatUsesNumberFountain.ColumnPrefix}_SystemLastEditTimeUtc", Value = $"'{DateTime.UtcNow.ToString("s")}'" },
				new { Name = $"{ColumnThatUsesNumberFountain.ColumnPrefix}_SystemLastEditUser", Value = $"'A'" },
			};

			foreach (var column in auditColumns)
			{
				if (ColumnThatUsesNumberFountain.TableSchema.GetSchemaColumn(column.Name) != null && AdditionalInsertValues.Get(column.Name) == null)
				{
					columnList += ", " + column.Name;
					valueList += ", " + column.Value;
				}
			}

			string sqlText = String.Format("INSERT {0} ({3}, {1}) VALUES ({4}, {2})", ColumnThatUsesNumberFountain.TableName, columnList, valueList, ColumnThatUsesNumberFountain.TableSchema.PK.Name, "NEWID()");
			using (Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteNonQuery(sqlText); // Internal Number Fountain manipulation - UnitTest
				Db.Connection.CommitTransaction();
			}

			AssertEquals("Fountain value should NOT have changed", initialValue + 1, testFountain.PeekPreliminary(Factory));

			BusinessObject testBizO2 = GetNewTestBizO();

			try
			{
				Factory.Save();
				Fail("First save attempt should have failed");
			}
			catch (ZSaveException e)
			{
				DeleteAddedRecords(Factory);
				Assert("No notification before handler", UnitTestUserNotification.Instance.LastMessage.WasNone);

				ZExceptionReporting.HandleSaveException(e);

				Assert("Notification shown after handler", !UnitTestUserNotification.Instance.LastMessage.WasNone);

				AssertStartsWith("Notification was shown",
				@"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.",
				UnitTestUserNotification.Instance.LastMessage.Text);
			}

			// HandleSaveException should have fixed the fountain
			AssertEquals("Value after fixing fountain", initialValue + 2, testFountain.PeekPreliminary(Factory));
			string expectedColumnThatUsesNumberFountainValue2 = testFountain.PeekPreliminaryFormatted(Factory);

			Factory.Save();

			AssertEquals("Value on ColumnThatUsesNumberFountain (BizO 1)", expectedColumnThatUsesNumberFountainValue2, testBizO2[ColumnThatUsesNumberFountain.Name].ToString());
			AssertEquals("Fountain value after saving 2nd BizO", initialValue + 3, testFountain.PeekPreliminary(Factory));
		}

		#region Implementation

		protected abstract SchemaColumn ColumnThatUsesNumberFountain { get; }
		protected abstract INumberFountainProxy NumberFountainToTest { get; }
		protected abstract Type BizOTypeToTest { get; }

		protected virtual void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
		}

		protected virtual NameValueCollection AdditionalInsertValues => new NameValueCollection();

		protected BusinessObjectFactory Factory { get; } = new BusinessObjectFactory();

		BusinessObject GetNewTestBizO()
		{
			var testBizO = Factory.New(BizOTypeToTest);
			SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			return testBizO;
		}

		public static void DeleteAddedRecords(BusinessObjectFactory factory)
		{
			foreach (DataTable table in ((INeedDataSet)factory).Data.Tables)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row.RowState == DataRowState.Added)
					{
						DbCommand command = Db.Connection.Command("delete from " + table.TableName + " where " + table.PrimaryKey[0].ColumnName + " = '" + row[table.PrimaryKey[0]] + "'");
						command.ExecuteNonQuery();
					}
				}
			}
		}

		#endregion // Implementation
	}
}
