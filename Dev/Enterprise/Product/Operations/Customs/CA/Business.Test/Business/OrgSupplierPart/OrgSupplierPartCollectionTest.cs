using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	sealed class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		public void TestAddingNewPartExportLookup()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			cusClass.CC_TariffNum = "01213231";
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_StateOrRegionOfOrigin = "ON";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], cusClass.PK, ClassificationTypeList.Codes.SHB, "01213231");
			AssertEquals("ON", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_RN_NKOrigin);

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "0121323122";
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], cusClass.PK, ClassificationTypeList.Codes.HTE, "0121323122");
			AssertEquals("ON", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_RN_NKOrigin);
		}

		public void TestAddingNewPartExportTariff()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01213231";
			invoiceLine.JI_CountryOfOrigin = "CA";
			invoiceLine.JI_StateOrRegionOfOrigin = "ON";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.SHB, "01213231");
			AssertEquals("ON", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_RN_NKOrigin);

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			invoiceLine.JI_Tariff = "0121323122";
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, true);
			part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.HTE, "0121323122");
			AssertEquals("ON", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_RN_NKOrigin);
		}

		public void TestAddingNewPartImportLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_CC = cusClass.PK;
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_StateOrRegionOfOrigin = "CA";
			invoiceLine.CA_ValueForDutyCode = "13";
			invoiceLine.CA_TreatmentCode = "02";
			invoiceLine.CA_99TariffCode = "9901";
			invoiceLine.CA_AuthorityNumber = "AUTHNO";
			invoiceLine.CA_TRSNumber = "TRSNO";
			var gst = invoiceLine.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_ExemptCode = "48";
			declaration.ResumeApportionment();
			var sima = invoiceLine.DutiesAndTaxes.AddNew();
			sima.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			sima.C1_ExemptCode = "10";
			var ext = invoiceLine.DutiesAndTaxes.AddNew();
			ext.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			ext.C1_ExemptCode = "85";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], cusClass.PK, ClassificationTypeList.Codes.HTI, "0121323122");
			AssertEquals("9901", part.PivotsForBinding[0].CCA_99TariffCode);
			AssertEquals("AUTHNO", part.PivotsForBinding[0].CCA_AuthorityNumber);
			AssertEquals("85", part.PivotsForBinding[0].CCA_ETExemption);
			AssertEquals("48", part.PivotsForBinding[0].CCA_GSTStatusCode);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("US", part.PivotsForBinding[0].CCA_RN_NKOrigin);
			AssertEquals("02", part.PivotsForBinding[0].CCA_TreatmentCode);
			AssertEquals("TRSNO", part.PivotsForBinding[0].CCA_TRSNumber);
			AssertEquals("13", part.PivotsForBinding[0].CCA_ValueForDutyCode);
		}

		public void TestAddingNewPartImportTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_StateOrRegionOfOrigin = "CA";
			invoiceLine.CA_ValueForDutyCode = "13";
			invoiceLine.CA_TreatmentCode = "02";
			invoiceLine.CA_99TariffCode = "9901";
			invoiceLine.CA_AuthorityNumber = "AUTHNO";
			invoiceLine.CA_TRSNumber = "TRSNO";

			declaration.ResumeApportionment();
			var gst = invoiceLine.DutiesAndTaxes.AddNew();
			gst.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gst.C1_ExemptCode = "48";

			var sima = invoiceLine.DutiesAndTaxes.AddNew();
			sima.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			sima.C1_ExemptCode = "10";
			var ext = invoiceLine.DutiesAndTaxes.AddNew();
			ext.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			ext.C1_ExemptCode = "85";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.HTI, "0121323122");
			AssertEquals("9901", part.PivotsForBinding[0].CCA_99TariffCode);
			AssertEquals("AUTHNO", part.PivotsForBinding[0].CCA_AuthorityNumber);
			AssertEquals("85", part.PivotsForBinding[0].CCA_ETExemption);
			AssertEquals("48", part.PivotsForBinding[0].CCA_GSTStatusCode);
			AssertEquals("CA", part.PivotsForBinding[0].CCA_ProvinceOfOrigin);
			AssertEquals("US", part.PivotsForBinding[0].CCA_RN_NKOrigin);
			AssertEquals("02", part.PivotsForBinding[0].CCA_TreatmentCode);
			AssertEquals("TRSNO", part.PivotsForBinding[0].CCA_TRSNumber);
			AssertEquals("13", part.PivotsForBinding[0].CCA_ValueForDutyCode);
		}

		public void TestAddingNewPartImportCFIA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";

			invoiceLine.CA_RequirementID = "1";
			invoiceLine.CA_RequirementVer = "2";
			invoiceLine.CA_AirsCode = "3";
			invoiceLine.CA_DestinationProvince = "AB";
			invoiceLine.CA_EndUse = "4";
			invoiceLine.CA_MiscID = "5";
			invoiceLine.CA_RN_NKCFIAOrigin = "US";
			invoiceLine.CA_CFIAUSStateOfOrigin = "NY";
			invoiceLine.CFIARegistrationNumbers.AddNew("CD1", "CFIA1");
			invoiceLine.CFIARegistrationNumbers.AddNew("CD2", "CFIA2");

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.HTI, "0121323122");
			AssertEquals("3", part.PivotsForBinding[0].CCA_AirsCode);
			AssertEquals("NY", part.PivotsForBinding[0].CCA_CFIAUSStateOfOrigin);
			AssertEquals("AB", part.PivotsForBinding[0].CCA_DestinationProvince);
			AssertEquals("4", part.PivotsForBinding[0].CCA_EndUse);
			AssertEquals("5", part.PivotsForBinding[0].CCA_MiscID);
			AssertEquals("1", part.PivotsForBinding[0].CCA_RequirementID);
			AssertEquals("2", part.PivotsForBinding[0].CCA_RequirementVersion);
			AssertEquals("US", part.PivotsForBinding[0].CCA_RN_NKCFIAOrigin);
			AssertEquals("CFIA Reg 1 code", "CD1", part.PivotsForBinding[0].CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("CFIA Reg 1 number", "CFIA1", part.PivotsForBinding[0].CFIARegistrationNumbers[0].CY_Data);
			AssertEquals("CFIA Reg 2 code", "CD2", part.PivotsForBinding[0].CFIARegistrationNumbers[1].CY_Code);
			AssertEquals("CFIA Reg 2 number", "CFIA2", part.PivotsForBinding[0].CFIARegistrationNumbers[1].CY_Data);
		}

		public void TestAddingNewPartImportTC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			var tcPGAHeader = invoiceLine.TCPGAHeader;
			tcPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			invoiceLine.JI_BrandName = "Brand";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();

			AssertEquals("Brand", part.PivotsForBinding[0].CCA_BrandName);
		}

		public void TestAddingNewPartImportSITT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";

			invoiceLine.CA_ImportReasonCode = "1";
			invoiceLine.CA_Model = "2";
			invoiceLine.CA_ModelNumber = "3";
			invoiceLine.JI_BrandName = "4";
			invoiceLine.SITTCertificationNumbers.AddNew("SITT1");
			invoiceLine.SITTCertificationNumbers.AddNew("SITT2");

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.HTI, "0121323122");
			AssertEquals("4", part.PivotsForBinding[0].CCA_BrandName);
			AssertEquals("1", part.PivotsForBinding[0].CCA_ImportReasonCode);
			AssertEquals("2", part.PivotsForBinding[0].CCA_Model);
			AssertEquals("3", part.PivotsForBinding[0].CCA_ModelNumber);
			AssertEquals("SITT Reg 1 number", "SITT1", part.PivotsForBinding[0].SITTCertificationNumbers[0].CY_Data);
			AssertEquals("SITT Reg 2 number", "SITT2", part.PivotsForBinding[0].SITTCertificationNumbers[1].CY_Data);
		}

		public void TestAddingNewPartImportOtherOGD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";
			invoiceLine.CA_TypeSize = "1";
			invoiceLine.CA_TIIN = "2";
			invoiceLine.CA_CompliantCompletion = true;
			invoiceLine.CA_CompliantImportDate = true;

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var part = collection.AddNew();
			AssertPivot(part.PivotsForBinding[0], ZGuid.Empty, ClassificationTypeList.Codes.HTI, "0121323122");
			Assert(part.PivotsForBinding[0].CCA_CompliantCompletion);
			Assert(part.PivotsForBinding[0].CCA_CompliantImportDateIndicator);
			AssertEquals("2", part.PivotsForBinding[0].CCA_TIIN);
			AssertEquals("1", part.PivotsForBinding[0].CCA_TypeSize);
		}

		void AssertPivot(CusClassPartPivot pivot, ZGuid lookupPK, ZString childType, ZString tariff)
		{
			AssertEquals("Child Type", childType, pivot.CI_ChildType);
			AssertEquals("Lookup", lookupPK, pivot.CI_CC);
			AssertEquals("Tariff Number", tariff, pivot.TariffNumber);
		}

		public void TestOP_StockKeepingUnit()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";
			invoiceLine.JI_InvoiceUQ = CustomsUnitOfMeasureList.Codes.Kilogram;
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			OrgSupplierPart orgSupplierPart = collection.AddNew();
			AssertEquals("Convert to standard unit ", Core.Constants.Weight.Kilograms, orgSupplierPart.OP_StockKeepingUnit);

			invoiceLine.JI_InvoiceUQ = CustomsUnitOfMeasureList.Codes.Joule;
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			orgSupplierPart = collection.AddNew();
			AssertEquals("Can't convert to standard unit, just copy", CustomsUnitOfMeasureList.Codes.Joule, orgSupplierPart.OP_StockKeepingUnit);
		}

		public void TestCopySIMADutiesFromInvoiceLineToProduct()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			foreach (var dutyAndTax in invoiceLine.DutiesAndTaxes)
			{
				dutyAndTax.C1_ExemptCode = SIMACodes.Codes.C31;
			}

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var orgSupplierPart = collection.AddNew();
			AssertEquals(1, orgSupplierPart.PivotsForBinding.Count);
			var pivot = orgSupplierPart.PivotsForBinding[0];
			AssertEquals("AD1407", pivot.CCA_SIMADumpingNumber);
			AssertEquals(3, pivot.DutiesAndTaxes.Count);
			var surTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInPivot, SIMACodes.Codes.C31, false, 10m, "", 0m, "", 0m, "");
			var addTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInPivot, SIMACodes.Codes.C31, false, 100.5m, "NMB", 0m, "", 0m, "");
			var cvdTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInPivot, SIMACodes.Codes.C31, false, 200m, "KGM", 0m, "", 0m, "");

			var surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			surTaxInInvoiceLine.C1_Override = true;
			surTaxInInvoiceLine.C1_Rate = 20m;
			var addTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			addTaxInInvoiceLine.C1_Override = true;
			addTaxInInvoiceLine.C1_NormalValuePerUnit = 10m;
			addTaxInInvoiceLine.C1_NormalValueCurrency = "CNY";
			var cvdTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			cvdTaxInInvoiceLine.C1_Override = true;
			cvdTaxInInvoiceLine.C1_ForeignRate = 15m;
			cvdTaxInInvoiceLine.C1_ForeignCurrency = "CNY";
			cvdTaxInInvoiceLine.C1_Rate = 200m;

			collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			orgSupplierPart = collection.AddNew();
			AssertEquals(1, orgSupplierPart.PivotsForBinding.Count);
			pivot = orgSupplierPart.PivotsForBinding[0];
			AssertEquals("AD1407", pivot.CCA_SIMADumpingNumber);
			AssertEquals(3, pivot.DutiesAndTaxes.Count);
			surTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInPivot, SIMACodes.Codes.C31, true, 20m, "", 0m, "", 0m, "");
			addTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInPivot, SIMACodes.Codes.C31, true, 100.5m, "NMB", 10m, "CNY", 0m, "");
			cvdTaxInPivot = pivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInPivot, SIMACodes.Codes.C31, true, 200m, "KGM", 0m, "", 15m, "CNY");
		}

		public void TestAddingNewPartImport_AMMV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			CombineAssertions(() =>
			{
				invoiceLine.CA_AMMVPercentage = 10m;
				var part1 = collection.AddNew();
				AssertEquals("AMMVPercentage", 10m, part1.PivotsForBinding[0].CCA_AMMVPercentage);

				invoiceLine.CA_AMMVPerUnit = 20m;
				var part2 = collection.AddNew();
				AssertEquals("AMMVPerUnit", 20m, part2.PivotsForBinding[0].CCA_AMMVPerUnit);
			});
		}

		public void TestAddingNewPartImport_BrandName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0121323122";

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			CombineAssertions(() =>
			{
				invoiceLine.JI_BrandName = "Brand";
				var part1 = collection.AddNew();
				AssertEquals("Brandname", "Brand", part1.PivotsForBinding[0].CCA_BrandName);

				invoiceLine.JI_BrandName = "The maximum length of CCA_BrandName is 25.";
				var part2 = collection.AddNew();
				AssertEquals("Brandname", "The maximum length of CCA", part2.PivotsForBinding[0].CCA_BrandName);
			});
		}

		void AssertDutyProperties(DutyAndTax dutyAndTax, ZString exemptCode, ZBool isOverride, ZDecimal rate, ZString uom, ZDecimal normalValue, ZString normalCurrency, ZDecimal foreignRate, ZString foreignCurrency)
		{
			AssertEquals(exemptCode, dutyAndTax.C1_ExemptCode);
			AssertEquals(isOverride, dutyAndTax.C1_Override);
			AssertEquals(rate, dutyAndTax.C1_Rate);
			AssertEquals(uom, dutyAndTax.C1_UnitOfMeasure);
			AssertEquals(normalValue, dutyAndTax.C1_NormalValuePerUnit);
			AssertEquals(normalCurrency, dutyAndTax.C1_NormalValueCurrency);
			AssertEquals(foreignRate, dutyAndTax.C1_ForeignRate);
			AssertEquals(foreignCurrency, dutyAndTax.C1_ForeignCurrency);
		}
	}
}
