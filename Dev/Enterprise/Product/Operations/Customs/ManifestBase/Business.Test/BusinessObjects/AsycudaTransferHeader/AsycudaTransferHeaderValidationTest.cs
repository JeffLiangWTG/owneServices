using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaTransferHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckATF_TransferType()
		{
			var transferHeader = Factory.New<AsycudaTransferHeader>();
			transferHeader.Validation.ValidateATF_TransferType();
			AssertHasErrorContaining(transferHeader.ATF_TransferTypeInfo, MandatoryValidation.MustBeEntered);
			transferHeader.ATF_TransferType = "*";
			AssertNoErrorContaining(transferHeader.ATF_TransferTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(transferHeader.ATF_TransferTypeInfo, ListValidation.InvalidCodeError);
			transferHeader.ATF_TransferType = TransferTypeList.Codes.Domestic;
			AssertNoErrorContaining(transferHeader.ATF_TransferTypeInfo, ListValidation.InvalidCodeError);
		}
	}
}
