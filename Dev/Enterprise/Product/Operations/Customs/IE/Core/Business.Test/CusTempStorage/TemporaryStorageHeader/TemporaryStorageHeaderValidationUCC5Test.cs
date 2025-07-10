using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderValidationUCC5Test : BusinessObjectValidationTestCase
	{
		public void TestCheckDeclarantAndRepresentativeAreDifferent()
		{
			var tsHeader = Factory.New<TemporaryStorageHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			tsHeader.AMA_OA_Declarant = orgAddress.PK;
			tsHeader.AMA_OA_Representative = orgAddress.PK;
			tsHeader.Validation.ValidateAMA_OA_Declarant();
			AssertEquals(false, tsHeader.AMA_OA_DeclarantInfo.HasMessageError(TemporaryStorageHeaderValidation.DeclarantAndRepresentativeShouldBeDifferentErrorMessage));
		}

		public void TestCheckRepresentativeAndDeclarantAreDifferent()
		{
			var tsHeader = Factory.New<TemporaryStorageHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			tsHeader.AMA_OA_Declarant = orgAddress.PK;
			tsHeader.AMA_OA_Representative = orgAddress.PK;
			tsHeader.Validation.ValidateAMA_OA_Representative();
			AssertEquals(false, tsHeader.AMA_OA_RepresentativeInfo.HasMessageError(TemporaryStorageHeaderValidation.RepresentativeAndDeclarantShouldBeDifferentErrorMessage));
		}
	}
}
