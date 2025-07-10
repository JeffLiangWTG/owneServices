using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public abstract class CashAdvanceRequestInfoByInvoiceTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestProperties()
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);

			SetCashAdvanceRequirement(charge1);
			SetCashAdvanceRequirement(charge2);
			SetCashAdvanceRequirement(charge4);

			Factory.Save();

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M);
			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M);

			var invoice = GetInvoice(charge1, charge2, charge3, charge4);
			var loader = new CashAdvanceRequestInfoByInvoice(invoice);

			AssertEquals("Job Charges Count", 4, loader.Charges.Count);
			foreach (var charge in new[] { charge1, charge2, charge3, charge4 })
			{
				AssertCollectionContains("Charge should exist", charge, loader.Charges);
			}

			AssertEquals("Cash advance requirement count", 3, loader.CashAdvanceRequirements.Count);
			foreach (var charge in new[] { charge1, charge2, charge4 })
			{
				AssertCollectionContains("CashAdvanceRequirement should exist", GetCashAdvanceRequirement(charge), loader.CashAdvanceRequirements);
			}

			AssertEquals("Cash advance request header count", 2, loader.CashAdvanceRequestHeaders.Count);
			foreach (var cah in new[] { cah1, cah4 })
			{
				var matchedCAH = loader.CashAdvanceRequestHeaders.FirstOrDefault(ca => ca.PK == cah.PK);
				AssertNotNull("CashAdvanceRequest header should exist", matchedCAH);
				AssertType<AccCashAdvanceRequestHeader>(cah);
				AssertType<CashAdvanceRequestHeader>(matchedCAH);
			}
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, ObjectCreator.Debtor, LedgerType, localAmount, osAmount, "AUD");
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			LinkChargeWithRequestLine(charge, cal.PK);
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
			Factory.Save();
			return (cah, cal);
		}

		protected abstract void SetCashAdvanceRequirement(Charge charge);

		protected abstract ICashAdvanceRequirement GetCashAdvanceRequirement(Charge charge);

		protected abstract void LinkChargeWithRequestLine(Charge charge, ZGuid linePK);

		protected abstract string LedgerType { get; }

		protected abstract Invoice GetInvoice(params Charge[] charge);

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
