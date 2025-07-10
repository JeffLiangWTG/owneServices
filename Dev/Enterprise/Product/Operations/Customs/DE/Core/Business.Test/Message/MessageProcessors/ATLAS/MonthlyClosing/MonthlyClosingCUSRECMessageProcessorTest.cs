using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;
using static Enterprise.Customs.DE.Messaging.MonthlyClosingMessageSubTypeList.Codes;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(MonthlyClosingCUSRECMessageProcessor))]
	sealed class MonthlyClosingCUSRECMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingCUSRECMessageProcessor, AtlasInboundEDIMessage<ICUSREC>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals("LinkedObject from ReferencedMessageIdentifier", declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectNoReferencedMessageIdentifier()
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("LinkedObject obtained from MRN", declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectMRN()
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "24DE12345678901234";
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("LinkedObject obtained from MRN", declaration, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("ABC000000");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestNoLinkedObjectNoReferenceNoMrn()
		{
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("ABC000000");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.MRN).Returns((string)null);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSREC)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestStoreRegistrationNumber()
		{
			dataProviderMock.Setup(x => x.MRN).Returns((string)null);
			ProcessMessage(message);
			AssertEquals("ATB150000620520195875", message.GetLogbookRegistrationNumber());
		}

		public void TestStoreMRN()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			ProcessMessage(message);
			AssertEquals("24DE12345678901234", message.GetLogbookRegistrationNumber());
		}

		public void TestStoreRegistrationNumberAndMRN()
		{
			ProcessMessage(message);
			AssertEquals("ATB150000620520195875, 24DE12345678901234", message.GetLogbookRegistrationNumber());
		}

		public void TestCreateCusEntryNumber()
		{
			CombineAssertions(() =>
			{
				AssertDeclarationMRN("Initially", false);
				ProcessMessage(message);
				AssertDeclarationMRN("After processing message", true, "ATB150000620520195875", new ZDate(2020, 9, 17));
			});
		}

		public void TestUpdateCusEntryNumber()
		{
			var cusEntryNum = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNum.CE_EntryNum = "XYZ123";
			cusEntryNum.CE_IssueDate = new ZDate(2020, 01, 01);
			CombineAssertions(() =>
			{
				AssertDeclarationMRN("Initially", true, "XYZ123", new ZDate(2020, 01, 01));
				ProcessMessage(message);
				AssertDeclarationMRN("After processing message", true, "ATB150000620520195875", new ZDate(2020, 9, 17));
			});
		}

		public void TestCusEntryNumberFromMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			var cusEntryNum = CusEntryNumber.New(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNum.CE_EntryNum = "XYZ123";
			cusEntryNum.CE_IssueDate = new ZDate(2020, 01, 01);
			CombineAssertions(() =>
			{
				AssertDeclarationMRN("Initially", true, "XYZ123", new ZDate(2020, 01, 01));
				ProcessMessage(message);
				AssertDeclarationMRN("After processing message", true, "24DE12345678901234", new ZDate(2020, 9, 17));
			});
		}

		public void TestUpdateCusReconDeclarationStatuses_ReferenceNumberNotATA()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CRD_CustomsStatus", EntryStatus.RC2, declaration.CRD_CustomsStatus);
				AssertEquals("CRD_MessageStatus", EDIMessage.Status.Received, declaration.CRD_MessageStatus);
			});
		}

		public void TestUpdateCusReconDeclarationStatuses_ReferenceNumberATA()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATA150000620520195875");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CRD_CustomsStatus", EntryStatus.REJ, declaration.CRD_CustomsStatus);
				AssertEquals("CRD_MessageStatus", EDIMessage.Status.Rejected, declaration.CRD_MessageStatus);
			});
		}

		public void TestUpdateCusReconDeclarationStatuses_ReferenceNumberEmpty()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(string.Empty);
			dataProviderMock.Setup(x => x.MRN).Returns(string.Empty);
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CRD_CustomsStatus", EntryStatus.REJ, declaration.CRD_CustomsStatus);
				AssertEquals("CRD_MessageStatus", EDIMessage.Status.Rejected, declaration.CRD_MessageStatus);
			});
		}

		public void TestUpdateCusReconDeclarationStatuses_HeaderError()
		{
			dataProviderMock.Setup(x => x.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CRD_CustomsStatus", EntryStatus.REJ, declaration.CRD_CustomsStatus);
				AssertEquals("CRD_MessageStatus", EDIMessage.Status.Rejected, declaration.CRD_MessageStatus);
			});
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestUpdateCusReconEntryLines_NotificationSeverityINF()
		{
			AssertCusReconEntryLineStatusAndLodgedSnapshotUpdated(NotificationTypeList.Codes.Information, EntryStatus.RC2);
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestUpdateCusReconEntryLines_NotificationSeverityWRG()
		{
			AssertCusReconEntryLineStatusAndLodgedSnapshotUpdated(NotificationTypeList.Codes.Warning, EntryStatus.ERR);
		}

		public void TestUpdateCusReconEntryLines_NotificationSeverityERR_CRL_CustomsStatusEmpty()
		{
			var (line1, line2, _, _) = SetupCusReconEntryLines();
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("1", NotificationTypeList.Codes.Error, "100"),
				CreateGoodsItem("2", NotificationTypeList.Codes.Error, "100")
			});
			line1.CRL_CustomsStatus = EntryStatus.RC1;
			CreateCurrentAndLodgedSnapshot(line1);
			CreateCurrentAndLodgedSnapshot(line2);

			CombineAssertions(() =>
			{
				AssertEquals("Line1: Initially CRL_CustomsStatus not empty", EntryStatus.RC1, line1.CRL_CustomsStatus);
				AssertEquals("Line2: Initially CRL_CustomsStatus is empty", ZString.Empty, line2.CRL_CustomsStatus);
				ProcessMessage(message);
				AssertEquals("Line1: CRL_CustomsStatus not updated to REJ", EntryStatus.ERR, line1.CRL_CustomsStatus);
				AssertNull("Line1: currentSnapshot deleted", line1.CurrentSnapshot);
				AssertNotNull("Line1: lodgedSnapshot not deleted", line1.LodgedSnapshot);
				AssertEquals("Line2: CRL_CustomsStatus updated to REJ", EntryStatus.REJ, line2.CRL_CustomsStatus);
				AssertNull("Line2: currentSnapshot deleted", line2.CurrentSnapshot);
				AssertNotNull("Line2: lodgedSnapshot not deleted", line2.LodgedSnapshot);
			});
		}

		public void TestUpdateCusReconEntryLines_NotificationSeverityERR_CRL_CustomsStatusREJ()
		{
			var (line1, line2, _, _) = SetupCusReconEntryLines();
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("1", NotificationTypeList.Codes.Error, "100"),
				CreateGoodsItem("2", NotificationTypeList.Codes.Error, MonthlyClosingCUSRECMessageProcessor.ATLASNotificationCodes_819)
			});
			line1.CRL_CustomsStatus = EntryStatus.REJ;
			line2.CRL_CustomsStatus = EntryStatus.REJ;
			CreateCurrentAndLodgedSnapshot(line1);
			CreateCurrentAndLodgedSnapshot(line2);

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Line1: CRL_CustomsStatus not updated", EntryStatus.REJ, line1.CRL_CustomsStatus);
				AssertNull("Line1: currentSnapshot deleted", line1.CurrentSnapshot);
				AssertNotNull("Line1: lodgedSnapshot not deleted", line1.LodgedSnapshot);
				AssertEquals("Line2: CRL_CustomsStatus not updated", EntryStatus.REJ, line2.CRL_CustomsStatus);
				AssertNotNull("Line2: currentSnapshot not deleted", line2.CurrentSnapshot);
				AssertNotNull("Line2: lodgedSnapshot not deleted", line2.LodgedSnapshot);
			});
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestUpdateCusReconEntryLines_NotificationSeverityERR_CRL_CustomsStatusNotEmptyAndNotREJ()
		{
			AssertCusReconEntryLineStatusAndLodgedSnapshotUpdated(NotificationTypeList.Codes.Error, EntryStatus.ERR, EntryStatus.RC1);
		}

		public void TestUpdateCusReconEntryLines_NotificationSeverityERRAndNotificationCode819()
		{
			var (line1, line2, _, _) = SetupCusReconEntryLines();
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("1", NotificationTypeList.Codes.Error, "100"),
				CreateGoodsItem("2", NotificationTypeList.Codes.Error, MonthlyClosingCUSRECMessageProcessor.ATLASNotificationCodes_819)
			});
			CreateCurrentAndLodgedSnapshot(line1);
			CreateCurrentAndLodgedSnapshot(line2);

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Line1: CRL_CustomsStatus updated to REJ", EntryStatus.REJ, line1.CRL_CustomsStatus);
				AssertNull("Line1: currentSnapshot deleted", line1.CurrentSnapshot);
				AssertEquals("Line2: CRL_CustomsStatus updated to REJ", EntryStatus.REJ, line2.CRL_CustomsStatus);
				AssertNotNull("Line2: currentSnapshot not deleted", line2.CurrentSnapshot);
			});
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestUpdateCusReconEntryLines_UpdateInventory()
		{
			var helper = new WhsDataTestHelper(Factory);
			var procedure = WhsDataTestHelper.CreateOutwardCusProcedure(Factory);
			var procedureCode = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK);
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var attr = helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);

			// order with same entry key should not be updated
			var order = helper.GetNewWhsOrder(helper.Importer.PK, helper.WhsWarehouse, "Reference");
			var orderLine = helper.GetNewWhsOrderLine(order, helper.Part, 1m);
			orderLine.WE_BondedEntryKey = "OTH1234-1";

			var orderAttr = helper.GetNewWhsBondedWarehouseAttribute(orderLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", ZString.Empty, "ENT1234", 1);

			// order with InwardEntry Key matching receive should  be updated
			var orderForReceive = helper.GetNewWhsOrder(helper.Importer.PK, helper.WhsWarehouse, "Reference1");
			var orderLineForReceive = helper.GetNewWhsOrderLine(orderForReceive, helper.Part, 1m);
			orderLineForReceive.WE_BondedEntryKey = "ENT1234-1";

			var jobDeclaration =
				helper.GetNewDeclarationWithInstruction(Factory, "IMP", "BOO1", "ENT0001", 20, false);

			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

			var entry = declaration.CusReconEntries.AddNew();
			entry.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;
			entry.CRE_CH_OriginalEntry = entryHeader.PK;
			entry.CRE_OA_DeclarantAddress = helper.Importer.MainAddress.PK;
			var line = entry.CusReconEntryLines.AddNew();
			line.CRL_OriginalEntryLineNumber = 1;
			line.CRL_LineNumber = 2;
			Factory.Save();

			var stmNote = outgoingMessage.Notes.AddNew();
			stmNote.ST_Description = MonthlyClosingCUSRECMessageProcessor.MonthlyClosingLinesNoteDescription;

			var invoiceLine = jobDeclaration.Invoices[0].InvoiceLines[0];

			var goodsItem = CreateGoodsItem("2", NotificationTypeList.Codes.Information, "100");

			void AssertUpdatesInventory(bool expectUpdate, bool isInCurrentMessage, bool isInOriginalMessage, string messageSubtype, string notificationSeverity = "ERR", bool isOutOfWarehouse = false)
			{
				messageMock.SetupGet(x => x.EM_MessageSubType).Returns(messageSubtype);
				dataProviderMock.SetupGet(e => e.ReferenceNumber).Returns("ATCNEWNUMBER0001");

				line.CRL_CustomsStatus = EntryStatus.RC1;

				entry.CRE_OriginalEntryNumber = "ENT1234";

				Mock.Get(goodsItem).Setup(x => x.NotificationSeverity).Returns(notificationSeverity);

				dataProviderMock
					.Setup(x => x.GoodsItems)
					.Returns(isInCurrentMessage ? new[] { goodsItem } : Array.Empty<ICUSRECGoodsItem>());

				stmNote.ST_NoteDataAsText = isInOriginalMessage ? "2" : "";

				attr.WB_EntryKey = "ENT1234";
				attr.WB_EntryLineNo = 1;
				receiveLine.WE_BondedEntryKey = "ENT1234-1";
				orderLineForReceive.WE_BondedEntryKey = "ENT1234-1";
				invoiceLine.JI_Procedure = isOutOfWarehouse ? procedureCode : "1000";

				ProcessMessage(message);

				var testMessage =
					$"For line that Is in current message: {isInCurrentMessage}, Is in original message: {isInOriginalMessage}, message sub type: {messageSubtype}, notification severity: {notificationSeverity}, is OutOfWarehouse: {isOutOfWarehouse}. Expected update: {expectUpdate}";

				if (expectUpdate)
				{
					AssertEquals(testMessage, (ZShort)2, attr.WB_EntryLineNo);
					AssertEquals(testMessage, "ATCNEWNUMBER0001", attr.WB_EntryKey);
					AssertEquals(testMessage, "ATCNEWNUMBER0001-2", receiveLine.WE_BondedEntryKey);
					AssertEquals(testMessage, "ATCNEWNUMBER0001-2", orderLineForReceive.WE_BondedEntryKey);
				}
				else
				{
					AssertEquals(testMessage, (ZShort)1, attr.WB_EntryLineNo);
					AssertEquals(testMessage, "ENT1234", attr.WB_EntryKey);
					AssertEquals(testMessage, "ENT1234-1", receiveLine.WE_BondedEntryKey);
					AssertEquals(testMessage, "ENT1234-1", orderLineForReceive.WE_BondedEntryKey);
				}

				AssertEquals("order should never be updated", (ZShort)1, orderAttr.WB_EntryLineNo);
				AssertEquals("order should never be updated", "ENT1234", orderAttr.WB_EntryKey);
				AssertEquals(testMessage, "OTH1234-1", orderLine.WE_BondedEntryKey);
			}

			CombineAssertions(() =>
			{
				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: false,
					isInOriginalMessage: true, MonthlyClosingBondedWarehouse);

				AssertUpdatesInventory(expectUpdate: false, isInCurrentMessage: false,
					isInOriginalMessage: true, MonthlyClosingFreeCirculation);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: false,
					isInOriginalMessage: true, MonthlyClosingFreeCirculation, isOutOfWarehouse: true);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: false,
					isInOriginalMessage: true, MonthlyClosingInwardProcessing);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingBondedWarehouse,
					notificationSeverity: NotificationTypeList.Codes.Information);

				AssertUpdatesInventory(expectUpdate: false, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingFreeCirculation,
					notificationSeverity: NotificationTypeList.Codes.Information);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingFreeCirculation,
					notificationSeverity: NotificationTypeList.Codes.Information, isOutOfWarehouse: true);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingInwardProcessing,
					notificationSeverity: NotificationTypeList.Codes.Information);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingBondedWarehouse,
					notificationSeverity: NotificationTypeList.Codes.Warning);

				AssertUpdatesInventory(expectUpdate: false, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingFreeCirculation,
					notificationSeverity: NotificationTypeList.Codes.Warning);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingFreeCirculation,
					notificationSeverity: NotificationTypeList.Codes.Warning, isOutOfWarehouse: true);

				AssertUpdatesInventory(expectUpdate: true, isInCurrentMessage: true,
					isInOriginalMessage: false, MonthlyClosingInwardProcessing,
					notificationSeverity: NotificationTypeList.Codes.Warning);
			});
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestNotAffectedCusReconEntryLines()
		{
			var (line1, _, _, _) = SetupCusReconEntryLines();
			CreateCurrentAndLodgedSnapshot(line1);

			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("2", NotificationTypeList.Codes.Error, MonthlyClosingCUSRECMessageProcessor.ATLASNotificationCodes_819)
			});

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Not part of outgoing message: line1.CRL_CustomsStatus not updated to RC2", false, line1.CRL_CustomsStatus == EntryStatus.RC2);
				AssertNotNull("Not part of outgoing message: CUR snapshot not deleted", line1.CurrentSnapshot);
				AssertEquals("Not part of outgoing message: LDG.CRS_SnapshotXml not updated", (ZString)LODGEDSnapshot, line1.LodgedSnapshot.CRS_SnapshotXml);

				var stmNote = outgoingMessage.Notes.AddNew();
				stmNote.ST_Description = MonthlyClosingCUSRECMessageProcessor.MonthlyClosingLinesNoteDescription;
				stmNote.ST_NoteDataAsText = "1|2|3|4";
				line1.CRL_CustomsStatus = EntryStatus.RC1;
				ProcessMessage(message);
				AssertEquals("Part of outgoing message: line1.CRL_CustomsStatus updated to RC2", EntryStatus.RC2, line1.CRL_CustomsStatus);
				AssertNull("Part of outgoing message: CUR snapshot deleted", line1.CurrentSnapshot);
				AssertEquals("Part of outgoing message: LDG.CRS_SnapshotXml updated from CUR snapshot", (ZString)MERGEDSnapshot, line1.LodgedSnapshot.CRS_SnapshotXml);
			});
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestUpdateCusReconEntries()
		{
			const string currentSnapshot = "<DEMonthlyClosingEntrySnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><Document><Division>4</Division><Type>ABC</Type><ReferenceNumber>REF1</ReferenceNumber><IssuingDate>2022-01-15</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
			const string lodgedSnapshot = "<DEMonthlyClosingEntrySnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><Document><Division>4</Division><Type>DEF</Type><ReferenceNumber>REF2</ReferenceNumber><IssuingDate>2022-01-20</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
			const string mergedSnapshot = "<DEMonthlyClosingEntrySnapshot LastUpdateTimeUtc=\"2022-01-25T15:41:19Z\" xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><Document><Division>4</Division><Type>ABC</Type><ReferenceNumber>REF1</ReferenceNumber><IssuingDate>2022-01-15</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
			var (line1, line2, entry1, entry2) = SetupCusReconEntryLines();
			var entry1CurrentSnapshot = entry1.CusReconSnapshots.AddNew();
			entry1CurrentSnapshot.CRS_Type = CusReconConstants.Current;
			entry1CurrentSnapshot.CRS_SnapshotXml = currentSnapshot;
			var entry1LodgedSnapshot = entry1.CusReconSnapshots.AddNew();
			entry1LodgedSnapshot.CRS_Type = CusReconConstants.Lodged;
			entry1LodgedSnapshot.CRS_SnapshotXml = lodgedSnapshot;
			var entry2CurrentSnapshot = entry2.CusReconSnapshots.AddNew();
			entry2CurrentSnapshot.CRS_Type = CusReconConstants.Current;
			var entry2LodgedSnapshot = entry2.CusReconSnapshots.AddNew();
			entry2LodgedSnapshot.CRS_Type = CusReconConstants.Lodged;
			entry2LodgedSnapshot.CRS_SnapshotXml = "entry2 LDG snapshot XML";
			var stmNote = outgoingMessage.Notes.AddNew();
			stmNote.ST_Description = MonthlyClosingCUSRECMessageProcessor.MonthlyClosingLinesNoteDescription;
			stmNote.ST_NoteDataAsText = "1";

			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("2", NotificationTypeList.Codes.Error, "100")
			});

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("line1.CRL_CustomsStatus", EntryStatus.RC2, line1.CRL_CustomsStatus);
				AssertEquals("line2.CRL_CustomsStatus", EntryStatus.REJ, line2.CRL_CustomsStatus);

				AssertNull("entry1 CUR snapshot deleted", entry1.CurrentSnapshot);
				AssertNull("entry2 CUR snapshot deleted", entry2.CurrentSnapshot);

				AssertEquals("entry1.CRS_SnapshotXml updated from CUR snapshot", mergedSnapshot, entry1.LodgedSnapshot.CRS_SnapshotXml);
				AssertEquals("entry2.CRS_SnapshotXml not updated", "entry2 LDG snapshot XML", entry2.LodgedSnapshot.CRS_SnapshotXml);
			});
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);

			var reference = declaration.CRD_JobReferenceNumber;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Monthly Closing CUSREC – Customs Receipt Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing CUSREC – Customs Receipt Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} received a Customs Receipt Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ "<tr><td>MRN</td><td>24DE12345678901234</td></tr>"
									+ "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
									+ "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
									+ "<tr><td>Registration Date</td><td>17.09.2020</td></tr>"
									+ "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestUpdateCusReconEntryLines_MultipleOccurrencesOfSameLineNumber()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var entry1 = declaration.CusReconEntries.AddNew();
			entry1.CRE_CH_OriginalEntry = entryHeader.PK;
			var line1 = entry1.CusReconEntryLines.AddNew();
			line1.CRL_LineNumber = 1;
			line1.CRL_OriginalEntryLineNumber = 1;
			var currentSnapshot1 = line1.CusReconSnapshots.AddNew();
			currentSnapshot1.CRS_Type = CusReconConstants.Current;
			var line2 = entry1.CusReconEntryLines.AddNew();
			line2.CRL_LineNumber = 2;
			line2.CRL_OriginalEntryLineNumber = 2;
			var currentSnapshot2 = line2.CusReconSnapshots.AddNew();
			currentSnapshot2.CRS_Type = CusReconConstants.Current;
			var line3 = entry1.CusReconEntryLines.AddNew();
			line3.CRL_LineNumber = 3;
			line3.CRL_OriginalEntryLineNumber = 3;
			var line4 = entry1.CusReconEntryLines.AddNew();
			line4.CRL_LineNumber = 4;
			line4.CRL_OriginalEntryLineNumber = 4;
			var line5 = entry1.CusReconEntryLines.AddNew();
			line5.CRL_LineNumber = 5;
			line5.CRL_OriginalEntryLineNumber = 5;
			var line6 = entry1.CusReconEntryLines.AddNew();
			line6.CRL_LineNumber = 6;
			line6.CRL_OriginalEntryLineNumber = 6;

			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("1", NotificationTypeList.Codes.Error, "1"),
				CreateGoodsItem("1", NotificationTypeList.Codes.Error, "2"),
				CreateGoodsItem("1", NotificationTypeList.Codes.Warning, "1"),
				CreateGoodsItem("1", NotificationTypeList.Codes.Information, "1"),

				CreateGoodsItem("2", NotificationTypeList.Codes.Error, "819"),
				CreateGoodsItem("2", NotificationTypeList.Codes.Warning, "1"),
				CreateGoodsItem("2", NotificationTypeList.Codes.Information, "1"),

				CreateGoodsItem("3", NotificationTypeList.Codes.Warning, "1"),
				CreateGoodsItem("3", NotificationTypeList.Codes.Warning, "2"),
				CreateGoodsItem("3", NotificationTypeList.Codes.Information, "1"),

				CreateGoodsItem("4", NotificationTypeList.Codes.Information, "1"),
				CreateGoodsItem("4", NotificationTypeList.Codes.Information, "2"),

				CreateGoodsItem("5", string.Empty, "1"),

				CreateGoodsItem("0", NotificationTypeList.Codes.Error, "1"),
			});
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Line1 - CustomsStatus", EntryStatus.REJ, line1.CRL_CustomsStatus);
				AssertEquals("Line1 snapshot deleted", true, currentSnapshot1.IsDeleted);
				AssertEquals("Line2 - CustomsStatus", EntryStatus.REJ, line1.CRL_CustomsStatus);
				AssertEquals("Line2 snapshot not deleted", false, currentSnapshot2.IsDeleted);
				AssertEquals("Line3 - CustomsStatus", EntryStatus.ERR, line3.CRL_CustomsStatus);
				AssertEquals("Line4 - CustomsStatus", EntryStatus.RC2, line4.CRL_CustomsStatus);
				AssertEquals("Line5 - CustomsStatus", ZString.Empty, line5.CRL_CustomsStatus);
				AssertEquals("Line6 - CustomsStatus", ZString.Empty, line6.CRL_CustomsStatus);
			});
		}

		protected override ZString MessageFriendlyName => "Monthly Closing CUSREC Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSREC>> Processor => new MonthlyClosingCUSRECMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, "ABC123456");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<ICUSREC>();
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.MRN).Returns("24DE12345678901234");
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("ABC123456");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("MAS/22/11/22027");
			dataProviderMock.Setup(x => x.RegistrationDate).Returns(new ZDate(2020, 9, 17));
			dataProviderMock.Setup(x => x.NotificationSeverity).Returns(Array.Empty<ZString>());
			dataProviderMock.Setup(x => x.GoodsItems).Returns(Array.Empty<ICUSRECGoodsItem>());

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSREC>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusReconDeclaration declaration;
		Mock<ICUSREC> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSREC>> messageMock;
		AtlasInboundEDIMessage<ICUSREC> message;
		EDIMessage outgoingMessage;

		void AssertDeclarationMRN(string testCase, bool expectedToExist, string expectedEntryNum = "", ZDate expectedIssueDate = default)
		{
			var cusEntryNum = CusEntryNumber.Load(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			if (expectedToExist)
			{
				AssertEquals($"{testCase}: CE_EntryNum", expectedEntryNum, cusEntryNum.CE_EntryNum);
				AssertEquals($"{testCase}: CE_IssueDate", expectedIssueDate, cusEntryNum.CE_IssueDate);
				AssertEquals($"{testCase}: CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, cusEntryNum.CE_Category);
				AssertEquals($"{testCase}: CE_EntryIsSystemGenerated", true, cusEntryNum.CE_EntryIsSystemGenerated);
			}
			else
			{
				AssertNull($"{testCase}: CusEntryNum doesn't exist", cusEntryNum);
			}
		}

		ICUSRECGoodsItem CreateGoodsItem(string sequenceNumber, string notificationSeverity, string notificationCode)
		{
			var goodsItemMock = new Mock<ICUSRECGoodsItem>();
			goodsItemMock.Setup(x => x.SequenceNumber).Returns(sequenceNumber);
			goodsItemMock.Setup(x => x.NotificationSeverity).Returns(notificationSeverity);
			goodsItemMock.Setup(x => x.NotificationCode).Returns(notificationCode);
			return goodsItemMock.Object;
		}

		void AssertCusReconEntryLineStatusAndLodgedSnapshotUpdated(string notificationSeverity, string expectedCustomsStatus, string presetCRL_CustomsStatus = "")
		{
			var (_, line2, _, _) = SetupCusReconEntryLines();
			line2.CRL_CustomsStatus = presetCRL_CustomsStatus;
			dataProviderMock.Setup(x => x.GoodsItems).Returns(new[]
			{
				CreateGoodsItem("2", notificationSeverity, "100")
			});

			CombineAssertions(() =>
			{
				AssertNull("line2 doesn't have CUR snapshot", line2.CurrentSnapshot);
				ProcessMessage(message);
				AssertEquals("line2.CRL_CustomsStatus updated", expectedCustomsStatus, line2.CRL_CustomsStatus);
				AssertNull("LDG snapshot not created", line2.LodgedSnapshot);

				var (currentSnapshot, lodgedSnapshot) = CreateCurrentAndLodgedSnapshot(line2);
				AssertSame("line2 has CUR snapshot", currentSnapshot, line2.CurrentSnapshot);
				ProcessMessage(message);
				AssertNull("CUR snapshot deleted", line2.CurrentSnapshot);
				AssertEquals("LodgedSnapshot.CRS_SnapshotXml updated from CUR snapshot", (ZString)MERGEDSnapshot, lodgedSnapshot.CRS_SnapshotXml);

				var currentSnapshot2 = line2.CusReconSnapshots.AddNew();
				currentSnapshot2.CRS_Type = CusReconConstants.Current;
				currentSnapshot2.CRS_SnapshotXml = CURRENTSnapshot2;
				AssertSame("line2 has new CUR snapshot", currentSnapshot2, line2.CurrentSnapshot);
				ProcessMessage(message);
				AssertNull("New CUR snapshot deleted", line2.CurrentSnapshot);
				AssertEquals("LodgedSnapshot.CRS_SnapshotXml updated from CUR snapshot", (ZString)MERGEDSnapshot2, lodgedSnapshot.CRS_SnapshotXml);
			});
		}

		(CusReconEntryLine, CusReconEntryLine, CusReconEntry, CusReconEntry) SetupCusReconEntryLines()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var entry1 = declaration.CusReconEntries.AddNew();
			entry1.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;
			entry1.CRE_CH_OriginalEntry = entryHeader.PK;
			entry1.CRE_OA_DeclarantAddress = orgAddress;
			var line1 = entry1.CusReconEntryLines.AddNew();
			line1.CRL_LineNumber = 1;
			line1.CRL_OriginalEntryLineNumber = 1;

			var entry2 = declaration.CusReconEntries.AddNew();
			entry2.CRE_EntryType = ImportDeclarationTypeList.Codes.AAV;
			entry2.CRE_CH_OriginalEntry = entryHeader.PK;
			entry2.CRE_OA_DeclarantAddress = orgAddress;
			var line2 = entry2.CusReconEntryLines.AddNew();
			line2.CRL_LineNumber = 2;
			line2.CRL_OriginalEntryLineNumber = 2;
			Factory.Save();

			return (line1, line2, entry1, entry2);
		}

		(Customs.Business.CusReconSnapshot, Customs.Business.CusReconSnapshot) CreateCurrentAndLodgedSnapshot(CusReconEntryLine cusReconLine)
		{
			var currentSnapshot = cusReconLine.CusReconSnapshots.AddNew();
			currentSnapshot.CRS_Type = CusReconConstants.Current;
			currentSnapshot.CRS_SnapshotXml = CURRENTSnapshot;
			var lodgedSnapshot = cusReconLine.CusReconSnapshots.AddNew();
			lodgedSnapshot.CRS_Type = CusReconConstants.Lodged;
			lodgedSnapshot.CRS_SnapshotXml = LODGEDSnapshot;
			return (currentSnapshot, lodgedSnapshot);
		}

		const string CURRENTSnapshot = "<DEMonthlyClosingEntryLineSnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><CommodityCode>CUR Snapshot XML</CommodityCode></DEMonthlyClosingEntryLineSnapshot>";
		const string CURRENTSnapshot2 = "<DEMonthlyClosingEntryLineSnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><CommodityCode>CUR Snapshot XML2</CommodityCode></DEMonthlyClosingEntryLineSnapshot>";
		const string LODGEDSnapshot = "<DEMonthlyClosingEntryLineSnapshot xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><TobaccoRevenueStampNumber>LDG</TobaccoRevenueStampNumber><CommodityCode>LDG Snapshot XML</CommodityCode></DEMonthlyClosingEntryLineSnapshot>";
		const string MERGEDSnapshot = "<DEMonthlyClosingEntryLineSnapshot LastUpdateTimeUtc=\"2022-01-25T15:41:19Z\" xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><TobaccoRevenueStampNumber>LDG</TobaccoRevenueStampNumber><CommodityCode>CUR Snapshot XML</CommodityCode></DEMonthlyClosingEntryLineSnapshot>";
		const string MERGEDSnapshot2 = "<DEMonthlyClosingEntryLineSnapshot LastUpdateTimeUtc=\"2022-01-25T15:41:19Z\" xmlns=\"http://www.cargowise.com/Schemas/DEMonthlyClosing\"><TobaccoRevenueStampNumber>LDG</TobaccoRevenueStampNumber><CommodityCode>CUR Snapshot XML2</CommodityCode></DEMonthlyClosingEntryLineSnapshot>";
	}
}
