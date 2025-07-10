using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(APCashAdvanceChargeCollection))]
	public class APCashAdvanceChargeCollectionTest : ChargeCollectionTest
	{
		public new void TestAllowNew()
		{
			var coll = new APCashAdvanceChargeCollection(Factory.NewWithValidTestData<ChargeWithCost>());
			AssertEquals(false, coll.AllowNew);
		}

		public void TestAllowRemove()
		{
			var coll = new APCashAdvanceChargeCollection(Factory.NewWithValidTestData<ChargeWithCost>());
			AssertEquals(false, coll.AllowRemove);
		}

		public void TestRelationshipFilter_ChargeNotLinkedToCashAdvance()
		{
			var newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);

			var job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";

			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge1.JR_OSCostAmt = 200m;

			//charge has every filtered field same value
			var charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge2.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge2.JR_OSCostAmt = 200m;

			//charge has different creditor
			var charge3 = job1.Charges.AddNew();
			charge3.JR_AC = chargeCodeInCurrentCompany.PK;
			charge3.JR_OH_CostAccount = objectCreator.Creditor2.PK;
			charge3.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge3.JR_OSCostAmt = 200m;

			//charge has different cost currency
			var charge4 = job1.Charges.AddNew();
			charge4.JR_AC = chargeCodeInCurrentCompany.PK;
			charge4.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge4.JR_RX_NKCostCurrency = objectCreator.USD.Code;
			charge4.JR_OSCostAmt = 200m;

			//charge has cost amount 0
			var charge5 = job1.Charges.AddNew();
			charge5.JR_AC = chargeCodeInCurrentCompany.PK;
			charge5.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge5.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge5.JR_OSCostAmt = 0m;

			//charge belongs to another job
			var job2 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_JobNum = "5522566";
			var job2Charge0 = job2.Charges.AddNew();
			job2Charge0.JR_AC = chargeCodeInCurrentCompany.PK;
			job2Charge0.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			job2Charge0.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			job2Charge0.JR_OSCostAmt = 200m;

			//charge has been posted
			var charge6 = job1.Charges.AddNew();
			charge6.JR_AC = chargeCodeInCurrentCompany.PK;
			charge6.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge6.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge6.JR_OSCostAmt = 200m;

			// Posted this Job
			var apInvoice = newFactory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_JH = job1.PK;
			apInvoice.AH_OH = objectCreator.Creditor1.PK;
			var line = newFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = apInvoice.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AG = objectCreator.GLHeader1.PK;
			line.AL_OSAmount = 200m;
			line.AL_LineAmount = 200m;
			charge6.JR_AL_APLine = line.PK;
			charge6.SetAmountsFromLinkedLinesForTests();

			newFactory.Save();

			var charges = new APCashAdvanceChargeCollection(charge1);
			charges.Load();
			AssertEquals(2, charges.Count);
			AssertCollectionContains(charge1, charges);
			AssertCollectionContains(charge2, charges);
		}

		public void TestRelationshipFilter_ChargeLinkedToCashAdvance()
		{
			var newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);

			var job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";

			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge1.JR_OSCostAmt = 200m;

			var charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge2.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge2.JR_OSCostAmt = 200m;

			var cashAdvanceHeader = objectCreator.CreateCashAdvanceRequestHeader(job1.PK, objectCreator.Creditor1.PK, LedgerTypes.AccountsPayable, 300m, 300m, objectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1 = objectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			var cashAdvanceLine2 = objectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 200m, 200m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_APLine = cashAdvanceLine1.PK;
			charge2.JR_CAL_APLine = cashAdvanceLine2.PK;
			cashAdvanceHeader.Lines.Add(cashAdvanceLine1);
			cashAdvanceHeader.Lines.Add(cashAdvanceLine2);

			newFactory.Save();

			var charges = new APCashAdvanceChargeCollection(charge1);
			charges.Load();
			AssertEquals(2, charges.Count);
			AssertCollectionContains(charge1, charges);
			AssertCollectionContains(charge2, charges);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APCashAdvanceChargeCollection(Factory.NewWithValidTestData<ChargeWithCost>());
		}
	}
}
