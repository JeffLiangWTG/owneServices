using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	class UploadDocumentsAdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestAddInfosGridColumns()
		{
			using (var control = new UploadDocumentsAdditionalInfoUserControl())
			{
				var grid = control.FindSingle<ZGrid>("AddInfosGrid");
				AssertNotNull("AddInfosGrid", grid);
				AssertEquals("AddInfosGrid Column Count", 3, grid.ColumnStyles.Count);
				AssertEquals("Binding Member", "AddInfoCollection", grid.GetBindingMember());
			}
		}

		public void TestAttachmentGridColumns()
		{
			using (var control = new UploadDocumentsAdditionalInfoUserControl())
			{
				var attachmentsGrid = control.FindSingle<ZGrid>("AttachmentsGrid");
				AssertNotNull("AttachmentsGrid", attachmentsGrid);
				AssertEquals("AttachmentsGrid ColumnStyles Count", 4, attachmentsGrid.ColumnStyles.Count);
				AssertEquals("Binding Member", "AddInfoCollection.EDocsCollection", attachmentsGrid.GetBindingMember());
			}
		}

		public void TestDescriptionColumn_ReadOnly()
		{
			using (var control = new UploadDocumentsAdditionalInfoUserControl())
			{
				var attachmentsGrid = control.FindSingle<ZGrid>("AttachmentsGrid");
				var fileDescColumn = attachmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName.Equals("FileDescription"));
				Assert("File description is editable", !fileDescColumn.IsReadOnly);
			}
		}
	}
}
