using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.Module;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Module.Testing
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
				var grid = filterControl.Grid;
				form.Controls.Add(filterControl);
				form.Show();

				foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
				{
					AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
				}
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
			(CusExitReport.ESSchema.ArrivalDate, "Arrival Date", 83),
			(CusExitReport.ESSchema.ClearanceDate, "Clearance Date", 83),
			(CusExitReport.ESSchema.Circuit, "Circuit", 80),
		};
	}
}
