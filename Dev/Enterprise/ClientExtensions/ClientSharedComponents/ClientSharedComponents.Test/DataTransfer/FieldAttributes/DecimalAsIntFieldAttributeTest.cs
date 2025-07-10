using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class DecimalAsIntFieldAttributeTest : FieldAttributeTest
	{
		protected override void TestGetValueCore()
		{
			DecimalAsIntFieldAttribute attrib = new DecimalAsIntFieldAttribute(0);
			FlatFileDataRow dataRow = new FlatFileDataRow(1);
			dataRow[0] = "00599";
			AssertEquals("dataRow[0]", 599m, attrib.GetValue(dataRow));

			DecimalAsIntFieldAttribute attrib2 = new DecimalAsIntFieldAttribute(0, 5, 2);
			AssertEquals("dataRow[0]", 5.99m, attrib2.GetValue(dataRow));
		}

		protected override void TestSetValueAsStringCore()
		{
			IZType value = (ZDecimal)5.99m;
			FlatFileDataRow dataRow = new FlatFileDataRow(1);

			AssertEquals("datarow[0] ", ZString.Empty, dataRow[0]);
			DecimalAsIntFieldAttribute attrib = new DecimalAsIntFieldAttribute(0, 8, 2, AlignTypes.Right);
			attrib.SetValueAsString(value, dataRow);
			AssertEquals("datarow[0]", "00000599", dataRow[0]);
		}
	}
}
