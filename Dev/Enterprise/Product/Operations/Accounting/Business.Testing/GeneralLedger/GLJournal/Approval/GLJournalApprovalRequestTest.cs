using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalApprovalRequest))]
	sealed class GLJournalApprovalRequestTest : TransactionApprovalRequestTest<GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestReferenceType()
		{
			AssertEquals("Journal", TestApprovalRequest.ReferenceType);
		}

		public override void TestIsPostingActionTheSame()
		{
			var a = (GLJournalApprovalRequest)GetNewBusinessObject();
			var b = (GLJournalApprovalRequest)GetNewBusinessObject();
			a.PostingDetails.MaxAmountToApprove = 1;
			b.PostingDetails.MaxAmountToApprove = 0;
			Assert("Posting action is the same for any details", a.IsPostingActionTheSame(b));
			Assert("Posting action is the same for any details", b.IsPostingActionTheSame(a));
		}

		public override void TestArePostingDetailsTheSame()
		{
			var approvalForTest1 = (GLJournalApprovalRequest)GetNewBusinessObject();
			var approvalForTest2 = (GLJournalApprovalRequest)GetNewBusinessObject();

			Assert(approvalForTest1.ArePostingDetailsTheSame(approvalForTest2));

			approvalForTest1.PostingDetails.MaxAmountToApprove = 1;
			Assert("MaxAmountToApprove is ignored", approvalForTest1.ArePostingDetailsTheSame(approvalForTest2));
		}

		public void TestArePostingDetailsTheSame_WithoutLoadingJournal()
		{
			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var journal = testObjectCreatorInNewFactory.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			journal.AH_Desc = "1";

			Func<GLJournalApprovalRequest> createNewRequest = () =>
			{
				var request = Factory.New<GLJournalApprovalRequest>();
				request.Initialize(journal);
				return request;
			};
			var approval1 = createNewRequest();

			Func<GLJournalApprovalRequest, GLJournalApprovalRequest> createRequestSharingTheSameJournal  = (sourceRequest) =>
			{
				var request = Factory.New<GLJournalApprovalRequest>();
				request.Initialize(sourceRequest.GetLinkedJournal(new BusinessObjectFactory()).journal);
				return request;
			};
			var approval2 = createRequestSharingTheSameJournal(approval1);
			Factory.Save();

			Action<bool> assert = (isTheSame) =>
			{
				Factory.Save();
				GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly = 0;
				var approval1InNewFactory = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(approval1.PK);
				var approval2InNewFactory = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(approval2.PK);
				AssertEquals("ArePostingDetailsTheSame", isTheSame, approval1InNewFactory.ArePostingDetailsTheSame(approval2InNewFactory));
				AssertEquals("Performance check: ReadXMLFromBlobAndDeserialize_CallsCount", isTheSame ? 0 : 2, GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly);
			};
			assert(true);

			journal.AH_Desc = "2";
			approval1 = createNewRequest();
			assert(false);

			journal.AH_Desc = "1";
			var line1 = testObjectCreatorInNewFactory.CreateGLJournalLine(journal, 10, DebitCredit.CR, testObjectCreatorInNewFactory.CashOnHandAccount.PK);
			line1.FillWithValidTestData();
			approval1 = createNewRequest();
			assert(false);

			approval2 = createRequestSharingTheSameJournal(approval1);
			assert(true);

			var line2 = testObjectCreatorInNewFactory.CreateGLJournalLine(journal, 10, DebitCredit.CR, testObjectCreatorInNewFactory.CashAtBankAccount.PK);
			line2.FillWithValidTestData();
			approval2 = createNewRequest();
			assert(false);

			approval1 = createRequestSharingTheSameJournal(approval2);
			assert(true);

			var line3 = testObjectCreatorInNewFactory.CreateGLJournalLine(journal, 20, DebitCredit.DR, testObjectCreatorInNewFactory.GLJournalClearingAccount.PK);
			line3.FillWithValidTestData();
			approval1 = createNewRequest();
			assert(false);

			approval2 = createRequestSharingTheSameJournal(approval1);
			assert(true);

			journal.Lines.Sort("UnsignedOSLineAmount", ListSortDirection.Descending);
			approval1 = createNewRequest();
			AssertEquals("Precondition: lines have different order.", 20m, approval1.PostingDetails.Journal.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Precondition: lines have different order.", -10m, approval2.PostingDetails.Journal.Lines[0].AL_OSExTaxAmount);
			assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestReLinksEdocsFromPreviousRequestToCurrentRequest()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			journal.Factory.SetContext(GLJournalApprovalRequest.Context.Editing);

			var anotherFactory = new BusinessObjectFactory();
			var previousRequest = anotherFactory.New<GLJournalApprovalRequest>();
			previousRequest.Initialize(journal);
			var storageMainOnPreviousRequest = previousRequest.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(previousRequest, Core.Constants.DocManagerCodes.GLJournalApprovalRequest) as StorageMain;
			var tIFfileContents = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif");
			var document = (StorageDocs)storageMainOnPreviousRequest.AddFileOrDocument(tIFfileContents, new AddFileOrDocumentDto
			{
				FileName = "small.tif",
				DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument,
			});
			anotherFactory.ChildFactories.Add(document.MasterFactory);
			anotherFactory.Save();

			var newRequest = anotherFactory.New<GLJournalApprovalRequest>();
			AssertEquals("Storage docs count", 1, previousRequest.StorageMain.eDocs.Count);
			AssertEquals("Storage docs count", 0, newRequest.StorageMain.eDocs.Count);
			newRequest.Initialize(journal);
			anotherFactory.Save();

			AssertNullOrEmpty("No exceptions should be reported", ErrorReporter.LastMessageReported);
			var storageMainOnNewRequest = newRequest.StorageMain;
			AssertEquals("Storage docs count", 1, storageMainOnNewRequest.eDocs.Count);
			AssertEquals("Doc Name", "small.tif", storageMainOnNewRequest.eDocs[0].GetFileNameOnlyWithExtension());
			AssertEquals("Doc Type", Core.Constants.RefDocTypes.MiscellaneousDocument, storageMainOnNewRequest.eDocs[0].DocType.RT_DocType);
			Assert("Old document is deleted as new document is created and contents are copied over", document.IsDeleted);
			AssertEquals("Storage docs count", 0, previousRequest.StorageMain.eDocs.Count);
		}

		public void TestDocManagerCode()
		{
			var request = Factory.New<GLJournalApprovalRequest>();
			AssertEquals("GJR", ((IDocManagerSupport)request).DocManagerInfo.DocManagerCode);
		}

		[TestDate(2014, 11, 12)]
		public void TestXP_ApprovalRequestData()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			var request = (GLJournalApprovalRequest)Factory.New(GetExpectedBusinessObjectType());
			request.Initialize(journal);
			Factory.Save();

			var testApprovalRequest_inNewFactory = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(request.PK);
			string expectedXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><GLJournalApprovalRequestDetails><Journal><GLJournal xmlns=\"http://www.edi.com.au/EnterpriseService/\"><GLDetail><JournalType>GJL</JournalType><Description>GENERAL LEDGER JOURNAL</Description><InPeriod>201503</InPeriod><OutPeriod>0</OutPeriod><Branch>BNE</Branch><Department>BRN</Department></GLDetail><JournalLines><JournalLine><Account>2020.10.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GENERAL LEDGER JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>CR</DRCR><PK>Line1PK</PK></JournalLine><JournalLine><Account>2020.20.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GENERAL LEDGER JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>DR</DRCR><PK>Line2PK</PK></JournalLine></JournalLines></GLJournal></Journal></GLJournalApprovalRequestDetails>";
			var xml = Encoding.Unicode.GetString(testApprovalRequest_inNewFactory.XP_ApprovalRequestData).Replace(journal.Lines[0].PK.ToString(), "Line1PK").Replace(journal.Lines[1].PK.ToString(), "Line2PK");
			this.AssertXMLEqualsByDiff("XML saved in XP_ApprovalRequestData", expectedXML, xml);
			var expectedTransactionNumber = "number";
			request.PostingDetails.Journal.AH_TransactionNum = expectedTransactionNumber;
			request.PostingDetails.HasChanges = true; //to force serialization
			Factory.Save();
			expectedXML = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><GLJournalApprovalRequestDetails><Journal><GLJournal xmlns=\"http://www.edi.com.au/EnterpriseService/\"><GLDetail><JournalType>GJL</JournalType><JournalNumber>number</JournalNumber><Description>GENERAL LEDGER JOURNAL</Description><InPeriod>201503</InPeriod><OutPeriod>0</OutPeriod><Branch>BNE</Branch><Department>BRN</Department></GLDetail><JournalLines><JournalLine><Account>2020.10.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GENERAL LEDGER JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>CR</DRCR><PK>Line1PK</PK></JournalLine><JournalLine><Account>2020.20.00</Account><Branch>BNE</Branch><Department>BRN</Department><Description>GENERAL LEDGER JOURNAL</Description><LocalAmount>10</LocalAmount><DRCR>DR</DRCR><PK>Line2PK</PK></JournalLine></JournalLines></GLJournal></Journal></GLJournalApprovalRequestDetails>";
			xml = Encoding.Unicode.GetString(testApprovalRequest_inNewFactory.XP_ApprovalRequestData).Replace(request.PostingDetails.Journal.Lines[0].PK.ToString(), "Line1PK").Replace(request.PostingDetails.Journal.Lines[1].PK.ToString(), "Line2PK");
			this.AssertXMLEqualsByDiff("XML saved in XP_ApprovalRequestData using internal PostingDetails.Journal and not one passed in Initialize", expectedXML, xml);

			AssertEquals("ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, testApprovalRequest_inNewFactory.XP_ParentTableCode);
			AssertEquals("PostingDetails.MaxAmountToApprove", 0M, testApprovalRequest_inNewFactory.PostingDetails.MaxAmountToApprove);
			var restoredJournal = testApprovalRequest_inNewFactory.PostingDetails.Journal;
			AssertNotNull("Journal", restoredJournal);
			AssertEquals("TransactionNum", expectedTransactionNumber, restoredJournal.AH_TransactionNum);
			AssertEquals("TransactionType", TransactionTypes.GLStandardJournal, restoredJournal.AH_TransactionType);
			AssertEquals("PostPeriod", 201503, restoredJournal.PostPeriod);
			AssertEquals("PostDate", journal.AH_PostDate, restoredJournal.AH_PostDate);
			AssertEquals("AgePeriod", 0, restoredJournal.AgePeriod);
			AssertEquals("DueDate", journal.AH_DueDate, restoredJournal.AH_DueDate);
			AssertEquals("Line count", 2, restoredJournal.Lines.Count);

			var line = (GLJournalLine)journal.Lines[0];
			AssertEquals("UnsignedLocalLineAmount", 10m, line.UnsignedLocalLineAmount);
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.CR, line.DebitCreditSign);
			AssertEquals("GLHeader", TestObjectCreator.ExchangeGainLossControlAccount.PK, line.GLHeader.PK);

			line = (GLJournalLine)journal.Lines[1];
			AssertEquals("UnsignedLocalLineAmount", 10m, line.UnsignedLocalLineAmount);
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.DR, line.DebitCreditSign);
			AssertEquals("GLHeader", TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK, line.GLHeader.PK);

			request.PostingDetails.Journal.AH_Desc = "Changed Desc";
			line = (GLJournalLine)request.PostingDetails.Journal.Lines[0];
			line.UnsignedLocalLineAmount = 12;
			Factory.Save();
			var messagePrefix = "Update through data refresh bus is not supported: ";
			AssertEquals(messagePrefix + "Journal.AH_Desc", "GENERAL LEDGER JOURNAL", testApprovalRequest_inNewFactory.PostingDetails.Journal.AH_Desc);
			line = (GLJournalLine)testApprovalRequest_inNewFactory.PostingDetails.Journal.Lines[0];
			AssertEquals(messagePrefix + "UnsignedLocalLineAmount", 10m, line.UnsignedLocalLineAmount);

			testApprovalRequest_inNewFactory.PostingDetails.Journal.AH_Desc = "Changed Desc 777";
			request.PostingDetails.Journal.AH_Desc = "Changed Desc 2";
			Factory.Save();
			messagePrefix = "Update through data refresh bus should be done as object in current factory is changed: ";
			AssertEquals(messagePrefix + "Journal.AH_Desc", "Changed Desc 777", testApprovalRequest_inNewFactory.PostingDetails.Journal.AH_Desc);
		}

		#region TestInitialize

		public void TestInitialize()
		{
			AssertInitialize(false);
		}

		public void TestInitializeWithSavedJournal()
		{
			AssertInitialize(true);
		}

		void AssertInitialize(bool withSavedJournal)
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			if (withSavedJournal)
			{
				Factory.Save();
			}
			TestApprovalRequest.Initialize(journal);

			AssertEquals("ParentID", withSavedJournal ? journal.PK : ZGuid.Empty, TestApprovalRequest.XP_ParentID);
			AssertEquals("ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, TestApprovalRequest.XP_ParentTableCode);
			AssertNotNull("PostingDetails.Journal", TestApprovalRequest.PostingDetails.Journal);
			AssertNotEquals("PostingDetails.Journal contains copy of the journal", journal, TestApprovalRequest.PostingDetails.Journal);
			AssertEquals("PostingDetails.MaxAmountToApprove", 0M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.Journal.Lines.Count", 2, TestApprovalRequest.PostingDetails.Journal.Lines.Count);
			AssertEquals("PostingDetails.Journal.Lines[0].AL_LineAmount", -10m, TestApprovalRequest.PostingDetails.Journal.Lines[0].AL_LineAmount);
			AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 10m, TestApprovalRequest.PostingDetails.Journal.Lines[1].AL_LineAmount);

			var anotherApprovalRequest = (GLJournalApprovalRequest)GetNewBusinessObject();
			anotherApprovalRequest.Initialize(journal);
			AssertNotNull("PostingDetails.Journal", anotherApprovalRequest.PostingDetails.Journal);
			AssertNotEquals("PostingDetails.Journal contains copy of the journal", journal, TestApprovalRequest.PostingDetails.Journal);

			Assert("PostingDetails created from the same journal are equal", TestApprovalRequest.PostingDetails == anotherApprovalRequest.PostingDetails);
		}

		#endregion

		#region TestGetLinkedJournal

		public void TestGetLinkedJournalWithOriginalTransaction()
		{
			var originalJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			originalJournal.AH_TransactionBelongsToGroup = TestApprovalRequest.PK;
			Factory.Save();

			var newJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			ReleaseFactory();

			TestApprovalRequest.Initialize(newJournal);
			var requestInNewFactory = Factory.Load<GLJournalApprovalRequest>(TestApprovalRequest.PK);
			var linkedJournal = requestInNewFactory.GetLinkedJournal().journal;

			Assert(linkedJournal.IsReverseTransaction);
			AssertEquals(originalJournal.PK, linkedJournal.OriginalTransaction.PK);
		}

		public void TestGetLinkedJournal()
		{
			AssertGetLinkedJournal(withSavedJournal: false);
		}

		public void TestGetLinkedJournalWithSavedJournal()
		{
			AssertGetLinkedJournal(withSavedJournal: true);
		}

		public void TestGetLinkedJournalWithSavedJournalAndExportedLine()
		{
			AssertGetLinkedJournal(withSavedJournal: true, withExportedLine: true);
		}

		public void TestGetLinkedJournalWithSavedJournalAndExportedLineWhichWasUpdated()
		{
			AssertGetLinkedJournal(withSavedJournal: true, withExportedLine: true, exportedLineWasUpdated: true);
		}

		void AssertGetLinkedJournal(bool withSavedJournal = false, bool withExportedLine = false, bool exportedLineWasUpdated = false)
		{
			if (!withSavedJournal && withExportedLine)
			{
				Fail("Export line can be tested only with saved journal");
			}
			if (!withExportedLine && exportedLineWasUpdated)
			{
				Fail("Exported line that was updated can be exported only when we test with exported line.");
			}

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 7, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 5, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			var line3 = TestObjectCreator.CreateGLJournalLine(journal, 2, DebitCredit.DR, TestObjectCreator.GLJournalClearingAccount.PK);

			if (withSavedJournal)
			{
				Factory.Save();
			}

			line1.UnsignedOSLineAmount = 10;
			TestObjectCreator.CreateGLJournalLine(journal, 20, DebitCredit.CR, TestObjectCreator.CashOnHandAccount.PK);
			if (!withExportedLine || exportedLineWasUpdated)
			{
				line2.UnsignedOSLineAmount = 10;
				TestObjectCreator.CreateGLJournalLine(journal, 20, DebitCredit.DR, TestObjectCreator.CashAtBankAccount.PK);
			}
			else
			{
				TestObjectCreator.CreateGLJournalLine(journal, 25, DebitCredit.DR, TestObjectCreator.CashAtBankAccount.PK);
			}
			line3.Delete();

			ReleaseFactory();
			TestApprovalRequest.Initialize(journal);
			AssertNotEquals("Precondition: journal and request should be in different factories to save them separately", journal.Factory, TestApprovalRequest.Factory);

			AssertEquals("ParentID", withSavedJournal ? journal.PK : ZGuid.Empty, TestApprovalRequest.XP_ParentID);
			AssertEquals("ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, TestApprovalRequest.XP_ParentTableCode);
			AssertNotNull("PostingDetails.Journal", TestApprovalRequest.PostingDetails.Journal);
			AssertNotEquals("PostingDetails.Journal contains copy of the journal", journal, TestApprovalRequest.PostingDetails.Journal);
			AssertEquals("PostingDetails.MaxAmountToApprove", 0M, TestApprovalRequest.PostingDetails.MaxAmountToApprove);
			AssertEquals("PostingDetails.Journal.Lines.Count", 4, TestApprovalRequest.PostingDetails.Journal.Lines.Count);
			AssertEquals("PostingDetails.Journal.Lines[0].AL_LineAmount", -10m, TestApprovalRequest.PostingDetails.Journal.Lines[0].AL_LineAmount);
			AssertEquals("PostingDetails.Journal.Lines[2].AL_LineAmount", -20m, TestApprovalRequest.PostingDetails.Journal.Lines[2].AL_LineAmount);
			if (!withExportedLine || exportedLineWasUpdated)
			{
				AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 10m, TestApprovalRequest.PostingDetails.Journal.Lines[1].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[3].AL_LineAmount", 20m, TestApprovalRequest.PostingDetails.Journal.Lines[3].AL_LineAmount);
			}
			else
			{
				AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 5m, TestApprovalRequest.PostingDetails.Journal.Lines[1].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[3].AL_LineAmount", 25m, TestApprovalRequest.PostingDetails.Journal.Lines[3].AL_LineAmount);
			}

			Factory.Save();

			if (withExportedLine)
			{
				var loadedJournal = new BusinessObjectFactory().Load<GLJournal>(journal.PK);
				var loadedLine2 = loadedJournal.Lines.FindByPK(line2.PK);
				Assert("Precondition: journal line read only", !loadedLine2.ReadOnly);
				var exportFactory = new BusinessObjectFactory();
				var exportSequence = exportFactory.NewWithValidTestData<GenExportBatchSequence>();
				exportSequence.XB_ParentID = line2.PK;
				exportFactory.Save();
				Assert("Precondition: line.HasAssignedExportBatchNumber", line2.HasAssignedExportBatchNumber);
				loadedJournal = new BusinessObjectFactory().Load<GLJournal>(journal.PK);
				loadedLine2 = loadedJournal.Lines.FindByPK(line2.PK);
				Assert("Precondition: journal line read only", loadedLine2.ReadOnly);
			}

			ReleaseFactory();
			var requestInNewFactory = Factory.Load<GLJournalApprovalRequest>(TestApprovalRequest.PK);
			var linkedJournal = requestInNewFactory.GetLinkedJournal().journal;

			if (withSavedJournal)
			{
				AssertEquals("linkedJournal.PK", journal.PK, linkedJournal.PK);
			}
			else
			{
				AssertNotEquals("linkedJournal.PK", journal.PK, linkedJournal.PK);
			}
			AssertEquals("linkedJournal is in the request Factory", requestInNewFactory.Factory, linkedJournal.Factory);
			Assert("linkedJournal has no suspended validation", !linkedJournal.IsValidationSuspended);
			Assert("linkedJournal lines have no suspended validation", linkedJournal.Lines.All(x => !x.IsValidationSuspended));
			Assert("linkedJournal is not read only", !linkedJournal.ReadOnly);
			Assert("linkedJournal lines are not read only", linkedJournal.Lines.Where(x => x.PK != line2.PK).All(x => !x.ReadOnly));
			Assert("linkedJournal line is read only when exported", linkedJournal.Lines.Where(x => x.PK == line2.PK).All(x => x.ReadOnly == withExportedLine));

			if (exportedLineWasUpdated)
			{
				AssertEquals("PostingDetails.Journal.Lines.Count", 5, linkedJournal.Lines.Count);
				AssertEquals("PostingDetails.Journal.Lines[0].AL_LineAmount", -10m, linkedJournal.Lines[0].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 5m, linkedJournal.Lines[1].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[2].AL_LineAmount", 10m, linkedJournal.Lines[2].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[3].AL_LineAmount", -20m, linkedJournal.Lines[3].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[4].AL_LineAmount", 20m, linkedJournal.Lines[4].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[0].GLHeader.AG_AccountNum", TestObjectCreator.ExchangeGainLossControlAccount.AG_AccountNum, linkedJournal.Lines[0].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[1].GLHeader.AG_AccountNum", TestObjectCreator.ExchangeGainLossAdjustmentAccount.AG_AccountNum, linkedJournal.Lines[1].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[2].GLHeader.AG_AccountNum", TestObjectCreator.ExchangeGainLossAdjustmentAccount.AG_AccountNum, linkedJournal.Lines[2].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[3].GLHeader.AG_AccountNum", TestObjectCreator.CashOnHandAccount.AG_AccountNum, linkedJournal.Lines[3].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[4].GLHeader.AG_AccountNum", TestObjectCreator.CashAtBankAccount.AG_AccountNum, linkedJournal.Lines[4].GLHeader.AG_AccountNum);

				AssertEquals("Exported line should not be deleted or updated. PK:", line2.PK, linkedJournal.Lines[1].PK);
				AssertEquals("Exported line should not be deleted or updated. Line Amount:", 5m, linkedJournal.Lines[1].AL_LineAmount);
				AssertEquals("Exported line should not be deleted or updated. Account Number:", line2.GLHeader.AG_AccountNum, linkedJournal.Lines[1].GLHeader.AG_AccountNum);
			}
			else
			{
				AssertEquals("PostingDetails.Journal.Lines.Count", 4, linkedJournal.Lines.Count);

				if (!withExportedLine)
				{
					var lineNumber = 0;
					foreach (var line in linkedJournal.Lines)
					{
						AssertEquals($"Performance check: Journal.Lines[{lineNumber}].RoundAmountToCurrencyDecimals_CallsCount_ForTestOnly", 3, linkedJournal.Lines[lineNumber].RoundAmountToCurrencyDecimals_CallsCount_ForTestOnly);
						lineNumber++;
					}
				}

				AssertEquals("PostingDetails.Journal.Lines[0].AL_LineAmount", -10m, linkedJournal.Lines[0].AL_LineAmount);
				AssertEquals("PostingDetails.Journal.Lines[2].AL_LineAmount", -20m, linkedJournal.Lines[2].AL_LineAmount);
				if (withExportedLine)
				{
					AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 5m, linkedJournal.Lines[1].AL_LineAmount);
					AssertEquals("PostingDetails.Journal.Lines[3].AL_LineAmount", 25m, linkedJournal.Lines[3].AL_LineAmount);

					AssertEquals("Exported line should not be deleted or updated. PK:", line2.PK, linkedJournal.Lines[1].PK);
					AssertEquals("Exported line should not be deleted or updated. Line Amount:", 5m, linkedJournal.Lines[1].AL_LineAmount);
					AssertEquals("Exported line should not be deleted or updated. Account Number:", line2.GLHeader.AG_AccountNum, linkedJournal.Lines[1].GLHeader.AG_AccountNum);
				}
				else
				{
					AssertEquals("PostingDetails.Journal.Lines[1].AL_LineAmount", 10m, linkedJournal.Lines[1].AL_LineAmount);
					AssertEquals("PostingDetails.Journal.Lines[3].AL_LineAmount", 20m, linkedJournal.Lines[3].AL_LineAmount);
				}
				AssertEquals("PostingDetails.Journal.Lines[0].GLHeader.AG_AccountNum", TestObjectCreator.ExchangeGainLossControlAccount.AG_AccountNum, linkedJournal.Lines[0].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[1].GLHeader.AG_AccountNum", TestObjectCreator.ExchangeGainLossAdjustmentAccount.AG_AccountNum, linkedJournal.Lines[1].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[2].GLHeader.AG_AccountNum", TestObjectCreator.CashOnHandAccount.AG_AccountNum, linkedJournal.Lines[2].GLHeader.AG_AccountNum);
				AssertEquals("PostingDetails.Journal.Lines[3].GLHeader.AG_AccountNum", TestObjectCreator.CashAtBankAccount.AG_AccountNum, linkedJournal.Lines[3].GLHeader.AG_AccountNum);
			}
		}

		#endregion

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var approvalForTest = (GLJournalApprovalRequest)GetNewBusinessObject();
			AssertEquals("ApprovalType", Enterprise.Core.Constants.GenApprovalRequestApprovalType.GLJournal, approvalForTest.XP_ApprovalType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = (GLJournalApprovalRequest)base.GetNewBusinessObjectForDeleteTest(factory);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			bizo.Initialize(journal);

			return bizo;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = (GLJournalApprovalRequest)base.GetNewBusinessObject();
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			bizo.Initialize(journal);

			return bizo;
		}

		protected override string GetExpectedEmailSubjectForTestSendEmail(GLJournalApprovalRequest request) => "General Ledger Journal approval request number '00000001'";

		public override void TestSentOnSaving()
		{
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);
			AccountingConfigurationRegistry.Instance.GLJournalsApprovalNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			base.TestSentOnSaving();
		}

		protected override bool ShouldSendEmailOnSaving
		{
			get { return true; }
		}

		protected override bool ShouldSetRequestIDFromNumberFountain
		{
			get { return true; }
		}
	}
}
