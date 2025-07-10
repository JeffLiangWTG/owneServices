using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCusEntryCPDec))]
	public class CMRCusEntryCPDecTest : EnterpriseBusinessObjectTestCase
	{
		public void TestQuestionsForPaidEntryAreConditionalForN30()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("PreCondition:IsNature30", true, entryHeader.IsNature30);
			AssertEquals("Not paid yet", false, entryHeader.IsCustomsChargePaid);

			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			foreach (int questionID in new int[] { 4, 6, 7, 8, 9, 10, 12, 13, 14, 15, 282, 326, 375 })
			{
				question.ON_CPDecNum = questionID;
				AssertEquals("Should be Conditional" + questionID, true, question.IsOptionalQuestion);
			}
		}

		public void TestQuestion10IsConditional()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 10;
			AssertEquals("Conditional question", true, question.IsOptionalQuestion);
		}

		public void TestLineAttacheeWithOrgHeader()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);

			CMRCusEntryCPDec question = orgAttachee.Questions.AddNew();
			AssertEquals("Line Attachee should be OrgHeader", org.PK, question.LineAttachee.PK);
		}

		[TestDate(2001, 1, 1)]
		public void TestON_PermitReadOnlyIsGovernedByAnswerOnly()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "00000000 00";

			CMRCusEntryCPDec cPQuestion = entryLine.Questions.AddNew();
			cPQuestion.ON_CPDecNum = 400;
			cPQuestion.ON_CL = entryLine.PK;

			cPQuestion.ON_AnswerCode = Answers.YES;
			cPQuestion.ON_Permit = "XXX";
			AssertEquals("ON_PermitInfo.ReadOnly", false, cPQuestion.ON_PermitInfo.ReadOnly);

			cPQuestion.ON_AnswerCode = Answers.NO;
			AssertEquals("ON_PermitInfo.ReadOnly", true, cPQuestion.ON_PermitInfo.ReadOnly);
			AssertEquals("Permit should have been cleared", ZString.Empty, cPQuestion.ON_Permit);
		}

		public void TestQuestionID()
		{
			ZDateTime currentDate = ZDateTime.Today;
			CMRCusEntryCPDec question = Factory.New<CMRCusEntryCPDec>();
			question.ON_CPDecNum = 123;
			AssertEquals("QuestionID", "123", question.QuestionID);
		}

		public void TestSettingQuestionIDWithCompleteCode()
		{
			ZDateTime currentDate = new ZDateTime(2006, 1, 1);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);

			CMRCusEntryCPDec question = orgAttachee.Questions.AddNew();
			question.ON_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			question.QuestionID = "400";
			AssertEquals("ON_CPDecNum", 400, question.ON_CPDecNum);

			question.QuestionID = "bla";
			AssertEquals("ON_CPDecNum", 400, question.ON_CPDecNum);

			question.QuestionID = "400-1-JAN-2006";
			AssertEquals("ON_CPDecNum", 400, question.ON_CPDecNum);
			AssertEquals("StartDate", currentDate, question.ON_CPDecStartDate);
		}

		public void TestSettingQuestionIDSetsTheMostAvailableStartDate()
		{
			ZDateTime currentDate = ZDateTime.Today;
			CMRLodgementQuestion lodgementQuestion1 = CMRLodgementQuestion.New(Factory);
			lodgementQuestion1.CQ_LodgementQuestionIdentifier = 1000;
			lodgementQuestion1.CQ_LodgementQuestionType = "CPQ";
			lodgementQuestion1.CQ_LodgementQuestionStartDate = currentDate.AddDays(-100);
			lodgementQuestion1.CQ_LodgementQuestionEndDate = currentDate.AddDays(-3);

			CMRLodgementQuestion lodgementQuestion2 = CMRLodgementQuestion.New(Factory);
			lodgementQuestion2.CQ_LodgementQuestionIdentifier = 1000;
			lodgementQuestion2.CQ_LodgementQuestionType = "CPQ";
			lodgementQuestion2.CQ_LodgementQuestionStartDate = currentDate.AddDays(-4);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;

			Factory.Save();

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec = orgAttachee.Questions.AddNew();
			cPDec.QuestionID = "1000";
			AssertEquals("Question Start Date", currentDate.AddDays(-4), cPDec.ON_CPDecStartDate);
		}

		public void TestQuestionType()
		{
			CMRLodgementQuestion question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			question.CQ_LodgementQuestionName = "TESTTEST";

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec qA = entryHeader.Questions.AddNew();
			qA.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			AssertNull("LodgementQuetsion", qA.LodgementQuestion);

			qA.ON_CPDecNum = 400;
			AssertNotNull("LodgementQuetsion", qA.LodgementQuestion);
			AssertEquals("Question type", question.CQ_LodgementQuestionName, qA.QuestionType);
		}

		public void TestEntryLineDescription()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = (short)10;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00 00";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Description = "BOOKS";

			CMRCusEntryCPDec question = entryLine.Questions.AddNew();

			AssertEquals("Entry Line description", "Entry Line No: 10/0000.00.00 00/BOOKS", question.EntryLineDescription);
		}

		public void TestQUestionIDReadonly()
		{
			CMRCusEntryCPDec question = Factory.New<CMRCusEntryCPDec>();
			AssertEquals("QUestion id is readonly", true, question.ON_CPDecNumInfo.ReadOnly);
		}

		[TestDate(2001, 1, 1)]
		public void TestIsPermitRelevant()
		{
			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			profile.CP_TariffClassificationNumberfield = "00000000";

			CMRLodgementQuestion question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 400;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
			question.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 500;
			risk.CK_StartDate = new ZDateTime(2001, 1, 1);
			risk.CK_EndDate = new ZDateTime(2001, 12, 31);
			risk.CK_LodgementQuestionIdentifier = 400;
			risk.CK_PermitApplicationIndicator = true;

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2001, 1, 1);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "00000000 00";

			CMRCusEntryCPDec cPQuestion = entryLine.Questions.AddNew();
			cPQuestion.ON_CPDecNum = 400;
			cPQuestion.ON_CPDecStartDate = question.CQ_LodgementQuestionStartDate;
			cPQuestion.ON_CL = entryLine.PK;
			AssertEquals("IsPermitRelevant", true, cPQuestion.IsPermitRelevant);

			cPQuestion.ON_AnswerCode = Answers.YES;
			AssertEquals("IsPermitRelevant", true, cPQuestion.IsPermitRelevant);
		}

		public void TestIsYes()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_AnswerCode = Answers.YES;
			AssertEquals("Is Yes", true, question.IsYes);

			question.ON_AnswerCode = Answers.NO;
			AssertEquals("Is Yes", false, question.IsYes);
		}

		public void TestIsAnswered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			AssertEquals("Is not answered", false, question.IsAnswered);

			question.ON_AnswerCode = Answers.NO;
			AssertEquals("Is answered now", true, question.IsAnswered);
		}

		public void TestRelatedBizObjects()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CMRCusEntryCPDec testCPDec = (CMRCusEntryCPDec)GetNewBusinessObject();
			testCPDec.ON_CH = entryHeader.PK;
			AssertEquals("EntryHeader", entryHeader, testCPDec.EntryHeader);
			AssertEquals("JobDeclaration", testDec, testCPDec.Declaration);

			testCPDec.ON_CH = ZGuid.Empty;
			testCPDec.ON_CL = entryLine.PK;
			AssertEquals("EntryHeader", null, testCPDec.EntryHeader);
			AssertEquals("Entry Line", entryLine, testCPDec.EntryLine);
			AssertEquals("JobDeclaration", testDec, testCPDec.Declaration);
		}

		public void TestAcknowledgeQuestions()
		{
			CMRCusEntryCPDec testCPDec = (CMRCusEntryCPDec)GetNewBusinessObject();
			ArrayList result = testCPDec.AcknowledgeQuestions;
			AssertEquals("1 is ack", true, result.Contains(new ZInt(1)));
			AssertEquals("2 is ack", true, result.Contains(new ZInt(2)));
			AssertEquals("3 is ack", true, result.Contains(new ZInt(3)));
			AssertEquals("4 is ack", true, result.Contains(new ZInt(4)));
			AssertEquals("5 is ack", true, result.Contains(new ZInt(5)));
			AssertEquals("10 is ack", true, result.Contains(new ZInt(10)));
			AssertEquals("11 is ack", true, result.Contains(new ZInt(11)));
			AssertEquals("12 is ack", true, result.Contains(new ZInt(12)));
			AssertEquals("13 is ack", true, result.Contains(new ZInt(13)));
			AssertEquals("16 is ack", true, result.Contains(new ZInt(16)));
			AssertEquals("326 is ack", true, result.Contains(new ZInt(326)));
			AssertEquals("375 is ack", true, result.Contains(new ZInt(375)));
			AssertEquals("508 is ack", true, result.Contains(new ZInt(508)));
			AssertEquals("533 is ack", true, result.Contains(new ZInt(533)));
		}

		public void TestIsAcknowledge()
		{
			CMRCusEntryCPDec testCPDec = (CMRCusEntryCPDec)GetNewBusinessObject();
			testCPDec.ON_CPDecNum = 1;
			testCPDec.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			AssertEquals("IsAcknowledge", true, testCPDec.IsAcknowledge);
		}

		public void TestLodgementQuestion()
		{
			CMRLodgementQuestion question1 = Factory.New<CMRLodgementQuestion>();
			question1.CQ_LodgementQuestionIdentifier = 400;
			question1.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
			question1.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			CMRLodgementQuestion question2 = Factory.New<CMRLodgementQuestion>();
			question2.CQ_LodgementQuestionIdentifier = 401;
			question2.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 1);
			question2.CQ_LodgementQuestionEndDate = new ZDateTime(2001, 1, 1);
			question2.CQ_LodgementQuestionText = "Cuckoo Squeakers are great!";

			CMRLodgementQuestion question3 = Factory.New<CMRLodgementQuestion>();
			question3.CQ_LodgementQuestionIdentifier = 401;
			question3.CQ_LodgementQuestionStartDate = new ZDateTime(2001, 1, 2);
			question3.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			CMRCusEntryCPDec testCPDec = (CMRCusEntryCPDec)GetNewBusinessObject();
			testCPDec.ON_CPDecNum = 401;
			testCPDec.ON_CPDecStartDate = new ZDateTime(2001, 1, 1);
			AssertEquals("Lodgement Question", question2, testCPDec.LodgementQuestion);
			AssertEquals("Lodgement Question", question2.CQ_LodgementQuestionText, testCPDec.Question);
		}

		public void TestAQISDecsNoLongerRequiredAfterDelivery()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			CusContainer container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			CusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			CMRCusEntryCPDec question1 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question2 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question3 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question4 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question5 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question6 = entryHeader.Questions.AddNew();
			CMRCusEntryCPDec question7 = entryHeader.Questions.AddNew();
			question1.ON_CPDecNum = 11;
			question1.ON_AnswerCode = Answers.YES;
			question2.ON_CPDecNum = 6;
			question2.ON_AnswerCode = Answers.YES;
			question3.ON_CPDecNum = 7;
			question3.ON_AnswerCode = Answers.NO;
			question4.ON_CPDecNum = 8;
			question4.ON_AnswerCode = Answers.NO;
			question5.ON_CPDecNum = 9;
			question5.ON_AnswerCode = Answers.YES;
			question6.ON_CPDecNum = 4;
			question6.ON_AnswerCode = Answers.YES;

			question7.ON_CPDecNum = 14;
			question7.ON_AnswerCode = Answers.NO;

			AssertEquals("Non-AQIS mandatory dec should not change", Answers.YES, question1.ON_AnswerCode);
			AssertEquals("Non-AQIS mandatory dec should not be optional", false, question1.IsOptionalQuestion);
			AssertEquals("AQIS dec should not change yet", Answers.YES, question2.ON_AnswerCode);
			AssertEquals("AQIS dec should not be optional", false, question2.IsOptionalQuestion);
			AssertEquals("AQIS dec should not change yet", Answers.NO, question3.ON_AnswerCode);
			AssertEquals("AQIS dec should not be optional", false, question3.IsOptionalQuestion);
			AssertEquals("AQIS dec should not change yet", Answers.NO, question4.ON_AnswerCode);
			AssertEquals("AQIS dec should not be optional", false, question4.IsOptionalQuestion);
			AssertEquals("AQIS dec should not change yet", Answers.YES, question5.ON_AnswerCode);
			AssertEquals("AQIS dec should not be optional", false, question5.IsOptionalQuestion);
			AssertEquals("Non-AQIS optional dec should not change", Answers.YES, question6.ON_AnswerCode);
			AssertEquals("Non-AQIS optional dec should be optional", true, question6.IsOptionalQuestion);
			AssertEquals("Delivered dec should not change", Answers.NO, question7.ON_AnswerCode);
			AssertEquals("Delivered dec should not be optional", false, question7.IsOptionalQuestion);

			question7.ON_AnswerCode = Answers.YES;
			AssertEquals("Non-AQIS mandatory dec should not change", Answers.YES, question1.ON_AnswerCode);
			AssertEquals("Non-AQIS mandatory dec should not be optional", false, question1.IsOptionalQuestion);
			AssertEquals("AQIS dec should now be null", ZString.Empty, question2.ON_AnswerCode);
			AssertEquals("AQIS dec should now be optional", true, question2.IsOptionalQuestion);
			AssertEquals("AQIS dec should now be null", ZString.Empty, question3.ON_AnswerCode);
			AssertEquals("AQIS dec should now be optional", true, question3.IsOptionalQuestion);
			AssertEquals("AQIS dec should now be null", ZString.Empty, question4.ON_AnswerCode);
			AssertEquals("AQIS dec should now be optional", true, question4.IsOptionalQuestion);
			AssertEquals("AQIS dec should now be null", ZString.Empty, question5.ON_AnswerCode);
			AssertEquals("AQIS dec should now be optional", true, question5.IsOptionalQuestion);
			AssertEquals("Non-AQIS optional dec should not change", Answers.YES, question6.ON_AnswerCode);
			AssertEquals("Non-AQIS optional dec should be optional", true, question6.IsOptionalQuestion);
			AssertEquals("Delivered dec should not change", Answers.YES, question7.ON_AnswerCode);
			AssertEquals("Delivered dec should not be optional", false, question7.IsOptionalQuestion);

			question2.ON_AnswerCode = Answers.YES;
			AssertEquals("Should have message error", true, question2.HasMessageErrors);
			question2.ON_AnswerCode = ZString.Empty;
			AssertEquals("Should not have message error", false, question2.HasMessageErrors);
			question3.ON_AnswerCode = Answers.YES;
			AssertEquals("Should have message error", true, question3.HasMessageErrors);
			question3.ON_AnswerCode = ZString.Empty;
			AssertEquals("Should not have message error", false, question3.HasMessageErrors);
			question4.ON_AnswerCode = Answers.YES;
			AssertEquals("Should have message error", true, question4.HasMessageErrors);
			question4.ON_AnswerCode = ZString.Empty;
			AssertEquals("Should not have message error", false, question4.HasMessageErrors);
			question5.ON_AnswerCode = Answers.YES;
			AssertEquals("Should have message error", true, question5.HasMessageErrors);
			question5.ON_AnswerCode = ZString.Empty;
			AssertEquals("Should not have message error", false, question5.HasMessageErrors);
			question6.ON_AnswerCode = Answers.YES;
			AssertEquals("Non-AQIS optional dec should not have message error", false, question6.HasMessageErrors);
			question6.ON_AnswerCode = ZString.Empty;
			AssertEquals("Non-AQIS optional dec should not have message error", false, question6.HasMessageErrors);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(CMRCusEntryCPDec));
		}
	}
}
