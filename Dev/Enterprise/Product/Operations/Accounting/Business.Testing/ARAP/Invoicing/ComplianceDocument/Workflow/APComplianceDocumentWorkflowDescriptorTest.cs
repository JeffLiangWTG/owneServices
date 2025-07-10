using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APComplianceDocumentWorkflowDescriptor))]
	public class APComplianceDocumentWorkflowDescriptorTest : AccComplianceDocumentWorkflowDescriptorTest<APComplianceDocumentWorkflowDescriptor>
	{
		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.APComplianceDocument, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(APComplianceDocumentHeader), WorkflowDescriptor.WorkflowProviderType);
		}
		public void TestControllerID_ShouldReturnAPComplianceDocumentController()
		{
			AssertEquals(ControllerIDs.APComplianceDocument, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.APComplianceDocumentCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AP Compliance Document", WorkflowDescriptor.Description);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			return new IWorkflowProvider[] { header };
		}
	}
}
