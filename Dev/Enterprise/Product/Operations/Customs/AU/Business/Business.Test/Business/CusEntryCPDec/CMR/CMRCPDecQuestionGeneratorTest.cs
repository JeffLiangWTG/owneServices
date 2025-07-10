using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRCPDecQuestionGeneratorTest : TestCaseWithFactory
	{
		public void TestGetDefaultAnswers()
		{
			SetRisksAndLodgmentQuestions();

			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;

			DummyLineAttachee sourceToDefault = Factory.New<DummyLineAttachee>();
			sourceToDefault.QuestionsExposed = new CMRCusEntryCPDecCollection(sourceToDefault);

			dummyAttachee.DefaultUniqueQuestionsExposed = new LineDefaultQuestions(new[]
			{
					new LineDefaultQuestions.DefaultAnswer()
					{
						CPDecNum = 401,
						CPDecStartDate = dummyAttachee.SelectionDate,
						AnswerCode = CMRCusEntryCPDec.Answers.YES,
						Permit = "XXXYYY333"
					}
				});
			dummyAttachee.SourcesToDefaultExposed = new ICPQALineAttachee[] { sourceToDefault };

			testGenerator.GenerateQuestionForLoadedRisks(dummyAttachee, new ZInt[] { 401 });
			AssertEquals("Yes answer is defaulted", CMRCusEntryCPDec.Answers.YES, dummyAttachee.Questions[0].ON_AnswerCode);
			AssertEquals("Permit number is defaulted", "XXXYYY333", dummyAttachee.Questions[0].ON_Permit);
		}

		public void TestPreviousAnswersDoNotGetUsedIfInvoiceLinesHaveDifferentAnswers()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);

			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";
			invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode = "N";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);

			AssertEquals("Previous answers should not be copied", ZString.Empty, invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
		}

		public void TestWhenMergeByChangesToNON()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode = "Y";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);

			AssertEquals("Previous answers can be copied", "Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("Previous answers can be copied", "Y", invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode);
		}

		public void TestWhenMergeByChangesToNONWithProductDefaults()
		{
			var profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_ModeofTransportfield = "S";
			profile.CP_OriginCountryCodefield = "KR";
			profile.CP_StatisticalClassificationCodefield = "11";
			profile.CP_TariffClassificationNumberfield = "00000000";

			var risk = Factory.New<CMRCommunityProtectionRisk>();
			risk.CK_Identifier = 400;
			risk.CK_StartDate = ZDateTime.BrettsBirthday;
			risk.CK_LodgementQuestionIdentifier = 401;

			var question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 401;
			question.CQ_LodgementQuestionStartDate = ZDateTime.BrettsBirthday;
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "TEST";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			var classification = Factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "0000000011";

			var pivot1 = product.AddNewImportPivotWithClassification(classification.PK);
			pivot1.ClassificationAddInfo.ZA_ORG = "KR";
			pivot1.RefreshQuestions();
			pivot1.Questions[0].ON_AnswerCode = "Y";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000011";
			invoiceLine1.JI_CountryOfOrigin = "KR";
			invoiceLine1.JI_PartNo = "TEST";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000011";
			invoiceLine2.JI_CountryOfOrigin = "KR";
			invoiceLine2.JI_PartNo = "TEST";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0000000011";
			invoiceLine3.JI_CountryOfOrigin = "KR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine3.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals("Answer defaulted from product", "Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);

			invoiceLine3.CusEntryLine.Questions[0].ON_AnswerCode = "N";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals(invoiceLine1.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1, invoiceLine1.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine2.CusEntryLine.Questions.Count);
			AssertEquals(1, invoiceLine3.CusEntryLine.Questions.Count);

			AssertEquals("Previous answers can be copied", "Y", invoiceLine1.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("Previous answers can be copied", "Y", invoiceLine2.CusEntryLine.Questions[0].ON_AnswerCode);
			AssertEquals("Previous answers against the invoice line is copied", "N", invoiceLine3.CusEntryLine.Questions[0].ON_AnswerCode);
		}

		public void TestGetUniqueQuestionIDs()
		{
			SetRisksAndLodgmentQuestions();

			risk1.CK_LodgementQuestionIdentifier = 401;
			risk2.CK_LodgementQuestionIdentifier = 401;

			ZInt[] result = testGenerator.GetUniqueLodgementQuestionIDs(new CMRCommunityProtectionRisk[] { risk1, risk2 });
			AssertEquals("Unique Q IDs", 1, result.Length);
			AssertEquals("Unique Q IDs", 401, result[0]);
		}

		public void TestGenerateQuestionForLoadedRisks()
		{
			SetRisksAndLodgmentQuestions();

			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;
			AssertEquals("There should be no question added", 0, dummyAttachee.Questions.Count);

			testGenerator.GenerateQuestionForLoadedRisks(dummyAttachee, new ZInt[] { 401, 401 });

			AssertEquals("There should be one question added", 1, dummyAttachee.Questions.Count);
			AssertEquals("Question ID should have been filled in", question1.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[0].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question1.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[0].ON_CPDecStartDate);
			AssertEquals("EndDate should have been filled in", question1.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[0].ON_CPDecEndDate);
		}

		public void TestGenerateQuestions()
		{
			SetRisksAndLodgmentQuestions();
			SetProfiles();

			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;
			dummyAttachee.CPQuestionKeyExposed = cPQAKey;
			DummyLineAttachee dummyAttachee2 = Factory.New<DummyLineAttachee>();
			dummyAttachee2.SelectionDateExposed = dummyAttachee.SelectionDate.AddDays(1);
			dummyAttachee2.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;
			dummyAttachee2.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee2);
			dummyAttachee2.CPQuestionKeyExposed = cPQAKey;
			risk2.CK_StartDate = dummyAttachee2.SelectionDate;

			testHolder.LinesExposed = new ICPQALineAttachee[] { dummyAttachee, dummyAttachee2 };
			testGenerator.GenerateQuestions();

			AssertEquals("There should one question added", 1, dummyAttachee.Questions.Count);
			AssertEquals("Question ID should have been filled in", question1.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[0].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question1.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[0].ON_CPDecStartDate);
			AssertEquals("EndDate should have been filled in", question1.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[0].ON_CPDecEndDate);

			AssertEquals("There should one question added", 1, dummyAttachee2.Questions.Count);
			AssertEquals("Question ID should have been filled in", question2.CQ_LodgementQuestionIdentifier, dummyAttachee2.Questions[0].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question2.CQ_LodgementQuestionStartDate, dummyAttachee2.Questions[0].ON_CPDecStartDate);
			AssertEquals("EndDate should have been filled in", question2.CQ_LodgementQuestionEndDate, dummyAttachee2.Questions[0].ON_CPDecEndDate);
		}

		public void TestGenerateQuestionsWhenRiskHistorySupported()
		{
			SetRisksAndLodgmentQuestions();
			SetProfiles();
			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;
			dummyAttachee.CPQuestionKeyExposed = cPQAKey;
			dummyAttachee.IsRiskHistorySupportedExposed = true;
			testHolder.LinesExposed = new ICPQALineAttachee[] { dummyAttachee };
			testGenerator.GenerateQuestions();

			AssertEquals("There should be one question added", 1, dummyAttachee.Questions.Count);
			AssertEquals("Question ID should have been filled in", question1.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[0].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question1.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[0].ON_CPDecStartDate);
			AssertEquals("EndDate should have been filled in", question1.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[0].ON_CPDecEndDate);

			dummyAttachee.SelectionDateExposed = dummyAttachee.SelectionDate.AddDays(1);
			var question3 = dummyAttachee.Questions.AddNew();
			question3.ON_CPDecNum = 422;
			question3.ON_CPDecStartDate = dummyAttachee.SelectionDate;
			question3.ON_CPDecEndDate = dummyAttachee.SelectionDate;
			dummyAttachee.Questions[0].ON_CPDecEndDate = ZDateTime.Empty;
			AssertEquals("Pre-condition", 2, dummyAttachee.Questions.Count);
			AssertEquals("Pre-condition", ZDateTime.Empty, dummyAttachee.Questions[0].ON_CPDecEndDate);
			AssertEquals("Pre-condition", 422, dummyAttachee.Questions[1].ON_CPDecNum);

			var q1Pk = dummyAttachee.Questions[0].PK;
			testGenerator.GenerateQuestions();

			AssertEquals("There should now be two questions", 2, dummyAttachee.Questions.Count);
			AssertEquals("Question 1 should still be there", q1Pk, dummyAttachee.Questions[0].PK);
			AssertEquals("Question ID should have been filled in", question1.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[0].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question1.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[0].ON_CPDecStartDate);
			AssertEquals("EndDate should have been re-instated", question1.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[0].ON_CPDecEndDate);
			AssertEquals("Question 2 should have been removed and replaced by new version of question 1", question2.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[1].ON_CPDecNum);
			AssertEquals("StartDate should have been filled in", question2.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[1].ON_CPDecStartDate);
			AssertEquals("EndDate should be empty", ZDateTime.Empty, dummyAttachee.Questions[1].ON_CPDecEndDate);

			profile.Delete();
			testGenerator.GenerateQuestions();
			AssertEquals("There should now be no questions", 0, dummyAttachee.Questions.Count);
		}

		public void TestQuestionResponseWithOldDate()
		{
			SetRisksAndLodgmentQuestions();
			var risk3 = Factory.New<CMRCommunityProtectionRisk>();
			risk3.CK_Identifier = 483;
			risk3.CK_StartDate = new ZDateTime(2005, 1, 1);
			risk3.CK_EndDate = new ZDateTime(2016, 3, 1);
			risk3.CK_LodgementQuestionIdentifier = 483;

			var risk4 = Factory.New<CMRCommunityProtectionRisk>();
			risk4.CK_Identifier = 483;
			risk4.CK_StartDate = new ZDateTime(2016, 3, 2);
			risk4.CK_LodgementQuestionIdentifier = 483;

			var question3 = Factory.New<CMRLodgementQuestion>();
			question3.CQ_LodgementQuestionIdentifier = 483;
			question3.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question3.CQ_LodgementQuestionEndDate = dummyAttachee.SelectionDate;

			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CL;
			AssertEquals("There should be no question added", 0, dummyAttachee.Questions.Count);

			testGenerator.GenerateQuestionForLoadedRisks(dummyAttachee, new ZInt[] { 401, 401, 483 });
			AssertEquals("There should be two questions added", 2, dummyAttachee.Questions.Count);
			AssertEquals("Question ID 483 & 401 should have been returned", true, (dummyAttachee.Questions[0].ON_CPDecNum == question1.CQ_LodgementQuestionIdentifier || dummyAttachee.Questions[0].ON_CPDecNum == question3.CQ_LodgementQuestionIdentifier));
			AssertEquals("Question ID 483 & 401 should have been returned", true, (dummyAttachee.Questions[1].ON_CPDecNum == question1.CQ_LodgementQuestionIdentifier || dummyAttachee.Questions[1].ON_CPDecNum == question3.CQ_LodgementQuestionIdentifier));
		}

		#region Implementation

		void SetProfiles()
		{
			cPQAKey.TariffNumber = "00000000";
			cPQAKey.StatCode = "00";
			cPQAKey.OriginCode = "XX";
			cPQAKey.Nature = "N10";
			cPQAKey.ModeOfTransport = "A";

			profile = Factory.New<CMRCommunityProtectionProfile>();
			profile.CP_CommunityProtectionRiskIdentifier = 400;
			profile.CP_LineNatureTypefield = cPQAKey.Nature;
			profile.CP_ModeofTransportfield = cPQAKey.ModeOfTransport;
			profile.CP_OriginCountryCodefield = cPQAKey.OriginCode;
			profile.CP_StatisticalClassificationCodefield = cPQAKey.StatCode;
			profile.CP_TariffClassificationNumberfield = cPQAKey.TariffNumber;
		}

		void SetRisksAndLodgmentQuestions()
		{
			dummyAttachee = Factory.New<DummyLineAttachee>();
			dummyAttachee.SelectionDateExposed = new ZDateTime(2005, 1, 1);

			risk1 = Factory.New<CMRCommunityProtectionRisk>();
			risk1.CK_Identifier = 400;
			risk1.CK_StartDate = dummyAttachee.SelectionDate;
			risk1.CK_LodgementQuestionIdentifier = 401;

			risk2 = Factory.New<CMRCommunityProtectionRisk>();
			risk2.CK_Identifier = 400;
			risk2.CK_StartDate = dummyAttachee.SelectionDate;
			risk2.CK_LodgementQuestionIdentifier = 401;

			question1 = Factory.New<CMRLodgementQuestion>();
			question1.CQ_LodgementQuestionIdentifier = 401;
			question1.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate;
			question1.CQ_LodgementQuestionEndDate = dummyAttachee.SelectionDate;

			question2 = Factory.New<CMRLodgementQuestion>();
			question2.CQ_LodgementQuestionIdentifier = 401;
			question2.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate.AddDays(1);
			question2.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			CMRLodgementQuestion question2Duplicate = Factory.New<CMRLodgementQuestion>();
			question2Duplicate.CQ_LodgementQuestionIdentifier = 401;
			question2Duplicate.CQ_LodgementQuestionStartDate = dummyAttachee.SelectionDate.AddDays(1);
			question2Duplicate.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
		}

		CMRCommunityProtectionProfile profile;
		CMRCPDecQuestionGenerator testGenerator;
		DummyHolder testHolder;
		DummyLineAttachee dummyAttachee;
		CMRCommunityProtectionRisk risk1;
		CMRCommunityProtectionRisk risk2;
		CMRLodgementQuestion question1;
		CMRLodgementQuestion question2;
		CPQuestionKeys cPQAKey;

		protected override void SetUp()
		{
			base.SetUp();
			testHolder = Factory.New<DummyHolder>();

			testHolder.LinesExposed = new ICPQALineAttachee[] { dummyAttachee };
			testGenerator = new CMRCPDecQuestionGenerator(testHolder);
			cPQAKey = new CPQuestionKeys();
		}

		#endregion
	}
}
