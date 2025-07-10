using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI.Testing
{
	public class OrgSupplierPartFormCustomsControlGlobalTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestPivotGridNewColumns()
		{
			using (var userControl = new OrgSupplierPartFormCustomsControlGlobal())
			{
				var pivotGrid = userControl.FindSingle<ZGrid>("PivotGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("CI_RN_NKCountryOfOrigin", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_RN_NKCountryOfOrigin)));
					AssertNotNull("CI_PrimaryPreference", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_PrimaryPreference)));
					AssertNotNull("CI_TradeControlOrderAppendix", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_TradeControlOrderAppendix)));
					AssertNotNull("CI_FEFTAArticle48", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_FEFTAArticle48)));
					AssertNotNull("CI_StorageType", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_StorageType)));
					AssertNotNull("CI_AdvanceRulingOnClassification", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_AdvanceRulingOnClassification)));
					AssertNotNull("CI_AdvanceRulingOnOrigin", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_AdvanceRulingOnOrigin)));
					AssertNotNull("CI_DutyReductionExemptionRefundCode", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_DutyReductionExemptionRefundCode)));
					AssertNotNull("CI_DomesticConsumptionTaxExemptionCode", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_DomesticConsumptionTaxExemptionCode)));
					AssertNotNull("CI_DomesticConsumptionTaxExemptionIsPartial", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartial)));
					AssertNotNull("CI_DutyReductionAmount", pivotGrid.GetColumnStyle(nameof(CusClassPartPivot.CI_DutyReductionAmount)));
				});
			}
		}
	}
}
