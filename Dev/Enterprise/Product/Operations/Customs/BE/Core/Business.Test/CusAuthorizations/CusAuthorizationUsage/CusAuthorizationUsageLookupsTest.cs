using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Testing;

class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListImport()
	{
		var cusAuthorizationUsage = GetCusAuthorizationUsage(MessageTypeList.Codes.Import);
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsage.Lookups.CodeList;

		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", codeList.CodesAsString, "BTI, BOI, BES1, BES2, EUS");
			AssertSame("Cached", codeList, cusAuthorizationUsage.Lookups.CodeList);
		});
	}

	public void TestCodeListExport()
	{
		var cusAuthorizationUsage = GetCusAuthorizationUsage(MessageTypeList.Codes.Export);
		var codeList = (CodeDescriptionPairList)cusAuthorizationUsage.Lookups.CodeList;

		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", codeList.CodesAsString, "BTI, BOI");
			AssertSame("Cached", codeList, cusAuthorizationUsage.Lookups.CodeList);
		});
	}

	CusAuthorizationUsage GetCusAuthorizationUsage(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew();
	}
}
