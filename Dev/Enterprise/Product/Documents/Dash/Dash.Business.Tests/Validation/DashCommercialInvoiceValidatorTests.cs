using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.Validation;

namespace Enterprise.Dash.Business.Tests.Validation
{
	public class DashCommercialInvoiceValidatorTests : TestCaseWithFactory
	{
		public void TestValidate_Returns_ErrorMessage_When_MandatoryDataRule_Returns_ErrorMessage()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = string.Empty;

			var validator = new DashCommercialInvoiceValidator();

			// act
			var errorMessage = validator.Validate(dashCommercialInvoice);

			// assert
			var expectedErrorMessage = $"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_InvoiceNumber)} must not be empty";

			AssertEquals(expectedErrorMessage, errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_InvoiceTotalsRule_Returns_ErrorMessage()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 200m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = 20m;
			line.DLI_Quantity = 10;
			line.DLI_LineTotal = 20;

			var validator = new DashCommercialInvoiceValidator();

			// act
			var errorMessage = validator.Validate(dashCommercialInvoice);

			// assert
			var expectedErrorMessage = $"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(DashCommercialInvoiceLineItem.DLI_LineTotal)} is not equal to the product of {nameof(DashCommercialInvoiceLineItem.DLI_Quantity)} and {nameof(DashCommercialInvoiceLineItem.DLI_PricePerUnit)}. {nameof(DashCommercialInvoiceLineItem.DLI_LineTotal)}: 20. Calculated line total: 200";

			AssertEquals(expectedErrorMessage, errorMessage);
		}

		public void TestValidate_Returns_Empty_String_When_Validation_Passed()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 200m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = 20m;
			line.DLI_Quantity = 10;
			line.DLI_LineTotal = 200;

			var validator = new DashCommercialInvoiceValidator();

			// act
			var errorMessage = validator.Validate(dashCommercialInvoice);

			// assert
			AssertEquals(string.Empty, errorMessage);
		}
	}
}
