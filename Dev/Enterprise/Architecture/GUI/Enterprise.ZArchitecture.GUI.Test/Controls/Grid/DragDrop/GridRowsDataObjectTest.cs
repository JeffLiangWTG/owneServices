using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridRowsDataObjectTest : TestCaseWithFactory
	{
		public void TestData()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var dummy1 = collection.AddNew();
			dummy1.Z0_Code = "ABC";
			dummy1.Z0_Number = 100;
			dummy1.Z0_Description = "One";

			var dummy2 = collection.AddNew();
			dummy2.Z0_Code = "XYZ";
			dummy2.Z0_Number = 999;
			dummy2.Z0_Description = "Two";

			var dummy3 = collection.AddNew();
			dummy3.Z0_Code = "ZZZ";
			dummy3.Z0_Number = 123;
			dummy3.Z0_Description = "Three";

			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Number, 80, 0));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 80) { CharacterCasing = CharacterCasing.Normal });

				form.Controls.Add(grid);
				grid.SetDataBinding(collection, "");

				var dataObject = new GridRowsDataObject(grid, new BusinessObject[] { dummy1, dummy3 });

				Assert(dataObject.GetDataPresent(GridRowsDataObject.DataFormatType));
				Assert(dataObject.GetDataPresent(typeof(ArrayList)));
				Assert(dataObject.GetDataPresent(DataFormats.CommaSeparatedValue));
				Assert(dataObject.GetDataPresent(DataFormats.Text));

				AssertEquals(2, dataObject.Elements.Count);
				AssertSame(dummy1, dataObject.Elements[0].BaseBusinessObject);
				AssertSame(dummy3, dataObject.Elements[1].BaseBusinessObject);

				var arrayList = (ArrayList)dataObject.GetData(typeof(ArrayList));
				AssertNotNull(arrayList);
				AssertEquals(2, arrayList.Count);
				AssertSame(dummy1, arrayList[0]);
				AssertSame(dummy3, arrayList[1]);

				var expectedText = "Code\tNumber\tDescription\r\nABC\t100\tOne\r\nZZZ\t123\tThree\r\n";
				AssertEquals(expectedText, dataObject.GetData(DataFormats.CommaSeparatedValue));
				AssertEquals(expectedText, dataObject.GetData(DataFormats.Text));
			}
		}
	}
}
