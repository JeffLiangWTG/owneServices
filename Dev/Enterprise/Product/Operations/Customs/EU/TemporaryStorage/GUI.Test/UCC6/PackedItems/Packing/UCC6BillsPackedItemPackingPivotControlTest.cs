
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	internal class UCC6BillsPackedItemPackingPivotControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6BillsPackedItemPackingPivotControl())
			{
				var temporaryStorageLinkPackagesGrid = control.FindSingle<ZGrid>("TemporaryStorageLinkPackagesGrid");
				AssertNotNull(temporaryStorageLinkPackagesGrid);

				CombineAssertions("Column", () =>
				{
					AssertEquals(3, temporaryStorageLinkPackagesGrid.ColumnStyles.Count);
					AssertEquals("IsLinked", temporaryStorageLinkPackagesGrid.GetColumnStyle(AutoTemporaryStorageLinkPackage.Schema.IsLinked).ColumnName);
					AssertEquals("PackageNumber", temporaryStorageLinkPackagesGrid.GetColumnStyle(AutoTemporaryStorageLinkPackage.Schema.PackageNumber).ColumnName);
					AssertEquals("PackQty", temporaryStorageLinkPackagesGrid.GetColumnStyle(AutoTemporaryStorageLinkPackage.Schema.PackQty).ColumnName);
				});
			}
		}
	}
}
