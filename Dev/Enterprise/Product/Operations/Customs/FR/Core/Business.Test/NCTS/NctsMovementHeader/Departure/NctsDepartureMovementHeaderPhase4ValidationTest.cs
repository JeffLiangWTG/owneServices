using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Testing;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class NctsDepartureMovementHeaderPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckChargePaymentOrDestinationI_ListValidation()
		{
			AssertListValidationInvalidCodeMessageError(departureMovement.ChargePaymentOrDestinationIDInfo, false);

			departureMovement.ChargePaymentOrDestinationID = "AAA";
			AssertListValidationInvalidCodeMessageError(departureMovement.ChargePaymentOrDestinationIDInfo, true);
		}

		public void TestCheckChargePaymentOrDestination_MandatoryValidation()
		{
			SetupPortTaxes();
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should be empty.", 0, departureMovement.FRLookups.PaymentDestinationList.Count);
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			var departureOffice = nctsHeader.CustomsOffices.AddNew();
			departureOffice.CY_Code = "DEP";
			departureOffice.CY_Data = "FR000120";
			nctsHeader.PortOfDispatch = "FRBAS";
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should not be empty.", "010, 202", departureMovement.FRLookups.PaymentDestinationList.CodesAsString);
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			departureMovement.ChargePaymentOrDestinationID = "010";
			AssertNoMessageErrorContaining(departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);

			departureOffice.CY_Data = "FR000133";
			AssertEquals("Prerequisite: ChargePaymentOrDestinationID list should not be empty.", "333", departureMovement.FRLookups.PaymentDestinationList.CodesAsString);

			departureMovement.ChargePaymentOrDestinationID = ZString.Empty;
			AssertHasMessageErrorContaining("Message error should be added when one of the THI codes in the list has harbour rate but no value is selected.", departureMovement.ChargePaymentOrDestinationIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void SetupPortTaxes()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusCodeType("PTX", "FR Port Tax Combination", "FR", 3);
			Factory.Save();
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000120");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "202", "BORDEAUX-BASSENS", "FRBAS", "FR000120");
			UniversalReferenceDataHelperTests.CreatePortTax(referenceDataHelper, "333", "BORDEAUX-BASSENS", "FRBAS", "FR000133");
			referenceDataHelper.CreateHarbourRate("IMP", "333", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
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
