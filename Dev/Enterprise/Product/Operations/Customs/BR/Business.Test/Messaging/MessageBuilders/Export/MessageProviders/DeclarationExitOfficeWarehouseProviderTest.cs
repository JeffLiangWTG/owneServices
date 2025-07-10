using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationExitOfficeWarehouseProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationExitOfficeWarehouse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitOfficeWareHouse = new DeclarationExitOfficeWarehouseProvider(declaration);
			AssertNullOrEmptyOrWhitespace("ID should be empty", exitOfficeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 0m, exitOfficeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 0m, exitOfficeWareHouse.LongitudeMeasure);
			AssertNullOrEmptyOrWhitespace("AddressLine should be empty", exitOfficeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", true, exitOfficeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", false, exitOfficeWareHouse.HomeDispatch);

			declaration.BoardingOfficeIsCustomsEnclosure = false;

			var docAddress = declaration.BoardingLocalAddress;
			docAddress.DocAddressType = DocAddressType.BoardingLocalDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "Address1";
			docAddress.E2_Address2 = "Address2";

			declaration.ClearanceOfficeIsHomeDispatch = true;
			var docAddress2 = declaration.ClearanceLocalInvolvedParty;
			docAddress2.DocAddressType = DocAddressType.ClearanceLocalInvolvedParty;
			docAddress2.E2_AddressOverride = true;
			docAddress2.E2_GovRegNum = "25.043.511/0001-11";

			exitOfficeWareHouse = new DeclarationExitOfficeWarehouseProvider(declaration);
			AssertEquals("ID should be", "25043511000111", exitOfficeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 0m, exitOfficeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 0m, exitOfficeWareHouse.LongitudeMeasure);
			AssertEquals("AddressLine should be", "Address1 Address2", exitOfficeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", false, exitOfficeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", true, exitOfficeWareHouse.HomeDispatch);

			declaration.BoardingOfficeIsCustomsEnclosure = true;
			declaration.BoardingEnclosureCode = "8289050";

			exitOfficeWareHouse = new DeclarationExitOfficeWarehouseProvider(declaration);
			AssertEquals("ID should be", "8289050", exitOfficeWareHouse.ID);
			AssertEquals("Latitude Measure should be", 0m, exitOfficeWareHouse.LatitudeMeasure);
			AssertEquals("Longitude Measure should be", 0m, exitOfficeWareHouse.LongitudeMeasure);
			AssertNullOrEmptyOrWhitespace("AddressLine should be empty", exitOfficeWareHouse.AddressLine);
			AssertEquals("InCustomsEnclosure should be", true, exitOfficeWareHouse.InCustomsEnclosure);
			AssertEquals("HomeDispatch should be", true, exitOfficeWareHouse.HomeDispatch);
		}
	}
}
