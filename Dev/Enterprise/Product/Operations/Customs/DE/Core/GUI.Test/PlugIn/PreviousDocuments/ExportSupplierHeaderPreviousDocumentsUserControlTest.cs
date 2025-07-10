using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportSupplierHeaderPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnNames()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		public void TestGridDefaultColumnOrder()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var frm = new ZForm(declaration))
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGridColumnWidths()
		{
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		public void TestGridColumnType()
		{
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, _, columnType) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnType, grid.GetColumnStyle(columnName).GetType());
					}
				});
			}
		}

		public void TestControlsVisibility()
		{
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				AssertEquals("PrevDocsGroupBox", true, control.FindSingle<ZGroupBox>("PrevDocsGroupBox").Visible);
				AssertEquals("PrevDocsTypeDropEdit", false, control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
				AssertEquals("CodeFindBox", true, control.FindSingle<ZCodeFindBox>("CodeFindBox").Visible);
			}
		}

		public void TestPrevDocsReferenceTextBox()
		{
			using (var control = new ExportSupplierHeaderPreviousDocumentsUserControl())
			{
				AssertEquals("CharacterCasing of PrevDocsReferenceTextBox", System.Windows.Forms.CharacterCasing.Normal, control.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").CharacterCasing);
			}
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth, Type ColumnType)[] OrderedColumnDetails => new[]
		{
			(PreviousDocument.Schema.CSI_Code, "Type", 72, typeof(ZCodeFindBoxColumnStyleInfo)),
			(PreviousDocument.Schema.CSI_ReferenceNumber, "Reference", 250, typeof(ZTextBoxColumnStyleInfo))
		};
	}
}
