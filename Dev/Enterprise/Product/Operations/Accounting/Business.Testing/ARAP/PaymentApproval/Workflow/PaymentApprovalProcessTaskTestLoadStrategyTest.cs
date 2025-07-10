using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class PaymentApprovalProcessTaskTestLoadStrategyTest : TestCaseWithFactory
	{
		public void TestTypeDeciderWorksForARAPPaymentApproval()
		{
			var apPaymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), TransactionTypes.Payment, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			var arPaymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(ARPaymentApprovalWithAuthorisation), TransactionTypes.Payment, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			var apTask = MasterFilesTestHelper.CreateTask(apPaymentApproval);
			var arTask = MasterFilesTestHelper.CreateTask(arPaymentApproval);
			apTask.P9_Description = "AP Payment Approval";
			arTask.P9_Description = "AR Payment Approval";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.Load<APPaymentApprovalWithAuthorisation>(apPaymentApproval.PK);
			newFactory.Load<ARPaymentApprovalWithAuthorisation>(arPaymentApproval.PK);

			var typeDecider = ProcessTaskTypeDecider.GetInstance();
			var apResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new APPaymentApprovalWorkflowDescriptor()));
			var arResults = newFactory.Load<ProcessTask>(typeDecider.GetQueryForLoad(new ARPaymentApprovalWorkflowDescriptor()));

			AssertEquals(1, apResults.Length);
			AssertEquals(1, arResults.Length);
			AssertEquals("AP Payment Approval", apResults[0].P9_Description);
			AssertEquals("AR Payment Approval", arResults[0].P9_Description);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
