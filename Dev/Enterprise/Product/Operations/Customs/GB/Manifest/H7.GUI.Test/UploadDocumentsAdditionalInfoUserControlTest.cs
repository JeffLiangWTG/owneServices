using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	public class UploadDocumentsAdditionalInfoUserControlTest : TestCaseWithFactory
	{
		public void TestUploadDocumentsAdditionalInfoUserControl()
		{
			using (var control = new UploadDocumentsAdditionalInfoUserControl())
			{
				control.Show();

				var attachmentsGroupBox = control.FindSingle<ZGroupBox>("AttachmentsGroupBox");
				var attachmentsGrid = control.FindSingle<ZGrid>("AttachmentsGrid");

				var columnNameList = new List<string>(new string[] { "EDoc", "FileName", "FileSizeInKB", "DocumentType", "FileDescription" });

				CombineAssertions(() =>
				{
					AssertNotNull("AttachmentsGroupBox", attachmentsGroupBox);
					AssertEquals("Attachments", attachmentsGroupBox.CaptionResourceString.Caption);
					AssertNotNull("AttachmentsGrid", attachmentsGrid);
					AssertEquals("Binding Member", "EDocsCollection", attachmentsGrid.GetBindingMember());

					var columnStyles = attachmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
					AssertEquals("AttachmentsGrid ColumnStyles Count", 5, columnStyles.Count());
					foreach (var columnName in columnNameList)
					{
						var columnInfo = columnStyles.FirstOrDefault(s => s.ColumnName == columnName);
						AssertNotNull($"{columnName}", columnInfo);
						AssertEquals($"The column '{columnInfo.ColumnName}' should be visible", true, columnInfo.IsVisible);
					}
				});
			}
		}
	}
}

