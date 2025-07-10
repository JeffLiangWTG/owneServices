using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentApprovalDocumentSupporter))]
	public class PaymentApprovalDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetContactOrganisation()
		{
			PaymentApprovalBase testPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			AssertNull("No contact org yet", testPaymentApproval.PaymentApprovalDocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader);
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			AssertEquals("Contact Org for Document not null", TestOrgHeader.PK, testPaymentApproval.PaymentApprovalDocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestBusinessContext()
		{
			PaymentApprovalBase testPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			AssertEquals("BusinessContext", BusinessContext.PaymentApproval, testPaymentApproval.PaymentApprovalDocumentSupporter.BusinessContext);
		}

		public void TestNoCrashIfApprovalIsNotAttachedToAnyBatch()
		{
			var approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = PaymentApprovalDocumentSupporter.PaymentBatchListingMenuName;

			AssertNotNull(approval.PaymentApprovalDocumentSupporter.GetDocumentWrappersInternal_ForTestOnly(DataContext.GenericFreightJob, menu));
		}

		public void TestGetDocumentWrappersInternal()
		{
			PaymentApprovalBase testPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			testPaymentApproval.AV_PaymentComment = "AP PAYMENT DESCRIPTION";
			testPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			testPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			testPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			testPaymentApproval.AV_Amount = 1000M;

			DocumentWrapper[] wrappers = testPaymentApproval.PaymentApprovalDocumentSupporter.GetDocumentWrappersInternal_ForTestOnly(Core.Constants.DataContext.APPayment, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals(testPaymentApproval.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);

			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, testPaymentApproval, 1000M);
			testPaymentApproval.CreateNewPayment();

			wrappers = testPaymentApproval.PaymentApprovalDocumentSupporter.GetDocumentWrappersInternal_ForTestOnly(Core.Constants.DataContext.APPayment, null);
			AssertEquals(1, wrappers.Length);
			AssertEquals(testPaymentApproval.TransactionHeader.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		protected override void DoSetupForDocument(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
		{
			base.DoSetupForDocument(documentCommand, documentSupportableBO);
			if (documentCommand.SU_MenuName == PaymentApprovalDocumentSupporter.PaymentBatchListingMenuName && documentSupportableBO is APPaymentApprovalWithAuthorisation approval)
			{
				var batch = Factory.New<AccPaymentBatch>();
				approval.AV_APB_PaymentBatch = batch.PK;
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APPaymentApprovalWithAuthorisation>();
		}

		protected override ZString TestCountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			TestOrgHeader = Factory.New<OrgHeader>();
			TestOrgHeader.OH_Code = TestObjectCreator.GetRandomString(10);
			TestOrgHeader.OH_IsCreditor = true;
			Factory.Save();
		}

		OrgHeader TestOrgHeader;

		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		protected TestObjectCreator fTestObjectCreator;
	}
}
