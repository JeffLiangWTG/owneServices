using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalInformationLookups))]
sealed class AdditionalInformationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAdditionalInformationCodeList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodes(Factory);
		RefCusCodeTestHelper.CreateExportAddDocAdditionalInformationCodes(Factory);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var list = InvoiceLineAdditionalInformation.Lookups.AdditionalInformationCodeList;
		AssertExportList("Invoice Line", RefCusCodeTestHelper.ValidExportAddDocAdditionalInformationCodeItemLevel, RefCusCodeTestHelper.ValidExportAddDocAdditionalInformationCodeHeaderLevel);

		list = EntryInstructionAdditionalInformation.Lookups.AdditionalInformationCodeList;
		AssertExportList("Entry Instruction", RefCusCodeTestHelper.ValidExportAddDocAdditionalInformationCodeHeaderLevel, RefCusCodeTestHelper.ValidExportAddDocAdditionalInformationCodeItemLevel);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		list = InvoiceLineAdditionalInformation.Lookups.AdditionalInformationCodeList;
		AssertEquals($"{Declaration.JE_MessageType} valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly));
		AssertEquals($"{Declaration.JE_MessageType} invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidAdditionalInformationCode));

		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);
		JobComInvoiceLine.JI_CEI = entryInstruction.PK;
		list = InvoiceLineAdditionalInformation.Lookups.AdditionalInformationCodeList;
		AssertEquals($"{Declaration.JE_MessageType} valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly_Expired));
		AssertEquals($"{Declaration.JE_MessageType} invalid code", false, list.ContainsCode(RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly));

		void AssertExportList(string listName, string validLevel, string invalidLevel)
		{
			AssertEquals($"{Declaration.JE_MessageType} {listName}: valid level code", true, list.ContainsCode(validLevel));
			AssertEquals($"{Declaration.JE_MessageType} {listName}: wrong level code", false, list.ContainsCode(invalidLevel));
			AssertEquals($"{Declaration.JE_MessageType} {listName}: wrong attribute value code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidExportAddDocAdditionalInformationCodeWithWrongAttributeValue));
			AssertEquals($"{Declaration.JE_MessageType} {listName}: import code", false, list.ContainsCode(RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly));
			AssertEquals($"{Declaration.JE_MessageType} {listName}: invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidExportAddDocAdditionalInformationCode));
			AssertEquals($"{Declaration.JE_MessageType} {listName}: invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidAdditionalInformationCode));
		}
	});

	public void TestReferenceNumberCodeList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateFreeZoneTrafficCodes(Factory);
		RefCusCodeTestHelper.CreateBorderZoneTrafficCodes(Factory);
		RefCusCodeTestHelper.CreateExportCodeMineralOil(Factory);

		InvoiceLineAdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
		var list = InvoiceLineAdditionalInformation.Lookups.ReferenceNumberCodeList;
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidFreeZoneTrafficCode));
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidFreeZoneTrafficCode));

		InvoiceLineAdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
		list = InvoiceLineAdditionalInformation.Lookups.ReferenceNumberCodeList;
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidBorderZoneTrafficCode));
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidBorderZoneTrafficCode));

		InvoiceLineAdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		list = InvoiceLineAdditionalInformation.Lookups.ReferenceNumberCodeList;
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} valid code", true, list.ContainsCode(RefCusCodeTestHelper.ValidExportCodeMineralOil));
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} invalid code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidExportCodeMineralOil));
	});

	public void TestDescriptionCodeList()
	{
		RefCusCodeTestHelper.CreateTBMGCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGACodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGBCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGCCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGDCodeList(Factory);
		RefCusCodeTestHelper.CreateTBSGECodeList(Factory);

		var additionalInformation = JobComInvoiceLine.AdditionalInformations.AddNew();

		JobComInvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;

		InvoiceLineAdditionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		var listTBMG = InvoiceLineAdditionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} valid code", true, listTBMG.ContainsCode(RefCusCodeTestHelper.ValidTBMGCode));
		AssertEquals($"CSI_Code={InvoiceLineAdditionalInformation.CSI_Code} invalid code", false, listTBMG.ContainsCode(RefCusCodeTestHelper.InvalidTBMGCode));

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;
		var list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} list Empty", 0, list.Count);

		InvoiceLineAdditionalInformation.CSI_Description = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigars;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} valid Code", true, list.ContainsCode(RefCusCodeTestHelper.ValidTBSGACode));
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} invalid Code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidTBSGACode));

		InvoiceLineAdditionalInformation.CSI_Description = UniversalReferenceConstants.TobaccoMainGroupCodes.Cigarettes;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} valid Code", true, list.ContainsCode(RefCusCodeTestHelper.ValidTBSGBCode));
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} invalid Code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidTBSGBCode));

		InvoiceLineAdditionalInformation.CSI_Description = UniversalReferenceConstants.TobaccoMainGroupCodes.CutTobacco;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} valid Code", true, list.ContainsCode(RefCusCodeTestHelper.ValidTBSGCCode));
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} invalid Code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidTBSGCCode));

		InvoiceLineAdditionalInformation.CSI_Description = UniversalReferenceConstants.TobaccoMainGroupCodes.Assortment;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} valid Code", true, list.ContainsCode(RefCusCodeTestHelper.ValidTBSGDCode));
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} invalid Code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidTBSGDCode));

		InvoiceLineAdditionalInformation.CSI_Description = UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} valid Code", true, list.ContainsCode(RefCusCodeTestHelper.ValidTBSGECode));
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} invalid Code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidTBSGECode));

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		list = additionalInformation.Lookups.DescriptionCodeList;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} list Empty", 0, list.Count);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;
		JobComInvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertEquals($"CSI_Code={additionalInformation.CSI_Code} and CSI_Description of A1402 additional info = {InvoiceLineAdditionalInformation.CSI_Description} but RefundType = {JobComInvoiceLine.JI_RefundType} list Empty", 0, list.Count);
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	AdditionalInformation EntryInstructionAdditionalInformation => entryInstructionAdditionalInformation ??= Declaration.CustomsEntryInstructions.AddNew().AdditionalInformations.AddNew();
	AdditionalInformation entryInstructionAdditionalInformation;

	AdditionalInformation InvoiceLineAdditionalInformation => invoiceLineAdditionalInformation ??= JobComInvoiceLine.AdditionalInformations.AddNew();
	AdditionalInformation invoiceLineAdditionalInformation;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= Declaration.Invoices.AddNew().InvoiceLines.AddNew();
	JobComInvoiceLine jobComInvoiceLine;
}
