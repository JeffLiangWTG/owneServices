using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class GuaranteeConfigurationAbstractTest<G> : TestCaseWithFactory
		where G : GuaranteeConfiguration
	{
		public abstract void TestOverrideSupport();
		public abstract void TestApplySecurityToPW_Override();

		public void TestGetValidationDecider() => CombineAssertions(() =>
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull("NCTS4 Departure", configuration.GetValidationDecider(departureHeader));

			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType("NCTS5 Departure", ExpectedDeparturePhase5GuaranteeValidationDeciderType, configuration.GetValidationDecider(departureHeader));

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull("NCTS4 Arrival", configuration.GetValidationDecider(arrivalHeader));

			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertNull("NCTS5 Arrival", configuration.GetValidationDecider(arrivalHeader));
		});

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (G)Activator.CreateInstance(typeof(G));
		}
		protected G configuration;

		protected virtual Type ExpectedDeparturePhase5GuaranteeValidationDeciderType => typeof(NctsGuaranteeDeparturePhase5ValidationDecider);
	}

	sealed class GuaranteeConfigurationTest : GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
	{
		public override void TestOverrideSupport()
		{
			AssertEquals(true, configuration.OverrideSupport(Header));
		}

		public override void TestApplySecurityToPW_Override()
		{
			AssertEquals(false, configuration.ApplySecurityToPW_Override(Header));
		}

		public void TestDefaultPercentageForLiabilityAmountCalculation() => AssertEquals(25, configuration.DefaultPercentageForLiabilityAmountCalculation);

		public void TestAllowDefaultLiabilityAmount() => AssertEquals(false, configuration.AllowDefaultLiabilityAmount);

		public void TestDefaultLiabilityAmount() => AssertEquals(10000m, configuration.DefaultLiabilityAmount);

		public void TestUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods() => AssertEquals(expected: true, configuration.UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods);

		NctsHeader Header => header ??= SetUpNctsHeader();
		NctsHeader header;

		NctsHeader SetUpNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header;
		}
	}
}
