using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Testing;

class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListImport()
	{
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Import;
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsage.Lookups.CodeList;

		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", codeList.CodesAsString, "AEOC, AEOF, AEOS, AWB, CCL, CGU, CVA, CW2, CWP, DPO, EIR, IPO, OPO, REM, REP, SDE, TEA, TST");
			AssertSame("Cached", codeList, cusAuthorizationUsage.Lookups.CodeList);
		});
	}

	public void TestCodeListExport()
	{
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsage.Lookups.CodeList;

		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", codeList.CodesAsString, "AEOC, AEOF, AEOS, CCL, CW2, CWP, EIR, IPO, OPO, REM, REP, SDE, TST");
			AssertSame("Cached", codeList, cusAuthorizationUsage.Lookups.CodeList);
		});
	}

	public void TestCodeListImportItem()
	{
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Import;
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsageItem.Lookups.CodeList;

		CombineAssertions(() =>
		{
			Assert(codeList.CodesAsString.Equals("BES1, BES2, BOI, BTI, EUS"));
			AssertSame("Cached", codeList, cusAuthorizationUsageItem.Lookups.CodeList);
		});
	}

	public void TestCodeListExportItem()
	{
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsageItem.Lookups.CodeList;

		CombineAssertions(() =>
		{
			Assert(codeList.CodesAsString.Equals("BOI, BTI"));
			AssertSame("Cached", codeList, cusAuthorizationUsageItem.Lookups.CodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsageItem = declaration.Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew();
	}
	JobDeclaration declaration;
	CusAuthorizationUsage cusAuthorizationUsage;
	CusAuthorizationUsage cusAuthorizationUsageItem;
}
