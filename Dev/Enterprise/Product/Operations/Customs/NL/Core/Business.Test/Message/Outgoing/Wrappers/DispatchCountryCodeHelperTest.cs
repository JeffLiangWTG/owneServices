using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DispatchCountryCodeHelperTest : DataProviderTestCase<DispatchCountryCodeHelper>
{
	public void TestConstructor()
	{
		entryHeader = Factory.New<CusEntryHeader>();
		AssertExceptionThrown<ArgumentNullException>("Exception when Declaration is null", () => new DispatchCountryCodeHelper(entryHeader));
	}

	public void TestGetDispatchCountryCodeForDeclarationType()
	{
		declaration.JE_RL_NKOrigin = "NLAMS";
		var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault();
		var combinations = new List<(ZString ceiStyle, ZString expectedResult)>
		{ ("H1", "NL"), ("H2", "NL"), ("H3", "NL"), ("H4", "NL"), ("H5", "NL"), ("I1", "NL"),
			("I2", ZString.Empty), ("H6", ZString.Empty) };

		foreach (var combination in combinations)
		{
			entryInstruction.CEI_Style = combination.ceiStyle;
			AssertEquals($"Declaration type: {combination.ceiStyle}", combination.expectedResult, GetGoodsItemWrapper(entryLine1).DispatchCountryCode);
		}
	}

	public void TestGetDispatchCountryCodeForEmptyNKOrigin()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertEquals("JE_RL_NKOrigin: empty and EntryLine has CountryOfDispatch filled - line1", "NL", GetGoodsItemWrapper(entryLine1).DispatchCountryCode);
			AssertEquals("JE_RL_NKOrigin: empty and EntryLine has CountryOfDispatch filled - line2", "BE", GetGoodsItemWrapper(entryLine2).DispatchCountryCode);
			AssertEquals("JE_RL_NKOrigin: empty, GoodsShipmentWrapper has empty DispatchCountryCode", string.Empty, GetGoodsShipmentWrapper(entryHeader).DispatchCountryCode);
		});
	}

	public void TestGetDispatchCountryCodeForDifferentValues()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RL_NKOrigin = "NLAMS";
			AssertEquals("JE_RL_NKOrigin: not empty and one CountryOfDispatch is different - line1", "NL", GetGoodsItemWrapper(entryLine1).DispatchCountryCode);
			AssertEquals("JE_RL_NKOrigin: not empty and one CountryOfDispatch is different - line2", "BE", GetGoodsItemWrapper(entryLine2).DispatchCountryCode);
			AssertEquals("JE_RL_NKOrigin: not empty, CountryOfDispatch on lines are different", string.Empty, GetGoodsShipmentWrapper(entryHeader).DispatchCountryCode);

			declaration.JE_RL_NKOrigin = "ITTAR";
			JobComInvoiceLine invLine1 = (JobComInvoiceLine)declaration.InvoiceLines.FirstOrDefault();
			invLine1.ZG_CountryOfDispatch = ZString.Empty;
			AssertEquals("JE_RL_NKOrigin: not empty and CountryOfDispatch is empty", "IT", GetGoodsItemWrapper(entryLine1).DispatchCountryCode);
		});
	}

	public void TestGetDispatchCountryCodeForSameValues()
	{
		declaration.JE_RL_NKOrigin = "NLAMS";
		foreach (JobComInvoiceLine invLine in declaration.InvoiceLines)
		{
			invLine.ZG_CountryOfDispatch = Core.Constants.CountryCodes.Netherlands;
		}
		CombineAssertions(() =>
		{
			AssertEquals("When JE_RL_NKOrigin is not empty and all CountryOfDispatch values are same", "NL", GetGoodsShipmentWrapper(entryHeader).DispatchCountryCode);
			AssertEquals("GoodsItemWrapper should have empty DispatchCountryCode - line1", string.Empty, GetGoodsItemWrapper(entryLine1).DispatchCountryCode);
			AssertEquals("GoodsItemWrapper should have empty DispatchCountryCode - line2", string.Empty, GetGoodsItemWrapper(entryLine2).DispatchCountryCode);
		});
	}

	protected override DispatchCountryCodeHelper GetProvider() => new DispatchCountryCodeHelper(entryHeader);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "I1";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryLine1 = entryHeader.MergedLines.AddNew();
		var invoiceHeader1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine1.ZG_CountryOfDispatch = Core.Constants.CountryCodes.Netherlands;
		invoiceHeader1.JZ_OH_Supplier = orgHeader1.PK;

		orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		entryLine2 = entryHeader.MergedLines.AddNew();
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.ZG_CountryOfDispatch = Core.Constants.CountryCodes.Belgium;
		invoiceHeader2.JZ_OH_Supplier = orgHeader2.PK;
	}

	GoodsItemWrapper GetGoodsItemWrapper(CusEntryLine entryLine) => new GoodsItemWrapper(entryLine, new JobDeclarationMessageSendingObject(entryHeader));
	GoodsShipmentWrapper GetGoodsShipmentWrapper(CusEntryHeader entryHeader) => new GoodsShipmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	OrgHeader orgHeader1;
	OrgHeader orgHeader2;

	CusEntryLine entryLine1;
	CusEntryLine entryLine2;
}
