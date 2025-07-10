using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;

namespace CargoWise.EntityFramework.Extensions.Testing
{
	sealed class DataIndexerExtensionsTest : TestCaseWithFactory
	{
		public void TestGetValue_ZString()
		{
			var stringValue = "HELLO";
			row[DummyBizoSchema.Constants.Z0_Description] = stringValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", stringValue, row.GetValue(DummyBizoSchema.Z0_Description));
				AssertEquals("SchemaColumn", stringValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Description));
			});
		}

		public void TestGetValue_ZByte()
		{
			var byteValue = (byte)15;
			row[DummyBizoSchema.Constants.Z0_Byte] = byteValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", byteValue, row.GetValue(DummyBizoSchema.Z0_Byte));
				AssertEquals("SchemaColumn", byteValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Byte));
			});
		}

		public void TestGetValue_ZDecimal()
		{
			var decimalValue = 598.64m;
			row[DummyBizoSchema.Constants.Z0_Decimal] = decimalValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", decimalValue, row.GetValue(DummyBizoSchema.Z0_Decimal));
				AssertEquals("SchemaColumn", decimalValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Decimal));
			});
		}

		public void TestGetValue_ZBool()
		{
			var boolValue = true;
			row[DummyBizoSchema.Constants.Z0_Bool] = boolValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", ZBool.True, row.GetValue(DummyBizoSchema.Z0_Bool));
				AssertEquals("SchemaColumn", ZBool.True, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Bool));
			});
		}

		public void TestGetValue_ZDate()
		{
			var dateValue = ZDate.BrettsBirthday.ToDateTime();
			row[DummyBizoSchema.Constants.Z0_DateOnly] = dateValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", dateValue, row.GetValue(DummyBizoSchema.Z0_DateOnly));
				AssertEquals("SchemaColumn", dateValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_DateOnly));
			});
		}

		public void TestGetValue_ZDateTime()
		{
			var dateTimeValue = ZDateTime.BrettsBirthday.ToDateTime();
			row[DummyBizoSchema.Constants.Z0_Date] = dateTimeValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", dateTimeValue, row.GetValue(DummyBizoSchema.Z0_Date));
				AssertEquals("SchemaColumn", dateTimeValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Date));
			});
		}

		public void TestGetValue_ZTime()
		{
			var dateTimeValue = new ZTime(12, 34).ToTimeSpan();
			row[DummyBizoSchema.Constants.Z0_Time] = dateTimeValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", dateTimeValue, row.GetValue(DummyBizoSchema.Z0_Time));
				AssertEquals("SchemaColumn", dateTimeValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Time));
			});
		}

		public void TestGetValue_ZDateTimeOffset()
		{
			var dateTimeOffsetValue = ZDateTimeOffset.Today.ToDateTimeOffset();
			row[DummyBizoSchema.Constants.Z0_DateTimeOffset] = dateTimeOffsetValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", dateTimeOffsetValue, row.GetValue(DummyBizoSchema.Z0_DateTimeOffset));
				AssertEquals("SchemaColumn", dateTimeOffsetValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_DateTimeOffset));
			});
		}

		public void TestGetValue_ZGeography()
		{
			var geographyValue = SqlGeography.STGeomFromText(new SqlChars("POINT (-121 48)"), 4326);
			row[DummyBizoSchema.Constants.Z0_Geography] = geographyValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", geographyValue, row.GetValue(DummyBizoSchema.Z0_Geography));
				AssertEquals("SchemaColumn", geographyValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Geography));
			});
		}

		public void TestGetValue_ZInt()
		{
			var intValue = 895;
			row[DummyBizoSchema.Constants.Z0_Number] = intValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", intValue, row.GetValue(DummyBizoSchema.Z0_Number));
				AssertEquals("SchemaColumn", intValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Number));
			});
		}

		public void TestGetValue_ZShort()
		{
			var shortValue = (short)56;
			row[DummyBizoSchema.Constants.Z0_Short] = shortValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", shortValue, row.GetValue(DummyBizoSchema.Z0_Short));
				AssertEquals("SchemaColumn", shortValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Short));
			});
		}

		public void TestGetValue_ZLong()
		{
			var longValue = 23456789L;
			row[DummyBizoSchema.Constants.Z0_Long] = longValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", longValue, row.GetValue(DummyBizoSchema.Z0_Long));
				AssertEquals("SchemaColumn", longValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Long));
			});
		}

		public void TestGetValue_ZGuid()
		{
			var guid = Guid.NewGuid();
			row[DummyBizoSchema.Constants.Z0_Guid] = guid;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", guid, row.GetValue(DummyBizoSchema.Z0_Guid));
				AssertEquals("SchemaColumn", guid, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_Guid));
			});
		}

		public void TestGetValue_ZBlob()
		{
			var blobValue = System.Text.Encoding.ASCII.GetBytes("HELLO");
			row[DummyBizoSchema.Constants.Z0_VarBinaryMax] = blobValue;
			CombineAssertions(() =>
			{
				AssertEquals("Typed Column", blobValue, row.GetValue(DummyBizoSchema.Z0_VarBinaryMax));
				AssertEquals("SchemaColumn", blobValue, row.GetValue((SchemaColumn)DummyBizoSchema.Z0_VarBinaryMax));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			row = (IColumnIndexer)((IBusinessObjectInternals)Factory.New<DummyBusinessObject>()).Row;
		}
		IColumnIndexer row;
	}
}
