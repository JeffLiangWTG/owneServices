namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSHOTFileImportLineRowTest : CASSHOTFileLineRowTest
	{
		public override void TestPublicFields()
		{
			base.TestPublicFields();
			AssertEquals("", ImportLineRowForTest.VATIndicator);
		}

		protected CASSHOTFileImportLineRow ImportLineRowForTest
		{
			get { return (CASSHOTFileImportLineRow)DataRowForTest; }
		}

		protected override CASSHOTFileDataRow GetCASSHOTFileDataRowForTest()
		{
			return new CASSHOTFileImportLineRow();
		}
	}
}
