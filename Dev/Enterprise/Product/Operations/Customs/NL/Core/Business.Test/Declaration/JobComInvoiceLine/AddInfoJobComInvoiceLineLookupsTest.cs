using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestLookupCountryOfDestinationExport()
	{
		var decl = Factory.New<JobDeclaration>();
		decl.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var invoiceLine = decl.Invoices.AddNew().InvoiceLines.AddNew();
		var lookups = invoiceLine.AddInfoLookups;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "US", "United States", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "AU", "Australia", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "DE", "Germany", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var list = lookups.CountryOfDestinationList;
		list.Load();

		CombineAssertions(() =>
		{
			AssertEquals("Number of elements", 2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "US"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "AU"));
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
		});
	}

	public void TestLookupCountryOfDestinationImport()
	{
		var decl = Factory.New<JobDeclaration>();
		decl.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var invoiceLine = decl.Invoices.AddNew().InvoiceLines.AddNew();
		var lookups = invoiceLine.AddInfoLookups;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "US", "United States", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "AU", "Australia", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "Country of destination");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "DE", "Germany", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		var list = lookups.CountryOfDestinationList;
		list.Load();

		CombineAssertions(() =>
		{
			AssertEquals("Number of elements", 3, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "FR"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "DE"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "IT"));
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
		});
	}
}
