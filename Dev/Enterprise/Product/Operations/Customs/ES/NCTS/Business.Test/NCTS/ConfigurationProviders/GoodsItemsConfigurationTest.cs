using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(GoodsItemsConfiguration))]
	sealed class GoodsItemsConfigurationTest : EU.NCTS.Business.Testing.GoodsItemsConfigurationAbstractTest<GoodsItemsConfiguration>
	{
		public override void TestImportMethodOfPaymentVisible()
		{
			AssertEquals(false, configuration.ImportMethodOfPaymentVisible(header));
		}

		public override void TestAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.AdditionalInfosSupport(header));
		}

		public override void TestSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.SupportingDocumentsSupport(header));
		}

		public override void TestPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.PreviousDocumentsSupport(header));
		}

		public override void TestTaxSupport()
		{
			AssertEquals(true, configuration.TaxSupport(header));
		}

		public void TestDeleteConfirmationSupport()
		{
			AssertEquals(true, configuration.DeleteConfirmationSupport(header));
		}

		public void TestIsLiabilityCalculationForArrivalSupported()
		{
			AssertEquals("IsLiabilityCalculationForArrivalSupported true in ES", true, configuration.IsLiabilityCalculationForArrivalSupported());
		}

		protected override Type ExpectedNctsPreviousDocumentConfigurationType => typeof(NctsPreviousDocumentConfiguration);

		protected override Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureCargoDescPhase5ValidationDecider);

		protected override Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalCargoDescPhase5ValidationDecider);

		protected override Type ExpectedAdditionalInfoPhase5ValidationDeciderType => typeof(NctsAdditionalInfoPhase5ValidationDecider);

		protected override bool ExpectedIsCL016CodeListFilterActive => true;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
