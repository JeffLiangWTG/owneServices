using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportDeclarationMessageSendingObjectValidation))]
sealed class ExportDeclarationMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckMessageType() => CombineAssertions(() =>
	{
		SendingObject.ShouldSend = true;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.MessageTypeInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.MessageTypeInfo, "XXX", PassarMessageTypeList.Codes.NE015);

		SendingObject.ShouldSend = false;
		SendingObject.MessageType = ZString.Empty;
		AssertNoNotifications(SendingObject.MessageTypeInfo);
		SendingObject.MessageType = "XXX";
		AssertNoNotifications(SendingObject.MessageTypeInfo);
	});

	public void TestCheckVOCReason() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory, UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1053);

		SendingObject.ShouldSend = true;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NE013;
		ValidationTestHelper.AssertErrorIfNotEntered(SendingObject.VOCReasonInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.VOCReasonInfo, RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode, RefCusCodeTestHelper.ValidCorrectionReasonTypeCode);

		SendingObject.VOCReason = ZString.Empty;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NE015;
		AssertNoNotifications(SendingObject.VOCReasonInfo);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE013;
		SendingObject.ShouldSend = false;
		AssertNoNotifications(SendingObject.VOCReasonInfo);
		SendingObject.VOCReason = RefCusCodeTestHelper.InvalidCorrectionReasonTypeCode;
		AssertNoNotifications(SendingObject.VOCReasonInfo);
	});

	public void TestCheckReasonText() => CombineAssertions(() =>
	{
		AssertRuleError(true, PassarMessageTypeList.Codes.NE013);
		AssertRuleError(false, PassarMessageTypeList.Codes.NE015);
		AssertRuleError(false, PassarMessageTypeList.Codes.NE013, reasonCode: string.Empty);
		AssertRuleError(false, PassarMessageTypeList.Codes.NE013, reasonText: "Some reason");

		void AssertRuleError(bool errorExpected, string messageType, string reasonCode = UniversalReferenceConstants.PassarReasonCodes.Others, ZString? reasonText = null)
		{
			SendingObject.ShouldSend = true;
			SendingObject.MessageType = messageType;
			SendingObject.VOCReason = reasonCode;
			SendingObject.ReasonText = reasonText ?? ZString.Empty;
			var assertionMessage = $"MessageType={SendingObject.MessageType} ReasonCode={SendingObject.VOCReason}";
			if (errorExpected)
			{
				AssertHasErrorContaining(assertionMessage, SendingObject.ReasonTextInfo, MandatoryValidation.MustBeEntered);
				SendingObject.ShouldSend = false;
				SendingObject.Validation.ValidateReasonText();
				AssertNoErrorContaining(assertionMessage, SendingObject.ReasonTextInfo, MandatoryValidation.MustBeEntered);
			}
			else
			{
				AssertNoErrorContaining(assertionMessage, SendingObject.ReasonTextInfo, MandatoryValidation.MustBeEntered);
			}
		}
	});

	public void TestNextProcedure() => CombineAssertions(() =>
	{
		const string errorMessageIfInvalidCode = "Enter a valid Next Procedure.";
		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		RefCusCodeTestHelper.CreateNextProcedureList(Factory);

		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.NextProcedureInfo, RefCusCodeTestHelper.InvalidNextProcedureCode, RefCusCodeTestHelper.ValidNextProcedureCode, errorMessageIfInvalidCode);

		sendingObject.NextProcedure = RefCusCodeTestHelper.InvalidNextProcedureCode;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NE015;
		sendingObject.Validation.ValidateNextProcedure();
		AssertNoErrorContaining("Invalid Code, Error not NE015", sendingObject.NextProcedureInfo, errorMessageIfInvalidCode);

		sendingObject.Header.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		SendingObject.MessageType = PassarMessageTypeList.Codes.NC123;
		sendingObject.Validation.ValidateNextProcedure();
		AssertNoErrorContaining("Invalid Code, No Error on ExportDeclarationActivation", sendingObject.NextProcedureInfo, errorMessageIfInvalidCode);

		SendingObject.MessageType = PassarMessageTypeList.Codes.NE015;
		sendingObject.Validation.ValidateNextProcedure();
		AssertNoErrorContaining("Invalid Code, Error not NE015", sendingObject.NextProcedureInfo, errorMessageIfInvalidCode);
	});

	ExportDeclarationMessageSendingObject SendingObject => sendingObject ??= CreateMessageSendingObject();
	ExportDeclarationMessageSendingObject sendingObject;

	ExportDeclarationMessageSendingObject CreateMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.Invoices.AddNew();
		var invoiceLine = header.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		DoMerge(declaration);
		var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
		var sendingObjectParent = new ExportDeclarationMessageSendingObjectParent(declaration);
		return new ExportDeclarationMessageSendingObject(sendingObjectParent, entryHeader);
	}

	static void DoMerge(BaseJobDeclaration declaration)
	{
		var sendsMessagesToCustomsShutterUpperer = new SendsMessagesToCustomsShutterUpperer(throwExceptionOnInvalidOperation: false);
		sendsMessagesToCustomsShutterUpperer.AnswerToContinueWithAction = true;
		declaration.DoMerge(sendsMessagesToCustomsShutterUpperer);
	}
}
