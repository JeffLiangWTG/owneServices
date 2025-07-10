using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSendingObjectValidation))]
sealed class ImportDeclarationMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckVOCReason() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);

		SendingObject.ShouldSend = true;
		SendingObject.VOCReason = ZString.Empty;

		SendingObject.MessageType = PassarMessageTypeList.Codes.NI015;
		ValidationTestHelper.AssertFieldIsNotMandatory(SendingObject.VOCReasonInfo);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NI014;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.VOCReasonInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.VOCReasonInfo, RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode, RefCusCodeTestHelper.ValidCorrectionReasonTypeCode);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NI016;
		ValidationTestHelper.AssertFieldIsNotMandatory(SendingObject.VOCReasonInfo);

		SendingObject.ShouldSend = false;
		SendingObject.VOCReason = ZString.Empty;
		AssertNoNotifications(SendingObject.VOCReasonInfo);
		SendingObject.VOCReason = RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode;
		AssertNoNotifications(SendingObject.VOCReasonInfo);
	});

	public void TestCheckMessageType() => CombineAssertions(() =>
	{
		SendingObject.ShouldSend = true;
		SendingObject.MessageType = ZString.Empty;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.MessageTypeInfo);

		SendingObject.Header.CH_Status = ZString.Empty;
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.MessageTypeInfo, "XXX", PassarMessageTypeList.Codes.NI015);

		SendingObject.ShouldSend = false;
		SendingObject.MessageType = ZString.Empty;
		AssertNoNotifications(SendingObject.MessageTypeInfo);
		SendingObject.MessageType = "XXX";
		AssertNoNotifications(SendingObject.MessageTypeInfo);
	});

	public void TestCheckVOCReason_R292() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);

		var reasonInfo = SendingObject.VOCReasonInfo;
		var entryInstruction = SendingObject.Header.Declaration.CustomsEntryInstructions.FirstOrDefault();

		AssertNoMessageError("No message error should be shown when values are empty", reasonInfo, ValidationMessages.Plausi.MessageR292);

		SendingObject.MessageType = MessageSubTypeCodeList.Codes.CustomsRejected;
		entryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
		SendingObject.Validation.ValidateVOCReason();
		AssertNoMessageError(reasonInfo, ValidationMessages.Plausi.MessageR292);

		SendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.ConvertProvisoryToDefinitiveDeclaration;
		AssertNoMessageError(reasonInfo, ValidationMessages.Plausi.MessageR292);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NI013;
		SendingObject.Validation.ValidateVOCReason();
		AssertHasMessageError("Message error should be shown when VOCReason = 7, MessageType = COR and CEI_Style = 2", reasonInfo, ValidationMessages.Plausi.MessageR292);

		SendingObject.MessageType = MessageSubTypeCodeList.Codes.Accepted;
		SendingObject.Validation.ValidateVOCReason();
		AssertNoMessageError(reasonInfo, ValidationMessages.Plausi.MessageR292);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NI016;
		SendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.Inspect;
		AssertNoMessageError(reasonInfo, ValidationMessages.Plausi.MessageR292);

		entryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Definitive;
		SendingObject.VOCReason = UniversalReferenceConstants.CorrectionReason.ConvertProvisoryToDefinitiveDeclaration;
		AssertNoMessageError(reasonInfo, ValidationMessages.Plausi.MessageR292);
	});

	ImportDeclarationMessageSendingObject SendingObject => sendingObject ??= CreateMessageSendingObject();
	ImportDeclarationMessageSendingObject sendingObject;

	ImportDeclarationMessageSendingObject CreateMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		var entryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		return new ImportDeclarationMessageSendingObject(entryHeader);
	}
}
