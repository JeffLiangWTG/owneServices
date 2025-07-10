using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business.Validation;

namespace Enterprise.Dash.Business.Tests.Validation
{
	public class ValidateMandatoryDataRuleTests : TestCaseWithFactory
	{
		public void TestValidate_Returns_ErrorMessage_When_InvoiceNumber_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = string.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_InvoiceNumber)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_InvoiceDate_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_InvoiceDate)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_GrossTotal_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = ZDecimal.Zero;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_GrossTotal)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_MatchedImporter_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_OH_MatchedImporterID)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_MatchedSupplier_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_OH_MatchedSupplierID)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Currency_Is_Empty()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = ZString.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_RX_NKInvoiceCurrency)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_No_Lines_Exist()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK} does not have commercial invoice lines", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Line_Without_MatchedProduct_Exists()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.Empty;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(line.DLI_OP_MatchedProductCodeID)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Line_Without_PricePerUnit_Exists()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = ZDecimal.Zero;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(line.DLI_PricePerUnit)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Line_Without_Quantity_Exists()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = 20m;
			line.DLI_Quantity = ZDecimal.Zero;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(line.DLI_Quantity)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_ErrorMessage_When_Line_Without_LineTotal_Exists()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = 20m;
			line.DLI_Quantity = 10;
			line.DLI_LineTotal = ZDecimal.Zero;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals($"{nameof(DashCommercialInvoiceLineItem)} with PK {line.PK}: {nameof(line.DLI_LineTotal)} must not be empty", errorMessage);
		}

		public void TestValidate_Returns_Empty_String_When_Validated_Passed()
		{
			var dashCommercialInvoice = Factory.NewWithValidTestData<DashCommercialInvoice>();
			dashCommercialInvoice.DCI_InvoiceNumber = "12345";
			dashCommercialInvoice.DCI_InvoiceDate = ZDate.Today;
			dashCommercialInvoice.DCI_GrossTotal = 90m;
			dashCommercialInvoice.DCI_OH_MatchedImporterID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_OH_MatchedSupplierID = ZGuid.NewZGuid();
			dashCommercialInvoice.DCI_RX_NKInvoiceCurrency = "AUD";

			var line = dashCommercialInvoice.CommercialInvoiceLineItems.AddNew();
			line.DLI_OP_MatchedProductCodeID = ZGuid.NewZGuid();
			line.DLI_PricePerUnit = 20m;
			line.DLI_Quantity = 10;
			line.DLI_LineTotal = 30;

			var validateMandatoryDataRule = new ValidateMandatoryDataRule();
			var errorMessage = validateMandatoryDataRule.Validate(dashCommercialInvoice);

			AssertEquals(string.Empty, errorMessage);
		}
	}
}
