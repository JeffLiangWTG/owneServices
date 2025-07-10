using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	sealed class ExitControlFilterStripControlTest : TestCaseWithFactory
	{
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

		ExitControlFilterStripControl GetNewFilterControl()
		{
			var collection = new CusExitHeaderCollection<CusExitHeader>(Factory);
			var filterBizO = new ExitControlFilterBusinessObject();
			return new ExitControlFilterStripControl(collection, filterBizO);
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(CusExitHeader.Schema.CXH_JobReference, "Job Number", 150),
			(CusExitHeader.Schema.CarrierCode, "Carrier", 100),
			(CusExitHeader.Schema.CarrierName, "Carrier Name", 150),
			(CusExitHeader.Schema.ExporterCode, "Exporter", 100),
			(CusExitHeader.Schema.ExporterName, "Exporter Name", 150),
			(CusExitHeader.Schema.BranchCode, "Branch", 100),
			(CusExitHeader.Schema.BranchName, "Branch Name", 150),
			(CusExitHeader.Schema.Broker, "Broker", 100),
			(CusExitHeader.Schema.BrokerName, "Broker Name", 150),
			(CusExitHeader.Schema.ReferenceNumber, "Registration Number (ext.)", 150),
			(CusExitHeader.Schema.UniqueConsignmentReference, "Reference Number UCR", 150),
			(CusExitHeader.Schema.Consignment, "Entry/Consignment", 150),
			(CusExitHeader.Schema.Status, "Status", 100),
			(CusExitHeader.Schema.StatusDescription, "Status Description", 150),
			(CusExitHeader.Schema.MessageStatus, "Message Status", 100),
			(CusExitHeader.Schema.MessageStatusDescription, "Message Status Description", 150),
		};
	}
}
