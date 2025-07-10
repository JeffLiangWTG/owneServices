using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AccountingReceiptBatchDataContextManager))]
	sealed class AccountingReceiptBatchDataContextManagerTest : AccountingReceiptPaymentBatchDataContextManagerTest<AccountingReceiptBatchDataContextManager, Receipt>
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
Information - Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
Information -   Completed Processing Transaction.
Information - Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
Information -   Completed Processing Transaction.", logger.Logs);
			Assert("The result should be true after calling UseIncomingTransactionBatchData with valid data.", result);
		}

		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ProcessingManager()
		{
			var tupleResult = CreateTransactionBatch(XUBMessage_Successful);
			var invoice = CreateInvoiceForMatching();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			manager.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			var savedMatching = TestFactory.ExistsInDatabase(AccTransactionMatchLinkSchema.Constants.TableName, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK.ToGuid()));
			Assert("The Matching should be saved.", savedMatching);

			var matchedRECfilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			matchedRECfilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.ABIGAS.PK.ToGuid()));
			matchedRECfilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_InvoiceAmount, -1000M));
			var matchedREC = TestFactory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, matchedRECfilter);
			Assert("The matched REC should be saved.", matchedREC);

			var singleRECfilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			singleRECfilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.ABIGAS.PK.ToGuid()));
			singleRECfilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_InvoiceAmount, -100M));
			var singleREC = TestFactory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, singleRECfilter);
			Assert("The REC should be saved.", singleREC);

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
  Completed Processing Transaction.
Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
  Completed Processing Transaction.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, tupleResult.ediMessage.EM_Status);
		}

		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_DuplicateOrganizationsTransactionID()
		{
			var tupleResult1 = CreateTransactionBatch(XUBMessage_OrganizationsTransactionID);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			manager.Process(tupleResult1.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			var filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			filter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.ABIGAS.PK.ToGuid()));
			filter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_InvoiceAmount, -100M));
			var singleREC = TestFactory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, filter);
			Assert("The REC should be saved.", singleREC);

			var referenceFilter = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.OTI);
			referenceFilter.AddToFilter(new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Reference, "test OTI"));
			var hasSavedOTIReference = TestFactory.ExistsInDatabase(AccTransactionHeaderReferenceSchema.Constants.TableName, referenceFilter);
			Assert("The OTI data should be saved", hasSavedOTIReference);

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
  Completed Processing Transaction.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, tupleResult1.ediMessage.EM_Status);

			serviceTaskLog.ClearLogs();

			var tupleResult2 = CreateTransactionBatch(XUBMessage_OrganizationsTransactionID);
			manager.Process(tupleResult2.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
ERROR - Transaction ID test OTI has already been posted in the system with the same organization, ledger and transaction type.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, tupleResult1.ediMessage.EM_Status);

			var reference = TestFactory.Load<AccTransactionHeaderReference>(referenceFilter);
			AssertEquals("The test OTI reference still be one record.", 1, reference.Length);
		}

		[TestDate(2021, 03, 15)]
		public void TestDoNotSaveMatchingWhenFollowingProcessOccurError()
		{
			var tupleResult = CreateTransactionBatch(XUBMessage_MatchingNoErrorAndRECWithError);
			var invoice = CreateInvoiceForMatching();

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			var savedMatching = TestFactory.ExistsInDatabase(AccTransactionMatchLinkSchema.Constants.TableName, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK.ToGuid()));

			AssertEquals(EDIMessageStatusList.Codes.Discarded, tupleResult.ediMessage.EM_Status);
			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CASH: 
  Completed Processing Transaction.
Begin processing Transaction AR REC  ZHSBCAUD CASH: 
ERROR - No matches were found for the following Organization: 
ERROR - Account: Please enter an Account.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
			Assert("The Matching isn't saved due to the error occured when processing the following REC item.", !savedMatching);
		}

		[TestDate(2021, 03, 15)]
		public void TestStopProcessWhenOccurAnyError()
		{
			var tupleResult = CreateTransactionBatch(XUBMessage_WithError);

			var logger = new TestErrorLogger();
			var result = GetDataContextManager().UseIncomingTransactionBatchData(tupleResult.ediMessage, tupleResult.transactionBatch, logger, Factory);
			Assert("The result should be false due to the error occurred.", !result);
			AssertEquals(@"Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Information - Begin processing Transaction AR REC  ZHSBCAUD CASH: 
Error - No matches were found for the following Organization: 
Error - Account: Please enter an Account.
Information -   This transaction has errors and was not imported.", logger.Logs);
		}

		protected override Receipt GetNewBusinessObjectForTesting()
		{
			var result = Factory.NewWithValidTestData<ARReceipt>();
			Factory.SaveForTesting();
			return result;
		}

		protected override AccountingReceiptBatchDataContextManager GetDataContextManager()
		{
			return new AccountingReceiptBatchDataContextManager();
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
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
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
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
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

		readonly string XUBMessage_OrganizationsTransactionID = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
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
				<OrganizationsTransactionID>test OTI</OrganizationsTransactionID>
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

		readonly string XUBMessage_MatchingNoErrorAndRECWithError = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
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
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
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
					<!--No Org Address-->
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

		readonly string XUBMessage_WithError = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
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
					<!--No Org Address-->
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
					<AddressType>ConsigneeDocumentaryAddress</AddressType>
					<AddressShortCode>Pick Up Address</AddressShortCode>
					<OrganizationCode>BAROPT</OrganizationCode>
					<Address1>12 COOLIBAH DRIVE</Address1>
					<Address2></Address2>
					<AddressOverride>false</AddressOverride>
					<City>PALM BEACH</City>
					<CompanyName>BARZ OPTICS</CompanyName>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4221</Postcode>
					<State>QLD</State>
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
