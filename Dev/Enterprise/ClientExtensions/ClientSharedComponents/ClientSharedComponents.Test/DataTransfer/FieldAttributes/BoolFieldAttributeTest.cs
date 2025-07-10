using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class BoolFieldAttributeTest : FieldAttributeTest
	{
		protected override void TestGetValueCore()
		{
			BoolFieldAttribute attrib = new BoolFieldAttribute(0);
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = "TRUE";
			AssertEquals("dataRow[0]", ZBool.True, (ZBool)attrib.GetValue(dataRow));

			attrib = new BoolFieldAttribute(0, 3, BoolFieldAttribute.BoolTypes.YesNo);
			dataRow[0] = "YES";
			AssertEquals("dataRow[0]", ZBool.True, (ZBool)attrib.GetValue(dataRow));
		}

		protected override void TestSetValueAsStringCore()
		{
			IZType value = ZBool.True;
			FlatFileDataRow dataRow = new FlatFileDataRow(1);

			AssertEquals("datarow[0] ", ZString.Empty, dataRow[0]);
			BoolFieldAttribute attrib = new BoolFieldAttribute(0, 4, BoolFieldAttribute.BoolTypes.YesNo, AlignTypes.Right);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("datarow[0]", " YES", dataRow[0]);
		}
	}
}
