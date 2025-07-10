using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class CheckThatWarehousesWithEmptyRegistrationNumbersShowMessageError : TestCaseWithFactory
	{
		public void TestCheckThatWarehousesWithEmptyRegistrationNumbersShowMessageError()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();

			var cusOrg = org.CustomsCodes.AddNew();
			cusOrg.OK_CustomsRegNo = "12345678";
			cusOrg.OK_CodeType = "CCP";

			declaration.WarehouseDocAddress.OrganisationPK = org.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, GB.Business.Declaration.JobDeclarationValidation.E02944_ErrorMessage);

			cusOrg.OK_CustomsRegNo = "";
			declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			declaration.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertHasMessageError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, GB.Business.Declaration.JobDeclarationValidation.E02944_ErrorMessage);
		}
	}
}
