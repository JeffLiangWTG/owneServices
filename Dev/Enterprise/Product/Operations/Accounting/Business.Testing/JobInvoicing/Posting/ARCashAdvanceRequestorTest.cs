using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.Posting
{
	public class ARCashAdvanceRequestorTest : TestCaseWithFactory
	{
		public void TestGenerateRequests()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			job.ExchangeRates.AddRate(creator.USD, 0.65m, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);

			for (int i = 0; i < 5; i++)
			{
				job.Charges.AddNew();
				job.Charges[i].JR_AC = creator.CC1.PK;
				job.Charges[i].JR_OH_SellAccount = creator.AALSHI.PK;
				job.Charges[i].JR_RX_NKSellCurrency = "AUD";
				job.Charges[i].JR_OSSellAmt = (i + 1) * 100;
				job.Charges[i].JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				job.Charges[i].JR_IsARCashAdvance = true;
			}

			job.Charges[1].JR_OH_SellAccount = job.Charges[3].JR_OH_SellAccount = creator.ABIGAS.PK;

			job.Charges[0].JR_RX_NKSellCurrency = job.Charges[2].JR_RX_NKSellCurrency = "USD";
			job.Charges[0].JR_InvoiceType = job.Charges[2].JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			job.Charges[0].JR_OSSellAmt = job.Charges[2].JR_OSSellAmt = 123.45m;
			AssertEquals(189.92m, job.Charges[0].JR_LocalSellAmt);
			AssertEquals(189.92m, job.Charges[2].JR_LocalSellAmt);

			Factory.Save();

			var requestor = new ARCashAdvanceRequestor(job);
			var result = requestor.GenerateRequests();

			AssertEquals(@"The following Advance Payment requests have been generated:
Debtor/Request ID/Currency/Total Amount
AALSHI, 00001000, USD, 246.90
ABIGAS, 00001001, AUD, 600.00
AALSHI, 00001002, AUD, 500.00", result.Item1);

			var requests = result.Item2;
			AssertEquals(3, requests.Count);

			var request1 = requests.First(x => x.CAH_RequestReferenceNumber == "00001000");
			AssertEquals(2, request1.Lines.Count);
			AssertCashAdvanceRequest(job, request1, creator.AALSHI.PK, "USD", 246.90m, 379.84m);

			var request2 = requests.First(x => x.CAH_RequestReferenceNumber == "00001001");
			AssertEquals(2, request2.Lines.Count);
			AssertCashAdvanceRequest(job, request2, creator.ABIGAS.PK, "AUD", 600.00m, 600.00m);

			var request3 = requests.First(x => x.CAH_RequestReferenceNumber == "00001002");
			AssertEquals(1, request3.Lines.Count);
			AssertCashAdvanceRequest(job, request3, creator.AALSHI.PK, "AUD", 500.00m, 500.00m);
		}

		void AssertCashAdvanceRequest(Job job, AccCashAdvanceRequestHeader header, ZGuid orgPK, ZString currency, ZDecimal osAmount, ZDecimal localAmount)
		{
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			AssertEquals(LedgerTypes.AccountsReceivable, header.CAH_Ledger);
			AssertEquals(GlbCompany.CurrentCompany.PK, header.CAH_GC_Company);
			AssertEquals(orgPK, header.CAH_OH_Organization);
			AssertEquals(job.PK, header.CAH_JH_Job);
			AssertEquals(currency, header.CAH_RX_NKTransactionCurrency);
			AssertEquals(osAmount, header.CAH_OSAmount);
			AssertEquals(localAmount, header.CAH_LocalAmount);
			AssertEquals(0m, header.CAH_OSPaidAmount);
			AssertEquals(0m, header.CAH_LocalPaidAmount);

			var lines = Factory.Load<AccCashAdvanceRequestLine>(new ZQuery(AccCashAdvanceRequestLineSchema.CAL_CAH_RequestHeader, header.PK));
			foreach (var line in lines)
			{
				var charge = job.Charges.Cast<Charge>().FirstOrDefault(c => c.JR_CAL_ARLine == line.PK);
				AssertNotNull(charge);
				AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, line.CAL_Status);
				AssertEquals(charge.JR_Sell_LocalSellAmount, line.CAL_LocalAmount);
				if (header.CAH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					AssertEquals(charge.JR_Calc_OSSellAmtWithGST, line.CAL_OSAmount);
				}
				else
				{
					AssertEquals(charge.JR_Sell_LocalSellAmount, line.CAL_OSAmount);
				}
				AssertEquals(0m, line.CAL_OSPaidAmount);
				AssertEquals(0m, line.CAL_LocalPaidAmount);
				AssertEquals(GlbCompany.CurrentCompany.PK, line.CAL_GC_Company);
			}
		}

		public void TestHeaderTransactionCurrencyDependsOnInvoiceType()
		{
			var foreignCurrencyInvoiceTypes = new string[] {
				InvoiceTypesList.Codes.FreightInvoice,
				InvoiceTypesList.Codes.FreightInvoice_Batching,
				InvoiceTypesList.Codes.ForeignCurrencyInvoice,
				InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
				AgencyInvoiceTypesList.Codes.ForeignCollect,
				AgencyInvoiceTypesList.Codes.ForeignCollect_Batching,
				AgencyInvoiceTypesList.Codes.ForeignPrePaid,
				AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching,
				InvoiceTypesList.Codes.SelfBillingInvoice };
			foreach (var invoiceType in foreignCurrencyInvoiceTypes)
			{
				AssertTransactionCurrencyDependsOnInvoiceType(invoiceType, "USD, 123.45");
			}

			var localCurrencyInvoiceTypes = new string[] {
				InvoiceTypesList.Codes.InvoicePerTaxCode,
				InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
				InvoiceTypesList.Codes.FinalInvoice,
				InvoiceTypesList.Codes.FinalInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInvoice,
				InvoiceTypesList.Codes.DisbursementInvoice_Batching,
				InvoiceTypesList.Codes.DestinationChargesInvoice,
				InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
				AgencyInvoiceTypesList.Codes.LocalCollect,
				AgencyInvoiceTypesList.Codes.LocalCollect_Batching,
				AgencyInvoiceTypesList.Codes.LocalPrePaid,
				AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching,
				AgencyInvoiceTypesList.Codes.Misc,
				AgencyInvoiceTypesList.Codes.Misc_Batching };
			foreach (var invoiceType in localCurrencyInvoiceTypes)
			{
				AssertTransactionCurrencyDependsOnInvoiceType(invoiceType, "AUD, 189.92");
			}
		}

		void AssertTransactionCurrencyDependsOnInvoiceType(ZString invoiceType, ZString expectedMessage)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			job.ExchangeRates.AddRate(creator.USD, 0.65m, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);

			var charge = job.Charges.AddNew();
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OH_SellAccount = creator.AALSHI.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = invoiceType;
			charge.JR_OSSellAmt = 123.45m;
			AssertEquals(189.92m, charge.JR_LocalSellAmt);

			charge.JR_IsARCashAdvance = true;
			Factory.Save();

			var requestor = new ARCashAdvanceRequestor(job);
			var result = requestor.GenerateRequests();

			Assert(result.Item1.EndsWith(expectedMessage));
		}

		public void TestRejectZeroBalanceCashAdvanceRequests()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			job.ExchangeRates.AddRate(creator.USD, 0.65m, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);

			for (var i = 0; i < 4; i++)
			{
				job.Charges.AddNew();
				job.Charges[i].JR_AC = creator.CC1.PK;
				job.Charges[i].JR_OH_SellAccount = creator.AALSHI.PK;
				job.Charges[i].JR_RX_NKSellCurrency = "AUD";
				job.Charges[i].JR_OSSellAmt = 0m;
				job.Charges[i].JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				job.Charges[i].JR_IsARCashAdvance = true;
			}

			job.Charges[2].JR_RX_NKSellCurrency = job.Charges[3].JR_RX_NKSellCurrency = "USD";
			job.Charges[2].JR_InvoiceType = job.Charges[3].JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			job.Charges[2].JR_OSSellAmt = job.Charges[3].JR_OSSellAmt = 0m;
			AssertEquals(0m, job.Charges[2].JR_LocalSellAmt);
			AssertEquals(0m, job.Charges[3].JR_LocalSellAmt);

			Factory.Save();

			var requestor = new ARCashAdvanceRequestor(job);
			var result = requestor.GetValidErrorsBeforeGenerateRequest();

			AssertEquals(@"Cannot request Advance Payment, charge amounts are missing.", result);
		}

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
		}
	}
}
