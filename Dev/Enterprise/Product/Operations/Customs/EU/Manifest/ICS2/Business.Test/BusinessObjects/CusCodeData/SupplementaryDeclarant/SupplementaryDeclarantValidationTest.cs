using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SupplementaryDeclarantValidationTest : CusCodeDataValidationTest
	{
		public void TestCY_CodeListValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			var bill = header.Bills.AddNew();

			var supplementaryDeclarant = bill.SupplementaryDeclarants.AddNew();
			AssertNoMessageError(supplementaryDeclarant.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			supplementaryDeclarant.CY_Code = "5";
			AssertHasMessageErrorContaining(supplementaryDeclarant.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			supplementaryDeclarant.CY_Code = "1";
			AssertNoMessageError(supplementaryDeclarant.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
