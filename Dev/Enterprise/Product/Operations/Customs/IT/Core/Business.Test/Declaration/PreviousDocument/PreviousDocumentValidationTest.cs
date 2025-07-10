using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentValidationTest : BasePreviousDocumentValidationTest
{
	public void TestReferenceNumberFormatValidation()
	{
		var invalidFormatMessageError = "Number entered is in an invalid format.";
		AssertEquals("Format error text", invalidFormatMessageError, ValidationCaptions.PreviousDocument.ReferenceNumberEnteredIsInvalidFormat);

		previousDocument.CSI_Procedure = "AWB";
		previousDocument.CSI_ReferenceNumber = "AA23D";
		AssertHasMessageError("ReferenceNumber = AA23D", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		previousDocument.CSI_ReferenceNumber = "122321321";
		AssertHasMessageError("ReferenceNumber = 122321321", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		previousDocument.CSI_ReferenceNumber = "123456k";
		AssertHasMessageError("ReferenceNumber = 123456k", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		previousDocument.CSI_ReferenceNumber = "A";
		AssertHasMessageError("ReferenceNumber = A", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		previousDocument.CSI_ReferenceNumber = "123456A";
		AssertNoMessageErrorContaining("ReferenceNumber = 123456A", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

		previousDocument.CSI_Procedure = ZString.Empty;
		previousDocument.CSI_ReferenceNumber = "AA23D";
		AssertNoMessageErrorContaining("ReferenceNumber = AA23D", previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
	}

	public void TestReferenceNumberCheckDigitValidation()
	{
		var checkDigitMessageErrorPrefix = "The check digit entered is incorrect.";
		AssertEquals("Check Digit error prefix", checkDigitMessageErrorPrefix, ValidationCaptions.PreviousDocument.CheckDigitIsIncorrectErrorPrefix);

		previousDocument.CSI_Procedure = "AWB";
		CombineAssertions("ReferenceNumber = 123456Z", () =>
		{
			previousDocument.CSI_ReferenceNumber = "123456Z";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, $"{checkDigitMessageErrorPrefix} Current value is: 'Z', expected 'A'.");
		});

		previousDocument.CSI_ReferenceNumber = "123456A";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);

		CombineAssertions("ReferenceNumber = 30919Z", () =>
		{
			previousDocument.CSI_ReferenceNumber = "30919Z";
			AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, $"{checkDigitMessageErrorPrefix} Current value is: 'Z', expected 'X'.");
		});

		previousDocument.CSI_ReferenceNumber = "30919X";
		AssertNoMessageErrorContaining("ReferenceNumber = 30919X", previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);

		previousDocument.CSI_Procedure = "";
		previousDocument.CSI_ReferenceNumber = "30919Z";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, checkDigitMessageErrorPrefix);
	}

	public void TestReferenceNumberFormatValidation_ForManualA3Procedure()
	{
		var invalidFormatMessageError = "Number entered is in an invalid format.";
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_Status = "M";

		previousDocument.CSI_ReferenceNumber = "";
		AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		previousDocument.CSI_ReferenceNumber = "123456";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

		previousDocument.CSI_ReferenceNumber = "123456Z";
		AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
	}

	public void TestReferenceNumberNoFormatValidationForCIMProcedure()
	{
		var invalidFormatMessageError = "Number entered is in an invalid format.";

		previousDocument.CSI_Procedure = "CIM";

		CombineAssertions("procedure = CIM, there should be no validation for reference number", () =>
		{
			previousDocument.CSI_ReferenceNumber = "AA23D";
			AssertNoMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "122321321";
			AssertNoMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "123456k";
			AssertNoMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "A";
			AssertNoMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		});

		previousDocument.CSI_Procedure = "AWB";

		CombineAssertions("procedure = AWB, there should be validation for reference number", () =>
		{
			previousDocument.CSI_ReferenceNumber = "AA23D";
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "122321321";
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "123456k";
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);

			previousDocument.CSI_ReferenceNumber = "A";
			AssertHasMessageError(previousDocument.CSI_ReferenceNumberInfo, invalidFormatMessageError);
		});
	}

	public void TestCheckCSI_Quantity()
	{
		CombineAssertions("When it is a summary declaration document", () =>
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Procedure = "A3";
			Assert("PRE-CONDITION:", previousDocument.IsSummaryDeclarationDocument);
			AssertNoMessageErrorContaining(previousDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Quantity = 100.12m;
			AssertNoMessageErrorContaining(previousDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When it is a previous procedure document", () =>
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Procedure = "2";
			Assert("PRE-CONDITION:", previousDocument.IsPreviousProcedureDocument);
			AssertHasMessageErrorContaining(previousDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Quantity = 100.12m;
			AssertNoMessageErrorContaining(previousDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		});

		previousDocument.CSI_UnitOfQuantity = "G";
		previousDocument.CSI_Quantity = 100.127;

		AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 100.127 Grams will be: 0.10013 Kilograms.");

		previousDocument.CSI_Quantity = 100.13;

		AssertNoMessageErrors(previousDocument.CSI_QuantityInfo);
	}

	public void TestCheckCSI_UnitOfQuantity()
	{
		previousDocument.CSI_UnitOfQuantity = "X";
		AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);

		previousDocument.CSI_UnitOfQuantity = "";
		AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		previousDocument.CSI_Quantity = 100.127;
		previousDocument.CSI_UnitOfQuantity = "G";

		AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 100.127 Grams will be: 0.10013 Kilograms.");

		previousDocument.CSI_Quantity = 100.13;
		previousDocument.CSI_UnitOfQuantity = "G";

		AssertNoMessageErrors(previousDocument.CSI_QuantityInfo);
	}

	public void TestCheckCSI_UnitOfQuantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		previousDocument.CSI_UnitOfQuantity2 = "XXX";
		AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);
		previousDocument.CSI_UnitOfQuantity2 = "KGMG";
		AssertNoMessageErrorContaining(previousDocument.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckCSI_UnitOfQuantity3()
	{
		previousDocument.CSI_UnitOfQuantity3 = "X";
		AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantity3Info, ListValidation.InvalidCodeMessageError);

		previousDocument.CSI_UnitOfQuantity3 = "";
		AssertHasMessageErrorContaining(previousDocument.CSI_UnitOfQuantity3Info, MandatoryValidation.YouHaveNotEntered);

		previousDocument.CSI_Quantity3 = 456.883;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		AssertHasWarningContaining(previousDocument.CSI_Quantity3Info, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 456.883 Grams will be: 0.45688 Kilograms.");

		previousDocument.CSI_Quantity3 = 456.88;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		AssertNoMessageErrors(previousDocument.CSI_Quantity3Info);
	}

	public void TestCheckCSI_QuantityRoundingWarningWithDeclarationTypeAndUnit()
	{
		previousDocument.CSI_Quantity = 123.45678;

		CombineAssertions("For EXP", () =>
		{
			declaration.JE_MessageType = "EXP";

			previousDocument.CSI_UnitOfQuantity = "G";
			AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 123.45678 Grams will be: 0.12346 Kilograms.");

			previousDocument.CSI_UnitOfQuantity = "HG";
			AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Hectograms exceeding 1 decimal could be rounded in Customs declaration message. For instance 123.45678 Hectograms will be: 12.34568 Kilograms.");
		});

		CombineAssertions("For IMP", () =>
		{
			declaration.JE_MessageType = "IMP";

			previousDocument.CSI_Quantity = 123.45678;

			previousDocument.CSI_UnitOfQuantity = "G";
			AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Kilograms exceeding 6 decimals could be rounded in Customs declaration message. For instance 123.45678 Grams will be: 0.123457 Kilograms.");

			previousDocument.CSI_UnitOfQuantity = "HG";
			AssertNoWarnings("123.45678 Hectograms will be: 12.345678 Kilograms", previousDocument.CSI_QuantityInfo);

			previousDocument.CSI_Quantity = 123.4567;
			previousDocument.CSI_UnitOfQuantity = "G";
			AssertHasWarningContaining(previousDocument.CSI_QuantityInfo, "Be aware that quantity in Kilograms exceeding 6 decimals could be rounded in Customs declaration message. For instance 123.4567 Grams will be: 0.123457 Kilograms.");

			previousDocument.CSI_UnitOfQuantity = "HG";
			AssertNoWarnings("123.4567 Hectograms will be: 12.34567 Kilograms", previousDocument.CSI_QuantityInfo);

			previousDocument.CSI_Quantity = 123.456;
			previousDocument.CSI_UnitOfQuantity = "G";
			AssertNoWarnings("123.456 Grams will be: 0.123456 Kilograms", previousDocument.CSI_QuantityInfo);

			previousDocument.CSI_UnitOfQuantity = "HG";
			AssertNoWarnings("123.456 Hectograms will be: 12.3456 Kilograms", previousDocument.CSI_QuantityInfo);
		});
	}

	public void TestCheckCSI_Quantity3RoundingWarningWithDeclarationTypeAndUnit()
	{
		previousDocument.CSI_Quantity3 = 123.45678;

		CombineAssertions("For EXP", () =>
		{
			declaration.JE_MessageType = "EXP";

			previousDocument.CSI_UnitOfQuantity3 = "G";
			AssertHasWarningContaining(previousDocument.CSI_Quantity3Info, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 123.45678 Grams will be: 0.12346 Kilograms.");

			previousDocument.CSI_UnitOfQuantity3 = "HG";
			AssertHasWarningContaining(previousDocument.CSI_Quantity3Info, "Be aware that quantity in Hectograms exceeding 1 decimal could be rounded in Customs declaration message. For instance 123.45678 Hectograms will be: 12.34568 Kilograms.");
		});
	}

	public void TestCheckCSI_Quantity3Mandatory_IsPreviousProcedureDocument()
	{
		var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEntered;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var previousDocumentRP2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP2.CSI_Procedure = "2";
		previousDocumentRP2.CSI_Quantity3 = 0m;

		CombineAssertions("Case 1: no NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			Assert(!entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, youHaveNotEnteredMessage);
		});

		var previousDocumentPAA3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA3.CSI_Procedure = "A3";
		previousDocumentPAA3.CSI_Quantity3 = 0m;
		var previousDocumentRP5 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP5.CSI_Procedure = "5";
		previousDocumentRP5.CSI_Quantity3 = 0m;

		CombineAssertions("Case 2: NB with PA", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, youHaveNotEnteredMessage);
		});

		CombineAssertions("Case 3: NB without PA", () =>
		{
			invoiceLine.PreviousDocuments.RemoveAndDelete(previousDocumentPAA3);
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertHasMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, youHaveNotEnteredMessage);

			previousDocumentRP2.CSI_Quantity3 = 1m;
			previousDocumentRP5.CSI_Quantity3 = 1m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, youHaveNotEnteredMessage);
		});
	}

	public void TestCheckCSI_Quantity3Mandatory_IsSummaryDeclarationDocument()
	{
		var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEntered;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var previousDocumentPAA3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA3.CSI_Procedure = "A3";
		previousDocumentPAA3.CSI_Quantity3 = 0m;

		CombineAssertions("Case 1: no NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentPAA3.Validation.ValidateAll();
			Assert(!entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_Quantity3Info, youHaveNotEnteredMessage);
		});

		var previousDocumentPAA44 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA44.CSI_Procedure = "A44";
		previousDocumentPAA44.CSI_Quantity3 = 0m;

		CombineAssertions("Case 2: NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertHasMessageErrorContaining(previousDocumentPAA3.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentPAA44.CSI_Quantity3Info, youHaveNotEnteredMessage);

			previousDocumentPAA3.CSI_Quantity3 = 1m;
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentPAA44.CSI_Quantity3Info, youHaveNotEnteredMessage);

			previousDocumentPAA44.CSI_Quantity3 = 2m;
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_Quantity3Info, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentPAA44.CSI_Quantity3Info, youHaveNotEnteredMessage);
		});
	}

	public void TestCheckCSI_Quantity3GreaterThanOrEqualToNetMass()
	{
		var grossMassMustBeGreaterThanOrEqualToNetMassMessage = ValidationCaptions.PreviousDocument.GrossMassMustBeGreaterThanOrEqualToNetMass;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var previousDocumentRP2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP2.CSI_Procedure = "2";
		previousDocumentRP2.CSI_Quantity = 100m;
		previousDocumentRP2.CSI_Quantity3 = 50m;
		CombineAssertions("Case 1: no NB and different mass values", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			Assert(!entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
		});

		var previousDocumentPAA3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA3.CSI_Procedure = "A3";
		previousDocumentPAA3.CSI_Quantity = 100m;
		previousDocumentPAA3.CSI_Quantity3 = 50m;
		var previousDocumentRP5 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP5.CSI_Procedure = "5";
		previousDocumentRP5.CSI_Quantity = 100m;
		previousDocumentRP5.CSI_Quantity3 = 50m;

		CombineAssertions("Case 2: NB with RP and PA and different mass values ", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
		});

		CombineAssertions("Case 3: NB without PA and different mass values", () =>
		{
			invoiceLine.PreviousDocuments.RemoveAndDelete(previousDocumentPAA3);
			declaration.ResetApportionedPreviousDocuments();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			AssertHasMessageErrorContaining(previousDocumentRP2.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
			AssertHasMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
		});

		CombineAssertions("Case 4: NB without PA and correct mass values", () =>
		{
			previousDocumentRP5.CSI_Quantity = 100m;
			previousDocumentRP5.CSI_Quantity3 = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);

			previousDocumentRP5.CSI_Quantity = 50m;
			previousDocumentRP5.CSI_Quantity3 = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_Quantity3Info, grossMassMustBeGreaterThanOrEqualToNetMassMessage);
		});
	}

	public void TestCheckCSI_Quantity3DecimalDigitsTruncationWarning()
	{
		previousDocument.CSI_UnitOfQuantity3 = "G";
		previousDocument.CSI_Quantity3 = 456.883;

		AssertHasWarningContaining(previousDocument.CSI_Quantity3Info, "Be aware that quantity in Grams exceeding 1 decimal could be rounded in Customs declaration message. For instance 456.883 Grams will be: 0.45688 Kilograms");
		previousDocument.CSI_Quantity3 = 456.88;
		AssertNoMessageErrors(previousDocument.CSI_Quantity3Info);
	}

	protected override IPreviousDocumentForTesting GetPreviousDocumentForTesting() => previousDocument;

	public void TestCheckDocumentAtJobLevel()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "61", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: false);
		helper.CreateRefCusProcedure(currentCountry, "A", "40", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		Factory.Save();

		var youCanHaveJustOneSummaryDeclarationDocumentPerJob = "For Reimport procedures you can have just 1 PA document per job. Remove additional documents.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var declarationPreviousDocument1 = declaration.PreviousDocuments.AddNew();
		var declarationPreviousDocument2 = declaration.PreviousDocuments.AddNew();

		CombineAssertions("When it is not an reimport procedure", () =>
		{
			entryInstruction.CEI_Procedure = "40";
			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertNoRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);

			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument2.CSI_Procedure = "A3";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertNoRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);

			declarationPreviousDocument1.CSI_Procedure = "2";
			declarationPreviousDocument2.CSI_Procedure = "2";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertNoRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
		});

		CombineAssertions("When it is an reimport procedure", () =>
		{
			entryInstruction.CEI_Procedure = "61";
			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertNoRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);

			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument2.CSI_Procedure = "A3";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertHasRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertHasRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);

			declarationPreviousDocument1.CSI_Procedure = "2";
			declarationPreviousDocument2.CSI_Procedure = "2";
			declarationPreviousDocument1.Validation.ValidateAll();
			declarationPreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(declarationPreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
			AssertNoRowWarningContaining(declarationPreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJob);
		});
	}

	public void TestCheckDocumentAtInvoiceLevel()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "61", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: false);
		helper.CreateRefCusProcedure(currentCountry, "A", "40", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		Factory.Save();

		var youCanHaveJustOneSummaryDeclarationDocumentPerInvoice = "For Reimport procedures you can have just 1 PA document per invoice. Remove additional documents.";
		var youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine = "For Reimport procedures you can have just 1 PA document at job/invoice/invoice line level at the same time. Remove one or more of them.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var declarationPreviousDocument1 = declaration.PreviousDocuments.AddNew();
		var declarationPreviousDocument2 = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoicePreviousDocument1 = invoice.PreviousDocuments.AddNew();
		var invoicePreviousDocument2 = invoice.PreviousDocuments.AddNew();

		CombineAssertions("When it is not an reimport procedure", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "40";
			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);

			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument2.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);

			invoicePreviousDocument1.CSI_Procedure = "2";
			invoicePreviousDocument2.CSI_Procedure = "2";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);

			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument2.CSI_Procedure = "A3";
			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument2.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
		});

		CombineAssertions("When it is an reimport procedure and job does not have any PA document", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "61";
			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);

			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument2.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertHasRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertHasRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);

			invoicePreviousDocument1.CSI_Procedure = "2";
			invoicePreviousDocument2.CSI_Procedure = "2";
			invoicePreviousDocument1.Validation.ValidateAll();
			invoicePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
			AssertNoRowWarningContaining(invoicePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoice);
		});

		CombineAssertions("When it is an reimport procedure and job have 1 PA document", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "61";
			declarationPreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoicePreviousDocument1.Validation.ValidateAll();
			AssertHasRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);

			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument1.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoicePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
		});
	}

	public void TestCheckDocumentAtInvoiceLineLevel()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "61", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: false);
		helper.CreateRefCusProcedure(currentCountry, "A", "40", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		Factory.Save();

		var youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine = "For Reimport procedures you can have just 1 PA document per invoice line. Remove additional documents.";
		var youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine = "For Reimport procedures you can have just 1 PA document at job/invoice/invoice line level at the same time. Remove one or more of them.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var declarationPreviousDocument1 = declaration.PreviousDocuments.AddNew();
		var declarationPreviousDocument2 = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoicePreviousDocument1 = invoice.PreviousDocuments.AddNew();
		var invoicePreviousDocument2 = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var invoiceLinePreviousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		var invoiceLinePreviousDocument2 = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions("When it is not an reimport procedure", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "40";
			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument2.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "2";
			invoiceLinePreviousDocument2.CSI_Procedure = "2";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);

			declarationPreviousDocument1.CSI_Procedure = "A3";
			declarationPreviousDocument2.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument2.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
		});

		CombineAssertions("When it is an reimport procedure and job/invoice does not have any PA document", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "61";
			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument2.CSI_Procedure = "";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument2.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertHasRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertHasRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "2";
			invoiceLinePreviousDocument2.CSI_Procedure = "2";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			invoiceLinePreviousDocument2.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
			AssertNoRowWarningContaining(invoiceLinePreviousDocument2, youCanHaveJustOneSummaryDeclarationDocumentPerInvoiceLine);
		});

		CombineAssertions("When it is an reimport procedure and job/invoice have 1 PA document", () =>
		{
			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "61";
			declarationPreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			AssertHasRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);

			declarationPreviousDocument1.CSI_Procedure = "";
			declarationPreviousDocument2.CSI_Procedure = "";
			invoicePreviousDocument1.CSI_Procedure = "";
			invoicePreviousDocument2.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.CSI_Procedure = "";

			entryInstruction.CEI_Procedure = "61";
			invoicePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.CSI_Procedure = "A3";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			AssertHasRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);

			invoiceLinePreviousDocument1.CSI_Procedure = "";
			invoiceLinePreviousDocument1.Validation.ValidateAll();
			AssertNoRowWarningContaining(invoiceLinePreviousDocument1, youCanHaveJustOneSummaryDeclarationDocumentPerJobAndInvoiceAndLine);
		});
	}

	public void TestCheckCSI_Tariff()
	{
		CombineAssertions("When it's a RP document", () =>
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
			Assert("Pre - condition: should be RP document", previousDocument.IsPreviousProcedureDocument);

			previousDocument.Validation.ValidateCSI_Tariff();
			AssertHasWarningContaining("A warning should be displayed when LineNo and Tariff are empty", previousDocument.CSI_TariffInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);

			previousDocument.FormattedTariff = "1";
			AssertNoWarningContaining("A warning should be displayed when LineNo and Tariff are empty", previousDocument.CSI_TariffInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);
		});
	}

	public void TestCheckPreviousDocumentNetMassAgainstEntryLinesNetMassWhereItHasBeenApportioned()
	{
		var expectedMessage = ValidationCaptions.PreviousDocument.TheQuantitiesOfDocumentsDoNotMatchWithQuantitiesOfEntryLine;
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		var previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "2";
		previousDocument1.CSI_ReferenceNumber = "1A";
		var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "2";
		previousDocument2.CSI_ReferenceNumber = "2B";
		var previousDocument3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "A3";
		previousDocument3.CSI_ReferenceNumber = "3C";
		CombineAssertions("When the entry line has a procedure ending with '00' or '51' or starting with '61'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4071", shipmentType: "IMP", configurationAction: x => x.ZZ6_OutOfWarehouse = "Y");
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.JI_NetWeight = 10m;
			previousDocument1.CSI_Quantity = 50m;
			previousDocument2.CSI_Quantity = 15m;
			previousDocument3.CSI_Quantity = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertHasMessageErrorContaining(previousDocument1.CSI_QuantityInfo, expectedMessage);
			AssertHasMessageErrorContaining(previousDocument2.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_QuantityInfo, expectedMessage);

			invoiceLine.JI_NetWeight = 65m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_QuantityInfo, expectedMessage);
		});

		CombineAssertions("When the entry line has a procedure ending with '00' or '51' or starting with '61'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "6100", shipmentType: "IMP");
			invoiceLine.JI_Procedure = "6100";
			invoiceLine.JI_NetWeight = 10m;
			previousDocument1.CSI_Quantity = 50m;
			previousDocument2.CSI_Quantity = 15m;
			previousDocument3.CSI_Quantity = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_QuantityInfo, expectedMessage);

			invoiceLine.JI_NetWeight = 65m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_QuantityInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_QuantityInfo, expectedMessage);
		});
	}

	public void TestCheckPreviousDocumentGrossMassAgainstEntryLinesGrossMassWhereItHasBeenApportioned()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		var previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		previousDocument1.CSI_ReferenceNumber = "1A";
		var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "A3";
		previousDocument2.CSI_ReferenceNumber = "2B";
		var previousDocument3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "2";
		previousDocument3.CSI_ReferenceNumber = "3C";
		CombineAssertions("When the entry line has a procedure ending with '00'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4000", shipmentType: "IMP");
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_Weight = 10m;
			previousDocument1.CSI_Quantity3 = 50m;
			previousDocument2.CSI_Quantity3 = 15m;
			previousDocument3.CSI_Quantity3 = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoNotifications(previousDocument1.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument2.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument3.CSI_Quantity3Info);

			invoiceLine.JI_Weight = 65m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoNotifications(previousDocument1.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument2.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument3.CSI_Quantity3Info);
		});

		CombineAssertions("When the entry line has not a procedure ending with '00'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4071", shipmentType: "IMP", configurationAction: x => x.ZZ6_OutOfWarehouse = "Y");
			invoiceLine.JI_Procedure = "4071";
			invoiceLine.JI_Weight = 10m;
			previousDocument1.CSI_Quantity3 = 50m;
			previousDocument2.CSI_Quantity3 = 15m;
			previousDocument3.CSI_Quantity3 = 100m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoNotifications(previousDocument1.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument2.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument3.CSI_Quantity3Info);

			invoiceLine.JI_Weight = 65m;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoNotifications(previousDocument1.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument2.CSI_Quantity3Info);
			AssertNoNotifications(previousDocument3.CSI_Quantity3Info);
		});
	}

	public void TestCheckPackageQuantityMandatory_IsPreviousProcedureDocument()
	{
		var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEntered;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var previousDocumentRP2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP2.CSI_Procedure = "2";
		previousDocumentRP2.CSI_PackQty = 0;

		CombineAssertions("Case 1: no NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			Assert(!entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_PackQtyInfo, youHaveNotEnteredMessage);
		});

		var previousDocumentPAA3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA3.CSI_Procedure = "A3";
		previousDocumentPAA3.CSI_PackQty = 0;
		var previousDocumentRP5 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentRP5.CSI_Procedure = "5";
		previousDocumentRP5.CSI_PackQty = 0;

		CombineAssertions("Case 2: NB with PA", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_PackQtyInfo, youHaveNotEnteredMessage);
		});

		CombineAssertions("Case 3: NB without PA", () =>
		{
			invoiceLine.PreviousDocuments.RemoveAndDelete(previousDocumentPAA3);
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertHasMessageErrorContaining(previousDocumentRP2.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentRP5.CSI_PackQtyInfo, youHaveNotEnteredMessage);

			previousDocumentRP2.CSI_PackQty = 1;
			previousDocumentRP5.CSI_PackQty = 1;
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentRP2.Validation.ValidateAll();
			previousDocumentRP5.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentRP2.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentRP5.CSI_PackQtyInfo, youHaveNotEnteredMessage);
		});
	}

	public void TestCheckPackageQuantityMandatory_IsSummaryDeclarationDocument()
	{
		var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEntered;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		var previousDocumentPAA3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA3.CSI_Procedure = "A3";
		previousDocumentPAA3.CSI_PackQty = 0;

		CombineAssertions("Case 1: no NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentPAA3.Validation.ValidateAll();
			Assert(!entryLine.GroupedPreviousDocuments.Any());
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_PackQtyInfo, youHaveNotEnteredMessage);
		});

		var previousDocumentPAA44 = invoiceLine.PreviousDocuments.AddNew();
		previousDocumentPAA44.CSI_Procedure = "A44";
		previousDocumentPAA44.CSI_Quantity3 = 0m;

		CombineAssertions("Case 2: NB", () =>
		{
			declaration.ResetApportionedPreviousDocuments();
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			Assert(entryLine.GroupedPreviousDocuments.Any());
			AssertHasMessageErrorContaining(previousDocumentPAA3.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentPAA44.CSI_PackQtyInfo, youHaveNotEnteredMessage);

			previousDocumentPAA3.CSI_PackQty = 1;
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertHasMessageErrorContaining(previousDocumentPAA44.CSI_PackQtyInfo, youHaveNotEnteredMessage);

			previousDocumentPAA44.CSI_PackQty = 2;
			previousDocumentPAA3.Validation.ValidateAll();
			previousDocumentPAA44.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocumentPAA3.CSI_PackQtyInfo, youHaveNotEnteredMessage);
			AssertNoMessageErrorContaining(previousDocumentPAA44.CSI_PackQtyInfo, youHaveNotEnteredMessage);
		});
	}

	public void TestCheckPreviousDocumentPackageQuantityAgainstEntryLinesPackageQuantityWhereItHasBeenApportioned()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		var expectedMessage = ValidationCaptions.PreviousDocument.TheQuantitiesOfDocumentsDoNotMatchWithQuantitiesOfEntryLine;

		var declaration = Factory.New<JobDeclaration>();
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		declaration.JE_MessageType = "IMP";
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = declaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		package1.CW_PackType = "VG";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		var packageInvoiceLine1 = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;
		var previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		previousDocument1.CSI_ReferenceNumber = "1A";
		var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "A3";
		previousDocument2.CSI_ReferenceNumber = "2B";
		var previousDocument3 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "2";
		previousDocument3.CSI_ReferenceNumber = "3C";

		CombineAssertions("When the entry line has a procedure ending with '00'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4000", shipmentType: "IMP");
			invoiceLine.JI_Procedure = "4000";
			packageInvoiceLine1.CHC_NumberOfPacks = 10;
			previousDocument1.CSI_PackQty = 50;
			previousDocument2.CSI_PackQty = 15;
			previousDocument3.CSI_PackQty = 100;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertHasMessageErrorContaining(previousDocument1.CSI_PackQtyInfo, expectedMessage);
			AssertHasMessageErrorContaining(previousDocument2.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_PackQtyInfo, expectedMessage);

			packageInvoiceLine1.CHC_NumberOfPacks = 65;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_PackQtyInfo, expectedMessage);
		});

		CombineAssertions("When the entry line has not a procedure ending with '00'", () =>
		{
			testDataHelper.CreateNewRefCusProcedure(procedureCode: "4071", shipmentType: "IMP", configurationAction: x => x.ZZ6_OutOfWarehouse = "Y");
			invoiceLine.JI_Procedure = "4071";
			packageInvoiceLine1.CHC_NumberOfPacks = 10;
			previousDocument1.CSI_PackQty = 50;
			previousDocument2.CSI_PackQty = 15;
			previousDocument3.CSI_PackQty = 100;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_PackQtyInfo, expectedMessage);

			packageInvoiceLine1.CHC_NumberOfPacks = 65;
			declaration.ResetApportionedPreviousDocuments();
			previousDocument1.Validation.ValidateAll();
			previousDocument2.Validation.ValidateAll();
			previousDocument3.Validation.ValidateAll();
			AssertNoMessageErrorContaining(previousDocument1.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument2.CSI_PackQtyInfo, expectedMessage);
			AssertNoMessageErrorContaining(previousDocument3.CSI_PackQtyInfo, expectedMessage);
		});
	}

	public void TestCheckCSI_Quantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var importTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		var exportTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Export);
		Factory.Save();

		var importTariffWithUOM = helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(importTariffWithUOM, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var exportTariffWithUOM = helper.CreateTariff(Core.Constants.CountryCodes.Italy, exportTariffType.PK, "33333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(exportTariffWithUOM, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, exportTariffType.PK, "44444444", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions("When it is a summary declaration document", () =>
		{
			previousDocument.CSI_Procedure = "A3";
			Assert("PRE-CONDITION:", previousDocument.IsSummaryDeclarationDocument);

			previousDocument.FormattedTariff = "2222222222";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "1111111111";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "1111111111";
			previousDocument.CSI_Quantity2 = 1m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);
		});

		CombineAssertions("When it is a previous procedure document and export declaration", () =>
		{
			declaration.JE_MessageType = "EXP";
			previousDocument.CSI_Procedure = "2";
			Assert("PRE-CONDITION:", previousDocument.IsPreviousProcedureDocument);

			previousDocument.FormattedTariff = "44444444";
			previousDocument.CSI_Quantity2 = 0m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "33333333";
			previousDocument.CSI_Quantity2 = 0m;
			AssertHasMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);

			previousDocument.FormattedTariff = "33333333";
			previousDocument.CSI_Quantity2 = 1m;
			AssertNoMessageErrorContaining(previousDocument.CSI_Quantity2Info, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckPreviousDocumentGrossMassAgainstEntryLinesGrossMassWhereItHasBeenApportioned_WithPendingApportionment()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MessageType = "IMP";
		testDataHelper.CreateNewRefCusProcedure(procedureCode: "4000", shipmentType: "IMP");
		entryInstruction.CEI_Procedure = "40";
		invoiceLine.JI_Procedure = "4000";
		invoiceLine.JI_Weight = 500m;
		var previousDocument = PreviousDocumentTestHelper.CreateDocument(Factory, "", new ZDate(2020, 01, 01), "1P", "Z", "", "", 0, "A3", "", "", "", 0m, 0m, 500m, 0);
		invoiceLine.PreviousDocuments.Add(previousDocument);
		declaration.DoMerge();
		previousDocument.Validation.ValidateAll();

		CombineAssertions("Case 1: no NB lines", () =>
		{
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("No NB lines", 0, declaration.CustomsEntryHeaders[0].MergedLines[0].GroupedPreviousDocuments.Count);
			Assert(!declaration.MergeManager.RequiresMerge);
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			previousDocument.CSI_Quantity3 = 1000m;
			Assert(declaration.MergeManager.RequiresMerge);
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			declaration.DoMerge();
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			previousDocument.CSI_Quantity3 = 500m;
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			declaration.DoMerge();
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);
		});

		CombineAssertions("Case 2: NB lines", () =>
		{
			var previousDocument2 = PreviousDocumentTestHelper.CreateDocument(Factory, "", new ZDate(2020, 01, 01), "1P", "Z", "", "", 0, "MRN", "", "", "", 0m, 0m, 0m, 0);
			invoiceLine.PreviousDocuments.Add(previousDocument2);
			declaration.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("NB lines", 2, declaration.CustomsEntryHeaders[0].MergedLines[0].GroupedPreviousDocuments.Count);
			Assert(!declaration.MergeManager.RequiresMerge);
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			previousDocument.CSI_Quantity3 = 1000m;
			Assert(declaration.MergeManager.RequiresMerge);
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			declaration.DoMerge();
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			previousDocument.CSI_Quantity3 = 500m;
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);

			declaration.DoMerge();
			previousDocument.Validation.ValidateAll();
			AssertNoNotifications(previousDocument.CSI_Quantity3Info);
		});
	}

	public void TestCheckCSI_LineNo()
	{
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_LineNo validation when CSI_Procedure is Empty", delegate
		{
			AssertNoWarningContaining("No warning should be displayed when LineNo and Tariff are empty and is not RP Document", previousDocument.CSI_LineNoInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);

			previousDocument.CSI_LineNo = 1;
			AssertHasMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);

			previousDocument.CSI_LineNo = ZShort.Zero;
			AssertNoMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_LineNo validation when CSI_Procedure is LC", delegate
		{
			AssertNoWarningContaining("No warning should be displayed when LineNo and Tariff are empty and is not RP Document", previousDocument.CSI_LineNoInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);

			previousDocument.CSI_LineNo = 1;
			AssertHasMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);

			previousDocument.CSI_LineNo = ZShort.Zero;
			AssertNoMessageErrorContaining("CSI_LineNoInfo", previousDocument.CSI_LineNoInfo, MandatoryValidation.DoNotEntered);
		});

		CombineAssertions("Checking CSI_LineNo validation when is RP Document", delegate
		{
			previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione;
			Assert("Pre-condition: should be RP document", previousDocument.IsPreviousProcedureDocument);
			AssertHasWarningContaining("A warning should be displayed when LineNo and Tariff are empty", previousDocument.CSI_LineNoInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);

			previousDocument.CSI_LineNo = 1;
			AssertNoWarningContaining("No warning should be displayed when LineNo and Tariff are empty", previousDocument.CSI_LineNoInfo, ValidationCaptions.PreviousDocument.ForRpDocumentsLineNoAndTariffAreEmpty);
		});
	}

	public void TestCheckCSI_UnitOfQuantity_NoMessageErrorsIfFieldIsReadOnly()
	{
		declaration.JE_MessageType = "IMP";

		previousDocument.CSI_Procedure = "NUM";
		previousDocument.CSI_UnitOfQuantity = "";
		AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantityInfo);
	}

	public void TestCheckCSI_UnitOfQuantity3_NoMessageErrorsIfFieldIsReadOnly()
	{
		declaration.JE_MessageType = "IMP";

		previousDocument.CSI_Procedure = "NUM";
		previousDocument.CSI_UnitOfQuantity3 = "";
		AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantity3Info);
	}

	public void TestCheckCSI_Quantity_TotalDigits()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var previousDocumentOfInvoiceHeader = invoiceHeader.PreviousDocuments.AddNew();
		var previousDocumentOfInvoiceLine = invoiceHeader.InvoiceLines.AddNew().PreviousDocuments.AddNew();
		var previousDocumentOfEntryInstruction = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();

		var decimalsWithLength16 = new[] { 123456789012.1234m, 1234567890.123456m, 02234567890.123456000m };
		var decimalsWithLengthGreaterThan16 = new[] { 1234567890123.1234m, 123456789012.12345m, 12345678901.123456m };

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions("When Export and UCC6", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
			{
				AssertQuantityDigitNumber(previousDocumentOfInvoiceHeader, decimalsWithLength16, errorExpected: false);
				AssertQuantityDigitNumber(previousDocumentOfInvoiceHeader, decimalsWithLengthGreaterThan16, errorExpected: true);

				AssertQuantityDigitNumber(previousDocumentOfInvoiceLine, decimalsWithLength16, errorExpected: false);
				AssertQuantityDigitNumber(previousDocumentOfInvoiceLine, decimalsWithLengthGreaterThan16, errorExpected: true);

				AssertQuantityDigitNumber(previousDocumentOfEntryInstruction, decimalsWithLengthGreaterThan16, errorExpected: false);
			}
		});

		CombineAssertions("When Export and not UCC6", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: false))
			{
				AssertQuantityDigitNumber(previousDocumentOfInvoiceHeader, decimalsWithLengthGreaterThan16, errorExpected: false);
				AssertQuantityDigitNumber(previousDocumentOfInvoiceLine, decimalsWithLengthGreaterThan16, errorExpected: false);

				AssertQuantityDigitNumber(previousDocumentOfEntryInstruction, decimalsWithLengthGreaterThan16, errorExpected: false);
			}
		});

		Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");

		CombineAssertions("When Import", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;

				AssertQuantityDigitNumber(previousDocumentOfInvoiceLine, decimalsWithLength16, errorExpected: false);
				AssertQuantityDigitNumber(previousDocumentOfInvoiceLine, decimalsWithLengthGreaterThan16, errorExpected: true);

				AssertQuantityDigitNumber(previousDocumentOfEntryInstruction, decimalsWithLength16, errorExpected: false);
				AssertQuantityDigitNumber(previousDocumentOfEntryInstruction, decimalsWithLengthGreaterThan16, errorExpected: true);
			}
		});

		CombineAssertions("Without declaration", () =>
		{
			var orphanPreviousDocument = Factory.New<PreviousDocument>();
			AssertQuantityDigitNumber(orphanPreviousDocument, decimalsWithLengthGreaterThan16, errorExpected: false);
		});

		CombineAssertions("When previous Document NOT in [Invoice Header , Invoice Line, Entry Instruction]", () =>
		{
			using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
			{
				var previousDocumentOther = declaration.PreviousDocuments.AddNew();
				AssertQuantityDigitNumber(previousDocumentOther, decimalsWithLengthGreaterThan16, errorExpected: false);
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = Factory.New<PreviousDocumentForTest>();
		declaration
			.CustomsEntryInstructions.AddNew()
			.PreviousDocuments.Add(previousDocument);
	}

	void AssertQuantityDigitNumber(PreviousDocument previousDocument, decimal[] values, bool errorExpected)
	{
		const string expectedMessageError = "The maximum allowed number of digits is 16.";

		var previousDocumentType = GetPreviousDocumentType(previousDocument.CSI_ParentTableCode);
		values.ForEach((value) =>
		{
			previousDocument.CSI_Quantity = value;
			if (errorExpected)
			{
				AssertHasMessageError($"When Previous Document type is {previousDocumentType}, CSI_Quantity = {value}", previousDocument.CSI_QuantityInfo, expectedMessageError);
			}
			else
			{
				AssertNoMessageError($"When Previous Document type is {previousDocumentType}, CSI_Quantity = {value}", previousDocument.CSI_QuantityInfo, expectedMessageError);
			}
		});
	}

	string GetPreviousDocumentType(string parentTableCode)
	{
		switch (parentTableCode)
		{
			case JobComInvoiceHeaderSchema.Constants.Prefix:
				return "Invoice Header";
			case JobComInvoiceLineSchema.Constants.Prefix:
				return "Invoice Line";
			case CusEntryInstructionSchema.Constants.Prefix:
				return "Entry Instruction";
			default:
				return "Unknown";
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool isUCC6) => EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	JobDeclaration declaration;

	PreviousDocumentForTest previousDocument;
}
