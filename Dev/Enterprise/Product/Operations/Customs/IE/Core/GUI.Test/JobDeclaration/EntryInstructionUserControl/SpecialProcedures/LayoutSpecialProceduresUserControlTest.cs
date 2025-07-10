using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI.Testing
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
					AssertType<IdentificationOfGoodsUserControl>(control.IdentificationOfGoodsUserControl);
					AssertType<ConditionsAndTermsUserControl>(control.ConditionsAndTermsUserControl);
					// add rest of controls here
				});
			}
		}
	}
}
