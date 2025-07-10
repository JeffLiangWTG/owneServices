using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARAmendingChargeCollectionBuilderTest : ARGenericChargeCollectionBuilderTest
	{
		public new void TestGenerateGenericChargeCollection()
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			GenericChargeCollection charges = BuilderToTest.GetBuiltButNotLoadedCollection();
			charges.Load();

			AssertEquals(11, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Revenue Charge Code", charges.Contains(RevenueChargeCode.PK));
			Assert("Collection should contain MJA Charge Code", charges.Contains(MJAChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain non control MAR Account", charges.Contains(Margin100Code));
			Assert("Collection should contain non control MAR Account", charges.Contains(Margin60Code));
			Assert("Collection should contain non control DSB Account", charges.Contains(this.DisbursementChargeCode));
			Assert("Collection should contain non control account", charges.Contains(GlHeader1.PK));
		}

		public new void TestGenerateGenericChargeCollectionForFindBox()
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			GenericChargeCollection charges = BuilderToTest.GetBuiltButNotLoadedCollection();
			charges.Load();

			AssertEquals(11, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Revenue Charge Code", charges.Contains(RevenueChargeCode.PK));
			Assert("Collection should contain MJA Charge Code", charges.Contains(MJAChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain non control MAR Account", charges.Contains(Margin100Code));
			Assert("Collection should contain non control MAR Account", charges.Contains(Margin60Code));
			Assert("Collection should contain non control DSB Account", charges.Contains(this.DisbursementChargeCode));
			Assert("Collection should contain non control account", charges.Contains(GlHeader1.PK));
		}

		protected override GenericChargeCollectionBuilder BuilderToTest
		{
			get { return new ARAmendingChargeCollectionBuilder(Requestor); }
		}
	}
}
