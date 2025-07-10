using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(DsbJobCloseBatch))]
	public class DsbJobCloseBatchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			AssertEquals(AccountingConstants.DsbJobBatchStatus.Open, Batch.JBB_BatchStatus);
			AssertEquals(GlbCompany.CurrentCompany.PK, Batch.JBB_GC);
		}

		public void TestDsbJobCloseBatchCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("AUD", Batch.LocalCurrencyCode);
				AssertEquals(2, Batch.LocalCurrencyDecimals);
				AssertEquals(Batch.LocalCurrencyCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				AssertEquals(Batch.LocalCurrencyDecimals, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals(Batch.LocalCurrencyPK, GlbCompany.CurrentCompany.LocalCurrency.PK);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AssertEquals("JPY", Batch.LocalCurrencyCode);
				AssertEquals(0, Batch.LocalCurrencyDecimals);
				AssertEquals(Batch.LocalCurrencyCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				AssertEquals(Batch.LocalCurrencyDecimals, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals(Batch.LocalCurrencyPK, GlbCompany.CurrentCompany.LocalCurrency.PK);
			}
		}

		public void TestDsbJobCloseBatchJobsAndLines()
		{
			AssertEquals(1, Batch.Jobs.Count);
			AssertEquals(2, Batch.TransactionLines.Count);
			AssertEquals(0m, Batch.BalanceAmount);
			AssertEquals(0m, Batch.Jobs[0].DSBSurplusAndShortfallAmount);

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J002";
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
			charge.JR_APInvoiceNum = "INV002";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			new InvoicingPostManager(testJob).CreateTransactions(JobInvoicingPostingOption.Costs);
			Factory.Save();
			var lines = Factory.LoadTop1<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, testJob.PK).AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Cost ));
			lines.AL_JBB = Batch.PK;
			Factory.Save();

			this.ReleaseFactory();
			var batch = Factory.Load<DsbJobCloseBatch>(Batch.PK);
			AssertEquals(2, batch.Jobs.Count);
			AssertEquals(3, batch.TransactionLines.Count);
			AssertEquals(-100m, batch.BalanceAmount);
			AssertEquals(0m, batch.Jobs.AsEnumerable().FirstOrDefault(x => x.JH_JobNum == "J001").DSBSurplusAndShortfallAmount);
			AssertEquals(-100m, batch.Jobs.AsEnumerable().FirstOrDefault(x => x.JH_JobNum == "J002").DSBSurplusAndShortfallAmount);
		}

		#region override

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			TestObjectCreator.CC1.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CC1.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J001";
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
			charge.JR_APInvoiceNum = "INV001";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			new InvoicingPostManager(testJob).CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			var lines = Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, testJob.PK));
			lines.ForEach(x => x.AL_JBB = Batch.PK);
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Batch;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Batch;

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => Batch;

		protected override BusinessObject GetNewBusinessObject()
		{
			Factory.Save();
			return Batch;
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		#endregion

		TestObjectCreator TestObjectCreator
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
		TestObjectCreator fTestObjectCreator;

		DsbJobCloseBatch Batch
		{
			get
			{
				if (fBatch == null)
				{
					fBatch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
					fBatch.JBB_BatchNumber = "B001";
				}

				return fBatch;
			}
		}
		DsbJobCloseBatch fBatch;
	}
}
