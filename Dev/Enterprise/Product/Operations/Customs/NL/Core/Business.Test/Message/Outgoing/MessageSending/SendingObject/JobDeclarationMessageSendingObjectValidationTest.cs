using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class JobDeclarationMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
{
	public void TestRepresentative() => CombineAssertions(() =>
	{
		var errorMessage = "EORI number from Representative is required.";
		sendingObject.Validation.ValidateAll();
		AssertNoRowError("no Representative no EORI", sendingObject, errorMessage);

		var representative = Factory.New<OrgHeader>();
		var representativeAddress = representative.Addresses.AddNew();
		declaration.JE_OA_Representative = representativeAddress.PK;
		sendingObject.Validation.ValidateAll();
		AssertHasRowError("has Representative no EORI", sendingObject, errorMessage);

		representativeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123987465", "NL");
		sendingObject.Validation.ValidateAll();
		AssertNoRowError("has Representative has EORI", sendingObject, errorMessage);
	});

	public void TestMessageTypeValidation()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		entry.CH_EntrySubmittedDate = ZDateTime.Empty;
		var testItem = new JobDeclarationMessageSendingObject(entry);
		var mustBeEnteredError = MandatoryValidation.MustBeEntered + " a " + testItem.MessageTypeInfo.Description + ".";
		var listValidationError = ListValidation.InvalidCodeError + testItem.MessageTypeInfo.Description + ".";

		testItem.MessageType = ZString.Empty;
		AssertHasErrorContaining(testItem.MessageTypeInfo, mustBeEnteredError);

		testItem.MessageType = "ABC";
		AssertNoErrorContaining(testItem.MessageTypeInfo, mustBeEnteredError);
		AssertHasError(testItem.MessageTypeInfo, listValidationError);

		testItem.MessageType = ImportSendMessageTypes.Codes.DEC;
		AssertNoErrorContaining(testItem.MessageTypeInfo, listValidationError);
	}

	public void TestReasonOfInvalidationValidation() => CombineAssertions(() =>
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		var testItem = messageSendingObjectParent.SendingObjectsCollection[0];

		testItem.MessageType = ExportSendMessageTypes.Codes.CAN;
		ValidationTestHelper.AssertErrorIfNotEntered(testItem.ReasonForInvalidationInfo);

		testItem.ReasonForInvalidation = ZString.Empty;
		testItem.MessageType = ExportSendMessageTypes.Codes.DEC;
		AssertNoErrorContaining(testItem.ReasonForInvalidationInfo, "Please enter");
	});

	public void TestZG_TypeOfSecurityValidation()
	{
		var messageError = "Security is required for this declaration type";
		var messageWarning = "Security may be required for this declaration type";

		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Style = "B1";
			sendingObject.ZG_TypeOfSecurity = ZString.Empty;
			AssertHasMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);
			sendingObject.ZG_TypeOfSecurity = EU.Business.ExportSecurityTypeList.Codes.NotUsed;
			AssertNoMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);

			entryInstruction.CEI_Style = "B2";
			sendingObject.ZG_TypeOfSecurity = ZString.Empty;
			AssertHasMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);
			sendingObject.ZG_TypeOfSecurity = EU.Business.ExportSecurityTypeList.Codes.NotUsed;
			AssertNoMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);

			entryInstruction.CEI_Style = "C1";
			sendingObject.ZG_TypeOfSecurity = ZString.Empty;
			AssertHasWarning(sendingObject.ZG_TypeOfSecurityInfo, messageWarning);
			sendingObject.ZG_TypeOfSecurity = EU.Business.ExportSecurityTypeList.Codes.NotUsed;
			AssertNoWarning(sendingObject.ZG_TypeOfSecurityInfo, messageWarning);

			entryInstruction.CEI_Style = "B3";
			sendingObject.ZG_TypeOfSecurity = ZString.Empty;
			AssertNoMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);
			AssertNoWarning(sendingObject.ZG_TypeOfSecurityInfo, messageWarning);
			sendingObject.ZG_TypeOfSecurity = EU.Business.ExportSecurityTypeList.Codes.NotUsed;
			AssertNoMessageError(sendingObject.ZG_TypeOfSecurityInfo, messageError);
			AssertNoWarning(sendingObject.ZG_TypeOfSecurityInfo, messageWarning);
		});
	}

	public void TestMessageTypeValidation_InvoiceLine_DV1()
	{
		var errorMessage = "You cannot send VI (valuation indicators aka DV1) if there is an invoice line with valuation method different from 1 for declaration types H1, H4 or H5.";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		instruction.CEI_Style = DeclarationTypeList.Codes.H1;
		instruction.ZG_IsHighValueOvrd = true;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._2;

		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		var testItem = new JobDeclarationMessageSendingObject(entry);

		testItem.MessageType = ExportSendMessageTypes.Codes.DEC;
		AssertHasError("MessageType - DEC, IsHighValueOvrd - true, CEI_Style - H1, JI_ValuationCode - 2", testItem.MessageTypeInfo, errorMessage);

		invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
		testItem.Validation.ValidateMessageType();
		AssertNoError("MessageType - DEC, IsHighValueOvrd - true, CEI_Style - H1, JI_ValuationCode - 1", testItem.MessageTypeInfo, errorMessage);

		invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._2;
		instruction.CEI_Style = DeclarationTypeList.Codes.B1;
		testItem.Validation.ValidateMessageType();
		AssertNoError("MessageType - DEC, IsHighValueOvrd - true, CEI_Style - B1, JI_ValuationCode - 2", testItem.MessageTypeInfo, errorMessage);

		instruction.CEI_Style = DeclarationTypeList.Codes.H1;
		instruction.ZG_IsHighValueOvrd = false;
		testItem.Validation.ValidateMessageType();
		AssertNoError("MessageType - DEC, IsHighValueOvrd - false, CEI_Style - H1, JI_ValuationCode - 2", testItem.MessageTypeInfo, errorMessage);

		instruction.ZG_IsHighValueOvrd = true;
		testItem.MessageType = ExportSendMessageTypes.Codes.CAN;
		AssertNoError("MessageType - INV, IsHighValueOvrd - true, CEI_Style - H1, JI_ValuationCode - 2", testItem.MessageTypeInfo, errorMessage);
	}

	public void TestMessageTypeValidation_Authorisation_DV1()
	{
		var errorMessage = "You cannot sent VI (valuation indicators aka DV1) if there is no authorization with code DPO registered in the Entries Instruction tab.";
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var instruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;
		instruction.ZG_IsHighValueOvrd = true;
		var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Code = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		var testItem = new JobDeclarationMessageSendingObject(entry);

		testItem.MessageType = ExportSendMessageTypes.Codes.DEC;
		AssertHasError("MessageType - DEC, IsHighValueOvrd - true, Auth. Code - ETD", testItem.MessageTypeInfo, errorMessage);

		authorizationUsage.AGC_Code = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes.DeferredPayment;
		testItem.Validation.ValidateMessageType();
		AssertNoError("MessageType - DEC, IsHighValueOvrd - true, Auth. Code - DPO", testItem.MessageTypeInfo, errorMessage);

		authorizationUsage.AGC_Code = Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes.ElectronicTransportDocument;
		instruction.ZG_IsHighValueOvrd = false;
		testItem.Validation.ValidateMessageType();
		AssertNoError("MessageType - DEC, IsHighValueOvrd - false, Auth. Code - ETD", testItem.MessageTypeInfo, errorMessage);

		instruction.ZG_IsHighValueOvrd = true;
		testItem.MessageType = ExportSendMessageTypes.Codes.CAN;
		AssertNoError("MessageType - CAN, IsHighValueOvrd - true, Auth. Code - ETD", testItem.MessageTypeInfo, errorMessage);
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;
}
