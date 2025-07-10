using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI.PlugIn;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.GUI.Testing;

class PreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestCodeVisibility()
	{
		using (var control = new PreviousDocumentsUserControl())
		{
			var codeDropEdit = control.PrevDocsTypeFindBox;
			AssertNotNull(nameof(codeDropEdit), codeDropEdit);
			AssertEquals(nameof(codeDropEdit.Visible), true, codeDropEdit.Visible);
		}
	}

	public void TestReferenceVisibility()
	{
		using (var control = new PreviousDocumentsUserControl())
		{
			var referenceTextBox = control.PrevDocsReferenceTextBox;
			AssertNotNull(nameof(referenceTextBox), referenceTextBox);
			AssertEquals(nameof(referenceTextBox.Visible), true, referenceTextBox.Visible);
		}
	}

	public void TestAdditionalInformationVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var control = new PreviousDocumentsUserControl())
		{
			control.JobDeclaration = declaration;

			var additionalInformationGridColumn = control.PrevDocsGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Description);
			var additionalInformationTextBox = control.PrevDocsAdditionalInformationTextBox;

			AssertNotNull(nameof(additionalInformationGridColumn), additionalInformationGridColumn);
			AssertNotNull(nameof(additionalInformationTextBox), additionalInformationTextBox);

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			AssertEquals(nameof(additionalInformationGridColumn.IsUnavailable), false, additionalInformationGridColumn.IsUnavailable);
			AssertEquals(nameof(additionalInformationTextBox.Visible), true, additionalInformationTextBox.Visible);

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
			AssertEquals(nameof(additionalInformationGridColumn.IsUnavailable), true, additionalInformationGridColumn.IsUnavailable);
			AssertEquals(nameof(additionalInformationTextBox.Visible), false, additionalInformationTextBox.Visible);
		}
	}

	public void TestInitializeGridLayoutCore()
	{
		using (var control = new PreviousDocumentsUserControl())
		{
			var previousDocumentsGridControl = (ZGrid)control.Controls.Find("PrevDocsGrid", true).First();
			var previousDocumentGridColumsStyles = previousDocumentsGridControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Code, 0);
				UserControlTestHelper.AssertColumnStyles(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_ReferenceNumber, 1);
				UserControlTestHelper.AssertColumnStyles(previousDocumentGridColumsStyles, PreviousDocument.Schema.CSI_Description, 2);
			});
		}
	}
}
