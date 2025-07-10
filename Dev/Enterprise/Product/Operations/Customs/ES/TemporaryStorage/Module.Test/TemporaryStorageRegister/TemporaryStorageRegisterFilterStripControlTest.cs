using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing;

[TestedType(typeof(TemporaryStorageRegisterFilterStripControl))]
sealed class TemporaryStorageRegisterFilterStripControlTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestHeaderGridColumnNames()
	{
		AssertGridColumnNames(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	[RequiresSTA]
	public void TestLinesGridColumnNames()
	{
		AssertGridColumnNames(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

	[RequiresSTA]
	public void TestHeaderGridDefaultColumnOrder()
	{
		AssertGridDefaultColumnOrder(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	[RequiresSTA]
	public void TestLinesGridDefaultColumnOrder()
	{
		AssertGridDefaultColumnOrder(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

	[RequiresSTA]
	public void TestHeaderGridColumnWidths()
	{
		AssertGridColumnWidths(OrderedHeaderGridColumnDetails, x => x.Grid);
	}

	[RequiresSTA]
	public void TestLinesGridColumnWidths()
	{
		AssertGridColumnWidths(OrderedLinesGridColumnDetails, x => x.LinesGrid);
	}

		[RequiresSTA]
		public void TestPresentationDateColumnFormat()
		{
			using (var filterControl = new TemporaryStorageRegisterFilterStripControl(new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageRegisterModule.AppCode), new TemporaryStorageRegisterFilterBusinessObject()))
			{
				var presentationDateColumn = filterControl.Grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_PresentationDate) as ZDateEditColumnStyleInfo;
				AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, presentationDateColumn.DateTimeFormat);
			}
		}

		[RequiresSTA]
		public void TestGridsSplitContainer()
		{
			using (var form = new ZForm())
			using (var filterControl = new TemporaryStorageRegisterFilterStripControl(new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageRegisterModule.AppCode), new TemporaryStorageRegisterFilterBusinessObject()))
			{
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
	}

		void AssertGridColumnNames(ColumnDetails[] orderedColumnDetails, Func<TemporaryStorageRegisterFilterStripControl, ZFilterGrid> getGrid)
		{
			using (var form = new ZForm())
			using (var filterControl = new TemporaryStorageRegisterFilterStripControl(new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageRegisterModule.AppCode), new TemporaryStorageRegisterFilterBusinessObject()))
			{
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
	}

		void AssertGridDefaultColumnOrder(ColumnDetails[] orderedColumnDetails, Func<TemporaryStorageRegisterFilterStripControl, ZFilterGrid> getGrid)
		{
			using (var form = new ZForm())
			using (var filterControl = new TemporaryStorageRegisterFilterStripControl(new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageRegisterModule.AppCode), new TemporaryStorageRegisterFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

			AssertSequencesEqual(orderedColumnDetails.Select(x => x.Name), getGrid(filterControl).DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
		}
	}

		void AssertGridColumnWidths(ColumnDetails[] orderedColumnDetails, Func<TemporaryStorageRegisterFilterStripControl, ZFilterGrid> getGrid)
		{
			using (var filterControl = new TemporaryStorageRegisterFilterStripControl(new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageRegisterModule.AppCode), new TemporaryStorageRegisterFilterBusinessObject()))
			{
				var grid = getGrid(filterControl);
				CombineAssertions(() =>
				{
					foreach (var column in orderedColumnDetails)
					{
						AssertEquals(column.Name, column.Width, grid.GetColumnStyle(column.Name).Width);
					}
				});
			}
		}

	ColumnDetails[] OrderedHeaderGridColumnDetails => new[]
	{
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegPremises.Schema.SRP_Code, "Premise Code", 100, premisesPrefix),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegPremises.Schema.SRP_CustomsLocation, "Premise Location", 130, premisesPrefix),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegPremises.Schema.SRP_Type, "Premise Type", 100, premisesPrefix),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_InternalReference, "Job Reference", 130),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_Reference, "TSD Number", 130),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_ArrivalDate, "Arrival Date", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PresentationDate, "Presentation Date", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType, "Previous Ref Type", 100),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_PreviousReference, "Previous Ref Number", 120),
		new ColumnDetails(CusTempStorageRegHeader.Schema.SRH_Status, "Status", 100),
		new ColumnDetails(EU.Business.CusTempStorage.TemporaryStorageHeaderGuarantee.Schema.PW_BondNumber, "Guarantee", 130, guaranteePrefix),
		new ColumnDetails(EU.Business.CusTempStorage.TemporaryStorageHeaderGuarantee.Schema.PW_BondAmount, "Liability Amount", 100, guaranteePrefix),
		new ColumnDetails(EU.Business.CusTempStorage.TemporaryStorageHeaderGuarantee.Schema.PW_RX_NKCurrency, "Currency", 75, guaranteePrefix)
	};

	const string premisesPrefix = "Premises";
	const string guaranteePrefix = "Guarantee";

	ColumnDetails[] OrderedLinesGridColumnDetails => new[]
	{
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_LineNumber, "Line Number", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_LimitDate, "Limit Date", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_GoodsOwnerIdentifier, "Owner EORI", 100),
		new ColumnDetails(nameof(ES.Business.CusTempStorage.CusTempStorageRegLine.NumberOfItems), "Number of Items", 100),
		new ColumnDetails(nameof(ES.Business.CusTempStorage.CusTempStorageRegLine.TSDItemNumbers), "TSD Item Nº", 130),
		new ColumnDetails(nameof(ES.Business.CusTempStorage.CusTempStorageRegLine.ItemCommodityCode), "Item Commodity Code", 130),
		new ColumnDetails(nameof(ES.Business.CusTempStorage.CusTempStorageRegLine.ItemGoodsDescription), "Item Goods Description", 200),
		new ColumnDetails(nameof(EU.TemporaryStorage.Business.CusTempStorageRegLine.PackagesRemainingCalculated), "Package Count", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_PackageType, "Package Type", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_PackageMarks, "Package Marks/Vehicles", 160),
		new ColumnDetails(nameof(EU.TemporaryStorage.Business.CusTempStorageRegLine.GrossWeightRemainingCalculated), "GWT Count", 130),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_GrossWeightUQ, "GWT UQ", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_CustomsStatus, "Line Status", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_LocationOfGoods, "Location of Goods", 110),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_OwnerReference, "Owner Reference Number", 160),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_OwnerReferenceType, "Owner Reference Type", 130),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_GoodsDescription, "Goods Description", 100),
		new ColumnDetails(nameof(EU.TemporaryStorage.Business.CusTempStorageRegLine.BondAmountRemainingCalculated), "Liability Amount Remaining", 150),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_UnionStatus, "Union Status", 80),
		new ColumnDetails(EU.TemporaryStorage.Business.CusTempStorageRegLine.Schema.SRL_CustodianIdentifier, "Custodian EORI", 100),
	};

	sealed class ColumnDetails
	{
		public string Name { get; }
		public string Caption { get; }
		public int Width { get; }
		public string Prefix { get; }

		public ColumnDetails(string name, string caption, int width, string prefix = "")
		{
			Name = (prefix != ZString.Empty ? prefix + "+" : ZString.Empty) + name;
			Caption = caption;
			Width = width;
			Prefix = prefix;
		}
	}
}
