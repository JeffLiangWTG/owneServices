using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageJobHeaderWorkflowDescriptor))]
	public class CusTempStorageJobHeaderWorkflowDescriptorTest : Enterprise.MasterFiles.Business.Testing.WorkflowDescriptorTestCase<CusTempStorageJobHeaderWorkflowDescriptor>
	{
		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		public override void TestID()
		{
			AssertEquals("CTJ", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Customs Temp. Storage Job Header", WorkflowDescriptor.Description);
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
			AssertEquals(typeof(CusTempStorageJobHeader), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				CusTempStorageJobHeaderWithConfiguredOrganisationParties,
			};
		}

		CusTempStorageJobHeader CusTempStorageJobHeaderWithConfiguredOrganisationParties
		{
			get
			{
				if (cusTempStorageJobHeaderWithConfiguredOrganisationParties == null)
				{
					cusTempStorageJobHeaderWithConfiguredOrganisationParties = Factory.New<CusTempStorageJobHeader>();
				}
				return cusTempStorageJobHeaderWithConfiguredOrganisationParties;
			}
		}

		CusTempStorageJobHeader cusTempStorageJobHeaderWithConfiguredOrganisationParties;
	}
}
