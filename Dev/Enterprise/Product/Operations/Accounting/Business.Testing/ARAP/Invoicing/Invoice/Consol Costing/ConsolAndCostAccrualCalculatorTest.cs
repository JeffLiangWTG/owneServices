using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ConsolAndCostAccrualCalculatorTest : TestCaseWithFactory
	{
		TestObjectCreator ObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		public void TestGetAccrualMethods()
		{
			Factory.SetContext(BusinessContext.APInvoiceApportionToConsol);
			var consol1 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol1);
			var job1 = ObjectCreator.CreateJob(shipment1, false);
			var charge1 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC1, "cc1", ObjectCreator.AUD, 10, ObjectCreator.Creditor1, ObjectCreator.AUD, 10, ObjectCreator.Debtor);
			var charge2 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC3, "cc3", ObjectCreator.AUD, 20, ObjectCreator.Creditor1, ObjectCreator.AUD, 20, ObjectCreator.Debtor);
			var charge3 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC1, "cc1", ObjectCreator.AUD, 3, null, ObjectCreator.AUD, 3, ObjectCreator.Debtor);
			var charge4 = ObjectCreator.CreateCharge(job1, ObjectCreator.CC3, "cc3", ObjectCreator.AUD, 2, null, ObjectCreator.AUD, 2, ObjectCreator.Debtor);

			var consol2 = ObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0002");
			var shipment2 = ObjectCreator.CreateShipment("S0002", consol2);
			var job2 = ObjectCreator.CreateJob(shipment2, false);

			Factory.Save();

			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "inv1", ObjectCreator.AUD, 1.0m, ObjectCreator.Creditor1);
			var cost1 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost1.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol1.PK, JobConsolSchema.Constants.Prefix);
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost1.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost1.E6_LocalCostAmount = 1000m;

			var cost2 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost2.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol1.PK, JobConsolSchema.Constants.Prefix);
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost2.E6_AC_ChargeCode = ObjectCreator.CC1.PK;
			cost2.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost2.E6_LocalCostAmount = 300m;

			var cost3 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost3.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol1.PK, JobConsolSchema.Constants.Prefix);
			cost3.E6_AC_ChargeCode = ObjectCreator.CC3.PK;
			cost3.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost3.E6_LocalCostAmount = 400m;

			var cost4 = invoice.ConsolCosting.ConsolCosts.AddNew();
			cost4.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol2.PK, JobConsolSchema.Constants.Prefix);
			cost4.E6_AC_ChargeCode = ObjectCreator.CC3.PK;
			cost4.E6_RX_NKCurrency = ObjectCreator.AUD.RX_Code;
			cost4.E6_LocalCostAmount = 500m;

			Factory.Save();

			var accrualCalculator = Factory.GetCachedValue<ConsolAndCostAccrualCalculator>(ConsolAndCostAccrualCalculator.ConsolAndCostAccrualCalculatorKey, () => { return null; });
			AssertNotNull(accrualCalculator);
			AssertEquals(2, accrualCalculator.PendingConsolCosts.Select(x => x.E6_ParentID).Distinct().Count());

			AssertEquals(1300m, accrualCalculator.GetAccrualBasedOnConsolCost(cost1));
			AssertEquals(13m, accrualCalculator.GetAccrualBasedOnConsolShipment(cost1));
			AssertEquals(1313m, accrualCalculator.GetAccrualBasedOnConsolShipmentAndConsolCost(cost1));

			AssertEquals(1300m, accrualCalculator.GetAccrualBasedOnConsolCost(cost2));
			AssertEquals(13m, accrualCalculator.GetAccrualBasedOnConsolShipment(cost2));
			AssertEquals(1313m, accrualCalculator.GetAccrualBasedOnConsolShipmentAndConsolCost(cost2));

			AssertEquals(400m, accrualCalculator.GetAccrualBasedOnConsolCost(cost3));
			AssertEquals(22m, accrualCalculator.GetAccrualBasedOnConsolShipment(cost3));
			AssertEquals(422m, accrualCalculator.GetAccrualBasedOnConsolShipmentAndConsolCost(cost3));

			AssertEquals(500m, accrualCalculator.GetAccrualBasedOnConsolCost(cost4));
			AssertEquals(0m, accrualCalculator.GetAccrualBasedOnConsolShipment(cost4));
			AssertEquals(500m, accrualCalculator.GetAccrualBasedOnConsolShipmentAndConsolCost(cost4));
		}
	}
}
