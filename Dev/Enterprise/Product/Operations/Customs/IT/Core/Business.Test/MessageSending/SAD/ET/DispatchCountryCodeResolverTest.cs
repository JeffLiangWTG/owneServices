using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DispatchCountryCodeResolverTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when CusEntryHeader is null", () => new DispatchCountryCodeResolver(null));

		entryHeader = Factory.New<CusEntryHeader>();
		AssertExceptionThrown<ArgumentNullException>("Exception when Declaration is null", () => new DispatchCountryCodeResolver(entryHeader));

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Exception when Instruction is null", () => new DispatchCountryCodeResolver(entryHeader));
	}

	public void TestGetDispatchCountryCodeForHeader()
	{
		declaration.JE_RL_NKOrigin = ZString.Empty;
		AssertEquals("When JE_RL_NKOrigin is empty and EntryHeader has one OA_RN_NKCountryCode", "IT", GetHeaderWrapper(entryHeader).CountryOfDispatch);
		declaration.JE_RL_NKOrigin = "ITTAR";
		AssertEquals("When JE_RL_NKOrigin is not empty and EntryHeader has one OA_RN_NKCountryCode and they are equal", "IT", GetHeaderWrapper(entryHeader).CountryOfDispatch);
		declaration.JE_RL_NKOrigin = "KRBNP";
		AssertEquals("When JE_RL_NKOrigin is not empty and EntryHeader has one OA_RN_NKCountryCode and they are different", "KR", GetHeaderWrapper(entryHeader).CountryOfDispatch);
		orgHeader2.MainAddress.OA_RN_NKCountryCode = "ES";
		declaration.JE_RL_NKOrigin = ZString.Empty;
		entryHeader.ResetInvoiceHeadersAndLines();
		AssertEquals("When JE_RL_NKOrigin is empty and EntryHeader has more OA_RN_NKCountryCode", "", GetHeaderWrapper(entryHeader).CountryOfDispatch);
		declaration.JE_RL_NKOrigin = "KRBNP";
		AssertEquals("When JE_RL_NKOrigin is not empty and EntryHeader has more OA_RN_NKCountryCode", "", GetHeaderWrapper(entryHeader).CountryOfDispatch);
	}
	public void TestGetDispatchCountryCodeForLine()
	{
		declaration.JE_RL_NKOrigin = ZString.Empty;
		AssertEquals("When JE_RL_NKOrigin is empty and EntryHeader has one Supplier", "", GetLineWrapper(entryLine1).DispatchCountryCode);
		declaration.JE_RL_NKOrigin = "ITTAR";
		AssertEquals("When JE_RL_NKOrigin is not empty and EntryHeader has one Supplier, so one OA_RN_NKCountryCode and they are equal", "", GetLineWrapper(entryLine1).DispatchCountryCode);
		declaration.JE_RL_NKOrigin = "KRBNP";
		AssertEquals("When JE_RL_NKOrigin is not empty and EntryHeader has one Supplier, with different OA_RN_NKCountryCode", "", GetLineWrapper(entryLine1).DispatchCountryCode);
		orgHeader2.MainAddress.OA_RN_NKCountryCode = "ES";
		declaration.JE_RL_NKOrigin = ZString.Empty;
		entryHeader.ResetInvoiceHeadersAndLines();
		AssertEquals("IT - When JE_RL_NKOrigin is empty and EntryHeader has more Suppliers with different OA_RN_NKCountryCode", "IT", GetLineWrapper(entryLine1).DispatchCountryCode);
		AssertEquals("ES - When JE_RL_NKOrigin is empty and EntryHeader has more Suppliers with different OA_RN_NKCountryCode", "ES", GetLineWrapper(entryLine2).DispatchCountryCode);
		declaration.JE_RL_NKOrigin = "KRBNP";
		AssertEquals("IT - When JE_RL_NKOrigin is not empty and EntryHeader has more Suppliers with different OA_RN_NKCountryCode", "IT", GetLineWrapper(entryLine1).DispatchCountryCode);
		AssertEquals("ES - When JE_RL_NKOrigin is not empty and EntryHeader has more Suppliers with different OA_RN_NKCountryCode", "ES", GetLineWrapper(entryLine2).DispatchCountryCode);
	}
	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.MainAddress.OA_RN_NKCountryCode = "IT";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;

		orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.MainAddress.OA_RN_NKCountryCode = "IT";
		entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceHeader2.JZ_OH_Supplier = orgHeader2.PK;
	}

	ETLineWrapper GetLineWrapper(CusEntryLine entryLine) => new ETLineWrapper(entryLine);
	ETHeaderWrapper GetHeaderWrapper(CusEntryHeader entryHeader) => new ETHeaderWrapper(entryHeader);

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	OrgHeader orgHeader1;
	OrgHeader orgHeader2;

	CusEntryLine entryLine1;
	CusEntryLine entryLine2;
}
