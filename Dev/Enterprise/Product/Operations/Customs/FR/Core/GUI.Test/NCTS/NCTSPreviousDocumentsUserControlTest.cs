using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NCTSPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestColumnArePresent()
		{
			using (var control = new NctsPreviousDocumentsUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				control.Show();
				AssertNotNull(previousDocumentsGrid);
				CombineAssertions(() =>
				{
					AssertNotNull("CSI_Cod is present", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Code));
					AssertNotNull("CSI_ReferenceNumber is present", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber));
					AssertNotNull("CSI_Description is present", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Description));
					AssertNull("CSI_SubType is not present", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_SubType));
					AssertNotNull("CSI_LineNo is present", previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_LineNo));
				});
			}
		}

		public void TestPrevDocsReferenceVisibility()
		{
			using (var control = new NctsPreviousDocumentsUserControl())
			{
				var previousDocumentsCodeFindBox = control.FindSingle<ZCodeFindBox>("PrevDocsReferenceCodeFindBox");
				var prevDocsReferenceTextBox = control.FindSingle<ZTextBox>("PrevDocsReferenceTextBox");
				var prevDocsTypeDropEdit = control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit");
				control.Show();

				prevDocsTypeDropEdit.Text = PreviousDocumentCodeList.Codes._337;
				AssertEquals("PrevDocsReferenceCodeFindBox should be visible as CSI_COde ==PreviousDocumentCodeList.Codes._337", true, previousDocumentsCodeFindBox.Visible);
				AssertEquals("PrevDocsReferenceTextBox should not be visible as CSI_COde ==PreviousDocumentCodeList.Codes._337", false, prevDocsReferenceTextBox.Visible);

				prevDocsTypeDropEdit.Text = PreviousDocumentCodeList.Codes._720;
				AssertEquals("PrevDocsReferenceCodeFindBox should not be visible as CSI_COde !=PreviousDocumentCodeList.Codes._337", false, previousDocumentsCodeFindBox.Visible);
				AssertEquals("PrevDocsReferenceTextBox should be visible as CSI_COde !=PreviousDocumentCodeList.Codes._337", true, prevDocsReferenceTextBox.Visible);
			}
		}
	}
}
