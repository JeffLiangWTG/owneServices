using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	using System.Text;
	using Microsoft.SqlServer.Types;
	using NUnit.Framework;

	#region ZSqlCommandBuilderTest

	internal abstract class ZSqlCommandBuilderTest : TransactionedTestCase
	{
		public void TestInvalidDateTimeIsWrangledCorrectly()
		{
			//the only case we need to get right is datetimes with the year of 1
			var builder = GetBuilder(CreateDummyRow());
			var earliestDateString = builder.QuoteStringExposedForTest(SqlFormatInfo.ToSqlDateTimeString(new DateTime(ZDateTime.MinSmallDateTimeValue.Year, 1, 1)));
			Assert(DummyBizoSchema.Z0_Date.IsNullable);
			AssertEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(), DummyBizoSchema.Z0_Date));
			AssertEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(1, 1, 1), DummyBizoSchema.Z0_Date));
			AssertEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(1, 2, 1), DummyBizoSchema.Z0_Date));
			AssertEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(1), DummyBizoSchema.Z0_Date));
			AssertEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(DateTime.MinValue, DummyBizoSchema.Z0_Date));
			AssertNotEquals("NULL", builder.GetQuoteEscapeTruncateAndCompressExposedForTest(DateTime.Now, DummyBizoSchema.Z0_Date));

			var nonnullablecolumn = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "x", 19, SqlDbType.SmallDateTime, DBNull.Value, false);
			AssertEquals(earliestDateString, builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(), nonnullablecolumn));

			AssertEquals(earliestDateString, builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(1, 1, 1), nonnullablecolumn));
			AssertExceptionThrown("Out of range small date time should throw ArgumentOutOfRangeException", typeof(ArgumentOutOfRangeException), () => { builder.GetQuoteEscapeTruncateAndCompressExposedForTest(new DateTime(1), nonnullablecolumn); });

			AssertEquals(earliestDateString, builder.GetQuoteEscapeTruncateAndCompressExposedForTest(DateTime.MinValue, nonnullablecolumn));
			AssertNotEquals(earliestDateString, builder.GetQuoteEscapeTruncateAndCompressExposedForTest(DateTime.Now, nonnullablecolumn));
			Assert("Should NOT have sent a silent error report on set of a SmallDateTime", (ErrorReporter.TotalErrorCount == 0));
		}

		public void TestSetupGetsCalledFromLazyProperties()
		{
			var commandText = new StringBuilder();
			var builder = GetBuilder(CreateDummyRow());
			builder.AppendCommandTextAndBlobSaver(commandText);
			AssertNotNull(commandText.ToString());

			commandText = new StringBuilder();
			var builder2 = GetBuilder(CreateDummyRow());
			builder2.AppendCommandTextAndBlobSaver(commandText);
			AssertNotNull(commandText.ToString());
		}

		public void TestGetBytesAsString()
		{
			var builder = GetBuilder(CreateDummyRow());
			var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 255 };

			AssertEquals("0x000102030405060708090A0B0C0D0E0F102030405060708090A0B0C0D0E0F0FF", builder.GetBytesAsString(bytes));
		}

		public void TestSmallDateTime_OutOfRange()
		{
			var factory = new BusinessObjectFactory();
			var newDate = new ZDateTime(1898, 8, 27, 10, 30, 00);
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_SmallDateTime = newDate;

			AssertExceptionThrown("Out of range small date time should throw ArgumentOutOfRangeException", (typeof(ArgumentOutOfRangeException)), () => { factory.Save(); });
			Assert("Should NOT have sent a silent error report on set of a SmallDateTime", (ErrorReporter.TotalErrorCount == 0));

			dummy.Z0_SmallDateTime = ZDateTime.MinSmallDateTimeValue;
			factory.Save();

			newDate = new ZDateTime(2099, 8, 27, 10, 30, 00);
			dummy.Z0_SmallDateTime = newDate;
			AssertExceptionThrown("Out of range small date time should throw ArgumentOutOfRangeException", (typeof(ArgumentOutOfRangeException)), () => { factory.Save(); });
			Assert("Should NOT have sent a silent error report on set of a SmallDateTime", (ErrorReporter.TotalErrorCount == 0));
		}

		public abstract void TestWithDummyRow();
		protected abstract ZSqlCommandBuilder GetBuilder(DataRow row);

		protected virtual DataRow CreateDummyRow()
		{
			return CreateDummyRow(PK);
		}

		protected DataRow CreateDummyRow(Guid pk)
		{
			var table = CreateTable();

			ByteJunkThatShouldGetCompressed = new byte[1000];
			for (var i = 0; i < 1000; i++)
			{
				ByteJunkThatShouldGetCompressed[i] = (byte)(i % 100);
			}

			var row = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Z0_Description.Name] = "hello";
			row[DummyBizoSchema.Z0_Geography.Name] = SqlGeography.STGeomFromText(new SqlChars("POINT (123 -44)"), 4326);
			row[DummyBizoSchema.Z0_Guid.Name] = Guid.Empty;
			row[DummyBizoSchema.Z0_Byte.Name] = byte.MaxValue;
			row[DummyBizoSchema.Z0_Short.Name] = (short)20;
			row[DummyBizoSchema.Z0_Number.Name] = 300;
			row[DummyBizoSchema.Z0_Date.Name] = DateTime.MaxValue;
			row[DummyBizoSchema.Z0_DateTimeOffset.Name] = new DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.FromHours(11));
			row[DummyBizoSchema.Z0_DateOnly.Name] = DateTime.MaxValue;
			row[DummyBizoSchema.Z0_Decimal.Name] = (decimal)2.455;
			row[DummyBizoSchema.Z0_Bool.Name] = true;
			row[DummyBizoSchema.Z0_AnotherNumber.Name] = 12;
			row[DummyBizoSchema.Z0_AnotherDecimal.Name] = 1.2;
			row[DummyBizoSchema.Z0_NVarCharMax.Name] = "\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E";
			row[DummyBizoSchema.Z0_VarCharMax.Name] = "SomeTextValue";
			row[DummyBizoSchema.Z0_VarBinaryMax.Name] = ByteJunkThatShouldGetCompressed;
			row[DummyBizoSchema.PK.Name] = pk;

			row.Table.Rows.Add(row);

			return row;
		}

		protected ZDataTable CreateTable()
		{
			var table = new RowFactory().GetTable(DummyBizoSchema.Constants.TableName);
			return table;
		}

		protected byte[] ByteJunkThatShouldGetCompressed;
		protected Guid PK = new Guid("78dbd2d5-457a-4368-ae9f-80b207877842");

		#region TestGetQuoteEscapeTruncateAndCompress

		public void TestGetQuoteEscapeTruncateAndCompress_EscapedString()
		{
			var builder = GetBuilder(CreateDummyRow());

			AssertEquals("'a\\\\\n\nb'", builder.GetQuoteEscapeTruncateAndCompressExposedForTest("a\\\nb", DummyBizoSchema.Z0_Description));
			AssertEquals("'a\\\\\r\n\r\nb'", builder.GetQuoteEscapeTruncateAndCompressExposedForTest("a\\\r\nb", DummyBizoSchema.Z0_Description));
			AssertEquals("'a\\b'", builder.GetQuoteEscapeTruncateAndCompressExposedForTest("a\\b", DummyBizoSchema.Z0_Description));
		}

		public void TestGetQuoteEscapeTruncateAndCompress_SpecialGuidValue()
		{
			var builder = GetBuilder(CreateDummyRow());

			builder.GetQuoteEscapeTruncateAndCompressExposedForTest(Guid.NewGuid(), DummyBizoSchema.Z0_Guid);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			builder.GetQuoteEscapeTruncateAndCompressExposedForTest(ZGuid.Invalid, DummyBizoSchema.Z0_Guid);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ZGuid.Invalid is saved to database in column Z0_Guid", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			builder.GetQuoteEscapeTruncateAndCompressExposedForTest(ZGuid.Missing, DummyBizoSchema.Z0_Guid);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ZGuid.Missing is saved to database in column Z0_Guid", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion
	}

	#endregion

	#region ZSqlCommandBuilderConcurrencyCheckWhereClauseTest

	sealed class ZSqlCommandBuilderConcurrencyCheckWhereClauseTest : ConcurrencyCheckerTest
	{
		protected override ZSqlCommandBuilder GetBuilderForConcurrencyChecks(DataRow row, string tableName, IApplicationSchemaResolver schemaResolver)
		{
			return new DummyZSqlCommandBuilder(row, tableName, schemaResolver.GetTableSchema(tableName));
		}

		public void TestConcurrencyPolicyShouldCheck()
		{
			AssertConcurrencyPolicyShouldCheck(DummyBaseBusinessObject.Schema.Z0_Description);

			// binary and large text
			AssertConcurrencyPolicyShouldCheck(DummyBaseBusinessObject.Schema.Z0_NVarCharMax);
			AssertConcurrencyPolicyShouldCheck(DummyBaseBusinessObject.Schema.Z0_VarBinaryMax);
			AssertConcurrencyPolicyShouldCheck(DummyBaseBusinessObject.Schema.Z0_VarCharMax);
			AssertConcurrencyPolicyShouldCheck(DummyBaseBusinessObject.Schema.Z0_Xml);
		}

		void AssertConcurrencyPolicyShouldCheck(string columnName)
		{
			Factory.Save();

			var column = Dummy.Table.Columns[columnName];
			var info = Dummy.ZPropertyInfoHash[columnName];
			var row = Dummy.Row;

			ZSqlCommandBuilder builder;
			MockPolicy policy;

			// positive check
			policy = SetPolicy(true, info);
			builder = CreateSqlBuilder();

			Assert(policy.ShouldCheckWasCalled);
			Assert(policy.ColumnPassedToShouldCheck == column);
			Assert(policy.RowPassedToShouldCheck == row);
			Assert(builder.ConcurrencyCheckFields.Contains(column));

			// negative check
			policy = SetPolicy(false, info);
			builder = CreateSqlBuilder();

			Assert(policy.ShouldCheckWasCalled);
			Assert(policy.ColumnPassedToShouldCheck == column);
			Assert(policy.RowPassedToShouldCheck == row);
			Assert(!builder.ConcurrencyCheckFields.Contains(column));
		}

		public void TestConcurrencyPolicyWithIgnoreAllChecks()
		{
			Factory.Save();

			var info = Dummy.Z0_CodeInfo;
			SetPolicy(true, info);

			var builder = new DummyZSqlCommandBuilder(Dummy.Row, Dummy.TableName, GetNewSchemaResolver().GetTableSchema(Dummy.TableName));

			var result = builder.AddConcurrencyCheckWhereClauseExposed(false);
			Assert(result.Contains("\t" + DummyBizoSchema.Constants.PK + " = "));
			Assert(result.Contains("\tAND " + DummyBizoSchema.Constants.Z0_Code + " = "));

			result = builder.AddConcurrencyCheckWhereClauseExposed(true);
			Assert(result.Contains("\t" + DummyBizoSchema.Constants.PK + " = "));
			Assert(!result.Contains("\tAND " + DummyBizoSchema.Constants.Z0_Code + " = "));
		}

		#region TestConcurrencyCheckWithChangedMergeableColumn

		public void TestConcurrencyCheckWithChangedMergeableColumn()
		{
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "AAA", ConcurrencyPolicy.Default, true, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "BBB", ConcurrencyPolicy.Default, true, true);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, DBNull.Value, DBNull.Value, ConcurrencyPolicy.Default, true, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, DBNull.Value, "BBB", ConcurrencyPolicy.Default, true, true);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", DBNull.Value, ConcurrencyPolicy.Default, true, true);

			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "AAA", ConcurrencyPolicy.Ignore, false, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "BBB", ConcurrencyPolicy.Ignore, false, false);

			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "AAA", ConcurrencyPolicy.Observe, false, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "BBB", ConcurrencyPolicy.Observe, true, true);

			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "AAA", ConcurrencyPolicy.Protect, false, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "BBB", ConcurrencyPolicy.Protect, true, false);

			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "AAA", ConcurrencyPolicy.Strict, true, false);
			AssertConcurrencyCheckWithChangedMergeableColumn(DummyBizoSchema.Z0_Code, "AAA", "BBB", ConcurrencyPolicy.Strict, true, false);
		}

		void AssertConcurrencyCheckWithChangedMergeableColumn(SchemaColumn column, object originalValue, object newValue, ConcurrencyPolicy concurrencyPolicy, bool expectCheck, bool expectMultiValue)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(Dummy.Row, column.Name, concurrencyPolicy);
			Dummy[column] = originalValue;
			Dummy.Factory.Save();
			Dummy[column] = newValue;

			var builder = new DummyZSqlCommandBuilder(Dummy.Row, Dummy.TableName, GetNewSchemaResolver().GetTableSchema(Dummy.TableName));
			var concurrenceCheckWhereClause = builder.AddConcurrencyCheckWhereClauseExposed(false);

			if (!expectCheck)
			{
				AssertNotContains(column.Name, concurrenceCheckWhereClause);
			}
			else
			{
				var sb = new StringBuilder();
				builder.AddColumnValueCheckForTest(sb, column, originalValue);
				var originalValueCheck = sb.ToString();

				sb = new StringBuilder();
				builder.AddColumnValueCheckForTest(sb, column, newValue);
				var newValueCheck = sb.ToString();

				var multiValueCheck = "(" + originalValueCheck + " OR " + newValueCheck + ")";

				if (!expectMultiValue)
				{
					AssertContains(originalValueCheck, concurrenceCheckWhereClause);
					AssertNotContains(multiValueCheck, concurrenceCheckWhereClause);
				}
				else
				{
					AssertContains(multiValueCheck, concurrenceCheckWhereClause);
				}
			}
		}

		#endregion
	}

	#endregion

	#region ConcurrencyChecker
	abstract class ConcurrencyCheckerTest : TestCaseWithDummy
	{
		protected virtual ZSqlCommandBuilder GetBuilderForConcurrencyChecks(DataRow row, string tableName, IApplicationSchemaResolver schemaResolver)
		{
			return new DummyZSqlCommandBuilder(row, tableName, schemaResolver.GetTableSchema(tableName));
		}

		#region Implementation

		protected static MockPolicy SetPolicy(bool shouldCheckResult, ZPropertyInfo info)
		{
			var policy = new MockPolicy();
			policy.ShouldCheckResult = shouldCheckResult;
			ConcurrencyInfo.SetConcurrencyPolicy(info.BizObj.Row, info.Name, policy);
			return policy;
		}

		protected ZSqlCommandBuilder CreateSqlBuilder()
		{
			var commandText = new StringBuilder();
			var builder = GetBuilderForConcurrencyChecks(Dummy.Row, Dummy.TableName, GetNewSchemaResolver());
			builder.AppendCommandTextAndBlobSaver(commandText);
			return builder;
		}

		protected IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		#endregion

		#region MockPolicy

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		protected class MockPolicy : ConcurrencyPolicy
		{
			public bool ShouldCheckWasCalled;
			public bool ShouldCheckResult;
			public DataRow RowPassedToShouldCheck;
			public DataColumn ColumnPassedToShouldCheck;

			public override bool ShouldCheck(DataRow row, DataColumn column)
			{
				ShouldCheckWasCalled = true;
				RowPassedToShouldCheck = row;
				ColumnPassedToShouldCheck = column;
				return ShouldCheckResult;
			}
		}

		#endregion

		#region DummyZSqlCommandBuilder

		protected class DummyZSqlCommandBuilder : ZSqlCommandBuilder
		{
			public DummyZSqlCommandBuilder(DataRow row, string tableName, ITableSchema schema)
				: base(row, tableName, true, schema)
			{
			}

			protected override void Build()
			{
				AddConcurrencyCheckWhereClause();
			}

			public override bool IsConcurrencyCheckRequired
			{
				get { return true; }
			}

			public string AddConcurrencyCheckWhereClauseExposed(bool ignoreAllConcurrencyCheck)
			{
				var oldCommandText = CommandText;
				try
				{
					CommandText = new StringBuilder();
					AddConcurrencyCheckWhereClause(ignoreAllConcurrencyCheck);
					return CommandText.ToString();
				}
				finally
				{
					CommandText = oldCommandText;
				}
			}

			public int AddColumnValueCheckForTest(StringBuilder sb, SchemaColumn column, object value) => AddColumnValueCheck(sb, column, value);
		}

		#endregion
	}
	#endregion

	#region ZSqlCommandBuilderGeneralTest

	sealed class ZSqlCommandBuilderGeneralTest : TestCaseWithFactory
	{
		/// <summary>
		/// Tests concurrency does not fail on SmallDateTime fields
		/// </summary>
		public void TestUsesSmallDateTimes()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_SmallDateTime = new ZDateTime(2010, 1, 1, 12, 34, 45);
			dummy.Z0_NVarChar = "blah";

			Factory.Save();
			dummy.Z0_NVarChar = "blah2";
			dummy.Delete();
			Factory.Save();

			// Row should be deleted
			AssertNull(new BusinessObjectFactory().Load<DummyBusinessObject>(dummy.PK));
		}

		public void TestBackSlashFeedLineCombinationIsNotEliminatedInSql()
		{
			var dummy1 = new BusinessObjectFactory { RefreshEnabled = false }.New<DummyBusinessObject>();
			dummy1.Z0_Description = "abc\\\ndef";
			dummy1.Factory.Save();

			var dummy2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy1.PK);
			AssertEquals("abc\\\ndef", dummy2.Z0_Description);

			ConcurrencyInfo.SetConcurrencyPolicy(dummy2.Row, "Z0_Description", ConcurrencyPolicy.Strict);
			dummy2.Z0_Description = "klm\\\n\n\n";
			dummy2.Factory.Save();

			var dummy3 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy1.PK);
			AssertEquals("klm\\\n\n\n", dummy3.Z0_Description);

			dummy3.Delete();
			dummy3.Factory.Save();

			var dummy4 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy1.PK);
			AssertNull(dummy4);
		}

		[ExpectNoExceptions]
		public void TestLargeXmlIsSavedWithoutExceptions()
		{
			var xml = "<root>" + "x".PadRight(ZLargeColumnSaver.MaxChunkSize, 'x') + "</root>";

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Xml = xml;
			Factory.Save();

			AssertEquals(xml, new BusinessObjectFactory().Load<DummyBusinessObject>(dummy.PK).Z0_Xml);
		}
	}

	#endregion
}
