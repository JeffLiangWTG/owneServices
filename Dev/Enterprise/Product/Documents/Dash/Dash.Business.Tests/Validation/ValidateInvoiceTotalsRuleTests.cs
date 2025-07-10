using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business.Validation;

namespace Enterprise.Dash.Business.Tests.Validation
{
	public class ValidateInvoiceTotalsRuleTests : TestCaseWithFactory
	{
		public void TestValidate_Returns_ErrorMessage_When_LineTotal_Is_Not_Equal_To_Product_Of_Quantity_And_PricePerUnit()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_Quantity = 5;
			line.DLI_PricePerUnit = 10;
			line.DLI_LineTotal = 45;

			var validateInvoiceTotalsRule = new ValidateInvoiceTotalsRule();

			// act
			var errorMessage = validateInvoiceTotalsRule.Validate(dashCommercialInvoice);

			// assert
			var expectedErrorMessage = $"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(DashCommercialInvoiceLineItem.DLI_LineTotal)} is not equal to the product of {nameof(DashCommercialInvoiceLineItem.DLI_Quantity)} and {nameof(DashCommercialInvoiceLineItem.DLI_PricePerUnit)}. {nameof(DashCommercialInvoiceLineItem.DLI_LineTotal)}: 45. Calculated line total: 50";

			AssertEquals(expectedErrorMessage, errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Sum_Of_LineTotals_Is_Not_Equal_To_Invoice_GrossTotal()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_GrossTotal = 63;

			var line1 = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line1.DLI_Quantity = 5;
			line1.DLI_PricePerUnit = 10;
			line1.DLI_LineTotal = 50;

			var line2 = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line2.DLI_Quantity = 3;
			line2.DLI_PricePerUnit = 5;
			line2.DLI_LineTotal = 15;

			var validateInvoiceTotalsRule = new ValidateInvoiceTotalsRule();

			// act
			var errorMessage = validateInvoiceTotalsRule.Validate(dashCommercialInvoice);

			// assert
			var expectedErrorMessage = $"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_GrossTotal)} is not equal to the sum of line totals. {nameof(dashCommercialInvoice.DCI_GrossTotal)}: 63. Sum of line totals: 65";

			AssertEquals(expectedErrorMessage, errorMessage);
		}

		public void TestValidate_Returns_Empty_String_When_Validation_Passed()
		{
			// arrange
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_GrossTotal = 65;

			var line1 = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line1.DLI_Quantity = 5;
			line1.DLI_PricePerUnit = 10;
			line1.DLI_LineTotal = 50;

			var line2 = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line2.DLI_Quantity = 3;
			line2.DLI_PricePerUnit = 5;
			line2.DLI_LineTotal = 15;

			var validateInvoiceTotalsRule = new ValidateInvoiceTotalsRule();

			// act
			var errorMessage = validateInvoiceTotalsRule.Validate(dashCommercialInvoice);

			// assert
			AssertEquals(string.Empty, errorMessage);
		}
	}
}
