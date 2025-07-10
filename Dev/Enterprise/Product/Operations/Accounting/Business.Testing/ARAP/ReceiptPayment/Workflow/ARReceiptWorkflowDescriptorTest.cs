using CargoWise.Definitions;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARReceiptWorkflowDescriptor))]
	public class ARReceiptWorkflowDescriptorTest : ReceiptPaymentBaseWorkflowDescriptorTest<ARReceiptWorkflowDescriptor>
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(Receipt), WorkflowDescriptor.WorkflowProviderType);
		}

		public void TestControllerID_ShouldReturnAPInvoiceController()
		{
			AssertEquals(ControllerIDs.ZARReceipt, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ARReceiptWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AR Receipt", WorkflowDescriptor.Description);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var receipt = Factory.NewWithValidTestData<ARReceipt>();
			return new IWorkflowProvider[] { receipt };
		}

		public void TestDocumentBusinessContext()
		{
			var documentSupporter = GetParentsWithConfiguredOrganisationPartiesForTest()[0] as IDocumentSupportable;
			AssertEquals(1, WorkflowDescriptor.DocumentBusinessContext.Length);
			AssertEquals("Correct DocumentBusinessContext", BusinessContext.ARTransaction, WorkflowDescriptor.DocumentBusinessContext[0]);
			AssertEquals("DocumentBusinessContext should be same with documentSupporter.BusinessContext", documentSupporter.DocumentSupporter.BusinessContext, WorkflowDescriptor.DocumentBusinessContext[0]);
		}
	}
}
