using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportLicense.Testing
{
	class ImportLicenseProviderTest : TestCaseWithFactory
	{
		public void TestImportLicenseFields()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "BTH", "Currency Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), Core.Constants.CountryCodes.Brazil);

			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Code Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, Core.Constants.CountryCodes.Brazil, "105", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, Core.Constants.CountryCodes.China, "160", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_CustomsOffice = "0817800";
			declaration.EntranceOfficeCode = "0817800";
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.China;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 1412.00m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.ExchangeHedgeType = "1";
			invoice.ExchangeHedgeFinancialInstitution = "01";
			invoice.ExchangeHedgeReason = "30";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.ImportLicenseIdentifier = "LI_123";
			entryHeader.CH_EntryStatus = "CUS";

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.JI_Tariff = "40030000";
			invoiceLine1.NaladiHs = "999";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			CombineAssertions(() =>
			{
				AssertEquals("<<UNIQUE BATCH NUMBER PLACE HOLDER>>", provider.UniqueBatchNumber);
				AssertEquals("LI_123", provider.ImportLicenseIdentification);
				AssertEquals("0817800", provider.CustomsOfficeEntranceCode);
				AssertEquals("0817800", provider.CustomsOfficeClearanceCode);
				AssertEquals("40030000", provider.TariffCode);
				AssertEquals("999", provider.SubTariffCode);
				AssertEquals("220", provider.InvoiceCurrency);
				AssertEquals("CIF", provider.Incoterm);
				AssertEquals("1", provider.ExchangeCover);
				AssertEquals("01", provider.FinancialInstitution);
				AssertEquals("30", provider.Reason);
				AssertEquals("105", provider.CommodityOriginCountry);
				AssertEquals("160", provider.CargoProvenance);
			});
		}

		public void TestUsedMaterialFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			AssertEquals("N", provider.IsUsedMaterial);
			AssertEquals("", provider.UsedMaterialRegime);
			AssertEquals("", provider.UsedMaterialOperationType);

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			invoiceLine1.JI_UsedMaterialOperationType = GoodsConditionOperationTypeList.Codes.ExTariff;

			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("S", provider.IsUsedMaterial);
			AssertEquals(UsedMaterialRegimeList.Codes.TemporaryAdmission, provider.UsedMaterialRegime);
			AssertEquals(GoodsConditionOperationTypeList.Codes.ExTariff, provider.UsedMaterialOperationType);
		}

		public void TestTariffDetachList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			AssertEquals(0, provider.TariffDetachList.Count());

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.TariffDetachs.AddNew().CY_Code = "";
			invoiceLine1.TariffDetachs.AddNew().CY_Code = "111";
			invoiceLine1.TariffDetachs.AddNew().CY_Code = "999";

			var invoiceLine2 = entryLine.InvoiceLines[1] as JobComInvoiceLine;
			invoiceLine2.TariffDetachs.AddNew().CY_Code = "222";
			invoiceLine2.TariffDetachs.AddNew().CY_Code = "999";

			provider = new ImportLicenseProvider(messageObject);
			AssertEquals(3, provider.TariffDetachList.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "111", "222", "999" }, provider.TariffDetachList.Select(x => x.TariffDetachCode));
		}

		public void TestDrawbackNcmItemsList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			AssertEquals(2, provider.DrawbackNcmItemsList.Count());
		}

		public void TestConsentingProcessList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			AssertEquals(0, provider.ConsentingProcessList.Count());

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.ConsentingProcessCollection.AddNew();
			var consProcess1 = invoiceLine1.ConsentingProcessCollection.AddNew();
			consProcess1.CSI_ReferenceNumber = "11-58";
			consProcess1.CSI_CustomsOffice = "ANVISA";

			var invoiceLine2 = entryLine.InvoiceLines[1] as JobComInvoiceLine;
			var consProcess2 = invoiceLine2.ConsentingProcessCollection.AddNew();
			consProcess2.CSI_ReferenceNumber = "99/157";
			consProcess2.CSI_CustomsOffice = "INMETRO";
			var consProcess3 = invoiceLine2.ConsentingProcessCollection.AddNew();
			consProcess3.CSI_ReferenceNumber = "99/157";
			consProcess3.CSI_CustomsOffice = "ANVISA";

			provider = new ImportLicenseProvider(messageObject);
			AssertEquals(3, provider.ConsentingProcessList.Count());
		}

		public void TestSupplierAddress()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "US", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "EG", "072", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var supplier1 = Factory.New<OrgHeader>();
			supplier1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			supplier1.MainAddress.CompanyName = "SUPPLIER NAME";
			supplier1.MainAddress.OA_Address1 = "MAIN ADDRESS 2548";
			supplier1.MainAddress.OA_City = "NEW YORK";
			supplier1.MainAddress.OA_State = "NY";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			supplier2.MainAddress.CompanyName = "SUPPLIER NAME 2";
			supplier2.MainAddress.OA_Address1 = "MAIN ADDRESS 2";
			supplier2.MainAddress.OA_City = "ALEXANDRIA";
			supplier2.MainAddress.OA_State = "ALX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			provider = new ImportLicenseProvider(messageObject);
			declaration.JE_OH_Supplier = supplier1.PK;

			var invoiceHeader = entryHeader.InvoiceHeaders[0];
			invoiceHeader.JZ_OA_SupplierAddress = supplier2.MainAddress.PK;
			invoiceHeader.SupplierDocumentaryAddress.E2_OA_Address = supplier2.MainAddress.PK;
			invoiceHeader.SupplierDocumentaryAddress.E2_Contact = "Contact Name";
			CombineAssertions(() =>
			{
				AssertEquals("Supplier Name should be", supplier2.MainAddress.CompanyName, provider.SupplierAddress.Name);
				AssertEquals("Address should be", "MAIN ADDRESS 2", provider.SupplierAddress.Address);
				AssertEquals("Address number should be", "0", provider.SupplierAddress.AddressNumber);
				AssertEquals("CityName should be", supplier2.MainAddress.OA_City, provider.SupplierAddress.CityName);
				AssertEquals("AddressStateCode should be", "Alexandria", provider.SupplierAddress.AddressState);
				AssertEquals("Contact should be", "Contact Name", provider.SupplierAddress.Contact);
				AssertEquals("AddressCountryCode shoulg be", "072", provider.SupplierAddress.AddressCountryCode);
			});
		}

		public void TestManufacturerAddressWithIndicator()
		{
			var manufacturer = OrgHeader.New(Factory);
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.ManufacturerDocAddress.OrganisationPK = manufacturer.MainAddress.PK;
			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AssertEquals("1", provider.ManufacturerIndicator);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Name);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Email);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Address);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressComplementary);
			AssertEquals(string.Empty, provider.ManufacturerAddress.CityName);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressStateCode);

			invoiceLine1.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;
			invoiceLine1.ManufacturerDocAddress.E2_OA_Address = manufacturer.MainAddress.PK;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("MANUFACTURER", provider.ManufacturerAddress.Name);
			AssertEquals("MANUFACTURER@TEST.COM", provider.ManufacturerAddress.Email);
			AssertEquals("MANUFACTURER ADDRESS 1", provider.ManufacturerAddress.Address);
			AssertEquals("MANUFACTURER ADDITIONAL ADDRESS", provider.ManufacturerAddress.AddressComplementary);
			AssertEquals("MANUFACTURER CITY", provider.ManufacturerAddress.CityName);
			AssertEquals("MS", provider.ManufacturerAddress.AddressStateCode);

			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine1.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;
			invoiceLine1.ManufacturerDocAddress.E2_OA_Address = manufacturer.MainAddress.PK;

			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("2", provider.ManufacturerIndicator);
			AssertEquals("MANUFACTURER", provider.ManufacturerAddress.Name);
			AssertEquals("MANUFACTURER@TEST.COM", provider.ManufacturerAddress.Email);
			AssertEquals("MANUFACTURER ADDRESS 1", provider.ManufacturerAddress.Address);
			AssertEquals("MANUFACTURER ADDITIONAL ADDRESS", provider.ManufacturerAddress.AddressComplementary);
			AssertEquals("MANUFACTURER CITY", provider.ManufacturerAddress.CityName);
			AssertEquals("MS", provider.ManufacturerAddress.AddressStateCode);

			invoiceLine1.ManufacturerDocAddress.OrganisationPK = ZGuid.Empty;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("2", provider.ManufacturerIndicator);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Name);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Email);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Address);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressComplementary);
			AssertEquals(string.Empty, provider.ManufacturerAddress.CityName);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressStateCode);

			invoiceLine1.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;
			invoiceLine1.ManufacturerDocAddress.E2_OA_Address = manufacturer.MainAddress.PK;
			invoiceLine1.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("3", provider.ManufacturerIndicator);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Name);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Email);
			AssertEquals(string.Empty, provider.ManufacturerAddress.Address);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressComplementary);
			AssertEquals(string.Empty, provider.ManufacturerAddress.CityName);
			AssertEquals(string.Empty, provider.ManufacturerAddress.AddressStateCode);
		}

		public void TestImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "T1";
			importer.OH_FullName = "test org";
			importer.PrimaryRegistrationNumber.NumberTypeForDisplay = "CJN";
			importer.PrimaryRegistrationNumber.Number = "01001001000101";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			AssertEquals("", provider.ImporterType);
			AssertEquals("", provider.ImporterID);

			declaration.JE_OH_Importer = importer.PK;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals("1", provider.ImporterType);
			AssertEquals("01001001000101", provider.ImporterID);

			importer.PrimaryRegistrationNumber.NumberTypeForDisplay = "CPF";
			importer.PrimaryRegistrationNumber.Number = "09.045.277/0001-05";

			AssertEquals("2", provider.ImporterType);
			AssertEquals("09045277000105", provider.ImporterID);

			importer.PrimaryRegistrationNumber.NumberTypeForDisplay = "CMT";
			importer.PrimaryRegistrationNumber.Number = "09.045.277/0001-05";

			AssertEquals("", provider.ImporterType);
			AssertEquals("", provider.ImporterID);
		}

		public void TestDrawbackFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);
			AssertEquals(0, provider.ConsentingProcessList.Count());

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.DrawbackModality = DrawbackModalityList.Codes.GenericSuspension;
			invoiceLine1.DrawbackCANumber = "123";
			AssertEquals(DrawbackModalityList.Codes.GenericSuspension, provider.DrawbackCode);
			AssertEquals("123", provider.DrawbackCANumberSuspension);
			AssertEquals(ZString.Empty, provider.DrawbackCANumberExemption);

			invoiceLine1.DrawbackModality = DrawbackModalityList.Codes.NonGenericSuspension;
			AssertEquals(DrawbackModalityList.Codes.NonGenericSuspension, provider.DrawbackCode);
			AssertEquals("123", provider.DrawbackCANumberSuspension);
			AssertEquals(ZString.Empty, provider.DrawbackCANumberExemption);

			invoiceLine1.DrawbackModality = DrawbackModalityList.Codes.ExemptionWeb;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals(DrawbackModalityList.Codes.ExemptionWeb, provider.DrawbackCode);
			AssertEquals(ZString.Empty, provider.DrawbackCANumberSuspension);
			AssertEquals("123", provider.DrawbackCANumberExemption);

			invoiceLine1.DrawbackModality = DrawbackModalityList.Codes.NoDrawback;
			provider = new ImportLicenseProvider(messageObject);
			AssertEquals(DrawbackModalityList.Codes.NoDrawback, provider.DrawbackCode);
			AssertEquals(ZString.Empty, provider.DrawbackCANumberSuspension);
			AssertEquals(ZString.Empty, provider.DrawbackCANumberExemption);
		}

		void AddTwoInvoiceLines(JobComInvoiceHeader invoice, CusEntryLine entryLine, short lineNum)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = lineNum;
			invoiceLine.JI_Description = "Test Description";
			entryLine.InvoiceLines.Add(invoiceLine);

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = ++lineNum;
			invoiceLine.JI_Description = "Test Description2";
			entryLine.InvoiceLines.Add(invoiceLine);
		}

		public void TestAdditionalInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.AdditionalInformation = "AdditionalInformation";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			AssertEquals("Additional Information should be", "AdditionalInformation", provider.AdditionalInformation);
		}
		public void TestTaxRegime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			var invoiceLine1 = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine1.DutyTaxRegime = "4";
			invoiceLine1.DutyLegalBase = "35";
			AssertEquals("Tax Regime should be 4", "4", provider.TaxRegime);
			AssertEquals("Legal Base should be 35", "35", provider.LegalBase);
		}

		public void TestTariffAgreementType()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine.JI_SecondaryPreference = "MX99";
			entryLine.InvoiceLines.Add(invoiceLine);

			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			AssertNullOrEmpty("TariffAgreementType should be", provider.TariffAgreementType);

			invoiceLine.NaladiHs = "123";
			provider = new ImportLicenseProvider(messageObject);

			AssertEquals("TariffAgreementType should be", "2", provider.TariffAgreementType);

			invoiceLine.JI_SecondaryPreference = "ASGPC";
			provider = new ImportLicenseProvider(messageObject);

			AssertNullOrEmpty("TariffAgreementType should be", provider.TariffAgreementType);
		}

		public void TestAladiAgreementCode()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AddTwoInvoiceLines(invoice, entryLine, 1);

			var invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
			invoiceLine.JI_SecondaryPreference = "MX99";

			entryLine.InvoiceLines.Add(invoiceLine);
			var messageObject = new ImportLicenseMessageSendingObject(entryHeader);
			var provider = new ImportLicenseProvider(messageObject);

			AssertNotNull("AladiAgreementCode should NOT be null", provider.AladiAgreementCode);
			AssertEquals("AladiAgreementCode should be", string.Empty, provider.AladiAgreementCode);

			invoiceLine.NaladiHs = "123";
			provider = new ImportLicenseProvider(messageObject);

			AssertEquals("AladiAgreementCode should be", "336", provider.AladiAgreementCode);

			invoiceLine.JI_SecondaryPreference = "CO99";
			provider = new ImportLicenseProvider(messageObject);

			Assert("AladiAgreementCode should be", provider.AladiAgreementCode.IsEmpty());

			invoiceLine.JI_SecondaryPreference = "ASGPC";
			provider = new ImportLicenseProvider(messageObject);

			Assert("AladiAgreementCode should be", provider.AladiAgreementCode.IsEmpty());
		}
	}
}
