using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EntrySnapshotWrapperTest : TestCaseWithFactory
	{
		public void TestImportEntrySnapshotData()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "나대표", true);
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1028142299" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);

			var payer = Factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "Test";
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1248105504", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "4208092046311", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DateOfArrival = new ZDate(2021, 10, 10);
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNull("ImportEntryWrapper does not exist", wrapper.ImportEntryWrapper);

			CreateImportEntrySnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNotNull("ImportEntryHeader exists", wrapper.ImportEntryWrapper);
			AssertEquals(new ZDate(2021, 10, 10), wrapper.ImportEntryWrapper.Header.ArrivalDateAtDischargePort);

			importer.Contacts[0].OC_ContactName = "홍길동";

			AssertEquals("나대표", wrapper.ImportEntryWrapper.Importer.RepresentativeName);
			AssertEquals("420809-2046311", wrapper.ImportEntryWrapper.PayerFormattedKoreanRegNoForResident);

			declaration.JE_DateOfArrival = new ZDate(2021, 11, 11);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			entry = declaration.CustomsEntryHeaders[0];
			CreateImportEntrySnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertEquals(new ZDate(2021, 11, 11), wrapper.ImportEntryWrapper.Header.ArrivalDateAtDischargePort);
		}

		void CreateImportEntrySnapshot(CusEntryHeader entry)
		{
			var header = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			Factory.Save();
		}

		public void TestExportEntrySnapshotData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = "A";
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNull("ExportEntryHeader does not exist", wrapper.ExportEntryWrapper);

			CreateExportEntrySnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNotNull("ExportEntryHeader exists", wrapper.ExportEntryWrapper);
			AssertEquals("A", wrapper.ExportEntryWrapper.Header.ExportTypeCode);

			declaration.JE_MessageSubType = "B";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			entry = declaration.CustomsEntryHeaders[0];
			CreateExportEntrySnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertEquals("B", wrapper.ExportEntryWrapper.Header.ExportTypeCode);
		}

		void CreateExportEntrySnapshot(CusEntryHeader entry)
		{
			var header = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				Factory.Save();
			}
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
			Factory.Save();
		}

		public void Test5ULSnapshotData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "5UL";
			entryNum.CE_EntryNum = "1111";
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5UL, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNull("5UL does not exist", wrapper.DataProvider5UL);

			new TestDataSetupHelper(Factory).Create5ULSnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5UL, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNotNull("5UL exists", wrapper.DataProvider5UL);
			AssertEquals("1111", wrapper.DataProvider5UL.Header.RefundDeclarationNumber);

			entryNum.CE_EntryNum = "2222";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			entry = declaration.CustomsEntryHeaders[0];
			new TestDataSetupHelper(Factory).Create5ULSnapshot(entry);
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5UL, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertEquals("2222", wrapper.DataProvider5UL.Header.RefundDeclarationNumber);
		}

		public void Test5SCDHRSnapshotData()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntry();

			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNull("5SC does not exist", wrapper.FTAHeader);

			var header = new ImportFTACreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC);
				Factory.Save();
			}

			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNotNull("5SC exists", wrapper.FTAHeader);
			AssertEquals(2, wrapper.FTAHeader.FTALineItems.Count);
			AssertEquals(1, wrapper.FTAHeader.FTALineItems[0].FirstLine.EntryLineNo);
			AssertEquals(2, wrapper.FTAHeader.FTALineItems[0].SecondLine.EntryLineNo);
			AssertEquals(3, wrapper.FTAHeader.FTALineItems[0].ThirdLine.EntryLineNo);
			AssertEquals(4, wrapper.FTAHeader.FTALineItems[1].FirstLine.EntryLineNo);
			AssertEquals(5, wrapper.FTAHeader.FTALineItems[1].SecondLine.EntryLineNo);
			AssertEquals(0, wrapper.FTAHeader.FTALineItems[1].ThirdLine.EntryLineNo);
			AssertEquals(0, wrapper.FTAHeader.DHRInvoiceLineItems.Count);

			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._DHR, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNull("DHR does not exist", wrapper.FTAHeader);

			var headerDHR = new ImportDHRCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(headerDHR))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR);
				Factory.Save();
			}
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._DHR, EntrySnapshotStatus.Lodged);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertNotNull("DHR exists", wrapper.FTAHeader);
			AssertEquals(2, wrapper.FTAHeader.FTALineItems.Count);
			AssertEquals(1, wrapper.FTAHeader.FTALineItems[0].FirstLine.EntryLineNo);
			AssertEquals(2, wrapper.FTAHeader.FTALineItems[0].SecondLine.EntryLineNo);
			AssertEquals(3, wrapper.FTAHeader.FTALineItems[0].ThirdLine.EntryLineNo);
			AssertEquals(4, wrapper.FTAHeader.FTALineItems[1].FirstLine.EntryLineNo);
			AssertEquals(5, wrapper.FTAHeader.FTALineItems[1].SecondLine.EntryLineNo);
			AssertEquals(0, wrapper.FTAHeader.FTALineItems[1].ThirdLine.EntryLineNo);
			AssertEquals(5, wrapper.FTAHeader.DHRInvoiceLineItems.Count);
			AssertEquals(1, wrapper.FTAHeader.DHRInvoiceLineItems[0].invoiceLine.InvoiceLineNo);
			AssertEquals(2, wrapper.FTAHeader.DHRInvoiceLineItems[1].invoiceLine.InvoiceLineNo);
			AssertEquals(3, wrapper.FTAHeader.DHRInvoiceLineItems[2].invoiceLine.InvoiceLineNo);
			AssertEquals(4, wrapper.FTAHeader.DHRInvoiceLineItems[3].invoiceLine.InvoiceLineNo);
			AssertEquals(5, wrapper.FTAHeader.DHRInvoiceLineItems[4].invoiceLine.InvoiceLineNo);
		}

		public void TestEntryLineObjects5FN()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var universalTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, TariffTypes.HarmonizedSystem);
			var universalTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, universalTariffType.PK, "4203400000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제88조제1항제1호 해당물품");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			Create5FNEntryLineData(1, "1234500000");
			Create5FNEntryLineData(2, "1234500001");

			var message1 = entry.Messages.AddNew();
			message1.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message1.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(EntrySnapshotWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", "GOVCBR5FN_D4.xml");
			message1.EM_MessageText = messageText;

			var message2 = entry.Messages.AddNew();
			message2.EM_MessageType = ElectronicDocumentTypeList.Codes._5FN;
			message2.EM_ApplicationReference = "2";
			message2.EM_MessageText = messageText;

			CreateImportEntrySnapshot(entry);
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory);
			AssertEquals(2, wrapper.ImportEntryWrapper.EntryLineObjects5FN.Count);

			var entryLineObjects5FN = wrapper.ImportEntryWrapper.EntryLineObjects5FN.Cast<MessageSendingEntryLineObject>().FirstOrDefault();
			AssertEquals("4203400000", entryLineObjects5FN.HSCode);
			AssertEquals(universalTariff.ZZ1_Description, entryLineObjects5FN.HSDescription);
			AssertEquals("T", entryLineObjects5FN.DutyReduction);
			AssertEquals("N", entryLineObjects5FN.PostClearanceYN);
			AssertEquals("4203.40-0000", entryLineObjects5FN.FormattedHSCode);
			AssertEquals("서울시 강남구 도산대로 458 (청담동,리츠타워 701호,801호) 리츠타워 701호,801호)", entryLineObjects5FN.GoodsLocationAddressLine);
			AssertEquals("02-513-3220", entryLineObjects5FN.GoodsLocationTelNo);
			AssertEquals("060-62", entryLineObjects5FN.FormattedGoodsLocationPostcode);
			AssertEquals("모델명", entryLineObjects5FN.ModelName);
			AssertEquals("123123112", entryLineObjects5FN.SerialNumber);
			AssertEquals("주문수집 후 재수출 예정", entryLineObjects5FN.UseCodeDescription);
			AssertEquals("020", entryLineObjects5FN.CustomsOffice);
			AssertEquals("040", entryLineObjects5FN.ReExportCustomsOffice);
			AssertEquals("FR", entryLineObjects5FN.DestinationCountry);
			AssertEquals("01", entryLineObjects5FN.GroupNumber);
			AssertEquals("45", entryLineObjects5FN.SequenceNumber);
			AssertEquals("12", entryLineObjects5FN.ItemNumber);
			AssertEquals(new ZDateTime(2020, 10, 31), entryLineObjects5FN.EstimateDate);

			void Create5FNEntryLineData(short lineNumber, string tariff)
			{
				var entryNumber = entry.EntryNumbers.AddNew();
				entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
				entryNumber.CE_EntryLineReference = $"{lineNumber}";
				entryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
				entryNumber.CE_IssueDate = new ZDateTime(2025, 1, lineNumber);

				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = lineNumber;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Tariff = tariff;
				invoiceLine.JI_SecondaryPreference = "A093000004";
				invoiceLine.JI_IsSpecificUseCode = true;
			}
		}
	}
}
