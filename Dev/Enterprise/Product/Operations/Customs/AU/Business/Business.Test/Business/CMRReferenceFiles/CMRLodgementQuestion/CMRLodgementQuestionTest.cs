using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRLodgementQuestion))]
	sealed class CMRLodgementQuestionTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2004, 1, 1)]
		public void TestCPRiskRiskIdentifier()
		{
			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "000000";
			profile.CP_CommunityProtectionRiskIdentifier = 500;

			var profile2 = CMRCommunityProtectionProfile.New(Factory);
			profile2.CP_TariffClassificationNumberfield = "000000";
			profile2.CP_CommunityProtectionRiskIdentifier = 501;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2004, 1, 1);
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 500;
			risk.CK_LodgementQuestionIdentifier = 400;
			risk.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk.CK_EndDate = ZDateTime.Empty;

			var risk2 = Factory.New<CMRCommunityProtectionRisk>();
			risk2.CK_Identifier = 501;
			risk2.CK_LodgementQuestionIdentifier = 400;
			risk2.CK_StartDate = new ZDateTime(2004, 1, 1);
			risk2.CK_EndDate = new ZDateTime(2004, 12, 31);

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2004, 1, 1);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "00000000";

			var cPDecQuestion = entryLine.Questions.AddNew();
			cPDecQuestion.ON_CPDecNum = 400;
			cPDecQuestion.ON_CPDecStartDate = new ZDateTime(2004, 1, 1);

			AssertEquals("CPRisk ID", 501, question.GetRiskIndentifier(cPDecQuestion));
		}

		[TestDate(2004, 1, 1)]
		public void TestIsPermitRelevant()
		{
			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "000000";
			profile.CP_CommunityProtectionRiskIdentifier = 500;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 500;
			risk1.CK_StartDate = new ZDateTime(2001, 1, 1);
			risk1.CK_EndDate = new ZDateTime(2001, 12, 31);
			risk1.CK_PermitApplicationIndicator = false;
			risk1.CK_LodgementQuestionIdentifier = 400;

			var risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 500;
			risk2.CK_StartDate = new ZDateTime(2002, 1, 1);
			risk2.CK_EndDate = ZDateTime.Empty;
			risk2.CK_PermitApplicationIndicator = true;
			risk2.CK_LodgementQuestionIdentifier = 400;

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2002, 1, 1);

			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "0000000000";

			var cPQuestion = entryLine.Questions.AddNew();
			cPQuestion.ON_CPDecNum = 400;
			cPQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			cPQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			cPQuestion.ON_CL = entryLine.PK;
			AssertEquals("IsPermitRelevant", risk2.CK_PermitApplicationIndicator, question.IsPermitRelevant(cPQuestion));

			var classification = Factory.New<Classification>();
			classification.CC_TariffNum = "0000000000";
			cPQuestion = classification.Questions.AddNew();
			cPQuestion.ON_CPDecNum = 400;
			cPQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			cPQuestion.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			AssertEquals("Line attachee for this question", classification, cPQuestion.LineAttachee);
			AssertEquals("IsPermitRelevant", risk2.CK_PermitApplicationIndicator, question.IsPermitRelevant(cPQuestion));
		}

		public void TestKey()
		{
			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			AssertEquals("Key", "40001-Jan-05", question.Key);
		}

		public void TestQuestionIDAndStartDate()
		{
			var cPDecQuestion = CMRLodgementQuestion.New(Factory);
			AssertEquals("QuestionIDAndStartDate", "0", cPDecQuestion.QuestionIDAndStartDate);
			cPDecQuestion.CQ_LodgementQuestionIdentifier = 2;
			AssertEquals("QuestionIDAndStartDate", "2", cPDecQuestion.QuestionIDAndStartDate);

			cPDecQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2006, 1, 6);
			AssertEquals("QuestionIDAndStartDate", "2-06-Jan-06", cPDecQuestion.QuestionIDAndStartDate);
		}

		public void TestICodeDescriptionMembers()
		{
			var cPDecQuestion = CMRLodgementQuestion.New(Factory);
			cPDecQuestion.CQ_LodgementQuestionIdentifier = 2;
			cPDecQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2006, 1, 6);
			var cPDecICodeDescription = cPDecQuestion as ICodeDescription;
			AssertEquals("ICodeDescription.Code", "2-06-Jan-06", cPDecICodeDescription.Code);
			AssertEquals("ICodeDescription.PK", cPDecQuestion.PK, cPDecICodeDescription.PK);
			AssertEquals("ICodeDescription.Description", "UNUSED", cPDecICodeDescription.Description);
		}

		public void TestGetRisks()
		{
			var profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "000000";
			profile.CP_CommunityProtectionRiskIdentifier = 1001;

			var risk1 = CMRCommunityProtectionRisk.New(Factory);
			risk1.CK_Identifier = 1000;
			risk1.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk1.CK_PermitApplicationIndicator = false;
			risk1.CK_LodgementQuestionIdentifier = 700;

			var risk2 = CMRCommunityProtectionRisk.New(Factory);
			risk2.CK_Identifier = 1001;
			risk2.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk2.CK_PermitApplicationIndicator = true;
			risk2.CK_LodgementQuestionIdentifier = 700;

			var cPDecQuestion = CMRLodgementQuestion.New(Factory);
			cPDecQuestion.CQ_LodgementQuestionIdentifier = 700;
			cPDecQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000 00";
			invoiceLine.JI_CL = entryLine.PK;

			var question = entryLine.Questions.AddNew();
			question.ON_CPDecNum = 700;
			question.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);

			AssertEquals("Risk identifier", 1001, cPDecQuestion.GetRiskIndentifier(question));
			AssertEquals("Is permit relevant", true, cPDecQuestion.IsPermitRelevant(question));
		}

		public void TestLoad2()
		{
			SetUpQuestions();
			var result = CMRLodgementQuestion.Load(Factory, question1.CQ_LodgementQuestionIdentifier, question1.CQ_LodgementQuestionStartDate, SQLComparisonOperator.Equal);
			AssertEquals("LodgementQuestion loaded with ID and StartDate", question1, result);

			result = CMRLodgementQuestion.Load(Factory, 401, dummyAttachee.SelectionDate, SQLComparisonOperator.LessThan);
			AssertEquals("LodgementQuestion loaded with ID and StartDate", question4, result);
		}

		public void TestLoad()
		{
			SetUpQuestions();
			var results = CMRLodgementQuestion.Load(dummyAttachee, new ZInt[] { 400, 401 });

			AssertEquals("There should be only two records", 2, results.Length);

			bool hasSeenQuestion1 = false;
			bool hasSeenQuestion3 = false;

			foreach (var q in results)
			{
				hasSeenQuestion1 |= q == question1;
				hasSeenQuestion3 |= q == question3;
			}
			Assert("First record found", hasSeenQuestion1);
			Assert("Second record found", hasSeenQuestion3);

			results = CMRLodgementQuestion.Load(dummyAttachee, System.Array.Empty<ZInt>());
			AssertEquals("There should be only no record", 0, results.Length);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRLodgementQuestion.New(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			dummyAttachee = Factory.New<DummyLineAttachee>();
		}

		DummyLineAttachee dummyAttachee;
		CMRLodgementQuestion question1;
		CMRLodgementQuestion question2;
		CMRLodgementQuestion question3;
		CMRLodgementQuestion question4;

		void SetUpQuestions()
		{
			dummyAttachee.SelectionDateExposed = new ZDateTime(2005, 1, 1);
			question1 = Factory.New<CMRLodgementQuestion>();
			question1.CQ_LodgementQuestionIdentifier = 400;
			question1.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate;
			question1.CQ_LodgementQuestionEndDate = dummyAttachee.SelectionDate;

			question2 = Factory.New<CMRLodgementQuestion>();
			question2.CQ_LodgementQuestionIdentifier = 400;
			question2.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate.AddDays(1);
			question2.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			question3 = Factory.New<CMRLodgementQuestion>();
			question3.CQ_LodgementQuestionIdentifier = 401;
			question3.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate;
			question3.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			question4 = Factory.New<CMRLodgementQuestion>();
			question4.CQ_LodgementQuestionIdentifier = 401;
			question4.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate.AddDays(-1);
			question4.CQ_LodgementQuestionEndDate = dummyAttachee.SelectionDate.AddDays(-1);
		}
	}
}
