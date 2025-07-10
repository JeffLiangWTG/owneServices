using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing.G5V1.PackedItems
{
	public class G5V1TemporaryStoragePackedItemGridControlTest : TestCaseWithFactory
	{
		public void TestInitializeGridLayout()
		{
			using (var form = new ZForm(Factory.New<TemporaryStorageHeader>()))
			using (var control = new G5V1TemporaryStoragePackedItemGridControl())
			{
				form.Controls.Add(control);
				form.Show();

				var packedItemGrid = control.FindSingle<ZGrid>("PackedItemGrid");
				var missingColumnStyle = (ZCheckBoxColumnStyleInfo)packedItemGrid.GetColumnStyle(TemporaryStoragePackedItem.Schema.IsMissing);

				CombineAssertions(() =>
				{
					AssertEquals("IsMissing ColumnName", TemporaryStoragePackedItem.Schema.IsMissing, missingColumnStyle.ColumnName);
					AssertEquals("IsMissing TextAlign", System.Windows.Forms.HorizontalAlignment.Center, missingColumnStyle.TextAlign);
				});
			}
		}

		public void TestMissingCheckBoxVisibility()
		{
			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			using (var form = new ZForm(tempStorageHeader))
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var tempStoragePackedItem = control.FindSingleOrDefault<ZCheckBox>("MissingCheckBox");
					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
					AssertEquals("MissingCheckBox is not visible when declaration is not G5P", false, tempStoragePackedItem.Visible);

					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
					AssertEquals("MissingCheckBox is visible when declaration is G5P", true, tempStoragePackedItem.Visible);
				});
			}
		}
	}
}
