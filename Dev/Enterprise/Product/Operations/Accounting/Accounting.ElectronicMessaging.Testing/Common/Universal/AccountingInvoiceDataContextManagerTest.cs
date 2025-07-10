using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.ElectronicMessaging.Common.Universal.AccountingInvoiceDataContextManager;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	[TestedType(typeof(AccountingInvoiceDataContextManager))]
	public class AccountingInvoiceDataContextManagerTest : DataContextManagerTestCase<AccountingInvoiceDataContextManager, InvoicingBase>
	{
		public void TestGetDataContextKeyMatchingQuery_ARInvoice()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = "ErrorCode", Value = "RandomCode" } };
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals(string.Format("Linked Event to {0}.", invoice.HumanReadableName), importResults.Single().ToString());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_DuplicateAPInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = "ES";
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = company.PK;

				var creditor = Factory.NewWithValidTestData<OrgHeader>();
				creditor.OH_Code = "OrgAAA";
				creditor.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "K11223344", Core.Constants.CountryCodes.Spain);
				var invoice1 = Factory.NewWithValidTestData<APInvoice>();
				invoice1.AH_GC = company.PK;
				invoice1.AH_GB = branch.PK;
				invoice1.AH_TransactionCount = 1;
				invoice1.AH_OH = creditor.PK;
				invoice1.AH_InvoiceDate = new ZDateTime(2018, 01, 23, 09, 21, 51);

				var invoice2 = Factory.NewWithValidTestData<APInvoice>();
				invoice2.AH_GC = company.PK;
				invoice2.AH_GB = branch.PK;
				invoice2.AH_TransactionNum = invoice1.AH_TransactionNum;
				invoice2.AH_TransactionCount = 2;
				invoice2.AH_OH = creditor.PK;
				invoice2.AH_InvoiceDate = new ZDateTime(2018, 02, 26, 09, 21, 51);
				Factory.SaveForTesting();

				var eventDataObject = new UniversalEventDataObject();
				eventDataObject.DataContext = DataContextFactory.New();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice1.AH_Ledger, invoice1.AH_TransactionType, invoice1.AH_TransactionNum));
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
				eventDataObject.ContextCollection = new List<Context>();
				eventDataObject.ContextCollection.Add(new Context { Type = "ErrorCode", Value = "RandomCode" });
				eventDataObject.ContextCollection.Add(new Context { Type = "ErrorDescription", Value = "RandomValue" });
				eventDataObject.ContextCollection.Add(new Context { Type = EventDataConstants.Context_TaxRegNumber, Value = "ESNIFK11223344" });
				eventDataObject.ContextCollection.Add(new Context { Type = EventDataConstants.Context_InvoiceDate, Value = "26-02-2018" });
				eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = EventDataConstants.ElectronicInvoicingEventType,
					MessageSubType = EventDataConstants.ElectronicInvoicingEventSubType_Spain
				};

				var message = GetQueuedUniversalEventMessage(eventDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;
				AssertEquals("invoice1 should not be processed", 0, invoice1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
				AssertEquals("invoice2 should be processed", 1, invoice2.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());

				var eventDataObject1 = new UniversalEventDataObject();
				eventDataObject1.DataContext = DataContextFactory.New();
				eventDataObject1.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice2.AH_Ledger, invoice2.AH_TransactionType, invoice2.AH_TransactionNum));
				eventDataObject1.DataContext.SetCompanyAndDataProviderDetails(company);
				eventDataObject1.ContextCollection = new List<Context>();
				eventDataObject1.ContextCollection.Add(new Context { Type = EventDataConstants.Context_OrganizationCode, Value = "OrgAAA" });
				eventDataObject1.ContextCollection.Add(new Context { Type = EventDataConstants.Context_InvoiceDate, Value = "26-02-2018" });
				eventDataObject1.EventType = Events.DataImportCode;
				eventDataObject1.EventTime = ZDateTimeOffset.Now;
				eventDataObject1.EventParameters = new EventParameters
				{
					MessageType = EventDataConstants.ElectronicInvoicingEventType_SingleTransactionHeader
				};
				var message1 = GetQueuedUniversalEventMessage(eventDataObject1);
				var manager1 = new UniversalMessageProcessingManager(serviceTaskLog);

				var importResults1 = manager1.Process(message1).ImportResults;

				AssertEquals("invoice1 should not be processed", 0, invoice1.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
				AssertEquals("invoice2 should be processed", 1, invoice2.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			}
		}

		public void TestGetGetDataContextKeyMatchingQueryWithTransactionNumberContainsWhiteSpaces()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_TransactionNum = " 000 010  00  ";
			invoice.AH_Desc = "Initial invoice description";
			Factory.SaveForTesting();

			AssertEquals("Pre: The AH_TransactionNum should be ' 000 010  00' as AH_TransactionNum setter of AutoAcctransactionHeader will do TrimEndSpaceTab()", " 000 010  00", invoice.AH_TransactionNum);

			var eventData = new UniversalEventDataObject();
			eventData.DataContext = DataContextFactory.New();
			eventData.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum));
			eventData.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventData.EventType = Events.MessageAcceptedCode;
			eventData.EventTime = ZDateTimeOffset.Now;
			eventData.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>();
			eventData.AdditionalFieldsToUpdateCollection.Add(new AdditionalFieldToUpdate() { Type = "APInvoice.AH_Desc", Value = "Updated  invoice description" });

			var message = GetQueuedUniversalEventMessage(eventData);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			AssertEquals($@"Linked Event to {invoice.HumanReadableName}.
Field [APInvoice.AH_Desc] has been updated to value [Updated  invoice description] on Accounts Payable Invoice." , importResults.Single().ToString());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.MessageAcceptedCode).Count());
			AssertContains("Linked Event to Accounts Payable Invoice.", serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQueryNotInvoicingBaseKey()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_GC = GlbCompany.CurrentCompany.PK;
			payment.AH_TransactionNum = "00001000";
			Factory.SaveForTesting();

			var eventData = new UniversalEventDataObject();
			eventData.DataContext = DataContextFactory.New();
			eventData.DataContext.AddDataTarget(DataContextType.AccountingPayment, string.Format("{0} {1} {2}", payment.AH_Ledger, payment.AH_TransactionType, payment.AH_TransactionNum));
			eventData.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventData.EventType = Events.MessageAcceptedCode;
			eventData.EventTime = ZDateTimeOffset.Now;

			var message = GetQueuedUniversalEventMessage(eventData);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", importResults.Single().ToString());

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_TransactionNum = "00001000";
			Factory.SaveForTesting();

			eventData.DataContext.AddDataTarget(DataContextType.AccountingInvoice, string.Format("{0} {1} {2}", invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum));
			message = GetQueuedUniversalEventMessage(eventData);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			importResults = manager.Process(message).ImportResults;
			AssertEquals("Linked Event to Accounts Payable Invoice.", importResults.Single().ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_ChinaSIU()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = "CN";
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = company.PK;

				var creditor = Factory.NewWithValidTestData<OrgHeader>();
				creditor.OH_Code = "OrgAAA";
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_GC = company.PK;
				invoice.AH_GB = branch.PK;
				invoice.AH_TransactionCount = 1;
				invoice.AH_OH = creditor.PK;
				invoice.AH_InvoiceDate = new ZDateTime(2018, 01, 23, 09, 21, 51);

				Factory.SaveForTesting();

				AssertEquals("invoice has no IAK log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());

				var eventDataObject = new UniversalEventDataObject();
				eventDataObject.DataContext = DataContextFactory.New();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, "0");
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
				eventDataObject.ContextCollection = new List<Context>();
				eventDataObject.ContextCollection.Add(new Context { Type = "EINV_ComplianceNumber", Value = "1111" });
				eventDataObject.ContextCollection.Add(new Context { Type = "EINV_CN_SerialNumber", Value = invoice.PK.ToString() + "|EDIEDIDAT" });
				eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = EventDataConstants.ElectronicInvoicingEventType_China,
					MessageSubType = EventDataConstants.ElectronicInvoicingEventSubType_SIU
				};

				var message = GetQueuedUniversalEventMessage(eventDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				AssertEquals("invoice should be processed", 1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			}
		}

		public void TestOnLogParentFoundFromEDIMessageWithChinaSIU()
		{
			var eventXmlText = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key/>
              <Type>AccountingInvoice</Type>
            </DataTarget>
          </DataTargetCollection>
          <Company>
            <Code>DCN</Code>
          </Company>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>CN</MessageType>
           <MessageSubType>SIU</MessageSubType>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>EINV_ComplianceNumber</Type>
            <Value>15200018635723479938</Value>
          </Context>
          <Context>
			<Type>EINV_CN_SerialNumber</Type>
			<Value>{0}|EDIEDIDAT</Value>
		  </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			invoice.AH_GC = company.PK;
			invoice.AH_GB = company.Branches[0].PK;

			Factory.SaveForTesting();

			Assert("Pre-condition: Invoice compliance number is empty.", invoice.AH_TransactionReference.IsEmpty);

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, "0");
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context { Type = "EINV_ComplianceNumber", Value = "1111" });
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = EventDataConstants.ElectronicInvoicingEventType_China,
				MessageSubType = EventDataConstants.ElectronicInvoicingEventSubType_SIU
			};

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			eventXmlText = string.Format(eventXmlText, invoice.PK.ToString());
			using (var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText))
			{
				var manager = new AccountingInvoiceDataContextManager();
				var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
				manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, message, invoice);

				Assert("Post-condition: Invoice compliance number has been updated.", !invoice.AH_TransactionReference.IsEmpty);
			}
		}

		public void TestOnLogParentFoundFromEDIMessageWithInvalidSubType()
		{
			var eventXmlText = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AR INV 00001000</Key>
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
           <MessageSubType>Canada</MessageSubType>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>ErrorCode</Type>
            <Value>3000</Value>
          </Context>
          <Context>
            <Type>ErrorDescription</Type>
            <Value>Factura Duplicada</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
			invoice.AH_GC = company.PK;
			invoice.AH_GB = company.Branches[0].PK;

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_Status = Core.Constants.EInvoicingBatchState.Sent;
			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoice.PK;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.SetCompanyAndCountryCode(company);

			Factory.SaveForTesting();

			AssertEquals("Precondition", "BCH", pivot.AIP_Status);
			AssertEquals("Precondition", "", pivot.AIP_ErrorDescription);

			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var manager = new AccountingInvoiceDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, invoice);
			AssertEquals("Postcondition", "BCH", pivot.AIP_Status);
			AssertEquals("Postcondition", "", pivot.AIP_ErrorDescription);
			AssertEquals("Error - Cannot process message for unknown sub type 'Canada'", logger.ToString());
		}

		public void TestOnLogParentFoundFromEDIMessageWithInvalidObject()
		{
			var eventXmlText = @"<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>AR INV 00001000</Key>
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
           <MessageSubType>Canada</MessageSubType>
        </EventParameters>
        <ContextCollection>
          <Context>
            <Type>ErrorCode</Type>
            <Value>3000</Value>
          </Context>
          <Context>
            <Type>ErrorDescription</Type>
            <Value>Factura Duplicada</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var manager = new AccountingInvoiceDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, Factory.NewWithValidTestData<AccTaxRate>());
			AssertContains("Error - Unable to process - unsupported type detected, Parent BO type is", logger.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_AuthorisedCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "OrgAAA";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GC = company.PK;
			invoice.AH_GB = branch.PK;
			invoice.AH_TransactionCount = 1;
			invoice.AH_OH = debtor.PK;
			invoice.AH_InvoiceDate = new ZDateTime(2022, 8, 1, 09, 21, 51);
			Factory.SaveForTesting();

			EInvoicingEventMessageProcessorForInvoiceTest.EnableCustomFunctionality(isPilotFunctionality: false, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			AssertEquals("Invoice has no ATH log.", 0, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertNull("Invoice has no AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(Factory.BOFactory, invoice.PK, company.GC_RN_NKCountryCode));

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccountingInvoice, $"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}");
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.ContextCollection = new List<Context>();

			// Required contexts for creating AuthorisationRecord
			eventDataObject.ContextCollection.Add(new Context() { Type = EventContextTypeCode.AHF_Counter, Value = "1" });
			eventDataObject.ContextCollection.Add(new Context() { Type = EventContextTypeCode.AHF_Number, Value = "100" });
			eventDataObject.ContextCollection.Add(new Context() { Type = EventContextTypeCode.AHF_DateTime, Value = "2022-08-01T09:30:10" });
			eventDataObject.ContextCollection.Add(new Context() { Type = EventContextTypeCode.AHF_IDType, Value = "ID" });

			eventDataObject.EventType = Events.AuthorisedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = EventDataConstants.ElectronicInvoicingEventType_SingleTransactionHeader,
			};

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals("invoice should be processed", 1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertNotNull("Invoice has AuthorisationRecord", AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(Factory.BOFactory, invoice.PK, company.GC_RN_NKCountryCode));
		}

		protected override InvoicingBase GetNewBusinessObjectForTesting()
		{
			InvoicingBase result = Factory.NewWithValidTestData<ARInvoice>();
			Factory.SaveForTesting();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory.BOFactory);

			setupControlAccounts();
		}

		TestObjectCreator TestObjectCreator { get; set; }

		void setupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = TestObjectCreator.CreateCFXAccount();
			Factory.SaveForTesting();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
		}
	}
}
