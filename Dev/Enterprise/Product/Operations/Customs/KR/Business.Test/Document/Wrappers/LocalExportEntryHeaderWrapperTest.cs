using System.Collections.ObjectModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportEntryHeaderWrapper))]
	sealed class LocalExportEntryHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			var wrapper = new LocalExportEntryHeaderWrapper(entry.PK, ElectronicDocumentTypeList.Codes._5DP, entryHeader5DP, Factory);
			wrapper.Decorate(entry);
			AssertSame(entryHeader5DP, wrapper.Header);

			var entryHeader5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			wrapper = new LocalExportEntryHeaderWrapper(entry.PK, ElectronicDocumentTypeList.Codes._5DQ, entryHeader5DQ, Factory);
			wrapper.Decorate(entry);
			AssertSame(entryHeader5DQ, wrapper.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader5DP = new LocalExport5DPEntryHeaderCreator().Create(entry);
			var wrapper = new LocalExportEntryHeaderWrapper(entry.PK, ElectronicDocumentTypeList.Codes._5DP, entryHeader5DP, Factory);
			wrapper.Decorate(entry);
			return wrapper;
		}

		public void Test5DPSnapshots()
		{
			LocalExportEntryHeaderSnapshots(ElectronicDocumentTypeList.Codes._5DP);
		}

		public void Test5DQSnapshots()
		{
			LocalExportEntryHeaderSnapshots(ElectronicDocumentTypeList.Codes._5DQ);
		}

		public void Test5DPCurrent()
		{
			LocalExportEntryHeaderCurrent(ElectronicDocumentTypeList.Codes._5DP);
		}

		public void Test5DQCurrent()
		{
			LocalExportEntryHeaderCurrent(ElectronicDocumentTypeList.Codes._5DQ);
		}

		[TestDate(2021, 10, 10)]
		void LocalExportEntryHeaderSnapshots(ZString messageType)
		{
			var entry = CeateEntry(messageType);
			var header = CeateHeader(entry, messageType);

			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				var snapshot = entry.Snapshots.AddNew(messageType);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));

				var wrapper = new EntrySnapshotWrapper(snapshot, Factory).LocalExportEntryWrapper;
				LocalExportEntryHeaderAssert(wrapper, messageType);
			}
		}

		[TestDate(2021, 10, 10)]
		void LocalExportEntryHeaderCurrent(ZString messageType)
		{
			var entry = CeateEntry(messageType);
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			LocalExportEntryHeaderAssert(wrapper, messageType);
		}

		CusEntryHeader CeateEntry(ZString messageType)
		{
			CusEntryHeader entry;
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			}
			else
			{
				entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			}
			CreatgeEdiMessage(entry, messageType);
			return entry;
		}

		LocalExportEntryHeader CeateHeader(CusEntryHeader entry, ZString messageType)
		{
			LocalExportEntryHeader header;
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				header = new LocalExport5DPEntryHeaderCreator().Create(entry);
			}
			else
			{
				header = new LocalExport5DQEntryHeaderCreator().Create(entry);
			}
			return header;
		}

		public void CreatgeEdiMessage(CusEntryHeader entry, ZString messageType)
		{
			ZString rR3FileName = "Wrapper_GOVCBRRR3_" + messageType + ".xml";
			ZString r38FileName = "Wrapper_GOVCBRR38_" + messageType + ".xml";

			CreateOutgoingMessage(entry, messageType);
			CreateIncomingMessage(entry, rR3FileName, ElectronicDocumentTypeList.Codes._RR3);
			CreateIncomingMessage(entry, r38FileName, ElectronicDocumentTypeList.Codes._R38);

			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
		}

		void LocalExportEntryHeaderAssert(LocalExportEntryHeaderWrapper wrapper, ZString messageType)
		{
			if (messageType == ElectronicDocumentTypeList.Codes._5DP)
			{
				LocalExportEntryHeader5DPAssert(wrapper);
			}
			else
			{
				LocalExportEntryHeader5DQAssert(wrapper);
			}
		}

		void LocalExportEntryHeader5DPAssert(LocalExportEntryHeaderWrapper wrapper)
		{
			var entryHeader = wrapper.Header;

			AssertNotNull("ExportEntryHeader exists", wrapper);

			#region Header Assert
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._01, entryHeader.DeclarationType);
			AssertEquals("010", entryHeader.DeclarationCustomsOffice);
			AssertEquals("10", entryHeader.DeclarationCustomsDivision);
			AssertEquals("01023010-보세구역이름", entryHeader.BondedAreaCode);
			AssertEquals("1", entryHeader.GoodsType);
			AssertEquals("1", entryHeader.DrawbackApplicantType);
			AssertEquals(900m, entryHeader.TotalGrossWeight);
			AssertEquals(100, entryHeader.TotalPackages);
			AssertEquals("1083699012345", entryHeader.DeclarationNumber);
			AssertEquals("20130101", entryHeader.DeclarationDate.ToString(DateFormatType.Date));
			#endregion

			#region Line Assert
			var entryLines = new Collection<ILocalExportEntryLine>();
			foreach (var entryLine in entryHeader.EntryLines)
			{
				entryLines.Add(entryLine);
			}

			var entryLine1 = entryLines[0];
			AssertEquals(1, entryLine1.EntryLineNo);
			AssertEquals("1234567890", entryLine1.HSCode);
			AssertEquals("STAINLESS STEEL", entryLine1.InvoiceDescription);
			AssertEquals("000000000", entryLine1.GoodsNo);
			AssertEquals(10000m, entryLine1.NetWeight);
			AssertEquals("KG", entryLine1.QuantityUnit);
			AssertEquals("L172770912345", entryLine1.DocumentNo);
			AssertEquals("1", entryLine1.DocumentType);
			AssertEquals("VL", entryLine1.PackagesType);
			AssertEquals("010151234567001999", entryLine1.PreviousTransactionReferenceNo);
			AssertEquals("01", entryLine1.PreviousTransactionReferenceNoType);
			AssertEquals(1000m, entryLine1.FOBAmount);

			var entryLine2 = entryLines[1];
			AssertEquals(2, entryLine2.EntryLineNo);
			AssertEquals("0987654321", entryLine2.HSCode);
			AssertEquals("STAINLESS STEEL2", entryLine2.InvoiceDescription);
			AssertEquals("1111111111", entryLine2.GoodsNo);
			AssertEquals(2222m, entryLine2.NetWeight);
			AssertEquals("KG", entryLine2.QuantityUnit);
			AssertEquals("L172770925459", entryLine2.DocumentNo);
			AssertEquals("2", entryLine2.DocumentType);
			AssertEquals("VL", entryLine2.PackagesType);
			AssertEquals("999100765432151010", entryLine2.PreviousTransactionReferenceNo);
			AssertEquals("02", entryLine2.PreviousTransactionReferenceNoType);
			AssertEquals(3333m, entryLine2.FOBAmount);
			#endregion

			#region Add Properties
			AssertEquals("01610200093751", wrapper.CustomsReferenceNumber);
			AssertEquals("20201202133905", wrapper.AcceptanceDate.ToString(DateFormatType.DateTime));
			AssertEquals("", wrapper.EntryReleaseDate.ToString(DateFormatType.DateTime));
			AssertEquals("세관담당자:윤동화, 서류제출생략", wrapper.CustomsRemarks);
			AssertEquals("조성은 181796", wrapper.CustomsReviewOfficer);
			AssertEquals("20201120172629", wrapper.CustomsReviewDate.ToString(DateFormatType.DateTime));
			#endregion
		}

		void LocalExportEntryHeader5DQAssert(LocalExportEntryHeaderWrapper wrapper)
		{
			var entryHeader = wrapper.Header;

			AssertNotNull("ExportEntryHeader exists", wrapper);

			#region Header Assert
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._07, entryHeader.DeclarationType);
			AssertEquals(15, entryHeader.CrewCount);
			AssertEquals(24, entryHeader.ScheduledSailingDays);
			AssertEquals("M/V MARIA", entryHeader.FlightNoOrVesselName);
			AssertEquals("20GLKO0080I", entryHeader.MRNNo);
			AssertEquals(ZDate.Invalid, entryHeader.DeclarationDate);
			#endregion

			#region Stevedore Assert
			var stevedores = new Collection<ILocalExportStevedore>();
			foreach (var stevedore in entryHeader.Stevedores)
			{
				stevedores.Add(stevedore);
			}

			var stevedores1 = stevedores[0];
			AssertEquals(1, stevedores1.SequenceNo);
			AssertEquals("홍길동", stevedores1.FullName);
			AssertEquals("19910506", stevedores1.Birthday.ToString(DateFormatType.Date));
			AssertEquals("110001", stevedores1.RoadNameCode);
			AssertEquals("121200", stevedores1.BuildingNumber);
			AssertEquals("43012", stevedores1.Postcode);
			AssertEquals("기본주소", stevedores1.AddressLine1);
			AssertEquals("상세주소", stevedores1.AddressLine2);

			var stevedores2 = stevedores[1];
			AssertEquals(2, stevedores2.SequenceNo);
			AssertEquals("Hong-Gil-Dong", stevedores2.FullName);
			AssertEquals("19910606", stevedores2.Birthday.ToString(DateFormatType.Date));
			#endregion

			#region Line Assert
			var entryLines = new Collection<ILocalExportEntryLine>();
			foreach (var entryLine in entryHeader.EntryLines)
			{
				entryLines.Add(entryLine);
			}

			var entryLine1 = entryLines[0];
			AssertEquals(1, entryLine1.EntryLineNo);
			AssertEquals("1234567890", entryLine1.HSCode);
			AssertEquals("STAINLESS STEEL", entryLine1.InvoiceDescription);
			AssertEquals("물품식별번호", entryLine1.GoodsNo);
			AssertEquals(9999m, entryLine1.Quantity);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine1.QuantityUnit);
			AssertEquals("L172770912345", entryLine1.DocumentNo);
			AssertEquals("1", entryLine1.DocumentType);
			AssertEquals(99, entryLine1.Packages);
			AssertEquals("VL", entryLine1.PackagesType);
			AssertEquals("010151234567001999", entryLine1.PreviousTransactionReferenceNo);
			AssertEquals("01", entryLine1.PreviousTransactionReferenceNoType);
			AssertEquals(1000m, entryLine1.FOBAmount);
			AssertEquals(9999m, entryLine1.NetWeight);
			AssertEquals("20130101", entryLine1.InboundDate.ToString(DateFormatType.Date));

			var entryLine2 = entryLines[1];
			AssertEquals(2, entryLine2.EntryLineNo);
			AssertEquals("1234567891", entryLine2.HSCode);
			AssertEquals("STAINLESS STEEL2", entryLine2.InvoiceDescription);
			AssertEquals("물품식별번호2", entryLine2.GoodsNo);
			AssertEquals(999m, entryLine2.Quantity);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine2.QuantityUnit);
			AssertEquals("L172770925458", entryLine2.DocumentNo);
			AssertEquals("2", entryLine2.DocumentType);
			AssertEquals(9, entryLine2.Packages);
			AssertEquals("VL", entryLine2.PackagesType);
			AssertEquals("010151234567001990", entryLine2.PreviousTransactionReferenceNo);
			AssertEquals("02", entryLine2.PreviousTransactionReferenceNoType);
			AssertEquals(999m, entryLine2.FOBAmount);
			AssertEquals(999m, entryLine2.NetWeight);
			AssertEquals("20130102", entryLine2.InboundDate.ToString(DateFormatType.Date));
			#endregion

			#region Add Properties
			AssertEquals("01610200093751", wrapper.CustomsReferenceNumber);
			AssertEquals("20201202133905", wrapper.AcceptanceDate.ToString(DateFormatType.DateTime));
			AssertEquals("20201120172629", wrapper.EntryReleaseDate.ToString(DateFormatType.DateTime));
			AssertEquals("세관담당자:윤동화, 서류제출생략", wrapper.CustomsRemarks);
			AssertEquals("조성은 181796", wrapper.CustomsReviewOfficer);
			AssertEquals("", wrapper.CustomsReviewDate.ToString(DateFormatType.DateTime));
			#endregion
		}

		void CreateOutgoingMessage(CusEntryHeader entry, string em_messgeType)
		{
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_messgeType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = entry.PK;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_MessageSubType = "";
			outgoingMessage.EM_MessageNum = "1";
		}

		void CreateIncomingMessage(CusEntryHeader entry, string fileName, string messageType)
		{
			var fileReader = new TestFileReader(typeof(LocalExportEntryHeaderWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationReference = "1";
			incomingMessage.EM_MessageText = messageText;
		}

		public void TestLocalExportNewProperties()
		{
			var entry = CeateEntry(ElectronicDocumentTypeList.Codes._5DQ);
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;

			AssertEquals(LocalExportTransactionNatureCodeList.Descriptions._07, wrapper.DeclarationTypeName);
			AssertEquals("41777-21-000030", wrapper.FormattedEntryNumber);
			AssertEquals("016-10-20-009375-1", wrapper.FormattedCustomsReferenceNumber);
			AssertEquals("1", wrapper.WorkingVesselLloydsNumber);
			AssertEquals("1", wrapper.TransportVehicleRegNo);
			AssertEquals(2, wrapper.TotalEntryLineCount);
		}

		public void TestSupplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNull(wrapper.Header.Supplier);

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
			supplier.OH_FullName = "READYKOREA";
			var contact = supplier.Contacts.AddNew();
			contact.OC_ContactName = "RepresentativeName";
			var declarantAllocation = contact.Allocations.AddNew();
			declarantAllocation.PC_Type = OrgConstants.ContactAllocationType.CEOForKRCustoms;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "서울특별시 서초구 서초대로 64길 55";
			supplierAddress.OA_Address2 = "(서초동,준원빌딩3층)";
			declaration.JE_OH_Supplier = supplier.PK;

			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNotNull(wrapper.Header.Supplier);
			AssertEquals("READYKOREA", wrapper.Header.Supplier.CompanyName);
			AssertEquals("RepresentativeName", wrapper.Header.Supplier.RepresentativeName);
			AssertEquals("서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", wrapper.Supplier.AddressDetails);

			supplier.OH_FullName = "READYKOREA NEW";
			contact.OC_ContactName = "홍길동";
			supplierAddress.OA_Address1 = "서울특별시 서초구";
			supplierAddress.OA_Address2 = "";

			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("READYKOREA NEW", wrapper.Header.Supplier.CompanyName);
			AssertEquals("홍길동", wrapper.Header.Supplier.RepresentativeName);
			AssertEquals("서울특별시 서초구", wrapper.Supplier.AddressDetails);
		}

		public void TestExporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNull(wrapper.Header.Exporter);

			var exporter = Factory.New<OrgHeader>();
			declaration.JE_OH_Exporter = exporter.PK;
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNotNull(wrapper.Header.Exporter);

			declaration.JE_OH_Exporter = ZGuid.Empty;
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNull(wrapper.Header.Exporter);
		}

		public void TestManufacturer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNull(wrapper.Header.Manufacturer);

			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
			manufacturer.OH_FullName = "READYKOREA";
			var contact = manufacturer.Contacts.AddNew();
			contact.OC_ContactName = "RepresentativeName";
			var declarantAllocation = contact.Allocations.AddNew();
			declarantAllocation.PC_Type = OrgConstants.ContactAllocationType.CEOForKRCustoms;

			var manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.OA_Address1 = "서울특별시 서초구 서초대로 64길 55";
			manufacturerAddress.OA_Address2 = "(서초동,준원빌딩3층)";
			invoice.JZ_OH_Manufacturer = manufacturer.PK;

			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNotNull(wrapper.Header.Manufacturer);
			AssertEquals("READYKOREA", wrapper.Header.Manufacturer.CompanyName);
			AssertEquals("RepresentativeName", wrapper.Header.Manufacturer.RepresentativeName);
			AssertEquals("서울특별시 서초구 서초대로 64길 55 (서초동,준원빌딩3층)", wrapper.Manufacturer.AddressDetails);

			manufacturer.OH_FullName = "READYKOREA NEW";
			contact.OC_ContactName = "홍길동";
			manufacturerAddress.OA_Address1 = "서울특별시 서초구";
			manufacturerAddress.OA_Address2 = "";

			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("READYKOREA NEW", wrapper.Header.Manufacturer.CompanyName);
			AssertEquals("홍길동", wrapper.Header.Manufacturer.RepresentativeName);
			AssertEquals("서울특별시 서초구", wrapper.Manufacturer.AddressDetails);
		}

		public void TestImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNull(wrapper.Header.Importer);

			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK", "");
			invoice.JZ_OH_Buyer = importer.PK;
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertNotNull(wrapper.Header.Importer);
		}

		public void TestBondedAreaAndTransportMeans()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals(ZString.Empty, wrapper.BondedAreaAndTransportMeans);

			declaration.JE_SubLocationOfGoods = "보세구역이름";

			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("보세구역이름", wrapper.BondedAreaAndTransportMeans);

			var transportMean1 = declaration.TransportMeans.AddNew();
			transportMean1.CY_Order = 1;
			transportMean1.CY_Code = "SAMARIA TEST";
			transportMean1.CY_Data = "서울 허12 3456";

			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("보세구역이름 / 서울 허12 3456", wrapper.BondedAreaAndTransportMeans);

			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "SAMARIA TEST";
			vessel.RV_MalaysiaVesselId = "9182643";

			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("보세구역이름 / 9182643,서울 허12 3456", wrapper.BondedAreaAndTransportMeans);

			declaration.JE_SubLocationOfGoods = ZString.Empty;

			entry = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("9182643,서울 허12 3456", wrapper.BondedAreaAndTransportMeans);
		}

		public void TestPreviousTransactionReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine1.PK;

			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("", wrapper.EntryLineItems[0].PreviousTransactionReferenceWrapperString);

			invoiceLine1.JI_PreviousEntryNumber = "010151234567001999";
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("010151234567001999", wrapper.EntryLineItems[0].PreviousTransactionReferenceWrapperString);

			invoiceLine1.JI_OriginalStateDocType = "01";
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("010151234567001999 (01:수입신고서)", wrapper.EntryLineItems[0].PreviousTransactionReferenceWrapperString);

			invoiceLine1.JI_PreviousEntryNumber = "";
			wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("(01:수입신고서)", wrapper.EntryLineItems[0].PreviousTransactionReferenceWrapperString);
		}

		[TestDate(2022, 02, 03)]
		public void TestGetStmALog()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var log1 = Factory.New<SimpleStmALog>();
			log1.SL_Parent = entry.PK;
			log1.SL_Table = "CusEntryHeader";
			log1.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
			log1.SL_EventTime = new ZDateTime("2022-02-03");
			log1.SL_GS_NKUser = "DEM";
			log1.SL_PostedTimeUtc = ZDateTime.UtcNow;

			var log2 = Factory.New<SimpleStmALog>();
			log2.SL_Parent = entry.PK;
			log2.SL_Table = "CusEntryHeader";
			log2.SL_SE_NKEvent = Events.CustomsEntryStatus.Code;
			log2.SL_EventTime = new ZDateTime("2022-02-02");
			log2.SL_GS_NKUser = "DEM";
			log2.SL_PostedTimeUtc = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;
			AssertEquals("StmAlog SL_EventTime is updated", new ZDateTime("2022-02-02"), wrapper.MostRecentCESLogSLEventTime);
		}

		public TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(LocalExportEntryHeaderWrapperTest)));
		TestFileReader fileReader;

		public TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Incoming";

		class SimpleStmALog : AutoStmALog
		{
			public SimpleStmALog(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}
	}
}
