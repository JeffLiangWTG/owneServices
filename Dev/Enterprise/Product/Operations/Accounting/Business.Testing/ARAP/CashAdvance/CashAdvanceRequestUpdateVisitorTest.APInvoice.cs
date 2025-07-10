using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public partial class CashAdvanceRequestUpdateVisitorTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_Requested()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");

			var (cah2, cal2) = CreateAPCashAdvanceRequest(charge2, 300M, 300M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah2.CAH_LocalPaidAmount = 300M;
			cah2.CAH_OSPaidAmount = 300M;
			cal2.CAL_LocalPaidAmount = 300M;
			cal2.CAL_OSPaidAmount = 300M;
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Requested, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cah2.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);
			AssertEquals(true, charge3.JR_IsAPCashAdvance);
			AssertEquals(true, charge4.JR_IsAPCashAdvance);

			var invoice = GetAPInvoice(charge1, charge2);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah2.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal2.CAL_Status);
			AssertEquals(true, charge3.JR_IsAPCashAdvance);
			AssertEquals(true, charge4.JR_IsAPCashAdvance);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_NotFullyPaid()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var (cah3, cal3) = CreateAPCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetAPInvoice(charge1, charge2, charge3, charge4);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah3.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals(false, charge2.JR_IsAPCashAdvance);
			AssertEquals(ZDateTime.Empty, invoice.AH_FullyPaidDate);
			AssertEquals("Unpaid Charge2 and GST", -350M, invoice.AH_OutstandingAmount);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_FullyPaid()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var (cah3, cal3) = CreateAPCashAdvanceRequest(charge3, 400M, 400M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetAPInvoice(charge3, charge4);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah3.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals(ZDateTime.Today, invoice.AH_FullyPaidDate);
			AssertEquals("Unpaid Charge2 and GST", 0M, invoice.AH_OutstandingAmount);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_PartiallyInvoiced()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetAPInvoice(charge3, charge4);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_APInvoiceReversed()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = ObjectCreator.CreateShipment("S000001");
			var job = ObjectCreator.CreateJob(shipment, ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			Factory.Save();

			cah4.CancelRequest();

			var invoice = GetAPInvoice(charge3, charge4);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, cah4.CAH_Status);

			AssertEquals(false, invoice.IsReversed);
			AssertEquals(-150M, invoice.AH_OutstandingAmount);

			var reverser = new APInvoiceReversing(invoice);
			reverser.Reverse();
			AssertEquals(true, invoice.IsReversed);
			AssertEquals(0M, invoice.AH_OutstandingAmount);

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Cancelled, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Cancelled, cah4.CAH_Status);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_APInvoiceReversedButCAHNotInInvoicedState()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = ObjectCreator.CreateShipment("S000001");
			var job = ObjectCreator.CreateJob(shipment, ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			Factory.Save();

			var invoice = GetAPInvoice(charge3);
			var validator = new CashAdvanceRequestUpdateVisitor();
			validator.Visit(invoice);

			AssertEquals(false, invoice.IsReversed);
			AssertEquals(0M, invoice.AH_OutstandingAmount);
			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);

			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cah1.CAH_Status);

			var reverser = new APInvoiceReversing(invoice);
			reverser.Reverse();

			AssertEquals(true, invoice.IsReversed);
			AssertEquals(0M, invoice.AH_OutstandingAmount);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal3.CAL_Status);
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateAPCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, ObjectCreator.Debtor, LedgerTypes.AccountsPayable, localAmount, osAmount, currency);
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			charge.JR_CAL_APLine = cal.PK;
			cal.CAL_Status = status;
			return (cah, cal);
		}

		APInvoice GetAPInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), currency: ObjectCreator.AUD, organisation: ObjectCreator.Creditor1) as APInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateAPInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.CostCurrency, 1.0M, "DESC01", charge.JR_OSCostAmt);
				charge.Accrual.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_APLine = line.PK;
			}
			return invoice;
		}
	}
}
