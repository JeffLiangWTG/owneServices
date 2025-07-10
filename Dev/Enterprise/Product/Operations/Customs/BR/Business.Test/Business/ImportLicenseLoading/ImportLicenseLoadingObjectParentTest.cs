using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseLoadingObjectParent))]
	sealed class ImportLicenseLoadingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadXMLFileLogDetails()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "00000000000001";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
			importLicenseParent.AddLog = AppendLog(messageBuilder);

			using (var wrongFile = new MemoryStream(Encoding.UTF8.GetBytes(wrongXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", wrongFile);
				AssertContains("Log must contain the message: Unable to read Response.xml", "Unable to read Response.xml", messageBuilder.ToString());

				importLicenseParent.CreateDataFromXml();
				AssertContains("Log must contain the message: Valid response file has not been loaded.", "Valid response file has not been loaded.", messageBuilder.ToString());
			}

			messageBuilder.Clear();
			using (var rightXML = new MemoryStream(Encoding.UTF8.GetBytes(RightXML)))
			{
				importLicenseParent.LoadAndValidateXML("ResponseRight.xml", rightXML);
				AssertContains("System should not match the importer number", "The Importer Registration Number contained in the XML file (08.264.406/0001-93) does not match the Importer of this Job.", messageBuilder.ToString());
			}

			messageBuilder.Clear();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";
			using (var rightXML = new MemoryStream(Encoding.UTF8.GetBytes(RightXML)))
			{
				importLicenseParent.LoadAndValidateXML("ResponseRight.xml", rightXML);
				AssertContains("Log must contain the message: File loading complete!", "File loading complete!", messageBuilder.ToString());
			}
		}

		public void TestLoadRightXMLFile()
		{
			var date = ZDateTime.Now;
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var codesCountry = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CL", "158"),
				new KeyValuePair<string, string>("US", "220"),
				new KeyValuePair<string, string>("BR", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codesCountry);

			var codesCurrency = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codesCurrency);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(RightXML)))
			{
				var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);

				importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
				AssertContains("Log must contain the message: File loading complete!", "File loading complete!", messageBuilder.ToString());

				var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;
				AssertNotNull("importLicenseLoadingObject must not be null", importLicenseLoadingObject);

				importLicenseLoadingObject.InvoiceHeaderPK = invoiceHeader.PK;
				importLicenseLoadingObject.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
				importLicenseLoadingObject.ImportLicenseAuthorizationDate = date;
				importLicenseLoadingObject.ImportLicenseFeeType = "F1ND";
				CombineAssertions(() =>
				{
					AssertEquals("ImportLicenseNo must be ", "22/1070364-8", importLicenseLoadingObject.ImportLicenseNo);
					AssertEquals("RegistrationDate must be ", new ZDateTime(2022, 4, 25), importLicenseLoadingObject.RegistrationDate);
					AssertEquals("InvoiceNoPK must be ", invoiceHeader.PK, importLicenseLoadingObject.InvoiceHeaderPK);
					AssertEquals("Incoterm must be ", "FCA", importLicenseLoadingObject.Incoterm);
					AssertEquals("Currency must be ", "USD", importLicenseLoadingObject.Currency);
					AssertEquals("VMLE must be ", 41840m, importLicenseLoadingObject.VMLE);
					AssertEquals("VMCV must be ", 41840m, importLicenseLoadingObject.VMCV);
					AssertEquals("NetWeight must be ", 13815m, importLicenseLoadingObject.NetWeight);
					AssertEquals("UQ must be ", "KG", importLicenseLoadingObject.UQ);
					AssertEquals("Tariff must be ", "22042100", importLicenseLoadingObject.Tariff);
					AssertEquals("ExchangeHedging must be ", "1", importLicenseLoadingObject.ExchangeHedging);
					AssertEquals("ImportLicenseLoadingObjectNcmDetailsCollection must have 3", 3, importLicenseLoadingObject.ImportLicenseLoadingObjectNcmDetailsCollection.Count);
					Assert("NcmCode must be Empty", importLicenseLoadingObject.NcmCode.IsEmpty);
					AssertEquals("ManufacturerIndicator must be ", "3", importLicenseLoadingObject.ManufacturerIndicator);
					AssertEquals("NaladiHs must be ", "22042110", importLicenseLoadingObject.NaladiHs);
					AssertEquals("GoodsCondition must be ", "N", importLicenseLoadingObject.GoodsCondition);
					AssertEquals("DutyTaxRegime must be ", "1", importLicenseLoadingObject.DutyTaxRegime);
					AssertEquals("DutyLegalBase must be ", "01", importLicenseLoadingObject.DutyLegalBase);
					AssertEquals("ExchangeHedgeFinancialInstitution must be ", "01", importLicenseLoadingObject.ExchangeHedgeFinancialInstitution);
					AssertEquals("ExchangeHedgeReason must be ", "30", importLicenseLoadingObject.ExchangeHedgeReason);
					AssertEquals("ImportLicenseType must be ", ImportLicenseType.Codes.PreBoarding, importLicenseLoadingObject.ImportLicenseType);
					AssertEquals("ImportLicenseAuthorizationDate must be ", date, importLicenseLoadingObject.ImportLicenseAuthorizationDate);
					AssertEquals("ImportLicenseFeeType must be ", "F1ND", importLicenseLoadingObject.ImportLicenseFeeType);
					AssertEquals("GoodsOrigin must be ", "CL", importLicenseLoadingObject.GoodsOrigin);
					AssertEquals("AdditionalTariffsType must be ", "MX99", importLicenseLoadingObject.AdditionalTariffsType);
				});
			}
		}

		public void TestCreateDataFromXmlCorrectFileWithEntryInstructionAndInvoiceHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER S.A";

			var manufacture = Factory.NewWithValidTestData<OrgHeader>();
			manufacture.OH_FullName = "MANUFACTURE S.A";

			var date = ZDateTime.Now;
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var codesCountry = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CL", "158"),
				new KeyValuePair<string, string>("US", "220"),
				new KeyValuePair<string, string>("BR", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codesCountry);

			var codesCurrency = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codesCurrency);

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(RightXML)))
			{
				var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);

				importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
				AssertContains("Log must contain the message: File loading complete!", "(100/100) File loading complete!", messageBuilder.ToString());

				var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;
				AssertNotNull("importLicenseLoadingObject must not be null", importLicenseLoadingObject);

				messageBuilder.Clear();
				importLicenseLoadingObject.InvoiceHeaderPK = invoiceHeader.PK;
				importLicenseLoadingObject.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
				importLicenseLoadingObject.ImportLicenseAuthorizationDate = date;
				importLicenseLoadingObject.ImportLicenseFeeType = "F1ND";
				importLicenseLoadingObject.SupplierAddressPK = supplier.MainAddress.PK;
				importLicenseLoadingObject.ManufacturerAddressPK = manufacture.MainAddress.PK;
				importLicenseLoadingObject.ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
				importLicenseParent.CreateDataFromXml();

				AssertEquals("InvoiceHeader should have 3 invoice lines", 3, invoiceHeader.InvoiceLines.Count);

				var invoiceLine1 = invoiceHeader.InvoiceLines[0];
				var invoiceLine2 = invoiceHeader.InvoiceLines[1];
				var invoiceLine3 = invoiceHeader.InvoiceLines[2];
				CombineAssertions(() =>
				{
					AssertEquals("invoiceHeader.IncoTerm should be ", "FCA", invoiceHeader.IncoTerm);
					AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency  should be ", "USD", invoiceHeader.JZ_RX_NKInvoice_Currency);
					AssertEquals("invoiceHeader.JZ_NetWeight should be ", 13815m, invoiceHeader.JZ_NetWeight);
					AssertEquals("invoiceHeader.JZ_NetWeightUQ should be ", "KG", invoiceHeader.JZ_NetWeightUQ);
					AssertEquals("invoiceHeader.JZ_InvoiceAmount should be ", 41840m, invoiceHeader.JZ_InvoiceAmount);
					AssertEquals("invoiceHeader.ExchangeHedgeType should be ", "1", invoiceHeader.ExchangeHedgeType);
					AssertEquals("invoiceHeader.ExchangeHedgeFinancialInstitution should be ", "01", invoiceHeader.ExchangeHedgeFinancialInstitution);
					AssertEquals("invoiceHeader.ExchangeHedgeReason should be ", "30", invoiceHeader.ExchangeHedgeReason);
					AssertEquals("invoiceHeader.JZ_OA_SupplierAddress should be ", importLicenseLoadingObject.SupplierAddressPK, invoiceHeader.JZ_OA_SupplierAddress);

					AssertEquals("invoiceLine1.JI_CEI should be ", entryInstruction.PK, invoiceLine1.JI_CEI);
					AssertEquals("invoiceLine1.JI_JZ should be ", invoiceHeader.PK, invoiceLine1.JI_JZ);
					AssertEquals("invoiceLine1.JI_Tariff should be ", "22042100", invoiceLine1.JI_Tariff);
					AssertEquals("invoiceLine1.JI_LineNo should be ", (ZShort)1, invoiceLine1.JI_LineNo);
					AssertEquals("invoiceLine1.JI_InvoiceUQ should be ", "BOX", invoiceLine1.JI_InvoiceUQ);
					AssertEquals("invoiceLine1.JI_NetWeight should be ", 5625m, invoiceLine1.JI_NetWeight);
					AssertEquals("invoiceLine1.JI_InvoiceQuantity should be ", 1250m, invoiceLine1.JI_InvoiceQuantity);
					AssertEquals("invoiceLine1.JI_CustomsQuantity should be ", 5625m, invoiceLine1.JI_CustomsQuantity);
					AssertEquals("invoiceLine1.JI_LinePrice should be ", 20000m, invoiceLine1.JI_LinePrice);
					AssertEquals("invoiceLine1.FullGoodsDescription should be ", "1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine1.FullGoodsDescription);
					AssertEquals("invoiceLine1.ImportLicenseNumber should be ", "2210703648", invoiceLine1.ImportLicenseNumber);
					Assert("invoiceLine1.TariffDetachCollection should NOT contain any element", !invoiceLine1.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine1.JI_ManufacturerIndicator should be ", "2", invoiceLine1.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine1.NaladiHs should be ", "22042110", invoiceLine1.NaladiHs);
					AssertEquals("invoiceLine1.JI_GoodsCondition should be ", ZString.Empty, invoiceLine1.JI_GoodsCondition);
					AssertEquals("invoiceLine1.DutyTaxRegime should be ", "1", invoiceLine1.DutyTaxRegime);
					AssertEquals("invoiceLine1.DutyLegalBase should be ", "01", invoiceLine1.DutyLegalBase);
					AssertEquals("invoiceLine1.ImportLicenseType must be ", ImportLicenseType.Codes.PreBoarding, invoiceLine1.ImportLicenseType);
					AssertEquals("invoiceLine1.AuthorizationDate must be ", date, invoiceLine1.ImportLicenseAuthorizationDate);
					AssertEquals("invoiceLine1.FeeType must be ", "F1ND", invoiceLine1.ImportLicenseFeeType);
					AssertEquals("invoiceLine1.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine1.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine1.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine1.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine1.JI_OA_ManufacturerAddress should be ", importLicenseLoadingObject.ManufacturerAddressPK, invoiceLine1.JI_OA_ManufacturerAddress);

					AssertEquals("invoiceLine2.JI_CEI should be ", entryInstruction.PK, invoiceLine2.JI_CEI);
					AssertEquals("invoiceLine2.JI_JZ should be ", invoiceHeader.PK, invoiceLine2.JI_JZ);
					AssertEquals("invoiceLine2.JI_Tariff should be ", "22042100", invoiceLine2.JI_Tariff);
					AssertEquals("invoiceLine2.JI_LineNo should be ", (ZShort)2, invoiceLine2.JI_LineNo);
					AssertEquals("invoiceLine2.JI_InvoiceUQ should be ", "BOX", invoiceLine2.JI_InvoiceUQ);
					AssertEquals("invoiceLine2.JI_NetWeight should be ", 4410m, invoiceLine2.JI_NetWeight);
					AssertEquals("invoiceLine2.JI_InvoiceQuantity should be ", 980m, invoiceLine2.JI_InvoiceQuantity);
					AssertEquals("invoiceLine2.JI_CustomsQuantity should be ", 4410m, invoiceLine2.JI_CustomsQuantity);
					AssertEquals("invoiceLine2.JI_LinePrice should be ", 11760m, invoiceLine2.JI_LinePrice);
					AssertEquals("invoiceLine2.JI_Description should be ", "980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine2.JI_Description);
					AssertEquals("invoiceLine2.ImportLicenseNumber should be ", "2210703648", invoiceLine2.ImportLicenseNumber);
					Assert("invoiceLine2.TariffDetachCollection should NOT contain any element", !invoiceLine2.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine2.JI_ManufacturerIndicator should be ", "2", invoiceLine2.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine2.NaladiHs should be ", "22042110", invoiceLine2.NaladiHs);
					AssertEquals("invoiceLine2.JI_GoodsCondition should be ", ZString.Empty, invoiceLine2.JI_GoodsCondition);
					AssertEquals("invoiceLine2.DutyTaxRegime should be ", "1", invoiceLine2.DutyTaxRegime);
					AssertEquals("invoiceLine2.DutyLegalBase should be ", "01", invoiceLine2.DutyLegalBase);
					AssertEquals("invoiceLine2.ImportLicenseType must be ", ImportLicenseType.Codes.PreBoarding, invoiceLine2.ImportLicenseType);
					AssertEquals("invoiceLine2.AuthorizationDate must be ", date, invoiceLine2.ImportLicenseAuthorizationDate);
					AssertEquals("invoiceLine2.FeeType must be ", "F1ND", invoiceLine2.ImportLicenseFeeType);
					AssertEquals("invoiceLine2.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine2.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine2.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine2.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine2.JI_OA_ManufacturerAddress should be ", importLicenseLoadingObject.ManufacturerAddressPK, invoiceLine2.JI_OA_ManufacturerAddress);

					AssertEquals("invoiceLine3.JI_CEI should be ", entryInstruction.PK, invoiceLine3.JI_CEI);
					AssertEquals("invoiceLine3.JI_JZ should be ", invoiceHeader.PK, invoiceLine3.JI_JZ);
					AssertEquals("invoiceLine3.JI_Tariff should be ", "22042100", invoiceLine3.JI_Tariff);
					AssertEquals("invoiceLine3.JI_LineNo should be ", (ZShort)3, invoiceLine3.JI_LineNo);
					AssertEquals("invoiceLine3.JI_InvoiceUQ should be ", "BOX", invoiceLine3.JI_InvoiceUQ);
					AssertEquals("invoiceLine3.JI_NetWeight should be ", 3780m, invoiceLine3.JI_NetWeight);
					AssertEquals("invoiceLine3.JI_InvoiceQuantity should be ", 840m, invoiceLine3.JI_InvoiceQuantity);
					AssertEquals("invoiceLine3.JI_CustomsQuantity should be ", 3780m, invoiceLine3.JI_CustomsQuantity);
					AssertEquals("invoiceLine3.JI_LinePrice should be ", 10080m, invoiceLine3.JI_LinePrice);
					AssertEquals("invoiceLine3.JI_Description should be ", "840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine3.JI_Description);
					AssertEquals("invoiceLine3.ImportLicenseNumber should be ", "2210703648", invoiceLine3.ImportLicenseNumber);
					Assert("invoiceLine3.TariffDetachCollection should NOT contain any element", !invoiceLine3.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine3.JI_ManufacturerIndicator should be ", "2", invoiceLine3.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine3.NaladiHs should be ", "22042110", invoiceLine3.NaladiHs);
					AssertEquals("invoiceLine3.JI_GoodsCondition should be ", ZString.Empty, invoiceLine3.JI_GoodsCondition);
					AssertEquals("invoiceLine3.DutyTaxRegime should be ", "1", invoiceLine3.DutyTaxRegime);
					AssertEquals("invoiceLine3.DutyLegalBase should be ", "01", invoiceLine3.DutyLegalBase);
					AssertEquals("invoiceLine3.ImportLicenseType must be ", ImportLicenseType.Codes.PreBoarding, invoiceLine3.ImportLicenseType);
					AssertEquals("invoiceLine3.AuthorizationDate must be ", date, invoiceLine3.ImportLicenseAuthorizationDate);
					AssertEquals("invoiceLine3.FeeType must be ", "F1ND", invoiceLine3.ImportLicenseFeeType);
					AssertEquals("invoiceLine3.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine3.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine3.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine3.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine3.JI_OA_ManufacturerAddress should be ", importLicenseLoadingObject.ManufacturerAddressPK, invoiceLine3.JI_OA_ManufacturerAddress);

					AssertEquals("Log must contain the message: Invoice Lines for Invoice Header INV1 data is loaded!", @"(1/4) |(2/4) |(3/4) |(4/4) Invoice Lines for Invoice Header INV1 data is loaded!", messageBuilder.ToStringWithDelimiterBetweenAppends("|"));
				});
			}
		}

		public void TestCreateDataFromXmlCorrectFileWithoutEntryInstructionCreatedAndInvoiceHeaderSelection()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var codesCountry = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CL", "158"),
				new KeyValuePair<string, string>("US", "220"),
				new KeyValuePair<string, string>("BR", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codesCountry);

			var codesCurrency = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codesCurrency);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(RightXML)))
			{
				var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);

				importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
				AssertContains("Log must contain the message: File loading complete!", "(100/100) File loading complete!", messageBuilder.ToString());

				var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;
				AssertNotNull("importLicenseLoadingObject must not be null", importLicenseLoadingObject);

				messageBuilder.Clear();
				importLicenseLoadingObject.InvoiceHeaderPK = ZGuid.Empty;
				importLicenseParent.CreateDataFromXml();

				AssertEquals("Declaration should have 1 entry instruction", 1, declaration.CustomsEntryInstructions.Count);
				AssertEquals("Declaration should have 2 invoice header", 2, declaration.Invoices.Count);
				AssertEquals("InvoiceHeader should have 3 invoice lines", 3, declaration.Invoices[1].InvoiceLines.Count);

				var entryInstruction = declaration.CustomsEntryInstructions[0];

				var newInvoiceHeader = declaration.Invoices[1];
				AssertEquals("New InvoiceHeader.JZ_InvoiceNumber should be", "22/1070364-8", newInvoiceHeader.JZ_InvoiceNumber);

				var invoiceLine1 = newInvoiceHeader.InvoiceLines[0];
				var invoiceLine2 = newInvoiceHeader.InvoiceLines[1];
				var invoiceLine3 = newInvoiceHeader.InvoiceLines[2];

				CombineAssertions(() =>
				{
					AssertEquals("invoiceHeader.IncoTerm should be ", "FCA", newInvoiceHeader.IncoTerm);
					AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency  should be ", "USD", newInvoiceHeader.JZ_RX_NKInvoice_Currency);
					AssertEquals("invoiceHeader.JZ_NetWeight should be ", 13815m, newInvoiceHeader.JZ_NetWeight);
					AssertEquals("invoiceHeader.JZ_NetWeightUQ should be ", "KG", newInvoiceHeader.JZ_NetWeightUQ);
					AssertEquals("invoiceHeader.JZ_InvoiceAmount should be ", 41840m, newInvoiceHeader.JZ_InvoiceAmount);
					AssertEquals("invoiceHeader.ExchangeHedgeType should be ", "1", newInvoiceHeader.ExchangeHedgeType);
					AssertEquals("invoiceHeader.ExchangeHedgeFinancialInstitution should be ", "01", newInvoiceHeader.ExchangeHedgeFinancialInstitution);
					AssertEquals("invoiceHeader.ExchangeHedgeReason should be ", "30", newInvoiceHeader.ExchangeHedgeReason);
					AssertEquals("invoiceHeader.JZ_OA_SupplierAddress should be ", ZGuid.Empty, newInvoiceHeader.JZ_OA_SupplierAddress);

					AssertEquals("invoiceLine1.JI_CEI should be ", entryInstruction.PK, invoiceLine1.JI_CEI);
					AssertEquals("invoiceLine1.JI_JZ should be ", newInvoiceHeader.PK, invoiceLine1.JI_JZ);
					AssertEquals("invoiceLine1.JI_Tariff should be ", "22042100", invoiceLine1.JI_Tariff);
					AssertEquals("invoiceLine1.JI_LineNo should be ", (ZShort)1, invoiceLine1.JI_LineNo);
					AssertEquals("invoiceLine1.JI_InvoiceUQ should be ", "BOX", invoiceLine1.JI_InvoiceUQ);
					AssertEquals("invoiceLine1.JI_NetWeight should be ", 5625m, invoiceLine1.JI_NetWeight);
					AssertEquals("invoiceLine1.JI_InvoiceQuantity should be ", 1250m, invoiceLine1.JI_InvoiceQuantity);
					AssertEquals("invoiceLine1.JI_CustomsQuantity should be ", 5625m, invoiceLine1.JI_CustomsQuantity);
					AssertEquals("invoiceLine1.JI_LinePrice should be ", 20000m, invoiceLine1.JI_LinePrice);
					AssertEquals("invoiceLine1.FullGoodsDescription should be ", "1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine1.FullGoodsDescription);
					AssertEquals("invoiceLine1.ImportLicenseNumber should be ", "2210703648", invoiceLine1.ImportLicenseNumber);
					Assert("invoiceLine1.TariffDetachCollection should NOT contain any element", !invoiceLine1.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine1.JI_ManufacturerIndicator should be ", "3", invoiceLine1.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine1.NaladiHs should be ", "22042110", invoiceLine1.NaladiHs);
					AssertEquals("invoiceLine1.JI_GoodsCondition should be ", ZString.Empty, invoiceLine1.JI_GoodsCondition);
					AssertEquals("invoiceLine1.DutyTaxRegime should be ", "1", invoiceLine1.DutyTaxRegime);
					AssertEquals("invoiceLine1.DutyLegalBase should be ", "01", invoiceLine1.DutyLegalBase);
					AssertEquals("invoiceLine1.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine1.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine1.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine1.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine1.JI_OA_ManufacturerAddress should be ", ZGuid.Empty, invoiceLine1.JI_OA_ManufacturerAddress);

					AssertEquals("invoiceLine2.JI_CEI should be ", entryInstruction.PK, invoiceLine2.JI_CEI);
					AssertEquals("invoiceLine2.JI_JZ should be ", newInvoiceHeader.PK, invoiceLine2.JI_JZ);
					AssertEquals("invoiceLine2.JI_Tariff should be ", "22042100", invoiceLine2.JI_Tariff);
					AssertEquals("invoiceLine2.JI_LineNo should be ", (ZShort)2, invoiceLine2.JI_LineNo);
					AssertEquals("invoiceLine2.JI_InvoiceUQ should be ", "BOX", invoiceLine2.JI_InvoiceUQ);
					AssertEquals("invoiceLine2.JI_NetWeight should be ", 4410m, invoiceLine2.JI_NetWeight);
					AssertEquals("invoiceLine2.JI_InvoiceQuantity should be ", 980m, invoiceLine2.JI_InvoiceQuantity);
					AssertEquals("invoiceLine2.JI_CustomsQuantity should be ", 4410m, invoiceLine2.JI_CustomsQuantity);
					AssertEquals("invoiceLine2.JI_LinePrice should be ", 11760m, invoiceLine2.JI_LinePrice);
					AssertEquals("invoiceLine2.JI_Description should be ", "980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine2.JI_Description);
					AssertEquals("invoiceLine2.ImportLicenseNumber should be ", "2210703648", invoiceLine2.ImportLicenseNumber);
					Assert("invoiceLine2.TariffDetachCollection should NOT contain any element", !invoiceLine2.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine2.JI_ManufacturerIndicator should be ", "3", invoiceLine2.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine2.NaladiHs should be ", "22042110", invoiceLine2.NaladiHs);
					AssertEquals("invoiceLine2.JI_GoodsCondition should be ", ZString.Empty, invoiceLine2.JI_GoodsCondition);
					AssertEquals("invoiceLine2.DutyTaxRegime should be ", "1", invoiceLine2.DutyTaxRegime);
					AssertEquals("invoiceLine2.DutyLegalBase should be ", "01", invoiceLine2.DutyLegalBase);
					AssertEquals("invoiceLine2.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine2.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine2.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine2.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine2.JI_OA_ManufacturerAddress should be ", ZGuid.Empty, invoiceLine2.JI_OA_ManufacturerAddress);

					AssertEquals("invoiceLine3.JI_CEI should be ", entryInstruction.PK, invoiceLine3.JI_CEI);
					AssertEquals("invoiceLine3.JI_JZ should be ", newInvoiceHeader.PK, invoiceLine3.JI_JZ);
					AssertEquals("invoiceLine3.JI_Tariff should be ", "22042100", invoiceLine3.JI_Tariff);
					AssertEquals("invoiceLine3.JI_LineNo should be ", (ZShort)3, invoiceLine3.JI_LineNo);
					AssertEquals("invoiceLine3.JI_InvoiceUQ should be ", "BOX", invoiceLine3.JI_InvoiceUQ);
					AssertEquals("invoiceLine3.JI_NetWeight should be ", 3780m, invoiceLine3.JI_NetWeight);
					AssertEquals("invoiceLine3.JI_InvoiceQuantity should be ", 840m, invoiceLine3.JI_InvoiceQuantity);
					AssertEquals("invoiceLine3.JI_CustomsQuantity should be ", 3780m, invoiceLine3.JI_CustomsQuantity);
					AssertEquals("invoiceLine3.JI_LinePrice should be ", 10080m, invoiceLine3.JI_LinePrice);
					AssertEquals("invoiceLine3.JI_Description should be ", "840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL", invoiceLine3.JI_Description);
					AssertEquals("invoiceLine3.ImportLicenseNumber should be ", "2210703648", invoiceLine3.ImportLicenseNumber);
					Assert("invoiceLine3.TariffDetachCollection should NOT contain any element", !invoiceLine3.TariffDetachs.Cast<TariffDetach>().Any());
					AssertEquals("invoiceLine3.JI_ManufacturerIndicator should be ", "3", invoiceLine3.JI_ManufacturerIndicator);
					AssertEquals("invoiceLine3.NaladiHs should be ", "22042110", invoiceLine3.NaladiHs);
					AssertEquals("invoiceLine3.JI_GoodsCondition should be ", ZString.Empty, invoiceLine3.JI_GoodsCondition);
					AssertEquals("invoiceLine3.DutyTaxRegime should be ", "1", invoiceLine3.DutyTaxRegime);
					AssertEquals("invoiceLine3.DutyLegalBase should be ", "01", invoiceLine3.DutyLegalBase);
					AssertEquals("invoiceLine3.AdditionalTariffs[0].LegalActSubject should be ", AdditionalTaxTypeList.Codes.TariffAgreement, invoiceLine3.AdditionalTariffs[0].LegalActSubject);
					AssertEquals("invoiceLine3.AdditionalTariffs[0].TariffType should be ", "MX99", invoiceLine3.AdditionalTariffs[0].TariffType);
					AssertEquals("invoiceLine3.JI_OA_ManufacturerAddress should be ", ZGuid.Empty, invoiceLine3.JI_OA_ManufacturerAddress);

					AssertEquals("Log must contain the message: Invoice Lines for Invoice Header INV1 data is loaded!", @"(1/4) |(2/4) |(3/4) |(4/4) Invoice Lines for Invoice Header 22/1070364-8 data is loaded!", messageBuilder.ToStringWithDelimiterBetweenAppends("|"));
				});
			}
		}

		public void TestCreateDataFromXmlWithoutNcmDetails()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "Inst-1";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";

			using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(xmlWithout_lista_detalhe_ncm)))
			{
				var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);

				importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
				AssertContains("Log must contain the message: File loading complete!", "File loading complete!", messageBuilder.ToString());

				var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;
				AssertNotNull("importLicenseLoadingObject must not be null", importLicenseLoadingObject);

				importLicenseLoadingObject.InvoiceHeaderPK = invoiceHeader.PK;
				importLicenseParent.CreateDataFromXml();

				Assert("InvoiceHeader should NOT have invoice lines", !invoiceHeader.InvoiceLines.Any());
				AssertContains("Log must contain the message: Invoice Lines for Invoice Header INV1 were not created, because XML do not contain NCM details.", "Invoice Lines for Invoice Header INV1 were not created, because XML do not contain NCM details.", messageBuilder.ToString());
				CombineAssertions(() =>
				{
					AssertEquals("invoiceHeader.IncoTerm should be ", "FCA", invoiceHeader.IncoTerm);
					AssertEquals("invoiceHeader.JZ_RX_NKInvoice_Currency  should be ", "USD", invoiceHeader.JZ_RX_NKInvoice_Currency);
					AssertEquals("invoiceHeader.JZ_NetWeight should be ", 13815m, invoiceHeader.JZ_NetWeight);
					AssertEquals("invoiceHeader.JZ_NetWeightUQ should be ", "KG", invoiceHeader.JZ_NetWeightUQ);
					AssertEquals("invoiceHeader.JZ_InvoiceAmount should be ", 41840m, invoiceHeader.JZ_InvoiceAmount);
					AssertEquals("invoiceHeader.ExchangeHedgeType should be ", "1", invoiceHeader.ExchangeHedgeType);
					AssertEquals("invoiceHeader.ExchangeHedgeFinancialInstitution should be ", ZString.Empty, invoiceHeader.ExchangeHedgeFinancialInstitution);
					AssertEquals("invoiceHeader.ExchangeHedgeReason should be ", ZString.Empty, invoiceHeader.ExchangeHedgeReason);
				});
			}
		}

		public void TestUnknownSupplierAndManufacturerFound()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "0001";
			supplier.OH_FullName = "SUPPLIERNAME";

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "0002";
			manufacturer.OH_FullName = "MANUFACTURERNAME";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "08264406000193";

			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = importer.PK;

			Factory.Save();

			void AssertFindSupplierAndManufacturer(string manufacturerIndicator, ZString supplierName, ZString manufacturerName, ZGuid expectedSupplierAddressPK, ZGuid expectedManufacturerAddressPK, string expectedLogs)
			{
				messageBuilder.Clear();

				using (var rightFile = new MemoryStream(Encoding.UTF8.GetBytes(GetXmlByManufacturerIndicator(manufacturerIndicator, supplierName, manufacturerName))))
				{
					var importLicenseParent = new ImportLicenseLoadingObjectParent(declaration);
					importLicenseParent.UnknownSupplierCodeFound += FindOrganisationCode;
					importLicenseParent.UnknownManufacturerCodeFound += FindOrganisationCode;
					importLicenseParent.LoadAndValidateXML("Response.xml", rightFile);
					var importLicenseLoadingObject = importLicenseParent.Collection.FirstOrDefault() as ImportLicenseLoadingObject;

					CombineAssertions("ManufacturerIndicator = 1", () =>
					{
						AssertEquals("importLicenseLoadingObject.SupplierAddressPK should be ", expectedSupplierAddressPK, importLicenseLoadingObject.SupplierAddressPK);
						AssertEquals("importLicenseLoadingObject.ManufacturerAddressPK should be ", expectedManufacturerAddressPK, importLicenseLoadingObject.ManufacturerAddressPK);
						AssertEquals(expectedLogs, messageBuilder.ToStringWithDelimiterBetweenAppends(";"));
					});

					void FindOrganisationCode(object sender, UnknownOrganisationCodeEventArgs e)
					{
						e.Code = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, e.Name))?.OH_Code ?? ZString.Empty;
						messageBuilder.Append(e.Code.IsEmpty ? $"Cannot find Organization for Name {e.Name}" : $"Found Organization {e.Code} for Name {e.Name}");
					}
				}
			}

			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._1, supplier.OH_FullName, manufacturer.OH_FullName, supplier.MainAddress.PK, supplier.MainAddress.PK,
				"Found Organization 0001 for Name SUPPLIERNAME");
			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._3, supplier.OH_FullName, manufacturer.OH_FullName, supplier.MainAddress.PK, ZGuid.Empty,
				"Found Organization 0001 for Name SUPPLIERNAME");
			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._2, supplier.OH_FullName, manufacturer.OH_FullName, supplier.MainAddress.PK, manufacturer.MainAddress.PK,
				"Found Organization 0001 for Name SUPPLIERNAME;Found Organization 0002 for Name MANUFACTURERNAME");
			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._2, supplier.OH_FullName, supplier.OH_FullName, supplier.MainAddress.PK, supplier.MainAddress.PK,
				"Found Organization 0001 for Name SUPPLIERNAME");
			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._2, "SUPPLIERNAME1", "MANUFACTURERNAME1", ZGuid.Empty, ZGuid.Empty,
				"Cannot find Organization for Name SUPPLIERNAME1;Cannot find Organization for Name MANUFACTURERNAME1");
			AssertFindSupplierAndManufacturer(ManufacturerIndicatorList.Codes._2, "", "", ZGuid.Empty, ZGuid.Empty, "");
		}

		Action<int, int, string> AppendLog(ZStringBuilder messageBuilder)
		{
			return (completedCount, totalCount, messageText) => messageBuilder.Append($"({completedCount}/{totalCount}) {messageText}");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseLoadingObjectParent(Factory.New<JobDeclaration>());
		}

		#endregion

		public static string RightXML = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<resposta-consulta-li versao=""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
	<identificador-consulta>251</identificador-consulta>
	<lista-mensagens-e-erros/>
	<lista-li-completa>
		<li-completa>
			<Grupo-Dados-Basicos>
				<numero-li>22/1070364-8</numero-li>
				<Importador>
					<importador-tipo>1</importador-tipo>
					<importador-identificador>08.264.406/0001-93</importador-identificador>
					<importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
					<importador-atividade-economica/>
					<importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS. LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
					<importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
					<importador-endereco-numero>690</importador-endereco-numero>
					<importador-endereco-complemento/>
					<importador-endereco-bairro>UMBARA</importador-endereco-bairro>
					<importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
					<importador-endereco-uf>PR</importador-endereco-uf>
					<importador-endereco-cep>81930165</importador-endereco-cep>
					<importador-telefone>41 - 38882000</importador-telefone>
					<importador-pais/>
					<importador-pais-nome/>
				</Importador>
				<Outras-Informacoes>
					<pais-procedencia-mercadoria>158</pais-procedencia-mercadoria>
					<pais-procedencia-mercadoria-nome>CHILE</pais-procedencia-mercadoria-nome>
					<urf-entrada>1017500</urf-entrada>
					<urf-entrada-nome>ALF - URUGUAIANA</urf-entrada-nome>
					<urf-despacho>0917900</urf-despacho>
					<urf-despacho-nome>ALF - CURITIBA</urf-despacho-nome>
				</Outras-Informacoes>
				<Informacoes-Complementares>
					<texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100                                                                                                                                                                                             </texto-informacoes-complementares>
				</Informacoes-Complementares>
			</Grupo-Dados-Basicos>
			<Grupo-Fornecedor>
				<fornecedor-tipo>3</fornecedor-tipo>
				<pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
				<pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
				<pais-origem-mercadoria>158</pais-origem-mercadoria>
				<pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
				<Fornecedor>
					<fornecedor-estrangeiro-nome>TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
					<fornecedor-estrangeiro-email/>
					<fornecedor-estrangeiro-responsavel/>
					<fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
					<fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
					<fornecedor-estrangeiro-complemento/>
					<fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
					<fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
				</Fornecedor>
				<Fabricante>
					<fabricante-nome/>
					<fabricante-email/>
					<fabricante-responsavel/>
					<fabricante-endereco-logradoudo/>
					<fabricante-endereco-numero/>
					<fabricante-endereco-complemento/>
					<fabricante-endereco-cidade/>
					<fabricante-endereco-estado/>
				</Fabricante>
			</Grupo-Fornecedor>
			<Grupo-Mercadoria>
				<Dados-Gerais>
					<subitem-ncm>2204.21.00</subitem-ncm>
					<subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
					<unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
					<mercadoria-naladi>22042110</mercadoria-naladi>
					<mercadoria-naladi-nome/>
					<moeda>220</moeda>
					<moeda-nome>DOLAR DOS EUA</moeda-nome>
					<incoterm>FCA</incoterm>
					<incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
				</Dados-Gerais>
				<Condicao-Mercadoria>
					<condicao-mercadoria>N</condicao-mercadoria>
					<condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
					<tipo-enquadramento-material-usado/>
					<tipo-enquadramento-material-usado-nome/>
					<tipo-operacao-enquadramento-material-usado/>
					<tipo-operacao-enquadramento-material-usado-nome/>
				</Condicao-Mercadoria>
				<lista-destaque-ncm/>
				<lista-processo-anuente/>
				<Informacoes-Drawback>
					<drawback-regime>3</drawback-regime>
					<drawback-numero-ato-isencao/>
					<drawback-numero-ato-suspencao/>
				</Informacoes-Drawback>
				<lista-detalhe-ncm>
					<detalhe-ncm-item-drawback>
						<numero-sequencial-produto>1</numero-sequencial-produto>
						<nome-unidade-medida-comercializada>CAIXA</nome-unidade-medida-comercializada>
						<peso-liquido-total>5.625,00000</peso-liquido-total>
						<qtd-mercadoria-unidade-comercializada>1.250,00000</qtd-mercadoria-unidade-comercializada>
						<qtd-mercadoria-unidade-estatistica>5.625,00000</qtd-mercadoria-unidade-estatistica>
						<valor-total-local-embarque>20.000,0000000</valor-total-local-embarque>
						<valor-unitario-condicao-venda>16,0000000</valor-unitario-condicao-venda>
						<valor-total-condicao-venda>20.000,0000000</valor-total-condicao-venda>
						<descricao-produto>1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
						<marca/>
						<modelo/>
						<numero-serie/>
						<ano-fabricacao/>
						<item-ac-drawback/>
					</detalhe-ncm-item-drawback>
					<detalhe-ncm-item-drawback>
						<numero-sequencial-produto>2</numero-sequencial-produto>
						<nome-unidade-medida-comercializada>CAIXA</nome-unidade-medida-comercializada>
						<peso-liquido-total>4.410,00000</peso-liquido-total>
						<qtd-mercadoria-unidade-comercializada>980,00000</qtd-mercadoria-unidade-comercializada>
						<qtd-mercadoria-unidade-estatistica>4.410,00000</qtd-mercadoria-unidade-estatistica>
						<valor-total-local-embarque>11.760,0000000</valor-total-local-embarque>
						<valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
						<valor-total-condicao-venda>11.760,0000000</valor-total-condicao-venda>
						<descricao-produto>980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
						<marca/>
						<modelo/>
						<numero-serie/>
						<ano-fabricacao/>
						<item-ac-drawback/>
					</detalhe-ncm-item-drawback>
					<detalhe-ncm-item-drawback>
						<numero-sequencial-produto>3</numero-sequencial-produto>
						<nome-unidade-medida-comercializada>CAIXA</nome-unidade-medida-comercializada>
						<peso-liquido-total>3.780,00000</peso-liquido-total>
						<qtd-mercadoria-unidade-comercializada>840,00000</qtd-mercadoria-unidade-comercializada>
						<qtd-mercadoria-unidade-estatistica>3.780,00000</qtd-mercadoria-unidade-estatistica>
						<valor-total-local-embarque>10.080,0000000</valor-total-local-embarque>
						<valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
						<valor-total-condicao-venda>10.080,0000000</valor-total-condicao-venda>
						<descricao-produto>840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
						<marca/>
						<modelo/>
						<numero-serie/>
						<ano-fabricacao/>
						<item-ac-drawback/>
					</detalhe-ncm-item-drawback>
				</lista-detalhe-ncm>
				<Totalizadores>
					<quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
					<peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
					<valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
					<valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
				</Totalizadores>
			</Grupo-Mercadoria>
			<Grupo-Negociacao>
				<regime-acordo-tributario>1</regime-acordo-tributario>
				<regime-acordo-tributario-nome>RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
				<fundamento-legal-regime>01</fundamento-legal-regime>
				<fundamento-legal-regime-nome>LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV</fundamento-legal-regime-nome><fundamento-legal-regime-nome/>
				<tipo-acordo-tarifario>2</tipo-acordo-tarifario>
				<tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
				<codigo-acordo-aladi>336</codigo-acordo-aladi>
				<codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
				<cobertura-cambial>1</cobertura-cambial>
				<cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
				<modalidade-pagamento>31</modalidade-pagamento>
				<modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
				<numero-dias-limite-pagamento/>
				<codigo-orgao-financeiro-internacional>01</codigo-orgao-financeiro-internacional>
				<codigo-orgao-financeiro-internacional-nome>AID - AGENCIA PARA O DESENVOLVIMENTO INTERNACIONAL - ESTADOS UNIDOS</codigo-orgao-financeiro-internacional-nome>
				<codigo-motivo-sem-cobertura>30</codigo-motivo-sem-cobertura>
				<codigo-motivo-sem-cobertura-nome>INVESTIMENTO DE CAPITAL ESTRANGEIRO</codigo-motivo-sem-cobertura-nome>
			</Grupo-Negociacao>
			<Grupo-LI-Anuencias>
				<Informacoes-LI>
					<data-registro>25/04/2022</data-registro>
					<hora-registro>16:22</hora-registro>
					<data-situacao>16/05/2022</data-situacao>
					<hora-situacao>14:25:40</hora-situacao>
					<codigo-situacao>17</codigo-situacao>
					<nome-situacao>DESEMBARACADA</nome-situacao>
					<data-restricao-embarque/>
					<data-validade-embarque>11/08/2022</data-validade-embarque>
					<data-validade-despacho>09/11/2022</data-validade-despacho>
					<numero-li-substituida/>
					<numero-li-substitutiva/>
				</Informacoes-LI>
				<Informacoes-LI-Vinculada-DI>
					<declaracao-vinculada>2209102544</declaracao-vinculada>
					<adicao-vinculada>001</adicao-vinculada>
					<retificacao/>
				</Informacoes-LI-Vinculada-DI>
				<Informacoes-Cancelamento-Vencimento-LI>
					<motivo/>
					<cpf-importador-efetuou-cancelamento/>
					<data-cancelamento-vencimento/>
					<hora-cancelamento-vencimento/>
				</Informacoes-Cancelamento-Vencimento-LI>
				<lista-anuencias>
					<anuencia>
						<orgao-anuente>ANVISA</orgao-anuente>
						<codigo-situacao-anuencia>05</codigo-situacao-anuencia>
						<nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>25/05/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>09:25</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque>25/08/2022</data-validade-embarque>
						<data-validade-despacho>25/11/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>21</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>DESTAQUE DE MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>ANVISA/GGPAF/PAFPS Deferimento do pleito de importação com base em análise documental, em conformidade com os requisitos da RDC 81/2008 e legislações correlatas. Cumprimento de exigência satisfatório. ANVISA/GGPAF/PAFPS LI em exigência sanitária: 1) Apresentar Declaração do Detentor do Registro (DDR) devidamente preenchida e assinada digitalmente com certificação digital no padrão da Infraestrutura de Chaves Públicas Brasileira - ICP-Brasil, conforme RDC nº 74/2016 e Decreto nº 10.278/2020, pelo Responsável Legal da empresa detentora da regularização do produto. Verificamos no Instituto Nacional de Tecnologia da Informação (https://verificador.iti.gov.br/verifier-2.6.1/) a assinatura do RL na DDR apresentada ao dossiê, foi reprovada. Prazo para cumprimento: 30 dias da data desta exigência.                                                                                                      </texto-anuente>
					</anuencia>
					<anuencia>
						<orgao-anuente>DECEX</orgao-anuente>
						<codigo-situacao-anuencia>11</codigo-situacao-anuencia>
						<nome-situacao-anuencia>CANCELADA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>15/06/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>12:15</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque/>
						<data-validade-despacho>15/07/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>21</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>DESTAQUE DE MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>Com base no Decreto nº 9.745/2019, Anexo I, art. 93, inciso V, e na Portaria SECEX nº 23/2011, artigos 19, 21 e 30, solicitamos que o importador apresente à SUEXT/CGOP documentação que comprove que o preço declarado na LI está compatível com os preços praticados no mercado internacional.
O processo deverá ser instruído com documentos indicados no art. 30 da Portaria SECEX nº 23/2011 com o objetivo de comprovar os aspectos comerciais da operação, tais como, lista de preços de fornecedores do mesmo produto originário de outros países (diferentes do declarado na LI, com tradução para o vernáculo); estatísticas oficiais nacionais e estrangeiras (destacando o preço praticado por outros países exportadores do mesmo produto); cotação de bolsas internacionais de mercadorias (se for o caso); publicações especializadas; contratos de bens de capital fabricados sob encomenda; e quaisquer outras informações porventura necessárias.
A documentação deverá ser entregue por meio de anexação eletrônica no módulo Visão Integrada da plataforma Portal Siscomex, de acordo o item 10.1.2 do Manual do módulo Anexação Eletrônica de Documentos.
Ao anexar o Termo de Instrução de Processo SECEX (DECEX) ao dossiê, conforme o item 10.1.2.3 do Manual, o importador deverá selecionar a palavra-chave Fiscalização de Preço.
SECEX/SUEXT/CGOP/COIMP - 30/09/2020</texto-anuente>
					</anuencia>
					<anuencia>
						<orgao-anuente>MAPA</orgao-anuente>
						<codigo-situacao-anuencia>05</codigo-situacao-anuencia>
						<nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>05/05/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>14:05</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque>05/08/2022</data-validade-embarque>
						<data-validade-despacho>05/11/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA. Despacho autorizado.                                                                            </texto-anuente>
					</anuencia>
				</lista-anuencias>
			</Grupo-LI-Anuencias>
		</li-completa>
	</lista-li-completa>
</resposta-consulta-li>";

		readonly string xmlWithout_lista_detalhe_ncm = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<resposta-consulta-li versao=""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
	<identificador-consulta>251</identificador-consulta>
	<lista-mensagens-e-erros/>
	<lista-li-completa>
		<li-completa>
			<Grupo-Dados-Basicos>
				<numero-li>22/1070364-8</numero-li>
				<Importador>
					<importador-tipo>1</importador-tipo>
					<importador-identificador>08.264.406/0001-93</importador-identificador>
					<importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
					<importador-atividade-economica/>
					<importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS. LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
					<importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
					<importador-endereco-numero>690</importador-endereco-numero>
					<importador-endereco-complemento/>
					<importador-endereco-bairro>UMBARA</importador-endereco-bairro>
					<importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
					<importador-endereco-uf>PR</importador-endereco-uf>
					<importador-endereco-cep>81930165</importador-endereco-cep>
					<importador-telefone>41 - 38882000</importador-telefone>
					<importador-pais/>
					<importador-pais-nome/>
				</Importador>
				<Outras-Informacoes>
					<pais-procedencia-mercadoria>158</pais-procedencia-mercadoria>
					<pais-procedencia-mercadoria-nome>CHILE</pais-procedencia-mercadoria-nome>
					<urf-entrada>1017500</urf-entrada>
					<urf-entrada-nome>ALF - URUGUAIANA</urf-entrada-nome>
					<urf-despacho>0917900</urf-despacho>
					<urf-despacho-nome>ALF - CURITIBA</urf-despacho-nome>
				</Outras-Informacoes>
				<Informacoes-Complementares>
					<texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100                                                                                                                                                                                             </texto-informacoes-complementares>
				</Informacoes-Complementares>
			</Grupo-Dados-Basicos>
			<Grupo-Fornecedor>
				<fornecedor-tipo>3</fornecedor-tipo>
				<pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
				<pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
				<pais-origem-mercadoria>158</pais-origem-mercadoria>
				<pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
				<Fornecedor>
					<fornecedor-estrangeiro-nome>TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
					<fornecedor-estrangeiro-email/>
					<fornecedor-estrangeiro-responsavel/>
					<fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
					<fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
					<fornecedor-estrangeiro-complemento/>
					<fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
					<fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
				</Fornecedor>
				<Fabricante>
					<fabricante-nome/>
					<fabricante-email/>
					<fabricante-responsavel/>
					<fabricante-endereco-logradoudo/>
					<fabricante-endereco-numero/>
					<fabricante-endereco-complemento/>
					<fabricante-endereco-cidade/>
					<fabricante-endereco-estado/>
				</Fabricante>
			</Grupo-Fornecedor>
			<Grupo-Mercadoria>
				<Dados-Gerais>
					<subitem-ncm>2204.21.00</subitem-ncm>
					<subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
					<unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
					<mercadoria-naladi>22042110</mercadoria-naladi>
					<mercadoria-naladi-nome/>
					<moeda>220</moeda>
					<moeda-nome>DOLAR DOS EUA</moeda-nome>
					<incoterm>FCA</incoterm>
					<incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
				</Dados-Gerais>
				<Condicao-Mercadoria>
					<condicao-mercadoria>N</condicao-mercadoria>
					<condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
					<tipo-enquadramento-material-usado/>
					<tipo-enquadramento-material-usado-nome/>
					<tipo-operacao-enquadramento-material-usado/>
					<tipo-operacao-enquadramento-material-usado-nome/>
				</Condicao-Mercadoria>
				<lista-destaque-ncm/>
				<lista-processo-anuente/>
				<Informacoes-Drawback>
					<drawback-regime>3</drawback-regime>
					<drawback-numero-ato-isencao/>
					<drawback-numero-ato-suspencao/>
				</Informacoes-Drawback>
				<lista-detalhe-ncm/>
				<Totalizadores>
					<quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
					<peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
					<valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
					<valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
				</Totalizadores>
			</Grupo-Mercadoria>
			<Grupo-Negociacao>
				<regime-acordo-tributario>1</regime-acordo-tributario>
				<regime-acordo-tributario-nome>RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
				<fundamento-legal-regime/>
				<fundamento-legal-regime-nome/>
				<tipo-acordo-tarifario>2</tipo-acordo-tarifario>
				<tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
				<codigo-acordo-aladi>336</codigo-acordo-aladi>
				<codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
				<cobertura-cambial>1</cobertura-cambial>
				<cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
				<modalidade-pagamento>31</modalidade-pagamento>
				<modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
				<numero-dias-limite-pagamento/>
				<codigo-orgao-financeiro-internacional/>
				<codigo-orgao-financeiro-internacional-nome/>
				<codigo-motivo-sem-cobertura/>
				<codigo-motivo-sem-cobertura-nome/>
			</Grupo-Negociacao>
			<Grupo-LI-Anuencias>
				<Informacoes-LI>
					<data-registro>25/04/2022</data-registro>
					<hora-registro>16:22</hora-registro>
					<data-situacao>16/05/2022</data-situacao>
					<hora-situacao>14:25:40</hora-situacao>
					<codigo-situacao>17</codigo-situacao>
					<nome-situacao>DESEMBARACADA</nome-situacao>
					<data-restricao-embarque/>
					<data-validade-embarque>11/08/2022</data-validade-embarque>
					<data-validade-despacho>09/11/2022</data-validade-despacho>
					<numero-li-substituida/>
					<numero-li-substitutiva/>
				</Informacoes-LI>
				<Informacoes-LI-Vinculada-DI>
					<declaracao-vinculada>2209102544</declaracao-vinculada>
					<adicao-vinculada>001</adicao-vinculada>
					<retificacao/>
				</Informacoes-LI-Vinculada-DI>
				<Informacoes-Cancelamento-Vencimento-LI>
					<motivo/>
					<cpf-importador-efetuou-cancelamento/>
					<data-cancelamento-vencimento/>
					<hora-cancelamento-vencimento/>
				</Informacoes-Cancelamento-Vencimento-LI>
				<lista-anuencias>
					<anuencia>
						<orgao-anuente>ANVISA</orgao-anuente>
						<codigo-situacao-anuencia>05</codigo-situacao-anuencia>
						<nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>25/05/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>09:25</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque>25/08/2022</data-validade-embarque>
						<data-validade-despacho>25/11/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>21</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>DESTAQUE DE MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>ANVISA/GGPAF/PAFPS Deferimento do pleito de importação com base em análise documental, em conformidade com os requisitos da RDC 81/2008 e legislações correlatas. Cumprimento de exigência satisfatório. ANVISA/GGPAF/PAFPS LI em exigência sanitária: 1) Apresentar Declaração do Detentor do Registro (DDR) devidamente preenchida e assinada digitalmente com certificação digital no padrão da Infraestrutura de Chaves Públicas Brasileira - ICP-Brasil, conforme RDC nº 74/2016 e Decreto nº 10.278/2020, pelo Responsável Legal da empresa detentora da regularização do produto. Verificamos no Instituto Nacional de Tecnologia da Informação (https://verificador.iti.gov.br/verifier-2.6.1/) a assinatura do RL na DDR apresentada ao dossiê, foi reprovada. Prazo para cumprimento: 30 dias da data desta exigência.                                                                                                      </texto-anuente>
					</anuencia>
					<anuencia>
						<orgao-anuente>DECEX</orgao-anuente>
						<codigo-situacao-anuencia>11</codigo-situacao-anuencia>
						<nome-situacao-anuencia>CANCELADA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>15/06/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>12:15</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque/>
						<data-validade-despacho>15/07/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>21</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>DESTAQUE DE MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>Com base no Decreto nº 9.745/2019, Anexo I, art. 93, inciso V, e na Portaria SECEX nº 23/2011, artigos 19, 21 e 30, solicitamos que o importador apresente à SUEXT/CGOP documentação que comprove que o preço declarado na LI está compatível com os preços praticados no mercado internacional.
O processo deverá ser instruído com documentos indicados no art. 30 da Portaria SECEX nº 23/2011 com o objetivo de comprovar os aspectos comerciais da operação, tais como, lista de preços de fornecedores do mesmo produto originário de outros países (diferentes do declarado na LI, com tradução para o vernáculo); estatísticas oficiais nacionais e estrangeiras (destacando o preço praticado por outros países exportadores do mesmo produto); cotação de bolsas internacionais de mercadorias (se for o caso); publicações especializadas; contratos de bens de capital fabricados sob encomenda; e quaisquer outras informações porventura necessárias.
A documentação deverá ser entregue por meio de anexação eletrônica no módulo Visão Integrada da plataforma Portal Siscomex, de acordo o item 10.1.2 do Manual do módulo Anexação Eletrônica de Documentos.
Ao anexar o Termo de Instrução de Processo SECEX (DECEX) ao dossiê, conforme o item 10.1.2.3 do Manual, o importador deverá selecionar a palavra-chave Fiscalização de Preço.
SECEX/SUEXT/CGOP/COIMP - 30/09/2020</texto-anuente>
					</anuencia>
					<anuencia>
						<orgao-anuente>MAPA</orgao-anuente>
						<codigo-situacao-anuencia>05</codigo-situacao-anuencia>
						<nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
						<data-diagnostico-anuencia>05/05/2022</data-diagnostico-anuencia>
						<hora-diagnostico-anuencia>14:05</hora-diagnostico-anuencia>
						<data-restricao-embarque/>
						<data-validade-embarque>05/08/2022</data-validade-embarque>
						<data-validade-despacho>05/11/2022</data-validade-despacho>
						<codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
						<nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
						<texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA. Despacho autorizado.                                                                            </texto-anuente>
					</anuencia>
				</lista-anuencias>
			</Grupo-LI-Anuencias>
		</li-completa>
	</lista-li-completa>
</resposta-consulta-li>";

		readonly string wrongXML = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<resposta-consulta-li versao=""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
	<identificador-consulta>251</identificador-consulta>
	<lista-mensagens-e-erros/>
</resposta-consulta-li>";

		public static string GetXmlByManufacturerIndicator(string manufacturerIndicator, string nameSupplier, string nameManufacturer) => $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<resposta-consulta-li versao=""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
	<identificador-consulta>251</identificador-consulta>
	<lista-mensagens-e-erros/>
	<lista-li-completa>
		<li-completa>
			<Grupo-Dados-Basicos>
				<numero-li>22/1070364-8</numero-li>
				<Importador>
					<importador-tipo>1</importador-tipo>
					<importador-identificador>08.264.406/0001-93</importador-identificador>
					<importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
					<importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS. LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
					<importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
					<importador-endereco-numero>690</importador-endereco-numero>
					<importador-endereco-bairro>UMBARA</importador-endereco-bairro>
					<importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
					<importador-endereco-uf>PR</importador-endereco-uf>
					<importador-endereco-cep>81930165</importador-endereco-cep>
					<importador-telefone>41 - 38882000</importador-telefone>
					<importador-pais/>
					<importador-pais-nome/>
				</Importador>
			</Grupo-Dados-Basicos>
			<Grupo-Fornecedor>
				<fornecedor-tipo>{manufacturerIndicator}</fornecedor-tipo>
				<pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
				<pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
				<pais-origem-mercadoria>158</pais-origem-mercadoria>
				<pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
				<Fornecedor>
					<fornecedor-estrangeiro-nome>{nameSupplier}</fornecedor-estrangeiro-nome>
					<fornecedor-estrangeiro-email/>
					<fornecedor-estrangeiro-responsavel/>
					<fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
					<fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
					<fornecedor-estrangeiro-complemento/>
					<fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
					<fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
				</Fornecedor>
				<Fabricante>
					<fabricante-nome>{nameManufacturer}</fabricante-nome>
					<fabricante-email/>
					<fabricante-responsavel/>
					<fabricante-endereco-logradoudo/>
					<fabricante-endereco-numero/>
					<fabricante-endereco-complemento/>
					<fabricante-endereco-cidade/>
					<fabricante-endereco-estado/>
				</Fabricante>
			</Grupo-Fornecedor>
			<Grupo-Mercadoria>
				<Dados-Gerais>
					<subitem-ncm>2204.21.00</subitem-ncm>
					<subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
					<unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
					<mercadoria-naladi>22042110</mercadoria-naladi>
					<mercadoria-naladi-nome/>
					<moeda>220</moeda>
					<moeda-nome>DOLAR DOS EUA</moeda-nome>
					<incoterm>FCA</incoterm>
					<incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
				</Dados-Gerais>
				<Totalizadores>
					<quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
					<peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
					<valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
					<valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
				</Totalizadores>
			</Grupo-Mercadoria>
		</li-completa>
	</lista-li-completa>
</resposta-consulta-li>";
	}
}
