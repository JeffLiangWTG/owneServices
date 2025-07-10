using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationAdditionItemProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationAdditionItemProvider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "US", "249", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_GoodsOrigin = "US";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoice.JZ_InvoiceAmount = 2500.00m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._1;
			invoice.JZ_ValuationCode = "01";
			invoice.JZ_IncoTermPlace = "US";
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			invoice.JZ_InvoiceCurrExRate = 0.5m;
			invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.BuyerSellerRelationNoInfluence;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;
			cusEntryLine.CL_LineNumber = 1;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;
			invLine.JI_Tariff = "02011001";
			invLine.NaladiNcca = "0800001";
			invLine.NaladiHs = "0800002";
			invLine.JI_CountryOfOrigin = "US";
			invLine.JI_LinePrice = 2500m;
			invLine.JI_NetWeight = 100m;
			invLine.JI_InvoiceUQ = "KG";
			invLine.JI_InvoiceQuantity = 50m;
			invLine.JI_CustomsUnitQty = "KG";
			invLine.JI_CustomsQuantity = 50m;
			invLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
			invLine.JI_TemporaryAdmissionReason = "01";
			invLine.ImportLicenseNumber = "0001";
			invLine.PisCofinsTaxRegime = "1";
			invLine.PisCofinsLegalBase = "2";

			var oNve = invLine.NVECusCodeDataCollection.AddNew();
			oNve.CY_Data = "XXXX";
			oNve.CY_Code = "XX";

			oNve = invLine.NVECusCodeDataCollection.AddNew();
			oNve.CY_Data = "AAAA";
			oNve.CY_Code = "02";

			var oLinkeddoc = invLine.PreviousDocuments.AddNew();
			oLinkeddoc.CSI_ReferenceNumber = "1";
			oLinkeddoc.CSI_Code = ImportPreviousDocumentList.Codes.DI;
			oLinkeddoc = invLine.PreviousDocuments.AddNew();
			oLinkeddoc.CSI_ReferenceNumber = "1";
			oLinkeddoc.CSI_Code = ImportPreviousDocumentList.Codes.DI;
			oLinkeddoc = invLine.PreviousDocuments.AddNew();
			oLinkeddoc.CSI_ReferenceNumber = "2";
			oLinkeddoc.CSI_Code = ImportPreviousDocumentList.Codes.DI;
			oLinkeddoc = invLine.PreviousDocuments.AddNew();

			var oDetach = invLine.TariffDetachs.AddNew();
			oDetach.CY_Code = "999";
			oDetach = invLine.TariffDetachs.AddNew();
			oDetach.CY_Code = "001";

			var oMercosul = invLine.MercosulForeignDeclarations.AddNew();
			oMercosul.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			oMercosul.CSI_Description = "000000000";
			oMercosul = invLine.MercosulForeignDeclarations.AddNew();
			oMercosul.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			oMercosul.CSI_Description = "1111111111";
			oMercosul = invLine.MercosulForeignDeclarations.AddNew();
			oMercosul.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			oMercosul.CSI_Description = "000000000";

			var additionItem = new DeclarationAdditionItemProvider(cusEntryLine);
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationProductDetailsList count should be", 1, additionItem.DeclarationProductDetailsList.Count());
				AssertEquals("DeclarationCustomsValuationList count should be", 2, additionItem.DeclarationCustomsValuationList.Count());
				AssertEquals("LinkedDocumentList count should be", 3, additionItem.DeclarationLinkedDocumentList.Count());
				AssertEquals("DetachList count should be", 2, additionItem.DeclarationDetachList.Count());
				AssertEquals("DeclarationMercosulDocumentList count should be", 2, additionItem.DeclarationMercosulDocumentList.Count());

				AssertEquals("ExchangeCover should be", ExchangeHedgeList.Codes._1, additionItem.ExchangeCover);
				AssertEquals("IncoTerm should be", BRIncoTermList.Codes.FOB, additionItem.IncoTerm);
				AssertEquals("TariffCode should be", "02011001", additionItem.TariffCode);
				AssertEquals("NaladiNcca should be", "0800001", additionItem.NaladiNcca);
				AssertEquals("NaladiHs should be", "0800002", additionItem.NaladiHs);
				AssertEquals("ValuationCode should be", "01", additionItem.ValuationCode);
				AssertEquals("GoodsOrigin should be", "249", additionItem.GoodsOrigin);
				AssertEquals("IncoTermPlace should be", "US", additionItem.IncoTermPlace);
				AssertEquals("AdditionNumber should be", "1", additionItem.AdditionNumber);
				AssertEquals("NetWeight should be", 100m, additionItem.NetWeight);
				AssertEquals("CustomsQty should be", 50m, additionItem.CustomsQty);
				AssertEquals("InvoiceQty should be", 50m, additionItem.InvoiceQty);
				AssertEquals("FOBValue should be", 2500m, additionItem.FOBValue);
				AssertEquals("FOBValueLocalCurrency should be", 5000m, additionItem.FOBValueInLocalCurrency);
				AssertEquals("InvoiceAmount should be", 2500m, additionItem.InvoiceAmount);
				AssertEquals("InvoiceAmountLocalCurrency should be", 5000m, additionItem.InvoiceAmountInLocalCurrency);
				AssertEquals("ManufacturerIndicator should be", "3", additionItem.ManufacturerIndicator);
				AssertEquals("ReasonForTemporaryAdmission should be", "01", additionItem.ReasonForTemporaryAdmission);
				AssertEquals("BuySellerRelationship should be", "2", additionItem.BuySellerRelationship);
				AssertEquals("MercosulForeignDeclarationType should be", "2", additionItem.MercosulForeignDeclarationType);
				AssertEquals("ImportLicenseNumber should be", "0001", additionItem.ImportLicenseNumber);
				AssertEquals("PisCofinsTaxRegime should be", "1", additionItem.PisCofinsTaxRegime);
				AssertEquals("PisCofinsLegalBase should be", "2", additionItem.PisCofinsLegalBase);
				AssertNull("ExchangeHedgeValue should be null", additionItem.ExchangeHedgeValue);
				invoice.ExchangeHedgeValue = 100m;
				additionItem = new DeclarationAdditionItemProvider(cusEntryLine);
				AssertEquals("ExchangeHedgeValue should NOT be null", 100m, additionItem.ExchangeHedgeValue);
				AssertNull("CargoProvenance should be null", additionItem.CargoProvenance);
			});
		}

		public void TestDiscountCharges()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "BTH", "Currency Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "BRL", "790", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "EUR", "978", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceCurrExRate = 2m;
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;
			invLine.JI_LinePrice = 500m;

			var charges = invLine.Charges.AddNew();
			charges.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charges.J7_RX_NKCurrency = "USD";
			charges.J7_Amount = 50m;

			charges = invLine.Charges.AddNew();
			charges.J7_ChargeType = ImportCustomsChargeTypeList.Codes.CommissionsBrokerage;
			charges.J7_RX_NKCurrency = "USD";
			charges.J7_Amount = 20m;

			charges = invLine.Charges.AddNew();
			charges.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charges.J7_RX_NKCurrency = "USD";
			charges.J7_Amount = 30m;

			charges = invLine.Charges.AddNew();
			charges.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charges.J7_RX_NKCurrency = "EUR";
			charges.J7_Amount = 40m;

			charges = invLine.Charges.AddNew();
			charges.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue;
			charges.J7_RX_NKCurrency = "BRL";
			charges.J7_Amount = 100m;

			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_CL = cusEntryLine.PK;
			invLine2.JI_LineNo = 2;
			invLine2.JI_LinePrice = 500m;

			var charges2 = invLine2.Charges.AddNew();
			charges2.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charges2.J7_RX_NKCurrency = "USD";
			charges2.J7_Amount = 35m;

			charges2 = invLine2.Charges.AddNew();
			charges2.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue;
			charges2.J7_RX_NKCurrency = "BRL";
			charges2.J7_Amount = 90m;

			charges2 = invLine2.Charges.AddNew();
			charges2.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherAdditionsCustomsValue;
			charges2.J7_RX_NKCurrency = "USD";
			charges2.J7_Amount = 0m;

			var chargeInvoice = invoice.Charges.AddNew();
			chargeInvoice.J7_ChargeType = ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly;
			chargeInvoice.J7_RX_NKCurrency = "USD";
			chargeInvoice.J7_Amount = 500m;
			chargeInvoice.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;

			declaration.ResumeApportionment();

			var additionItem = new DeclarationAdditionItemProvider(cusEntryLine);
			var discountChargeList = additionItem.DeclarationDiscountChargeList.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCurrencyCode should be", "220", additionItem.InvoiceCurrencyCode);

				AssertEquals("DeclarationDiscountChargeList count should be", 4, discountChargeList.Length);
				AssertEquals("ChargeCode 1 should be", "01", discountChargeList[0].ChargeCode);
				AssertEquals("ChargeCode 2 should be", "01", discountChargeList[1].ChargeCode);
				AssertEquals("ChargeCode 3 should be", "07", discountChargeList[2].ChargeCode);
				AssertEquals("ChargeCode 4 should be", "06", discountChargeList[3].ChargeCode);

				AssertEquals("ChargeCode 1 should be", 82.73m, discountChargeList[0].AmountInLocalCurrency);
				AssertEquals("ChargeCode 2 should be", 24.39m, discountChargeList[1].AmountInLocalCurrency);
				AssertEquals("ChargeCode 3 should be", 190m, discountChargeList[2].AmountInLocalCurrency);
				AssertEquals("ChargeCode 4 should be", 359.72m, discountChargeList[3].AmountInLocalCurrency);

				AssertEquals("ChargeCode 1 should be", 115m, discountChargeList[0].Amount);
				AssertEquals("ChargeCode 2 should be", 40m, discountChargeList[1].Amount);
				AssertEquals("ChargeCode 3 should be", 190m, discountChargeList[2].Amount);
				AssertEquals("ChargeCode 4 should be", 500m, discountChargeList[3].Amount);

				AssertEquals("ChargeCode 1 should be", "220", discountChargeList[0].CurrencyCode);
				AssertEquals("ChargeCode 2 should be", "978", discountChargeList[1].CurrencyCode);
				AssertEquals("ChargeCode 3 should be", "790", discountChargeList[2].CurrencyCode);
				AssertEquals("ChargeCode 4 should be", "220", discountChargeList[3].CurrencyCode);
			});
		}

		public void TestDeclarationAdditionTaxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();

			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.IPI, 10m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 10m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.PIS, 10m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.ICMS, 10m);
			var antidumping = entryLine.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 10m);
			antidumping.CF_Rate = 10m;

			var additionItemProvider = new DeclarationAdditionItemProvider(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals(5, additionItemProvider.DeclarationAdditionTaxes.Count());
				AssertEquals("DTY", "0001", additionItemProvider.DeclarationAdditionTaxes.ElementAt(0).TaxType);
				AssertEquals("IPI", "0002", additionItemProvider.DeclarationAdditionTaxes.ElementAt(1).TaxType);
				AssertEquals("ADD", "0003", additionItemProvider.DeclarationAdditionTaxes.ElementAt(2).TaxType);
				AssertEquals("PIS", "0005", additionItemProvider.DeclarationAdditionTaxes.ElementAt(3).TaxType);
				AssertEquals("COF", "0006", additionItemProvider.DeclarationAdditionTaxes.ElementAt(4).TaxType);
			});
		}

		public void TestDeclarationAdditionLegalActList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.DutyTaxRegime = TaxRegimeList.Codes.FullCollection;
			invoiceLine1.JI_PrimaryPreference = Constants.RatePreferenceType.FreeTradeAgreement;

			var additionalTariff = invoiceLine1.AdditionalTariffs.AddNew();
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExDutyTariff;
			additionalTariff.LegalActType = "AD";
			additionalTariff.LegalActNumber = "151021";
			additionalTariff.LegalActIssuingBody = "ALADI";
			additionalTariff.LegalActYear = "2022";

			additionalTariff = invoiceLine1.AdditionalTariffs.AddNew();
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.ExIPITariff;
			additionalTariff.LegalActType = "ADE";
			additionalTariff.LegalActNumber = "151515";
			additionalTariff.LegalActIssuingBody = "CAMEX";
			additionalTariff.LegalActYear = "2020";

			additionalTariff = invoiceLine1.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement) ?? invoiceLine1.AdditionalTariffs.AddNew();
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.TariffAgreement;
			additionalTariff.LegalActType = "3";
			additionalTariff.TariffType = "MX99";
			additionalTariff.LegalActNumber = "151584";
			additionalTariff.LegalActIssuingBody = "AgreeImp";
			additionalTariff.LegalActYear = "2019";

			additionalTariff = invoiceLine1.AdditionalTariffs.AddNew();
			additionalTariff.LegalActSubject = AdditionalTaxTypeList.Codes.Antidumping;
			additionalTariff.LegalActType = "ADV";
			additionalTariff.LegalActNumber = "1996";
			additionalTariff.LegalActIssuingBody = "ALADI";
			additionalTariff.LegalActYear = "2025";

			invoiceLine1.IPITaxBenefitLegalActType = "ADE";
			invoiceLine1.IPITaxBenefitLegalActNumber = "2589";
			invoiceLine1.IPITaxBenefitLegalActIssuingBody = "CAMEX";
			invoiceLine1.IPITaxBenefitLegalActYear = "2021";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationAdditionLegalActList should be", 6, additionItem.DeclarationAdditionLegalActList.Count());
				AssertEquals("AladiCode should be", "336", additionItem.AladiCode);
				AssertEquals("TypeOfAgreement should be", "2", additionItem.TypeOfAgreement);
			});

			invoiceLine1.DutyTaxRegime = ZString.Empty;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationAdditionLegalActList should be", 6, additionItem.DeclarationAdditionLegalActList.Count());
				AssertEquals("AladiCode should be", null, additionItem.AladiCode);
				AssertEquals("TypeOfAgreement should be", null, additionItem.TypeOfAgreement);
			});
		}

		public void TestSupplierAddress()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			supplier.MainAddress.CompanyName = "SUPPLIER NAME";
			supplier.MainAddress.OA_Address1 = "VIA ANTONIO CAVALIERI DUCATI 3";
			supplier.MainAddress.OA_AdditionalAddressInformation = "SAN VITALE, BO";
			supplier.MainAddress.OA_AddressMap = "SNA1[29-29]SA1[0-27]";
			supplier.MainAddress.OA_City = "ALEXANDRIA";
			supplier.MainAddress.OA_State = "ALX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;
			cusEntryLine.CL_LineNumber = 1;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;
			invLine.JI_LinePrice = 2500m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);

			CombineAssertions(() =>
			{
				AssertEquals("Supplier Name should be", supplier.MainAddress.CompanyName, additionItem.SupplierAddress.Name);
				AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI", additionItem.SupplierAddress.Address);
				AssertEquals("Address number should be", "3", additionItem.SupplierAddress.AddressNumber);
				AssertEquals("Address Complementary should be", "SAN VITALE, BO", additionItem.SupplierAddress.AddressComplementary);
				AssertEquals("CityName should be", supplier.MainAddress.OA_City, additionItem.SupplierAddress.CityName);
				AssertEquals("AddressStateCode should be", "Alexandria", additionItem.SupplierAddress.AddressState);
			});
		}

		public void TestManufacturerAddress()
		{
			var manufacturer = OrgHeader.New(Factory);
			manufacturer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			manufacturer.MainAddress.CompanyName = "MANUFACTURER NAME";
			manufacturer.MainAddress.OA_Address1 = "VIA ANTONIO CAVALIERI DUCATI 3";
			manufacturer.MainAddress.OA_AdditionalAddressInformation = "SAN VITALE, BO";
			manufacturer.MainAddress.OA_AddressMap = "SNA1[29-29]SA1[0-27]";
			manufacturer.MainAddress.OA_City = "ALEXANDRIA";
			manufacturer.MainAddress.OA_State = "ALX";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;
			cusEntryLine.CL_LineNumber = 1;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;
			invLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);

			AssertNull("Manufacturer should be null", additionItem.ManufacturerAddress);

			invLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("Manufacturer Name should be", manufacturer.MainAddress.CompanyName, additionItem.ManufacturerAddress.Name);
			AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI", additionItem.ManufacturerAddress.Address);
			AssertEquals("Address number should be", "3", additionItem.ManufacturerAddress.AddressNumber);
			AssertEquals("Address Complementary should be", "SAN VITALE, BO", additionItem.ManufacturerAddress.AddressComplementary);
			AssertEquals("CityName should be", manufacturer.MainAddress.OA_City, additionItem.ManufacturerAddress.CityName);
			AssertEquals("AddressStateCode should be", "Alexandria", additionItem.ManufacturerAddress.AddressState);
		}

		public void TestGoodsCondition()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine.CL_CH = entryHeader.PK;
			cusEntryLine.CL_LineNumber = 1;

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_LineNo = 1;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("GoodsUsedMaterial should be", "N", additionItem.GoodsUsedMaterial);
			AssertEquals("GoodsMadeToOrder should be", "N", additionItem.GoodsMadeToOrder);

			invLine.JI_GoodsCondition = GoodsConditionTypeList.Codes.UsedMaterial;
			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("GoodsUsedMaterial should be", "S", additionItem.GoodsUsedMaterial);
			AssertEquals("GoodsMadeToOrder should be", "N", additionItem.GoodsMadeToOrder);

			invLine.JI_GoodsCondition = GoodsConditionTypeList.Codes.GoodsMadeToOrder;
			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("GoodsUsedMaterial should be", "N", additionItem.GoodsUsedMaterial);
			AssertEquals("GoodsMadeToOrder should be", "S", additionItem.GoodsMadeToOrder);
		}

		public void TestCargoProvenance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "AU", "072", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "US", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_GoodsOrigin = "AU";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			var additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);

			AssertNull("CargoProvenance must be empty", additionItem.CargoProvenance);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._18;
			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("CargoProvenance must be", "072", additionItem.CargoProvenance);

			declaration.JE_GoodsOrigin = "US";
			additionItem = new DeclarationAdditionItemProvider(cusEntryHeader.MergedLines[0]);
			AssertEquals("CargoProvenance must be", "074", additionItem.CargoProvenance);
		}
	}
}
