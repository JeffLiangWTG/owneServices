using System.IO;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(UnallocatedTransactionDataAdapter))]
	sealed class UnallocatedTransactionDataAdapterTest : BaseAccountingDataAdapterTest<TransactionPendingAllocation, TxnHeader>
	{
		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "UnallocatedTransaction"; }
		}

		string EmptySampleLocation
		{
			get
			{
				if (string.IsNullOrEmpty(emptySampleLocation))
				{
					emptySampleLocation = Env.GetTempFileName(Env.TempPath, "xml");
					using var stream = File.Open(emptySampleLocation, FileMode.Create);
					using var writer = new StreamWriter(stream);
					writer.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>
<UnallocatedTransaction xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <Ledger>PA</Ledger>
  <DebtorOrCreditor EDICode=""XVBQP68SIYXQ"" OwnerCode=""XVBQP68SIYXQ"">
    <OrganisationDetails>
      <Addresses>
        <Address AddressType=""MAIN"">
          <AddressLine1>#1</AddressLine1>
          <AddressCode>#1</AddressCode>
          <Language>EN</Language>
          <Sequence>1</Sequence>
          <AddressCapabilities>
            <AddressCapability AddressType=""MAIN"" />
            <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
          </AddressCapabilities>
        </Address>
      </Addresses>
    </OrganisationDetails>
  </DebtorOrCreditor>
  <TxnType>IPA</TxnType>
  <TxnCount>1</TxnCount>
  <TxnCategory>STD</TxnCategory>
  <TxnNumber>ABC</TxnNumber>
  <Description>Description</Description>
  <InvoiceDate>2004-12-31T10:30:00+02:00</InvoiceDate>
  <InvTerm>COD</InvTerm>
  <InvTermDays>0</InvTermDays>
  <DueDate>2005-01-02T10:30:00+02:00</DueDate>
  <PostDate>2005-01-01T10:30:00+02:00</PostDate>
  <GLPeriod>200507</GLPeriod>
  <Branch>BNE</Branch>
  <Department>BRN</Department>
  <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">100</LocalInvoiceAmtExclTax>
  <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">110</LocalInvoiceAmtInclTax>
  <LocalTaxAmount CurrencyCode=""AUD"">10</LocalTaxAmount>
  <LocalWHTAmount CurrencyCode=""AUD"">0</LocalWHTAmount>
  <OsInvoiceAmtExclTax CurrencyCode=""AUD"">100</OsInvoiceAmtExclTax>
  <OsInvoiceAmtInclTax CurrencyCode=""AUD"">110</OsInvoiceAmtInclTax>
  <OsTaxAmount CurrencyCode=""AUD"">10</OsTaxAmount>
  <OsWHTAmount CurrencyCode=""AUD"">0</OsWHTAmount>
  <TxnHeaderGUID>headerGUID</TxnHeaderGUID>
</UnallocatedTransaction>");
				}
				return emptySampleLocation;
			}
		}
		string emptySampleLocation;

		string FullyPopulatedSampleLocation
		{
			get
			{
				if (string.IsNullOrEmpty(fullyPopulatedSampleLocation))
				{
					fullyPopulatedSampleLocation = Env.GetTempFileName(Env.TempPath, "xml");
					using var stream = File.Open(fullyPopulatedSampleLocation, FileMode.Create);
					using var writer = new StreamWriter(stream);
					writer.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>
<UnallocatedTransaction xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <Ledger>PA</Ledger>
  <DebtorOrCreditor EDICode=""XVBQP68SIYXQ"" OwnerCode=""XVBQP68SIYXQ"">
    <OrganisationDetails>
      <Addresses>
        <Address AddressType=""MAIN"">
          <AddressLine1>#1</AddressLine1>
          <AddressCode>#1</AddressCode>
          <Language>EN</Language>
          <Sequence>1</Sequence>
          <AddressCapabilities>
            <AddressCapability AddressType=""MAIN"" />
            <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
          </AddressCapabilities>
        </Address>
      </Addresses>
    </OrganisationDetails>
  </DebtorOrCreditor>
  <TxnType>IPA</TxnType>
  <TxnCount>1</TxnCount>
  <TxnCategory>STD</TxnCategory>
  <TxnNumber>ABC</TxnNumber>
  <Description>Description</Description>
  <InvoiceDate>2004-12-31T10:30:00+02:00</InvoiceDate>
  <InvTerm>COD</InvTerm>
  <InvTermDays>0</InvTermDays>
  <DueDate>2005-01-02T10:30:00+02:00</DueDate>
  <PostDate>2005-01-01T10:30:00+02:00</PostDate>
  <GLPeriod>200507</GLPeriod>
  <Branch>BNE</Branch>
  <Department>BRN</Department>
  <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">100</LocalInvoiceAmtExclTax>
  <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">110</LocalInvoiceAmtInclTax>
  <LocalTaxAmount CurrencyCode=""AUD"">10</LocalTaxAmount>
  <LocalWHTAmount CurrencyCode=""AUD"">0</LocalWHTAmount>
  <OsInvoiceAmtExclTax CurrencyCode=""AUD"">100</OsInvoiceAmtExclTax>
  <OsInvoiceAmtInclTax CurrencyCode=""AUD"">110</OsInvoiceAmtInclTax>
  <OsTaxAmount CurrencyCode=""AUD"">10</OsTaxAmount>
  <OsWHTAmount CurrencyCode=""AUD"">0</OsWHTAmount>
  <TxnHeaderGUID>headerGUID</TxnHeaderGUID>
</UnallocatedTransaction>");
				}
				return fullyPopulatedSampleLocation;
			}
		}
		string fullyPopulatedSampleLocation;

		protected override void TearDown()
		{
			base.TearDown();
			if (File.Exists(emptySampleLocation))
			{
				File.Delete(emptySampleLocation);
			}
			if (File.Exists(fullyPopulatedSampleLocation))
			{
				File.Delete(fullyPopulatedSampleLocation);
			}
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
			transaction.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_PostDate = PostDate;
			transaction.AH_InvoiceDate = PostDate.AddDays(-1);
			transaction.AH_DueDate = PostDate.AddDays(1);
			transaction.AH_OSExTaxAmount = 100m;
			transaction.AH_OSTaxAmount = 10m;
			transaction.AH_Desc = "Description";
			transaction.AH_TransactionNum = "ABC";
			return new BusinessObjectAndExpectedOutputFileName(transaction, EmptySampleLocation, ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
			transaction.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_PostDate = PostDate;
			transaction.AH_InvoiceDate = PostDate.AddDays(-1);
			transaction.AH_DueDate = PostDate.AddDays(1);
			transaction.AH_OSExTaxAmount = 100m;
			transaction.AH_OSTaxAmount = 10m;
			transaction.AH_TransactionNum = "ABC";
			transaction.AH_Desc = "Description";
			return new BusinessObjectAndExpectedOutputFileName(transaction, FullyPopulatedSampleLocation, ValidationKind.None, "Fully Populated Invoice");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
			{
					"DebtorOrCreditor",
					"JobInvoiceNo",
					"TxnReference",
					"DisbursementFlag",
					"CashBasisTaxIndicator",
					"GlAccount",
					"BankCode",
					"ChequeBook",
					"ReceiptPaymentType",
					"OrderReference",
					"OwnerReference",
					"ChequeOrReference",
					"ChequeDrawer",
					"DrawerBank",
					"DrawerBankBranch",
					"CreatedUserId",
					"DebtorOrCreditorGUID",
					"AmountPaidThisPayment/CurrencyCode",
					"TxnLines",
					"PaidTransactions",
					"PaymentReceiptBatchDate",
					"FullyPaidDate",
					"PaymentReference",
					"MatchStatus",
					"MatchStatusReasonCode",
					"ThirdPartyReference",
					"Attachments/FileName",
					"ENettStoragePaymentDetails/ContainerReference",
					"ENettStoragePaymentDetails/TerminalCode",
					"ENettStoragePaymentDetails/PickupDate",
					"Attachments/FilePath",
					"Attachments/DocumentType",
					//We only import these elements
					"OverrideSystemExchangeRate",
					"TxnLines/OverrideSystemExchangeRate",

					//Not relevent to this transaction type
					"TxnOverrideAddress",
					"TxnOverrideContact",

					//Not real node, just a flag
					"OsCurrencyEmptyFlag",
					"ShouldCreateDuringMatching"
				};
			}
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override ValueObjectDataAdapter<TransactionPendingAllocation, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new UnallocatedTransactionDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
			transaction.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transaction.AH_PostDate = PostDate;
			transaction.AH_InvoiceDate = PostDate.AddDays(-1);
			transaction.AH_DueDate = PostDate.AddDays(1);
			transaction.AH_OSExTaxAmount = 100m;
			transaction.AH_OSTaxAmount = 10m;
			transaction.AH_TransactionNum = "ABC";
			transaction.AH_Desc = "Description";
			return new BusinessObjectAndExpectedOutputFileName(transaction, FullyPopulatedSampleLocation, ValidationKind.None, "Fully Populated Invoice");
		}
	}
}
