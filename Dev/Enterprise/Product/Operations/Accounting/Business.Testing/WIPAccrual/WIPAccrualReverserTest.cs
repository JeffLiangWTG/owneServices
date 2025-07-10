using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	public class WIPAccrualReverserTest : TransactionedTestCase
	{
		public void TestReverseAllAndSaveWithCollection()
		{
			WIPAccrualReverser reverser = new WIPAccrualReverser(WIPsAndAccruals);
			int transactionsReversed = reverser.ReverseAll();
			AssertEquals("Four Transactions should have been reversed", 4, transactionsReversed);
			BusinessObjectFactory factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP1.AL_ReverseDate.Date);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP2.AL_ReverseDate.Date);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR1.AL_ReverseDate.Date);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR2.AL_ReverseDate.Date);
		}

		public void TestReverseAllAndSaveWithJob()
		{
			Job jobForThirdWIP = Factory.NewJobWithValidTestDataForTesting<Job>();
			WIP wipWithDifferentJob = Factory.New<WIP>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			InsertWIPAccrual(wipWithDifferentJob, jobForThirdWIP.PK, charge);
			Factory.Save();
			WIPAccrualReverser reverser = new WIPAccrualReverser(Job);
			int transactionsReversed = reverser.ReverseAll();
			AssertEquals("Four Transactions should have been reversed", 4, transactionsReversed);
			BusinessObjectFactory factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP1.AL_ReverseDate.Date);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP2.AL_ReverseDate.Date);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR1.AL_ReverseDate.Date);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR2.AL_ReverseDate.Date);
		}

		public void TestReverseAllWithCollectionAndExternalFactory()
		{
			BusinessObjectFactory externalFactory = new BusinessObjectFactory();
			WIPAccrualReverser reverser = new WIPAccrualReverser(Job, externalFactory);
			int transactionsReversed = reverser.ReverseAll();
			AssertEquals("Four Transactions should have been reversed", 4, transactionsReversed);
			BusinessObjectFactory factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			Assert("Should not be reversed", !WIP1.IsReversed);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			Assert("Should not be reversed", !WIP2.IsReversed);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			Assert("Should not be reversed", !ACR1.IsReversed);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			Assert("Should not be reversed", !ACR2.IsReversed);
			externalFactory.Save(); // This emulates the caller calling save on the factory passed into the reverser
			factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP1.AL_ReverseDate.Date);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP2.AL_ReverseDate.Date);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR1.AL_ReverseDate.Date);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR2.AL_ReverseDate.Date);
		}

		public void TestReverseAllAndPocessApportionedCharges()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<ForwardingConsol>().GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = WIP1.AL_AC;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_LocalCostAmount = 50m;
			cost.E6_OSCostAmount = cost.E6_LocalCostAmount;
			JobCharge charge1 = WIP1.RelatedJobCharge;
			charge1.JR_E6 = cost.PK;
			charge1.JR_LocalSellAmt = WIP1.AL_LocalExTaxAmount;
			charge1.JR_LocalCostAmt = ACR1.AL_LocalExTaxAmount;
			charge1.JR_AC = WIP1.AL_AC;
			charge1.JR_GB = WIP1.AL_GB;
			charge1.JR_GE = WIP1.AL_GE;
			JobCharge charge2 = WIP2.RelatedJobCharge;
			charge2.JR_LocalSellAmt = WIP2.AL_LocalExTaxAmount;
			charge2.JR_LocalCostAmt = ACR2.AL_LocalExTaxAmount;
			charge2.JR_AC = WIP2.AL_AC;
			charge2.JR_GB = WIP2.AL_GB;
			charge2.JR_GE = WIP2.AL_GE;
			Factory.Save();
			WIPAccrualReverser reverser = new WIPAccrualReverser(WIPsAndAccruals);
			int transactionsReversed = reverser.ReverseAll(true);
			AssertEquals("Four Transactions should have been reversed", 4, transactionsReversed);
			BusinessObjectFactory factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP1.AL_ReverseDate.Date);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP2.AL_ReverseDate.Date);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR1.AL_ReverseDate.Date);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR2.AL_ReverseDate.Date);
		}

		public void TestReverseAllAndDontPocessApportionedCharges()
		{
			JobConsolCost cost = Factory.NewWithValidTestData<ForwardingConsol>().GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = WIP1.AL_AC;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_LocalCostAmount = 50m;
			cost.E6_OSCostAmount = cost.E6_LocalCostAmount;
			JobCharge charge1 = WIP1.RelatedJobCharge;
			charge1.JR_E6 = cost.PK;
			charge1.JR_LocalSellAmt = WIP1.AL_LocalExTaxAmount;
			charge1.JR_LocalCostAmt = ACR1.AL_LocalExTaxAmount;
			charge1.JR_AC = WIP1.AL_AC;
			charge1.JR_GB = WIP1.AL_GB;
			charge1.JR_GE = WIP1.AL_GE;
			JobCharge charge2 = WIP2.RelatedJobCharge;
			charge2.JR_LocalSellAmt = WIP2.AL_LocalExTaxAmount;
			charge2.JR_LocalCostAmt = ACR2.AL_LocalExTaxAmount;
			charge2.JR_AC = WIP2.AL_AC;
			charge2.JR_GB = WIP2.AL_GB;
			charge2.JR_GE = WIP2.AL_GE;
			Factory.Save();
			WIPAccrualReverser reverser = new WIPAccrualReverser(WIPsAndAccruals);
			int transactionsReversed = reverser.ReverseAll(false);
			AssertEquals("Three Transactions should have been reversed", 3, transactionsReversed);
			BusinessObjectFactory factoryForAssertions = new BusinessObjectFactory();
			WIP1 = factoryForAssertions.Load<WIP>(WIP1.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP1.AL_ReverseDate.Date);
			WIP2 = factoryForAssertions.Load<WIP>(WIP2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, WIP2.AL_ReverseDate.Date);
			ACR1 = factoryForAssertions.Load<Accrual>(ACR1.PK);
			AssertEquals("Should not be reversed with current Date", ZDate.Empty, ACR1.AL_ReverseDate.Date);
			ACR2 = factoryForAssertions.Load<Accrual>(ACR2.PK);
			AssertEquals("Should be reversed with current Date", Env.Time.CurrentLocalDate.Date, ACR2.AL_ReverseDate.Date);
		}

		WIPAccrualCollection WIPsAndAccruals;
		Job Job;
		WIP WIP1;
		WIP WIP2;
		Accrual ACR1;
		Accrual ACR2;
		BusinessObjectFactory Factory;
		protected void InsertWIPAccrual(BaseWIPAccrual wIPToInsert, ZGuid jobPK, BaseCharge charge)
		{
			wIPToInsert.AL_OSExTaxAmount = 50.00m;
			wIPToInsert.AL_JH = jobPK;
			wIPToInsert.AL_AC = TestObjectCreator.FRT.PK;
			wIPToInsert.AL_GB = GlbBranch.CurrentBranch.PK;
			wIPToInsert.AL_GE = GlbDepartment.CurrentDepartment.PK;
			if (wIPToInsert.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = wIPToInsert.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
			}

			if (wIPToInsert.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = wIPToInsert.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job.JH_JobNum = TestObjectCreator.GetRandomString(9);
			BaseCharge charge1 = Factory.NewWithValidTestData<BaseCharge>();
			BaseCharge charge2 = Factory.NewWithValidTestData<BaseCharge>();
			WIP1 = Factory.New<WIP>();
			WIP2 = Factory.New<WIP>();
			ACR1 = Factory.New<Accrual>();
			ACR2 = Factory.New<Accrual>();
			InsertWIPAccrual(WIP1, Job.PK, charge1);
			InsertWIPAccrual(WIP2, Job.PK, charge2);
			InsertWIPAccrual(ACR1, Job.PK, charge1);
			InsertWIPAccrual(ACR2, Job.PK, charge2);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			WIPsAndAccruals = new WIPAccrualCollection(newFactory, new ZQuery());
			WIPsAndAccruals.AddFromDatabase(WIP1.PK);
			WIPsAndAccruals.AddFromDatabase(WIP2.PK);
			WIPsAndAccruals.AddFromDatabase(ACR1.PK);
			WIPsAndAccruals.AddFromDatabase(ACR2.PK);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}

		protected TestObjectCreator fTestObjectCreator;
	}
}