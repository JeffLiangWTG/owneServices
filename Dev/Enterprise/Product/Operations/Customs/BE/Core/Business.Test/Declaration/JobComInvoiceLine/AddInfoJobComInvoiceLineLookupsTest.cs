using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestRegionOfDestinationList()
	{
		var invoiceLine = GetInvoiceLine();
		var lookups = invoiceLine.AddInfoLookups;
		var list = lookups.RegionOfDestinationList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "1, 2, 3", ((CodeDescriptionPairList)list).CodesAsString);
			AssertSame("Cached", lookups.RegionOfDestinationList, list);
		});
	}

	public void TestLookupCountryOfDestination()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "US", "United States", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "AU", "Australia", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "DE", "Germany", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var invoiceLine = GetInvoiceLine();
		var lookups = invoiceLine.AddInfoLookups;
		var list = lookups.CountryOfDestinationList;
		var exportDeclaration = Factory.New<JobDeclaration>();
		exportDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var exportInvoiceLine = exportDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
		var exportLookups = exportInvoiceLine.AddInfoLookups;
		var exportList = exportLookups.CountryOfDestinationList;
		exportList.Load();
		list.Load();
		CombineAssertions(() =>
		{
			AssertType<ZZRefCusCodeListCombinedCollection>("Import Collection Type", list);
			AssertArrayEqualsByElements("Import values", new ZString[] { "DE", "FR", "IT" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
			AssertSame("Import Cached", lookups.CountryOfDestinationList, list);

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", exportList);
			AssertArrayEqualsByElements("Export values", new ZString[] { "AU", "US" }, exportList.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
			AssertSame("Export Cached", exportLookups.CountryOfDestinationList, exportList);
		});
	}

	JobComInvoiceLine GetInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		return declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
}
