using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportEntryHeaderWrapper))]
	sealed class ExportEntryHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			var wrapper = new ExportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			wrapper.Decorate(entry);
			AssertSame(entryHeader, wrapper.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader = new ExportEntryHeaderCreator().Create(entry);
			return new ExportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
		}

		[TestDate(2021, 10, 10)]
		public void TestExportEntryHeaderSnapshots()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();

			var header = new ExportEntryHeaderCreator().Create(entry);

			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));

				var wrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;
				ExportEntryHeaderAssert(wrapper);
			}
		}

		[TestDate(2021, 10, 10)]
		public void TestExportEntryHeaderCurrent()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();

			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			ExportEntryHeaderAssert(wrapper);
		}

		void ExportEntryHeaderAssert(ExportEntryHeaderWrapper wrapper)
		{
			var entryHeader = wrapper.Header;

			AssertNotNull("ExportEntryHeader exists", wrapper);
			AssertEquals("6N00221000025X", entryHeader.ExportDeclarationNumber);
			AssertEquals("11", entryHeader.TransactionType);
			AssertEquals("B", entryHeader.ExportTypeCode);
			AssertEquals("130", entryHeader.DeclarationCustomsOffice);
			AssertEquals("10", entryHeader.DeclarationCustomsDivision);
			AssertEquals("HK", entryHeader.CountryOfDestination);
			AssertEquals("KRINC", entryHeader.PortOfLoading);
			AssertEquals("08589", entryHeader.GoodsLocationPostcode);
			AssertEquals("서울 금천구 가산디지털1로 119", entryHeader.GoodsLocationAddress);
			AssertEquals("서브", entryHeader.GoodsLocationAdditionalDetails);
			AssertEquals("02", entryHeader.SouthNorthTradeIdentification);
			AssertEquals("55555", entryHeader.FinalLoadingPlace);
			AssertEquals("99999999", entryHeader.GoodsLocationBondedAreaCode);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.BondedTransportationFromDate);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.BondedTransportationToDate);
			AssertEquals(new ZDateTime(2014, 01, 01), entryHeader.DepartureDate);
			AssertEquals("1", entryHeader.DrawbackApplicantType);
			AssertEquals("H", entryHeader.DeclarationProcedureType);
			AssertEquals("TT", entryHeader.InvoicePaymentTerm);
			AssertEquals("C", entryHeader.ExporterType);
			AssertEquals("N", entryHeader.OutOfHoursDeclarationIndicator);
			AssertEquals("ZZ", entryHeader.ReturnReason);
			AssertEquals("A", entryHeader.ReturnType);
			AssertEquals("O", entryHeader.GoodsStatus);
			AssertEquals("NO", entryHeader.ApplicationForSimpleDrawback);
			AssertEquals(true, entryHeader.ContainerizedIndicator);
			AssertEquals("N", entryHeader.SouthNorthTradeYN);
			AssertEquals("1234567", entryHeader.LCNo);
			AssertEquals("16HJSC0686I00080001", entryHeader.CargoManagement.ImportCargoManagementNumber);
			AssertEquals("10", entryHeader.TransportMode);
			AssertEquals("99999999999999999", entryHeader.UCR);
			AssertEquals("9999999999", entryHeader.LocationIDInBondedArea);
			AssertEquals("6N002", entryHeader.UnipassDeclarantID);
			AssertEquals("나대표", entryHeader.FreightForwarderContactName);
			AssertEquals("KE", entryHeader.CarrierID);
			AssertEquals("KOREAN AIR", entryHeader.ShippingLineOrAirlineName);
			AssertEquals("AA9999", entryHeader.VesselNameOrFlightNo);
			AssertEquals(34587292.99m, entryHeader.TotalCustomsValue);
			AssertEquals(899999999m, entryHeader.Freight);
			AssertEquals(799999999m, entryHeader.Insurance);
			AssertEquals("CFR", entryHeader.Incoterm);
			AssertEquals("USD", entryHeader.Currency);
			AssertEquals(27670m, entryHeader.TotalInvoiceAmount);
			AssertEquals(1300.75m, entryHeader.ExchangeRate);
			AssertEquals("신고인 기재란", entryHeader.DeclarantAdditionalDescription);
			AssertEquals(3m, entryHeader.TotalPackQty);
			AssertEquals("OU", entryHeader.PackType);
			AssertEquals(29600m, entryHeader.TotalGrossWeightInKG);

			#region Organisation
			var declarant = entryHeader.Declarant;
			AssertEquals("레디코리아", declarant.CompanyName);
			AssertEquals("김환태", declarant.RepresentativeName);

			var exporter = entryHeader.Exporter;
			AssertEquals("레디코리아", exporter.CompanyName);
			AssertEquals("레디코리-1-97-1-01-8", wrapper.Exporter.FormattedUnipassIDForOrganization);
			AssertEquals("00000", wrapper.Exporter.OfficeID);

			var manufacturer = entryHeader.Manufacturer;
			AssertEquals("레디코리아", manufacturer.CompanyName);
			AssertEquals("04784", manufacturer.Postcode);

			AssertEquals("레디코리-아-99-9-00-0", wrapper.Manufacturer.FormattedUnipassIDForOrganization);
			AssertEquals("00001", wrapper.Manufacturer.OfficeID);
			AssertEquals("888", entryHeader.IndustrialParkCode);

			var importer = entryHeader.Importer;
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", importer.CompanyName);
			AssertEquals("HKBOARAM0001A", wrapper.Importer.BuyerID);

			var supplier = entryHeader.Supplier;
			AssertEquals("레디코리아", supplier.CompanyName);
			AssertEquals("김택윤", supplier.RepresentativeName);
			AssertEquals("06561", supplier.Postcode);

			AssertEquals("서울특별시 서초구 동광로 41 레디인빌딩", wrapper.Supplier.AddressDetails);
			AssertEquals("레디코리-1-97-1-01-8", wrapper.Supplier.FormattedUnipassIDForOrganization);
			AssertEquals("102-81-42299", wrapper.Supplier.FormattedBusinessRegNo);
			#endregion

			#region Add Properties
			AssertEquals(1084.25m, wrapper.InvoiceCurrencyExchangeRate);
			AssertEquals(26590m, wrapper.TotalCustomsValueUSD);
			AssertEquals("999999999999999", wrapper.FirstContainerNumber);
			AssertEquals("16HJSC0686I-0008-0001", wrapper.FormattedCargoManagementNo);
			#endregion

			#region Add InterFace
			AssertEquals("20211010", wrapper.DeclarationDate.ToString(DateFormatType.Date));
			AssertEquals("20221010", wrapper.ExpectedLoadingDate.ToString(DateFormatType.Date));
			AssertEquals("20211010", wrapper.EntryReleaseDateTime.ToString(DateFormatType.Date));
			AssertEquals("ABCDEF", wrapper.CustomsMessageRemarks);
			AssertEquals("AAAA-BBB", wrapper.ResponsibleCustomsOfficer);
			AssertEquals("20140101", wrapper.ActualLoadingDate.ToString(DateFormatType.Date));
			#endregion
		}

		[TestDate(2021, 10, 10)]
		public void TestFormattedCargoManagementNo()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			var entryHeader = wrapper.Header;

			AssertEquals("16HJSC0686I00080001", entryHeader.CargoManagement.ImportCargoManagementNumber);
			AssertEquals("16HJSC0686I-0008-0001", wrapper.FormattedCargoManagementNo);

			entry.RandomHeader.JZ_ImportCargoManagementNumber = "16HJSC0686I0008";
			wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			entryHeader = wrapper.Header;

			AssertEquals("16HJSC0686I0008", entryHeader.CargoManagement.ImportCargoManagementNumber);
			AssertEquals("16HJSC0686I-0008", wrapper.FormattedCargoManagementNo);
		}

		[TestDate(2021, 10, 10)]
		public void TestExportEntryHeaderDocumentTitle()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			entry.CH_EntryReleaseDate = ZDateTime.Empty;
			entry.Declaration.JE_EntryDate = ZDate.Empty;
			var header = new ExportEntryHeaderCreator().Create(entry);
			header.DeclarationProcedureType = "M";

			var wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("반  송  신  고  서(갑지)", wrapper.DocumentTitle);
			AssertEquals("반  송  신  고  서(을지)", wrapper.SubDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(A)", wrapper.EnglishDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(B)", wrapper.EnglishSubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "D";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("자유무역지역 반출(국외반출)신고서(갑지)", wrapper.DocumentTitle);
			AssertEquals("자유무역지역 반출(국외반출)신고서(을지)", wrapper.SubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("수  출  신  고  서(갑지)", wrapper.DocumentTitle);
			AssertEquals("수  출  신  고  서(을지)", wrapper.SubDocumentTitle);

			entry.CH_EntryReleaseDate = ZDateTime.Today;
			header = new ExportEntryHeaderCreator().Create(entry);
			header.DeclarationProcedureType = "M";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("반송신고수리내역서(적재전, 갑지)", wrapper.DocumentTitle);
			AssertEquals("반송신고수리내역서(적재전, 을지)", wrapper.SubDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(BEFORE LOADING, A)", wrapper.EnglishDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(BEFORE LOADING, B)", wrapper.EnglishSubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "D";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("자유무역지역 반출(국외반출)내역서(적재전, 갑지)", wrapper.DocumentTitle);
			AssertEquals("자유무역지역 반출(국외반출)내역서(적재전, 을지)", wrapper.SubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("수출신고수리내역서(적재전, 갑지)", wrapper.DocumentTitle);
			AssertEquals("수출신고수리내역서(적재전, 을지)", wrapper.SubDocumentTitle);

			entry.Declaration.JE_EntryDate = ZDate.Today;
			header = new ExportEntryHeaderCreator().Create(entry);
			header.DeclarationProcedureType = "M";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("반송신고수리내역서(수출이행, 갑지)", wrapper.DocumentTitle);
			AssertEquals("반송신고수리내역서(수출이행, 을지)", wrapper.SubDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(AFTER EXPORTING, A)", wrapper.EnglishDocumentTitle);
			AssertEquals("EXPORT DECLARATION CERTIFICATE(AFTER EXPORTING, B)", wrapper.EnglishSubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "D";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("자유무역지역 반출(국외반출)내역서(수출이행, 갑지)", wrapper.DocumentTitle);
			AssertEquals("자유무역지역 반출(국외반출)내역서(수출이행, 을지)", wrapper.SubDocumentTitle);

			header.DeclarationProcedureType = "";
			header.ExportTypeCode = "";

			wrapper = new ExportEntryHeaderWrapper(entry.PK, header, Factory);
			wrapper.Decorate(entry);
			AssertEquals("수출신고수리내역서(수출이행, 갑지)", wrapper.DocumentTitle);
			AssertEquals("수출신고수리내역서(수출이행, 을지)", wrapper.SubDocumentTitle);
		}

		[TestDate(2021, 10, 10)]
		public void TestExportEntryHeaderSnapshotsVsCurrent()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();

			var currentWrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			var header = new ExportEntryHeaderCreator().Create(entry);

			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));

				var snapshotWrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;

				AssertEquals(currentWrapper.Header.ExportDeclarationNumber, snapshotWrapper.Header.ExportDeclarationNumber);
				AssertEquals(currentWrapper.Header.TransactionType, snapshotWrapper.Header.TransactionType);
				AssertEquals(currentWrapper.Header.ExportTypeCode, snapshotWrapper.Header.ExportTypeCode);

				entry.Declaration.JE_ExportGoodsType = "20";
				entry.Declaration.JE_MessageSubType = "A";
				currentWrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;

				AssertEquals("20", currentWrapper.Header.TransactionType);
				AssertEquals("A", currentWrapper.Header.ExportTypeCode);

				AssertEquals(currentWrapper.Header.ExportDeclarationNumber, snapshotWrapper.Header.ExportDeclarationNumber);
				AssertNotEquals(currentWrapper.Header.TransactionType, snapshotWrapper.Header.TransactionType);
				AssertNotEquals(currentWrapper.Header.ExportTypeCode, snapshotWrapper.Header.ExportTypeCode);
			}
		}

		public void TestExportVehicleNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			#region invoiceLine
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_SequenceNumber = 2;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_SequenceNumber = 1;
			#endregion

			#region vehicleNumber
			var vehicleNumber1 = invoiceLine2.VehicleNumbers.AddNew();
			vehicleNumber1.CY_Order = 2;
			vehicleNumber1.CY_Data = "KN3HNP6N18K283119";

			var vehicleNumber2 = invoiceLine1.VehicleNumbers.AddNew();
			vehicleNumber2.CY_Order = 1;
			vehicleNumber2.CY_Data = "CCCCZZZ";
			#endregion

			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			AssertEquals("InvoiceLine Should be ordered by JI_SequenceNumber", "01", wrapper.VehicleItems[0].InvoiceLineNo);
			AssertEquals("KN3HNP6N18K283119", wrapper.VehicleItems[0].VehicleNo.VIN);
			AssertEquals("InvoiceLine Should be ordered by JI_SequenceNumber", "02", wrapper.VehicleItems[1].InvoiceLineNo);
			AssertEquals("CCCCZZZ", wrapper.VehicleItems[1].VehicleNo.VIN);
		}

		public void TestPortOfLoadingDescription()
		{
			RefUNLOCO seaPort1 = Factory.New<RefUNLOCO>();
			seaPort1.RL_Code = "XXPUS";
			seaPort1.RL_IATA = "PUS";
			seaPort1.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			RefLanguageText seaPortText1 = Factory.New<RefLanguageText>();
			seaPortText1.RLT_ParentId = seaPort1.PK;
			seaPortText1.RLT_Text = "부산항";
			seaPortText1.RLT_ColumnName = RefUNLOCO.Schema.RL_PortName;
			seaPortText1.RLT_ParentTableCode = "RL";
			seaPortText1.RLT_Language = "KO-KR";
			seaPortText1.RLT_IsSystem = true;

			RefUNLOCO seaPort2 = Factory.New<RefUNLOCO>();
			seaPort2.RL_Code = "XXHIN";
			seaPort2.RL_IATA = "HIN";
			seaPort2.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			RefLanguageText seaPortText2 = Factory.New<RefLanguageText>();
			seaPortText2.RLT_ParentId = seaPort2.PK;
			seaPortText2.RLT_Text = "진주항";
			seaPortText2.RLT_ColumnName = RefUNLOCO.Schema.RL_PortName;
			seaPortText2.RLT_ParentTableCode = "RL";
			seaPortText2.RLT_Language = "KO-KR";
			seaPortText2.RLT_IsSystem = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_RL_NKPortOfLoading = seaPort1.RL_Code;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			AssertEquals(seaPortText1.RLT_Text, wrapper.PortOfLoadingDescription);

			var header = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			var snapshotWrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;
			AssertEquals(seaPortText1.RLT_Text, snapshotWrapper.PortOfLoadingDescription);

			declaration.JE_RL_NKPortOfLoading = seaPort2.RL_Code;
			AssertEquals(seaPortText1.RLT_Text, snapshotWrapper.PortOfLoadingDescription);

			var amendmentHeader = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(amendmentHeader))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			snapshotWrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;
			AssertEquals(seaPortText2.RLT_Text, snapshotWrapper.PortOfLoadingDescription);
		}

		public void TestContainerNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "11111111111";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "TEST1234567";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "ZZZZZZZZZZZ";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			var export830 = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;
			AssertEquals("11111111111", wrapper.FirstContainerNumber);

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[2].IsForInvoiceLine = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];

			export830 = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream, 2);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830);
			wrapper = new EntrySnapshotWrapper(snapshot, Factory).ExportEntryWrapper;
			AssertEquals("TEST1234567", wrapper.FirstContainerNumber);
		}

		public void TestEnglishDeclarant()
		{
			var declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.CompanyName.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "TestCompanyName");
			KRCustomsRegistry.Instance.RepresentativeName.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "TestRepresentativeName");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new EntryDocumentWrapper(entry, Factory).ExportEntryWrapper;
			AssertEquals("TestCompanyName", wrapper.EnglishCompanyName);
			AssertEquals("TestRepresentativeName", wrapper.EnglishRepresentativeName);
		}
	}
}
