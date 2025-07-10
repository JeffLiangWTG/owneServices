using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public sealed class AsycudaTransportDocumentsGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new AsycudaTransportDocumentsGridControl())
			{
				var billsGrid = (ZGrid)control.Controls.Find("gridTransportDocuments", true).First();
				var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("gridTransportDocuments Column Names",
				new[] { "CSI_CodeUserInterface",
						"CSI_ReferenceNumberUserInterface"
				}, columns.Select(x => x.ColumnName));
			}
		}
	}
}
