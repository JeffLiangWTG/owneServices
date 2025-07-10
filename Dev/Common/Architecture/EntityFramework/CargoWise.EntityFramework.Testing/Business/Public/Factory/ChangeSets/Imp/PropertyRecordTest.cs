using System;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[UseSnapshotProtection]
	sealed class PropertyRecordTest : TestCase
	{
		BusinessObjectFactory factory;

		DummyBusinessObject sessionInstance;
		DummyBusinessObject databaseInstance;

		PropertyRecord record;

		ZPropertyInfo info;
		string column;

		protected override void SetUp()
		{
			factory = new BusinessObjectFactory();

			sessionInstance = factory.New<DummyBusinessObject>();
			factory.Save();

			databaseInstance = new BusinessObjectFactory().Load<DummyBusinessObject>(sessionInstance.PK);

			info = sessionInstance.Z0_DescriptionInfo;
			column = DummyBaseBusinessObject.Schema.Z0_Description;
		}

		PropertyRecord Create()
		{
			return new PropertyRecord(info, column, sessionInstance.Row, databaseInstance.Row, new Lazy<string>(() => "LAST"));
		}

		public void TestEqualValues_GeographyTypes()
		{
			var zValue = ZGeography.CreatePoint(1, 1);
			var sqlValue = SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT (1 1)")), 4326);

			CombineAssertions(() =>
			{
				Assert("ZGeography and ZGeography", PropertyRecord.EqualValues(zValue, zValue));
				Assert("SqlGeography and SqlGeography", PropertyRecord.EqualValues(sqlValue, sqlValue));
				Assert("ZGeography and SqlGeography", PropertyRecord.EqualValues(zValue, sqlValue));
				Assert("SqlGeography and ZGeography", PropertyRecord.EqualValues(sqlValue, zValue));
			});
		}

		public void TestEqualValues_Inequalities()
		{
			CombineAssertions(() =>
			{
				Assert("1", !PropertyRecord.EqualValues("", ZString.Empty));
				Assert("2", !PropertyRecord.EqualValues("", null));
				Assert("3", !PropertyRecord.EqualValues(ZString.Empty, null));
				Assert("4", !PropertyRecord.EqualValues("", DBNull.Value));
				Assert("5", !PropertyRecord.EqualValues(ZString.Empty, DBNull.Value));
				Assert("6", !PropertyRecord.EqualValues(DBNull.Value, null));

				Assert("7", !PropertyRecord.EqualValues(ZDateTime.Empty, null));
				Assert("8", !PropertyRecord.EqualValues(ZDateTime.Invalid, null));
				Assert("9", !PropertyRecord.EqualValues(ZDateTime.Empty, DBNull.Value));
				Assert("10", !PropertyRecord.EqualValues(ZDateTime.Invalid, DBNull.Value));
				Assert("11", !PropertyRecord.EqualValues(ZDateTime.Empty, ZDateTime.Invalid));

				Assert("12", !PropertyRecord.EqualValues(1, new ZInt(1)));
				Assert("13", PropertyRecord.EqualValues(new ZDateTime(2000, 1, 1), new DateTime(2000, 1, 1)));
			});
		}

		public void TestDisplayName()
		{
			record = Create();
			Assert(record.DisplayName == info.HumanReadableName);
		}

		public void TestHasNotChangedInDatabaseIfDatabaseRecordModifiedInMemory()
		{
			record = Create();
			Assert(!record.HasChangedInDatabase);

			databaseInstance.Z0_Description = "CHANGED";

			record = Create();
			Assert(!record.HasChangedInDatabase);
		}

		public void TestHasChangedInDatabase()
		{
			record = Create();
			Assert(!record.HasChangedInDatabase);

			databaseInstance.Z0_Description = "CHANGED";
			((IBusinessObjectInternals)databaseInstance).Row.AcceptChanges();

			record = Create();
			Assert(record.HasChangedInDatabase);
		}

		public void TestHasChangedInSession()
		{
			record = Create();
			Assert(!record.HasChangedInSession);

			sessionInstance.Z0_Description = "CHANGED";

			record = Create();
			Assert(record.HasChangedInSession);
		}

		public void TestIsConsistentWithDatabase()
		{
			record = Create();
			Assert(record.IsConsistentWithDatabase);

			sessionInstance.Z0_Description = "CHANGED";

			record = Create();
			Assert(!record.IsConsistentWithDatabase);
		}

		public void TestIsMergeAllowed()
		{
			record = Create();
			Assert(record.IsMergeAllowed);

			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, column, ConcurrencyPolicy.Strict);

			record = Create();
			Assert(!record.IsMergeAllowed);
		}

		public void TestLastModified()
		{
			AssertEquals("LAST", Create().LastModified);
		}

		public void TestGetWarning()
		{
			AssertEquals("Another user (lastModified) has changed this field.\r\nYours: 'value', Theirs: 'databaseValue'", PropertyRecord.GetWarning("lastModified", "value", "databaseValue"));
		}

		[ExpectException(typeof(Exception))]
		public void TestMergeThrowsAnExceptionIfNotAllowed()
		{
			sessionInstance.Row[column] = "CHANGED";
			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, column, ConcurrencyPolicy.Strict);
			record = Create();
			record.Merge();
		}

		public void TestMergeAppliesCurrentSessionValueIfThereAreNoChangesInDatabase()
		{
			sessionInstance.Z0_Description = "CURRENT";

			record = Create();
			sessionInstance.Row[column] = "";

			record.Merge();
			Assert(sessionInstance.Z0_Description == "CURRENT");
		}

		public void TestMergeAppliesDatabaseValueIfThereAreChangesInDatabase()
		{
			sessionInstance.Z0_Description = "CURRENT";

			databaseInstance.Z0_Description = "CHANGED";
			((IBusinessObjectInternals)databaseInstance).Row.AcceptChanges();

			record = Create();
			record.Merge();

			Assert(sessionInstance.Z0_Description == "CHANGED");
		}

		public void TestMergePutWarningOnMergedProperties()
		{
			sessionInstance.Z0_Description = "CURRENT";

			databaseInstance.Z0_Description = "CHANGED";
			((IBusinessObjectInternals)databaseInstance).Row.AcceptChanges();

			record = Create();
			record.Merge();

			Assert(sessionInstance.Z0_Description == "CHANGED");
			AssertEquals(1, info.GetWarnings().GetUniqueMessageList().Length);

			string warning = string.Format("Another user ({0}) has changed this field.\r\nYours: '{1}', Theirs: '{2}'",
											"LAST", record.CurrentValue, record.DatabaseValue);

			Assert(info.GetWarnings().GetFirstMessage() == warning);
		}

		public void TestMergeDoNotPutWarningOnPropertiesWithSilentMergePolicy()
		{
			ConcurrencyInfo.SetConcurrencyPolicy(sessionInstance.Row, column, ConcurrencyPolicy.Ignore);

			sessionInstance.Z0_Description = "CURRENT";

			databaseInstance.Z0_Description = "CHANGED";
			((IBusinessObjectInternals)databaseInstance).Row.AcceptChanges();

			record = Create();
			record.Merge();

			Assert(sessionInstance.Z0_Description == "CHANGED");
			Assert(info.GetWarnings().Count() == 0);
		}

		public void TestCurrentValueOfDeleted()
		{
			sessionInstance.Z0_Description = "ABCDE";
			record = Create();
			AssertEquals("ABCDE", record.CurrentValue);

			sessionInstance.Delete();
			record = Create();
			AssertEquals(DBNull.Value, record.CurrentValue);
		}
	}
}
