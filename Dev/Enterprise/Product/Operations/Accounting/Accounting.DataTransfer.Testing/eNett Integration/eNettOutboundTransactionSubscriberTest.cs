using System;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.BatchProcessor.Accounting.eNettOutboundTransactionSubscriber;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[TestedType(typeof(eNettOutboundTransactionSubscriber))]
	public class eNettOutboundTransactionSubscriberTest : LogSubscriberTest<eNettOutboundTransactionSubscriber>
	{
		const int MaxRetriesForConcurrencyException = 10;
		class TestDirectDebitData
		{
			public DirectDebitBatchHeader DirectDebitBatchHeaderRecord { get; set; }
			public Payment PaymentRecord { get; set; }
		}

		public void TestPreconditionsForFurtherTesting()
		{
			AssertEquals("Pre-requisite: original company shouldn't be eNett reg'd", String.Empty, AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode);
			AssertEquals("Pre-requisite: valid eNett company should be eNett reg'd", "201649", AccountingConfigurationRegistry.Instance.ENettRegistration.GetFallBackValueAtAllLevels(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).RegistrationCode);
			AssertEquals("Pre-requisite: valid orgheader should be eNett reg'd", "201654", eNettRegisteredOrg.ENettRegistrationNumber);
			AssertEquals("Pre-requisite: invalid orgheader shouldn't be eNett reg'd", String.Empty, invalidENettOrg.ENettRegistrationNumber);
		}

		public void TestProcessTransactionsInvalidForENett()
		{
			InvoicingBase invoiceWithInvalidCompany, invoiceWithInvalidOrgHeader, invoiceWithInvalidCompanyAndOrg;

			invoiceWithInvalidCompany = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00001", eNettRegisteredOrg.PK);
			invoiceWithInvalidCompanyAndOrg = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00002", invalidENettOrg.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				invoiceWithInvalidOrgHeader = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00003", invalidENettOrg.PK);

				Factory.Save();
			}

			ProcessLogs();
			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 0, 0, 0, 0);
		}

		public void TestDontProcessWhenNoENettRegistrationDetails()
		{
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode());

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ARInvoice invoice = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK, true);
				Factory.Save();
			}

			ProcessLogs();
			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 0, 0, 0, 0);
		}

		public void TestTransactionIsNotSentTwice()
		{
			InvoicingBase invoice;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				invoice = CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK, true);

				Factory.Save();
			}

			MockENettWebService.Instance.ResetProperties();
			ProcessLogs();
			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 1, 0, 0, 0);

			MockENettWebService.Instance.ResetProperties();
			ProcessLogs();
			AssertCountOfMethodCalls(0, 0, 0, 0, 0, 0);
		}

		[TestDate(2009, 01, 02)]
		public void TestProcessCreateInvoices()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "";
			var originalValue = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.GetValueWithoutFallback(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			try
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABCD");
				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

				OrgHeader brokerAIR;
				JobHeader jobHeader;
				// naming convention: XYZ, where X: is company eNett-reg'd; Y: is debtor eNett-reg'd; Z: is ledger valid.
				InvoicingBase invNNN, invNNY, invNYN, invNYY, invYNN, invYNY, invYYN, invYYY;

				brokerAIR = Factory.NewWithValidTestData<OrgHeader>();
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("CompanyData is for eeNetttBCompany", eNettCompanyBranch.Company.PK, brokerAIR.CompanyData.OB_GC);
					brokerAIR.CompanyData.OB_IsDebtor = true;
				}
				brokerAIR.CompanyData.OB_IsDebtor = true; // Set for the current company as well
				brokerAIR.CustomsCodes.AddNew(OrgCusCode.CodeTypes.eNettRegistrationNumber, "TSTBRK001");

				eNettRegisteredOrg.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Broker;
				OrgRelatedParty relatedPartyAIR = eNettRegisteredOrg.ConsigneeRelatedParties.AddNew();
				relatedPartyAIR.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
				relatedPartyAIR.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
				relatedPartyAIR.PR_OH_RelatedParty = brokerAIR.PK;
				Factory.Save();

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
				shipment.ConsigneePK = eNettRegisteredOrg.PK;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";

				jobHeader = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();

				invNNN = (APInvoice)CreateTransaction(typeof(APInvoice), "00001", invalidENettOrg.PK, true);
				invNNY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00002", invalidENettOrg.PK, true);
				invNYN = (APInvoice)CreateTransaction(typeof(APInvoice), "00003", eNettRegisteredOrg.PK, true);
				invNYY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00004", eNettRegisteredOrg.PK, true);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					invYNN = (APInvoice)CreateTransaction(typeof(APInvoice), "00005", invalidENettOrg.PK, true);
					invYNY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00006", invalidENettOrg.PK, true);
					invYYN = (APInvoice)CreateTransaction(typeof(APInvoice), "00007", eNettRegisteredOrg.PK, true);
					invYYY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK, true);
					invYYY.AH_JH = jobHeader.PK;

					invYYY.Lines[0].AL_AW = TestObjectCreator.WHTFREE1.PK;
					invYYY.Lines[0].RelatedJobCharge.JR_AW_CostWHTRate = TestObjectCreator.WHTFREE1.PK;
					Factory.Save();
				}

				ProcessLogs();

				FactorySaveAlerterTestFailureAlternatives(false);
				AssertCountOfMethodCalls(0, 0, 1, 0, 0, 0);
				AssertEquals("TSTBRK001", MockENettWebService.Instance.LastBrokerPassedWhenCreateInvoiceCalled);
				AssertEquals("Transaction Number Passed to Web Service", invYYY.TransactionNumberPrefixed, MockENettWebService.Instance.LastTransactionNumberWhenCreateInvoiceCalled);

				var createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.eNett));

				AssertEquals("Should have created 1 message", 1, createdMessages.Length);
				var dateTimeOffset = new DateTimeOffset(TestDateAttribute.Date).Offset;
				AssertEDIMessage(createdMessages[0], 1, "CNI", true, String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>

    <XmlType>Verbose</XmlType>
    <Source>
      <CompanyCode>ABC</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>username</LoginName>
      <LoginUserEmailAddress>A@B.COM</LoginUserEmailAddress>
    </Source>
    <Target />
    <EDIOrganisation EDICode=""ZABCPROXY"" OwnerCode=""ZABCPROXY"">
      <OrganisationDetails>
        <Name>Test Company Name</Name>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>184 Bourke Road</AddressLine1>
            <AddressCode>184 Bourke Road</AddressCode>
            <CityOrSuburb>Alexandria</CityOrSuburb>
            <StateOrProvince>NSW</StateOrProvince>
            <Language>EN</Language>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
        <RegistrationNumbers>
          <RegistrationNumber>
            <CountryOfRegistration>AU</CountryOfRegistration>
            <NumberType>ENE</NumberType>
            <Number>201654</Number>
          </RegistrationNumber>
        </RegistrationNumbers>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <FinancialTransactions>
      <FinancialInvoice>
        <Ledger>AR</Ledger>
        <DebtorOrCreditor EDICode=""ZVALID"" OwnerCode=""ZVALID"">
          <OrganisationDetails>
            <Name>Test Company Name</Name>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>184 Bourke Road</AddressLine1>
                <AddressCode>184 Bourke Road</AddressCode>
                <CityOrSuburb>Alexandria</CityOrSuburb>
                <StateOrProvince>NSW</StateOrProvince>
                <Language>EN</Language>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
            <RegistrationNumbers>
              <RegistrationNumber>
                <CountryOfRegistration>AU</CountryOfRegistration>
                <NumberType>ENE</NumberType>
                <Number>201654</Number>
              </RegistrationNumber>
            </RegistrationNumbers>
            <AccountsReceivables>
              <AccountsReceivable>
                <AllowMultiCurrencyPayment>false</AllowMultiCurrencyPayment>
              </AccountsReceivable>
            </AccountsReceivables>
          </OrganisationDetails>
        </DebtorOrCreditor>
        <TxnType>INV</TxnType>
        <TxnCount>1</TxnCount>
        <TxnNumber>{0}</TxnNumber>
        <Description>Test Invoice</Description>

        <InvTerm>COD</InvTerm>
        <InvTermDays>0</InvTermDays>


        <GLPeriod>999998</GLPeriod>
        <Branch>BR1</Branch>
        <Department>BRN</Department>
        <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">10.00</LocalInvoiceAmtExclTax>
        <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">10.00</LocalInvoiceAmtInclTax>
        <LocalTaxAmount CurrencyCode=""AUD"">0.00</LocalTaxAmount>
        <LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount>
        <OsInvoiceAmtExclTax CurrencyCode=""AUD"">10.00</OsInvoiceAmtExclTax>
        <OsInvoiceAmtInclTax CurrencyCode=""AUD"">10.00</OsInvoiceAmtInclTax>
        <OsTaxAmount CurrencyCode=""AUD"">0.00</OsTaxAmount>
        <OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount>
        <CashBasisTaxIndicator>N</CashBasisTaxIndicator>
        <CreatedUserId>username</CreatedUserId>
        <TxnHeaderGUID>headerGUID</TxnHeaderGUID>
        <Attachments>
          <Attachment>
            <FileName>Invoice 00001001.pdf</FileName>

          </Attachment>
        </Attachments>
        <TxnLines>
          <TxnLine>
            <LineType>REV</LineType>
            <Sequence>1</Sequence>
            <ChargeCode>ZZCC1</ChargeCode>
            <ChargeGroup>NGC</ChargeGroup>
            <GLAccount>{1}</GLAccount>
            <Description>tee he he</Description>
            <Branch>BR1</Branch>
            <Department>BRN</Department>
            <ConsolOrJobNo>7Y0DGQI2UW8ZO5CHQQZW</ConsolOrJobNo>
            <ConsolOrJobType>SHP</ConsolOrJobType>
            <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">10.00</LocalInvoiceAmtExclTax>
            <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">10.00</LocalInvoiceAmtInclTax>
            <LocalTaxAmount CurrencyCode=""AUD"">0.00</LocalTaxAmount>
            <LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount>
            <OsInvoiceAmtExclTax CurrencyCode=""AUD"">10.00</OsInvoiceAmtExclTax>
            <OsInvoiceAmtInclTax CurrencyCode=""AUD"">10.00</OsInvoiceAmtInclTax>
            <OsTaxAmount CurrencyCode=""AUD"">0.00</OsTaxAmount>
            <OsWHTAmount CurrencyCode=""AUD"">0</OsWHTAmount>
            <WHTCode>ZZWHTFREE1</WHTCode>
            <DepartmentActivity>Miscellaneous</DepartmentActivity>
            <Weight DimensionType=""KG"">0.000</Weight>
            <Volume DimensionType=""M3"">0.000</Volume>
            <Chargeable>0.000</Chargeable>
            <OSChargeAmount CurrencyCode=""AUD"">10.00</OSChargeAmount>
            <OSChargeTaxAmount CurrencyCode=""AUD"">0</OSChargeTaxAmount>
            <TxnLineGUID>{2}</TxnLineGUID>
            <RevenueRecognitionDate>2009-01-02T00:00:00{3}:{4}</RevenueRecognitionDate>
          </TxnLine>
        </TxnLines>
      </FinancialInvoice>
    </FinancialTransactions>
  </Payload>
</XmlInterchange>", invYYY.TransactionNumberPrefixed, invYYY.Lines[0].GLHeader.AG_AccountNum, invYYY.Lines[0].PK,
dateTimeOffset.Hours.ToString("+00;-0#"),
dateTimeOffset.Minutes.ToString("00")
), invYYY.AH_GB, invYYY.AH_GE, invYYY.PK);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalValue);
			}
		}

		[TestDate(2050, 01, 01)]
		[SuspendCriticalValidation]
		public void TestPaidTransactionsNodeExcludingNonInvTypeTransactions()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "";

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				eNettRegisteredOrg.APSettlementGroupPK = eNettRegisteredOrg2.PK;

				SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

				APInvoice inv;
				APPayment pay;
				AccBankAccount acc = TestObjectCreator.CreateBankAccount("BANKACC", "Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				acc.AB_BSB = "123-123";
				acc.AB_AccountNum = "123123132";

				inv = (APInvoice)CreateTransaction(typeof(APInvoice), "00010", eNettRegisteredOrg2.PK);
				pay = (APPayment)CreatePayment(typeof(APPayment), "20001", eNettRegisteredOrg.PK, acc.PK, 11);

				APMatchingBase matching = new APMatchingBase(Factory, pay);

				((IMatching)inv).OSPartialPaymentAmount = 11;
				matching.UnmatchedTransactions.Add(inv);
				matching.MoveAllFromUnmatchToMatch();
				matching.MatchAndClearTransactions();

				Factory.Save();
				ProcessLogs();

				FactorySaveAlerterTestFailureAlternatives(false);
				AssertCountOfMethodCalls(0, 0, 0, 0, 1, 0);
				EDIMessage[] createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.ProcessDirectDebit));
				AssertEquals("Should have created 1 message", 1, createdMessages.Length);
				var dateTimeOffset = new DateTimeOffset(TestDateAttribute.Date).Offset;
				AssertEDIMessage(createdMessages[0], 1, "PDD", true, String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>

    <XmlType>Verbose</XmlType>
    <Source>
      <CompanyCode>ABC</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>username</LoginName>
      <LoginUserEmailAddress>A@B.COM</LoginUserEmailAddress>
    </Source>
    <Target />
    <EDIOrganisation EDICode=""ZABCPROXY"" OwnerCode=""ZABCPROXY"">
      <OrganisationDetails>
        <Name>Test Company Name</Name>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>184 Bourke Road</AddressLine1>
            <AddressCode>184 Bourke Road</AddressCode>
            <CityOrSuburb>Alexandria</CityOrSuburb>
            <StateOrProvince>NSW</StateOrProvince>
            <Language>EN</Language>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
        <RegistrationNumbers>
          <RegistrationNumber>
            <CountryOfRegistration>AU</CountryOfRegistration>
            <NumberType>ENE</NumberType>
            <Number>201654</Number>
          </RegistrationNumber>
        </RegistrationNumbers>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <FinancialTransactions>
      <FinancialInvoice>
        <Ledger>AP</Ledger>
        <DebtorOrCreditor EDICode=""ZVALID"" OwnerCode=""ZVALID"">
          <OrganisationDetails>
            <Name>Test Company Name</Name>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>184 Bourke Road</AddressLine1>
                <AddressCode>184 Bourke Road</AddressCode>
                <CityOrSuburb>Alexandria</CityOrSuburb>
                <StateOrProvince>NSW</StateOrProvince>
                <Language>EN</Language>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
            <RegistrationNumbers>
              <RegistrationNumber>
                <CountryOfRegistration>AU</CountryOfRegistration>
                <NumberType>ENE</NumberType>
                <Number>201654</Number>
              </RegistrationNumber>
            </RegistrationNumbers>
          </OrganisationDetails>
        </DebtorOrCreditor>
        <TxnType>PAY</TxnType>
        <Description>AP PAYMENT</Description>


        <Branch>BR1</Branch>
        <Department>BRN</Department>
        <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">11.00</LocalInvoiceAmtExclTax>
        <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">11.00</LocalInvoiceAmtInclTax>
        <OsInvoiceAmtExclTax CurrencyCode=""AUD"">11.00</OsInvoiceAmtExclTax>
        <OsInvoiceAmtInclTax CurrencyCode=""AUD"">11.00</OsInvoiceAmtInclTax>
        <BankCode>BANKACC</BankCode>
        <ReceiptPaymentType>END</ReceiptPaymentType>
        <CreatedUserId>username</CreatedUserId>
        <PaidTransactions>
          <PaidTransaction>
            <Ledger>AP</Ledger>
            <DebtorOrCreditor EDICode=""ZVALID2"" OwnerCode=""ZVALID2"">
              <OrganisationDetails>
                <Name>Test Company Name</Name>
                <Addresses>
                  <Address AddressType=""MAIN"">
                    <AddressLine1>184 Bourke Road</AddressLine1>
                    <AddressCode>184 Bourke Road</AddressCode>
                    <CityOrSuburb>Alexandria</CityOrSuburb>
                    <StateOrProvince>NSW</StateOrProvince>
                    <Language>EN</Language>
                    <Sequence>1</Sequence>
                    <AddressCapabilities>
                      <AddressCapability AddressType=""MAIN"" />
                      <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                    </AddressCapabilities>
                  </Address>
                </Addresses>
                <RegistrationNumbers>
                  <RegistrationNumber>
                    <CountryOfRegistration>AU</CountryOfRegistration>
                    <NumberType>ENE</NumberType>
                    <Number>201655</Number>
                  </RegistrationNumber>
                </RegistrationNumbers>
              </OrganisationDetails>
            </DebtorOrCreditor>
            <TxnType>INV</TxnType>
            <TxnNumber>00010</TxnNumber>
            <Description>Test Invoice</Description>



            <Branch>BR1</Branch>
            <Department>BRN</Department>
            <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">-10.00</LocalInvoiceAmtExclTax>
            <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">-11.00</LocalInvoiceAmtInclTax>
            <OsInvoiceAmtExclTax CurrencyCode=""AUD"">-10.00</OsInvoiceAmtExclTax>
            <OsInvoiceAmtInclTax CurrencyCode=""AUD"">-11.00</OsInvoiceAmtInclTax>
            <AmountPaidThisPayment CurrencyCode=""AUD"">11.00</AmountPaidThisPayment>
            <ENettStoragePaymentDetails />
          </PaidTransaction>
        </PaidTransactions>
      </FinancialInvoice>
    </FinancialTransactions>
  </Payload>
</XmlInterchange>"), pay.AH_GB, pay.AH_GE, pay.PK);
				createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.Response));
				AssertEquals("Should have created 1 message", 1, createdMessages.Length);
				AssertEDIMessage(createdMessages[0], 2, "RES", true, String.Format(@"<?xml version=""1.0""?>
<Response_ProcessDirectDebit xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""https://enettlogistics.com/"">
  <success>true</success>
  <authorised>false</authorised>
  <processedDateTime></processedDateTime>
  <batchDate>2050-01-01T00:00:00{0}:{1}</batchDate>
</Response_ProcessDirectDebit>",
dateTimeOffset.Hours.ToString("+00;-0#"),
dateTimeOffset.Minutes.ToString("00")
), pay.AH_GB, pay.AH_GE, pay.PK);
			}
		}

		[TestDate(2050, 01, 01)]
		[SuspendCriticalValidation]
		public void TestProcessDirectDebitWithoutConcurrencyException()
		{
			try
			{
				var directDebitBatchData = TestProcessDirectDebit();

				var directDebitBatchHeader = directDebitBatchData.DirectDebitBatchHeaderRecord;
				var payment = directDebitBatchData.PaymentRecord;

				//No DB Concurrency update exception, the following two amounts are 100 (initial) - 20 (invoice) = 80;
				AssertEquals((ZDecimal)80, directDebitBatchHeader.AH_InvoiceAmount);
				AssertEquals((ZDecimal)80, directDebitBatchHeader.AH_OSTotal);

				AssertEquals("Payment.AH_ReceiptBatchNo == DirectDebitBatchHeader.AH_TransactionNum", directDebitBatchHeader.AH_TransactionNum, payment.AH_ReceiptBatchNo);
			}
			finally
			{
				eNettOutboundTransactionSubscriber.SeteNettBeforeBatchingCommit(null);
			}
		}

		[TestDate(2050, 01, 01)]
		[SuspendCriticalValidation]
		public void TestProcessDirectDebitWithConcurrencyExceptionAndFinalSuccess()
		{
			try
			{
				var mockErrorReporter = new Mock<IErrorReporter>();
				ErrorReporter.Instance = mockErrorReporter.Object;

				var directDebitBatchData = TestProcessDirectDebit((sender, e) =>
				{
					if (e.SJ_RetryCount < MaxRetriesForConcurrencyException)
					{
						var newFactory = Factory.CreateNewFactory();
						var batchRecord = newFactory.Load<DirectDebitBatchHeader>(e.BatchHeaderPK);
						batchRecord.AH_InvoiceAmount += 100;
						batchRecord.AH_OSTotal += 100;
						newFactory.Save();
					}
				});
				// With nine DB Concurrency update exceptions, and one final successful updates the following amounts
				// end up with 100 (initial) - 20 (invoice) + 9*100 = 980
				var directDebitBatchHeader = directDebitBatchData.DirectDebitBatchHeaderRecord;
				var payment = directDebitBatchData.PaymentRecord;

				AssertEquals((ZDecimal)980, directDebitBatchHeader.AH_InvoiceAmount);
				AssertEquals((ZDecimal)980, directDebitBatchHeader.AH_OSTotal);
				AssertEquals("Payment.AH_ReceiptBatchNo == DirectDebitBatchHeader.AH_TransactionNum", directDebitBatchHeader.AH_TransactionNum, payment.AH_ReceiptBatchNo);
				//Make sure no error reported
				mockErrorReporter.VerifyNoOtherCalls();
			}
			finally
			{
				eNettOutboundTransactionSubscriber.SeteNettBeforeBatchingCommit(null);
			}
		}

		[TestDate(2050, 01, 01)]
		[SuspendCriticalValidation]
		public void TestProcessDirectDebitWithConcurrencyExceptionAndFinalFailure()
		{
			try
			{
				var directDebitBatchData = TestProcessDirectDebit((sender, e) =>
					{
						var newFactory = Factory.CreateNewFactory();
						var batchRecord = newFactory.Load<DirectDebitBatchHeader>(e.BatchHeaderPK);
						batchRecord.AH_InvoiceAmount += 100;
						batchRecord.AH_OSTotal += 100;
						newFactory.Save();
					});
				//With DB Concurrency update exceptions and retries maxed out,
				//the following amount failed to be updated from the subscriber factotry eventually, therefore, still keeping the original value 
				var directDebitBatchHeader = directDebitBatchData.DirectDebitBatchHeaderRecord;

				AssertEquals((ZDecimal)100, directDebitBatchHeader.AH_InvoiceAmount);
				AssertEquals((ZDecimal)100, directDebitBatchHeader.AH_OSTotal);
			}
			finally
			{
				eNettOutboundTransactionSubscriber.SeteNettBeforeBatchingCommit(null);
				ExceptionReporterTestListener.Instance.Clear();
				ExceptionReporter.Instance.TotalReportCount = 0;
			}
		}

		//For further testing info
		[ExpectNoExceptions]
		public void TestCreateInvoiceForMock()
		{
			InvoicingBase inv;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK);
				Factory.Save();
			}

			using (new TemporaryUserContext() { BranchPK = inv.AH_GB.ToGuid(), DepartmentPK = inv.AH_GE.ToGuid() }.Set())
			{
				eNettOutboundFinancialInvoiceDataAdapter adapter = new eNettOutboundFinancialInvoiceDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(adapter.ValueObjectType);
				DataTransfer.Xml.XsdVersion1.TxnHeader invoiceValue = adapter.ExportToValueObject(inv, new ValueObjectExportContext(new NotificationBuffer()));
				XmlElement xml = serializer.SerialiseToXmlElement(invoiceValue);
			}
		}

		public void TestProcessCancelInvoices_InvalidInvoiceNumber()
		{
			InvoicingBase inv, crd;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "10001", eNettRegisteredOrg.PK, true);
				Factory.Save();
				crd = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00004", eNettRegisteredOrg.PK);
				crd.AH_TransactionBelongsToGroup = inv.PK;
				crd.AH_IsCancelled = true;
				((IMatching)crd).CurrentMatchGroup.AddNew().AP_AH = crd.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(crd);
			}

			Factory.Save();
			ProcessLogs();

			AssertCountOfMethodCalls(0, 0, 1, 1, 0, 0);
			AssertEquals("One email should have been sent as the original invoice is wrong", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[SuspendCriticalValidation]
		public void TestProcessCancelInvoices_ValidInvoiceNumber()
		{
			InvoicingBase inv, crdNNN, crdNNY, crdNYN, crdNYY, crdYNN, crdYNY, crdYYN, crdYYY;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00001000", eNettRegisteredOrg.PK, true);
				crdYYN = (APCreditNote)CreateTransaction(typeof(APCreditNote), "00001", eNettRegisteredOrg.PK);
				Factory.Save();
				crdYNN = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00002", invalidENettOrg.PK);
				Factory.Save();
				crdYNY = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00003", invalidENettOrg.PK);
				crdYNY.AH_TransactionBelongsToGroup = crdYYN.PK;
				crdYNY.AH_IsCancelled = true;
				((IMatching)crdYNY).CurrentMatchGroup.AddNew().AP_AH = crdYNY.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(crdYNY);
				crdYYY = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00004", eNettRegisteredOrg.PK);
				crdYYY.AH_TransactionBelongsToGroup = inv.PK;
				crdYYY.AH_IsCancelled = true;
				((IMatching)crdYYY).CurrentMatchGroup.AddNew().AP_AH = crdYYY.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(crdYYY);
				MockENettWebService.Instance.RegisteredInvoices.Add("00001000");
				Factory.Save();
			}

			crdNNN = (APCreditNote)CreateTransaction(typeof(APCreditNote), "00005", invalidENettOrg.PK);
			Factory.Save();
			crdNNY = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00006", invalidENettOrg.PK);
			crdNNY.AH_TransactionBelongsToGroup = crdYYN.PK;
			crdNNY.AH_IsCancelled = true;
			((IMatching)crdNNY).CurrentMatchGroup.AddNew().AP_AH = crdNNY.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(crdNNY);
			crdNYN = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00007", eNettRegisteredOrg.PK);
			crdNYY = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00008", eNettRegisteredOrg.PK);
			crdNYY.AH_TransactionBelongsToGroup = crdYYN.PK;
			crdNYY.AH_IsCancelled = true;
			((IMatching)crdNYY).CurrentMatchGroup.AddNew().AP_AH = crdNYY.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(crdNYY);
			MockENettWebService.Instance.RegisteredInvoices.Add("00001002");

			Factory.Save();
			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 1, 1, 0, 0);
			AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessCancelInvoices_InvoiceWasNotSent()
		{
			InvoicingBase inv, crd;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00001000", eNettRegisteredOrg.PK, true);

				MockENettWebService.Instance.RegisteredInvoices.Add("00001000");
			}

			Factory.Save();
			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 1, 0, 0, 0);
			AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.eNett);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "ENE");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, AccTransactionHeader.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, inv.PK);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
			const string indexName = "NR_RX__EM_LinkUniqueID"; // there is no existing constant for this index name
			query.TableIndexHints.Add(new TableIndexHint(indexName));

			eNettEDIMessage message = Factory.LoadTop1<eNettEDIMessage>(query);
			AssertNotNull("Sent Message", message);
			message.EM_Status = EDIMessage.Status.Failed;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				crd = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "00004", eNettRegisteredOrg.PK);
				crd.AH_TransactionBelongsToGroup = inv.PK;
				crd.AH_IsCancelled = true;
				((IMatching)crd).CurrentMatchGroup.AddNew().AP_AH = crd.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(crd);
			}

			Factory.Save();

			DbConnection conn = Db.Connection;
			conn.BeginTransaction();

			try
			{
				MockENettWebService.Instance.ResetProperties();
				ProcessLogs();

				AssertCountOfMethodCalls(0, 0, 0, 0, 0, 0);
				AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				conn.RollbackTransaction();
			}

			message.EM_Status = EDIMessage.Status.Sent;

			Factory.Save();

			MockENettWebService.Instance.ResetProperties();
			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 0, 1, 0, 0);
			AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[SuspendCriticalValidation]
		public void TestProcessOfflinePayment()
		{
			AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			// naming convention: XYZ, where X: is company eNett-reg'd; Y: is debtor eNett-reg'd; Z: is ledger valid.
			InvoicingBase invNNN, invNNY, invNYN, invNYY, invYNN, invYNY, invYYN, invYYY, invYYYCancelled;

			invNNN = (APInvoice)CreateTransaction(typeof(APInvoice), "00001", invalidENettOrg.PK);
			invNNY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00002", invalidENettOrg.PK);
			invNYN = (APInvoice)CreateTransaction(typeof(APInvoice), "00003", eNettRegisteredOrg.PK);
			invNYY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00004", eNettRegisteredOrg.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				invYNN = (APInvoice)CreateTransaction(typeof(APInvoice), "00005", invalidENettOrg.PK);
				invYNY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00006", invalidENettOrg.PK);
				invYYN = (APInvoice)CreateTransaction(typeof(APInvoice), "00007", eNettRegisteredOrg.PK);
				invYYY = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK);
				invYYYCancelled = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00009", eNettRegisteredOrg.PK);
				invYYYCancelled.AH_IsCancelled = true;
				((IMatching)invYYYCancelled).CurrentMatchGroup.AddNew().AP_AH = invYYYCancelled.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(invYYYCancelled);

				Factory.Save();
			}

			TransactionMatchLink matchLinkForInvNNN, matchLinkForInvNNY, matchLinkForInvNYN, matchLinkForInvNYY, matchLinkForInvYNN, matchLinkForInvYNY, matchLinkForInvYYN, matchLinkForInvYYY, matchLinkforInvYYYCancelled;
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			matchLinkForInvNNN = matchGroup.AddNew();
			matchLinkForInvNNY = matchGroup.AddNew();
			matchLinkForInvNYN = matchGroup.AddNew();
			matchLinkForInvNYY = matchGroup.AddNew();
			matchLinkForInvYNN = matchGroup.AddNew();
			matchLinkForInvYNY = matchGroup.AddNew();
			matchLinkForInvYYN = matchGroup.AddNew();
			matchLinkForInvYYY = matchGroup.AddNew();
			matchLinkforInvYYYCancelled = matchGroup.AddNew();

			matchLinkForInvNNN.AP_Amount =
			matchLinkForInvNNY.AP_Amount =
			matchLinkForInvNYN.AP_Amount =
			matchLinkForInvNYY.AP_Amount =
			matchLinkForInvYNN.AP_Amount =
			matchLinkForInvYNY.AP_Amount =
			matchLinkForInvYYN.AP_Amount =
			matchLinkForInvYYY.AP_Amount = 20M;
			matchLinkforInvYYYCancelled.AP_Amount = -160M;

			matchLinkForInvNNN.AP_AH = invNNN.PK;
			matchLinkForInvNNY.AP_AH = invNNY.PK;
			matchLinkForInvNYN.AP_AH = invNYN.PK;
			matchLinkForInvNYY.AP_AH = invNYY.PK;
			matchLinkForInvYNN.AP_AH = invYNN.PK;
			matchLinkForInvYNY.AP_AH = invYNY.PK;
			matchLinkForInvYYN.AP_AH = invYYN.PK;
			matchLinkForInvYYY.AP_AH = invYYY.PK;
			matchLinkforInvYYYCancelled.AP_AH = invYYYCancelled.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(matchGroup);

			Factory.Save();

			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 1, 0, 0, 1);
			AssertEquals("No emails should be sent out", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			EDIMessage[] createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.OfflinePayment));
			AssertEquals("Should have created 1 message", 1, createdMessages.Length);
			AssertEDIMessage(createdMessages[0], 1, "OFP", true, String.Format(@""), invYYY.AH_GB, invYYY.AH_GE, invYYY.PK);
			createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.Response));
			AssertEquals("Should have created 1 message", 1, createdMessages.Length);
			AssertEDIMessage(createdMessages[0], 2, "RES", true, String.Format(@"<?xml version=""1.0""?>
<Response_OfflinePaymentNotification xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""https://enettlogistics.com/"">
  <success>true</success>
  <processedDateTime></processedDateTime>
</Response_OfflinePaymentNotification>"), invYYY.AH_GB, invYYY.AH_GE, invYYY.PK);
		}

		public void TestProcessOfflinePayment_InvoiceWasNotSent()
		{
			InvoicingBase inv;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "00008", eNettRegisteredOrg.PK);
			}

			Factory.Save();
			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 1, 0, 0, 0);
			AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.eNett);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "ENE");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, AccTransactionHeader.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, inv.PK);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
			const string indexName = "NR_RX__EM_LinkUniqueID"; // there is no existing constant for this index name
			query.TableIndexHints.Add(new TableIndexHint(indexName));

			eNettEDIMessage message = Factory.LoadTop1<eNettEDIMessage>(query);
			AssertNotNull("Sent Message", message);
			message.EM_Status = EDIMessage.Status.Failed;

			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink transactionMatchLinkmatchLinkForInv = matchGroup.AddNew();
			transactionMatchLinkmatchLinkForInv.AP_AH = inv.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(transactionMatchLinkmatchLinkForInv);
			inv.AH_Desc = "just to pass matchlink critical validation";

			Factory.Save();

			DbConnection conn = Db.Connection;
			conn.BeginTransaction();
			try
			{
				MockENettWebService.Instance.ResetProperties();
				ProcessLogs();

				AssertCountOfMethodCalls(0, 0, 0, 0, 0, 0);
				AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				conn.RollbackTransaction();
			}

			message.EM_Status = EDIMessage.Status.Sent;

			Factory.Save();

			MockENettWebService.Instance.ResetProperties();
			ProcessLogs();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 0, 0, 0, 1);
			AssertEquals("No emails should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation

		void AssertCountOfMethodCalls(int countGetNewInvoicesWasInvoked, int countGetNewPaymentsWasInvoked, int countCreateInvoiceWasInvoked, int countCancelInvoiceWasInvoked, int countProcessDirectDebitWasInvoked, int countOfflinePaymentNotificationWasInvoked)
		{
			AssertEquals("Number of times GetNewInvoices() was invoked", countGetNewInvoicesWasInvoked, MockENettWebService.Instance.CountGetNewInvoicesWasInvoked);
			AssertEquals("Number of times GetNewPayments() was invoked", countGetNewPaymentsWasInvoked, MockENettWebService.Instance.CountGetNewPaymentsWasInvoked);
			AssertEquals("Number of times CreateInvoice() was invoked", countCreateInvoiceWasInvoked, MockENettWebService.Instance.CountCreateInvoiceWasInvoked);
			AssertEquals("Number of times CancelInvoice() was invoked", countCancelInvoiceWasInvoked, MockENettWebService.Instance.CountCancelInvoiceWasInvoked);
			AssertEquals("Number of times ProcessDirectDebit() was invoked", countProcessDirectDebitWasInvoked, MockENettWebService.Instance.CountProcessDirectDebitWasInvoked);
			AssertEquals("Number of times OfflinePaymentNotification() was invoked", countOfflinePaymentNotificationWasInvoked, MockENettWebService.Instance.CountOfflinePaymentNotificationWasInvoked);
		}

		void AssertEDIMessage(EDIMessage message, int messageNumber, string messageSubType, bool isSuccess, string xml, ZGuid branchPK, ZGuid departmentPK, ZGuid transactionPK)
		{
			string prefix = String.Format("Message{0}.", messageNumber);
			AssertEquals(prefix + "EM_ApplicationCode", EDIMessage.ApplicationCodes.eNett, message.EM_ApplicationCode);
			AssertEquals(prefix + "EM_MessageType", "ENE", message.EM_MessageType);
			AssertEquals(prefix + "EM_MessageSubType", messageSubType, message.EM_MessageSubType);
			AssertEquals(prefix + "EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(prefix + "EM_Status", isSuccess ? EDIMessage.Status.Sent : EDIMessage.Status.Failed, message.EM_Status);
			if (!string.IsNullOrEmpty(xml))
			{
				this.AssertXMLEqualsByDiff(prefix + "EM_MessageText", xml, Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(
					message.EM_MessageText.ToString(),
					"<Date>.*</Date>", string.Empty),
					"<InvoiceDate>.*</InvoiceDate>", string.Empty),
					"<PostDate>.*</PostDate>", string.Empty),
					"<DueDate>.*</DueDate>", string.Empty),
					"<Data>.*</Data>", string.Empty),
					"<processedDateTime>.*</processedDateTime>", "<processedDateTime></processedDateTime>"));
			}
			AssertEquals(prefix + "EM_GB", branchPK, message.EM_GB);
			AssertEquals(prefix + "EM_GE", departmentPK, message.EM_GE);
			AssertEquals(prefix + "EM_LinkTable", AccTransactionHeader.Schema.TableName, message.EM_LinkTable);
			AssertEquals(prefix + "EM_LinkUniqueID", transactionPK, message.EM_LinkUniqueID);
		}

		InvoicingBase CreateTransaction(Type transactionType, string aH_TransactionNum, ZGuid aH_OH)
		{
			return CreateTransaction(transactionType, aH_TransactionNum, aH_OH, false);
		}

		InvoicingBase CreateTransaction(Type transactionType, string aH_TransactionNum, ZGuid aH_OH, bool createJob)
		{
			InvoicingBase result = TestObjectCreator.CreateInvoice(transactionType, TestObjectCreator.AUD, 1);
			result.AH_TransactionNum = aH_TransactionNum;
			result.AH_OH = aH_OH;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(result, TestObjectCreator.AUD, 1, 10);
			line.AL_AC = TestObjectCreator.CC1.PK;
			if (createJob)
			{
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				line.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			}
			result.Factory.Save();
			return result;
		}

		Payment CreatePayment(Type paymentType, string aH_TransactionNum, ZGuid aH_OH, ZGuid aH_AB, ZDecimal amount)
		{
			Payment result = (Payment)Factory.New(paymentType);
			result.AH_OH = aH_OH;
			result.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			result.AH_AB = aH_AB;
			result.AH_OSExTaxAmount = amount;
			result.AH_TransactionNum = aH_TransactionNum;
			return result;
		}

		TestDirectDebitData TestProcessDirectDebit(Action<object, eNettBatchingUpdateEventArgs> eNettEventHandler = null)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "";
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			APInvoice inv1, inv2;
			APPayment pay;
			AccBankAccount acc = TestObjectCreator.CreateBankAccount("BANKACC", "Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			acc.AB_BSB = "123-123";
			acc.AB_AccountNum = "123123132";
			acc.AB_GC = eNettCompanyBranch.GB_GC;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv1 = (APInvoice)CreateTransaction(typeof(APInvoice), "00010", eNettRegisteredOrg.PK);
				inv2 = (APInvoice)CreateTransaction(typeof(APInvoice), "00020", eNettRegisteredOrg.PK);
				pay = (APPayment)CreatePayment(typeof(APPayment), "20001", eNettRegisteredOrg.PK, acc.PK, -20);
			}

			APMatchingBase matching = new APMatchingBase(Factory, pay);

			((IMatching)inv1).OSPartialPaymentAmount = 10;
			matching.UnmatchedTransactions.Add(inv1);
			((IMatching)inv2).OSPartialPaymentAmount = 10;
			matching.UnmatchedTransactions.Add(inv2);
			matching.MoveAllFromUnmatchToMatch();
			matching.MatchAndClearTransactions();

			ZGuid batchPK = CreateBatchRecord(ZDateTime.Today, acc.PK);

			Factory.Save();

			if (eNettEventHandler != null)
			{
				eNettOutboundTransactionSubscriber.SeteNettBeforeBatchingCommit(eNettEventHandler);
			}

			ProcessLogs();

			Factory.Save();

			FactorySaveAlerterTestFailureAlternatives(false);
			AssertCountOfMethodCalls(0, 0, 0, 0, 1, 0);
			EDIMessage[] createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.ProcessDirectDebit));
			AssertEquals("Should have created 1 message", 1, createdMessages.Length);
			var dateTimeOffset = new DateTimeOffset(TestDateAttribute.Date).Offset;
			AssertEDIMessage(createdMessages[0], 1, "PDD", true, String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>

    <XmlType>Verbose</XmlType>
    <Source>
      <CompanyCode>ABC</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>username</LoginName>
      <LoginUserEmailAddress>A@B.COM</LoginUserEmailAddress>
    </Source>
    <Target />
    <EDIOrganisation EDICode=""ZABCPROXY"" OwnerCode=""ZABCPROXY"">
      <OrganisationDetails>
        <Name>Test Company Name</Name>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>184 Bourke Road</AddressLine1>
            <AddressCode>184 Bourke Road</AddressCode>
            <CityOrSuburb>Alexandria</CityOrSuburb>
            <StateOrProvince>NSW</StateOrProvince>
            <Language>EN</Language>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
        <RegistrationNumbers>
          <RegistrationNumber>
            <CountryOfRegistration>AU</CountryOfRegistration>
            <NumberType>ENE</NumberType>
            <Number>201654</Number>
          </RegistrationNumber>
        </RegistrationNumbers>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <FinancialTransactions>
      <FinancialInvoice>
        <Ledger>AP</Ledger>
        <DebtorOrCreditor EDICode=""ZVALID"" OwnerCode=""ZVALID"">
          <OrganisationDetails>
            <Name>Test Company Name</Name>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>184 Bourke Road</AddressLine1>
                <AddressCode>184 Bourke Road</AddressCode>
                <CityOrSuburb>Alexandria</CityOrSuburb>
                <StateOrProvince>NSW</StateOrProvince>
                <Language>EN</Language>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
            <RegistrationNumbers>
              <RegistrationNumber>
                <CountryOfRegistration>AU</CountryOfRegistration>
                <NumberType>ENE</NumberType>
                <Number>201654</Number>
              </RegistrationNumber>
            </RegistrationNumbers>
          </OrganisationDetails>
        </DebtorOrCreditor>
        <TxnType>PAY</TxnType>
        <Description>AP PAYMENT</Description>


        <Branch>BR1</Branch>
        <Department>BRN</Department>
        <LocalInvoiceAmtExclTax CurrencyCode=""AUD"">-20.00</LocalInvoiceAmtExclTax>
        <LocalInvoiceAmtInclTax CurrencyCode=""AUD"">-20.00</LocalInvoiceAmtInclTax>
        <OsInvoiceAmtExclTax CurrencyCode=""AUD"">-20.00</OsInvoiceAmtExclTax>
        <OsInvoiceAmtInclTax CurrencyCode=""AUD"">-20.00</OsInvoiceAmtInclTax>
        <BankCode>BANKACC</BankCode>
        <ReceiptPaymentType>END</ReceiptPaymentType>
        <CreatedUserId>username</CreatedUserId>
      </FinancialInvoice>
    </FinancialTransactions>
  </Payload>
</XmlInterchange>"), pay.AH_GB, pay.AH_GE, pay.PK);
			createdMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.Response));
			AssertEquals("Should have created 1 message", 1, createdMessages.Length);
			AssertEDIMessage(createdMessages[0], 2, "RES", true, String.Format(@"<?xml version=""1.0""?>
<Response_ProcessDirectDebit xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""https://enettlogistics.com/"">
  <success>true</success>
  <authorised>false</authorised>
  <processedDateTime></processedDateTime>
  <batchDate>2050-01-01T00:00:00{0}:{1}</batchDate>
</Response_ProcessDirectDebit>",
dateTimeOffset.Hours.ToString("+00;-0#"),
dateTimeOffset.Minutes.ToString("00")), pay.AH_GB, pay.AH_GE, pay.PK);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ZDateTime batchDate = ZDateTime.Today;
			ZQuery findBatchHeaderQuery = new ZQuery();
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.eNettDirectDebit);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.EqualToDatePartOnly, batchDate);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_DateClearedInCashbook, null);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_AB, acc.PK);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, ZBool.False);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, eNettCompany.PK);
			findBatchHeaderQuery.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + " DESC ";

			var directDebitBatchHeader = newFactory.LoadTop1<DirectDebitBatchHeader>(findBatchHeaderQuery);
			var payment = newFactory.Load<Payment>(pay.PK);

			var testDirectDebitData = new TestDirectDebitData()
			{
				DirectDebitBatchHeaderRecord = directDebitBatchHeader,
				PaymentRecord = payment
			};
			return testDirectDebitData;
		}

		ZGuid CreateBatchRecord(ZDateTime batchDate, ZGuid accPK)
		{
			var ddrBatch = Factory.New<DirectDebitBatchHeader>();
			ddrBatch.AH_InvoiceDate = batchDate;
			ddrBatch.AH_PostDate = batchDate;
			ddrBatch.AH_AB = accPK;
			ddrBatch.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			ddrBatch.AH_Desc = GetDescriptionForCompayBatchDate(batchDate);
			ddrBatch.AH_GC = eNettCompany.PK;
			ddrBatch.AH_InvoiceAmount = 100;
			ddrBatch.AH_OSTotal = 100;
			ddrBatch.IsCancelled = false;
			Factory.Save();
			return ddrBatch.PK;
		}

		string GetDescriptionForCompayBatchDate(ZDateTime batchDate)
		{
			string dayOfTheMonth = (batchDate.IsValid && !batchDate.IsEmpty) ? batchDate.Day.ToString("00") : "  ";
			string comPayRegistrationNumber = AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.Left(50);
			string date = (batchDate.IsValid && !batchDate.IsEmpty) ? batchDate.ToString("ddMMyyyy") : "";
			return string.Format("ENI{0}-{1} CREDIT-{2}", dayOfTheMonth, comPayRegistrationNumber, date);  // Compay batch date description should not be translated
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		void FactorySaveAlerterTestFailureAlternatives(bool hasSendEmail)
		{
			if (hasSendEmail)
			{
				AssertEquals(LogSubscriber.EnettWebServiceFailedEmail_ForTestOnly.IsAutoFactorySave_ForTest, false);
				AssertEquals(LogSubscriber.EnettWebServiceFailedEmail_ForTestOnly.Factory_ForTest, LogSubscriber.Factory_ForTest);
			}
			else
			{
				AssertNull(LogSubscriber.EnettWebServiceFailedEmail_ForTestOnly);
			}

			AssertEquals(LogSubscriber.EnettWebServiceWrapper_ForTestOnly.IsAutoFactorySave_ForTest, false);
			AssertEquals(LogSubscriber.EnettWebServiceWrapper_ForTestOnly.Factory_ForTest, LogSubscriber.Factory_ForTest);
		}

		GlbCompany eNettCompany;
		GlbBranch eNettCompanyBranch;
		OrgHeader eNettRegisteredOrg;
		OrgHeader eNettRegisteredOrg2;
		OrgHeader invalidENettOrg;

		internal void ProcessLogs()
		{
			RunLogWalkerCycleForTest();
		}

		protected override void SetUp()
		{
			base.SetUp();
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			eNettCompany = TestObjectCreator.CreateNewCompany("ABC");
			Factory.Save();
			OrgHeader companyProxy = TestObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
			eNettCompany.GC_OH_OrgProxy = companyProxy.PK;
			eNettCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			Factory.Save();

			eNettCompanyBranch = TestObjectCreator.CreateNewBranch(eNettCompany, "BR1");
			eNettRegisteredOrg = TestObjectCreator.CreateOrgHeader("VALID", true, true);
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			eNettRegisteredOrg2 = TestObjectCreator.CreateOrgHeader("VALID2", true, true);
			eNettRegisteredOrg2.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201655");
			invalidENettOrg = TestObjectCreator.CreateOrgHeader("INVALID", true, true);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			}

			MockENettWebService.ClearInstance();
			MockENettWebService.Instance.SetupForTesting("CARGOWISE");

			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.ENettNotificationsGroup.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", OrganisationPK = companyProxy.PK });
			eNettWebServiceWrapper.UseRealWebService_ForTesting = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		#endregion
	}
}
