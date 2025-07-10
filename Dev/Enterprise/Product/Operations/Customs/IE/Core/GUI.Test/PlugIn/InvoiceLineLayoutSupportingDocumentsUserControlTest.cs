using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class InvoiceLineLayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestUCC5AndImportAvailableColumnNames()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, UCC5AndImportOrderedColumnNamesAndColumnStyleTypes);
			}
		}

		void AssertColumnNamesAndColumnStyle(ZGrid grid, IEnumerable<(string ColumnName, Type ColumnType)> expectedColumnNamesAndColumnStyle)
		{
			CombineAssertions(() =>
			{
				AssertEquals("columns count", expectedColumnNamesAndColumnStyle.Count(), grid.Columns.Count);

				var indexCounter = 0;
				foreach (var (columnName, columnType) in expectedColumnNamesAndColumnStyle)
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

		static IEnumerable<(string ColumnName, Type ColumnType)> UCC5AndImportOrderedColumnNamesAndColumnStyleTypes => new[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber2), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
		};
	}
}
