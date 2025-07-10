using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTariffFormatter()
		{
			AssertType<TariffFormatter>((GetNewBusinessObject() as ITariffFormatProvider).TariffFormatter);
		}

		public void TestSetCI_ChildType()
		{
			var cusClassPartPivot = Factory.New<CusClassPartPivot>();

			cusClassPartPivot.CI_PrimaryPreference = "1";
			cusClassPartPivot.CI_StorageType = "1";
			cusClassPartPivot.CI_AdvanceRulingOnClassification = "1";
			cusClassPartPivot.CI_AdvanceRulingOnOrigin = "1";
			cusClassPartPivot.CI_DutyReductionAmount = 123m;

			CombineAssertions(() =>
			{
				Assert(!cusClassPartPivot.CI_PrimaryPreferenceInfo.ReadOnly);
				Assert(!cusClassPartPivot.CI_StorageTypeInfo.ReadOnly);
				Assert(!cusClassPartPivot.CI_AdvanceRulingOnClassificationInfo.ReadOnly);
				Assert(!cusClassPartPivot.CI_AdvanceRulingOnOriginInfo.ReadOnly);
				Assert(!cusClassPartPivot.CI_DutyReductionAmountInfo.ReadOnly);

				cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_PrimaryPreference));
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_StorageType));
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_AdvanceRulingOnClassification));
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_AdvanceRulingOnOrigin));
				AssertEquals(ZDecimal.Zero, cusClassPartPivot.CI_DutyReductionAmount);

				Assert(cusClassPartPivot.CI_PrimaryPreferenceInfo.ReadOnly);
				Assert(cusClassPartPivot.CI_StorageTypeInfo.ReadOnly);
				Assert(cusClassPartPivot.CI_AdvanceRulingOnClassificationInfo.ReadOnly);
				Assert(cusClassPartPivot.CI_AdvanceRulingOnOriginInfo.ReadOnly);
				Assert(cusClassPartPivot.CI_DutyReductionAmountInfo.ReadOnly);

				cusClassPartPivot.CI_FEFTAArticle48 = "1";
				cusClassPartPivot.CI_DomesticConsumptionTaxExemptionCode = "1";
				cusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartial = true;
				Assert(!cusClassPartPivot.CI_FEFTAArticle48Info.ReadOnly);
				Assert(!cusClassPartPivot.CI_DomesticConsumptionTaxExemptionCodeInfo.ReadOnly);
				Assert(!cusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartialInfo.ReadOnly);

				cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_FEFTAArticle48));
				Assert(string.IsNullOrEmpty(cusClassPartPivot.CI_DomesticConsumptionTaxExemptionCode));
				Assert(!cusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartial);

				Assert(cusClassPartPivot.CI_FEFTAArticle48Info.ReadOnly);
				Assert(cusClassPartPivot.CI_DomesticConsumptionTaxExemptionCodeInfo.ReadOnly);
				Assert(cusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartialInfo.ReadOnly);
			});
		}
	}
}
