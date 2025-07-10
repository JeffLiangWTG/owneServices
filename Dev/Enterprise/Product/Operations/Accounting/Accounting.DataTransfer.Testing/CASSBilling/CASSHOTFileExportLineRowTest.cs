namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSHOTFileExportLineRowTest : CASSHOTFileLineRowTest
	{
		public override void TestPublicFields()
		{
			base.TestPublicFields();
			ExportLineRowForTest.SetField(CASSHOTFileExportLineRow.Schema.VATIndicator, "Y");
			AssertEquals("Y", ExportLineRowForTest.VATIndicator);
		}

		protected CASSHOTFileExportLineRow ExportLineRowForTest
		{
			get { return (CASSHOTFileExportLineRow)DataRowForTest; }
		}

		protected override CASSHOTFileDataRow GetCASSHOTFileDataRowForTest()
		{
			return new CASSHOTFileExportLineRow();
		}
	}
}
