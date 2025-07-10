using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

class SupplierHeaderPreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestBottomPanelSize()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var panel = control.FindSingle<ZPanel>("BottomPanel");

			var a = control.FindSingle<ZTextBox>("PrevDocsReferenceTextBox");

			CombineAssertions(() =>
			{
				AssertEquals("BottomPanel Size.Width", 679, panel.Size.Width);
				AssertEquals("BottomPanel Size.Height", 227, panel.Size.Height);
			});
		}
	}

	public void TestPrevDocsGroupBoxSize()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var groupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");

			CombineAssertions(() =>
			{
				AssertEquals("BottomPanel Size.Width", 679, groupBox.Size.Width);
				AssertEquals("BottomPanel Size.Height", 227, groupBox.Size.Height);
			});
		}
	}

	public void TestGridColumnNames()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();
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
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
			AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
		}
	}

	public void TestGridColumnWidths()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

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
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

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
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		using (var control = new SupplierHeaderPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("PrevDocsGroupBox", true, control.FindSingle<ZGroupBox>("PrevDocsGroupBox").Visible);
				AssertEquals("PrevDocsReferenceTextBox", true, control.FindSingle<ZTextBox>("PrevDocsReferenceTextBox").Visible);
				AssertEquals("PrevDocsTypeDropEdit", false, control.FindSingle<ZDropEdit>("PrevDocsTypeDropEdit").Visible);
				AssertEquals("CodeCodeFindBox", true, control.FindSingle<ZCodeFindBox>("CodeCodeFindBox").Visible);
			});
		}
	}

	static (string ColumnName, string ColumnCaption, int ColumnWidth, Type ColumnType)[] OrderedColumnDetails => new[]
	{
		(PreviousDocument.Schema.CSI_Code, "Type", 72, typeof(ZCodeFindBoxColumnStyleInfo)),
		(PreviousDocument.Schema.CSI_ReferenceNumber, "Reference", 250, typeof(ZTextBoxColumnStyleInfo))
	};
}
