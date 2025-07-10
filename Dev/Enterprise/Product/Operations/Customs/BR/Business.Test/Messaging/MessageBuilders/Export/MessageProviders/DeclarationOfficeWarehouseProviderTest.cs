using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationOfficeWarehouseProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "1017500";
			var officeWareHouse = new DeclarationOfficeWarehouseProvider(declaration);
			AssertEquals("ID should be", ZString.Empty, officeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 0m, officeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 0m, officeWareHouse.LongitudeMeasure);
			AssertEquals("AddressLine should be", ZString.Empty, officeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", true, officeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", false, officeWareHouse.HomeDispatch);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var orgAddress = header.MainAddress;
			orgAddress.OA_Address1 = "Test Line 1";
			orgAddress.OA_Address2 = "Test Line 2";
			orgAddress.OA_City = "City Test";
			orgAddress.OA_Email = "Emails Test";
			orgAddress.OA_PostCode = "PostCode";
			orgAddress.OA_State = "State";
			orgAddress.OA_Latitude = 5m;
			orgAddress.OA_Longitude = 55m;

			declaration.ClearanceOfficeIsHomeDispatch = true;
			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			declaration.JE_LocationOfGoods = "1111111";
			declaration.ClearanceLocalInvolvedParty.OrganisationPK = header.PK;

			officeWareHouse = new DeclarationOfficeWarehouseProvider(declaration);
			AssertEquals("ID should be", "58500398000105", officeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 5m, officeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 55m, officeWareHouse.LongitudeMeasure);
			AssertEquals("AddressLine should be", "Test Line 1 Test Line 2", officeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", false, officeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", true, officeWareHouse.HomeDispatch);

			var oDocAddress = declaration.ClearanceLocalInvolvedParty;
			oDocAddress.DocAddressType = DocAddressType.ClearanceLocalInvolvedParty;
			oDocAddress.E2_AddressOverride = true;
			oDocAddress.E2_CompanyName = "CompanyTest";
			oDocAddress.E2_Address1 = "Address1 Test";
			oDocAddress.E2_Address2 = "Address Complementary";
			oDocAddress.E2_City = "City";
			oDocAddress.E2_Latitude = 15m;
			oDocAddress.E2_Longitude = 10m;
			oDocAddress.E2_GovRegNum = "57.012.650/0001-74";

			officeWareHouse = new DeclarationOfficeWarehouseProvider(declaration);
			AssertEquals("ID should be", "57012650000174", officeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 15m, officeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 10m, officeWareHouse.LongitudeMeasure);
			AssertEquals("AddressLine should be", "Address1 Test Address Complementary", officeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", false, officeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", true, officeWareHouse.HomeDispatch);
		}
	}
}
