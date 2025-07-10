using System.Net.Http;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.ServiceHost;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	class eAdaptorControllerAccountingTest : TestCaseWithFactory
	{
		[TestDate(2021, 03, 15)]
		public void TestUniversalTransactionEadaptorImport_WithValidationError_AR()
		{
			var inboundXml = @"<UniversalTransaction>
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

			CreateTestPeriod();
			CreateAUDBankAccount();
			Factory.Save();

			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;

			var messages = Factory.Load<EDIMessage>(messagesQuery);
			AssertEquals("Precondition: No existing messages", 0, messages.Length);

			var financialTransactionsBefore = Factory.Load<AccTransactionHeader>(new ZQuery());
			AssertEquals("Precondition: no transactions", 0, financialTransactionsBefore.Length);

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;

				using (var response = controller.Post())
				{
					var newMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
					AssertEquals(1, newMessages.Length);
					var message = newMessages[0];
					AssertEquals("message.EM_Status", EDIMessage.Status.Rejected, message.EM_Status);

					CombineAssertions(() =>
					{
						this.AssertXMLEqualsByDiff($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIF</EventType>

    <ContextCollection>
      <Context>
        <Type>FailureReason</Type>
        <Value>Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage &gt; General Ledger &gt; Period Management &gt; Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.
</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - Line 80: &lt;TransactionInfo&gt;.&lt;OrganizationAddress&gt;.&lt;OrganizationCode&gt;The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Line 117: &lt;TransactionInfo&gt;.&lt;PostingJournalCollection&gt;.&lt;PostingJournal&gt;.&lt;Organization&gt;.&lt;Key&gt;The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Matching 'OFC':- No match found for '[Org. Code: DEPORT_AU]'.
Error - Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage &gt; General Ledger &gt; Period Management &gt; Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.
</ProcessingLog>
</UniversalResponse>", response.Content.ReadAsStringAsync().Result);

						var newTransactions = Factory.Load<AccTransactionHeader>(new ZQuery());
						AssertEquals("imports with validation errors should not create database records. This is very bad for Accounting.", 0, newTransactions.Length);
					});
				}
			}
		}

		void CreateTestPeriod()
		{
			var period = Factory.New<AccPeriodManagement>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_Year = 2021;
			period.AM_Period = 202103;
			period.AM_StartDate = new ZDateTime(2021, 03, 01);
			period.AM_EndDate = new ZDateTime(2021, 03, 31);
		}

		void CreateAUDBankAccount()
		{
			var fAUDBankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD"));
			if (fAUDBankAccount == null)
			{
				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZAUDAcc";
				fAUDBankAccount = Factory.New<AccBankAccount>();
				fAUDBankAccount.AB_Code = "ZHSBCAUD";
				fAUDBankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
				fAUDBankAccount.AB_Desc = "HSBC AUD ACCT";
				fAUDBankAccount.AB_AG = header.PK;
				fAUDBankAccount.AB_BankName = "HSBC";
				fAUDBankAccount.AB_BankAbbreviation = "AUD";
				fAUDBankAccount.AB_BSB = "123456";
				fAUDBankAccount.AB_AccountNum = "12345678";
				fAUDBankAccount.AB_RX_NKAccountCurrency = "AUD";
			}
		}
	}
}
