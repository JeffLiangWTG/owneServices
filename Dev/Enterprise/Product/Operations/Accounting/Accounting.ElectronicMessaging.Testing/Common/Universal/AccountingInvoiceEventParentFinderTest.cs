using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	public class AccountingInvoiceEventParentFinderTest : TestCaseWithFactory
	{
		public void TestHandleDuplicateAPInvoiceNumberWhenMissingContextCollection()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "S5678900", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_TransactionNum = "00001000";
			invoice2.AH_OH = creditor2.PK;
			invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice2.AH_GC = company.PK;
			invoice2.AH_GB = company.Branches[0].PK;

			Factory.Save();

			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals("00001000", invoice2.AH_TransactionNum);
			AssertEquals(creditor1.PK, invoice1.AH_OH);
			AssertEquals(creditor2.PK, invoice2.AH_OH);

			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_NoContextCollection, out logger);
			AssertEquals("Error - More than one matched invoice #AP INV 00001000 for creditor '' with invoice date [] found in the system.", logger.ToString());

			logger = null;
			ProcessEventXml(eventXmlText_NoContextCollectionForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Error - More than one matched invoice #AP INV 00001000 for creditor '' with invoice date [] found in the system.", logger.ToString());
		}

		public void TestHandleDuplicateAPInvoiceNumberWithDifferentCreditor()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "ORGPPP";
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "S5678900", Core.Constants.CountryCodes.Spain);
			creditor2.OH_Code = "ORGAAA";

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_TransactionNum = "00001000";
			invoice2.AH_OH = creditor2.PK;
			invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice2.AH_GC = company.PK;
			invoice2.AH_GB = company.Branches[0].PK;

			Factory.Save();

			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals("00001000", invoice2.AH_TransactionNum);
			AssertEquals(creditor1.PK, invoice1.AH_OH);
			AssertEquals(creditor2.PK, invoice2.AH_OH);

			XmlSessionTracker logger = null;
			var logParent = ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Expect find invoice1 due to the creditor matches", invoice1, logParent as APInvoice);

			logger = null;
			logParent = ProcessEventXml(eventXmlText_CreditorForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Expect find invoice2 due to the creditor matches", invoice2, logParent as APInvoice);
		}

		public void TestHandleDuplicateAPInvoiceNumberWithSameCreditorButDifferentInvoiceDate()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ORGAAA";
			creditor.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor.PK;
			invoice1.AH_TransactionCount = 1;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 01, 21, 13, 56, 34);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_TransactionNum = "00001000";
			invoice2.AH_OH = creditor.PK;
			invoice2.AH_TransactionCount = 2;
			invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice2.AH_GC = company.PK;
			invoice2.AH_GB = company.Branches[0].PK;

			Factory.Save();

			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals("00001000", invoice2.AH_TransactionNum);
			AssertEquals(creditor.PK, invoice1.AH_OH);
			AssertEquals(creditor.PK, invoice2.AH_OH);

			XmlSessionTracker logger = null;
			var logParent = ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Expect find invoice2 due to the invoice date matches", invoice2, logParent as APInvoice);

			logger = null;
			logParent = ProcessEventXml(eventXmlText_CreditorForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Expect find invoice2 due to the invoice date matches", invoice2, logParent as APInvoice);
		}

		public void TestGetLogParentsForEventUsingContextWhenNIFNotFound()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();

			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Error - Unable to find an unique creditor due to registration number 'ES NIF K11223344' not found in the system.", logger.ToString());

			logger = null;
			ProcessEventXml(eventXmlText_CreditorForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Error - Unable to find an unique Creditor / Debtor due to code 'ORGAAA' not found in the system.", logger.ToString());
		}

		public void TestGetLogParentsForEventUsingContextWhenDuplicateNIFFound()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();

			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Error - Unable to find an unique creditor due to duplicate registration number 'ES NIF K11223344' detected in the system.", logger.ToString());
		}

		public void TestGetLogParentsForEventUsingContextWhenInvoiceNotFound()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "ORGAAA";
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();

			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Error - Unable to find a matched invoice #AP INV 00001000 for creditor 'ORGAAA' with invoice date [26-Feb-18] in the system.", logger.ToString());

			logger = null;
			ProcessEventXml(eventXmlText_CreditorForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Error - Unable to find a matched invoice #AP INV 00001000 for creditor 'ORGAAA' with invoice date [26-Feb-18] in the system.", logger.ToString());
		}

		public void TestGetLogParentsForEventUsingContextWhenMultipleInvoicesFound()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "ORGAAA";
			creditor.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor.PK;
			invoice1.AH_TransactionCount = 1;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 13, 56, 34);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_TransactionNum = "00001000";
			invoice2.AH_OH = creditor.PK;
			invoice2.AH_TransactionCount = 2;
			invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice2.AH_GC = company.PK;
			invoice2.AH_GB = company.Branches[0].PK;

			Factory.Save();

			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals("00001000", invoice2.AH_TransactionNum);
			AssertEquals(creditor.PK, invoice1.AH_OH);
			AssertEquals(creditor.PK, invoice2.AH_OH);
			AssertEquals("26-Feb-18", invoice1.AH_InvoiceDate.ToShortDateString());
			AssertEquals("26-Feb-18", invoice2.AH_InvoiceDate.ToShortDateString());
			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Error - More than one matched invoice #AP INV 00001000 for creditor 'ORGAAA' with invoice date [26-Feb-18] found in the system.", logger.ToString());

			logger = null;
			ProcessEventXml(eventXmlText_CreditorForUpdateSingleTransactionHeader, out logger);
			AssertEquals("Error - More than one matched invoice #AP INV 00001000 for creditor 'ORGAAA' with invoice date [26-Feb-18] found in the system.", logger.ToString());
		}

		public void TestHandleDuplicateAPInvoiceNumberWithES_NIF_Fallback()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.IGC, "K11223344", Core.Constants.CountryCodes.Spain);
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.DNI, "S5678900", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;

			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice2.AH_TransactionNum = "00001000";
			invoice2.AH_OH = creditor2.PK;
			invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice2.AH_GC = company.PK;
			invoice2.AH_GB = company.Branches[0].PK;

			Factory.Save();

			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals("00001000", invoice2.AH_TransactionNum);
			AssertEquals(creditor1.PK, invoice1.AH_OH);
			AssertEquals(creditor2.PK, invoice2.AH_OH);

			XmlSessionTracker logger = null;
			var logParent = ProcessEventXml(eventXmlText_ESCreditor, out logger);
			AssertEquals("Expect find invoice1 due to the creditor matches", invoice1, logParent as APInvoice);

			var newEventXmlText = eventXmlText_ESCreditor.Replace("K11223344", "S5678900");
			logParent = ProcessEventXml(newEventXmlText, out logger);
			AssertEquals("Expect find invoice2 due to the creditor matches", invoice2, logParent as APInvoice);
		}

		public void TestGetLogParentsForEventUsingContextForNonESCreditor()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "AUORG";
			creditor1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678", Core.Constants.CountryCodes.Australia);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();
			XmlSessionTracker logger = null;
			var invoice = ProcessEventXml(eventXmlText_NonESCreditor, out logger);
			AssertEquals(invoice1, invoice);
		}

		public void TestGetLogParentsForEventUsingContextWhenTaxRegNumberIsTooShort()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "ORGAAA";
			creditor1.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();
			XmlSessionTracker logger = null;
			ProcessEventXml(eventXmlText_NonESCreditor.Replace("AUABN12345678", "AUABN"), out logger);
			AssertEquals("Error - <Tax Reg Number> is too short, it needs to have at least 6 characters: 'AUABN'.", logger.ToString());
		}

		public void TestGetLogParentsForEventUsingContextWithTransactionNumberContainSpaces()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "AUORG";
			creditor1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678", Core.Constants.CountryCodes.Australia);

			var invoice1 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.AH_TransactionNum = " 000 010  00  ";
			invoice1.AH_OH = creditor1.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
			invoice1.AH_GC = company.PK;
			invoice1.AH_GB = company.Branches[0].PK;
			Factory.Save();
			XmlSessionTracker logger = null;
			var invoice = ProcessEventXml(eventXmlText_NonESCreditor.Replace("00001000", " 000 010  00  "), out logger);
			AssertEquals("The AH_TransactionNum should be ' 000 010  00' as AH_TransactionNum setter of AutoAcctransactionHeader will do TrimEndSpaceTab() and the Deserializer will do TrimEnd() for <Key>.", " 000 010  00", ((APInvoice)invoice).AH_TransactionNum);
			AssertEquals(invoice1, invoice);
		}

		public void TestGetLogParentsForEventUsingContextWithNotInvoicingBaseKey()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var receipt = Factory.NewWithValidTestData<APReceipt>();
			receipt.AH_TransactionNum = "00001000";
			receipt.AH_GC = company.PK;
			receipt.AH_GB = company.Branches[0].PK;
			Factory.Save();
			XmlSessionTracker logger = null;
			var logParent = ProcessEventXml(eventXmlText_NoContextCollection.Replace("INV", "REC"), out logger);

			AssertNull("Expect no log parent", logParent);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_GC = company.PK;
			invoice.AH_GB = company.Branches[0].PK;
			Factory.Save();
			logParent = ProcessEventXml(eventXmlText_NoContextCollection, out logger);

			AssertNotNull("Expect no log parent", logParent);
		}

		BusinessObject ProcessEventXml(string eventXmlMessage, out XmlSessionTracker logger)
		{
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var subscriber = new AccountingInvoiceEventParentFinder(Factory, new AccountingInvoiceDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		readonly string eventXmlText_ESCreditor = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AP INV 00001000</Key>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>Electronic Reporting Reception Acknowledgement</MessageType>
           <MessageSubType>Spain</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>Tax Reg Number</Type>
             <Value>ESNIFK11223344</Value>
           </Context>
           <Context>
             <Type>Invoice Date</Type>
             <Value>26-02-2018</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_CreditorForUpdateSingleTransactionHeader = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AP INV 00001000</Key>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>DIM</EventType>
        <EventParameters>
           <MessageType>Update If Single Match Is Found</MessageType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>Organization Code</Type>
             <Value>ORGAAA</Value>
           </Context>
           <Context>
             <Type>Invoice Date</Type>
             <Value>26-02-2018</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_NonESCreditor = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AP INV 00001000</Key>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>Electronic Reporting Reception Acknowledgement</MessageType>
           <MessageSubType>Spain</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>Tax Reg Number</Type>
             <Value>AUABN12345678</Value>
           </Context>
           <Context>
             <Type>Invoice Date</Type>
             <Value>26-02-2018</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_NoContextCollection = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AP INV 00001000</Key>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>Electronic Reporting Reception Acknowledgement</MessageType>
           <MessageSubType>Spain</MessageSubType>
        </EventParameters>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_NoContextCollectionForUpdateSingleTransactionHeader = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AP INV 00001000</Key>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DEM</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>DIM</EventType>
        <EventParameters>
           <MessageType>Update If Single Match Is Found</MessageType>
        </EventParameters>
      </Event>
</UniversalEvent>";
	}
}