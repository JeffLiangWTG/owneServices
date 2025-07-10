namespace Enterprise.Accounting.Business.WIPAccrual.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;

	public class WIPAccrualReverserHelperTest : TestCaseWithFactory
	{
		public void TestGetErrorMessageForMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals()
		{
			//Setup data
			var creator = new TestObjectCreator(Factory);
			//Job1
			var job = creator.CreateJob("S00001001", creator.ABIGAS, 0m, null, 0m);
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = creator.CC1.PK;
			var wip = Factory.NewWithValidTestData<WIP>();
			var acr = Factory.NewWithValidTestData<Accrual>();
			FillJobAndTransactionLine(job, charge1, acr, wip);
			//Job2
			var job2 = creator.CreateJob("S00001022", creator.ABIGAS, 0m, null, 0m);
			var charge2 = job2.Charges.AddNew();
			charge2.JR_AC = creator.CC2.PK;
			var acr2 = Factory.NewWithValidTestData<Accrual>();
			var wip2 = Factory.NewWithValidTestData<WIP>();
			FillJobAndTransactionLine(job2, charge2, acr2, wip2);
			//Job 3
			var job3 = creator.CreateJob("S00001033", creator.AALSHI, 0m, null, 0m);
			var charge3 = job3.Charges.AddNew();
			charge3.JR_AC = creator.CC1.PK;
			var acr3 = Factory.NewWithValidTestData<Accrual>();
			var wip3 = Factory.NewWithValidTestData<WIP>();
			FillJobAndTransactionLine(job3, charge3, acr3, wip3);
			Factory.Save();
			//Consol Cost
			var cost = Factory.NewWithValidTestData<ForwardingConsol>().GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			var cost2 = Factory.NewWithValidTestData<ForwardingConsol>().GetApportionments().CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = creator.CC2.PK;
			cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			CreateApportionSplitCharge(cost, charge1, charge3);
			CreateApportionSplitCharge(cost2, charge2);
			//Assertion
			var selectedACRandWIPs = new BaseWIPAccrual[] { acr, wip, acr2, wip2 };
			var reverserHelper = new WIPAccrualReverserHelper(selectedACRandWIPs);
			var result = reverserHelper.PopulateErrorMessagesForMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals();
			AssertEquals("Error message should only be available for acr", 1, result.Count);
			Assert(result.ContainsKey(acr.PK));
			Assert(!result.ContainsKey(acr2.PK));
			Assert(!result.ContainsKey(wip.PK));
			Assert(!result.ContainsKey(wip2.PK));

			var expectedMsg = string.Format($@"This accrual is apportioned at consol level.
To continue reversing this accrual, following accruals will have to be selected as well.

Job Number: {job3.JH_JobNum} - Charge Code: {creator.CC1.AC_Code}");

			result.TryGetValue(acr.PK, out ZString messageForacr);
			AssertMultilineASCIIEquals(expectedMsg, messageForacr);

			selectedACRandWIPs = new BaseWIPAccrual[] { acr, wip, acr2, wip2, acr3, wip3 };
			reverserHelper = new WIPAccrualReverserHelper(selectedACRandWIPs);
			result = reverserHelper.PopulateErrorMessagesForMissingApportionedAccrualsThatBelongToConsolCostOnSelectedAccruals();
			AssertEquals("No Error message, as all accruals are selected", 0, result.Count);
		}

		void FillJobAndTransactionLine(Job job, JobCharge charge, Accrual acr, WIP wip)
		{
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			FillWIP(wip, charge);
			FillAccrual(acr, charge);
		}

		void FillAccrual(Accrual acr, JobCharge charge)
		{
			charge.JR_AL_APLine = acr.PK;
			acr.AL_JH = charge.JR_JH;
			acr.AL_AC = charge.JR_AC;
			acr.AL_GB = charge.JR_GB;
			acr.AL_GE = charge.JR_GE;
		}

		void FillWIP(WIP wip, JobCharge charge)
		{
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AC = charge.JR_AC;
			wip.AL_GB = charge.JR_GB;
			wip.AL_GE = charge.JR_GE;
		}

		void CreateApportionSplitCharge(JobConsolCost cost, params JobCharge[] charges)
		{
			for (int i = 0; i < charges.Length; i++)
			{
				var splitCharge = cost.ApportionmentCharges.AddNew();
				splitCharge.JR_E6 = cost.PK;
				splitCharge.JR_IsUsedForApportionment = true;
				splitCharge.JR_AL_APLine = charges[i].JR_AL_APLine;
				splitCharge.JR_AL_ARLine = charges[i].JR_AL_ARLine;
				charges[i].JR_E6 = cost.PK;
			}
		}
	}
}