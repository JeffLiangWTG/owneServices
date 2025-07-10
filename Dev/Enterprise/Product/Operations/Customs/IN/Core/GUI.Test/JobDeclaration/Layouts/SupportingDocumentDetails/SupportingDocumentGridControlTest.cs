using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentGridControl))]
sealed class SupportingDocumentGridControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		using (var control = new SupportingDocumentGridControl())
		{
			var grid = (ZGrid)control.Controls.Find("SupportingDocumentGrid", true).Single();
			AssertNotNull(grid);
			var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
			AssertContainsExactElementsInExactOrder("Grid Column Names", expectedColumnNames, columns.Select(x => x.ColumnName));
		}
	}

	static readonly string[] expectedColumnNames =
		new[]
		{
				"CSI_LineNo",
				"CSI_ReferenceNumber2",
				"CSI_Code",
				"OrganizationPK",
				"CSI_IssuerType"
		};
}
