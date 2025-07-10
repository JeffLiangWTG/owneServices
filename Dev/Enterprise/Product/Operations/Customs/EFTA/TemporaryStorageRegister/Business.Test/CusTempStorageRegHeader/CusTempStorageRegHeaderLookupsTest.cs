using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderLookups))]
sealed class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPackageTypeListLookup()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateCusCodeType("UNPKG", "Packagings", "UNE");
		_ = helper.CreateCusCodeList("UNE", "UNPKG", "ABC", "abc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateCusCodeList("UNE", "UNPKG", "DEF", "def", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		AssertEquals("ABC, DEF", header.Lookups.PackTypeList.CodesAsString);
	}

	public void TestCustomsOfficeList()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		AssertSame(header.Lookups.CustomsOfficeList.Factory, header.Lookups.CustomsOfficeList.Factory);
	}

	public void TestPreviousReferenceTypeList()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "Previous Documents Of PNTS");
			_ = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");
			_ = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Purpose", "Purpose", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, Core.Constants.CountryCodes.France);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "A", "Test 1", yesterday, tomorrow);
			_ = codeLists1.Attributes.AddNew("Purpose", "Declaration");
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "B", "Test 1", yesterday, tomorrow);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "B", "Test 2", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_AppCode = "123";
			header.SRH_Status = "OK";
			header.SRH_Reference = "TEST";
			var lookups = new CusTempStorageRegHeaderLookups(header);
			AssertEquals("Should contain CusCodeList items of type DC40T", "A", lookups.PreviousReferenceTypeList.CodesAsString);
		}
	}
}
