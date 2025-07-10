using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.UniversalDataBuss.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AccountingMatchingBatchDataContextManager))]
	sealed class AccountingMatchingBatchDataContextManagerTest : AccountingReceiptPaymentBatchDataContextManagerTest<AccountingMatchingBatchDataContextManager, MatchingBase>
	{
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData()
		{
			var tupleResult = CreateTransactionBatch(XUBMessage_MatchingOnly);
			CreateInvoiceForMatching();

			var aRRec = TestFactory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = TestObjectCreator.ABIGAS.PK;
			aRRec.AH_LocalExTaxAmount = 1000M;
			aRRec.AH_OSExTaxAmount = 1000M;
			aRRec.AH_TransactionNum = "00001000";
			TestFactory.Save();

			var logger = new TestErrorLogger();
			var result = GetDataContextManager().UseIncomingTransactionBatchData(tupleResult.ediMessage, tupleResult.transactionBatch, logger, Factory);

			AssertEquals(@"Information - Matching 'OFC':- Matched to 'ABIGAS' by code, address 'PST: 171 ABBOTSFORD ROAD' by short code.
Information - Matching 'OFC':- Matched to 'ABIGAS' by code, address 'PST: 171 ABBOTSFORD ROAD' by short code.
Information - Begin processing Transaction AR REC ABIGAS  : 
Information -   Completed Processing Transaction.", logger.Logs);
			Assert("The result should be true after calling UseIncomingTransactionBatchData with valid data.", result);
		}

		protected override MatchingBase GetNewBusinessObjectForTesting()
		{
			var result = new ARMatchingBase(TestFactory);
			return result;
		}

		protected override AccountingMatchingBatchDataContextManager GetDataContextManager()
		{
			return new AccountingMatchingBatchDataContextManager();
		}

		readonly string XUBMessage_MatchingOnly = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingMatching</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<MatchLineCollection>
					<MatchLine>
						<LinkedTransactionIDCollection>
							<LinkedTransactionID>
								<Type>AccountingInvoice</Type>
								<Key>AR REC 00001000</Key>
							</LinkedTransactionID>
						</LinkedTransactionIDCollection>
						<OSPaidAmount>-1000.0000</OSPaidAmount>
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
				<PostDate>2021-03-15T14:12:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";
	}
}
