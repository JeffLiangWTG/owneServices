using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Testing;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	sealed class NctsHeaderPhase4ValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("NctsHeader should not be null in ValueSetStrategy", () => new NctsHeaderPhase4ValueSetStrategy(null));
		}

		public void TestDefaultTHIWhenDispatchPortChanges()
		{
			SetupPortTaxes();

			var departureOffice = nctsHeader.CustomsOffices.AddNew();
			departureOffice.CY_Code = "DEP";
			departureOffice.CY_Data = "FR000120";

			AssertEquals("Prequisite: ChargePaymentOrDestinationID default value should be empty.", ZString.Empty, departureMovement.ChargePaymentOrDestinationID);
			nctsHeader.PortOfDispatch = "FRBAS";

			AssertEquals("ChargePaymentOrDestinationID should have been defaulted when PortOfDispatch changes.", "010", departureMovement.ChargePaymentOrDestinationID);
		}

		public void TestDefaultTHIWhenDepartureCustomsOfficeChanges()
		{
			SetupPortTaxes();

			var departureOffice = nctsHeader.CustomsOffices.AddNew();
			departureOffice.CY_Code = "DEP";
			departureOffice.CY_Data = ZString.Empty;
			nctsHeader.PortOfDispatch = "FRBAS";
			AssertEquals("Prequisite: ChargePaymentOrDestinationID default value should be empty.", ZString.Empty, departureMovement.ChargePaymentOrDestinationID);

			departureOffice.CY_Data = "FR000120";
			AssertEquals("ChargePaymentOrDestinationID should have been defaulted when Departure Office changes.", "010", departureMovement.ChargePaymentOrDestinationID);
		}

		void SetupPortTaxes()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusCodeType("PTX", "FR Port Tax Combination", "FR", 3);
			Factory.Save();
			referenceDataHelper.CreateHarbourRate("IMP", "010", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000120");
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
