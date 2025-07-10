using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(DocumentGeneratingActionCollection))]
	sealed class DocumentGeneratingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentGeneratingActionCollection>
	{
		public void TestDefaultTobeDeliveredWhenThereIsOnlyOneElement()
		{
			Declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = (JobDeclarationDocumentSupporter)Declaration.DocumentSupporter;
			AssertEquals(1, documentSupporter.DocumentGenerationActions.Count);
			Assert(documentSupporter.DocumentGenerationActions[0].ToBeDelivered);

			Declaration.CustomsEntryHeaders.AddNew();
			documentSupporter.DocumentGenerationActions.InitialiseFor(Factory.New<StmMenuItem>());
			AssertEquals(2, documentSupporter.DocumentGenerationActions.Count);
			Assert(!documentSupporter.DocumentGenerationActions[0].ToBeDelivered);
			Assert(!documentSupporter.DocumentGenerationActions[1].ToBeDelivered);
		}
		protected override DocumentGeneratingActionCollection GetCollectionToTest() => new DocumentGeneratingActionCollection(Declaration);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentGeneratingAction(Factory.New<CusEntryHeader>(), "ABC111", JobDeclarationDocumentSupporter.DataContexts.EXPEntryHeaderBO);

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		void AssertInitiateForDataContext(ZString jobMessageType, ZString messageType, ZString menuName, ZBool statusRelevant)
		{
			Declaration.JE_MessageType = jobMessageType;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = jobMessageType;
			entryNumber1.CE_EntryNum = "AAA111";
			if (!messageType.IsEmpty)
			{
				entry1.Messages.AddNew().EM_MessageType = messageType;
			}
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = jobMessageType;
			entryNumber2.CE_EntryNum = "BBB111";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = menuName;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			if (!messageType.IsEmpty)
			{
				collection.InitialiseFor(menu);
				AssertEquals("Actions for entries with Message", 1, collection.Count);
				AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
				AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);
			}
			if (!messageType.IsEmpty)
			{
				entry2.Messages.AddNew().EM_MessageType = messageType;
			}
			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 2, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("Actions for entries with Message", "BBB111", collection[1].EntryNumber);

			AssertEquals(statusRelevant, collection.IsStatusRelevant);
		}

		public void Test830()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Export, ZString.Empty, JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate, ZBool.True);
		}

		public void Test830English()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Export, ZString.Empty, JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English, ZBool.True);
		}

		public void Test830_ExportVehicleNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = JobMessageTypeList.Codes.Export;
			entryNumber1.CE_EntryNum = "AAA111";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo;
			var collection = new DocumentGeneratingActionCollection(Declaration);
			collection.InitialiseFor(menu);
			AssertEquals(0, collection.Count);

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = JobMessageTypeList.Codes.Export;
			entryNumber2.CE_EntryNum = "AAA111";
			var entryLine2 = entry2.MergedLines.AddNew();
			var invoiceLine2 = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.VehicleNumbers.AddNew();

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			var entry3 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber3 = entry3.EntryNumbers.AddNew();
			entryNumber3.CE_EntryType = JobMessageTypeList.Codes.Export;
			entryNumber3.CE_EntryNum = "BBB111";
			var entryLine3 = entry3.MergedLines.AddNew();
			var invoiceLine3 = Declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.VehicleNumbers.AddNew();
			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 2, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("Actions for entries with Message", "BBB111", collection[1].EntryNumber);

			AssertEquals(true, collection.IsStatusRelevant);
		}

		public void Test5AS_EffectiveCollection()
		{
			Declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = Common.KR.KRJobMessageTypeList.Codes.Export;
			entryNumber1.CE_EntryNum = "1234520100523X";
			Factory.Save();

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 0, collection.Count);

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry1)))
			{
				var snapshot = entry1.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry1.Factory.Save();
			}

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "1234520100523X", collection[0].EntryNumber);
		}

		public void Test5BD()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BD, JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease, ZBool.True);
		}

		public void Test5BA()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";

			var entryInstruction1 = Declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_AgreedDutyRatePreferenceCode = "A";
			entry1.CH_CEI_Instruction = entryInstruction1.PK;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries printable", 1, collection.Count);
			AssertEquals("Actions for entries printable", "AAA111", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber2.CE_EntryNum = "BBB111";

			var entryInstruction2 = Declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_AgreedDutyRatePreferenceCode = ZString.Empty;
			entry2.CH_CEI_Instruction = entryInstruction2.PK;

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries printable", 1, collection.Count);
			AssertEquals("Actions for entries printable", "AAA111", collection[0].EntryNumber);
		}

		public void Test5TM()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5TM, JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration, ZBool.True);
		}

		public void Test5FN()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5FN, JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate, ZBool.True);
		}

		public void Test5BF()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BF, JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration, ZBool.True);
		}

		public void Test5GV()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5GV, JobDeclarationDocumentSupporter.MenuNames.NoticeOfAmendmentOrSupplementaryActions, ZBool.False);
		}

		public void TestD72()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._D72, JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate, ZBool.True);
		}

		public void Test5WN()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5WN, JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment, ZBool.False);
		}

		public void Test5GU()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5GU, JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin, ZBool.False);
		}

		public void Test5UO()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5UO, JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund, ZBool.False);
		}

		public void Test5TW()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5TW, JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults, ZBool.False);
		}

		public void Test5TV()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5TV, JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment, ZBool.False);
		}

		public void Test5FV()
		{
			AssertInitiateForDataContext(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5FV, JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase, ZBool.False);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test5DR_EffectiveCollection()
		{
			var declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(Declaration, ElectronicDocumentTypeList.Codes._5DP, "3271420001710", ElectronicDocumentTypeList.Codes._5DR, "GOVCBR5DR_Test.xml");
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration;
			var collection = new DocumentGeneratingActionCollection(declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "3271420001710", collection[0].EntryNumber);
			AssertEquals(ZBool.True, collection.IsStatusRelevant);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test5DS_EffectiveCollection()
		{
			var declaration = new TestDataSetupHelper(Factory).SetLocalExportEntryData(Declaration, ElectronicDocumentTypeList.Codes._5DQ, "3271420001710", ElectronicDocumentTypeList.Codes._5DS, "GOVCBR5DS_Test.xml");
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration;
			var collection = new DocumentGeneratingActionCollection(declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "3271420001710", collection[0].EntryNumber);

			AssertEquals(ZBool.True, collection.IsStatusRelevant);
		}

		public void Test5FK()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber1.CE_EntryNum);
			var statement1 = entry1.CustomsDisbursementBills[0];
			statement1.B2_StatementNumber = "0127020112001320507";
			statement1.B2_StatementAmount = 1000000;
			statement1.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries printable", 1, collection.Count);
			AssertEquals("Actions for entries printable", "AAA111", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber2.CE_EntryNum = "BBB111";

			CreateNewStatementHeader(declaration.CompanyPK, entryNumber2.CE_EntryNum);
			var statement2 = entry2.CustomsDisbursementBills[0];
			statement2.B2_StatementNumber = "0127020112001320508";
			statement2.B2_StatementAmount = 1000000;
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries printable", 2, collection.Count);
			AssertEquals("Actions for entries printable", "AAA111", collection[0].EntryNumber);
		}

		void CreateNewStatementHeader(ZGuid companyPK, ZString entryNumber)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = companyPK;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_SequenceNumber = 1;
			Factory.Save();
		}

		public void Test5UL_SingleSnapshot()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";

			var entryNumber5UL = entry1.EntryNumbers.AddNew();
			entryNumber5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry1, "11111", entryNumber5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry1, refundDetails)))
			{
				var snapshot = entry1.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry1.Factory.Save();
			}

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber2.CE_EntryNum = "BBB111";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RefundRequest;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			AssertEquals(true, collection.IsStatusRelevant);
		}

		public void Test5UL_MultiSnapshots()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";

			var entryNumber5UL = entry1.EntryNumbers.AddNew();
			entryNumber5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry1, "11111", entryNumber1);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry1, refundDetails)))
			{
				var snapshot = entry1.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry1.Factory.Save();
			}

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNumber2.CE_EntryNum = "BBB111";

			entryNumber5UL = entry2.EntryNumbers.AddNew();
			entryNumber5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry2, "22222", entryNumber5UL);
			refundDetails = new GOVCBR5ULDetails(sendingObject);
			using (var stream = KRXmlObjectSerializer.Serialize(new Import5ULCreator().Create(entry2, refundDetails)))
			{
				var snapshot = entry2.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5UL);
				snapshot.CES_VersionNumber = (ZShort)2;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new Enterprise.Messaging.Business.TextReaderSource(stream));
				entry2.Factory.Save();
			}

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RefundRequest;
			var collection = new DocumentGeneratingActionCollection(Declaration);

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 2, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("Actions for entries with Message", "BBB111", collection[1].EntryNumber);

			AssertEquals(true, collection.IsStatusRelevant);
		}

		public void Test929_ReImportofExportedGoods()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber1.CE_EntryNum = "AAA111";

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods;
			var collection = new DocumentGeneratingActionCollection(Declaration);
			collection.InitialiseFor(menu);
			AssertEquals(0, collection.Count);

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber2.CE_EntryNum = "AAA111";
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.PreviousExpDecLineCollection.AddNew();

			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 1, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			var entry3 = Declaration.CustomsEntryHeaders.AddNew();
			var entryNumber3 = entry3.EntryNumbers.AddNew();
			entryNumber3.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumber3.CE_EntryNum = "BBB111";
			var entryLine3 = entry3.MergedLines.AddNew();
			entryLine3.PreviousExpDecLineCollection.AddNew();
			collection.InitialiseFor(menu);
			AssertEquals("Actions for entries with Message", 2, collection.Count);
			AssertEquals("Actions for entries with Message", "AAA111", collection[0].EntryNumber);
			AssertEquals("Actions for entries with Message", "BBB111", collection[1].EntryNumber);

			AssertEquals(true, collection.IsStatusRelevant);
		}

		public void Test5DP5DQ()
		{
			AssertInitiateForDataContext(KRJobMessageTypeList.Codes.LocalExport, ZString.Empty, JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration, ZBool.True);
		}

		public void TestDKJ()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var collection = new DocumentGeneratingActionCollection(Declaration);
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration;

			collection.InitialiseFor(menu);
			AssertEquals("No entry exists.", 0, collection.Count);

			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "AAA111";
			collection.InitialiseFor(menu);
			AssertEquals(0, collection.Count);

			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "BBB222";
			CreateMessage(entry2, ElectronicDocumentTypeList.Codes._5AF);
			collection.InitialiseFor(menu);
			AssertEquals(0, collection.Count);

			var entry3 = Declaration.CustomsEntryHeaders.AddNew();
			entry3.EntryNumber = "CCC333";
			CreateMessage(entry3, ElectronicDocumentTypeList.Codes._5AF);
			CreateMessage(entry3, ElectronicDocumentTypeList.Codes._DKJ);
			collection.InitialiseFor(menu);
			AssertEquals(1, collection.Count);
			AssertEquals("Actions for entries with Message", "CCC333", collection[0].EntryNumber);
			AssertEquals("If there is only one element, then ToBeDelivered is ticked off by system", true, collection[0].ToBeDelivered);

			var entry4 = Declaration.CustomsEntryHeaders.AddNew();
			entry4.EntryNumber = "DDD444";
			CreateMessage(entry4, ElectronicDocumentTypeList.Codes._5AF);
			CreateMessage(entry4, ElectronicDocumentTypeList.Codes._DKJ);
			collection.InitialiseFor(menu);
			AssertEquals(2, collection.Count);
			AssertEquals("Actions for entries with Message", "CCC333", collection[0].EntryNumber);
			AssertEquals("Actions for entries with Message", "DDD444", collection[1].EntryNumber);

			AssertEquals(true, collection.IsStatusRelevant);

			void CreateMessage(CusEntryHeader entry, string messageType)
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
			}
		}
	}
}
