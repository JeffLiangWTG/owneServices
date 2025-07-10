using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(ARAPPaymentDataAdapter))]
	class ARAPPaymentDataAdapterTest : BaseAccountingDataAdapterTest<Payment, TxnHeader>
	{
		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "FinancialInvoice"; }
		}

		protected override Payment NewBusinessObject()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			APPayment payment = Factory.New<APPayment>();
			payment.AH_OH = creator.ABIGAS.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectDebit;
			payment.AH_AB = creator.AUDBankAccount.PK;
			payment.AH_ChequeOrReference = "ABC123";
			payment.AH_OSExTaxAmount = 100m;
			return payment;
		}

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, Payment bizObjToImportTo, TxnHeader exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
		{
			Assert(true);   //Export Only Data Adapter
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert(true);   //Export Only Data Adapter
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_InvoiceDate = new ZDateTime(2010, 3, 31, 14, 15, 0);
			payment.AH_PostDate = payment.AH_InvoiceDate;
			return new BusinessObjectAndExpectedOutputFileName(payment, Retriever.SaveResourceToFile("EmptyPayment.xml"), ValidationKind.None, "Empty Payment");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_InvoiceDate = new ZDateTime(2010, 3, 31, 14, 15, 0);
			payment.AH_PostDate = payment.AH_InvoiceDate;
			return new BusinessObjectAndExpectedOutputFileName(payment, Retriever.SaveResourceToFile("EmptyPayment.xml"), ValidationKind.None, "Semi Populated Payment");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_InvoiceDate = new ZDateTime(2010, 3, 31, 14, 15, 0);
			payment.AH_PostDate = payment.AH_InvoiceDate;
			return new BusinessObjectAndExpectedOutputFileName(payment, Retriever.SaveResourceToFile("EmptyPayment.xml"), ValidationKind.None, "Fully Populated Payment");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override ValueObjectDataAdapter<Payment, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new ARAPPaymentDataAdapter();
		}

		#region Exporting

		public void TestExportForeignCurrencyPaymentMatchedToForeignAndLocalCurrencyInvoices()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(testDataFactory);
			Job job = creator.CreateJob("S00001001", creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccChargeCode chargeCode = creator.CC1;
			testDataFactory.Save();

			APInvoice invoice1 = testDataFactory.New<APInvoice>();
			invoice1.SubmittedFromInvoicingForm = true;
			invoice1.AH_OH = creator.ABIGAS.PK;
			invoice1.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			invoice1.AH_ExchangeRate = 0.9037m;
			invoice1.AH_TransactionNum = "INVOICE1NUM";
			APInvoiceLine line1 = (APInvoiceLine)invoice1.Lines.AddNew();
			line1.GenericCharge = chargeCode.PK;
			line1.AL_JH = job.PK;
			line1.AL_OSExTaxAmount = 25m;
			AssertEquals("Local Value of Invoice 1", 27.66m, invoice1.AH_LocalExTaxAmount);

			APInvoice invoice2 = testDataFactory.New<APInvoice>();
			invoice2.SubmittedFromInvoicingForm = true;
			invoice2.AH_OH = creator.ABIGAS.PK;
			invoice2.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			invoice2.AH_ExchangeRate = 0.9371m;
			invoice2.AH_TransactionNum = "INVOICE2NUM";
			APInvoiceLine line2 = (APInvoiceLine)invoice2.Lines.AddNew();
			line2.GenericCharge = chargeCode.PK;
			line2.AL_JH = job.PK;
			line2.AL_OSExTaxAmount = 45m;
			AssertEquals("Local Value of Invoice 2", 48.02m, invoice2.AH_LocalExTaxAmount);

			Factory.Save();

			APPayment payment = testDataFactory.New<APPayment>();
			payment.AH_OH = creator.ABIGAS.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectDebit;
			payment.AH_AB = creator.AUDBankAccount.PK;
			payment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			payment.ExchangeRate.Rate = 1.00m;
			payment.AH_ChequeOrReference = "ENETTREF";
			payment.AH_OSExTaxAmount = 75.68m;
			AssertEquals("Local Payment Amount", 75.68m, payment.AH_LocalExTaxAmount);

			MatchingBase paymentMatcher = payment.MatchingBaseObject;
			paymentMatcher.UnmatchedTransactions.Load();
			paymentMatcher.MoveFromUnmatchToMatch(new BusinessObject[] { invoice1, invoice2 });
			paymentMatcher.MatchedTransactions.SetPartialPaidAmount();

			paymentMatcher.MatchAndClearTransactions();

			testDataFactory.Save();

			APPayment paymentInNewFactory = Factory.Load<APPayment>(payment.PK);

			ARAPPaymentDataAdapter adapter = new ARAPPaymentDataAdapter();
			TxnHeader xmlPayment = adapter.ExportToValueObject(paymentInNewFactory, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("DebtorOrCreditor EDICode", payment.Header.OH_Code, xmlPayment.DebtorOrCreditor.EDICode);

			Assert("DebtorOrCreditor: EDI Code should not be empty", !xmlPayment.DebtorOrCreditor.EDICode.IsEmpty);
			Assert("DebtorOrCreditor: EDI Code should be specified", xmlPayment.DebtorOrCreditor.IsSpecified);

			Assert("DebtorOrCreditor: Name should not be empty", !xmlPayment.DebtorOrCreditor.OrganisationDetails.Name.IsEmpty);
			Assert("DebtorOrCreditor: Name should be specified", xmlPayment.DebtorOrCreditor.OrganisationDetails.IsSpecified);

			Assert("DebtorOrCreditor: Address 1 should not be empty", !xmlPayment.DebtorOrCreditor.OrganisationDetails.Addresses[0].AddressLine1.IsEmpty);
			Assert("DebtorOrCreditor: Address 1 should be specified", xmlPayment.DebtorOrCreditor.OrganisationDetails.Addresses[0].IsSpecified);

			AssertEquals("Description", payment.AH_Desc, xmlPayment.Description);
			AssertEquals("Payment Date", payment.AH_InvoiceDate.Date, xmlPayment.InvoiceDate.Date);
			AssertEquals("Due Date", payment.AH_DueDate.Date, xmlPayment.DueDate.Date);
			AssertEquals("LocalTotalAmount on XmlPayment", payment.AH_LocalTotalAmount, xmlPayment.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalExtTaxAmount on XmlPayment", payment.AH_LocalTotalAmount, xmlPayment.LocalInvoiceAmtExclTax.Value);
			AssertEquals("Local Currency on XmlPayment", payment.Branch.Company.GC_RX_NKLocalCurrency, xmlPayment.LocalInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("OSTotalAmount on XmlPayment", payment.AH_OSTotalAmount, xmlPayment.OsInvoiceAmtInclTax.Value);
			AssertEquals("OSExTaxAmount on XmlPayment", payment.AH_OSTotalAmount, xmlPayment.OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Currency on XmlPayment", payment.TransactionCurrency.RX_Code, xmlPayment.OsInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("Leger", payment.AH_Ledger, xmlPayment.Ledger.ToString());
			AssertEquals("Department Code", payment.Department.GE_Code, xmlPayment.Department);
			AssertEquals("Branch Code", payment.Branch.GB_Code, xmlPayment.Branch);
			AssertEquals("Post Date", payment.AH_PostDate.Date, xmlPayment.PostDate.Date);
			Assert("Is Specified", xmlPayment.IsSpecified);
			AssertEquals("Paid Transactions Count should be 2", 2, xmlPayment.PaidTransactions.Count);

			AssertEquals(payment.AH_LocalTotalAmount, xmlPayment.PaidTransactions[0].AmountPaidThisPayment.Value + xmlPayment.PaidTransactions[1].AmountPaidThisPayment.Value);
			AssertEquals("Payment.PaidTransaction LocalInvoiceAmtInclTax.CurrencyCode should be Local", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, xmlPayment.PaidTransactions[0].LocalInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("Payment.PaidTransaction LocalInvoiceAmtInclTax.CurrencyCode should be Local", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, xmlPayment.PaidTransactions[1].LocalInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("Payment.PaidTransaction LocalInvoiceAmtExclTax.CurrencyCode should be Local", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, xmlPayment.PaidTransactions[0].LocalInvoiceAmtExclTax.CurrencyCode);
			AssertEquals("Payment.PaidTransaction LocalInvoiceAmtExclTax.CurrencyCode should be Local", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, xmlPayment.PaidTransactions[1].LocalInvoiceAmtExclTax.CurrencyCode);

			TxnHeader xMLInvoice1 = GetInvoiceFromPaidList(xmlPayment, invoice1.AH_TransactionNum);
			TxnHeader xMLInvoice2 = GetInvoiceFromPaidList(xmlPayment, invoice2.AH_TransactionNum);

			AssertNotNull(xMLInvoice1);
			AssertNotNull(xMLInvoice2);

			AssertEquals(27.66m, xMLInvoice1.AmountPaidThisPayment.Value);
			AssertEquals(48.02m, xMLInvoice2.AmountPaidThisPayment.Value);
		}

		public void TestExportForeignCurrencyPaymentMatchedToForeignCurrencyInvoices()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(testDataFactory);
			Job job = creator.CreateJob("S00001001", creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccChargeCode chargeCode = creator.CC1;
			testDataFactory.Save();

			APInvoice invoice1 = testDataFactory.New<APInvoice>();
			invoice1.SubmittedFromInvoicingForm = true;
			invoice1.AH_OH = creator.ABIGAS.PK;
			invoice1.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			invoice1.AH_ExchangeRate = 0.9037m;
			invoice1.AH_TransactionNum = "INVOICE1NUM";
			APInvoiceLine line1 = (APInvoiceLine)invoice1.Lines.AddNew();
			line1.GenericCharge = chargeCode.PK;
			line1.AL_JH = job.PK;
			line1.AL_OSExTaxAmount = 25m;
			AssertEquals("Local Value of Invoice 1", 27.66m, invoice1.AH_LocalExTaxAmount);

			APInvoice invoice2 = testDataFactory.New<APInvoice>();
			invoice2.SubmittedFromInvoicingForm = true;
			invoice2.AH_OH = creator.ABIGAS.PK;
			invoice2.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			invoice2.AH_ExchangeRate = 0.9371m;
			invoice2.AH_TransactionNum = "INVOICE2NUM";
			APInvoiceLine line2 = (APInvoiceLine)invoice2.Lines.AddNew();
			line2.GenericCharge = chargeCode.PK;
			line2.AL_JH = job.PK;
			line2.AL_OSExTaxAmount = 45m;
			AssertEquals("Local Value of Invoice 2", 48.02m, invoice2.AH_LocalExTaxAmount);

			Factory.Save();

			APPayment payment = testDataFactory.New<APPayment>();
			payment.AH_OH = creator.ABIGAS.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectDebit;
			payment.AH_AB = creator.AUDBankAccount.PK;
			payment.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			payment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			payment.ExchangeRate.Rate = 0.9254m;
			payment.AH_ChequeOrReference = "ENETTREF";
			payment.AH_OSExTaxAmount = 100.00m;
			AssertEquals("Local Payment Amount", 108.06m, payment.AH_LocalExTaxAmount);

			MatchingBase paymentMatcher = payment.MatchingBaseObject;
			paymentMatcher.UnmatchedTransactions.Load();
			paymentMatcher.MoveFromUnmatchToMatch(new BusinessObject[] { invoice1, invoice2 });
			paymentMatcher.MatchedTransactions.SetPartialPaidAmount();

			APExchangeDifference eXX = (APExchangeDifference)paymentMatcher.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
			AssertEquals("EXX Amount", 32.38m, eXX.AH_LocalExTaxAmount);
			paymentMatcher.AddMiscellaneousTransaction(eXX);

			paymentMatcher.MatchAndClearTransactions();

			testDataFactory.Save();

			APPayment paymentInNewFactory = Factory.Load<APPayment>(payment.PK);

			ARAPPaymentDataAdapter adapter = new ARAPPaymentDataAdapter();
			TxnHeader xmlPayment = adapter.ExportToValueObject(paymentInNewFactory, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("DebtorOrCreditor EDICode", payment.Header.OH_Code, xmlPayment.DebtorOrCreditor.EDICode);

			Assert("DebtorOrCreditor: EDI Code should not be empty", !xmlPayment.DebtorOrCreditor.EDICode.IsEmpty);
			Assert("DebtorOrCreditor: EDI Code should be specified", xmlPayment.DebtorOrCreditor.IsSpecified);

			Assert("DebtorOrCreditor: Name should not be empty", !xmlPayment.DebtorOrCreditor.OrganisationDetails.Name.IsEmpty);
			Assert("DebtorOrCreditor: Name should be specified", xmlPayment.DebtorOrCreditor.OrganisationDetails.IsSpecified);

			Assert("DebtorOrCreditor: Address 1 should not be empty", !xmlPayment.DebtorOrCreditor.OrganisationDetails.Addresses[0].AddressLine1.IsEmpty);
			Assert("DebtorOrCreditor: Address 1 should be specified", xmlPayment.DebtorOrCreditor.OrganisationDetails.Addresses[0].IsSpecified);

			AssertEquals("Description", payment.AH_Desc, xmlPayment.Description);
			AssertEquals("Payment Date", payment.AH_InvoiceDate.Date, xmlPayment.InvoiceDate.Date);
			AssertEquals("Due Date", payment.AH_DueDate.Date, xmlPayment.DueDate.Date);
			AssertEquals("LocalTotalAmount on XmlPayment", payment.AH_LocalTotalAmount, xmlPayment.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalExtTaxAmount on XmlPayment", payment.AH_LocalTotalAmount, xmlPayment.LocalInvoiceAmtExclTax.Value);
			AssertEquals("Local Currency on XmlPayment", payment.Branch.Company.LocalCurrency.RX_Code, xmlPayment.LocalInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("OSTotalAmount on XmlPayment", payment.AH_OSTotalAmount, xmlPayment.OsInvoiceAmtInclTax.Value);
			AssertEquals("OSExTaxAmount on XmlPayment", payment.AH_OSTotalAmount, xmlPayment.OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Currency on XmlPayment", payment.TransactionCurrency.RX_Code, xmlPayment.OsInvoiceAmtInclTax.CurrencyCode);
			AssertEquals("Leger", payment.AH_Ledger, xmlPayment.Ledger.ToString());
			AssertEquals("Department Code", payment.Department.GE_Code, xmlPayment.Department);
			AssertEquals("Branch Code", payment.Branch.GB_Code, xmlPayment.Branch);
			AssertEquals("Post Date", payment.AH_PostDate.Date, xmlPayment.PostDate.Date);
			Assert("Is Specified", xmlPayment.IsSpecified);
			AssertEquals("Paid Transactions Count should be 2", 2, xmlPayment.PaidTransactions.Count);

			AssertEquals(-payment.AH_OSTotalAmount, xmlPayment.PaidTransactions[0].AmountPaidThisPayment.Value + xmlPayment.PaidTransactions[1].AmountPaidThisPayment.Value);
			TxnHeader xMLInvoice1 = GetInvoiceFromPaidList(xmlPayment, invoice1.AH_TransactionNum);
			TxnHeader xMLInvoice2 = GetInvoiceFromPaidList(xmlPayment, invoice2.AH_TransactionNum);

			AssertNotNull(xMLInvoice1);
			AssertNotNull(xMLInvoice2);

			AssertEquals(-36.55m, xMLInvoice1.AmountPaidThisPayment.Value);
			AssertEquals(-63.45m, xMLInvoice2.AmountPaidThisPayment.Value);
		}

		public void TestExportOverrideAddressAndOverrideContact()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = ObjectCreator.AALSHI.PK;
			var address = ObjectCreator.CreateAddress(payment.Header);
			var contact = ObjectCreator.CreateContact(payment.Header);
			Factory.Save();

			var adapter = new ARAPPaymentDataAdapter();
			var xmlPayment = adapter.ExportToValueObject(payment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(ZGuid.Empty, payment.AH_OA_InvoiceAddressOverride);
			AssertEquals(ZGuid.Empty, payment.AH_OC_InvoiceContactOverride);

			AssertEquals(false, xmlPayment.TxnOverrideAddress.IsSpecified);
			AssertEquals(false, xmlPayment.TxnOverrideContact.IsSpecified);

			payment.AH_OA_InvoiceAddressOverride = address.PK;
			payment.AH_OC_InvoiceContactOverride = contact.PK;
			Factory.Save();

			xmlPayment = adapter.ExportToValueObject(payment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(address.PK, payment.AH_OA_InvoiceAddressOverride);
			AssertEquals(contact.PK, payment.AH_OC_InvoiceContactOverride);

			AssertEquals(true, xmlPayment.TxnOverrideAddress.IsSpecified);
			AssertEquals(true, xmlPayment.TxnOverrideContact.IsSpecified);

			AssertEquals(address.OA_Code, xmlPayment.TxnOverrideAddress.AddressCode);
			AssertEquals(contact.Name, xmlPayment.TxnOverrideContact.Name);
		}

		TxnHeader GetInvoiceFromPaidList(TxnHeader xMLPayment, ZString invoiceNumber)
		{
			TxnHeader result = null;
			foreach (TxnHeader header in xMLPayment.PaidTransactions)
			{
				if (header.TxnNumber == invoiceNumber)
				{
					result = header;
					break;
				}
			}
			return result;
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				//Fully populated DirectDebitBatchHeader export tested explicitly in its own test
				return new string[] {
										"DebtorOrCreditor/OrganisationDetails",
										"DebtorOrCreditor/Notes/CustomNoteTypeName",
										"DebtorOrCreditor/Notes/NoteData",
										"DebtorOrCreditor/Notes/NoteCreatedDateTime",
										"DebtorOrCreditor/EDICode",
										"DebtorOrCreditor/OwnerCode",
										"TxnCount",
										"TxnCategory",
										"TxnNumber",
										"JobInvoiceNo",
										"TxnReference",
										"DisbursementFlag",
										"InvTerm",
										"InvTermDays",
										"DueDate",
										"GLPeriod",
										"LocalTaxAmount/CurrencyCode",
										"LocalWHTAmount/CurrencyCode",
										"OsTaxAmount/CurrencyCode",
										"OsWHTAmount/CurrencyCode",
										"CashBasisTaxIndicator",
										"GlAccount",
										"BankCode",
										"OrderReference",
										"OwnerReference",
										"ChequeOrReference",
										"ChequeBook",
										"ChequeDrawer",
										"DrawerBank",
										"DrawerBankBranch",
										"CreatedUserId",
										"DebtorOrCreditorGUID",
										"AmountPaidThisPayment/CurrencyCode",
										"TxnHeaderGUID",
										"PaymentReceiptBatchDate",
										"FullyPaidDate",
										"PaymentReference",
										"MatchStatus",
										"MatchStatusReasonCode",
										"ThirdPartyReference",
										"ENettStoragePaymentDetails/ContainerReference",
										"ENettStoragePaymentDetails/TerminalCode",
										"ENettStoragePaymentDetails/PickupDate",
										"Attachments/FileName",
										"TxnLines",
										"PaidTransactions",
										"Attachments/FilePath",
										"Attachments/DocumentType",
										//We only import these elements
										"OverrideSystemExchangeRate",
										"TxnLines/OverrideSystemExchangeRate",
										"PaidTransactions/TxnLines/OverrideSystemExchangeRate",
										"PaidTransactions/OverrideSystemExchangeRate",
										//Not relevent to this transaction type
										"TxnOverrideAddress",
										"TxnOverrideContact",
										//Not real node, just a flag
										"OsCurrencyEmptyFlag",
										"ShouldCreateDuringMatching"
									};
			}
		}

		#endregion
	}
}
