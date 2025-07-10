using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionUserControl))]
	sealed class ExportEntryInstructionUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportEntryInstructionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl");
				var certificateTabPage = form.FindSingle<ZTabPage>("CertificateTabPage");
				var entryInstructionsGrid = form.FindSingle<ZGrid>("EntryInstructionsGrid");
				mainTabControl.SelectedTab = certificateTabPage;
				AssertEquals(true, certificateTabPage.AutoScroll);

				var approvalCertificateClassificationDropEdit = form.FindSingle<ZDropEdit>("ApprovalCertificateCategoryDropEdit");
				Assert("ApprovalCertificateCategoryDropEdit", approvalCertificateClassificationDropEdit.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("ApprovalCertificateCategoryDropEdit", !approvalCertificateClassificationDropEdit.Visible);

				var marksAndNumbersTextBox = form.FindSingle<ZTextBox>("MarksAndNumbersTextBox");
				AssertEquals(ImeMode.Disable, marksAndNumbersTextBox?.ImeMode);

				declaration.JE_TransportMode = Business.TransportTypeList.Codes.Air;
				AssertEquals(false, marksAndNumbersTextBox?.Visible);

				TestHelper.AssertControlExists(control, "DetailsLayoutPanel", "CustomsEntryInstructions");
			}
		}

		public void TestExportControlNumberVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Air;

			jobDeclartion.CustomsEntryInstructions.AddNew();

			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var customsEntryInstructionUserControl = (ExportEntryInstructionUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;

				var grid = customsEntryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
				var exportControlNumberColumn = grid.GetColumnStyle("ExportControlNumber");
				Assert(exportControlNumberColumn.IsUnavailable);

				jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Sea;
				Assert(!exportControlNumberColumn.IsUnavailable);
			}
		}

		public void TestAwbOrBillNumberVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Sea;

			jobDeclartion.CustomsEntryInstructions.AddNew();

			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var customsEntryInstructionUserControl = (ExportEntryInstructionUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;

				var grid = customsEntryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
				var awbOrBillNumberColumn = grid.GetColumnStyle("CEI_BillNumber");
				Assert(awbOrBillNumberColumn.IsUnavailable);

				jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Air;
				Assert(!awbOrBillNumberColumn.IsUnavailable);
			}
		}

		public void TestECRTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveInDestinationInfo = entryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_Code = "TEST";
			moveInDestinationInfo.InventoryNumbers.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportEntryInstructionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl");
				var ecrTabPage = form.FindSingle<ZTabPage>("ECRTabPage");
				AssertNotNull("ECRTabPage should exist under MainTabControl", ecrTabPage);

				mainTabControl.SelectedTab = ecrTabPage;
				Assert(ecrTabPage.TabVisible);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				Assert(!ecrTabPage.TabVisible);

				var ecrTabControl = ecrTabPage.FindSingle<ZTabControl>("ECRTabControl");
				var ecrMainDetailPage = ecrTabControl.FindSingle<ZTabPage>("ECRMainDetailsTabPage");
				var ecrLocationsInventoriesPage = ecrTabControl.FindSingle<ZTabPage>("ECRLocationsInventoriesTabPage");
				AssertNotNull("ecrMainDetailPage should exist under ecrTabPage", ecrMainDetailPage);
				AssertNotNull("ecrLocationsInventoriesPage should exist under ecrTabPage", ecrLocationsInventoriesPage);

				var moveInInformationGroupBox = ecrLocationsInventoriesPage.FindSingle<ZGroupBox>("MoveInInformationGroupBox");
				AssertNotNull("MoveInInformationGroupBox should exist under ECRTabPage", moveInInformationGroupBox);

				var ecrGrid = moveInInformationGroupBox.FindSingle<ZGrid>("ECRGrid");
				AssertNotNull("ECRGrid should exist under MoveInInformationGroupBox", ecrGrid);
				AssertEquals("MaximumRows of ECRGrid should be 5", 5, ecrGrid.MaximumRows);
				Assert(!ecrTabPage.TabVisible);

				var inventoryNumbersGroupBox = moveInInformationGroupBox.FindSingle<ZGroupBox>("InventoryNumbersGroupBox");
				AssertNotNull("InventoryNumbersGroupBox should exist under MoveInInformationGroupBox", inventoryNumbersGroupBox);

				ecrTabControl.SelectedTab = ecrLocationsInventoriesPage;
				Assert("InventoryNumbersGroupBox should be not visible when nothing of ECRGrid is selected.", !inventoryNumbersGroupBox.Visible);

				ecrGrid.CurrentCell = new DataGridCell(0, 1);
				Assert("InventoryNumbersGroupBox should be visible when nothing of ECRGrid is selected.", inventoryNumbersGroupBox.Visible);

				var inventoryNumbersGrid = inventoryNumbersGroupBox.FindSingle<ZGrid>("InventoryNumbersGrid");
				AssertNotNull("InventoryNumbersGrid should exist under InventoryNumbersGroupBox", inventoryNumbersGrid);
			}
		}

		public void TestCDB01TabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportEntryInstructionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

				var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl");
				var cdb01TabPageTabPage = form.FindSingle<ZTabPage>("CDB01TabPage");
				AssertNotNull("CDB01TabPage should exist under MainTabControl", cdb01TabPageTabPage);

				mainTabControl.SelectedTab = cdb01TabPageTabPage;
				Assert(cdb01TabPageTabPage.TabVisible);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				Assert(!cdb01TabPageTabPage.TabVisible);
			}
		}

		public void TestInventoryNumbersGrid()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("InventoryNumbersGrid");
				CombineAssertions(() =>
				{
					AssertEquals("MaximumRows", 15, grid.MaximumRows);
					AssertColumn(grid, nameof(MoveInDestination.CSI_Code), true);
					AssertColumn(grid, nameof(MoveInDestination.CSI_Quantity), true, 0, 99999999);
				});
			}
		}

		void AssertColumn(ZGrid grid, string name, bool isMandatory = false, int decimals = 2, decimal maxValue = int.MinValue)
		{
			var column = grid.GetColumnStyle(name);
			AssertNotNull(name, column);
			AssertEquals($"{name} : Mandatory", isMandatory, column.IsMandatory);
			if (column is ZCalcEditColumnStyleInfo caclEditColumn)
			{
				AssertEquals($"{name} : Decimals", decimals, caclEditColumn.Decimals);
				if (maxValue > int.MinValue)
				{
					AssertEquals($"{name} : MaxValue", maxValue, caclEditColumn.MaxValue);
				}
			}
		}

		public void TestECRGrid()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("ECRGrid");
				CombineAssertions(() =>
				{
					AssertColumn(grid, nameof(MoveInDestination.CSI_Code));
					AssertColumn(grid, nameof(MoveInDestination.CSI_DateOfIssue));
					AssertColumn(grid, nameof(MoveInDestination.CSI_ReferenceNumber));
					AssertColumn(grid, nameof(MoveInDestination.CSI_Quantity), false, 0, 99999999);
					AssertColumn(grid, nameof(MoveInDestination.CSI_Quantity2), decimals: 3);
					AssertColumn(grid, nameof(MoveInDestination.CSI_Quantity3), decimals: 3);
					AssertColumn(grid, nameof(MoveInDestination.CSI_Description));
				});
			}
		}

		public void TestExportEntryInstructionsGrid()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var dateForDutyColumn = grid.GetColumnStyle("CEI_DateForDuty");
				var valueTypeColumn = grid.GetColumnStyle("CEI_ValueType");
				var displaySequenceColumn = grid.GetColumnStyle("CEI_DisplaySequence");
				var customsInspectionCodeColumn = grid.GetColumnStyle("CEI_CustomsInspectionCode");
				var loadingConfirmationIsRequiredColumn = grid.GetColumnStyle("CEI_LoadingConfirmationIsRequired");
				var preInspectedCargoTypeColumn = grid.GetColumnStyle("CEI_PreInspectedCargoType");
				var tradeTypeFirstCharColumn = grid.GetColumnStyle("TradeTypeFirstChar");
				var tradeTypeSecondCharColumn = grid.GetColumnStyle("TradeTypeSecondChar");
				var tradeTypeThirdCharColumn = grid.GetColumnStyle("TradeTypeThirdChar");
				var approvalCertificateCategoryColumn = grid.GetColumnStyle("CEI_ApprovalCertificateCategory");
				var customsNotesColumn = grid.GetColumnStyle("JP_CustomsNotes");
				var brokerNotesColumn = grid.GetColumnStyle("JP_BrokersNotes");
				var ownersNotesColumn = grid.GetColumnStyle("JP_OwnersNotes");
				var styleColumn = grid.GetColumnStyle("CEI_Style");
				var subStyleColumn = grid.GetColumnStyle("CEI_SubStyle");
				var grossWeightColumn = grid.GetColumnStyle("CEI_GrossWeight");
				var grossWeightUQColumn = grid.GetColumnStyle("CEI_GrossWeightUnit");
				var containerCountColumn = grid.GetColumnStyle("CEI_ContainerCount");
				var exportControlNumberColumn = grid.GetColumnStyle("ExportControlNumber");
				var awbOrbillNumbeColumn = grid.GetColumnStyle("CEI_BillNumber");
				var declarationCargoTypeColumn = grid.GetColumnStyle("CEI_DeclarationCargoType");
				var declarationConditionColumn = grid.GetColumnStyle("CEI_DeclarationCondition");
				var declarationConditionDescriptionColumn = grid.GetColumnStyle("DeclarationConditionDescription");
				var customsVolumeColumn = grid.GetColumnStyle("CEI_CustomsVolume");
				var customsVolumeUQColumn = grid.GetColumnStyle("CEI_CustomsVolumeUnit");
				var volumeColumn = grid.GetColumnStyle("CEI_Volume");
				var volumeUQColumn = grid.GetColumnStyle("CEI_VolumeUnit");
				var cargoQuantityColumn = grid.GetColumnStyle("CEI_CargoQuantity");
				var cargoQuantityUQColumn = grid.GetColumnStyle("CEI_CargoQuantityUnit");
				var rcrActionColumn = grid.GetColumnStyle("CEI_RCRAction");
				var viaLocationColumn = grid.GetColumnStyle("CEI_ViaLocation");
				var previousBillNumberColumn = grid.GetColumnStyle("CEI_PreviousBillNumber");

				CombineAssertions(() =>
				{
					ColumnTestHelper(valueTypeColumn);
					ColumnTestHelper(dateForDutyColumn);
					AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short, (dateForDutyColumn as ZDateEditColumnStyleInfo).DateTimeFormat);
					ColumnTestHelper(displaySequenceColumn);
					ColumnTestHelper(customsInspectionCodeColumn);
					ColumnTestHelper(loadingConfirmationIsRequiredColumn);
					ColumnTestHelper(preInspectedCargoTypeColumn);
					ColumnTestHelper(tradeTypeFirstCharColumn);
					ColumnTestHelper(tradeTypeSecondCharColumn);
					ColumnTestHelper(tradeTypeThirdCharColumn);
					ColumnTestHelper(approvalCertificateCategoryColumn);
					ColumnTestHelper(customsNotesColumn, false);
					ColumnTestHelper(brokerNotesColumn, false);
					ColumnTestHelper(ownersNotesColumn, false);
					ColumnTestHelper(grossWeightColumn);
					ColumnTestHelper(grossWeightUQColumn);
					ColumnTestHelper(containerCountColumn);
					ColumnTestHelper(exportControlNumberColumn);
					ColumnTestHelper(awbOrbillNumbeColumn);
					ColumnTestHelper(declarationCargoTypeColumn);
					ColumnTestHelper(declarationConditionColumn, false);
					ColumnTestHelper(declarationConditionDescriptionColumn, false);
					ColumnTestHelper(customsVolumeColumn);
					ColumnTestHelper(customsVolumeUQColumn);
					ColumnTestHelper(volumeColumn, false);
					ColumnTestHelper(volumeUQColumn, false);
					ColumnTestHelper(cargoQuantityColumn);
					ColumnTestHelper(cargoQuantityUQColumn);
					ColumnTestHelper(rcrActionColumn, false);
					ColumnTestHelper(viaLocationColumn, false);
					ColumnTestHelper(previousBillNumberColumn, false);
					AssertEquals(typeof(ZDropEditColumnStyle), valueTypeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), styleColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), subStyleColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), customsNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), brokerNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), ownersNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), grossWeightColumn.ColumnStyleType);
					AssertEquals("UQ", grossWeightUQColumn.CaptionResourceString.Caption);
					AssertEquals(typeof(ZDropEditColumnStyle), grossWeightUQColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), containerCountColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), declarationCargoTypeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), declarationConditionColumn.ColumnStyleType);
					AssertEquals(typeof(ZTextBoxColumnStyle), declarationConditionDescriptionColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), customsVolumeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), customsVolumeUQColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), volumeColumn.ColumnStyleType);
					AssertEquals("UQ", volumeUQColumn.CaptionResourceString.Caption);
					AssertEquals(typeof(ZDropEditColumnStyle), volumeUQColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), cargoQuantityColumn.ColumnStyleType);
					AssertEquals("UQ", cargoQuantityUQColumn.CaptionResourceString.Caption);
					AssertEquals(typeof(ZDropEditColumnStyle), cargoQuantityUQColumn.ColumnStyleType);
					AssertEquals(typeof(ZTextBoxColumnStyle), awbOrbillNumbeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), rcrActionColumn.ColumnStyleType);
					AssertEquals(typeof(ZCodeFindBoxColumnStyle), viaLocationColumn.ColumnStyleType);
					AssertEquals(typeof(ZTextBoxColumnStyle), previousBillNumberColumn.ColumnStyleType);
					AssertEquals(30, displaySequenceColumn.Width);
				});
			}
		}

		public void TestExporttEntryInstructionsGridGroupNames()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var tradeTypeFirstCharColumn = grid.GetColumnStyle("TradeTypeFirstChar");
				var tradeTypeSecondCharColumn = grid.GetColumnStyle("TradeTypeSecondChar");
				var tradeTypeThirdCharColumn = grid.GetColumnStyle("TradeTypeThirdChar");
				var grossWeightColumn = grid.GetColumnStyle("CEI_GrossWeight");
				var grossWeightUQColumn = grid.GetColumnStyle("CEI_GrossWeightUnit");
				var declarationConditionColumn = grid.GetColumnStyle("CEI_DeclarationCondition");
				var declarationConditionDescriptionColumn = grid.GetColumnStyle("DeclarationConditionDescription");

				CombineAssertions(() =>
				{
					AssertEquals(tradeTypeFirstCharColumn.GroupName, tradeTypeSecondCharColumn.GroupName);
					AssertEquals(tradeTypeSecondCharColumn.GroupName, tradeTypeThirdCharColumn.GroupName);
					AssertEquals(grossWeightColumn.GroupName, grossWeightUQColumn.GroupName);
					AssertEquals(declarationConditionColumn.GroupName, declarationConditionDescriptionColumn.GroupName);
				});
			}
		}

		public void TestExportEntryInstructionGridColumnsOrders()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				var expectedColumnsOrders = new string[]
				{
					"CEI_DisplaySequence",
					"CEI_BillNumber",
					"ExportControlNumber",
					"CEI_Description",
					"CEI_GoodsDescription",
					"CEI_SpecialCargoCode",
					"CEI_ValueType",
					"CEI_Style",
					"CEI_SubStyle",
					"CEI_CustomsInspectionCode",
					"TradeTypeFirstChar",
					"TradeTypeSecondChar",
					"TradeTypeThirdChar",
					"JP_CustomsNotes",
					"JP_BrokersNotes",
					"JP_OwnersNotes",
					"CEI_LoadingConfirmationIsRequired",
					"CEI_PreInspectedCargoType",
					"CEI_ApprovalCertificateCategory",
					"CEI_DateForDuty",
					"CEI_DeclarationCondition",
					"DeclarationConditionDescription",
					"CEI_GrossWeight",
					"CEI_GrossWeightUnit",
					"CEI_CustomsWeight",
					"CEI_CustomsWeightUnit",
					"CEI_ContainerCount",
					"CEI_DeclarationCargoType",
					"CEI_Volume",
					"CEI_VolumeUnit",
					"CEI_CustomsVolume",
					"CEI_CustomsVolumeUnit",
					"CEI_CargoQuantity",
					"CEI_CargoQuantityUnit",
					"CEI_RCRAction",
					"CEI_ViaLocation",
					"CEI_PreviousBillNumber",
				};

				AssertArrayEqualsByElements(expectedColumnsOrders, columnStyles.ToArray());
			}
		}

		public void TestEntryApprovalCertificateInfoGrid()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("ApprovalCertificateInfoGrid");
				var codeColumn = grid.GetColumnStyle("CSI_Code");
				var numberColumn = grid.GetColumnStyle("CSI_ReferenceNumber");
				var descriptionColumn = grid.GetColumnStyle("ReferenceNumberDescription");

				AssertType<ZDropEditColumnStyleInfo>(codeColumn);
				AssertType<ZDropEditColumnStyleInfo>(numberColumn);
				AssertType<ZTextBoxColumnStyleInfo>(descriptionColumn);
				AssertEquals("MaximumRows of ApprovalCertificateInfoGrid must be equal to ApprovalCertificateInfoCollection.MaxCountForExport", ApprovalCertificateInfoCollection.MaxCountForExport, grid.MaximumRows);
			}
		}

		public void TestNotesForBrokerAndNotesForOwner()
		{
			using (var control = new ExportEntryInstructionUserControl())
			{
				var notesForBrokerTextBox = control.FindSingle<ZTextBox>("NotesForBrokerTextBox");
				var notesForOwnerTextBox = control.FindSingle<ZTextBox>("NotesForOwnerTextBox");

				AssertEquals(false, notesForBrokerTextBox.Multiline);
				AssertEquals(false, notesForOwnerTextBox.Multiline);
			}
		}

		void ColumnTestHelper(ZGridColumnInfo column, bool isVisible = true)
		{
			AssertNotNull(column);
			Assert(!column.IsMandatory);
			AssertEquals($"Visible of {column.ColumnName} should be {isVisible}", isVisible, column.IsVisible);
		}
	}
}
