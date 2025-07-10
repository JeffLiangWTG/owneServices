using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AlternateChartCurrencyTranslationControl))]
	public class AlternateChartCurrencyTranslationControlTest : TestCaseWithFactory
	{
		public void TestLoadControl()
		{
			using (var filterControl = new AlternateChartCurrencyTranslationControl())
			{
				filterControl.Show();

				var gridColumnStyles = (filterControl.Controls.Find("AlternateChartCurrencyTranslationGrid", true).First() as ZGrid).ColumnStyles.Cast<ZGridColumnInfo>();
				Assert(gridColumnStyles.First(style => style.ColumnName == "ART_Type").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ART_AGA_AlternateAccount").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ART_CurrencyTranslationLevel").IsVisible);
				Assert(gridColumnStyles.First(style => style.ColumnName == "ART_ExRateType").IsVisible);
			}
		}
	}
}
