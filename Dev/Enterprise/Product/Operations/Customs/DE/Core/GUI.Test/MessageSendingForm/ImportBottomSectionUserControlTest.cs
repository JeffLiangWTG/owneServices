using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ImportBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				using (var control = new ImportBottomSectionUserControl())
				{
					var bottomGroupBox = control.BottomGroupBox;
					var transportPanel = control.TransportPanel;
					AssertEquals("BottomGroupBox", true, control.Contains(bottomGroupBox));
					AssertEquals("AdditionalWarningUserControl", true, control.Contains(control.AdditionalWarningUserControl));

					AssertEquals("TransportPanel", true, bottomGroupBox.Contains(transportPanel));
					AssertEquals("PreviousDocumentsUserControl", true, bottomGroupBox.Contains(control.PreviousDocumentsUserControl));

					AssertEquals("TransportIDInLandTextBox", true, transportPanel.Contains(control.TransportIDInLandTextBox));
					AssertEquals("GoodsLocationTextBox", true, transportPanel.Contains(control.GoodsLocationTextBox));
				}
			});
		}

		public void TestPreviousDocumentsUserControl()
		{
			CombineAssertions(() =>
			{
				var previousDocument = entryInstruction.PreviousDocuments.AddNew();
				previousDocument.CSI_Procedure = PreviousProcedureList.Codes._ATZL;
				previousDocument.CSI_ReferenceNumber = "R123";
				previousDocument.CSI_LineNo = 1;

				var action = new ImportEntryMessageSendingAction(entry, null) { CusCon = true };
				using (var form = new ZForm(action))
				using (var control = new ImportBottomSectionUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var grid = ((SendingImportPreviousDocumentsUserControl)control.PreviousDocumentsUserControl.HostedControl).FindSingle<ZGrid>("PreviousDocumentsGrid");
					AssertEquals("Grid readonly", true, grid.ReadOnly);
					AssertEquals("Grid has 1 row", 1, grid.VisibleRowCount);
				}
			});
		}

		public void TestBottomGroupBox_Visible()
		{
			CombineAssertions(() =>
			{
				var action = new ImportEntryMessageSendingAction(entry, null);
				using (var form = new ZForm(action))
				using (var control = new ImportBottomSectionUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					var bottomGroupBox = control.BottomGroupBox;
					AssertEquals("Default", false, bottomGroupBox.Visible);
					action.CusCon = true;
					AssertEquals("CusCon->True", true, bottomGroupBox.Visible);
					action.CusCon = false;
					AssertEquals("CusCon->False", false, bottomGroupBox.Visible);
				}
			});
		}

		public void TestAdditionalWarningUserControl_Visible()
		{
			var actionParent = new ImportDeclarationMessageSendingActionParentForTest(declaration);
			actionParent.WarningToAdd = "this is a warning";
			var action = new ImportEntryMessageSendingAction(entry, actionParent);
			using (var form = new ZForm(action))
			using (var control = new ImportBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				action.ShouldSend = true;
				AssertEquals(true, control.AdditionalWarningUserControl.Visible);
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;
		}
		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
	}
}
