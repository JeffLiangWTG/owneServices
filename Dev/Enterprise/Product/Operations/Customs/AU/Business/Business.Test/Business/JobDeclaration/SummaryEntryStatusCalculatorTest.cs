using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SummaryEntryStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestSummaryEntryStatus()
		{
			AssertEquals("Entry Status", "", calculator.SummaryEntryStatus);

			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.Processing.Code, calculator.SummaryEntryStatus);

			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.Processing.Code, calculator.SummaryEntryStatus);

			entryHeader1.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.MultiStatus.Code, calculator.SummaryEntryStatus);

			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.MultiStatus.Code, calculator.SummaryEntryStatus);

			entryHeader2.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.MultiStatus.Code, calculator.SummaryEntryStatus);

			entryHeader1.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("Entry Status", CMRImportEntryAdvice.Finalised.Code, calculator.SummaryEntryStatus);
		}

		public void TestSummaryMessageStatus()
		{
			AssertEquals("Final status", "", calculator.SummaryMessageStatus);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("Final status", CustomsEntryStatus.NotSent.Code, calculator.SummaryMessageStatus);

			entryHeader.Messages.AddNew().EM_MessageText = "A";
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingPreLodge.Code;
			AssertEquals("Final status", CustomsEntryStatus.AwaitingPreLodge.Code, calculator.SummaryMessageStatus);
		}

		public void TestNotSentStatusNotConsidered()
		{
			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_Status = CustomsEntryStatus.NotSent.Code;

			AssertEquals("One status will be considered", 1, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("AwaitingFormalLodge", CustomsEntryStatus.AwaitingFormalLodge, calculator.CurrentEntryMessageStatus[0]);
		}

		public void TestNeedRefreshCurrentEntryMessageStatus()
		{
			AssertNull("PreCondition:CurrentStatus is null", calculator.fCurrentEntryMessageStatus);
			AssertEquals("Needs to be calculated", true, calculator.NeedRefreshCurrentEntryMessageStatus);

			AssertEquals("CurrentEntryMessageStatus is accessed and calculated", 0, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("Dont need to be calculated again", false, calculator.NeedRefreshCurrentEntryMessageStatus);

			testDec.CustomsEntryHeaders.AddNew().Messages.AddNew().EM_MessageText = "A";
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Entries' status needs to be recalculated", true, testDec.CustomsEntryHeaders.StatusNeedsRecalculation);
			AssertEquals("Needs to be recalculated", true, calculator.NeedRefreshCurrentEntryMessageStatus);
		}

		public void TestNeedRefreshCurrentEntryMessageStatusForHoldingAndWorkCompleted()
		{
			AssertEquals("CurrentEntryMessageStatus is accessed and calculated", 0, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("Dont need to be calculated again", false, calculator.NeedRefreshCurrentEntryMessageStatus);

			testDec.PlaceDeclarationWorkComplete("REASON");
			AssertEquals("Job is completed", true, testDec.IsDeclarationWorkFinished);
			AssertEquals("MostRecentHolding log is not in DB", false, testDec.MostRecentDWC.IsInDatabase);
			AssertEquals("need to be calculated again", true, calculator.NeedRefreshCurrentEntryMessageStatus);
			AssertEquals("CurrentEntryMessageStatus", 1, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("CurrentEntryMessageStatus", CustomsEntryStatus.DeclarationWorkComplete, calculator.CurrentEntryMessageStatus[0]);
			Factory.Save();

			testDec.PlaceHold("REASON");
			AssertEquals("Job is Holding", true, testDec.IsHolding);
			AssertEquals("MostRecentHolding log is not in DB", false, testDec.MostRecentHold.IsInDatabase);
			AssertEquals("need to be calculated again", true, calculator.NeedRefreshCurrentEntryMessageStatus);
			AssertEquals("CurrentEntryMessageStatus", 1, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("CurrentEntryMessageStatus", CustomsEntryStatus.HoldAwaiting, calculator.CurrentEntryMessageStatus[0]);
		}

		public void TestRefreshStatusWhenResetToOriginal()
		{
			testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("CurrentEntryMessageStatus is accessed and calculated", 0, calculator.CurrentEntryMessageStatus.Length);
			AssertEquals("Dont need to be calculated again", false, calculator.NeedRefreshCurrentEntryMessageStatus);

			testDec.CustomsEntryHeaders[0].CH_Status = "F";
			AssertEquals("Dont need to be calculated again", false, calculator.NeedRefreshCurrentEntryMessageStatus);

			testDec.CustomsEntryHeaders.RemoveAndDeleteAll();
			AssertEquals("Dont need to be calculated again", true, calculator.NeedRefreshCurrentEntryMessageStatus);
		}

		JobDeclaration testDec;
		SummaryEntryStatusCalculator calculator;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			calculator = new SummaryEntryStatusCalculator(testDec);
		}
	}
}
