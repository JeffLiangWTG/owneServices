using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	public class AccEInvoicingBatchEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents_InterchangeRejectedCode()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_InterchangeRejectedCode, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith("Error - Unable to find batch with Batch Number and Company Code."));
		}

		public void TestGetLogParents_MessageSubTypeRiceviFile()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_MessageSubTypeRiceviFile, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith("Error - Unable to find batch with Batch Number and Company Code."));
		}

		public void TestGetLogParents_EmptyContextCollection()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_EmptyContextCollection, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith("Error - <ContextCollection> is missing in Universal Event."));
		}

		public void TestGetLogParents_ContextsNotPresent_WarningsAndErrors()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_ContextsNotPresent_WarningsAndErrors, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith(@"Error - <AccEInvoiceBatch> Key element is missing in Universal Event.
CompanyCode context is missing in Universal Event.

Warning - GovernmentAllocatedNumber context is missing in Universal Event.
eHubAllocatedNumber context is missing in Universal Event."));
		}

		public void TestGetLogParents_ContextsNotPresent_OnlyWarnings()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_ContextsNotPresent_OnlyWarnings, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			Assert(logger.ToString().StartsWith(@"Warning - GovernmentAllocatedNumber context is missing in Universal Event.
eHubAllocatedNumber context is missing in Universal Event."));
			Assert(!logger.ToString().Contains("Error -"));
		}

		public void TestGetLogParents_ContextsNotPresent_OnlyErrors()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_ContextsNotPresent_OnlyErrors, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith(@"Error - <AccEInvoiceBatch> Key is empty in Universal Event.
CompanyCode context is missing in Universal Event."));
			Assert(!logger.ToString().Contains("Warning -"));
		}

		public void TestGetLogParents_ContextValuesNotPresent()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_MandatoryContextValuesNotPresent, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith(@"Error - CompanyCode is empty in Universal Event.

Warning - GovernmentAllocatedNumber is empty in Universal Event.
eHubAllocatedNumber is empty in Universal Event."));
		}

		public void TestGetLogParents_BatchNumberMismatch()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 2;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith("Error - Unable to find a batch with batch number 1 for company DEM as there are 0 matches."));
		}

		public void TestGetLogParents_CompanyCodeMismatch()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertNull("Expect no batch is found", objects);
			Assert(logger.ToString().StartsWith("Error - Unable to find a batch with batch number 1 for company DEM as there are 0 matches."));
		}

		public void TestGetLogParents_UsingEventDataContext()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_RiceviFile, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			AssertEquals("Expect no errors or warnings", "", logger.ToString());
		}

		public void TestGetLogParents_UsingEventContextCollection()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			AssertEquals("Expect no errors or warnings", "", logger.ToString());
		}

		public void TestGetLogParents_WarnIfGovernmentAllocatedNumberAndEHubNumberDoesNotMatch()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667789";
			batch.AIB_EHubAllocatedNumber = "EHub-1234568";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			Assert(logger.ToString().StartsWith(@"Warning - For batch number 1 of company DEM, Government Allocated Number does not match between the Universal Event (GOV-55667788) and the batch (GOV-55667789).
For batch number 1 of company DEM, e-Hub Allocated Number does not match between the Universal Event (EHub-1234567) and the batch (EHub-1234568)."));
		}

		public void TestGetLogParents_WarnIfGovernmentAllocatedNumberDoesNotMatch()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667789";
			batch.AIB_EHubAllocatedNumber = "EHub-1234567";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			Assert(logger.ToString().StartsWith("Warning - For batch number 1 of company DEM, Government Allocated Number does not match between the Universal Event (GOV-55667788) and the batch (GOV-55667789)."));
			Assert(!logger.ToString().Contains("e-Hub Allocated Number does not match"));
		}

		public void TestGetLogParents_WarnIfEHubNumberDoesNotMatch()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_GovernmentAllocatedNumber = "GOV-55667788";
			batch.AIB_EHubAllocatedNumber = "EHub-1234568";
			batch.AIB_Status = "SNT";

			var invoice1 = AddNewInvoiceToBatch(batch);
			var invoice2 = AddNewInvoiceToBatch(batch);

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_NotificaMancataConsegna, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 invoices", new ZGuid[] { batch.PK, invoice1.PK, invoice2.PK }, objects.Select(x => x.PK));
			Assert(logger.ToString().StartsWith("Warning - For batch number 1 of company DEM, e-Hub Allocated Number does not match between the Universal Event (EHub-1234567) and the batch (EHub-1234568)."));
			Assert(!logger.ToString().Contains("Government Allocated Number does not match"));
		}

		public void TestGetLogParents_UsingEventDataContext_Taiwan()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_Status = "SNT";

			var complianceDocument1 = AddNewComplianceToBatch(batch, "001");
			var complianceDocument2 = AddNewComplianceToBatch(batch, "002");

			Factory.Save();

			XmlSessionTracker logger = null;
			var objects = ProcessEventXml(eventXmlText_InterchangeAcknowledgeCode_Taiwan, out logger);
			AssertEquals(3, objects.Length);
			AssertContainsExactElementsInAnyOrder("should contain 1 batch and 2 compliance documents", new ZGuid[] { batch.PK, complianceDocument1.PK, complianceDocument2.PK }, objects.Select(x => x.PK));
			AssertEquals("Expect no errors or warnings", "", logger.ToString());
		}

		#region Italy

		readonly string eventXmlText_RiceviFile = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>rispostaSdIRiceviFile</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value>DEM</Value>
           </Context>
           <Context>
             <Type>eHubAllocatedNumber</Type>
             <Value>EHub-1234567</Value>
           </Context>
           <Context>
             <Type>GovernmentAllocatedNumber</Type>
             <Value>GOV-55667788</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_NotificaMancataConsegna = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value>DEM</Value>
           </Context>
           <Context>
             <Type>eHubAllocatedNumber</Type>
             <Value>EHub-1234567</Value>
           </Context>
           <Context>
             <Type>GovernmentAllocatedNumber</Type>
             <Value>GOV-55667788</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_InterchangeRejectedCode = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection/>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_MessageSubTypeRiceviFile = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>rispostaSdIRiceviFile</MessageSubType>
        </EventParameters>
        <ContextCollection/>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_EmptyContextCollection = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection/>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_ContextsNotPresent_WarningsAndErrors = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>Company</Type>
             <Value>DEM</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_ContextsNotPresent_OnlyWarnings = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value>DEM</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_ContextsNotPresent_OnlyErrors = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key></Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>Company</Type>
             <Value>DEM</Value>
           </Context>
           <Context>
             <Type>eHubAllocatedNumber</Type>
             <Value>EHub-1234567</Value>
           </Context>
           <Context>
             <Type>GovernmentAllocatedNumber</Type>
             <Value>GOV-55667788</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
		readonly string eventXmlText_MandatoryContextValuesNotPresent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value/>
           </Context>
           <Context>
             <Type>eHubAllocatedNumber</Type>
             <Value/>
           </Context>
           <Context>
             <Type>GovernmentAllocatedNumber</Type>
             <Value/>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

		#endregion

		#region Taiwan

		readonly string eventXmlText_InterchangeAcknowledgeCode_Taiwan = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>TW</MessageType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value>DEM</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

		#endregion

		InvoicingBase AddNewInvoiceToBatch(AccEInvoicingBatch batch)
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GC = batch.AIB_GC;
			invoice.AH_GB = batch.Company.Branches[0].PK;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_AIB = batch.PK;
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_ParentTableCode = "AH";
			pivot.SetCompanyAndCountryCode(batch.Company);
			return invoice;
		}

		AccComplianceDocumentHeader AddNewComplianceToBatch(AccEInvoicingBatch batch, ZString invoiceNumber)
		{
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNumber, objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, objectCreator.Debtor, objectCreator.GST1.PK);
			arInvoice.Lines[0].AL_AG = objectCreator.GLHeader1.PK;
			var complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA" + invoiceNumber, "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
			return complianceDocument;
		}

		BusinessObject[] ProcessEventXml(string eventXmlMessage, out XmlSessionTracker logger)
		{
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var subscriber = new AccEInvoicingBatchEventParentFinder(Factory, new AccEInvoicingBatchDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			return subscriber.GetLogParentsForEvent(xmlEvent);
		}

		protected override void SetUp()
		{
			base.SetUp();

			objectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator objectCreator;
	}
}
