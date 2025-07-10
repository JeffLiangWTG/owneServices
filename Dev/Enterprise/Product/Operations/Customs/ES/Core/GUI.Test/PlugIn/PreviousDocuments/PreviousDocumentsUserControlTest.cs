using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestPrevDocsTypeDropEditVisibility()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				control.Show();

				var typeDropEdit = control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit");
				AssertEquals(false, typeDropEdit.Visible);
			}
		}

		public void TestCSI_CodeCodeFindBoxVisibility()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				control.Show();

				AssertEquals(true, control.CSI_CodeCodeFindBox.Visible);
			}
		}

		public void TestBindingSourceType()
		{
			using (var control = new PreviousDocumentsUserControl())
			{
				AssertEquals(typeof(EU.Business.Declaration.JobDeclaration), control.BindingSource.DataSourceType);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument);
			using (var control = new PreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(PreviousDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_SubType: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_DateOfIssue: Format", ZDateTimePickerFormat.Long, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue)).DateTimeFormat);
					AssertEquals("CSI_LineNo: BindToDecimalPlaces", null, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo)).BindToDecimalPlaces);
					AssertEquals("CSI_Quantity: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity)).Decimals);
					AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
				});
			}
		}

		public void TestGridColumns()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument);
			using (var control = new PreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				grid.SetDataBinding(collection, "");
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("columns count", OrderedColumnNamesAndColumnStyleTypes.Count(), grid.ColumnStyles.Count);

					var indexCounter = 0;
					foreach (var (columnName, columnType) in OrderedColumnNamesAndColumnStyleTypes)
					{
						var column = grid.Columns.SingleOrDefault(x => x.ColumnName == columnName);
						if (column == null)
						{
							Assert($"Column: {columnName} does not exists", false);
						}
						else
						{
							var actualColumnStyleType = column.ColumnStyle.GetType();
							AssertEquals($"Column: {columnName}: exists at expected position '{indexCounter}'", columnName, grid.Columns[indexCounter].ColumnName);
							AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
							Assert($"Column: {columnName}: visible", column.IsVisible);
						}
						indexCounter++;
					}
				});
			}
		}

		public void TestPackColumnsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var control = new PreviousDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				previousDocumentsGrid.SetDataBinding(declaration.PreviousDocuments, "");

				CombineAssertions(() =>
				{
					AssertNull("Export declaration, CSI_PackQty should be hidden", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_PackQty]);
					AssertNull("Export declaration, CSI_PackType should be hidden", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_PackType]);

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertNotNull("Import declaration, CSI_PackQty should be visible", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_PackQty]);
					AssertNotNull("Import declaration, CSI_PackType should be visible", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_PackType]);
				});
			}
		}

		public void TestNKCountryCodeVisibility() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			using (var form = new ZForm(declaration))
			using (var control = new PreviousDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				previousDocumentsGrid.SetDataBinding(declaration.PreviousDocuments, "");

				AssertNotNull("CSI_RN_NKCountryCode Visible for IMP and H1 activate", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode]);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertNull("CSI_RN_NKCountryCode not Visible for EXP and H1 activate", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode]);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
			using (var form = new ZForm(declaration))
			using (var control = new PreviousDocumentsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				previousDocumentsGrid.SetDataBinding(declaration.PreviousDocuments, "");

				AssertNull("CSI_RN_NKCountryCode not Visible for EXP and not H1 activate", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode]);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertNull("CSI_RN_NKCountryCode not Visible for IMP and not H1 activate", previousDocumentsGrid.Columns[AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode]);
			}
		});

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyle)),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyle)),
			(PreviousDocument.Schema.CSI_RN_NKCountryCode, typeof(ZCodeFindBoxColumnStyle)),
		};
	}
}
