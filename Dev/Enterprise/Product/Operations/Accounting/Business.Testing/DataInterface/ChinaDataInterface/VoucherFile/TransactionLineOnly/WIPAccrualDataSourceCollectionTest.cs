using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class WIPAccrualDataSourceCollectionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSQLValidity()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(201501, TransactionLineTypes.Accrual);
			testCollection.LoadCollection(201501, TransactionLineTypes.Accrual, true);
		}

		public void TestWIPCollectionForSamePeriod()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(201503, TransactionLineTypes.WIP);
			AssertEquals(1, testCollection.Count);
			AssertEquals(-49.0m, testCollection[0].Amount);

			testCollection.LoadCollection(201503, TransactionLineTypes.WIP, true);
			AssertEquals(1, testCollection.Count);
			AssertEquals(-49.0m, testCollection[0].Amount);
		}

		public void TestWIPCollectionForReversePeriod()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(201502, TransactionLineTypes.WIP);
			AssertEquals(2, testCollection.Count);
			Assert(testCollection.Select(x => x.Amount == -44.0M).Any());
			Assert(testCollection.Select(x => x.Amount == 49.0M).Any());

			testCollection.LoadCollection(201502, TransactionLineTypes.WIP, true);
			AssertEquals(2, testCollection.Count);
			Assert(testCollection.Select(x => x.Amount == -44.0M).Any());
			Assert(testCollection.Select(x => x.Amount == 49.0M).Any());
		}

		[TestDate(2015, 1, 9)]
		public void TestAccrualCollectionForSamePeriod()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(201503, TransactionLineTypes.Accrual);
			AssertEquals(1, testCollection.Count);
			AssertEquals(27.0m, testCollection[0].Amount);

			testCollection.LoadCollection(201503, TransactionLineTypes.Accrual, true);
			AssertEquals(1, testCollection.Count);
			AssertEquals(27.0m, testCollection[0].Amount);
		}

		[TestDate(2015, 1, 9)]
		public void TestAccrualCollectionForReversePeriod()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(201502, TransactionLineTypes.Accrual);
			AssertEquals(2, testCollection.Count);
			Assert(testCollection.Select(x => x.Amount == -11.0M).Any());
			Assert(testCollection.Select(x => x.Amount == 22.0M).Any());

			testCollection.LoadCollection(201502, TransactionLineTypes.Accrual, true);
			AssertEquals(2, testCollection.Count);
			Assert(testCollection.Select(x => x.Amount == -11.0M).Any());
			Assert(testCollection.Select(x => x.Amount == 22.0M).Any());
		}

		[TestDate(2015, 1, 9)]
		public void TestDateFilter()
		{
			WIPAccrualDataSourceCollection testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(new ZDateTime(2015, 2, 16), new ZDateTime(2015, 2, 28), TransactionLineTypes.Accrual);
			AssertEquals(0, testCollection.Count);
			testCollection.LoadCollection(new ZDateTime(2015, 2, 11), new ZDateTime(2015, 2, 28), TransactionLineTypes.Accrual);
			AssertEquals(2, testCollection.Count);
		}

		[TestDate(2015, 1, 9)]
		public void TestGLHeader()
		{
			var testCollection = new WIPAccrualDataSourceCollection(Factory);
			testCollection.LoadCollection(new ZDateTime(2015, 3, 1), new ZDateTime(2015, 3, 30), TransactionLineTypes.Accrual);
			AssertEquals("Pre-condition", 1, testCollection.Count);

			AssertEquals("We should GLHeader get from line's AL_AG.", new TestObjectCreator(Factory).CC5.AC_AG_AccrualAccount, testCollection[0].GLHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			InsertTestWIPandACR(Factory);
		}

		public void InsertTestWIPandACR(BusinessObjectFactory testFactory, bool isZeroAmount = false)
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(testFactory);
			TestObjectCreator testObjectCreator = new TestObjectCreator(testFactory);
			Year = 2015;
			Period201501 = 201501;
			Period201502 = 201502;
			Period201503 = 201503;
			testHelper.SetupSinglePeriod(Period201501, new ZDateTime(Year, 1, 1), new ZDateTime(Year, 1, 31));
			testHelper.SetupSinglePeriod(Period201502, new ZDateTime(Year, 2, 1), new ZDateTime(Year, 2, 28));
			testHelper.SetupSinglePeriod(Period201503, new ZDateTime(Year, 3, 1), new ZDateTime(Year, 3, 31));
			AccChargeCode[] charges = new AccChargeCode[] { testObjectCreator.CC1, testObjectCreator.CC2, testObjectCreator.CC3, testObjectCreator.CC4, testObjectCreator.CC6, testObjectCreator.CC8 };
			var job = testObjectCreator.CreateJobHeader();
			Accrual accrual1 = testObjectCreator.CreateAccrual(job);
			Accrual accrual2 = testObjectCreator.CreateAccrual(job);
			Accrual accrual3 = testObjectCreator.CreateAccrual(job);
			WIP wIP1 = testObjectCreator.CreateWIP();
			WIP wIP2 = testObjectCreator.CreateWIP();
			WIP wIP3 = testObjectCreator.CreateWIP();
			accrual1.AL_PostDate = new ZDateTime(Year, 1, 15);
			accrual2.AL_PostDate = new ZDateTime(Year, 2, 15);
			accrual3.AL_PostDate = new ZDateTime(Year, 3, 15);
			wIP1.AL_PostDate = new ZDateTime(Year, 1, 15);
			wIP2.AL_PostDate = new ZDateTime(Year, 2, 15);
			wIP3.AL_PostDate = new ZDateTime(Year, 3, 15);

			accrual1.RelatedJobCharge.ReverseAccrual(new ZDateTime(Year, 2, 15));

			wIP3.RelatedJobCharge.ReverseWIP(new ZDateTime(Year, 2, 15));

			accrual1.AL_AC = charges[0].PK;
			accrual2.AL_AC = charges[1].PK;
			wIP1.AL_AC = charges[2].PK;
			wIP2.AL_AC = charges[3].PK;
			accrual3.AL_AC = charges[4].PK;
			wIP3.AL_AC = charges[5].PK;
			if (isZeroAmount)
			{
				wIP1.AL_OSExTaxAmount = 0.0m;
				wIP2.AL_OSExTaxAmount = 0.0m;
				wIP3.AL_OSExTaxAmount = 0.0m;
			}
			else
			{
				accrual1.AL_OSExTaxAmount = 11.0m;
				accrual2.AL_OSExTaxAmount = 28.0m;
				accrual3.AL_OSExTaxAmount = 27.0m;
				wIP1.AL_OSExTaxAmount = 33.0m;
				wIP2.AL_OSExTaxAmount = 44.0m;
				wIP3.AL_OSExTaxAmount = 49.0m;
			}
			accrual1.AL_GB = GlbBranch.CurrentBranch.PK;
			accrual2.AL_GB = GlbBranch.CurrentBranch.PK;
			accrual3.AL_GB = NonCurrentBranch.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual3);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);
			ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Constants.AccountType.ProfitAndLossAccount);
			filter.MaximumRows = 6;
			GLHeader = testFactory.Load<AccGLHeader>(filter);
			charges[0].AC_AG_AccrualAccount = GLHeader[0].PK;
			charges[1].AC_AG_AccrualAccount = GLHeader[1].PK;
			charges[2].AC_AG_WIPAccount = GLHeader[2].PK;
			charges[3].AC_AG_WIPAccount = GLHeader[3].PK;
			charges[4].AC_AG_AccrualAccount = GLHeader[4].PK;
			charges[5].AC_AG_WIPAccount = GLHeader[5].PK;
			GLAccountNum1 = GLHeader[0].AG_AccountNum + "in Chinese";
			GLAccountNum2 = GLHeader[1].AG_AccountNum + "in Chinese";
			GLAccountNum3 = GLHeader[2].AG_AccountNum + "in Chinese";
			GLAccountNum4 = GLHeader[3].AG_AccountNum + "in Chinese";
			GLAccountNum5 = GLHeader[4].AG_AccountNum + "in Chinese";
			GLAccountNum6 = GLHeader[5].AG_AccountNum + "in Chinese";
			AccountDescriptor1 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[0].PK, GLAccountNum1, GLAccountNum1, GLHeader[0].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			AccountDescriptor2 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[1].PK, GLAccountNum2, GLAccountNum2, GLHeader[1].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			AccountDescriptor3 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[2].PK, GLAccountNum3, GLAccountNum3, GLHeader[2].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			AccountDescriptor4 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[3].PK, GLAccountNum4, GLAccountNum4, GLHeader[3].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			AccountDescriptor5 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[4].PK, GLAccountNum4, GLAccountNum5, GLHeader[4].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			AccountDescriptor6 = TestUtils.AddGLHeaderDescriptor(testFactory, GLHeader[5].PK, GLAccountNum4, GLAccountNum6, GLHeader[5].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);

			accrual1.AL_AG = charges[0].AC_AG_AccrualAccount;
			accrual2.AL_AG = charges[1].AC_AG_AccrualAccount;
			wIP1.AL_AG = charges[2].AC_AG_WIPAccount;
			wIP2.AL_AG = charges[3].AC_AG_WIPAccount;
			accrual3.AL_AG = testObjectCreator.CC5.AC_AG_AccrualAccount;
			wIP3.AL_AG = charges[5].AC_AG_WIPAccount;

			testFactory.Save();
		}

		GlbBranch fNonCurrentBranch;
		protected GlbBranch NonCurrentBranch
		{
			get
			{
				if (fNonCurrentBranch == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
					filter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
					fNonCurrentBranch = Factory.LoadTop1(typeof(GlbBranch), filter) as GlbBranch;
				}

				return fNonCurrentBranch;
			}
		}

		protected internal AccGLAccountDescriptor AccountDescriptor6;
		protected internal AccGLAccountDescriptor AccountDescriptor5;
		protected internal AccGLAccountDescriptor AccountDescriptor4;
		protected internal AccGLAccountDescriptor AccountDescriptor3;
		protected internal AccGLAccountDescriptor AccountDescriptor2;
		protected internal AccGLAccountDescriptor AccountDescriptor1;
		protected internal string GLAccountNum6;
		protected internal string GLAccountNum5;
		protected internal string GLAccountNum4;
		protected internal string GLAccountNum3;
		protected internal string GLAccountNum2;
		protected internal string GLAccountNum1;
		protected AccGLHeader[] GLHeader;
		protected internal int Year;
		protected internal int Period201501;
		protected internal int Period201502;
		protected internal int Period201503;
	}
}
