using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoicingWorkflowDescriptor))]
	public class APInvoicingWorkflowDescriptorTest : InvoicingBaseWorkflowDescriptorTest<APInvoicingWorkflowDescriptor>
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(InvoicingBase), WorkflowDescriptor.WorkflowProviderType);
		}

		public void TestSupportsBufferManagement()
		{
			Assert(WorkflowDescriptor.SupportsBufferManagement);
		}

		public void TestControllerID_ShouldReturnAPInvoiceController()
		{
			AssertEquals(ControllerIDs.APInvoice, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.APInvoiceCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AP Invoice", WorkflowDescriptor.Description);
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
			AssertNull(resultProcessor);

			processTask = invoice.WorkflowItems.Triggers.AddNew();
			action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode });
			Assert("Processor for SendUniversalTransactionXML should be IUniversalXmlWorkflowProcessor", resultProcessor is IUniversalXmlWorkflowProcessor);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.APInvoice, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
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
				mode.EK_Module = WorkflowDescriptors.APInvoiceCode;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
				mode.EK_Destination = provider.Header.OH_FullNameTruncated + "@notificationemail.cargowise.com";
			}
		}
	}
}
