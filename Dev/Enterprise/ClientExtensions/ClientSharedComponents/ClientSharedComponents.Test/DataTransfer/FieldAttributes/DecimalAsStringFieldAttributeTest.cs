using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class DecimalAsStringFieldAttributeTest : FieldAttributeTest
	{
		protected override void TestGetValueCore()
		{
			DecimalAsStringFieldAttribute attrib = new DecimalAsStringFieldAttribute(0);
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = "005.99";
			AssertEquals("dataRow[0]", 6m, attrib.GetValue(dataRow));

			DecimalAsStringFieldAttribute attrib2 = new DecimalAsStringFieldAttribute(0, 5, 2);
			AssertEquals("dataRow[0]", 5.99m, attrib2.GetValue(dataRow));
		}

		protected override void TestSetValueAsStringCore()
		{
			IZType value = (ZDecimal)5.99m;
			FlatFileDataRow dataRow = new FlatFileDataRow(1);

			AssertEquals("datarow[0] ", ZString.Empty, dataRow[0]);
			DecimalAsStringFieldAttribute attrib = new DecimalAsStringFieldAttribute(0, 8, 2, AlignTypes.Right);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("datarow[0]", "00005.99", dataRow[0]);
		}
	}
}
