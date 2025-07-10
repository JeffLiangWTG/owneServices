using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageWorkflowDescriptor))]
	public class IncidentTriageWorkflowDescriptorTest : WorkflowDescriptorTestCase<IncidentTriageWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals(IncidentTriageConstants.WorkflowDescriptorInformation.Description, WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(IncidentTriageConstants.WorkflowDescriptorInformation.Code, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals("RequiresBranch should be false", false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals("RequiresClient should be false", false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals("RequiresDepartment should be false", true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", false, WorkflowDescriptor.RequiresPort1);
			AssertEquals("RequiresPort2", false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1", "Product", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "Product Area", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "Sec./Svc./Req.", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking should be true", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<IncidentTriage>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email; }
		}
	}
}
