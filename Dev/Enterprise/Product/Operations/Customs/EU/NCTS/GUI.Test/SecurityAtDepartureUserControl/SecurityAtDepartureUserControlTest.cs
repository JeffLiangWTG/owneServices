using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class SecurityAtDepartureUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestPlaceOfUnloadingCodeFindBox()
		{
			var placeOfUnloadingCodeFindBox = control.PlaceOfUnloadingCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", placeOfUnloadingCodeFindBox);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_PlaceOfUnloading), placeOfUnloadingCodeFindBox.BindTo);
			});
		}

		public void TestSpecificCircumstanceIndicatorDropEdit()
		{
			var specificCircumstanceIndicatorDropEdit = control.SpecificCircumstanceIndicatorDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", specificCircumstanceIndicatorDropEdit);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_SpecificCircumstance), specificCircumstanceIndicatorDropEdit.BindTo);
			});
		}

		public void TestPlaceOfLoadingUserControl()
		{
			var placeOfLoadingControl = control.PlaceOfLoadingUserControl;

			CombineAssertions(() =>
			{
				AssertType<SecurityAtDeparturePlaceOfLoadingUserControl>("Type", placeOfLoadingControl);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader), placeOfLoadingControl.GetBindingMember());
			});
		}

		public void TestPlaceOfUnloadingUserControl()
		{
			var placeOfUnloadingControl = control.PlaceOfUnloadingUserControl;

			CombineAssertions(() =>
			{
				AssertType<SecurityAtDeparturePlaceOfUnloadingUserControl>("Type", placeOfUnloadingControl);
				AssertEquals("BindTo", nameof(NctsHeader.MovementHeader), placeOfUnloadingControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SecurityAtDepartureUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		SecurityAtDepartureUserControl control;
	}
}
