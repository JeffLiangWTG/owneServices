using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupWorkflowDescriptor))]
	public class IncidentManagementGroupWorkflowDescriptorTest : WorkflowDescriptorTestCase<IncidentManagementGroupWorkflowDescriptor>
	{
		public void TestBusinessContext()
		{
			AssertEquals(1, WorkflowDescriptor.DocumentBusinessContext.Length);
			AssertEquals(BusinessContext.IncidentGroup, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", IncidentManagementGroupConstants.WorkflowDescriptorInformation.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", IncidentManagementGroupConstants.WorkflowDescriptorInformation.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1", "Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2", "Product", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Sub Type 3", "Product Area", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", false, WorkflowDescriptor.RequiresPort1);
			AssertEquals("RequiresPort2", false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals("RequiresClient", false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals("RequiresBranch", false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals("RequiresDepartment", false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<IncidentManagementGroup>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}
	}
}
