using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.com.enett991.Testing
{
	sealed class eNettInboundTransactionProcessorTest : TestCaseWithFactory
	{
		public void TestExtensionMethodGivesYouCorrectDateKindBasedOnRegistrySetting()
		{
			DateTime utcTime = ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime();

			AssertEquals("Precondition: registry setting default should be false", false, AccountingConfigurationRegistry.Instance.IncludeTimeZoneInformationForLastUpdateDate.Value);
			DateTime withoutRegistryOverride = utcTime.FromUTCToMelbourneLocalTime();
			AssertEquals("DateTimeKind should be Unspecified", DateTimeKind.Unspecified, withoutRegistryOverride.Kind);

			using (AccountingConfigurationRegistry.Instance.IncludeTimeZoneInformationForLastUpdateDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("registry setting value is now overridden", true, AccountingConfigurationRegistry.Instance.IncludeTimeZoneInformationForLastUpdateDate.Value);
				DateTime withRegistryOverride = utcTime.FromUTCToMelbourneLocalTime();
				AssertEquals("DateTimeKind should be Local", DateTimeKind.Local, withRegistryOverride.Kind);
				AssertEquals("should be the same datetime value with and without the registry setting", withoutRegistryOverride, withRegistryOverride);
			}
		}

		[TestDate(2008, 06, 01)]
		public void TestNewAPInvoicesDoNotTryToSaveWithCriticalValidationErrors()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var invoices = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
				invoices.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				AssertEquals("Precondition: no existing emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Precondition: there should be no invoices", 0, Factory.Load<APInvoice>(invoices).Length);
				AssertEquals("Precondition: ENettGetLastInvoiceDate should be date set by test'",
					ZDateTime.Now.Date,
					AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value.Date);

				var creditor = TestObjectCreator.ABIGAS;
				TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);

				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoiceDoc = new XmlDocument();
				invoiceDoc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>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Rm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>FRT</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");

				var getNewReceiptsResponse = new Response_GetNewPayments { success = true, receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime() };
				var getNewInvoicesResponse = new Response_GetNewInvoices { success = true, updates = invoiceDoc.DocumentElement, receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime() };

				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				var createdInvoices = Factory.Load<InvoicingBase>(invoices);
				AssertEquals("Expected to have created an invoice and not a critical validation exception because there is no job but is importing FRT charge code", 1, createdInvoices.Length);
				AssertEquals("Expected to have imported the line on the invoice as well", 1, createdInvoices[0].Lines.Count);

				mockWebServiceClient.VerifyAll();
			}
		}

		[TestDate(2008, 12, 01)]
		public void TestInactiveBranchesAreNotUsed()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(UAInvoice)));
			var globalCompanies = Factory.Load<GlbCompany>(new ZQuery());
			var companiesAndOrgProxies = globalCompanies.ToDictionary(company => company, company => company.GC_OH_OrgProxy);

			try
			{
				foreach (GlbCompany company in globalCompanies)
				{
					company.GC_OH_OrgProxy = ZGuid.Empty;
				}

				GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
				newCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				newCompany.Branches.DeleteAll();
				GlbBranch inactive = newCompany.Branches.AddNew();
				inactive.FillWithValidTestData();
				inactive.GB_IsActive = false;
				GlbBranch active = newCompany.Branches.AddNew();
				active.FillWithValidTestData();
				active.GB_IsActive = true;

				new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(2008, newCompany.PK);

				Factory.Save();

				using (new TemporaryUserContext { BranchPK = active.PK.ToGuid() }.Set())
				{
					OrgHeader creditor = TestObjectCreator.ABIGAS;
					TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
					creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
					creditor.CompanyData.OB_IsCreditor = true;
					TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				}

				Factory.Save();

				AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode());
				AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = TestObjectCreator.AALSHI.PK });

				Factory.Save();

				var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
				using (ObjectFactory.Substitute(mockWebServiceClient.Object))
				{
					var processorForTesting = new eNettInboundTransactionProcessor();

					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					var getNewInvoicesResponse = new Response_GetNewInvoices { success = true };
					var doc = new XmlDocument();
					doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
					getNewInvoicesResponse.updates = doc.DocumentElement;
					getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

					var getNewReceiptsResponse = new Response_GetNewPayments { success = true };
					var doc2 = new XmlDocument();
					doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
					getNewReceiptsResponse.updates = doc2.DocumentElement;
					getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

					mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
					mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
					mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
					mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
					processorForTesting.Process(Notifications);
				}

				AssertEquals("Should have loaded a correct (active) branch and created an UAInvoice", 1, Factory.GetDatabaseCount(typeof(UAInvoice)));
				mockWebServiceClient.VerifyAll();
			}
			finally
			{
				foreach (var pair in companiesAndOrgProxies)
				{
					pair.Key.GC_OH_OrgProxy = pair.Value;
				}

				Factory.Save();
			}
		}

		[TestDate(2012, 11, 07)]
		public void TestHighWatermarkResponseDateReceivedAndSentInSameTimeZone()
		{
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode());

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN").PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = TestObjectCreator.AALSHI.PK });

				Factory.Save();

				new eNettInboundTransactionProcessorForTesting().Process(Notifications);

				AssertEquals("High watermark date from service got saved as UTC", ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime(), AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value);

				DateTime expectedHighwatermarkDateAdjustedToMelbourneLocalTime = ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime().FromUTCToMelbourneLocalTime();
				AssertEquals("DateTime Kind should be unspecified to make sure we don't have problems with automatic datetime conversion.", DateTimeKind.Unspecified, expectedHighwatermarkDateAdjustedToMelbourneLocalTime.Kind);

				var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
				using (ObjectFactory.Substitute(mockWebServiceClient.Object))
				{
					var processorForTesting = new eNettInboundTransactionProcessor("CARGOWISE");
					var integratorKey = AccountingConfigurationRegistry.Instance.ENettIntegratorKey.Value;

					var getNewInvoicesResponse = new Response_GetNewInvoices();
					getNewInvoicesResponse.success = true;
					var doc = new XmlDocument();
					doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
					getNewInvoicesResponse.updates = doc.DocumentElement;
					getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

					var getNewReceiptsResponse = new Response_GetNewPayments();
					getNewReceiptsResponse.success = true;
					var doc2 = new XmlDocument();
					doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
					getNewReceiptsResponse.updates = doc2.DocumentElement;
					getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

					mockWebServiceClient
						.Setup(m => m.GetNewInvoices(processorForTesting.Integrator, integratorKey, AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode, expectedHighwatermarkDateAdjustedToMelbourneLocalTime, 1, 1, AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode))
						.Returns(getNewInvoicesResponse);

					mockWebServiceClient
						.Setup(m => m.GetNewPayments(processorForTesting.Integrator, integratorKey, AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode, expectedHighwatermarkDateAdjustedToMelbourneLocalTime, 1, 1, AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode))
						.Returns(getNewReceiptsResponse);
					mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

					processorForTesting.Process(Notifications);
					mockWebServiceClient.VerifyAll();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 12, 01)]
		public void TestInboundInvoicesAndReceiptsAndCancelledInvoices()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var invoices = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
				invoices.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				var receipts = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
				receipts.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no invoices", 0, Factory.Load<APInvoice>(invoices).Length);
				AssertEquals("Prerequisite: there should be no receipts", 0, Factory.Load<ARReceipt>(receipts).Length);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("Prerequisite: value of ENettGetLastInvoiceDate - should be 'Today's Date'", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value.Date);
				AssertEquals("Prerequisite: value of ENettGetLastPaymentDate - should be 'Today's Date'", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value.Date);

				var creditor = TestObjectCreator.ABIGAS;
				TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>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Rm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				var doc2 = new XmlDocument();
				doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getCancelledInvoicesResponce = new Response_GetCancelledInvoices();
				getCancelledInvoicesResponce.success = true;
				var doc3 = new XmlDocument();
				doc3.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>JVBERi0xLjMKJeLjz9MKMiAwIG9iago8PAovQ3JlYXRpb25EYXRlIChEOjIwMDkwNTI3MTEzODI0KzAzJzAwJykKL01vZERhdGUgKEQ6MjAwOTA1MjcxMTM4MjQrMDMnMDAnKQovUHJvZHVjZXIgKEJDTCBlYXN5UERGIDYuMDAgXCgwMzIwXCkpCi9DcmVhdG9yIChOaXRyb1BERiA2LjApCj4+CmVuZG9iagoKMSAwIG9iago8PAovQ291bnQgMAo+PgplbmRvYmoKCjMgMCBvYmoKPDwKL1R5cGUgL0NhdGFsb2cKL1BhZ2VzIDQgMCBSCj4+CmVuZG9iagoKOSAwIG9iago8PCAvVHlwZSAvQW5ub3QKL1N1YnR5cGUgL0xpbmsKL1JlY3QgWzI5NSAzOSA0MDIgMjldCi9Cb3JkZXIgWzAgMCAwXQovQSA8PCAvUyAvVVJJIC9VUkkgKGh0dHA6Ly93d3cubml0cm9wZGYuY29tLykgPj4KPj4KZW5kb2JqCgoxMCAwIG9iago8PAovTGVuZ3RoIDE4NjEKL0ZpbHRlciAvRmxhdGVEZWNvZGUKPj4Kc3RyZWFtCnjadVfJbh03ELy/r5ijAyRjNofDJccgyy3Iolvgg/AsL4EVOZIC/X66qprzXgwEBvymRLK7Wb0y21hqG2vty/0pW123utRuay5LtrKm7ijjJ5utI0909r1pHfVYTWPtnSdbdtQhEMgy925rcdw6jrgW/3RNOT43fGIXgEFOtn2txVHSrn3FWu34Iza6TqxmmumqiQp+cqpXyPemTLOFbQwIxEk3xMbO67qWiivZqLxwbWvB3roCFP/fRqM9NcM67OwEIdIl1iRbjBL3BnTRvu8iaVsbVvnHAyXJnLhAHgCuXJouZeS6lDVra9JWW3fsTfhz3QZ0H6hBifxUJ/wE6GBbK3nzK+UBRfCYH8p+WdndyYQxMMApnGu7FOzr7hLpWRuFdhpkXwCC4Aq7M0m2/7Vag81gjQo6RIoo5xwKSVRB4NScdL9AWW46MPmatOWDYqjMFSclF8bmrlXTySHPdVqUwQOusk/Y4fENhGwJgWuD8VI3U+zbYJQCO6nWBxnewbv1ShXuGsPFrG+MiLIz4jrl1cJrW2tXyPe2HaE0F52OUmG9PrfQDbB1ktjorroVxqunFGLUTaYPzfOqzJubx7iJJY9/qwwrMLpTcd2g2J3hJ60aF91rGSiBBzm7MGLka/8G7Z7Z7hTzIMYtvWJ0oSG/F5q8K/29usDOXQx5IAAwp6GaCQNLEBfyvFUlqIclTQkU/gq4Ib7N0xTmbBk/kNLpEWWrZ2plfsCXfj3QmXm1ImZ5OzcFTi07TaRj5DQ3P5eJQAODa64WFkDkpl/ESiJ57v7Gve6potz02DLPwnYJlUDuIXnhgjftpY921J8tYjMKqsHlJBS+3egDpB3scXRJT88Rck2XZaQ9fOR2OqW1HxI8XbLkkS/ETDlS1fnOclEl6MrTIUMSC5xHEm1mQaq5HZ9ujUgLXBKvw7oOzgwyWZsCQb2K/1zNEtSJ3Ar4DAbTVFMYIw1NIa5QsiT6SMOYFWooYp2fUa8C1kHEa+P2plrEcPUioW9XMeLyLQz1clLLjGSvNLGIOEjKqBYRmBSrxgZoiSZG+pmZfNtFvqmBbTQQ91WoWZ5gLzIzoKcBipmpljTSDCFoAq1Mmna6u/GGQAjZ1kRpmc1ZYhOugwYN4/xWbJFM5aFIaJHXXt7ZWrXG1giZZX53U12bkNf1uorbdqb0YCLUHt50P2CiaNjhTsAKu9PwGWWkcA6LrRdgyBr0m+QbGp+DIhqTQsxF4saJVWww0OA2v8WIacKv6IC9E7d1gS1xILDEAGwJ1YP7ViC5IrEDtKQUmWjEIBHYPduPyGjGRgWp/tNs19SD0ABkFQH3blRLzDV07jI1wonc0xRL5uLyFSr4CcS8oM65ynlk2u5VBAdL3N/TDQYpKJHgvqcZpxJkbZN1vJeXVy6aqhnnryYzJzo4mNh9OxO+iQmUGwSV0gkWqCMMdbVc6TYkCj/3iOEsV1cJQK4NdefMc4gVjnyzuiiOwg7EGIzkGFj7Hs3U6zRjn2OhFRWXFn0KkRQA7aEpabQYvSNSqBYFf1ekVI6Ena3AmiaQzpFttmwhNGmNVrHqP6aTftBTBAfbJLSPOUFjHilTn88th+Y5/BHO4bMnTXiBNhWvA3OYwmTEtKxzOiviN6Y8lj/kFgdj1YShitwjfExdubPLNtUNbFUGwROD4ReIQ7SyLevNsEF1U+pgAB3KyyFrVcDG0N4s1LVXPRczer/e2zBIGysFogjDoqm0j00VDA8eEDk4EHtKIo5UofD64cFN1kJQ6sdqPDrgCBqr+IvnicadjoIOjXxEcWjOl9cW9h3g0+kDUxbVrJGO+wtO6m4smLWSEJQC0FODHvSQelm0yymbArmJHQEJpP2q3FPYLOtT1VwNM+LgtZHnL4zmNbImOs8+X7iPtGWiYgThfBYF30wk9ujf6koMfkhu6jyoHziZZqaiNGMiZCyzGlxrPH9hwaS20bPkdWPKeTAj500PBvUyULHlicRr6ZdVVjog1urBnpJni41FW8m7ziX125CaJmmhM1Z1qojdsPN8bTTuMIaCij31/gp6OfHBZp9M9gtqHNIU+SgJ6NJN798eHbxfr43JMMaea3Xn/2oXpZsSgLPx/QVrAGfDUHbgO/Nb1TnGnVjTe3meCrTFY+CCWz7mpGA/pJaozqEw1g7bWl7+a+n5C8txl3env09/bG8WW956R2g+pi2b9eUbfyM83p3Sgn+//XT6/fTHG//CHleVfE/1PQl7zM9iwnh878K+u4kzjl7/6K+JtNy888LYF39a3Lw9vbr58PFp+Xz7/m55uX1azo93t893b5d/nj7+9X75arn500/VOPXq54/Pjw+/fP9j/H1Ke7U8P368/bQ8Pbx7frl9vFu5/sPN/yhHudqXLUn9w/L5n8fzh9unu6+X9w/L84O04pTh3KsPz8+fv339+uXlZf0L+j+/fbeeH+5fH9so3rVl7z15YGzx6++TLRMPv57+BYcak8YKZW5kc3RyZWFtCmVuZG9iagoKNiAwIG9iago8PAovVHlwZSAvUGFnZQovUGFyZW50IDQgMCBSCi9NZWRpYUJveCBbIDAgMCA2MTIgNzkyIF0KL1Jlc291cmNlcyA8PAovRm9udCA8PAovRjUgNyAwIFIKL0Y2IDggMCBSCj4+Ci9Qcm9jU2V0IFsvUERGIC9UZXh0IC9JbWFnZUIgL0ltYWdlQ10gPj4KL0NvbnRlbnRzIDEwIDAgUgovQW5ub3RzIFsKOSAwIFIKXQo+PgplbmRvYmoKCjQgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFsKNiAwIFIKXQovQ291bnQgMQo+PgplbmRvYmoKCjcgMCBvYmoKPDwKL1R5cGUgL0ZvbnQgL1N1YnR5cGUgL1R5cGUxIC9CYXNlRm9udCAvSGVsdmV0aWNhIC9FbmNvZGluZyAvV2luQW5zaUVuY29kaW5nCj4+CmVuZG9iagoKOCAwIG9iago8PAovVHlwZSAvRm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getCancelledInvoicesResponce.updates = doc3.DocumentElement;
				getCancelledInvoicesResponce.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getCancelledInvoicesResponce);

				new eNettInboundTransactionProcessor().Process(Notifications);

				var createdInvoices = Factory.Load<InvoicingBase>(invoices);
				Receipt[] createdReceipts = Factory.Load<ARReceipt>(receipts);
				AssertEquals("Should have created 1 invoice", 1, createdInvoices.Length);
				var invoice = createdInvoices[0];

				if (invoice.IsCancelled && invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					AssertEquals("Cancelled UnapprovedPayableTransactions should have 0 line", 0, invoice.Lines.Count);
				}
				else
				{
					AssertEquals("New invoice should have 1 line", 1, invoice.Lines.Count);
				}

				AssertEquals("New invoice should have 1 eDoc attached", 1, invoice.DocManagerInfo.AllEDocs.Count);
				AssertFileSameAsBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf", invoice.DocManagerInfo.AllEDocs[0].ImageData);

				AssertEquals("Should have created 1 receipt", 1, createdReceipts.Length);
				AssertEquals("Should have created 2 messages", 3, Factory.Load<EDIMessage>(new ZQuery()).Length);
				var msg1 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewInvoices));
				var msg2 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewPayments));
				var msg3 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewCancellations));
				AssertEDIMessage(msg1, 1, eNettMessageSubTypeList.Codes.GetNewInvoices, true, @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>", new ZGuid("27a55065-ac88-4ec3-8bed-e575e79172cb"), GlbDepartment.CurrentDepartment.PK, createdInvoices[0].PK);
				AssertEDIMessage(msg2, 2, eNettMessageSubTypeList.Codes.GetNewPayments, true, @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions /></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>", new ZGuid("27a55065-ac88-4ec3-8bed-e575e79172cb"), GlbDepartment.CurrentDepartment.PK, createdReceipts[0].PK);
				AssertEDIMessage(msg3, 3, eNettMessageSubTypeList.Codes.GetNewCancellations, true, @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>", new ZGuid("27a55065-ac88-4ec3-8bed-e575e79172cb"), GlbDepartment.CurrentDepartment.PK, createdInvoices[0].PK);
				AssertEquals("Value of ENettGetLastInvoiceDate", ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime(), AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value);
				AssertEquals("Value of ENettGetLastPaymentDate", ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime(), AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value);
				AssertEquals("Value of ENettGetLastCancelledInvoicesDate", ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime(), AccountingConfigurationRegistry.Instance.ENettGetLastCancelledInvoicesDate.Value);
				mockWebServiceClient.VerifyAll();
			}
		}

		[TestDate(2009, 12, 01)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInboundInvoicesConversionToAPandUnapproved()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var uAInvoicesQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
				uAInvoicesQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				var invoicesQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				invoicesQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

				var chargeCode = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);
				chargeCode.AC_DepartmentFilterList += ", " + GlbDepartment.CurrentDepartment.GE_Code;

				AssertEquals("Prerequisite: there should be no UAInvoices", 0, Factory.Load<APInvoice>(uAInvoicesQuery).Length);
				AssertEquals("Prerequisite: there should be no Invoices", 0, Factory.Load<APInvoice>(invoicesQuery).Length);
				AssertNotNull("Prerequisite: there should be a FRT Charge Code", chargeCode);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("Prerequisite: value of ENettGetLastInvoiceDate - should be 'Today's Date'", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value.Date);

				OrgHeader creditor = TestObjectCreator.CreateOrgHeader("CREDITOR", true, false, "AUSYD");
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				creditor.CompanyData.SetAPTaxApplicable(true);
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				var department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_HouseBill = "H456";
				Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
				job.JH_JobNum = "123";
				job.JH_GE = department.PK;
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>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Rm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getNewPaymentsResponse = new Response_GetNewPayments();
				getNewPaymentsResponse.success = true;
				getNewPaymentsResponse.updates = null;
				getNewPaymentsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewPaymentsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>167</TxnNumber><JobInvoiceNo>233</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>JVBERi0xLjMKJeLjz9MKMiAwIG9iago8PAovQ3JlYXRpb25EYXRlIChEOjIwMDkwNTI3MTEzODI0KzAzJzAwJykKL01vZERhdGUgKEQ6MjAwOTA1MjcxMTM4MjQrMDMnMDAnKQovUHJvZHVjZXIgKEJDTCBlYXN5UERGIDYuMDAgXCgwMzIwXCkpCi9DcmVhdG9yIChOaXRyb1BERiA2LjApCj4+CmVuZG9iagoKMSAwIG9iago8PAovQ291bnQgMAo+PgplbmRvYmoKCjMgMCBvYmoKPDwKL1R5cGUgL0NhdGFsb2cKL1BhZ2VzIDQgMCBSCj4+CmVuZG9iagoKOSAwIG9iago8PCAvVHlwZSAvQW5ub3QKL1N1YnR5cGUgL0xpbmsKL1JlY3QgWzI5NSAzOSA0MDIgMjldCi9Cb3JkZXIgWzAgMCAwXQovQSA8PCAvUyAvVVJJIC9VUkkgKGh0dHA6Ly93d3cubml0cm9wZGYuY29tLykgPj4KPj4KZW5kb2JqCgoxMCAwIG9iago8PAovTGVuZ3RoIDE4NjEKL0ZpbHRlciAvRmxhdGVEZWNvZGUKPj4Kc3RyZWFtCnjadVfJbh03ELy/r5ijAyRjNofDJccgyy3Iolvgg/AsL4EVOZIC/X66qprzXgwEBvymRLK7Wb0y21hqG2vty/0pW123utRuay5LtrKm7ijjJ5utI0909r1pHfVYTWPtnSdbdtQhEMgy925rcdw6jrgW/3RNOT43fGIXgEFOtn2txVHSrn3FWu34Iza6TqxmmumqiQp+cqpXyPemTLOFbQwIxEk3xMbO67qWiivZqLxwbWvB3roCFP/fRqM9NcM67OwEIdIl1iRbjBL3BnTRvu8iaVsbVvnHAyXJnLhAHgCuXJouZeS6lDVra9JWW3fsTfhz3QZ0H6hBifxUJ/wE6GBbK3nzK+UBRfCYH8p+WdndyYQxMMApnGu7FOzr7hLpWRuFdhpkXwCC4Aq7M0m2/7Vag81gjQo6RIoo5xwKSVRB4NScdL9AWW46MPmatOWDYqjMFSclF8bmrlXTySHPdVqUwQOusk/Y4fENhGwJgWuD8VI3U+zbYJQCO6nWBxnewbv1ShXuGsPFrG+MiLIz4jrl1cJrW2tXyPe2HaE0F52OUmG9PrfQDbB1ktjorroVxqunFGLUTaYPzfOqzJubx7iJJY9/qwwrMLpTcd2g2J3hJ60aF91rGSiBBzm7MGLka/8G7Z7Z7hTzIMYtvWJ0oSG/F5q8K/29usDOXQx5IAAwp6GaCQNLEBfyvFUlqIclTQkU/gq4Ib7N0xTmbBk/kNLpEWWrZ2plfsCXfj3QmXm1ImZ5OzcFTi07TaRj5DQ3P5eJQAODa64WFkDkpl/ESiJ57v7Gve6potz02DLPwnYJlUDuIXnhgjftpY921J8tYjMKqsHlJBS+3egDpB3scXRJT88Rck2XZaQ9fOR2OqW1HxI8XbLkkS/ETDlS1fnOclEl6MrTIUMSC5xHEm1mQaq5HZ9ujUgLXBKvw7oOzgwyWZsCQb2K/1zNEtSJ3Ar4DAbTVFMYIw1NIa5QsiT6SMOYFWooYp2fUa8C1kHEa+P2plrEcPUioW9XMeLyLQz1clLLjGSvNLGIOEjKqBYRmBSrxgZoiSZG+pmZfNtFvqmBbTQQ91WoWZ5gLzIzoKcBipmpljTSDCFoAq1Mmna6u/GGQAjZ1kRpmc1ZYhOugwYN4/xWbJFM5aFIaJHXXt7ZWrXG1giZZX53U12bkNf1uorbdqb0YCLUHt50P2CiaNjhTsAKu9PwGWWkcA6LrRdgyBr0m+QbGp+DIhqTQsxF4saJVWww0OA2v8WIacKv6IC9E7d1gS1xILDEAGwJ1YP7ViC5IrEDtKQUmWjEIBHYPduPyGjGRgWp/tNs19SD0ABkFQH3blRLzDV07jI1wonc0xRL5uLyFSr4CcS8oM65ynlk2u5VBAdL3N/TDQYpKJHgvqcZpxJkbZN1vJeXVy6aqhnnryYzJzo4mNh9OxO+iQmUGwSV0gkWqCMMdbVc6TYkCj/3iOEsV1cJQK4NdefMc4gVjnyzuiiOwg7EGIzkGFj7Hs3U6zRjn2OhFRWXFn0KkRQA7aEpabQYvSNSqBYFf1ekVI6Ena3AmiaQzpFttmwhNGmNVrHqP6aTftBTBAfbJLSPOUFjHilTn88th+Y5/BHO4bMnTXiBNhWvA3OYwmTEtKxzOiviN6Y8lj/kFgdj1YShitwjfExdubPLNtUNbFUGwROD4ReIQ7SyLevNsEF1U+pgAB3KyyFrVcDG0N4s1LVXPRczer/e2zBIGysFogjDoqm0j00VDA8eEDk4EHtKIo5UofD64cFN1kJQ6sdqPDrgCBqr+IvnicadjoIOjXxEcWjOl9cW9h3g0+kDUxbVrJGO+wtO6m4smLWSEJQC0FODHvSQelm0yymbArmJHQEJpP2q3FPYLOtT1VwNM+LgtZHnL4zmNbImOs8+X7iPtGWiYgThfBYF30wk9ujf6koMfkhu6jyoHziZZqaiNGMiZCyzGlxrPH9hwaS20bPkdWPKeTAj500PBvUyULHlicRr6ZdVVjog1urBnpJni41FW8m7ziX125CaJmmhM1Z1qojdsPN8bTTuMIaCij31/gp6OfHBZp9M9gtqHNIU+SgJ6NJN798eHbxfr43JMMaea3Xn/2oXpZsSgLPx/QVrAGfDUHbgO/Nb1TnGnVjTe3meCrTFY+CCWz7mpGA/pJaozqEw1g7bWl7+a+n5C8txl3env09/bG8WW956R2g+pi2b9eUbfyM83p3Sgn+//XT6/fTHG//CHleVfE/1PQl7zM9iwnh878K+u4kzjl7/6K+JtNy888LYF39a3Lw9vbr58PFp+Xz7/m55uX1azo93t893b5d/nj7+9X75arn500/VOPXq54/Pjw+/fP9j/H1Ke7U8P368/bQ8Pbx7frl9vFu5/sPN/yhHudqXLUn9w/L5n8fzh9unu6+X9w/L84O04pTh3KsPz8+fv339+uXlZf0L+j+/fbeeH+5fH9so3rVl7z15YGzx6++TLRMPv57+BYcak8YKZW5kc3RyZWFtCmVuZG9iagoKNiAwIG9iago8PAovVHlwZSAvUGFnZQovUGFyZW50IDQgMCBSCi9NZWRpYUJveCBbIDAgMCA2MTIgNzkyIF0KL1Jlc291cmNlcyA8PAovRm9udCA8PAovRjUgNyAwIFIKL0Y2IDggMCBSCj4+Ci9Qcm9jU2V0IFsvUERGIC9UZXh0IC9JbWFnZUIgL0ltYWdlQ10gPj4KL0NvbnRlbnRzIDEwIDAgUgovQW5ub3RzIFsKOSAwIFIKXQo+PgplbmRvYmoKCjQgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFsKNiAwIFIKXQovQ291bnQgMQo+PgplbmRvYmoKCjcgMCBvYmoKPDwKL1R5cGUgL0ZvbnQgL1N1YnR5cGUgL1R5cGUxIC9CYXNlRm9udCAvSGVsdmV0aWNhIC9FbmNvZGluZyAvV2luQW5zaUVuY29kaW5nCj4+CmVuZG9iagoKOCAwIG9iago8PAovVHlwZSAvRm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>FRT</ChargeCode><Description /><ConsolOrJobNo>123</ConsolOrJobNo><ConsolOrJobType>SHP</ConsolOrJobType><MasterBillNo /><HouseBIllNo>H456</HouseBIllNo><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewPaymentsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				InvoicingBase[] createdInvoices = Factory.Load<InvoicingBase>(new ZQuery());
				AssertEquals("Should have created 2 invoices", 2, createdInvoices.Length);
				var invoice = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "165").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Invoice should be UAInvoice", invoice as UAInvoice);
				AssertEquals("New invoice should have 1 line", 1, invoice.Lines.Count);
				AssertEquals("New invoice should have 1 eDoc attached", 1, invoice.DocManagerInfo.AllEDocs.Count);
				AssertFileSameAsBytes(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\TransactionHeaderBuilder\Testing\Test Attachment.pdf", invoice.DocManagerInfo.AllEDocs[0].ImageData);

				var invoice2 = Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "167").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertNotNull("Invoice should be APInvoice", invoice2 as APInvoice);
				AssertEquals("Invoice Transaction Type should be Invoice", TransactionTypes.Invoice, invoice2.AH_TransactionType);
				AssertEquals("Invoice Ledger should be AP", LedgerTypes.AccountsPayable, invoice2.AH_Ledger);
				AssertEquals("New invoice2 should have 1 line", 1, invoice2.Lines.Count);

				AssertEquals("Should have created 2 messages", 2, Factory.Load<EDIMessage>(new ZQuery()).Length);
				var messageQuery = new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewInvoices);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, invoice.PK);
				var msg1 = Factory.LoadTop1<EDIMessage>(messageQuery);
				messageQuery = new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewInvoices);
				messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, invoice2.PK);
				var msg2 = Factory.LoadTop1<EDIMessage>(messageQuery);
				AssertEDIMessage(msg1, 1, eNettMessageSubTypeList.Codes.GetNewInvoices, true, @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>", new ZGuid("27a55065-ac88-4ec3-8bed-e575e79172cb"), GlbDepartment.CurrentDepartment.PK, invoice.PK);
				AssertEDIMessage(msg2, 2, eNettMessageSubTypeList.Codes.GetNewInvoices, true, @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>167</TxnNumber><JobInvoiceNo>233</JobInvoiceNo><Description /><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>FRT</ChargeCode><Description /><ConsolOrJobNo>123</ConsolOrJobNo><ConsolOrJobType>SHP</ConsolOrJobType><MasterBillNo /><HouseBIllNo>H456</HouseBIllNo><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>", new ZGuid("27a55065-ac88-4ec3-8bed-e575e79172cb"), GlbDepartment.CurrentDepartment.PK, invoice2.PK);
				AssertEquals("Value of ENettGetLastInvoiceDate", ZDateTime.BrettsBirthday.ToDateTime().ToUniversalTime(), AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value);
				mockWebServiceClient.VerifyAll();
			}
		}

		[TestDate(2009, 12, 30)]
		public void TestInboundReceiptsToAvoidDuplication()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var receipts = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
				receipts.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no receipts", 0, Factory.Load<ARReceipt>(receipts).Length);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("Prerequisite: value of ENettGetLastPaymentDate - should be 'Today's Date'", ZDateTime.Now.Date, AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value.Date);

				OrgHeader creditor = TestObjectCreator.ABIGAS;
				TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><Attachments><Attachment><FileName>Invoice 00001003.pdf</FileName><Data>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Rm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZCAvRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKCnhyZWYKMCAxMQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAxNzQgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjA1IDAwMDAwIG4gCjAwMDAwMDI1MjQgMDAwMDAgbiAKMDAwMDAwMDAwMCAwMDAwMCBuIAowMDAwMDAyMzI4IDAwMDAwIG4gCjAwMDAwMDI1ODQgMDAwMDAgbiAKMDAwMDAwMjY4MiAwMDAwMCBuIAowMDAwMDAwMjU1IDAwMDAwIG4gCjAwMDAwMDAzOTIgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSAxMQovUm9vdCAzIDAgUgovSW5mbyAyIDAgUgovSURbPGM1Mzk5YjU4MGM0ZWYxMWI2NmIyYzVhZWJjMzNmMWQxPjxjNTM5OWI1ODBjNGVmMTFiNjZiMmM1YWViYzMzZjFkMT5dCj4+CnN0YXJ0eHJlZgoyNzg1CiUlRU9GCg==</Data></Attachment></Attachments><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				var doc2 = new XmlDocument();
				doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				Receipt[] createdReceipts = Factory.Load<ARReceipt>(receipts);
				AssertEquals("Should have created 1 receipt", 1, createdReceipts.Length);
				AssertEquals("Should have created 2 messages", 4, Factory.Load<EDIMessage>(new ZQuery()).Length);
				EDIMessage[] messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewPayments));
				AssertEquals("Should have created 2 GetNewPayments EDIMessages", 2, messages.Length);
				string pattern = @"\<PRE\>
An error occurred during processing transaction\(s\) received from eNett.
Message: There was en error processing payment.
Message: Receipt with Company ID '12345678' already exists.
See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+"">edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+</a> for more details.
\</PRE\>".Replace("{PK}", messages[1].PK.ToString().ToLower()).Replace("{VersionNumber}", new EnterpriseInformationRetriever().VersionNumber);
				Assert(string.Format(@"Wrong email body:
{0}", Env.OutgoingMailManager.EmailsCreated[1].Body), Regex.IsMatch(Env.OutgoingMailManager.EmailsCreated[1].Body, pattern, RegexOptions.Multiline));
			}
			mockWebServiceClient.VerifyAll();
		}

		[TestDate(2009, 01, 29)]
		public void TestInboundReceiptsUseBankAccountFromRegistry()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var receipts = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
				receipts.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no receipts", 0, Factory.Load<ARReceipt>(receipts).Length);

				OrgHeader creditor = TestObjectCreator.CreateOrgHeader("CREDITOR", true, false);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);

				Factory.Save();

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = false;
				var doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions/></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				var doc2 = new XmlDocument();
				doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2009-01-13T12:19:00+11:00</InvoiceDate><PostDate>2009-01-13T12:19:00+11:00</PostDate><CreatedUserId>USERNAME</CreatedUserId><BankCode>BANK</BankCode><ChequeOrReference>C101480</ChequeOrReference><ReceiptPaymentType>END</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">220.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">220.00</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">220.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">220.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">0</OsTaxAmount><PaidTransactions><PaidTransaction><Ledger>AP</Ledger><TxnType>INV</TxnType><TxnNumber>00001197</TxnNumber><Description>AP INVOICE</Description><InvoiceDate>2009-01-13T12:19:00+11:00</InvoiceDate><DueDate>2009-01-27T12:19:00+11:00</DueDate><PostDate>2009-01-13T12:19:00+11:00</PostDate><Branch /><Department /><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NUMBERTYPE=""BUSINESS"">61-03-234243234</TelephoneNumber><TelephoneNumber NUMBERTYPE=""FAX"">61-03-2343242</TelephoneNumber></TelephoneNumbers><LOCATION /><SEQUENCE>1</SEQUENCE></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">220.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">220.00</LocalInvoiceAmtInclTax></PaidTransaction></PaidTransactions></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				Receipt[] createdReceipts = Factory.Load<ARReceipt>(receipts);
				AssertEquals("Should have created 1 receipt", 1, createdReceipts.Length);
				AssertEquals("Compay receipts should use the default receipt bank account defined in the registry", TestObjectCreator.AUDBankAccount.PK, createdReceipts[0].AH_AB);

				mockWebServiceClient.VerifyAll();
			}
		}

		[TestDate(2009, 12, 01)]
		public void TestInboundReceiptsGetCorrectChequeOrReference()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var receiptQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
				receiptQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no receipt", 0, Factory.Load<ARReceipt>(receiptQuery).Length);
				AssertEquals("Prerequisite: there should be no email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var creditor = TestObjectCreator.ABIGAS;
				TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc1 = new XmlDocument();
				getNewInvoicesResponse.updates = doc1.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var xmlTemp = @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>{0}</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>";

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var doc2 = new XmlDocument();
				doc2.LoadXml(string.Format(xmlTemp, ""));
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
				new eNettInboundTransactionProcessor().Process(Notifications);

				var createdReceipts = new BusinessObjectFactory().Load<ARReceipt>(receiptQuery);
				AssertEquals("Should have created 0 receipt", 0, createdReceipts.Length);
				AssertEquals("Should have created 1 notification email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("Notification email should have error message", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Error: Bank Reference Number: Please enter a Reference Number."));

				Env.OutgoingMailManager.EmailsCreated.Clear();
				doc2.LoadXml(string.Format(xmlTemp, "12345678"));
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
				new eNettInboundTransactionProcessor().Process(Notifications);

				createdReceipts = new BusinessObjectFactory().Load<ARReceipt>(receiptQuery);
				AssertEquals("Should have created 1 receipt", 1, createdReceipts.Length);
				AssertEquals("Should not creat notification email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				var receipt = createdReceipts[0];
				AssertEquals("When input value is specific", "12345678", receipt.AH_ChequeOrReference);
				mockWebServiceClient.VerifyAll();
			}
		}

		[TestDate(2017, 12, 01)]
		public void TestInboundReceiptsChequeOrReferenceForClientData()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var receiptQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
				receiptQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no receipts", 0, Factory.Load<ARReceipt>(receiptQuery).Length);

				var creditor = TestObjectCreator.ABIGAS;
				TestObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
				creditor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201635");
				creditor.CompanyData.OB_IsCreditor = true;
				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc1 = new XmlDocument();
				getNewInvoicesResponse.updates = doc1.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				//XML data provided by the client
				var xmlData = @"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2017-05-29T09:57:47.7501768+10:00</Date><Source /><Target /><EDIOrganisation EDICode=""201635"" OwnerCode=""""><OrganisationDetails><Name>EDI CUSTOMS BROKERS PTY LTD</Name><Location Country=""AU"" City=""ALBION""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-07-38624788</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-07-38624889</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>P O BOX 1376</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>FORTITUDE VALLEY</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4006</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>201635</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode=""201826"" OwnerCode=""""><OrganisationDetails><Name>QUAY SHIPPING</Name><Location Country=""AU"" City=""MURARRIE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-07-38624788</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-07-38624889</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>P O BOX 1376</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>FORTITUDE VALLEY</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4006</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Joe Eaton</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>edibneqs@quay-shipping.com</EmailAddress><JobTitle>Operations Manager</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2017-05-29T09:57:47.0000000+10:00</InvoiceDate><PostDate>2017-05-29T09:57:47.0000000+10:00</PostDate><CreatedUserId /><PaymentReceiptBatchDate>2017-05-31T00:00:00.0000000+10:00</PaymentReceiptBatchDate><BankCode>BANK</BankCode><ChequeOrReference>C3827227</ChequeOrReference><ReceiptPaymentType>END</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">1215.50</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">1215.50</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">1215.50</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">1215.50</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">0</OsTaxAmount><PaidTransactions><PaidTransaction><Ledger>AP</Ledger><TxnType>INV</TxnType><TxnNumber>00238393</TxnNumber><Description>AP INVOICE</Description><InvoiceDate>2017-05-29T09:57:47.0000000+10:00</InvoiceDate><DueDate>2017-05-29T09:57:47.0000000+10:00</DueDate><PostDate>2017-05-29T09:57:47.0000000+10:00</PostDate><Branch /><Department /><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>QUAY SHIPPING</Name><Location Country=""AU"" City=""MURARRIE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>10 HUTCHESON STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>ALBION</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4010</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-07-38624788</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-07-38624889</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>P O BOX 1376</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>FORTITUDE VALLEY</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4006</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Joe Eaton</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>edibneqs@quay-shipping.com</EmailAddress><JobTitle>Operations Manager</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">1215.50</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">1215.50</LocalInvoiceAmtInclTax><AmountPaidThisPayment CurrencyCode=""AUD"">-1215.50</AmountPaidThisPayment></PaidTransaction></PaidTransactions></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>";

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var doc2 = new XmlDocument();
				doc2.LoadXml(xmlData);
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
				new eNettInboundTransactionProcessor().Process(Notifications);

				var createdReceipts = Factory.Load<ARReceipt>(receiptQuery);
				AssertEquals("Should have created 1 receipt", 1, createdReceipts.Length);
				var receipt = createdReceipts[0];
				AssertEquals("Should have correct AH_ChequeOrReference from input", "C3827227", receipt.AH_ChequeOrReference);
				mockWebServiceClient.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestHandleWebException()
		{
			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);
				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(new WebException());

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);
			}
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestHandleSOAPException()
		{
			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);
				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(new SoapException());

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);
			}
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestHandleSocketException()
		{
			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);
				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(new SocketException());

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);
			}
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestHandleInvalidOperationException()
		{
			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);
				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(new InvalidOperationException());

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);
			}
			mock.VerifyAll();
		}

		public void TestCriticalValidationException()
		{
			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var expectedException = new OnSavingCriticalCheckException<UAInvoice>(Factory.New<UAInvoice>(),
					CriticalValidationErrorType.TransactionLineHasNoJobWhenRequiredByChargeCode_2,
					"badness",
					CriticalValidationMessageTemplate.GetTransactionLineHasNoJobWhenRequiredByChargeCodeErrorMessage("Test Charge"));

				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);

				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(expectedException);

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);

				AssertEquals("Expected an error to have been added to notifications", true, Notifications.HasErrors);
				AssertContains(CriticalValidationMessageTemplate.GetTransactionLineHasNoJobWhenRequiredByChargeCodeErrorMessage("Test Charge"), Notifications.AsString);
			}
			mock.VerifyAll();
		}

		public void TestWebExceptionErrorLogBuilderIntegration()
		{
			AccountingConfigurationRegistry.Instance.EnableExtraLoggingForENettWebExceptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var testException = new WebException("Exception For WebExceptionErrorLogBuilder Integration Test", WebExceptionStatus.UnknownError);
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);

				mock.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Throws(testException);

				var processor = new eNettInboundTransactionProcessor();
				processor.Process(Notifications);

				var expectedNotification = @"Error connecting to eNett web service
Exception Message: Exception For WebExceptionErrorLogBuilder Integration Test
Exception Status: UnknownError
Web Response was null";

				AssertEquals("Expected an error to have been added to notifications", true, Notifications.HasErrors);
				AssertContains(expectedNotification, Notifications.AsString);
			}
			mock.VerifyAll();
		}

		[ExpectNoExceptions()]
		public void TestDontBlowUpWhenNoBranchesForACompany()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateNewCompany("ZOI");
			company.Factory.Save();

			var demoCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "ZOI");
			demoCompany.Branches.DeleteAll();
			Factory.Save();

			new eNettInboundTransactionProcessorForTesting().Process(Notifications);
		}

		[TestDate(2017, 03, 21)]
		public void TestEmailSentOnlyWhenCallToWebMethodFailed()
		{
			MockENettWebService.Instance.SetupForTesting("CARGOWISE");
			MockENettWebService.Instance.FailInGetNewInvoicesInvalidIntegrator = true;

			new eNettInboundTransactionProcessorForTesting().Process(Notifications);

			AssertEquals(1, MockENettWebService.Instance.CountGetNewInvoicesWasInvoked);
			AssertEquals("Should have sent 1 email for 1 web method call failed", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailSentWhenCallToAnyWebMethodFailed()
		{
			MockENettWebService.Instance.SetupForTesting("CARGOWISE");

			new eNettInboundTransactionProcessorForTesting("WRONGINTEGRATOR").Process(Notifications);

			AssertEquals(1, MockENettWebService.Instance.CountGetNewInvoicesWasInvoked);
			AssertEquals("Should have sent 2 emails for 2 web method calls failed", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestDbHitCount()
		{
			new eNettInboundTransactionProcessorForTesting().Process(Notifications); // to load caches

			int hitCountBefore = CargoWise.Data.Db.Connection.ExecutedCommandCountForAllConnections;
			new eNettInboundTransactionProcessorForTesting().Process(Notifications);
			int hitCountAfter = CargoWise.Data.Db.Connection.ExecutedCommandCountForAllConnections;

			int dbHits = hitCountAfter - hitCountBefore;

			Assert("db hit count too high: " + dbHits.ToString(), dbHits <= 103); // Note that Env should be caching user contexts. Otherwise causes major performance problems at e.g. Mainfreight.
		}

		[TestDate(2017, 03, 21)]
		public void TestEmailContainsLinkToEDIMessage()
		{
			MockENettWebService.Instance.SetupForTesting("CARGOWISE");

			var oldPayload = MockENettWebService.Instance.NewInvoicePayload;
			MockENettWebService.Instance.NewInvoicePayload = ENettWebServiceTestConstants.BadNewInvoice;

			try
			{
				new eNettInboundTransactionProcessorForTesting().Process(Notifications);

				AssertEquals("Should have sent 1 email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EDIMessage msg = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewInvoices));
				AssertNotNull(msg);
				string pattern = @"\<PRE\>
An error occurred during processing transaction\(s\) received from eNett.
Message: There was en error processing invoice 165.
Message: .+
See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+"">edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+</a> for more details.
\</PRE\>".Replace("{PK}", msg.PK.ToString().ToLower()).Replace("{VersionNumber}", new EnterpriseInformationRetriever().VersionNumber);
				Assert(String.Format(@"Wrong email body:
{0}", Env.OutgoingMailManager.EmailsCreated[0].Body), Regex.IsMatch(Env.OutgoingMailManager.EmailsCreated[0].Body, pattern, RegexOptions.Singleline));
			}
			finally
			{
				MockENettWebService.Instance.NewInvoicePayload = oldPayload;
			}
		}

		[TestDate(2009, 12, 01)]
		public void TestEmailSentWhenInvoiceHasErrors()
		{
			var mockWebServiceClient = new Mock<IeNettWebServiceClient>();
			using (ObjectFactory.Substitute(mockWebServiceClient.Object))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var invoices = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.UAInvoice);
				invoices.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				AssertEquals("Prerequisite: there should be no invoices", 0, Factory.Load<APInvoice>(invoices).Length);
				AssertEquals("Prerequisite: there should be no EDI messages", 0, Factory.Load<EDIMessage>(new ZQuery()).Length);

				TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);

				Factory.Save();

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var getNewInvoicesResponse = new Response_GetNewInvoices();
				getNewInvoicesResponse.success = true;
				var doc = new XmlDocument();
				doc.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-19T19:51:04.305625+11:00</Date><Source /><Target /><EDIOrganisation EDICode=""CREDITOR"" OwnerCode=""""><OrganisationDetails><Name>SWANS INC</Name><Location Country=""AU"" City=""SUBURB""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>123123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""AALSHI"" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>1 ADD</AddressLine1><AddressLine2>2 ADD</AddressLine2><AddressCode /><CityOrSuburb>SUBURB</CityOrSuburb><StateOrProvince>VIC</StateOrProvince><PostCode>1233</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>165</TxnNumber><JobInvoiceNo>231</JobInvoiceNo><Description /><InvoiceDate>2008-03-17T00:00:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>23</InvTermDays><DueDate>2008-03-17T00:00:00+11:00</DueDate><PostDate>2008-03-17T00:00:00+11:00</PostDate><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>986</CreatedUserId><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TxnLines><TxnLine><Description>Desc</Description><LineType>REV</LineType><Sequence>1</Sequence><ChargeCode>AWBFEE</ChargeCode><Description /><ConsolOrJobNo /><ConsolOrJobType>ACR</ConsolOrJobType><MasterBillNo /><HouseBIllNo /><OriginPortCode Country="""" City=""""></OriginPortCode><DestinationPortCode Country="""" City=""""></DestinationPortCode><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">23.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0.00</OsWHTAmount><TaxCode>GST</TaxCode><ETA>2008-03-17T16:18:09+11:00</ETA><ETD>2008-03-17T16:18:09+11:00</ETD><Chargeable>0.00</Chargeable></TxnLine></TxnLines></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewInvoicesResponse.updates = doc.DocumentElement;
				getNewInvoicesResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				var getNewReceiptsResponse = new Response_GetNewPayments();
				getNewReceiptsResponse.success = true;
				var doc2 = new XmlDocument();
				doc2.LoadXml(@"<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2008-03-21T01:24:24.7275+11:00</Date><Source /><Target /><EDIOrganisation EDICode="""" OwnerCode=""""><OrganisationDetails><Name>BOB'S BUSINESS</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>GST</NumberType><Number>ABN123</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>ENE</NumberType><Number>923456</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails></EDIOrganisation></InterchangeInfo><Payload><FinancialTransactions><FinancialInvoice><Ledger>AP</Ledger><DebtorOrCreditor EDICode="""" OwnerCode=""""><OrganisationDetails><Name>PAYMENTS INCORPORATED</Name><Location Country=""AU"" City=""MELBOURNE""></Location><Addresses><Address AddressType=""MAIN""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">61-03-234243234</TelephoneNumber><TelephoneNumber NumberType=""Fax"">61-03-2343242</TelephoneNumber></TelephoneNumbers><Location /><Sequence>1</Sequence></Address><Address AddressType=""PST""><AddressLine1>11/50 QUEEN STREET</AddressLine1><AddressLine2 /><AddressCode /><CityOrSuburb>MELBOURNE</CityOrSuburb><StateOrProvince>ACT</StateOrProvince><PostCode>3000</PostCode><Location /><Sequence>2</Sequence></Address></Addresses><Contacts><Contact><Name>Bob Bitchin</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><EmailAddress>pomeara@enett.com</EmailAddress><JobTitle>position</JobTitle><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></DebtorOrCreditor><TxnType>PAY</TxnType><Description>AP Payment</Description><InvoiceDate>2008-03-21T01:24:24+11:00</InvoiceDate><PostDate>2008-03-21T01:24:24+11:00</PostDate><CreatedUserId /><ReceiptPaymentType>STD</ReceiptPaymentType><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">99.49</LocalInvoiceAmtInclTax><OsInvoiceAmtExclTax CurrencyCode=""AUD"">209.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">232.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">23.00</OsTaxAmount><BankCode>BANK</BankCode><ChequeOrReference>12345678</ChequeOrReference><ChequeDrawer>CHEQUEDRAWER</ChequeDrawer><PaidTransactions/></FinancialInvoice></FinancialTransactions></Payload></XmlInterchange>");
				getNewReceiptsResponse.updates = doc2.DocumentElement;
				getNewReceiptsResponse.receivedDateTime = ZDateTime.BrettsBirthday.ToDateTime();

				mockWebServiceClient.Setup(m => m.GetNewPayments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewReceiptsResponse);
				mockWebServiceClient.Setup(m => m.GetNewInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(getNewInvoicesResponse);
				mockWebServiceClient.Setup(m => m.GetCancelledInvoices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));

				new eNettInboundTransactionProcessor().Process(Notifications);

				InvoicingBase[] createdInvoices = Factory.Load<InvoicingBase>(invoices);
				AssertEquals("Should have not created invoice due to errors", 0, createdInvoices.Length);

				EDIMessage msg = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewInvoices));
				AssertNotNull(msg);
				string pattern = @"\<PRE\>
An error occurred during processing transaction\(s\) received from eNett.
Message: There was en error processing invoice 165.
Message: Error: Attempt to process ComPay inbound transaction failed because not organization match could be found for ComPay Debtor ID 'CREDITOR', Code '123123'.
Error: No matches were found for the following Organization: ?
Error: Address Override: Please enter an Account.
Error: Account: Please enter an Account.
See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+"">edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+</a> for more details.
\</PRE\>".Replace("{PK}", msg.PK.ToString().ToLower()).Replace("{VersionNumber}", new EnterpriseInformationRetriever().VersionNumber);
				Assert(String.Format(@"Wrong email body:
{0}", Env.OutgoingMailManager.EmailsCreated[0].Body), Regex.IsMatch(Env.OutgoingMailManager.EmailsCreated[0].Body, pattern, RegexOptions.Multiline));

				msg = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageSubType, eNettMessageSubTypeList.Codes.GetNewPayments));
				AssertNotNull(msg);
				pattern = @"\<PRE\>
An error occurred during processing transaction\(s\) received from eNett.
Message: There was en error processing payment.
Message: Error: Attempt to process ComPay inbound transaction failed because not organization match could be found for ComPay Debtor Name 'BOB'S BUSINESS', Code '923456'.
Error: Account: Please enter an Account.
See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+"">edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=LinkedeNettEDIMessage&BusinessEntityPK={PK}&VersionNumber={VersionNumber}&Hash=.+</a> for more details.
\</PRE\>".Replace("{PK}", msg.PK.ToString().ToLower()).Replace("{VersionNumber}", new EnterpriseInformationRetriever().VersionNumber);
				Assert(String.Format(@"Wrong email body:
{0}", Env.OutgoingMailManager.EmailsCreated[1].Body), Regex.IsMatch(Env.OutgoingMailManager.EmailsCreated[1].Body, pattern, RegexOptions.Multiline));
			}
			mockWebServiceClient.VerifyAll();
		}

		#region Implementation
		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		void AssertEDIMessage(EDIMessage message, int messageNumber, string messageSubType, bool isSuccess, string xml, ZGuid branchPK, ZGuid departmentPK, ZGuid transactionPK)
		{
			string prefix = String.Format("Message{0}.", messageNumber);
			AssertEquals(prefix + "EM_ApplicationCode", EDIMessage.ApplicationCodes.eNett, message.EM_ApplicationCode);
			AssertEquals(prefix + "EM_MessageType", "ENE", message.EM_MessageType);
			AssertEquals(prefix + "EM_MessageSubType", messageSubType, message.EM_MessageSubType);
			AssertEquals(prefix + "EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals(prefix + "EM_Status", isSuccess ? EDIMessage.Status.Received : EDIMessage.Status.Failed, message.EM_Status);
			this.AssertXMLEqualsByDiff(prefix + "EM_MessageText", xml, Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(message.EM_MessageText.ToString(), "<Date>.*</Date>", ""), "<InvoiceDate>.*</InvoiceDate>", ""), "<PostDate>.*</PostDate>", ""), "<DueDate>.*</DueDate>", ""), "<Data>.*</Data>", ""));
			AssertEquals(prefix + "EM_GB", branchPK, message.EM_GB);
			AssertEquals(prefix + "EM_GE", departmentPK, message.EM_GE);
			AssertEquals(prefix + "EM_LinkTable", AccTransactionHeader.Schema.TableName, message.EM_LinkTable);
			AssertEquals(prefix + "EM_LinkUniqueID", transactionPK, message.EM_LinkUniqueID);
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Now.Year);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Now.Year - 1);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Now.Year + 1);
			TestObjectCreator.TestOrganisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123456");
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = TestObjectCreator.AALSHI.PK });

			var col = new ENettRegisteredBankAccountCollection();
			var eNettRegisteredBankAccount = new ENettRegisteredBankAccount(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory) { BankAccountPK = TestObjectCreator.AUDBankAccount.PK, IsDefault = true };
			col.Add(eNettRegisteredBankAccount);
			AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, col);

			// Ensure the ENettNotificationsGroup has a single user with a valid email
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			group.Staff.Add(currentUserInCurrentFactory);
			AccountingConfigurationRegistry.Instance.ENettNotificationsGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);
			}

			Factory.Save();
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		#endregion
	}
}
