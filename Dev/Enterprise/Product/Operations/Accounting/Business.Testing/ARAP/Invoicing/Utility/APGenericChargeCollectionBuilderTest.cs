using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APGenericChargeCollectionBuilderTest : GenericChargeCollectionBuilderTest
	{
		public void TestGenerateGenericChargeCollectionWithNullJob()
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			ZQuery query = BuilderToTest.GetQueryForValidation();
			GenericChargeCollection charges = new GenericChargeCollection(Factory, query);
			charges.Load();

			AssertEquals(5, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Overhead Charge Code", charges.Contains(OverheadChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain non control GlHeader", charges.Contains(GlHeader1.PK));
		}

		public void TestGenerateGenericChargeCollectionFilterWithValidJob()
		{
			Requestor.IsJobRelated = true;
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			ZQuery query = BuilderToTest.GetQueryForValidation();
			GenericChargeCollection charges = new GenericChargeCollection(Factory, query);
			charges.Load();

			AssertEquals("AP Line with null job should have 3 items in collection", 4, charges.Count);
			Assert("Collection should contain Disbursement Charge Code", charges.Contains(DisbursementChargeCode.PK));
			Assert("Collection should contain MJA Charge Code", charges.Contains(MJAChargeCode.PK));
			Assert("Collection should contain Margin Charge Codes", charges.Contains(Margin100Code.PK));
			Assert("Collection should contain Margin Charge Codes", charges.Contains(Margin60Code.PK));
		}

		public void TestGenerateGenericChargeCollectionForFindBox()
		{
			Requestor.Department = GlbDepartment.CurrentDepartment;
			Requestor.Factory = Factory;

			GenericChargeCollection charges = BuilderToTest.GetBuiltButNotLoadedCollection();
			charges.Load();

			AssertEquals(11, charges.Count);
			Assert("Collection should contain NonAccrual Charge", charges.Contains(NonAccrualChargeCode.PK));
			Assert("Collection should contain Overhead Charge Code", charges.Contains(OverheadChargeCode.PK));
			Assert("Collection should contain MJA Charge Code", charges.Contains(MJAChargeCode.PK));
			Assert("Collection should contain P&L Account", charges.Contains(PandLAccount.PK));
			Assert("Collection should contain non control BSH Account", charges.Contains(BSHAccount.PK));
			Assert("Collection should contain Disbursement Charge Code", charges.Contains(DisbursementChargeCode.PK));
			Assert("Collection should contain Margin Charge Codes", charges.Contains(Margin100Code.PK));
			Assert("Collection should contain Margin Charge Codes", charges.Contains(Margin60Code.PK));
			Assert("Collection should contain NonCurrentDept Margin Charge Codes", charges.Contains(Margin100NonCurrentDepartmentCode.PK));
			Assert("Collection should contain NonCurrentDept Margin Charge Codes", charges.Contains(Margin60NonCurrentDepartmentCode.PK));
			Assert("Collection should contain non control GlHeader", charges.Contains(GlHeader1.PK));
		}

		protected override GenericChargeCollectionBuilder BuilderToTest
		{
			get { return new APGenericChargeCollectionBuilder(Requestor); }
		}

		protected override AccChargeCode FEAChargeCodeForDepartmentTest
		{
			get { return TestObjectCreator.CreateChargeCode("FEACode", "Test", Constants.ChargeType.Disbursement, 0M, GST, WHT, FEADepartment); }
		}
	}
}
