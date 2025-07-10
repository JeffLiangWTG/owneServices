using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleB2101ValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When departure movement header is null", () => new NctsDepartureMovementHeaderPhase5RuleB2101Validation(null));

		AssertNoExceptionThrown("When departure movement header is not  null", () => new NctsDepartureMovementHeaderPhase5RuleB2101Validation(nctsDepartureMovementHeader));
	}

	public void TestCheckBM_ActiveBorderIdentificationType()
	{
		const string expectedErrorMessage = "[B2101] You have not entered a Type of ID";

		var activeBorderIdentificationTypeInfo = nctsDepartureMovementHeader.BM_ActiveBorderIdentificationTypeInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
		{
			nctsDepartureMovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertHasMessageError("TransportBorder>'Type Of ID' is empty", activeBorderIdentificationTypeInfo, expectedErrorMessage);

			nctsDepartureMovementHeader.BM_ActiveBorderIdentificationType = "AB";
			AssertNoMessageError("TransportBorder>'Type Of ID' is NOT empty", activeBorderIdentificationTypeInfo, expectedErrorMessage);
		});

		testContext.SetNctsTransitionPeriod(true);
		nctsDepartureMovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>'Type Of ID' is empty", activeBorderIdentificationTypeInfo, expectedErrorMessage);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		nctsDepartureMovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
		AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>'Type Of ID' is empty", activeBorderIdentificationTypeInfo, expectedErrorMessage);
	}

	public void TestCheckBM_CustomsOfficeAtBorder()
	{
		const string expectedErrorMessage = "[B2101] You have not entered a Customs Office";

		var customsOfficeAtBorderInfo = nctsDepartureMovementHeader.BM_CustomsOfficeAtBorderInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
		{
			nctsDepartureMovementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
			AssertHasMessageError("TransportBorder>'Customs Office' is empty ", customsOfficeAtBorderInfo, expectedErrorMessage);

			nctsDepartureMovementHeader.BM_CustomsOfficeAtBorder = "ABC";
			AssertNoMessageError("TransportBorder>'Customs Office' is NOT empty", customsOfficeAtBorderInfo, expectedErrorMessage);
		});

		testContext.SetNctsTransitionPeriod(true);
		nctsDepartureMovementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
		AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>'Customs Office' is empty", customsOfficeAtBorderInfo, expectedErrorMessage);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		nctsDepartureMovementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
		AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>'Customs Office' is empty", customsOfficeAtBorderInfo, expectedErrorMessage);
	}

	public void TestCheckBM_TOLCarrierID()
	{
		const string expectedErrorMessage = "[B2101] You have not entered a Transport ID";

		var tOLCarrierIDInfo = nctsDepartureMovementHeader.BM_TOLCarrierIDInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
		{
			nctsDepartureMovementHeader.BM_TOLCarrierID = ZString.Empty;
			AssertHasMessageError("TransportBorder>'Transport ID' is empty", tOLCarrierIDInfo, expectedErrorMessage);

			nctsDepartureMovementHeader.BM_TOLCarrierID = "ABC";
			AssertNoMessageError("TransportBorder>'Transport ID' is NOT empty", tOLCarrierIDInfo, expectedErrorMessage);
		});

		testContext.SetNctsTransitionPeriod(true);
		nctsDepartureMovementHeader.BM_TOLCarrierID = ZString.Empty;
		AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>'Transport ID' is empty", tOLCarrierIDInfo, expectedErrorMessage);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		nctsDepartureMovementHeader.BM_TOLCarrierID = ZString.Empty;
		AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>'Transport ID' is empty", tOLCarrierIDInfo, expectedErrorMessage);
	}

	public void TestCheckBM_RN_NKTOLCarrierNationality()
	{
		const string expectedErrorMessage = "[B2101] You have not entered a Nationality";

		var nKTOLCarrierNationalityInfo = nctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationalityInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When B2101 active and TransitionPeriod OFF", () =>
		{
			nctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertHasMessageError("TransportBorder>'Nationality' is empty", nKTOLCarrierNationalityInfo, expectedErrorMessage);

			nctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Italy;
			AssertNoMessageError("TransportBorder>'Nationality' is NOT empty", nKTOLCarrierNationalityInfo, expectedErrorMessage);
		});

		testContext.SetNctsTransitionPeriod(true);
		nctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
		AssertNoMessageError("When B2101 active and TransitionPeriod ON and TransportBorder>'Nationality' is empty", nKTOLCarrierNationalityInfo, expectedErrorMessage);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		nctsDepartureMovementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
		AssertNoMessageError("When B2101 disable and TransitionPeriod OFF and TransportBorder>'Nationality' is empty", nKTOLCarrierNationalityInfo, expectedErrorMessage);
	}

	public void TestCheckBM_TransportAtDepartureType()
	{
		var targetInfo = nctsDepartureMovementHeader.BM_TransportAtDepartureTypeInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When RuleB2101: true, Transition Period: false", () =>
		{
			Assert("BM_TransportAtDepartureType, BM_TransportAtDeparture, BM_RN_NKTransportAtDepartureCountry are empty", string.Empty, string.Empty, string.Empty, false);
			Assert("BM_TransportAtDepartureType, BM_RN_NKTransportAtDepartureCountry are empty", string.Empty, "12345", string.Empty, true);
			Assert("BM_TransportAtDepartureType, BM_TransportAtDeparture are empty", string.Empty, string.Empty, "IT", true);
			Assert("BM_TransportAtDepartureType is empty", string.Empty, "12345", "IT", true);
			Assert("BM_TransportAtDepartureType, BM_TransportAtDeparture, BM_RN_NKTransportAtDepartureCountry are populated", "10", "12345", "IT", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions("When RuleN0002: true, Transition Period: false", () =>
		{
			Assert("BM_TransportAtDepartureType is empty, RuleN0002 isn't applied", string.Empty, "12345", "IT", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_TransportAtDepartureType is empty, RuleN0002 is applied", string.Empty, "12345", "IT", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);
		testContext.SetNctsTransitionPeriod(true);
		Assert("When RuleB2101: true, Transition Period: true, BM_TransportAtDepartureType is empty", string.Empty, "12345", "IT", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert("When RuleB2101: false, Transition Period: false, BM_TransportAtDepartureType is empty", string.Empty, "12345", "IT", false);
		return;

		void Assert(string message, string transportAtDepartureType, string transportAtDeparture, string transportAtDepartureCountry, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			nctsDepartureMovementHeader.BM_TransportAtDeparture = transportAtDeparture;
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureCountry = transportAtDepartureCountry;
			nctsDepartureMovementHeader.Validation.ValidateBM_TransportAtDepartureType();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	public void TestCheckBM_TransportAtDepartureTrailer1RegNo()
	{
		var targetInfo = nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer1RegNoInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When RuleB2101: true, Transition Period: false", () =>
		{
			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer1RegNo, BM_RN_NKTransportAtDepartureTrailer1Nationality are empty", string.Empty, string.Empty, false);
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer1RegNo is empty", string.Empty, "IT", true);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			Assert("BM_InlandTransportMode = 4, BM_TransportAtDepartureTrailer1RegNo is empty", string.Empty, "IT", false);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer1RegNo, BM_RN_NKTransportAtDepartureTrailer1Nationality are populated", "TRAILER 1", "IT", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions("When RuleN0002: true, Transition Period: false", () =>
		{
			Assert("BM_TransportAtDepartureTrailer1RegNo is empty, RuleN0002 isn't applied", string.Empty, "IT", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_TransportAtDepartureTrailer1RegNo is empty, RuleN0002 is applied", string.Empty, "IT", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert("When RuleB2101: true, Transition Period: true, BM_TransportAtDepartureTrailer1RegNo is empty", string.Empty, "IT", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert("When RuleB2101: false, Transition Period: false, BM_TransportAtDepartureTrailer1RegNo is empty", string.Empty, "IT", false);
		return;

		void Assert(string message, string transportAtDepartureTrailer1RegNo, string transportAtDepartureTrailer1Nationality, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer1RegNo = transportAtDepartureTrailer1RegNo;
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = transportAtDepartureTrailer1Nationality;

			nctsDepartureMovementHeader.Validation.ValidateBM_TransportAtDepartureTrailer1RegNo();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	public void TestCheckBM_TransportAtDepartureTrailer2RegNo()
	{
		var targetInfo = nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer2RegNoInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When RuleB2101: true, Transition Period: false", () =>
		{
			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer2RegNo, BM_RN_NKTransportAtDepartureTrailer2Nationality are empty", string.Empty, string.Empty, false);
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer2RegNo is empty", string.Empty, "IT", true);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			Assert("BM_InlandTransportMode = 4, BM_TransportAtDepartureTrailer2RegNo is empty", string.Empty, "IT", false);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_TransportAtDepartureTrailer2RegNo, BM_RN_NKTransportAtDepartureTrailer2Nationality are populated", "TRAILER 2", "IT", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions("When RuleN0002: true, Transition Period: false", () =>
		{
			Assert("BM_TransportAtDepartureTrailer2RegNo is empty, RuleN0002 isn't applied", string.Empty, "IT", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_TransportAtDepartureTrailer2RegNo is empty, RuleN0002 is applied", string.Empty, "IT", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert("When RuleB2101: true, Transition Period: true, BM_TransportAtDepartureTrailer2RegNo is empty", string.Empty, "IT", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert("When RuleB2101: false, Transition Period: false, BM_TransportAtDepartureTrailer2RegNo is empty", string.Empty, "IT", false);
		return;

		void Assert(string message, string transportAtDepartureTrailer2RegNo, string transportAtDepartureTrailer2Nationality, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer2RegNo = transportAtDepartureTrailer2RegNo;
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = transportAtDepartureTrailer2Nationality;

			nctsDepartureMovementHeader.Validation.ValidateBM_TransportAtDepartureTrailer2RegNo();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	public void TestCheckBM_RN_NKTransportAtDepartureCountry()
	{
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._1_SeaTransport);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._2_RailTransport);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._3_RoadTransport);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._4_AirTransport);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._7_FixedTransportInstallations);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._8_InlandWaterwayTransport);
		AssertCheckBM_RN_NKTransportAtDepartureCountry(ModeOfTransportList.Codes._9_OwnPropulsion);
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer1Nationality()
	{
		var targetInfo = nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When RuleB2101: true, Transition Period: false", () =>
		{
			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_RN_NKTransportAtDepartureTrailer1Nationality, BM_TransportAtDepartureTrailer1RegNo are empty", string.Empty, string.Empty, false);
			Assert("BM_InlandTransportMode = 3, BM_RN_NKTransportAtDepartureTrailer1Nationality is empty", string.Empty, "TRAILER 1", true);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			Assert("BM_InlandTransportMode = 4, BM_RN_NKTransportAtDepartureTrailer1Nationality is empty", string.Empty, "TRAILER 1", false);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3,BM_RN_NKTransportAtDepartureTrailer1Nationality, BM_TransportAtDepartureTrailer1RegNo are populated", "IT", "TRAILER 1", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions("When RuleN0002: true, Transition Period: false", () =>
		{
			Assert("BM_RN_NKTransportAtDepartureTrailer1Nationality is empty, RuleN0002 isn't applied", string.Empty, "TRAILER 1", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_RN_NKTransportAtDepartureTrailer1Nationality is empty, RuleN0002 is applied", string.Empty, "TRAILER 1", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert("When RuleB2101: true, Transition Period: true, BM_RN_NKTransportAtDepartureTrailer1Nationality is empty", string.Empty, "TRAILER 1", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert("When RuleB2101: false, Transition Period: false, BM_RN_NKTransportAtDepartureTrailer1Nationality is empty", string.Empty, "TRAILER 1", false);
		return;

		void Assert(string message, string transportAtDepartureTrailer1Nationality, string transportAtDepartureTrailer1RegNo, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = transportAtDepartureTrailer1Nationality;
			nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer1RegNo = transportAtDepartureTrailer1RegNo;

			nctsDepartureMovementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	public void TestCheckBM_RN_NKTransportAtDepartureTrailer2Nationality()
	{
		var targetInfo = nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions("When RuleB2101: true, Transition Period: false", () =>
		{
			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_RN_NKTransportAtDepartureTrailer2Nationality, BM_TransportAtDepartureTrailer2RegNo are empty", string.Empty, string.Empty, false);
			Assert("BM_InlandTransportMode = 3, BM_RN_NKTransportAtDepartureTrailer2Nationality is empty", string.Empty, "TRAILER 2", true);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			Assert("BM_InlandTransportMode = 4, BM_RN_NKTransportAtDepartureTrailer2Nationality is empty", string.Empty, "TRAILER 2", false);

			nctsDepartureMovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			Assert("BM_InlandTransportMode = 3, BM_RN_NKTransportAtDepartureTrailer2Nationality, BM_TransportAtDepartureTrailer2RegNo are populated", "IT", "TRAILER 2", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions("When RuleN0002: true, Transition Period: false", () =>
		{
			Assert("BM_RN_NKTransportAtDepartureTrailer2Nationality is empty, RuleN0002 isn't applied", string.Empty, "TRAILER 2", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_RN_NKTransportAtDepartureTrailer2Nationality is empty, RuleN0002 is applied", string.Empty, "TRAILER 2", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert("When RuleB2101: true, Transition Period: true, BM_RN_NKTransportAtDepartureTrailer2Nationality is empty", string.Empty, "TRAILER 2", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert("When RuleB2101: false, Transition Period: false, BM_RN_NKTransportAtDepartureTrailer2Nationality is empty", string.Empty, "TRAILER 2", false);
		return;

		void Assert(string message, string transportAtDepartureTrailer2Nationality, string transportAtDepartureTrailer2RegNo, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = transportAtDepartureTrailer2Nationality;
			nctsDepartureMovementHeader.BM_TransportAtDepartureTrailer2RegNo = transportAtDepartureTrailer2RegNo;

			nctsDepartureMovementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	public void TestCheckBM_TransportAtDeparture()
	{
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._1_SeaTransport);
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._2_RailTransport);
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._3_RoadTransport);
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._7_FixedTransportInstallations);
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._8_InlandWaterwayTransport);
		AssertCheckBM_TransportAtDeparture(ModeOfTransportList.Codes._9_OwnPropulsion);
	}

	void AssertCheckBM_TransportAtDeparture(string transportMode)
	{
		var targetInfo = nctsDepartureMovementHeader.BM_TransportAtDepartureInfo;
		nctsDepartureMovementHeader.BM_InlandTransportMode = transportMode;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions($"When RuleB2101: true, Transition Period: false, BM_InlandTransportMode: {transportMode}", () =>
		{
			Assert("BM_TransportAtDeparture, BM_TransportAtDepartureType, BM_RN_NKTransportAtDepartureCountry are empty", string.Empty, string.Empty, string.Empty, false);
			Assert("BM_TransportAtDeparture, BM_RN_NKTransportAtDepartureCountry are empty", string.Empty, "10", string.Empty, true);
			Assert("BM_TransportAtDeparture, BM_TransportAtDepartureType are empty", string.Empty, string.Empty, "IT", true);
			Assert("BM_TransportAtDeparture is empty", string.Empty, "10", "IT", true);
			Assert("BM_TransportAtDeparture, BM_TransportAtDepartureType, BM_RN_NKTransportAtDepartureCountry are populated", "12345", "10", "IT", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions($"When RuleN0002: true, Transition Period: false, BM_InlandTransportMode: {transportMode}", () =>
		{
			Assert("BM_TransportAtDeparture is empty, RuleN0002 isn't applied", string.Empty, "10", "IT", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_TransportAtDeparture is empty, RuleN0002 is applied", string.Empty, "10", "IT", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert($"When RuleB2101: true, Transition Period: true, BM_InlandTransportMode: {transportMode}, BM_TransportAtDeparture is empty", string.Empty, "10", "IT", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert($"When RuleB2101: false, Transition Period: false, BM_InlandTransportMode: {transportMode}, BM_TransportAtDeparture is empty", string.Empty, "10", "IT", false);
		return;

		void Assert(string message, string transportAtDeparture, string transportAtDepartureType, string transportAtDepartureCountry, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_TransportAtDeparture = transportAtDeparture;
			nctsDepartureMovementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureCountry = transportAtDepartureCountry;
			nctsDepartureMovementHeader.Validation.ValidateBM_TransportAtDeparture();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	void AssertCheckBM_RN_NKTransportAtDepartureCountry(string transportMode)
	{
		var targetInfo = nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureCountryInfo;
		nctsDepartureMovementHeader.BM_InlandTransportMode = transportMode;
		testContext.EnableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		CombineAssertions($"When RuleB2101: true, Transition Period: false, BM_InlandTransportMode: {transportMode}", () =>
		{
			Assert("BM_RN_NKTransportAtDepartureCountry, BM_TransportAtDepartureType, BM_TransportAtDeparture are empty", string.Empty, string.Empty, string.Empty, false);
			Assert("BM_RN_NKTransportAtDepartureCountry, BM_TransportAtDeparture are empty", string.Empty, "10", string.Empty, true);
			Assert("BM_RN_NKTransportAtDepartureCountry, BM_TransportAtDepartureType are empty", string.Empty, string.Empty, "12345", true);
			Assert("BM_RN_NKTransportAtDepartureCountry is empty", string.Empty, "10", "12345", true);
			Assert("BM_RN_NKTransportAtDepartureCountry, BM_TransportAtDepartureType, BM_TransportAtDeparture are populated", "IT", "10", "12345", false);
		});

		testContext.EnableRule(x => x.IsRuleN0002Active);
		CombineAssertions($"When RuleN0002: true, Transition Period: false, BM_InlandTransportMode: {transportMode}", () =>
		{
			Assert("BM_RN_NKTransportAtDepartureCountry is empty, RuleN0002 isn't applied", string.Empty, "10", "12345", true);

			var supportingDocument = nctsDepartureMovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "66YY";
			Assert("BM_RN_NKTransportAtDepartureCountry is empty, RuleN0002 is applied", string.Empty, "10", "12345", false);
		});

		testContext.DisableRule(x => x.IsRuleN0002Active);

		testContext.SetNctsTransitionPeriod(true);
		Assert($"When RuleB2101: true, Transition Period: true, BM_InlandTransportMode: {transportMode}, BM_RN_NKTransportAtDepartureCountry is empty", string.Empty, "10", "12345", false);

		testContext.DisableRule(x => x.IsRuleB2101Active);
		testContext.SetNctsTransitionPeriod(false);
		Assert($"When RuleB2101: false, Transition Period: false, BM_InlandTransportMode: {transportMode}, BM_RN_NKTransportAtDepartureCountry is empty", string.Empty, "10", "12345", false);
		return;

		void Assert(string message, string transportAtDepartureCountry, string transportAtDepartureType, string transportAtDeparture, bool expectedMandatory)
		{
			nctsDepartureMovementHeader.BM_RN_NKTransportAtDepartureCountry = transportAtDepartureCountry;
			nctsDepartureMovementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			nctsDepartureMovementHeader.BM_TransportAtDeparture = transportAtDeparture;
			nctsDepartureMovementHeader.Validation.ValidateBM_RN_NKTransportAtDepartureCountry();

			if (expectedMandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, message);
			}
			else
			{
				ValidationTestHelper.AssertErrorFieldIsNotMandatory(targetInfo, message);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		nctsDepartureMovementHeader = header.MovementHeader;
		testContext = nctsDepartureMovementHeader.CreateDeparturePhase5ValidationTestContext();
	}

	protected override void TearDown()
	{
		base.TearDown();
		testContext?.Dispose();
	}

	MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> testContext;
	NctsDepartureMovementHeader nctsDepartureMovementHeader;
}
