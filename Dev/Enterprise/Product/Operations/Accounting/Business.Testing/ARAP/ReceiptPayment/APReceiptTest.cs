using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APReceipt))]
	public class APReceiptTest : ReceiptTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<APReceipt>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override string ExceptedWorkflowType => WorkflowDescriptors.APReceiptWorkflowDescriptorCode;

		public override void TestDefaultDescription()
		{
			AssertEquals("Default description should be AP RECEIPT", "AP RECEIPT", ReceiptPaymentBase.AH_Desc);
		}

		public override void TestDefaultBankAccount()
		{
			DefaultBankAccountAPTest();
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
			Assert("Should contain the APOrg", ReceiptPaymentBase.Lookups.Headers.Contains(aPOrg));
			Assert("Should not contain the AROrg", !ReceiptPaymentBase.Lookups.Headers.Contains(aROrg));
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PRC", ((IDocManagerSupport)Factory.New<APReceipt>()).DocManagerInfo.DocManagerCode);
		}
	}
}
