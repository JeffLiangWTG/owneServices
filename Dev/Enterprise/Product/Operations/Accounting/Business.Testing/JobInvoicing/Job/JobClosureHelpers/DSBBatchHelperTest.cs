using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DSBBatchHelperTest : TestCaseWithFactory
	{
		public void TestGetBatchPkForJob_Normal()
		{
			var runningTime = ZDateTime.UtcNow.ToDateTime();

			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);

			AssertEquals("Should not create new batch if no needed", false, DSBBatchHelper.GetBatchPkForJob(job1, runningTime, false).IsValid);

			var batchPK1 = DSBBatchHelper.GetBatchPkForJob(job1, runningTime, true);
			Assert("Create new batch when needed", batchPK1.IsValid);
			AssertEquals("Everyday should only create one batch", batchPK1, DSBBatchHelper.GetBatchPkForJob(job1, runningTime, true));

			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			job2.JH_GC = TestObjectCreator.CreateNewCompany("OMG").PK;

			var batchPK2 = DSBBatchHelper.GetBatchPkForJob(job2, runningTime, true);
			Assert("Create new batch when needed", batchPK2.IsValid);
			AssertNotEquals("Every company own it's batch", batchPK1, DSBBatchHelper.GetBatchPkForJob(job2, runningTime, true));
		}

		public void TestGetBatchPkForJobWithMultiThreadingSenario()
		{
			var runningTime = ZDateTime.UtcNow.ToDateTime();

			var batchCountBeforeTest = new BusinessObjectFactory().Load<DsbJobCloseBatch>(new ZQuery()).Length;
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment), false);
			using (SafeInjectMethodToDSBBatchHelper(() =>
			{
				using (SafeInjectMethodToDSBBatchHelper(null))
				{
					DSBBatchHelper.GetBatchPkForJob(job1, runningTime);
				}
			}
			))
			{
				DSBBatchHelper.GetBatchPkForJob(job1, runningTime);
			}

			AssertEquals("If Batch is create during another process , we should try to keep one batch created"
				, batchCountBeforeTest + 1
				, new BusinessObjectFactory().Load<DsbJobCloseBatch>(new ZQuery()).Length
			);
		}

		public void TestGetIsExistDsbBatchByCharge()
		{
			var profitAndLossGLHeader = TestObjectCreator.CreateGLHeader();
			profitAndLossGLHeader.AG_AccountType = Enterprise.Core.Constants.AccountType.ProfitAndLossAccount;
			Factory.Save();

			var charge = TestObjectCreator.CreateChargeCode("TT1");
			charge.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			charge.AC_AG_DisbursementShortfallAccount = profitAndLossGLHeader.PK;
			charge.AC_AG_DisbursementSurplusAccount = profitAndLossGLHeader.PK;
			Factory.Save();

			AssertEquals("No relative Dsb Job Close Batch yet.", false, DSBBatchHelper.GetIsExistDsbBatchByCharge(charge.PK));

			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "T000001", TestObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m);
			var line = header.Lines[0];
			Factory.Save();

			var batch = TestObjectCreator.CreateDsbJobCloseBatch("B001");
			var jobHeader = TestObjectCreator.CreateJobHeader();
			Factory.Save();

			line.AL_JBB = batch.PK;
			line.AL_AC = charge.PK;
			line.AL_JH = jobHeader.PK;
			TestObjectCreator.CreateJobCharge(line, jobHeader, charge);
			Factory.Save();

			AssertEquals("Has charge relative DSB Job Close Batch.", true, DSBBatchHelper.GetIsExistDsbBatchByCharge(charge.PK));
		}

		IDisposable SafeInjectMethodToDSBBatchHelper(Action injectMethod)
		{
			var oldUserContext = DSBBatchHelper.InjectMethod_GetBatchPkForJob_ForTestOnly;
			var disposableAction = new DisposableAction(
				() => DSBBatchHelper.InjectMethod_GetBatchPkForJob_ForTestOnly = injectMethod,
				() => DSBBatchHelper.InjectMethod_GetBatchPkForJob_ForTestOnly = oldUserContext);
			return disposableAction;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
