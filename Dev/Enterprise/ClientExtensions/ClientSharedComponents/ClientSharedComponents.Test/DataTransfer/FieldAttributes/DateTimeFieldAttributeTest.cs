using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class DateTimeFieldAttributeTest : FieldAttributeTest
	{
		protected override void TestGetValueCore()
		{
			DateTimeFieldAttribute attrib = new DateTimeFieldAttribute(0, "bad");
			IZType value = new ZString("12/12/09");
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = "2009-12-12T23:12:59";
			AssertEquals("dataRow[0]", ZDateTime.Empty, attrib.GetValue(dataRow));

			attrib = new DateTimeFieldAttribute(0, "yyyy-MM-dd HH:mm:ss");
			AssertEquals("dataRow[0]", new ZDateTime(2009, 12, 12, 23, 12, 59), attrib.GetValue(dataRow));

			attrib = new DateTimeFieldAttribute(0, 4, "bad");
			dataRow = new FlatFileDataRow(1);
			dataRow[0] = "20091212231259";
			AssertEquals("dataRow[0]", ZDateTime.Empty, attrib.GetValue(dataRow));

			DateTimeFieldAttribute defaultAttrib = new DateTimeFieldAttribute(11);
			attrib = new DateTimeFieldAttribute(0, 14, "yyyyMMddHHmmss");
			AssertEquals("dataRow[0]", new ZDateTime(2009, 12, 12, 23, 12, 59), attrib.GetValue(dataRow));
			AssertNotEquals("Date format overrides default format", attrib.DateTimeFormat, defaultAttrib.DateTimeFormat);
		}

		protected override void TestSetValueAsStringCore()
		{
			DateTimeFieldAttribute attrib = new DateTimeFieldAttribute(0, "yyyyMMddHHmmss");
			IZType value = new ZDateTime(2009, 12, 30, 23, 20, 59);
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("dataRow[0]", "20091230232059", dataRow[0]);
		}
	}
}
