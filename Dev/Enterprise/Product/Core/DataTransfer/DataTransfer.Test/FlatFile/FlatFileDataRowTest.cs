using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	public class FlatFileDataRowTest : TransactionedTestCase
	{
		#region SetField

		public void TestSetUsingSystemDecimalWithDecimalPlaces()
		{
			decimal val = 563.2352m;
			DataRow.SetField(0, val, 2);
			AssertEquals("563.24", DataRow[0]);
		}

		public void TestSetUsingSystemInt()
		{
			int val = 500;
			DataRow.SetField(0, val);
			AssertEquals("500", DataRow.GetField(0));
		}

		public void TestSetUsingSystemDecimal()
		{
			decimal val = 500.32m;
			DataRow.SetField(0, val);
			AssertEquals("500.32", DataRow.GetField(0));
		}

		public void TestSetUsingZDateTime()
		{
			ZDateTime now = ZDateTime.Now;
			DataRow.SetField(0, now, String.Empty);
			AssertEquals(now.ToString(), DataRow.GetField(0));
		}

		public void TestSetUsingZDateTimeOffset()
		{
			ZDateTimeOffset now = ZDateTimeOffset.Now;
			DataRow.SetField(0, now, String.Empty);
			AssertEquals(now.ToString(), DataRow.GetField(0));
		}

		public void TestSetUsingSystemTimeSpan()
		{
			var now = new ZTime(1, 2).ToTimeSpan();
			DataRow.SetField(0, now, String.Empty);
			AssertEquals(now.ToString(ZTime.TimeFormat), DataRow.GetField(0));
		}

		public void TestSetUsingZTime()
		{
			var now = new ZTime(1, 2);
			DataRow.SetField(0, now, String.Empty);
			AssertEquals(now.ToString(ZTime.TimeFormat), DataRow.GetField(0));
		}

		public void TestSetUsingGeography()
		{
			var geo = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars("POINT (121 47)"), 4326);
			DataRow.SetField(0, geo);
			AssertEquals(geo.AsTextZM().ToSqlString().ToString(), DataRow.GetField(0));
		}

		public void TestSetUsingZGeography()
		{
			ZGeography geo = new ZGeography("POINT (-121 48)");
			DataRow.SetField(0, geo);
			AssertEquals(geo.ToString(), DataRow.GetField(0));
		}

		public void TestFlatFileDataSetField()
		{
			string testValue = "HELLOASL";
			DataRow.SetField(3, testValue);
			AssertEquals("Did not equal the set test value", testValue, DataRow.InternalValueArray[3]);
		}

		public void TestSetIndexer()
		{
			DataRow[1] = "ABC";
			AssertEquals("setting Column 1 should have new value", "ABC", DataRow.InternalValueArray[1]);

			DataRow[1] = "123";
			AssertEquals("setting Column 1 should have new value", "123", DataRow.InternalValueArray[1]);
		}

		#endregion

		#region GetField

		public void TestGetFieldIndexOverrunReturnsEmpty()
		{
			AssertEquals(String.Empty, DataRow.GetField(5));
		}

		public void TestGetFieldAsInteger()
		{
			string validInteger = "566";
			DataRow[0] = validInteger;
			AssertEquals(566, DataRow.GetFieldAsZInt(0));

			string invalidInteger = "564abc";
			DataRow[1] = invalidInteger;
			AssertEquals(0, DataRow.GetFieldAsZInt(1));

			AssertEquals(0, DataRow.GetFieldAsZInt(2));
		}

		public void TestGetFieldAsDecimal()
		{
			string validDecimal = "564.16";
			DataRow[0] = validDecimal;
			AssertEquals(564.16m, DataRow.GetFieldAsZDecimal(0));
			AssertEquals(564.2m, DataRow.GetFieldAsZDecimal(0, 1));

			string invalidDecimal = "564.1abc";
			DataRow[1] = invalidDecimal;
			AssertEquals(0m, DataRow.GetFieldAsZDecimal(1));

			AssertEquals(0m, DataRow.GetFieldAsZDecimal(2));
		}

		public void TestGetFieldAsDecimalWithCulture()
		{
			string validDecimal = "564,16";
			DataRow[0] = validDecimal;
			AssertEquals(564.16m, DataRow.GetFieldAsZDecimal(0, new CultureInfo("es-AR")));
			AssertEquals(0m, DataRow.GetFieldAsZDecimal(1, new CultureInfo("en-AU")));

			string validDecimalSimbolSeparator = "564.16";
			DataRow[1] = validDecimalSimbolSeparator;
			AssertEquals(0m, DataRow.GetFieldAsZDecimal(1, new CultureInfo("es-AR")));
			AssertEquals(564.16m, DataRow.GetFieldAsZDecimal(1, new CultureInfo("en-AU")));
		}

		public void TestGetFieldAsZDateTime()
		{
			DataRow = new FlatFileDataRowTestClass(14);
			DataRow.SetField(0, "250198");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(0, "ddMMyy"));

			DataRow.SetField(1, "259801");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(1, "ddyyMM"));

			DataRow.SetField(2, "982501");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(2, "yyddMM"));

			DataRow.SetField(3, "25/01/98");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(3, "dd/MM/yy"));

			DataRow.SetField(4, "25/98/01");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(4, "dd/yy/MM"));

			DataRow.SetField(5, "98/25/01");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(5, "yy/dd/MM"));

			DataRow.SetField(6, "25/01/2098");
			AssertEquals(new ZDateTime(2098, 01, 25), DataRow.GetFieldAsZDateTime(6, "dd/MM/yyyy"));

			DataRow.SetField(7, "25/2098/01");
			AssertEquals(new ZDateTime(2098, 01, 25), DataRow.GetFieldAsZDateTime(7, "dd/yyyy/MM"));

			DataRow.SetField(8, "2098/25/01");
			AssertEquals(new ZDateTime(2098, 01, 25), DataRow.GetFieldAsZDateTime(8, "yyyy/dd/MM"));

			DataRow.SetField(9, "25-01-98");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(9, "dd-MM-yy"));

			DataRow.SetField(10, "25-98-01");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(10, "dd-yy-MM"));

			DataRow.SetField(11, "98-25-01");
			AssertEquals(new ZDateTime(1998, 01, 25), DataRow.GetFieldAsZDateTime(11, "yy-dd-MM"));

			DataRow.SetField(12, "");
			AssertEquals(ZDateTime.Empty, DataRow.GetFieldAsZDateTime(12, "yyddMM"));

			DataRow.SetField(13, "LOLASJS");
			AssertEquals(ZDateTime.Empty, DataRow.GetFieldAsZDateTime(13, "POPGOESTHEWORLD"));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetFieldAsZDateTimeOffset()
		{
			DataRow = new FlatFileDataRowTestClass(14);
			DataRow.SetField(0, "1998-01-25T01:02:03.123+08:00");
			AssertEquals(new ZDateTimeOffset(1998, 01, 25, 01, 02, 03, 123, TimeSpan.FromHours(8)), DataRow.GetFieldAsZDateTimeOffset(0, "yyyy-MM-ddTHH:mm:ss.fffzzzzzzz"));
			DataRow.SetField(0, "1998-01-25T01:02:03.123+00:00");
			AssertEquals(new ZDateTimeOffset(1998, 01, 25, 01, 02, 03, 123, TimeSpan.Zero), DataRow.GetFieldAsZDateTimeOffset(0, "yyyy-MM-ddTHH:mm:ss.fffzzzzzzz"));
		}

		public void TestGetFieldAsZDateTime_DateTimeEmpty()
		{
			DataRow = new FlatFileDataRowTestClass(1);
			DataRow.SetField(0, "");
			AssertEquals(DateTime.MinValue, DataRow.GetFieldAsZDateTime(0, "ddMMyy"));

			DataRow.SetField(0, (string)null);
			AssertEquals(DateTime.MinValue, DataRow.GetFieldAsZDateTime(0, "ddMMyy"));
		}

		public void TestGetFieldAsZDateTimeOffset_DateTimeOffsetEmpty()
		{
			DataRow = new FlatFileDataRowTestClass(1);
			DataRow.SetField(0, "");
			AssertEquals(DateTimeOffset.MinValue, DataRow.GetFieldAsZDateTimeOffset(0, "ddMMyy"));

			DataRow.SetField(0, (string)null);
			AssertEquals(DateTimeOffset.MinValue, DataRow.GetFieldAsZDateTimeOffset(0, "ddMMyy"));
		}

		public void TestGetFieldAsZTime_TimeEmpty()
		{
			DataRow = new FlatFileDataRowTestClass(1);
			DataRow.SetField(0, "");
			AssertEquals(TimeSpan.Zero, DataRow.GetFieldAsZTime(0, ZTime.TimeFormat));

			DataRow.SetField(0, (string)null);
			AssertEquals(TimeSpan.Zero, DataRow.GetFieldAsZTime(0, ZTime.TimeFormat));
		}

		public void TestGetFieldAsZGeography_GeographyEmpty()
		{
			DataRow = new FlatFileDataRowTestClass(1);
			DataRow.SetField(0, "");
			AssertEquals(ZGeography.Empty, DataRow.GetFieldAsZGeography(0));

			DataRow.SetField(0, (string)null);
			AssertEquals(ZGeography.Empty, DataRow.GetFieldAsZGeography(0));
		}

		public void TestGetFieldAsZShort()
		{
			string validShort = "56";
			DataRow[0] = validShort;
			AssertEquals(new ZShort((short)56), DataRow.GetFieldAsZShort(0));

			string invalidShort = "564abc";
			DataRow[1] = invalidShort;
			AssertEquals(ZShort.Zero, DataRow.GetFieldAsZShort(1));

			AssertEquals(ZShort.Zero, DataRow.GetFieldAsZShort(2));
		}

		public void TestGetFieldAsZBool()
		{
			string validBool = "Y";
			DataRow[0] = validBool;
			AssertEquals(new ZBool("Y"), DataRow.GetFieldAsZBool(0));

			validBool = "y";
			DataRow[0] = validBool;
			AssertEquals(new ZBool("Y"), DataRow.GetFieldAsZBool(0));

			validBool = "N";
			DataRow[0] = validBool;
			AssertEquals(new ZBool("N"), DataRow.GetFieldAsZBool(0));

			validBool = "n";
			DataRow[0] = validBool;
			AssertEquals(new ZBool("N"), DataRow.GetFieldAsZBool(0));

			validBool = "";
			DataRow[0] = validBool;
			AssertEquals(new ZBool(""), DataRow.GetFieldAsZBool(0));

			validBool = "   ";
			DataRow[0] = validBool;
			AssertEquals(new ZBool(""), DataRow.GetFieldAsZBool(0));

			string invalidBool = "Z";
			DataRow[0] = invalidBool;
			AssertEquals(false, DataRow.GetFieldAsZBool(0));

			invalidBool = "Yes";
			DataRow[0] = invalidBool;
			AssertEquals(false, DataRow.GetFieldAsZBool(0));
		}

		public void TestFlatFileDataGetField()
		{
			string testValue = "HELLOASL";
			DataRow.InternalValueArray[3] = testValue;
			AssertEquals("Did not get the correct field", testValue, DataRow.GetField(3));
		}

		public void TestGetIndexer()
		{
			DataRow.InternalValueArray[1] = "ABC";
			AssertEquals("getting via indexer should return correct value", "ABC", DataRow[1]);

			DataRow.InternalValueArray[1] = "123";
			AssertEquals("getting via indexer should return correct value", "123", DataRow[1]);
		}

		#endregion

		public void TestDecimalPlacesAreRoundedUpAndDown()
		{
			DataRow.SetField(0, 32.0000m, 2);
			ZDecimal result = DataRow.GetFieldAsZDecimal(0, 2);
			AssertEquals("Result should be 32.00", 32.00m, result);
			AssertEquals("Result as string", "32.00", result.ToString());

			DataRow.SetField(0, 32m, 2);
			result = DataRow.GetFieldAsZDecimal(0, 2);
			AssertEquals("Result should be 32.00", 32.00m, result);
			AssertEquals("Result as string", "32.00", result.ToString());

			result = DataRow.GetFieldAsZDecimal(0, 1);
			AssertEquals("Result should be 32.00", 32.0m, result);
			AssertEquals("Result as string", "32.0", result.ToString());

			DataRow.SetField(0, 43.2346m, 3);
			result = DataRow.GetFieldAsZDecimal(0, 3);
			AssertEquals("Result should be", 43.235m, result);
			AssertEquals("Result as string", "43.235", result.ToString());

			DataRow.SetField(0, 43.2345m, 3);
			result = DataRow.GetFieldAsZDecimal(0, 3);
			AssertEquals("Result should be", 43.235m, result);
			AssertEquals("Result as string", "43.235", result.ToString());
		}

		public void TestClone()
		{
			string[] values = new string[] { "hello", "world", "!" };
			FlatFileDataRow row = new FlatFileDataRow(values);
			var cloneOfRow = row.Clone();

			AssertEquals(row[0], cloneOfRow[0]);
			AssertEquals(row[1], cloneOfRow[1]);
			AssertEquals(row[2], cloneOfRow[2]);

			row.SetField(2, "asl");
			Assert(cloneOfRow.GetField(2) != "asl");

			Assert("CloneOfRow should of had values", cloneOfRow.FieldCount != 0);
		}

		public void TestConstructor()
		{
			FlatFileDataRow dataRow = new FlatFileDataRow(2);
			AssertEquals(2, dataRow.FieldCount);

			dataRow = new FlatFileDataRow(new string[] { "Hello World", "Hello People" });
			AssertEquals(2, dataRow.FieldCount);
			AssertEquals("Hello World", dataRow[0]);
			AssertEquals("Hello People", dataRow[1]);

			FlatFileDataRow dataRow2 = new FlatFileDataRow(dataRow);
			AssertEquals(2, dataRow2.FieldCount);
			AssertEquals("Hello World", dataRow2[0]);
			AssertEquals("Hello People", dataRow2[1]);
		}

		public void TestIsEmpty_Empty()
		{
			FlatFileDataRow row = new FlatFileDataRow(2);
			row.SetField(0, String.Empty);

			AssertEquals(true, row.IsEmpty);
		}

		public void TestIsEmpty_NotEmpty()
		{
			FlatFileDataRow row = new FlatFileDataRow(2);
			row.SetField(0, "ASL");

			AssertEquals(false, row.IsEmpty);
		}

		public void TestFlatFileDataRowDataArrayLength()
		{
			AssertEquals("Should have 5 fields", 5, DataRow.FieldCount);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			if (!String.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
			DataRow = new FlatFileDataRowTestClass(5);
		}
		protected ZString StoredCountry;

		protected override void TearDown()
		{
			base.TearDown();
			if (!String.IsNullOrEmpty(StoredCountry))
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}
		}

		protected virtual ZString TestingCountry
		{
			get { return null; }
		}

		FlatFileDataRowTestClass DataRow;

		class FlatFileDataRowTestClass : FlatFileDataRow
		{
			public FlatFileDataRowTestClass(int numberOfFields)
				: base(numberOfFields)
			{
			}

			public string[] InternalValueArray
			{
				get { return base.DataRow; }
			}
		}

		#endregion
	}
}
