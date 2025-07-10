using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeaderWorkflowDescriptor))]
	sealed class CusExitHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusExitHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Exit Control", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
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
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestSupportedTriggerLineTypes() => AssertContainsExactElementsInAnyOrder(new[] { TriggerLineTypes.Codes.CusExitReport }, ((CusExitHeaderWorkflowDescriptor)WorkflowDescriptor).SupportedTriggerLineTypes);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			return new[] { cusExitHeader };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => new[] { CusExitReportSchema.CER_MessageStatus, CusExitReportSchema.CER_Status };

		public new void TestGetFieldColumnDescription()
		{
			AssertEquals("Message Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusExitReportSchema.CER_MessageStatus));
			AssertEquals("Status", WorkflowDescriptor.GetFieldColumnDescription(Factory, CusExitReportSchema.CER_Status));

			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
				AssertEquals("A description for field column '" + fieldColumn.Name + "' must be specified", false, description.Contains("_"));
			}
		}
	}
}
