using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsForEntryInstructionGridExport()
		{
			var columnsOnlyForExport = new string[]
			{
				CusEntryInstruction.Schema.CEI_SubStyle,
				CusEntryInstruction.Schema.CEI_Style,
				AutoCusEntryInstruction.Schema.ZG_PartyConstellation,
				CusEntryInstruction.Schema.CEI_Description,
				CusEntryInstruction.Schema.CEI_DateForDuty,
				AutoCusEntryInstruction.Schema.ZG_ExitDate,
			};
			AssertColumnsForEntryInstructionGrid(MessageTypeList.Codes.Export, columnsOnlyForExport);
		}

		public void TestColumnsForEntryInstructionGridImport()
		{
			var columnsOnlyForImport = new string[]
			{
				CusEntryInstruction.Schema.CEI_Style,
				CusEntryInstruction.Schema.CEI_SubStyle,
				CusEntryInstruction.Schema.CEI_Procedure,
				CusEntryInstruction.Schema.CEI_Description,
				CusEntryInstruction.Schema.CEI_LocalClearanceDate,
				CusEntryInstruction.Schema.CEI_EarlyClearanceFlag,
			};

			AssertColumnsForEntryInstructionGrid(MessageTypeList.Codes.Import, columnsOnlyForImport);
		}

		public void TestDetailsTabPageVisible_Export()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, DetailsTabPage, true);
		}

		public void TestDetailsTabPageVisible_Import()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, DetailsTabPage, true);
		}

		public void TestOutwardProcessingVisible_Default()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var outwardProcessingTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == "OutwardProcessingTabPage");
				Assert(!outwardProcessingTabPage.TabVisible);
			}
		}

		public void TestOutwardProcessingVisible_NoEntryInstruction()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, OutwardProcessingTabPage, false);
		}

		public void TestOutwardProcessingVisible_Export_EnabledOutwardProcessing()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;

			AssertTabPageVisible(MessageTypeList.Codes.Export, OutwardProcessingTabPage, true);
		}

		public void TestInwardProcessingVisible_NoEntryInstruction()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, InwardProcessingTabPage, false);
		}

		public void TestInwardProcessingVisible_EAV()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;

			AssertTabPageVisible(MessageTypeList.Codes.Import, InwardProcessingTabPage, true);
		}

		public void TestInwardProcessingVisible_LUZ()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			AssertTabPageVisible(MessageTypeList.Codes.Import, InwardProcessingTabPage, false);
		}

		public void TestInwardProcessingMainAccountingDocAddressControlVisible()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			AssertMainAccountingDocAddressControlVisible(false);
		}

		public void TestInwardProcessingMainAccountingDocAddressControlVisible_J()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			AssertMainAccountingDocAddressControlVisible(true);
		}

		public void TestInwardProcessingMainAccountingDocAddressControlVisible_N()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			AssertMainAccountingDocAddressControlVisible(false);
		}

		public void TestPanel2MinimumSize()
		{
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var splitContainer = control.FindSingle<KSplitContainer>("SplitContainer");
				AssertEquals(432, splitContainer.Panel2MinSize);
			}
		}

		public void TestPreviousDocumentsTabPageVisible_Export()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, PreviousDocumentsTabPage, false);
		}

		public void TestPreviousDocumentsTabPageVisible_Import()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, PreviousDocumentsTabPage, true);
		}

		public void TestPreviousDocumentsTabPageVisible_IPR_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertTabPageVisible(MessageTypeList.Codes.Import, PreviousDocumentsTabPage, false);
		}

		public void TestFiscalReferencesTabPageVisible_Export()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, FiscalReferencesTabPage, false);
		}

		public void TestFiscalReferencesTabPageVisible_Import()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, FiscalReferencesTabPage, true);
		}

		public void TestFiscalReferencesTabPageVisible_IPR_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertTabPageVisible(MessageTypeList.Codes.Import, FiscalReferencesTabPage, false);
		}

		public void TestDV1DetailsTabPageVisible_Export()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, DV1DetailsTabPage, false);
		}

		public void TestDV1DetailsTabPageVisible_Import()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, DV1DetailsTabPage, true);
		}

		public void TestDV1DetailsTabPageVisible_IPR_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertTabPageVisible(MessageTypeList.Codes.Import, DV1DetailsTabPage, false);
		}

		public void TestSupplyChainActorReferencesTabPageVisible_Export()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Export, SupplyChainActorReferencesTabPage, true);
		}

		public void TestSupplyChainActorReferencesTabPageVisible_Import()
		{
			AssertTabPageVisible(MessageTypeList.Codes.Import, SupplyChainActorReferencesTabPage, false);
		}

		public void TestDV1DetailsTabPageVisible_ChangeFromExportToImport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				CombineAssertions(() =>
				{
					var tabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == DV1DetailsTabPage);
					AssertEquals("JE_MessageType is EXP", false, tabPage.TabVisible);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("JE_MessageType is changed to IMP", true, tabPage.TabVisible);
				});
			}
		}

		public void TestReferenceProcedureTypeCanHaveSinglePreviousDocumentOnly()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var previousDocumentMaster = declaration.CustomsEntryInstructions.AddNew().PreviousDocumentMaster;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var previousDocsTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == PreviousDocumentsTabPage);
				instructionTabControl.SelectTab(previousDocsTabPage);
				var grid = instructionTabControl.FindSingle<ZGrid>("PreviousDocumentsGrid");

				CombineAssertions(() =>
				{
					previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATAV;
					AssertEquals("ATAV, can add more rows", true, grid.List.AllowNew);
					AssertEquals("ATAV, rows count", 2, grid.VisibleRowCount);

					previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATA;
					AssertEquals("ATA, cannot add more rows", false, grid.List.AllowNew);
					AssertEquals("ATA, rows count", 1, grid.VisibleRowCount);

					previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._T1;
					AssertEquals("T1, cannot add more rows", false, grid.List.AllowNew);
					AssertEquals("T1, rows count", 1, grid.VisibleRowCount);

					previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATNEU;
					AssertEquals("ATNEU, can add more rows", true, grid.List.AllowNew);
					AssertEquals("ATNEU, rows count", 2, grid.VisibleRowCount);
				});
			}
		}

		public void TestPreviousDocumentsGridWithNoColumnsNotVisibleOnLoad()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var previousDocumentMaster = declaration.CustomsEntryInstructions.AddNew().PreviousDocumentMaster;
			previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._200;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var previousDocsTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == PreviousDocumentsTabPage);
				instructionTabControl.SelectTab(previousDocsTabPage);
				var grid = instructionTabControl.FindSingle<ZGrid>("PreviousDocumentsGrid");

				AssertEquals("PreviousDocumentsGrid shouldn't be visible when loading procedure '200'", false, grid.Visible);
			}
		}

		public void TestFiscalReferencesUserControl()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var fiscalReferencesTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == FiscalReferencesTabPage);
				instructionTabControl.SelectTab(fiscalReferencesTabPage);
				var fiscalReferencesUserControl = fiscalReferencesTabPage.Controls.Find("FiscalReferencesUserControl", true).First();

				AssertType<EU.GUI.EntryInstructionFiscalReferencesUserControl>(fiscalReferencesUserControl);
			}
		}

		public void TestSupplyChainActorReferencesUserControl()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var supplyChainActorReferencesTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == SupplyChainActorReferencesTabPage);
				instructionTabControl.SelectTab(supplyChainActorReferencesTabPage);
				var supplyChainActorReferencesUserControl = supplyChainActorReferencesTabPage.Controls.Find("SupplyChainActorReferencesUserControl", true).First();

				CombineAssertions(() =>
				{
					AssertEquals("Tab caption", "Add. Supply Chain Actor", supplyChainActorReferencesTabPage.CaptionResourceString.Caption);
					AssertType<EU.GUI.PlugIn.SupplyChainActorReferencesUserControl>("Type", supplyChainActorReferencesUserControl);
				});
			}
		}

		public void TestAuthorizationsTabPageVisible()
		{
			CombineAssertions(() =>
			{
				AssertTabPageVisible(MessageTypeList.Codes.Import, AuthorizationsTabPage, true);
				AssertTabPageVisible(MessageTypeList.Codes.Export, AuthorizationsTabPage, true);
			});
		}

		public void TestAuthorizationsTabPageVisible_IPR_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertTabPageVisible(MessageTypeList.Codes.Import, AuthorizationsTabPage, false);
		}

		public void TestAuthorisationsUserControl()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var authorizationsTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == AuthorizationsTabPage);
				instructionTabControl.SelectTab(authorizationsTabPage);
				var authorizationsUserControl = authorizationsTabPage.Controls.Find("AuthorizationsUserControl", true).First();

				CombineAssertions(() =>
				{
					AssertEquals("Tab caption", "Authorizations", authorizationsTabPage.CaptionResourceString.Caption);
					AssertType<EU.GUI.EntryInstructionAuthorisationsUserControl>("Type", authorizationsUserControl);
				});
			}
		}

		public void TestTabPagesOrder()
		{
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var tabPages = instructionTabControl.AllTabPages;
				AssertArrayEqualsByElements(new[] { DetailsTabPage, OutwardProcessingTabPage, InwardProcessingTabPage, PreviousDocumentsTabPage, DV1DetailsTabPage, FiscalReferencesTabPage, SupplyChainActorReferencesTabPage, AuthorizationsTabPage }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		void AssertColumnsForEntryInstructionGrid(ZString messageType, string[] columns)
		{
			declaration.JE_MessageType = messageType;

			bool isExport = declaration.IsExport;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				CombineAssertions(() =>
				{
					var entryInstructionsGrid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
					AssertContainsExactElementsInAnyOrder("Correct Columns", columns, entryInstructionsGrid.Columns.GetVisibleColumnMappingNames());

					var styleColumn = entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
					AssertEquals("Style column caption", isExport ? "Type (Procedure)" : "Declaration Type", styleColumn.Caption);
					AssertEquals("Style column character case", System.Windows.Forms.CharacterCasing.Normal, styleColumn.CharacterCasing);

					var subStyleColumn = entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
					AssertEquals("Sub Style column caption", isExport ? "Type (Time)" : "Sub Style", subStyleColumn.Caption);
					AssertEquals("Sub Style column character case (use lower case to show export outward processing tabpage)", System.Windows.Forms.CharacterCasing.Normal, subStyleColumn.CharacterCasing);

					var partyConstellationColumn = entryInstructionsGrid.GetColumnStyle(AutoCusEntryInstruction.Schema.ZG_PartyConstellation);
					AssertEquals("Party Constellation availability", isExport, !partyConstellationColumn.IsUnavailable);
					AssertEquals("Party Constellation column character case", System.Windows.Forms.CharacterCasing.Normal, partyConstellationColumn.CharacterCasing);

					var descriptionColumn = entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Description);
					AssertEquals("Description column character case", System.Windows.Forms.CharacterCasing.Normal, descriptionColumn.CharacterCasing);

					AssertEquals("Date For Duty availability", !isExport, entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_DateForDuty).IsUnavailable);
					AssertEquals("Exit Date availability", !isExport, entryInstructionsGrid.GetColumnStyle(AutoCusEntryInstruction.Schema.ZG_ExitDate).IsUnavailable);
					AssertEquals("Early Clearance Flag availability", isExport, entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_EarlyClearanceFlag).IsUnavailable);
					AssertEquals("Procedure availability", isExport, entryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_EarlyClearanceFlag).IsUnavailable);
				});
			}
		}

		void AssertTabPageVisible(ZString messageType, ZString tabPageName, bool expectedResult)
		{
			declaration.JE_MessageType = messageType;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var tabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == tabPageName);
				AssertEquals(expectedResult, tabPage.TabVisible);
			}
		}

		void AssertMainAccountingDocAddressControlVisible(bool expectedResult)
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();

				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var outwardProcessingTabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == InwardProcessingTabPage);
				instructionTabControl.SelectTab(outwardProcessingTabPage);
				var mainAccountingDocAddressControl = outwardProcessingTabPage.FindSingle<ZDocAddressControl>("MainAccountingDocAddressControl");
				AssertEquals(expectedResult, mainAccountingDocAddressControl.Visible);
			}
		}

		const string OutwardProcessingTabPage = "OutwardProcessingTabPage";
		const string InwardProcessingTabPage = "InwardProcessingTabPage";
		const string DetailsTabPage = "DetailsTabPage";
		const string PreviousDocumentsTabPage = "PreviousDocumentsTabPage";
		const string FiscalReferencesTabPage = "FiscalReferencesTabPage";
		const string DV1DetailsTabPage = "DV1DetailsTabPage";
		const string SupplyChainActorReferencesTabPage = "SupplyChainActorReferencesTabPage";
		const string AuthorizationsTabPage = "AuthorizationsTabPage";
	}
}
