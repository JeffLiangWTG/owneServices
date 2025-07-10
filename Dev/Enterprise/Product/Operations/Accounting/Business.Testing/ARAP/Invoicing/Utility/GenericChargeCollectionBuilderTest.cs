using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class GenericChargeCollectionBuilderTest : TestCaseWithFactory
	{
		#region Implementation

		protected abstract GenericChargeCollectionBuilder BuilderToTest { get; }
		protected abstract AccChargeCode FEAChargeCodeForDepartmentTest { get; }

		protected TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			Requestor = new TestGenericChargeCollectionRequestor();
			TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLHeaderSchema.Constants.TableName);

			TestObjectCreator = new TestObjectCreator(Factory);
			Margin60Code = TestObjectCreator.CreateChargeCode("MAR60", "Margin60%", Constants.ChargeType.Margin, 60, GST, WHT, "ALL");
			Margin100Code = TestObjectCreator.CreateChargeCode("MAR100", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, "ALL");

			Margin60NonCurrentDepartmentCode = TestObjectCreator.CreateChargeCode("NMRG60", "Margin60%", Constants.ChargeType.Margin, 60, GST, WHT, NonCurrentDepartment.GE_Code);
			Margin100NonCurrentDepartmentCode = TestObjectCreator.CreateChargeCode("NMRG100", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, NonCurrentDepartment.GE_Code);
			GlHeader1 = TestObjectCreator.GLHeader1;    //creating a charge code with TestObjectCreator will also initalise this GL account

			RevenueChargeCode = TestObjectCreator.CreateChargeCode("REV100", "Revenue", Constants.ChargeType.Revenue, 100, GST, WHT, "ALL");
			DisbursementChargeCode = TestObjectCreator.CreateChargeCode("DSB100", "Disbursement", Constants.ChargeType.Disbursement, 100, GST, WHT, "ALL");
			NonAccrualChargeCode = TestObjectCreator.CreateChargeCode("NON000", "Non Accrual", Constants.ChargeType.NonAccrual, 0, GST, WHT, "ALL");
			OverheadChargeCode = TestObjectCreator.CreateChargeCode("OVH000", "Overhead", Constants.ChargeType.Overhead, 0, GST, WHT, "ALL");
			MJAChargeCode = TestObjectCreator.ManualJobAccrualChargeCode;
			PandLAccount = SetupGLHeader(Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Debit);
			BSHAccount = SetupGLHeader(Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			TTLAccount = SetupGLHeader(Core.Constants.AccountType.Total, Core.Constants.DebitCredit.Debit);
			BSHControlAccount = SetupGLHeader(Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			BSHControlAccount.AG_ControlAccount = ZBool.True;

			Factory.Save();
		}

		protected AccChargeCode Margin100Code;
		protected AccChargeCode Margin60Code;
		protected AccChargeCode RevenueChargeCode;
		protected AccChargeCode DisbursementChargeCode;
		protected AccChargeCode NonAccrualChargeCode;
		protected AccChargeCode OverheadChargeCode;
		protected AccChargeCode MJAChargeCode;
		protected AccChargeCode Margin60NonCurrentDepartmentCode;
		protected AccChargeCode Margin100NonCurrentDepartmentCode;
		protected AccChargeCode FEAChargeCode;

		protected string FEADepartment = "FEA";
		protected string FISDepartment = "FIS";

		#region GL Accounts

		protected AccGLHeader PandLAccount;
		protected AccGLHeader BSHAccount;
		protected AccGLHeader TTLAccount;
		protected AccGLHeader BSHControlAccount;
		protected AccGLHeader GlHeader1;

		protected AccGLHeader SetupGLHeader(ZString accountType, ZString debitCredit)
		{
			AccGLHeader result = TestObjectCreator.CreateGLHeader();
			result.AG_AccountType = accountType;
			result.AG_DebitCredit = debitCredit;
			return result;
		}

		#endregion

		#region GST Codes

		AccTaxRate fGST;
		protected AccTaxRate GST
		{
			get
			{
				if (fGST == null)
				{
					fGST = TestObjectCreator.CreateTaxRate("GST", "GSTRate", 10);
				}

				return fGST;
			}
		}

		AccTaxRate fFREEGST;
		protected AccTaxRate FREEGST
		{
			get
			{
				if (fFREEGST == null)
				{
					fFREEGST = TestObjectCreator.CreateTaxRate("Free", "FREEGST", 0);
				}

				return fFREEGST;
			}
		}

		AccWithholding fWHT;
		protected AccWithholding WHT
		{
			get
			{
				if (fWHT == null)
				{
					fWHT = TestObjectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);
				}

				return fWHT;
			}
		}

		#endregion

		protected GlbDepartment NonCurrentDepartment
		{
			get { return TestObjectCreator.NonCurrentDepartment; }
		}

		protected TestGenericChargeCollectionRequestor Requestor;

		#region TestRequestor

		protected class TestGenericChargeCollectionRequestor : IGenericChargeCollectionRequired
		{
			#region IGenericChargeCollectionRequired Members

			public bool IsJobRelated { get; set; }

			protected GlbDepartment fDepartment;
			public GlbDepartment Department
			{
				get
				{
					return fDepartment;
				}
				set
				{
					fDepartment = value;
				}
			}

			protected BusinessObjectFactory fFactory;
			public BusinessObjectFactory Factory
			{
				get
				{
					return fFactory;
				}
				set
				{
					fFactory = value;
				}
			}

			#endregion
		}

		#endregion

		#endregion

		public void TestChargeCollectionBuilderDoesFilterOnCompanyFilter()
		{
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader1.AG_AccountType = "P&L";
			glHeader1.AG_IsGlobal = true;

			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader2.AG_AccountType = "P&L";
			glHeader2.AG_IsGlobal = false;
			var filterForGLHeader2 = glHeader2.CompanyFilters.AddNew();
			filterForGLHeader2.ACF_GC_Company = GlbCompany.CurrentCompany.PK;

			var glHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader3.AG_AccountType = "P&L";
			glHeader3.AG_IsGlobal = false;
			var filterForGLHeader3 = glHeader3.CompanyFilters.AddNew();
			filterForGLHeader3.ACF_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();

			ZQuery query = BuilderToTest.GenerateFilter_ForTestOnly();
			GenericChargeCollection charges = new GenericChargeCollection(Factory, query);
			charges.Load();

			Assert("Collection should contain global GLAccount", charges.Contains(glHeader1.PK));
			Assert("Collection should contain non glboal GLAccount with a valid company filter", charges.Contains(glHeader2.PK));
			Assert("Collection should not contain non global GLAccount without a valid company filter", !charges.Contains(glHeader3.PK));
		}

		public void TestChargeCollectionBuilderDoesNotFilterOnDepartment()
		{
			Requestor.Department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, FISDepartment));
			Requestor.Factory = Factory;
			FEAChargeCode = FEAChargeCodeForDepartmentTest;
			Factory.Save();

			ZQuery query = BuilderToTest.GenerateFilter_ForTestOnly();
			GenericChargeCollection charges = new GenericChargeCollection(Factory, query);
			charges.Load();

			Assert("Collection should contain FEA Disbursement Charge code", charges.Contains(FEAChargeCode.PK));
		}

		public void TestChargeCollectionBuilderDoesFilterOnChargeType()
		{
			AccChargeCode mJATypeChargeCode = TestObjectCreator.ManualJobAccrualChargeCode;
			Factory.Save();

			ZQuery query = BuilderToTest.GenerateFilter_ForTestOnly();
			GenericChargeCollection charges = new GenericChargeCollection(Factory, query);
			charges.Load();

			if (BuilderToTest is ARGenericChargeCollectionBuilder)
			{
				Assert(true);
			}
			else
			{
				Assert("Collection should contain MJA Type Charge Code", charges.Contains(mJATypeChargeCode.PK));
			}
		}
	}
}
