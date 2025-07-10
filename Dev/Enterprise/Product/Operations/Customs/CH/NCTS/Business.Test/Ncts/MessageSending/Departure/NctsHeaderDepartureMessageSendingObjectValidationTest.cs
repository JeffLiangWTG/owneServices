using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;
using CoreRefCusCodeListTypeCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectValidation))]
sealed class NctsHeaderDepartureMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestMessageType() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.MessageTypeInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.MessageTypeInfo, "XXX", PassarMessageTypeList.Codes.NT015);
	});

	public void TestReasonCode() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(RefCusCodeList.PassarTypes.N1053).CreateCode("53");
		testHelper.CreateCodeList(RefCusCodeList.PassarTypes.N1054).CreateCode("54");
		testHelper.CreateCodeList(RefCusCodeList.PassarTypes.N1141).CreateCode("41");
		Factory.Save();

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
		ValidationTestHelper.AssertInvalidCodeMessageError(SendingObject.ReasonCodeInfo, "54", "53");

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT014;
		ValidationTestHelper.AssertInvalidCodeMessageError(SendingObject.ReasonCodeInfo, "53", "54");

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.ReasonCodeInfo);
		ValidationTestHelper.AssertInvalidCodeMessageError(SendingObject.ReasonCodeInfo, "54", "41");
	});

	public void TestReasonCode_NZ50016() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.NZ50016.GetRuleCodeMessagePrefix(true) + MandatoryValidation.MustBeEnteredMessage("Reason Code");

		AssertRule(PassarMessageTypeList.Codes.NT014, DeparturePhaseList.Codes.Activation, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, ZString.Empty, true);
		AssertRule(PassarMessageTypeList.Codes.NT014, DeparturePhaseList.Codes.Activation, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, PassarReasonCodes.Others, false);
		AssertRule(PassarMessageTypeList.Codes.NT013, DeparturePhaseList.Codes.Activation, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, ZString.Empty, false);
		AssertRule(PassarMessageTypeList.Codes.NT014, DeparturePhaseList.Codes.Amendment, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, ZString.Empty, false);
		AssertRule(PassarMessageTypeList.Codes.NT014, DeparturePhaseList.Codes.Activation, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, ZString.Empty, false);

		void AssertRule(ZString messageType, ZString phase, ZString customsStatus, ZString reasonCode, bool expectedError)
		{
			SendingObject.MessageType = messageType;
			SendingObject.MovementHeader.BM_Phase = phase;
			SendingObject.MovementHeader.BM_CustomsStatus = customsStatus;
			SendingObject.ReasonCode = reasonCode;

			if (expectedError)
			{
				AssertHasError($"MessageType={messageType},  Phase={phase}, CustomsStatus={customsStatus}, ReasonCode={reasonCode}", SendingObject.ReasonCodeInfo, message);
			}
			else
			{
				AssertNoError($"MessageType={messageType},  Phase={phase}, CustomsStatus={customsStatus}, ReasonCode={reasonCode}", SendingObject.ReasonCodeInfo, message);
			}
		}
	});

	public void TestReasonText_NS30035() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30035(SendingObject.ReasonTextInfo.HumanReadableName);

		AssertRuleError(true);
		AssertRuleError(false, messageType: PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, tc11DeliveryDate: ZDateTime.Empty);
		AssertRuleError(false, reasonText: "some reason");

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT141, ZDateTime? tc11DeliveryDate = null, ZString? reasonText = null)
		{
			SendingObject.MessageType = messageType;
			SendingObject.TC11DeliveryDate = tc11DeliveryDate ?? ZDateTime.Now;
			SendingObject.ReasonText = reasonText ?? ZString.Empty;
			var assertionMessage = $"MessageType={SendingObject.MessageType} TC11DeliveryDate={SendingObject.TC11DeliveryDate}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
		}
	});

	public void TestReasonText_NS30093_NT013() => AssertReasonText_NS30093(PassarMessageTypeList.Codes.NT013);

	public void TestReasonText_NS30093_NT513() => AssertReasonText_NS30093(PassarMessageTypeList.Codes.NT513);

	void AssertReasonText_NS30093(string messageType) => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30093, SendingObject.ReasonTextInfo.HumanReadableName);

		AssertRuleError(true, messageType);
		AssertRuleError(false, PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, messageType, reasonCode: string.Empty);
		AssertRuleError(false, messageType, reasonText: "some reason");

		void AssertRuleError(bool errorExpected, string messageType, string reasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others, ZString? reasonText = null)
		{
			SendingObject.MessageType = messageType;
			SendingObject.ReasonCode = reasonCode;
			SendingObject.ReasonText = reasonText ?? ZString.Empty;
			var assertionMessage = $"MessageType={SendingObject.MessageType} ReasonCode={SendingObject.ReasonCode}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
		}
	});

	public void TestReasonText_NS30094() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30094, SendingObject.ReasonTextInfo.HumanReadableName);

		AssertRuleError(true);
		AssertRuleError(false, messageType: PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, reasonCode: PassarReasonCodes.Duplication);
		AssertRuleError(false, reasonText: "some reason");

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT014, string reasonCode = PassarReasonCodes.Others, ZString? reasonText = null)
		{
			SendingObject.MessageType = messageType;
			SendingObject.ReasonCode = reasonCode;
			SendingObject.ReasonText = reasonText ?? ZString.Empty;
			var assertionMessage = $"MessageType={SendingObject.MessageType} ReasonCode={SendingObject.ReasonCode}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.ReasonTextInfo, message);
			}
		}
	});

	public void TestActualDestinationCustomsOffice() => CombineAssertions(() =>
	{
		const string message = "Invalid Destination Customs Office";

		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice)
			.CreateCode("DES01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES")
			.CreateCode("DEP01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
		Factory.Save();

		AssertRuleError(true);
		AssertRuleError(false, actualDestinationCustomsOffice: "DES01");
		AssertRuleError(false, actualDestinationCustomsOffice: string.Empty);

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT141, string actualDestinationCustomsOffice = "DEP01")
		{
			SendingObject.MessageType = messageType;
			SendingObject.ActualDestinationCustomsOffice = actualDestinationCustomsOffice;
			var assertionMessage = $"MessageType={SendingObject.MessageType}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.ActualDestinationCustomsOfficeInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.ActualDestinationCustomsOfficeInfo, message);
			}
		}
	});

	public void TestActualDestinationCustomsOffice_NS30035() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30035(SendingObject.ActualDestinationCustomsOfficeInfo.HumanReadableName);

		AssertRuleError(true);
		AssertRuleError(false, messageType: PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, tc11DeliveryDate: ZDateTime.Empty);
		AssertRuleError(false, actualCustomsOffice: "CH123456");

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT141, ZDateTime? tc11DeliveryDate = null, ZString? actualCustomsOffice = null)
		{
			SendingObject.MessageType = messageType;
			SendingObject.TC11DeliveryDate = tc11DeliveryDate ?? ZDateTime.Now;
			SendingObject.ActualDestinationCustomsOffice = actualCustomsOffice ?? ZString.Empty;
			var assertionMessage = $"MessageType={SendingObject.MessageType} TC11DeliveryDate={SendingObject.TC11DeliveryDate}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.ActualDestinationCustomsOfficeInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.ActualDestinationCustomsOfficeInfo, message);
			}
		}
	});

	public void TestDoubleEntryMRN() => CombineAssertions(() =>
	{
		const string message = "[NS30006]";

		AssertRuleError(true);
		AssertRuleError(false, PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, doubleEntryMRNValid: true);

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT141, bool doubleEntryMRNValid = false)
		{
			SendingObject.MessageType = messageType;
			SendingObject.DoubleEntryMRN = doubleEntryMRNValid ? "23IT123456789012J1" : "23IT123456789012J9";
			var assertionMessage = $"MesssageType={SendingObject.MessageType} DoubleEntryMRN={SendingObject.DoubleEntryMRN}";
			if (errorExpected)
			{
				AssertHasErrorContaining(assertionMessage, SendingObject.DoubleEntryMRNInfo, message);
			}
			else
			{
				AssertNoErrorContaining(assertionMessage, SendingObject.DoubleEntryMRNInfo, message);
			}
		}
	});

	public void TestDoubleEntryMRN_NS30122() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30122, SendingObject.DoubleEntryMRNInfo.HumanReadableName);

		AssertRuleError(true);
		AssertRuleError(false, PassarMessageTypeList.Codes.NT015);
		AssertRuleError(false, reasonCode: "99");
		AssertRuleError(false, doubleEntryMRN: "00CH56789012345678");

		void AssertRuleError(bool errorExpected, string messageType = PassarMessageTypeList.Codes.NT141, string reasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Duplication, string doubleEntryMRN = "")
		{
			SendingObject.MessageType = messageType;
			SendingObject.ReasonCode = reasonCode;
			SendingObject.DoubleEntryMRN = doubleEntryMRN;
			var assertionMessage = $"MessageType={SendingObject.MessageType} ReasonCode={SendingObject.ReasonCode}";
			if (errorExpected)
			{
				AssertHasError(assertionMessage, SendingObject.DoubleEntryMRNInfo, message);
			}
			else
			{
				AssertNoError(assertionMessage, SendingObject.DoubleEntryMRNInfo, message);
			}
		}
	});

	public void TestActualConsignee() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30034;

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;

		SetupWithError();
		SendingObject.ReasonText = "";
		AssertNoError("Triggered by ReasonText", SendingObject.ActualConsignee.E2_OA_AddressInfo, message);

		SetupWithError();
		SendingObject.ActualDestinationCustomsOffice = "CH123456";
		AssertNoError("Triggered by ActualDestinationCustomsOffice", SendingObject.ActualConsignee.E2_OA_AddressInfo, message);

		void SetupWithError()
		{
			SendingObject.ReasonText = "some reason";
			SendingObject.ActualDestinationCustomsOffice = ZString.Empty;
			SendingObject.ActualConsignee.E2_OA_Address = ZGuid.Empty;
			AssertHasError(SendingObject.ActualConsignee.E2_OA_AddressInfo, message);
		}
	});

	public void TestCheckInlandMethodOfTransport() => CombineAssertions(() =>
	{
		var notEnteredMessage = MandatoryValidation.MustBeEnteredMessage("Inland M.O.T.");
		var invalidCodeMessage = ListValidation.InvalidCodeError + "Inland M.O.T..";
		const string invalidCode = "1";

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingObject.InlandTransportModeAtDeparture = ZString.Empty;
		AssertHasError(AssertionMessage("empty"), SendingObject.InlandTransportModeAtDepartureInfo, notEnteredMessage);

		SendingObject.InlandTransportModeAtDeparture = invalidCode;
		AssertHasError(AssertionMessage("invalid code"), SendingObject.InlandTransportModeAtDepartureInfo, invalidCodeMessage);

		foreach (var inlandMethodOfTransport in ValidModeOfTransportListCodes)
		{
			SendingObject.InlandTransportModeAtDeparture = inlandMethodOfTransport;
			AssertNoErrors(AssertionMessage("valid code"), SendingObject.InlandTransportModeAtDepartureInfo);
		}

		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			SendingObject.MessageType = messageType;
			SendingObject.InlandTransportModeAtDeparture = ZString.Empty;
			AssertNoErrors(AssertionMessage("empty, but not NC123"), SendingObject.InlandTransportModeAtDepartureInfo);
			SendingObject.InlandTransportModeAtDeparture = invalidCode;
			AssertNoErrors(AssertionMessage("invalid code, but not NC123"), SendingObject.InlandTransportModeAtDepartureInfo);
		}

		string AssertionMessage(string info) => $"MessageType={SendingObject.MessageType} - {info}";
	});

	public void TestCheckTypeOfID() => CombineAssertions(() =>
	{
		var notEnteredMessage = "[NS30106] " + MandatoryValidation.MustBeEnteredMessage("Type of ID");
		var invalidCodeMessage = "[NS30000] " + ListValidation.InvalidCodeError + "Type of ID.";
		var validCode = SendingObject.MovementHeader.Lookups.TransportAtDepartureTypeOfIdList[0].Code;
		var invalidCode = "XX";

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;

		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		SendingObject.TransportTypeAtDeparture = ZString.Empty;
		AssertHasError(AssertionMessage("empty"), SendingObject.TransportTypeAtDepartureInfo, notEnteredMessage);
		SendingObject.TransportTypeAtDeparture = invalidCode;
		AssertHasError(AssertionMessage("invalid code"), SendingObject.TransportTypeAtDepartureInfo, invalidCodeMessage);
		SendingObject.TransportTypeAtDeparture = validCode;
		AssertNoErrors(AssertionMessage("valid code"), SendingObject.TransportTypeAtDepartureInfo);

		foreach (var inlandMethodOfTransport in ValidModeOfTransportListCodes.Where(x => x != ModeOfTransportList.Codes._9_OwnPropulsion))
		{
			SendingObject.InlandTransportModeAtDeparture = inlandMethodOfTransport;
			SendingObject.TransportTypeAtDeparture = ZString.Empty;
			AssertNoErrors(AssertionMessage("empty"), SendingObject.TransportTypeAtDepartureInfo);
			SendingObject.TransportTypeAtDeparture = invalidCode;
			AssertNoErrors(AssertionMessage("invalid code"), SendingObject.TransportTypeAtDepartureInfo);
		}

		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			SendingObject.MessageType = messageType;
			SendingObject.TransportTypeAtDeparture = ZString.Empty;
			AssertNoErrors(AssertionMessage("empty, but not NC123"), SendingObject.TransportTypeAtDepartureInfo);
			SendingObject.TransportTypeAtDeparture = invalidCode;
			AssertNoErrors(AssertionMessage("invalid code, but not NC123"), SendingObject.TransportTypeAtDepartureInfo);
		}

		string AssertionMessage(string info) => $"MessageType={SendingObject.MessageType} InlandMethodOfTransport={SendingObject.InlandTransportModeAtDeparture} - {info}";
	});

	public void TestCheckTransportID() => CombineAssertions(() =>
	{
		const string prefix = "[NS30106] ";
		var noTrainNumberMessage = prefix + MandatoryValidation.MustBeEnteredMessage("Train Number");
		var noTransportIDMessage = prefix + MandatoryValidation.MustBeEnteredMessage("Transport ID");
		var noAircraftIDMessage = prefix + MandatoryValidation.MustBeEnteredMessage("Registration Number");
		var noVesselMessage = prefix + MandatoryValidation.MustBeEnteredMessage("Vessel");

		SendingObject.AircraftIDAtDeparture = "XXX";
		AssertMandatoryMessage(ModeOfTransportList.Codes._2_RailTransport, noTrainNumberMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._3_RoadTransport, noTransportIDMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._4_AirTransport, null);
		AssertMandatoryMessage(ModeOfTransportList.Codes._7_FixedTransportInstallations, null);
		AssertMandatoryMessage(ModeOfTransportList.Codes._8_InlandWaterwayTransport, noVesselMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._9_OwnPropulsion, noTransportIDMessage);
		AssertMandatoryMessage(string.Empty, null);

		SendingObject.AircraftIDAtDeparture = ZString.Empty;
		AssertMandatoryMessage(ModeOfTransportList.Codes._2_RailTransport, noTrainNumberMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._3_RoadTransport, noTransportIDMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._4_AirTransport, noAircraftIDMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._7_FixedTransportInstallations, null);
		AssertMandatoryMessage(ModeOfTransportList.Codes._8_InlandWaterwayTransport, noVesselMessage);
		AssertMandatoryMessage(ModeOfTransportList.Codes._9_OwnPropulsion, noTransportIDMessage);
		AssertMandatoryMessage(string.Empty, null);

		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			AssertMandatoryMessage(ModeOfTransportList.Codes._2_RailTransport, null, messageType: messageType);
			AssertMandatoryMessage(ModeOfTransportList.Codes._3_RoadTransport, null, messageType: messageType);
			AssertMandatoryMessage(ModeOfTransportList.Codes._4_AirTransport, null, messageType: messageType);
			AssertMandatoryMessage(ModeOfTransportList.Codes._7_FixedTransportInstallations, null, messageType: messageType);
			AssertMandatoryMessage(ModeOfTransportList.Codes._8_InlandWaterwayTransport, null, messageType: messageType);
			AssertMandatoryMessage(ModeOfTransportList.Codes._9_OwnPropulsion, null, messageType: messageType);
		}

		void AssertMandatoryMessage(string inlandMethodOfTransport, string expectedMessage, string messageType = PassarMessageTypeList.Codes.NC123)
		{
			var assertionMessage = $"MessageType={messageType} InlandMethodOfTransport={inlandMethodOfTransport}";
			SendingObject.MessageType = messageType;
			SendingObject.InlandTransportModeAtDeparture = inlandMethodOfTransport;

			SendingObject.TransportAtDeparture = ZString.Empty;
			if (expectedMessage == null)
			{
				AssertNoErrorContaining(assertionMessage, SendingObject.TransportAtDepartureInfo, MandatoryValidation.MustBeEntered);
			}
			else
			{
				AssertHasError(assertionMessage, SendingObject.TransportAtDepartureInfo, expectedMessage);
				AssertHasErrorContaining(assertionMessage, SendingObject.TransportAtDepartureInfo, MandatoryValidation.MustBeEntered);
			}

			SendingObject.TransportAtDeparture = "X";
			AssertNoErrorContaining(assertionMessage, SendingObject.TransportAtDepartureInfo, MandatoryValidation.MustBeEntered);
		}
	});

	public void TestCheckAircraftIDAtDeparture() => CombineAssertions(() =>
	{
		const string message = "[NS30106] Please enter an Aircraft Identification.";

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;

		SendingObject.TransportAtDeparture = "XXX";
		SendingObject.AircraftIDAtDeparture = "XXX";
		AssertNoError("Both not empty", SendingObject.AircraftIDAtDepartureInfo, message);

		SendingObject.TransportAtDeparture = "XXX";
		SendingObject.AircraftIDAtDeparture = ZString.Empty;
		AssertNoError("Only Registraion No.", SendingObject.AircraftIDAtDepartureInfo, message);

		SendingObject.TransportAtDeparture = ZString.Empty;
		SendingObject.AircraftIDAtDeparture = "XXX";
		AssertNoError("Only Aircraft ID", SendingObject.AircraftIDAtDepartureInfo, message);

		SendingObject.TransportAtDeparture = ZString.Empty;
		SendingObject.AircraftIDAtDeparture = ZString.Empty;
		AssertHasError("Both empty", SendingObject.AircraftIDAtDepartureInfo, message);

		foreach (var inlandMethodOfTransport in ValidModeOfTransportListCodes.Where(x => x != ModeOfTransportList.Codes._4_AirTransport))
		{
			SendingObject.InlandTransportModeAtDeparture = inlandMethodOfTransport;
			SendingObject.Validation.ValidateAircraftIDAtDeparture();
			AssertNoError($"InlandMOT={inlandMethodOfTransport}", SendingObject.AircraftIDAtDepartureInfo, message);
		}

		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			SendingObject.MessageType = messageType;
			SendingObject.Validation.ValidateAircraftIDAtDeparture();
			AssertNoError($"MessageType={messageType}", SendingObject.AircraftIDAtDepartureInfo, message);
		}
	});

	public void TestCheckTransportNationality() => CombineAssertions(() =>
	{
		var notEnteredMessage = "[NS30106] " + MandatoryValidation.MustBeEnteredMessage("Nationality");
		const string invalidCodeMessage = "[NS30000] Enter a valid Nationality.";

		var refDataTestHelper = new RefDataTestHelper(Factory);
		refDataTestHelper.CreateCodeList(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, EconomicGroupList.Codes.EuropeanUnion).CreateCode("N1");
		refDataTestHelper.CreateCodeList(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, EconomicGroupList.Codes.ASEAN).CreateCode("N2");
		Factory.Save();

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;

		foreach (var inlandMethodOfTransport in ValidModeOfTransportListCodes.Where(x => x != ModeOfTransportList.Codes._7_FixedTransportInstallations))
		{
			SendingObject.InlandTransportModeAtDeparture = inlandMethodOfTransport;
			SendingObject.TransportCountryAtDeparture = ZString.Empty;
			AssertHasError(AssertionMessage("empty"), SendingObject.TransportCountryAtDepartureInfo, notEnteredMessage);
			SendingObject.TransportCountryAtDeparture = "N2";
			AssertHasError(AssertionMessage("invalid code"), SendingObject.TransportCountryAtDepartureInfo, invalidCodeMessage);
			SendingObject.TransportCountryAtDeparture = "N1";
			AssertNoErrors(AssertionMessage("valid code"), SendingObject.InlandTransportModeAtDepartureInfo);
		}

		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		SendingObject.TransportCountryAtDeparture = ZString.Empty;
		AssertNoErrors(AssertionMessage("empty"), SendingObject.TransportCountryAtDepartureInfo);
		SendingObject.TransportCountryAtDeparture = "N2";
		AssertNoErrors(AssertionMessage("empty"), SendingObject.TransportCountryAtDepartureInfo);

		SendingObject.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			SendingObject.MessageType = messageType;
			SendingObject.TransportCountryAtDeparture = ZString.Empty;
			AssertNoErrors(AssertionMessage("empty"), SendingObject.TransportCountryAtDepartureInfo);
			SendingObject.TransportCountryAtDeparture = "N2";
			AssertNoErrors(AssertionMessage("empty"), SendingObject.TransportCountryAtDepartureInfo);
		}

		string AssertionMessage(string info) => $"MessageType={SendingObject.MessageType} InlandMethodOfTransport={SendingObject.InlandTransportModeAtDeparture} - {info}";
	});

	public void TestCheckCommunicationLanguage() => CombineAssertions(() =>
	{
		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.CommunicationLanguageInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.CommunicationLanguageInfo, "XX", SwissCustomsLanguageList.Codes.French);

		foreach (var messageType in new PassarMessageTypeList().GetAllCodes().Where(x => x != PassarMessageTypeList.Codes.NC123))
		{
			SendingObject.MessageType = messageType;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(SendingObject.CommunicationLanguageInfo, $"MessageType={messageType}");
		}
	});

	public void TestCheckGoodsLocationDescription()
	{
		const string MessageErrorsWithinLocationOfGoods = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";

		var cusGoodsLocationAddress = SendingObject.GoodsLocation.Address;

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		cusGoodsLocationAddress.E2_GovRegNum = ZString.Empty;
		SendingObject.Validation.ValidateAll();
		AssertNoError($"{SendingObject.MessageType}: Authorization Number is empty", SendingObject.GoodsLocationDescriptionInfo, MessageErrorsWithinLocationOfGoods);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		SendingObject.Validation.ValidateAll();
		AssertHasError($"{SendingObject.MessageType}: Authorization Number is empty", SendingObject.GoodsLocationDescriptionInfo, MessageErrorsWithinLocationOfGoods);

		cusGoodsLocationAddress.E2_GovRegNum = "002";
		SendingObject.Validation.ValidateAll();
		AssertNoError($"{SendingObject.MessageType}: Authorization Number is not empty", SendingObject.GoodsLocationDescriptionInfo, MessageErrorsWithinLocationOfGoods);
	}

	NctsHeaderDepartureMessageSendingObject SendingObject => sendingObject ??= CreateMessageSendingObject();
	NctsHeaderDepartureMessageSendingObject sendingObject;

	NctsHeaderDepartureMessageSendingObject CreateMessageSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return new NctsHeaderDepartureMessageSendingObject(nctsHeader);
	}

	string[] ValidModeOfTransportListCodes => new[]
	{
			ModeOfTransportList.Codes._2_RailTransport,
			ModeOfTransportList.Codes._3_RoadTransport,
			ModeOfTransportList.Codes._4_AirTransport,
			ModeOfTransportList.Codes._7_FixedTransportInstallations,
			ModeOfTransportList.Codes._8_InlandWaterwayTransport,
			ModeOfTransportList.Codes._9_OwnPropulsion,
	};
}
