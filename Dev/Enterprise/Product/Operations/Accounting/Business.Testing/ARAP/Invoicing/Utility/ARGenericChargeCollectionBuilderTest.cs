using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARGenericChargeCollectionBuilderTest : GenericChargeCollectionBuilderTest
	{
		public void TestGenerateGenericChargeCollection() //Job is ALWAYS null in AR
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			GenericChargeCollection charges = BuilderToTest.GetBuiltButNotLoadedCollection();
			charges.Load();

			AssertEquals(5, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Revenue Charge Code", charges.Contains(RevenueChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain non control GlHeader", charges.Contains(GlHeader1.PK));
		}

		public void TestGenerateGenericChargeCollectionForFindBox()
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			GenericChargeCollection charges = BuilderToTest.GetBuiltButNotLoadedCollection();
			charges.Load();

			AssertEquals(5, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Revenue Charge Code", charges.Contains(RevenueChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain non control GlHeader", charges.Contains(GlHeader1.PK));
		}

		protected override GenericChargeCollectionBuilder BuilderToTest
		{
			get { return new ARGenericChargeCollectionBuilder(Requestor); }
		}

		protected override AccChargeCode FEAChargeCodeForDepartmentTest
		{
			get { return TestObjectCreator.CreateChargeCode("FEACode", "Test", Constants.ChargeType.Revenue, 0M, GST, WHT, FEADepartment); }
		}
	}
}
