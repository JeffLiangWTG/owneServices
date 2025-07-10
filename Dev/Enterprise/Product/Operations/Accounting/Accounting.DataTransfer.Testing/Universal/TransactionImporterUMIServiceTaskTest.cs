using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	class TransactionImporterUMIServiceTaskTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestUMIImportUniversalTransactionWithValidationError()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var incomingMessage = Factory.New<EDIMessage>();

				incomingMessage.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
				incomingMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
				incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				incomingMessage.EM_Status = XmlEDIMessage.Status.Queued;
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_MessageNum = "12345678";
				incomingMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
				incomingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransaction>
	<TransactionInfo>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccountingInvoice</Type>
					<Key>AR INV TI00034457</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>AU</Code>
				</Country>
			</Company>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>ORP</Code>
					<Description>Organisation Proxy</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<APAccountGroup>
			<Code>TPY</Code>
		</APAccountGroup>
		<ARAccountGroup>
			<Code>TPY</Code>
		</ARAccountGroup>
		<Branch>
			<Code>SIM</Code>
		</Branch>
		<BranchAddress>
			<AddressType>OFC</AddressType>
			<Address1>10 HUTCHESON STREET</Address1>
			<Address2>ALBION  QLD</Address2>
			<AddressOverride>false</AddressOverride>
			<AddressShortCode>PST: 10 HUTCHESON STREET</AddressShortCode>
			<City></City>
			<CompanyName>EDI CUSTOMS BROKERS</CompanyName>
			<Country>
				<Code>AU</Code>
				<Name>Australia</Name>
			</Country>
			<Email></Email>
			<Fax></Fax>
			<OrganizationCode>EDICUS</OrganizationCode>
			<Phone></Phone>
			<Port>
				<Code>AUBNE</Code>
				<Name>Brisbane</Name>
			</Port>
			<Postcode>4010</Postcode>
			<ScreeningStatus>
				<Code>UNK</Code>
				<Description>Unknown</Description>
			</ScreeningStatus>
			<State></State>
		</BranchAddress>
		<Category>FIN</Category>
		<Department>
			<Code>BRN</Code>
		</Department>
		<Description>Murray's test description so its obvious</Description>
		<IsCancelled>false</IsCancelled>
		<Job>
			<Type>Job</Type>
		</Job>
		<JobInvoiceNumber/>
		<Ledger>AR</Ledger>
		<LocalCurrency>
			<Code>AUD</Code>
		</LocalCurrency>
		<LocalExVATAmount>126084.600</LocalExVATAmount>
		<LocalTotal>138693.060</LocalTotal>
		<LocalVATAmount>12608.460</LocalVATAmount>
		<Number>00034457</Number>
		<OrganizationAddress>
			<AddressType>OFC</AddressType>
			<AddressOverride>false</AddressOverride>
			<OrganizationCode>
DEPORT_AU</OrganizationCode>
		</OrganizationAddress>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
		<OSExGSTVATAmount>126084.600</OSExGSTVATAmount>
		<OSGSTVATAmount>12608.460</OSGSTVATAmount>
		<OSTotal>138693.060</OSTotal>
		<PostDate>2021-08-31</PostDate>
		<TransactionDate>2021-08-31</TransactionDate>
		<DueDate>2021-09-30</DueDate>
		<TransactionType>INV</TransactionType>
		<PostingJournalCollection>
			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>Rail Rebate 20FT</Description>
				<GLAccount>
					<AccountCode>1080.10.10</AccountCode>
				</GLAccount>
				<GLPostDate>2021-08-31</GLPostDate>
				<Job>
					<Type>Job</Type>
				</Job>
				<LocalAmount>26552.70</LocalAmount>
				<LocalCurrency>
					<Code>AUD</Code>
				</LocalCurrency>
				<LocalGSTVATAmount>2655.27</LocalGSTVATAmount>
				<LocalTotalAmount>29207.97</LocalTotalAmount>
				<Organization>
					<Type>Organization</Type>
					<Key>
DEPORT_AU</Key>
				</Organization>
				<OSCurrency>
					<Code>AUD</Code>
				</OSCurrency>
				<OSAmount>26552.70</OSAmount>
				<OSGSTVATAmount>2655.27</OSGSTVATAmount>
				<OSTotalAmount>2655.27</OSTotalAmount>
				<RevenueRecognitionType>IMM</RevenueRecognitionType>
				<TransactionCategory>FIN</TransactionCategory>
				<TransactionType>REV</TransactionType>
				<VATTaxID>
					<TaxCode>GST</TaxCode>
					<Description>Standard Rated (Non-Capital)</Description>
					<TaxRate>10</TaxRate>
					<TaxType>
						<Code>RAT</Code>
					</TaxType>
				</VATTaxID>
			</PostingJournal>
			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>Rail Rebate 40FT</Description>
				<GLAccount>
					<AccountCode>1080.10.10</AccountCode>
				</GLAccount>
				<GLPostDate>2021-08-31</GLPostDate>
				<Job>
					<Type>Job</Type>
				</Job>
				<LocalAmount>99531.90</LocalAmount>
				<LocalCurrency>
					<Code>AUD</Code>
				</LocalCurrency>
				<LocalGSTVATAmount>9953.19</LocalGSTVATAmount>
				<LocalTotalAmount>109485.09</LocalTotalAmount>
				<Organization>
					<Type>Organization</Type>
					<Key>DEPORT_AU</Key>
				</Organization>
				<OSCurrency>
					<Code>AUD</Code>
				</OSCurrency>
				<OSAmount>99531.90</OSAmount>
				<OSGSTVATAmount>9953.19</OSGSTVATAmount>
				<OSTotalAmount>9953.19</OSTotalAmount>
				<RevenueRecognitionType>IMM</RevenueRecognitionType>
				<TransactionCategory>FIN</TransactionCategory>
				<TransactionType>REV</TransactionType>
				<VATTaxID>
					<TaxCode>GST</TaxCode>
					<Description>Standard Rated (Non-Capital)</Description>
					<TaxRate>10</TaxRate>
					<TaxType>
						<Code>RAT</Code>
					</TaxType>
				</VATTaxID>
			</PostingJournal>
		</PostingJournalCollection>
	</TransactionInfo>
</UniversalTransaction>";

				incomingMessage.Factory.Save();

				var serviceTask = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
				serviceTask.RunTask();

				var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
				AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

				var notes = ((StmNoteCollection)reloadedMessage.Notes.GetAllNotes());
				AssertEquals("Should have one note", 1, notes.Count);

				var logger = (TestServiceLogger)serviceTask.ServiceLogger;

				CombineAssertions(delegate
				{
					AssertContains("logger", "Information|Starting processing Message #12345678", logger.ToString());
					AssertContains("logger", "Warning|Exception processing message 12345678: [Import failed because transaction has validation errors:", logger.ToString());
					AssertContains("logger", "Error - Account: Please enter an Account.", logger.ToString());
					AssertContains("logger", "Information|Finished processing Message #12345678", logger.ToString());

					AssertMultilineASCIIEquals("EDI Message Note text", @"Warning - Line 81: <TransactionInfo>.<OrganizationAddress>.<OrganizationCode>The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Line 118: <TransactionInfo>.<PostingJournalCollection>.<PostingJournal>.<Organization>.<Key>The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Matching 'OFC':- No match found for '[Org. Code: DEPORT_AU]'.
Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.", notes[0].ST_NoteDataAsText);

					AssertEquals("reloadedMessage.EM_Status", EDIMessage.Status.Rejected, reloadedMessage.EM_Status);

					var createdInvoices = Factory.Load<AccTransactionHeader>(new ZQuery());
					AssertEquals(0, createdInvoices.Length);
				});
			}
		}
	}
}
