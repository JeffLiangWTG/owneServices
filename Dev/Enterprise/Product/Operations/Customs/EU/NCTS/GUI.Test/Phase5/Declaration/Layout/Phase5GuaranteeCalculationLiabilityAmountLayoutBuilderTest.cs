using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder))]
	class Phase5GuaranteeCalculationLiabilityAmountLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder, CalculateLiabilityBizObj, Phase5GuaranteeCalculationLiabilityAmountControlBag>
	{
		public void TestLiabilityAmountTotalValueCalculationMethodUserControl_Visible()
		{
			using (NctsConfigurationTestHelper.TemporarilySetUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsConfigurationConfiguration(Factory, useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods: true))
			{
				var layout = ((IPanelLayoutProvider)new Phase5GuaranteeCalculationLiabilityAmountLayout()).Layout;
				AssertEquals(expected: true, layout.IsVisible(Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.LiabilityAmountTotalValueCalculationMethodUserControl, GetCalculateLiabilityBizObj()));
			}
		}

		public void TestLiabilityAmountTotalValueCalculationMethodUserControl_NotVisible()
		{
			using (NctsConfigurationTestHelper.TemporarilySetUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsConfigurationConfiguration(Factory, useDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods: false))
			{
				var layout = ((IPanelLayoutProvider)new Phase5GuaranteeCalculationLiabilityAmountLayout()).Layout;
				AssertEquals(expected: false, layout.IsVisible(Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance.LiabilityAmountTotalValueCalculationMethodUserControl, GetCalculateLiabilityBizObj()));
			}
		}

		protected override int ExpectedMaxColumns => 1;

		protected override Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder GetColumnLayoutBuilderForTesting() => new Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => base.ExpectedCaptionWidth + 20;

		CalculateLiabilityBizObj GetCalculateLiabilityBizObj()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			return new CalculateLiabilityBizObj(Factory, guarantee);
		}
	}
}
