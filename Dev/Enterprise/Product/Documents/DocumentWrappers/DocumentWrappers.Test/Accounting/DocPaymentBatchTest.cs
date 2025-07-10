using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocPaymentBatch))]
	sealed class DocPaymentBatchTest : DocumentWrapperTestCase
	{
		[TestDate(2020, 1, 1)]
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "STRAWBERRY";

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_Code = "CBA";

			var check = Factory.NewWithValidTestData<AccChequeBook>();
			check.AK_Code = "XAB";

			Factory.Save();

			var batchWrapper = DocPaymentBatch.New(CreateNewBatch(bank, check, org), Factory);
			var generictTransactions = batchWrapper.GenericTransactions.Cast<DocGenericTransactionHeader>().ToArray();

			CombineAssertions(() =>
			 {
				 AssertEquals("TransactionNumber", "00001001", batchWrapper.TransactionNumber);
				 AssertEquals("TransactionAPPaymentMethod", "CHQ", batchWrapper.TransactionAPPaymentMethod);
				 AssertEquals("BankAccountCode", "CBA", batchWrapper.BankAccountCode);
				 AssertEquals("CheckBookCode", "XAB", batchWrapper.CheckBookCode);
				 AssertEquals("CreatingUser", "CargoWise Support", batchWrapper.CreatingUser);
				 AssertEquals("CreatedDate", new ZDateTime(2020, 1, 1), new ZDate(batchWrapper.CreatedDate).ToZDateTime());
				 AssertEquals("FullyPaidDate", new ZDateTime(2020, 1, 1), batchWrapper.FullyPaidDate);
				 AssertEquals("TotalLocalInvoiceAmount", 300m, batchWrapper.TotalLocalInvoiceAmount);
				 AssertEquals("GenericTransactions", 3, batchWrapper.GenericTransactions.Count);
			 });
		}

		APInvoice createInvoice(OrgHeader oh)
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OSExTaxAmount = 100M;
			invoice.AH_OH = oh.PK;
			invoice.AH_RequisitionStatus = "XYZ";
			invoice.AH_DueDate = new ZDateTime(2020, 3, 4);
			invoice.AH_RequisitionDate = new ZDateTime(2020, 2, 3);
			return invoice;
		}

		AccPaymentApproval createApproval(AccPaymentBatch pb, APInvoice invoice, OrgHeader oh)
		{
			var approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_APB_PaymentBatch = pb.PK;
			approval.AV_OH = oh.PK;
			approval.AV_AB = pb.APB_AB;
			approval.AV_AK = pb.APB_AK;
			approval.AV_PaymentType = pb.APB_PaymentType;
			approval.AV_PaymentDate = pb.APB_PaymentDate;
			approval.AV_PostDate = pb.APB_PostDate;
			approval.AV_Amount = 100m;

			var approvalItem = Factory.New<PaymentApprovalItem>();
			approvalItem.A2_PaymentThisRun = 100M;
			approvalItem.A2_AH = invoice.PK;
			approvalItem.A2_AV = approval.PK;
			return approval;
		}

		void addInvoiceWithApproval(AccPaymentBatch pb, OrgHeader oh)
		{
			createApproval(pb, createInvoice(oh), oh);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocPaymentBatch.New(CreateNewBatch(TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook, TestObjectCreator.ActiveOrg), Factory) };
		}

		AccPaymentBatch CreateNewBatch(AccBankAccount bank, AccChequeBook check, OrgHeader org)
		{
			var batch = Factory.New<AccPaymentBatch>();
			batch.APB_AB = bank.PK;
			batch.APB_AK = check.PK;

			addInvoiceWithApproval(batch, org);
			addInvoiceWithApproval(batch, org);
			addInvoiceWithApproval(batch, org);
			Factory.Save();

			return batch;
		}
	}
}
