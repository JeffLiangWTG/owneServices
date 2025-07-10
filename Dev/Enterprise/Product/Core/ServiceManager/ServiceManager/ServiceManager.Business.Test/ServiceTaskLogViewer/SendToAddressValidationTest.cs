using CargoWise.EntityFramework.Testing;

namespace Enterprise.ServiceManager.Business.Testing
{
	class SendToAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAddress()
		{
			var sendToAddress = new SendToAddress();
			sendToAddress.Validation.ValidateAddress();
			AssertHasError(sendToAddress.AddressInfo, "Please enter a value.");

			sendToAddress.Address = ";";
			AssertHasError(sendToAddress.AddressInfo, "Please enter a value.");

			sendToAddress.Address = "aaa.bbb.com";
			AssertHasError(sendToAddress.AddressInfo, "Address 'aaa.bbb.com'is not valid.");

			sendToAddress.Address = "aaa@bbb.com";
			AssertNoErrors(sendToAddress.AddressInfo);

			sendToAddress.Address = "aaa@bbb.com; aaa.ddd.com";
			AssertHasError(sendToAddress.AddressInfo, "Address 'aaa.ddd.com'is not valid.");

			sendToAddress.Address = "aaa@bbb.com; aaa@ddd.com";
			AssertNoErrors(sendToAddress.AddressInfo);
		}
	}
}