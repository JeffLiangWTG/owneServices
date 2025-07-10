using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	public class InvoiceDataXmlBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 5, 7, 15, 26, 32, 253)]
		public void TestInvoiceDataDocument()
		{
			using (var testStream = new MemoryStream())
			{
				var model = CreateModelForTest();
				var invoiceDataElement = GetInvoiceDataXmlBuilder(model).BuildInvoiceData() as IXmlElement;
				invoiceDataElement.WriteToXmlStream(testStream);

				var actualXml = Encoding.UTF8.GetString(testStream.ToArray());
				var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDataDocument_ExpectedXmlFileName);
				XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
			}
		}
		string InvoiceDataDocument_ExpectedXmlFileName => "InvoiceData_FullDocument.xml";

		#region <invoiceReference> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceReferenceXmlFragment_WhenNo()
		{
			var model = CreateModelForTest();

			var actualXmlElement = GetInvoiceDataXmlBuilder(model).BuildInvoiceReference();
			AssertNull(actualXmlElement);
		}

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceReferenceXmlFragment_WhenOriginalInvoiceSubmitted()
		{
			var previousInvoices = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "AR000999", WasSubmittedSuccessfully = true },
			};
			var model = CreateModelForTest(previousInvoices: previousInvoices);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceReference().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceReferenceXmlFragment_WhenOriginalInvoiceSubmitted_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceReferenceXmlFragment_WhenOriginalInvoiceSubmitted_ExpectedXmlFile => "InvoiceData_InvoiceReference_OriginalInvoiceSubmitted.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceReferenceXmlFragment_WhenOriginalInvoiceNotSubmitted()
		{
			var previousInvoices = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "AR000999", WasSubmittedSuccessfully = false },
			};
			var model = CreateModelForTest(previousInvoices: previousInvoices);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceReference().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceReferenceXmlFragment_WhenOriginalInvoiceNotSubmitted_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceReferenceXmlFragment_WhenOriginalInvoiceNotSubmitted_ExpectedXmlFile => "InvoiceData_InvoiceReference_OriginalInvoiceNotSubmitted.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceReferenceXmlFragment_WhenTwoInvoicesSubmitted()
		{
			var previousInvoices = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "AR000998", WasSubmittedSuccessfully = true },
				new PreviousInvoice() { AH_TransactionNum = "AR000999", WasSubmittedSuccessfully = true },
			};
			var model = CreateModelForTest(previousInvoices: previousInvoices);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceReference().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceReferenceXmlFragment_WhenTwoInvoicesSubmitted_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceReferenceXmlFragment_WhenTwoInvoicesSubmitted_ExpectedXmlFile => "InvoiceData_InvoiceReference_TwoInvoicesSubmitted.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceReferenceXmlFragment_CreditNote()
		{
			var previousInvoices = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "AR000998", WasSubmittedSuccessfully = true },
			};
			var model = CreateModelForTest(previousInvoices: previousInvoices, transactionType: TransactionType.CRD);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceReference().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceReferenceXmlFragment_CreditNote_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceReferenceXmlFragment_CreditNote_ExpectedXmlFile => "InvoiceData_InvoiceReference_CreditNote.xml";

		#endregion

		#region <supplierInfo> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_Defaults()
		{
			var model = CreateModelForTest();
			var dataElement = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo() as IXmlElement;
			var actualXml = dataElement.ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_Defaults_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_Defaults_ExpectedXmlFile => "InvoiceData_SupplierInfo_Defaults.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_WithGBR_WithoutVAT()
		{
			var model = CreateModelForTest(companyVatNumber: "", companyGbrNumber: "12345678");
			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_WithGBR_WithoutVAT_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_WithGBR_WithoutVAT_ExpectedXmlFile => "InvoiceData_SupplierInfo_WithGBR_WithoutVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_WithoutGBR_WithVAT()
		{
			var model = CreateModelForTest(companyVatNumber: "87654321", companyGbrNumber: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_WithoutGBR_WithVAT_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_WithoutGBR_WithVAT_ExpectedXmlFile => "InvoiceData_SupplierInfo_WithoutGBR_WithVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_WithoutGBR_WithoutVAT()
		{
			var model = CreateModelForTest(companyVatNumber: "", companyGbrNumber: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_WithoutGBR_WithoutVAT_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_WithoutGBR_WithoutVAT_ExpectedXmlFile => "InvoiceData_SupplierInfo_WithoutGBR_WithoutVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_LongerThan8Characters()
		{
			var model = CreateModelForTest(companyVatNumber: "0987654321", companyGbrNumber: "1234567890");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_LongerThan8Characters_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_LongerThan8Characters_ExpectedXmlFile => "InvoiceData_SupplierInfo_LongerThan8Characters.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_WithLeadingHU()
		{
			var model = CreateModelForTest(companyVatNumber: "HU87654321", companyGbrNumber: "HU12345678");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_WithLeadingHU_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_WithLeadingHU_ExpectedXmlFile => "InvoiceData_SupplierInfo_WithLeadingHU.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestSupplierInfoXmlFragment_WithoutSecondAddressLine()
		{
			var model = CreateModelForTest(companyAddressLine2: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildSupplierInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(SupplierInfoXmlFragment_WithoutSecondAddressLine_ExpectedXmlFile);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string SupplierInfoXmlFragment_WithoutSecondAddressLine_ExpectedXmlFile => "InvoiceData_SupplierInfo_WithoutSecondAddessLine.xml";

		#endregion

		#region <customerInfo> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_Defaults()
		{
			var model = CreateModelForTest();

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_Defaults_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_Defaults_ExpectedXmlFileName => "InvoiceData_CustomerInfo_Defaults.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_WithGBR_WithoutVAT()
		{
			var model = CreateModelForTest(debtorVatNumber: "", debtorGbrNumber: "12345678");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_WithGBR_WithoutVAT_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_WithGBR_WithoutVAT_ExpectedXmlFileName => "InvoiceData_CustomerInfo_WithGBR_WithoutVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_WithoutGBR_WithVAT()
		{
			var model = CreateModelForTest(debtorVatNumber: "87654321", debtorGbrNumber: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_WithoutGBR_WithVAT_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_WithoutGBR_WithVAT_ExpectedXmlFileName => "InvoiceData_CustomerInfo_WithoutGBR_WithVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_WithoutGBR_WithoutVAT()
		{
			var model = CreateModelForTest(debtorVatNumber: "", debtorGbrNumber: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_WithoutGBR_WithoutVAT_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_WithoutGBR_WithoutVAT_ExpectedXmlFileName => "InvoiceData_CustomerInfo_WithoutGBR_WithoutVAT.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_LongerThan8Characters()
		{
			var model = CreateModelForTest(debtorVatNumber: "0987654321", debtorGbrNumber: "1234567890");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_LongerThan8Characters_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_LongerThan8Characters_ExpectedXmlFileName => "InvoiceData_CustomerInfo_LongerThan8Characters.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_WithLeadingHU()
		{
			var model = CreateModelForTest(debtorVatNumber: "HU87654321", debtorGbrNumber: "HU12345678");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_WithLeadingHU_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_WithLeadingHU_ExpectedXmlFileName => "InvoiceData_CustomerInfo_WithLeadingHU.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestCustomerInfoXmlFragment_WithoutSecondAddressLine()
		{
			var model = CreateModelForTest(debtorAddressLine2: "");

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildCustomerInfo().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(CustomerInfoXmlFragment_WithoutSecondAddressLine_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}
		string CustomerInfoXmlFragment_WithoutSecondAddressLine_ExpectedXmlFileName => "InvoiceData_CustomerInfo_WithoutSecondAddessLine.xml";

		#endregion

		#region <invoiceDetail> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_Defaults()
		{
			var model = CreateModelForTest();

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_Defaults_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceDetailXmlFragment_Defaults_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_Defaults.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_DeliveryDate()
		{
			var model = CreateModelForTest();
			AddJournal(new ZDate(2020, 1, 1));
			AddJournal(new ZDate(2020, 1, 15));
			AssertEquals(new ZDate(2020, 1, 15), model.DeliveryDate);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_DeliveryDate_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);

			void AddJournal(ZDate date)
			{
				var journal = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
				journal.TaxDate = date;
				model.Transaction.PostingJournalCollection.Add(journal);
			}
		}

		string InvoiceDetailXmlFragment_DeliveryDate_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_DeliveryDate.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_PeriodicInvoice()
		{
			var model = CreateModelForTest(invoiceCategory: InvoiceTypesList.Codes.FinalInvoice_Batching);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_PeriodicInvoice_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceDetailXmlFragment_PeriodicInvoice_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_PeriodicInvoice.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_ForeignCurrencyOverOne()
		{
			var model = CreateModelForTest(currencyCode: "EUR", exchangeRate: 12.345078m);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_ForeignCurrencyOverOne_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceDetailXmlFragment_ForeignCurrencyOverOne_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_ForeignCurrencyOverOne.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_ForeignCurrencyUnderOne()
		{
			var model = CreateModelForTest(currencyCode: "EUR", exchangeRate: 0.0101m);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_ForeignCurrencyUnderOne_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceDetailXmlFragment_ForeignCurrencyUnderOne_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_ForeignCurrencyUnderOne.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceDetailXmlFragment_PaymentDate()
		{
			var model = CreateModelForTest(dueDate: new ZDate(2020, 12, 12));

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceDetail().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceDetailXmlFragment_PaymentDate_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceDetailXmlFragment_PaymentDate_ExpectedXmlFileName => "InvoiceData_InvoiceDetail_PaymentDate.xml";

		#endregion

		#region <invoiceLines> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_Defaults()
		{
			var model = CreateModelForTest();

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_Defaults_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_Defaults_ExpectedXmlFileName => "InvoiceData_InvoiceLines_Defaults.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_Empty()
		{
			var model = CreateModelForTest(lineItems: new List<PostingJournal>());

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_Empty_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_Empty_ExpectedXmlFileName => "InvoiceData_InvoiceLines_Empty.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_ForeignCurrency()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 0.82m,
					OSGSTVATAmount = 0.22m,
					OSTotalAmount = 1.04m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = 200m,
					OSGSTVATAmount = 54m,
					OSTotalAmount = 254m,
					LocalAmount = 74821.90m,
					LocalGSTVATAmount = 20201.91m,
					LocalTotalAmount = 95023.81m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var model = CreateModelForTest(lineItems: lineItems);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_ForeignCurrency_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_ForeignCurrency_ExpectedXmlFileName => "InvoiceData_InvoiceLines_ForeignCurrency.xml";

		[TestDate(2023, 11, 7, 15, 35, 21)]
		public void TestInvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsReciprocal()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					ChargeCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "EUR" },
					OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "USD" },
					OSAmount = 0.82m,
					OSGSTVATAmount = 0.22m,
					OSTotalAmount = 1.04m,
					LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					ChargeCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "USD" },
					OSAmount = 200m,
					OSGSTVATAmount = 54m,
					OSTotalAmount = 254m,
					LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					LocalAmount = 74821.90m,
					LocalGSTVATAmount = 20201.91m,
					LocalTotalAmount = 95023.81m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var model = CreateModelForTest(lineItems: lineItems, isReciprocal: true, invoiceCategory: InvoiceTypesList.Codes.FinalInvoice_Batching);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsReciprocal_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsReciprocal_ExpectedXmlFileName => "InvoiceData_InvoiceLines_ForeignCurrency_PeriodicInvoice_IsReciprocal.xml";

		[TestDate(2023, 11, 7, 15, 35, 21)]
		public void TestInvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsNotReciprocal()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					ChargeCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "EUR" },
					OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "USD" },
					OSAmount = 0.82m,
					OSGSTVATAmount = 0.22m,
					OSTotalAmount = 1.04m,
					LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					ChargeCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					OSCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "USD" },
					OSAmount = 200m,
					OSGSTVATAmount = 54m,
					OSTotalAmount = 254m,
					LocalCurrency = new UniversalDataBuss.DataObjects.Universal.Currency() { Code = "HUF" },
					LocalAmount = 74821.90m,
					LocalGSTVATAmount = 20201.91m,
					LocalTotalAmount = 95023.81m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var model = CreateModelForTest(lineItems: lineItems, isReciprocal: false, invoiceCategory: InvoiceTypesList.Codes.FinalInvoice_Batching);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsNotReciprocal_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_ForeignCurrency_PeriodicInvoice_IsNotReciprocal_ExpectedXmlFileName => "InvoiceData_InvoiceLines_ForeignCurrency_PeriodicInvoice_IsNotReciprocal.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_CreditNote()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = -100m,
					OSGSTVATAmount = -27m,
					OSTotalAmount = -127m,
					LocalAmount = -100m,
					LocalGSTVATAmount = -27m,
					LocalTotalAmount = -127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = -200m,
					OSGSTVATAmount = -54m,
					OSTotalAmount = -254m,
					LocalAmount = -200m,
					LocalGSTVATAmount = -54m,
					LocalTotalAmount = -254m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var previousInvoice = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "100", AH_TransactionType = "INV", WasSubmittedSuccessfully = true, MaximumLineSequence = 3 }
			};
			var model = CreateModelForTest(transactionType: TransactionType.CRD, lineItems: lineItems, previousInvoices: previousInvoice);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_CreditNote_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_CreditNote_ExpectedXmlFileName => "InvoiceData_InvoiceLines_CreditNote.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_AmendInvoice()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 4,
					Description = "Description of line 1",
					OSAmount = 100m,
					OSGSTVATAmount = 27m,
					OSTotalAmount = 127m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var previousInvoice = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "100", AH_TransactionType = "INV", WasSubmittedSuccessfully = true, MaximumLineSequence = 12 }
			};
			var model = CreateModelForTest(lineItems: lineItems, previousInvoices: previousInvoice);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_AmendInvoice_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_AmendInvoice_ExpectedXmlFileName => "InvoiceData_InvoiceLines_AmendInvoice.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceLinesXmlFragment_PeriodicInvoice()
		{
			var taxId = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } };
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 100m,
					OSGSTVATAmount = 27m,
					OSTotalAmount = 127m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = taxId,
					ChargeExchangeRate = 1.15m,
					TaxDate = ZDate.Today,
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "Description of line 2",
					OSAmount = 150m,
					OSGSTVATAmount = 40.5m,
					OSTotalAmount = 190.5m,
					LocalAmount = 150m,
					LocalGSTVATAmount = 40.5m,
					LocalTotalAmount = 190.5m,
					VATTaxID = taxId,
					ChargeExchangeRate = 1.15m,
					TaxDate = ZDate.Today.AddMonths(1),
				},
			};
			var model = CreateModelForTest(lineItems: lineItems, invoiceCategory: InvoiceTypesList.Codes.FinalInvoice_Batching);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceLines().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceLinesXmlFragment_PeriodicInvoice_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceLinesXmlFragment_PeriodicInvoice_ExpectedXmlFileName => "InvoiceData_InvoiceLines_PeriodicInvoice.xml";

		#endregion

		#region <invoiceSummary> Tests

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceSummaryXmlFragment_Defaults()
		{
			var model = CreateModelForTest();

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceSummary().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceSummaryXmlFragment_Defaults_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceSummaryXmlFragment_Defaults_ExpectedXmlFileName => "InvoiceData_InvoiceSummary_Defaults.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceSummaryXmlFragment_Empty()
		{
			var model = CreateModelForTest(lineItems: new List<PostingJournal>());

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceSummary().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceSummaryXmlFragment_Empty_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceSummaryXmlFragment_Empty_ExpectedXmlFileName => "InvoiceData_InvoiceSummary_Empty.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceSummaryXmlFragment_ForeignCurrency()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 0.82m,
					OSGSTVATAmount = 0.22m,
					OSTotalAmount = 1.04m,
					LocalAmount = 100m,
					LocalGSTVATAmount = 27m,
					LocalTotalAmount = 127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = 200m,
					OSGSTVATAmount = 54m,
					OSTotalAmount = 254m,
					LocalAmount = 74821.90m,
					LocalGSTVATAmount = 20201.91m,
					LocalTotalAmount = 95023.81m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var model = CreateModelForTest(lineItems: lineItems);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceSummary().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceSummaryXmlFragment_ForeignCurrency_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceSummaryXmlFragment_ForeignCurrency_ExpectedXmlFileName => "InvoiceData_InvoiceSummary_ForeignCurrency.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceSummaryXmlFragment_CreditNote()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = -100m,
					OSGSTVATAmount = -27m,
					OSTotalAmount = -127m,
					LocalAmount = -100m,
					LocalGSTVATAmount = -27m,
					LocalTotalAmount = -127m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "The second description",
					OSAmount = -200m,
					OSGSTVATAmount = -54m,
					OSTotalAmount = -254m,
					LocalAmount = -200m,
					LocalGSTVATAmount = -54m,
					LocalTotalAmount = -254m,
					VATTaxID = new TaxID() { TaxCode = "VAT", TaxRate = 27, TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
			};
			var previousInvoice = new[]
			{
				new PreviousInvoice() { AH_TransactionNum = "100", AH_TransactionType = "INV", WasSubmittedSuccessfully = true, MaximumLineSequence = 3 }
			};
			var model = CreateModelForTest(transactionType: TransactionType.CRD, lineItems: lineItems, previousInvoices: previousInvoice);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceSummary().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceSummaryXmlFragment_CreditNote_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceSummaryXmlFragment_CreditNote_ExpectedXmlFileName => "InvoiceData_InvoiceSummary_CreditNote.xml";

		[TestDate(2020, 5, 7, 15, 26, 32)]
		public void TestInvoiceSummaryXmlFragment_SummaryGrossData()
		{
			var lineItems = new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 1,
					Description = "Description of line 1",
					OSAmount = 100m,
					OSTotalAmount = 100m,
					LocalAmount = 100m,
					LocalTotalAmount = 100
				},
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Sequence = 2,
					Description = "Description of line 2",
					OSAmount = 200m,
					OSTotalAmount = 200m,
					LocalAmount = 200m,
					LocalTotalAmount = 200,
				},
			};
			var model = CreateModelForTest(lineItems: lineItems);

			var actualXml = GetInvoiceDataXmlBuilder(model).BuildInvoiceSummary().ToString();
			var expectedXml = TestHelper.GetEmbeddedResourceTestCaseAsUtf8String(InvoiceSummaryXmlFragment_SummaryGrossData_ExpectedXmlFileName);
			XmlComparison.CompareAndAssertXml(expectedXml, actualXml);
		}

		string InvoiceSummaryXmlFragment_SummaryGrossData_ExpectedXmlFileName => "InvoiceData_InvoiceSummary_SummaryGrossData.xml";

		#endregion

		public InvoiceDataXmlBuilder GetInvoiceDataXmlBuilder(ManageInvoiceRequestModel model)
		{
			return new InvoiceDataXmlBuilder(model.GetInvoiceDataModel());
		}

		public ManageInvoiceRequestModel CreateModelForTest(
			string invoiceNumber = "AR001000",
			TransactionType transactionType = TransactionType.INV,
			string invoiceCategory = "FIN",
			string currencyCode = "HUF",
			decimal exchangeRate = 1.0m,
			bool isReciprocal = false,
			string companyVatNumber = "12036024",
			string companyGbrNumber = "81237290",
			string companyAddressLine2 = "Second address line",
			string debtorVatNumber = "43729838",
			string debtorGbrNumber = "76803943",
			string debtorAddressLine2 = "Line number two",
			ZDate? dueDate = null,
			List<PostingJournal> lineItems = null,
			IReadOnlyCollection<PreviousInvoice> previousInvoices = null)
		{
			return CreateModelForBaseTest(invoiceNumber
				, transactionType
				, invoiceCategory
				, currencyCode
				, exchangeRate
				, isReciprocal
				, companyVatNumber
				, companyGbrNumber
				, companyAddressLine2
				, debtorVatNumber
				, debtorGbrNumber
				, debtorAddressLine2
				, dueDate
				, lineItems
				, previousInvoices);
		}

		ManageInvoiceRequestModel CreateModelForBaseTest(
			string invoiceNumber = "AR001000",
			TransactionType transactionType = TransactionType.INV,
			string invoiceCategory = "FIN",
			string currencyCode = "HUF",
			decimal exchangeRate = 1.0m,
			bool isReciprocal = false,
			string companyVatNumber = "12036024",
			string companyGbrNumber = "81237290",
			string companyAddressLine2 = "Second address line",
			string debtorVatNumber = "43729838",
			string debtorGbrNumber = "76803943",
			string debtorAddressLine2 = "Line number two",
			ZDate? dueDate = null,
			List<PostingJournal> lineItems = null,
			IReadOnlyCollection<PreviousInvoice> previousInvoices = null)
		{
			var info = TestHelper.GetTransactionInfo(invoiceNumber
				, transactionType
				, invoiceCategory
				, currencyCode
				, exchangeRate
				, companyVatNumber
				, companyGbrNumber
				, companyAddressLine2
				, debtorVatNumber
				, debtorGbrNumber
				, debtorAddressLine2
				, dueDate
				, lineItems);

			var model = new ManageInvoiceRequestModel(info.transactionInfo, info.extraInfo, "0001", new NotificationBuffer());

			model.SetData_ForTestOnly(companyCode: "DHU",
				companyIsReciprocal: isReciprocal,
				branchCode: "BHU",
				loginId: "h9nupbpmgi8yhet",
				passwordHash: "3581511529344BF49B6B93A44BDEDBE8AED53942D04F45D6BA0702E2E5F7A928667FB0C11CDEBA39A45D010FBD89C1F951CE9FF1B6789412780C70613CDAB56B",
				signatureKey: "fb-8530-0ead7916e5432DA4ZSW1UKWX",
				replacementKey: "8a042DA4ZSW17HY6",
				softwareVersion: "20.4.30.9",
				previousInvoices: previousInvoices);

			return model;
		}
	}
}
