using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremisesValidation))]
sealed class CusTempStorageRegPremisesValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckIfDuplicatePremiseExists()
	{
		var premisesInDB = Factory.NewWithValidTestData<CusTempStorageRegPremises>();
		premisesInDB.SRP_Code = "AA";
		premisesInDB.SRP_Description = "DESC";
		premisesInDB.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premisesInDB.SRP_CustomsLocation = "AAA";
		premisesInDB.AuthorizationNumber = "123";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premisesInDB.SRP_OA_PremisesAddress = orgAddress.PK;
		Factory.Save();

		var premises = Factory.NewWithValidTestData<CusTempStorageRegPremises>();
		premises.SRP_Code = "AA";
		premises.SRP_Description = "DESC";
		premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
		premises.SRP_CustomsLocation = "AAA";
		premises.AuthorizationNumber = "123";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		premises.Validation.ValidateSRP_CustomsLocation();
		AssertHasError(premises.SRP_CustomsLocationInfo, "Duplicate premises: Already exists a Premises record with the same Type (ADT) and Location (AAA)");
	}
}
