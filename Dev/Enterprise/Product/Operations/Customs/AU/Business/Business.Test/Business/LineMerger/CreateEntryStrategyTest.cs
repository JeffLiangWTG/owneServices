using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CreateEntryStrategyTest : TestCaseWithFactory
	{
		public void TestWhenAnInvoiceLineIsDeletedAndAddedAgain()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			declaration.DoMerge();
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "1";
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("PreCondition:HasBeenLodged", true, entry.HasBeenLodgedAtCustoms);

			ZGuid existingEntryLine = invoiceLine1.JI_CL;
			declaration.InvoiceLines.RemoveAndDeleteAll();
			BaseJobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			declaration.DoMerge();

			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("existing entry line recycled", existingEntryLine, invoiceLine2.JI_CL);
		}

		public void TestRemergeAfterOneEntryLineNatureChanges()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "1";

			declaration.DoMerge();
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one entry line", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			declaration.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;
			line.CusEntryLine.Header.EntryNumber = "ABC";
			line.CusEntryLine.Header.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("HasBeenLodged", true, line.CusEntryLine.Header.HasBeenLodgedAtCustoms);
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature10, line.CusEntryLine.ZA_DetailsNotToBeAmended);
			Factory.Save();

			ZGuid existingEntryHeader = declaration.CustomsEntryHeaders[0].PK;
			line.JI_IsPackToBondForLine = true;
			AssertEquals("PreCondition:HasNonAmendableChanges", true, line.CusEntryLine.HasNonAmendableChanges);

			declaration.DoMerge();
			AssertEquals("existing entry header is kept", existingEntryHeader, line.CusEntryLine.Header.PK);
			AssertHasMessageError(line.CusEntryLine.Header.CH_BGMReferenceInfo, string.Format(CusEntryHeaderValidation.HasNonAmendableNatureChanges, "N10", "N20"));
			AssertEquals("Two entry lines are there", 2, line.CusEntryLine.Header.AllEntryLines.Count);
		}

		public void TestRemergeAfterEntryLineNatureChanges()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "1";

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2";

			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "3";
			line3.JI_IsPackToBondForLine = true;

			declaration.DoMerge();
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("one entry line", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			ZGuid existingEntryLine = line.JI_CL;

			line.JI_IsPackToBondForLine = true;
			declaration.DoMerge();
			AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("three entry lines", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("It is OK to reuse an entry line as it is not lodged at Customs yet", existingEntryLine, line.JI_CL);

			declaration.CustomsEntryHeaders[0].CH_HighestLineNumber = 1;
			line.CusEntryLine.Header.EntryNumber = "ABC";
			line.CusEntryLine.Header.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("HasBeenLodged", true, line.CusEntryLine.Header.HasBeenLodgedAtCustoms);
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature20, line.CusEntryLine.ZA_DetailsNotToBeAmended);
			Factory.Save();

			line.JI_IsPackToBondForLine = false;
			AssertEquals("PreCondition:HasNonAmendableChanges", true, line.CusEntryLine.HasNonAmendableChanges);

			declaration.DoMerge();
			AssertNotEquals("new entry line should have been created", existingEntryLine, line.JI_CL);

			CusEntryLine deactivatedEntryLine = Factory.Load<CusEntryLine>(existingEntryLine);
			AssertEquals("Should have been deactivated", false, deactivatedEntryLine.IsActive);

			ZGuid secondEntryLine = line.JI_CL;
			declaration.DoMerge();
			AssertEquals("PreCondition:HasNonAmendableChanges", false, line.CusEntryLine.HasNonAmendableChanges);
			AssertEquals("secondEntryLine is not lodged at Customs yet, therefore it should be reused", secondEntryLine, line.JI_CL);
			AssertEquals("deactivedEntryLine should still be deactivated", true, line.CusEntryLine.Header.PendingDeletionEntryLines.Contains(deactivatedEntryLine));
		}

		public void TestAssignLineNumbersForCMR()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1";
			declaration.DoMerge();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[0];
			AssertEquals("Line Number", (ZShort)1, entryLine1.CL_LineNumber);

			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2";
			declaration.DoMerge();

			CusEntryLine entryLine2 = entryHeader.MergedLines[1];
			AssertEquals("Line Number", (ZShort)1, entryLine1.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)2, entryLine2.CL_LineNumber);

			entryHeader.CH_HighestLineNumber = 2;

			JobComInvoiceLine line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "3";
			declaration.DoMerge();
			CusEntryLine entryLine3 = entryHeader.MergedLines[2];
			AssertEquals("Line Number", (ZShort)1, entryLine1.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)2, entryLine2.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)3, entryLine3.CL_LineNumber);

			JobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			line4.JI_Tariff = "4";
			declaration.DoMerge();
			CusEntryLine entryLine4 = entryHeader.MergedLines[3];
			AssertEquals("Line Number", (ZShort)1, entryLine1.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)2, entryLine2.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)3, entryLine3.CL_LineNumber);
			AssertEquals("Line Number", (ZShort)4, entryLine4.CL_LineNumber);
		}

		public void TestCreateEntryStrategy()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			LineMerger merger = new LineMerger(testDec);

			AssertEquals("IsImporCMR", true, testDec.IsImportCMR);

			EntryCreationStrategy[] result = merger.GetEntryCreationStrategiesInternal();
			AssertEquals("1 Strategy", 1, result.Length);
			AssertEquals("CMR Strategy", typeof(CMREntryCreationStrategy), result[0].GetType());

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines; // SAC
			result = merger.GetEntryCreationStrategiesInternal();
			AssertEquals("1 Strategy", 1, result.Length);
			AssertEquals("CMR SAC Strategy", typeof(CMRSACEntryCreationStrategy), result[0].GetType());

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines; // SWL
			result = merger.GetEntryCreationStrategiesInternal();
			AssertEquals("1 Strategy", 1, result.Length);
			AssertEquals("CMR SAC Strategy", typeof(CMRSACEntryCreationStrategy), result[0].GetType());
		}

		public void TestGenerateQuestionsAfterMerged()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			TestMerger merger = new TestMerger(testDec);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("No questions at this point", 0, entryHeader.Questions.Count);

			merger.PerformCountrySpecificOperationAfterMergeBeforeCalculateDutyInternal();
			AssertEquals("Questions genenrated", true, merger.GenerateQeustionsInitiated);

			entryHeader.Questions.RemoveAndDeleteAll();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			merger = new TestMerger(testDec);

			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			merger.PerformCountrySpecificOperationAfterMergeBeforeCalculateDutyInternal();
			AssertEquals("No questions as this is Legacy", false, merger.GenerateQeustionsInitiated);
		}

		class TestMerger : LineMerger
		{
			public TestMerger(JobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public bool GenerateQeustionsInitiated;
			protected override void GenerateQuestions()
			{
				base.GenerateQuestions();
				GenerateQeustionsInitiated = true;
			}
		}
	}
}
