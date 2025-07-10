using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class IntFieldAttributeTest : FieldAttributeTest
	{
		protected override void TestGetValueCore()
		{
			IntFieldAttribute attrib = new IntFieldAttribute(0);
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = "5";
			AssertEquals("dataRow[0]", 5, attrib.GetValue(dataRow));
		}

		protected override void TestSetValueAsStringCore()
		{
			IZType value = (ZInt)5;
			FlatFileDataRow dataRow = new FlatFileDataRow(1);

			AssertEquals("datarow[0] ", ZString.Empty, dataRow[0]);
			IntFieldAttribute attrib = new IntFieldAttribute(0, 8, AlignTypes.Right);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("datarow[0]", "00000005", dataRow[0]);
		}
	}
}
