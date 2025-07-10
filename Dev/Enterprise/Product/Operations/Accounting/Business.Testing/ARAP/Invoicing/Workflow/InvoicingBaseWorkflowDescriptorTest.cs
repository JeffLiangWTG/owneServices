using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestsSubclassesOf(typeof(InvoicingBaseWorkflowDescriptor))]
	public abstract class InvoicingBaseWorkflowDescriptorTest<T> : WorkflowDescriptorTestCase<T> where T : InvoicingBaseWorkflowDescriptor, new()
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(InvoicingBase), WorkflowDescriptor.WorkflowProviderType);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return true; }
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected TestObjectCreator TestObjectCreator;
	}
}
