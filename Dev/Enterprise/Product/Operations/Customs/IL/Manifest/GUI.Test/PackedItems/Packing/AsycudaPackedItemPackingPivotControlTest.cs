using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public class AsycudaPackedItemPackingPivotControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaPackedItemPackingPivotControl())
			{
				var asycudaLinkPackagesGrid = control.FindSingle<ZGrid>("AsycudaLinkPackagesGrid");
				AssertNotNull(asycudaLinkPackagesGrid);

				CombineAssertions("Column", () =>
				{
					AssertEquals(4, asycudaLinkPackagesGrid.ColumnStyles.Count);
					AssertEquals("IsLinked", asycudaLinkPackagesGrid.GetColumnStyle(AsycudaLinkPackage.Schema.IsLinked).ColumnName);
					AssertEquals("PackageNumber", asycudaLinkPackagesGrid.GetColumnStyle(AsycudaLinkPackage.Schema.PackageNumber).ColumnName);
					AssertEquals("ContainerPK", asycudaLinkPackagesGrid.GetColumnStyle(AsycudaLinkPackage.Schema.ContainerPK).ColumnName);
					AssertEquals("PackQty", asycudaLinkPackagesGrid.GetColumnStyle(AsycudaLinkPackage.Schema.PackQty).ColumnName);
				});
			}
		}
	}
}
