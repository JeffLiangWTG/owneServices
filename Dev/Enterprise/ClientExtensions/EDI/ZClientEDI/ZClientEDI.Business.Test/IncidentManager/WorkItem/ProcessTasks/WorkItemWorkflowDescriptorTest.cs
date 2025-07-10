using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkItemWorkflowDescriptor))]
	class WorkItemWorkflowDescriptorTest : WorkflowDescriptorTestCase<WorkItemWorkflowDescriptor>
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

		public override void TestSubTypes()
		{
			Assert(true);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var workItem = Factory.New<NewWorkItem>();
			return new IWorkflowProvider[] { workItem };
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new SchemaColumn[]
		{
			WorkItemSchema.WKI_ActivitySubtype,
			WorkItemSchema.WKI_ActivityType,
			WorkItemSchema.WKI_GB_AssignedBranch,
			WorkItemSchema.WKI_GC_AssignedCompany,
			WorkItemSchema.WKI_GE_AssignedDepartment,
			WorkItemSchema.WKI_PortOrCountry,
			WorkItemSchema.WKI_Priority,
			WorkItemSchema.WKI_Status,
			WorkItemSchema.WKI_Summary,
			WorkItemSchema.WKI_WorkItemArea,
			WorkItemSchema.WKI_WorkItemType,
		};
	}
}
