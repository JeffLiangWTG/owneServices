using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportEntryHeaderWrapper))]
	sealed class ImportEntryHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "나대표", true);
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1028142299" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			var payer = Factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "Test";
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1248105504", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "4208092046311", Core.Constants.CountryCodes.KoreaSouth);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01001001", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "01011041", "삼원산업 보세창고", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = PackageKindCodeList.Codes.BA;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_CustomsOffice = "010";
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_LocationOtherInformation = "01001001";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000025X";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2024, 04, 24);
			entry.CusEntryNumber.CE_ExpiryDate = new ZDateTime(2025, 04, 24);
			entry.CH_EntryReleaseDate =  new ZDateTime(2024, 04, 01);
			var entryNum5GU = entry.EntryNumbers.AddNew();
			entryNum5GU.CE_EntryType = "5GU";
			var entryNum5UA = entry.EntryNumbers.AddNew();
			entryNum5UA.CE_EntryType = "5UA";
			var customsOfficer = entry.CustomsOfficers.AddNew();
			customsOfficer.CY_Code = CustomsOfficerTypeList.Codes.CancellationCustomsOfficer;
			customsOfficer.CY_Data = "COF678-이민주";
			var entryLine1 = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ImportCargoManagementNumber = "01KE0766SS200100003";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			Factory.Save();

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertSame(entryHeader, wrapper.Header);
			AssertEquals("20240424", wrapper.DeclarationDate.ToString(DateFormatType.Date));
			AssertEquals("20250424", wrapper.CancellationDecisionDate.ToString(DateFormatType.Date));
			AssertEquals("20240401", wrapper.EntryReleaseDate.ToString(DateFormatType.Date));
			AssertEquals("BA(Barrel)", wrapper.EntryLinePackTitle);
			AssertEquals("경의선철도 입출경검사장(지상)", wrapper.BondedAreaName);
			AssertEquals("001", wrapper.FormattedTotalEntryLineCount);
			AssertSame(customsOfficer, wrapper.CustomsOfficers5BF);
			AssertEquals("01KE0766SS2-0010-0003", wrapper.FormattedCargoManagementNo);
			AssertEquals("나대표", wrapper.Importer.RepresentativeName);
			AssertEquals(declaration.BrokerAddress.CompanyName, wrapper.Declarant.CompanyName);
			AssertEquals(declaration.PayerAddress.CompanyName, wrapper.Payer.CompanyName);
			AssertEquals("124-81-05504", wrapper.PayerFormattedBusinessRegNo);
			AssertEquals("서울세관", wrapper.CustomsOfficeName);
			AssertEquals(entryNum5GU, wrapper.EntryNum5GU);
			AssertEquals(entryNum5UA, wrapper.EntryNum5UA);
		}

		public void TestReImportOfExportGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "CFR";
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_PaymentTerms = "DA";
			invoice1.JZ_InvoiceCurrExRate = 1210.12m;
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_SequenceNumber = 1;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "CFR";
			invoice2.JZ_RX_NKInvoice_Currency = "KRW";
			invoice2.JZ_PaymentTerms = "DA";
			invoice2.JZ_InvoiceCurrExRate = 1210.12m;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 1;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062001070010U";
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 4;
			entryLine1.CL_AdValoremTariff = "0208100000";
			entryLine1.CL_CustomsValue = 999999999990m;
			entryLine1.CL_ValueForVAT = 11m;
			invoiceLine.JI_CL = entryLine1.PK;
			var importPreviousExpDecLine1 = entryLine1.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine1.CSI_ReferenceNumber = "4062001088809X";
			importPreviousExpDecLine1.CSI_ReferenceNumber2 = "456";
			importPreviousExpDecLine1.CSI_ItemNumber = 21;
			importPreviousExpDecLine1.CSI_UnitOfQuantity = Messaging.PackageKindCodeList.Codes.CT;
			importPreviousExpDecLine1.CSI_Quantity = 321;
			importPreviousExpDecLine1.CSI_LineNo = 3;
			var importPreviousExpDecLine2 = entryLine1.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine2.CSI_ReferenceNumber = "4062001088808X";
			importPreviousExpDecLine2.CSI_ReferenceNumber2 = "123";
			importPreviousExpDecLine2.CSI_ItemNumber = 11;
			importPreviousExpDecLine2.CSI_UnitOfQuantity = Messaging.PackageKindCodeList.Codes.VG;
			importPreviousExpDecLine2.CSI_Quantity = 1000;
			importPreviousExpDecLine2.CSI_LineNo = 2;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			entryLine2.CL_AdValoremTariff = "0208100000";
			entryLine2.CL_CustomsValue = 12340m;
			entryLine2.CL_ValueForVAT = 15m;
			invoiceLine2.JI_CL = entryLine2.PK;
			var importPreviousExpDecLine3 = entryLine2.PreviousExpDecLineCollection.AddNew();
			importPreviousExpDecLine3.CSI_ReferenceNumber = "4062001088709X";
			importPreviousExpDecLine3.CSI_ReferenceNumber2 = "987";
			importPreviousExpDecLine3.CSI_ItemNumber = 21;
			importPreviousExpDecLine3.CSI_UnitOfQuantity = Messaging.PackageKindCodeList.Codes.BG;
			importPreviousExpDecLine3.CSI_Quantity = 321;
			importPreviousExpDecLine3.CSI_LineNo = 1;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("40620-01-070010U", wrapper.FormattedImportDeclarationNumber);

			AssertEquals(1, wrapper.ImportPreviousExpDecLines[0].ImportEntryLineNo);
			AssertEquals("40620-01-088709X", wrapper.ImportPreviousExpDecLines[0].FormattedDeclarationNumber);
			AssertEquals(987, wrapper.ImportPreviousExpDecLines[0].ImportPreviousExpDecLine.EntryLineNo);
			AssertEquals(21, wrapper.ImportPreviousExpDecLines[0].ImportPreviousExpDecLine.InvoiceLineNo);
			AssertEquals(321m, wrapper.ImportPreviousExpDecLines[0].ImportPreviousExpDecLine.UsedQty);
			AssertEquals("BG", wrapper.ImportPreviousExpDecLines[0].ImportPreviousExpDecLine.UQ);
			AssertEquals((short)1, wrapper.ImportPreviousExpDecLines[0].ImportPreviousExpDecLine.SequenceNumber);

			AssertEquals(4, wrapper.ImportPreviousExpDecLines[1].ImportEntryLineNo);
			AssertEquals("40620-01-088808X", wrapper.ImportPreviousExpDecLines[1].FormattedDeclarationNumber);
			AssertEquals(123, wrapper.ImportPreviousExpDecLines[1].ImportPreviousExpDecLine.EntryLineNo);
			AssertEquals(11, wrapper.ImportPreviousExpDecLines[1].ImportPreviousExpDecLine.InvoiceLineNo);
			AssertEquals(1000m, wrapper.ImportPreviousExpDecLines[1].ImportPreviousExpDecLine.UsedQty);
			AssertEquals("VG", wrapper.ImportPreviousExpDecLines[1].ImportPreviousExpDecLine.UQ);
			AssertEquals((short)2, wrapper.ImportPreviousExpDecLines[1].ImportPreviousExpDecLine.SequenceNumber);

			AssertEquals(4, wrapper.ImportPreviousExpDecLines[2].ImportEntryLineNo);
			AssertEquals("40620-01-088809X", wrapper.ImportPreviousExpDecLines[2].FormattedDeclarationNumber);
			AssertEquals(456, wrapper.ImportPreviousExpDecLines[2].ImportPreviousExpDecLine.EntryLineNo);
			AssertEquals(21, wrapper.ImportPreviousExpDecLines[2].ImportPreviousExpDecLine.InvoiceLineNo);
			AssertEquals(321m, wrapper.ImportPreviousExpDecLines[2].ImportPreviousExpDecLine.UsedQty);
			AssertEquals("CT", wrapper.ImportPreviousExpDecLines[2].ImportPreviousExpDecLine.UQ);
			AssertEquals((short)3, wrapper.ImportPreviousExpDecLines[2].ImportPreviousExpDecLine.SequenceNumber);
		}

		public void TestFirstEntryLine()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", new ZDateTime(2013, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "TACKS1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208122222", new ZDateTime(2013, 01, 01).AddDays(-2), ZDateTime.Today.AddDays(1), "TACKS2");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4062001070010U";
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "0208100000";
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0208122222";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0208100000";
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CustomsQuantity = 10m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0208122222";
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_CustomsQuantity = 20m;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("0208100000", wrapper.FirstEntryLine.HSCode);
			AssertEquals("TACKS1", wrapper.FirstEntryLine.HSDescription);
			AssertEquals(10m, wrapper.FirstEntryLine.Quantity);
		}

		public void TestEntryNumD72()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumD72 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D72);
			entryNumD72.CE_ExpiryDate = ZDateTime.Today.AddDays(1);
			entryNumD72.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZDateTime.Today.AddDays(1), wrapper.EntryNumD72.CE_ExpiryDate);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, wrapper.EntryNumD72.CE_EntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalSent, wrapper.EntryNumD72.EntryStatusDescription);
		}

		public void TestIsImportCancellationDeclinedByCustoms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ExpiryDate = ZDateTime.Today;

			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			var message = entry.Messages.AddNew();
			message.EM_MessageType = "5BG";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZBool.False, wrapper.IsImportCancellationDeclinedByCustoms);

			entryNumber.CE_ExpiryDate = ZDateTime.Empty;
			entryHeader = new ImportEntryHeaderCreator().Create(entry);
			wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(ZBool.True, wrapper.IsImportCancellationDeclinedByCustoms);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			return wrapper;
		}

		public void TestMessageData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message5BF_1 = CreateOutgoingEDIMessage(ElectronicDocumentTypeList.Codes._5BF, new ZDateTime("2024-01-01"), "GOVCBR5BF_0.xml");
			var message5BF_2 = CreateOutgoingEDIMessage(ElectronicDocumentTypeList.Codes._5BF, new ZDateTime("2024-01-02"), "GOVCBR5BF_1.xml");
			var message5GU_1 = CreateIncomingEDIMessage(ElectronicDocumentTypeList.Codes._5GU, new ZDateTime("2024-01-01"), "GOVCBR5GU_WithoutVer.xml");
			var message5GU_2 = CreateIncomingEDIMessage(ElectronicDocumentTypeList.Codes._5GU, new ZDateTime("2024-01-02"), "GOVCBR5GU_RealData.xml");
			var message5UB_1 = CreateIncomingEDIMessage(ElectronicDocumentTypeList.Codes._5UB, new ZDateTime("2024-01-03"), "GOVCBR5UB_0.xml");
			var message5UB_2 = CreateIncomingEDIMessage(ElectronicDocumentTypeList.Codes._5UB, new ZDateTime("2024-01-04"), "GOVCBR5UB_WithoutDutyTaxFee.xml");

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);

			AssertEquals("취하사유", "사유", wrapper.MessageSendingObject5BF.CancellationReason);
			AssertNotEquals(wrapper.MessageSendingObject5BF.CancellationReason, new EDIMessageWrapper(message5BF_1).MessageSendingObject5BF.CancellationReason);
			AssertEquals(wrapper.MessageSendingObject5BF.CancellationReason, new EDIMessageWrapper(message5BF_2).MessageSendingObject5BF.CancellationReason);

			AssertEquals("문서번호", "040-C2-시정-21-01363", wrapper.MessageData5GU.ComplementNumber);
			AssertEquals("일자", new ZDate(2021, 07, 08), wrapper.MessageData5GU.CustomsRegistryDate);
			AssertNotEquals(wrapper.MessageData5GU.ComplementNumber, new EDIMessageWrapper(message5GU_1).MessageData5GU.ComplementNumber);
			AssertEquals(wrapper.MessageData5GU.ComplementNumber, new EDIMessageWrapper(message5GU_2).MessageData5GU.ComplementNumber);

			AssertEquals(10.5m, wrapper.MessageData5UB.PenaltyExemptionAmount);
			AssertEquals("광주세관 납세심사과-1914 (가산세 면제 통지)", wrapper.MessageData5UB.ResultReason);
			AssertNotEquals(wrapper.MessageData5UB.PenaltyExemptionAmount, new EDIMessageWrapper(message5UB_1).MessageData5UB.PenaltyExemptionAmount);
			AssertEquals(wrapper.MessageData5UB.PenaltyExemptionAmount, new EDIMessageWrapper(message5UB_2).MessageData5UB.PenaltyExemptionAmount);

			EDIMessage CreateOutgoingEDIMessage(string messageType, ZDateTime systemCrateTimeUTC, string fileName)
			{
				return CreateEDIMessage(messageType, systemCrateTimeUTC, fileName, EDIMessage.Direction.Transmit, TestOutgoingFilesPath);
			}

			EDIMessage CreateIncomingEDIMessage(string messageType, ZDateTime systemCrateTimeUTC, string fileName)
			{
				return CreateEDIMessage(messageType, systemCrateTimeUTC, fileName, EDIMessage.Direction.Receive, TestIncomingFilesPath);
			}

			EDIMessage CreateEDIMessage(string messageType, ZDateTime systemCrateTimeUTC, string fileName, string receiveTransmit, string filePath)
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_ReceiveTransmit = receiveTransmit;
				message.EM_SystemCreateTimeUtc = systemCrateTimeUTC;

				var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
				var messageText = fileReader.GetEmbeddedFileText(filePath, fileName);
				message.EM_MessageText = messageText;

				return message;
			}
		}

		public void TestGOVCBRD72Messages()
		{
			#region Tariff
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "KRC";
			company.GC_RN_NKCountryCode = "KR";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRC";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "01234", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "USED EXCAVATOR");
			#endregion

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "나대표", true);
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1028142299" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			#region OrgHeader
			var broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READY1", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "김환태", true);
			TestOrgDataSetUpHelper.AddOrgAddress(broker.MainAddress, "서울특별시 서초구 동광로 41", "레디인빌딩", "06561", "101010", "020120");
			var brokerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1028142299" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(broker, brokerCodes);
			broker.MainAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			branch.GB_OH_OrgProxy = broker.PK;
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaymentPartyList.Codes.BRK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_GB = branch.PK;
			declaration.JE_CustomsOffice = "010";
			var countryCode = declaration.CountryCode;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000010M";

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "6N00221000010M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_EntryStatus = ZString.Empty;
			entryNumber.CE_RN_NKCountryCode = countryCode;

			var entryNumber1 = entry.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNumber1.CE_EntryLineReference = "1";
			entryNumber1.CE_RN_NKCountryCode = countryCode;

			#region D72Line
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Model = "02JAC106I603C087";
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_LinePrice = 68m;
			invoiceLine2.UnitPrice = 6m;
			invoiceLine2.JI_CountryOfOrigin = countryCode;
			invoiceLine2.JI_Tariff = "01234";
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_ScheduledReExportDate = new ZDateTime(2021, 03, 31);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Model = "9E1J29A3802 X0849";
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_LinePrice = 610m;
			invoiceLine1.UnitPrice = 610m;
			invoiceLine1.JI_CountryOfOrigin = countryCode;
			invoiceLine1.JI_Tariff = "01234";
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_ScheduledReExportDate = new ZDateTime(2021, 03, 31);

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_LineNumber = 002;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = "01234";
			#endregion
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime(2021, 03, 31));
				messageSendingObject.ReasonDescription = "주문 수집 일정 변동에 따른 재수출기한 연장 신청";
				messageSendingObject.NewReExportDate = new ZDateTime(2021, 04, 06);

				AssertEquals(0, entry.Messages.Count);
				AssertEquals(0, entry.SubsequentMessageDetails.GOVCBRD72Messages.Count);

				Factory.Save();

				var entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
				CreateEDIMessage(entryLoaded, ElectronicDocumentTypeList.Codes._929, "TRX", "1");
				CreateEDIMessage(entryLoaded, ElectronicDocumentTypeList.Codes._R99, "RCV", "2", "1");

				var entryNumD72 = entryLoaded.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D72);
				entryNumD72.CE_EntryLineReference = "";
				CreateEDIMessageD72(entryLoaded, ElectronicDocumentTypeList.Codes._D72, "TRX", "3", "1");
				CreateEDIMessage(entryLoaded, ElectronicDocumentTypeList.Codes._R20, "RCV", "4", "3");
				CreateEDIMessageD72(entryLoaded, ElectronicDocumentTypeList.Codes._D72, "TRX", "5", "1");
				CreateEDIMessage(entryLoaded, ElectronicDocumentTypeList.Codes._R99, "RCV", "6", "5");
				CreateEDIMessage(entryLoaded, ElectronicDocumentTypeList.Codes._R43, "RCV", "7", "5");

				entryNumD72.CE_EntryLineReference = "1";
				CreateEDIMessageD72(entryLoaded, ElectronicDocumentTypeList.Codes._D72, "TRX", "8", "2");
				AssertEquals(8, entryLoaded.Messages.Count);
				AssertEquals(3, entryLoaded.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._D72).Count());

				AssertEquals(2, entryLoaded.SubsequentMessageDetails.GOVCBRD72Messages.Count);
			}
			EDIMessage CreateEDIMessage(CusEntryHeader entry, string messageType, string receiveTransmit, string messageNum, string applicationReference = "")
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_ReceiveTransmit = receiveTransmit;
				message.EM_MessageNum = messageNum;
				message.EM_ApplicationReference = applicationReference;
				Factory.Save();
				return message;
			}

			EDIMessage CreateEDIMessageD72(CusEntryHeader entry, string messageType, string receiveTransmit, string messageNum, string applicationReference = "")
			{
				var message = entry.Messages.AddNew();
				message.EM_MessageType = messageType;
				message.EM_ReceiveTransmit = receiveTransmit;
				message.EM_MessageNum = messageNum;
				message.EM_ApplicationReference = applicationReference;

				var messageSendingObject = new ExtendReExportDateMessageSendingObject(entry, new ZDateTime(2021, 03, 31));
				var importD72 = new ImportD72Creator().Create(entry, messageSendingObject, 1);
				var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();

				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					message.SetEM_MessageTextOrDataSource(makeStream);
					Factory.Save();
				}
				return message;
			}
		}

		public void TestEntryNum5TM()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNum5TM = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TM);
			entryNum5TM.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, wrapper.EntryNum5TM.CE_EntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalSent, wrapper.EntryNum5TM.EntryStatusDescription);
		}

		public void TestGoldVATDeclarationEntryLineObjects()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "HSDescription1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208200000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "HSDescription2");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			CreateEntryLineData(1, true, 10m, 100m, "0208100000", 1000m);
			CreateEntryLineData(2, false, 20m, 200m, "0208200000", 2000m);

			var messageSendingEntryLineObjectCollection = new MessageSendingEntryLineObjectCollection(entry);
			messageSendingEntryLineObjectCollection.PopulateElementsFromMergedLines(x => true, ElectronicDocumentTypeList.Codes._5TM);
			var lineObject1 = messageSendingEntryLineObjectCollection.Cast<MessageSendingEntryLineObject>().FirstOrDefault(x => x.EntryLineNo == 1);
			AssertEntryLineObject(lineObject1, 10m, 100m, "0208100000", "HSDescription1", 1000m);

			var lineObject2 = messageSendingEntryLineObjectCollection.Cast<MessageSendingEntryLineObject>().FirstOrDefault(x => x.EntryLineNo == 2);
			AssertEntryLineObject(lineObject2, 20m, 200m, "0208200000", "HSDescription2", 2000m);

			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertEquals(1, wrapper.GoldVATDeclarationEntryLineObjects.Count);

			var goldVATDeclarationEntryLineObject = wrapper.GoldVATDeclarationEntryLineObjects.Cast<MessageSendingEntryLineObject>().FirstOrDefault();
			AssertEntryLineObject(goldVATDeclarationEntryLineObject, 10m, 100m, "0208100000", "HSDescription1", 1000m);

			void CreateEntryLineData(short lineNumber, bool isGoldOrItsProduct, decimal valueForVat, decimal vat, string tariff, decimal netWeight)
			{
				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = lineNumber;
				entryLine.CL_IsGoldOrItsProduct = isGoldOrItsProduct;
				entryLine.CL_ValueForVAT = valueForVat;
				var fee = entryLine.Fees.AddNew();
				fee.CF_ChargeType = ChargeTypeList.Codes.VAT;
				fee.CF_ChargeAmount = vat;

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.JI_NetWeight = netWeight;
				invoiceLine.JI_NetWeightUQ = "KG";
			}

			void AssertEntryLineObject(MessageSendingEntryLineObject lineObject, decimal valueForVat, decimal vat, string hsCode, string hsDescription, decimal netWeight)
			{
				AssertEquals(valueForVat, lineObject.ValueForVAT);
				AssertEquals(vat, lineObject.VAT);
				AssertEquals(hsCode, lineObject.HSCode);
				AssertEquals(hsDescription, lineObject.HSDescription);
				AssertEquals(netWeight, lineObject.NetWeightInKG);
			}
		}

		string TestIncomingFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
		string TestOutgoingFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
