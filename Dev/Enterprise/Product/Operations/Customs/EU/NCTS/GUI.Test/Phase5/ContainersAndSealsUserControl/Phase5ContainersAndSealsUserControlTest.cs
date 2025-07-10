using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5ContainersAndSealsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestContainersGrid()
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				var containerTabPage = control.ContainerTabPage;
				var containersGrid = control.ContainersGrid;
				CombineAssertions(() =>
				{
					AssertEquals("ContainerTabPage: Caption", "Containers/Equipment", containerTabPage.CaptionResourceString.Caption);
					AssertEquals("ContainersGrid: inside ContainerTabPage", true, containerTabPage.Contains(containersGrid));
					AssertEquals("ContainersGrid: Binding", nameof(NctsHeader.DepartureHeaderContainers), containersGrid.BindTo);

					var modeColumnStyle = containersGrid.GetColumnStyle(nameof(NctsDepartureHeaderContainer.BC_Mode));
					AssertEquals("ContainersGrid: BC_Mode CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, modeColumnStyle.CharacterCasing);
					AssertEquals("ContainersGrid: BC_Mode column width", 100, modeColumnStyle.Width);

					AssertEquals("ContainersGrid: BC_ContainerNum column width", 175, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.BC_ContainerNum)));
					AssertEquals("ContainersGrid: BC_ContainerNum CharacterCasing", CharacterCasing.Normal,
						containersGrid.GetColumnStyle(nameof(NctsDepartureHeaderContainer.BC_ContainerNum)).CharacterCasing);

					AssertEquals("ContainersGrid: Seal1 column width", 110, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.Seal1)));
					AssertEquals("ContainersGrid: Seal1 CharacterCasing", CharacterCasing.Normal,
						containersGrid.GetColumnStyle(nameof(NctsDepartureHeaderContainer.Seal1)).CharacterCasing);

					AssertEquals("ContainersGrid: Seal2 column width", 110, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.Seal2)));
					AssertEquals("ContainersGrid: Seal2 CharacterCasing", CharacterCasing.Normal,
						containersGrid.GetColumnStyle(nameof(NctsDepartureHeaderContainer.Seal2)).CharacterCasing);

					AssertEquals("ContainersGrid: TotalSealCount column width", 95, containersGrid.GetColumnWidth(nameof(NctsDepartureHeaderContainer.TotalSealCount)));
				});
			}
		}

		public void TestContainersAndSealsSpliiter()
		{
			var containersAndSealsSpliiter = control.ContainersAndSealsSpliiter;
			CombineAssertions(() =>
			{
				AssertEquals("Panel 1 Minimum Size", 100, containersAndSealsSpliiter.Panel1MinSize);
				AssertEquals("Panel 2 Minimum Size", 100, containersAndSealsSpliiter.Panel2MinSize);
			});
		}

		public void TestAdditionalSealsGrid()
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				var additionalSealsTabPage = control.AdditionalSealsTabPage;
				var additionalSealsGrid = control.AdditionalSealsGrid;
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Additional Seals", additionalSealsTabPage.CaptionResourceString.Caption);
					AssertEquals("AdditionalSealsGrid inside AdditionalSealsTabPage", true, additionalSealsTabPage.Contains(additionalSealsGrid));
					AssertEquals("Binding", nameof(NctsHeader.DepartureHeaderContainers) + "." + nameof(NctsDepartureHeaderContainer.AdditionalSeals), additionalSealsGrid.BindTo);

					AssertEquals("BK_SealNumber column width", 135, additionalSealsGrid.GetColumnWidth(nameof(CusSeal.BK_SealNumber)));
					AssertEquals("AdditionalSealsGrid: BK_SealNumber CharacterCasing", CharacterCasing.Normal,
						additionalSealsGrid.GetColumnStyle(nameof(CusSeal.BK_SealNumber)).CharacterCasing);
				});
			}
		}

		[RequiresSTA]
		public void TestContainerTabVisibleAndSelected()
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				var containerTabPage = control.ContainerTabPage;
				var sealTabControl = control.SealTabControl;
				CombineAssertions(() =>
				{
					AssertEquals("containerTabPage is visible", true, containerTabPage.TabVisible);
					AssertEquals("SealTabControl: Visible)", true, sealTabControl.Visible);
					AssertEquals("SealTabControl: Selected", containerTabPage, sealTabControl.SelectedTab);
					AssertEquals("AdditionalSealsTabControl is visible", true, control.AdditionalSealsTabControl.Visible);
					AssertEquals("AdditionalSealsTabPage is visible", true, control.AdditionalSealsTabPage.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestGridColumnLayoutProvider()
		{
			using (var form = new ZForm(header))
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var containerColumnsNames = control.ContainersGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
					AssertSequencesEqual("Columns", new[] { "BC_Mode", "BC_ContainerNum", "Seal1", "Seal2", "TotalSealCount" }, containerColumnsNames);

					var additionalSealsColumnsNames = control.AdditionalSealsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
					AssertSequencesEqual("Columns", new[] { "BK_SealNumber" }, additionalSealsColumnsNames);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Phase5ContainersAndSealsUserControl();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_SealType = SealTypeList.Codes.ContainerSeal;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			control.SetDataBinding(header, null);
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		NctsHeader header;
		Phase5ContainersAndSealsUserControl control;
	}
}
