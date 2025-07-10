using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EntryDocumentWrapperTest : XMLMessageTestHelper<EntryDocumentWrapperTest>
	{
		public void TestExporterType()
		{
			AssertEquals("Pre-condition", "", wrapper.Entry.Declaration.JE_ExporterType);
			declaration.JE_ExporterType = "A";
			AssertEquals("A", wrapper.Entry.Declaration.JE_ExporterType);
		}

		public void TestCustomsOfficeAndDivision()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "20";
			entry = declaration.CustomsEntryHeaders.AddNew();

			wrapper = new EntryDocumentWrapper(entry, Factory);

			AssertEquals(new ZString("내륙기지통관과"), wrapper.CustomsDepartmentName);
		}

		public void TestIsImportCancellationDeclinedByCustoms()
		{
			wrapper = new EntryDocumentWrapper(entry, Factory);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ExpiryDate = ZDateTime.Today;
			wrapper = new EntryDocumentWrapper(entry, Factory);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			var message = entry.Messages.AddNew();
			message.EM_MessageType = "5BG";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			wrapper = new EntryDocumentWrapper(entry, Factory);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			entryNumber.CE_ExpiryDate = ZDateTime.Empty;
			wrapper = new EntryDocumentWrapper(entry, Factory);
			AssertEquals(ZBool.True, wrapper.IsImportCancellationDeclinedByCustoms);
		}

		public void TestGetLatestOriginal5TWMessageONE()
		{
			SetUpStaff();
			entry.EntryNumber = "2292611001080U";
			var incomingMessage = CreateMessageForTest("GOVCBR5TW_ONE.xml", ElectronicDocumentTypeList.Codes._5TW);
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			var importEntryWrapper = new EntryDocumentWrapper(entry, Factory).ImportEntryWrapper;
			AssertNotNull(importEntryWrapper.MessageData5TW);
			AssertEquals("22926-11-001080U", importEntryWrapper.MessageData5TW.FormattedImportDeclarationNumber);

			var messageWrapper = new EDIMessageWrapper(incomingMessage);
			AssertEquals("22926-11-001080U", messageWrapper.MessageData5TW.FormattedImportDeclarationNumber);
		}

		public void TestGetLatestOriginal5TWMessageMUL()
		{
			SetUpStaff();
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "KR1";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "KR1";
			var incomingMessage2 = CreateMessageForTest("GOVCBR5TW_MUL2.xml", ElectronicDocumentTypeList.Codes._5TW);
			incomingMessage2.EM_GB = branch2.PK;
			incomingMessage2.EM_MessageSubType = OneOrMultiple.MUL;

			var entry1 = CreateImportEntry("2292611001049U");
			var entry2 = CreateImportEntry("2292611001270U");
			var entry3 = CreateImportEntry("2292611001533U");

			var incomingMessage = CreateMessageForTest("GOVCBR5TW_MUL.xml", ElectronicDocumentTypeList.Codes._5TW);
			Factory.Save();

			incomingMessage2.EM_MessageNum = incomingMessage.EM_MessageNum;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			var messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, ElectronicDocumentTypeList.Codes._5TW);
			messageFilter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.NotEqual, DBNull.Value);
			var messages = new BusinessObjectFactory().Load<EDIMessage>(messageFilter).ToList();
			AssertEquals(3, messages.Count);

			AssertEquals("MUL", incomingMessage.EM_MessageSubType);
			AssertEquals("OST", messages[0].EM_MessageSubType);
			AssertEquals("OST", messages[1].EM_MessageSubType);
			AssertEquals("OST", messages[2].EM_MessageSubType);
			AssertEquals("Original EDIMessage should not connect to object.", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry1.PK));
			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry2.PK));
			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry3.PK));

			var importEntryWrapper1 = new EntryDocumentWrapper(entry1, Factory).ImportEntryWrapper;
			AssertNotNull(importEntryWrapper1.MessageData5TW);
			AssertEquals("OriginalEDIMessage value", "22926-11-001049U", importEntryWrapper1.MessageData5TW.FormattedImportDeclarationNumber);
			AssertEquals(4, importEntryWrapper1.MessageData5TW.Lines.Count);

			var importEntryWrapper2 = new EntryDocumentWrapper(entry2, Factory).ImportEntryWrapper;
			AssertNotNull(importEntryWrapper2.MessageData5TW);
			AssertEquals("OriginalEDIMessage value", "22926-11-001049U", importEntryWrapper2.MessageData5TW.FormattedImportDeclarationNumber);
			AssertEquals(4, importEntryWrapper2.MessageData5TW.Lines.Count);

			var importEntryWrapper3 = new EntryDocumentWrapper(entry3, Factory).ImportEntryWrapper;
			AssertNotNull(importEntryWrapper3.MessageData5TW);
			AssertEquals("OriginalEDIMessage value", "22926-11-001049U", importEntryWrapper3.MessageData5TW.FormattedImportDeclarationNumber);
			AssertEquals(4, importEntryWrapper3.MessageData5TW.Lines.Count);

			var lineData = importEntryWrapper3.MessageData5TW.Lines;
			AssertEquals(4, lineData.Count);
			AssertEquals(2, lineData[0].ImportEntryLineNo);
			AssertEquals(new ZDate("2011-10-25"), lineData[0].ExamineStartDate);
			AssertEquals(new ZDate("2011-11-17"), lineData[0].ExamineEndDate);
			AssertEquals("분석결과에 따른 감액보정 내용(C-22-04595)", lineData[0].ContentDescription);
			AssertEquals("이상없음", lineData[0].CorrectionResult);
			AssertEquals("0106411000096", lineData[0].RequestDocumentNumber);
			AssertEquals("22926-11-001049U", lineData[0].FormattedAttachedDeclarationNumber);
			AssertEquals(new ZDate("2011-04-23"), lineData[0].IssueDate);

			CusEntryHeader CreateImportEntry(string entryNumber)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = entryNumber;

				return entry;
			}
		}

		EDIMessage CreateMessageForTest(string fileName, string messageType)
		{
			var fileReader = new TestFileReader(typeof(EntryDocumentWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "1";
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		void SetUpStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T2";
			staff.GS_LoginName = "Test2";
			staff.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			var importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff.PK;
		}

		public void TestGetLatestOriginal5TWMessageIsNull()
		{
			var wrapper = new EntryDocumentWrapper(entry, Factory).ImportEntryWrapper;
			Exception exception = null;
			try
			{
				var check5TWMessage = wrapper.MessageData5TW;
			}
			catch (ArgumentNullException ex)
			{
				exception = ex;
			}
			AssertNull(wrapper.MessageData5TW);
			AssertNull(exception);
		}

		public void TestImportEntryWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062022070010M";

			wrapper = new EntryDocumentWrapper(entry, Factory);

			AssertEquals("4062022070010M", wrapper.ImportEntryWrapper.Header.ImportDeclarationNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryDocumentWrapper(entry, Factory);
		}
		JobDeclaration declaration;
		CusEntryHeader entry;
		EntryDocumentWrapper wrapper;

		void SetupCustoms()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList.PK, ZZ.CodeListAttributeNames.BusinessNumber, "1218300561");
			helper.CreateCusCodeListAttribute(codeList.PK, ZZ.CodeListAttributeNames.Address, "인천광역시 중구 서해대로 339 (항동7가)");
			helper.CreateCusCodeListAttribute(codeList.PK, ZZ.CodeListAttributeNames.BankAccountID, "110288");
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(codeList2.PK, ZZ.CodeListAttributeNames.BusinessNumber, "99999000001");
			helper.CreateCusCodeListAttribute(codeList2.PK, ZZ.CodeListAttributeNames.Address, "부산광역시 해운대구 19");
			helper.CreateCusCodeListAttribute(codeList2.PK, ZZ.CodeListAttributeNames.BankAccountID, "789011");
			helper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			helper.CreateTaxOrFee(ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.00022m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, "FLA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		EDIMessage SetupMessageData(CusEntryHeader entry, string messageType, string messageNum, string messageSubType = null, string applicationReference = null, string fileName = null)
		{
			var message = entry.Messages.AddNew();
			message.EM_MessageType = messageType;
			message.EM_MessageNum = messageNum;
			message.EM_MessageSubType = messageSubType;
			message.EM_ApplicationReference = applicationReference;
			if (fileName != null)
			{
				var fileReader = new TestFileReader(typeof(EntryDocumentWrapperTest));
				var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
				message.EM_MessageText = messageText;
			}
			Factory.Save();
			return message;
		}

		void CreateImportEntrySnapshot(CusEntryHeader entry, ZString messageType, ImportEntryHeader header)
		{
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, messageType);
				Factory.Save();
			}
		}

		public void TestImport5FK()
		{
			SetupCustoms();
			new TestDataSetupHelper(Factory).SetEntry929Tariff();
			var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);

			var outgoingMessage929 = SetupMessageData(entry, "929", "1");
			var header = new ImportEntryHeaderCreator().Create(entry);
			header.TotalDutyAmount = 110000;
			header.TotalLiquorTax = 130000;
			header.TotalAgricultureTax = 150000;
			header.TotalTransportationTax = 170000;
			header.TotalEducationTax = 190000;
			header.TotalSpecialConsumptionTax = 210000;
			header.TotalVAT = 230000;
			header.PenaltyForLateDeclaration = 270000;
			header.PenaltyForMissedDeclaration = 290000;

			CreateImportEntrySnapshot(entry, ElectronicDocumentTypeList.Codes._929, header);
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			snapshot.CES_VersionNumber = 1;
			snapshot.CES_SystemCreateTimeUtc = outgoingMessage929.EM_SystemCreateTimeUtc;

			var responseMessage = SetupMessageData(entry, "5FK", "2", "5FE", "1");
			SetupNewStatementHeader(declaration.CompanyPK, "4062001070010U", "0127020112001320507", responseMessage.EM_MessageNum, entry.Declaration.JE_OH_DutyPayer);
			Factory.Save();

			var wrapper = new IndividualStatementWrapperCollection(entry, Factory).Cast<IndividualStatementWrapper>().First();
			var individualInvoice = wrapper.IndividualInvoice;

			AssertEquals(125.6m, wrapper.TotalGrossWeightInKG);
			AssertEquals(2, wrapper.TotalPackQty);
			AssertEquals("", wrapper.IndividualInvoice.FormattedImporterID);
			AssertEquals(new ZDateTime(2013, 01, 01), wrapper.DeclarationDate);
			AssertEquals("상호", wrapper.Declarant.CompanyName);
			AssertEquals("0000000000", wrapper.Declarant.PhoneNumber);
			AssertEquals("HJSC98100123AB01", wrapper.HouseBillNumber);
			AssertEquals(new ZDateTime(2014, 01, 01), individualInvoice.B2_DueDate);
			AssertEquals(new ZDateTime(2013, 01, 02), individualInvoice.B2_PrintDate);
			AssertEquals("789011", individualInvoice.BankAccountID);
			AssertEquals("0127", wrapper.FormattedStatementNumberFirstLine);
			AssertEquals("020-11-20-0-132050-7", wrapper.FormattedStatementNumberSecondLine);
			AssertEquals(905068210m, individualInvoice.B2_StatementAmount);
			AssertEquals(932197450m, individualInvoice.OverdueAmount);

			AssertEquals("40620-01-070010U", wrapper.FormattedImportDeclarationNumber);
			AssertEquals("홍나리", individualInvoice.PayerRepresentativeName);
			AssertEquals("서울시 강남구 논현동 235 7층 101호", individualInvoice.PayerAddressDetails);
			AssertEquals("모나리자(주)", individualInvoice.PayerCompanyName);
			AssertEquals("부산세관", individualInvoice.CustomsOfficeName);
			AssertEquals(36708730m, individualInvoice.FirstLine.DutyAmount);
			AssertEquals(0m, individualInvoice.FirstLine.LiquorTax);
			AssertEquals(2755490m, individualInvoice.FirstLine.AgricultureTax);
			AssertEquals(0m, individualInvoice.FirstLine.TransportationTax);
			AssertEquals(19351280m, individualInvoice.FirstLine.EducationTax);
			AssertEquals(64504410m, individualInvoice.FirstLine.SpecialConsumptionTax);
			AssertEquals(774406560m, individualInvoice.FirstLine.VAT);
			AssertEquals(0m, individualInvoice.FirstLine.PenaltyAndInterest);
			AssertEquals(7341740m, individualInvoice.FirstLine.PenaltyForLatePayment);
		}

		public void Test5WNStatement()
		{
			SetupCustoms();
			new TestDataSetupHelper(Factory).SetEntry929Tariff();
			var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);

			var outgoingMessage929 = SetupMessageData(entry, "929", "1");
			var header = new ImportEntryHeaderCreator().Create(entry);
			header.TotalDutyAmount = 110000;
			header.TotalLiquorTax = 130000;
			header.TotalAgricultureTax = 150000;
			header.TotalTransportationTax = 170000;
			header.TotalEducationTax = 190000;
			header.TotalSpecialConsumptionTax = 210000;
			header.TotalVAT = 230000;
			header.PenaltyForLateDeclaration = 270000;
			header.PenaltyForMissedDeclaration = 290000;

			var responseMessage = SetupMessageData(entry, "5WN", "2", fileName: "GOVCBR5WN_Empty.xml");
			SetupNewStatementHeader(declaration.CompanyPK, "4062001070010U", "020112001320507", responseMessage.EM_MessageNum, entry.Declaration.JE_OH_DutyPayer);
			Factory.Save();

			var wrapper = new IndividualStatementWrapperCollection(entry, Factory).Cast<IndividualStatementWrapper>().First();
			var individualInvoice = wrapper.IndividualInvoice;

			AssertEquals(125.6m, wrapper.TotalGrossWeightInKG);
			AssertEquals(2, wrapper.TotalPackQty);
			AssertEquals("", wrapper.IndividualInvoice.FormattedImporterID);
			AssertEquals(new ZDateTime(2013, 01, 01), wrapper.DeclarationDate);
			AssertEquals("상호", wrapper.Declarant.CompanyName);
			AssertEquals("0000000000", wrapper.Declarant.PhoneNumber);
			AssertEquals("HJSC98100123AB01", wrapper.HouseBillNumber);
			AssertEquals(new ZDateTime(2014, 01, 01), individualInvoice.B2_DueDate);
			AssertEquals(new ZDateTime(2013, 01, 02), individualInvoice.B2_PrintDate);
			AssertEquals("789011", individualInvoice.BankAccountID);
			AssertEquals(ZString.Empty, wrapper.FormattedStatementNumberFirstLine);
			AssertEquals("020-11-20-0-132050-7", wrapper.FormattedStatementNumberSecondLine);
			AssertEquals(905068210m, individualInvoice.B2_StatementAmount);
			AssertEquals(932197450m, individualInvoice.OverdueAmount);

			AssertEquals("40620-01-070010U", wrapper.FormattedImportDeclarationNumber);
			AssertEquals("홍나리", individualInvoice.PayerRepresentativeName);
			AssertEquals("서울시 강남구 논현동 235 7층 101호", individualInvoice.PayerAddressDetails);
			AssertEquals("모나리자(주)", individualInvoice.PayerCompanyName);
			AssertEquals("부산세관", individualInvoice.CustomsOfficeName);
			AssertEquals(36708730m, individualInvoice.FirstLine.DutyAmount);
			AssertEquals(0m, individualInvoice.FirstLine.LiquorTax);
			AssertEquals(2755490m, individualInvoice.FirstLine.AgricultureTax);
			AssertEquals(0m, individualInvoice.FirstLine.TransportationTax);
			AssertEquals(19351280m, individualInvoice.FirstLine.EducationTax);
			AssertEquals(64504410m, individualInvoice.FirstLine.SpecialConsumptionTax);
			AssertEquals(774406560m, individualInvoice.FirstLine.VAT);
			AssertEquals(0m, individualInvoice.FirstLine.PenaltyAndInterest);
			AssertEquals(7341740m, individualInvoice.FirstLine.PenaltyForLatePayment);
		}

		void SetupNewStatementHeader(ZGuid companyPK, ZString entryNumber, ZString statementNumber, ZString incomingMessageNum, ZGuid payerPK)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = companyPK;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			statement.B2_IncomingMessageNo = incomingMessageNum;

			statement.B2_DueDate = new ZDateTime(2014, 01, 01);
			statement.B2_PrintDate = new ZDateTime(2013, 01, 02);
			statement.B2_ProcessPort = "030";
			statement.B2_StatementNumber = statementNumber;
			statement.B2_StatementAmount = 905068210m;
			statement.B2_OH_Importer = payerPK;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_SequenceNumber = 1;
			SetChargeAmount(statementLine, ChargeTypeList.Codes.Duty, 36708730m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.LiquorTax, 0m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.AgricultureTax, 2755490m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.TransportationTax, 0m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.EducationTax, 19351280m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.SpecialConsumptionTax, 64504410m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.VAT, 774406560m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.PenaltyAndInterest, 0m);
			SetChargeAmount(statementLine, ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 7341740m);
			Factory.Save();

			void SetChargeAmount(CusStatementLine statementLine, ZString chargeType, ZDecimal chargeAmount)
			{
				var charge = statementLine.Charges.AddNew();
				charge.B4_ChargeType = chargeType;
				charge.B4_ChargeAmount = chargeAmount;
			}
		}

		public void TestFTAHeaderWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_FTARelationArticleCode = "4";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = "4062022070010M";
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.CusEntryLine.CL_LineNumber = 1;
			invoiceLine.CusEntryLine.CL_FTASequenceNumber = 1;
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine.JI_COOSupportingDocType = "1";
			invoiceLine.JI_SequenceNumber = 1;

			wrapper = new EntryDocumentWrapper(entry, Factory);

			AssertEquals("4062022070010M", wrapper.FTAHeader.Header.ImportDeclarationNumber);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
