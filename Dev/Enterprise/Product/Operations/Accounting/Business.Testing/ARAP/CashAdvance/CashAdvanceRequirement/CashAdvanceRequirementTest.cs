using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public abstract class CashAdvanceRequirementTest : TestCaseWithFactory
	{
		public void TestProperties_JobChargeRequiresCashAdvance_Pending()
		{
			var (charge, cashAdvanceReq) = SetupData();
			AssertNotNull(cashAdvanceReq);

			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), ZGuid.Empty, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 0M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 0M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 0M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 0M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), charge.JR_RX_NKSellCurrency, cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), true, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), false, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), false, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_UndoCashAdvanceRequest()
		{
			var (charge, cashAdvanceReq) = SetupData();
			AssertNotNull(cashAdvanceReq);

			cashAdvanceReq.RemoveCashAdvanceRequirement();
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), ZGuid.Empty, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 0M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 0M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 0M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 0M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 0M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 0M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), ZString.Empty, cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), false, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), false, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), false, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_Requested()
		{
			var (charge, cashAdvanceReq) = SetupData();
			var (cah, cal) = CreateCashAdvanceRequest(charge);
			AssertNotNull(cashAdvanceReq);

			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), cal.PK, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNotNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 0M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 0M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 200M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 200M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), "AUD", cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), false, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), true, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), true, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_UndoCashAdvanceRequestNotAllowed()
		{
			var (charge, cashAdvanceReq) = SetupData();
			var (cah, cal) = CreateCashAdvanceRequest(charge);
			AssertNotNull(cashAdvanceReq);

			AssertExceptionThrown<InvalidOperationException>(FormattableString.Invariant($"Cash advance request for charge: {charge.ChargeCode.AC_Code} is not in pending state."), () => cashAdvanceReq.RemoveCashAdvanceRequirement());

			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), cal.PK, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNotNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 0M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 0M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 200M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 200M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), "AUD", cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), false, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), true, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), true, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_Paid()
		{
			var (charge, cashAdvanceReq) = SetupData();
			var (cah, cal) = CreateCashAdvanceRequest(charge);
			AssertNotNull(cashAdvanceReq);

			cal.CAL_LocalPaidAmount = 200M;
			cal.CAL_OSPaidAmount = 200M;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;

			Factory.Save();

			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), cal.PK, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNotNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 200M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 200M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 0M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 0M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), "AUD", cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), false, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), false, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), true, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), true, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_Invoiced()
		{
			var (charge, cashAdvanceReq) = SetupData();
			var (cah, cal) = CreateCashAdvanceRequest(charge);
			AssertNotNull(cashAdvanceReq);

			cal.CAL_LocalPaidAmount = 200M;
			cal.CAL_OSPaidAmount = 200M;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestHeader.Paid;

			Factory.Save();

			var arInvoice = ObjectCreator.CreateInvoice(typeof(ARInvoice)) as ARInvoice;
			arInvoice.AH_OutstandingAmount = 200M;
			cashAdvanceReq.MarkAsInvoiced(arInvoice);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), cal.PK, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNotNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 200M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 200M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 0M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 0M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), "AUD", cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), false, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), false, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), true, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cal.IsInvoiced), true, cal.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cashAdvanceReq.HasActiveCashAdvanceRequestLine), true, cashAdvanceReq.HasActiveCashAdvanceRequestLine);
		}

		public void TestProperties_JobChargeRequiresCashAdvance_Cancelled()
		{
			var (charge, cashAdvanceReq) = SetupData();
			var (cah, cal) = CreateCashAdvanceRequest(charge);
			AssertNotNull(cashAdvanceReq);

			cashAdvanceReq.Cancel();
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequestLinePK), ZGuid.Empty, cashAdvanceReq.CashAdvanceRequestLinePK);
			AssertNull(nameof(cashAdvanceReq.CashAdvanceRequest), cashAdvanceReq.CashAdvanceRequest);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.OSAmount);
			AssertEquals(nameof(cashAdvanceReq.CashAdvanceRequest), 200M, cashAdvanceReq.LocalAmount);
			AssertEquals(nameof(cashAdvanceReq.OSPaidAmount), 0M, cashAdvanceReq.OSPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalPaidAmount), 0M, cashAdvanceReq.LocalPaidAmount);
			AssertEquals(nameof(cashAdvanceReq.OSOutstandingAmount), 0M, cashAdvanceReq.OSOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.LocalOutstandingAmount), 0M, cashAdvanceReq.LocalOutstandingAmount);
			AssertEquals(nameof(cashAdvanceReq.OSCurrency), "AUD", cashAdvanceReq.OSCurrency);
			AssertEquals(nameof(cashAdvanceReq.IsPending), true, cashAdvanceReq.IsPending);
			AssertEquals(nameof(cashAdvanceReq.IsRequested), false, cashAdvanceReq.IsRequested);
			AssertEquals(nameof(cashAdvanceReq.IsPaid), false, cashAdvanceReq.IsPaid);
			AssertEquals(nameof(cashAdvanceReq.IsInvoiced), false, cashAdvanceReq.IsInvoiced);
			AssertEquals(nameof(cashAdvanceReq.IsCancelled), false, cashAdvanceReq.IsCancelled);
			AssertEquals(nameof(cal.IsCancelled), true, cal.IsCancelled);
		}

		(Charge charge, ICashAdvanceRequirement cashAdvanceReq) SetupData()
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var cashAdvanceReq = SetCashAdvanceRequirement(charge);
			Factory.Save();

			return (charge, cashAdvanceReq);
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, ObjectCreator.Debtor, LedgerType, 200M, 200M, "AUD");
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, 200M, 200M);
			LinkChargeWithRequestLine(charge, cal.PK);
			Factory.Save();
			return (cah, cal);
		}

		protected abstract void LinkChargeWithRequestLine(Charge charge, ZGuid linePK);

		protected abstract ICashAdvanceRequirement SetCashAdvanceRequirement(Charge charge);

		protected abstract string LedgerType { get; }

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
