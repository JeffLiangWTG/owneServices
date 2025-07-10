using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_Code_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lineAdditionalInfo.CSI_CodeInfo);
	}

	public void TestCSI_SubType_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lineAdditionalInfo.CSI_SubTypeInfo);
	}

	public void TestCheckCSI_ReferenceNumber_R9007() => CombineAssertions(() =>
	{
		var message = "[R9007] Combination of Type and Reference must be unique for Kind REF";

		var addDoc1 = invoiceLine.AdditionalInfos.AddNew();
		addDoc1.CSI_SubType = "REF";
		addDoc1.CSI_Code = "same";
		addDoc1.CSI_ReferenceNumber = "same";

		var addDoc2 = invoiceLine.AdditionalInfos.AddNew();
		addDoc2.CSI_SubType = "REF";
		addDoc2.CSI_Code = "same";
		addDoc2.CSI_ReferenceNumber = "same";

		addDoc1.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageError("duplicate exists", addDoc1.CSI_ReferenceNumberInfo, message);

		addDoc2.CSI_Code = "different";
		addDoc1.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageError("CSI_Code is different", addDoc1.CSI_ReferenceNumberInfo, message);
		addDoc2.CSI_Code = "same";

		addDoc2.CSI_ReferenceNumber = "different";
		addDoc1.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageError("CSI_ReferenceNumber is different", addDoc1.CSI_ReferenceNumberInfo, message);
		addDoc2.CSI_ReferenceNumber = "same";

		addDoc2.CSI_SubType = "INF";
		addDoc1.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageError("CSI_SubType is not REF", addDoc1.CSI_ReferenceNumberInfo, message);

		var addDocInvoice1 = invoice.AdditionalInfos.AddNew();
		addDocInvoice1.CSI_SubType = "REF";
		addDocInvoice1.CSI_Code = "same";
		addDocInvoice1.CSI_ReferenceNumber = "same";
		var addDocInvoice2 = invoice.AdditionalInfos.AddNew();
		addDocInvoice2.CSI_SubType = "REF";
		addDocInvoice2.CSI_Code = "same";
		addDocInvoice2.CSI_ReferenceNumber = "same";
		addDocInvoice1.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageError("Parent is not invoice line", addDocInvoice1.CSI_ReferenceNumberInfo, message);
	});

	public void TestCheckCSI_ReferenceNumber_R9008()
	{
		var errorMessage = "[R9008] Combination of Type and Reference must be unique for Kind TRA";
		lineAdditionalInfo.CSI_SubType = "TRA";
		lineAdditionalInfo.CSI_Code = "Y904";
		lineAdditionalInfo.CSI_ReferenceNumber = "Must be unique";

		var lineAdditionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
		lineAdditionalInfo2.CSI_SubType = "TRA";
		lineAdditionalInfo2.CSI_Code = "Y904";
		lineAdditionalInfo2.CSI_ReferenceNumber = "Must be unique";

		CombineAssertions(() =>
		{
			AssertHasMessageError("TransportInfo is not unique", lineAdditionalInfo2.CSI_ReferenceNumberInfo, errorMessage);
			lineAdditionalInfo2.CSI_ReferenceNumber = "Is unique";
			AssertNoMessageError("TransportInfo is unique", lineAdditionalInfo2.CSI_ReferenceNumberInfo, errorMessage);
		});
	}

	public void TestCSI_SubType_ListValidation()
	{
		headerAdditionalInfo.CSI_SubType = "ABC";
		CombineAssertions(() =>
		{
			AssertHasMessageError("CSI_SubType 'ABC'", headerAdditionalInfo.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);

			headerAdditionalInfo.CSI_SubType = "TRA";
			AssertNoMessageError("CSI_Status 'TRA'", headerAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCheckForExistanceOfNRV()
	{
		var expectedMessage = "[C9003] If Sub style is B, C, E or F an additional document kind INF is required of type NRV";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = instruction.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "INF";
		additionalInfo.CSI_Code = "TST001";

		CombineAssertions(() =>
		{
			instruction.CEI_SubStyle = "X";
			additionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("Sub style X", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "B";
			additionalInfo.Validation.ValidateAll();
			AssertHasRowMessageError("Sub style B", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "C";
			additionalInfo.Validation.ValidateAll();
			AssertHasRowMessageError("Sub style C", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "E";
			additionalInfo.Validation.ValidateAll();
			AssertHasRowMessageError("Sub style E", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "F";
			additionalInfo.Validation.ValidateAll();
			AssertHasRowMessageError("Sub style F", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "B";
			additionalInfo.CSI_Code = "NRV001";
			AssertNoRowMessageError("NRV - Sub style B", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "C";
			additionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("NRV - Sub style C", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "E";
			additionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("NRV - Sub style E", additionalInfo, expectedMessage);

			instruction.CEI_SubStyle = "F";
			additionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("NRV - Sub style F", additionalInfo, expectedMessage);
		});
	}

	public void TestCheckREFForDuplicatedCSI_CodeCSI_ReferenceNumber()
	{
		var expectedErrorMessage = "[R9006] Combination of Type and Reference must be unique for Kind REF";
		var additionalInfo1 = cusEntryInstruction.AdditionalInfos.AddNew();
		var additionalInfo2 = cusEntryInstruction.AdditionalInfos.AddNew();

		additionalInfo1.CSI_SubType = "REF";
		additionalInfo1.CSI_Code = "AA";
		additionalInfo1.CSI_ReferenceNumber = "BB";

		additionalInfo2.CSI_SubType = "REF";
		additionalInfo2.CSI_Code = "AA";
		additionalInfo2.CSI_ReferenceNumber = "BB";

		CombineAssertions(() =>
		{
			additionalInfo1.Validation.ValidateAll();
			additionalInfo2.Validation.ValidateAll();

			AssertHasError("CSI_ReferenceNumber Line 1", additionalInfo1.CSI_ReferenceNumberInfo, expectedErrorMessage);
			AssertHasError("CSI_ReferenceNumber Line 2", additionalInfo2.CSI_ReferenceNumberInfo, expectedErrorMessage);

			additionalInfo2.CSI_ReferenceNumber = "CC";

			additionalInfo1.Validation.ValidateAll();
			additionalInfo2.Validation.ValidateAll();

			AssertNoError("CSI_ReferenceNumber Line 1", additionalInfo1.CSI_ReferenceNumberInfo, expectedErrorMessage);
			AssertNoError("CSI_ReferenceNumber Line 2", additionalInfo2.CSI_ReferenceNumberInfo, expectedErrorMessage);
		});
	}

	public void TestValidateTypeCodeAndReferenceNumberUnique()
	{
		var errorMessage = "[R9009] Combination of Type and Reference must be unique for Kind TRA";
		var additionalInfo1 = cusEntryInstruction.AdditionalInfos.AddNew();
		var additionalInfo2 = cusEntryInstruction.AdditionalInfos.AddNew();

		additionalInfo1.CSI_SubType = "TRA";
		additionalInfo1.CSI_Code = "A123";
		additionalInfo1.CSI_ReferenceNumber = "REF001";

		additionalInfo2.CSI_SubType = "TRA";
		additionalInfo2.CSI_Code = "A123";
		additionalInfo2.CSI_ReferenceNumber = "REF001";

		CombineAssertions(() =>
		{
			additionalInfo1.Validation.ValidateAll();
			additionalInfo2.Validation.ValidateAll();

			AssertHasMessageError("Duplicate Type and Reference for kind TRA", additionalInfo1.CSI_ReferenceNumberInfo, errorMessage);
			AssertHasMessageError("Duplicate Type and Reference for kind TRA", additionalInfo2.CSI_ReferenceNumberInfo, errorMessage);

			additionalInfo2.CSI_Code = "A1234";

			additionalInfo1.Validation.ValidateAll();
			additionalInfo2.Validation.ValidateAll();

			AssertNoMessageError("Unique Type and Reference for kind TRA", additionalInfo1.CSI_ReferenceNumberInfo, errorMessage);
			AssertNoMessageError("Unique Type and Reference for kind TRA", additionalInfo2.CSI_ReferenceNumberInfo, errorMessage);
		});
	}

	public void TestRuleNR9024()
	{
		var errorMessage = "[NR9024] Additional information type 00100 is only allowed on line item level";
		var additionalInfo = cusEntryInstruction.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "INF";

		CombineAssertions(() =>
		{
			additionalInfo.CSI_Code = "00100";
			AssertHasMessageError("Additional Info Type 00100 not allowed on entry instruction.", additionalInfo.CSI_CodeInfo, errorMessage);
			additionalInfo.CSI_Code = "00200";
			AssertNoMessageError("Additional Info Type is not 00100.", additionalInfo.CSI_CodeInfo, errorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageStatus = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		headerAdditionalInfo = invoice.AdditionalInfos.AddNew();
		lineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	AdditionalInfo headerAdditionalInfo;
	AdditionalInfo lineAdditionalInfo;
	CusEntryInstruction cusEntryInstruction;
}
