using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AnchorStyles = System.Windows.Forms.AnchorStyles;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("SealTypeDropEdit", control.SealTypeDropEdit);
				AssertNotNull("SealQtyCalcEdit", control.SealQtyCalcEdit);
				AssertNotNull("SealTabControl", control.SealTabControl);
				AssertNotNull("PackageTabPage", control.PackageTabPage);
				AssertNotNull("ContainerTabPage", control.ContainerTabPage);
			});
		}

		public void TestControlProperties()
		{
			var sealTypeControl = control.SealTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertEquals("SealTypeControl: Binding", $"{nameof(NctsHeader.MovementHeader)}.{NctsDepartureMovementHeader.Schema.BM_SealType}", sealTypeControl.BindTo);
				AssertEquals("SealTypeControl: Description width", 205, sealTypeControl.Width);
				AssertEquals("SealQtyControl: Binding", $"{nameof(NctsHeader.MovementHeader)}.{NctsDepartureMovementHeader.Schema.BM_SealQty}", control.SealQtyCalcEdit.BindTo);
				AssertEquals("ContainerTabPage: Caption", "Containers", control.ContainerTabPage.CaptionResourceString.Caption);
				AssertEquals("PackageTabPage: Caption", "Package Seals", control.PackageTabPage.CaptionResourceString.Caption);
			});
		}

		public void TestContainersGrid()
		{
			var containersGrid = control.ContainersGrid;
			CombineAssertions(() =>
			{
				AssertEquals("ContainersGrid: Binding", nameof(NctsHeader.DepartureHeaderContainers), containersGrid.BindTo);
				AssertEquals("ContainersGrid: Anchor", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, containersGrid.Anchor);
				AssertEquals("ContainersGrid: BC_ContainerNum column width", 130, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.BC_ContainerNum)));
				AssertEquals("ContainersGrid: BC_Seal1 column width", 135, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.BC_Seal1)));
				AssertEquals("ContainersGrid: BC_Seal2 column width", 135, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.BC_Seal2)));
			});
		}

		public void TestPackageSealGrid()
		{
			var packageSealGrid = control.PackageSealGrid;
			CombineAssertions(() =>
			{
				AssertEquals("PackageSealGrid: Binding", nameof(NctsHeader.Seals), packageSealGrid.BindTo);
				AssertEquals("PackageSealGrid: Anchor", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, packageSealGrid.Anchor);
				AssertEquals("PackageSealGrid: CY_Data column width", 135, packageSealGrid.GetColumnWidth(nameof(Seal.CY_Data)));
			});
		}

		[RequiresSTA]
		public void TestPackageTabVisibleAndSelected()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_SealType = SealTypeList.Codes.PackageSeal;

			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				var packageTabPage = control.PackageTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("containerTabPage is NOT visible", false, control.ContainerTabPage.TabVisible);
					AssertEquals("packageTabPage is visible", true, packageTabPage.TabVisible);
					AssertEquals("Selected ", packageTabPage, control.SealTabControl.SelectedTab);
				});
			}
		}

		public void TestContainerTabVisibleAndSelected()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;

			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				var containerTabPage = control.ContainerTabPage;
				CombineAssertions(() =>
				{
					AssertEquals("packageTabPage is NOT visible", false, control.PackageTabPage.TabVisible);
					AssertEquals("containerTabPage is visible", true, containerTabPage.TabVisible);
					AssertEquals("Selected ", containerTabPage, control.SealTabControl.SelectedTab);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ContainersAndSealsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ContainersAndSealsUserControl control;
	}
}
