using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EDIMessageWrapperTest : XMLMessageTestHelper<EDIMessageWrapperTest>
	{
		public void TestEntryAndPayer()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "TEST";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			var declarationMessage = Factory.New<EDIMessage>();
			declaration.Messages.Add(declarationMessage);

			var messageWrapper = new EDIMessageWrapper(declarationMessage);
			AssertNull(messageWrapper.Entry);
			//AssertNull(messageWrapper.Payer);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryMessage = Factory.New<EDIMessage>();
			entry.Messages.Add(entryMessage);

			messageWrapper = new EDIMessageWrapper(entryMessage);
			AssertEquals(entry, messageWrapper.Entry);
			//AssertEquals("TEST", messageWrapper.Payer.CompanyName);
		}

		[TestDate(2021, 04, 06)]
		public void TestMessageSendingObject5BDPropertiesArePopulated()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000010M";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime("2021-04-06");

			var messageSendingObject = new EarlyReleaseMiscMessageSendingObject(entry);
			messageSendingObject.AmendmentReason = "수리전반출신청";
			messageSendingObject.SecurityType = "99";
			messageSendingObject.SecurityStartDate = new ZDateTime("2021-04-06");
			messageSendingObject.SecurityEndDate = new ZDateTime("2021-05-05");
			messageSendingObject.SecurityAmount = 100000m;
			messageSendingObject.OtherSecurityType = "현금";
			messageSendingObject.ReasonForEarlyRemoval = "01";

			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;
			entry.Messages.Add(message);

			var import5BD = new Import5BDCreator().Create(entry, messageSendingObject);
			var result = new GOVCBR5BDMessageBuilder(import5BD).GenerateMessage();
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				message.SetEM_MessageTextOrDataSource(makeStream);
				Factory.Save();
			}

			var messageWrapper = new EDIMessageWrapper(message);
			AssertEquals("수리전반출신청", messageWrapper.MessageSendingObject5BD.AmendmentReason);
			AssertEquals("99", messageWrapper.MessageSendingObject5BD.SecurityType);
			AssertEquals(new ZDateTime("2021-04-06"), messageWrapper.MessageSendingObject5BD.SecurityStartDate);
			AssertEquals(new ZDateTime("2021-05-05"), messageWrapper.MessageSendingObject5BD.SecurityEndDate);
			AssertEquals(100000m, messageWrapper.MessageSendingObject5BD.SecurityAmount);
			AssertEquals("현금", messageWrapper.MessageSendingObject5BD.OtherSecurityType);
			AssertEquals("01", messageWrapper.MessageSendingObject5BD.ReasonForEarlyRemoval);
			AssertEquals("미조립상태 분할선적 물품", messageWrapper.MessageSendingObject5BD.ReasonForEarlyRemovalName);
			AssertEquals("기타", messageWrapper.MessageSendingObject5BD.SecurityTypeName);
			AssertEquals(new ZDateTime("2021-04-06"), messageWrapper.MessageSendingObject5BD.RequestDate);
			Assert("Should have been cleared", !messageWrapper.HasChanges);

			var message2 = Factory.New<EDIMessage>();
			message2.EM_MessageType = ElectronicDocumentTypeList.Codes._5SI;
			entry.Messages.Add(message2);
			IsIncoming = false;
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5SI_D1.xml");
			message2.EM_MessageText = messageText;
			Factory.Save();

			var messageWrapper2 = new EDIMessageWrapper(message2);
			AssertEquals("", messageWrapper2.MessageSendingObject5BD.AmendmentReason);
			AssertEquals("", messageWrapper2.MessageSendingObject5BD.SecurityType);
			AssertEquals(ZDateTime.Empty, messageWrapper2.MessageSendingObject5BD.SecurityStartDate);
			AssertEquals(ZDateTime.Empty, messageWrapper2.MessageSendingObject5BD.SecurityEndDate);
			AssertEquals(0m, messageWrapper2.MessageSendingObject5BD.SecurityAmount);
			AssertEquals("", messageWrapper2.MessageSendingObject5BD.OtherSecurityType);
			AssertEquals("", messageWrapper2.MessageSendingObject5BD.ReasonForEarlyRemoval);
			AssertEquals("", messageWrapper2.MessageSendingObject5BD.ReasonForEarlyRemovalName);
		}

		bool IsIncoming { get; set; }
		public override string TestFilesPath => IsIncoming ? "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming" : "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		public void TestMessageSendingObject5BFPropertiesArePopulated()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N00221000010M";

			var messageSendingObject = new CancellationMessageSendingObject(entry);
			messageSendingObject.CancellationReason = "취하신청사유";

			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5BF;
			entry.Messages.Add(message);

			var import5BF = new Import5BFCancelCreator().Create(entry, messageSendingObject);
			var result = new GOVCBR5BFMessageBuilder(import5BF).GenerateMessage();
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				message.SetEM_MessageTextOrDataSource(makeStream);
				Factory.Save();
			}

			var messageWrapper = new EDIMessageWrapper(message);
			AssertEquals("취하신청사유", messageWrapper.MessageSendingObject5BF.CancellationReason);
		}

		public void TestMessageSentDateTimeInLocalTimeZone()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_SystemCreateTimeUtc = new ZDateTime(2021, 12, 1, 23, 0, 0);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				AssertEquals(new ZDateTime(2021, 12, 2, 8, 0, 0), new EDIMessageWrapper(message).SentDateTimeInLocalTimeZone);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Poland))
			{
				AssertEquals(new ZDateTime(2021, 12, 2, 0, 0, 0), new EDIMessageWrapper(message).SentDateTimeInLocalTimeZone);
			}
		}

		[TestDate(2021, 04, 06)]
		public void TestMessageSendingObjectD72PropertiesArePopulated()
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
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
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

			var entryNumber5FN = entry.EntryNumbers.AddNew();
			entryNumber5FN.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			entryNumber5FN.CE_EntryLineReference = "2";
			entryNumber5FN.CE_RN_NKCountryCode = countryCode;

			var entryNumberD72 = entry.EntryNumbers.AddNew();
			entryNumberD72.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNumberD72.CE_EntryLineReference = "1";
			entryNumberD72.CE_RN_NKCountryCode = countryCode;

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

				AssertEquals(1, messageSendingObject.D72EntryLines.Count);
				AssertEquals(2, messageSendingObject.MessageSendingInvoiceLines.Count);
				messageSendingObject.MessageSendingInvoiceLines[0].Remark = "Remark1";
				messageSendingObject.MessageSendingInvoiceLines[1].Remark = "Remark2";

				var importD72 = new ImportD72Creator().Create(entry, messageSendingObject, 1);
				var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();
				var message = entry.Messages.AddNew();
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					message.SetEM_MessageTextOrDataSource(makeStream);
					Factory.Save();
				}

				var messageWrapper = new EDIMessageWrapper(message);
				AssertNullOrEmpty(messageWrapper.MessageSendingObjectD72.MessageOrEntryStatus);
				AssertNullOrEmpty(messageWrapper.MessageSendingObjectD72.MessageOrEntryStatusDescription);
				AssertEquals("서울세관", messageWrapper.MessageSendingObjectD72.CustomsOfficeName);
				AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", messageWrapper.MessageSendingObjectD72.ReasonDescription);
				AssertEquals(new ZDateTime("2021-04-06"), messageWrapper.MessageSendingObjectD72.NewReExportDate);
				AssertEquals(new ZDateTime("2021-03-31"), messageWrapper.MessageSendingObjectD72.CurrentReExportScheduledDate);
				AssertEquals(ZDateTime.Empty, messageWrapper.MessageSendingObjectD72.EffectiveDateTime);

				AssertEquals("레디코리아", messageWrapper.MessageSendingObjectD72.CompanyNameForDocument);
				AssertEquals("김환태", messageWrapper.MessageSendingObjectD72.RepresentativeNameForDocument);
				AssertEquals("102-81-42299", messageWrapper.MessageSendingObjectD72.FormattedNoForDocument);
				AssertEquals("서울특별시 서초구 동광로 41 레디인빌딩", messageWrapper.MessageSendingObjectD72.AddressDetailsForDocument);

				AssertEquals("MessageSendingInvoiceLines count should be 2", 2, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines.Count);
				AssertEquals((ZShort)2, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].EntryLineNo);
				AssertEquals((ZShort)1, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].InvoiceLineNo);
				AssertEquals(1m, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].Quantity);
				AssertEquals(610m, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].LinePrice);
				AssertEquals("KG", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].UQ);
				AssertEquals("USED EXCAVATOR", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].HSDescription);
				AssertEquals("9E1J29A3802 X0849", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].ItemDescription);
				AssertEquals("EUR", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].AmountCurrency);
				AssertEquals("Remark1", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[0].Remark);

				AssertEquals((ZShort)2, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].EntryLineNo);
				AssertEquals((ZShort)2, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].InvoiceLineNo);
				AssertEquals(5m, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].Quantity);
				AssertEquals(30m, messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].LinePrice);
				AssertEquals("PC", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].UQ);
				AssertEquals("USED EXCAVATOR", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].HSDescription);
				AssertEquals("02JAC106I603C087", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].ItemDescription);
				AssertEquals("EUR", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].AmountCurrency);
				AssertEquals("Remark2", messageWrapper.MessageSendingObjectD72.MessageSendingInvoiceLines[1].Remark);

				var messageANT = CreateD72messageAndIncomingMessage(entry, messageSendingObject, 2, CustomsEntryStatusTypeList.Codes.ANT, "GOVCBRR43_Result_C.xml");
				var statusANTMessageWrapper = new EDIMessageWrapper(messageANT);
				AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, statusANTMessageWrapper.MessageSendingObjectD72.MessageOrEntryStatus);
				AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, statusANTMessageWrapper.MessageSendingObjectD72.MessageOrEntryStatusDescription);
				AssertEquals(new ZDateTime("2020-12-28"), statusANTMessageWrapper.MessageSendingObjectD72.EffectiveDateTime);

				var messageDMS = CreateD72messageAndIncomingMessage(entry, messageSendingObject, 3, CustomsEntryStatusTypeList.Codes.DMS, "GOVCBRR43_Result_E.xml");
				var statusDMSMessageWrapper = new EDIMessageWrapper(messageDMS);
				AssertEquals(CustomsEntryStatusTypeList.Codes.DMS, statusDMSMessageWrapper.MessageSendingObjectD72.MessageOrEntryStatus);
				AssertEquals(CustomsEntryStatusTypeList.Descriptions.DMS, statusDMSMessageWrapper.MessageSendingObjectD72.MessageOrEntryStatusDescription);
				AssertEquals(ZDateTime.Empty, statusDMSMessageWrapper.MessageSendingObjectD72.EffectiveDateTime);
			}
		}

		EDIMessage CreateD72messageAndIncomingMessage(CusEntryHeader entry, ExtendReExportDateMessageSendingObject messageSendingObject, ZInt versionNumber, ZString messageOwner, ZString fileName)
		{
			var importD72 = new ImportD72Creator().Create(entry, messageSendingObject, versionNumber);
			var result = new GOVCBRD72MessageBuilder(importD72).GenerateMessage();
			var message = entry.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				message.SetEM_MessageTextOrDataSource(makeStream);
				Factory.Save();
			}

			var messageR43 = entry.Messages.AddNew();
			messageR43.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR43.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR43.EM_MessageType = ElectronicDocumentTypeList.Codes._R43;
			messageR43.EM_ApplicationReference = message.EM_MessageNum;
			messageR43.EM_MessageOwner = messageOwner;
			IsIncoming = true;
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			messageR43.EM_MessageText = messageText;

			Factory.Save();

			return message;
		}

		public void Test5BEMessageData()
		{
			var incomingMessage = CreateMessage("GOVCBR5BE_0.xml", ElectronicDocumentTypeList.Codes._5BE);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5BE);
			AssertEquals("6N00220000076M", wrapper.MessageData5BE.ImportDeclarationNumber);
			AssertEquals("C", wrapper.MessageData5BE.ResultType);
			AssertEquals("01", wrapper.MessageData5BE.ResultCode);
		}

		public void Test5GVMessageData()
		{
			var incomingMessage = CreateMessage("GOVCBR5GV_0.xml", ElectronicDocumentTypeList.Codes._5GV);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5GV);
			AssertEquals(1, wrapper.MessageData5GV.Lines.Count);
			AssertEquals((short)1, wrapper.MessageData5GV.Lines[0].FirstLineNo);
			AssertEquals("B407", wrapper.MessageData5GV.Lines[0].FirstLineDataItemID);
		}

		public void Test5GVMessageDataWhenLinesAreAbsent()
		{
			var incomingMessage = CreateMessage("GOVCBR5GV_NoGoodsShipment.xml", ElectronicDocumentTypeList.Codes._5GV);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5GV);
			AssertEquals(0, wrapper.MessageData5GV.Lines.Count);
		}

		public void Test5GVMessageDataWhenLinesAreMany()
		{
			var incomingMessage = CreateMessage("GOVCBR5GVWith22GoodsShipments.xml", ElectronicDocumentTypeList.Codes._5GV);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "014", "안산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5GV);
			AssertEquals("문서번호", "014-10-보완-21-00013", wrapper.MessageData5GV.ComplementNumber);
			AssertEquals("발행일자", new ZDate(2021, 03, 26), wrapper.MessageData5GV.NoticeDate);
			AssertEquals("신고일자", new ZDate(2021, 03, 27), wrapper.MessageData5GV.DocumentNoticeDate);
			AssertEquals("보완요구기한일자", new ZDate(2021, 04, 10), wrapper.MessageData5GV.ComplementByDate);

			AssertEquals(11, wrapper.MessageData5GV.Lines.Count);
			AssertEquals((short)0, wrapper.MessageData5GV.Lines[0].FirstLineNo);
			AssertEquals("A408", wrapper.MessageData5GV.Lines[0].FirstLineDataItemID);
			AssertEquals((short)0, wrapper.MessageData5GV.Lines[0].SecondLineNo);
			AssertEquals("A203", wrapper.MessageData5GV.Lines[0].SecondLineDataItemID);

			AssertEquals((short)1, wrapper.MessageData5GV.Lines[1].FirstLineNo);
			AssertEquals("B407", wrapper.MessageData5GV.Lines[1].FirstLineDataItemID);
			AssertEquals("관세감면분납코드", wrapper.MessageData5GV.Lines[1].FirstLineDataItemIDDescription);
			AssertEquals((short)1, wrapper.MessageData5GV.Lines[1].SecondLineNo);
			AssertEquals("B408", wrapper.MessageData5GV.Lines[1].SecondLineDataItemID);
			AssertEquals("관세감면율", wrapper.MessageData5GV.Lines[1].SecondLineDataItemIDDescription);

			AssertEquals("보완요구사유", "일부 항목 정정보완", wrapper.MessageData5GV.ComplementDescription);
			AssertEquals("보완요구부서", "안산세관 통관지원(1)과", wrapper.MessageData5GV.DeclarationOffice);
			AssertEquals("담당과장", "김보성", wrapper.MessageData5GV.PrimaryOfficialName);
			AssertEquals("담당자", "하현순", wrapper.MessageData5GV.CustomsManagerName);
			AssertEquals("연락처", "031-8085-3863", wrapper.MessageData5GV.CustomsManagerPhoneNumber);

			var incomingMessage2 = CreateMessage("GOVCBR5GV_TransactionNatureCode2.xml", ElectronicDocumentTypeList.Codes._5GV);
			Factory.Save();

			var wrapper2 = new EDIMessageWrapper(incomingMessage2);
			AssertNotNull(wrapper2.MessageData5GV);
			AssertEquals("보완요구서류", "수입요건 관련 서류\r\n수입통관 서류 일체(인보이스,BL, 등)\r\n요건비대상 관련 서류 제출", wrapper2.MessageData5GV.DocumentName);
			AssertEquals("발급기관", "기타\r\n기타2\r\n식품의약품안전처", wrapper2.MessageData5GV.IssuingPartyName);
		}

		public void Test5GVMessageDataWhenMessageIsNot5GV()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5GV);
		}

		public void Test5UOMessageDataTaxItem()
		{
			var incomingMessage = CreateMessage("GOVCBR5UO_CUS.xml", ElectronicDocumentTypeList.Codes._5UO);
			Factory.Save();
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5UO);

			var messageData5UO = wrapper.MessageData5UO;

			AssertEquals("6N00220000085M", messageData5UO.RefundDeclarationNumber);
			AssertEquals("켐트리 코퍼레이션", messageData5UO.ImportCompanyName);
			AssertEquals("이재천", messageData5UO.ImportRepresentativeName);
			AssertEquals("경기 성남시 수정구 위례순환로 211,", messageData5UO.FirstImportAddressLine);
			AssertEquals("5405동 504호 (창곡동,위례센트럴푸르지오)", messageData5UO.SecondImportAddressLine);
			AssertEquals("부산세관", messageData5UO.CustomsOfficeName);
			AssertEquals("심사정보과", messageData5UO.CustomsDivisionName);
			AssertEquals(new ZDate("2020-08-27"), messageData5UO.SubmissionDate);
			AssertEquals(new ZDate("2020-09-24"), messageData5UO.ApprovalDate);
			AssertEquals("030752019608", messageData5UO.ApprovalNo);
			AssertEquals("기업은행", messageData5UO.BankCodeName);
			AssertEquals("한국은행", messageData5UO.BankCodeName2);
			AssertEquals("98600471901016", messageData5UO.BankAccountNumber);
			AssertEquals(new ZDate("2014-05-06"), messageData5UO.NoticeDate);

			AssertEquals("If DutyTaxType is VAT, OriginalAmount value.", 536820m, messageData5UO.VAT);
			AssertEquals("If DutyTaxType is CUD, OriginalAmount value.", 5368150m, messageData5UO.DutyAmount);
			AssertEquals("If DutyTaxType is 5CZ, OriginalAmount value.", 5913667m, messageData5UO.TotalAmount);
			AssertEquals("If DutyTaxType is IND, OriginalAmount value.", 100m, messageData5UO.SpecialConsumptionTax);
			AssertEquals("If DutyTaxType is ENV, OriginalAmount value.", 200m, messageData5UO.TransportationTax);
			AssertEquals("If DutyTaxType is ACT, OriginalAmount value.", 300m, messageData5UO.LiquorTax);
			AssertEquals("If DutyTaxType is 5AB, OriginalAmount value.", 400m, messageData5UO.EducationTax);
			AssertEquals("If DutyTaxType is CAP, OriginalAmount value.", 500m, messageData5UO.AgricultureTax);
			AssertEquals("If DutyTaxType is 5CS, OriginalAmount value.", 600m, messageData5UO.NonDutyTaxRevenue);
			AssertEquals("totalAdditionalAmount + totalInterestAmount + Sum(TypeCode == (5AC/5AY/5CT), OriginalAmount)", 6597m, messageData5UO.Penalty);
			AssertEquals("messageData5UO.TotalAmount - Sum(TypeCode == (5AC/5AY/5CT), OriginalAmount)", 5907667m, messageData5UO.RefundAmount);
		}

		public void Test5UOMessageDataWhenMessageIsNot5UO()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5UO);
		}

		public void Test5UBMessageData()
		{
			var incomingMessage = CreateMessage("GOVCBR5UB_WithoutDutyTaxFee.xml", ElectronicDocumentTypeList.Codes._5UB);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5UB);
			AssertEquals(10.5m, wrapper.MessageData5UB.PenaltyExemptionAmount);
			AssertEquals("광주세관 납세심사과-1914 (가산세 면제 통지)", wrapper.MessageData5UB.ResultReason);
		}

		public void Test5UBMessageDataWhenMessageIsNot5UB()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5GV;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5UB);
		}

		public void Test5TWMessageData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var incomingMessage = CreateMessage("GOVCBR5TW_MUL.xml", ElectronicDocumentTypeList.Codes._5TW);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5TW);
			AssertEquals("010-11-000096", wrapper.MessageData5TW.FormattedNoticeNumber);
			AssertEquals("22926-11-001049U", wrapper.MessageData5TW.FormattedImportDeclarationNumber);
			AssertEquals("서울세관", wrapper.MessageData5TW.NoticeCustomsOfficeName);

			wrapper.MessageData5TW.ContentDescription = "값이 그대로 출력됩니다.";
			AssertEquals("값이 그대로 출력됩니다.", wrapper.MessageData5TW.ContentDescriptionShort);
			wrapper.MessageData5TW.CorrectionResult = "값이 그대로 출력됩니다.";
			AssertEquals("값이 그대로 출력됩니다.", wrapper.MessageData5TW.CorrectionResultShort);

			wrapper.MessageData5TW.ContentDescription = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다여기부터짤립니다";
			AssertEquals("테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", wrapper.MessageData5TW.ContentDescriptionShort);
			wrapper.MessageData5TW.CorrectionResult = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다여기부터짤립니다";
			AssertEquals("테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", wrapper.MessageData5TW.CorrectionResultShort);

			var lineData = wrapper.MessageData5TW.Lines;
			AssertEquals(4, lineData.Count);
			AssertEquals(2, lineData[0].ImportEntryLineNo);
			AssertEquals(new ZDate("2011-10-25"), lineData[0].ExamineStartDate);
			AssertEquals(new ZDate("2011-11-17"), lineData[0].ExamineEndDate);
			AssertEquals("분석결과에 따른 감액보정 내용(C-22-04595)", lineData[0].ContentDescription);
			AssertEquals("이상없음", lineData[0].CorrectionResult);
			AssertEquals("0106411000096", lineData[0].RequestDocumentNumber);
			AssertEquals("22926-11-001049U", lineData[0].FormattedAttachedDeclarationNumber);
			AssertEquals(new ZDate("2011-04-23"), lineData[0].IssueDate);

			lineData[0].ContentDescription = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다";
			lineData[0].CorrectionResult = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다";
			AssertEquals("테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", lineData[0].ContentDescriptionShort);
			AssertEquals("테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", lineData[0].CorrectionResultShort);

			lineData[1].ContentDescription = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다여기부터짤립니다.";
			lineData[1].CorrectionResult = "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다여기부터짤립니다.";
			AssertEquals("value has been cut off => '.'", "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", lineData[1].ContentDescriptionShort);
			AssertEquals("value has been cut off => '.'", "테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다테스트데이터값입니다", lineData[1].CorrectionResultShort);
		}

		public void TestArgumentNullException()
		{
			Exception exception = null;
			try
			{
				var wrapper = new EDIMessageWrapper(null);
				var check5TWMessage = wrapper.MessageData5TW;
			}
			catch (ArgumentNullException ex)
			{
				exception = ex;
			}
#if NETFRAMEWORK
			AssertContains("Parameter name: message", exception.Message);
#else
			AssertContains("Message (Parameter 'message')", exception.Message);
#endif
		}

		public void Test5TWMessageDataWhenMessageIsNot5TW()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5TW);
		}

		public void Test5TVMessageDataWhenMessageIsNot5TV()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5TV);
		}

		public void Test5TVMessageData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "1", new ZDateTime("2014-01-01").AddDays(-2), new ZDateTime("2014-01-01").AddDays(1), "품명1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2", new ZDateTime("2014-01-01").AddDays(-2), new ZDateTime("2014-01-01").AddDays(1), "품명2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "3", new ZDateTime("2014-01-01").AddDays(-2), new ZDateTime("2014-01-01").AddDays(1), "품명3");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "4", new ZDateTime("2014-01-01").AddDays(-2), new ZDateTime("2014-01-01").AddDays(1), "품명4");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_IssueDate = new ZDateTime("2014-01-01");

			var incomingMessage = CreateMessage("GOVCBR5TV_0.xml", ElectronicDocumentTypeList.Codes._5TV);
			incomingMessage.EM_LinkedObject = entry;
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5TV);
			var messageData5TV = wrapper.MessageData5TV;
			AssertEquals("01014546123", messageData5TV.NoticeNumber);
			AssertEquals("01020", messageData5TV.CustomsOfficeAndCustomsDivision);
			AssertEquals("나세관", messageData5TV.CustomsManagerName);
			AssertEquals("나과장", messageData5TV.CustomsPersonName);
			AssertEquals("041-658-9856", messageData5TV.CustomsPersonPhoneNumber);
			AssertEquals("6N00220000084M", messageData5TV.ImportDeclarationNumber);
			AssertEquals("서울세관", messageData5TV.CustomsOfficeName);
			AssertEquals("내륙기지통관과", messageData5TV.CustomsDepartmentName);
			AssertEquals("010-14-546123", messageData5TV.FormattedNoticeNumber);

			AssertEquals(new ZDate("2014-01-01"), messageData5TV.ImportDeclarationDate);

			var messageData5TVLine = wrapper.MessageData5TV.Lines;
			AssertEquals(4, messageData5TVLine.Count);

			AssertEquals(new ZShort(12), messageData5TVLine[0].EntryLineNo);
			AssertEquals((int)Constants.BeforeOrAfterAmendment.Before, messageData5TVLine[0].BeforeOrAfterAmendmentIndicator);
			AssertEquals("1", messageData5TVLine[0].HSCode);
			AssertEquals("품명1", messageData5TVLine[0].HSCodeDescription);
			AssertEquals(15m, messageData5TVLine[0].DutyRate);
			AssertEquals(10m, messageData5TVLine[0].CustomsValueKRW);
			AssertEquals(20m, messageData5TVLine[0].DutyAmount);
			AssertEquals(50m, messageData5TVLine[0].VAT);
			AssertEquals(90m, messageData5TVLine[0].IndividualConsumptionTax);
			AssertEquals(70m, messageData5TVLine[0].LiquorTax);
			AssertEquals(25m, messageData5TVLine[0].TransportationTax);
			AssertEquals(110m, messageData5TVLine[0].SpecialAgriculturalTax);
			AssertEquals(80m, messageData5TVLine[0].EducationTax);
			AssertEquals(10m, messageData5TVLine[0].LinesTotalTax);

			AssertEquals(new ZShort(12), messageData5TVLine[1].EntryLineNo);
			AssertEquals((int)Constants.BeforeOrAfterAmendment.After, messageData5TVLine[1].BeforeOrAfterAmendmentIndicator);
			AssertEquals("2", messageData5TVLine[1].HSCode);
			AssertEquals("품명2", messageData5TVLine[1].HSCodeDescription);
			AssertEquals(25m, messageData5TVLine[1].DutyRate);
			AssertEquals(20m, messageData5TVLine[1].CustomsValueKRW);
			AssertEquals(30m, messageData5TVLine[1].DutyAmount);
			AssertEquals(90m, messageData5TVLine[1].VAT);
			AssertEquals(170m, messageData5TVLine[1].IndividualConsumptionTax);
			AssertEquals(130m, messageData5TVLine[1].LiquorTax);
			AssertEquals(40m, messageData5TVLine[1].TransportationTax);
			AssertEquals(210m, messageData5TVLine[1].SpecialAgriculturalTax);
			AssertEquals(150m, messageData5TVLine[1].EducationTax);
			AssertEquals(20m, messageData5TVLine[1].LinesTotalTax);

			AssertEquals(new ZShort(13), messageData5TVLine[2].EntryLineNo);
			AssertEquals((int)Constants.BeforeOrAfterAmendment.Before, messageData5TVLine[2].BeforeOrAfterAmendmentIndicator);
			AssertEquals("3", messageData5TVLine[2].HSCode);
			AssertEquals("품명3", messageData5TVLine[2].HSCodeDescription);
			AssertEquals(15m, messageData5TVLine[2].DutyRate);
			AssertEquals(10m, messageData5TVLine[2].CustomsValueKRW);
			AssertEquals(20m, messageData5TVLine[2].DutyAmount);
			AssertEquals(50m, messageData5TVLine[2].VAT);
			AssertEquals(90m, messageData5TVLine[2].IndividualConsumptionTax);
			AssertEquals(70m, messageData5TVLine[2].LiquorTax);
			AssertEquals(25m, messageData5TVLine[2].TransportationTax);
			AssertEquals(110m, messageData5TVLine[2].SpecialAgriculturalTax);
			AssertEquals(80m, messageData5TVLine[2].EducationTax);
			AssertEquals(10m, messageData5TVLine[2].LinesTotalTax);

			AssertEquals(new ZShort(13), messageData5TVLine[3].EntryLineNo);
			AssertEquals((int)Constants.BeforeOrAfterAmendment.After, messageData5TVLine[3].BeforeOrAfterAmendmentIndicator);
			AssertEquals("4", messageData5TVLine[3].HSCode);
			AssertEquals("품명4", messageData5TVLine[3].HSCodeDescription);
			AssertEquals(25m, messageData5TVLine[3].DutyRate);
			AssertEquals(20m, messageData5TVLine[3].CustomsValueKRW);
			AssertEquals(30m, messageData5TVLine[3].DutyAmount);
			AssertEquals(90m, messageData5TVLine[3].VAT);
			AssertEquals(170m, messageData5TVLine[3].IndividualConsumptionTax);
			AssertEquals(130m, messageData5TVLine[3].LiquorTax);
			AssertEquals(40m, messageData5TVLine[3].TransportationTax);
			AssertEquals(210m, messageData5TVLine[3].SpecialAgriculturalTax);
			AssertEquals(150m, messageData5TVLine[3].EducationTax);
			AssertEquals(20m, messageData5TVLine[3].LinesTotalTax);

			AssertEquals(20m, wrapper.MessageData5TV.TotalDutyDifferenceAmount);
			AssertEquals(80m, wrapper.MessageData5TV.TotalDifferenceVAT);
			AssertEquals(160m, wrapper.MessageData5TV.TotalIndividualConsumptionDifferenceTax);
			AssertEquals(120m, wrapper.MessageData5TV.TotalLiquorDifferenceTax);
			AssertEquals(30m, wrapper.MessageData5TV.TotalTransportationDifferenceTax);
			AssertEquals(200m, wrapper.MessageData5TV.TotalSpecialAgriculturalDifferenceTax);
			AssertEquals(140m, wrapper.MessageData5TV.TotalEducationDifferenceTax);
		}

		public void Test5WNMessageData()
		{
			var incomingMessage = CreateMessage("GOVCBR5WN_Document.xml", ElectronicDocumentTypeList.Codes._5WN);
			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5WN);
			AssertEquals("인천세관 조사(납세)심사과", wrapper.MessageData5WN.CustomsDepartment);
			AssertEquals("임승현", wrapper.MessageData5WN.CustomsPersonName);
			AssertEquals("김진갑", wrapper.MessageData5WN.CustomsPrimaryOfficial);
			AssertEquals("032-722-4062", wrapper.MessageData5WN.CustomsPersonPhoneNumber);
			AssertEquals("1234520000045M", wrapper.MessageData5WN.ImportDeclarationNumber);
			AssertEquals(new ZDate("2020 -01-02"), wrapper.MessageData5WN.DeclarationDate);

			AssertEquals(-2756130m, wrapper.MessageData5WN.DutyAmountDifference);
			AssertEquals(100m, wrapper.MessageData5WN.SpecialConsumptionTaxDifference);
			AssertEquals(200m, wrapper.MessageData5WN.TransportationTaxDifference);
			AssertEquals(300m, wrapper.MessageData5WN.LiquorTaxDifference);
			AssertEquals(400m, wrapper.MessageData5WN.EducationTaxDifference);
			AssertEquals(500m, wrapper.MessageData5WN.AgriculturalTaxDifference);
			AssertEquals(-275610m, wrapper.MessageData5WN.VATDifference);
			AssertEquals(10m, wrapper.MessageData5WN.PenaltyOnLateDeclarationDifference);
			AssertEquals(20m, wrapper.MessageData5WN.PenaltyOnMissedDeclarationForPersonalItemsDifference);
			AssertEquals(30m, wrapper.MessageData5WN.PenaltyOnDutyForUnderDeclarationDifference);
			AssertEquals(40m, wrapper.MessageData5WN.PenaltyOnDomesticTaxForUnderDeclarationDifference);
			AssertEquals(50m, wrapper.MessageData5WN.PenaltyOnDutyForLatePaymentDifference);
			AssertEquals(60m, wrapper.MessageData5WN.PenaltyOnDomesticTaxForLatePaymentDifference);
			AssertEquals(70m, wrapper.MessageData5WN.PenaltyOnMissedDeclarationDifference);
			AssertEquals(80m, wrapper.MessageData5WN.PenaltyOnNonCompliantDeclarationDifference);
			AssertEquals(90m, wrapper.MessageData5WN.PenaltyOnBreachOfReExportationDifference);
			AssertEquals(1000m, wrapper.MessageData5WN.PenaltyOnOverdrawbackDifference);

			var linesData5WN = wrapper.MessageData5WN.Lines;
			AssertEquals(1, linesData5WN.Count);
			AssertEquals(new ZShort(001), linesData5WN[0].EntryLineNo);

			AssertEquals("9401.90-2000", linesData5WN[0].HSCodeBefore);
			AssertEquals("PARTS OF AUTOMOTIVE SEAT", linesData5WN[0].HSDescriptionBefore);
			AssertEquals("88051-LF540 CUSH PANEL ASSY", linesData5WN[0].DescriptionBefore);
			AssertEquals(10m, linesData5WN[0].QuantityBefore);
			AssertEquals(10m, linesData5WN[0].AdditionalTariffAmountBefore);

			AssertEquals("9401.90-2222", linesData5WN[0].HSCodeAfter);
			AssertEquals("PARTS OF AUTOMOTIVE SEAT2", linesData5WN[0].HSDescriptionAfter);
			AssertEquals("88051-LF540 CUSH PANEL ASSY2", linesData5WN[0].DescriptionAfter);
			AssertEquals(20m, linesData5WN[0].QuantityAfter);
			AssertEquals(100m, linesData5WN[0].AdditionalTariffAmountAfter);

			var detailsData = linesData5WN[0].DutyTaxDetails;
			AssertEquals(16, detailsData.Count);
			AssertEquals("CUD", detailsData["\"CUD, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"CUD, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(57419430m, detailsData["\"CUD, 1\""].BaseValue);
			AssertEquals(8m, detailsData["\"CUD, 1\""].DutyTaxRate);
			AssertEquals(0m, detailsData["\"CUD, 1\""].ReducedOrExemptAmount);
			AssertEquals(4593554m, detailsData["\"CUD, 1\""].DutyTaxAmount);

			AssertEquals("IND", detailsData["\"IND, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"IND, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(3m, detailsData["\"IND, 1\""].BaseValue);
			AssertEquals(1m, detailsData["\"IND, 1\""].DutyTaxRate);
			AssertEquals(2m, detailsData["\"IND, 1\""].ReducedOrExemptAmount);
			AssertEquals(4m, detailsData["\"IND, 1\""].DutyTaxAmount);

			AssertEquals("ENV", detailsData["\"ENV, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"ENV, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(7m, detailsData["\"ENV, 1\""].BaseValue);
			AssertEquals(5m, detailsData["\"ENV, 1\""].DutyTaxRate);
			AssertEquals(6m, detailsData["\"ENV, 1\""].ReducedOrExemptAmount);
			AssertEquals(8m, detailsData["\"ENV, 1\""].DutyTaxAmount);

			AssertEquals("ACT", detailsData["\"ACT, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"ACT, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(11m, detailsData["\"ACT, 1\""].BaseValue);
			AssertEquals(9m, detailsData["\"ACT, 1\""].DutyTaxRate);
			AssertEquals(10m, detailsData["\"ACT, 1\""].ReducedOrExemptAmount);
			AssertEquals(12m, detailsData["\"ACT, 1\""].DutyTaxAmount);

			AssertEquals("5AB", detailsData["\"5AB, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"5AB, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(15m, detailsData["\"5AB, 1\""].BaseValue);
			AssertEquals(13m, detailsData["\"5AB, 1\""].DutyTaxRate);
			AssertEquals(14m, detailsData["\"5AB, 1\""].ReducedOrExemptAmount);
			AssertEquals(16m, detailsData["\"5AB, 1\""].DutyTaxAmount);

			AssertEquals("5DC", detailsData["\"5DC, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"5DC, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(19m, detailsData["\"5DC, 1\""].BaseValue);
			AssertEquals(17m, detailsData["\"5DC, 1\""].DutyTaxRate);
			AssertEquals(18m, detailsData["\"5DC, 1\""].ReducedOrExemptAmount);
			AssertEquals(20m, detailsData["\"5DC, 1\""].DutyTaxAmount);

			AssertEquals("5CL", detailsData["\"5CL, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"5CL, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(23m, detailsData["\"5CL, 1\""].BaseValue);
			AssertEquals(21m, detailsData["\"5CL, 1\""].DutyTaxRate);
			AssertEquals(22m, detailsData["\"5CL, 1\""].ReducedOrExemptAmount);
			AssertEquals(24m, detailsData["\"5CL, 1\""].DutyTaxAmount);

			AssertEquals("VAT", detailsData["\"VAT, 1\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.Before, detailsData["\"VAT, 1\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(62012984m, detailsData["\"VAT, 1\""].BaseValue);
			AssertEquals(25m, detailsData["\"VAT, 1\""].DutyTaxRate);
			AssertEquals(26m, detailsData["\"VAT, 1\""].ReducedOrExemptAmount);
			AssertEquals(6201298m, detailsData["\"VAT, 1\""].DutyTaxAmount);

			AssertEquals("CUD", detailsData["\"CUD, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"CUD, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(57419430m, detailsData["\"CUD, 2\""].BaseValue);
			AssertEquals(3.2m, detailsData["\"CUD, 2\""].DutyTaxRate);
			AssertEquals(0m, detailsData["\"CUD, 2\""].ReducedOrExemptAmount);
			AssertEquals(1837421m, detailsData["\"CUD, 2\""].DutyTaxAmount);
			AssertEquals(-2756133m, detailsData["\"CUD, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("IND", detailsData["\"IND, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"IND, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(13m, detailsData["\"IND, 2\""].BaseValue);
			AssertEquals(11m, detailsData["\"IND, 2\""].DutyTaxRate);
			AssertEquals(12m, detailsData["\"IND, 2\""].ReducedOrExemptAmount);
			AssertEquals(14m, detailsData["\"IND, 2\""].DutyTaxAmount);
			AssertEquals(-10m, detailsData["\"IND, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("ENV", detailsData["\"ENV, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"ENV, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(17m, detailsData["\"ENV, 2\""].BaseValue);
			AssertEquals(15m, detailsData["\"ENV, 2\""].DutyTaxRate);
			AssertEquals(16m, detailsData["\"ENV, 2\""].ReducedOrExemptAmount);
			AssertEquals(28m, detailsData["\"ENV, 2\""].DutyTaxAmount);
			AssertEquals(-20m, detailsData["\"ENV, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("ACT", detailsData["\"ACT, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"ACT, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(21m, detailsData["\"ACT, 2\""].BaseValue);
			AssertEquals(19m, detailsData["\"ACT, 2\""].DutyTaxRate);
			AssertEquals(20m, detailsData["\"ACT, 2\""].ReducedOrExemptAmount);
			AssertEquals(42m, detailsData["\"ACT, 2\""].DutyTaxAmount);
			AssertEquals(-30m, detailsData["\"ACT, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("5AB", detailsData["\"5AB, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"5AB, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(25m, detailsData["\"5AB, 2\""].BaseValue);
			AssertEquals(23m, detailsData["\"5AB, 2\""].DutyTaxRate);
			AssertEquals(24m, detailsData["\"5AB, 2\""].ReducedOrExemptAmount);
			AssertEquals(56m, detailsData["\"5AB, 2\""].DutyTaxAmount);
			AssertEquals(-40m, detailsData["\"5AB, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("5DC", detailsData["\"5DC, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"5DC, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(29m, detailsData["\"5DC, 2\""].BaseValue);
			AssertEquals(27m, detailsData["\"5DC, 2\""].DutyTaxRate);
			AssertEquals(28m, detailsData["\"5DC, 2\""].ReducedOrExemptAmount);
			AssertEquals(70m, detailsData["\"5DC, 2\""].DutyTaxAmount);
			AssertEquals(-50m, detailsData["\"5DC, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("5CL", detailsData["\"5CL, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"5CL, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(33m, detailsData["\"5CL, 2\""].BaseValue);
			AssertEquals(31m, detailsData["\"5CL, 2\""].DutyTaxRate);
			AssertEquals(32m, detailsData["\"5CL, 2\""].ReducedOrExemptAmount);
			AssertEquals(84m, detailsData["\"5CL, 2\""].DutyTaxAmount);
			AssertEquals(-60m, detailsData["\"5CL, 2\""].IncreaseDutyTaxAmount);

			AssertEquals("VAT", detailsData["\"VAT, 2\""].DutyTaxType);
			AssertEquals(Constants.BeforeOrAfterAmendment.After, detailsData["\"VAT, 2\""].BeforeOrAfterAmendmentIndicator);
			AssertEquals(59256851m, detailsData["\"VAT, 2\""].BaseValue);
			AssertEquals(1.2m, detailsData["\"VAT, 2\""].DutyTaxRate);
			AssertEquals(10000m, detailsData["\"VAT, 2\""].ReducedOrExemptAmount);
			AssertEquals(5925685m, detailsData["\"VAT, 2\""].DutyTaxAmount);
			AssertEquals(-275613m, detailsData["\"VAT, 2\""].IncreaseDutyTaxAmount);
		}

		public void Test5WNMessageDataWhenMessageIsNot5WN()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5WN);
		}

		public void Test5GUMessageData()
		{
			var incomingMessage = CreateMessage("GOVCBR5GU_RealData.xml", ElectronicDocumentTypeList.Codes._5GU);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "12", "수입2과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9608911000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9608911234", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS2");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "9608915678", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS3");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 0.8573m, uSD);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today, ZDateTime.Today, 0.75m, uSD);

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var billRef = declaration.DeclarationRefs.AddNew();
			billRef.J3_ReferenceType = Constants.MRN;
			billRef.J3_ReferenceNumber = "01KE0766SS2";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.N;
			masterBill.CU_BillSeqNo = "0010";
			masterBill.CU_BillNum = "DBSC96100123AB01";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;
			houseBill.CU_BillSeqNo = "003";
			houseBill.CU_HBSplitDecInd = HouseBillSplitDeclarationIndicatorCodeList.Codes.Y;
			houseBill.CU_HBSplitDecReasonCode = "A";
			houseBill.HBSplitDecReasonRemark = "a";
			houseBill.CU_BillNum = "HJSC98100123AB01";

			var line1 = entry.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			line1.CL_CustomsValue = 250m;
			line1.CL_AdValoremTariff = "9608911000";

			var line2 = entry.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			line2.CL_CustomsValue = 200m;
			line2.CL_AdValoremTariff = "9608911234";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JZ_ImportCargoManagementNumber = "01KE0766SS200100003";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9608911000";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine1.JI_CustomsQuantity = 3m;
			invoiceLine1.JI_CustomsUnitQty = "PC";
			invoiceLine1.JI_NetWeight = 1500m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1.JI_CL = line1.PK;
			invoiceLine1.CertificateOfOriginIssueStatus = ZString.Empty;
			invoiceLine1.CertificateOfOriginData.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9608911234";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2.JI_CustomsQuantity = 4m;
			invoiceLine2.JI_CustomsUnitQty = "PC";
			invoiceLine2.JI_NetWeight = 3m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_CL = line2.PK;
			invoiceLine2.CertificateOfOriginIssueStatus = ZString.Empty;
			invoiceLine2.CertificateOfOriginData.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			Factory.Save();

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNotNull(wrapper.MessageData5GU);
			AssertEquals("01KE0766SS2-0010-0003", entry.FormattedCargoManagementNo);
			AssertEquals("문서번호", "040-C2-시정-21-01363", wrapper.MessageData5GU.ComplementNumber);
			AssertEquals("일자", new ZDate(2021, 07, 08), wrapper.MessageData5GU.CustomsRegistryDate);

			AssertEquals("총란수", 2, entry.MergedLines.Count);
			AssertEquals("총란수", "002", entry.FormattedTotalEntryLineCount);
			AssertEquals("1란 란번호", "1", wrapper.MessageData5GU.Corrections[0].EntryLineNo.ToString());
			AssertEquals("1란 품명/규격", "PEN NIBS1", wrapper.MessageData5GU.Corrections[0].TariffDescription);
			AssertEquals("1란 세번", "9608.91-1000", wrapper.MessageData5GU.Corrections[0].Tariff);
			AssertEquals("1란 원산지", "JP", wrapper.MessageData5GU.Corrections[0].CountryOfOrigin);
			AssertEquals("1란 수량", 3m, wrapper.MessageData5GU.Corrections[0].CustomsQuantity);
			AssertEquals("1란 수량단위", "PC", wrapper.MessageData5GU.Corrections[0].CustomsUnitQty);
			AssertEquals("1란 중량", 1.5m, wrapper.MessageData5GU.Corrections[0].NetWeightInKG);
			AssertEquals("1란 과세가격", 291m, wrapper.MessageData5GU.Corrections[0].CustomsValueUSD);
			AssertEquals("1란 위반유형 코드", "04", wrapper.MessageData5GU.Corrections[0].ViolationCode);
			AssertEquals("1란 위반유형", "미표시", wrapper.MessageData5GU.Corrections[0].ViolationDescription);
			AssertEquals("1란 위반내용", "원산지 미표시", wrapper.MessageData5GU.Corrections[0].ViolationName);
			AssertEquals("1란 시정밥법", "표시방법기타 : 최소포장 식별 가능한 곳에 MADE IN CHAINA 불멸 잉크로 표기예정 표시단위 : 최소포장", wrapper.MessageData5GU.Corrections[0].CorrectionMethod);

			AssertEquals("2란 란번호", "2", wrapper.MessageData5GU.Corrections[1].EntryLineNo.ToString());
			AssertEquals("2란 품명/규격", "PEN NIBS2", wrapper.MessageData5GU.Corrections[1].TariffDescription);
			AssertEquals("2란 세번", "9608.91-1234", wrapper.MessageData5GU.Corrections[1].Tariff);
			AssertEquals("2란 원산지", "CN", wrapper.MessageData5GU.Corrections[1].CountryOfOrigin);
			AssertEquals("2란 수량", 4m, wrapper.MessageData5GU.Corrections[1].CustomsQuantity);
			AssertEquals("2란 수량", "PC", wrapper.MessageData5GU.Corrections[1].CustomsUnitQty);
			AssertEquals("2란 중량", 3m, wrapper.MessageData5GU.Corrections[1].NetWeightInKG);
			AssertEquals("2란 과세가격", 233m, wrapper.MessageData5GU.Corrections[1].CustomsValueUSD);
			AssertEquals("2란 위반유형 코드", "04", wrapper.MessageData5GU.Corrections[1].ViolationCode);
			AssertEquals("2란 위반유형", "미표시", wrapper.MessageData5GU.Corrections[1].ViolationDescription);
			AssertEquals("2란 위반내용", "원산지 미표시 1회차", wrapper.MessageData5GU.Corrections[1].ViolationName);
			AssertEquals("2란 시정밥법", "기타시정사항 : 최소포장에 불멸잉크로 'MADE IN JAPAN' 표기", wrapper.MessageData5GU.Corrections[1].CorrectionMethod);

			AssertEquals("시정명령기한", new ZDate(2021, 07, 22), wrapper.MessageData5GU.CorrectionOrderDeadline);
			AssertEquals("세관장", "인천세관", wrapper.MessageData5GU.CustomsOffice);
			AssertEquals("담당부서", "수입2과", wrapper.MessageData5GU.CustomsDivision);
			AssertEquals("담당자", "최선희", wrapper.MessageData5GU.CustomsPersonName);
			AssertEquals("전화번호", "032-722-4252", wrapper.MessageData5GU.CustomsPersonPhoneNumber);

			line1.CL_AdValoremTariff = "9608915678";
			wrapper = new EDIMessageWrapper(incomingMessage);
			AssertEquals("1란 세번", "9608.91-1000", wrapper.MessageData5GU.Corrections[0].Tariff);

			import929 = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}
			wrapper = new EDIMessageWrapper(incomingMessage);
			AssertEquals("1란 세번", "9608.91-5678", wrapper.MessageData5GU.Corrections[0].Tariff);
		}

		public void Test5GUMessageDataWhenMessageIsNot5GU()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5GU);
		}

		public void Test5FVMessageData()
		{
			var incomingMessageNormal = CreateMessage("GOVCBR5FV_Normal.xml", ElectronicDocumentTypeList.Codes._5FV);
			var incomingMessageRefund = CreateMessage("GOVCBR5FV_Refund.xml", ElectronicDocumentTypeList.Codes._5FV);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.BusinessNumber, "2118301204");
			helper.CreateCusCodeListAttribute(codeList.PK, Constants.ZZ.CodeListAttributeNames.Address, "서울 강남구 논현동 71");

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "노드슨코리아(주)");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1268110513", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			var orgHeaderWrapper = OrgHeaderWrapper.New(importer);
			orgHeaderWrapper.ZO_TypeOfBusiness = "제조업,도매";
			orgHeaderWrapper.ZO_ItemOfBusiness = "일반산업기기및도장설";

			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessageNormal);
			AssertNotNull(wrapper.MessageData5FV);

			AssertEquals("No.", "10043271", wrapper.MessageData5FV.FormattedTaxInvoiceNumber);
			AssertEquals("(세관명)등록번호", "211-83-01204", wrapper.MessageData5FV.FormattedCustomsOfficeBusinessNumber);
			AssertEquals("(세관명)세관명", "서울세관", wrapper.MessageData5FV.CustomsOffice.ZZD_Description);
			AssertEquals("(세관명)세관주소", "서울 강남구 논현동 71", wrapper.MessageData5FV.CustomsOfficeAddress);
			AssertEquals("수입신고번호", "22926-20-002712M", wrapper.MessageData5FV.FormattedImportDeclarationNumber);
			AssertEquals("(수입자)등록번호", "126-81-10513", wrapper.MessageData5FV.FormattedImporterID);
			AssertEquals("(수입자)상호", "노드슨코리아(주)", wrapper.MessageData5FV.ImporterCompanyName);
			AssertEquals("(수입자)성명", "신현섭", wrapper.MessageData5FV.ImporterRepresentativeName);
			AssertEquals("(수입자)사업장주소", "경기도 성남시 중원구 사기막골로 90 (상대원동)", wrapper.MessageData5FV.ImporterAddressLine);

			AssertEquals("년월일", "2020-08-12", wrapper.MessageData5FV.PaymentDate.ToString(DateFormatType.DateKorean));
			AssertEquals("공란수", "5", wrapper.MessageData5FV.BlankCount);
			AssertEquals("과세표준", 12, wrapper.MessageData5FV.CustomsValueDigits.Length);
			AssertEquals("과세표준", "33043236", string.Join("", wrapper.MessageData5FV.CustomsValueDigits));
			AssertEquals("세액", 11, wrapper.MessageData5FV.TaxDigits.Length);
			AssertEquals("세액", "3304320", string.Join("", wrapper.MessageData5FV.TaxDigits));
			AssertEquals("비고", "0127-010-11-20-0-004530-7", wrapper.MessageData5FV.FormattedNoticeNumber);

			wrapper = new EDIMessageWrapper(incomingMessageRefund);
			AssertNotNull(wrapper.MessageData5FV);

			AssertEquals("비고", "020-75-20-29523", wrapper.MessageData5FV.FormattedRefundApprovalNo);
			AssertEquals("과세표준", 12, wrapper.MessageData5FV.CustomsValueDigits.Length);
			AssertEquals("과세표준", "-69300", string.Join("", wrapper.MessageData5FV.CustomsValueDigits));
			AssertEquals("세액", 11, wrapper.MessageData5FV.TaxDigits.Length);
			AssertEquals("세액", "-6930", string.Join("", wrapper.MessageData5FV.TaxDigits));
		}

		public void Test5FVMessageDataWhenMessageIsNot5FV()
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			var wrapper = new EDIMessageWrapper(incomingMessage);
			AssertNull(wrapper.MessageData5FV);
		}

		RefExchangeRate SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();

			return result;
		}

		EDIMessage CreateMessage(string fileName, string messageType)
		{
			IsIncoming = true;
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			return incomingMessage;
		}

		public void Test5SGMessageData()
		{
			var finalPriceReportByDateExtensionHeaderObj = new FinalPriceReportByDateExtensionHeader(Factory);
			var finalPriceReportByDateExtensionLineObj1 = finalPriceReportByDateExtensionHeaderObj.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportByDateExtensionLineObj1.ImportDeclarationNumber = "0000000000";
			finalPriceReportByDateExtensionLineObj1.ExtensionDate = new ZDateTime("2021-12-28");
			finalPriceReportByDateExtensionLineObj1.ApplicationReason = "TestResult1";

			var finalPriceReportByDateExtensionLineObj2 = finalPriceReportByDateExtensionHeaderObj.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportByDateExtensionLineObj2.ImportDeclarationNumber = "1111111111";
			finalPriceReportByDateExtensionLineObj2.ExtensionDate = new ZDateTime("2021-12-29");
			finalPriceReportByDateExtensionLineObj2.ApplicationReason = "TestResult2";

			var message = Factory.New<EDIMessage>();

			var import5SG = new Import5SGHeaderCreator().Create(finalPriceReportByDateExtensionHeaderObj);
			var result = new GOVCBR5SGMessageBuilder(import5SG).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;
				message.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
			}

			var entry = Factory.New<CusMiscRequestHeader>();
			entry.Messages.Add(message);

			var wrapper = new EDIMessageWrapper(message);

			AssertNotNull(wrapper.MessageData5SG);
			AssertEquals(2, wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines.Count);

			AssertEquals("0000000000", wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[0].ImportDeclarationNumber);
			AssertEquals(new ZDateTime("2021-12-28"), wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[0].ExtensionDate);
			AssertEquals("TestResult1", wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[0].ApplicationReason);

			AssertEquals("1111111111", wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[1].ImportDeclarationNumber);
			AssertEquals(new ZDateTime("2021-12-29"), wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[1].ExtensionDate);
			AssertEquals("TestResult2", wrapper.MessageData5SG.FinalPriceReportByDateExtensionLines[1].ApplicationReason);
		}

		public void TestGOVCBRD72Message()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var messageD72 = entry.Messages.AddNew();
			messageD72.EM_MessageNum = "1";
			messageD72.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			messageD72.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageD72.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			messageD72.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestOutGoingFilesPath, "GOVCBRD72_Result_D1.xml");
			messageD72.EM_MessageData = testMsgFile;

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR99.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ApplicationReference = messageD72.EM_MessageNum;
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-01");
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBRR99_D72.xml");
			messageR99.EM_MessageData = testMsgFile;

			var messageR43 = entry.Messages.AddNew();
			messageR43.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR43.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR43.EM_MessageType = ElectronicDocumentTypeList.Codes._R43;
			messageR43.EM_ApplicationReference = messageD72.EM_MessageNum;
			messageR43.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			messageR43.EM_MessageSubType = ElectronicDocumentTypeList.Codes._D72;
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBRR43_Result_C.xml");
			messageR43.EM_MessageData = testMsgFile;
			Factory.Save();

			var wrapper = new EDIMessageWrapper(messageD72);
			AssertEquals(1, wrapper.GOVCBRD72Message.SequenceNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, wrapper.GOVCBRD72Message.MessageStatus);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, wrapper.GOVCBRD72Message.MessageStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), wrapper.GOVCBRD72Message.AcceptedDate);
			AssertEquals(new ZDateTime("2022-01-12"), wrapper.GOVCBRD72Message.DecisionDate);
			AssertEquals("승인통보", wrapper.GOVCBRD72Message.NoticeTypeDescription);
			AssertEquals(new ZDateTime("2021-03-31"), wrapper.GOVCBRD72Message.BeforeReExportScheduledDate);
			AssertEquals(new ZDateTime("2022-01-12"), wrapper.GOVCBRD72Message.AfterReExportScheduledDate);
			AssertEquals("주문 수집 일정 변동에 따른 재수출기한 연장 신청", wrapper.GOVCBRD72Message.ReasonDescription);
		}
		
		public void TestGOVCBR5BBMessage()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var message5BB = entry.Messages.AddNew();
			message5BB.EM_MessageNum = "1";
			message5BB.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5BB.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5BB.EM_MessageType = ElectronicDocumentTypeList.Codes._5BB;
			message5BB.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestOutGoingFilesPath, "GOVCBR5BB_Update.xml");
			message5BB.EM_MessageData = testMsgFile;

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR99.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ApplicationReference = message5BB.EM_MessageNum;
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-01");
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBRR99_5BB.xml");
			messageR99.EM_MessageData = testMsgFile;

			var message5BC = entry.Messages.AddNew();
			message5BC.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5BC.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5BC.EM_MessageType = ElectronicDocumentTypeList.Codes._5BC;
			message5BC.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BB;
			message5BC.EM_ApplicationReference = message5BB.EM_MessageNum;
			message5BC.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, "GOVCBR5BC_AmendTypeIsA.xml");
			message5BC.EM_MessageData = testMsgFile;
			Factory.Save();

			var wrapper = new EDIMessageWrapper(message5BB);
			AssertEquals(1, wrapper.GOVCBR5BBMessage.SequenceNo);
			AssertEquals("ANT", wrapper.GOVCBR5BBMessage.MessageOrEntryStatus);
			AssertEquals("승인통보", wrapper.GOVCBR5BBMessage.MesageOrEntryStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), wrapper.GOVCBR5BBMessage.AcceptedDate);
			AssertEquals(new ZDateTime("2021-05-06"), wrapper.GOVCBR5BBMessage.ReviewDate);
			AssertEquals("승인통보", wrapper.GOVCBR5BBMessage.ReviewResultDescription);
			AssertEquals("기재오류로 인한 정정", wrapper.GOVCBR5BBMessage.AmendReasonDescription);
		}

		public void Test5UAMessageData()
		{
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UA_D1.xml");
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = messageText;
			Factory.Save();

			var wrapper = new EDIMessageWrapper(outgoingMessage);
			AssertNotNull(wrapper.MessageData5UA);
			AssertEquals(new ZDateTime(2021, 04, 01), wrapper.MessageData5UA.AmendmentDeclarationDate);
			AssertEquals(1, wrapper.MessageData5UA.AmendmentVersionNo);
			AssertEquals(PenaltyExemptionCodeList.Codes.A, wrapper.MessageData5UA.PenaltyType);
			AssertEquals(PenaltyExemptionCodeList.Descriptions.A, wrapper.MessageData5UA.PenaltyTypeDescription);
			AssertEquals(PenaltyExemptionReasonCodeList.Codes.B5, wrapper.MessageData5UA.PenaltyExemptionReasonsCode);
			AssertEquals(PenaltyExemptionReasonCodeList.Descriptions.B5, wrapper.MessageData5UA.PenaltyExemptionReasonsCodeDescription);
			AssertEquals(100m, wrapper.MessageData5UA.PenaltyExemptionAmount);
		}
		public void TestCaptions()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(EDIMessageWrapper), nameof(EDIMessageWrapper.ApplicationReference), false, attribute => attribute.Caption == "Version No.");
		}

		public void Test5ULMessageData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;
			entry.CH_CEI_Instruction = instruction.PK;

			var supporting = Factory.New<CusSupportingInfo>();
			supporting.CSI_Type = ElectronicDocumentTypeList.Codes._5UL;
			supporting.CSI_ReferenceNumber = "229262000043U";
			supporting.CSI_ReferenceNumber2 = "1234567890123456789";
			supporting.CSI_ParentID = instruction.PK;
			supporting.CSI_ParentTableCode = instruction.TablePrefix;

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageOwner = "229262000043U";
			outgoingMessage.EM_MessageNum = "1";
			entry.Messages.Add(outgoingMessage);

			Factory.Save();

			var wrapper = new EDIMessageWrapper(outgoingMessage);
			AssertNotNull(wrapper.MessageData5UL);
			AssertEquals("22926-20-00043U", wrapper.MessageData5UL.RefundDeclarationNumber);
		}

		const string TestOutGoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
		const string TestImcomingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
