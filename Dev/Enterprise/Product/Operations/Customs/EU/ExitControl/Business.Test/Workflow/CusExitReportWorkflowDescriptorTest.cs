using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportWorkflowDescriptor))]
	sealed class CusExitReportWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusExitReportWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals(WorkflowDescriptors.CusExitReportWorkflowDescriptorCode, WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("Exit Report Line Trigger", WorkflowDescriptor.Description);

		public override void TestSupportsEventTracking() => AssertEquals(expected: true, WorkflowDescriptor.SupportsEventTracking);

		public override void TestSupportsWorkflowTemplates() => AssertEquals(expected: false, WorkflowDescriptor.SupportsWorkflowTemplates);

		public override void TestSubTypes() => AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

		public override void TestRequiresPorts()
		{
			AssertEquals(expected: false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(expected: false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestWorkflowProviderType() => AssertEquals(typeof(CusExitReport), WorkflowDescriptor.WorkflowProviderType);

		public override void TestRequiresClient() => AssertEquals(expected: false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresBranch() => AssertEquals(expected: false, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresDepartment() => AssertEquals(expected: false, WorkflowDescriptor.RequiresDepartment);

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			cusExitHeader.CXH_OH_Exporter = Factory.NewWithValidTestData<OrgHeader>().PK;
			cusExitHeader.CXH_OA_Carrier = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			return new[] { cusExitHeader };
		}
	}
}
