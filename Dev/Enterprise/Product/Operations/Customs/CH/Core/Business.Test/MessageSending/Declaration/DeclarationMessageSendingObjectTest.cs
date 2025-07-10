using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class DeclarationMessageSendingObjectTest<TMessageSendingObject> : NonPersistentBusinessObjectTestCase
														where TMessageSendingObject : DeclarationMessageSendingObject
{
	protected override BusinessObject GetNewBusinessObject() => SendingObject;

	protected abstract TMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader);

	protected abstract ZString ExpectedFriendlyNameForMessageManager { get; }

	protected abstract (string MessageType, string ExpectedMessageType)[] MessageTypesForEDIMessage { get; }

	public void TestSubStyleCaption() => AssertEquals("Sub Style", SendingObject.SubStyleInfo.Description);

	public void TestDeclarationTypeCaption() => AssertEquals("Declaration Type", SendingObject.DeclarationTypeInfo.Description);

	public void TestDescriptionCaption() => AssertEquals("Description", SendingObject.DescriptionInfo.Description);

	public void TestRegistrationNumberCaption() => AssertEquals("Registration Number", SendingObject.LocalReferenceNumberInfo.Description);

	public void TestEntryStatusCaption() => AssertEquals("Entry Status", SendingObject.EntryStatusInfo.Description);

	public void TestProperties()
	{
		var declarationType = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
		var subStyle = "BB";
		var description = "Description";
		var registrationNumber = "1234";
		var entryStatus = Common.Shared.EntryStatusList.Codes.Clear;

		var entryHeader = SendingObject.Header;
		var entryInstruction = entryHeader.EntryInstruction;
		entryInstruction.CEI_Style = declarationType;
		entryInstruction.CEI_SubStyle = subStyle;
		entryInstruction.CEI_Description = description;
		entryHeader.CH_BGMReference = registrationNumber;
		entryHeader.CH_EntryStatus = entryStatus;

		CombineAssertions(() =>
		{
			AssertEquals(ExpectedFriendlyNameForMessageManager, SendingObject.FriendlyNameForMessageManager);
			AssertEquals(subStyle, SendingObject.SubStyle);
			AssertEquals(declarationType, SendingObject.DeclarationType);
			AssertEquals(description, SendingObject.Description);
			AssertEquals(registrationNumber, SendingObject.LocalReferenceNumber);
			AssertEquals(entryStatus, SendingObject.EntryStatus);
		});
	}

	public void TestShouldSend() => CombineAssertions(() =>
	{
		var entryHeader = Declaration.CustomsEntryHeaders.First();

		AssertEquals("Entry header not sent / sendable - ShouldSend true", true, SendingObject.ShouldSend);

		entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
		var sendingObj = CreateNewMessageSendingObject(entryHeader);
		AssertEquals("Entry header sent - ShouldSend false", false, sendingObj.ShouldSend);

		entryHeader.CH_Status = CHLogicalStatusList.Codes.Acknowledged;
		sendingObj = CreateNewMessageSendingObject(entryHeader);
		AssertEquals("Entry header acknowledged - ShouldSend false", false, sendingObj.ShouldSend);
	});

	public void TestShouldSend_ReadOnly() => CombineAssertions(() =>
	{
		var entryHeader = Declaration.CustomsEntryHeaders.First();

		AssertShouldSend_ReadOnly(false, string.Empty, true);
		AssertShouldSend_ReadOnly(false, CHLogicalStatusList.Codes.Sent, true);
		AssertShouldSend_ReadOnly(false, CHLogicalStatusList.Codes.Acknowledged, true);

		AssertShouldSend_ReadOnly(false, string.Empty, false);
		AssertShouldSend_ReadOnly(true, CHLogicalStatusList.Codes.Sent, false);
		AssertShouldSend_ReadOnly(true, CHLogicalStatusList.Codes.Acknowledged, false);

		void AssertShouldSend_ReadOnly(bool expectedValue, string entryHeaderStatus, bool resendAllowed)
		{
			Env.Security.CHCustomsDeclarationAllowResendToCustoms.IsAllowed = resendAllowed;
			entryHeader.CH_Status = entryHeaderStatus;

			AssertEquals($"EntryHeaderStatus: {entryHeaderStatus} - Resend allowed: {resendAllowed} - expected: {expectedValue}", expectedValue, SendingObject.ShouldSendInfo.ReadOnly);
		}
	});

	public void TestMessageTypeForEDIMessage() => CombineAssertions(() =>
	{
		foreach (var testPair in MessageTypesForEDIMessage)
		{
			SendingObject.Header.Declaration.JE_MessageType = testPair.MessageType;
			AssertEquals($"JE_MessageType= '{SendingObject.Header.Declaration.JE_MessageType}'", testPair.ExpectedMessageType, SendingObject.MessageTypeForEDIMessage);
		}
	});

	public void TestIsCancellationOrDataRequest() => CombineAssertions(() =>
	{
		var cancellationDataRequestMessageTypes = new string[]
		{
				PassarMessageTypeList.Codes.NE014,
				PassarMessageTypeList.Codes.NI014,
				PassarMessageTypeList.Codes.NC016,
				PassarMessageTypeList.Codes.NI016,
		};

		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals($"IsCancellationOrDataRequest({messageType})", messageType.In(cancellationDataRequestMessageTypes), SendingObject.IsCancellationOrDataRequest);
		}
	});

	protected JobDeclaration Declaration => declaration ??= CreateDeclaration(Factory);
	JobDeclaration declaration;

	JobDeclaration CreateDeclaration(BusinessObjectFactory factory)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		return declaration;
	}

	protected TMessageSendingObject SendingObject => sendingObject ??= CreateNewMessageSendingObject(Declaration.CustomsEntryHeaders.First());
	TMessageSendingObject sendingObject;
}
