using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(GoodsItemsConfiguration))]
	public abstract class GoodsItemsConfigurationAbstractTest<G> : TestCaseWithFactory
	where G : GoodsItemsConfiguration
	{
		public abstract void TestImportMethodOfPaymentVisible();

		public abstract void TestAdditionalInfosSupport();

		public abstract void TestSupportingDocumentsSupport();

		public abstract void TestPreviousDocumentsSupport();

		public abstract void TestTaxSupport();

		public void TestValidationDecider_DeparturePhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			AssertType(ExpectedDeparturePhase5ValidationDeciderType, configuration.GetValidationDecider(goodsItem));
		}

		public void TestValidationDecider_ArrivalPhase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			AssertType(ExpectedArrivalPhase5ValidationDeciderType, configuration.GetValidationDecider(goodsItem));
		}

		public void TestGetAdditionalInfoValidationDecider_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			AssertType(ExpectedAdditionalInfoPhase5ValidationDeciderType, configuration.GetAdditionalInfoValidationDecider(header));
		}

		public void TestGetCusCodeListAttributeFilters()
		{
			if (ExpectedIsCL016CodeListFilterActive)
			{
				var filters = configuration.GetCusCodeListAttributeFilters();
				CombineAssertions("When ExpectedIsCL016CodeListFilterActive is true", () =>
				{
					AssertNotNull("GetCusCodeListAttributeFilters should not return null", filters);
					Assert("GetCusCodeListAttributeFilters should contain a filter for CL016", filters.Any(x => x.Key.Contains("CL016")));
				});
			}
			else
			{
				AssertEquals("When ExpectedIsCL016CodeListFilterActive is false, GetCusCodeListAttributeFilters returns a collection of count 0", configuration.GetCusCodeListAttributeFilters().Count, 0);
			}
		}

		public void TestNctsPreviousDocumentConfiguration() => AssertType(ExpectedNctsPreviousDocumentConfigurationType, configuration.NctsPreviousDocumentConfiguration);

		public void TestNctsSupportingDocumentConfiguration() => AssertType(ExpectedNctsSupportingDocumentConfigurationType, configuration.NctsSupportingDocumentConfiguration);

		protected virtual Type ExpectedDeparturePhase5ValidationDeciderType => typeof(NctsDepartureCargoDescPhase5ValidationDecider);

		protected virtual Type ExpectedArrivalPhase5ValidationDeciderType => typeof(NctsArrivalCargoDescPhase5ValidationDecider);

		protected virtual Type ExpectedAdditionalInfoPhase5ValidationDeciderType => typeof(NctsAdditionalInfoPhase5ValidationDecider);

		protected virtual Type ExpectedNctsPreviousDocumentConfigurationType => typeof(NctsPreviousDocumentConfiguration);

		protected virtual Type ExpectedNctsSupportingDocumentConfigurationType => typeof(NctsSupportingDocumentConfiguration);

		protected virtual bool ExpectedIsCL016CodeListFilterActive => false;

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (G)Activator.CreateInstance(typeof(G));
		}
		protected G configuration;
	}

	[TestedType(typeof(GoodsItemsConfiguration))]
	sealed class GoodsItemsConfigurationBaseOnlyTest : GoodsItemsConfigurationAbstractTest<GoodsItemsConfiguration>
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
			AssertEquals(false, configuration.DeleteConfirmationSupport(header));
		}

		public void TestValidationDecider_ArrivalPhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			AssertNull(configuration.GetValidationDecider(goodsItem));
		}

		public void TestValidationDecider_DeparturePhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			AssertNull(configuration.GetValidationDecider(goodsItem));
		}

		public void TestGetAdditionalInfoValidationDecider_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(configuration.GetAdditionalInfoValidationDecider(header));
		}

		public void TestIsLiabilityCalculationForArrivalSupported()
		{
			AssertEquals(false, configuration.IsLiabilityCalculationForArrivalSupported());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
