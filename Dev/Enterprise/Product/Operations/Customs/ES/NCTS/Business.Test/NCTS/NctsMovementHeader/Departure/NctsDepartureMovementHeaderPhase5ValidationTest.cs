using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEDocPivotCollection()
		{
			var expectedError = "A copy of TNN Document must be sent. Please add the document to eDocs tab in this declaration and then select the eDoc in the Annexes tab.";
			var header = departureMovement.Header;
			departureMovement.BM_Phase = "AH3";
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageErrorContaining("When not TNN", departureMovement.BM_InBondEntryTypeInfo, expectedError);

			departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertHasMessageErrorContaining("Message Error when empty EDocPivotCollection and TNN", departureMovement.BM_InBondEntryTypeInfo, expectedError);

			header.EDocPivotCollection.AddNew();
			departureMovement.Validation.ValidateBM_InBondEntryType();
			AssertNoMessageErrorContaining("No Message Error when not empty EDocPivotCollection and TNN", departureMovement.BM_InBondEntryTypeInfo, expectedError);
		}

		public void TestCheckBM_InlandTransportMode()
		{
			var expectedError = MandatoryValidation.YouHaveNotEntered + " an Inland M.O.T.";
			departureMovement.BM_Phase = "AH3";
			departureMovement.BM_InlandTransportMode = ZString.Empty;
			AssertNoMessageErrorContaining("When not TNN", departureMovement.BM_InlandTransportModeInfo, expectedError);

			departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
			departureMovement.Validation.ValidateBM_InlandTransportMode();
			AssertHasMessageErrorContaining("When empty and TNN", departureMovement.BM_InlandTransportModeInfo, expectedError);

			departureMovement.BM_InlandTransportMode = "AH";
			AssertNoMessageErrorContaining("When not empty TNN", departureMovement.BM_InlandTransportModeInfo, expectedError);
		}

		public void TestCheckGoodsLocationDescription()
		{
			var cusGoodsLocation = departureMovement.GoodsLocation;
			cusGoodsLocation.CGL_Qualifier = "Y";
			departureMovement.Validation.ValidateGoodsLocationDescription();

			AssertNoMessageErrors("Validation not apply in ES", departureMovement.GoodsLocationDescriptionInfo);
		}

		public void TestCheckBM_TransportAtDepartureType_Mandatory_B1891_1()
		{
			const string message = "[B1891-1] You have not entered a Type of Identification for Transport Departure.";

			using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);

			using (SetTransitionPeriod(true))
			{
				deciderTestContext.EnableRule(x => x.IsRuleB1891_1Active);
				departureMovement.BM_TransportAtDepartureType = ZString.Empty;
				AssertHasMessageError("Departure has message error", departureMovement.BM_TransportAtDepartureTypeInfo, message);

				departureMovement.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				departureMovement.Validation.ValidateBM_TransportAtDepartureType();
				AssertNoMessageErrorContaining("TNN has not message error", departureMovement.BM_TransportAtDepartureTypeInfo, message);
			}
		}

		public void TestValidateAuthorizations()
		{
			var expectedWarning = "When an ACR authorization is declared, it is mandatory to declare also an SSE authorization.";
			var authDep = departureMovement.CusAuthorizationUsages.AddNew();
			authDep.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			departureMovement.Validation.ValidateAll();
			AssertHasRowWarningContaining(authDep, expectedWarning);

			var authDep2 = departureMovement.CusAuthorizationUsages.AddNew();
			authDep2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			departureMovement.Validation.ValidateAll();
			AssertNoRowWarningContaining("Has both ACR and SSE", authDep, expectedWarning);
		}

		public void TestIsRuleTR0050Applicable()
		{
			var validation = new NctsDepartureMovementHeaderPhase5ValidationForTest(departureMovement);
			using (SetTransitionPeriod(true))
			{
				AssertEquals("In transition period", false, validation.IsRuleTR0050ApplicableExposed);
			}

			using (SetTransitionPeriod(false))
			{
				AssertEquals("Not in transition period", true, validation.IsRuleTR0050ApplicableExposed);
			}
		}

		static IDisposable SetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.NCTSTransitionPeriod,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				ZDate.Today,
				isActive);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureMovement = header.MovementHeader;
		}
		NctsDepartureMovementHeader departureMovement;

		class NctsDepartureMovementHeaderPhase5ValidationForTest : NctsDepartureMovementHeaderPhase5Validation
		{
			public NctsDepartureMovementHeaderPhase5ValidationForTest(NctsDepartureMovementHeader parent) : base(parent)
			{
			}

			public bool IsRuleTR0050ApplicableExposed => IsRuleTR0050Applicable;
		}
	}
}
