using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsHeaderMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestMessageSubType_MandatoryValidation()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.MessageSubTypeInfo);
	}

	public void TestMessageSubType_ListValidation()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		ValidationTestHelper.AssertErrorIfInvalidCode(
			sendingObject.MessageSubTypeInfo,
			invalidCode: "XX",
			validCode: "D1");
	}

	public void TestMessageType_WhenMessageTypeIsEmpty_MandatoryValidation()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.MessageTypeInfo);
	}

		public void TestMessageType_ListValidation_ForCancellation()
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = "";
			nctsHeader.CommonMovementHeader.BM_MessageStatus = "";
			nctsHeader.MovementHeader.BM_Phase = "013";
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		ValidationTestHelper.AssertErrorIfInvalidCode(
			sendingObject.MessageTypeInfo,
			invalidCode: "XX",
			validCode: "CAN");
	}

	public void TestMessageType_ListValidation_ForAmendment()
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = string.Empty;
		nctsHeader.BH_MessageStatus = string.Empty;
		nctsHeader.MovementHeader.BM_Phase = "013";

		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		ValidationTestHelper.AssertErrorIfInvalidCode(
			sendingObject.MessageTypeInfo,
			invalidCode: "XX",
			validCode: "AMD");
	}

	public void TestReason_MandatoryValidation()
	{
		AssertReason_MandatoryValidation("CAN", true);
		AssertReason_MandatoryValidation("AMD", true);
		AssertReason_MandatoryValidation("NEW", false);
	}

	public void TestReason_ListValidation()
	{
		AssertReason_ListValidation("CAN", true);
		AssertReason_ListValidation("AMD", true);
		AssertReason_ListValidation("NEW", false);
	}

	public void TestReference_MandatoryValidation()
	{
		AssertReference_MandatoryValidation("CAN", true);
		AssertReference_MandatoryValidation("AMD", true);
		AssertReference_MandatoryValidation("NEW", false);
	}

	public void TestReference_ListValidation()
	{
		AssertReference_ListValidation("CAN", true, "2", "5");
		AssertReference_ListValidation("AMD", true, "1", "5");
		AssertReference_ListValidation("NEW", false, string.Empty, string.Empty);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		CreateNewMRN();
	}

	void CreateNewMRN()
	{
		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.Branch.Company.GC_RN_NKCountryCode);
		mrn.CE_EntryNum = "MRN123";
	}

	NctsHeader nctsHeader;

	void AssertReason_MandatoryValidation(string messageType, bool errorIfNotEntered)
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		CombineAssertions($"When MessageType: {messageType}, Validations for {nameof(sendingObject.Reason)} not selected, ErrorExpected: {errorIfNotEntered}", () =>
		{
			var reasonInfo = sendingObject.ReasonInfo;
			sendingObject.MessageType = messageType;
			if (errorIfNotEntered)
			{
				ValidationTestHelper.AssertErrorIfNotEntered(reasonInfo);
			}
			else
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(reasonInfo);
			}
		});
	}

	void AssertReason_ListValidation(string messageType, bool errorIfNotEntered)
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		CombineAssertions($"MessageType: {messageType}, Validations for {nameof(sendingObject.Reason)} not in list, Error Expected: {errorIfNotEntered}", () =>
		{
			var reasonInfo = sendingObject.ReasonInfo;
			sendingObject.MessageType = messageType;
			if (errorIfNotEntered)
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(
					reasonInfo,
					invalidCode: "Z",
					validCode: "A");
			}
			else
			{
				AssertNoErrorContaining(reasonInfo, ListValidation.InvalidCodeError);
			}
		});
	}

	void AssertReference_MandatoryValidation(string messageType, bool errorIfNotEntered)
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		CombineAssertions($"MessageType: {messageType}, Validations for {nameof(sendingObject.LegislativeReference)} not selected, Error Expected: {errorIfNotEntered}", () =>
		{
			sendingObject.MessageType = messageType;
			if (errorIfNotEntered)
			{
				ValidationTestHelper.AssertErrorIfNotEntered(sendingObject.LegislativeReferenceInfo);
			}
			else
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(sendingObject.LegislativeReferenceInfo);
			}
		});
	}

	void AssertReference_ListValidation(string messageType, bool errorIfNotEntered, string validCode, string invalidCode)
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		CombineAssertions($"MessageType: {messageType}, Validations for {nameof(sendingObject.LegislativeReference)} not in list, Error Expected: {errorIfNotEntered}", () =>
		{
			var legislativeReferenceInfo = sendingObject.LegislativeReferenceInfo;
			sendingObject.MessageType = messageType;

			if (errorIfNotEntered)
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(
					legislativeReferenceInfo,
					invalidCode: invalidCode,
					validCode: validCode);
			}
			else
			{
				AssertNoErrorContaining(legislativeReferenceInfo, ListValidation.InvalidCodeError);
			}
		});
	}
}
