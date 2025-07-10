using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class SendingImportPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsReadOnly()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument, false);
			using (var control = new SendingImportPreviousDocumentsUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				previousDocumentGrid.SetDataBinding(collection, ZString.Empty);
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("PreviousDocumentsGrid", true, previousDocumentGrid.ReadOnly);

					var procedureGroupBox = control.FindSingle<ZGroupBox>("ProcedureGroupBox");
					AssertEquals("ProcedureGroupBox", false, procedureGroupBox.Enabled);

					var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
					AssertEquals("PrevDocsGroupBox", false, prevDocsGroupBox.Enabled);
				});
			}
		}

		public void TestControlsBindingMember()
		{
			using (var control = new SendingImportPreviousDocumentsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("DataSourceType", typeof(ImportEntryMessageSendingAction), control.BindingSource.DataSourceType);

					var previousProcedureDropEdit = control.FindSingle<ZDropEdit>("PreviousProcedureDropEdit");
					AssertEquals("PreviousProcedureDropEdit", "EntryInstruction.PreviousDocumentMaster.CSI_Procedure", control.BindingSource.GetBindingMember(previousProcedureDropEdit));

					var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
					AssertEquals("PreviousDocumentsGrid", "EntryInstruction.PreviousDocuments", control.BindingSource.GetBindingMember(previousDocumentGrid));

					var prevDocsReferenceTextBox = control.FindSingle<ZTextBox>("PrevDocsReferenceTextBox");
					AssertEquals("PrevDocsReferenceTextBox", "EntryInstruction.PreviousDocuments.CSI_ReferenceNumber", control.BindingSource.GetBindingMember(prevDocsReferenceTextBox));

					var prevDocsTypeDropEdit = control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit");
					AssertEquals("PrevDocsTypeDropEdit", "EntryInstruction.PreviousDocuments.CSI_Code", control.BindingSource.GetBindingMember(prevDocsTypeDropEdit));
				});
			}
		}
	}
}
