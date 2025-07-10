using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeaderWorkflowDescriptor))]
	public class TemporaryStorageHeaderWorkflowDescriptorTest : Enterprise.MasterFiles.Business.Testing.WorkflowDescriptorTestCase<TemporaryStorageHeaderWorkflowDescriptor>
	{
		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		public override void TestID()
		{
			AssertEquals("TSH", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("UCC6 Temporary Storage Header", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(TemporaryStorageHeader), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				TemporaryStorageHeaderWithConfiguredOrganisationParties,
			};
		}

		TemporaryStorageHeader TemporaryStorageHeaderWithConfiguredOrganisationParties
		{
			get
			{
				if (temporaryStorageHeaderWithConfiguredOrganisationParties == null)
				{
					temporaryStorageHeaderWithConfiguredOrganisationParties = Factory.New<TemporaryStorageHeader>();
				}
				return temporaryStorageHeaderWithConfiguredOrganisationParties;
			}
		}

		TemporaryStorageHeader temporaryStorageHeaderWithConfiguredOrganisationParties;
	}
}
