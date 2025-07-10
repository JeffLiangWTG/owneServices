using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARPayment))]
	public class ARPaymentTest : PaymentTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARPayment>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override string ExceptedWorkflowType => WorkflowDescriptors.ARPaymentWorkflowDescriptorCode;

		public void TestLoadDocumentCommands()
		{
			ARPayment aRPayment = Factory.New<ARPayment>();
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Remittance Advice");
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, "Legacy Documents");
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(aRPayment);
			documentCommands.Load();
			BusinessObject[] paymentCommands = documentCommands.Find(filter);
			AssertEquals("Should be one legacy command found", 1, paymentCommands.Length);

			filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Remittance Advice");
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, "");
			paymentCommands = documentCommands.Find(filter);
			AssertEquals("Should be one non legacy command found", 1, paymentCommands.Length);
		}

		public override void TestDefaultDescription()
		{
			AssertEquals("Default description should be AR PAYMENT", "AR PAYMENT", ReceiptPaymentBase.AH_Desc);
		}

		public override void TestDefaultBankAccount()
		{
			DefaultBankAccountARTest();
		}

		public override void TestOrgHeaders()
		{
			OrgHeader aPOrg = Factory.NewWithValidTestData<OrgHeader>();
			aPOrg.OH_IsCreditor = true;
			aPOrg.OH_IsDebtor = false;

			OrgHeader aROrg = Factory.NewWithValidTestData<OrgHeader>();
			aROrg.OH_IsCreditor = false;
			aROrg.OH_IsDebtor = true;

			Factory.Save();

			ReceiptPaymentBase.Lookups.Headers.Load();
			Assert("Should contain the AROrg", ReceiptPaymentBase.Lookups.Headers.Contains(aROrg));
			Assert("Should not contain the APOrg", !ReceiptPaymentBase.Lookups.Headers.Contains(aPOrg));
		}

		public override void TestMatchingBaseObject()
		{
			base.TestMatchingBaseObject();
			Assert("Payment Matcher should be for AR", fMatchingBaseObject is ARMatchingBase);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "RPA", ((IDocManagerSupport)Factory.New<ARPayment>()).DocManagerInfo.DocManagerCode);
		}
	}
}
