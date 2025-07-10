using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business.Testing.Util;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using UniversalConstants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ImportFTACreatorTest : TestCaseWithFactory
	{
		public void TestDepartureDate_Invalid()
		{
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(DateTime.MinValue, importFTAHeader.DepartureDate);
		}

		public void TestTransshipmentDate_Invalid()
		{
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(DateTime.MinValue, importFTAHeader.TransshipmentDate);
		}

		public void TestAdditionalInvoiceIssuedInThirdCountryYN()
		{
			var importFTACreator = new ImportFTACreator();
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = Core.Constants.CountryCodes.Afghanistan;
			var importFTAHeader = importFTACreator.Create(entry);
			AssertEquals("Country Entered", Constants.YesNo.Yes, importFTAHeader.EntryLines.Single().AdditionalInvoiceIssuedInThirdCountryYN);
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = string.Empty;
			importFTAHeader = importFTACreator.Create(entry);
			AssertEquals("Country Blank", Constants.YesNo.No, importFTAHeader.EntryLines.Single().AdditionalInvoiceIssuedInThirdCountryYN);
		}

		public void TestCertificateOfOriginExporterNumber_SupplierNull()
		{
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(null, importFTAHeader.EntryLines.Single().CertificateOfOriginExporterNumber);
		}

		public void TestCertificateOfOriginExporterNumber_NoRegistrationNumber()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceLine.JI_CoveredByCOOExporter = true;
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(ZString.Empty, importFTAHeader.EntryLines.Single().CertificateOfOriginExporterNumber);
		}

		public void TestCertificateOfOriginExporterNumber_WithRegistrationNumber()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew(Constants.IdentificationType.CertificateOfOriginExporterNumber, "P641150129351");
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceLine.JI_CoveredByCOOExporter = true;

			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals("P641150129351", importFTAHeader.EntryLines.Single().CertificateOfOriginExporterNumber);

			invoiceLine.JI_CoveredByCOOExporter = false;

			importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(null, importFTAHeader.EntryLines.Single().CertificateOfOriginExporterNumber);
		}

		public void TestCertificateOfOriginIssueDate()
		{
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(DateTime.MinValue, importFTAHeader.EntryLines.Single().CertificateOfOriginIssueDate);
		}

		public void TestImporter_Null()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertNull(importFTAHeader.Importer);
		}

		public void TestImporter_UnipassIDForIndividual()
		{
			var importer = CreateImporter();
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals("IsIndividual", true, importFTAHeader.Importer.IsIndividual);
			AssertEquals("ID345876", importFTAHeader.Importer.UnipassIDForIndividual);
		}

		public void TestImporter_FirstMatchedNull()
		{
			var importer = CreateImporter();

			importer.OH_Category = OrgConstants.Category.Business;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals("IsIndividual", false, importFTAHeader.Importer.IsIndividual);
			AssertNull(importFTAHeader.Importer.UnipassIDForIndividual);
			AssertEquals("TEST**1234567", importFTAHeader.Importer.UnipassIDForOrganization);
		}

		public void TestImporter_FirstMatchedBusiness()
		{
			var importer = CreateImporter();
			importer.OH_Category = OrgConstants.Category.Business;
			importer.CustomsCodes.AddNew(Constants.IdentificationType.BusinessRegNo, "B841250871532");
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals("IsIndividual", false, importFTAHeader.Importer.IsIndividual);
			AssertNull(importFTAHeader.Importer.UnipassIDForIndividual);
			AssertEquals("TEST**1234567", importFTAHeader.Importer.UnipassIDForOrganization);
			AssertEquals("B841250871532", importFTAHeader.Importer.BusinessRegNo);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSerialization()
		{
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_Code = "XXINC";
			port.RL_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			port.RL_PortName = "Incheon";
			Factory.Save();

			SetupChinaSouthKoreaTariffRates();

			var importer = CreateImporter();
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			var supplier = CreateSuppllier();
			var manufacturer = CreateManufacturer();

			declaration.JE_ExportDate = new ZDateTime(2021, 01, 01);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_TransshipmentDate = new ZDateTime(2021, 01, 02);
			declaration.JE_TransshipmentPort = "XXINC";
			entryInstruction.CEI_FTARelationArticleCode = "4";
			entryInstruction.CEI_StatementNumber5WN = "192113334901920";
			entry.CH_CEI_Instruction = entryInstruction.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			SetupInvoiceLine("0101299000", Core.Constants.CountryCodes.Indonesia, Core.Constants.CountryCodes.China, 1, "1", "FCN1", 2.34, Core.Constants.Weight.Kilograms);
			SetupInvoiceLine("0101299010", Core.Constants.CountryCodes.Indonesia, Core.Constants.CountryCodes.UnitedStates, 1, "2", "FUS1", 1.03, Core.Constants.Weight.Tonnes);

			invoiceLine.CertificateOfOriginIssueStatus = "G";
			invoiceLine.JI_CoveredByCOOExporter = true;
			var certificateOfOrigin = invoiceLine.CertificateOfOriginData;
			certificateOfOrigin.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Belarus;
			certificateOfOrigin.CSI_DateOfIssue = new ZDateTime(2021, 4, 1);
			certificateOfOrigin.CSI_ReferenceNumber = "800324356053";
			certificateOfOrigin.CSI_IssuerType = "0";
			certificateOfOrigin.CSI_Description = "FIRST CERTIFICATE OF ORIGIN";
			certificateOfOrigin.CSI_Quantity2 = 12.34;
			certificateOfOrigin.CSI_UnitOfQuantity2 = Core.Constants.Weight.Grams;

			var importFTA = new ImportFTACreator().Create(entry);
			using (var dataProviderStream = KRXmlObjectSerializer.Serialize(importFTA))
			{
				AssertASCIIFileSameAsString(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\DataProviders\Import\GOVCBR5SC\FTACreatorTestXml.txt"), XmlHelper.IgnoreXmlnsAttrOrder(dataProviderStream.WriteToString()));
			}
		}

		public void TestStatementNumber5WN()
		{
			entryInstruction.CEI_StatementNumber5WN = "192113334901920";
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var importFTA = new ImportFTACreator().Create(entry);
			AssertEquals("192113334901920", importFTA.StatementNumber5WN);
		}

		public void TestManufacturer()
		{
			invoiceLine.JI_PrimaryPreference = "FIL1";

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 002;
			entryLine2.CL_FTASequenceNumber = 002;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_PrimaryPreference = "FCN1";

			var manufacturer = CreateManufacturer();
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("인천광역시 남동구 청능대로718번길 7 (논현동 소래마을풍림아파트) 105동 503호", invoiceHeader.ManufacturerAddress.Address1);
			AssertEquals("상세주소", invoiceHeader.ManufacturerAddress.Address2);
			AssertEquals("21670", invoiceHeader.ManufacturerAddress.Postcode);

			var importFTAHeader = new ImportFTACreator().Create(entry);
			AssertEquals(2, importFTAHeader.EntryLines.Length);
			AssertEquals("인천광역시 남동구 청능대로718번길 7 (논현동 소래마을풍림아파트) 105동 503호", importFTAHeader.EntryLines[0].Manufacturer.AddressLine1);
			AssertEquals("상세주소", importFTAHeader.EntryLines[0].Manufacturer.AddressLine2);
			AssertEquals("21670", importFTAHeader.EntryLines[0].Manufacturer.Postcode);
			AssertEquals(null, importFTAHeader.EntryLines[1].Manufacturer);
		}

		public void TestTariffRate()
		{
			#region ZZ Data_NullRate
			var zzDataSetUpper = new UniversalReferenceTestDataHelper(Factory);
			zzDataSetUpper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = zzDataSetUpper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var dutyRateType = zzDataSetUpper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var adValoremRateCode = zzDataSetUpper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, dutyRateType.PK);

			var cnTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.China);
			zzDataSetUpper.AddCountry(cnTradeGroup, Core.Constants.CountryCodes.China);
			var fcn1Preference = zzDataSetUpper.CreatePreferenceForCountry("FCN1", "한ㆍ중국 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);

			var allTradeGroup = zzDataSetUpper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.All);
			zzDataSetUpper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.China);
			var aPreference = zzDataSetUpper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff = zzDataSetUpper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299000", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			#endregion

			var importFTA = new ImportFTACreator().Create(entry);
			AssertEquals(0m, importFTA.EntryLines[0].TariffRate);

			#region ZZ Data_Rate
			var allRate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.08", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate = zzDataSetUpper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "VFD * 0.005", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			zzDataSetUpper.CreateCusApplicability(allRate, allTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			zzDataSetUpper.CreateCusApplicability(fcn1Rate, cnTradeGroup, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion
			invoiceLine.JI_Tariff = "0101299000";
			invoiceLine.JI_PrimaryPreference = "FCN1";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			importFTA = new ImportFTACreator().Create(entry);
			AssertEquals("010129", importFTA.EntryLines[0].HSCode);
			AssertEquals(0.5m, importFTA.EntryLines[0].TariffRate);
		}

		void SetupChinaSouthKoreaTariffRates()
		{
			var universalhelper = new UniversalReferenceTestDataHelper(Factory);
			universalhelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = universalhelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.TariffTypes.HarmonizedSystem);

			var dutyRateType = universalhelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.KoreaSouth, UniversalConstants.RateTypes.Duty);
			var adValoremRateCode = universalhelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, dutyRateType.PK);

			var cnTradeGroup = universalhelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.China);
			universalhelper.AddCountry(cnTradeGroup, Core.Constants.CountryCodes.China);
			var fcn1Preference = universalhelper.CreatePreferenceForCountry("FCN1", "한ㆍ중국 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);

			var allTradeGroup = universalhelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TradeGroups.All);
			universalhelper.AddCountry(allTradeGroup, Core.Constants.CountryCodes.China);
			var aPreference = universalhelper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);

			var hsTariff = universalhelper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0101299000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var allRate = universalhelper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.08", aPreference.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var fcn1Rate = universalhelper.CreateRate(hsTariff, adValoremRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.005", fcn1Preference.PK, "", Core.Constants.CountryCodes.KoreaSouth);

			universalhelper.CreateCusApplicability(allRate, allTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalhelper.CreateCusApplicability(fcn1Rate, cnTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		void SetupInvoiceLine(string tariff, string secondCommercialInvoiceCountry, string countryOfOrigin, int cooSplitOrder, string supportingDocTypeType, string primaryPreference, ZDecimal netWeight, string netWeightUQ)
		{
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = secondCommercialInvoiceCountry;
			invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
			invoiceLine.COOSplitOrder = cooSplitOrder;
			invoiceLine.JI_COOSupportingDocType = supportingDocTypeType;
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			invoiceLine.JI_NetWeight = netWeight;
			invoiceLine.JI_NetWeightUQ = netWeightUQ;
		}

		OrgHeader CreateImporter()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, OrgConstants.Category.Business, "YMY", "윤민용");
			TestOrgDataSetUpHelper.AddOrgContact(importer, "윤민용", true);
			TestOrgDataSetUpHelper.AddOrgAddress(importer.MainAddress, "인천광역시 남동구 청능대로718번길 7 (논현동 소래마을풍림아파트) 105동 503호", "상세주소", "21670", "101010", "10201", Core.Constants.CountryCodes.KoreaSouth);
			importer.MainAddress.OA_Fax = "01056987349";
			importer.MainAddress.OA_Phone = "01044587479";
			importer.MainAddress.OA_Email = "test@skorea.com";

			importer.CustomsCodes.AddNew(Constants.IdentificationType.UnipassIDForIndividual, "ID345876");
			importer.CustomsCodes.AddNew(Constants.IdentificationType.UnipassIDForOrganization, "TEST**1234567");
			return importer;
		}

		OrgHeader CreateManufacturer()
		{
			var manufacturer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, OrgConstants.Category.Business, "BGB", "BOSSY GLOBAL GB");
			TestOrgDataSetUpHelper.AddOrgContact(manufacturer, "BRETT PAINE GB", true);
			TestOrgDataSetUpHelper.AddOrgAddress(manufacturer.MainAddress, "인천광역시 남동구 청능대로718번길 7 (논현동 소래마을풍림아파트) 105동 503호", "상세주소", "21670", "", "");
			manufacturer.MainAddress.OA_Fax = "67358901235";
			manufacturer.MainAddress.OA_Phone = "67358901234";
			manufacturer.MainAddress.OA_Email = "test@skorea.com";
			return manufacturer;
		}

		OrgHeader CreateSuppllier()
		{
			var supplier = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, OrgConstants.Category.Business, "TEST", "PARAGON INTERNATIONAL");
			TestOrgDataSetUpHelper.AddOrgContact(supplier, "MARK CONNEL", true);
			supplier.CustomsCodes.AddNew(Constants.IdentificationType.CertificateOfOriginExporterNumber, "P641150121378");

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Fax = "18444893773";
			supplierAddress.OA_Phone = "18444893772";
			supplierAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			return supplier;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "1364021237195M";
			invoiceLine.CusEntryLine.CL_LineNumber = 001;
			invoiceLine.CusEntryLine.CL_FTASequenceNumber = 001;
		}
		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
