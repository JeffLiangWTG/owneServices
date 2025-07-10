using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	internal class AlternateChartofAccountsFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var accAlternateCharts = new AccAlternateChartCollection(Factory);
			var filterBO = new AlternateChartofAccountsFilterBusinessObject();

			using (var form = new ZForm())
			{
				var filterControl = new AlternateChartofAccountsFilterControl_ForTest(accAlternateCharts, filterBO);
				form.Controls.Add(filterControl);
				form.Show();

				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "AAC_Code").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "AAC_Description").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "AAC_IsGlobal").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "AAC_IsFixedLength").IsVisible);
				Assert(filterControl.Grid.Columns.First(column => column.ColumnName == "AAC_BalanceSheetStyle").IsVisible);

				var strip = filterControl.NewZFilterStrip();
				Assert(strip is AlternateGLAccountFilterStrip);
				strip.Dispose();
			}
		}
	}

	public class AlternateChartofAccountsFilterControl_ForTest : AlternateChartofAccountsFilterControl
	{
		public AlternateChartofAccountsFilterControl_ForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
		}

		public new ZFilterStrip NewZFilterStrip()
		{
			return base.NewZFilterStrip();
		}
	}
}
