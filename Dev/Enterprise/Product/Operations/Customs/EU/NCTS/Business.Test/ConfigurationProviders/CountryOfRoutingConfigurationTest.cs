using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class CountryOfRoutingConfigurationAbstractTest<C> : TestCaseWithFactory
		where C : CountryOfRoutingConfiguration
	{
		protected virtual Type GetCountryOfRoutingDeparturePhase5ValidationDeciderForTest() => typeof(CountryOfRoutingDeparturePhase5ValidationDecider);

		public void TestGetValidationDecider()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions("When MovementType: Departure", () =>
			{
				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetValidationDecider(departureHeader));

				departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertType("NCTS5", GetCountryOfRoutingDeparturePhase5ValidationDeciderForTest(), configuration.GetValidationDecider(departureHeader));
			});

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			CombineAssertions("When MovementType: Arrival", () =>
			{
				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertNull("NCTS4", configuration.GetValidationDecider(arrivalHeader));

				arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertNull("NCTS5", configuration.GetValidationDecider(arrivalHeader));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (C)Activator.CreateInstance(typeof(C));
		}
		protected C configuration;
	}

	sealed class CountryOfRoutingConfigurationTest : CountryOfRoutingConfigurationAbstractTest<CountryOfRoutingConfiguration>
	{
	}
}
