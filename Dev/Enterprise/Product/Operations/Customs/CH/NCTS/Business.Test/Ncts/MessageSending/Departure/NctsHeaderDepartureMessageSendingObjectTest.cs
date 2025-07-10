using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CoreRefCusCodeListTypeCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObject))]
sealed class NctsHeaderDepartureMessageSendingObjectTest : NctsHeaderCommonMessageSendingObjectTest
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsHeaderDepartureMessageSendingObject(null));
		NctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		AssertExceptionThrown<ArgumentException>("When nctsHeader is not a departure job", () => new NctsHeaderDepartureMessageSendingObject(NctsHeader));
	});

	public void TestLookups() => CombineAssertions(() =>
	{
		AssertType<NctsHeaderDepartureMessageSendingObjectLookups>(SendingObject.Lookups);
		AssertSame("cached", SendingObject.Lookups, SendingObject.Lookups);
	});

	public void TestGetNewValidation() => AssertType<NctsHeaderDepartureMessageSendingObjectValidation>(SendingObject.Validation);

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(SendingObject.LRNInfo, caption: "Registration Number (LRN)");
		CaptionTestHelper.AssertCaptions(SendingObject.MessageTypeInfo, caption: "Message Type");
		CaptionTestHelper.AssertCaptions(SendingObject.ReasonCodeInfo, caption: "Reason Code");
		CaptionTestHelper.AssertCaptions(SendingObject.ReasonTextInfo, caption: "Reason Text");
		CaptionTestHelper.AssertCaptions<NctsHeaderDepartureMessageSendingObject>(nameof(NctsHeaderDepartureMessageSendingObject.IdentificationNumber), caption: "Identification Number");
		CaptionTestHelper.AssertCaptions<NctsHeaderDepartureMessageSendingObject>(nameof(NctsHeaderDepartureMessageSendingObject.ContactName), caption: "Contact Name");
		CaptionTestHelper.AssertCaptions<NctsHeaderDepartureMessageSendingObject>(nameof(NctsHeaderDepartureMessageSendingObject.PhoneNumber), caption: "Phone Number");
		CaptionTestHelper.AssertCaptions<NctsHeaderDepartureMessageSendingObject>(nameof(NctsHeaderDepartureMessageSendingObject.EmailAddress), caption: "Email");
		CaptionTestHelper.AssertCaptions(SendingObject.InlandTransportModeAtDepartureInfo, caption: "Inland M.O.T.");
		CaptionTestHelper.AssertCaptions(SendingObject.TransportTypeAtDepartureInfo, caption: "Type of ID");
		CaptionTestHelper.AssertCaptions(SendingObject.TransportAtDepartureInfo, caption: "Transport ID");
		CaptionTestHelper.AssertCaptions(SendingObject.AircraftIDAtDepartureInfo, caption: "Aircraft ID");
		CaptionTestHelper.AssertCaptions(SendingObject.TransportCountryAtDepartureInfo, caption: "Nationality");
		CaptionTestHelper.AssertCaptions(SendingObject.CommunicationLanguageInfo, caption: "Language");
	});

	public void TestSchema() => CombineAssertions(() =>
	{
		AssertEquals("MaxLength", 22, SendingObject.LRNInfo.MaxLength);
		AssertEquals("MaxLength", 35, SendingObject.MRNInfo.MaxLength);
		AssertEquals("MaxLength", 5, SendingObject.MessageTypeInfo.MaxLength);
		AssertEquals("MaxLength", 512, SendingObject.ReasonTextInfo.MaxLength);
	});

	public void TestRegistrationNumber()
	{
		MovementHeader.BM_PaperlessInbondNum = "22CH123456789012N0";
		AssertEquals("22CH123456789012N0", SendingObject.LRN);
	}

	public void TestReasonCode_ReadOnly() => CombineAssertions(() =>
	{
		AssertEquals("ReadOnly default", true, SendingObject.ReasonCodeInfo.ReadOnly);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT013, false, SendingObject.ReasonCodeInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT014, false, SendingObject.ReasonCodeInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT015, true, SendingObject.ReasonCodeInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT141, false, SendingObject.ReasonCodeInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT513, false, SendingObject.ReasonCodeInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NC123, true, SendingObject.ReasonCodeInfo);
	});

	public void TestReasonText_ReadOnly() => CombineAssertions(() =>
	{
		AssertEquals("ReadOnly default", true, SendingObject.ReasonTextInfo.ReadOnly);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT013, false, SendingObject.ReasonTextInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT014, false, SendingObject.ReasonTextInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT015, true, SendingObject.ReasonTextInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT141, false, SendingObject.ReasonTextInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NT513, false, SendingObject.ReasonTextInfo);
		AssertReadOnlyProperty(PassarMessageTypeList.Codes.NC123, true, SendingObject.ReasonTextInfo);
	});

	public void TestReasonCleared() => CombineAssertions(() =>
	{
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
		SendingObject.ReasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others;
		SendingObject.ReasonText = "some reason";
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		AssertEquals(nameof(SendingObject.ReasonCode), ZString.Empty, SendingObject.ReasonCode);
		AssertEquals(nameof(SendingObject.ReasonText), ZString.Empty, SendingObject.ReasonText);
	});

	public void TestActualDestinationCustomsOffice_default() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice)
			.CreateCode("DES01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES")
			.CreateCode("DEP01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
		Factory.Save();

		MovementHeader.CustomsOffices.AddNew("DEP", "DEP01");
		MovementHeader.CustomsOffices.AddNew("DES", "DES01");

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals("Default ActualDestinationCustomsOffice", "DES01", SendingObject.ActualDestinationCustomsOffice);

		SendingObject.ActualDestinationCustomsOffice = "DES02";
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals("Not default if not empty", "DES02", SendingObject.ActualDestinationCustomsOffice);
	});

	public void TestActualConsignee_default() => CombineAssertions(() =>
	{
		NctsHeader.Consignee.E2_Address1 = "consignee";

		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals("Default E2_AddressOverride", false, SendingObject.ActualConsignee.E2_AddressOverride);
		AssertEquals("Default E2_Address1", "consignee", SendingObject.ActualConsignee.E2_Address1);

		NctsHeader.Consignee.E2_AddressOverride = true;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals("Default E2_AddressOverride", true, SendingObject.ActualConsignee.E2_AddressOverride);

		SendingObject.ActualConsignee.E2_Address1 = "consignee 1";
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals("Not default if not empty", "consignee 1", SendingObject.ActualConsignee.E2_Address1);
	});

	public void TestGoodsLocation() => CombineAssertions(() =>
	{
		MovementHeader.GoodsLocation.AdditionalIdentifier = "A100";
		MovementHeader.GoodsLocation.Address.E2_AddressOverride = ZBool.True;
		MovementHeader.GoodsLocation.Address.E2_GovRegNum = "G100";

		AssertEquals("DisplayText", MovementHeader.GoodsLocation.DisplayText, SendingObject.GoodsLocation.DisplayText);
		AssertNotSame("New GoodsLocation", MovementHeader.GoodsLocation, SendingObject.GoodsLocation);
		AssertNotSame("New Address", MovementHeader.GoodsLocation.Address, SendingObject.GoodsLocation.Address);
	});

	public void TestIdentificationNumber()
	{
		var representativeOrg = Factory.New<OrgHeader>();
		representativeOrg.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "BID123");
		MovementHeader.Representative.OrganisationPK = representativeOrg.PK;
		AssertEquals("BID123", SendingObject.IdentificationNumber);
	}

	public void TestNC123Properties() => CombineAssertions(() =>
	{
		GlbStaff.CurrentUser.GS_FullName = "staff-name";
		GlbStaff.CurrentUser.GS_WorkPhone = "staff-phone";
		GlbStaff.CurrentUser.GS_EmailAddress = "staff-mail";

		AssertEquals("ContactName", "staff-name", SendingObject.ContactName);
		AssertEquals("PhoneNumber", "staff-phone", SendingObject.PhoneNumber);
		AssertEquals("EmailAddress", "staff-mail", SendingObject.EmailAddress);
	});

	public void TestNC123_defaults()
	{
		MovementHeader.BM_InlandTransportMode = "2";
		NctsHeader.BH_CommunicationLanguage = "L2";
		MovementHeader.BM_TransportAtDeparture = "T2";
		MovementHeader.BM_AircraftIDAtDeparture = "A2";
		MovementHeader.BM_TransportAtDepartureType = "Y2";
		MovementHeader.BM_RN_NKTransportAtDepartureCountry = "C2";

		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		AssertValues("Empty properties initialized");

		MovementHeader.BM_InlandTransportMode = "3";
		NctsHeader.BH_CommunicationLanguage = "L3";
		MovementHeader.BM_TransportAtDeparture = "T3";
		MovementHeader.BM_AircraftIDAtDeparture = "A3";
		MovementHeader.BM_TransportAtDepartureType = "Y3";
		MovementHeader.BM_RN_NKTransportAtDepartureCountry = "C3";

		SendingObject.MessageType = ZString.Empty;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		AssertValues("Non-empty properties not changed");

		void AssertValues(string assertionMessage)
		{
			AssertEquals($"{assertionMessage} - InlandMethodOfTransport", "2", SendingObject.InlandTransportModeAtDeparture);
			AssertEquals($"{assertionMessage} - CommunicationLanguage", "L2", SendingObject.CommunicationLanguage);
			AssertEquals($"{assertionMessage} - TransportAtDeparture", "T2", SendingObject.TransportAtDeparture);
			AssertEquals($"{assertionMessage} - AircraftIDAtDeparture", "A2", SendingObject.AircraftIDAtDeparture);
			AssertEquals($"{assertionMessage} - TransportTypeAtDeparture", "Y2", SendingObject.TransportTypeAtDeparture);
			AssertEquals($"{assertionMessage} - TransportCountryAtDeparture", "C2", SendingObject.TransportCountryAtDeparture);
		}
	}

	public void TestMessageSubTypeForEDIMessageNT013() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT013, MessageSubTypeCodeList.Codes.PassarAmendment, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT014() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT014, MessageSubTypeCodeList.Codes.PassarWithdrawal, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT015() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT015, MessageSubTypeCodeList.Codes.PassarDeclaration, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT141() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT141, MessageSubTypeCodeList.Codes.NctsNotArrivedTransitMovement, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT513() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT513, MessageSubTypeCodeList.Codes.NctsNationalDepartureAmendment, SendingObject);

	public void TestMessageSubTypeForEDIMessageNT515() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NT515, MessageSubTypeCodeList.Codes.NctsNationalDeparture, SendingObject);

	public void TestMessageSubTypeForEDIMessageNC016() => AssertMessageSubTypeForEDIMessage(PassarMessageTypeList.Codes.NC016, MessageSubTypeCodeList.Codes.PassarRequestDataJourney, SendingObject);

	public void TestToMessageStringForNT013() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT013, SendingObject);

	public void TestToMessageStringForNT014() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT014, SendingObject);

	public void TestToMessageStringForNT015() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT015, SendingObject);

	public void TestToMessageStringForNT141() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT141, SendingObject);

	public void TestToMessageStringForNT515() => AssertTestToMessageString(PassarMessageTypeList.Codes.NT515, SendingObject);

	public void TestToMessageStringForNC016() => AssertTestToMessageString(PassarMessageTypeList.Codes.NC016, SendingObject);

	public void TestIsAmendment() => CombineAssertions(() =>
	{
		var amendmentCodes = new[] { PassarMessageTypeList.Codes.NT013, PassarMessageTypeList.Codes.NT513 };

		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals(messageType, amendmentCodes.Contains(messageType), SendingObject.IsAmend);
		}
	});

	public void TestActualDestinationCustomsOffice_Caption() => AssertEquals("Actual Customs Office", DataBoundResourceStrings.GetDataForProperty(SendingObject.ActualDestinationCustomsOfficeInfo).Caption);

	public void TestActualDestinationCustomsOffice_Lookups() => AssertEquals("Lookups.ActualDestinationCustomsOfficeList", SendingObject.ActualDestinationCustomsOfficeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);

	public void TestDoubleEntryMRN_Caption() => AssertEquals("Double Entry MRN", DataBoundResourceStrings.GetDataForProperty(SendingObject.DoubleEntryMRNInfo).Caption);

	public void TestTC11DeliveryDate_Caption() => AssertEquals("TC11 Delivery Date", DataBoundResourceStrings.GetDataForProperty(SendingObject.TC11DeliveryDateInfo).Caption);

	public void TestMRN_ReadOnly() => AssertEquals("MRN is read only", true, SendingObject.MRNInfo.ReadOnly);

	public void TestShowValidationErrors() => CombineAssertions(() =>
	{
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NT015, true);
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NT141, false);
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NT013, true);
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NT014, false);
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NC016, false);
		AssertShowValidationErrors(PassarMessageTypeList.Codes.NC123, false);

		void AssertShowValidationErrors(string messageType, bool expectedShowValidationErrors)
		{
			SendingObject.MessageType = messageType;
			AssertEquals($"MessageType='{messageType}'", expectedShowValidationErrors, SendingObject.ShowValidationErrors);
		}
	});

	public void TestIsNT141() => CombineAssertions(() =>
	{
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals(messageType, messageType == PassarMessageTypeList.Codes.NT141, SendingObject.IsNT141);
		}
	});

	public void TestIsNC123() => CombineAssertions(() =>
	{
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals(messageType, messageType == PassarMessageTypeList.Codes.NC123, SendingObject.IsNC123);
		}
	});

	public void TestIsNT014() => CombineAssertions(() =>
	{
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals(messageType, messageType == PassarMessageTypeList.Codes.NT014, SendingObject.IsNT014);
		}
	});

	public void TestIsInPhase5TransitionPeriod()
	{
		IDepartureTransportMeansProvider provider = SendingObject;

		using (TemporarilySetTransitionPeriod(active: true))
		{
			AssertEquals(nameof(provider.IsInPhase5TransitionPeriod), true, provider.IsInPhase5TransitionPeriod);
		}

		using (TemporarilySetTransitionPeriod(active: false))
		{
			AssertEquals(nameof(provider.IsInPhase5TransitionPeriod), false, provider.IsInPhase5TransitionPeriod);
		}

		IDisposable TemporarilySetTransitionPeriod(bool active)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, active);
	}

	public void TestMessageType_Default()
	{
		var sendingObject = GetNewBusinessObject() as NctsHeaderDepartureMessageSendingObject;
		var firstElement = sendingObject.Lookups.MessageTypeList[0];
		AssertEquals("MessageType default", firstElement.Code, sendingObject.MessageType);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderDepartureMessageSendingObject(NctsHeader);

	NctsHeaderDepartureMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderDepartureMessageSendingObject(NctsHeader));
	NctsHeaderDepartureMessageSendingObject sendingObject;

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}

	NctsDepartureMovementHeader MovementHeader => NctsHeader.MovementHeader;

	void AssertReadOnlyProperty(string messageType, bool expectedReadOnly, ZPropertyInfo property)
	{
		SendingObject.MessageType = messageType;
		AssertEquals($"ReadOnly {SendingObject.MessageType}", expectedReadOnly, property.ReadOnly);
	}

	public void TestIsTransitOperation()
	{
		AssertEquals(true, SendingObject.IsTransitOperation);
	}
}
