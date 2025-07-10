using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.DeviceManagement.Service
{
	internal class EdiDeviceLicenceServiceTest : TestCaseWithFactory
	{
		public void TestGetEnterpriseCodeList()
		{
			var enterpriseCode = service.GetEnterpriseCodeList().Single();
			AssertEquals("BBB", enterpriseCode.Code);
			AssertEquals("My Company", enterpriseCode.Description);
		}

		public void TestGetServerCodeList()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "CCC - MY COMPANY APARTMENT A 123 TEST STREET SYDNEY NSW", "DDD - MY COMPANY BLOCK B 456 UNIT WAY SYDNEY NSW" }, service.GetServerCodeList("BBB").Select(x => x.CodeAndDescription).ToArray());
		}

		public void TestGetCustomer()
		{
			var customerPK = service.GetCustomer("BBB", "CCC");
			AssertEquals(orgHeaderPK, customerPK);
		}

		public void TestGetCustomer_NoLicence()
		{
			AssertEquals(service.GetCustomer("BBB", "EEE"), Guid.Empty);
		}

		EdiDeviceLicenceService service;
		Guid orgHeaderPK;
		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			orgHeader.OH_FullName = "My Company";
			orgHeaderPK = orgHeader.PK.ToGuid();
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_OH = orgHeaderPK;
			licenceEnterprise.LE_EnterpriseCode = "BBB";
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_OH = orgHeaderPK;
			orgAddress1.OA_State = "NSW";
			orgAddress1.OA_Address1 = "Apartment A";
			orgAddress1.OA_Address2 = "123 Test Street";
			orgAddress1.OA_City = "Sydney";
			var licenceDatabase1 = Factory.New<LicenceDatabase>();
			licenceDatabase1.LD_LE = licenceEnterprise.PK;
			licenceDatabase1.LD_ServerCode = "CCC";
			licenceDatabase1.LD_OA_SoftwareInstallAddressDetails = orgAddress1.PK;
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = orgHeader.PK;
			orgAddress2.OA_State = "NSW";
			orgAddress2.OA_Address1 = "Block B";
			orgAddress2.OA_Address2 = "456 Unit Way";
			orgAddress2.OA_City = "Sydney";
			var licenceDatabase2 = Factory.New<LicenceDatabase>();
			licenceDatabase2.LD_LE = licenceEnterprise.PK;
			licenceDatabase2.LD_ServerCode = "DDD";
			licenceDatabase2.LD_OA_SoftwareInstallAddressDetails = orgAddress2.PK;
			Factory.Save();
			service = new EdiDeviceLicenceService();
		}
	}
}
