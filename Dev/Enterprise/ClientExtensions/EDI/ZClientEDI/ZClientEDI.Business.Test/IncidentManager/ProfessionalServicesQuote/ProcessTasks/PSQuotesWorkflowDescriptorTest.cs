using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(PSQuotesWorkflowDescriptor))]
	class PSQuotesWorkflowDescriptorTest : WorkflowDescriptorTestCase<PSQuotesWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", EDIJobInvoicingConsumerTypes.PSQuote.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", EDIJobInvoicingConsumerTypes.PSQuote.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Quotation Type", "Quotation Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2 is Program Area", "Program Area", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3 is Product", "Product", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
		}

		public override void TestRequiresPorts()
		{
			Assert(WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
			AssertEquals("Client Location", WorkflowDescriptor.Port1Name);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			Assert(WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			Assert(WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			ProfessionalServicesQuote quote = Factory.New<ProfessionalServicesQuote>();
			quote.IM_OH_Client = ClientOrg.PK;
			return new IWorkflowProvider[] { quote };
		}

		bool? OriginalValueOfServiceTaskBizoBinding;

		protected override void SetUp()
		{
			base.SetUp();
			OriginalValueOfServiceTaskBizoBinding = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;
			SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = false;
			EDIClientDbSchemaUpgradeForTest.UpgradeViewClientProcessHeader(TestConnection);
		}
		protected override void TearDown()
		{
			base.TearDown();
			if (OriginalValueOfServiceTaskBizoBinding.HasValue)
			{
				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = OriginalValueOfServiceTaskBizoBinding.Value;
			}
		}
	}
}
