using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DataVersionLogValueFormatterTest : TestCaseWithFactory
	{
		public void TestGetStringFromPropertyValue()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_AnotherDecimal = 10.23M;
			bizo.Z0_Bool = true;

			var row = ((INeedRow)bizo).Row;

			AssertEquals("Original value of Z0_AnotherDecimal is expected", "10.23", Formatter.GetStringFromPropertyValue(bizo.Z0_AnotherDecimalInfo, row, true));
			AssertEquals("Original value for Z0_Bool is expected", "Y", Formatter.GetStringFromPropertyValue(bizo.Z0_BoolInfo, row, true));
			AssertEquals("Original value for Z0_Money is expected", "0", Formatter.GetStringFromPropertyValue(bizo.Z0_MoneyInfo, row, true));

			AssertEquals("Z0_AnotherDecimal", "10.23", Formatter.GetStringFromPropertyValue(bizo.Z0_AnotherDecimalInfo, row));
			AssertEquals("Z0_Bool", "Y", Formatter.GetStringFromPropertyValue(bizo.Z0_BoolInfo, row));
			AssertEquals("Z0_Money", "0", Formatter.GetStringFromPropertyValue(bizo.Z0_MoneyInfo, row));

			Factory.Save();
			bizo.Z0_AnotherDecimal = 10.33M;
			bizo.Z0_Bool = false;

			AssertEquals("Original value of Z0_AnotherDecimal is expected", "10.23", Formatter.GetStringFromPropertyValue(bizo.Z0_AnotherDecimalInfo, row, true));
			AssertEquals("Original value for Z0_Bool is expected", "Y", Formatter.GetStringFromPropertyValue(bizo.Z0_BoolInfo, row, true));
			AssertEquals("Original value for Z0_Money is expected", "0", Formatter.GetStringFromPropertyValue(bizo.Z0_MoneyInfo, row, true));

			AssertEquals("Z0_AnotherDecimal", "10.33", Formatter.GetStringFromPropertyValue(bizo.Z0_AnotherDecimalInfo, row));
			AssertEquals("Z0_Bool", "N", Formatter.GetStringFromPropertyValue(bizo.Z0_BoolInfo, row));
			AssertEquals("Z0_Money", "0", Formatter.GetStringFromPropertyValue(bizo.Z0_MoneyInfo, row));
		}

		public void TestGetStringFromPropertyValueForZDateTime()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_Date = ZDateTime.Empty;

			var row = ((INeedRow)bizo).Row;

			AssertEquals("Empty value", "", Formatter.GetStringFromPropertyValue(bizo.Z0_DateInfo, row));

			bizo.Z0_Date = new ZDateTime(2011, 06, 10, 09, 15, 45);
			AssertEquals("10-Jun-11 09:15", Formatter.GetStringFromPropertyValue(bizo.Z0_DateInfo, row));

			Factory.Save();
			bizo.Z0_Date = new ZDateTime(2012, 06, 10, 09, 15, 45);
			AssertEquals("Original value should be taken.", "10-Jun-11 09:15", Formatter.GetStringFromPropertyValue(bizo.Z0_DateInfo, row, true));
			AssertEquals("10-Jun-12 09:15", Formatter.GetStringFromPropertyValue(bizo.Z0_DateInfo, row));
		}

		public void TestGetStringFromPropertyValueForZBlob()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			bizo.Z0_VarBinaryMax = ZBlob.Empty;
			var row = ((INeedRow)bizo).Row;
			AssertEquals("Empty value", "", Formatter.GetStringFromPropertyValue(bizo.Z0_VarBinaryMaxInfo, row));

			Factory.Save();
			bizo.Z0_VarBinaryMax = new ZBlob(new byte[] { 10 });
			AssertEquals("Original value should be taken.", "", Formatter.GetStringFromPropertyValue(bizo.Z0_VarBinaryMaxInfo, row, true));
			AssertEquals("<changed>", Formatter.GetStringFromPropertyValue(bizo.Z0_VarBinaryMaxInfo, row));

			Factory.Save();
			bizo.Z0_VarBinaryMax = new ZBlob(new byte[] { 20 });
			AssertEquals("Original value should be taken.", "<changed>", Formatter.GetStringFromPropertyValue(bizo.Z0_VarBinaryMaxInfo, row, true));
			AssertEquals("<changed>", Formatter.GetStringFromPropertyValue(bizo.Z0_VarBinaryMaxInfo, row));
		}

		public void TestGetStringFromPropertyValueForUnrelatedZGuid()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var row = ((INeedRow)bizo).Row;
			bizo.Z0_Guid = ZGuid.Empty;
			AssertEquals("Empty value", "", Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row));

			string expectedGuid = "e4965396-8fba-467e-969d-7b8cd1da6de9";
			bizo.Z0_Guid = new ZGuid(expectedGuid);
			AssertEquals(expectedGuid, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row));

			Factory.Save();
			string expectedGuid2 = "a52f3c35-f497-416f-bdea-1a06e982b7e7";
			bizo.Z0_Guid = new ZGuid(expectedGuid2);
			AssertEquals("Original value should be taken.", expectedGuid, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row, true));
			AssertEquals(expectedGuid2, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row));
		}

		public void TestGetStringFromPropertyValueForZGuidWithRelatedBusinessObject()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var row = ((INeedRow)bizo).Row;
			var relatedBizo = Factory.New<DummyBusinessObject>();
			string expectedCode = "XYZ";
			relatedBizo.Z0_Code = expectedCode;
			bizo.Z0_Guid = relatedBizo.PK;

			AssertEquals(expectedCode, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row));

			Factory.Save();
			var relatedBizo2 = Factory.New<DummyBusinessObject>();
			string expectedCode2 = "XYZ2";
			relatedBizo2.Z0_Code = expectedCode2;
			bizo.Z0_Guid = relatedBizo2.PK;
			AssertEquals("Original value should be taken.", expectedCode, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row, true));
			AssertEquals(expectedCode2, Formatter.GetStringFromPropertyValue(bizo.Z0_GuidInfo, row));
		}

		DataVersionLogValueFormatter Formatter
		{
			get { return formatter ?? (formatter = GetNewFormatter()); }
		}
		DataVersionLogValueFormatter formatter;

		DataVersionLogValueFormatter GetNewFormatter()
		{
			return new DataVersionLogValueFormatter();
		}
	}
}
