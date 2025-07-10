using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class DeclarationRequestedDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestRequestedDocumentsGrid_CSI_ReferenceNumber() => CombineAssertions(() =>
		{
			using (var control = new DeclarationRequestedDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("RequestedDocumentsGrid");

				var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(EU.Business.RequestedDocument.CSI_ReferenceNumber));
				AssertEquals("Width", 150, columnInfo.Width);

				var captionResourceString = columnInfo.CaptionResourceString;
				AssertEquals("Short", "Ref. No.", captionResourceString.ShortCaption);
				AssertEquals("Medium", "Reference No.", captionResourceString.MediumCaption);
				AssertEquals("Caption", "Reference Number", captionResourceString.Caption);
				AssertEquals("Full Description", "Requested document reference number.", captionResourceString.FullDescription);
			}
		});

		public void TestRequestedDocumentsControls_Import()
		{
			using (var control = new DeclarationRequestedDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("RequestedDocumentsGrid");
				var columnNames = string.Join(",", grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => !c.IsUnavailable).Select(x => x.ColumnName));
				AssertEquals("Fields", "CSI_Code,RequestInformation,CSI_DateOfIssue,CSI_DateOfExpiry,CSI_Status,StatusDescription,CSI_ReferenceNumber", columnNames);
			}
		}

		public void TestRequestedDocumentsControls_Export()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.JobDeclaration.JE_MessageType = "EXP";

			using (var form = new ZForm(entryInstruction))
			{
				using (var control = new DeclarationRequestedDocumentsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var grid = control.FindSingle<ZGrid>("RequestedDocumentsGrid");
					var columnNames = string.Join(",", grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => !c.IsUnavailable).Select(x => x.ColumnName));
					AssertEquals("Fields", "CSI_Code,RequestInformation,CSI_DateOfIssue,CSI_DateOfExpiry,CSI_Status,StatusDescription", columnNames);
				}
			}
		}
	}
}
