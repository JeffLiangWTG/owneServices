using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CopyAndSendToCustomsFormDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckNumberOfCopies()
		{
			var formData = new CopyAndSendToCustomsFormData();
			formData.NumberOfCopies = -1;
			var numberOfCopiesInfo = formData.NumberOfCopiesInfo;
			AssertHasErrorContaining("NumberOfCopies is negative", numberOfCopiesInfo, $"{numberOfCopiesInfo.HumanReadableName} should be greater than zero.");

			formData.NumberOfCopies = 0;
			AssertHasErrorContaining("NumberOfCopies is zero", numberOfCopiesInfo, $"{numberOfCopiesInfo.HumanReadableName} should be greater than zero.");

			formData.NumberOfCopies = 1;
			AssertNoErrors("NumberOfCopies is positive", formData.NumberOfCopiesInfo);
		}

		public void TestCheckNumberOfInvoiceLinesCopies()
		{
			var formData = new CopyAndSendToCustomsFormData();
			formData.NumberOfInvoiceLinesCopies = -1;
			var numberOfInvoiceLinesCopiesInfo = formData.NumberOfInvoiceLinesCopiesInfo;
			ValidationTestHelper.AssertErrorIfValueIsNegative(numberOfInvoiceLinesCopiesInfo, "NumberOfInvoiceLinesCopies is negative");

			formData.NumberOfInvoiceLinesCopies = 0;
			AssertNoErrors("NumberOfInvoiceLinesCopies is zero", formData.NumberOfInvoiceLinesCopiesInfo);

			formData.NumberOfInvoiceLinesCopies = 1;
			AssertNoErrors("NumberOfInvoiceLinesCopies is positive", formData.NumberOfInvoiceLinesCopiesInfo);
		}
	}
}
