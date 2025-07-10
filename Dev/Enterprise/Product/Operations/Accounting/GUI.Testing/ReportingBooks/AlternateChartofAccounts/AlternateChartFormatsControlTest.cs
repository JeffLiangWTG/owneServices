using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AlternateChartFormatsControl))]
	public class AlternateChartFormatsControlTest : TestCaseWithFactory
	{
		public void TestLoadControl()
		{
			using (var filterControl = new AlternateChartFormatsControl())
			{
				filterControl.Show();

				var gridColumnStyles = filterControl.GetAlternateChartFormatGrid_ForTest().ColumnStyles.Cast<ZGridColumnInfo>();
				Assert(gridColumnStyles.First(style => style.ColumnName == "ANF_Tier").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ANF_Format").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ANF_Description").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ANF_Separator").IsVisible);
			}
		}
	}
}
