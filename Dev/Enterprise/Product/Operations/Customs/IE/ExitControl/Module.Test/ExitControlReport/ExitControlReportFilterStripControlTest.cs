using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Module.Testing
{
	sealed class ExitControlReportFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetNewFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGridColumnWidths()
		{
			using (var filterControl = GetNewFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		ExitControlReportFilterStripControl GetNewFilterControl()
		{
			var collection = new ExitControlBase.Business.CusExitReportCollection<CusExitReport>(Factory, new ZQuery());
			var filterBizO = new ExitControlReportFilterBusinessObject();
			return new ExitControlReportFilterStripControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(nameof(CusExitReport.Header) + "+" + nameof(CusExitHeader.Schema.CXH_JobReference), "Job Number", 150),
			(nameof(CusExitReport.Header) + "+" + nameof(CusExitHeader.Schema.BranchCode), "Branch", 100),
			(nameof(CusExitReport.Header) + "+" + nameof(CusExitHeader.Schema.ExporterCode), "Exporter", 100),
			(nameof(CusExitReport.Header) + "+" + nameof(CusExitHeader.Schema.CarrierCode), "Carrier", 100),
			(CusExitReport.Schema.CER_Type, "Report Type", 80),
			(CusExitReport.Schema.CER_CXC_Consignment, "Entry/Consignment", 150),
			(CusExitReport.Schema.CER_Status, "Status", 150),
			(CusExitReport.Schema.StatusDescription, "Status Description", 150),
			(CusExitReport.Schema.CER_MessageStatus, "Message Status", 200),
			(CusExitReport.Schema.MessageStatusDescription, "Message Status Description", 250),
			(CusExitReport.Schema.CER_OfficeOfExit, "Office of Exit", 120),
			(CusExitReport.Schema.CER_Calc_FormattedDateTime, "Exit/Arrival Date", 83),
		};
	}
}
