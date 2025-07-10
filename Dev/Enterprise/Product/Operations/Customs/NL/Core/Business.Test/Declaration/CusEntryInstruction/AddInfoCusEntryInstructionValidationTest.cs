using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class AddInfoCusEntryInstructionValidationTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionValidationTest
{
	public void TestCheckZG_TransNature()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands", eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "12", "12 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var tranNatureListCacheKey = "ZZRefCusCodeList_All_" + RefCusCodeListTypes.GetKey(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, ZDate.Today, "", false);
		Factory.ClearCachedValue<CodeDescriptionPairList>(tranNatureListCacheKey);

		var declaration = Factory.New<JobDeclaration>();
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
		ValidationTestHelper.AssertInvalidCodeMessageError(cusEntryInstruction.ZG_TransNatureInfo, "NO", "12");

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = cusEntryInstruction.PK;

		AssertTransNatureMandatory(cusEntryInstruction, invoiceHeader, invoiceLine);

		AssertTransactionNatureAtEntryOrInvoice(cusEntryInstruction, invoiceHeader, invoiceLine);
	}

	void AssertTransNatureMandatory(CusEntryInstruction cusEntryInstruction, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine)
	{
		var expectedError = "Transaction nature must be filled for declarations with type B1, B2, C1, H1, H3, H4, H5 or I1";
		invoiceHeader.JZ_ValuationCode = ZString.Empty;
		invoiceLine.ZG_TransNature = ZString.Empty;
		cusEntryInstruction.CEI_Style = "B4";
		cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
		AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

		var validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
		foreach(var style in validDeclarationTypesForMessage)
		{
			cusEntryInstruction.CEI_Style = style;
			invoiceHeader.JZ_ValuationCode = ZString.Empty;
			invoiceLine.ZG_TransNature = ZString.Empty;
			cusEntryInstruction.ZG_TransNature = ZString.Empty;
			cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
			AssertHasMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

			invoiceLine.ZG_TransNature = "72";
			cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
			AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

			invoiceHeader.JZ_ValuationCode = "72";
			invoiceLine.ZG_TransNature = ZString.Empty;
			cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
			AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

			invoiceHeader.JZ_ValuationCode = ZString.Empty;
			cusEntryInstruction.ZG_TransNature = "72";
			cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
			AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);
		}
	}

	void AssertTransactionNatureAtEntryOrInvoice(CusEntryInstruction cusEntryInstruction, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine)
	{
		var expectedError = "Transaction nature must be filled at entry instruction OR at invoice.";
		invoiceHeader.JZ_ValuationCode = "72";
		cusEntryInstruction.ZG_TransNature = "72";
		AssertHasMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

		invoiceHeader.JZ_ValuationCode = ZString.Empty;
		invoiceLine.ZG_TransNature = "72";
		cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
		AssertHasMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);

		invoiceLine.ZG_TransNature = ZString.Empty;
		cusEntryInstruction.AddInfoValidation.ValidateZG_TransNature();
		AssertNoMessageError(cusEntryInstruction.ZG_TransNatureInfo, expectedError);
	}
}
