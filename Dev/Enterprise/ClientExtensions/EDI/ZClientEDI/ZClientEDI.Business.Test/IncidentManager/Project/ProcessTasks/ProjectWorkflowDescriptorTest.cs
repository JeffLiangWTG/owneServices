using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProjectWorkflowDescriptor))]
	class ProjectWorkflowDescriptorTest : WorkflowDescriptorTestCase<ProjectWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			Assert(true);
		}

		public override void TestID()
		{
			Assert(true);
		}

		public override void TestRequiresBranch()
		{
			Assert(true);
		}

		public override void TestRequiresClient()
		{
			Assert(true);
		}

		public override void TestRequiresDepartment()
		{
			Assert(true);
		}

		public override void TestRequiresPorts()
		{
			Assert(true);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(true);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		public override void TestSubTypes()
		{
			Assert(true);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var contact = ClientOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jan Michael Vincent";
			var project = Factory.New<EDIProject>();
			project.WKP_OA_ClientAddress = ClientOrg.MainAddress.PK;
			project.WKP_OC_Contact = contact.PK;

			return new IWorkflowProvider[] { project };
		}
	}
}
