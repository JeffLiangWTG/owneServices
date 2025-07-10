using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	sealed class ReportStatisticsFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			ReportStatisticsFilterBusinessObject filterBizObj = new ReportStatisticsFilterBusinessObject();

			using (ZForm form = new ZForm())
			using (ReportStatisticsFilterControl filterControl = new ReportStatisticsFilterControl(new StmReportRunCollection(Factory), filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestReportStatisticsFilterControlHasSystemColumn()
		{
			var filterBizObj = new ReportStatisticsFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new ReportStatisticsFilterControl(new StmReportRunCollection(Factory), filterBizObj))
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				AssertEquals(true, filterControl.Grid.Columns.Contains("RRI_IsSystemDefined"));
				var column = filterControl.Grid.Columns["RRI_IsSystemDefined"];
				AssertEquals("System", column.ColumnStyle.HeaderText);
			}
		}
	}
}
