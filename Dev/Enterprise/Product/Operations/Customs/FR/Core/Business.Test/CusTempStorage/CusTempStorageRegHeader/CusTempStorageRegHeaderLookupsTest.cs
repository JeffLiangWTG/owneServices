using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPreviousReferenceTypeListWhenSRH_AppCodeIsSTO()
		{
			header.SRH_AppCode = FRConstants.TemporaryStorage.AppCodeIST;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "Previous Documents Of PNTS");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "A", "Test 1", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "B", "Test 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "C", "Test 3", yesterday, tomorrow);
			Factory.Save();

			var list = Factory.GetCachedValue<PreviousDocumentCodeList>();
			AssertSame("PreviousReferenceTypeList override the result only when SRH_AppCode= IST", list, lookups.PreviousReferenceTypeList);
		}

		public void TestPreviousReferenceTypeListWhenSRH_AppCodeIsNotSTO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "Previous Documents Of PNTS");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Purpose", "Purpose", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, Core.Constants.CountryCodes.France);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "A", "Test 1", yesterday, tomorrow);
			codeLists1.Attributes.AddNew("Purpose", "Declaration");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "B", "Test 2", yesterday, tomorrow);
			Factory.Save();

			AssertEquals("PreviousReferenceTypeList dose not override the result when SRH_AppCode= IST", "A", lookups.PreviousReferenceTypeList.CodesAsString);
		}

		public void TestStatusList()
		{
			var list = lookups.StatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "OPN, CLS", list.CodesAsString);
				AssertSame("Cached", list, lookups.StatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_AppCode = "123";
			header.SRH_Status = "OK";
			header.SRH_Reference = "TEST";
			lookups = new CusTempStorageRegHeaderLookups(header);
		}

		CusTempStorageRegHeader header;
		CusTempStorageRegHeaderLookups lookups;
	}
}
