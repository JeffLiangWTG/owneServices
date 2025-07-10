using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocJobLineDetailForRevenueTest : TestCaseWithFactory
	{
		[TestDate(2016, 10, 20)]
		public void TestRecognizeDateWithRevenueRecongnizeRegistry()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			var transactionCreator = ObjectFactory.Get<ITransactionCreator>();
			var objectCreator = new TestObjectCreator(Factory, true);

			periodHelper.SetupPeriods();

			var arInvoice = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var line = ((ARInvoice)arInvoice).Lines[0];
			var job = objectCreator.Job1;
			((ARInvoice)arInvoice).Lines[0].AL_JH = job.PK;

			using (Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				line.AL_LineAmount = 100m;
				line.AL_GC = job.JH_GC;
				line.AL_AC = objectCreator.FRT.PK;

				arInvoice.AH_InvoiceDate = DateTime.Now;
				arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				arInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				arInvoice.AH_InvoiceAmount = 100m;
				arInvoice.AH_OutstandingAmount = 100m;

				line.AL_RevRecognitionType = "ARV";

				var charge = objectCreator.CreateCharge(line, job);
				var originalRecognitionDate = periodHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(1);

				var revRec = Factory.NewWithValidTestData<JobChargeRevRecognition>();
				revRec.D3_JH = line.AL_JH;
				// Revenue Rec data setting should be within the last sub ledger closed period as in client senario.
				revRec.D3_RecognitionDate = originalRecognitionDate;
				revRec.D3_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

				Factory.Save();

				var nextOpenPeriod = new AccountingPeriodCalculator(Factory).GetNextSubLedgerOpenPeriodManagementFromDate(originalRecognitionDate, GlbCompany.CurrentCompany.PK);
				var expectedRecognitionDate = nextOpenPeriod.AM_EndDate;

				AssertNotEquals("Line RecognizeDate in Production Module Forms.", expectedRecognitionDate, originalRecognitionDate);
				AssertEquals("Line RecognizeDate in Production Module Forms.", expectedRecognitionDate, line.AL_ReverseDate);

				var lineWrapper = DocJobLineDetail.New(line, Factory);

				AssertEquals("Line RecognizeDate in Production Module Forms.", expectedRecognitionDate, lineWrapper.RecognizeDate);
			}
		}
	}
}
