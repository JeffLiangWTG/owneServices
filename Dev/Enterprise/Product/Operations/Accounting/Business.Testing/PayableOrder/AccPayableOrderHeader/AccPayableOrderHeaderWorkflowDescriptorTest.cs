using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	[TestedType(typeof(AccPayableOrderHeaderWorkflowDescriptor))]
	public class AccPayableOrderHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<AccPayableOrderHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "POD", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Payable Order", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Order Type", "Order Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			CodeDescriptionPairList transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals(5, transportModeList.Count);
			AssertEquals("", transportModeList[0].Code);
			AssertEquals("All", transportModeList[0].Description);
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

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.New<AccPayableOrderHeader>() };
		}
	}
}
