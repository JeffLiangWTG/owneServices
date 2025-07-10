using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExport5DQEntryHeaderTest : XMLMessageTestHelper<LocalExport5DQEntryHeaderTest>
	{
		internal const string BOX = "BOX";
		internal const string PLT = "PLT";

		public void Test5DQNormalHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);
			var builder = new GOVCBR5DQMessageBuilder(result).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExport5DQEntryHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5DQ_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(builder))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			#region Header Assert
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._07, result.DeclarationType);
			AssertEquals(15, result.CrewCount);
			AssertEquals(24, result.ScheduledSailingDays);
			AssertEquals("M/V MARIA", result.FlightNoOrVesselName);
			AssertEquals("20GLKO0080I", result.MRNNo);
			AssertEquals(ZDate.Invalid, result.DeclarationDate);
			#endregion

			#region OrgHeader Assert
			AssertEquals("사업자등록번호", result.Supplier.BusinessRegNo);
			AssertEquals("통관고유부호", result.Supplier.UnipassIDForOrganization);

			AssertEquals("1234567890", result.Exporter.BusinessRegNo);
			AssertEquals("사업자등록번호", result.Manufacturer.BusinessRegNo);
			AssertEquals("통관고유부호", result.Manufacturer.UnipassIDForOrganization);

			AssertEquals("무한상사", result.Importer.CompanyName);
			AssertEquals("김대표", result.Importer.RepresentativeName);
			AssertEquals("기본주소", result.Importer.AddressLine1);
			AssertEquals("상세주소", result.Importer.AddressLine2);
			AssertEquals("32012", result.Importer.Postcode);
			AssertEquals("110001", result.Importer.RoadNameCode);
			AssertEquals("121200", result.Importer.BuildingNumber);
			AssertEquals("1200020212", result.Importer.BusinessRegNo);
			#endregion

			#region Stevedore Assert
			AssertEquals(1, result.Stevedores[0].SequenceNo);
			AssertEquals("홍길동", result.Stevedores[0].FullName);
			AssertEquals("19910506", result.Stevedores[0].Birthday.ToString(DateFormatType.Date));
			AssertEquals("110001", result.Stevedores[0].RoadNameCode);
			AssertEquals("121200", result.Stevedores[0].BuildingNumber);
			AssertEquals("43012", result.Stevedores[0].Postcode);
			AssertEquals("기본주소", result.Stevedores[0].AddressLine1);
			AssertEquals("상세주소", result.Stevedores[0].AddressLine2);

			AssertEquals(2, result.Stevedores[1].SequenceNo);
			AssertEquals("Hong-Gil-Dong", result.Stevedores[1].FullName);
			AssertEquals("19910606", result.Stevedores[1].Birthday.ToString(DateFormatType.Date));
			#endregion

			#region EntryLine Assert
			AssertEquals(1, result.EntryLines[0].EntryLineNo);
			AssertEquals("1234567890", result.EntryLines[0].HSCode);
			AssertEquals("STAINLESS STEEL", result.EntryLines[0].InvoiceDescription);
			AssertEquals("물품식별번호", result.EntryLines[0].GoodsNo);
			AssertEquals(9999m, result.EntryLines[0].Quantity);
			AssertEquals(Core.Constants.Weight.Kilograms, result.EntryLines[0].QuantityUnit);
			AssertEquals("L172770912345", result.EntryLines[0].DocumentNo);
			AssertEquals("1", result.EntryLines[0].DocumentType);
			AssertEquals(99, result.EntryLines[0].Packages);
			AssertEquals("VL", result.EntryLines[0].PackagesType);
			AssertEquals("010151234567001999", result.EntryLines[0].PreviousTransactionReferenceNo);
			AssertEquals("01", result.EntryLines[0].PreviousTransactionReferenceNoType);
			AssertEquals(1000m, result.EntryLines[0].FOBAmount);
			AssertEquals(9999m, result.EntryLines[0].NetWeight);
			AssertEquals("20130101", result.EntryLines[0].InboundDate.ToString(DateFormatType.Date));

			AssertEquals(2, result.EntryLines[1].EntryLineNo);
			AssertEquals("1234567891", result.EntryLines[1].HSCode);
			AssertEquals("STAINLESS STEEL2", result.EntryLines[1].InvoiceDescription);
			AssertEquals("물품식별번호2", result.EntryLines[1].GoodsNo);
			AssertEquals(999m, result.EntryLines[1].Quantity);
			AssertEquals(Core.Constants.Weight.Kilograms, result.EntryLines[1].QuantityUnit);
			AssertEquals("L172770925458", result.EntryLines[1].DocumentNo);
			AssertEquals("2", result.EntryLines[1].DocumentType);
			AssertEquals(9, result.EntryLines[1].Packages);
			AssertEquals("VL", result.EntryLines[1].PackagesType);
			AssertEquals("010151234567001990", result.EntryLines[1].PreviousTransactionReferenceNo);
			AssertEquals("02", result.EntryLines[1].PreviousTransactionReferenceNoType);
			AssertEquals(999m, result.EntryLines[1].FOBAmount);
			AssertEquals(999m, result.EntryLines[1].NetWeight);
			AssertEquals("20130102", result.EntryLines[1].InboundDate.ToString(DateFormatType.Date));
			#endregion
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Outgoing";

		[TestDate(2021, 1, 12)]
		public void Test5DQNewVesselHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 900000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Grams;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);

			CombineAssertions("Header Assert", () =>
			{
				AssertEquals("DeclarationType", LocalExportTransactionNatureCodeList.Codes._09, result.DeclarationType);
				AssertEquals("TotalGrossWeight", 900m, result.TotalGrossWeight);
				AssertEquals("MRNNo", "21ZZZZZZZZZ", result.MRNNo);
				AssertNull("Supplier", result.Supplier);
				AssertNull("Exporter", result.Exporter);
				AssertNull("Manufacturer", result.Manufacturer);
				AssertNull("Importer", result.Importer);
			});
		}

		public void Test5DQChangeOfQualificationHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			declaration.JE_VesselName = "M/V MARIA";

			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = declaration.JE_VesselName;
			vessel.RV_RadioCallSign = "5VDP9";
			declaration.JE_MRNType = MRNTypeList.Codes.ChangeOfQualification;

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);

			#region Header Assert
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._17, result.DeclarationType);
			AssertEquals("35VDP9", result.MRNNo);
			#endregion
		}

		public void Test5DQNotSeaHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			declaration.JE_MRNType = MRNTypeList.Codes.Normal;
			declaration.JE_VoyageFlightNo = "NotSea";

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);

			#region Header Assert
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._08, result.DeclarationType);
			AssertEquals("NotSea", result.FlightNoOrVesselName);
			#endregion
		}

		public void Test5DQLocalExportOtherTransportMeans()
		{
			if (Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, "SAMARIA TEST")) == null)
			{
				var vessel = RefVessel.New(Factory);
				vessel.RV_Code = "SAMARIA TEST";
				vessel.RV_MalaysiaVesselId = "USR907780";
			}
			if (Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, "Oerssleff TEST")) == null)
			{
				var vessel1 = RefVessel.New(Factory);
				vessel1.RV_Code = "Oerssleff TEST";
				vessel1.RV_MalaysiaVesselId = "MPC870428";
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			declaration.JE_TotalWeight = 900000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;
			declaration.JE_MRNType = MRNTypeList.Codes.NewVessel;

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var transportMean = declaration.TransportMeans.AddNew();
			transportMean.CY_Order = 1;
			transportMean.CY_Code = "SAMARIA TEST";
			transportMean.CY_Data = "서울 허12 3456";

			var transportMean1 = declaration.TransportMeans.AddNew();
			transportMean1.CY_Order = 2;
			transportMean1.CY_Code = "Oerssleff TEST";
			transportMean1.CY_Data = "서울 라24 7890";

			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);

			CombineAssertions("Header Assert", () =>
			{
				AssertEquals("SequenceNo", 1, result.OtherTransportMeans[0].SequenceNo);
				AssertEquals("WorkingVesselName", "SAMARIA TEST", result.OtherTransportMeans[0].WorkingVesselName);
				AssertEquals("WorkingVesselLloydsNumber", "USR907780", result.OtherTransportMeans[0].WorkingVesselLloydsNumber);
				AssertEquals("TransportVehicleRegNo", "서울 허12 3456", result.OtherTransportMeans[0].TransportVehicleRegNo);

				AssertEquals("SequenceNo", 2, result.OtherTransportMeans[1].SequenceNo);
				AssertEquals("WorkingVesselName", "Oerssleff TEST", result.OtherTransportMeans[1].WorkingVesselName);
				AssertEquals("WorkingVesselLloydsNumber", "MPC870428", result.OtherTransportMeans[1].WorkingVesselLloydsNumber);
				AssertEquals("TransportVehicleRegNo", "서울 라24 7890", result.OtherTransportMeans[1].TransportVehicleRegNo);
			});
		}

		public void Test5DQDeclarationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._17;
			declaration.JE_EntryDate = new ZDate(2023, 01, 01);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.LocalExport;
			entryNum.CE_EntryNum = "4177721000030";

			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._17, result.DeclarationType);
			AssertEquals(ZDate.Invalid, (ZDate)result.DeclarationDate);

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			result = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals(ZDate.Invalid, (ZDate)result.DeclarationDate);
		}

		public void TestEmptyXml()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithPartialData();
			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);
			var builder = new GOVCBR5DQMessageBuilder(result).GenerateMessage();
			var fileReader = new TestFileReader(typeof(LocalExport5DQEntryHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5DQ_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(builder))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestEmptySupplier()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithPartialData();
			entry.Declaration.JE_OH_Supplier = ZGuid.Empty;
			var result = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertNull(result.Supplier);
			var message = new GOVCBR5DQMessageBuilder(result).GenerateMessage();
			AssertNull(message.Submitter.Id[0].Value);
		}

		public void TestTariffHasRequiringInvQuantityInCustomsUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208100000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Attribute InvoiceQuantity in CU1");
			helper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.InvoiceQuantityInCU1, YesNo.Yes, tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0208122222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Has Not Attribute");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._09;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLines1 = invoice.InvoiceLines.AddNew();
			invoiceLines1.JI_Tariff = tariff1.ZZ1_TariffCode;
			invoiceLines1.JI_CustomsQuantity = 1;
			invoiceLines1.JI_CustomsUnitQty = "U";
			invoiceLines1.JI_InvoiceQuantity = 2;
			invoiceLines1.JI_InvoiceUQ = "BAG";

			var invoiceLines2 = invoice.InvoiceLines.AddNew();
			invoiceLines2.JI_Tariff = tariff2.ZZ1_TariffCode;
			invoiceLines2.JI_CustomsQuantity = 1;
			invoiceLines2.JI_CustomsUnitQty = "U";
			invoiceLines2.JI_InvoiceQuantity = 2;
			invoiceLines2.JI_InvoiceUQ = "BAG";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));
			var result = new LocalExport5DQEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("Has Attribute", "0208100000", result.EntryLines[0].HSCode);
			AssertEquals(1m, result.EntryLines[0].Quantity);
			AssertEquals("U", result.EntryLines[0].QuantityUnit);

			AssertEquals("Has Not Attribute", "0208122222", result.EntryLines[1].HSCode);
			AssertEquals(2m, result.EntryLines[1].Quantity);
			AssertEquals("BAG", result.EntryLines[1].QuantityUnit);
		}

		public void TestEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var entryNum1 = entry.EntryNumbers.AddNew();
			entryNum1.CE_EntryNum = "1083699012345";
			entryNum1.CE_EntryType = "LEX";
			entryNum1.CE_EntryLineReference = "2";
			Factory.Save();

			var localExport5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals("1083699012345", localExport5DQ.DeclarationNumber);

			var entryNum2 = entry.EntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "1083699098765";
			entryNum2.CE_EntryType = "LEX";
			entryNum2.CE_EntryLineReference = "1";
			localExport5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals("1083699098765", localExport5DQ.DeclarationNumber);
		}

		public void TestExporter()
		{
			var exporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA1", "레디코리아1");
			exporter.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1234567890", Core.Constants.CountryCodes.KoreaSouth);

			var emptyExporter = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA2", "레디코리아2");

			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA3", "레디코리아3");
			supplier.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0987654321", Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_Exporter = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(false));

			var result = new LocalExport5DQEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("If the exporter is null, the supplier business registration number is used.", "0987654321", result.Exporter.BusinessRegNo);

			declaration.JE_OH_Exporter = emptyExporter.PK;
			result = new LocalExport5DQEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("If the exporter is not null, the supplier business registration number is not used.", null, result.Exporter.BusinessRegNo);

			declaration.JE_OH_Exporter = exporter.PK;
			result = new LocalExport5DQEntryHeaderCreator().Create(declaration.CustomsEntryHeaders[0]);
			AssertEquals("1234567890", result.Exporter.BusinessRegNo);
		}

		public void TestSupplierAddress()
		{
			var supplierWithMainAddress = Factory.NewWithValidTestData<OrgHeader>();
			supplierWithMainAddress.OH_FullName = "5DQ Company";
			supplierWithMainAddress.MainAddress.Address1 = "Main Address1";
			supplierWithMainAddress.MainAddress.Address2 = "Main Address2";
			var supplierContact = supplierWithMainAddress.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DQ CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplierWithMainAddress.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			supplierWithMainAddress.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);

			var supplierWithCustomsAddress = Factory.NewWithValidTestData<OrgHeader>();
			supplierWithCustomsAddress.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			supplierWithCustomsAddress.OH_FullName = "5DQ Company";
			supplierWithCustomsAddress.CustomsAddress.Address1 = "Customs Address1";
			supplierWithCustomsAddress.CustomsAddress.Address2 = "Customs Address2";
			supplierContact = supplierWithCustomsAddress.Contacts.AddNew();
			supplierContact.OC_ContactName = "5DQ CompanyRepresentative";
			supplierContact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			supplierWithCustomsAddress.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "111111111111111", Core.Constants.CountryCodes.KoreaSouth);
			supplierWithCustomsAddress.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0000000000", Core.Constants.CountryCodes.KoreaSouth);

			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			entry.Declaration.JE_OH_Supplier = supplierWithMainAddress.PK;
			var localExport5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals(localExport5DQ.Supplier.AddressLine1, "Main Address1");
			AssertEquals(localExport5DQ.Supplier.AddressLine2, "Main Address2");

			entry.Declaration.JE_OH_Supplier = supplierWithCustomsAddress.PK;
			localExport5DQ = new LocalExport5DQEntryHeaderCreator().Create(entry);
			AssertEquals(localExport5DQ.Supplier.AddressLine1, "Customs Address1");
			AssertEquals(localExport5DQ.Supplier.AddressLine2, "Customs Address2");
		}
	}
}
