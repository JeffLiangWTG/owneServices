using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	[TestedType(typeof(AccrualCollection))]
	public class AccrualCollectionTest : WIPAccrualCollectionTest
	{
		public new void TestCompany()
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
			AssertEquals("Only Accrual should be loaded.", 1, TestCollection.Count);
			AssertCollectionContains("Only Accrual should be loaded.", aCR1, TestCollection);
		}

		public override void TestDelete()
		{
			Base_TestDelete();
		}

		public void TestAllowAddRemove()
		{
			Assert("Allow New by default", TestCollection.AllowNew);
			Assert("Allow Remove by default", TestCollection.AllowRemove);
			((AccrualCollection)TestCollection).SetAllowNew(false);
			Assert("Allow New should be changed", !TestCollection.AllowNew);
			((AccrualCollection)TestCollection).SetAllowRemove(false);
			Assert("Allow Remove should be changed", !TestCollection.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccrualCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(Accrual));
		}
	}
}
