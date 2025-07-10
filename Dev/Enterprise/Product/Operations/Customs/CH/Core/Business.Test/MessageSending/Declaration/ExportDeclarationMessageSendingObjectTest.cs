using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportDeclarationMessageSendingObject))]
sealed class ExportDeclarationMessageSendingObjectTest : DeclarationMessageSendingObjectTest<ExportDeclarationMessageSendingObject>
{
	protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeCodeList.Descriptions.Export;

	protected override (string MessageType, string ExpectedMessageType)[] MessageTypesForEDIMessage => new (string, string)[] { (CHJobMessageTypeList.Codes.Export, CHJobMessageTypeList.Codes.Export), (CHJobMessageTypeList.Codes.ExportDeclarationActivation, CHJobMessageTypeList.Codes.Export) };

	protected override ExportDeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new ExportDeclarationMessageSendingObject(new ExportDeclarationMessageSendingObjectParent(entryHeader.Declaration), entryHeader);

	public void TestValidationType() => AssertType<ExportDeclarationMessageSendingObjectValidation>(SendingObject.Validation);

	public void TestLookupType() => AssertType<ExportDeclarationMessageSendingObjectLookups>(SendingObject.Lookups);

	public void TestMessageTypeDefault() => AssertEquals("MessageType initial value", PassarMessageTypeList.Codes.NE015, SendingObject.MessageType);

	public void TestMessageSubTypeForEDIMessageDefault() => AssertEquals("SubType initial value", PassarDeclarationPhaseList.Codes.Declaration, SendingObject.MessageSubTypeForEDIMessage);

	public void TestVOCReasonCaptions() => CombineAssertions(() =>
	{
		var propertyData = DataBoundResourceStrings.GetDataForProperty(SendingObject.VOCReasonInfo);
		AssertEquals("ShortCaption", "Reason", propertyData.ShortCaption);
		AssertEquals("Caption", "Reason Code", propertyData.Caption);
		AssertEquals("FullDescription", "The correction/cancellation reason code to be sent to customs.", propertyData.FullDescription);
	});

	public void TestVOCReasonReadOnly() => CombineAssertions(() =>
	{
		AssertReadOnly(SendingObject.VOCReasonInfo, string.Empty, false);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NE015, true);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NE014, false);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NE013, false);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NE130, true);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NC016, true);
		AssertReadOnly(SendingObject.VOCReasonInfo, PassarMessageTypeList.Codes.NC123, true);
	});

	public void TestReasonTextCaptions() => AssertEquals("Caption", "Reason Text", SendingObject.ReasonTextInfo.Description);

	public void TestReasonTextReadOnly() => CombineAssertions(() =>
	{
		AssertReadOnly(SendingObject.ReasonTextInfo, string.Empty, false);
		AssertReadOnly(SendingObject.ReasonTextInfo, PassarMessageTypeList.Codes.NE015, true);
		AssertReadOnly(SendingObject.ReasonTextInfo, PassarMessageTypeList.Codes.NE014, false);
		AssertReadOnly(SendingObject.ReasonTextInfo, PassarMessageTypeList.Codes.NE013, false);
		AssertReadOnly(SendingObject.ReasonTextInfo, PassarMessageTypeList.Codes.NC016, true);
		AssertReadOnly(SendingObject.ReasonTextInfo, PassarMessageTypeList.Codes.NC123, true);
	});

	public void TestDeclarationNumber() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<ExportDeclarationMessageSendingObject>(nameof(SendingObject.DeclarationNumber), caption: "Entry Number (GDRN)", shortCaption: "GDRN");
		SendingObject.Header.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendingObject.Header.Declaration.DeclarationNumber = "GDRN123";
		AssertEquals("DeclarationNumber", "GDRN123", SendingObject.DeclarationNumber);
	});

	public void TestApplicationCode() => AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.CHCustomsPassar, SendingObject.ApplicationCode);

	public void TestGetApplicationReference() => AssertEquals("MessageIdentifier", SendingObject.MessageIdentifier, SendingObject.GetApplicationReference());

	public void TestToMessageStringForNE015() => CombineAssertions(() =>
	{
		SendingObject.MessageType = PassarMessageTypeList.Codes.NE015;
		var ediMessageText = SendingObject.ToMessageString();

		AssertEquals("Produces a XML", true, ediMessageText.StartsWith("<?xml"));
		AssertEquals($"Contains MessageType '{PassarMessageTypeList.Codes.NE015}'", true, ediMessageText.Contains($"<{PassarMessageTypeList.Codes.NE015}"));
	});

	public void TestIsNC123() => CombineAssertions(() =>
	{
		foreach (var messageType in new PassarMessageTypeList().GetAllCodes())
		{
			SendingObject.MessageType = messageType;
			AssertEquals($"IsNC123({messageType})", messageType == PassarMessageTypeList.Codes.NC123, SendingObject.IsNC123);
		}
	});

	public void TestNextProcedure() => CombineAssertions(() =>
	{
		var entryInstruction = Declaration.CustomsEntryInstructions[0];
		entryInstruction.CEI_NextProcedure = "1";

		AssertEquals("Initial value", "1", SendingObject.NextProcedure);
		CaptionTestHelper.AssertCaptions(SendingObject.NextProcedureInfo, caption: "Next Procedure");
	});

	void AssertReadOnly(ZPropertyInfo property, string messageType, bool expectedReadOnly)
	{
		SendingObject.MessageType = messageType;
		AssertEquals($"MessageType='{SendingObject.MessageType}'", expectedReadOnly, property.ReadOnly);
	}
}
