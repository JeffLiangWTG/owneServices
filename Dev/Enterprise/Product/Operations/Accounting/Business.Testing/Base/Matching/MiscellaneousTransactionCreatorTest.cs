using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class MiscellaneousTransactionCreatorTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected MiscellaneousTransactionCreator TestMiscTransCreator;
		protected OrgHeader TestOrg;

		protected Overpayment TestOVP;
		protected Discount TestDSC;
		protected ExchangeDifference TestEXX;
		protected Journal TestJNL;

		protected override void SetUp()
		{
			base.SetUp();
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestMiscTransCreator = GetTestMiscTransCreator();
		}

		protected abstract MiscellaneousTransactionCreator GetTestMiscTransCreator();

		#endregion

		#region TestCreateOverpayment

		public void TestCreateOverpayment()
		{
			TestOVP = TestMiscTransCreator.CreateOverpayment(50M, 2M, TestOrg);
			AssertEquals("Currency should be that of Current Company", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestOVP.AH_RX_NKTransactionCurrency);
			AssertEquals("Overpayment account should be TestOrg", TestOrg.PK, TestOVP.AH_OH);
			Assert("Overpayment Description should be set", TestOVP.AH_Desc.StartsWith("MATCH NO."));
			Assert("Invoice date should not be null", !TestOVP.AH_InvoiceDate.IsEmpty);
			Assert("Due date should not be empty", !TestOVP.AH_DueDate.IsEmpty);
			Assert("Post date should not be empty", !TestOVP.AH_PostDate.IsEmpty);
			AssertEquals("Branch should be set", GlbBranch.CurrentBranch.PK, TestOVP.AH_GB);
			AssertEquals("Department should be set", GlbDepartment.CurrentDepartment.PK, TestOVP.AH_GE);

			AssertEquals("AH_OSTotal should be 50", 50M, TestOVP.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount should be 25", 25M, TestOVP.AH_InvoiceAmount);
			AssertEquals("AH_OutstandingAmount should be 25", 25M, TestOVP.AH_OutstandingAmount);
			AssertEquals("AH_ExchangeRate should be 2", 2M, TestOVP.AH_ExchangeRate);
		}

		#endregion

		#region TestCreateDiscount

		public void TestCreateDiscount()
		{
			TestDSC = TestMiscTransCreator.CreateDiscount(-75M, TestOrg);
			AssertEquals("Currency should be that of Current Company", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestDSC.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_OH on Discount should be TestOrg", TestOrg.PK, TestDSC.AH_OH);
			Assert("Discount description should be set", TestDSC.AH_Desc.StartsWith("MATCH NO."));
			Assert("Invoice date should not be empty", !TestDSC.AH_InvoiceDate.IsEmpty);
			Assert("Due date should not be empty", !TestDSC.AH_DueDate.IsEmpty);
			Assert("Post date should not be empty", !TestDSC.AH_PostDate.IsEmpty);
			AssertEquals("Branch should be set", GlbBranch.CurrentBranch.PK, TestDSC.AH_GB);
			AssertEquals("Department should be set", GlbDepartment.CurrentDepartment.PK, TestDSC.AH_GE);

			AssertEquals("AH_OSTotal should be -75", -75M, TestDSC.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount should be -75", -75M, TestDSC.AH_InvoiceAmount);
			AssertEquals("AH_OutstandingAmount should be -75", -75M, TestDSC.AH_OutstandingAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1M, TestDSC.AH_ExchangeRate);
		}

		#endregion

		#region TestCreateExchangeDifference

		public void TestCreateExchangeDifference()
		{
			TestEXX = TestMiscTransCreator.CreateExchangeDifference(90M, TestOrg);
			AssertEquals("Currency should be that of Current Company", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestEXX.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_OH on Exchange Diff should be TestOrg", TestOrg.PK, TestEXX.AH_OH);
			Assert("ExchangeDiff description should be set", TestEXX.AH_Desc.StartsWith("MATCH NO."));
			Assert("Invoice date should not be empty", !TestEXX.AH_InvoiceDate.IsEmpty);
			Assert("Due date should not be empty", !TestEXX.AH_DueDate.IsEmpty);
			Assert("Post date should not be empty", !TestEXX.AH_PostDate.IsEmpty);
			AssertEquals("Branch should be set", GlbBranch.CurrentBranch.PK, TestEXX.AH_GB);
			AssertEquals("Department should be set", GlbDepartment.CurrentDepartment.PK, TestEXX.AH_GE);

			AssertEquals("AH_OSTotal should be 90", 90M, TestEXX.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount should be 90", 90M, TestEXX.AH_InvoiceAmount);
			AssertEquals("AH_OutstandingAmount should be 90", 90M, TestEXX.AH_OutstandingAmount);
			AssertEquals("AH_ExchangeRate should be 1", 1M, TestEXX.AH_ExchangeRate);
		}

		#endregion

		public void TestCreateBankFee()
		{
			TestJNL = TestMiscTransCreator.CreateBankFee(TestOrg);
			AssertEquals("Currency should be that of Current Company", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestJNL.AH_RX_NKTransactionCurrency);
			AssertEquals("Currency should be editable", false, TestJNL.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
			AssertEquals("AH_OH on Discount should be TestOrg", TestOrg.PK, TestJNL.AH_OH);
			Assert("Bank Fee Journal description should be set", TestJNL.AH_Desc.StartsWith("BANK FEE JOURNAL RELATING TO MATCH NO"));
			Assert("Invoice date should not be empty", !TestJNL.AH_InvoiceDate.IsEmpty);
			Assert("Due date should not be empty", !TestJNL.AH_DueDate.IsEmpty);
			Assert("Post date should not be empty", !TestJNL.AH_PostDate.IsEmpty);
			AssertEquals("Branch should be set", GlbBranch.CurrentBranch.PK, TestJNL.AH_GB);
			AssertEquals("Department should be set", GlbDepartment.CurrentDepartment.PK, TestJNL.AH_GE);
			AssertEquals("AH_ExchangeRate should be 1", 1M, TestJNL.AH_ExchangeRate);
			AssertEquals("Bank Fee Journal OSPartialPaymentAmount should be readonly", true, TestJNL.OSPartialPaymentAmount_ReadOnly);
		}

		public void TestOVPDescriptionReadOnly()
		{
			TestOVP = TestMiscTransCreator.CreateOverpayment(50M, 2M, TestOrg);
			Assert("Description is NOT Readonly", !TestOVP.AH_DescInfo.ReadOnly);
		}

		public void TestEXXDescriptionReadOnly()
		{
			TestDSC = TestMiscTransCreator.CreateDiscount(50M, TestOrg);
			Assert("Description is NOT Readonly", !TestDSC.AH_DescInfo.ReadOnly);
		}

		public void TestDSCDescriptionReadOnly()
		{
			TestEXX = TestMiscTransCreator.CreateExchangeDifference(50M, TestOrg);
			Assert("Description is NOT Readonly", !TestEXX.AH_DescInfo.ReadOnly);
		}

		public void TestJNLDescriptionReadOnly()
		{
			TestJNL = TestMiscTransCreator.CreateBankFee(TestOrg);
			Assert("Description is NOT Readonly", !TestJNL.AH_DescInfo.ReadOnly);
		}
	}
}
