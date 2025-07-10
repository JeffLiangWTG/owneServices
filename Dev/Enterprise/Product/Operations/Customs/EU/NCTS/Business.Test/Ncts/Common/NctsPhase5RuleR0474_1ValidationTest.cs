using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPhase5RuleR0474_1ValidationTest : TestCaseWithFactory
	{
		public void TestCheckBM_TransportAtDeparture_RuleR0474_1()
		{
			var targetInfo = movementHeader.TransportAtDepartureInfo;
			using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			deciderTestContext.EnableRule(c => c.IsRuleR0474_1Active);

			CombineAssertions("When rule R0474_1 is active", () =>
			{
				movementHeader.InlandTransportModeAtDeparture = "3";
				movementHeader.TransportAtDeparture = "ABC";
				movementHeader.Trailer1IDAtDeparture = "XYZ";
				movementHeader.Trailer2IDAtDeparture = "DEF";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is not empty", targetInfo, ExpectedMessageError);

				movementHeader.TransportAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageError("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.Trailer2IDAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageError("Inland M.O.T is set 3, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.Trailer2IDAtDeparture = "DEF";
				movementHeader.Trailer1IDAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertHasMessageError("Inland M.O.T is set 3, trailer1Id is empty and trailer2Id is filled, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.Trailer2IDAtDeparture = ZString.Empty;
				movementHeader.Trailer1IDAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 3, trailer1Id and trailer2Id are empty, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.InlandTransportModeAtDeparture = "1";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 1, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "2";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 2, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "4";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 4, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "5";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 5, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "7";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 7, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "8";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 8, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
				movementHeader.InlandTransportModeAtDeparture = "9";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 9, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);
			});

			deciderTestContext.DisableRule(c => c.IsRuleR0474_1Active);
			CombineAssertions("When rule R0474_1 is inactive", () =>
			{
				movementHeader.InlandTransportModeAtDeparture = "3";
				movementHeader.TransportAtDeparture = ZString.Empty;
				movementHeader.Trailer1IDAtDeparture = "XYZ";
				movementHeader.Trailer2IDAtDeparture = "DEF";
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.Trailer2IDAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 3, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, ExpectedMessageError);

				movementHeader.Trailer2IDAtDeparture = "DEF";
				movementHeader.Trailer1IDAtDeparture = ZString.Empty;
				movementHeader.Validation.ValidateBM_TransportAtDeparture();
				AssertNoMessageError("Inland M.O.T is set 3, trailer1Id is empty and trailer2Id is filled, TransportId is empty", targetInfo, ExpectedMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = header.MovementHeader;
		}

		NctsDepartureMovementHeader movementHeader;

		const string ExpectedMessageError = "[R0474-1] Transport ID is mandatory if at least a Trailer ID is present.";
	}
}
