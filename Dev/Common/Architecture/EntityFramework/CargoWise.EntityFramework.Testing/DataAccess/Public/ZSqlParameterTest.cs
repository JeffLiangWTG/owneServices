using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using WTG.Statistics;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlParameterTest : TransactionedTestCase
	{
		public void TestFullyPopulatedParameterSearchConvertedToEquals()
		{
			var paramFactory = new ParameterNameFactory();

			var pStartShort = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength - 1), DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals("(Z0_Code like @P AND Z0_Code >= @CWO1_ AND Z0_Code <= @CWO2_)", (pStartShort as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

			var pStartFullLength = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength), DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals("Z0_Code = @P", (pStartFullLength as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

			var pEndsShort = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength - 1), DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith);
			AssertEquals("Z0_Code like @P", (pEndsShort as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

			var pEndsFullLength = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength), DummyBizoSchema.Z0_Code, SQLComparisonOperator.EndsWith);
			AssertEquals("Z0_Code = @P", (pEndsFullLength as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

			var pContainsShort = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength - 1), DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains);
			AssertEquals("Z0_Code like @P", (pContainsShort as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

			var pContainsFullLength = ZSqlParameter.New("@P", new string('X', DummyBizoSchema.Z0_Code.MaxLength), DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains);
			AssertEquals("Z0_Code = @P", (pContainsFullLength as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);
		}

		public void TestExceptionIsThrownForOutOfRangeSmallDateTime()
		{
			var value = DateTime.Now.AddYears(1000);
			ITableSchema schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("ProcessTasks");
			SchemaDateTimeColumn schemaColumn = new SchemaDateTimeColumn(schema, "dateTimeColumn", 0, SqlDbType.SmallDateTime, DateTime.Now.AddYears(1000), true);
			ErrorReporter.Clear();
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => ZSqlParameter.TruncateIfNecessary(value, schemaColumn));
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			value = DateTime.Now;

			AssertNoExceptionThrown(() => ZSqlParameter.TruncateIfNecessary(value, schemaColumn));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestAddsNForUnicodeColumns()
		{
			ZSqlParameter paramUnicode = ZSqlParameter.New("@name", "value", ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn("OH_Code", "OrgHeader"));
			ZSqlParameter paramNonUnicode = ZSqlParameter.New("@name", "value", ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn("SL_Reference", "StmALog"));
			Guid guid = Guid.NewGuid();
			ZSqlParameter paramPK = ZSqlParameter.New("@name", guid, ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn("OH_PK", "OrgHeader"));
			AssertEquals("N'value'", paramUnicode.ParameterValueTextSql);
			AssertEquals("'value'", paramNonUnicode.ParameterValueTextSql);
			AssertEquals("'" + guid + "'", paramPK.ParameterValueTextSql);
		}

		public void TestIsDataViewOptimisable()
		{
			ZSqlParameter paramEqual = ZSqlParameter.New("@MyParam", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal);
			AssertEquals(true, paramEqual.IsDataViewOptimisable);
			ZSqlParameter paramNotEqual = ZSqlParameter.New("@MyParam", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.NotEqual);
			AssertEquals(false, paramNotEqual.IsDataViewOptimisable);
		}

		public void TestManuallyInsertedParameterNamesAreNotChanged()
		{
			ZSqlParameter param = ZSqlParameter.New("@MYPARAM", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.NotSpecified);
			string text = ((IFilterPart)param).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText;
			AssertEquals("@MYPARAM", param.ParameterName);
		}

		public void TestFilterIsEmpty()
		{
			IFilterPart param = ZSqlParameter.New("@MYPARAM", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal);
			Assert(!param.FilterIsEmpty);
			param = ZSqlParameter.New("@MYPARAM", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.NotSpecified);
			Assert(param.FilterIsEmpty);
		}

		public void TestDeepClone()
		{
			IFilterPart param = ZSqlParameter.New("@MYPARAM", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal);
			IFilterPart clonedParam = param.DeepClone();
			Assert(param != clonedParam);
			Assert(param.LiteralTextADO == clonedParam.LiteralTextADO);
		}

		public void TestGetSimplifiedVersion()
		{
			IFilterPart param = ZSqlParameter.New("@MYPARAM", "X", DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal);
			AssertEquals(param, param.GetSimplifiedVersion(null)[0]);
		}

		public void TestCloneDoesNotLoseZDateTime()
		{
			ZSqlParameter param = ZSqlParameter.New("@MYPARAM", ZDateTime.Empty, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal);
			ZSqlParameter clonedParam = param.ShallowClone();
			AssertEquals(typeof(ZDateTime), typeof(ZSqlParameter).InvokeMember("fRawValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, clonedParam, null).GetType());
		}

		public void TestCloneOfHandWrittenSQL()
		{
			ZQuery filter = new ZQuery();
			filter.DefaultJoinCondition = JoinCondition.And;
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			ZSqlParameter parameter = ZSqlParameter.New("@MyParam", "Code", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal);
			parameters.Add(parameter);
			filter.AddFilterAndZSQLParameterCollection("Z0_Code = @MyParam", parameters);
			ZQuery clonedFilter = filter.ShallowClone();
			AssertEquals("Z0_Code = 'Code'", filter.LiteralTextADO);
			ZNonPersistentDataQuery query = filter.ParameterisedText;
			AssertEquals("Z0_Code = @MyParam", query.ParameterisedQueryText);
		}

		public void TestInvalidZDateTimeHandled()
		{
			try
			{
				var dateTime = new DateTime(1019, 09, 10);
				var offset = System.TimeZoneInfo.Local.GetUtcOffset(dateTime);
				var zDateTime = new ZDateTime(1019, 09, 10);
				AssertEquals("pre-condition", false, zDateTime.IsValidSqlDateTime);
				AssertExceptionThrown(typeof(ArgumentException), $"See WI00114690 for details. ParameterName='', SchemaColumn='Z0_Date', Value='1019-09-10T00:00:00.0000000+{offset:hh\\:mm} (Local)'", () => ZSqlParameter.New("", zDateTime, DummyBizoSchema.Z0_Date), true);

				var invalid = ZDateTime.Invalid;
				AssertEquals("pre-condition", false, invalid.IsValidSmallDateTime);
				AssertExceptionThrown(typeof(ArgumentException), "See WI00114690 for details.", () => ZSqlParameter.New("@P1", invalid, DummyBizoSchema.Z0_Date), true);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValueForSqlForDate()
		{
			ZDateTime currentZDateTime = ZDateTime.Now;
			ZSqlParameter param = ZSqlParameter.New("@P1", currentZDateTime, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal);
			AssertEquals(currentZDateTime, param.ValueForSql);
		}

		public void TestValueForSqlForDateLessThanOrEqualToDatePartOnly()
		{
			ZDateTime currentZDateTime = ZDateTime.Now;
			ZSqlParameter param = ZSqlParameter.New("@P1", currentZDateTime, DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly);
			AssertEquals(currentZDateTime.Date.AddDays(1), param.ValueForSql);
		}

		public void TestValueForSqlForString()
		{
			ZSqlParameter param = ZSqlParameter.New("@P1", "ZZZ", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal);
			AssertEquals("ZZZ", param.ValueForSql);
		}

		public void TestValueForSqlForGuid()
		{
			ZGuid guid = ZGuid.NewZGuid();
			ZSqlParameter p = ZSqlParameter.New("@param", guid, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal);
			AssertEquals(guid, p.ValueForSql);
		}

		public void TestValueWhenPassingCharToBooleanParameter()
		{
			ZSqlParameter param = ZSqlParameter.New("@P1", 'Y', DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals(true, param.Value);
		}

		public void TestValueForMSSqlForStartsWithChar()
		{
			ZSqlParameter param = ZSqlParameter.New("@P1", "z", DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals("z%", param.ValueForSql);
		}

		public void TestValueForMSSqlForStartsWithCharWithTilde()
		{
			ZSqlParameter param = ZSqlParameter.New("@P1", "z~", DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals("z~~%", param.ValueForSql);
		}

		public void TestEncloseInADOQuotesReturnsCorrectSQLDateFormat()
		{
			DateTime currentDateTime = DateTime.Now;
			ZDateTime currentZDateTime = currentDateTime;

			IFilterPart param = ZSqlParameter.New("@blah", currentZDateTime, DummyBizoSchema.Z0_Date);
			AssertEquals("ZDateTime", "Z0_Date = #" + currentZDateTime.SqlFormat + "#", param.LiteralTextADO);
			IFilterPart param2 = ZSqlParameter.New("@blah", currentDateTime, DummyBizoSchema.Z0_Date);
			AssertEquals("DateTime", "Z0_Date = #" + currentZDateTime.SqlFormat + "#", param2.LiteralTextADO);
		}

		public void TestGetTruncatedBasicValueForDatabase()
		{
			AssertEquals("Ready for DB", DBNull.Value, ZSqlParameter.GetTruncatedBasicValueForDatabase(null, DummyBizoSchema.Z0_Guid));
			AssertEquals("Ready for DB", DBNull.Value, ZSqlParameter.GetTruncatedBasicValueForDatabase(DBNull.Value, DummyBizoSchema.Z0_Guid));
			AssertEquals("Ready for DB", "abc", ZSqlParameter.GetTruncatedBasicValueForDatabase(new ZString("abc"), DummyBizoSchema.Z0_Description));
			AssertEquals("Ready for DB", "abc", ZSqlParameter.GetTruncatedBasicValueForDatabase("abc", DummyBizoSchema.Z0_Description));
			AssertEquals("Ready for DB", new decimal(5.599), ZSqlParameter.GetTruncatedBasicValueForDatabase(new ZDecimal(5.5999), DummyBizoSchema.Z0_AnotherDecimal));
			AssertEquals("Ready for DB", new decimal(5.0), ZSqlParameter.GetTruncatedBasicValueForDatabase(new ZDecimal(5.5999), DummyBizoSchema.Z0_Decimal));
			AssertEquals("Ready for DB", new decimal(5.599), ZSqlParameter.GetTruncatedBasicValueForDatabase(new decimal(5.5999), DummyBizoSchema.Z0_AnotherDecimal));
			AssertEquals("Ready for DB", new DateTime(2000, 1, 1, 1, 1, 59, 0), ZSqlParameter.GetTruncatedBasicValueForDatabase(new DateTime(2000, 1, 1, 1, 1, 59, 00), DummyBizoSchema.Z0_Date));
			AssertEquals("Ready for DB", new DateTime(2000, 1, 1, 1, 1, 59, 0), ZSqlParameter.GetTruncatedBasicValueForDatabase(new ZDateTime(2000, 1, 1, 1, 1, 59), DummyBizoSchema.Z0_Date));
			AssertEquals("Ready for DB", new DateTime(2000, 1, 1, 1, 1, 0, 0), ZSqlParameter.GetTruncatedBasicValueForDatabase(new ZDateTime(2000, 1, 1, 1, 1, 59), GlbStaffSchema.GS_LastPasswordChangeDate));
			AssertEquals("Ready for DB", ZBlob.FromUTF8("abc"), (byte[])ZSqlParameter.GetTruncatedBasicValueForDatabase(ZBlob.FromUTF8("abc"), DummyBizoSchema.Z0_VarBinaryMax));
			AssertEquals("Ready for DB", ZBlob.FromUTF8("abc"), (byte[])ZSqlParameter.GetTruncatedBasicValueForDatabase((byte[])ZBlob.FromUTF8("abc"), DummyBizoSchema.Z0_VarBinaryMax));
			AssertEquals("Ready for DB", true, ZSqlParameter.GetTruncatedBasicValueForDatabase(true, DummyBizoSchema.Z0_Bool));
			AssertEquals("Ready for DB", false, ZSqlParameter.GetTruncatedBasicValueForDatabase(false, DummyBizoSchema.Z0_Bool));
			AssertEquals("Ready for DB", true, ZSqlParameter.GetTruncatedBasicValueForDatabase(ZBool.True, DummyBizoSchema.Z0_Bool));
			AssertEquals("Ready for DB", false, ZSqlParameter.GetTruncatedBasicValueForDatabase(ZBool.False, DummyBizoSchema.Z0_Bool));
			AssertEquals("Ready for DB", true, ZSqlParameter.GetTruncatedBasicValueForDatabase("Y", DummyBizoSchema.Z0_Bool));
			AssertEquals("Ready for DB", false, ZSqlParameter.GetTruncatedBasicValueForDatabase("N", DummyBizoSchema.Z0_Bool));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetTruncatedBasicValueForDatabaseThrowExceptionWithUnknownType()
		{
			ZSqlParameter.GetTruncatedBasicValueForDatabase(new BusinessObjectFactory(), DummyBizoSchema.Z0_Guid);
		}

		public void TestEquals()
		{
			Assert("Should be equal to another instance with same constructor parameters",
					ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).Equals(ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description)));

			Assert("Should differ as the parameters' SchemaColumns should have different MaxLength attributes",
					!(ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).Equals(ZSqlParameter.New("@foo", "bah", GlbStaffSchema.GS_LoginName))));

			Assert("Should differ as the parameters' Names are different",
					!(ZSqlParameter.New("@foo1", "bah", DummyBizoSchema.Z0_Description).Equals(ZSqlParameter.New("@foo2", "bah", DummyBizoSchema.Z0_Description))));

			SqlParameter testSqlParameter = new SqlParameter("@foo", SqlDbType.VarChar);
			testSqlParameter.Value = "bah";
			Assert("Should be equal to VarChar SqlParameter", ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).Equals(testSqlParameter));

			SqlParameter testSqlParameter2 = new SqlParameter("@foo2", SqlDbType.VarChar);
			testSqlParameter2.Value = "bah";
			Assert("Should differ as the parameters' Names are different", !ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).Equals(testSqlParameter2));
		}

		public void TestEqualsIgnoringParameterName()
		{
			Assert("Should be equal to another instance with same constructor parameters",
					ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).EqualsIgnoringParameterName(ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description)));

			Assert("Should differ as the parameters' SchemaColumns should have different MaxLength attributes",
					!(ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).EqualsIgnoringParameterName(ZSqlParameter.New("@foo", "bah", GlbStaffSchema.GS_LoginName))));

			Assert("Should be equal as the parameters' Names are not compared",
					(ZSqlParameter.New("@foo1", "bah", DummyBizoSchema.Z0_Description).EqualsIgnoringParameterName(ZSqlParameter.New("@foo2", "bah", DummyBizoSchema.Z0_Description))));

			SqlParameter testSqlParameter = new SqlParameter("@foo", SqlDbType.VarChar);
			testSqlParameter.Value = "bah";
			Assert("Should be equal to VarChar SqlParameter", ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).EqualsIgnoringParameterName(testSqlParameter));

			SqlParameter testSqlParameter2 = new SqlParameter("@foo2", SqlDbType.VarChar);
			testSqlParameter2.Value = "bah";
			Assert("Should be equal as the parameters' Names are not compared", ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description).EqualsIgnoringParameterName(testSqlParameter2));
		}

		public void TestLiteralTextADOEqualsComparisonToNull()
		{
			IFilterPart param = ZSqlParameter.New("@Param1", null, DummyBizoSchema.Z0_Guid);
			AssertEquals("Z0_Guid is null", param.LiteralTextADO);
		}

		public void TestZTypesConstruction()
		{
			Assert(ZSqlParameter.New("@foo", new ZString("bah"), DummyBizoSchema.Z0_Description).Equals(ZSqlParameter.New("@foo", "bah", DummyBizoSchema.Z0_Description)));
		}

		public void TestSqlParamConstruction()
		{
			SqlParameter testSqlParameter = new SqlParameter("@foo", SqlDbType.VarChar);
			testSqlParameter.Value = "bah";
			Assert("Should be equal to VarChar SqlParameter", ZSqlParameter.New("@foo", new ZString("bah"), DummyBizoSchema.Z0_Description).Equals(testSqlParameter));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidParameterName()
		{
			ZSqlParameter.New("invalid", "hello", CargoWise.Schema.Schema.GenericStringSchemaColumn);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidParameterNameStartingWithTwoOrMoreAtSymbols()
		{
			ZSqlParameter.New("@@invalid", "hello", CargoWise.Schema.Schema.GenericStringSchemaColumn);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidRenamedParameterName()
		{
			var parameter = ZSqlParameter.New("@valid", "hello", CargoWise.Schema.Schema.GenericStringSchemaColumn);
			parameter.Rename("@@@invalid");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestInvalidSchemaColumn()
		{
			ZSqlParameter testParam = ZSqlParameter.New("@param", "value", null);
		}

		//			[ExpectException(typeof(ArgumentException))]
		//public void TestInvalidDataTypeForColumn()
		//{
		//    ZSqlParameter.New("@P", 3.2m, DummyBizoSchema.Z0_Short);
		//}

		public void TestParameterValueTextADODecimal()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", 3.25m, DummyBizoSchema.Z0_AnotherDecimal, SQLComparisonOperator.Equal);
			AssertEquals("3.25", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOInt()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", 1234567890, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal);
			AssertEquals("1234567890", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOShort()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", (short)3, DummyBizoSchema.Z0_Short, SQLComparisonOperator.Equal);
			AssertEquals("3", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOBool()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", false, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals("0", p.ParameterValueTextADO);
			ZSqlParameter q = ZSqlParameter.New("@param", true, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals("1", q.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOString()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", "Hello", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal);
			AssertEquals("'Hello'", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADODateTime()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", new ZDateTime(1971, 9, 18), DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal);
			AssertEquals("#1971-09-18 00:00:00.000#", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOTime()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", new ZTime(12, 9), DummyBizoSchema.Z0_Time, SQLComparisonOperator.Equal);
			AssertEquals("#12:09:00#", p.ParameterValueTextADO);
		}

		public void TestParameterValueTextADOGuid()
		{
			ZGuid guid = ZGuid.NewZGuid();
			ZSqlParameter p = ZSqlParameter.New("@param", guid, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal);
			AssertEquals(string.Format("CONVERT('{0}', 'System.Guid')", guid.ToString()), p.ParameterValueTextADO);
		}

		public void TestParameterValueTextSqlDecimal()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", 3.25m, DummyBizoSchema.Z0_AnotherDecimal, SQLComparisonOperator.Equal);
			AssertEquals("3.25", p.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlInt()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", 1234567890, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal);
			AssertEquals("1234567890", p.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlShort()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", (short)3, DummyBizoSchema.Z0_Short, SQLComparisonOperator.Equal);
			AssertEquals("3", p.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlBool()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", false, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals("'N'", p.ParameterValueTextSql);
			ZSqlParameter q = ZSqlParameter.New("@param", true, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals("'Y'", q.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlString()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", "Hello", DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal);
			AssertEquals("'Hello'", p.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlDateTime()
		{
			ZSqlParameter p = ZSqlParameter.New("@param", new ZDateTime(1971, 9, 18), DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal);
			AssertEquals("'1971-09-18 00:00:00.000'", p.ParameterValueTextSql);
		}

		public void TestParameterValueTextSqlGuid()
		{
			ZGuid guid = ZGuid.NewZGuid();
			ZSqlParameter p = ZSqlParameter.New("@param", guid, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal);
			AssertEquals(string.Format("'{0}'", guid.ToString()), p.ParameterValueTextSql);
		}

		public void TestContainsOrOperator()
		{
			IFilterPart filterPart = ZSqlParameter.New("@param", "hello", CargoWise.Schema.Schema.GenericStringSchemaColumn);
			AssertEquals(false, filterPart.ContainsOrOperator);
		}

		public void TestHasBlobFilters()
		{
			ZSqlParameter blobby = ZSqlParameter.New("@Param", ZBlob.FromAscii("Hello"), DummyBizoSchema.Z0_VarBinaryMax);
			AssertEquals(true, blobby.BlobFilters.Contains(DummyBizoSchema.Z0_VarBinaryMax));
			ZSqlParameter notBlobby = ZSqlParameter.New("@Param", "Hello", DummyBizoSchema.Z0_Code);
			AssertEquals(0, notBlobby.BlobFilters.Count());
		}

		public void TestLiteralTextADOForEqualTrue()
		{
			IFilterPart blobby = ZSqlParameter.New("@Param", true, DummyBizoSchema.Z0_Bool);
			AssertEquals("Z0_Bool = 1", blobby.LiteralTextADO);
		}

		public void TestLiteralTextADOForNotEqualTrue()
		{
			IFilterPart blobby = ZSqlParameter.New("@Param", true, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual);
			AssertEquals("Z0_Bool <> 1", blobby.LiteralTextADO);
		}

		public void TestLiteralTextADOForEqualFalse()
		{
			IFilterPart blobby = ZSqlParameter.New("@Param", false, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal);
			AssertEquals("Z0_Bool = 0", blobby.LiteralTextADO);
		}

		public void TestLiteralTextADOForNotEqualFalse()
		{
			IFilterPart blobby = ZSqlParameter.New("@Param", false, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual);
			AssertEquals("Z0_Bool <> 0", blobby.LiteralTextADO);
		}

		public void TestLiteralTextADOForNonPersistentColumn()
		{
			var param1 = ZSqlParameter.New("", 123, new SchemaIntColumn(CargoWise.Schema.Schema.GenericTableSchema, DummyBizoSchema.Constants.Z0_Number, 0, 0, false));
			AssertExceptionThrown<InvalidOperationException>(() => { var s = param1.LiteralTextADO; });
			AssertExceptionThrown<InvalidOperationException>(() => { var s = ((IFilterPart)param1).ParameterisedSql(new ParameterNameFactory()); });

			var dummy1 = new BusinessObjectFactory().New<DummyBusinessObject>();
			dummy1.Z0_Number = 5;
			((ISupportMainElement)param1).SetMainElement(dummy1);

			AssertEquals("5 = 123", param1.LiteralTextADO);
			AssertEquals("5 = @CWO1_", ((IFilterPart)param1).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText);

			dummy1.Z0_Number = 10;

			AssertEquals("10 = 123", param1.LiteralTextADO);
			AssertEquals("10 = @CWO1_", ((IFilterPart)param1).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText);

			var dummy2 = new BusinessObjectFactory().New<DummyBusinessObject>();
			dummy2.Z0_Number = 0;
			((ISupportMainElement)param1).SetMainElement(dummy2);

			AssertEquals("0 = 123", param1.LiteralTextADO);
			AssertEquals("0 = @CWO1_", ((IFilterPart)param1).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText);

			var param2 = ZSqlParameter.New("", new DateTime(2015, 10, 14, 10, 48, 57), new SchemaDateTimeColumn(CargoWise.Schema.Schema.GenericTableSchema, DummyBizoSchema.Constants.Z0_Date, 0, SqlDbType.DateTime, DateTime.MinValue, false));
			dummy1.Z0_Date = new ZDateTime(new DateTime(2014, 12, 10, 15, 55, 30));
			((ISupportMainElement)param2).SetMainElement(dummy1);

			AssertEquals("#2014-12-10 15:55:30.000# = #2015-10-14 10:48:57.000#", param2.LiteralTextADO);
			AssertEquals("'2014-12-10 15:55:30.000' = @CWO1_", ((IFilterPart)param2).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText);
		}

		public void TestHasParameters()
		{
			AssertEquals(true, ZSqlParameter.New("@Param", false, DummyBizoSchema.Z0_Bool, SQLComparisonOperator.Equal).HasParameters);
		}

		public void TestParameterisedSqlForBinaryField()
		{
			var parameterNameFactory = new ParameterNameFactory();

			IFilterPart param = ZSqlParameter.New("@Param", null, DummyBizoSchema.Z0_VarBinaryMax, SQLComparisonOperator.Equal);
			AssertEquals("Z0_VarBinaryMax is NULL", param.ParameterisedSql(parameterNameFactory).LiteralTextSql);

			param = ZSqlParameter.New("@Param", new ZBlob(new byte[] { 0x10, 0x3C, 0xFF }), DummyBizoSchema.Z0_VarBinaryMax, SQLComparisonOperator.Equal);
			AssertEquals("Z0_VarBinaryMax = 0x103CFF", param.ParameterisedSql(parameterNameFactory).LiteralTextSql);

			param = ZSqlParameter.New("@Param", "abc", DummyBizoSchema.Z0_VarBinaryMax, SQLComparisonOperator.Equal);
			AssertEquals("dbo.CLRUncompressAsString(Z0_VarBinaryMax) = 'abc'", param.ParameterisedSql(parameterNameFactory).LiteralTextSql);
		}

		public void TestParameterSuffixForBinaryField()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_VarBinaryMax = ZBlob.FromUTF8("abc");

			factory.Save();

			var sch_name = DummyBizoSchema.Constants.SqlSchemaName;
			var obj_name = DummyBizoSchema.Constants.TableName;
			var col_name = DummyBizoSchema.Constants.Z0_VarBinaryMax;
			TestConnection.ExecuteNonQuery($"CREATE STATISTICS _s ON {sch_name.QuoteName()}.{obj_name.QuoteName()} ({col_name.QuoteName()}) WITH FULLSCAN;");

			CombineAssertions(() =>
			{
				var sqlConnection = (IDbConnectionInternals)TestConnection;
				var histograms = new HistogramsRetriever().GetHistograms(sqlConnection.ADOConnection, sqlConnection.ADOTransaction, sch_name, obj_name);
				var persister = ObjectFactory.Get<IStatisticsPersister>();
				persister.Save(sch_name, obj_name, histograms);

				var query = new ZQuery(DummyBizoSchema.Z0_VarBinaryMax, "abc")
				{
					IsDBOnlyQuery = true
				};

				AssertEquals("dbo.CLRUncompressAsString(Z0_VarBinaryMax) = @CWO1_", query.FilterString);
				AssertEquals(1, new BusinessObjectFactory(TestConnection).Load<DummyBusinessObject>(query).Length);
			});
		}

		public void TestParameterValueTextSqlTVPWithNullZGuid()
		{
			var zguidList = new List<ZGuid>();
			zguidList.Add(ZGuid.Empty);
			ZSqlParameter p = ZSqlParameter.New(null, zguidList, DummyBizoSchema.PK, SQLComparisonOperator.Equal, ComparisonOptions.Default, true);
			AssertEquals("'" + ZGuid.Empty.ToString() + "'", p.ParameterValueTextSql);
		}

		public void TestIsLiteralOnly()
		{
			AssertEquals(true, DummyBizoSchema.Z0_BitFiltered.IsLiteralOnly);

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = String.Empty;

				var paramFactory = new ParameterNameFactory();
				var p = ZSqlParameter.New("", true, DummyBizoSchema.Z0_BitFiltered);
				AssertEquals(true, p.IsLiteralOnly);
				AssertEquals("Z0_BitFiltered = 1", (p as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);
			}

			AssertEquals(false, DummyBizoSchema.Z0_BitTrue.IsLiteralOnly);
			AssertEquals(false, DummyBizoSchema.Z0_BitFalse.IsLiteralOnly);

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = String.Empty;

				var paramFactory = new ParameterNameFactory();
				var p = ZSqlParameter.New("", true, DummyBizoSchema.Z0_BitTrue);
				AssertEquals(false, p.IsLiteralOnly);
				AssertEquals("Z0_BitTrue = @CWO1_", (p as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = ",,AAA, aaa   , , z0_bitfalse, , Z0_BITFALSE,Z0_BITTRUE,  z0_bittrue";

				AssertContainsExactElementsInAnyOrder(StringComparer.OrdinalIgnoreCase, new string[] { "AAA", "Z0_BitTrue", "Z0_BitFalse" }, ParameterSettingsCache.FieldsToLiteralize);

				var paramFactory = new ParameterNameFactory();

				var p1 = ZSqlParameter.New("", true, DummyBizoSchema.Z0_BitTrue);
				AssertEquals(true, p1.IsLiteralOnly);
				AssertEquals("Z0_BitTrue = 1", (p1 as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);

				var p2 = ZSqlParameter.New("", true, DummyBizoSchema.Z0_BitFalse);
				AssertEquals(true, p2.IsLiteralOnly);
				AssertEquals("Z0_BitFalse = 1", (p2 as IFilterPart).ParameterisedSql(paramFactory).ParameterisedQueryText);
			}
		}

		public void TestBucketizationOfTVPWorksBasedOnRegistrySettings()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				// minimum of 3 elements gets a TVP
				// 3-4 bucket 1
				// 5-9 bucket 2
				// 10-int32..maxvalue bucket 3
				settings.TVPRule = new TVPRule("3,5,10");

				var query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "A" });
				AssertEquals("Z0_Code = @CWO1_", query.ParameterisedText.ParameterisedQueryText);

				query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "A", "B" });
				AssertEquals("(Z0_Code in (@CWO1_, @CWO2_))", query.ParameterisedText.ParameterisedQueryText);

				query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "A", "B", "C" });
				AssertEquals("(Z0_Code in (SELECT Value FROM @CWO1_))", query.ParameterisedText.ParameterisedQueryText);

				AssertEquals(0, settings.TVPRule.BucketNumber(Int32.MinValue));
				AssertEquals(0, settings.TVPRule.BucketNumber(0));
				AssertEquals(0, settings.TVPRule.BucketNumber(1));
				AssertEquals(0, settings.TVPRule.BucketNumber(2));
				AssertEquals(1, settings.TVPRule.BucketNumber(3));
				AssertEquals(1, settings.TVPRule.BucketNumber(4));
				AssertEquals(2, settings.TVPRule.BucketNumber(5));
				AssertEquals(2, settings.TVPRule.BucketNumber(6));
				AssertEquals(2, settings.TVPRule.BucketNumber(7));
				AssertEquals(2, settings.TVPRule.BucketNumber(8));
				AssertEquals(2, settings.TVPRule.BucketNumber(9));
				AssertEquals(3, settings.TVPRule.BucketNumber(10));
				AssertEquals(3, settings.TVPRule.BucketNumber(11));
				AssertEquals(3, settings.TVPRule.BucketNumber(Int32.MaxValue));
			}
		}

		public void TestUseTVP()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("1");

				var query = new ZQuery { AllowTableValuedParameters = true };
				query.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "A", "B" });
				AssertEquals("(Z0_Code in (SELECT Value FROM @CWO1_))", query.ParameterisedText.ParameterisedQueryText);
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = null;

				var query = new ZQuery(DummyBizoSchema.Z0_Code, new string[] { "A", "B" }) { AllowTableValuedParameters = true };
				AssertEquals("(Z0_Code in (@CWO1_, @CWO2_))", query.ParameterisedText.ParameterisedQueryText);
			}
		}

		public void TestUseTVPDefaultValuesForTestEntityFrameworkSettingsAndRegistryAreEqual()
		{
			AssertEquals(EnvProxy.Instance.Registry.TVPRule.RegistryText, TestEntityFrameworkSettings.Get().TVPRule.RegistryText);
		}

		public void TestParameterFieldMaximumLengthExceeded()
		{
			var param = ZSqlParameter.New("@paramMaximumLengthNotExceeded", new ZString('a', 5), DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);

			param = ZSqlParameter.New("@paramMaximumLengthExceededCode", new string('a', 6), DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith);
			AssertEquals("FieldMaxLengthExceeded_" + param.SchemaColumn.Name, ErrorReporter.LastKeyReported);

			param = ZSqlParameter.New("@paramMaximumLengthExceededFKCode", new ZString('a', 6), DummyBizoSchema.Z0_FK_Code, SQLComparisonOperator.Contains);
			AssertEquals("FieldMaxLengthExceeded_" + param.SchemaColumn.Name, ErrorReporter.LastKeyReported);

			param = ZSqlParameter.New("@paramMaximumLengthExceededNVarChar", new ZString('a', 21), DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith);
			AssertEquals("FieldMaxLengthExceeded_" + param.SchemaColumn.Name, ErrorReporter.LastKeyReported);

			param = ZSqlParameter.New("@paramMaximumLengthExceededDescription", new ZString('a', 101), DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains);
			AssertEquals("FieldMaxLengthExceeded_" + param.SchemaColumn.Name, ErrorReporter.LastKeyReported);

			AssertEquals(4, ErrorReporter.TotalErrorCount);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#region HasComparisonOperator

		public void TestHasComparisonOperator()
		{
			var param = ZSqlParameter.New("@myBoolParam", true, DummyBizoSchema.Z0_Bool);
			AssertEquals(param.HasComparisonOperatorLike, false);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Like);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.StartsWith);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.DoesNotStartWith);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.EndsWith);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.DoesNotEndWith);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.Contains);
			AssertEquals(param.HasComparisonOperatorLike, true);

			param = ZSqlParameter.New("@P1", "A", DummyBizoSchema.Z0_NVarChar, SQLComparisonOperator.NotContains);
			AssertEquals(param.HasComparisonOperatorLike, true);
		}

		#endregion
	}

	sealed class ZSqlParameterValueConversionTest : TestCase
	{
		public void TestValidBinaryValues()
		{
			AssertEquals(string.Empty, ZSqlParameter.New("@param", string.Empty, DummyBizoSchema.Z0_VarBinaryMax).Value);
			AssertEquals(ZString.Empty, ZSqlParameter.New("@param", string.Empty, DummyBizoSchema.Z0_VarBinaryMax).Value);
			AssertArrayEqualsByElements(Array.Empty<byte>(), ZSqlParameter.New("@param", new ZBlob(), DummyBizoSchema.Z0_VarBinaryMax).Value as byte[]);
			AssertArrayEqualsByElements(Array.Empty<byte>(), ZSqlParameter.New("@param", Array.Empty<byte>(), DummyBizoSchema.Z0_VarBinaryMax).Value as byte[]);
		}

		public void TestInvalidBooleanValues()
		{
			AssertExceptionThrown<ArgumentException>(() => ZSqlParameter.New("@param", "HELLO", DummyBizoSchema.Z0_Bool));
			AssertExceptionThrown<ArgumentException>(() => ZSqlParameter.New("@param", (ZString)"HELLO", DummyBizoSchema.Z0_Bool));
		}

		public void TestValidBooleanValues()
		{
			AssertEquals(true, ZSqlParameter.New("@param", ZBool.True, DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(false, ZSqlParameter.New("@param", ZBool.False, DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(true, ZSqlParameter.New("@param", "Y", DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(false, ZSqlParameter.New("@param", "N", DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(true, ZSqlParameter.New("@param", (ZString)"Y", DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(false, ZSqlParameter.New("@param", (ZString)"N", DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(true, ZSqlParameter.New("@param", 'Y', DummyBizoSchema.Z0_Bool).Value);
			AssertEquals(false, ZSqlParameter.New("@param", 'N', DummyBizoSchema.Z0_Bool).Value);
		}

		public void TestValidByteValues()
		{
			AssertEquals((byte)0, ZSqlParameter.New("@param", (byte)0, DummyBizoSchema.Z0_Byte).Value);
			AssertEquals((byte)0, ZSqlParameter.New("@param", (ZByte)0, DummyBizoSchema.Z0_Byte).Value);
		}

		public void TestValidDateValues()
		{
			var brettsBirthday = ZDateTime.BrettsBirthday.ToDateTime();
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDateTime.BrettsBirthday, DummyBizoSchema.Z0_DateOnly).Value);
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDate.BrettsBirthday, DummyBizoSchema.Z0_DateOnly).Value);
		}

		public void TestValidDateTimeValues()
		{
			var brettsBirthday = ZDateTime.BrettsBirthday.ToDateTime();
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDateTime.BrettsBirthday, DummyBizoSchema.Z0_Date).Value);
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDate.BrettsBirthday, DummyBizoSchema.Z0_Date).Value);

			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", ZDateTime.Empty, DummyBizoSchema.Z0_Date));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", ZDate.Empty, DummyBizoSchema.Z0_Date));
		}

		public void TestValidDateTimeOffsetValues()
		{
			var brettsBirthday = ZDateTime.BrettsBirthday.ToDateTime();
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDateTime.BrettsBirthday, DummyBizoSchema.Z0_DateTimeOffset).Value);
			AssertEquals(brettsBirthday, ZSqlParameter.New("@param", ZDate.BrettsBirthday, DummyBizoSchema.Z0_DateTimeOffset).Value);

			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", ZDateTime.Empty, DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", ZDate.Empty, DummyBizoSchema.Z0_DateTimeOffset));

			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Utc), DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Local), DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.Zero), DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(14)), DummyBizoSchema.Z0_DateTimeOffset));
			AssertNoExceptionThrown(() => ZSqlParameter.New("@param", new ZDateTimeOffset(1971, 9, 18, 0, 0, 0, TimeSpan.FromHours(-14)), DummyBizoSchema.Z0_DateTimeOffset));
		}

		public void TestValidDecimalValues()
		{
			AssertEquals(decimal.Zero, ZSqlParameter.New("@param", ZInt.Zero, DummyBizoSchema.Z0_Decimal).Value);
			AssertEquals(decimal.Zero, ZSqlParameter.New("@param", ZDecimal.Zero, DummyBizoSchema.Z0_Decimal).Value);
		}

		public void TestValidGeographyValues()
		{
			var value = ZSqlParameter.New("@param", new ZGeography("POINT(-122 47)"), DummyBizoSchema.Z0_Geography).Value;
			AssertType<SqlGeography>(value);
			Assert(SqlGeography.STGeomFromText(new SqlChars("POINT(-122 47)"), 4326).STEquals((SqlGeography)value).Value);
		}

		public void TestValidGuidValues()
		{
			AssertEquals(Guid.Empty, ZSqlParameter.New("@param", ZGuid.Empty, DummyBizoSchema.Z0_Guid).Value);
		}

		public void TestValidIntValues()
		{
			AssertEquals(0, ZSqlParameter.New("@param", ZInt.Zero, DummyBizoSchema.Z0_Number).Value);
		}

		public void TestValidShortValues()
		{
			AssertEquals((byte)0, ZSqlParameter.New("@param", ZByte.Zero, DummyBizoSchema.Z0_Short).Value);
			AssertEquals((short)0, ZSqlParameter.New("@param", ZShort.Zero, DummyBizoSchema.Z0_Short).Value);
		}

		public void TestValidStringValues()
		{
			AssertEquals(string.Empty, ZSqlParameter.New("@param", ZString.Empty, DummyBizoSchema.Z0_NVarChar).Value);
		}

		public void TestValidXmlValues()
		{
			AssertEquals(string.Empty, ZSqlParameter.New("@param", ZString.Empty, DummyBizoSchema.Z0_Xml).Value);
		}
	}

	sealed class DbCommandExtensionsTest : TestCase
	{
		public void TestAddParameterAddsValueAdjustedForComparisonForTableValuedParameter()
		{
			var paramFactory = new ParameterNameFactory();
			var param = ZSqlParameter.New(paramFactory, new ZDateTime[] { ZDateTime.Today }, DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ComparisonOptions.Default, true);
			var paramName = paramFactory.GetParameterName(param);
			using (var command = Db.Connection.Command(ZString.Empty))
			{
				command.AddParameter(param);
				AssertEquals(true, param.IsTableValued);
				var dataTable = (DataTable)command.GetParameterValue(paramName);
				var list = (List<object>)(param.ValueAdjustedForComparison);
				AssertEquals(list[0], dataTable.Rows[0][0]);
			}
		}

		public void TestAddParameterAddsValueAdjustedForComparisonForParameterBasedOnDBColumn()
		{
			var param = ZSqlParameter.New("@paramDateTime", ZDateTime.Today, DummyBizoSchema.Z0_Date, SQLComparisonOperator.LessThanOrEqualToDatePartOnly);
			using (var command = Db.Connection.Command(ZString.Empty))
			{
				command.AddParameter(param);
				AssertEquals(false, param.IsTableValued);
				AssertEquals(param.ValueAdjustedForComparison, command.GetParameterValue("@paramDateTime"));
			}
		}
	}
}
