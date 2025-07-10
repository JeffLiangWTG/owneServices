using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSendingObject))]
sealed class ImportDeclarationMessageSendingObjectTest : DeclarationMessageSendingObjectTest<ImportDeclarationMessageSendingObject>
{
	protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeCodeList.Descriptions.Import;

	protected override (string MessageType, string ExpectedMessageType)[] MessageTypesForEDIMessage => new (string, string)[] { (CHJobMessageTypeList.Codes.Import, CHJobMessageTypeList.Codes.Import) };

	protected override ImportDeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new ImportDeclarationMessageSendingObject(entryHeader);

	public void TestValidationType() => AssertType<ImportDeclarationMessageSendingObjectValidation>(SendingObject.Validation);

	public void TestLookupType() => AssertType<ImportDeclarationMessageSendingObjectLookups>(SendingObject.Lookups);

	public void TestVOCReason()
	{
		var messageSendingObject = SendingObject;

		CombineAssertions(() =>
		{
			messageSendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.Inspect;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI015;
			AssertEquals("The VOCReason property should be read only if MessageType is NI015", true, messageSendingObject.VOCReasonInfo.ReadOnly);
			AssertEquals("The VOCReason property should be cleared if MessageType is NI015", ZString.Empty, messageSendingObject.VOCReason);

			messageSendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.Inspect;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI013;
			AssertEquals("The VOCReason property should not be read only if MessageType is NI013", false, messageSendingObject.VOCReasonInfo.ReadOnly);
			AssertEquals("The VOCReason property should not be cleared if MessageType is NI013", "3", messageSendingObject.VOCReason);

			messageSendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.Inspect;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI014;
			AssertEquals("The VOCReason property should not be read only if MessageType is not NI014", false, messageSendingObject.VOCReasonInfo.ReadOnly);
			AssertEquals("The VOCReason property should not be cleared if MessageType is NI014", "3", messageSendingObject.VOCReason);

			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI016;
			AssertEquals("The VOCReason property should be read only if MessageType is NI016", true, messageSendingObject.VOCReasonInfo.ReadOnly);
			AssertEquals("The VOCReason property should be cleared if MessageType is NI016", ZString.Empty, messageSendingObject.VOCReason);
		});
	}

	public void TestMessageType() => CombineAssertions(() =>
	{
		AssertMessageType(string.Empty, string.Empty, PassarMessageTypeList.Codes.NI015, false);

		AssertMessageType(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI016, false);
		AssertMessageType(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016, false);
		AssertMessageType(Common.Shared.MessageStatusList.Codes.Sent, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI016, false);

		AssertMessageType(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI013, false);
		AssertMessageType(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016, false);
		AssertMessageType(Common.Shared.MessageStatusList.Codes.AcknowledgedChange, ZString.Empty, ZString.Empty, false);

		AssertMessageType(CHLogicalStatusList.Codes.Acknowledged, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI016, false);
		AssertMessageType(CHLogicalStatusList.Codes.Acknowledged, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI016, false);
		AssertMessageType(CHLogicalStatusList.Codes.Acknowledged, ZString.Empty, ZString.Empty, false);

		AssertMessageType(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI013, false);
		AssertMessageType(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI014, false);
		AssertMessageType(CHLogicalStatusList.Codes.Invalid, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI015, false);
		AssertMessageType(CHLogicalStatusList.Codes.Invalid, ZString.Empty, ZString.Empty, false);

		AssertMessageType(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Amendment, PassarMessageTypeList.Codes.NI013, false);
		AssertMessageType(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Cancellation, PassarMessageTypeList.Codes.NI014, false);
		AssertMessageType(CHLogicalStatusList.Codes.Failed, PassarDeclarationPhaseList.Codes.Declaration, PassarMessageTypeList.Codes.NI015, false);
		AssertMessageType(CHLogicalStatusList.Codes.Failed, ZString.Empty, ZString.Empty, false);

		void AssertMessageType(string messageStatus, string messagePhaseStatus, string expectedDefaultValue, bool shouldBeReadOnly)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_Status = messageStatus;
			entryHeader.CH_PhaseStatus = messagePhaseStatus;
			var messageSendingObject = CreateNewMessageSendingObject(entryHeader);

			AssertEquals($"MessageStatus: {messageStatus}/{messagePhaseStatus} - should be read only: {shouldBeReadOnly}", shouldBeReadOnly, messageSendingObject.MessageTypeInfo.ReadOnly);
			AssertEquals($"MessageStatus: {messageStatus}/{messagePhaseStatus} - expected default Message Type: {expectedDefaultValue}", expectedDefaultValue, messageSendingObject.MessageType);
		}
	});
}
