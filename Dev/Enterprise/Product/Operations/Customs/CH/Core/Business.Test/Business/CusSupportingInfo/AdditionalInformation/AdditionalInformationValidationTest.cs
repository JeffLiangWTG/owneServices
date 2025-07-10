using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalInformationValidation))]
sealed class AdditionalInformationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_LineNo()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(AdditionalInformation.CSI_CodeInfo);
		});
	}

	public void TestCheckCSI_Code_Import()
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodes(Factory);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalInformation.CSI_CodeInfo, RefCusCodeTestHelper.InvalidAdditionalInformationCode, RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly);
		});
	}

	public void TestCheckCSI_Code_Export()
	{
		RefCusCodeTestHelper.CreateExportAddDocAdditionalInformationCodes(Factory);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalInformation.CSI_CodeInfo, new ZString[] { RefCusCodeTestHelper.InvalidExportAddDocAdditionalInformationCode, RefCusCodeTestHelper.InvalidExportAddDocAdditionalInformationCodeWithWrongAttributeValue }, new ZString[] { RefCusCodeTestHelper.ValidExportAddDocAdditionalInformationCodeItemLevel });
		});
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		RefCusCodeTestHelper.CreateFreeZoneTrafficCodes(Factory);
		RefCusCodeTestHelper.CreateBorderZoneTrafficCodes(Factory);
		CombineAssertions(() =>
		{
			AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalInformation.CSI_ReferenceNumberInfo, RefCusCodeTestHelper.InvalidFreeZoneTrafficCode, RefCusCodeTestHelper.ValidFreeZoneTrafficCode);

			AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(AdditionalInformation.CSI_ReferenceNumberInfo, RefCusCodeTestHelper.InvalidBorderZoneTrafficCode, RefCusCodeTestHelper.ValidBorderZoneTrafficCode);
		});
	}

	public void TestCheckCSI_ReferenceNumber_Not_EXP_Or_EDA() => CombineAssertions(() =>
	{
		AdditionalInformation.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters;
		AdditionalInformation.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageErrorContaining("JE_MessageType is EXP, Manatory message error should not appear", AdditionalInformation.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		AdditionalInformation.Parent.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AdditionalInformation.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageErrorContaining("JE_MessageType is EDA, Manatory message error should not appear", AdditionalInformation.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		AdditionalInformation.Parent.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AdditionalInformation.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageErrorContaining("JE_MessageType is not EXP/EDA, Manatory message error should appear", AdditionalInformation.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckCSI_Code_NS30003()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AdditionalInformation.Validation.ValidateCSI_Code();
		AssertNoRowMessageError("No error when Ordinary", AdditionalInformation, PassarValidationMessages.MessageNS30003_NotAllowed(AdditionalInformation.HumanReadableName));

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AdditionalInformation.Validation.ValidateCSI_Code();
		AssertHasRowMessageError("[NS30003] Error when Simplify", AdditionalInformation, PassarValidationMessages.MessageNS30003_NotAllowed(AdditionalInformation.HumanReadableName));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AdditionalInformation.Validation.ValidateCSI_Code();
		AssertNoRowMessageError("No error when EDA", AdditionalInformation, PassarValidationMessages.MessageNS30003_NotAllowed(AdditionalInformation.HumanReadableName));
	}

	public void TestCheckCSI_Description_NP70168()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		string expectedMessageError = PassarValidationMessages.MessageNP70168_MustBeDecimal;
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms;

		AdditionalInformation.CSI_Description = "1";
		AssertNoMessageErrorContaining("Assert no error if description is only digit", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);

		AdditionalInformation.CSI_Description = "1.2";
		AssertNoMessageErrorContaining("Assert no error if description is a valid decimal", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);

		AdditionalInformation.CSI_Description = "1.2KG";
		AssertHasMessageErrorContaining("Assert error if description is a valid decimal with unit", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);

		AdditionalInformation.CSI_Description = "ABC";
		AssertHasMessageErrorContaining("Assert error if description is not a valid decimal", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AdditionalInformation.Validation.ValidateCSI_Description();
		AssertNoMessageErrorContaining("Assert no error if description is not a valid decimal, but in eda", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
	}

	public void TestCheckCSI_Description_NP70212() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP702012_NotValidInteger;

		var additionalInformation = EntryInstruction.AdditionalInformations.AddNew();
		additionalInformation.CSI_Code = AdditionalInformationTypeCodes.PartialShipmentNumber;

		additionalInformation.CSI_Description = "x";
		AssertHasMessageError("CSI_Description char", additionalInformation.CSI_DescriptionInfo, messageError);

		additionalInformation.CSI_Description = ZString.Empty;
		AssertHasMessageError("CSI_Description empty", additionalInformation.CSI_DescriptionInfo, messageError);

		additionalInformation.CSI_Description = "1";
		AssertNoMessageError("CSI_Description int", additionalInformation.CSI_DescriptionInfo, messageError);

		additionalInformation.CSI_Code = "V1200";
		additionalInformation.CSI_Description = "x";
		AssertNoMessageError("not PartialShipment / CSI_Description char", additionalInformation.CSI_DescriptionInfo, messageError);
	});

	public void TestCheck_CSI_Description_NP70195()
	{
		string expectedErrorA1101 = PassarValidationMessages.MessageNP70195_DescriptionA1101;
		string expectedErrorA1102 = PassarValidationMessages.MessageNP70195_DescriptionA1102;
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol;
		AdditionalInformation.CSI_Description = "Test";
		AssertHasMessageError("if CSI_Code == A1101 AND CSI_Description.IsNotAValidDecimal, errorA1101", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1101);
		AdditionalInformation.CSI_Description = "1.2 L";
		AssertHasMessageError("if CSI_Code == A1102 AND CSI_Description.IsNotAValidDecimal, errorA1102", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1101);
		AdditionalInformation.CSI_Description = "1.2";
		AssertNoMessageError("if CSI_Code == A1101 AND CSI_Description.IsAValidDecimal, errorA1101", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1101);
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol;
		AdditionalInformation.CSI_Description = "Test";
		AssertHasMessageError("if CSI_Code == A1102 AND CSI_Description.IsNotAValidDecimal, errorA1102", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1102);
		AdditionalInformation.CSI_Description = "1.2 L";
		AssertHasMessageError("if CSI_Code == A1102 AND CSI_Description.IsNotAValidDecimal, errorA1102", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1102);
		AdditionalInformation.CSI_Description = "1.2";
		AssertNoMessageError("if CSI_Code == A1102 AND CSI_Description.IsAValidDecimal, errorA1102", AdditionalInformation.CSI_DescriptionInfo, expectedErrorA1102);
	}

	public void TestCheckCSI_Description_NP70197()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		string expectedMessageError = PassarValidationMessages.MessageNP70197_MustBeDecimal;
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters;
		AdditionalInformation.CSI_Description = "3";
		AssertNoMessageError("Expected no errors if description is number without decimal", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "1.2";
		AssertNoMessageError("Expected no errors if description is a valid decimal number", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "1.2L";
		AssertHasMessageError("Expected error if description is not a pure number", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "1,2";
		AssertHasMessageError("Expected error in case of comma as decimal separator", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "1,200";
		AssertHasMessageError("Expected error in case of comma as thousand separator", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "1,200.3";
		AssertHasMessageError("Expected error in case of comma as thousand separator", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "ABC";
		AssertHasMessageError("Expected error if description is not a number", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AdditionalInformation.CSI_Description = "1.2L";
		AssertNoMessageError("Expected no error if declaration is not Export, even if description is not a pure number", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
		AdditionalInformation.CSI_Description = "ABC";
		AssertNoMessageError("Expected no error if declaration is not Export, even if description is not a number", AdditionalInformation.CSI_DescriptionInfo, expectedMessageError);
	}

	public void TestCheckCSI_Code_NP70213()
	{
		var expectedMessage = PassarValidationMessages.MessageNP70213_V1201;
		var additionalInformation1 = EntryInstruction.AdditionalInformations.AddNew();
		var additionalInformation2 = EntryInstruction.AdditionalInformations.AddNew();
		additionalInformation1.CSI_Description = "2";
		additionalInformation1.CSI_Code = AdditionalInformationTypeCodes.PartialShipmentNumber;
		CombineAssertions(() =>
		{
			AssertHasMessageError("if CSI_Code equals to V1201 and CSI_Description is a digit > 1 and and no additional information with code V1202, show message error", additionalInformation1.CSI_CodeInfo, expectedMessage);

			additionalInformation1.CSI_Description = "1";
			additionalInformation1.Validation.ValidateCSI_Code();
			AssertNoMessageError("if CSI_Code equals to V1201 and CSI_Description is a digit <= 1, don't show Message error", additionalInformation1.CSI_CodeInfo, expectedMessage);

			additionalInformation1.CSI_Description = "2";
			additionalInformation2.CSI_Code = AdditionalInformationTypeCodes.ReferenceOfTheFristPartShipment;
			additionalInformation1.Validation.ValidateCSI_Code();
			AssertHasMessageError("if CSI_Code equals to V1201 and CSI_Description is a digit > 1 and and no additional information with code V1202 but a Empty Description, show message error", additionalInformation1.CSI_CodeInfo, expectedMessage);

			additionalInformation1.CSI_Description = "Test";
			additionalInformation1.Validation.ValidateCSI_Code();
			AssertNoMessageError("if CSI_Code equals to V1201 and CSI_Description is a digit > 1 and and no additional information with code V1202 but a NotEmpty Description, don't show Message error", additionalInformation1.CSI_CodeInfo, expectedMessage);

			additionalInformation2.CSI_Description = ZString.Empty;
			additionalInformation1.CSI_Code = AdditionalInformationTypeCodes.ExportCodeMineralOil;
			AssertNoMessageError("if CSI_Code different from V1201, don't show Message error", additionalInformation1.CSI_CodeInfo, expectedMessage);
		});
	}

	public void TestCheckCSI_Description_NP70213()
	{
		var expectedMessage = PassarValidationMessages.MessageNP70213_V1202;
		var additionalInformation = EntryInstruction.AdditionalInformations.AddNew();
		CombineAssertions(() =>
		{
			additionalInformation.CSI_Code = AdditionalInformationTypeCodes.ReferenceOfTheFristPartShipment;
			additionalInformation.Validation.ValidateCSI_Description();
			AssertHasMessageError("if CSI_Code equals to V1202 and CSI_Description is Empty, show message error", additionalInformation.CSI_DescriptionInfo, expectedMessage);

			additionalInformation.CSI_Description = "Test";
			AssertNoMessageError("if CSI_Code equals to V1202 and CSI_Description is NotEmpty, don't show message error", additionalInformation.CSI_DescriptionInfo, expectedMessage);

			additionalInformation.CSI_Code = AdditionalInformationTypeCodes.PartialShipmentNumber;
			additionalInformation.CSI_Description = ZString.Empty;
			AssertNoMessageError("if CSI_Code different from V1202, don't show Message error", additionalInformation.CSI_DescriptionInfo, expectedMessage);
		});
	}

	public void TestCheckCSI_Code_NS30104()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		string expectedMessageError = PassarValidationMessages.MessageNS30104;

		var invLineAddInfo1 = InvoiceLine.AdditionalInformations.AddNew();
		invLineAddInfo1.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms;
		var invLineAddInfo2 = InvoiceLine.AdditionalInformations.AddNew();
		invLineAddInfo2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VocQuantityInKilograms;
		AssertHasMessageError("Expected error if Invoice Line's Additional Information codes are not unique", invLineAddInfo2.CSI_CodeInfo, expectedMessageError);

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		invLineAddInfo2.Validation.ValidateCSI_Code();
		AssertNoMessageError("Expected no error if Declaration type is not Export, even if Invoice Line's Additional Information codes are not unique", invLineAddInfo2.CSI_CodeInfo, expectedMessageError);

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		invLineAddInfo2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		AssertNoMessageError("Expected no error if Invoice Line's Additional Information codes are unique", invLineAddInfo2.CSI_CodeInfo, expectedMessageError);

		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var entryInstructionAddInfo1 = entryInstruction.AdditionalInformations.AddNew();
		entryInstructionAddInfo1.CSI_Code = "CD";
		var entryInstructionAddInfo2 = entryInstruction.AdditionalInformations.AddNew();
		entryInstructionAddInfo2.CSI_Code = "CD";
		AssertNoMessageError("Expected no error if Additional Information codes are not unique and parent is not of type JobComInvoiceLine", entryInstructionAddInfo2.CSI_CodeInfo, expectedMessageError);
	}

	public void TestCheckCSI_Description_NP70155Mandatory()
	{
		string[] targetCodes = { UniversalReferenceConstants.AdditionalInformationTypeCodes.A1401,
			UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup,
			UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup,
			UniversalReferenceConstants.AdditionalInformationTypeCodes.A1404,
			UniversalReferenceConstants.AdditionalInformationTypeCodes.A1405,
			UniversalReferenceConstants.AdditionalInformationTypeCodes.A1406 };

		CombineAssertions(() =>
		{
			foreach (var code in targetCodes)
			{
				AdditionalInformation.CSI_Code = code;
				InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
				AdditionalInformation.CSI_Description = ZString.Empty;
				AssertNoMessageError($"if  additional information with code {code} and not tobacco and description.isEmpty, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_MandatoryDescription);

				InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
				AdditionalInformation.Validation.ValidateCSI_Description();
				AssertHasMessageError($"if additional information with code {code} and is tobacco and description.isEmpty, error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_MandatoryDescription);

				AdditionalInformation.CSI_Description = "TEST";
				AssertNoMessageError($"if additional information with code {code} and is tobacco and description.isNotEmpty, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_MandatoryDescription);
			}
		});
	}

	public void TestCheckCSI_Description_NP70155A1402ValidCode()
	{
		RefCusCodeTestHelper.CreateTBMGCodeList(Factory);
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		
		CombineAssertions(() =>
		{
			InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
			AdditionalInformation.CSI_Description = "TEST";
			AssertNoMessageError("if A1402 additional information and description is not in the list and is not tobacco, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1402ValidCode);

			InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
			AdditionalInformation.Validation.ValidateCSI_Description();
			AssertHasMessageError("if A1402 additional information and description is not in the list and is not tobacco, error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1402ValidCode);

			AdditionalInformation.CSI_Description = RefCusCodeTestHelper.ValidTBMGCode;
			AssertNoMessageError("if A1402 additional information and description is valid and is tobacco, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1402ValidCode);
		});
	}

	public void TestCheckCSI_Description_NP70155A1403ValidCode()
	{
		RefCusCodeTestHelper.CreateTBMGCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGACodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGBCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGCCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGDCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGECodeList(Factory);

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		var addInfo = InvoiceLine.AdditionalInformations.AddNew();
		addInfo.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;

		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;

		AssertA1403ValidCode(UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars, RefCusCodeTestHelper.ValidTBSGACode);
		AssertA1403ValidCode(UniversalReferenceConstants.TobaccoMainGroupCodes.Cigarettes, RefCusCodeTestHelper.ValidTBSGBCode);
		AssertA1403ValidCode(UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco, RefCusCodeTestHelper.ValidTBSGCCode);
		AssertA1403ValidCode(UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment, RefCusCodeTestHelper.ValidTBSGDCode);
		AssertA1403ValidCode(UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes, RefCusCodeTestHelper.ValidTBSGECode);

		AdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.A1405;
		addInfo.CSI_Description = "TEST";
		AssertNoMessageError($"if A1403 additional information and A1402 is not present and description is in the list and is tobacco, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1403ValidCode);

		void AssertA1403ValidCode(string a1402Description, string code)
		{
			addInfo.CSI_Description = a1402Description;

			InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
			AdditionalInformation.Validation.ValidateCSI_Description();
			AssertNoMessageError($"if A1403 additional information and A1402 is present with description {AdditionalInformation.CSI_Description}, and description is not in the list and is not tobacco, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1403ValidCode);

			InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
			AdditionalInformation.CSI_Description = "TEST";
			AssertHasMessageError($"if A1403 additional information and A1402 is present with description {AdditionalInformation.CSI_Description},and description is not in the list and is tobacco, error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1403ValidCode);

			AdditionalInformation.CSI_Description = code;
			AssertNoMessageError($"if A1403 additional information and A1402 is present with description {AdditionalInformation.CSI_Description}, and description is in the list and is tobacco, no error", AdditionalInformation.CSI_DescriptionInfo, PassarValidationMessages.MessageNP70155_A1403ValidCode);
		}
	}
	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invLine = InvoiceHeader.InvoiceLines.AddNew();
		invLine.JI_CEI = EntryInstruction.PK;
		return invLine;
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction entryInstruction;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateNewInvoiceLine();
	JobComInvoiceLine invoiceLine;

	AdditionalInformation AdditionalInformation => additionalInformation ??= InvoiceLine.AdditionalInformations.AddNew();
	AdditionalInformation additionalInformation;
}
