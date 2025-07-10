using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.Posting
{
	public class APCashAdvanceRequestorTest : TestCaseWithFactory
	{
		public void TestGenerateRequest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USLAX";

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_OA_LocalChargesAddr = creator.AALSHI.Addresses[0].PK;
			job.ExchangeRates.AddRate(creator.USD, 0.65m, creator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			for (int i = 0; i < 5; i++)
			{
				job.Charges.AddNew();
				job.Charges[i].JR_AC = creator.CC1.PK;
				job.Charges[i].JR_OH_CostAccount = creator.Creditor1.PK;
				job.Charges[i].JR_RX_NKCostCurrency = "AUD";
				job.Charges[i].JR_OSCostAmt = (i + 1) * 100;
				job.Charges[i].JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}

			job.Charges[1].JR_OH_CostAccount = creator.Creditor2.PK;
			job.Charges[2].JR_RX_NKCostCurrency = "USD";

			Factory.Save();

			var chargeToRun = job.Charges[0];
			chargeToRun.LoadRelevantChargesForAPCashAdvance();
			var requestor = new APCashAdvanceRequestor(chargeToRun);
			var header = requestor.GenerateRequest();

			AssertNotNull(header);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, header.CAH_Status);
			AssertEquals(LedgerTypes.AccountsPayable, header.CAH_Ledger);
			AssertEquals(GlbCompany.CurrentCompany.PK, header.CAH_GC_Company);
			AssertEquals(creator.Creditor1.PK, header.CAH_OH_Organization);
			AssertEquals(job.PK, header.CAH_JH_Job);
			AssertEquals("AUD", header.CAH_RX_NKTransactionCurrency);
			AssertEquals(1100m, header.CAH_OSAmount);
			AssertEquals(1100m, header.CAH_LocalAmount);
			AssertEquals(0m, header.CAH_OSPaidAmount);
			AssertEquals(0m, header.CAH_LocalPaidAmount);

			var lines = header.Lines.OfType<AccCashAdvanceRequestLine>();
			AssertEquals(3, lines.Count());

			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { job.Charges[0].JR_CAL_APLine, job.Charges[3].JR_CAL_APLine, job.Charges[4].JR_CAL_APLine }, lines.Select(x => x.PK));

			var charge1 = job.Charges[0];
			Assert(charge1.JR_IsAPCashAdvance);
			var cal1 = lines.First(x => x.PK == charge1.JR_CAL_APLine);
			AssertCashAdvanceLine(cal1, header.PK, CashAdvanceStatusCodes.RequestLine.Requested, 110m, 110m, 0m, 0m);

			var charge2 = job.Charges[1];
			Assert(!charge2.JR_IsAPCashAdvance);
			Assert(charge2.JR_CAL_APLine.IsEmpty);

			var charge3 = job.Charges[2];
			Assert(!charge3.JR_IsAPCashAdvance);
			Assert(charge3.JR_CAL_APLine.IsEmpty);

			var charge4 = job.Charges[3];
			Assert(charge4.JR_IsAPCashAdvance);
			var cal2 = lines.First(x => x.PK == charge4.JR_CAL_APLine);
			AssertCashAdvanceLine(cal2, header.PK, CashAdvanceStatusCodes.RequestLine.Requested, 440m, 440m, 0m, 0m);

			var charge5 = job.Charges[4];
			Assert(charge5.JR_IsAPCashAdvance);
			var cal3 = lines.First(x => x.PK == charge5.JR_CAL_APLine);
			AssertCashAdvanceLine(cal3, header.PK, CashAdvanceStatusCodes.RequestLine.Requested, 550m, 550m, 0m, 0m);
		}

		void AssertCashAdvanceLine(AccCashAdvanceRequestLine cal, ZGuid expectedHeaderPK, ZString expectedStatus, ZDecimal expectedLocalAmt, ZDecimal expectedOSAmount, ZDecimal expectedOSPaidAmount, ZDecimal expectedLocalPaidAmount)
		{
			AssertEquals(expectedHeaderPK, cal.CAL_CAH_RequestHeader);
			AssertEquals(expectedStatus, cal.CAL_Status);
			AssertEquals(expectedLocalAmt, cal.CAL_LocalAmount);
			AssertEquals(expectedOSAmount, cal.CAL_OSAmount);
			AssertEquals(expectedOSPaidAmount, cal.CAL_OSPaidAmount);
			AssertEquals(expectedLocalPaidAmount, cal.CAL_LocalPaidAmount);
			AssertEquals(GlbCompany.CurrentCompany.PK, cal.CAL_GC_Company);
		}

		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
		}
	}
}
