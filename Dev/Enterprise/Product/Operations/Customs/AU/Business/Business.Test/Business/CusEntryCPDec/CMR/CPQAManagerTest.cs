using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CPQAManagerTest : TestCaseWithFactory
	{
		public void TestSetCPDecQuestionGenerationDate()
		{
			testDec.AddInfo.ZA_CPQuestionGenDate_Hidden = new ZDateTime(2005, 1, 1);
			CMRReferenceFileUpdateLog updateLog = new CMRReferenceFileUpdateLog(Factory);
			updateLog.LogUpdateSuccess();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testManager.GenerateQuestions(false, false);
			updateLog.LoadLastSuccessfulUpdate();
			AssertEquals("Generation date is set to the last reference files update datetime", updateLog.SuccessfulUpdateFileTimeStamp.ToZDateTime(), testDec.AddInfo.ZA_CPQuestionGenDate_Hidden);
		}

		public void TestGenerateQuestionsForOriginalOrAmendment()
		{
			CreateLodgementQuestion(1);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			testManager.GenerateQuestionsForOriginalOrAmendment();
			AssertEquals("Entry header has a question generated", true, entryHeader.Questions.HasQuestionWithID(1));
		}

		public void TestGenerateQuestionsForConsolidatedEntryOriginalOrAmendment()
		{
			CreateLodgementQuestion(1);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 10000m;
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);

			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";
			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;

			var testCPQAManager = new CPQAManagerForTest(aggregateDeclaration);
			testCPQAManager.GenerateQuestionsForConsolidatedEntryOriginalOrAmendment();
			consolidatedDeclaration.UpdateQuestionsFromAggregateDeclaration(aggregateDeclaration);
			AssertEquals("Member Declaration Entry header has not a question generated", false, entryHeader.Questions.HasQuestionWithID(1));
			AssertEquals("Consolidated Declaration has a question generated", true, consolidatedDeclaration.Questions.HasQuestionWithID(1));
		}

		public void TestGenerateQuestionsForWithdrawal()
		{
			CreateLodgementQuestion(12);
			CreateLodgementQuestion(13);
			CreateLodgementQuestion(14);
			CreateLodgementQuestion(15);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 10000m;
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);

			testManager.GenerateQuestionsForWithdrawal();
			AssertEquals("Withdrawal question 13 generated", true, entryHeader.Questions.HasQuestionWithID(13));

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			entryHeader.ResetTotalsAndCachedValues();
			testManager.GenerateQuestionsForWithdrawal();
			AssertEquals("Withdrawal question 12 generated", true, entryHeader.Questions.HasQuestionWithID(12));
			AssertEquals("Withdrawal question 13 for paid entry removed", false, entryHeader.Questions.HasQuestionWithID(13));
			AssertEquals("Withdrawal question 15 for paid entry generated", true, entryHeader.Questions.HasQuestionWithID(15));
		}

		public void TestGenerateQuestionsForConsolidatedEntryWithdrawal()
		{
			CreateLodgementQuestion(12);
			CreateLodgementQuestion(13);
			CreateLodgementQuestion(14);
			CreateLodgementQuestion(15);

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadEntryHeader = leadDeclaration.EntryHeader;

			var entryLine = leadEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 10000m;
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);

			var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
			var testCPQAManager = new CPQAManagerForTest(aggregateDeclaration);
			testCPQAManager.GenerateQuestionsForConsolidatedEntryWithdrawal();

			var aggregateHeader = aggregateDeclaration.EntryHeader;
			AssertEquals("Withdrawal question 13 generated", true, aggregateHeader.Questions.HasQuestionWithID(13));

			leadEntryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			leadEntryHeader.ResetTotalsAndCachedValues();
			Factory.Save();

			aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
			aggregateHeader = aggregateDeclaration.EntryHeader;
			AssertEquals("Changes to Entry Header are aggregated", CMREntryPaymentStatusList.Codes.Paid, aggregateHeader.AddInfo.ZA_PaymentStatus_Hidden);

			testCPQAManager = new CPQAManagerForTest(aggregateDeclaration);
			testCPQAManager.GenerateQuestionsForConsolidatedEntryWithdrawal();

			AssertEquals("Withdrawal question 12 generated", true, aggregateHeader.Questions.HasQuestionWithID(12));
			AssertEquals("Withdrawal question 13 for paid entry removed", false, aggregateHeader.Questions.HasQuestionWithID(13));
			AssertEquals("Withdrawal question 15 for paid entry generated", true, aggregateHeader.Questions.HasQuestionWithID(15));
		}

		void CreateLodgementQuestion(int questionID)
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = questionID;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
		}

		public void TestGenerateQuestionsForWithdraw()
		{
			testManager.GenerateQuestions(true, false);
			AssertEquals("CP Dec questions are not generated for entry lines as this is withdraw", false, testManager.CPDecQuestionsGenerationInitiated);

			testDec.CustomsEntryHeaders[0].Questions.RemoveAndDeleteAll();
			testManager.GenerateQuestions(false, false);
			AssertEquals("CP Dec questions are generated for entry lines", true, testManager.CPDecQuestionsGenerationInitiated);
		}

		public void TestLodgementQuestionsAreGenerated()
		{
			testManager.GenerateQuestions(true,true);
			AssertEquals("Lodgement Questions are generated for Entry header", true, testManager.LodgementQuestionsInitiated);
		}

		public void TestCPDecQuestionsAreNotGeneratedForSAC()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testManager.GenerateQuestions(false, false);
			AssertEquals("Job is SAC", true, testDec.IsSACWithoutLines);
			AssertEquals("CP Dec questions are not generated for entry lines as this is SAC", false, testManager.CPDecQuestionsGenerationInitiated);
		}

		public void TestRebuildViewQuestionsAfterGeneratingQuestions()
		{
			ZDateTime dutyDate = new ZDateTime(2005, 1, 1);

			CMRCommunityProtectionProfile profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;

			CMRCommunityProtectionRisk risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;

			CMRLodgementQuestion question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			CMRCommunityProtectionProfile profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "00000001";
			profile2.CP_CommunityProtectionRiskIdentifier = 402;

			CMRCommunityProtectionRisk risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 402;
			risk2.CK_StartDate = dutyDate;
			risk2.CK_LodgementQuestionIdentifier = 5001;

			CMRLodgementQuestion question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 5001;
			question2.CQ_LodgementQuestionStartDate = dutyDate;

			JobComInvoiceLine line = testDec.FilteredInvoiceLines[0];
			line.JI_Tariff = "0000.00.00 00";
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_DateOfFirstArrival = dutyDate;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			testManager.GenerateQuestions(false, false);
			AssertEquals("Question, 4001 is attached", true, line.CusEntryLine.Questions.HasQuestionWithID(4001));
			AssertEquals("Question, 5001 is not attached", false, line.CusEntryLine.Questions.HasQuestionWithID(5001));
			AssertEquals("One question", 1, entryHeader.AllCPDecQuestions.Count);
			AssertEquals("Question, 4001 is in the all question collection", 4001, entryHeader.AllCPDecQuestions[0].ON_CPDecNum);
			AssertEquals("Question, 4001 is in the all question collection", 1, entryHeader.CPDecQuestionsViewCollection.Count);
			AssertEquals("Question, 4001 is in the all question collection", 4001, entryHeader.CPDecQuestionsViewCollection[0].ON_CPDecNum);

			line.JI_Tariff = "0000.00.01 00";
			testManager.GenerateQuestions(false, false);
			AssertEquals("Question, 4001 is not attached", false, line.CusEntryLine.Questions.HasQuestionWithID(4001));
			AssertEquals("Question, 5001 is attached", true, line.CusEntryLine.Questions.HasQuestionWithID(5001));
			AssertEquals("One question", 1, entryHeader.AllCPDecQuestions.Count);
			AssertEquals("Question, 5001 is in the all question collection", 5001, entryHeader.AllCPDecQuestions[0].ON_CPDecNum);
			AssertEquals("One question", 1, entryHeader.AllCPDecQuestions.Count);
			AssertEquals("Question, 5001 is in the all question collection", 5001, entryHeader.AllCPDecQuestions[0].ON_CPDecNum);
			AssertEquals("Question, 5001 is in the all question collection", 1, entryHeader.CPDecQuestionsViewCollection.Count);
			AssertEquals("Question, 5001 is in the all question collection", 5001, entryHeader.CPDecQuestionsViewCollection[0].ON_CPDecNum);
		}

		public void TestRegenerateQuestionsWhenAmendingHouseBillWhichDoesNotRequireRemerge()
		{
			ZDateTime dutyDate = new ZDateTime(2005, 1, 1);

			var profile1 = CMRCommunityProtectionProfile.New(Factory);
			profile1.CP_TariffClassificationNumberfield = "00000000";
			profile1.CP_CommunityProtectionRiskIdentifier = 400;

			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dutyDate;
			risk1.CK_LodgementQuestionIdentifier = 4001;

			var question1 = CMRLodgementQuestion.New(Factory);
			question1.CQ_LodgementQuestionIdentifier = 4001;
			question1.CQ_LodgementQuestionStartDate = dutyDate;

			var profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "00000001";
			profile2.CP_CommunityProtectionRiskIdentifier = 402;

			var risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 402;
			risk2.CK_StartDate = dutyDate;
			risk2.CK_LodgementQuestionIdentifier = 5001;

			var question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 5001;
			question2.CQ_LodgementQuestionStartDate = dutyDate;

			var line = testDec.FilteredInvoiceLines[0];
			line.JI_Tariff = "0000.00.00 00";
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = "FRM";
			testDec.JE_DateOfFirstArrival = dutyDate;

			var entryHeader = testDec.CustomsEntryHeaders[0];

			testManager.GenerateQuestions(false, false);
			entryHeader.AllCPDecQuestions[0].ON_AnswerCode = "Y";

			testManager.GenerateQuestionsForOriginalOrAmendment();//called by amendment process
			AssertEquals("Y", entryHeader.AllCPDecQuestions[0].ON_AnswerCode);

			entryHeader.AllCPDecQuestions[0].ON_AnswerCode = "N";
			testManager.GenerateQuestionsForOriginalOrAmendment();//called by amendment process
			AssertEquals("N", entryHeader.AllCPDecQuestions[0].ON_AnswerCode);
		}

		#region Implementation

		JobDeclaration testDec;
		CPQAManagerForTest testManager;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.CustomsEntryHeaders.AddNew();
			testDec.CustomsEntryHeaders[0].MergedLines.AddNew();
			testDec.Invoices.AddNew();
			testDec.FilteredInvoiceLines.AddNew();
			testDec.FilteredInvoiceLines[0].JI_CL = testDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			testManager = new CPQAManagerForTest(testDec);
		}

		class CPQAManagerForTest : CPQAManager
		{
			public CPQAManagerForTest(JobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public bool CPDecQuestionsGenerationInitiated;
			protected override void GenerateCPDecQuestions()
			{
				base.GenerateCPDecQuestions();
				CPDecQuestionsGenerationInitiated = true;
			}

			public bool LodgementQuestionsInitiated;
			protected override void GenerateLodgementQuestions(bool isWithdrawal)
			{
				base.GenerateLodgementQuestions(isWithdrawal);
				LodgementQuestionsInitiated = true;
			}
		}

		#endregion
	}
}
