using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_InvoiceQuantity()
		{
			const string ExpectedWhenEmpty = "You have not entered";
			var targetInfo = InvoiceLine.JI_InvoiceQuantityInfo;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity(); 
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", targetInfo, ExpectedWhenEmpty);
			InvoiceLine.JI_InvoiceQuantity = 10;
			AssertNoMessageErrorContaining("When the code is not empty, the empty message should not be shown.", targetInfo, ExpectedWhenEmpty);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			const string ExpectedWhenEmpty = "You have not entered";
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PKG", "Package Quantity Unit");
			helper.CreateNewOrGetExistingCusCodeList("IL", "PKG", "KG", "Kilogram", new ZDateTime(2025, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var targetInfo = InvoiceLine.JI_InvoiceUQInfo;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", targetInfo, ExpectedWhenEmpty);
			InvoiceLine.JI_InvoiceUQ = "IUQ";
			AssertNoMessageErrorContaining("When the code is not empty, the empty message should not be shown.", targetInfo, ExpectedWhenEmpty);
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_InvoiceUQ = "KG";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			const string ExpectedWhenEmpty = "You have not entered";
			var targetInfo = InvoiceLine.JI_CustomsQuantityInfo;
			invoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", targetInfo, ExpectedWhenEmpty);
			InvoiceLine.JI_CustomsQuantity = 10;
			AssertNoMessageErrorContaining("When the code is not empty, the empty message should not be shown.", targetInfo, ExpectedWhenEmpty);
		}

		public void TestCheckJI_CustomsUnitQty()
		{
			const string ExpectedWhenEmpty = "You have not entered";
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Quantity Unit");
			helper.CreateNewOrGetExistingCusCodeList("IL", "CUSUQ", "001", "Unit 1", new ZDateTime(2025, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var targetInfo = InvoiceLine.JI_CustomsUnitQtyInfo;
			invoiceLine.Validation.ValidateJI_CustomsUnitQty(); 
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", targetInfo, ExpectedWhenEmpty);
			InvoiceLine.JI_CustomsUnitQty = "UQ";
			AssertNoMessageErrorContaining("When the code is not empty, the empty message should not be shown.", targetInfo, ExpectedWhenEmpty);
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CustomsUnitQty = "001";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_LinePrice()
		{
			const string ExpectedWhenEmpty = "You have not entered";
			var targetInfo = InvoiceLine.JI_LinePriceInfo;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining("When the code is empty, the empty message should be shown.", targetInfo, ExpectedWhenEmpty);
			InvoiceLine.JI_LinePrice = 10;
			AssertNoMessageErrorContaining("When the code is not empty, the empty message should not be shown.", targetInfo, ExpectedWhenEmpty);
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var targetInfo = InvoiceLine.JI_CountryOfOriginInfo;
			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CountryOfOrigin = "XX";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Israel;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_WeightUQ()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var targetInfo = InvoiceLine.JI_WeightUQInfo;
			InvoiceLine.JI_WeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_WeightUQ = "XX";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Quantity Unit");
			helper.CreateNewOrGetExistingCusCodeList("IL", "CUSUQ", "001", "Unit 1", new ZDateTime(2025, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var targetInfo = InvoiceLine.JI_CustomsSecondUnitQtyInfo;
			InvoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CustomsSecondUnitQty = "UQ";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CustomsSecondUnitQty = "001";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Quantity Unit");
			helper.CreateNewOrGetExistingCusCodeList("IL", "CUSUQ", "001", "Unit 1", new ZDateTime(2025, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var targetInfo = InvoiceLine.JI_CustomsThirdUnitQtyInfo;
			InvoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CustomsThirdUnitQty = "UQ";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_CustomsThirdUnitQty = "001";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PKG", "Package Quantity Unit");
			helper.CreateNewOrGetExistingCusCodeList("IL", "PKG", "KG", "Kilogram", new ZDateTime(2025, 1, 1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var targetInfo = InvoiceLine.JI_BondedWhsUnitQtyInfo;
			InvoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_BondedWhsUnitQty = "IUQ";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_BondedWhsUnitQty = "KG";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			const string expectedWhenInvalid = "The code you have selected is not in the list.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.Israel, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Normal"); Factory.Save();
			var targetInfo = InvoiceLine.JI_ZZF_NKTaxTypeInfo;
			InvoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_ZZF_NKTaxType = "XXX";
			AssertHasMessageErrorContaining("When the code is invalid, the Invalid message should be shown.", targetInfo, expectedWhenInvalid);
			InvoiceLine.JI_ZZF_NKTaxType = "VAT";
			AssertNoMessageErrorContaining("When the code is valid, the Invalid message should not be shown.", targetInfo, expectedWhenInvalid);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Declaration.Invoices.AddNew();
				}

				return invoice;
			}
		}

		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Invoice.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
