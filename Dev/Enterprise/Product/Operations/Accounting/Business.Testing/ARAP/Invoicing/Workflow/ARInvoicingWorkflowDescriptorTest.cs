using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoicingWorkflowDescriptor))]
	public class ARInvoicingWorkflowDescriptorTest : InvoicingBaseWorkflowDescriptorTest<ARInvoicingWorkflowDescriptor>
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(InvoicingBase), WorkflowDescriptor.WorkflowProviderType);
		}

		public void TestSupportsBufferManagement()
		{
			Assert(WorkflowDescriptor.SupportsBufferManagement);
		}

		public void TestControllerID_ShouldReturnARInvoiceController()
		{
			AssertEquals(ControllerIDs.ARInvoice, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ARInvoiceCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AR Invoice", WorkflowDescriptor.Description);
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			IWorkflowProvider invoice = GetParentsWithConfiguredOrganisationPartiesForTest()[0];
			ProcessTask processTask = invoice.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			IProcessor resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendXML should be XmlMessageDeliver", resultProcessor is XmlMessageDeliver);

			processTask = invoice.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendXMLDebtorBalance should be XmlMessageDeliver", resultProcessor is XmlMessageDeliver);

			processTask = invoice.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode });
			Assert("Processor for SendUniversalTransactionXML should be IUniversalXmlWorkflowProcessor", resultProcessor is IUniversalXmlWorkflowProcessor);
		}

		public void TestSendXMLDebtorBalanceTriggerAction()
		{
			var triggerActionType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			var fileFormat = EDICommunicationsModeFileFormatList.Codes.XMB;
			var expectedEDIMessageType = EDIMessageTypeList.Codes.XMS;
			var expectedEDIMessageSubType = EDIMessageSubTypeXMLElementList.Codes.DebtorBalances;
			AssertTriggerActionWithEAdaptorInterfaceCommunicationMode(triggerActionType, fileFormat, expectedEDIMessageType, expectedEDIMessageSubType);
		}

		public void TestSendXMLTriggerAction()
		{
			var triggerActionType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			var fileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			var expectedEDIMessageType = EDIMessageTypeList.Codes.XMS;
			var expectedEDIMessageSubType = EDIMessageSubTypeXMLElementList.Codes.FinancialTransactions;
			AssertTriggerActionWithEAdaptorInterfaceCommunicationMode(triggerActionType, fileFormat, expectedEDIMessageType, expectedEDIMessageSubType);
		}

		void AssertTriggerActionWithEAdaptorInterfaceCommunicationMode(string triggerActionType, string fileFormat, string expectedEDIMessageType, string expectedEDIMessageSubType)
		{
			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			invoice.AH_OH = org.PK;

			var workflowProvider = invoice as IWorkflowProvider;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			trigger.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = triggerActionType;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.InvoiceDebtor;

			var mode = org.EDICommunicationsModes.AddNew();
			mode.EK_Module = WorkflowDescriptors.ARInvoiceCode;
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			mode.EK_Destination = "ANYWHERE";
			mode.EK_FileFormat = fileFormat;

			invoice.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Precondition: This assertion is for actions that use the XmlMessageDeliver processor", processor is XmlMessageDeliver);

			MasterFilesTestHelper.RunLogWalker();

			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(expectedEDIMessageType, ediMessage.EM_MessageType);
			AssertEquals(expectedEDIMessageSubType, ediMessage.EM_MessageSubType);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.ARInvoice, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestSupportedMessageRecipientPartiesForSpecificAction()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_IsShippingProvider = false;
			var xutTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, null, null);
			AssertEquals(3, xutTriggerParties.Count);
			AssertEquals("Organization Proxy", xutTriggerParties[0].Description);
			AssertEquals("Email", xutTriggerParties[1].Description);
			AssertEquals("Invoice Debtor", xutTriggerParties[2].Description);

			GlbCompany.CurrentCompany.OrgProxy.OH_IsShippingProvider = true;
			xutTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, null, null);
			AssertEquals(4, xutTriggerParties.Count);
			AssertEquals("Organization Proxy", xutTriggerParties[0].Description);
			AssertEquals("Email", xutTriggerParties[1].Description);
			AssertEquals("Invoice Debtor", xutTriggerParties[2].Description);
			AssertEquals("Carrier Messaging Debtor", xutTriggerParties[3].Description);

			var xueTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, null, null);
			AssertEquals(3, xueTriggerParties.Count);
			AssertEquals("Organization Proxy", xueTriggerParties[0].Description);
			AssertEquals("Email", xutTriggerParties[1].Description);
			AssertEquals("Invoice Debtor", xueTriggerParties[2].Description);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.InvoiceDebtor |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new CodeDescriptionPair[] { new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendARInvoice, WorkflowTriggerActionTypeConstants.Descriptions.SendARInvoice),
				new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs, WorkflowTriggerActionTypeConstants.Descriptions.GenerateARInvoiceToEdocs)
				};
			}
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			return invoice;
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			InvoicingBase provider = workflowProvider as InvoicingBase;
			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.InvoiceDebtor && provider != null)
			{
				provider.AH_OH = provider.Factory.NewWithValidTestData<OrgHeader>().PK;
				EDICommunicationsMode mode = provider.Header.EDICommunicationsModes.AddNew();
				mode.EK_Module = WorkflowDescriptors.ARInvoiceCode;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
				mode.EK_Destination = provider.Header.OH_FullNameTruncated + "@notificationemail.cargowise.com";
			}
		}
	}
}
