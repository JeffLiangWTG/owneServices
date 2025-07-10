using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class IB3HeaderHelperTest : TestCaseWithFactory
	{
		public void TestGetGSTNumber()
		{
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			AssertEquals("No errors with no gst number", ZString.Empty, IB3HeaderHelper.GetGSTNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "0987654321", canada);
			AssertEquals("GST number returned", "0987654321", IB3HeaderHelper.GetGSTNumber(orgHeader));
			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "987654321RT0001", canada);
			AssertEquals("Empty returned if not 10 characters", ZString.Empty, IB3HeaderHelper.GetGSTNumber(orgHeader));
		}

		public void TestGetBrokerBusinessNumber()
		{
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			AssertEquals(ZString.Empty, IB3HeaderHelper.GetBrokerBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123RM123", canada);
			AssertEquals("123RM123", IB3HeaderHelper.GetBrokerBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, "456RM456", canada);
			AssertEquals("456RM456", IB3HeaderHelper.GetBrokerBusinessNumber(orgHeader));
		}

		public void TestGetLVSBusinessNumber()
		{
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			AssertEquals(ZString.Empty, IB3HeaderHelper.GetLVSBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123RM123", canada);
			AssertEquals("123RM123", IB3HeaderHelper.GetLVSBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "456RM456", canada);
			AssertEquals("456RM456", IB3HeaderHelper.GetLVSBusinessNumber(orgHeader));
		}

		public void TestGetBusinessNumber()
		{
			var canada = Factory.Load<RefCountry>(Constants.CountryGuids.Canada);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			AssertEquals(ZString.Empty, IB3HeaderHelper.GetBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "123RM123", canada);
			AssertEquals("123RM123", IB3HeaderHelper.GetBusinessNumber(orgHeader));
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "456RM456", canada);
			AssertEquals("456RM456", IB3HeaderHelper.GetBusinessNumber(orgHeader));

			orgHeader.CustomsCodes.RemoveAndDeleteAll();
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.FillWithValidTestData();

			AssertEquals(ZString.Empty, IB3HeaderHelper.GetBusinessNumber(true, orgHeader, orgHeader1));
			AssertEquals(ZString.Empty, IB3HeaderHelper.GetBusinessNumber(false, orgHeader, orgHeader1));

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123RM123", canada);
			AssertEquals("123RM123", IB3HeaderHelper.GetBusinessNumber(true, orgHeader, orgHeader1));
			AssertEquals("123RM123", IB3HeaderHelper.GetBusinessNumber(false, orgHeader, orgHeader1));

			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "234RM234", canada);
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "345RM345", canada);
			AssertEquals("234RM234", IB3HeaderHelper.GetBusinessNumber(true, orgHeader, orgHeader1));
			AssertEquals("345RM345", IB3HeaderHelper.GetBusinessNumber(false, orgHeader, orgHeader1));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "456RM456", canada);
			AssertEquals("456RM456", IB3HeaderHelper.GetBusinessNumber(true, orgHeader, orgHeader1));
			AssertEquals("456RM456", IB3HeaderHelper.GetBusinessNumber(false, orgHeader, orgHeader1));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "567RM567", canada);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "789RM789", canada);
			AssertEquals("567RM567", IB3HeaderHelper.GetBusinessNumber(true, orgHeader, orgHeader1));
			AssertEquals("789RM789", IB3HeaderHelper.GetBusinessNumber(false, orgHeader, orgHeader1));
		}
	}
}
