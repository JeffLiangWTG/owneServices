using CargoWise.Definitions;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APPaymentWorkflowDescriptor))]
	public class APPaymentWorkflowDescriptorTest : ReceiptPaymentBaseWorkflowDescriptorTest<APPaymentWorkflowDescriptor>
	{
		public override void TestWorkflowProviderType()
		{
			AssertEquals("Correct WorkflowProviderType", typeof(Payment), WorkflowDescriptor.WorkflowProviderType);
		}

		public void TestControllerID_ShouldReturnAPInvoiceController()
		{
			AssertEquals(ControllerIDs.ZAPPayment, WorkflowDescriptor.ControllerID);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.APPaymentWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "AP Payment", WorkflowDescriptor.Description);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			return new IWorkflowProvider[] { payment };
		}

		public void TestDocumentBusinessContext()
		{
			var documentSupporter = GetParentsWithConfiguredOrganisationPartiesForTest()[0] as IDocumentSupportable;
			AssertEquals(1, WorkflowDescriptor.DocumentBusinessContext.Length);
			AssertEquals("Correct DocumentBusinessContext", BusinessContext.APTransaction, WorkflowDescriptor.DocumentBusinessContext[0]);
			AssertEquals("DocumentBusinessContext should be same with documentSupporter.BusinessContext", documentSupporter.DocumentSupporter.BusinessContext, WorkflowDescriptor.DocumentBusinessContext[0]);
		}
	}
}
