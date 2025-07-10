using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRLodgementQuestionGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingForFormalLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingFormalLodge.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingPayment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingPayment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingPreLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingPreLodge.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingSAC()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingSAC.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingAmendment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingAmendment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearFormalLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearFormalLodge.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearPayment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearPayment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearPreLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearPreLodge.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearSAC()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearSAC.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearAmendment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearAmendment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailFormalLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailFormalLodge.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailPayment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailPayment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailPreLodge()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailPreLodge.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailSAC()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailSAC.Code, true);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailAmendment()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailAmendment.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForAwaitingWithdrawal()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingWithdrawal.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForFailWithdrawal()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.FailWithdrawal.Code, false);
		}

		[TestDate(2012, 2, 27)]
		public void TestGenerateLodgementQuestionForClearWithdrawal()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.ClearWithdrawal.Code, false);
		}

		[TestDate(2012, 2, 26)]
		public void TestGenerateLodgementQuestionBefore442StartDate()
		{
			AssertDeclarationQuestionsOriginalAndAmendment(CustomsEntryStatus.AwaitingFormalLodge.Code, true, false);
		}

		public void TestQuestionsStayDespiteEntryChanges()
		{
			using (AUCustomsDataRegistry.Instance.LowValueSecurityDeclarationQuestion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 442))
			{
				SetUpAmendmentAndOriginalQuestions();
				var declaration = JobDeclaration.New(Factory);
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var testGenerator = new CMRLodgementQuestionGenerator(declaration);
				CombineAssertions(() =>
				{
					testGenerator.GenerateQuestions(false);
					AssertEquals("442 is not added by default", false, entryHeader.Questions.Cast<CMRCusEntryCPDec>().Any(q => q.ON_CPDecNum == 442));
					entryHeader.Questions.AddNew().ON_CPDecNum = 442;
					entryHeader.Questions.AddNew().ON_CPDecNum = 11;
					testGenerator.GenerateQuestions(false);
					AssertEquals("442 is kept if already added", true, entryHeader.Questions.Cast<CMRCusEntryCPDec>().Any(q => q.ON_CPDecNum == 442));
					AssertEquals("Other questions will be deleted as normal", false, entryHeader.Questions.Cast<CMRCusEntryCPDec>().Any(q => q.ON_CPDecNum == 11));
				});
			}
		}

		void AssertDeclarationQuestionsOriginalAndAmendment(string messageStatus, bool isOriginalExpected, bool is442Expected = true)
		{
			SetUpAmendmentAndOriginalQuestions();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "350";
			entryHeader.ResetTotalsAndCachedValues();
			var testGenerator = new CMRLodgementQuestionGenerator(declaration);

			testGenerator.GenerateQuestions(false);
			Assert("Entry header should not have 442", !entryHeader.Questions.HasQuestionWithID(442));

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "351";
			entryHeader.ResetTotalsAndCachedValues();
			entryHeader.CH_Status = messageStatus;

			testGenerator.GenerateQuestions(false);
			if (isOriginalExpected)
			{
				AssertEquals("Entry header should have an Original question with status, " + messageStatus, true, entryHeader.Questions.HasQuestionWithID(1));
				AssertEquals("Entry header should not have an Amendment question with status, " + messageStatus, false, entryHeader.Questions.HasQuestionWithID(11));
			}
			else
			{
				AssertEquals("Entry header should have an amendment question with status, " + messageStatus, true, entryHeader.Questions.HasQuestionWithID(11));
				AssertEquals("Entry header should not not have an Original question with status, " + messageStatus, false, entryHeader.Questions.HasQuestionWithID(1));
			}

			if (is442Expected)
			{
				Assert("After the implementation date the Entry header should always have 442 question with status, " + messageStatus, entryHeader.Questions.HasQuestionWithID(442));
			}
			else
			{
				Assert("Before the implementation date the Entry header should not have 442", !entryHeader.Questions.HasQuestionWithID(442));
			}
		}

		void SetUpAmendmentAndOriginalQuestions()
		{
			CMRLodgementQuestion amendmentQuestion = Factory.New<CMRLodgementQuestion>();
			amendmentQuestion.CQ_LodgementQuestionIdentifier = 11;
			amendmentQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion originalQuestion = Factory.New<CMRLodgementQuestion>();
			originalQuestion.CQ_LodgementQuestionIdentifier = 1;
			originalQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			var securityQuestion = Factory.New<CMRLodgementQuestion>();
			securityQuestion.CQ_LodgementQuestionIdentifier = 442;
			securityQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2011, 11, 9);
		}

		public void TestGenerateQuestionsOnUPEDeclaration()
		{
			var question1 = Factory.New<CMRLodgementQuestion>();
			question1.CQ_LodgementQuestionIdentifier = 1;
			question1.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			var question2 = Factory.New<CMRLodgementQuestion>();
			question2.CQ_LodgementQuestionIdentifier = 3;
			question2.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			var question3 = Factory.New<CMRLodgementQuestion>();
			question3.CQ_LodgementQuestionIdentifier = 375;
			question3.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			var declaration = JobDeclaration.New(Factory);

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "351";

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;

			var generator = new CMRLodgementQuestionGenerator(declaration);
			generator.GenerateQuestions(false);

			Assert("Should contains the question1.", entryHeader.Questions.HasQuestionWithID(1));
			Assert("Should not contains the question2 as it's code is 3 on a UPE declaration.", !entryHeader.Questions.HasQuestionWithID(3));
			Assert("Should not contains the question3 as it's code is 375 on a UPE declaration.", !entryHeader.Questions.HasQuestionWithID(375));
		}

		public void TestQuestion15GeneratedEvenForGSTDeferredParty()
		{
			CMRLodgementQuestion question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 15;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			testHolder.CachedQuestionsExposed = new CachedAnsweredQuestions();

			DummyHeaderAttachee dummyAttachee = Factory.New<DummyHeaderAttachee>();
			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CH;
			dummyAttachee.IsStatusPostLodgeExposed = true;
			dummyAttachee.SelectionDateExposed = new ZDateTime(2005, 1, 1);
			LodgementQuestionKeys questionkeys = new LodgementQuestionKeys();
			questionkeys.IsGSTDeferred = true;
			questionkeys.IsPaid = true;
			dummyAttachee.LodgementQuestionKeyExposed = questionkeys;

			testHolder.HeadersExposed = new ICPQAHeaderAttachee[] { dummyAttachee };
			testGenerator.GenerateQuestions(false);
			AssertEquals("Q15 appears even when gst is deferred", true, dummyAttachee.Questions.HasQuestionWithID(15));

			questionkeys = new LodgementQuestionKeys();
			questionkeys.IsGSTDeferred = false;
			questionkeys.IsPaid = true;
			dummyAttachee.LodgementQuestionKeyExposed = questionkeys;
			testGenerator.GenerateQuestions(false);
			AssertEquals("DummyAttachee has a question 15 generated", true, dummyAttachee.Questions.HasQuestionWithID(15));
		}

		public void TestQuestionIDForWithdrawalForLowValueEntry()
		{
			LodgementQuestionKeys key = new LodgementQuestionKeys();
			key.IsPaid = false;
			key.TotalCustomsValue = Deminimus;
			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(key, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("Should have question 12", true, result.Contains(new ZInt(12)));
			AssertEquals("Should have question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Should not have question 10", false, result.Contains(new ZInt(10)));
			AssertEquals("Should have question 15", true, result.Contains(new ZInt(15)));
			AssertEquals("Should have question 13", true, result.Contains(new ZInt(13)));

			key.IsPaid = false;
			key.TotalCustomsValue = Deminimus + 1;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(key, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("Should have question 12 until Customs fixes missing PAYREC", true, result.Contains(new ZInt(12)));
			AssertEquals("Should have question 14 until Customs fixes missing PAYREC", true, result.Contains(new ZInt(14)));
			AssertEquals("Should have question 10 until Customs fixes missing PAYREC", true, result.Contains(new ZInt(10)));
			AssertEquals("Should have question 15 until Customs fixes missing PAYREC", true, result.Contains(new ZInt(15)));
			AssertEquals("Should have question 13 until Customs fixes missing PAYREC", true, result.Contains(new ZInt(13)));

			key.TotalCustomsValue = Deminimus + 1;
			key.IsPaid = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(key, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("Should have question 12", true, result.Contains(new ZInt(12)));
			AssertEquals("Should have question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Should have question 10", true, result.Contains(new ZInt(10)));
			AssertEquals("Should have question 15", true, result.Contains(new ZInt(15)));
			AssertEquals("Should not have question 13", false, result.Contains(new ZInt(13)));
		}

		public void TestGetPreLodgeOrLodgeQuestionIDsForOptionalQuestions()
		{
			LodgementQuestionKeys key = new LodgementQuestionKeys();
			key.IsPaid = true;
			key.IsRefundAmendment = true;

			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(key, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Should have question 4", true, result.Contains(new ZInt(4)));
			AssertEquals("Should have question 10", true, result.Contains(new ZInt(10)));
			AssertEquals("Should have question 326", true, result.Contains(new ZInt(326)));
			AssertEquals("Should have question 282", true, result.Contains(new ZInt(282)));

			key.IsPaid = true;
			key.IsRefundAmendment = false;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(key, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Should have question 4", true, result.Contains(new ZInt(4)));
			AssertEquals("Should have question 10", true, result.Contains(new ZInt(10)));
			AssertEquals("Should have question 326", true, result.Contains(new ZInt(326)));
			AssertEquals("Should have question 282", true, result.Contains(new ZInt(282)));
		}

		public void TestGenerateQuestionsForAllEntryHeaders()
		{
			DummyHeaderAttachee dummyAttachee1 = Factory.New<DummyHeaderAttachee>();
			dummyAttachee1.SelectionDateExposed = new ZDateTime(2005, 1, 1);
			dummyAttachee1.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee1);
			dummyAttachee1.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CH;
			LodgementQuestionKeys key1 = new LodgementQuestionKeys();
			key1.IsSAC = true;
			dummyAttachee1.LodgementQuestionKeyExposed = key1;

			DummyHeaderAttachee dummyAttachee2 = Factory.New<DummyHeaderAttachee>();
			dummyAttachee2.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee2);
			dummyAttachee2.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CH;
			dummyAttachee2.SelectionDateExposed = new ZDateTime(2005, 1, 1);
			LodgementQuestionKeys key2 = new LodgementQuestionKeys();
			key2.IsSAC = true;
			dummyAttachee2.LodgementQuestionKeyExposed = key2;

			testHolder.HeadersExposed = new ICPQAHeaderAttachee[] { dummyAttachee1, dummyAttachee2 };
			testGenerator.GenerateQuestions(false);

			AssertEquals("Question 16", GetExpectedLodgementQuestionExistenceWithQuestionIDAndStartDate(16, dummyAttachee1.SelectionDate), dummyAttachee2.Questions.HasQuestionWithID(16));
			AssertEquals("Question 17", GetExpectedLodgementQuestionExistenceWithQuestionIDAndStartDate(17, dummyAttachee1.SelectionDate), dummyAttachee2.Questions.HasQuestionWithID(17));
			AssertEquals("Question 18", GetExpectedLodgementQuestionExistenceWithQuestionIDAndStartDate(18, dummyAttachee1.SelectionDate), dummyAttachee2.Questions.HasQuestionWithID(18));
			AssertEquals("Question 19", GetExpectedLodgementQuestionExistenceWithQuestionIDAndStartDate(19, dummyAttachee1.SelectionDate), dummyAttachee2.Questions.HasQuestionWithID(19));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		bool GetExpectedLodgementQuestionExistenceWithQuestionIDAndStartDate(ZInt questionID, ZDateTime selectionDate)
		{
			ZQuery filter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, questionID);
			filter.AddToFilter(JoinCondition.And, CMRLodgementQuestionSchema.CQ_LodgementQuestionStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, selectionDate);

			ZQuery endDateFilter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionEndDate, SQLComparisonOperator.Equal, null);
			endDateFilter.AddToFilter(JoinCondition.Or, CMRLodgementQuestionSchema.CQ_LodgementQuestionEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, selectionDate);

			filter.AddToFilter(endDateFilter, JoinCondition.And);
			return Factory.GetDatabaseCount(typeof(CMRLodgementQuestion), filter) > 0;
		}

		ZDecimal Deminimus
		{
			get
			{
				return UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());
			}
		}

		public void TestGenerateQuestions()
		{
			CMRLodgementQuestion question1 = Factory.New<CMRLodgementQuestion>();
			question1.CQ_LodgementQuestionIdentifier = 400;
			question1.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question1.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion question1Duplicate = Factory.New<CMRLodgementQuestion>();
			question1Duplicate.CQ_LodgementQuestionIdentifier = question1.CQ_LodgementQuestionIdentifier;
			question1Duplicate.CQ_LodgementQuestionStartDate = question1.CQ_LodgementQuestionStartDate;
			question1Duplicate.CQ_LodgementQuestionEndDate = question1.CQ_LodgementQuestionEndDate;

			AssertEquals("has the same key", question1.Key, question1Duplicate.Key);

			CMRLodgementQuestion question2 = Factory.New<CMRLodgementQuestion>();
			question2.CQ_LodgementQuestionIdentifier = 401;
			question2.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 2);
			question2.CQ_LodgementQuestionEndDate = ZDateTime.Empty;

			DummyHeaderAttachee dummyAttachee = Factory.New<DummyHeaderAttachee>();
			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CH;
			testGenerator.GenerateQuestions(dummyAttachee, new CMRLodgementQuestion[] { question1, question1Duplicate, question2 }, MessageTypesForCPQAGenerator.Original);
			AssertEquals("There should be two questions added", 2, dummyAttachee.Questions.Count);
			AssertEquals("First question", question1.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[0].ON_CPDecNum);
			AssertEquals("First question", question1.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[0].ON_CPDecStartDate);
			AssertEquals("First question", question1.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[0].ON_CPDecEndDate);

			AssertEquals("Second question", question2.CQ_LodgementQuestionIdentifier, dummyAttachee.Questions[1].ON_CPDecNum);
			AssertEquals("Second question", question2.CQ_LodgementQuestionStartDate, dummyAttachee.Questions[1].ON_CPDecStartDate);
			AssertEquals("Second question", question2.CQ_LodgementQuestionEndDate, dummyAttachee.Questions[1].ON_CPDecEndDate);
		}

		public void TestGenerateLowValueQuestions()
		{
			SetUpLowValueLodgementQuestions();

			TaxOrFeeTestHelper.SetDeminimus(Factory, 300m);
			Env.Registry.CMRTestMode = true;

			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();
			testKeys.IsNature30 = false;
			testKeys.TotalCustomsValue = Deminimus - 1;
			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should contain question 3", true, result.Contains(new ZInt(3)));
			AssertEquals("Result should contain question 375", true, result.Contains(new ZInt(375)));

			testKeys.TotalCustomsValue = Deminimus;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should contain question 3", true, result.Contains(new ZInt(3)));
			AssertEquals("Result should contain question 375", true, result.Contains(new ZInt(375)));

			testKeys.TotalCustomsValue = Deminimus + 1;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should not contain question 3", false, result.Contains(new ZInt(3)));
			AssertEquals("Result should not contain question 375", false, result.Contains(new ZInt(375)));

			testKeys.IsNature30 = true;
			testKeys.TotalCustomsValue = Deminimus;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should not contain question 3 as it is N30", false, result.Contains(new ZInt(3)));
			AssertEquals("Result should not contain question 375 as it is N30", false, result.Contains(new ZInt(375)));
			AssertEquals("Result should not contain question 14", false, result.Contains(new ZInt(14)));
			AssertEquals("Result should not contain question 15", false, result.Contains(new ZInt(15)));

			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Result should contain question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Result should contain question 15", true, result.Contains(new ZInt(15)));
		}

		public void TestLowValueQuestionsWhenUPE()
		{
			SetUpLowValueLodgementQuestions();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 300m);
			Env.Registry.CMRTestMode = true;

			var testKeys = new LodgementQuestionKeys();
			testKeys.IsNature30 = false;
			testKeys.TotalCustomsValue = Deminimus - 1;

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2020, 3, 24);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv1";
			invoice.JZ_InvoiceAmount = 150m;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.JZ_RN_NKDefaultOrigin = "SG";
			invoice.AddInfo.ZA_PST = "GEN";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_LinePrice = 755m;
			invoiceLine.JI_Tariff = "9999.40.15 41";
			invoiceLine.JI_Description = "PERSONAL EFFECTS";
			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "NO";

			var testGenerator = new CMRLodgementQuestionGenerator(declaration);
			testGenerator.GenerateQuestions(false);
			AssertEquals("Questions generated should contain question 3", true, entryHeader.Questions.HasQuestionWithID(3));
			AssertEquals("Questions generated should contain question 375", true, entryHeader.Questions.HasQuestionWithID(375));

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			entryHeader.ResetTotalsAndCachedValues();
			testGenerator.GenerateQuestions(false);
			Assert("Unaccompanied Personal Effects: Questions generated should now not contain question 3", !entryHeader.Questions.HasQuestionWithID(3));
			Assert("Unaccompanied Personal Effects: Questions generated should now not contain question 375", !entryHeader.Questions.HasQuestionWithID(375));
		}

		public void TestGenerateProdLowValueQuestions()
		{
			SetUpLowValueLodgementQuestions();

			TaxOrFeeTestHelper.SetDeminimus(Factory, 300m);
			Env.Registry.CMRTestMode = false;

			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();
			testKeys.IsNature30 = false;
			testKeys.TotalCustomsValue = Deminimus - 1;
			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should contain question 3", true, result.Contains(new ZInt(3)));
			AssertEquals("Result should contain question 375", true, result.Contains(new ZInt(375)));

			testKeys.TotalCustomsValue = Deminimus;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should contain question 3", true, result.Contains(new ZInt(3)));
			AssertEquals("Result should contain question 375", true, result.Contains(new ZInt(375)));

			testKeys.TotalCustomsValue = Deminimus + 1;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should not contain question 3", false, result.Contains(new ZInt(3)));
			AssertEquals("Result should not contain question 375", false, result.Contains(new ZInt(375)));

			testKeys.IsNature30 = true;
			testKeys.TotalCustomsValue = Deminimus;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Result should not contain question 3 as it is N30", false, result.Contains(new ZInt(3)));
			AssertEquals("Result should not contain question 375 as it is N30", false, result.Contains(new ZInt(375)));
			AssertEquals("Result should not contain question 14", false, result.Contains(new ZInt(14)));
			AssertEquals("Result should not contain question 15", false, result.Contains(new ZInt(15)));

			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Result should contain question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Result should contain question 15", true, result.Contains(new ZInt(15)));
		}

		void SetUpLowValueLodgementQuestions()
		{
			CMRLodgementQuestion question3 = Factory.New<CMRLodgementQuestion>();
			question3.CQ_LodgementQuestionIdentifier = 3;
			question3.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question3.CQ_LodgementQuestionEndDate = new ZDateTime(2079, 1, 1);

			CMRLodgementQuestion question17 = Factory.New<CMRLodgementQuestion>();
			question17.CQ_LodgementQuestionIdentifier = 17;
			question17.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question17.CQ_LodgementQuestionEndDate = new ZDateTime(2079, 1, 1);

			CMRLodgementQuestion question18 = Factory.New<CMRLodgementQuestion>();
			question18.CQ_LodgementQuestionIdentifier = 18;
			question18.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question18.CQ_LodgementQuestionEndDate = new ZDateTime(2079, 1, 1);

			CMRLodgementQuestion question19 = Factory.New<CMRLodgementQuestion>();
			question19.CQ_LodgementQuestionIdentifier = 19;
			question19.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question19.CQ_LodgementQuestionEndDate = new ZDateTime(2079, 1, 1);

			CMRLodgementQuestion question375 = Factory.New<CMRLodgementQuestion>();
			question375.CQ_LodgementQuestionIdentifier = 375;
			question375.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question375.CQ_LodgementQuestionEndDate = new ZDateTime(2079, 1, 1);
		}

		public void TestGetOptionalLodgementQuestionForSACWithdrawal()
		{
			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();
			testKeys.IsSAC = true;
			ArrayList result = testGenerator.GetSACQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("4 should not be there", false, result.Contains(new ZInt(4)));
			AssertEquals("326 should not be there", false, result.Contains(new ZInt(326)));
			AssertEquals("282 should not be there", false, result.Contains(new ZInt(282)));

			AssertEquals("12 should be there", false, result.Contains(new ZInt(12)));
			AssertEquals("14 should be there", false, result.Contains(new ZInt(14)));
			AssertEquals("10 should not be there", false, result.Contains(new ZInt(10)));
			AssertEquals("15 should not be there", false, result.Contains(new ZInt(15)));

			testKeys.IsSACWithLine = true;
			AssertEquals("12 should be there", false, result.Contains(new ZInt(12)));
			AssertEquals("14 should be there", false, result.Contains(new ZInt(14)));
			AssertEquals("10 should be there", false, result.Contains(new ZInt(10)));
			AssertEquals("15 should be there", false, result.Contains(new ZInt(15)));

			testKeys.IsSAC = true;
			testKeys.IsSACWithLine = false;
			result = testGenerator.GetSACQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("12 should be there", true, result.Contains(new ZInt(12)));
			AssertEquals("14 should be there", true, result.Contains(new ZInt(14)));
			AssertEquals("13 should be there", true, result.Contains(new ZInt(13)));
			AssertEquals("10 should be there", false, result.Contains(new ZInt(10)));
			AssertEquals("15 should be there", false, result.Contains(new ZInt(15)));

			testKeys.IsSACWithLine = true;
			result = testGenerator.GetSACQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("12 should be there", true, result.Contains(new ZInt(12)));
			AssertEquals("14 should be there", true, result.Contains(new ZInt(14)));
			AssertEquals("13 should be there", true, result.Contains(new ZInt(13)));
			AssertEquals("10 should be there", true, result.Contains(new ZInt(10)));
			AssertEquals("15 should be there", true, result.Contains(new ZInt(15)));
		}

		public void TestGetPreLodgeOrLodgeQuestions()
		{
			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();

			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("11 should not be there", false, result.Contains(new ZInt(11)));
			AssertEquals("1 should be there", true, result.Contains(new ZInt(1)));

			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("11 should be there", true, result.Contains(new ZInt(11)));
			AssertEquals("1 should not be there", false, result.Contains(new ZInt(1)));

			testKeys.IsPaid = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("14 should be there", true, result.Contains(new ZInt(14)));
			AssertEquals("15 should be there", true, result.Contains(new ZInt(15)));

			testKeys.IsNature20 = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("14 should be there", true, result.Contains(new ZInt(14)));
			AssertEquals("15 should not be there as GST refund not relevant for N20", false, result.Contains(new ZInt(15)));

			testKeys.IsNature20 = false;
			testKeys.IsPaidUnderProtest = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("2 should be there", true, result.Contains(new ZInt(2)));

			testKeys.IsABNQuotedForLCTAndWET = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("5 should be there", true, result.Contains(new ZInt(5)));

			testKeys.IsSea = true;
			testKeys.HasFCLOrFCXLines = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("6 should be there", true, result.Contains(new ZInt(6)));
			AssertEquals("7 should be there", true, result.Contains(new ZInt(7)));

			testKeys.HasFCLOrFCXLines = false;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("6 should not be there", false, result.Contains(new ZInt(6)));
			AssertEquals("7 should not be there", false, result.Contains(new ZInt(7)));

			testKeys.HasLCLLines = true;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("8 should be there", true, result.Contains(new ZInt(8)));
			AssertEquals("9 should be there", true, result.Contains(new ZInt(9)));

			testKeys.HasLCLLines = false;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("8 should not be there", false, result.Contains(new ZInt(8)));
			AssertEquals("9 should not be there", false, result.Contains(new ZInt(9)));

			testKeys.IsPaid = true;
			testKeys.TotalCustomsValue = Deminimus + 1;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("12 should not be there", true, result.Contains(new ZInt(12)));
			AssertEquals("14 should be there", true, result.Contains(new ZInt(14)));
			AssertEquals("13 should not be there as PAYREC has come back", false, result.Contains(new ZInt(13)));
			AssertEquals("15 should be there", true, result.Contains(new ZInt(15)));

			testKeys.IsPaid = false;
			testKeys.TotalCustomsValue = Deminimus + 1;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("12 should be there", true, result.Contains(new ZInt(12)));
			AssertEquals("14 should not be there", true, result.Contains(new ZInt(14)));
			AssertEquals("13 should not be there", true, result.Contains(new ZInt(13)));
			AssertEquals("15 should not be there", true, result.Contains(new ZInt(15)));
		}

		public void TestGetSACQuestionIDs()
		{
			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();
			testKeys.IsSAC = true;
			ArrayList result = testGenerator.GetSACQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("There should be four question IDs", 4, result.Count);
			AssertEquals("16 is there", true, result.Contains(new ZInt(16)));
			AssertEquals("17 is there", true, result.Contains(new ZInt(17)));
			AssertEquals("18 is there", true, result.Contains(new ZInt(18)));
			AssertEquals("19 is there", true, result.Contains(new ZInt(19)));

			testKeys.IsSAC = false;
			result = testGenerator.GetSACQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Original);
			AssertEquals("There should be No question IDs", 0, result.Count);
		}

		public void TestGenerateQuestionsForWithdrawnPaidJob()
		{
			SetUpLodgementQuestionsForPaidWithdrawals();
			LodgementQuestionKeys testKeys = new LodgementQuestionKeys();
			testKeys.TotalCustomsValue = Deminimus + 1;
			testKeys.IsPaid = true;
			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("Result should contain question 10", true, result.Contains(new ZInt(10)));
			AssertEquals("Result should contain question 12", true, result.Contains(new ZInt(12)));
			AssertEquals("Result should contain question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Result should contain question 15", true, result.Contains(new ZInt(15)));
			AssertEquals("Result should not contain question 13 as PAYREC has come back", false, result.Contains(new ZInt(13)));

			testKeys.IsPaid = false;
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(testKeys, MessageTypesForCPQAGenerator.Withdraw);
			AssertEquals("Result should contain question 10", true, result.Contains(new ZInt(10)));
			AssertEquals("Result should contain question 12", true, result.Contains(new ZInt(12)));
			AssertEquals("Result should contain question 14", true, result.Contains(new ZInt(14)));
			AssertEquals("Result should contain question 15", true, result.Contains(new ZInt(15)));
			AssertEquals("Result should contain question 13", true, result.Contains(new ZInt(13)));
		}

		public void TestQuestion15IsAskedForMixedNaures()
		{
			CMRLodgementQuestion question15 = Factory.New<CMRLodgementQuestion>();
			question15.CQ_LodgementQuestionIdentifier = 15;
			question15.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);

			OrgHeader importer = OrgHeader.New(Factory);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Enterprise.Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OH_Importer = importer.PK;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			line2.JI_CL = entryLine2.PK;

			AssertEquals("Entry Header is not N20", false, entryHeader.IsCMRNature20);

			AssertEquals("Customs Charge is paid", true, entryHeader.IsCustomsChargePaid);
			ArrayList result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Q15 should be asked", true, result.Contains(new ZInt(15)));

			line1.AddInfo.ZA_IsPackToBondForLine_Hidden = "Y";
			AssertEquals("Entry Header is now N20", true, entryHeader.IsCMRNature20);
			entryHeader.ResetTotalsAndCachedValues();
			result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey, MessageTypesForCPQAGenerator.Amend);
			AssertEquals("Q15 should be asked", false, result.Contains(new ZInt(15)));
		}

		public void TestSOFALodgementQuestion()
		{
			CMRLodgementQuestion question = Factory.New<CMRLodgementQuestion>();
			question.CQ_LodgementQuestionIdentifier = 533;
			question.CQ_LodgementQuestionStartDate = new ZDateTime(2016, 03, 1);
			question.CQ_LodgementQuestionEndDate = new ZDateTime(2016, 03, 1);

			testHolder.CachedQuestionsExposed = new CachedAnsweredQuestions();

			DummyHeaderAttachee dummyAttachee = Factory.New<DummyHeaderAttachee>();
			dummyAttachee.QuestionsExposed = new CMRCusEntryCPDecCollection(dummyAttachee);
			dummyAttachee.FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_CH;
			dummyAttachee.IsStatusPostLodgeExposed = true;
			dummyAttachee.SelectionDateExposed = new ZDateTime(2016, 03, 1);
			LodgementQuestionKeys questionkeys = new LodgementQuestionKeys();
			questionkeys.IsSOFADeclaration = true;
			dummyAttachee.LodgementQuestionKeyExposed = questionkeys;

			testHolder.HeadersExposed = new ICPQAHeaderAttachee[] { dummyAttachee };
			testGenerator.GenerateQuestions(false);
			AssertEquals("Q533 appears for SOFA declaration", true, dummyAttachee.Questions.HasQuestionWithID(533));

			questionkeys = new LodgementQuestionKeys();
			questionkeys.IsSOFADeclaration = true;
			dummyAttachee.LodgementQuestionKeyExposed = questionkeys;
			testGenerator.GenerateQuestions(false);
			AssertEquals("DummyAttachee has a question 533 generated", true, dummyAttachee.Questions.HasQuestionWithID(533));
		}

		public void TestLodgementQuestionForHasRemissionOnBunkerFuels()
		{
			var importer = OrgHeader.New(Factory);
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OH_Importer = importer.PK;

			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceHeader = testDec.Invoices.AddNew();
			var treatmentCode142InvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			treatmentCode142InvoiceLine.JI_CL = entryLine.PK;
			treatmentCode142InvoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "142";
			var result = testGenerator.GetPreLodgeOrLodgeQuestionIDs(((ICPQAHeaderAttachee)entryHeader).LodgementQuestionKey, MessageTypesForCPQAGenerator.Original);
			AssertEquals("Q797 should be asked", true, result.Contains(new ZInt(797)));
			AssertEquals("Q798 should be asked", true, result.Contains(new ZInt(798)));
			AssertEquals("Q799 should be asked", true, result.Contains(new ZInt(799)));
			AssertEquals("Q800 should be asked", true, result.Contains(new ZInt(800)));
		}

		void SetUpLodgementQuestionsForPaidWithdrawals()
		{
			CMRLodgementQuestion question10 = Factory.New<CMRLodgementQuestion>();
			question10.CQ_LodgementQuestionIdentifier = 10;
			question10.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question10.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion question12 = Factory.New<CMRLodgementQuestion>();
			question12.CQ_LodgementQuestionIdentifier = 12;
			question12.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question12.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion question13 = Factory.New<CMRLodgementQuestion>();
			question13.CQ_LodgementQuestionIdentifier = 13;
			question13.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question13.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion question14 = Factory.New<CMRLodgementQuestion>();
			question14.CQ_LodgementQuestionIdentifier = 14;
			question14.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question14.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);

			CMRLodgementQuestion question15 = Factory.New<CMRLodgementQuestion>();
			question15.CQ_LodgementQuestionIdentifier = 15;
			question15.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			question15.CQ_LodgementQuestionEndDate = new ZDateTime(2005, 1, 1);
		}

		#region Implementation

		CMRLodgementQuestionGenerator testGenerator;
		DummyHolder testHolder;
		protected override void SetUp()
		{
			base.SetUp();
			testHolder = Factory.New<DummyHolder>();
			testGenerator = new CMRLodgementQuestionGenerator(testHolder);
			TaxOrFeeTestHelper.SetUp();
		}

		#endregion

	}
}
