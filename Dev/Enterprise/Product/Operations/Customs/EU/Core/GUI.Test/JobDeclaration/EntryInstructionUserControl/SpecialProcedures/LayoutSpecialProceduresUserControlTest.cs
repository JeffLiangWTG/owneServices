using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class LayoutSpecialProceduresUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new LayoutSpecialProceduresUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);

					AssertType<PrimaryOwnerOfGoodsUserControl>(control.PrimaryOwnerOfGoodsUserControl);
					AssertType<OwnerOfGoodsUserControl>(control.OwnerOfGoodsUserControl);
					AssertType<FirstPlaceOfUseOrProcessingUserControl>(control.FirstPlaceOfUseOrProcessingUserControl);
					AssertType<PlaceOfUseOrProcessingGoodsLocationUserControl>(control.PlaceOfUseOrProcessingGoodsLocationUserControl);
					AssertType<PeriodForDischargeUserControl>(control.PeriodForDischargeUserControl);
					AssertType<BillOfDischargeUserControl>(control.BillOfDischargeUserControl);
					AssertType<ActivitiesAndProceduresUserControl>(control.ActivitiesAndProceduresUserControl);
					AssertType<SpecialProceduresOthersUserControl>(control.SpecialProceduresOthersUserControl);
				});
			}
		}
	}
}
