using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	class DataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestDataObjectStringCharacterCaseProperty

		public void TestStringValueCharacterCaseProperty()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			row[DummyBizoSchema.Constants.Z0_Description] = "nothing";

			var reader = new DataObjectReaderWithUpperCaseStringSetter(logger, Factory);
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Description, "hello");
			AssertEquals("HELLO", row[DummyBizoSchema.Constants.Z0_Description]);

			var reader1 = new DataObjectReaderTestClass(logger, Factory);
			reader1.SetValueExposed(row, DummyBizoSchema.Z0_Description, "hEllo");
			AssertEquals("hEllo", row[DummyBizoSchema.Constants.Z0_Description]);

			var reader2 = new DataObjectReaderWithLowerCaseStringSetter(logger, Factory);
			reader2.SetValueExposed(row, DummyBizoSchema.Z0_Description, "HELLO");
			AssertEquals("hello", row[DummyBizoSchema.Constants.Z0_Description]);
		}

		class DataObjectReaderWithUpperCaseStringSetter : DataObjectReaderTestClass
		{
			public DataObjectReaderWithUpperCaseStringSetter(IXmlImportLogger logger, UniversalObjectFactory factory) : base(logger, factory) { }
			protected override ZArchitecture.Environment.CharacterCase StringValueCharacterCase => ZArchitecture.Environment.CharacterCase.Upper;
		}

		class DataObjectReaderWithLowerCaseStringSetter : DataObjectReaderTestClass
		{
			public DataObjectReaderWithLowerCaseStringSetter(IXmlImportLogger logger, UniversalObjectFactory factory) : base(logger, factory) { }
			protected override ZArchitecture.Environment.CharacterCase StringValueCharacterCase => ZArchitecture.Environment.CharacterCase.Lower;
		}

		#endregion

		public void TestSetValueString()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			row[DummyBizoSchema.Constants.Z0_Description] = "HELLO";
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Description, (ZString?)null);
			AssertEquals("HELLO", row[DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Description, "BYE");
			AssertEquals("BYE", row[DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Description, new ZString("HI"));
			AssertEquals("HI", row[DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Description, ZString.Empty);
			AssertEquals("", row[DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals("Logs", "", logger.Logs);

			var maxValue = "A".PadRight(DummyBizoSchema.Z0_Code.MaxLength, 'B');
			var longValue = maxValue + "C";
			var expectedWarning = string.Format("Warning - Attempted to insert {0} characters into Field [{1}] which has a maximum length of {2} characters. Field was truncated.", DummyBizoSchema.Z0_Code.MaxLength + 1, DummyBizoSchema.Z0_Code.Name, DummyBizoSchema.Z0_Code.MaxLength);
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, longValue);
			AssertEquals(maxValue, row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", expectedWarning, logger.Logs);

			logger.ClearLogs();
			row[DummyBizoSchema.Constants.Z0_Code] = "";

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, longValue, delaySetters);
			AssertEquals("", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Code)].SetValue();
			AssertEquals(maxValue, row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", expectedWarning, logger.Logs);
		}

		[ExpectNoExceptions]
		public void TestSetValueString_ShouldTakeCorrectMaxLength()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var dummyRow = (IColumnIndexer)dummyBO;

			reader.SetValueExposed(dummyRow, DummyBizoSchema.Z0_Code, "HELLO");
			AssertEquals("HELLO", dummyRow[DummyBizoSchema.Constants.Z0_Code]);

			var derivedDummyBO = Factory.New<DerivedDummyBusinessObject>();
			var derivedDummyRow = (IColumnIndexer)derivedDummyBO;

			reader.SetValueExposed(derivedDummyRow, DummyBizoSchema.Z0_Description, "HELLO");
			AssertEquals("HEL", derivedDummyRow[DummyBizoSchema.Constants.Z0_Description]);

			reader.SetValueExposed(derivedDummyRow, DummyBizoSchema.Z0_Code, "HELLO");
			AssertEquals("H", derivedDummyRow[DummyBizoSchema.Constants.Z0_Code]);
		}

		public void TestSetValueGuid()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var zGuid1 = ZGuid.NewZGuid();
			row[DummyBizoSchema.Constants.Z0_Guid] = guid1;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, null);
			AssertEquals(guid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, guid2);
			AssertEquals(guid2, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, zGuid1);
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, ZGuid.Invalid);
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", string.Format("Error - Cannot set Invalid GUID '{0}' to Column {1}.", ZGuid.Invalid, DummyBizoSchema.Constants.Z0_Guid), logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, ZGuid.Missing);
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", string.Format("Error - Cannot set Invalid GUID '{0}' to Column {1}.", ZGuid.Missing, DummyBizoSchema.Constants.Z0_Guid), logger.Logs);

			logger.ClearLogs();
			AssertEquals("IsNullable", true, DummyBizoSchema.Z0_Guid.IsNullable);
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, ZGuid.Empty);
			AssertEquals(DBNull.Value, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			row[DummyBizoSchema.Constants.Z0_Guid] = zGuid1;
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, ZGuid.Invalid, delaySetters);
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Guid)].SetValue();
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", string.Format("Error - Cannot set Invalid GUID '{0}' to Column {1}.", ZGuid.Invalid, DummyBizoSchema.Constants.Z0_Guid), logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Guid, guid2, delaySetters);
			AssertEquals(zGuid1, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Guid)].SetValue();
			AssertEquals(guid2, row[DummyBizoSchema.Constants.Z0_Guid]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			row = (IColumnIndexer)RowFactory.New(GlbBranchSchema.Constants.TableName);
			AssertEquals("IsNullable", false, GlbBranchSchema.GB_GC.IsNullable);
			reader.SetValueExposed(row, GlbBranchSchema.GB_GC, ZGuid.Empty);
			AssertEquals(Guid.Empty, row[GlbBranchSchema.Constants.GB_GC]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueBool()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			row[DummyBizoSchema.Constants.Z0_Bool] = true;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Bool, null);
			AssertEquals(true, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Bool, false);
			AssertEquals(false, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Bool, ZBool.True);
			AssertEquals(true, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Bool, ZBool.False);
			AssertEquals(false, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Bool, ZBool.True, delaySetters);
			AssertEquals(false, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Bool)].SetValue();
			AssertEquals(true, row[DummyBizoSchema.Constants.Z0_Bool]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueBlob()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = ASCIIEncoding.ASCII.GetBytes("HI");
			var data2 = ASCIIEncoding.ASCII.GetBytes("BYE");
			row[DummyBizoSchema.Constants.Z0_VarBinaryMax] = data1;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_VarBinaryMax, null);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_VarBinaryMax, ZBlob.Empty);
			AssertEquals(DBNull.Value, row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_VarBinaryMax, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			AssertEquals("Logs", "", logger.Logs);

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_VarBinaryMax, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_VarBinaryMax)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_VarBinaryMax]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueByte()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = (byte)'A';
			var data2 = (byte)'B';
			ZByte data3 = ZByte.ParseSafe("C", ZByte.Zero);
			row[DummyBizoSchema.Constants.Z0_Byte] = data1;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Byte, null);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Byte, ZByte.Zero);
			AssertEquals((byte)0, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Byte, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Byte, data3);
			AssertEquals(data3, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Byte, data2, delaySetters);
			AssertEquals(data3, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Byte)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Byte]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueDateTime()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = ZDateTime.BrettsBirthday;
			var data2 = ZDateTime.BrettsBirthday.AddDays(2).ToDateTime();
			ZByte data3 = ZByte.ParseSafe("C", ZByte.Zero);
			row[DummyBizoSchema.Constants.Z0_Date] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, DateTime.MinValue);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Date to '<Invalid>' - Value is out of range.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, ZDateTime.Empty);
			AssertEquals(DBNull.Value, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, ZDateTime.Invalid);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Date to '<Invalid>' - Value is out of range.", logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, ZDateTime.Invalid, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Date)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Date to '<Invalid>' - Value is out of range.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Date, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Date)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Date]);
			AssertEquals("Logs", "", logger.Logs);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetValueDateTimeOffset()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = new ZDateTimeOffset(2008, 03, 20, 10, 11, 12, 123, TimeSpan.FromHours(11));
			var data2 = new ZDateTimeOffset(2008, 05, 20, 10, 11, 12, 123, TimeSpan.Zero);
			ZByte data3 = ZByte.ParseSafe("C", ZByte.Zero);
			row[DummyBizoSchema.Constants.Z0_DateTimeOffset] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, DateTimeOffset.MinValue);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_DateTimeOffset to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, ZDateTimeOffset.Empty);
			AssertEquals(DBNull.Value, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, ZDateTimeOffset.Invalid);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_DateTimeOffset to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, ZDateTimeOffset.Invalid, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_DateTimeOffset)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_DateTimeOffset to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_DateTimeOffset, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_DateTimeOffset)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_DateTimeOffset]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueTime()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = new ZTime(1, 2);
			var data2 = new ZTime(4, 5);
			ZByte data3 = ZByte.ParseSafe("C", ZByte.Zero);
			row[DummyBizoSchema.Constants.Z0_Time] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, ZTime.Empty);
			AssertEquals(DBNull.Value, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, ZTime.Invalid);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Time to '<Invalid>' - Value is out of range.", logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, ZTime.Invalid, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Time)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Time to '<Invalid>' - Value is out of range.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Time, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Time)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Time]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueGeography()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = new ZGeography("-121 48");
			var data2 = new ZGeography("-121.2 49.9");
			ZByte data3 = ZByte.ParseSafe("C", ZByte.Zero);
			row[DummyBizoSchema.Constants.Z0_Geography] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, ZGeography.Invalid);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Geography to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, ZGeography.Empty);
			Assert(ZGeography.EmptySqlGeography.STEquals((SqlGeography)row[DummyBizoSchema.Constants.Z0_Geography]).Value);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, ZGeography.Invalid);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Geography to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, ZGeography.Invalid, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Geography)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "Warning - Cannot set Column DummyBizo.Z0_Geography to '<Invalid>' - Value is invalid.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Geography, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Geography)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Geography]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueDate()
		{
			// Using GlbStaff as DummyBizO currently has no persistent date only column
			var row = (IColumnIndexer)RowFactory.New(GlbStaffSchema.Constants.TableName);
			var date1 = ZDateTime.BrettsBirthday.Date;
			var date2 = ZDateTime.BrettsBirthday.AddDays(2).Date;
			var date3 = ZDateTime.BrettsBirthday.AddDays(2);

			row[GlbStaffSchema.Constants.GS_Birthdate] = date2;
			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, null);
			AssertEquals(date2, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, date1);
			AssertEquals(date1, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, ZDate.Empty);
			AssertEquals(DBNull.Value, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, date2);
			AssertEquals(date2, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, date3); // ensure there is an overload which works with ZDateTimes on the UXML schema without casts
			AssertEquals(date3.Date, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, GlbStaffSchema.GS_Birthdate, ZDate.Invalid);
			AssertEquals(date2, row[GlbStaffSchema.Constants.GS_Birthdate]);
			AssertEquals("Logs", "Error - Cannot set Invalid Date to Column GlbStaff.GS_Birthdate.", logger.Logs);
		}

		public void TestSetValueInt()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = (ZInt)1;
			var data2 = 2;
			row[DummyBizoSchema.Constants.Z0_Number] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Number, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Number, ZInt.Zero);
			AssertEquals(0, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Number, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Number, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Number, data1, delaySetters);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Number)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Number]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueLong_FromNull()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = 2L;
			row[DummyBizoSchema.Constants.Z0_Long] = data;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Long, null);

			CombineAssertions(() =>
			{
				AssertEquals("Unchanged", data, row[DummyBizoSchema.Constants.Z0_Long]);
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		public void TestSetValueLong_ToZero()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			row[DummyBizoSchema.Constants.Z0_Long] = 2L;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Long, ZLong.Zero);

			CombineAssertions(() =>
			{
				AssertEquals("Set Zero", 0L, row[DummyBizoSchema.Constants.Z0_Long]);
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		public void TestSetValueLong_CastZLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)1;
			row[DummyBizoSchema.Constants.Z0_Long] = data;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Long, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", data, row[DummyBizoSchema.Constants.Z0_Long]);
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		public void TestSetValueLong_DelaySetters()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = 1L;
			var data2 = 2L;
			var delaySetters = new Dictionary<string, ValueSetter>();
			row[DummyBizoSchema.Constants.Z0_Long] = data2;

			CombineAssertions(() =>
			{
				reader.SetValueExposed(row, DummyBizoSchema.Z0_Long, data1, delaySetters);
				AssertEquals("Delayed Set", data2, row[DummyBizoSchema.Constants.Z0_Long]);

				delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Long)].SetValue();
				AssertEquals("Set", data1, row[DummyBizoSchema.Constants.Z0_Long]);
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		public void TestSetValueInt_FromLargeLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)SchemaIntColumn.MaxValue + 2;
			reader.SetValueIntColumnFromLongExposed(row, DummyBizoSchema.Z0_Number, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", (ZInt)SchemaIntColumn.MaxValue, row[DummyBizoSchema.Constants.Z0_Number]);
				AssertContains("Logs", "Field was truncated to the max value.", logger.Logs);
			});
		}

		public void TestSetValueInt_FromSmallLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)SchemaIntColumn.MinValue - 2;
			reader.SetValueIntColumnFromLongExposed(row, DummyBizoSchema.Z0_Number, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", (ZInt)SchemaIntColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Number]);
				AssertContains("Logs", "Field was truncated to the min value.", logger.Logs);
			});
		}

		public void TestSetValueInt_FromValidLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)1234;
			reader.SetValueIntColumnFromLongExposed(row, DummyBizoSchema.Z0_Number, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", 1234, row[DummyBizoSchema.Constants.Z0_Number]);
				AssertEquals("Logs", ZString.Empty, logger.Logs);
			});
		}

		public void TestSetValueShort_FromLargeLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)SchemaShortColumn.MaxValue + 2;
			reader.SetValueShortColumnFromLongExposed(row, DummyBizoSchema.Z0_Short, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", (ZShort)SchemaShortColumn.MaxValue, row[DummyBizoSchema.Constants.Z0_Short]);
				AssertContains("Logs", "Field was truncated to the max value.", logger.Logs);
			});
		}

		public void TestSetValueShort_FromSmallLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)SchemaShortColumn.MinValue - 2;
			reader.SetValueShortColumnFromLongExposed(row, DummyBizoSchema.Z0_Short, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", (ZShort)SchemaShortColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Short]);
				AssertContains("Logs", "Field was truncated to the min value.", logger.Logs);
			});
		}

		public void TestSetValueShort_FromValidLong()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data = (ZLong)567;
			reader.SetValueShortColumnFromLongExposed(row, DummyBizoSchema.Z0_Short, data);

			CombineAssertions(() =>
			{
				AssertEquals("Cast Set", (ZShort)567, row[DummyBizoSchema.Constants.Z0_Short]);
				AssertEquals("Logs", ZString.Empty, logger.Logs);
			});
		}

		public void TestSetValueShort()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = (ZShort)1;
			var data2 = (short)2;
			var data3 = 3;
			var data4 = (ZInt)4;
			row[DummyBizoSchema.Constants.Z0_Short] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, ZShort.Zero);
			AssertEquals((short)0, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data3);
			AssertEquals((short)3, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data4);
			AssertEquals((short)4, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			data4 = SchemaShortColumn.MaxValue + 2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data4);
			AssertEquals((short)SchemaShortColumn.MaxValue, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "Warning - Attempted to insert '32769' into Field [Z0_Short] which has a maximum numeric value of '32767'. Field was truncated to the max value.", logger.Logs);

			logger.ClearLogs();
			data4 = SchemaShortColumn.MinValue - 3;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data4);
			AssertEquals((short)SchemaShortColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "Warning - Attempted to insert '-32771' into Field [Z0_Short] which has a minimum numeric value of '-32768'. Field was truncated to the min value.", logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data4, delaySetters);
			AssertEquals((short)SchemaShortColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Short)].SetValue();
			AssertEquals((short)SchemaShortColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "Warning - Attempted to insert '-32771' into Field [Z0_Short] which has a minimum numeric value of '-32768'. Field was truncated to the min value.", logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Short, data2, delaySetters);
			AssertEquals((short)SchemaShortColumn.MinValue, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Short)].SetValue();
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_Short]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueDecimal()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = (ZDecimal)1;
			var data2 = 2m;
			var data3 = new ZDecimal(32232123121446554654546m);
			row[DummyBizoSchema.Constants.Z0_AnotherDecimal] = data2;
			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, null);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, ZDecimal.Zero);
			AssertEquals(0m, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, data1);
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, data2);
			AssertEquals(data2, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, data3);
			AssertEquals(999999999999999m, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", string.Format("Warning - Attempted to insert '{0}' into Field [{1}] which has a maximum numeric value of '999999999999999'. Field was truncated to the max value.", data3, DummyBizoSchema.Constants.Z0_AnotherDecimal), logger.Logs);

			logger.ClearLogs();
			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, data3, delaySetters);
			AssertEquals(999999999999999m, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_AnotherDecimal)].SetValue();
			AssertEquals(999999999999999m, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", string.Format("Warning - Attempted to insert '{0}' into Field [{1}] which has a maximum numeric value of '999999999999999'. Field was truncated to the max value.", data3, DummyBizoSchema.Constants.Z0_AnotherDecimal), logger.Logs);

			logger.ClearLogs();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_AnotherDecimal, data1, delaySetters);
			AssertEquals(999999999999999m, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_AnotherDecimal)].SetValue();
			AssertEquals(data1, row[DummyBizoSchema.Constants.Z0_AnotherDecimal]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestSetValueICodeDataObject()
		{
			var row = (IColumnIndexer)RowFactory.New(DummyBizoSchema.Constants.TableName);
			var data1 = new Product { Code = "BOB", Description = "BOB THE BUILDER" };
			var data2 = new Currency { Code = "aud", Description = "Australian, Dollars" };
			row[DummyBizoSchema.Constants.Z0_Code] = "AUD";
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, (ICodeDataObject)null);
			AssertEquals("AUD", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, data1);
			AssertEquals("BOB", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);

			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, data2);
			AssertEquals("AUD", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);

			var delaySetters = new Dictionary<string, ValueSetter>();
			reader.SetValueExposed(row, DummyBizoSchema.Z0_Code, data1, delaySetters);
			AssertEquals("AUD", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);

			delaySetters[ColumnValueSetter.GetKey(row.GetValue(DummyBizoSchema.PK), DummyBizoSchema.Z0_Code)].SetValue();
			AssertEquals("BOB", row[DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Logs", "", logger.Logs);
		}

		public void TestGetValueCustomizedField()
		{
			IZType expectedValue = ZBool.True;
			var field = CustomizedField.New("Bool", expectedValue);
			AssertEquals("true", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = ZDateTime.BrettsBirthday;
			field = CustomizedField.New("Date", expectedValue);
			AssertEquals(ZDateTime.BrettsBirthday.ToISO8601String(), field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = (ZDecimal)123.456m;
			field = CustomizedField.New("Decimal", expectedValue);
			AssertEquals("123.456", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = (ZInt)589;
			field = CustomizedField.New("Int", expectedValue);
			AssertEquals("589", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = (ZShort)236;
			field = CustomizedField.New("Short", expectedValue);
			AssertEquals("236", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = (ZByte)135;
			field = CustomizedField.New("Byte", expectedValue);
			AssertEquals("135", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));

			expectedValue = (ZString)"Hello";
			field = CustomizedField.New("String", expectedValue);
			AssertEquals("Hello", field.Value.GetValueOrDefault());
			AssertEquals(expectedValue, reader.GetValueExposed(field));
		}

		#region TestIgnoreInactiveTargetForMatching

		public void TestIgnoreInactiveTargetForMatching()
		{
			var reader = new CancellableDataObjectReader(new DummyDataObject(), logger, Factory);
			var bizo = reader.ReadIntoBusinessObject();
			bizo.IsCancelled = true;

			var readBizoAgain = reader.ReadIntoBusinessObject();
			AssertEquals("Should read into the same bizo", bizo, readBizoAgain);

			using (DataObjectReader.IgnoreInactiveTargetForMatching())
			{
				var readBizoIgnoringInactiveTargetForMatching = reader.ReadIntoBusinessObject();
				AssertNotEquals("Should not read cancelled bizo", bizo, readBizoIgnoringInactiveTargetForMatching);
			}
		}

		class CancellableBusinessObject : DummyBusinessObject, ICancellable
		{
			public CancellableBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsCancelled { get; set; }

			public bool IsCancelledHasChanged => throw new NotImplementedException();

			public string CanCancel() => throw new NotImplementedException();

			public string CanReactivate() => throw new NotImplementedException();
		}

		class CancellableDataObjectReader : DataObjectReader<DummyDataObject, CancellableBusinessObject>
		{
			public CancellableDataObjectReader(DummyDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(dataObject, logger, factory)
			{
				existingBizo = factory.BOFactory.New<CancellableBusinessObject>();
			}

			readonly CancellableBusinessObject existingBizo;

			protected override CancellableBusinessObject GetExistingBusinessObject()
			{
				return existingBizo;
			}

			protected override void PopulateBusinessObject(CancellableBusinessObject targetBO)
			{
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			reader = new DataObjectReaderTestClass(logger, Factory);
		}
		TestErrorLogger logger;
		DataObjectReaderTestClass reader;

		#region TestLogMessage

		public void TestLogMessage()
		{
			var logger = new TestErrorLogger();

			var reader = new DummyDataReader(new DummyDataObject(), logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertEquals("Information - Log for PopulateBusinessObject.", logger.Logs);
		}

		class DummyDataObject : IDataObject
		{ }

		class DummyDataReader : DataObjectReader<DummyDataObject, DummyBusinessObject>
		{
			public DummyDataReader(DummyDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(dataObject, logger, factory)
			{
			}

			protected override DummyBusinessObject GetExistingBusinessObject()
			{
				return factory.New<DummyBusinessObject>();
			}

			protected override void PopulateBusinessObject(DummyBusinessObject targetBO)
			{
				logger.Log(Enterprise.Integration.LogType.Information, "Log for PopulateBusinessObject.");
			}

			protected override void LogSuccessfullyLoadedMessage(string typeName)
			{
			}

			protected override void LogPopulatingMessage(string typeName)
			{
			}
		}

		#endregion

		RowFactory RowFactory
		{
			get { return Factory.RowFactory; }
		}

		class DataObjectReaderTestClass : DataObjectReader
		{
			public DataObjectReaderTestClass(IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(logger)
			{
			}

			public void SetValueExposed(IColumnIndexer row, SchemaStringColumn column, ZString? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaGuidColumn column, ZGuid? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaBoolColumn column, ZBool? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaBinaryColumn column, ZBlob? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaByteColumn column, ZByte? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaDateColumn column, ZDate? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaDateColumn column, ZDateTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaDateTimeColumn column, ZDateTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaDateTimeOffsetColumn column, ZDateTimeOffset? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}
			public void SetValueExposed(IColumnIndexer row, SchemaTimeColumn column, ZTime? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaGeographyColumn column, ZGeography? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaIntColumn column, ZInt? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaLongColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueIntColumnFromLongExposed(IColumnIndexer row, SchemaIntColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueShortColumnFromLongExposed(IColumnIndexer row, SchemaShortColumn column, ZLong? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaShortColumn column, ZInt? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaShortColumn column, ZShort? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaDecimalColumn column, ZDecimal? valueSource, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, valueSource, delaySetters);
			}

			public void SetValueExposed(IColumnIndexer row, SchemaStringColumn column, ICodeDataObject property, Dictionary<string, ValueSetter> delaySetters = null)
			{
				SetValue(row, column, property, delaySetters);
			}

			public IZType GetValueExposed(CustomizedField customFieldDataObject)
			{
				return base.GetValue(customFieldDataObject);
			}
		}

		class DerivedDummyBusinessObject : AutoDummyBizo
		{
			public DerivedDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[MaxLength(3)]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			[MaxLength(1)]
			public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }
		}
	}
}
