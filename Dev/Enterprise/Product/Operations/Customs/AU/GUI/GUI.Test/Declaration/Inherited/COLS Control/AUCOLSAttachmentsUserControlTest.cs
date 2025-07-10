using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCOLSAttachmentsUserControlTest : TestCaseWithFactory
	{
		public void TestAUCOLSAttachmentsUserControlHeaderColumns()
		{
			using (var userControl = new AUCOLSAttachmentsUserControl())
			{
				var quarantineColsHeader = Factory.New<QuarantineColsHeader>();
				userControl.SetDataBinding(Factory.New<QuarantineColsHeader>(), "EDocPivotCollection");
				CombineAssertions(() =>
				{
					AssertEquals("User Control AttachmentsGrid should have 5 columns", 5, userControl.AttachmentsGrid.ColumnStyles.Count);
					AssertEquals("User control should have eDoc column", "CSD_StorageDocReference", userControl.AttachmentsGrid.GetColumnStyle(CusStorageDocPivot.Schema.CSD_StorageDocReference).ColumnName);
					AssertEquals("User control should have Document Type column", "CSD_DocType", userControl.AttachmentsGrid.GetColumnStyle(CusStorageDocPivot.Schema.CSD_DocType).ColumnName);
					AssertEquals("User control should have Description column", "CSD_Description", userControl.AttachmentsGrid.GetColumnStyle(CusStorageDocPivot.Schema.CSD_Description).ColumnName);
					AssertEquals("User control should have Status column", "CSD_MessageStatus", userControl.AttachmentsGrid.GetColumnStyle(CusStorageDocPivot.Schema.CSD_MessageStatus).ColumnName);
					AssertEquals("User control should have Status Description column", "MessageStatusDescription", userControl.AttachmentsGrid.GetColumnStyle(CusStorageDocPivot.Schema.MessageStatusDescription).ColumnName);
				});
			}
		}
	}
}
