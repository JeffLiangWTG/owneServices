using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestInvoiceLineExportSecondQuantity()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 241m;
			dec.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, dec);
			AssertEquals(1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceLineData = declarationData.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.First();
			Assert(invoiceLineData.AddInfoCollection.Any(x => x.Key.Value == Constants.AddInfoKeys.InvoiceLine.Qty2 && x.Value.Value == "100"));
			Assert(invoiceLineData.AddInfoCollection.Any(x => x.Key.Value == Constants.AddInfoKeys.InvoiceLine.Qty3 && x.Value.Value == "241"));
			Assert(invoiceLineData.AddInfoCollection.Any(x => x.Key.Value == Constants.AddInfoKeys.InvoiceLine.Qty2UM && x.Value.Value == "KG"));
			Assert(invoiceLineData.AddInfoCollection.Any(x => x.Key.Value == Constants.AddInfoKeys.InvoiceLine.Qty3UM && x.Value.Value == "NO"));
		}

		public void TestCFIAPGACusCodeDataMappings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var pgaheader = invoiceLine.CFIAPGAHeader;
			var airLine1 = pgaheader.AIRSRegistrationNumbers.AddNew();
			airLine1.CY_Code = "01";
			airLine1.CY_Data = "1234567890";
			var airLine2 = pgaheader.AIRSRegistrationNumbers.AddNew();
			airLine2.CY_Code = "02";
			airLine2.CY_Data = "2345678901";

			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals(1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);

				var invoiceLineData = declarationData.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.First();
				var pgaData = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => (x.Type.Code ?? ZString.Empty) == CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader);
				AssertNotNull(pgaData);
				AssertNotNull(pgaData.CustomsReferenceCollection);
				var customsReference = pgaData.CustomsReferenceCollection.Where(x => (x.Type.Code ?? ZString.Empty) == CusCodeDataTypeList.Codes.AIRSNumber);
				AssertEquals(2, customsReference.Count());

				var first = customsReference.FirstOrDefault(x => x.Reference.Value == "1234567890");
				AssertNotNull(first);
				AssertEquals("01", first.SubType.Code.Value);
				AssertEquals(CusCodeDataTypeList.Descriptions.AIRSNumber, first.Type.Description.Value);

				var second = customsReference.FirstOrDefault(x => x.Reference.Value == "2345678901");
				AssertNotNull(second);
				AssertEquals("02", second.SubType.Code.Value);
				AssertEquals(CusCodeDataTypeList.Descriptions.AIRSNumber, second.Type.Description.Value);
			});
		}

		public void TestImportInvoiceMappings()
		{
			var now = ZDateTime.Now;

			var currency = RefCurrency.LoadFromCurrencyCode(Factory.BOFactory, Core.Constants.CurrencyCodes.UnitedStates) ?? Factory.New<RefCurrency>();

			if (!currency.IsInDatabase)
			{
				currency.FillWithValidTestData();
				currency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			}

			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = now.AddDays(-2);
			rate.RE_ExpiryDate = now.AddDays(2);
			rate.RE_SellRate = 2;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;

			Factory.SaveForTesting();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "37363536D";
			invoice.JZ_InvoiceAmount = 4000m;
			invoice.JZ_Weight = 25.70m;
			invoice.JZ_WeightUQ = UnitOfWeightList.Codes.Deciton;
			invoice.JZ_NetWeight = 24.90m;
			invoice.JZ_NetWeightUQ = UnitOfWeightList.Codes.Kiloton;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = Common.US.USStatesList.Codes.California;

			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "INCCONSIGNEE";
			invoice.JZ_OH_Consignee = consigneeOrg.PK;
			var consigneeAddress = consigneeOrg.Addresses.AddNew();
			consigneeAddress.Address1 = "CONSIGNEE ADDRESS";
			invoice.FinalConsigneeAddress.E2_OA_Address = consigneeAddress.PK;

			var purchaserOrg = Factory.New<OrgHeader>();
			purchaserOrg.OH_Code = "INCBUYER";
			invoice.JZ_OH_Buyer = purchaserOrg.PK;
			var purchaserAddress = purchaserOrg.Addresses.AddNew();
			purchaserAddress.Address1 = "BUYER ADDRESS";
			invoice.BuyerDocumentaryAddress.E2_OA_Address = purchaserAddress.PK;

			var exporterOrg = Factory.New<OrgHeader>();
			exporterOrg.OH_Code = "INCEXPORTER";
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporterOrg.PK;
			var exporterAddress = exporterOrg.Addresses.AddNew();
			exporterAddress.Address1 = "EXPORTER ADDRESS";
			invoice.ExporterDocumentaryAddress.E2_OA_Address = exporterAddress.PK;

			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.OH_Code = "INCSHIPPER";
			shipperOrg.Addresses[0].OA_RN_NKCountryCode = "US";
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipperOrg.PK;

			var manufacturerOrg = Factory.New<OrgHeader>();
			manufacturerOrg.OH_Code = "INCMANF";
			var manufacturerAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerAddress.Address1 = "MANUFACRER ADDRESS";
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			var originatorOrg = Factory.New<OrgHeader>();
			originatorOrg.OH_Code = "INCORIGIN";
			originatorOrg.MainAddress.OA_Address1 = "ORIGINATOR ADDRESS";
			invoice.CommercialInvoiceOriginator.E2_OA_Address = originatorOrg.MainAddress.PK;

			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_CompanyName = "VENDOR";
			invoice.SupplierDocumentaryAddress.E2_Address1 = "VENDOR ADDRESS";
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "US";

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2402100090";
			invoiceLine.CA_99TariffCode = "9901";
			invoiceLine.CA_CustomsValue = 100m;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;
			invoiceLine.CA_AuthorityNumber = "SPEC AUTHORITY";
			invoiceLine.CA_TRSNumber = "TRAS NUM";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = Common.US.USStatesList.Codes.California;
			invoiceLine.CA_CompliantCompletion = true;
			invoiceLine.CA_CompliantImportDate = true;

			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals(1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);

				var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
				AssertEquals("Net Weight", "24.9", invoiceData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceHeader.NetWeight).Value);
				AssertEquals("Net Weight UQ", "KT", invoiceData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceHeader.NetWeightUQ).Value);
				AssertEquals("Country/Region Of Origin", "US", invoiceData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceHeader.CountryOfOrigin).Value);
				AssertEquals("State Of Origin", "CA", invoiceData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceHeader.ProvinceOfOrigin).Value);

				var organizationCollection = invoiceData.OrganizationAddressCollection;
				AssertEquals("Consignee", "INCCONSIGNEE", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.Consignee).OrganizationCode);
				AssertEquals("Shipper", "INCSHIPPER", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.Shipper).OrganizationCode);
				AssertEquals("BuyerDocumentaryAddress", "BUYER ADDRESS", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "BuyerDocumentaryAddress").Address1);
				AssertEquals("ExportBrokerDocumentaryAddress", "EXPORTER ADDRESS", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "Exporter").Address1);
				AssertEquals("ManufacturerDocumentaryAddress", "MANUFACRER ADDRESS", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "Manufacturer").Address1);
				AssertEquals("FinalConsigneeAddress", "CONSIGNEE ADDRESS", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "FinalConsigneeAddress").Address1);
				AssertEquals("SupplierPickupDeliveryAddress", "INCSHIPPER", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "SupplierPickupDeliveryAddress").OrganizationCode);
				AssertEquals("CommercialInvoiceOriginator", "INCORIGIN", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "CommercialInvoiceOriginator").OrganizationCode);
				var vendorData = organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == "SupplierDocumentaryAddress");
				AssertEquals("SupplierDocumentaryAddress", "VENDOR", vendorData.CompanyName);
				AssertEquals("SupplierDocumentaryAddress", "VENDOR ADDRESS", vendorData.Address1);
				AssertEquals("SupplierDocumentaryAddress", true, vendorData.AddressOverride);

				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
				AssertEquals("State", "CA", invoiceLineData.StateOfOrigin.Code);
				AssertEquals("Tariff Code", "9901", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "99TariffCode").Value);
				AssertEquals("Value for Duty Code", "13", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ValueForDutyCode).Value);
				AssertEquals("Special Auth/Pmt", "SPEC AUTHORITY", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.AuthorityNumber).Value);
				AssertEquals("TRS Number", "TRAS NUM", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.TRSNumber).Value);
				AssertEquals("Compliant Completion", "Y", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.IsCompliantCompletion).Value);
				AssertEquals("Import Date Compliant", "Y", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.IsImportDateCompliant).Value);
				AssertEquals("CustomsValue In USD", "50", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.CustomsValueInUSD).Value);
				AssertEquals("State of Origin", "CA", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin).Value);
			});
		}

		public void TestLVXInvoiceMappings()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_PeriodMonth = 9;
			declaration.JE_PeriodYear = 2015;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "LVX1111";
			invoice.JZ_ValuationDateOverride = new ZDateTime(2015, 9, 28);
			invoice.JZ_InvoiceDate = new ZDateTime(2015, 9, 30);
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Australia;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "INCIMPORT";
			invoice.JZ_OH_Buyer = importerOrg.PK;

			var vendorOrg = Factory.New<OrgHeader>();
			vendorOrg.OH_Code = "INCVENDOR";
			vendorOrg.Addresses[0].OA_RN_NKCountryCode = "AU";
			invoice.JZ_OH_Supplier = vendorOrg.PK;

			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2402100090";
			invoiceLine.CA_99TariffCode = "9901";
			invoiceLine.CA_AuthorityNumber = "SPEC AUTH";
			invoiceLine.CA_TRSNumber = "TRS NO";

			Factory.SaveForTesting();

			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals(1, declarationData.CommercialInfo.CommercialInvoiceCollection.Count);

				var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
				AssertEquals("Carrier Date", invoice.JZ_InvoiceDate, invoiceData.InvoiceDate);
				AssertEquals("Country/Region of Origin", "AU", invoiceData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceHeader.CountryOfOrigin).Value);

				var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
				AssertEquals("Tariff Code", "9901", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == "99TariffCode").Value);
				AssertEquals("Special Auth/Pmt", "SPEC AUTH", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.AuthorityNumber).Value);
				AssertEquals("TRS Number", "TRS NO", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.TRSNumber).Value);
			});
		}

		public void TestPopulateBondedWarehouseDetails()
		{
			var helper = new WhsDataTestHelper(Factory.BOFactory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_PreviousEntryNumber = "1234500006789";
			invoiceLine.JI_PreviousEntryLineNumber = 1;

			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.BWI, declaration);
			AssertNull("BondedWarehouseQuantity", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
			AssertNull("BondedWarehouseQuantityUnit", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantityUnit);

			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
			AssertEquals("Warehouse Previous Tran Number", "1234500006789", invoiceLineData.PreviousEntryNumber.Value);
			AssertEquals("Warehouse Previous Tran Line", (ZShort)1, invoiceLineData.PreviousEntryLineNumber.Value);

			invoiceLine.JI_OP = helper.Part.PK;
			Factory.SaveForTesting();
			declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.BWI, declaration);
			AssertEquals("BondedWarehouseQuantity", 1000m, declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "KG", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantityUnit.Code);
		}

		public void TestPopulateBondedWarehouseDetailsForCAD()
		{
			var helper = new WhsDataTestHelper(Factory.BOFactory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_PreviousEntryNumber = "1234500006789";
			invoiceLine.JI_PreviousEntryLineNumber = 1;

			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.BWI, declaration);
			AssertNull("BondedWarehouseQuantity", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
			AssertNull("BondedWarehouseQuantityUnit", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantityUnit);

			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
			AssertEquals("Warehouse Previous Tran Number", "1234500006789", invoiceLineData.PreviousEntryNumber.Value);
			AssertEquals("Warehouse Previous Tran Line", (ZShort)1, invoiceLineData.PreviousEntryLineNumber.Value);

			invoiceLine.JI_OP = helper.Part.PK;
			Factory.SaveForTesting();
			declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.BWI, declaration);
			AssertEquals("BondedWarehouseQuantity", 1000m, declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "KG", declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantityUnit.Code);
		}

		public void TestOldCategorySchemaIsOutputForHCHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "112233HH";
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Test";
			invoiceLine.CA_HCInd = "Y";
			invoiceLine.JI_CountryOfOrigin = "CA";
			var hcHeader = invoiceLine.HCPGAHeader;
			hcHeader.CA_APIProgramInd = "Y";
			hcHeader.CA_BBCProgramInd = "Y";
			hcHeader.CA_CTOProgramInd = "Y";
			hcHeader.CA_CPRProgramInd = "Y";
			hcHeader.CA_DSEProgramInd = "Y";
			hcHeader.CA_HDRProgramInd = "Y";
			hcHeader.CA_NHPProgramInd = "Y";
			hcHeader.CA_OCSProgramInd = "Y";
			hcHeader.CA_MDEProgramInd = "Y";
			hcHeader.CA_PESProgramInd = "Y";
			hcHeader.CA_REDProgramInd = "Y";
			hcHeader.CA_VETProgramInd = "Y";
			Factory.SaveForTesting();

			var propertyNames = new List<string>(new[]
			{
				HCPGAHeader.Schema.CA_CategoryAPI, HCPGAHeader.Schema.CA_CategoryBBC,
				HCPGAHeader.Schema.CA_CategoryCPR, HCPGAHeader.Schema.CA_CategoryCTO,
				HCPGAHeader.Schema.CA_CategoryDSE, HCPGAHeader.Schema.CA_CategoryHDR,
				HCPGAHeader.Schema.CA_CategoryMDE, HCPGAHeader.Schema.CA_CategoryNHP,
				HCPGAHeader.Schema.CA_CategoryOCS, HCPGAHeader.Schema.CA_CategoryPES,
				HCPGAHeader.Schema.CA_CategoryRED, HCPGAHeader.Schema.CA_CategoryVET
			});
			var removedList = new List<string>();
			var assertListsData = new Action<string, List<AddInfo>>((cCode, addInfoCol) =>
			{
				propertyNames.ForEach(x =>
				{
					var key = x.Substring(3);
					AssertEquals(key, cCode, addInfoCol.GetZStringValue(key));
				});
			});
			var categoryCode = "HC" + propertyNames.Count.ToString().PadLeft(2, '0');
			propertyNames.ForEach(x => hcHeader[x] = categoryCode);
			Factory.SaveForTesting();
			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
			var hcElement = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAHCPGAHeader);
			var addInfoCollection = hcElement.AddInfoCollection;
			AssertEquals(categoryCode, addInfoCollection.GetZStringValue(Constants.AddInfoKeys.HCPGAHeader.Category));
			assertListsData(categoryCode, addInfoCollection);

			var categoryName = propertyNames[0];
			propertyNames.RemoveAt(0);
			removedList.Add(categoryName);
			hcHeader[categoryName] = "HC!@";
			Factory.SaveForTesting();
			declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();
			hcElement = invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAHCPGAHeader);
			addInfoCollection = hcElement.AddInfoCollection;

			AssertNull("Category", addInfoCollection.GetZStringValue(Constants.AddInfoKeys.HCPGAHeader.Category));
			assertListsData(categoryCode, addInfoCollection);
			removedList.ForEach(x =>
			{
				var key = x.Substring(3);
				AssertEquals("HC!@", addInfoCollection.GetZStringValue(key));
			});
		}

		public void TestImporterDeclarationCodeMappings()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(new BusinessObjectFactory(), "TC", "4011100000", "TPR");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_TCInd = "Y";
			invoiceLine.JI_Tariff = "4011100000";
			invoiceLine.JI_CountryOfOrigin = "US";
			var tcPGAHeader = invoiceLine.TCPGAHeader;
			tcPGAHeader.CA_TPRProgramInd = "Y";
			tcPGAHeader.CA_ImportReasonCode = TCIntendedUseCodes.Codes.TC01;
			tcPGAHeader.CA_ProductType = TCProductCategories.Codes.TC04;
			tcPGAHeader.CA_ProductClass = TCProductCategories.Codes.TC01;
			tcPGAHeader.CA_VehicleCondition = TCVehicleConditions.Codes.TC17;
			tcPGAHeader.IsZZImporterDeclared = true;
			Factory.SaveForTesting();

			AssertEquals("CA_ImporterDeclarationCode", "TC01", tcPGAHeader.CA_ImporterDeclarationCode);

			var declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();

			AssertEquals("Importer Declaration Code 2", "TC01", invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CATCPGAHeader).AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ImporterDeclarationCode2).Value);

			tcPGAHeader.IsUSImporterDeclared = true;
			Factory.SaveForTesting();

			AssertEquals("CA_ImporterDeclarationCode", "TC02", tcPGAHeader.CA_ImporterDeclarationCode);

			declarationData = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.First();
			invoiceLineData = invoiceData.CommercialInvoiceLineCollection.First();

			AssertEquals("Importer Declaration Code", "TC02", invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CATCPGAHeader).AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ImporterDeclarationCode).Value);
		}
	}
}
