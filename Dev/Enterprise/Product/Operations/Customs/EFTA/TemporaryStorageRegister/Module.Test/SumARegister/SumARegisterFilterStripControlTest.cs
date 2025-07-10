using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

sealed class SumARegisterFilterStripControlTest : TestCaseWithFactory
{
	public void TestHeaderGridColumnNames()
	{
		AssertGridColumnNames(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	public void TestLinesGridColumnNames()
	{
		AssertGridColumnNames(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

	public void TestHeaderGridDefaultColumnOrder()
	{
		AssertGridDefaultColumnOrder(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	public void TestLinesGridDefaultColumnOrder()
	{
		AssertGridDefaultColumnOrder(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

	public void TestHeaderGridColumnWidths()
	{
		AssertGridColumnWidths(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	public void TestLinesGridColumnWidths()
	{
		AssertGridColumnWidths(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

	public void TestPresentationDateColumnFormat()
	{
		using var filterControl = new SumARegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode), new SumARegisterFilterBusinessObject());
		var presentationDateColumn = filterControl.Grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_PresentationDate) as ZDateEditColumnStyleInfo;
		AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, presentationDateColumn.DateTimeFormat);
	}

	public void TestGridsSplitContainer()
	{
		using var form = new ZForm();
		using var filterControl = new SumARegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode), new SumARegisterFilterBusinessObject());
		form.Controls.Add(filterControl);
		form.Show();

		var splitContainer = filterControl.HeaderAndLinesGridSplitContainer;
		CombineAssertions(() =>
		{
			AssertEquals("SplitContainer Panel1 contains the Header FilteredGrid", true, splitContainer.Panel1.Contains(filterControl.Grid));
			AssertEquals("SplitContainer Panel2 contains the Header FilteredGrid", true, splitContainer.Panel2.Contains(filterControl.LinesGrid));

			AssertEquals("SplitContainer Orientation", Orientation.Horizontal, splitContainer.Orientation);
			AssertEquals("Panel1 Min Size", splitContainer.Panel1MinSize, ControlDpiScalingHelper.ScaleToCurrentDpiY(140));
			AssertEquals("Panel2 Min Size", splitContainer.Panel2MinSize, ControlDpiScalingHelper.ScaleToCurrentDpiY(140));
		});
	}

	void AssertGridColumnNames(ColumnDetails[] orderedColumnDetails, Func<SumARegisterFilterStripControl, ZFilterGrid> getGrid)
	{
		using var form = new ZForm();
		using var filterControl = new SumARegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode), new SumARegisterFilterBusinessObject());
		form.Controls.Add(filterControl);
		form.Show();
		var grid = getGrid(filterControl);

		CombineAssertions(() =>
		{
			foreach (var column in orderedColumnDetails)
			{
				AssertEquals(column.Name, column.Caption, grid.GetColumnCaption(column.Name));
			}
		});
	}

	void AssertGridDefaultColumnOrder(ColumnDetails[] orderedColumnDetails, Func<SumARegisterFilterStripControl, ZFilterGrid> getGrid)
	{
		using var form = new ZForm();
		using var filterControl = new SumARegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode), new SumARegisterFilterBusinessObject());
		form.Controls.Add(filterControl);
		form.Show();

		AssertSequencesEqual(orderedColumnDetails.Select(x => x.Name), getGrid(filterControl).DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
	}

	void AssertGridColumnWidths(ColumnDetails[] orderedColumnDetails, Func<SumARegisterFilterStripControl, ZFilterGrid> getGrid)
	{
		using var filterControl = new SumARegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterModule.SumARegisterAppCode), new SumARegisterFilterBusinessObject());
		var grid = getGrid(filterControl);
		CombineAssertions(() =>
		{
			foreach (var column in orderedColumnDetails)
			{
				AssertEquals(column.Name, column.Width, grid.GetColumnStyle(column.Name).Width);
			}
		});
	}

	ColumnDetails[] OrderedHeaderGridColumnDetails => new[]
	{
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_Reference, "TSD Number", 130),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_ArrivalDate, "Arrival Date", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PresentationDate, "Presentation Date", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType, "Previous Ref Type", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PreviousReference, "Previous Ref Number", 120),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_Status, "Status", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_InternalReference, "Job Reference", 130)
	};

	ColumnDetails[] OrderedLinesGridColumnDetails => new[]
	{
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_LineNumber, "Line Number", 80),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType, "Owner Reference Type", 130),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_OwnerReference, "Owner Reference Number", 160),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_LocationOfGoods, "Location of Goods", 110),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_GoodsDescription, "Goods Description", 100),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_LimitDate, "Limit Date", 80),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_CustodianIdentifier, "Custodian EORI", 100),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifier, "Owner ID", 140),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_PackagesRemaining, "Packages Remaining", 80),
		new ColumnDetails(CusTempStorageRegLine.Schema.SRL_CustomsStatus, "Customs Status", 80)
	};

	sealed class ColumnDetails
	{
		public string Name { get; }
		public string Caption { get; }
		public int Width { get; }

		public ColumnDetails(string name, string caption, int width)
		{
			Name = name;
			Caption = caption;
			Width = width;
		}
	}
}
