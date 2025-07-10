using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ARComplianceDocumentWorkflowDescriptor))]
	public class ARComplianceDocumentWorkflowDescriptorTest : AccComplianceDocumentWorkflowDescriptorTest<ARComplianceDocumentWorkflowDescriptor>
	{
		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.ARComplianceDocument, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(ARComplianceDocumentHeader), WorkflowDescriptor.WorkflowProviderType);
		}

		public void TestControllerID_ShouldReturnARComplianceDocumentController()
		{
			AssertEquals(ControllerIDs.ARComplianceDocument, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ARComplianceDocumentCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AR Compliance Document", WorkflowDescriptor.Description);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			return new IWorkflowProvider[] { header };
		}
	}
}
