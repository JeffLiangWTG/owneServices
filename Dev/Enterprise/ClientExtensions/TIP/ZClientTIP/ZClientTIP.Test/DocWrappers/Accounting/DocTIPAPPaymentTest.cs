using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.DocWrappers.Testing
{
	[TestedType(typeof(DocTIPAPPayment))]
	public class DocTIPAPPaymentTest : DocumentWrapperTestCase
	{
		public void TestMatchLink()
		{
			APPayment payment = GetPayment(true);
			Factory.Save();
			DocTIPAPPayment paymentWrapper = DocTIPAPPayment.New(payment, Factory);
			AssertNotNull(paymentWrapper.MatchLink);
		}

		public void TestChargeLines()
		{
			APPayment payment = GetPayment(true);
			Factory.Save();
			DocTIPAPPayment paymentWrapper = DocTIPAPPayment.New(payment, Factory);
			AssertEquals("ChargeLines.Count", 1, paymentWrapper.ChargeLines.Count);
			DocTIPHeaderLineTransaction headerLine = paymentWrapper.ChargeLines[0];
			AssertEquals("HeaderLine.JobNumber", "Sx0001001", headerLine.JobNumber);
			AssertEquals("HeaderLine.Desc", "Test Invoice", headerLine.Desc);
			AssertEquals("HeaderLine.ChargeCode", "ZZCC1", headerLine.ChargeCode);
		}

		public void TestChargeLinesPaymentWithNoLines()
		{
			APPayment payment = GetPayment(false);
			Factory.Save();
			DocTIPAPPayment paymentWrapper = DocTIPAPPayment.New(payment, Factory);
			AssertEquals("Should have one line", true, paymentWrapper.ChargeLines.Count > 0);
			DocTIPHeaderLineTransaction headerLine = paymentWrapper.ChargeLines[0];
			AssertEquals(ZString.Empty, headerLine.JobNumber);
		}

		public void TestMenuFilterTests()
		{
			APPayment payment = GetPayment(true);
			DocTIPAPPayment paymentWrapper = DocTIPAPPayment.New(payment, Factory);
			AssertEquals("PrintStandard should be false", ZBool.False, paymentWrapper.PrintStandard);
			AssertEquals("PrintClientSpecific should be true", ZBool.True, paymentWrapper.PrintClientSpecific);
		}

		APPayment GetPayment(bool invoiceWithLine)
		{
			APPayment result = Factory.New<APPayment>();
			result.AH_InvoiceDate = ZDateTime.Today;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			result.AH_RX_NKTransactionCurrency = "AUD";
			result.AH_TransactionNum = "10001";
			TransactionMatchLink paymentMatchLink = ((IMatching)result).CurrentMatchGroup.AddNew();
			paymentMatchLink.AP_AH = result.PK;
			paymentMatchLink.AP_MatchDate = ZDateTime.Today;
			paymentMatchLink.AP_MatchGroupNum = "Z0001987";
			TransactionMatchLink paymentMatchLink1 = ((IMatching)result).CurrentMatchGroup.AddNew();
			paymentMatchLink1.AP_AH = GetInvoice(invoiceWithLine).PK;
			paymentMatchLink1.AP_MatchDate = ZDateTime.Today;
			paymentMatchLink1.AP_MatchGroupNum = "Z0001987";
			return result;
		}

		APInvoice GetInvoice(bool withLine)
		{
			TestObjectCreator helper = new TestObjectCreator(Factory);
			Job job = helper.CreateJob("Sx0001001", helper.AALSHI, 0M, helper.ABIGAS, 0M);
			APInvoice result = (APInvoice)helper.CreateInvoice(typeof(APInvoice), helper.AUD, 1M);
			if (withLine)
			{
				APInvoiceLine invoiceLine = helper.CreateAPInvoiceLine(result, job, helper.CC1, helper.AUD, 1M, "APInvoiceLine", 100M);
				helper.CreateJobCharge(invoiceLine, job, helper.CC1, helper.AUD);
			}

			return result;
		}

		#region Overrides
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocTIPAPPayment.New(Factory.New<APPayment>(), Factory) };
		}
		#endregion
	}
}
