using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		var sendingObjectParent = configuration.GetNewNctsHeaderMessageSendingObjectParent(nctsHeader);
		AssertType<NctsHeaderMessageSendingObjectParent>("Sending Object Parent Type", sendingObjectParent);
	}

	public override void TestGetShouldSendDefault()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("Should Send Default", true, configuration.GetShouldSendDefault(sendingObject));
	}

	public override void TestMessageTypeList()
	{
		AssertMessageTypeCodeContainedWithCombination("NEW", expectedValue: true, customsStatus: string.Empty, messageStatus: string.Empty, phase: "014");
	}

	public void TestMessageTypeList_NoMRN()
	{
		AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "ACK", messageStatus: "ACC", phase: "014");
	}

	public void TestMessageTypeList_CancellationCode_ExpectingFalseWithValidMRN()
	{
		CreateNewMRN();

			CombineAssertions(() =>
			{
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: string.Empty, messageStatus: "SNT", phase: "XXX");
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "ACK", messageStatus: "ACC", phase: "014");
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "ACS", messageStatus: "ACC", phase: "014");
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "CAN", messageStatus: "XXX", phase: "014");
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "ACK", messageStatus: "ACC", phase: "013");
				AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: false, customsStatus: "XXX", messageStatus: "YYY", phase: "ZZZ");
			});
		}

	public void TestMessageTypeList_CancellationCode_ExpectingTrue()
	{
		CreateNewMRN();

			AssertMessageTypeCodeContainedWithCombination("CAN", expectedValue: true, customsStatus: "ACS", messageStatus: "ACC", phase: "013");
		}

	public void TestMessageTypeList_AmendmentCode_ExpectingTrue()
	{
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: true, customsStatus: string.Empty, messageStatus: string.Empty, phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: true, customsStatus: "ACK", messageStatus: "ERR", phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: true, customsStatus: "ACS", messageStatus: "FAL", phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: true, customsStatus: string.Empty, messageStatus: "ERR", phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: true, customsStatus: string.Empty, messageStatus: "FAL", phase: "013");
	}

	public void TestMessageTypeList_AmendmentCode_ExpectingFalse()
	{
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: false, customsStatus: string.Empty, messageStatus: string.Empty, phase: "014");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: false, customsStatus: "ACK", messageStatus: string.Empty, phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: false, customsStatus: string.Empty, messageStatus: "ERR", phase: "014");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: false, customsStatus: "ACK", messageStatus: "ACC", phase: "013");
		AssertMessageTypeCodeContainedWithCombination("AMD", expectedValue: false, customsStatus: "ACS", messageStatus: "SNT", phase: "013");
	}

	public void TestMessageTypeList_NewDeclarationCode_ExpectingFalse()
	{
		AssertMessageTypeCodeContainedWithCombination("NEW", expectedValue: false, customsStatus: string.Empty, messageStatus: string.Empty, phase: "013");
	}

	public override void TestSetDefaultMessageType()
	{
		nctsHeader.MovementHeader.BM_Phase = "014";
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("When BM_Phase is not 013 Default MessageType", "NEW", sendingObject.MessageType);

		nctsHeader.MovementHeader.BM_Phase = "013";
		sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("When BM_Phase is 013 Default MessageType", "AMD", sendingObject.MessageType);
	}

	public override void TestShowJustification()
	{
		AssertEquals(expected: false, configuration.ShowJustification(nctsHeader));
	}

	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(EU.NCTS.Business.NctsHeaderMessageSendingObjectValidationDecider);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}

	NctsHeader nctsHeader;

	void AssertContainsMewDeclarationMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "NEW");

	void AssertContainsCancellationMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "CAN");

	void AssertContainsAmendmentMessageType(bool expectedContainsCode)
		=> AssertContainsCode(expectedContainsCode, "AMD");

	void AssertContainsCode(bool expectedContainsCode, string codeBeingTested)
	{
		var sendingObject = GetNewSendingObject();
		var assertionMessage = $"When BM_CustomsStatus = '{nctsHeader.MovementHeader.BM_CustomsStatus}', BH_MessageStatus = '{nctsHeader.BH_MessageStatus}', BM_Phase = '{nctsHeader.MovementHeader.BM_Phase}', contains '{codeBeingTested}'?";
		AssertEquals(assertionMessage, expectedContainsCode, sendingObject.Lookups.MessageTypeList.ContainsCode(codeBeingTested));
	}
	void CreateNewMRN()
	{
		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.Branch.Company.GC_RN_NKCountryCode);
		mrn.CE_EntryNum = "MRN123";
	}

	void AssertMessageTypeCodeContainedWithCombination(string msgType, bool expectedValue, string customsStatus, string messageStatus, string phase)
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		nctsHeader.CommonMovementHeader.BM_MessageStatus = messageStatus;
		nctsHeader.MovementHeader.BM_Phase = phase;

		switch (msgType)
		{
			case "CAN":
				AssertContainsCancellationMessageType(expectedValue);
				break;
			case "AMD":
				AssertContainsAmendmentMessageType(expectedValue);
				break;
			case "NEW":
				AssertContainsMewDeclarationMessageType(expectedValue);
				break;
		}
	}

	NctsHeaderMessageSendingObject GetNewSendingObject() => new NctsHeaderMessageSendingObject(nctsHeader);
}
