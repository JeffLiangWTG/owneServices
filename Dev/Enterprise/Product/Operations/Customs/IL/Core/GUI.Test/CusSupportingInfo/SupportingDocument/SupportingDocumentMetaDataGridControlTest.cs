using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class SupportingDocumentMetaDataGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new SupportingDocumentMetaDataGridControl())
			{
				var grid = (ZGrid)control.Controls.Find("gridSupportingDocumentMetaData", true).Single();
				AssertNotNull(grid);
				var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("grid Column Names", expectedColumnNames, columns.Select(x => x.ColumnName));
			}
		}

		static readonly string[] expectedColumnNames =
			new[]
			{
				"CY_Code",
				"Description",
				"Mandatory",
				"CY_Data"
			};
	}
}
