using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusEntryHeaderCollectionTest : TestCaseWithFactory
	{
		public void TestHasEntryWithPostLodgeStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("HasEntryWithPostLodgeStatus", false, declaration.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("HasEntryWithPostLodgeStatus", false, declaration.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			entry.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			AssertEquals("Post Lodge status", true, entry.IsStatusPostLodge);
			AssertEquals("HasEntryWithPostLodgeStatus", true, declaration.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);

			entry.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("Post Lodge status", false, entry.IsStatusPostLodge);
			AssertEquals("HasEntryWithPostLodgeStatus", false, declaration.CustomsEntryHeaders.HasEntryWithPostLodgeStatus);
		}

		public void TestTotalAmountPayableForThisSession()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry1 = testDec.CustomsEntryHeaders.AddNew();
			entry1.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entry1.CH_TotalPaid = 1000m;
			AssertEquals("PreCondition:IsStatusPostLodge", true, entry1.IsStatusPostLodge);

			CMRIMDRMessage iMDRMessage = Factory.New<CMRIMDRMessage>();
			iMDRMessage.EM_MessageText = TestMessages.IMDRMessageText;
			iMDRMessage.EM_LinkedObject = entry1;
			iMDRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("Total payable in the message", 191.90m, new OutstandingAmountRetriever(iMDRMessage).OutstandingAmount);

			CusEntryHeader entry2 = testDec.CustomsEntryHeaders.AddNew();
			entry2.CH_TotalPaid = 2000m;

			CMRSACRMessage sACRMessage = Factory.New<CMRSACRMessage>();
			sACRMessage.EM_MessageText = TestMessages.SACRMessageTextPositive;
			sACRMessage.EM_LinkedObject = entry2;
			sACRMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals("Total payable in the message", 16m, new OutstandingAmountRetriever(sACRMessage).OutstandingAmount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Total Amount payable for this session for CMR", 2191.90m, testDec.CustomsEntryHeaders.TotalAmountPayableForThisSession);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("Total Amount payable for this session for Legacy", 3000m, testDec.CustomsEntryHeaders.TotalAmountPayableForThisSession);
		}

		public void TestAreWithdrawalLodgementQuestionsGeneratedAndAnswered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("No withdrawal questions", false, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);

			var mockCPDec = Factory.NewMoq<CMRCusEntryCPDec>();
			mockCPDec.Setup(m => m.IsOptionalQuestion).Returns(false);
			CMRCusEntryCPDec question = mockCPDec.Object;
			entryHeader.Questions.Add(question);
			question.ON_CPDecNum = 12;
			AssertEquals("Withdrawal question is generated, but not answered", false, question.IsAnswered);
			AssertEquals("No withdrawal questions", false, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);

			question.ON_AnswerCode = "Y";
			AssertEquals("Withdrawal question is generated, but not answered", true, question.IsAnswered);
			AssertEquals("No withdrawal questions", true, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);

			var mockCPDec2 = Factory.NewMoq<CMRCusEntryCPDec>();
			mockCPDec2.Setup(m => m.IsOptionalQuestion).Returns(false);
			CMRCusEntryCPDec question2 = mockCPDec2.Object;
			entryHeader.Questions.Add(question2);
			question2.ON_CPDecNum = 13;
			AssertEquals("Withdrawal question is generated, but not answered", false, question2.IsAnswered);
			AssertEquals("Withdrawal questions not answered", false, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);

			question2.ON_AnswerCode = "Y";
			AssertEquals("Withdrawal questions answered", true, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, testDec.IsSACWithoutLines);
			entryHeader.Questions.RemoveAndDeleteAll();
			AssertEquals("No Withdrawal questions for SAC", true, testDec.CustomsEntryHeaders.AreWithdrawalLodgementQuestionsGeneratedAndAnswered);
		}

		public void TestAreAmendmentLodgementQuestionsGeneratedAndAnswered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("No amendment questions", false, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);

			var mockCPDec = Factory.NewMoq<CMRCusEntryCPDec>();
			mockCPDec.Setup(m => m.IsOptionalQuestion).Returns(false);
			CMRCusEntryCPDec question = mockCPDec.Object;
			entryHeader.Questions.Add(question);
			question.ON_CPDecNum = 11;
			AssertEquals("Amendment question is generated, but not answered", false, question.IsAnswered);
			AssertEquals("Amendment questions not answered", false, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);

			question.ON_AnswerCode = "Y";
			AssertEquals("Amendment question answered", true, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);

			var mockCPDec2 = Factory.NewMoq<CMRCusEntryCPDec>();
			mockCPDec2.Setup(m => m.IsOptionalQuestion).Returns(false);
			CMRCusEntryCPDec question2 = mockCPDec2.Object;
			entryHeader.Questions.Add(question2);
			question2.ON_CPDecNum = 14;
			AssertEquals("Amendment question is generated, but not answered", false, question2.IsAnswered);
			AssertEquals("Amendment questions not answered", false, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);

			question2.ON_AnswerCode = "N";
			AssertEquals("Amendment question answered", true, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, testDec.IsSACWithoutLines);
			entryHeader.Questions.RemoveAndDeleteAll();
			AssertEquals("No Amendment questions for SAC", true, testDec.CustomsEntryHeaders.AreAmendmentLodgementQuestionsGeneratedAndAnswered);
		}

		public void TestIsAnyLodgementQuestion7AnsweredNo()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("No amendment questions", false, testDec.CustomsEntryHeaders.IsAnyLodgementQuestion7AnsweredNo);

			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 7;
			AssertEquals("Lodgement question 7 is generated, but not answered", false, question.IsAnswered);
			AssertEquals("Lodgement question 7 not answered NO", false, testDec.CustomsEntryHeaders.IsAnyLodgementQuestion7AnsweredNo);

			question.ON_AnswerCode = "Y";
			AssertEquals("Lodgement question 7 is answered NO", false, testDec.CustomsEntryHeaders.IsAnyLodgementQuestion7AnsweredNo);

			question.ON_AnswerCode = "N";
			AssertEquals("Lodgement question 7 is answered NO", true, testDec.CustomsEntryHeaders.IsAnyLodgementQuestion7AnsweredNo);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, testDec.IsSAC);
			AssertEquals("For SAC it must always return false", false, testDec.CustomsEntryHeaders.IsAnyLodgementQuestion7AnsweredNo);
		}

		public void TestDoAllEntriesHaveEntryNumber()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader1 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("DoAllEntriesHaveEntryNumber", false, testDec.CustomsEntryHeaders.DoAllEntriesHaveEntryNumber);

			entryHeader1.EntryNumber = "AAA";
			AssertEquals("DoAllEntriesHaveEntryNumber", false, testDec.CustomsEntryHeaders.DoAllEntriesHaveEntryNumber);

			entryHeader2.EntryNumber = "BBB";
			AssertEquals("DoAllEntriesHaveEntryNumber", true, testDec.CustomsEntryHeaders.DoAllEntriesHaveEntryNumber);
		}

		public void TestAreCPQuestionsAnswered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec q1 = entryHeader.Questions.AddNew();

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CMRCusEntryCPDec q2 = entryLine.Questions.AddNew();

			AssertEquals("AreCPQuestionsAnswered", false, testDec.CustomsEntryHeaders.AreAllCPQuestionsAnswered);

			q1.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
			AssertEquals("AreCPQuestionsAnswered", false, testDec.CustomsEntryHeaders.AreAllCPQuestionsAnswered);

			q2.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			AssertEquals("AreCPQuestionsAnswered", true, testDec.CustomsEntryHeaders.AreAllCPQuestionsAnswered);
		}

		public void TestStatusNeedsRecalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var collection = declaration.CustomsEntryHeaders;
			AssertEquals("Status Needs Recalculation", false, collection.StatusNeedsRecalculation);

			CusEntryHeader entryHeader = collection.AddNew();
			AssertEquals("Status Needs Recalculation", false, collection.StatusNeedsRecalculation);

			entryHeader.Messages.AddNew().EM_MessageText = "A";
			AssertEquals("Status Needs Recalculation", true, collection.StatusNeedsRecalculation);
		}

		public void TestConstructor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			Assert(collection != null);
		}

		public void TestTypedAddNew()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header = collection.AddNew();
			Assert(header != null);
		}

		public void TestSortEntries()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryHeader header2 = collection.AddNew();

			CusEntryLine entryLine1 = header1.MergedLines.AddNew();
			CusEntryLine entryLine2 = header2.MergedLines.AddNew();
			CusEntryLine entryLine3 = header2.MergedLines.AddNew();

			JobComInvoiceHeader invHeader1 = Factory.New<JobComInvoiceHeader>();
			invHeader1.JZ_InvoiceNumber = "BBB";
			invHeader1.JZ_JE = declaration.PK;
			JobComInvoiceHeader invHeader2 = Factory.New<JobComInvoiceHeader>();
			invHeader2.JZ_InvoiceNumber = "AAA";
			invHeader2.JZ_JE = declaration.PK;

			JobComInvoiceLine invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_LineNo = 1;

			JobComInvoiceLine invLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invLine2.JI_LineNo = 1;
			invLine2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;

			JobComInvoiceLine invLine3 = invHeader2.JobComInvoiceLines.AddNew();
			invLine3.JI_LineNo = 2;
			invLine3.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;

			invLine1.JI_CL = entryLine1.PK;
			invLine2.JI_CL = entryLine2.PK;
			invLine3.JI_CL = entryLine3.PK;

			AssertEquals(header1, collection[0]);
			AssertEquals(header2, collection[1]);

			collection.CustomSort();

			AssertEquals(header1, collection[1]);
			AssertEquals(header2, collection[0]);
		}

		public void TestDutyAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryLine line1 = header1.MergedLines.AddNew();
			CusEntryHeader header2 = collection.AddNew();
			CusEntryLine line2 = header2.MergedLines.AddNew();
			AssertEquals("PreCondition", 0m, collection.DutyAmount);
			line1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100);
			AssertEquals("After adding first header", 100m, collection.DutyAmount);
			line2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50m);
			AssertEquals("After adding second header", 150m, collection.DutyAmount);
		}

		public void TestGSTVATAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryLine line1 = header1.MergedLines.AddNew();
			CusEntryHeader header2 = collection.AddNew();
			CusEntryLine line2 = header2.MergedLines.AddNew();
			AssertEquals("PreCondition", 0m, collection.GSTAmount);
			line1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 100m);
			AssertEquals("After adding first header", 100m, collection.GSTAmount);
			line2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 50m);
			AssertEquals("After adding second header", 150m, collection.GSTAmount);
		}

		public void TestLCTAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryLine line1 = header1.MergedLines.AddNew();
			CusEntryHeader header2 = collection.AddNew();
			CusEntryLine line2 = header2.MergedLines.AddNew();
			AssertEquals("PreCondition", 0m, collection.LCTAmount);
			line1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 100);
			AssertEquals("After adding first header", 100m, collection.LCTAmount);
			line2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 50);
			AssertEquals("After adding second header", 150m, collection.LCTAmount);
		}

		public void TestWETAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryLine line1 = header1.MergedLines.AddNew();
			CusEntryHeader header2 = collection.AddNew();
			CusEntryLine line2 = header2.MergedLines.AddNew();
			AssertEquals("PreCondition", 0m, collection.WETAmount);
			line1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 100);
			AssertEquals("After adding first header", 100m, collection.WETAmount);
			line2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 50);
			AssertEquals("After adding second header", 150m, collection.WETAmount);
		}

		public void TestWoodLevyAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeaderCollection collection = new CusEntryHeaderCollection(declaration, Factory);
			CusEntryHeader header1 = collection.AddNew();
			CusEntryHeader header2 = collection.AddNew();
			AssertEquals("PreCondition", 0m, collection.WoodLevyAmount);
			header1.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 100);
			AssertEquals("After adding first header", 100m, collection.WoodLevyAmount);
			header2.Charges.AddNew(CusEntryChargeTypeList.Codes.Woodlevy, 50);
			AssertEquals("After adding second header", 150m, collection.WoodLevyAmount);
		}
	}
}
