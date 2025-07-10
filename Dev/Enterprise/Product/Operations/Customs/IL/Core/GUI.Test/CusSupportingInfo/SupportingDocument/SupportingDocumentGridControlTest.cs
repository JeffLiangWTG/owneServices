using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class SupportingDocumentGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new SupportingDocumentGridControl())
			{
				var grid = (ZGrid)control.Controls.Find("gridSupportingDocument", true).Single();
				AssertNotNull(grid);
				var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("grid Column Names", expectedColumnNames, columns.Select(x => x.ColumnName));
			}
		}

		static readonly string[] expectedColumnNames =
			new[]
			{
				"CSI_Code",
				"CSI_ReferenceNumber",
				"EDoc",
				"CSI_Status",
				"CSI_AdditionalDescription",
				"CSI_ReferenceNumber2"
			};
	}
}
