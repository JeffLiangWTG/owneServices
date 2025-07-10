using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ImportEntryHeaderWrapperDecoratorTest : TestCaseWithFactory
	{
		public void TestNewClassFieldValues()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01011041", "삼원산업 보세창고", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.BA;
			declaration.JE_LocationOtherInformation = "01001001";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "CFR";
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_PaymentTerms = "DA";
			invoice1.JZ_InvoiceCurrExRate = 1210.12m;
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			var importPreviousExpDecLine1 = invoiceLine.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine1.CSI_ReferenceNumber = "4062001088809X";
			importPreviousExpDecLine1.CSI_ReferenceNumber2 = "456";
			importPreviousExpDecLine1.CSI_ItemNumber = 21;
			importPreviousExpDecLine1.CSI_UnitOfQuantity = Messaging.PackageKindCodeList.Codes.CT;
			importPreviousExpDecLine1.CSI_Quantity = 321;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062001070010U";
			entry.CH_Status = "ORJ";
			entry.CH_EntryReleaseDate = new ZDateTime("2024-04-01");
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2024, 04, 24);
			var entryNumD72 = entry.EntryNumbers.AddNew();
			entryNumD72.CE_EntryType = "D72";
			var entryNum5BD = entry.EntryNumbers.AddNew();
			entryNum5BD.CE_EntryType = "5BD";
			var entryNum5TM = entry.EntryNumbers.AddNew();
			entryNum5TM.CE_EntryType = "5TM";
			var customsOfficer = entry.CustomsOfficers.AddNew();
			customsOfficer.CY_Code = CustomsOfficerTypeList.Codes._5BDResponsibleCustomsOfficer;
			customsOfficer.CY_Data = "김숙희";
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 4;
			entryLine1.CL_AdValoremTariff = "0208100000";
			entryLine1.CL_CustomsValue = 999999999990m;
			entryLine1.CL_ValueForVAT = 11m;
			invoiceLine.JI_CL = entryLine1.PK;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("ORJ", wrapper.MessageStatus);
			AssertEquals(new ZDateTime("2024-04-01"), wrapper.EntryReleaseDate);
			AssertEquals(new ZDateTime("2024-04-24"), wrapper.DeclarationDate);
			AssertEquals(entryNumD72, wrapper.EntryNumD72);
			AssertEquals(entryNum5BD, wrapper.EntryNum5BD);
			AssertEquals(entryNum5TM, wrapper.EntryNum5TM);
			AssertEquals("김숙희", wrapper.CustomsOfficerName5BD);
			AssertEquals("BA(Barrel)", wrapper.EntryLinePackTitle);
			AssertEquals("경의선철도 입출경검사장(지상)", wrapper.BondedAreaName);
		}

		public void TestPayerCustomsCodes()
		{
			var entryWithBusiness = CreateImportEntryData(OrgConstants.Category.Business, "KR1", "Business Company");
			AssertPayerCustomsCodes(entryWithBusiness, false);

			var entryWithIndividual = CreateImportEntryData(OrgConstants.Category.NaturalPersonIndividual, "KR2", "Person Individual");
			AssertPayerCustomsCodes(entryWithIndividual, true);

			void AssertPayerCustomsCodes(CusEntryHeader entry, bool isIndividual)
			{
				var entryHeader = new ImportEntryHeaderCreator().Create(entry);
				var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
				wrapper.Decorate(entry);

				AssertEquals("DutyPayer has CorporationCode", "1234561234567", entry.Declaration.DutyPayer.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == IdentificationType.CorporationCode).OK_CustomsRegNo);
				AssertEquals("Not saved in ImportEntryHeader.Payer.", null, entryHeader.Payer.CorporationCode);
				AssertEquals("123456-1234567", wrapper.PayerFormattedCorporationCode);

				if (isIndividual)
				{
					CombineAssertions("If category is NAT, only used 'Korean Reg No'", () =>
					{
						AssertEquals(null, entryHeader.Payer.BusinessRegNo);
						AssertEquals("4208092046311", entryHeader.Payer.KoreanRegNoForResident);

						AssertEquals(ZString.Empty, wrapper.PayerFormattedBusinessRegNo);
						AssertEquals("420809-2046311", wrapper.PayerFormattedKoreanRegNoForResident);
					});
				}
				else
				{
					CombineAssertions("If category is BUS, only used 'Business Reg No'", () =>
					{
						AssertEquals("1028142299", entryHeader.Payer.BusinessRegNo);
						AssertEquals(null, entryHeader.Payer.KoreanRegNoForResident);

						AssertEquals("102-81-42299", wrapper.PayerFormattedBusinessRegNo);
						AssertEquals(ZString.Empty, wrapper.PayerFormattedKoreanRegNoForResident);
					});
				}
			}

			CusEntryHeader CreateImportEntryData(string category, string code, string name)
			{
				var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, category, code, name);
				var payerCusCodes = new IDNumberAndType[]
				{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1028142299", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "4208092046311", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.CorporationCode, Number = "1234561234567", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth }
				};
				TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCusCodes);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_DutyPayer = payer.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				return entry;
			}
		}

		public void TestMessageData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message5TV_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5TV, new ZDateTime("2024-01-01"), TestIncomingFilesPath, "GOVCBR5TV_Empty.xml");
			var message5TV_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5TV, new ZDateTime("2024-01-02"), TestIncomingFilesPath, "GOVCBR5TV_0.xml");
			var message5UO_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5UO, new ZDateTime("2024-01-03"), TestIncomingFilesPath, "GOVCBR5UO_Empty.xml");
			var message5UO_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5UO, new ZDateTime("2024-01-04"), TestIncomingFilesPath, "GOVCBR5UO_CUS.xml");
			var message5WN_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5WN, new ZDateTime("2024-01-05"), TestIncomingFilesPath, "GOVCBR5WN_Empty.xml");
			var message5WN_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5WN, new ZDateTime("2024-01-06"), TestIncomingFilesPath, "GOVCBR5WN_Document.xml");
			var message5TW_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5TW, new ZDateTime("2024-01-03"), TestIncomingFilesPath, "GOVCBR5TW_Result2.xml");
			message5TW_1.EM_MessageSubType = "OST";
			var message5TW_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5TW, new ZDateTime("2024-01-04"), TestIncomingFilesPath, "GOVCBR5TW_ONE.xml");
			message5TW_2.EM_MessageSubType = "ONE";

			var message5BD_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BD, new ZDateTime("2024-01-01"), TestOutgoingFilesPath, "GOVCBR5BD_D2.xml");
			var message5BD_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BD, new ZDateTime("2024-01-02"), TestOutgoingFilesPath, "GOVCBR5BD_D1.xml");
			var message5GV_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5GV, new ZDateTime("2024-01-03"), TestIncomingFilesPath, "GOVCBR5GV_TransactionNatureCode2.xml");
			var message5GV_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5GV, new ZDateTime("2024-01-04"), TestIncomingFilesPath, "GOVCBR5GV_TransactionNatureCode3.xml");
			var message5BE_1 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BE, new ZDateTime("2024-01-05"), TestIncomingFilesPath, "GOVCBR5BE_0.xml");
			var message5BE_2 = CreateEDIMessage(ElectronicDocumentTypeList.Codes._5BE, new ZDateTime("2024-01-06"), TestIncomingFilesPath, "GOVCBR5BE_CCL.xml");

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("나세관", wrapper.MessageData5TV.CustomsManagerName);
			AssertNotEquals(wrapper.MessageData5TV.CustomsManagerName, new EDIMessageWrapper(message5TV_1).MessageData5TV.CustomsManagerName);
			AssertEquals(wrapper.MessageData5TV.CustomsManagerName, new EDIMessageWrapper(message5TV_2).MessageData5TV.CustomsManagerName);

			AssertEquals(new ZDate("2014-05-06"), wrapper.MessageData5UO.NoticeDate);
			AssertNotEquals(wrapper.MessageData5UO.NoticeDate, new EDIMessageWrapper(message5UO_1).MessageData5UO.NoticeDate);
			AssertEquals(wrapper.MessageData5UO.NoticeDate, new EDIMessageWrapper(message5UO_2).MessageData5UO.NoticeDate);

			AssertEquals("032-722-4062", wrapper.MessageData5WN.CustomsPersonPhoneNumber);
			AssertNotEquals(wrapper.MessageData5WN.CustomsPersonPhoneNumber, new EDIMessageWrapper(message5WN_1).MessageData5WN.CustomsPersonPhoneNumber);
			AssertEquals(wrapper.MessageData5WN.CustomsPersonPhoneNumber, new EDIMessageWrapper(message5WN_2).MessageData5WN.CustomsPersonPhoneNumber);

			AssertEquals("2292611001080U", wrapper.MessageData5TW.ImportDeclarationNumber);
			AssertNotEquals(wrapper.MessageData5TW.ImportDeclarationNumber, new EDIMessageWrapper(message5TW_1).MessageData5TW.ImportDeclarationNumber);
			AssertEquals(wrapper.MessageData5TW.ImportDeclarationNumber, new EDIMessageWrapper(message5TW_2).MessageData5TW.ImportDeclarationNumber);

			AssertEquals("수리전반출신청", wrapper.MessageSendingObject5BD.AmendmentReason);
			AssertNotEquals(wrapper.MessageSendingObject5BD.AmendmentReason, new EDIMessageWrapper(message5BD_1).MessageSendingObject5BD.AmendmentReason);
			AssertEquals(wrapper.MessageSendingObject5BD.AmendmentReason, new EDIMessageWrapper(message5BD_2).MessageSendingObject5BD.AmendmentReason);

			AssertEquals("이종렬", wrapper.MessageData5GV.CustomsManagerName);
			AssertNotEquals(wrapper.MessageData5GV.CustomsManagerName, new EDIMessageWrapper(message5GV_1).MessageData5GV.CustomsManagerName);
			AssertEquals(wrapper.MessageData5GV.CustomsManagerName, new EDIMessageWrapper(message5GV_2).MessageData5GV.CustomsManagerName);

			AssertEquals("G", wrapper.MessageData5BE.ResultType);
			AssertNotEquals(wrapper.MessageData5BE.ResultType, new EDIMessageWrapper(message5BE_1).MessageData5BE.ResultType);
			AssertEquals(wrapper.MessageData5BE.ResultType, new EDIMessageWrapper(message5BE_2).MessageData5BE.ResultType);

			EDIMessage CreateEDIMessage(string messageType, ZDateTime systemCrateTimeUTC, string filePath, string fileName)
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_SystemCreateTimeUtc = systemCrateTimeUTC;

				var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
				var messageText = fileReader.GetEmbeddedFileText(filePath, fileName);
				message.EM_MessageText = messageText;

				return message;
			}
		}

		public void TestImporterTypeOfBusiness()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZString.Empty, wrapper.ImporterTypeOfBusiness);

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "Test Company");
			var orgHeaderWrapper = OrgHeaderWrapper.New(importer);
			orgHeaderWrapper.ZO_TypeOfBusiness = "도매, 서비스";
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			wrapper.Decorate(entry);
			AssertEquals("도매, 서비스", wrapper.ImporterTypeOfBusiness);
		}

		public void TestHasAny5FNRejection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(false, wrapper.HasAny5FNRejection);

			Create5FNEntryNum();
			wrapper.Decorate(entry);
			AssertEquals(false, wrapper.HasAny5FNRejection);

			Create5FNEntryNum();
			wrapper.Decorate(entry);
			AssertEquals(false, wrapper.HasAny5FNRejection);

			Create5FNEntryNum(CustomsMessageStatusTypeList.Codes.OriginalRejected);
			wrapper.Decorate(entry);
			AssertEquals(true, wrapper.HasAny5FNRejection);

			void Create5FNEntryNum(string entryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent)
			{
				var entryNumber = entry.EntryNumbers.AddNew();
				entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
				entryNumber.CE_EntryStatus = entryStatus;
			}
		}

		string TestIncomingFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
		string TestOutgoingFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
