using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.UniversalDataBuss.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AccountingPaymentBatchDataContextManager))]
	sealed class AccountingPaymentBatchDataContextManagerTest : AccountingReceiptPaymentBatchDataContextManagerTest<AccountingPaymentBatchDataContextManager, Payment>
	{
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData()
		{
			var tupleResult = CreateTransactionBatch(XUBMessage_Successful);
			CreateInvoiceForMatching();

			var logger = new TestErrorLogger();
			var result = GetDataContextManager().UseIncomingTransactionBatchData(tupleResult.ediMessage, tupleResult.transactionBatch, logger, Factory);

			AssertEquals(@"Information - Matching 'OFC':- Matched to 'ABIGAS' by code, address 'PST: 171 ABBOTSFORD ROAD' by short code.
Information - Matching 'OFC':- Matched to 'ABIGAS' by code, address 'PST: 171 ABBOTSFORD ROAD' by short code.
Information - Matching 'OFC':- Matched to 'ABIGAS' by code, address 'PST: 171 ABBOTSFORD ROAD' by short code.
Information - Begin processing Transaction AR PAY ABIGAS ZHSBCAUD CASH: 
Information -   Completed Processing Transaction.
Information - Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
Information -   Completed Processing Transaction.", logger.Logs);
			Assert("The result should be true after calling UseIncomingTransactionBatchData with valid data.", result);
		}

		protected override Payment GetNewBusinessObjectForTesting()
		{
			var result = Factory.NewWithValidTestData<ARPayment>();
			Factory.SaveForTesting();
			return result;
		}

		protected override AccountingPaymentBatchDataContextManager GetDataContextManager()
		{
			return new AccountingPaymentBatchDataContextManager();
		}

		#region Test XUB Strings

		readonly string XUBMessage_Successful = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingPayment</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>PAY</TransactionType>
				<MatchLineCollection>
					<MatchLine>
						<LinkedTransactionIDCollection>
							<LinkedTransactionID>
								<Type>AccountingInvoice</Type>
								<Key>AR INV 00001000</Key>
							</LinkedTransactionID>
						</LinkedTransactionIDCollection>
						<OSPaidAmount>1000.0000</OSPaidAmount>
						<OrganizationAddress>
							<AddressType>OFC</AddressType>
							<Address1>171 ABBOTSFORD ROAD</Address1>
							<Address2>MAYNE, QLD</Address2>
							<AddressOverride>false</AddressOverride>
							<AddressShortCode>PST: 171 ABBOTSFORD ROAD</AddressShortCode>
							<City></City>
							<CompanyName>ABI GAS &amp; TOOLS</CompanyName>
							<Contact>LYNN MCVIE</Contact>
							<Country>
								<Code>AU</Code>
								<Name>Australia</Name>
							</Country>
							<Email></Email>
							<Fax></Fax>
							<OrganizationCode>ABIGAS</OrganizationCode>
							<Phone></Phone>
							<Port>
								<Code>AUBNE</Code>
								<Name>Brisbane</Name>
							</Port>
							<Postcode>4006</Postcode>
							<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
							</ScreeningStatus>
							<State></State>
						</OrganizationAddress>
						<MatchGroupNumber></MatchGroupNumber>
						<MatchDate>2021-03-15</MatchDate>
					</MatchLine>
				</MatchLineCollection>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
					<AddressType>OFC</AddressType>
					<Address1>171 ABBOTSFORD ROAD</Address1>
					<Address2>MAYNE, QLD</Address2>
					<AddressOverride>false</AddressOverride>
					<AddressShortCode>PST: 171 ABBOTSFORD ROAD</AddressShortCode>
					<City></City>
					<CompanyName>ABI GAS &amp; TOOLS</CompanyName>
					<Contact>LYNN MCVIE</Contact>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Email></Email>
					<Fax></Fax>
					<OrganizationCode>ABIGAS</OrganizationCode>
					<Phone></Phone>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4006</Postcode>
					<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
					</ScreeningStatus>
					<State></State>
				</OrganizationAddress>
				<Description>AR PAY</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>USD</Code>
					<Description>US Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>1000.0000</OSExGSTVATAmount>
				<OSTotal>1000.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>1000.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
					<AddressType>OFC</AddressType>
					<Address1>171 ABBOTSFORD ROAD</Address1>
					<Address2>MAYNE, QLD</Address2>
					<AddressOverride>false</AddressOverride>
					<AddressShortCode>PST: 171 ABBOTSFORD ROAD</AddressShortCode>
					<City></City>
					<CompanyName>ABI GAS &amp; TOOLS</CompanyName>
					<Contact>LYNN MCVIE</Contact>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Email></Email>
					<Fax></Fax>
					<OrganizationCode>ABIGAS</OrganizationCode>
					<Phone></Phone>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4006</Postcode>
					<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
					</ScreeningStatus>
					<State></State>
				</OrganizationAddress>
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
				<OSTotal>100.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>100.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		#endregion

	}
}
