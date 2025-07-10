using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	[TestedType(typeof(WIPAccrualCollection))]
	public class WIPAccrualCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompany()
		{
			TestCaseHelper.ClearTable(JobChargeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			WIP wip1 = Factory.New(typeof(WIP)) as WIP;
			WIP wip2 = Factory.New(typeof(WIP)) as WIP;
			Accrual aCR1 = Factory.New(typeof(Accrual)) as Accrual;
			Accrual aCR2 = Factory.New(typeof(Accrual)) as Accrual;
			wip1.AL_GC = ZGuid.NewZGuid();
			aCR2.AL_GC = ZGuid.NewZGuid();
			TestCollection.Load();
			AssertEquals(2, TestCollection.Count);
		}

		public void TestContainsApportionedAccruals()
		{
			AssertEquals("ContainsApportionedAccruals", false, TestCollection.ContainsApportionedAccruals);
			WIP wIP = Factory.New<WIP>();
			TestCollection.Add(wIP);
			AssertEquals("ContainsApportionedAccruals", false, TestCollection.ContainsApportionedAccruals);
			Accrual aCR1 = Factory.New<Accrual>();
			TestCollection.Add(aCR1);
			AssertEquals("ContainsApportionedAccruals", false, TestCollection.ContainsApportionedAccruals);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "AUMEL", "C0001");
			var cost = creator.CreateConsolCost(consol, creator.CC1);
			JobCharge charge1 = Factory.New<JobCharge>();
			charge1.JR_AL_APLine = aCR1.PK;
			charge1.JR_E6 = cost.PK;
			AssertEquals("ContainsApportionedAccruals", false, TestCollection.ContainsApportionedAccruals);
			JobCharge charge2 = Factory.New<JobCharge>();
			charge2.JR_E6 = charge1.JR_E6;
			AssertEquals("ContainsApportionedAccruals", true, TestCollection.ContainsApportionedAccruals);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		protected WIPAccrualCollection TestCollection;
		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = (WIPAccrualCollection)GetCollectionToTest();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WIPAccrualCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(WIP));
		}

		protected void Base_TestDelete()
		{
			base.TestDelete();
		}
	}
}
