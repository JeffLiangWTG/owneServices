using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryInstructionValidation))]
sealed class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEI_Style()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_StyleInfo, new ZString[] { "XX" }, Lookups.StyleList.GetAllCodesZString());

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertFieldIsNotMandatory(Instruction.CEI_StyleInfo);
	}

	public void TestCheckCEI_SubStyle()
	{
		RefDataSetupTestHelper.SetupNFEICategoryCodes(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Instruction.CEI_Style = DeclarationTypeList.Codes.NoForeignExchangeInvolved;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Instruction.CEI_SubStyleInfo, new ZString[] { "XX" }, Lookups.EntrySubStyleList.GetAllCodesZString());

		Instruction.CEI_Style = DeclarationTypeList.Codes.ForeignExchangeInvolved;
		Instruction.CEI_SubStyle = ZString.Empty;
		ValidationTestHelper.AssertFieldIsNotMandatory(Instruction.CEI_SubStyleInfo);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertFieldIsNotMandatory(Instruction.CEI_SubStyleInfo);
	}

	public void TestCheckCEI_NumberOfPackages()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertWarningIfNotEntered(Instruction.CEI_NumberOfPackagesInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertNoWarningIfNotEntered(Instruction.CEI_NumberOfPackagesInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestValidateRBIWaiverNumber()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryinstruction = Declaration.CustomsEntryInstructions.AddNew();
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(entryinstruction.RBIWaiverNumberInfo, entryinstruction.CEI_SubStyleInfo);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryinstruction.CEI_SubStyle = "01";
		entryinstruction.Validation.ValidateRBIWaiverNumber();
		ValidationTestHelper.AssertFieldIsNotMandatory(entryinstruction.RBIWaiverNumberInfo);
	}

	public void TestValidateRBIWaiverDate()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryinstruction = Declaration.CustomsEntryInstructions.AddNew();
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(entryinstruction.RBIWaiverDateInfo, entryinstruction.CEI_SubStyleInfo);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryinstruction.CEI_SubStyle = "01";
		entryinstruction.Validation.ValidateRBIWaiverDate();
		ValidationTestHelper.AssertFieldIsNotMandatory(entryinstruction.RBIWaiverDateInfo);
	}

	public void TestValidateTotalInvoices()
	{
		var message = "Only 99 Invoices are supported under one Entry Instructions/Shipping Bill.";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryinstruction = Declaration.CustomsEntryInstructions.AddNew();
		entryinstruction.Validation.ValidateAll();
		AssertNoRowMessageError("Not Greater than 99", entryinstruction, message);

		JobComInvoiceHeader invoice;
		JobComInvoiceLine line;
		var entryinstruction1 = Declaration.CustomsEntryInstructions.AddNew();
		for (var i = 0; i < 100; i++)
		{
			invoice = Declaration.Invoices.AddNew();
			line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CEI = entryinstruction1.PK;
		}
		entryinstruction.Validation.ValidateAll();
		AssertNoRowMessageError("Add in other entryinstruction", entryinstruction, message);

		for (var i = 0; i < 100; i++)
		{
			invoice = Declaration.Invoices.AddNew();
			line = invoice.JobComInvoiceLines.AddNew();
			line.JI_CEI = entryinstruction.PK;
		}
		entryinstruction.Validation.ValidateAll();
		AssertHasRowMessageError("Greater than 99", entryinstruction, message);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryinstruction.Validation.ValidateAll();
		AssertNoRowMessageError("Greater than 99 but not export", entryinstruction, message);
	}

	public void TestCheckCEI_TotalContainer()
	{
		var message = "Only 99 containers are allowed per Entry Instructions.";

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var info = Instruction.CEI_TotalContainerInfo;
		CombineAssertions(() =>
		{
			Instruction.CEI_TotalContainer = 100;
			AssertNoMessageError("Greater than 99 but not Containerised", info, message);

			Declaration.JE_ContainerMode = INContainerModeList.Codes.ContainerisedAndPackaged;
			Instruction.Validation.ValidateCEI_TotalContainer();
			AssertHasMessageError("Greater than 99", info, message);

			Instruction.CEI_TotalContainer = 99;
			AssertNoMessageError("Equal to 99", info, message);

			Instruction.CEI_TotalContainer = 1;
			AssertNoMessageError("Not greater than 99", info, message);
		});
	}

	public void TestCheckShippingBillNumber()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Instruction.ShippingBillNumberOverride = true;

		Instruction.ShippingBillNumber = "ABC";
		AssertHasError(Instruction.ShippingBillNumberInfo, "Shipping Bill No. must be numeric");

		Instruction.ShippingBillNumber = "123";
		AssertNoError(Instruction.ShippingBillNumberInfo, "Shipping Bill No. must be numeric");
	}

	[TestDate(2024, 06, 13, 08, 08, 01)]
	public void TestCheckShippingBillNumber_UniqInSameFinancialYear()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Instruction.ShippingBillNumber = "123";
		Instruction.ShippingBillDate = ZDateTime.Now;
		Factory.Save();

		var instruction2 = Declaration.CustomsEntryInstructions.AddNew();
		instruction2.ShippingBillNumberOverride = true;
		instruction2.ShippingBillNumber = "123";
		instruction2.ShippingBillDate = ZDateTime.Now.AddDays(-1);
		AssertHasError(instruction2.ShippingBillNumberInfo, "Shipping Bill No. is already present in " + Declaration.JobNumber);

		instruction2.ShippingBillNumber = "456";
		AssertNoErrors(instruction2.ShippingBillNumberInfo);

		var instruction3 = Declaration.CustomsEntryInstructions.AddNew();
		instruction3.ShippingBillNumberOverride = true;
		instruction3.ShippingBillNumber = "456";
		instruction3.ShippingBillDate = ZDateTime.Now.AddDays(-1);
		AssertHasError(instruction3.ShippingBillNumberInfo, "Shipping Bill No. is already present in " + Declaration.JobNumber);
	}

	[TestDate(2024, 06, 13, 08, 08, 01)]
	public void TestCheckShippingBillDate()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Instruction.ShippingBillNumberOverride = true;

		Instruction.ShippingBillDate = ZDateTime.Now;
		AssertNoErrors(Instruction.ShippingBillDateInfo);

		Instruction.ShippingBillDate = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
		AssertHasErrorContaining(Instruction.ShippingBillDateInfo, "the limit for this field.");

		Instruction.ShippingBillDate = ZDateTime.Now.AddDays(1);
		AssertHasError(Instruction.ShippingBillDateInfo, "Shipping Bill Date cannot be a future date.");
	}

	public void TestCheckCEI_WeightUQ()
	{
		const string expectedMessageErrorInvalidGrossWeight = "Gross Weight is greater than Max Value supported 9999999999.999.";
		const string expectedMessageInvalidNetWeight = "Gross Weight is less than Net Weight.";
		CombineAssertions(() =>
		{
			Header.CH_CEI_Instruction = Instruction.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.CustomsEntryHeaders.Add(header);

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_NetWeight = 10.000;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			instruction.Validation.ValidateCEI_WeightUQ();
			AssertHasMessageError("Gross Weight is less than Net Weight.", instruction.CEI_WeightUQInfo, expectedMessageInvalidNetWeight);

			invoiceLine.JI_Weight = 10.000;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			instruction.Validation.ValidateCEI_WeightUQ();
			AssertNoMessageError("Gross Weight is less than Net Weight.", instruction.CEI_WeightUQInfo, expectedMessageInvalidNetWeight);
			AssertNoMessageError("Gross Weight is less than Max Value supported", instruction.CEI_WeightUQInfo, expectedMessageErrorInvalidGrossWeight);

			invoiceLine.JI_Weight = 9999999999.999m;
			instruction.Validation.ValidateCEI_WeightUQ();
			AssertNoMessageError("Gross Weight is equal to Max Value supported", instruction.CEI_WeightUQInfo, expectedMessageErrorInvalidGrossWeight);

			invoiceLine.JI_Weight = 10000000000.000m;
			instruction.Validation.ValidateCEI_WeightUQ();
			AssertHasMessageError("Gross Weight is greater than Max Value supported", instruction.CEI_WeightUQInfo, expectedMessageErrorInvalidGrossWeight);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(instruction.CEI_WeightUQInfo, new ZString[] { "K" }, Lookups.WeightUQList.GetAllCodesZString());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(Instruction.CEI_WeightUQInfo);
		});
	}

	CusEntryInstructionLookups Lookups => lookups ??= Instruction.Lookups;
	CusEntryInstructionLookups lookups;

	CusEntryInstruction Instruction => instruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction instruction;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryHeader Header => header ??= Factory.New<CusEntryHeader>();
	CusEntryHeader header;
}
