using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class SharedFlatFileDataRowTest : TestCase
	{
		public void TestSetField()
		{
			SharedFlatFileDataRowTestClass row = new SharedFlatFileDataRowTestClass();

			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field1, 123456.7890m);
			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field2, new ZDateTime(2006, 2, 8, 17, 12, 42));
			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field3, "5 Level 2 Cote Road");
			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field4, (ZInt)69);

			AssertEquals("Field 1", 123456.79m, row.GetFieldAsZDecimal(SharedFlatFileDataRowTestClass.Schema.Field1.Name, 2));
			AssertEquals("Field 2", new ZDateTime(2006, 2, 8), row.GetFieldAsZDateTime(SharedFlatFileDataRowTestClass.Schema.Field2.Name, "dd/MM/yyyy"));
			AssertEquals("Field 3", "5 Level 2 Co", row.GetField(SharedFlatFileDataRowTestClass.Schema.Field3.Name));
			AssertEquals("Field 4", 69, row.GetFieldAsZInt(SharedFlatFileDataRowTestClass.Schema.Field4.Name));

			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field4, true);
			AssertEquals("Field 4", "Y", row.GetField(SharedFlatFileDataRowTestClass.Schema.Field4));

			AssertEquals("Length", 32, row.Length);
		}

		public void TestAlternateConstructor()
		{
			String dataRow = "123456.7890,08/02/2006,5 Level 2 Cote Road,69";
			FlatFileFormat format = new CsvFlatFileFormat();
			SharedFlatFileDataRowTestClass row = new SharedFlatFileDataRowTestClass(format.ConvertToRow(dataRow));

			AssertEquals("Field 1", 123456.79m, row.GetFieldAsZDecimal(SharedFlatFileDataRowTestClass.Schema.Field1.Name, 2));
			AssertEquals("Field 2", new ZDateTime(2006, 2, 8), row.GetFieldAsZDateTime(SharedFlatFileDataRowTestClass.Schema.Field2.Name, "dd/MM/yyyy"));
			AssertEquals("Field 3", "5 Level 2 Cote Road", row.GetField(SharedFlatFileDataRowTestClass.Schema.Field3.Name));
			AssertEquals("Field 4", 69, row.GetFieldAsZInt(SharedFlatFileDataRowTestClass.Schema.Field4.Name));
		}

		public void TestStoringDecimalsAsInts()
		{
			SharedFlatFileDataRowTestClass row = new SharedFlatFileDataRowTestClass();
			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field1, "1234");
			AssertEquals("Field 1 has 2 decimal places", 12.34m, row.GetIntFieldAsZDecimal(SharedFlatFileDataRowTestClass.Schema.Field1, 2));

			row.SetZDecimalFieldAsInt(SharedFlatFileDataRowTestClass.Schema.Field1, 12.34m, 2);
			AssertEquals("Field 1 stored as integer style string", "1234", row.GetField(SharedFlatFileDataRowTestClass.Schema.Field1));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestSetFieldDecimalPlaces()
		{
			SharedFlatFileDataRowTestClass row = new SharedFlatFileDataRowTestClass();
			row.SetField(SharedFlatFileDataRowTestClass.Schema.Field1, 55, -1);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+BadDataRowForTest.Rubbish has too many 'Attributes' - it should only have one.")]
		public void TestBadDataRow()
		{
			BadDataRowForTest badRow = new BadDataRowForTest();
		}

		[ExpectExceptionMessage(typeof(InvalidCastException), "Property field attribute type mismatch exception.\r\n\r\nProperty Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+DecimalTypeMismatchDataRowForTest.Cost type CargoWise.Types.ZDecimal and attribute type Enterprise.ClientSharedComponents.FieldAttribute are mismatched.\r\nExpecting field attribute type ZDecimal, DecimalAsInt or DecimalAsString.\r\n")]
		public void TestDecimalTypeMismatchDataRow()
		{
			DecimalTypeMismatchDataRowForTest badRow = new DecimalTypeMismatchDataRowForTest();
			badRow.Cost = 6.5m;
			badRow.PopulateFields();
		}

		[ExpectExceptionMessage(typeof(InvalidCastException), "Property field attribute type mismatch exception.\r\n\r\nProperty Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+StringTypeMismatchDataRowForTest.Name type CargoWise.Types.ZString and attribute type Enterprise.ClientSharedComponents.DecimalFieldAttribute are mismatched.\r\nExpecting field attribute type ZString.\r\n")]
		public void TestStringTypeMismatchDataRow()
		{
			StringTypeMismatchDataRowForTest badRow = new StringTypeMismatchDataRowForTest();
			badRow.Name = "Jessica";
			badRow.PopulateFields();
		}

		[ExpectExceptionMessage(typeof(InvalidCastException), "Property field attribute type mismatch exception.\r\n\r\nProperty Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+IntTypeMismatchDataRowForTest.Age type CargoWise.Types.ZInt and attribute type Enterprise.ClientSharedComponents.FieldAttribute are mismatched.\r\nExpecting field attribute type ZInt.\r\n")]
		public void TestIntTypeMismatchDataRow()
		{
			IntTypeMismatchDataRowForTest badRow = new IntTypeMismatchDataRowForTest();
			badRow.Age = 6;
			badRow.PopulateFields();
		}

		[ExpectExceptionMessage(typeof(InvalidCastException), "Property field attribute type mismatch exception.\r\n\r\nProperty Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+DateTimeTypeMismatchDataRowForTest.When type CargoWise.Types.ZDateTime and attribute type Enterprise.ClientSharedComponents.FieldAttribute are mismatched.\r\nExpecting field attribute type ZDateTime.\r\n")]
		public void TestDateTimeTypeMismatchDataRow()
		{
			DateTimeTypeMismatchDataRowForTest badRow = new DateTimeTypeMismatchDataRowForTest();
			badRow.When = ZDateTime.Now;
			badRow.PopulateFields();
		}

		[ExpectExceptionMessage(typeof(InvalidCastException), "Property field attribute type mismatch exception.\r\n\r\nProperty Enterprise.ClientSharedComponents.DataTransfer.Testing.SharedFlatFileDataRowTest+BoolTypeMismatchDataRowForTest.IsNow type CargoWise.Types.ZBool and attribute type Enterprise.ClientSharedComponents.FieldAttribute are mismatched.\r\nExpecting field attribute type ZBool.\r\n")]
		public void TestBoolTypeMismatchDataRow()
		{
			BoolTypeMismatchDataRowForTest badRow = new BoolTypeMismatchDataRowForTest();
			badRow.IsNow = true;
			badRow.PopulateFields();
		}

		public class BadDataRowForTest : SharedFlatFileDataRow
		{
			public BadDataRowForTest() : base() { }

			[Field(10)]
			[Field(3)]
			public ZString Rubbish { get; set; }
		}

		public class DecimalTypeMismatchDataRowForTest : SharedFlatFileDataRow
		{
			[Field(1)]
			public ZDecimal Cost { get; set; }
		}

		public class StringTypeMismatchDataRowForTest : SharedFlatFileDataRow
		{
			[DecimalField(5, 1)]
			public ZString Name { get; set; }
		}

		public class IntTypeMismatchDataRowForTest : SharedFlatFileDataRow
		{
			[Field(1)]
			public ZInt Age { get; set; }
		}

		public class DateTimeTypeMismatchDataRowForTest : SharedFlatFileDataRow
		{
			[Field(1)]
			public ZDateTime When { get; set; }
		}

		public class BoolTypeMismatchDataRowForTest : SharedFlatFileDataRow
		{
			[Field(1)]
			public ZBool IsNow { get; set; }
		}
	}
}
