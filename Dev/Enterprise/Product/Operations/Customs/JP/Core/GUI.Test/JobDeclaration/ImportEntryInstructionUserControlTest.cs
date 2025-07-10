using System.Linq;
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
	[TestedType(typeof(ImportEntryInstructionUserControl))]
	sealed class ImportEntryInstructionUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var control = new ImportEntryInstructionUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var certificateTabPage = form.FindSingle<ZTabPage>("CertificateTabPage");
				AssertEquals(expected: true, certificateTabPage.AutoScroll);
				TestHelper.AssertControlExists(control, "DetailsLayoutPanel", "CustomsEntryInstructions");
			}
		}

		public void TestNotesForCustomsOverrideCheckBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				using (var form = new ZForm(declaration))
				using (var control = new ImportEntryInstructionUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.MainTabControl.SelectTab("NotesTabPage");

					var notesForCustomsOverrideCheckBox = form.FindSingle<ZCheckBox>("NotesForCustomsOverrideCheckBox");
					var notesForCustomsTextBox = form.FindSingle<ZTextBox>("NotesForCustomsTextBox");
					AssertEquals("Initially non-overridable", false, notesForCustomsOverrideCheckBox.Visible);

					notesForCustomsTextBox.ReadOnly = true;
					AssertEquals("Overridable if default", true, notesForCustomsOverrideCheckBox.Visible);

					notesForCustomsOverrideCheckBox.Checked = true;
					notesForCustomsTextBox.ReadOnly = false;
					AssertEquals("Visible when overridden", false, notesForCustomsTextBox.ReadOnly);
				}
			});
		}

		public void TestImportEntryInstructionsGrid()
		{
			using (var control = new ImportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var dateForDutyColumn = grid.GetColumnStyle("CEI_DateForDuty");
				var displaySequenceColumn = grid.GetColumnStyle("CEI_DisplaySequence");
				var dutyOfDrawBackColumn = grid.GetColumnStyle("CEI_DutyDrawback");
				var customsInspectionCodeColumn = grid.GetColumnStyle("CEI_CustomsInspectionCode");
				var commercialValueTypeColumn = grid.GetColumnStyle("CEI_CommercialValueType");
				var contentInspectionResultColumn = grid.GetColumnStyle("CEI_ContentInspectionResult");
				var beforePermitApplicationReasonColumn = grid.GetColumnStyle("CEI_BeforePermitApplicationReason");
				var tradeTypeFirstCharColumn = grid.GetColumnStyle("TradeTypeFirstChar");
				var tradeTypeSecondCharColumn = grid.GetColumnStyle("TradeTypeSecondChar");
				var tradeTypeThirdCharColumn = grid.GetColumnStyle("TradeTypeThirdChar");
				var commonControlNumberColumn = grid.GetColumnStyle("CEI_CommonControlNumber");
				var foodHygieneCertificateTypeColumn = grid.GetColumnStyle("CEI_FoodHygieneCertificateType");
				var plantProtectionCertificateTypeColumn = grid.GetColumnStyle("CEI_PlantProtectionCertificateType");
				var animalQuarantineCertificateTypeColumn = grid.GetColumnStyle("CEI_AnimalQuarantineCertificateType");
				var customsNotesColumn = grid.GetColumnStyle("JP_CustomsNotes");
				var brokerNotesColumn = grid.GetColumnStyle("JP_BrokersNotes");
				var ownersNotesColumn = grid.GetColumnStyle("JP_OwnersNotes");
				var specialDeclarationOfficeColumn = grid.GetColumnStyle("CEI_CustomsOfficeForSpecialDeclarations");
				var specialDeclarationOfficeDepartmentColumn = grid.GetColumnStyle("CEI_CustomsOfficeDepartmentForSpecialDeclarations");
				var bondedLocationColumn = grid.GetColumnStyle("CEI_BondedLocationCode");
				var bondedLocationNameColumn = grid.GetColumnStyle("CEI_BondedLocationName");
				var styleColumn = grid.GetColumnStyle("CEI_Style");
				var subStyleColumn = grid.GetColumnStyle("CEI_SubStyle");
				var valueTypeColumn = grid.GetColumnStyle("CEI_ValueType");
				var grossWeightColumn = grid.GetColumnStyle("CEI_GrossWeight");
				var grossWeightUQColumn = grid.GetColumnStyle("CEI_GrossWeightUnit");
				var containerCountColumn = grid.GetColumnStyle("CEI_ContainerCount");
				var declarationCargoTypeColumn = grid.GetColumnStyle("CEI_DeclarationCargoType");
				var speciaDeclarationTypeColumn = grid.GetColumnStyle("CEI_SubStyle");
				var declarationConditionColumn = grid.GetColumnStyle("CEI_DeclarationCondition");
				var declarationConditionDescriptionColumn = grid.GetColumnStyle("DeclarationConditionDescription");

				CombineAssertions(() =>
				{
					ColumnTestHelper(dateForDutyColumn);
					AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short, (dateForDutyColumn as ZDateEditColumnStyleInfo).DateTimeFormat);
					ColumnTestHelper(displaySequenceColumn);
					ColumnTestHelper(valueTypeColumn);
					ColumnTestHelper(dutyOfDrawBackColumn);
					ColumnTestHelper(customsInspectionCodeColumn);
					ColumnTestHelper(commercialValueTypeColumn);
					ColumnTestHelper(contentInspectionResultColumn);
					ColumnTestHelper(beforePermitApplicationReasonColumn);
					ColumnTestHelper(tradeTypeFirstCharColumn);
					ColumnTestHelper(tradeTypeSecondCharColumn);
					ColumnTestHelper(tradeTypeThirdCharColumn);
					ColumnTestHelper(commonControlNumberColumn);
					ColumnTestHelper(foodHygieneCertificateTypeColumn);
					ColumnTestHelper(plantProtectionCertificateTypeColumn);
					ColumnTestHelper(animalQuarantineCertificateTypeColumn);
					ColumnTestHelper(customsNotesColumn, false);
					ColumnTestHelper(brokerNotesColumn, false);
					ColumnTestHelper(ownersNotesColumn, false);
					ColumnTestHelper(specialDeclarationOfficeColumn);
					ColumnTestHelper(specialDeclarationOfficeDepartmentColumn);
					ColumnTestHelper(bondedLocationColumn);
					ColumnTestHelper(bondedLocationNameColumn);
					ColumnTestHelper(grossWeightColumn);
					ColumnTestHelper(grossWeightUQColumn);
					ColumnTestHelper(containerCountColumn);
					ColumnTestHelper(declarationCargoTypeColumn);
					ColumnTestHelper(speciaDeclarationTypeColumn);
					ColumnTestHelper(declarationConditionColumn, false);
					ColumnTestHelper(declarationConditionDescriptionColumn, false);
					ColumnTestHelper(subStyleColumn);
					AssertEquals(typeof(ZDropEditColumnStyle), styleColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), subStyleColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), valueTypeColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), customsNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), brokerNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), ownersNotesColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), grossWeightColumn.ColumnStyleType);
					AssertEquals("UQ", grossWeightUQColumn.CaptionResourceString.Caption);
					AssertEquals(typeof(ZDropEditColumnStyle), grossWeightUQColumn.ColumnStyleType);
					AssertEquals(typeof(ZCalcEditColumnStyle), containerCountColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), declarationCargoTypeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), speciaDeclarationTypeColumn.ColumnStyleType);
					AssertEquals(typeof(ZDropEditColumnStyle), declarationConditionColumn.ColumnStyleType);
					AssertEquals(typeof(ZTextBoxColumnStyle), declarationConditionDescriptionColumn.ColumnStyleType);
					AssertEquals(30, displaySequenceColumn.Width);
				});
			}
		}

		public void TestImportEntryInstructionsGridGroupNames()
		{
			using (var control = new ImportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var tradeTypeFirstCharColumn = grid.GetColumnStyle("TradeTypeFirstChar");
				var tradeTypeSecondCharColumn = grid.GetColumnStyle("TradeTypeSecondChar");
				var tradeTypeThirdCharColumn = grid.GetColumnStyle("TradeTypeThirdChar");
				var specialDeclarationOfficeColumn = grid.GetColumnStyle("CEI_CustomsOfficeForSpecialDeclarations");
				var specialDeclarationOfficeDepartmentColumn = grid.GetColumnStyle("CEI_CustomsOfficeDepartmentForSpecialDeclarations");
				var grossWeightColumn = grid.GetColumnStyle("CEI_GrossWeight");
				var grossWeightUQColumn = grid.GetColumnStyle("CEI_GrossWeightUnit");
				var declarationConditionColumn = grid.GetColumnStyle("CEI_DeclarationCondition");
				var declarationConditionDescriptionColumn = grid.GetColumnStyle("DeclarationConditionDescription");

				CombineAssertions(() =>
				{
					AssertEquals(tradeTypeFirstCharColumn.GroupName, tradeTypeSecondCharColumn.GroupName);
					AssertEquals(tradeTypeSecondCharColumn.GroupName, tradeTypeThirdCharColumn.GroupName);
					AssertEquals(specialDeclarationOfficeColumn.GroupName, specialDeclarationOfficeDepartmentColumn.GroupName);
					AssertEquals(grossWeightColumn.GroupName, grossWeightUQColumn.GroupName);
					AssertEquals(declarationConditionColumn.GroupName, declarationConditionDescriptionColumn.GroupName);
				});
			}
		}

		public void TestImportEntryInstructionGridColumnsOrder()
		{
			using (var control = new ImportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				var expectedColumnsOrders = new string[]
				{
					"CEI_DisplaySequence",
					"CEI_Description",
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
					"CEI_DutyDrawback",
					"CEI_CommercialValueType",
					"CEI_ContentInspectionResult",
					"CEI_BeforePermitApplicationReason",
					"CEI_CommonControlNumber",
					"CEI_FoodHygieneCertificateType",
					"CEI_PlantProtectionCertificateType",
					"CEI_AnimalQuarantineCertificateType",
					"CEI_DateForDuty",
					"CEI_DeclarationCondition",
					"DeclarationConditionDescription",
					"CEI_CustomsOfficeForSpecialDeclarations",
					"CEI_CustomsOfficeDepartmentForSpecialDeclarations",
					"CEI_BondedLocationCode",
					"CEI_BondedLocationName",
					"CEI_GrossWeight",
					"CEI_GrossWeightUnit",
					"CEI_CustomsWeight",
					"CEI_CustomsWeightUnit",
					"CEI_ContainerCount",
					"CEI_DeclarationCargoType"
				};

				AssertArrayEqualsByElements(expectedColumnsOrders, columnStyles.ToArray());
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

		public void TestGuaranteesGrid()
		{
			using (var control = new ImportEntryInstructionUserControl())
			{
				var grid = control.FindSingle<ZGrid>("GuaranteesGrid");

				var referenceColumn = grid.GetColumnStyle("CFR_Reference");
				AssertNotNull(referenceColumn);
				Assert(!referenceColumn.IsMandatory);
				Assert(referenceColumn.IsVisible);
				AssertEquals("MaximumRows of GuaranteesGrid must be equal to CusGuaranteeReferenceCollection.MaxRowCount", CusGuaranteeReferenceCollection.MaxRowCount, grid.MaximumRows);
			}
		}
	}
}
