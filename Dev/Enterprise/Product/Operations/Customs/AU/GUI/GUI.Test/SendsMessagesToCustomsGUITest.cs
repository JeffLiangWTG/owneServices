using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class SendsMessagesToCustomsGUITest : TestCaseWithFactory
	{
		[TestDate(2005, 1, 1)]
		public void TestGenerateDeclarationQuestionIfRequiredAndShowCPQAForm()
		{
			var lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 1;
			lodgementQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = "IMP";
			testDec.JE_ApplicationCode = "CMR";
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			var header = testDec.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			testDec.MessageInitiator = testInitiator;
			AssertEquals("No Customs Entry Header", 0, testDec.CustomsEntryHeaders.Count);
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.All, true, true);
			AssertEquals("CallGenerateQuestionsForOriginalOrAmendment called as merge should have done and questions are generated in the course", false, testInitiator.GenerateQuestionsForOriginalOrAmendmentCalled);
			AssertEquals("Things are merged", true, testDec.IsMergeDone);
			var entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Questions are regenerated in the course of merging", true, entryHeader.Questions.HasQuestionWithID(1));
			AssertEquals("Questions are regenerated in the course of merging", true, testInitiator.CPQAFormIsShown);
			Factory.Save();
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.All, true, true);
			AssertEquals("TestDec.Merge is not required", false, testDec.MergeManager.RequiresMerge);
			AssertEquals("Merge is done", true, testDec.IsMergeDone);
			AssertEquals("CallGenerateQuestionsForOriginalOrAmendment called as merge should have done and questions are generated in the course", true, testInitiator.GenerateQuestionsForOriginalOrAmendmentCalled);
			AssertEquals("Questions are regenerated in the course of merging", true, entryHeader.Questions.HasQuestionWithID(1));
			AssertEquals("Questions are regenerated in the course of merging", true, testInitiator.CPQAFormIsShown);
		}

		public void TestQuestionsGeneratedWithPassedEntries()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			testInitiator.GenerateQuestionsForOriginalOrAmendmentCalled = false;
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, new CusEntryHeader[] { entry });
			AssertEquals("GenerateQuestionsForOriginalOrAmendment Called", true, testInitiator.GenerateQuestionsForOriginalOrAmendmentCalled);
		}

		public void TestShowCPQAFormWhenNotAllMandatoryQuestionsAreAnsweredWithPassedEntries()
		{
			TestCaseHelper.ClearTable(AutoCMRLodgementQuestion.Schema.TableName); //to make entry1' mandatory questions all answered
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 1;
			lodgementQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			var testDec = JobDeclaration.New(Factory);
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var answeredQ = entry1.Questions.AddNew();
			answeredQ.ON_CPDecNum = 1;
			answeredQ.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			answeredQ.ON_AnswerCode = "Y";
			var entry2 = testDec.CustomsEntryHeaders.AddNew();
			var unAnsweredQ = entry2.Questions.AddNew();
			unAnsweredQ.ON_CPDecNum = 1;
			unAnsweredQ.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, new CusEntryHeader[] { entry1 });
			AssertNotNull(testInitiator.FilteredEntryHeadersUsed);
			AssertEquals("There should be only one entry in FilteredEntryHeaders", 1, testInitiator.FilteredEntryHeadersUsed.Count);
			AssertEquals("There should be only one entry in FilteredEntryHeaders", entry1, testInitiator.FilteredEntryHeadersUsed[0]);
			AssertEquals("Only Entry1 is passed and Entry1 has all the questions answered. -> CPQAForm should not be shown", false, testInitiator.CPQAFormIsShown);
			testInitiator.FilteredEntryHeadersUsed = null;
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, new CusEntryHeader[] { entry1, entry2 });
			AssertNotNull(testInitiator.FilteredEntryHeadersUsed);
			AssertEquals("All entries should be there", 2, testInitiator.FilteredEntryHeadersUsed.Count);
			AssertEquals("Not all questions are answered -> CPQAForm should be shown", true, testInitiator.CPQAFormIsShown);
		}

		public void TestFilteredEntriesWithPassedFilterType()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var originalEntry = testDec.CustomsEntryHeaders.AddNew();
			originalEntry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			var amendmentEntry = testDec.CustomsEntryHeaders.AddNew();
			amendmentEntry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false);
			AssertNotNull(testInitiator.FilteredEntryHeadersUsed);
			AssertEquals("There should be only one entry", 1, testInitiator.FilteredEntryHeadersUsed.Count);
			AssertEquals("There should be only one entry", originalEntry, testInitiator.FilteredEntryHeadersUsed[0]);
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendAmendment, false, false);
			AssertNotNull(testInitiator.FilteredEntryHeadersUsed);
			AssertEquals("There should be only one entry", 1, testInitiator.FilteredEntryHeadersUsed.Count);
			AssertEquals("There should be only one entry", amendmentEntry, testInitiator.FilteredEntryHeadersUsed[0]);
		}

		public void TestShowCPQAFormAlways()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			AssertEquals("All mandatory questions are answered", true, entry.Questions.AreAllCPDecQuestionsAnswered);
			Factory.Save();
			AssertEquals("No merge is required", false, testDec.MergeManager.RequiresMerge);
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, true);
			AssertEquals("CPQA form is shown as it is forced", true, testInitiator.CPQAFormIsShown);
		}

		public void TestContinueWithSaveWhenUsersChooseCancelForQuestions()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			testInitiator.CPQAIsOKToProceedExposed = false;
			var result = testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, true);
			AssertEquals("users cancel the whole action", ContinueWithSave.No, result);
			testInitiator.CPQAIsOKToProceedExposed = true;
			result = testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, true);
			AssertEquals("users proceed", ContinueWithSave.Yes, result);
		}

		public void TestShowCPQAIfThereAreUnansweredQuestionsIfNotForced()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = 1;
			lodgementQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			var question = entry.Questions.AddNew();
			question.ON_CPDecNum = 1;
			question.ON_CPDecStartDate = new ZDateTime(2005, 1, 1);
			testInitiator.CPQAFormIsShown = false;
			testInitiator.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(testDec, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false);
			AssertEquals("CPQA form is shown as there are unaswered questions", true, testInitiator.CPQAFormIsShown);
		}

		public void TestFilteredEntryHeadersForWithdraw()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var originalEntry = testDec.CustomsEntryHeaders.AddNew();
			originalEntry.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			var withdrawEntry = testDec.CustomsEntryHeaders.AddNew();
			withdrawEntry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			testInitiator.GenerateWithdrawDecQuestionAndShowCPQAForm(testDec);
			AssertNotNull("FilteredEntryHeaders is generated", testInitiator.FilteredEntryHeadersUsed);
			AssertEquals("FilteredEntryHeaders should only have withdrawable entries", 1, testInitiator.FilteredEntryHeadersUsed.Count);
			AssertEquals("FilteredEntryHeaders should only have WithdrawEntry", withdrawEntry, testInitiator.FilteredEntryHeadersUsed[0]);
		}

		public void TestShowCPQAFormForWithdrawal()
		{
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			testInitiator.CPQAFormIsShown = false;
			testInitiator.GenerateWithdrawDecQuestionAndShowCPQAForm(testDec);
			AssertEquals("CPQA form is not shown as it is not forced", true, testInitiator.CPQAFormIsShown);
		}

		[TestDate(2024, 1, 1)]
		public void TestShowCPQAFormForConsolidatedDeclaration()
		{
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 0);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			PrepareDeclarationForAggregation(leadDeclaration);
			Factory.Save();

			var aggregatedDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregatedDeclaration.CPQAManager.GenerateQuestionsForConsolidatedEntryOriginalOrAmendment();
			AssertEquals("Questions", "3,1", string.Join(",", aggregatedDeclaration.EntryHeader.Questions.Select(q => q.ON_CPDecNum)));

			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			testInitiator.CPQAFormIsShown = false;
			AssertEquals(ContinueWithSave.No, testInitiator.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, aggregatedDeclaration, showCPQAFormAlways: false));
			AssertEquals("CPQA form is shown as there are unanswered questions", true, testInitiator.CPQAFormIsShown);

			foreach (CMRCusEntryCPDec question in aggregatedDeclaration.EntryHeader.Questions)
			{
				question.ON_AnswerCode = "Y";
			}

			testInitiator.CPQAFormIsShown = false;
			aggregatedDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			AssertEquals(ContinueWithSave.Yes, testInitiator.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, aggregatedDeclaration, showCPQAFormAlways: false));
			AssertEquals("CPQA form is not shown when all questions are answered", false, testInitiator.CPQAFormIsShown);

			AssertEquals(ContinueWithSave.No, testInitiator.ShowCPQAFormForConsolidatedDeclaration(consolidatedDeclaration, aggregatedDeclaration, showCPQAFormAlways: true));
			AssertEquals("CPQA form is shown when forced", true, testInitiator.CPQAFormIsShown);
		}

		void PrepareDeclarationForAggregation(JobDeclaration declaration)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			declaration.JE_MasterBill = "MB000";
			declaration.JE_HouseBill = "HB111";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 1, 1);
			declaration.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
			declaration.Volume = new ZArchitecture.ZVolume(200m, "");
			var line1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.EntryHeader.AllEntryLines.AddNew().InvoiceLines.AddRange(declaration.InvoiceLines);
			declaration.EntryHeader.MergedLines.Add(declaration.EntryHeader.AllEntryLines[0]);
			declaration.EntryHeader.CH_TotalPaid = 35m;
			line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "AUD");
		}

		public void TestGetSupervisorOverrides()
		{
			var testDec = JobDeclaration.New(Factory);
			var testInitiator = new SendsMessagesToCustomsGUIForTest();
			AssertType<IMDSupervisorOverrides>(testInitiator.GetSupervisorOverridesExposed(testDec, ""));
		}

		sealed class SendsMessagesToCustomsGUIForTest : SendsMessagesToCustomsGUI
		{
			public bool GenerateQuestionsForOriginalOrAmendmentCalled;
			protected override void CallGenerateQuestionsForOriginalOrAmendment(JobDeclaration declaration)
			{
				GenerateQuestionsForOriginalOrAmendmentCalled = true;
				base.CallGenerateQuestionsForOriginalOrAmendment(declaration);
			}

			public bool CPQAFormIsShown;
			protected internal override bool ShowCPQAForm(CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders)
			{
				CPQAFormIsShown = true;
				return base.ShowCPQAForm(filteredEntryHeaders);
			}

			public bool CPQAIsOKToProceedExposed;
			protected override CPQAForm GetCPQAForm(CusEntryHeaderMessageStatusFilteredCollection filteredEntryHeaders)
			{
				var result = base.GetCPQAForm(filteredEntryHeaders);
				result.IsOKToProceed = CPQAIsOKToProceedExposed;
				return result;
			}

			public CusEntryHeaderMessageStatusFilteredCollection FilteredEntryHeadersUsed;
			protected override CusEntryHeaderMessageStatusFilteredCollection GetFilteredEntryHeaders(JobDeclaration declaration, Customs.Business.EntryMessageStatusFilterType filterType)
			{
				FilteredEntryHeadersUsed = base.GetFilteredEntryHeaders(declaration, filterType);
				return FilteredEntryHeadersUsed;
			}

			protected override CusEntryHeaderMessageStatusFilteredCollection GetFilteredEntryHeaders(JobDeclaration declaration, CusEntryHeader[] entries)
			{
				FilteredEntryHeadersUsed = base.GetFilteredEntryHeaders(declaration, entries);
				return FilteredEntryHeadersUsed;
			}

			public Customs.Business.SupervisorOverrides GetSupervisorOverridesExposed(IBusiness businessEntity, string context) => base.GetSupervisorOverrides(businessEntity, context);
		}
	}
}
