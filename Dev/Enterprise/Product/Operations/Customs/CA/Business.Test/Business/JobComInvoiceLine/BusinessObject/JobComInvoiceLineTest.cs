using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineTest : BaseJobComInvoiceLineAbstractTest
	{
		public void TestUpdateCustomsUnitQtyFromGlobalTariff()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "2709000030", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff, "CU1", "MTQ");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.JI_CustomsUnitQty);
			invoiceLine.JI_Tariff = "2709000030";
			AssertEquals("MTQ", invoiceLine.JI_CustomsUnitQty);
		}

		public void TestUseUniversalTariff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(InvoiceLine.UseUniversalTariff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(InvoiceLine.UseUniversalTariff);
		}

		public void TestTariffType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<TariffWrapper>(InvoiceLine.Tariff);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<TariffWrapper>(InvoiceLine.Tariff);
		}

		public void TestCanDeleteForAcceptedInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			var entryLine = cadEntry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Factory.Save();

			Assert(!(invoiceLine as ICanDelete).CanDelete);
			AssertEquals("Invoice line may not be deleted because this CAD has already been reported, or is waiting for a response.", (invoiceLine as ICanDelete).ReasonForNotAbleToDelete);

			cadEntry.CH_EntryStatus = CADEntryStatusList.Codes.Rejected;
			Assert((invoiceLine as ICanDelete).CanDelete);

			cadEntry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Assert(!(invoiceLine as ICanDelete).CanDelete);
		}

		public void TestShouldHasAOverridedEXSForLuxuryTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;

			line.CA_ApplyLuxuryTax = true;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var luxuryTaxInvoiceLine = line.LuxuryTaxInvoiceLine;
			AssertEquals(1, luxuryTaxInvoiceLine.DutiesAndTaxes.Count);
			var exs = luxuryTaxInvoiceLine.DutiesAndTaxes[0];
			AssertEquals(DutyAndTaxTypes.Codes.ExciseTax, exs.C1_TaxType);
			Assert(exs.C1_Override);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(invoiceLine,
				"CAJobComInvoiceLine",
				schemaTypeName: nameof(AutoJobComInvoiceLine.Schema));
		}

		public void TestCA_SIMADumpingNumShouldNotBeEmptyWhenCCA_SIMADumpingNumberOfProductIsEmpty()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
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

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			supplier.OH_FullName = "TESTSUPPLIER";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			part.RelatedOrganisations.AddSupplier(supplier);
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "1234567890";
			AssertEquals(ZString.Empty, importPivot.CCA_SIMADumpingNumber);	// empty because we did not assign country of origin
			AssertEquals(0, importPivot.DutiesAndTaxes.Count);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.CA_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			Factory.Save();

			importPivot.CCA_SIMADumpingNumber = ZString.Empty;
			invoiceLine.PartSyncManager.Refresh();
			AssertEquals("Should not synchronized with product", "AD1407", invoiceLine.CA_SIMADumpingNum);
		}

		public void TestCA_SIMADumpingNumShouldNotBeEmptyWhenCCA_SIMADumpingNumberOfClassificationIsEmptyAndSIMAMeasuresHaveValue()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
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

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.CC_TariffNum = "1234567890";
			AssertEquals("AD1407", classification.CCA_SIMADumpingNumber);
			classification.CCA_SIMADumpingNumber = ZString.Empty;
			AssertEquals(ZString.Empty, classification.CCA_SIMADumpingNumber);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_SIMADumpingNum = "AD1406";
			AssertEquals("AD1406", invoiceLine.CA_SIMADumpingNum);
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("Should not synchronized with classification", "AD1406", invoiceLine.CA_SIMADumpingNum);
		}

		public void TestJI_CustomsSecondUnitQtyConversion()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;
			AssertEquals(1m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_CustomsQuantity = 2000m;
			AssertEquals(2m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestJI_CustomsThirdUnitQtyConversion()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsThirdUnitQty = CustomsUnitOfMeasureList.Codes.MetricTon;
			AssertEquals(1m, invoiceLine.JI_CustomsThirdQuantity);

			invoiceLine.JI_CustomsQuantity = 2000m;
			AssertEquals(2m, invoiceLine.JI_CustomsThirdQuantity);
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be CA", Core.Constants.CountryCodes.Canada, InvoiceLine.CustomsCountryCode);
		}

		public void TestCADSequences()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("[0,0]", invoiceLine.CommoditySequence);
			AssertEquals((ZShort)0, invoiceLine.GoodsShipmentSequence);

			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			AssertEquals("[4,3]", invoiceLine.CommoditySequence);
			AssertEquals((ZShort)4, invoiceLine.GoodsShipmentSequence);
		}

		public void TestCanDeleteForCADResponsedLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			Assert("CanDelete when no CAD responsed yet", ((ICanDelete)invoiceLine).CanDelete);
			entryLine.CL_CommoditySequence = 3;
			Assert("CanDelete when no CAD responsed yet", !((ICanDelete)invoiceLine).CanDelete);
		}

		public void TestDutiesAndTaxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(line.DutiesAndTaxes);
			AssertEquals(0, line.DutiesAndTaxes.Count);
		}

		public void TestLuxuryTaxInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.ShouldDeleteLuxuryTaxInvoiceLine += () => true;

			line.CA_ApplyLuxuryTax = true;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			var luxuryTaxInvoiceLine = line.LuxuryTaxInvoiceLine;
			AssertNotNull(luxuryTaxInvoiceLine);
			Assert(!(luxuryTaxInvoiceLine as ICanDelete).CanDelete);
			AssertEquals("The luxury tax invoice line can not be deleted because it has an associated line. To delete this line, uncheck the Luxury Tax Applies checkbox of the parent line.", (luxuryTaxInvoiceLine as ICanDelete).ReasonForNotAbleToDelete);
			AssertEquals("Unformatted", JobComInvoiceLine.LuxuryTaxTariffCode, luxuryTaxInvoiceLine.JI_Tariff);
			AssertEquals("Formatted", "0000.99.99 69", luxuryTaxInvoiceLine.JI_FormattedTariff);
			Assert(luxuryTaxInvoiceLine.CA_ApplyLuxuryTaxInfo.ReadOnly);
			AssertEquals("LUXURY TAX", luxuryTaxInvoiceLine.JI_Description);
			Assert(luxuryTaxInvoiceLine.ReadOnly);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newLine = newFactory.Load<JobComInvoiceLine>(luxuryTaxInvoiceLine.PK);
			Assert(newLine.ReadOnly);

			line.CA_ApplyLuxuryTax = false;
			AssertEquals(1, invoice.JobComInvoiceLines.Count);
			AssertNull(line.LuxuryTaxInvoiceLine);

			line.CA_ApplyLuxuryTax = true;
			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			invoice.JobComInvoiceLines.RemoveAndDelete(line);
			AssertEquals("When the original line is deleted, the luxury tax invoice line will also be removed.", 0, invoice.JobComInvoiceLines.Count);
			AssertNull(line.LuxuryTaxInvoiceLine);
		}

		public void TestGetCusAddInfoTypes()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = dec.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var supporter = invoiceLine1 as ICusAddInfoTypeSupporter;

			Type type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CADutyAndTax, out type);
			AssertEquals(typeof(DutyAndTax), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, out type);
			AssertEquals(typeof(HCPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader, out type);
			AssertEquals(typeof(PHACPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader, out type);
			AssertEquals(typeof(NRCanPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader, out type);
			AssertEquals(typeof(DFOPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader, out type);
			AssertEquals(typeof(GACPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader, out type);
			AssertEquals(typeof(CFIAPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader, out type);
			AssertEquals(typeof(CNSCPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader, out type);
			AssertEquals(typeof(ECCCPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.CATCPGAHeader, out type);
			AssertEquals(typeof(TCPGAHeader), type);
			type = null;
			supporter.GetCusAddInfoTypes().TryGetValue("ZZ!", out type);
			AssertNull(type);
		}

		public void TestRecalculatePageNumberWhenJI_JZChanged()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 1;
			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 2;
			JobComInvoiceHeader invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 3;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 3;
			JobComInvoiceHeader invoice3 = jobDeclaration.Invoices.AddNew();
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var line5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line5.CA_PageNumber = 4;
			var line6 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line6.CA_PageNumber = 5;

			line1.JI_JZ = invoice2.PK;
			AssertEquals(2, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(3, line3.CA_PageNumber);
			AssertEquals(3, line4.CA_PageNumber);
			AssertEquals(4, line5.CA_PageNumber);
			AssertEquals(5, line6.CA_PageNumber);

			line3.JI_JZ = invoice.PK;
			AssertEquals(3, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line3.CA_PageNumber);
			AssertEquals(4, line4.CA_PageNumber);
			AssertEquals(5, line5.CA_PageNumber);
			AssertEquals(6, line6.CA_PageNumber);

			line4.JI_JZ = invoice.PK;
			AssertEquals(4, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line3.CA_PageNumber);
			AssertEquals(3, line4.CA_PageNumber);
			AssertEquals(5, line5.CA_PageNumber);
			AssertEquals(6, line6.CA_PageNumber);

			line5.JI_JZ = invoice2.PK;
			AssertEquals(4, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(2, line3.CA_PageNumber);
			AssertEquals(3, line4.CA_PageNumber);
			AssertEquals(5, line5.CA_PageNumber);
			AssertEquals(6, line6.CA_PageNumber);

			line3.JI_JZ = invoice3.PK;
			AssertEquals(3, line1.CA_PageNumber);
			AssertEquals(1, line2.CA_PageNumber);
			AssertEquals(5, line3.CA_PageNumber);
			AssertEquals(2, line4.CA_PageNumber);
			AssertEquals(4, line5.CA_PageNumber);
			AssertEquals(6, line6.CA_PageNumber);
		}

		public void TestDefaultJI_OA_ConsigneeAddressFromInvoice()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org1.MainAddress;
			var address2 = org1.Addresses.AddNew(OrgAddressType.Delivery, false);
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address3 = org2.MainAddress;
			var address4 = org2.Addresses.AddNew(OrgAddressType.Delivery, false);
			var address5 = org2.Addresses.AddNew(OrgAddressType.PickupAndDelivery, false);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = dec.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			Assert(invoiceLine1.JI_OA_ConsigneeAddress.IsEmpty);

			invoice1.FinalConsigneeAddress.OrganisationPK = org1.PK;
			AssertEquals(address1.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			invoice1.FinalConsigneeAddress.E2_OA_Address = address2.PK;
			AssertEquals(address2.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			invoice1.FinalConsigneeAddress.OrganisationPK = org2.PK;
			AssertEquals(address3.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			invoiceLine1.JI_OA_ConsigneeAddress = address4.PK;
			invoice1.FinalConsigneeAddress.E2_OA_Address = address5.PK;
			AssertEquals(address4.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			AssertNotEquals(address5.PK, invoiceLine1.JI_OA_ConsigneeAddress);

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			AssertEquals(address5.PK, invoiceLine2.JI_OA_ConsigneeAddress);
			invoice1.FinalConsigneeAddress.OrganisationPK = ZGuid.Empty;
			Assert(invoiceLine2.JI_OA_ConsigneeAddress.IsEmpty);

			var invoice2 = dec.Invoices.AddNew();
			invoice2.FinalConsigneeAddress.OrganisationPK = org1.PK;
			invoiceLine2.JI_JZ = invoice2.PK;
			AssertEquals(address1.PK, invoiceLine2.JI_OA_ConsigneeAddress);

			invoice1.FinalConsigneeAddress.OrganisationPK = org1.PK;
			var invoiceLine3 = invoice1.InvoiceLines.AddNew();
			invoice2.FinalConsigneeAddress.OrganisationPK = org2.PK;
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = ZGuid.Empty;
			invoiceLine3.JI_OA_ConsigneeAddress = address2.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = ZGuid.Empty;
			invoiceLine4.JI_OA_ConsigneeAddress = address4.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(invoiceLine1.JI_OA_ConsigneeAddress.IsEmpty);
			AssertEquals(address2.PK, invoiceLine3.JI_OA_ConsigneeAddress);
			Assert(invoiceLine2.JI_OA_ConsigneeAddress.IsEmpty);
			AssertEquals(address4.PK, invoiceLine4.JI_OA_ConsigneeAddress);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(address1.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			AssertEquals(address2.PK, invoiceLine3.JI_OA_ConsigneeAddress);
			AssertEquals(address3.PK, invoiceLine2.JI_OA_ConsigneeAddress);
			AssertEquals(address4.PK, invoiceLine4.JI_OA_ConsigneeAddress);

			invoice1.FinalConsigneeAddress.E2_AddressOverride = true;
			AssertEquals(address1.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			AssertEquals(address2.PK, invoiceLine3.JI_OA_ConsigneeAddress);
			invoice1.FinalConsigneeAddress.E2_AddressOverride = false;
			AssertEquals(address1.PK, invoiceLine1.JI_OA_ConsigneeAddress);
			AssertEquals(address2.PK, invoiceLine3.JI_OA_ConsigneeAddress);

			invoice1.FinalConsigneeAddress.E2_OA_Address = ZGuid.Invalid;
			AssertEquals(ZGuid.Invalid, invoiceLine1.JI_OA_ConsigneeAddress);
			AssertEquals(address2.PK, invoiceLine3.JI_OA_ConsigneeAddress);
		}

		public void TestCountryAndStateOnIID()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = "IID";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var tariffNo = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "1701121000");
			invoiceLine.JI_Tariff = tariffNo;
			var nonCFIAInvoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = "IL";

			AssertEquals(Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKSource);
			AssertEquals("IL", invoiceLine.CA_StateOfSource);
			AssertEquals(ZString.Empty, nonCFIAInvoiceLine.CA_RN_NKSource);
			AssertEquals(ZString.Empty, nonCFIAInvoiceLine.CA_StateOfSource);

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = "DE";

			nonCFIAInvoiceLine.JI_Tariff = tariffNo;
			nonCFIAInvoiceLine.CA_CFIAInd = "Y";
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, nonCFIAInvoiceLine.CA_RN_NKSource);
			AssertEquals("DE", nonCFIAInvoiceLine.CA_StateOfSource);
		}

		public void TestPGARequirementsDefaultWhenProductHasNoIndicator()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824700308", "TPR");
			var part = GetPart();
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			importPivot.CI_TariffNum = "3824700308";
			AssertEquals(YesNoList.Codes.Yes, importPivot.CCA_TCIndicator);
			importPivot.CCA_TCIndicator = YesNoList.Codes.No;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.CA_TCInd);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals(YesNoList.Codes.No, invoiceLine.CA_TCInd);

			importPivot.CCA_TCIndicator = ZString.Empty;
			Factory.Save();

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine2.CA_TCInd);
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			AssertEquals(YesNoList.Codes.Yes, invoiceLine2.CA_TCInd);
		}

		public void TestIsSupportEmptyPackType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pack = declaration.Packages.AddNew();
			pack.CW_PackType = Core.Constants.PkgUnit.Container;
			pack.CW_PackQty = 15;

			var npbos = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault();
			npbo.IsLinked = true;
			npbo.Validation.ValidatePackQty();
			AssertEquals(15, npbo.PackQty);
			AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var npbos2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
			var npbo2 = npbos2.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault();
			npbo.IsLinked = true;
			npbo2.PackQty = 0;
			npbo2.Validation.ValidatePackQty();
			AssertNoMessageErrorContaining(npbo2.PackQtyInfo, "have not entered");
			npbo.PackQty = 14;
			npbo2.Validation.ValidatePackQty();
			AssertHasMessageErrorContaining(npbo2.PackQtyInfo, "have not entered");
		}

		public void TestInvoiceLinePackageValidationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 100000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var collection = new BaseCusLinkPackageCollection(invoiceLine);
			var npbo = collection.AddNew();
			AssertType<InvoiceLinePackageValidation>(supporter.GetNewLinkPackValidation(npbo));
		}

		public void TestLinePriceForBalanceCalc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 100000m;
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoiceLine.CA_AuthorityNumber = "XXXXX";
			invoiceLine.CA_PageNumber = 3;
			invoiceLine.JI_LinePrice = 100000m;
			declaration.ResumeApportionment();
			AssertEquals(0m, invoiceHeader.JZ_Calc_Balance);

			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SoftwareRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			var remissionLine = invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => !x.JI_ParentID.IsEmpty);
			AssertNotNull(remissionLine);
			remissionLine.JI_LinePrice = 150m;
			declaration.ResumeApportionment();
			AssertEquals(0m, invoiceHeader.JZ_Calc_Balance);
		}

		public void TestCA_CalculationMethod_SoftwareRemission()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoiceLine.CA_AuthorityNumber = "XXXXX";
			invoiceLine.CA_PageNumber = 3;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 2, 2, 600, 100, 90, 80, Core.Constants.Weight.Kilograms, 100m);

			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SoftwareRemission;
			AssertEquals("InvoiceLines Count", 1, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("Price cleared", 600m, invoiceLine.JI_LinePrice);
			AssertEquals("ContinueWithActionCaption", "Software Remission Line Creation", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to add a Software Remission line.\r\n" +
				"You will be able to override Price and Description for Software Remission calculations.\r\n" +
	"Do you want to continue?", messageInitiator.ContinueWithActionMessage);

			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SoftwareRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Price cleared", 600m, invoiceLine.JI_LinePrice);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.SoftwareRemission, invoiceLine.CA_CalculationMethod);
			Assert("RepairsRemission IsRemissionLine", invoiceLine.IsRemissionLine);

			var swLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines[1];
			AssertNotEquals("PK", invoiceLine.PK, swLine.PK);
			AssertEquals("JI_lineNo, repair child inserted immediatly after parent", (ZShort)2, swLine.JI_LineNo);
			AssertEquals("JI_Tariff", invoiceLine.JI_Tariff, swLine.JI_Tariff);
			AssertEquals("JI_Description", "MEDIA", swLine.JI_Description);
			AssertEquals("CA_ValueForDutyCode", "29", swLine.CA_ValueForDutyCode);
			AssertEquals("JI_ParentID", invoiceLine.PK, swLine.JI_ParentID);
			AssertEquals("Price copied", 0m, swLine.JI_LinePrice);
			AssertEquals("CA_CalculationMethod", invoiceLine.CA_CalculationMethod, swLine.CA_CalculationMethod);
			AssertEquals("JI_CountryOfOrigin", invoiceLine.JI_CountryOfOrigin, swLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", invoiceLine.JI_StateOrRegionOfOrigin, swLine.JI_StateOrRegionOfOrigin);
			Assert("DutyDefferalLine IsRemissionLine", swLine.IsRemissionLine);
			Assert(!swLine.JI_FormattedTariffInfo.ReadOnly);
			Assert(!swLine.JI_CountryOfOriginInfo.ReadOnly);
			swLine.JI_CountryOfOrigin = "CA";
			Assert(!swLine.JI_StateOrRegionOfOriginInfo.ReadOnly);
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			AssertEquals("InvoiceLines Count", 1, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Price restored", 600m, invoiceLine.JI_LinePrice);
			AssertEquals("ContinueWithActionCaption", "Software Remission Line Deletion", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to delete the Software Remission line.\r\n" +
	"Do you want to continue?", messageInitiator.ContinueWithActionMessage);
		}

		public void TestNotReCalculateCustomsQuantityForRepairLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoiceLine.CA_AuthorityNumber = "XXXXX";
			invoiceLine.CA_PageNumber = 3;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			invoiceLine.JI_CustomsQuantity = 100m;
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 2, 2, 600, 100, 90, 80, Core.Constants.Weight.Kilograms, 100m);

			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.SoftwareRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			var repairLine = invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => !x.JI_ParentID.IsEmpty);
			AssertEquals(100m, repairLine.JI_CustomsQuantity);
			invoiceLine.JI_LinePrice = 0m;
			AssertEquals(0m, repairLine.JI_LinePrice);
			AssertEquals(100m, repairLine.JI_CustomsQuantity);
		}

		public void TestRuling()
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var rulingCombined = Factory.New<ZZRefCusRulingCombined>();
			rulingCombined.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			rulingCombined.ZZX_RulingNumber = "RULING001";
			rulingCombined.ZZX_Description = "RULING001";
			rulingCombined.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			Factory.Save();

			var invoiceInOtherFactory = NewFactory().Load<JobComInvoiceHeader>(invoice.PK);

			var invoiceLine = invoiceInOtherFactory.JobComInvoiceLines.AddNew();
			invoiceLine.CA_AuthorityNumber = rulingCombined.ZZX_RulingNumber;

			var ruling = invoiceLine.Ruling;
			AssertNotNull("Should not null as the AuthorityNumber is a valid number.", ruling);
			AssertEquals("Should load the expected ruling from the CA_AuthorityNumber.", rulingCombined.PK, ruling.PK);

			rulingCombined.Delete();
			Factory.Save();

			Assert("Should be deleted from the data refresh bus.", ruling.IsDeleted);
			AssertNull("Should is null as the source ruling is deleted.", invoiceLine.Ruling);
		}

		public void TestSyncConfigurationsFromRuling()
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var ruling1 = Factory.New<ZZRefCusRulingCombined>();
			ruling1.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			ruling1.ZZX_RulingNumber = "RULING001";
			ruling1.ZZX_Description = "RULING001";
			ruling1.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			ruling1.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, RefCusRulingConfigTypes.Codes.Specific, 5, string.Empty);
			ruling1.Configurations.AddNew(RefCusRulingConfigCategories.Codes.EXC, RefCusRulingConfigTypes.Codes.AcceptAmount, 12, "T");
			ruling1.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DAT, RefCusRulingConfigTypes.Codes.REL, ZDecimal.Zero, ZString.Empty);

			var ruling2 = Factory.New<ZZRefCusRulingCombined>();
			ruling2.ZZX_RulingType = CalculationMethods.Codes.DeliveredDutyPaid;
			ruling2.ZZX_RulingNumber = "RULING002";
			ruling2.ZZX_Description = "RULING002";
			ruling2.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			ruling2.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DTY, RefCusRulingConfigTypes.Codes.Minimum, 0, "200");
			ruling2.Configurations.AddNew(RefCusRulingConfigCategories.Codes.SIM, RefCusRulingConfigTypes.Codes.ExemptCode, 0, "10");
			ruling2.Configurations.AddNew(RefCusRulingConfigCategories.Codes.GST, RefCusRulingConfigTypes.Codes.Rate, 32, string.Empty);
			ruling2.Configurations.AddNew(RefCusRulingConfigCategories.Codes.DAT, RefCusRulingConfigTypes.Codes.DSD, 0, string.Empty);

			Factory.Save();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.RulingConfigurations.RemoveAndDeleteAll();

			void AssertSyncConfigurations(ZZRefCusRulingCombined ruling)
			{
				var rulingNumber = ruling?.ZZX_RulingNumber ?? ZString.Empty;
				var countOfRulingConfigurations = ruling?.Configurations.Count ?? 0;

				invoiceLine.CA_AuthorityNumber = rulingNumber;

				if (ruling != null)
				{
					AssertEquals($"The total count should be {countOfRulingConfigurations - 1} whent the rulingNumber is '{rulingNumber}'.", countOfRulingConfigurations - 1, invoiceLine.RulingConfigurations.Count);
				}
				else
				{
					AssertEquals($"The total count should be {countOfRulingConfigurations} whent the rulingNumber is '{rulingNumber}'.", countOfRulingConfigurations, invoiceLine.RulingConfigurations.Count);
				}

				if (ruling != null)
				{
					foreach (var config in ruling.Configurations.Cast<CusRulingConfigCombined>())
					{
						var foundConfig = invoiceLine.RulingConfigurations
							.Cast<CusRulingConfigCombined>()
							.FirstOrDefault(c => c.ZZY_Category == config.ZZY_Category
								&& c.ZZY_Type == config.ZZY_Type
								&& c.ZZY_Rate == config.ZZY_Rate
								&& c.ZZY_Value == config.ZZY_Value
								&& c.ZZY_ZZX_CusRuling == ZGuid.Empty);
						if (config.ZZY_Category == RefCusRulingConfigCategories.Codes.DAT)
						{
							AssertNull("Should not sync config data with category DAT from the related ruling with all values except the ZZY_ZZX_CusRuling.", foundConfig);
						}
						else
						{
							AssertNotNull("Should sync all config data except data with category DAT from the related ruling with all values except the ZZY_ZZX_CusRuling.", foundConfig);
						}
					}
				}
			}

			AssertSyncConfigurations(ruling1);
			AssertSyncConfigurations(ruling2);
			AssertSyncConfigurations(null);
		}

		public void TestDefaultRemissionType()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var cusRuling1 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling1.ZZX_RulingType = "2";
			cusRuling1.ZZX_RulingNumber = "123456";
			cusRuling1.ZZX_Description = "D1";
			cusRuling1.ZZX_OA_AppliesTo = importer.MainAddress.PK;

			var cusRuling2 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling2.ZZX_RulingType = "T";
			cusRuling2.ZZX_RulingNumber = "123456";
			cusRuling2.ZZX_Description = "D2";
			cusRuling2.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			var cusRuling3 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling3.ZZX_RulingType = "DD";
			cusRuling3.ZZX_RulingNumber = "654321";
			cusRuling3.ZZX_Description = "D3";

			Factory.Save();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_AuthorityNumber = "123456";
			AssertEquals("2", invoiceLine1.CA_CalculationMethod);

			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_AuthorityNumber = "123456";
			AssertEquals("T", invoiceLine2.CA_CalculationMethod);

			invoiceLine2.CA_AuthorityNumber = "654321";
			invoiceLine2.AddInfoValidation.ValidateCA_AuthorityNumber();
			AssertEquals("T", invoiceLine2.CA_CalculationMethod);
			AssertHasMessageError(invoiceLine2.CA_AuthorityNumberInfo, ZZRefCusRulingValidator.AuthorityNumberDoesNotMatchRemissionType);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.CA_99TariffCode = DutyAndTaxManager.A99TariffCode0017;
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine3.CA_CalculationMethod);

			invoiceLine3.CA_CalculationMethod = ZString.Empty;
			invoiceLine3.CA_99TariffCode = DutyAndTaxManager.A99TariffCode0017;
			AssertEquals(ZString.Empty, invoiceLine3.CA_CalculationMethod);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			invoiceLine3.CA_99TariffCode = ZString.Empty;
			AssertEquals(CalculationMethods.Codes.NoRemission, invoiceLine3.CA_CalculationMethod);

			invoiceLine3.CA_99TariffCode = DutyAndTaxManager.A99TariffCode0017;
			invoiceLine3.CA_AuthorityNumber = "654321";
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine3.CA_CalculationMethod);

			invoiceLine3.CA_99TariffCode = ZString.Empty;
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine3.CA_CalculationMethod);

			invoiceLine3.CA_AuthorityNumber = ZString.Empty;
			AssertEquals(CalculationMethods.Codes.NoRemission, invoiceLine3.CA_CalculationMethod);

			invoiceLine3.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			invoiceLine3.CA_AuthorityNumber = ZString.Empty;
			invoiceLine3.CA_99TariffCode = ZString.Empty;
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine3.CA_CalculationMethod);
		}

		public void TestRulingConfigurations()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Assert(invoiceLine.RulingConfigurations.ReadOnly);
			AssertEquals(0, invoiceLine.RulingConfigurations.Count);
			invoiceLine.CA_CalculationMethod = "T";
			Assert(!invoiceLine.RulingConfigurations.ReadOnly);
			invoiceLine.RulingConfigurations.AddNew("DTY", "Maximum", 0.1m, ZString.Empty);
			AssertEquals(1, invoiceLine.RulingConfigurations.Count);
			invoiceLine.CA_CalculationMethod = "2";
			Assert(invoiceLine.RulingConfigurations.ReadOnly);
			AssertEquals(0, invoiceLine.RulingConfigurations.Count);
		}

		public void TestIsOGDCFIABlank()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "Inv1";
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line1.IsOGDCFIABlank);

			line1.CA_RequirementID = "ABC";
			AssertEquals(false, line1.IsOGDCFIABlank);

			var line2 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line2.IsOGDCFIABlank);

			line2.CFIARegistrationNumbers.AddNew();
			AssertEquals(false, line2.IsOGDCFIABlank);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.CountryOfOrigin;

			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();
			classification.CCA_TreatmentCode = "TR";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.CCA_RN_NKOrigin = "CA";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_OP = product.PK;
			line.JI_PartNo = product.OP_PartNum;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.CCA_RN_NKOrigin, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.JI_Tariff);
			AssertEquals("Refreshed by the registry, but synchronized by CI_CC", classification.CCA_TreatmentCode, line.CA_TreatmentCode);

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", pivot.CI_TariffNum, line.JI_Tariff);
			AssertEquals("Refreshed by the consignee config", pivot.CI_CC_CA_TreatmentCode, line.CA_TreatmentCode);
		}

		public void TestLVXB3LineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "Inv1";
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = line1.CusEntryLine;
			entry.CL_LineNumber = 2;

			AssertEquals(ZString.Empty, line1.LVXB3LineNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("2", line1.LVXB3LineNumber);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CAChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestGetContactDetails()
		{
			var broker = Factory.New<OrgHeader>();
			broker.OH_FullName = "TestBroker";
			broker.MainAddress.OA_Email = "broker@mail.com";
			broker.MainAddress.OA_Phone = "150";

			var importerHeader = Factory.New<OrgHeader>();
			importerHeader.OH_FullName = "TestImporter";

			var contact = importerHeader.Contacts.AddNew();
			contact.OC_Email = "importer@mail.com";
			contact.OC_Phone = "160";

			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CAPGA;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "staff@mail.com";
			staff.GS_WorkPhone = "170";

			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerHeader.PK;
			declaration.JE_GB = branch.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			var contactDetail = invoiceLine.GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Importer);
			AssertEquals("Should get contact detail from importer", "importer@mail.com", contactDetail.EmailAddress);
			AssertEquals("Should get contact detail from importer", "160", contactDetail.PhoneNumber);

			contactDetail = invoiceLine.GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Broker);
			AssertEquals("Should get contact detail from staff at first", "staff@mail.com", contactDetail.EmailAddress);
			AssertEquals("Should get contact detail from staff at first", "170", contactDetail.PhoneNumber);

			staff.GS_EmailAddress = ZString.Empty;
			staff.GS_WorkPhone = ZString.Empty;

			var branchBroker = Factory.New<OrgHeader>();
			branchBroker.OH_FullName = "TestBranchBroker";
			branchBroker.MainAddress.OA_Email = "branchBroker@mail.com";
			branchBroker.MainAddress.OA_Phone = "180";

			branch.GB_OH_OrgProxy = branchBroker.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			contactDetail = invoiceLine.GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Broker);
			AssertEquals("Should get contact detail from the proxy org of effective branch", "branchBroker@mail.com", contactDetail.EmailAddress);
			AssertEquals("Should get contact detail from the proxy org of effective branch", "180", contactDetail.PhoneNumber);
		}

		public void TestIsRegulatedByIIDCFIA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA, "0101210000");

			AssertEquals(true, invoiceLine.IsRegulatedByIIDCFIA);
		}

		public void TestCA_TradeNameShouldNotBeReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			AssertEquals(false, InvoiceLine.CA_TradeNameInfo.ReadOnly);

			invoiceLine.CA_NRCanInd = YesNoList.Codes.Yes;
			var nRCanHeader = invoiceLine.NRCanPGAHeader;
			nRCanHeader.CA_EEFProgramInd = YesNoList.Codes.Yes;

			AssertEquals(false, InvoiceLine.CA_TradeNameInfo.ReadOnly);

			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			var dfoHeader = invoiceLine.DFOPGAHeader;
			dfoHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;

			AssertEquals(false, InvoiceLine.CA_TradeNameInfo.ReadOnly);
		}

		public void TestIsTradeNameReadOnlyForCPRAndPES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;
			var hcHeader = invoiceLine.HCPGAHeader;
			hcHeader.CA_APIProgramInd = YesNoList.Codes.Yes;

			AssertEquals(false, invoiceLine.CA_TradeNameInfo.ReadOnly);
			AssertEquals(true, invoiceLine.CA_TradeNameCPRInfo.ReadOnly);
			AssertEquals(true, invoiceLine.CA_TradeNamePESInfo.ReadOnly);

			hcHeader.CA_CPRProgramInd = YesNoList.Codes.Yes;

			AssertEquals(false, invoiceLine.CA_TradeNameInfo.ReadOnly);
			AssertEquals(false, invoiceLine.CA_TradeNameCPRInfo.ReadOnly);
			AssertEquals(false, invoiceLine.CA_TradeNamePESInfo.ReadOnly);

			hcHeader.CA_PESProgramInd = YesNoList.Codes.No;

			AssertEquals(false, invoiceLine.CA_TradeNameInfo.ReadOnly);
			AssertEquals(false, invoiceLine.CA_TradeNameCPRInfo.ReadOnly);
			AssertEquals(false, invoiceLine.CA_TradeNamePESInfo.ReadOnly);
		}

		public void TestJI_Model_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.CA_DFOInd = YesNoList.Codes.No;

			AssertEquals(false, line.JI_ModelInfo.ReadOnly);

			line.CA_DFOInd = YesNoList.Codes.Yes;

			var pgaHeader = line.DFOPGAHeader;
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.No;

			Factory.InvalidateCachedProperties();
			AssertEquals(false, line.JI_ModelInfo.ReadOnly);

			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_CommonNameCode = ZString.Empty;

			Factory.InvalidateCachedProperties();
			AssertEquals(false, line.JI_ModelInfo.ReadOnly);

			pgaHeader.CA_CommonNameCode = DFOCommonNameCodes.Codes.FO21;

			Factory.InvalidateCachedProperties();
			AssertEquals(true, line.JI_ModelInfo.ReadOnly);
		}

		public void TestCA_CFIAAllProgramInd_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_CFIAAllProgramIndInfo.ReadOnly);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals(false, line.CA_CFIAAllProgramIndInfo.ReadOnly);
		}

		public void TestCA_HCProgramInd_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_APIProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_BBCProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_CTOProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_CPRProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_DSEProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_HDRProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_OCSProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_MDEProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_NHPProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_PESProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_REDProgramIndInfo.ReadOnly);
			AssertEquals(true, line.CA_VETProgramIndInfo.ReadOnly);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals(false, line.CA_APIProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_BBCProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_CTOProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_CPRProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_DSEProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_HDRProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_OCSProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_MDEProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_NHPProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_PESProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_REDProgramIndInfo.ReadOnly);
			AssertEquals(false, line.CA_VETProgramIndInfo.ReadOnly);
		}

		public void TestCA_ProductionDate_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_ProductionDateInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateAPIInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateCPRInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateHDRInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateMDEInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateNHPInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDatePESInfo.ReadOnly);
			AssertEquals(true, line.CA_ProductionDateVETInfo.ReadOnly);

			line.CA_APIProgramInd = true;
			AssertEquals(false, line.CA_ProductionDateInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateAPIInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateCPRInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateHDRInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateMDEInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateNHPInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDatePESInfo.ReadOnly);
			AssertEquals(false, line.CA_ProductionDateVETInfo.ReadOnly);
		}

		public void TestCA_ExpiryDate_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_ExpiryDateInfo.ReadOnly);
			AssertEquals(true, line.CA_ExpiryDateBBCInfo.ReadOnly);
			AssertEquals(true, line.CA_ExpiryDateCTOInfo.ReadOnly);

			line.CA_BBCProgramInd = true;
			AssertEquals(false, line.CA_ExpiryDateInfo.ReadOnly);
			AssertEquals(false, line.CA_ExpiryDateBBCInfo.ReadOnly);
			AssertEquals(false, line.CA_ExpiryDateCTOInfo.ReadOnly);
		}

		public void TestCA_GTINNumber_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_GTINNumberInfo.ReadOnly);

			line.CA_APIProgramInd = true;
			AssertEquals(false, line.CA_GTINNumberInfo.ReadOnly);
		}

		public void TestCA_BatchLotNumber_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_BatchLotNumberInfo.ReadOnly);

			line.CA_APIProgramInd = true;
			AssertEquals(false, line.CA_BatchLotNumberInfo.ReadOnly);
		}

		public void TestCA_Manufacturer_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_OA_ManufacturerAddressCPRInfo.ReadOnly);
			AssertEquals(true, line.CA_OA_ManufacturerAddressOCSInfo.ReadOnly);
			AssertEquals(true, line.CA_OA_ManufacturerAddressPESInfo.ReadOnly);

			line.CA_CPRProgramInd = true;
			AssertEquals(false, line.CA_OA_ManufacturerAddressCPRInfo.ReadOnly);
			AssertEquals(false, line.CA_OA_ManufacturerAddressOCSInfo.ReadOnly);
			AssertEquals(false, line.CA_OA_ManufacturerAddressPESInfo.ReadOnly);
		}

		public void TestCA_CFIA_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_RN_NKCountryOfSourceCFIAInfo.ReadOnly);
			AssertEquals(true, line.CA_AIRSEndUseCFIAInfo.ReadOnly);
			AssertEquals(true, line.CA_AIRSExtensionCodeCFIAInfo.ReadOnly);
			AssertEquals(true, line.CA_OA_ConsigneeAddressCFIAInfo.ReadOnly);
			AssertEquals(true, line.CA_RW_NKSourceStateCFIAInfo.ReadOnly);
			AssertEquals(true, line.CA_AIRSMiscellaneousCFIAInfo.ReadOnly);

			line.CA_CFIAAllProgramInd = true;
			AssertEquals(false, line.CA_RN_NKCountryOfSourceCFIAInfo.ReadOnly);
			AssertEquals(false, line.CA_AIRSEndUseCFIAInfo.ReadOnly);
			AssertEquals(false, line.CA_AIRSExtensionCodeCFIAInfo.ReadOnly);
			AssertEquals(false, line.CA_OA_ConsigneeAddressCFIAInfo.ReadOnly);
			AssertEquals(false, line.CA_AIRSMiscellaneousCFIAInfo.ReadOnly);

			line.CA_RN_NKSource = "US";
			AssertEquals(false, line.CA_RW_NKSourceStateCFIAInfo.ReadOnly);
		}

		public void TestCA_HC_API_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeAPIInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryAPIInfo.ReadOnly);

			line.CA_APIProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeAPIInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryAPIInfo.ReadOnly);
		}

		public void TestCA_HC_BBC_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeBBCInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryBBCInfo.ReadOnly);

			line.CA_BBCProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeBBCInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryBBCInfo.ReadOnly);
		}

		public void TestCA_HC_CTO_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeCTOInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryCTOInfo.ReadOnly);
			AssertEquals(true, line.CA_CTO_LCOInfo.ReadOnly);

			line.CA_CTOProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeCTOInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryCTOInfo.ReadOnly);
			AssertEquals(false, line.CA_CTO_LCOInfo.ReadOnly);
		}

		public void TestCA_HC_CPR_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeCPRInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryCPRInfo.ReadOnly);

			line.CA_CPRProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeCPRInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryCPRInfo.ReadOnly);
		}

		public void TestCA_HC_DSE_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeDSEInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryDSEInfo.ReadOnly);
			AssertEquals(true, line.CA_ComplianceStatementInfo.ReadOnly);

			line.CA_DSEProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeDSEInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryDSEInfo.ReadOnly);
			AssertEquals(false, line.CA_ComplianceStatementInfo.ReadOnly);
		}

		public void TestCA_HC_HDR_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeHDRInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryHDRInfo.ReadOnly);

			line.CA_HDRProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeHDRInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryHDRInfo.ReadOnly);
		}

		public void TestCA_HC_OCS_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeOCSInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryOCSInfo.ReadOnly);

			line.CA_OCSProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeOCSInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryOCSInfo.ReadOnly);
		}

		public void TestCA_HC_MDE_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeMDEInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryMDEInfo.ReadOnly);
			AssertEquals(true, line.CA_UniqueDeviceIDNumberInfo.ReadOnly);
			AssertEquals(true, line.CA_MDE_LEXInfo.ReadOnly);

			line.CA_MDEProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeMDEInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryMDEInfo.ReadOnly);
			AssertEquals(false, line.CA_UniqueDeviceIDNumberInfo.ReadOnly);
			AssertEquals(false, line.CA_MDE_LEXInfo.ReadOnly);
		}

		public void TestCA_HC_NHP_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeNHPInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryNHPInfo.ReadOnly);

			line.CA_NHPProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeNHPInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryNHPInfo.ReadOnly);
		}

		public void TestCA_HC_PES_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodePESInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryPESInfo.ReadOnly);
			AssertEquals(true, line.CA_CASNumberInfo.ReadOnly);
			AssertEquals(true, line.CA_PES_SPCPInfo.ReadOnly);
			AssertEquals(true, line.CA_PES_EPCPInfo.ReadOnly);

			line.CA_PESProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodePESInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryPESInfo.ReadOnly);
			AssertEquals(false, line.CA_CASNumberInfo.ReadOnly);
			AssertEquals(false, line.CA_PES_SPCPInfo.ReadOnly);
			AssertEquals(false, line.CA_PES_EPCPInfo.ReadOnly);
		}

		public void TestCA_HC_RED_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_CategoryREDInfo.ReadOnly);
			AssertEquals(true, line.CA_FDANumberInfo.ReadOnly);

			line.CA_REDProgramInd = true;
			AssertEquals(false, line.CA_CategoryREDInfo.ReadOnly);
			AssertEquals(false, line.CA_FDANumberInfo.ReadOnly);
		}

		public void TestCA_HC_VET_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_IntendedUseCodeVETInfo.ReadOnly);
			AssertEquals(true, line.CA_CategoryVETInfo.ReadOnly);

			line.CA_VETProgramInd = true;
			AssertEquals(false, line.CA_IntendedUseCodeVETInfo.ReadOnly);
			AssertEquals(false, line.CA_CategoryVETInfo.ReadOnly);
		}

		public void TestCA_RN_NKSource()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			AssertEquals("Default to empty", ZString.Empty, invoiceLine.CA_RN_NKSource);

			header.CA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Should get from the header as the value is empty on line.", Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKSource);

			invoiceLine.CA_RN_NKSource = Core.Constants.CountryCodes.Australia;
			AssertEquals("Should get from the line as the value is not empty.", Core.Constants.CountryCodes.Australia, invoiceLine.CA_RN_NKSource);
		}

		public void TestCA_StateOfSource()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			AssertEquals("Default to empty", ZString.Empty, invoiceLine.CA_StateOfSource);

			header.CA_StateOfSource = "NY";
			AssertEquals("Should get from the header as the value is empty on line.", "NY", invoiceLine.CA_StateOfSource);

			invoiceLine.CA_StateOfSource = "MI";
			AssertEquals("Should get from the line as the value is not empty.", "MI", invoiceLine.CA_StateOfSource);

			invoiceLine.CA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			Assert(!invoiceLine.CA_StateOfSourceInfo.ReadOnly);
			invoiceLine.CA_StateOfSource = USStatesList.Codes.Alabama;

			invoiceLine.CA_RN_NKSource = Core.Constants.CountryCodes.Canada;
			Assert(invoiceLine.CA_StateOfSourceInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine.CA_StateOfSource);
		}

		public void TestJI_CustomsValueInUSD()
		{
			var now = ZDateTime.Now;

			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates) ?? Factory.New<RefCurrency>();

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

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header = declaration.Invoices.AddNew();

			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.CA_CustomsValue = 150m;

			AssertEquals("Customs Value In CAD", 150m, line.JI_CustomsValue);
			AssertEquals("Customs Value In USD", 75m, line.JI_CustomsValueInUSD);
		}

		public void TestCopyPartNoToTradeName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			line.CA_TradeName = ZString.Empty;
			line.JI_PartNo = "0001";

			AssertEquals("0001", line.JI_PartNo);
			AssertEquals("Copy from JI_PartNo", "0001", line.CA_TradeName);

			line.JI_PartNo = "0002";

			AssertEquals("0002", line.JI_PartNo);
			AssertEquals("Should not change as the CA_TradeName is not empty.", "0001", line.CA_TradeName);
		}

		public void TestDefaultPGAIndicatorsWhenTariffChanges()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824700308", "TPR");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var header = declaration.Invoices.AddNew();
			header.JZ_ValuationDateOverride = new ZDateTime(2016, 1, 1);
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3824700308";
			AssertEquals("Y", invoiceLine.CA_TCInd);
		}

		public void TestJI_OA_ManufacturerAddress()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			OrgAddress address1 = manufacturer.MainAddress;
			address1.Address1 = "testAddress";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = address1.PK;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(address1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("testAddress", invoiceLine.JI_OA_ManufacturerAddress_ZAddress.AddressFull);
		}

		public void TestNotCloneSplitLinesFromIM2()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer();
			ZString hsCode = "0123";
			ZString tariff1 = "7326909031";
			ZString tariff2 = "7326909032";
			ZString auNumber = "1234";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line1.JI_FormattedTariff = tariff1;
			line1.CA_99TariffCode = hsCode;
			line1.CA_AuthorityNumber = auNumber;
			line2.JI_FormattedTariff = tariff1;
			line2.CA_99TariffCode = hsCode;
			line2.CA_AuthorityNumber = auNumber;

			declaration.DoMerge(notifier);
			var b2Declaration = declaration.GetNewCopyToB2Declaration();
			b2Declaration.DoMerge(notifier);
			Factory.Save();
			var entryLine = b2Declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).AllEntryLines;
			AssertEquals("Merged to 1 entry lines", 1, entryLine.Count);

			b2Declaration.Invoices[0].InvoiceLines[1].JI_FormattedTariff = tariff2;
			b2Declaration.DoMerge(notifier);
			b2Declaration.CA_B2AcceptedDate = new ZDateTime(2016, 01, 01);
			b2Declaration.TransactionNumber.SequentialNumber = "00000002";
			Factory.Save();
			entryLine = b2Declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).AllEntryLines;

			AssertEquals("Merged to 2 entry lines", 2, entryLine.Count);
			AssertEquals(line1.JI_B3LineNumber, entryLine[0].CA_B2LineNo.ToString());
			AssertEquals(line1.JI_B3LineNumber + "/SL", entryLine[1].CA_B2LineNo.ToString());

			var b2DeclarationNerVersion = b2Declaration.GetNewCopyToB2Declaration();
			b2DeclarationNerVersion.DoMerge(notifier);
			Factory.Save();

			AssertEquals("Only 1 row copied", 1, b2DeclarationNerVersion.Invoices[0].InvoiceLines.Count);
			Assert(!((JobComInvoiceLine)b2DeclarationNerVersion.Invoices[0].InvoiceLines[0]).JI_B3LineNumber.EndsWith("/SL", StringComparison.OrdinalIgnoreCase));
		}

		public void TestCA_TreatmentCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_ParentTableCode = invoiceLine.TablePrefix;
			Factory.Save();

			AssertEquals(ZString.Empty, invoiceLine2.CA_TreatmentCode);

			invoice.CA_TreatmentCode = "01";
			AssertEquals("01", invoiceLine.CA_TreatmentCode);
			AssertEquals("01", invoiceLine2.CA_TreatmentCode);

			invoiceLine.CA_TreatmentCode = "02";
			AssertEquals("02", invoiceLine2.CA_TreatmentCode);

			invoiceLine.CA_TreatmentCode = "";
			invoice.CA_TreatmentCode = "03";
			AssertEquals("03", invoiceLine.CA_TreatmentCode);
			AssertEquals("03", invoiceLine2.CA_TreatmentCode);

			var b2Declaration = Factory.New<JobDeclaration>();
			b2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var asAccountedInvoice = b2Declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asAccountedInvoiceLine = asAccountedInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine.CA_OriginalLineNo = "1";
			var asClaimedInvoice = asAccountedInvoice.CorrespondingAsClaimedForInvoice;
			var asClaimedInvoiceLine = asAccountedInvoiceLine.CorrespondingAsClaimedForInvoiceLine;
			asAccountedInvoice.CA_TreatmentCode = "04";
			asClaimedInvoice.CA_TreatmentCode = "02";
			AssertEquals("02", asClaimedInvoiceLine.CA_TreatmentCode);

			var b3XDeclaration = Factory.New<JobDeclaration>();
			b3XDeclaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			var asAccountedInvoiceX = b3XDeclaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoiceX.JZ_InvoiceNumber = "INV1";
			var asAccountedInvoiceLineX = asAccountedInvoiceX.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLineX.CA_OriginalLineNo = "1";
			var asClaimedInvoiceX = asAccountedInvoiceX.CorrespondingAsClaimedForInvoice;
			var asClaimedInvoiceLineX = asAccountedInvoiceLineX.CorrespondingAsClaimedForInvoiceLine;
			asAccountedInvoiceX.CA_TreatmentCode = "04";
			asClaimedInvoiceX.CA_TreatmentCode = "02";
			AssertEquals("02", asClaimedInvoiceLineX.CA_TreatmentCode);
		}

		public void TestCA_CalculationMethod_ReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert("is not read_only", !InvoiceLine.CA_CalculationMethodInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			Assert("is read_only", InvoiceLine.CA_CalculationMethodInfo.ReadOnly);
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			Assert("is read_only", InvoiceLine.CA_CalculationMethodInfo.ReadOnly);
			Declaration.CA_AllowOIC = true;
			Assert("is not read_only", !InvoiceLine.CA_CalculationMethodInfo.ReadOnly);
		}

		public void TestGetAdditionalDataForBorderWise()
		{
			Declaration.JE_MessageType = Business.JobMessageTypeList.Codes.B2Adjustments;
			Declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 5, 6);
			IHaveAdditionalDataForBorderWise additionalDataSource = InvoiceLine;
			AdditionalDataForBorderWise additionalData = additionalDataSource.GetAdditionalDataForBorderWise("");
			AssertEquals("AdditionalData.ParameterForBorderWise", "I", additionalData.ParameterForBorderWise);
			AssertEquals("AdditionalData.DateForDutyRate", new ZDateTime(2013, 5, 6), additionalData.DateForDutyRate);
		}

		public void TestGetPivotType()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoice.GetPropertyType()", ClassificationTypeList.Codes.SHB, invoiceLine.GetPartPivotType());

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.InvalidateCachedProperties();
			AssertEquals("invoice.GetPropertyType()", ClassificationTypeList.Codes.HTE, invoiceLine.GetPartPivotType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("invoice.GetPropertyType()", ClassificationTypeList.Codes.HTI, invoiceLine.GetPartPivotType());
		}

		public void TestSynchroniser()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			AssertNotNull(asClaimedInvoice);

			var line = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			var asClaimedLine = line.CorrespondingAsClaimedForInvoiceLine;
			AssertNotNull(asClaimedLine);
			AssertEquals("1", asClaimedLine.CA_OriginalLineNo);

			AssertNotEquals("0000", asClaimedLine.CA_99TariffCode);
			line.CA_99TariffCode = "0000";
			AssertEquals("0000", asClaimedLine.CA_99TariffCode);
			Factory.Save();

			var factoryReload = new BusinessObjectFactory();
			var decReload = factoryReload.Load<JobDeclaration>(declaration.PK);
			AssertEquals(1, decReload.B2AsAccountedForInvoices.Count);
			AssertEquals(1, decReload.B2AsClaimedForInvoices.Count);
			var asAccountInvoiceReload = decReload.B2AsAccountedForInvoices[0];
			var asClaimedInvoiceReload = decReload.B2AsClaimedForInvoices[0];
			AssertEquals(1, asAccountInvoiceReload.AsAccountForFilteredInvoiceLines.Count);
			AssertEquals(1, asClaimedInvoiceReload.AsClaimForFilteredInvoiceLines.Count);
			var asAccountLineReload = asAccountInvoiceReload.AsAccountForFilteredInvoiceLines[0];
			var asClaimedLineReload = asClaimedInvoiceReload.AsClaimForFilteredInvoiceLines[0];
			AssertNotEquals("test", asClaimedLineReload.JI_Description);
			asAccountLineReload.JI_Description = "test";
			AssertEquals("test", asClaimedLineReload.JI_Description);
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			invoiceLine.CA_OriginalLineNo = "1";
			var asClaimedInvoiceLine = invoiceLine.CorrespondingAsClaimedForInvoiceLine;
			AssertNotNull(asClaimedInvoiceLine);
			var asClaimedInvoiceLinePK = asClaimedInvoiceLine.PK;
			Factory.Save();

			invoiceLine.Delete();
			var asClaimedInvoiceLine1 = Factory.Load<JobComInvoiceLine>(asClaimedInvoiceLinePK);
			AssertNull(asClaimedInvoiceLine1);
		}

		public void TestCanBeSynchronizedAfterReload()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var declarationPK = declaration.PK;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var invoiceLine = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			invoiceLine.CA_OriginalLineNo = "1";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var declaration1 = factory.Load<JobDeclaration>(declarationPK);
			var invoice1 = declaration1.B2AsAccountedForInvoices[0];
			var invoiceLine1 = invoice1.AsAccountForFilteredInvoiceLines[0];
			var asClaimedInvoiceLine = invoiceLine1.CorrespondingAsClaimedForInvoiceLine;
			AssertNotEquals("0000", asClaimedInvoiceLine.CA_99TariffCode);
			invoiceLine1.CA_99TariffCode = "0000";
			AssertEquals("0000", asClaimedInvoiceLine.CA_99TariffCode);
		}

		public void TestJI_ParentID_ReadOnly()
		{
			AssertJI_ParentID_ReadOnly(JobMessageTypeList.Codes.B2Adjustments);
			AssertJI_ParentID_ReadOnly(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertJI_ParentID_ReadOnly(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.B2AsAccountedForInvoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = invoice.CorrespondingAsClaimedForInvoice;
			AssertNotNull(asClaimedInvoice);

			var line = invoice.AsAccountForFilteredInvoiceLines.AddNew();
			line.CA_OriginalLineNo = "1";
			var asClaimedLine = line.CorrespondingAsClaimedForInvoiceLine;

			Assert(asClaimedLine.JI_ParentIDInfo.ReadOnly);
		}

		public void TestGetWarningBeforeBeingDeleted()
		{
			AssertGetWarningBeforeBeingDeleted(JobMessageTypeList.Codes.B2Adjustments);
			AssertGetWarningBeforeBeingDeleted(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertGetWarningBeforeBeingDeleted(ZString messageType)
		{
			var b2Declaration = Factory.New<JobDeclaration>();
			b2Declaration.JE_MessageType = messageType;
			var asAccountedInvoice = b2Declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asAccountedInvoiceLine = asAccountedInvoice.AsAccountForFilteredInvoiceLines.AddNew();
			asAccountedInvoiceLine.CA_OriginalLineNo = "1";
			var asClaimedInvoice = asAccountedInvoice.CorrespondingAsClaimedForInvoice;
			var asClaimedInvoiceLine = asAccountedInvoiceLine.CorrespondingAsClaimedForInvoiceLine;
			AssertEquals("Deleting a line in 'As Claimed' will delete the corresponding line in 'As Accounted'.", asClaimedInvoiceLine.GetWarningBeforeBeingDeleted());

			var asClaimedInvoiceLine1 = asClaimedInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			asClaimedInvoiceLine1.JI_ParentID = asAccountedInvoiceLine.PK;
			asClaimedInvoiceLine1.JI_ParentTableCode = asAccountedInvoice.TablePrefix;
			AssertNullOrEmpty(asClaimedInvoiceLine1.GetWarningBeforeBeingDeleted());
		}

		public void TestSetingNewSubHeaderForB2AndB3X()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "NS";
			AssertEquals("NS1", invoice1.JZ_InvoiceNumber);
			var invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "NS";
			AssertEquals("NS2", invoice2.JZ_InvoiceNumber);

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;

			invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "NS";
			AssertEquals("NS1", invoice1.JZ_InvoiceNumber);
			invoice2 = dec.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "NS";
			AssertEquals("NS2", invoice2.JZ_InvoiceNumber);
		}

		public void TestClearCachedValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var dty1 = invoiceLine.DutiesAndTaxes.AddNew("DTY");
			dty1.C1_Override = true;
			dty1.C1_Amount = 15m;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(15m, invoiceLine.DutyAndTaxManager.NormalDutyPaidValue);
			AssertEquals(15m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals(15m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);

			dty1.C1_Amount = 0m;
			invoiceLine.DutyAndTaxManager.ClearCachedValues();
			AssertEquals(0m, invoiceLine.DutyAndTaxManager.NormalDutyPaidValue);
			AssertEquals(0m, invoiceLine.DutyAndTaxManager.NormalValueForTax);
			AssertEquals(0m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);
		}

		public void TestRefreshDutiesAndTaxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLineForTesting)invoice.InvoiceLines.AddNew(typeof(JobComInvoiceLineForTesting));

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertNotNull(invoiceLine.dutiesAndTaxesExposed);

			invoiceLine.RefreshDutiesAndTaxes();
			AssertNull(invoiceLine.dutiesAndTaxesExposed);
		}

		public void TestCA_OriginalLineNo()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var subHeader = dec.B2AsAccountedForInvoices.AddNew();
			subHeader.JZ_InvoiceNumber = "INV1";
			var accountLine = subHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			var claimLine1 = accountLine.CorrespondingAsClaimedForInvoiceLine;
			AssertEquals("1", claimLine1.CA_OriginalLineNo);
			var claimInvoice = subHeader.CorrespondingAsClaimedForInvoice;
			var claimLine2 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			claimLine2.JI_ParentID = accountLine.PK;
			claimLine2.JI_ParentTableCode = accountLine.TablePrefix;
			AssertEquals("1/SL", claimLine2.CA_OriginalLineNo);
			accountLine.CA_OriginalLineNo = "2";
			AssertEquals("2", claimLine1.CA_OriginalLineNo);
			AssertEquals("2/SL", claimLine2.CA_OriginalLineNo);

			var claimLine3 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			Assert(!claimLine3.CA_OriginalLineNoInfo.ReadOnly);
			claimLine3.CA_OriginalLineNo = "3";
			Assert(claimLine3.CA_OriginalLineNoInfo.ReadOnly);
			AssertEquals("3", claimLine3.CA_OriginalLineNo);
			claimLine3.JI_ParentID = accountLine.PK;
			claimLine3.JI_ParentTableCode = accountLine.TablePrefix;
			Assert(claimLine3.CA_OriginalLineNoInfo.ReadOnly);
			AssertEquals("2/SL", claimLine3.CA_OriginalLineNo);

			var claimLine4 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			Assert(!claimLine4.CA_OriginalLineNoInfo.ReadOnly);
			claimLine4.CA_OriginalLineNo = "2";
			Assert(claimLine4.CA_OriginalLineNoInfo.ReadOnly);
			AssertEquals("2/SL", claimLine4.CA_OriginalLineNo);
			AssertEquals(accountLine.PK, claimLine4.JI_ParentID);
			AssertEquals(accountLine.TablePrefix, claimLine4.JI_ParentTableCode);

			var claimLine5 = claimInvoice.AsClaimForFilteredInvoiceLines.AddNew();
			Assert(!claimLine5.CA_OriginalLineNoInfo.ReadOnly);
			claimLine5.CA_OriginalLineNo = "2/sl";
			Assert(claimLine5.CA_OriginalLineNoInfo.ReadOnly);
			AssertEquals("2/SL", claimLine5.CA_OriginalLineNo);
			AssertEquals(accountLine.PK, claimLine5.JI_ParentID);
			AssertEquals(accountLine.TablePrefix, claimLine5.JI_ParentTableCode);
		}

		public void TestCA_OriginalLineNo_ShouldBeOrdered()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var accountHeader = dec.B2AsAccountedForInvoices.AddNew();
			accountHeader.JZ_InvoiceNumber = "INV1";
			var accountLine = accountHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine.CA_OriginalLineNo = "1";
			accountLine.JI_Description = "TEST1";
			accountLine.CA_CVforCurrConv = 1;
			accountLine.JI_LineNo = 1;
			var accountLine2 = accountHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine2.CA_OriginalLineNo = "2";
			accountLine2.JI_Description = "TEST2";
			accountLine2.CA_CVforCurrConv = 2;
			accountLine2.JI_LineNo = 2;
			var accountLine3 = accountHeader.AsAccountForFilteredInvoiceLines.AddNew();
			accountLine3.CA_OriginalLineNo = "3";
			accountLine3.JI_Description = "TEST3";
			accountLine3.CA_CVforCurrConv = 3;
			accountLine3.JI_LineNo = 3;

			var claimHeader = accountHeader.CorrespondingAsClaimedForInvoice;
			var claimLine_SL = claimHeader.AsClaimForFilteredInvoiceLines.AddNew();
			claimLine_SL.JI_ParentID = accountLine2.PK;
			claimLine_SL.JI_ParentTableCode = accountLine2.TablePrefix;
			claimLine_SL.JI_LineNo = 4;
			AssertEquals("2/SL", claimLine_SL.CA_OriginalLineNo);

			var claimLine2 = accountLine2.CorrespondingAsClaimedForInvoiceLine;
			AssertEquals("2", claimLine2.CA_OriginalLineNo);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var decReload = newFactory.Load<JobDeclaration>(dec.PK);
			var claimHeaderReload = decReload.B2AsClaimedForInvoices[0];
			var claimLine2Reload = claimHeaderReload.AsClaimForFilteredInvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "TEST2");
			AssertEquals("2", claimLine2Reload.CA_OriginalLineNo);
		}

		public void TestCA_IsCasualImport_ReadOnly()
		{
			InvoiceHeader.CA_IsCasualImport = true;
			Assert("is read_only", InvoiceLine.CA_IsCasualImportInfo.ReadOnly);
			InvoiceHeader.CA_IsCasualImport = false;
			Assert("is not read_only", !InvoiceLine.CA_IsCasualImportInfo.ReadOnly);
		}

		public void TestCA_IsCasualImportValues_ReadOnly()
		{
			InvoiceLine.CA_IsCasualImport = true;
			Assert("is not read_only", !InvoiceLine.CA_CasualImportCommodityInfo.ReadOnly);
			Assert("is not read_only", !InvoiceLine.CA_CasualImportDestinationProvinceInfo.ReadOnly);
			Assert("is not read_only", !InvoiceLine.CA_IsExemptInfo.ReadOnly);
			InvoiceLine.CA_IsCasualImport = false;
			Assert("is read_only", InvoiceLine.CA_CasualImportCommodityInfo.ReadOnly);
			Assert("is read_only", InvoiceLine.CA_CasualImportDestinationProvinceInfo.ReadOnly);
			Assert("is read_only", InvoiceLine.CA_IsExemptInfo.ReadOnly);
		}

		public void TestCA_PageRelativeLineNumber_ReadOnly()
		{
			Assert("is read_only", InvoiceLine.CA_PageRelativeLineNumberInfo.ReadOnly);
			Declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("is not read_only", !InvoiceLine.CA_PageRelativeLineNumberInfo.ReadOnly);

			var cadEntry = Declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			Declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("is not read_only", !InvoiceLine.CA_PageRelativeLineNumberInfo.ReadOnly);
		}

		public void TestCA_AuthorityNumber_ReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			Assert("is not read_only", !InvoiceLine.CA_AuthorityNumberInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			Assert("is read_only", InvoiceLine.CA_AuthorityNumberInfo.ReadOnly);
			Declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			Assert("is read_only", InvoiceLine.CA_AuthorityNumberInfo.ReadOnly);
			Declaration.CA_AllowOIC = true;
			Assert("is not read_only", !InvoiceLine.CA_AuthorityNumberInfo.ReadOnly);
		}

		public void TestIsB2SeededLine()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceLine.CA_IsAccountForLine = true;
			InvoiceLine.CA_IsSeeded = false;
			Assert(!InvoiceLine.IsB2AsAccountForSeededLine);
			Assert(!InvoiceLine.ReadOnly);

			InvoiceLine.CA_IsSeeded = true;
			Assert(InvoiceLine.IsB2AsAccountForSeededLine);
			Assert(InvoiceLine.ReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			InvoiceLine.CA_IsAccountForLine = true;
			InvoiceLine.CA_IsSeeded = false;
			Assert(!InvoiceLine.IsB2AsAccountForSeededLine);

			InvoiceLine.CA_IsSeeded = true;
			Assert(InvoiceLine.IsB2AsAccountForSeededLine);
			Assert(InvoiceLine.ReadOnly);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusAddInfoTypeSupporter supporter = invoiceLine;
			supporter.AssertType(typeof(DutyAndTax), CusAddInfoTypeAttribute.Codes.CADutyAndTax);
			supporter.AssertType(null, "ZZ!");
			var dutyAndTax = invoiceLine.DutiesAndTaxes.AddNew();
			dutyAndTax.C1_Code = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(dutyAndTax.PK);
			AssertEquals(typeof(DutyAndTax), addInfo.GetType());
			AssertEquals("1", ((DutyAndTax)addInfo).C1_Code);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusCodeDataTypeSupporter supporter = invoiceLine;
			supporter.AssertType(typeof(SITTCertificationNumber), CusCodeDataTypeList.Codes.SITTNumber);
			supporter.AssertType(typeof(InvoiceLineExportPermit), CusCodeDataTypeList.Codes.Permit);
			supporter.AssertType(typeof(CFIARegistrationNumber), CusCodeDataTypeList.Codes.CFIANumber);
			supporter.AssertType(null, "ZZ!");

			var sITTCertificationNumber = invoiceLine.SITTCertificationNumbers.AddNew();
			sITTCertificationNumber.CY_Data = "1";
			var permit = invoiceLine.Permits.AddNew();
			permit.CY_Data = "1";
			var cFIARegistrationNumber = invoiceLine.CFIARegistrationNumbers.AddNew();
			cFIARegistrationNumber.CY_Data = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(sITTCertificationNumber.PK);
			AssertEquals(typeof(SITTCertificationNumber), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(permit.PK);
			AssertEquals(typeof(InvoiceLineExportPermit), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(cFIARegistrationNumber.PK);
			AssertEquals(typeof(CFIARegistrationNumber), codeData.GetType());
		}

		public void TestGetDutyAndTaxFrom()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceLine.DutiesAndTaxes.DeleteAll();
			var sima = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
			sima.C1_ExemptCode = SIMACodes.Codes.C20;
			sima.C1_Amount = 10m;
			var gst = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			gst.C1_ExemptCode = GSTStatusCodes.Codes.C49;
			gst.C1_Amount = 20m;
			gst.C1_Rate = 0.2m;
			gst.C1_RateType = RateTypes.Codes.Specific;
			var excise = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ExciseTax);
			excise.C1_ExemptCode = ExciseTaxExemptionCodes.Codes.C86;
			excise.C1_Amount = 30m;
			excise.C1_RateType = RateTypes.Codes.AdValorem;
			excise.C1_Rate = 0.2m;
			var duty1 = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty1.C1_Amount = 40m;
			duty1.C1_Rate = 0.2m;
			duty1.C1_RateType = RateTypes.Codes.Specific;
			var duty2 = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty2.C1_Amount = 50m;
			duty2.C1_Rate = 0.2m;
			duty2.C1_RateType = RateTypes.Codes.AdValorem;
			var duty3 = InvoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			duty3.C1_Amount = 60m;
			duty3.C1_Rate = 0.2m;
			duty3.C1_RateType = RateTypes.Codes.Free;

			var classLine1 = new ExpectedClassificationLine1ForTesting();
			classLine1.SIMACode = SIMACodes.Codes.C10;
			classLine1.SIMAAssessment = 1m;
			classLine1.GSTExemptionCode = GSTStatusCodes.Codes.C48;
			classLine1.GSTAmount = 2m;
			classLine1.RateOfGST = 0.1m;
			classLine1.GSTRateType = RateTypes.Codes.AdValorem;
			classLine1.ExciseTaxRateType = RateTypes.Codes.Specific;
			classLine1.ExciseExemptionCode = ExciseTaxExemptionCodes.Codes.C85;
			classLine1.ExciseTaxAmount = 3m;
			classLine1.ExciseTaxRate = 0.1m;

			var classLine2_1 = new ExpectedClassificationLine2ForTesting();
			classLine2_1.CustomsDutyRateType = RateTypes.Codes.Specific;
			classLine2_1.CustomsDutyAmount = 4m;
			classLine2_1.CustomsDutyRate = 0.2m;
			var classLine2_2 = new ExpectedClassificationLine2ForTesting();
			classLine2_2.CustomsDutyRateType = RateTypes.Codes.AdValorem;
			classLine2_2.CustomsDutyAmount = 5m;
			classLine2_2.CustomsDutyRate = 0.2m;
			var classLine2_3 = new ExpectedClassificationLine2ForTesting();
			classLine2_3.CustomsDutyRateType = RateTypes.Codes.Free;
			classLine2_3.CustomsDutyAmount = 6m;
			classLine2_3.CustomsDutyRate = 0.2m;

			classLine1.ClassificationLines = new[] { classLine2_1, classLine2_2, classLine2_3 };

			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;

			InvoiceLine.GetDutyAndTaxFrom(classLine1);
			AssertEquals(SIMACodes.Codes.C10, sima.C1_ExemptCode);
			AssertEquals(1m, sima.C1_Amount);
			Assert(sima.C1_Override);

			AssertEquals(GSTStatusCodes.Codes.C48, gst.C1_ExemptCode);
			AssertEquals(2m, gst.C1_Amount);
			AssertEquals(0.1m, gst.C1_Rate);
			AssertEquals(RateTypes.Codes.AdValorem, gst.C1_RateType);
			Assert(gst.C1_Override);

			AssertEquals(ExciseTaxExemptionCodes.Codes.C85, excise.C1_ExemptCode);
			AssertEquals(3m, excise.C1_Amount);
			AssertEquals(0.1m, excise.C1_Rate);
			AssertEquals(RateTypes.Codes.Specific, excise.C1_RateType);
			Assert(excise.C1_Override);

			AssertEquals(4m, duty1.C1_Amount);
			Assert(duty1.C1_Override);
			AssertEquals(5m, duty2.C1_Amount);
			Assert(duty2.C1_Override);
			AssertEquals(6m, duty3.C1_Amount);
			Assert(duty3.C1_Override);
		}

		public void TestGetLineDetailsFrom()
		{
			var classLine1 = new ExpectedClassificationLine1ForTesting();
			classLine1.B3LineNumber = 1;
			classLine1.PartNumberDescriptions = new ZString[] { "Description1", "Description2" };
			classLine1.AuthorityNumber = "1234";
			classLine1.ClassificationNumber = "4905910000";
			classLine1.TariffCode = "99AA";
			classLine1.ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;
			classLine1.ValueForCurrency = 1000m;
			var classLine2_1 = new ExpectedClassificationLine2ForTesting();
			classLine2_1.ClassificationLineQuantity = 1;
			classLine2_1.UnitOfMeasureCode = UnitOfWeightList.Codes.Kilogram;
			var classLine2_2 = new ExpectedClassificationLine2ForTesting();
			classLine2_2.ClassificationLineQuantity = 2;
			classLine2_2.UnitOfMeasureCode = UnitOfWeightList.Codes.Kiloton;
			var classLine2_3 = new ExpectedClassificationLine2ForTesting();
			classLine2_3.ClassificationLineQuantity = 3;
			classLine1.ClassificationLines = new[] { classLine2_1, classLine2_2, classLine2_3 };

			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceLine.CA_IsAccountForLine = true;
			InvoiceLine.CopyB3SubHeaderLineToInvoiceLine(classLine1);
			AssertEquals("1", InvoiceLine.CA_OriginalLineNo);
			AssertEquals("Description1", InvoiceLine.JI_Description);
			AssertEquals("1234", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("4905910000", InvoiceLine.JI_Tariff);
			AssertEquals("99AA", InvoiceLine.CA_99TariffCode);
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsComputedValue, InvoiceLine.CA_ValueForDutyCode);
			AssertEquals(1000m, InvoiceLine.CA_CVforCurrConv);
			AssertEquals(UnitOfWeightList.Codes.Kilogram, InvoiceLine.JI_CustomsUnitQty);
			AssertEquals(1m, InvoiceLine.JI_CustomsQuantity);
			AssertEquals(UnitOfWeightList.Codes.Kiloton, InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(2m, InvoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(UnitOfWeightList.Codes.Kilogram, InvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(3m, InvoiceLine.JI_CustomsThirdQuantity);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			InvoiceLine.CA_IsAccountForLine = true;
			InvoiceLine.CopyB3SubHeaderLineToInvoiceLine(classLine1);
			AssertEquals("1", InvoiceLine.CA_OriginalLineNo);
			AssertEquals("Description1", InvoiceLine.JI_Description);
			AssertEquals("1234", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("4905910000", InvoiceLine.JI_Tariff);
			AssertEquals("99AA", InvoiceLine.CA_99TariffCode);
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsComputedValue, InvoiceLine.CA_ValueForDutyCode);
			AssertEquals(1000m, InvoiceLine.CA_CVforCurrConv);
			AssertEquals(UnitOfWeightList.Codes.Kilogram, InvoiceLine.JI_CustomsUnitQty);
			AssertEquals(1m, InvoiceLine.JI_CustomsQuantity);
			AssertEquals(UnitOfWeightList.Codes.Kiloton, InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(2m, InvoiceLine.JI_CustomsSecondQuantity);
			AssertEquals(UnitOfWeightList.Codes.Kilogram, InvoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals(3m, InvoiceLine.JI_CustomsThirdQuantity);
		}

		public void TestLineDutyTaxEntryFeeItems()
		{
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var parser = new CACTestingDataHelper.CadexMessageProcessor();
			parser.ProcessRA(new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(CusEntryLineBusinessObjectTest.refFilesData))));
			var factory = ((IFactoryProvider)parser).Factory;

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine1, 1, 90m, 200m);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine2, 2, 10m, 20m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			IUltimateDistributee distributee = invoiceLine2;
			var fee = distributee.LineDutyTaxEntryFeeItems;
			AssertEquals("Duty", 1.4m, fee["TDT"]);
			AssertEquals("Other Duty", 20m, fee["OTH"]);
			AssertEquals("Tax", 0m, fee["EXC"]);

			IUltimateDistributee distributee1 = invoiceLine1;
			fee = distributee1.LineDutyTaxEntryFeeItems;
			AssertEquals("Duty", 12.6m, fee["TDT"]);
			AssertEquals("Other Duty", 200m, fee["OTH"]);
			AssertEquals("Tax", 0m, fee["EXC"]);

			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine3, 2, 10m, 20m);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			IUltimateDistributee distributee3 = invoiceLine3;
			fee = distributee3.LineDutyTaxEntryFeeItems;
			AssertEquals("Duty", 1.4m, fee["TDT"]);
			AssertEquals("Other Duty", 20m, fee["OTH"]);
			AssertEquals("Tax", 0m, fee["EXC"]);
		}

		public void TestRulingDescription()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var cusRuling1 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling1.ZZX_RulingType = "T";
			cusRuling1.ZZX_RulingNumber = "123456";
			cusRuling1.ZZX_Description = "D1";
			cusRuling1.ZZX_OA_AppliesTo = importer.MainAddress.PK;

			var cusRuling2 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling2.ZZX_RulingType = "T";
			cusRuling2.ZZX_RulingNumber = "123456";
			cusRuling2.ZZX_Description = "D2";
			cusRuling2.ZZX_OA_AppliesTo = importerOfRecord.MainAddress.PK;

			var cusRuling3 = Factory.New<ZZRefCusRulingCombined>();
			cusRuling3.ZZX_RulingType = "T";
			cusRuling3.ZZX_RulingNumber = "654321";
			cusRuling3.ZZX_Description = "D3";

			Factory.Save();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_AuthorityNumber = "123456";
			AssertEquals("D1", invoiceLine1.RulingDescription);

			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_AuthorityNumber = "123456";
			AssertEquals("D2", invoiceLine2.RulingDescription);

			invoiceLine2.CA_AuthorityNumber = "654321";
			AssertEquals("D3", invoiceLine2.RulingDescription);
		}

		public void TestSupplierName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "XYZ Supplier";
			var supplierMainAddress = supplier.Addresses.MainAddress;
			supplierMainAddress.Address1 = "Address1";
			supplierMainAddress.Address2 = "Address2";
			supplierMainAddress.City = "AB";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			IUltimateDistributee distributee = invoiceLine;

			invoice[JobComInvoiceHeaderSchema.JZ_OH_Supplier.Name] = supplier.PK;
			AssertEquals("SupplierName", "XYZ SUPPLIER ADDRESS1 ADDRESS2 AB", distributee.SupplierName);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("SupplierName", ZString.Empty, distributee.SupplierName);

			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("SupplierName", "XYZ SUPPLIER ADDRESS1 ADDRESS2 AB", distributee.SupplierName);

			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_CompanyName = "Other Supplier";
			invoice.SupplierDocumentaryAddress.Address1 = "Other Address1";
			invoice.SupplierDocumentaryAddress.Address2 = "Other Address2";
			invoice.SupplierDocumentaryAddress.City = "TT";
			AssertEquals("SupplierName", "OTHER SUPPLIER OTHER ADDRESS1 OTHER ADDRESS2 TT", distributee.SupplierName);
		}

		public void TestProperties()
		{
			AssertEquals("IsDataLoadingModule", Declaration.IsDataLoadingModule, InvoiceLine.IsDataLoadingModule);
		}

		public void TestIsGSTDirectPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				declaration.ImporterAddInfo.ZO_IsGSTDirectPayment = true;
				var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				Assert("GST Direct is set on Importer", invoiceLine.IsGSTDirectPayment);
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;
				Assert("GST Direct is set when on Buyer", invoiceLine.IsGSTDirectPayment);
				var buyer = Factory.New<OrgHeader>();
				buyer.FillWithValidTestData();
				var buyerAddInfo = OrgImpAddInfo.Get(buyer);
				invoiceLine.InvoiceHeader.JZ_OH_Buyer = buyer.PK;
				Assert("GST Direct is NOT set no Buyer", !invoiceLine.IsGSTDirectPayment);
				buyerAddInfo.ZO_IsGSTDirectPayment = true;
				Assert("GST Direct is set on Buyer", invoiceLine.IsGSTDirectPayment);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				declaration.ImporterAddInfo.ZO_CADIsBrokerToPay = false;
				var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				Assert("GST Direct is set on Importer", invoiceLine.IsGSTDirectPayment);
				declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				invoiceLine.InvoiceHeader.JZ_OH_Buyer = importer.PK;
				Assert("GST Direct is set when on Buyer", invoiceLine.IsGSTDirectPayment);
				var buyer = Factory.New<OrgHeader>();
				buyer.FillWithValidTestData();
				var buyerAddInfo = OrgImpAddInfo.Get(buyer);
				buyerAddInfo.ZO_CADIsBrokerToPay = true;
				invoiceLine.InvoiceHeader.JZ_OH_Buyer = buyer.PK;
				Assert("GST Direct is NOT set no Buyer", !invoiceLine.IsGSTDirectPayment);
				buyerAddInfo.ZO_CADIsBrokerToPay = false;
				Assert("GST Direct is set on Buyer", invoiceLine.IsGSTDirectPayment);
			}
		}

		public void TestUnitsAreUpperCased()
		{
			InvoiceLine.JI_InvoiceUQ = "kg";
			InvoiceLine.JI_CustomsUnitQty = "no";
			InvoiceLine.JI_CustomsSecondUnitQty = "l";
			InvoiceLine.JI_CustomsThirdUnitQty = "t";

			AssertEquals("Upper cased", "KG", InvoiceLine.JI_InvoiceUQ);
			AssertEquals("Upper cased", "NO", InvoiceLine.JI_CustomsUnitQty);
			AssertEquals("Upper cased", "L", InvoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("Upper cased", "T", InvoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestDutyAndTaxProperties()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dutieOrTax = InvoiceLine.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dutieOrTax.C1_ExemptCode = SIMACodes.Codes.C31;
			dutieOrTax.C1_Amount = 1m;
			dutieOrTax = InvoiceLine.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			dutieOrTax.C1_Amount = 2m;
			dutieOrTax = InvoiceLine.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			dutieOrTax.C1_Amount = 3m;
			dutieOrTax = InvoiceLine.DutiesAndTaxes.AddNew();
			dutieOrTax.C1_Override = true;
			dutieOrTax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			dutieOrTax.C1_Amount = 4m;
			CombineAssertions(() =>
			{
				AssertEquals("SIMA Duty", 1m, InvoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("Duty", 2m, InvoiceLine.JI_Calc_DutyAmount);
				AssertEquals("Normal Duty", 2m, InvoiceLine.JI_Calc_NormalDutyAmount);
				AssertEquals("Excise Tax", 3m, InvoiceLine.JI_Calc_ExciseTaxesAmount);
				AssertEquals("GST", 4m, InvoiceLine.JI_Calc_GSTVATAmount);

				InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("SIMA Duty", 0m, InvoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("Duty", 0m, InvoiceLine.JI_Calc_DutyAmount);
				AssertEquals("Normal Duty", 0m, InvoiceLine.JI_Calc_NormalDutyAmount);
				AssertEquals("Excise Tax", 0m, InvoiceLine.JI_Calc_ExciseTaxesAmount);
				AssertEquals("GST", 0m, InvoiceLine.JI_Calc_GSTAmount);

				InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				AssertEquals("SIMA Duty", 1m, InvoiceLine.JI_Calc_SIMADutyAmount);
				AssertEquals("Duty", 2m, InvoiceLine.JI_Calc_DutyAmount);
				AssertEquals("Normal Duty", 2m, InvoiceLine.JI_Calc_NormalDutyAmount);
				AssertEquals("Excise Tax", 3m, InvoiceLine.JI_Calc_ExciseTaxesAmount);
				AssertEquals("GST", 4m, InvoiceLine.JI_Calc_GSTAmount);
			});
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertEquals("Import JI_CustomsQuantityInfo.ReadOnly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = UnitOfMeasureListForDLM.Codes.Dozen;
			AssertEquals("Import JI_CustomsQuantityInfo.ReadOnly", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export JI_CustomsQuantityInfo.ReadOnly", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertEquals("Export JI_CustomsQuantityInfo.ReadOnly", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestJI_CustomsUnitQtyInfoReadOnly()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1111111111", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, CustomsTariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, CustomsTariffDescription);
			universalHelper.CreateTariffUOM(tariff1, "CU1", CustomsTariffUnits);

			var cacExportTariff = Factory.New<CACExportTariff>();
			cacExportTariff.CE_Code = "22222222";
			cacExportTariff.CE_Description = ExportTariffDescription;
			cacExportTariff.CE_Unit = ZString.Empty;
			Factory.Save();

			UseCustomsTariffList();
			InvoiceLine.JI_Tariff = ZString.Empty;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = CustomsTariffCode;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = ExportTariffCode;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			InvoiceLine.JI_Tariff = "1111111111";
			Assert(!InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			UseExportTarifList();
			InvoiceLine.JI_Tariff = ZString.Empty;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			InvoiceLine.JI_Tariff = CustomsTariffCode;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			InvoiceLine.JI_Tariff = ExportTariffCode;
			Assert(InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
			InvoiceLine.JI_Tariff = "22222222";
			Assert(!InvoiceLine.JI_CustomsUnitQtyInfo.ReadOnly);
		}

		public void TestValidation()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(ImportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals(typeof(JobComInvoiceLineValidation), InvoiceLine.Validation.GetType());

			Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals(typeof(ImportJobComInvoiceLineValidation), InvoiceLine.Validation.GetType());
		}

		public void TestJI_TariffPropertyInfo()
		{
			UseExportTarifList();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("TariffType", TariffType.Export, InvoiceLine.JI_FormattedTariffCodeTariffInfo.TariffType);
			AssertEquals("DateForDutyRate", InvoiceLine.EffectiveDateForDutyRate, InvoiceLine.JI_FormattedTariffCodeTariffInfo.DateForDutyRate);

			UseCustomsTariffList();
			AssertEquals("TariffType", TariffType.Import, InvoiceLine.JI_FormattedTariffCodeTariffInfo.TariffType);
			AssertEquals("DateForDutyRate", InvoiceLine.EffectiveDateForDutyRate, InvoiceLine.JI_FormattedTariffCodeTariffInfo.DateForDutyRate);
		}

		public void TestJI_FormattedTariffChanged()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var tax = InvoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			tax.C1_Override = false;

			tax = InvoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax.C1_Override = true;

			AssertEquals("DutiesAndTaxes.Count", 2, InvoiceLine.DutiesAndTaxes.Count);
			InvoiceLine.JI_FormattedTariff = "1234567890";
			AssertEquals("Not overriden duties should be removed", 1, InvoiceLine.DutiesAndTaxes.Count);
			AssertEquals("Overriden GST tax should stay", tax, InvoiceLine.DutiesAndTaxes[0]);
		}

		public void TestImportPGADataFromClassificationIntoInvoiceLine()
		{
			var caClassification = Factory.New<CusCAClassification>();
			caClassification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			caClassification.CCA_RN_NKSource = "US";
			caClassification.CCA_StateOfSource = "AL";

			var classification = Factory.New<CusClassification>();
			caClassification.CCA_ParentID = classification.PK;
			caClassification.CCA_ParentTableCode = classification.TablePrefix;
			classification.CC_TariffNum = "P";
			classification.CCA_CFIAIndicator = YesNoList.Codes.Yes;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_InvoiceQuantity = 1m;
			line.CA_RN_NKSource = "US";
			line.CA_StateOfSource = "TX";

			line.JI_CC = classification.PK;

			AssertEquals("Y", line.CA_CFIAInd);
			AssertEquals("US", line.CA_RN_NKSource);
			AssertEquals("AL", line.CA_StateOfSource);
		}

		#region Countries / States / Provinces

		public void TestCountriesStatesAndProvinces()
		{
			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStatesOrCanada(InvoiceLine.JI_StateOrRegionOfOriginInfo, InvoiceLine.JI_CountryOfOriginInfo, JobMessageTypeList.Codes.Import);
			AssertProvinceNotReadOnlyOrCleardForAnyCountry(InvoiceLine.JI_StateOrRegionOfOriginInfo, InvoiceLine.JI_CountryOfOriginInfo, JobMessageTypeList.Codes.Export);

			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_USStateOfExportInfo, InvoiceLine.CA_RN_NKExportInfo, JobMessageTypeList.Codes.Import);
			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_USStateOfExportInfo, InvoiceLine.CA_RN_NKExportInfo, JobMessageTypeList.Codes.LowValueShipments);
			AssertStateIsAvailableIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_USStateOfExportInfo, InvoiceLine.CA_RN_NKExportInfo, JobMessageTypeList.Codes.Export);

			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_CFIAUSStateOfOriginInfo, InvoiceLine.CA_RN_NKCFIAOriginInfo, JobMessageTypeList.Codes.Import);
			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_USStateOfExportInfo, InvoiceLine.CA_RN_NKExportInfo, JobMessageTypeList.Codes.LowValueShipments);
			AssertStateIsAvailableIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_CFIAUSStateOfOriginInfo, InvoiceLine.CA_RN_NKCFIAOriginInfo, JobMessageTypeList.Codes.Export);

			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_CFIAStateOfSourceInfo, InvoiceLine.CA_CFIACountryOfSourceInfo, JobMessageTypeList.Codes.Import);
			AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_CFIAStateOfSourceInfo, InvoiceLine.CA_CFIACountryOfSourceInfo, JobMessageTypeList.Codes.LowValueShipments);
			AssertStateIsAvailableIfCountrySetToOtherThanUnitedStates(InvoiceLine.CA_CFIAStateOfSourceInfo, InvoiceLine.CA_CFIACountryOfSourceInfo, JobMessageTypeList.Codes.Export);
		}

		void AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStates(ZPropertyInfo stateInfo, ZPropertyInfo countryInfo, string messageType)
		{
			Declaration.JE_MessageType = messageType;
			stateInfo.Value = (ZString)USStatesList.Codes.NewYork;
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(stateInfo.Name, USStatesList.Codes.NewYork, stateInfo.Value);
			AssertEquals(stateInfo.Name + " ReadOnly", false, stateInfo.ReadOnly);
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.Canada;
			AssertEquals(stateInfo.Name, string.Empty, stateInfo.Value);
			AssertEquals(stateInfo.Name + " ReadOnly", true, stateInfo.ReadOnly);
		}

		void AssertStateIsEmptyAndReadOnlyIfCountrySetToOtherThanUnitedStatesOrCanada(ZPropertyInfo stateInfo, ZPropertyInfo countryInfo, string messageType)
		{
			Declaration.JE_MessageType = messageType;
			stateInfo.Value = (ZString)USStatesList.Codes.NewYork;
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.UnitedStates;

			AssertEquals(stateInfo.Name, USStatesList.Codes.NewYork, stateInfo.Value);
			AssertEquals(stateInfo.Name + " ReadOnly", false, stateInfo.ReadOnly);

			countryInfo.Value = (ZString)Core.Constants.CountryCodes.Canada;

			AssertEquals(stateInfo.Name, USStatesList.Codes.NewYork, stateInfo.Value);
			AssertEquals(stateInfo.Name, false, stateInfo.ReadOnly);

			countryInfo.Value = (ZString)Core.Constants.CountryCodes.China;

			AssertEquals(stateInfo.Name, string.Empty, stateInfo.Value);
			AssertEquals(stateInfo.Name + " ReadOnly", true, stateInfo.ReadOnly);
		}

		void AssertStateIsAvailableIfCountrySetToOtherThanUnitedStates(ZPropertyInfo stateInfo, ZPropertyInfo countryInfo, string messageType)
		{
			Declaration.JE_MessageType = messageType;
			stateInfo.Value = (ZString)USStatesList.Codes.NewYork;
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.Canada;
			AssertEquals(stateInfo.Name, USStatesList.Codes.NewYork, stateInfo.Value);
			AssertEquals(stateInfo.Name + " ReadOnly", false, stateInfo.ReadOnly);
		}

		void AssertProvinceNotReadOnlyOrCleardForAnyCountry(ZPropertyInfo stateInfo, ZPropertyInfo countryInfo, string messageType)
		{
			Declaration.JE_MessageType = messageType;
			stateInfo.Value = (ZString)CanadianProvinceList.Codes.YukonTerritory;
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.Canada;
			AssertEquals(stateInfo.Name, CanadianProvinceList.Codes.YukonTerritory, stateInfo.Value);
			Assert(stateInfo.Name + " NOT ReadOnly", !stateInfo.ReadOnly);
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(stateInfo.Name, CanadianProvinceList.Codes.YukonTerritory, stateInfo.Value);
			Assert(stateInfo.Name + " NOT ReadOnly", !stateInfo.ReadOnly);
		}

		public void TestEffectiveProvinceOfOrigin()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.JZ_RW_NKOriginState = CanadianProvinceList.Codes.BritishColumbia;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(CanadianProvinceList.Codes.BritishColumbia, invoiceLine.EffectiveProvinceOfOrigin);

			invoice.JZ_RW_NKOriginState = CanadianProvinceList.Codes.Nunavut;
			AssertEquals(CanadianProvinceList.Codes.Nunavut, invoiceLine.EffectiveProvinceOfOrigin);

			invoiceLine.JI_StateOrRegionOfOrigin = CanadianProvinceList.Codes.PrinceEdwardIsland;
			invoice.JZ_RW_NKOriginState = CanadianProvinceList.Codes.Ontario;
			AssertEquals(CanadianProvinceList.Codes.PrinceEdwardIsland, invoiceLine.EffectiveProvinceOfOrigin);
		}

		public void TestEffectiveValueForDutyCode()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsComputedValue;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsComputedValue, invoiceLine.EffectiveValueForDutyCode);

			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsDeductiveValue;
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsDeductiveValue, invoiceLine.EffectiveValueForDutyCode);

			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods;
			invoice.CA_ValueForDutyCode = ValueForDutyCodes.Codes.RelatedFirmsPaidPayableWithAdjustments;
			AssertEquals(ValueForDutyCodes.Codes.RelatedFirmsIdenticalGoods, invoiceLine.EffectiveValueForDutyCode);
		}

		public void TestEffectiveCasualImportCommodity()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(string.Empty, invoiceLine.CA_CasualImportCommodity);

			invoiceLine.CA_CasualImportCommodity = "Cigarettes";
			AssertEquals("Cigarettes", invoiceLine.CA_CasualImportCommodity);

			invoice.CA_CasualImportCommodity = "Cigars";
			AssertEquals("Cigarettes", invoiceLine.CA_CasualImportCommodity);

			invoiceLine.CA_CasualImportCommodity = string.Empty;
			AssertEquals("Cigars", invoiceLine.CA_CasualImportCommodity);
		}

		public void TestEffectiveCasualImportDestinationProvince()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			org.MainAddress.OA_State = CanadianProvinceList.Codes.Manitoba;

			var address = org.Addresses.AddNew();
			address.OA_Address1 = "SOMEWHERE";
			address.OA_State = CanadianProvinceList.Codes.Ontario;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SOMEWHERE ELSE";
			address2.OA_State = ZString.Empty;
			address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			Factory.Save();

			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			invoice.CA_IsCasualImport = true;
			invoice.CA_CasualImportDestinationProvince = CanadianProvinceList.Codes.Quebec;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(CanadianProvinceList.Codes.Quebec, invoiceLine.CA_CasualImportDestinationProvince);

			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			AssertEquals(CanadianProvinceList.Codes.Ontario, invoiceLine.CA_CasualImportDestinationProvince);

			invoiceLine.CA_CasualImportDestinationProvince = ZString.Empty;
			invoiceLine.JI_OA_ConsigneeAddress = address2.PK;
			AssertEquals(CanadianProvinceList.Codes.Manitoba, invoiceLine.CA_CasualImportDestinationProvince);
		}

		public void TestDefaultCA_CasualDestProvince()
		{
			InvoiceHeader.CA_IsCasualImport = false;

			var headerConsignee = Factory.New<OrgHeader>();
			headerConsignee.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			headerConsignee.MainAddress.OA_City = "VANCOUVER";
			headerConsignee.MainAddress.OA_State = "BC";

			InvoiceHeader.FinalConsigneeAddress.E2_OA_Address = headerConsignee.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			consignee.MainAddress.OA_City = "TORONTO";
			consignee.MainAddress.OA_State = "ON";

			var line = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew();
			line.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;

			Assert("CA_IsCasualImport initially false", !line.CA_IsCasualImport);
			Assert("CA_CasualImportDestinationProvince should be empty", line.CA_CasualImportDestinationProvince.IsEmpty);

			line.CA_IsCasualImport = true;
			AssertEquals("CA_CasualImportDestinationProvince should be defaulted from consignee", "ON", line.CA_CasualImportDestinationProvince);

			InvoiceHeader.CA_CasualImportDestinationProvince = "BC";
			line.CA_IsCasualImport = false;
			line.JI_OA_ConsigneeAddress = ZGuid.Empty;
			line.CA_CasualImportDestinationProvince = "MB";

			line.CA_IsCasualImport = true;
			AssertEquals("CA_CasualImportDestinationProvince should show header value", "BC", line.CA_CasualImportDestinationProvince);
		}

		public void TestEffectiveIsCasualImport()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.IsEffectiveCasualImport);

			invoiceLine.CA_IsCasualImport = true;
			AssertEquals(true, invoiceLine.IsEffectiveCasualImport);

			invoice.CA_IsCasualImport = true;
			AssertEquals(true, invoiceLine.IsEffectiveCasualImport);

			invoiceLine.CA_IsCasualImport = false;
			AssertEquals(true, invoiceLine.IsEffectiveCasualImport);
		}

		public void TestEffectiveCasualImportClearanceProvince()
		{
			var port1 = "0301";
			var port2 = "0900";
			var province1 = "QC";
			var province2 = "NL";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, port1, port1, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, province1);

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, port2, port2, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, province2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_CustomsOffice = port1;
			AssertEquals("EffectiveCasualImportClearanceProvince", province1, invoiceLine.EffectiveCasualImportClearanceProvince);
			invoice.CA_PortOfClearance = port2;
			AssertEquals("EffectiveCasualImportClearanceProvince", province2, invoiceLine.EffectiveCasualImportClearanceProvince);
		}

		public void TestB3EntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "Inv1";
			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.CA_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			line1.JI_Tariff = "0101100025";
			line1.JI_LinePrice = 1m;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0101100025";
			line2.JI_LinePrice = 2m;
			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0101100925";
			line3.JI_LinePrice = 3m;
			AssertEquals("PreCondition : Invoice Line Count", 3, declaration.FilteredInvoiceLines.Count);
			header.JZ_InvoiceAmount = 30m;
			header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 5, 5);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("3 ACROSS Merged Lines", 3, declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease).MergedLines.Count);
			AssertEquals("2 B3 Merged Lines", 2, declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines.Count);
			AssertEquals("Line 1 B3 Entry Line", declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].PK, line1.B3EntryLine.PK);
			AssertEquals("Line 2 B3 Entry Line", declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[0].PK, line2.B3EntryLine.PK);
			AssertEquals("Line 3 B3 Entry Line", declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC).MergedLines[1].PK, line3.B3EntryLine.PK);
			AssertEquals("Line 1 B3 Line Number", "1", line1.JI_B3LineNumber);
			AssertEquals("Line 2 B3 Line Number", "1", line2.JI_B3LineNumber);
			AssertEquals("Line 3 B3 Line Number", "2", line3.JI_B3LineNumber);
		}

		public void TestB3EntryLineForLVX()
		{
			var lvxJob = new BusinessObjectFactory().New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoiceHeader = lvxJob.LVXInvoiceHeader;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_IsCasualImport = true;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 100m;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_CasualImportDestinationProvince = "ON";
			lvxJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			lvxJob.DoMerge();
			lvxJob.Factory.Save();

			AssertNull("No Release EntryHeader on LVX declaration", lvxJob.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease));
			var b3entry = lvxJob.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			AssertNotNull("B3 EntryHeader on LVX declaration", b3entry);

			AssertEquals(invoiceLine.B3EntryLine, b3entry.MergedLines[0]);
		}

		public void TestB3EntryLineForIM2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "Inv1";
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.CA_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			line1.JI_Tariff = "0101100025";
			line1.JI_LinePrice = 1m;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 5, 5);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var b2declaration = declaration.GetNewCopyToB2Declaration();
			b2declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			b2declaration.DoMerge();
			b2declaration.InvoiceLines[0].CusEntryLine.CA_B2LineNo = "1";
			AssertEquals("Line 1 B3 Line Number", "1", b2declaration.InvoiceLines[0].JI_B3LineNumber);
		}

		public void TestCA_IIDRegion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_IIDRegion = "America";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(invoice.CA_IIDRegion, invoiceLine.CA_IIDRegion);

			invoice.CA_IIDRegion = "Australia";
			AssertEquals(invoice.CA_IIDRegion, invoiceLine.CA_IIDRegion);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(invoice.CA_IIDRegion, invoiceLine2.CA_IIDRegion);
		}

		#endregion

		public void TestADDDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_ADD_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_ADD_Amount);
			AssertEquals(true, line.CA_ADD_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_ADD_Code);
			AssertEquals(true, line.CA_ADD_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_ADD_Description);
			AssertEquals(true, line.CA_ADD_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_ADD_ExemptCode);
			AssertEquals(false, line.CA_ADD_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_ADD_Override);
			AssertEquals(true, line.CA_ADD_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_ADD_Rate);
			AssertEquals(true, line.CA_ADD_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_ADD_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_ADD_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_ADD_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_ADD_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_ADD_Amount);
			AssertEquals(false, line.CA_ADD_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_ADD_Code);
			AssertEquals(false, line.CA_ADD_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_ADD_ExemptCode);
			AssertEquals(false, line.CA_ADD_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_ADD_Rate);
			AssertEquals(false, line.CA_ADD_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_ADD_RateType);
		}

		public void TestCPTDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_CPT_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CPT_Amount);
			AssertEquals(true, line.CA_CPT_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CPT_Code);
			AssertEquals(true, line.CA_CPT_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CPT_Description);
			AssertEquals(true, line.CA_CPT_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CPT_ExemptCode);
			AssertEquals(false, line.CA_CPT_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_CPT_Override);
			AssertEquals(true, line.CA_CPT_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CPT_Rate);
			AssertEquals(true, line.CA_CPT_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CPT_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_CPT_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_CPT_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CPT);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_CPT_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CPT_Amount);
			AssertEquals(true, line.CA_CPT_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_CPT_Code);
			AssertEquals(false, line.CA_CPT_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_CPT_ExemptCode);
			AssertEquals(false, line.CA_CPT_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CPT_Rate);
			AssertEquals(false, line.CA_CPT_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_CPT_RateType);
		}

		public void TestCTADutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_CTA_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CTA_Amount);
			AssertEquals(true, line.CA_CTA_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CTA_Code);
			AssertEquals(true, line.CA_CTA_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CTA_Description);
			AssertEquals(true, line.CA_CTA_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CTA_ExemptCode);
			AssertEquals(false, line.CA_CTA_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_CTA_Override);
			AssertEquals(true, line.CA_CTA_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CTA_Rate);
			AssertEquals(true, line.CA_CTA_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CTA_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_CTA_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_CTA_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CTA);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_CTA_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CTA_Amount);
			AssertEquals(true, line.CA_CTA_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_CTA_Code);
			AssertEquals(true, line.CA_CTA_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_CTA_ExemptCode);
			AssertEquals(false, line.CA_CTA_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CTA_Rate);
			AssertEquals(false, line.CA_CTA_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_CTA_RateType);
		}

		public void TestDTYDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_EXCDTY_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_EXCDTY_Amount);
			AssertEquals(true, line.CA_EXCDTY_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXCDTY_Code);
			AssertEquals(true, line.CA_EXCDTY_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXCDTY_Description);
			AssertEquals(true, line.CA_EXCDTY_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXCDTY_ExemptCode);
			AssertEquals(false, line.CA_EXCDTY_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_EXCDTY_Override);
			AssertEquals(true, line.CA_EXCDTY_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_EXCDTY_Rate);
			AssertEquals(true, line.CA_EXCDTY_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXCDTY_RateType);
			AssertEquals(true, line.CA_CLSDTY_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CLSDTY_Amount);
			AssertEquals(true, line.CA_CLSDTY_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CLSDTY_Code);
			AssertEquals(true, line.CA_CLSDTY_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CLSDTY_Description);
			AssertEquals(true, line.CA_CLSDTY_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CLSDTY_ExemptCode);
			AssertEquals(false, line.CA_CLSDTY_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_CLSDTY_Override);
			AssertEquals(true, line.CA_CLSDTY_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CLSDTY_Rate);
			AssertEquals(true, line.CA_CLSDTY_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CLSDTY_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_EXCDTY_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_EXCDTY_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_CLSDTY_Override = true;
			AssertEquals(2, line.DutiesAndTaxes.Count);
			line.CA_CLSDTY_Override = false;
			AssertEquals(2, line.DutiesAndTaxes.Count);

			var exciseDTY = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && x.C1_DutyType == DutyAndTaxManager.CombinedDuty.Excise);
			exciseDTY.C1_Override = true;
			exciseDTY.C1_ExemptCode = "10";
			exciseDTY.C1_Amount = 10;
			exciseDTY.C1_Code = "ABC";
			exciseDTY.C1_Rate = 10;
			exciseDTY.C1_RateType = "T";
			AssertEquals(false, line.CA_EXCDTY_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_EXCDTY_Amount);
			AssertEquals(false, line.CA_EXCDTY_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_EXCDTY_Code);
			AssertEquals(true, line.CA_EXCDTY_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_EXCDTY_ExemptCode);
			AssertEquals(false, line.CA_EXCDTY_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_EXCDTY_Rate);
			AssertEquals(false, line.CA_EXCDTY_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_EXCDTY_RateType);

			var classificationDTY = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && x.C1_DutyType == DutyAndTaxManager.CombinedDuty.Classification);
			classificationDTY.C1_Override = true;
			classificationDTY.C1_ExemptCode = "30";
			classificationDTY.C1_Amount = 100;
			classificationDTY.C1_Code = "XYZ";
			classificationDTY.C1_Rate = 20;
			classificationDTY.C1_RateType = "S";
			AssertEquals(false, line.CA_CLSDTY_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)100, line.CA_CLSDTY_Amount);
			AssertEquals(false, line.CA_CLSDTY_CodeInfo.ReadOnly);
			AssertEquals("XYZ", line.CA_CLSDTY_Code);
			AssertEquals(true, line.CA_CLSDTY_ExemptCodeInfo.ReadOnly);
			AssertEquals("30", line.CA_CLSDTY_ExemptCode);
			AssertEquals(false, line.CA_CLSDTY_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)20, line.CA_CLSDTY_Rate);
			AssertEquals(false, line.CA_CLSDTY_RateTypeInfo.ReadOnly);
			AssertEquals("S", line.CA_CLSDTY_RateType);
		}

		public void TestCVDDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_CVD_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CVD_Amount);
			AssertEquals(true, line.CA_CVD_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CVD_Code);
			AssertEquals(true, line.CA_CVD_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CVD_Description);
			AssertEquals(true, line.CA_CVD_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CVD_ExemptCode);
			AssertEquals(false, line.CA_CVD_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_CVD_Override);
			AssertEquals(true, line.CA_CVD_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_CVD_Rate);
			AssertEquals(true, line.CA_CVD_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_CVD_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_CVD_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_CVD_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_CVD_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CVD_Amount);
			AssertEquals(false, line.CA_CVD_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_CVD_Code);
			AssertEquals(false, line.CA_CVD_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_CVD_ExemptCode);
			AssertEquals(false, line.CA_CVD_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_CVD_Rate);
			AssertEquals(false, line.CA_CVD_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_CVD_RateType);
		}

		public void TestEXSDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_EXS_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_EXS_Amount);
			AssertEquals(true, line.CA_EXS_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXS_Code);
			AssertEquals(true, line.CA_EXS_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXS_Description);
			AssertEquals(true, line.CA_EXS_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXS_ExemptCode);
			AssertEquals(false, line.CA_EXS_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_EXS_Override);
			AssertEquals(true, line.CA_EXS_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_EXS_Rate);
			AssertEquals(true, line.CA_EXS_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_EXS_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_EXS_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_EXS_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ExciseTax);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_EXS_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_EXS_Amount);
			AssertEquals(false, line.CA_EXS_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_EXS_Code);
			AssertEquals(false, line.CA_EXS_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_EXS_ExemptCode);
			AssertEquals(false, line.CA_EXS_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_EXS_Rate);
			AssertEquals(false, line.CA_EXS_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_EXS_RateType);
		}

		public void TestGSTDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_GST_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_GST_Amount);
			AssertEquals(true, line.CA_GST_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_GST_Code);
			AssertEquals(true, line.CA_GST_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_GST_Description);
			AssertEquals(true, line.CA_GST_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_GST_ExemptCode);
			AssertEquals(false, line.CA_GST_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_GST_Override);
			AssertEquals(true, line.CA_GST_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_GST_Rate);
			AssertEquals(true, line.CA_GST_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_GST_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_GST_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_GST_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_GST_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_GST_Amount);
			AssertEquals(false, line.CA_GST_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_GST_Code);
			AssertEquals(false, line.CA_GST_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_GST_ExemptCode);
			AssertEquals(false, line.CA_GST_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_GST_Rate);
			AssertEquals(false, line.CA_GST_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_GST_RateType);
		}

		public void TestSAFDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_SAF_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_SAF_Amount);
			AssertEquals(true, line.CA_SAF_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SAF_Code);
			AssertEquals(true, line.CA_SAF_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SAF_Description);
			AssertEquals(true, line.CA_SAF_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SAF_ExemptCode);
			AssertEquals(false, line.CA_SAF_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_SAF_Override);
			AssertEquals(true, line.CA_SAF_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_SAF_Rate);
			AssertEquals(true, line.CA_SAF_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SAF_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_SAF_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_SAF_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SAF);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_SAF_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_SAF_Amount);
			AssertEquals(false, line.CA_SAF_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_SAF_Code);
			AssertEquals(false, line.CA_SAF_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_SAF_ExemptCode);
			AssertEquals(false, line.CA_SAF_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_SAF_Rate);
			AssertEquals(false, line.CA_SAF_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_SAF_RateType);
		}

		public void TestSURDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			AssertEquals(true, line.CA_SUR_AmountInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_SUR_Amount);
			AssertEquals(true, line.CA_SUR_CodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SUR_Code);
			AssertEquals(true, line.CA_SUR_DescriptionInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SUR_Description);
			AssertEquals(true, line.CA_SUR_ExemptCodeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SUR_ExemptCode);
			AssertEquals(false, line.CA_SUR_OverrideInfo.ReadOnly);
			AssertEquals(ZBool.False, line.CA_SUR_Override);
			AssertEquals(true, line.CA_SUR_RateInfo.ReadOnly);
			AssertEquals(ZDecimal.Zero, line.CA_SUR_Rate);
			AssertEquals(true, line.CA_SUR_RateTypeInfo.ReadOnly);
			AssertEquals(ZString.Empty, line.CA_SUR_RateType);

			AssertEquals(0, line.DutiesAndTaxes.Count);
			line.CA_SUR_Override = true;
			AssertEquals(1, line.DutiesAndTaxes.Count);
			line.CA_SUR_Override = false;
			AssertEquals(1, line.DutiesAndTaxes.Count);

			var dutyAndTax = line.DutiesAndTaxes.Single(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			dutyAndTax.C1_Override = true;
			dutyAndTax.C1_ExemptCode = "10";
			dutyAndTax.C1_Amount = 10;
			dutyAndTax.C1_Code = "ABC";
			dutyAndTax.C1_Rate = 10;
			dutyAndTax.C1_RateType = "T";
			AssertEquals(false, line.CA_SUR_AmountInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_SUR_Amount);
			AssertEquals(false, line.CA_SUR_CodeInfo.ReadOnly);
			AssertEquals("ABC", line.CA_SUR_Code);
			AssertEquals(false, line.CA_SUR_ExemptCodeInfo.ReadOnly);
			AssertEquals("10", line.CA_SUR_ExemptCode);
			AssertEquals(false, line.CA_SUR_RateInfo.ReadOnly);
			AssertEquals((ZDecimal)10, line.CA_SUR_Rate);
			AssertEquals(false, line.CA_SUR_RateTypeInfo.ReadOnly);
			AssertEquals("T", line.CA_SUR_RateType);
		}

		public void TestCA_ADJCode()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.CA_ADJCode = AmountTypes.Codes.Percent;
			AssertEquals("Shouldn't reset CA_ADJValue if CA_ADJCode = '%'", 10m, invoiceLine.CA_ADJValue);

			invoiceLine.CA_ADJCode = AmountTypes.Codes.Dollar;
			AssertEquals("Shouldn't reset CA_ADJValue if CA_ADJCode = '$'", 10m, invoiceLine.CA_ADJValue);

			invoiceLine.CA_ADJCode = ZString.Empty;
			AssertEquals("Should reset CA_ADJValue to zero if CA_ADJCode is empty", 0m, invoiceLine.CA_ADJValue);
		}

		public void TestCA_CalculationMethod_DutyDeferral()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoiceLine.CA_AuthorityNumber = "XXXXX";
			invoiceLine.CA_PageNumber = 3;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 2, 2, 600, 100, 90, 80, Core.Constants.Weight.Kilograms, 100m);

			//No lines added for Duty Deferral Remission if dialog result is "No"
			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DutyDeferral;
			AssertEquals("InvoiceLines Count", 1, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("ContinueWithActionCaption", "Duty Deferral (60/40) Remission Line Creation", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to add a Duty Deferral (60/40) Remission line.\r\n" +
				"You will be able to override Price and Description for Duty Deferral (60/40) Remission calculations.\r\n" +
	"Do you want to continue?", messageInitiator.ContinueWithActionMessage);

			//DD line is added for Duty Deferral Remission
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DutyDeferral;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.DutyDeferral, invoiceLine.CA_CalculationMethod);
			Assert("RepairsRemission IsRemissionLine", invoiceLine.IsRemissionLine);

			var ddLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines[1];
			AssertNotEquals("PK", ddLine.PK, invoiceLine.PK);
			AssertEquals("JI_lineNo, repair child inserted immediatly after parent", (ZShort)2, ddLine.JI_LineNo);
			AssertEquals("JI_Tariff", ddLine.JI_Tariff, invoiceLine.JI_Tariff);
			AssertEquals("CA_PageNumber", ddLine.CA_PageNumber, invoiceLine.CA_PageNumber);
			AssertEquals("JI_Description", ddLine.JI_Description, "Duty Deferral (60/40) Remission -");
			AssertEquals("CA_99TariffCode", ddLine.CA_99TariffCode, invoiceLine.CA_99TariffCode);
			AssertEquals("CA_TreatmentCode", ddLine.CA_TreatmentCode, invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", ddLine.CA_ValueForDutyCode, "29");
			AssertEquals("CA_ADJCode", ddLine.CA_ADJCode, invoiceLine.CA_ADJCode);
			AssertEquals("CA_ADJValue", ddLine.CA_ADJValue, invoiceLine.CA_ADJValue);
			AssertEquals("JI_CustomsQuantity", ddLine.JI_CustomsQuantity, invoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", ddLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondQuantity", ddLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("JI_CustomsSecondUnitQty", ddLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdQuantity", ddLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdUnitQty", ddLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("JI_ParentID", ddLine.JI_ParentID, invoiceLine.PK);
			AssertEquals("CA_CalculationMethod", ddLine.CA_CalculationMethod, invoiceLine.CA_CalculationMethod);
			AssertEquals("JI_PartNo", ZString.Empty, ddLine.JI_PartNo);
			AssertEquals("CA_AuthorityNumber", ddLine.CA_AuthorityNumber, "");
			AssertEquals("JI_CountryOfOrigin", ddLine.JI_CountryOfOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", ddLine.JI_StateOrRegionOfOrigin, invoiceLine.JI_StateOrRegionOfOrigin);
			Assert("DutyDefferalLine IsRemissionLine", ddLine.IsRemissionLine);
			Assert(ddLine.IsDutyDeferralLine);

			AssertEquals("The parent should have 40% of the original value", 240m, invoiceLine.JI_LinePrice);
			AssertEquals("The child should have 60% of the original value", 360m, ddLine.JI_LinePrice);
		}

		public void TestCA_CalculationMethod_WarrantyRepair()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_PageNumber = 3;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 2, 2, 600, 100, 90, 80, Core.Constants.Weight.Kilograms, 100m);

			var gstDuty = invoiceLine.DutiesAndTaxes.AddNew();
			gstDuty.C1_Override = true;
			gstDuty.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			gstDuty.C1_Amount = 200m;

			//No lines added for Warranty Repairs Remission if dialog result is "No"
			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			AssertEquals("InvoiceLines Count", 1, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("ContinueWithActionCaption", "Warranty Repairs Remission Line Creation", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to add a Warranty Repairs Remission line.\r\nYou will be able to override Price and Description for Warranty Repairs Remission calculations.\r\nDo you want to continue?", messageInitiator.ContinueWithActionMessage);

			//Warranty Repair line is added for Warranty Repairs Remission
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.WarrantyRepairsRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.WarrantyRepairsRemission, invoiceLine.CA_CalculationMethod);
			Assert("IsRemissionLine", invoiceLine.IsRemissionLine);

			var warrantyLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines[1];
			AssertNotEquals("PK", warrantyLine.PK, invoiceLine.PK);
			AssertEquals("JI_lineNo, repair child inserted immediatly after parent", (ZShort)2, warrantyLine.JI_LineNo);
			AssertEquals("JI_Tariff", warrantyLine.JI_Tariff, invoiceLine.JI_Tariff);
			AssertEquals("CA_PageNumber", warrantyLine.CA_PageNumber, invoiceLine.CA_PageNumber);
			AssertEquals("JI_Description", warrantyLine.JI_Description, "Warranty Repairs Remission -");
			AssertEquals("CA_99TariffCode", warrantyLine.CA_99TariffCode, invoiceLine.CA_99TariffCode);
			AssertEquals("CA_TreatmentCode", warrantyLine.CA_TreatmentCode, invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue, warrantyLine.CA_ValueForDutyCode);
			AssertEquals("CA_ADJCode", warrantyLine.CA_ADJCode, invoiceLine.CA_ADJCode);
			AssertEquals("CA_ADJValue", warrantyLine.CA_ADJValue, invoiceLine.CA_ADJValue);
			AssertEquals("JI_CustomsQuantity", warrantyLine.JI_CustomsQuantity, 0m);
			AssertEquals("JI_CustomsUnitQty", warrantyLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondQuantity", warrantyLine.JI_CustomsSecondQuantity, 0m);
			AssertEquals("JI_CustomsSecondUnitQty", warrantyLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdQuantity", warrantyLine.JI_CustomsThirdQuantity, 0m);
			AssertEquals("JI_CustomsThirdUnitQty", warrantyLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("JI_ParentID", warrantyLine.JI_ParentID, invoiceLine.PK);
			AssertEquals("CA_CalculationMethod", warrantyLine.CA_CalculationMethod, invoiceLine.CA_CalculationMethod);
			AssertEquals("JI_PartNo", ZString.Empty, warrantyLine.JI_PartNo);
			AssertEquals("CA_AuthorityNumber", warrantyLine.CA_AuthorityNumber, "");
			AssertEquals("JI_CountryOfOrigin", warrantyLine.JI_CountryOfOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", warrantyLine.JI_StateOrRegionOfOrigin, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals(false, warrantyLine.JI_CountryOfOriginInfo.ReadOnly);
			AssertEquals(false, warrantyLine.JI_StateOrRegionOfOriginInfo.ReadOnly);
			Assert("IsRemissionLine", warrantyLine.IsRemissionLine);
			Assert(warrantyLine.IsWarrantyRepairLine);
			AssertEquals(false, ((ICanDelete)warrantyLine).CanDelete);
			AssertEquals(false, ((ICanDelete)invoiceLine).CanDelete);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			AssertEquals("JI_JZ", warrantyLine.JI_JZ, invoiceLine.JI_JZ);

			invoiceLine.JI_JZ = invoiceHeader.PK;

			//Repair line is not deleted if dialog result is "No"
			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.WarrantyRepairsRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("ContinueWithActionCaption", "Warranty Repairs Remission Line Deletion", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to delete the Warranty Repairs Remission line.\r\nDo you want to continue?", messageInitiator.ContinueWithActionMessage);

			//Repair line is deleted
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertEquals("InvoiceLines Count", 1, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.OneSixtiethRemission, invoiceLine.CA_CalculationMethod);
			Assert("Child line is deleted (not parent)", warrantyLine.IsDeleted);
			Assert("OneSixtiethRemission IsRemissionLine", invoiceLine.IsRemissionLine);
			Assert("Should be able to delete non-repair line", ((ICanDelete)invoiceLine).CanDelete);
		}

		public void TestCA_CalculationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine.JI_PartNo = "123";
			invoiceLine.CA_ValueForDutyCode = ValueForDutyCodes.Codes.UnrelatedFirmsComputedValue;
			invoiceLine.CA_PageNumber = 3;
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine, 2, 2, 600, 100, 90, 80, Core.Constants.Weight.Kilograms);
			AssertEquals("CA_CalculationMethod default", CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("CA_CalculationMethod.ReadOnly", false, invoiceLine.CA_CalculationMethodInfo.ReadOnly);
			AssertEquals("NoRemission IsRemissionLine", false, invoiceLine.IsRemissionLine);

			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.JI_StateOrRegionOfOrigin = USStatesList.Codes.Arkansas;
			invoiceLine2.JI_PartNo = "123";
			JobComInvoiceLineTestHelper.FillInvoiceLine(invoiceLine2, 2, 2, 50, 100, 90, 80, Core.Constants.Weight.Kilograms);
			AssertEquals("line 2 line number", (ZShort)2, invoiceLine2.JI_LineNo);

			//No lines added for RegularRemission
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);

			//No lines added for RepairsRemission if dialog result is "No"
			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("ContinueWithActionCaption", "Repairs Remission Line Creation", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to add a Repairs Remission line.\r\nYou will be able to override Price and Description for Repairs Remission calculations.\r\nDo you want to continue?", messageInitiator.ContinueWithActionMessage);

			//Repair line is added for RepairsRemission
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			AssertEquals("InvoiceLines Count", 3, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RepairsRemission, invoiceLine.CA_CalculationMethod);
			Assert("RepairsRemission IsRemissionLine", invoiceLine.IsRemissionLine);

			var repairLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines[2];
			AssertNotEquals("PK", repairLine.PK, invoiceLine.PK);
			AssertNotEquals("PK", repairLine.PK, invoiceLine2.PK);
			AssertEquals("JI_lineNo, repair child inserted immediatly after parent", (ZShort)2, repairLine.JI_LineNo);
			AssertEquals("line 2 line number incremented", (ZShort)3, invoiceLine2.JI_LineNo);
			AssertEquals("JI_Tariff", repairLine.JI_Tariff, invoiceLine.JI_Tariff);
			AssertEquals("CA_PageNumber", repairLine.CA_PageNumber, invoiceLine.CA_PageNumber);
			AssertEquals("JI_Description", repairLine.JI_Description, "Repairs Remission -");
			AssertEquals("CA_99TariffCode", repairLine.CA_99TariffCode, invoiceLine.CA_99TariffCode);
			AssertEquals("CA_TreatmentCode", repairLine.CA_TreatmentCode, invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", repairLine.CA_ValueForDutyCode, "29");
			AssertEquals("CA_ADJCode", repairLine.CA_ADJCode, invoiceLine.CA_ADJCode);
			AssertEquals("CA_ADJValue", repairLine.CA_ADJValue, invoiceLine.CA_ADJValue);
			AssertEquals("JI_CustomsQuantity", repairLine.JI_CustomsQuantity, invoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsUnitQty", repairLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("JI_CustomsSecondQuantity", repairLine.JI_CustomsSecondQuantity, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("JI_CustomsSecondUnitQty", repairLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("JI_CustomsThirdQuantity", repairLine.JI_CustomsThirdQuantity, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("JI_CustomsThirdUnitQty", repairLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("JI_ParentID", repairLine.JI_ParentID, invoiceLine.PK);
			AssertEquals("CA_CalculationMethod", repairLine.CA_CalculationMethod, invoiceLine.CA_CalculationMethod);
			AssertEquals("JI_PartNo", ZString.Empty, repairLine.JI_PartNo);
			AssertEquals("CA_AuthorityNumber", repairLine.CA_AuthorityNumber, "");
			AssertEquals("JI_CountryOfOrigin", repairLine.JI_CountryOfOrigin, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", repairLine.JI_StateOrRegionOfOrigin, invoiceLine.JI_StateOrRegionOfOrigin);
			Assert("RepairLine IsRemissionLine", repairLine.IsRemissionLine);

			//Invoice Lines Total Amount
			repairLine.JI_LinePrice = 350;
			AssertEquals("InvoiceLineTotal should include repair line amount only", 400m, invoiceHeader.JZ_Calc_LinesEntered);

			invoiceLine2.JI_LineNo = 1;
			AssertEquals("repair parent renumbered", (ZShort)2, invoiceLine.JI_LineNo);
			AssertEquals("repair child renumbered", (ZShort)3, repairLine.JI_LineNo);
			AssertEquals("JI_ParentID syncronized", repairLine.JI_ParentID, invoiceLine.PK);

			Assert("Should not be able to delete repair parent", !((ICanDelete)invoiceLine).CanDelete);
			Assert("Should not be able to delete repair child", !((ICanDelete)repairLine).CanDelete);
			Assert("Should be able to delete non-repair line", ((ICanDelete)invoiceLine2).CanDelete);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			AssertEquals("JI_JZ", repairLine.JI_JZ, invoiceLine.JI_JZ);

			invoiceLine.JI_JZ = invoiceHeader.PK;

			//Repair line is not deleted if dialog result is "No"
			messageInitiator.AnswerToContinueWithAction = false;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertEquals("InvoiceLines Count", 3, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.RepairsRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals("ContinueWithActionCaption", "Repairs Remission Line Deletion", messageInitiator.ContinueWithActionCaption);
			AssertEquals("ContinueWithActionMessage", "The System is about to delete the Repairs Remission line.\r\nDo you want to continue?", messageInitiator.ContinueWithActionMessage);

			//Repair line is deleted
			messageInitiator.AnswerToContinueWithAction = true;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertEquals("InvoiceLines Count", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.OneSixtiethRemission, invoiceLine.CA_CalculationMethod);
			Assert("Child line is deleted (not parent)", repairLine.IsDeleted);
			Assert("OneSixtiethRemission IsRemissionLine", invoiceLine.IsRemissionLine);
			Assert("Should be able to delete non-repair line", ((ICanDelete)invoiceLine).CanDelete);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Assert("CA_CalculationMethod.ReadOnly", invoiceLine.CA_CalculationMethodInfo.ReadOnly);
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			Assert("RegularRemission IsRemissionLine", invoiceLine.IsRemissionLine);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			Assert("OneOneTwentiethRemission IsRemissionLine", invoiceLine.IsRemissionLine);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
			Assert("DeliveredDutyPaid NOT IsRemissionLine", !invoiceLine.IsRemissionLine);

			//Redefault when invoice line is linked to a different header that has different incoterm to previous header's incoterm
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header1 = dec.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INV1";
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			var header2 = dec.Invoices.AddNew();
			header2.JZ_InvoiceNumber = "INV2";
			header2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var line1 = (JobComInvoiceLine)header1.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = "INV1";
			AssertEquals("InCoTerm : DDP", Core.Constants.IncoTerms.DeliveredDutyPaid, line1.InvoiceHeader.JZ_IncoTerm);
			AssertEquals("CA_CalculationMethod : D", CalculationMethods.Codes.DeliveredDutyPaid, line1.CA_CalculationMethod);

			var line2 = (JobComInvoiceLine)header1.InvoiceLines.AddNew();
			line2.JI_Calc_Invoice = "INV2";
			AssertEquals("InCoTerm : FOB", Core.Constants.IncoTerms.FreeOnBoard, line2.InvoiceHeader.JZ_IncoTerm);
			AssertEquals("CA_CalculationMethod : N", CalculationMethods.Codes.NoRemission, line2.CA_CalculationMethod);
		}

		[ExpectNoExceptions]
		public void TestCA_CalculationMethodWhenInvoiceHeaderIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("EffectiveCasualImportClearanceProvince is empty", ZString.Empty, invoiceLine.EffectiveCasualImportClearanceProvince);
			Assert("CA_IsCasualImport.ReadOnly", invoiceLine.CA_IsCasualImportInfo.ReadOnly);
			Assert("CA_AuthorityNumber.ReadOnly", invoiceLine.CA_AuthorityNumberInfo.ReadOnly);
			Assert("CA_CalculationMethod.ReadOnly", invoiceLine.CA_CalculationMethodInfo.ReadOnly);
		}

		public void TestCA_CFIACountryOfSource()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_CFIACountryOfSource = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CA_CFIAStateOfSource = USStatesList.Codes.Arkansas;
			invoiceLine.CA_CFIACountryOfSource = Core.Constants.CountryCodes.Albania;
			Assert("Empty", invoiceLine.CA_CFIAStateOfSource.IsEmpty);
		}

		public void TestShouldPropertiesBeReadOnly()
		{
			var matrixOfExpectedReadOnlyValues =
				new Dictionary<string, bool[]>
				{
					//	Property	\	Line Type							  |Normal|Parent|Repair|
					{ JobComInvoiceLine.Schema.JI_LineNo, new[] { false, true, true } },
					{ JobComInvoiceLine.Schema.JI_LinePrice, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.JI_Description, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_ADJCode, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_ADJValue, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_CVforCurrConv, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_CustomsValue, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_CVforCurrConvOvr, new[] { false, false, false } },
					{ JobComInvoiceLine.Schema.CA_CustomsValueOvr, new[] { false, false, false } },

					//All other JobComInvoiceLine properites than above. Few are tested.
					{ JobComInvoiceLine.Schema.CA_99TariffCode, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsQuantity, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsUnitQty, new[] { true, true, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.CA_ValueForDutyCode, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.CA_AuthorityNumber, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.CA_TRSNumber, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.CA_CalculationMethod, new[] { false, false, true } },
					{ JobComInvoiceLine.Schema.CA_TreatmentCode, new[] { false, false, true } },
				};

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoiceHeader = declaration.Invoices.AddNew();
			var normal = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var parent = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			parent.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			var repair = invoiceHeader.InvoiceLines[2];

			foreach (JobComInvoiceLine line in invoiceHeader.InvoiceLines)
			{
				line.CA_ADJCode = AmountTypes.Codes.Percent;
				line.CA_CustomsValueOvr = true;
				line.CA_CVforCurrConvOvr = true;
			}

			foreach (var line in matrixOfExpectedReadOnlyValues)
			{
				AssertEquals("Normal Line: " + line.Key, line.Value[0], normal.FindPropertyInfo(line.Key).ReadOnly);
				AssertEquals("Repair Line Parent: " + line.Key, line.Value[1], parent.FindPropertyInfo(line.Key).ReadOnly);
				AssertEquals("Repair Line: " + line.Key, line.Value[2], repair.FindPropertyInfo(line.Key).ReadOnly);
			}
		}

		public void TestCA_CVforCurrConvOvr()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceLine.CA_CVforCurrConvOvr = false;
			Assert("CA_CVforCurrConvOvr always true for B2", InvoiceLine.CA_CVforCurrConvOvr);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			InvoiceLine.CA_CVforCurrConvOvr = false;
			Assert("CA_CVforCurrConvOvr always true for B3X", InvoiceLine.CA_CVforCurrConvOvr);
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			var caCompany = Factory.New<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Canada)).RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = caBranch.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the right type type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestPartNumberDescriptions_MaximumLength()
		{
			var classLine1 = new ExpectedClassificationLine1ForTesting();
			var cloneDescription = string.Concat(Enumerable.Repeat("X", InvoiceLine.JI_DescriptionInfo.MaxLength));

			classLine1.B3LineNumber = 1;
			classLine1.PartNumberDescriptions = new ZString[] { cloneDescription + "XXX" };
			classLine1.AuthorityNumber = "1234";
			classLine1.ClassificationNumber = "4905910000";
			classLine1.TariffCode = "99AA";

			var classLine2_1 = new ExpectedClassificationLine2ForTesting();
			classLine2_1.ClassificationLineQuantity = 1;
			classLine2_1.UnitOfMeasureCode = CanadianUnitOfWeightList.Codes.Kilogram;
			classLine1.ClassificationLines = new[] { classLine2_1 };

			Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			InvoiceLine.CA_IsAccountForLine = true;

			InvoiceLine.CopyB3SubHeaderLineToInvoiceLine(classLine1);
			AssertEquals(cloneDescription, InvoiceLine.JI_Description);
		}

		[ExpectNoExceptions]
		public void TestJZ_RW_NKOriginStateMaxLength()
		{
			var invoice = Factory.New<JobComInvoiceLine>();
			invoice.JI_StateOrRegionOfOrigin = "MEX";
			AssertEquals(2, invoice.JI_StateOrRegionOfOriginInfo.MaxLength);
			AssertEquals("ME", invoice.JI_StateOrRegionOfOrigin);
		}

		public void TestPartWithEffectiveImporterAndSupplierRelationshipForLVS()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.JE_OH_Importer = importer1.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			invoiceHeader.JZ_OH_Buyer = importer2.PK;

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
			invoiceLine.JI_PartNo = "AAA";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.FillWithValidTestData();
			part1.OP_PartNum = "AAA";
			part1.RelatedOrganisations.AddOwner(importer1);
			part1.RelatedOrganisations.AddSupplier(supplier1);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.FillWithValidTestData();
			part2.OP_PartNum = "AAA";
			part2.RelatedOrganisations.AddOwner(importer2);
			part2.RelatedOrganisations.AddSupplier(supplier2);
			Factory.Save();

			AssertEquals("Should pick up part with invoice header Importer/Supplier relationship", part2.PK, invoiceLine.Part.PK);

			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;

			AssertEquals("Should pick up part with declaration Importer/Supplier relationship", part1.PK, invoiceLine.Part.PK);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestMoneyFOB_CIF_LinePriceIsValid()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceLine.JI_LinePrice = 1000m;

			Declaration.CA_RX_DeclaredCurr = ZGuid.Empty;
			AssertEquals("JI_FOB is invalid : No Currency", false, InvoiceLine.JI_FOB.IsValid);
			AssertEquals("JI_CIF is invalid : No Currency", false, InvoiceLine.JI_CIF.IsValid);
			AssertEquals("JI_LinePriceMoney is invalid : No Currency", false, InvoiceLine.JI_LinePriceMoney.IsValid);

			var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, JobDeclaration.LocalCurrencyConstantCode);
			Declaration.CA_RX_DeclaredCurr = localCurrency != null ? localCurrency.PK : ZGuid.Empty;
			AssertEquals("JI_FOB is valid", true, InvoiceLine.JI_FOB.IsValid);
			AssertEquals("JI_CIF is valid", true, InvoiceLine.JI_CIF.IsValid);
			AssertEquals("JI_LinePriceMoney is valid", true, InvoiceLine.JI_LinePriceMoney.IsValid);
		}

		public void TestCusCodeData()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Permits", typeof(InvoiceLineExportPermitCollection), invoiceLine.Permits.GetType());
			AssertEquals("SITTCertificationNumbers", typeof(SITTCertificationNumberCollection), invoiceLine.SITTCertificationNumbers.GetType());
			AssertEquals("CFIARegistrationNumbers", typeof(CFIARegistrationNumberCollection), invoiceLine.CFIARegistrationNumbers.GetType());
		}

		[TestDate(2025, 1, 15)]
		public void TestCustomsUQandTariffDescription()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, CustomsTariffCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(1), CustomsTariffDescription);
			universalHelper.CreateTariffUOM(tariff1, "CU1", CustomsTariffUnits);

			JobComInvoiceLine CreateInvoiceLineForTesting(ZString messageType, ZDateTime effectiveDate)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.JE_EntryAuthorisationDate = effectiveDate;
				var invHeader = declaration.Invoices.AddNew();
				var invoiceLine = invHeader.JobComInvoiceLines.AddNew();
				return invoiceLine;
			}

			var invoiceLine = CreateInvoiceLineForTesting(JobMessageTypeList.Codes.Import, ZDateTime.Today);

			UseCustomsTariffList();
			invoiceLine.JI_Tariff = CustomsTariffCode;
			AssertEquals(CustomsTariffUnits, invoiceLine.CustomsUQ);
			AssertEquals(CustomsTariffDescription, invoiceLine.TariffDescription);

			invoiceLine = CreateInvoiceLineForTesting(JobMessageTypeList.Codes.Export, ZDateTime.Today.AddDays(1));
			invoiceLine.JI_Tariff = ExportTariffCode;
			AssertEquals(ExportTariffUnits, invoiceLine.CustomsUQ);
			AssertEquals(ExportTariffDescription, invoiceLine.TariffDescription);

			UseExportTarifList();
			Factory.ClearCachedValue<TariffViewCollection>("RefCusTariffCollection_CA_HSN_17-Jan-25 00:00:00");
			invoiceLine = CreateInvoiceLineForTesting(JobMessageTypeList.Codes.Import, ZDateTime.Today.AddDays(2));
			invoiceLine.JI_Tariff = CustomsTariffCode;
			AssertEquals(ZString.Empty, invoiceLine.CustomsUQ);
			AssertEquals(CustomsTariffDescription, invoiceLine.TariffDescription);

			invoiceLine = CreateInvoiceLineForTesting(JobMessageTypeList.Codes.Export, ZDateTime.Today.AddDays(3));
			invoiceLine.JI_Tariff = ExportTariffCode;
			AssertEquals(ExportTariffUnits, invoiceLine.CustomsUQ);
			AssertEquals(ExportTariffDescription, invoiceLine.TariffDescription);
		}

		public void TestSettingJI_CC()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.Both;
			classification.CC_LookupCode = "CC1";
			InvoiceLine.JI_Tariff = "1234567890";
			InvoiceLine.CA_99TariffCode = "9901";
			InvoiceLine.CA_ValueForDutyCode = "13";
			InvoiceLine.CA_AuthorityNumber = "AUTH";
			InvoiceLine.CA_TRSNumber = "TRS";
			InvoiceLine.CA_TreatmentCode = "11";
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Setting JI_CC should clear JI_Traiff", ZString.Empty, InvoiceLine.JI_Tariff);
			AssertEquals("Setting JI_CC should clear CA_99TariffCode", ZString.Empty, InvoiceLine.CA_99TariffCode);
			AssertEquals("Setting JI_CC should clear CA_ValueForDutyCode", ZString.Empty, InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Setting JI_CC should clear CA_AuthorityNumber", ZString.Empty, InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Setting JI_CC should clear CA_TRSNumber", ZString.Empty, InvoiceLine.CA_TRSNumber);
			AssertEquals("Setting JI_CC should clear CA_TreatmentCode", ZString.Empty, InvoiceLine.CA_TreatmentCode);
			classification.CC_TariffNum = "2300123490";
			classification.CCA_99TariffCode = "9902";
			classification.CCA_ValueForDutyCode = "28";
			classification.CCA_AuthorityNumber = "CCAUTH";
			classification.CCA_TRSNumber = "CCTRS";
			classification.CCA_TreatmentCode = "12";
			InvoiceLine.JI_CC = ZGuid.Empty;
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Setting JI_CC should set JI_Traiff", "2300123490", InvoiceLine.JI_Tariff);
			AssertEquals("Setting JI_CC should set CA_99TariffCode", "9902", InvoiceLine.CA_99TariffCode);
			AssertEquals("Setting JI_CC should set CA_ValueForDutyCode", "28", InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Setting JI_CC should set CA_AuthorityNumber", "CCAUTH", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Setting JI_CC should set CA_TRSNumber", "CCTRS", InvoiceLine.CA_TRSNumber);
			AssertEquals("Setting JI_CC should set CA_TreatmentCode", "12", InvoiceLine.CA_TreatmentCode);
			InvoiceLine.JI_CC = ZGuid.Empty;
			AssertEquals("Clearing JI_CC should not change JI_Traiff", "2300123490", InvoiceLine.JI_Tariff);
			AssertEquals("Clearing JI_CC should not change CA_99TariffCode", "9902", InvoiceLine.CA_99TariffCode);
			AssertEquals("Clearing JI_CC should not change CA_ValueForDutyCode", "28", InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Clearing JI_CC should not change CA_AuthorityNumber", "CCAUTH", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Clearing JI_CC should not change CA_TRSNumber", "CCTRS", InvoiceLine.CA_TRSNumber);
			AssertEquals("Clearing JI_CC should not change CA_TreatmentCode", "12", InvoiceLine.CA_TreatmentCode);

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.CA_99TariffCode = ZString.Empty;
			InvoiceLine.CA_ValueForDutyCode = ZString.Empty;
			InvoiceLine.CA_AuthorityNumber = ZString.Empty;
			InvoiceLine.CA_TRSNumber = ZString.Empty;
			InvoiceLine.CA_TreatmentCode = ZString.Empty;
			InvoiceLine.JI_CC = ZGuid.Empty;
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Setting JI_CC should set JI_Traiff on export dec", "2300123490", InvoiceLine.JI_Tariff);
			AssertEquals("Setting JI_CC should not set CA_99TariffCode on export dec", ZString.Empty, InvoiceLine.CA_99TariffCode);
			AssertEquals("Setting JI_CC should not set CA_ValueForDutyCode on export dec", ZString.Empty, InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Setting JI_CC should not set CA_AuthorityNumber on export dec", ZString.Empty, InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Setting JI_CC should not set CA_TRSNumber on export dec", ZString.Empty, InvoiceLine.CA_TRSNumber);
			AssertEquals("Setting JI_CC should not set CA_TreatmentCode on export dec", ZString.Empty, InvoiceLine.CA_TreatmentCode);
		}

		public void TestUseImportClassification()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Assert(InvoiceLine.UseImportClassification);

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(InvoiceLine.UseImportClassification);
		}

		public void TestSettingJI_CConB2AndB3X()
		{
			AssertSettingJI_CConB2OrB3X(JobMessageTypeList.Codes.B2Adjustments);
			AssertSettingJI_CConB2OrB3X(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertSettingJI_CConB2OrB3X(ZString messageType)
		{
			InvoiceLine.Declaration.JE_MessageType = messageType;
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.Both;
			classification.CC_LookupCode = "CC1";
			InvoiceLine.JI_Tariff = "1234567890";
			InvoiceLine.CA_99TariffCode = "9901";
			InvoiceLine.CA_ValueForDutyCode = "13";
			InvoiceLine.CA_AuthorityNumber = "AUTH";
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Setting JI_CC should clear JI_Traiff", ZString.Empty, InvoiceLine.JI_Tariff);
			AssertEquals("Setting JI_CC should clear CA_99TariffCode", ZString.Empty, InvoiceLine.CA_99TariffCode);
			AssertEquals("Setting JI_CC should clear CA_ValueForDutyCode", ZString.Empty, InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Setting JI_CC should clear CA_AuthorityNumber", ZString.Empty, InvoiceLine.CA_AuthorityNumber);
			classification.CC_TariffNum = "2300123490";
			classification.CCA_99TariffCode = "9902";
			classification.CCA_ValueForDutyCode = "28";
			classification.CCA_AuthorityNumber = "CCAUTH";
			classification.CCA_TRSNumber = "CCTRS";
			classification.CCA_TreatmentCode = "12";
			InvoiceLine.JI_CC = ZGuid.Empty;
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("Setting JI_CC should set JI_Traiff", "2300123490", InvoiceLine.JI_Tariff);
			AssertEquals("Setting JI_CC should set CA_99TariffCode", "9902", InvoiceLine.CA_99TariffCode);
			AssertEquals("Setting JI_CC should set CA_ValueForDutyCode", "28", InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Setting JI_CC should set CA_AuthorityNumber", "CCAUTH", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Setting JI_CC should not set CA_TRSNumber", ZString.Empty, InvoiceLine.CA_TRSNumber);
			AssertEquals("Setting JI_CC should not set CA_TreatmentCode", ZString.Empty, InvoiceLine.CA_TreatmentCode);
			InvoiceLine.JI_CC = ZGuid.Empty;
			AssertEquals("Clearing JI_CC should not change JI_Traiff", "2300123490", InvoiceLine.JI_Tariff);
			AssertEquals("Clearing JI_CC should not change CA_99TariffCode", "9902", InvoiceLine.CA_99TariffCode);
			AssertEquals("Clearing JI_CC should not change CA_ValueForDutyCode", "28", InvoiceLine.CA_ValueForDutyCode);
			AssertEquals("Clearing JI_CC should not change CA_AuthorityNumber", "CCAUTH", InvoiceLine.CA_AuthorityNumber);
			AssertEquals("Clearing JI_CC should not change CA_TRSNumber", ZString.Empty, InvoiceLine.CA_TRSNumber);
			AssertEquals("Clearing JI_CC should not change CA_TreatmentCode", ZString.Empty, InvoiceLine.CA_TreatmentCode);
		}

		public void TestIsContainerLinkMandatory()
		{
			AssertEquals("IsContainerLinkMandatory", false, InvoiceLine.IsContainerLinkMandatory);
		}

		public void TestValidateTotalValueForDutyWhenCustomsValueChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var messageError =
				"Total value for duty of Low Value Shipment should not exceed 1650 CAD, but it is 2000 CAD. If the entered amount is wrong then correct it and then run apportionment from the brokerage menu (if available) or press the Calculate Duty button if using the wizard.";
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			InvoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			InvoiceLine.CA_CustomsValueOvr = true;
			InvoiceLine.CA_CustomsValue = 2000;
			AssertHasError(InvoiceHeader.TotalValueForDutyInfo, messageError);

			InvoiceLine.CA_CustomsValue = 1500;
			AssertNoError(InvoiceHeader.TotalValueForDutyInfo, messageError);
		}

		public void TestDoNotDefaultExportCountryAndStateFromOrigin()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoiceLine = (JobComInvoiceLine)dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(ZString.Empty, invoiceLine.CA_RN_NKExport);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, invoiceLine.CA_RN_NKExport);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_StateOrRegionOfOrigin = "IL";
			AssertEquals(ZString.Empty, invoiceLine.CA_USStateOfExport);
			invoiceLine.JI_StateOrRegionOfOrigin = "AL";
			AssertEquals(ZString.Empty, invoiceLine.CA_USStateOfExport);
			invoiceLine.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			AssertEquals("", invoiceLine.CA_USStateOfExport);
			invoiceLine.JI_StateOrRegionOfOrigin = "IL";
			AssertEquals("", invoiceLine.CA_USStateOfExport);
		}

		public void TestSetAVSStatusIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			Assert("Precondition: AVSEventRequired", !declaration.AVSEventRequired);
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("Precondition: CA_OGDStatus", AVSStatusList.Codes.Blank, invoiceLine.CA_OGDStatus);
			invoiceLine.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.CFIA);
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_RequirementID = "XXX";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_RequirementVer = "XXX";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_AirsCode = "XXX";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_DestinationProvince = "AL";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_EndUse = "XXX";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_MiscID = "XXX";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_RN_NKCFIAOrigin = "US";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CA_CFIAUSStateOfOrigin = "IL";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			var regNumber = invoiceLine.CFIARegistrationNumbers.AddNew();
			regNumber.CY_Code = "14";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			regNumber.CY_Code = "28";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
			declaration.AVSEventRequired = false;
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.CFIARegistrationNumbers.RemoveAndDelete(regNumber);
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
		}

		public void TestSetAVSStatusIfNeededForIID()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "0101210000");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "0101210001");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert(!declaration.AVSEventRequired);
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101210000";

			invoiceLine.CA_CFIAInd = "Y";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);

			declaration.AVSEventRequired = false;
			invoiceLine.CA_CFIAInd = "";
			invoiceLine.CA_OGDStatus = AVSStatusList.Codes.Blank;
			invoiceLine.JI_Tariff = "0101210001";
			AssertEquals("CA_OGDStatus", AVSStatusList.Codes.NotValidated, invoiceLine.CA_OGDStatus);
			Assert("AVSEventRequired", declaration.AVSEventRequired);
		}

		public void TestGetLPCOHolderParty()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestImporterName";

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "TestExporterName";

			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_FullName = "TestSupplierName";

			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_FullName = "TestSupplierName";

			var importerOfRecord = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestImporterOfRecordName";

			var manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_FullName = "TestManufacturerName1";

			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_FullName = "TestManufacturerName2";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier1.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporter.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			invoice.JZ_OH_Supplier = supplier2.PK;

			AssertEquals(importer.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Importer).PK);
			AssertEquals(exporter.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Exporter).PK);
			AssertEquals(supplier2.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Supplier).PK);
			AssertEquals(importerOfRecord.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord).PK);
			AssertEquals(manufacturer1.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Manufacturer).PK);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			invoice.JZ_OH_Supplier = ZGuid.Empty;

			AssertEquals(supplier1.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Supplier).PK);
			AssertEquals(manufacturer2.PK, invoiceLine.GetLPCOHolderParty(LPCOHolderPartyTypeCodes.Codes.Manufacturer).PK);
		}

		public void TestRulingConfigurationsApportionmentDirty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var rulingConfig = invoiceLine.RulingConfigurations.AddNew();
			rulingConfig.ZZY_Category = "DTY";
			rulingConfig.ZZY_Type = "Rate";
			rulingConfig.ZZY_Rate = 15m;

			declaration.ApportionmentDirty = false;

			rulingConfig.ZZY_Rate = 15.69m;
			Assert(declaration.ApportionmentDirty);
		}

		public void TestDefaultJI_OA_ConsigneeAddress()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "TestConsigneeName";
			var dlvAddress = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			invoiceLine.JI_OA_ConsigneeAddress_ZAddress.OrgPK = consignee.PK;
			AssertEquals(dlvAddress.PK, invoiceLine.JI_OA_ConsigneeAddress);
		}

		#region Test Customs qty/unit defaulting

		public void TestDefaultingCustomsUnitsFromInvoiceUnits()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "20200601", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff, "CU1", "NMB");
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			InvoiceLine.JI_Tariff = "20200601";
			AssertEquals("Defaulting from Tariff", "NMB", InvoiceLine.JI_CustomsUnitQty);

			InvoiceLine.JI_CustomsUnitQty = ZString.Empty;
			InvoiceLine.JI_InvoiceUQ = "XXX";
			AssertEquals("Invalid Customs Unit", "", InvoiceLine.JI_CustomsUnitQty);

			InvoiceLine.JI_Tariff = "11111";
			InvoiceLine.JI_InvoiceUQ = "EA";
			AssertEquals("Customs Unit not set because no HS code yet", "", InvoiceLine.JI_CustomsUnitQty);
			InvoiceLine.JI_Tariff = "1111";
			AssertEquals("Customs Unit not set by HS set", "", InvoiceLine.JI_CustomsUnitQty);
		}

		#endregion

		#region TestPivot

		public override void TestPivot()
		{
			Declaration.JE_MessageType = DeclarationExportMessageType;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<OrgHeader>());

			var importClassification = Factory.New<BaseCusClassification>();
			importClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClassification.PK;

			var exportClassification = Factory.New<BaseCusClassification>();
			exportClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			exportPivot = part.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			exportPivot.CI_CC = exportClassification.PK;

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(exportPivot.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			AssertEquals(importPivot.PK, InvoiceLine.Pivot.PK);

			var ownRelation = part.RelatedOrganisations.AddOwner(Factory.New<OrgHeader>());
			var ownRelation2 = part.RelatedOrganisations.AddOwner(Factory.New<OrgHeader>());
			Declaration.JE_OH_Importer = ownRelation.OU_OH;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Part);
			AssertEquals(importPivot.PK, InvoiceLine.Pivot.PK);

			importPivot.CI_OH = ownRelation.OU_OH;
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CI_TariffNum = "1234567890";

			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = ownRelation.OU_OH;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Part);
			AssertEquals(importPivot.PK, InvoiceLine.Pivot.PK);

			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = ownRelation2.OU_OH;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Part);
			AssertNull(InvoiceLine.Pivot);

			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			InvoiceLine.JI_PartNo = ZString.Empty;
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Part);
			AssertEquals(importPivot.PK, InvoiceLine.Pivot.PK);
		}

		[TestDate(2000, 1, 1)]
		public void TestGetPivot()
		{
			Declaration.JE_MessageType = DeclarationExportMessageType;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(Factory.New<OrgHeader>());

			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_OH = supRelation.OU_OH;
			importPivot.CI_TariffNum = "1234567890";
			importPivot.CI_DateStart = new ZDate(1999, 12, 1);
			importPivot.CI_DateEnd = new ZDate(2000, 2, 1);

			exportPivot = part.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			exportPivot.CI_OH = supRelation.OU_OH;
			exportPivot.CI_TariffNum = "1234567890";
			exportPivot.CI_DateStart = new ZDate(1999, 12, 1);
			exportPivot.CI_DateEnd = new ZDate(2000, 2, 1);

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Pivot);
			Declaration.JE_ExportDate = new ZDate(2000, 3, 1);
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNull(InvoiceLine.Pivot);
			Declaration.JE_ExportDate = ZDate.Empty;
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Pivot);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			Declaration.JE_EntryAuthorisationDate = new ZDate(2000, 3, 2);
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNull(InvoiceLine.Pivot);
			Declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Pivot);
			var attrib1 = importPivot.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			var attrib2 = importPivot.Attributes2.AddNew();
			attrib2.BG_AttributeValue1 = "2";
			var attrib3 = importPivot.Attributes3.AddNew();
			attrib3.BG_AttributeValue1 = "3";
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNull(InvoiceLine.Pivot);
			InvoiceLine.JI_PartNo = "";
			InvoiceLine.JI_PartAttrib1 = "1";
			InvoiceLine.JI_PartAttrib2 = "2";
			InvoiceLine.JI_PartAttrib3 = "3";
			InvoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(InvoiceLine.Pivot);
		}

		[TestDate(2000, 1, 1)]
		public void TestEffectiveDateForDutyRateOverride()
		{
			var date2 = new ZDate(2000, 1, 2);
			var date3 = new ZDate(2000, 1, 3);
			var date4 = new ZDate(2000, 1, 4);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			Declaration.CA_EstReleaseDate = ZDate.Empty;
			Declaration.JE_DateOfFirstArrival = ZDate.Empty;
			AssertEquals(ZDate.Today, InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_EntryAuthorisationDate = date2;
			AssertEquals(date2, InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_EntryAuthorisationDate = ZDate.Empty;
			Declaration.CA_EstReleaseDate = date3;
			AssertEquals(date3, InvoiceLine.EffectiveDateForDutyRate);
			Declaration.CA_EstReleaseDate = ZDate.Empty;
			Declaration.JE_DateOfFirstArrival = date4;
			AssertEquals(date4, InvoiceLine.EffectiveDateForDutyRate);

			var date5 = new ZDate(2000, 1, 5);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ExportDate = ZDate.Empty;
			AssertEquals(ZDate.Today, InvoiceLine.EffectiveDateForDutyRate);
			Declaration.JE_ExportDate = date5;
			AssertEquals(date5, InvoiceLine.EffectiveDateForDutyRate);
		}

		[TestDate(2022, 6, 1)]
		public void TestEffectiveDateForDutyRateOverride_LVS()
		{
			var date = new ZDate(2022, 6, 1);
			var endOfJun = new ZDate(2022, 6, 30);
			var date2 = new ZDate(2022, 7, 17);
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvxJob.JE_PeriodMonth = 6;

			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvxJob.LVXInvoiceHeader.AttachToAdditionalDeclaration(lvsJob);

			var lvxLine = lvxJob.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(date, lvxLine.EffectiveDateForDutyRate);
			var lvsLine = lvsJob.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(endOfJun, lvsLine.EffectiveDateForDutyRate);

			lvxJob.JE_EntryAuthorisationDate = date2;

			AssertEquals(date2, lvxLine.EffectiveDateForDutyRate);
			AssertEquals(endOfJun, lvsLine.EffectiveDateForDutyRate);
		}

		public void TestSettingDefaultsFromExportPivot()
		{
			var part = GetPart();
			exportPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			exportPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			exportPivot.CI_TariffNum = "1234567890";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", CanadianProvinceList.Codes.Alberta, invoiceLine.JI_StateOrRegionOfOrigin);
		}

		public void TestDefaultCLP_RefNoWithoutSetter()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();

			var part = GetPart();
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			importPivot.CI_TariffNum = "1234567890";

			// some values for old OGA tabs
			importPivot.CCA_RequirementVersion = "111";
			importPivot.CCA_ImportReasonCode = "01";
			importPivot.CCA_TypeSize = "123";
			importPivot.CCA_TIIN = "222";

			AssertEquals("test assumes that manufacturer address is not empty", false, manufacturer.MainAddress.PK.IsEmpty);
			importPivot.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;

			// we need to check only one PGA example to make sure that PGARequirementCollection.CopyPersistentValuesFrom is called correctly, the rest is the responsibility of PGARequirementCollection
			importPivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			importPivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			var lpco = importPivot.CFIAPGAHeader.LPCOViews.AddNew();
			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			lpco.CLP_RefNo = "REF1";
			importPivot.CCA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			importPivot.CCA_StateOfSource = USStatesList.Codes.Alaska;

			importPivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			((IHasPGARequirements)importPivot).JI_BrandName = "PEUGEOT";
			((IHasPGARequirements)importPivot).JI_Model = "206";

			OrgHeader org = null;
			foreach (OrgPartRelation relation in part.RelatedOrganisations)
			{
				if (relation.Organisation != null && relation.IsOwner)
				{
					org = relation.Organisation;
				}
			}
			var sfc = org.CustomsCodes.AddNew();
			sfc.OK_CodeType = OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense;
			sfc.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			sfc.OK_CustomsRegNo = "REF2";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("test assumes that PGA idicator was not set before the test", ZString.Empty, invoiceLine.CA_CFIAInd);

			// setting part JI_PartNo should trigger copying of PGA requirements from the pivot
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("EC1", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals("REF1", invoiceLine.CFIAPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);
		}

		public void TestPGARequirementsAreCopiedFromPivotForIID()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();

			var part = GetPart();
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			importPivot.CI_TariffNum = "1234567890";

			// some values for old OGA tabs
			importPivot.CCA_RequirementVersion = "111";
			importPivot.CCA_ImportReasonCode = "01";
			importPivot.CCA_TypeSize = "123";
			importPivot.CCA_TIIN = "222";

			AssertEquals("test assumes that manufacturer address is not empty", false, manufacturer.MainAddress.PK.IsEmpty);
			importPivot.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;

			// we need to check only one PGA example to make sure that PGARequirementCollection.CopyPersistentValuesFrom is called correctly, the rest is the responsibility of PGARequirementCollection
			importPivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			importPivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			importPivot.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF1";
			importPivot.CCA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			importPivot.CCA_StateOfSource = USStatesList.Codes.Alaska;

			importPivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			((IHasPGARequirements)importPivot).JI_BrandName = "PEUGEOT";
			((IHasPGARequirements)importPivot).JI_Model = "206";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("test assumes that PGA idicator was not set before the test", ZString.Empty, invoiceLine.CA_CFIAInd);

			// setting part JI_PartNo should trigger copying of PGA requirements from the pivot
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(invoiceLine.Part);
			AssertEquals("JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", CanadianProvinceList.Codes.Alberta, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("JI_BrandName", "PEUGEOT", invoiceLine.JI_BrandName);
			AssertEquals("JI_Model", "206", invoiceLine.JI_Model);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("EC1", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals("REF1", invoiceLine.CFIAPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);

			AssertEquals(Core.Constants.CountryCodes.UnitedStates, invoiceLine.CFIAPGAHeader.RN_NKCountryOfSource);
			AssertEquals(USStatesList.Codes.Alaska, invoiceLine.CFIAPGAHeader.RW_NKSourceState);

			// check that values for old OGA tabs are not copied for IID declaration
			AssertEquals(ZString.Empty, invoiceLine.CA_RequirementVer);
			AssertEquals(ZString.Empty, invoiceLine.CA_ImportReasonCode);
			AssertEquals(ZString.Empty, invoiceLine.CA_TypeSize);
			AssertEquals(ZString.Empty, invoiceLine.CA_TIIN);
		}

		public void TestPGARequirementsAreCopiedFromPivotWithClassificationForIID()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();

			var part = GetPart();
			part.OP_Brand = ZString.Empty;
			part.OP_Model = ZString.Empty;
			importPivot.CCA_OA_Manufacturer = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = ZString.Empty;
			importPivot.CCA_ProvinceOfOrigin = ZString.Empty;
			importPivot.CCA_RN_NKSource = ZString.Empty;
			importPivot.CCA_StateOfSource = ZString.Empty;
			var classification = importPivot.Classification;
			classification.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			classification.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			classification.CCA_RN_NKSource = Core.Constants.CountryCodes.UnitedStates;
			classification.CCA_StateOfSource = USStatesList.Codes.Alaska;
			classification.CCA_BrandName = "PEUGEOT";
			classification.CCA_Model = "206";
			classification.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			classification.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			classification.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF1";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("test assumes that PGA idicator was not set before the test", ZString.Empty, invoiceLine.CA_CFIAInd);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals("JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", CanadianProvinceList.Codes.Alberta, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKSource);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Alaska, invoiceLine.CA_StateOfSource);
			AssertEquals("JI_BrandName", "PEUGEOT", invoiceLine.JI_BrandName);
			AssertEquals("JI_Model", "206", invoiceLine.JI_Model);

			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("EC1", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals("REF1", invoiceLine.CFIAPGAHeader.LPCOViews.OfType<LPCOView>().Single().CLP_RefNo);
		}

		public void TestIHasPGARequirementsTariffInfo()
		{
			AssertEquals(InvoiceLine.JI_TariffInfo, ((IHasPGARequirements)InvoiceLine).TariffInfo);
		}

		public void TestPGARequirementsAreNotCopiedFromPivotForNonIID()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();

			var part = GetPart();
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = CanadianProvinceList.Codes.Alberta;
			importPivot.CI_TariffNum = "1234567890";

			AssertEquals("test assumes that manufacturer address is not empty", false, manufacturer.MainAddress.PK.IsEmpty);
			importPivot.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;

			// we need to check only one PGA example to make sure that PGARequirementCollection.CopyPersistentValuesFrom is called correctly, the rest is the responsibility of PGARequirementCollection
			importPivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			importPivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EC1";
			importPivot.CFIAPGAHeader.LPCOViews.AddNew().CLP_RefNo = "REF1";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("test assumes that PGA idicator was not set before the test", ZString.Empty, invoiceLine.CA_CFIAInd);

			// setting part JI_PartNo should not trigger copying of PGA requirements from the pivot for non-IID declaration
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(invoiceLine.Part);
			AssertEquals("PGA idicator should remain empty after the test", ZString.Empty, invoiceLine.CA_CFIAInd);
		}

		OrgSupplierPart GetPartWithImportPivot()
		{
			var result = GetPart();
			importPivot.CCA_ValueForDutyCode = "13";
			importPivot.CCA_TreatmentCode = "12";
			importPivot.CCA_99TariffCode = "9901";
			importPivot.CCA_AuthorityNumber = "AUTH#";
			importPivot.CCA_TRSNumber = "TRS#";
			importPivot.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			importPivot.CCA_ProvinceOfOrigin = USStatesList.Codes.Illinois;
			importPivot.CCA_RequirementID = "REQID";
			importPivot.CCA_RequirementVersion = "2";
			importPivot.CCA_AirsCode = "AIRSCD";
			importPivot.CCA_DestinationProvince = CanadianProvinceList.Codes.Manitoba;
			importPivot.CCA_EndUse = "111";
			importPivot.CCA_MiscID = "222";
			importPivot.CCA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			importPivot.CCA_CFIAUSStateOfOrigin = USStatesList.Codes.NewYork;
			var cfiaRegNum1 = importPivot.CFIARegistrationNumbers.AddNew();
			cfiaRegNum1.CY_Code = "001";
			cfiaRegNum1.CY_Data = "1234";
			var cfiaRegNum2 = importPivot.CFIARegistrationNumbers.AddNew();
			cfiaRegNum2.CY_Code = "002";
			cfiaRegNum2.CY_Data = "5678";
			importPivot.CCA_ImportReasonCode = "01";
			importPivot.CCA_Model = "MDL";
			importPivot.CCA_ModelNumber = "MDLN111";
			importPivot.CCA_BrandName = "BNAME";
			var sittNum1 = importPivot.SITTCertificationNumbers.AddNew();
			sittNum1.CY_Data = "12345";
			var sittNum2 = importPivot.SITTCertificationNumbers.AddNew();
			sittNum2.CY_Data = "67890";
			importPivot.CCA_TypeSize = "TYPSIZ";
			importPivot.CCA_TIIN = "666";
			importPivot.CCA_CompliantCompletion = true;
			importPivot.CCA_CompliantImportDateIndicator = true;
			return result;
		}

		public void TestUpdatePGADetailsWhenValueIsBlankOnProduct()
		{
			var part = GetPart();
			importPivot.CCA_OA_Manufacturer = manufacturer.MainAddress.PK;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			importPivot.CCA_ProvinceOfOrigin = USStatesList.Codes.Illinois;
			((IHasPGARequirements)importPivot).JI_BrandName = "PEUGEOT";
			((IHasPGARequirements)importPivot).JI_Model = "206";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertEquals("JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Illinois, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("JI_BrandName", "PEUGEOT", invoiceLine.JI_BrandName);
			AssertEquals("JI_Model", "206", invoiceLine.JI_Model);

			importPivot.CCA_OA_Manufacturer = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = ZString.Empty;
			importPivot.CCA_ProvinceOfOrigin = ZString.Empty;
			((IHasPGARequirements)importPivot).JI_BrandName = ZString.Empty;
			((IHasPGARequirements)importPivot).JI_Model = ZString.Empty;
			Factory.Save();
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Illinois, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("JI_BrandName", "PEUGEOT", invoiceLine.JI_BrandName);
			AssertEquals("JI_Model", "206", invoiceLine.JI_Model);
		}

		public void TestSettingDefaultsFromImportPivot()
		{
			var part = GetPartWithImportPivot();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var existingCFIARegNumber = invoiceLine.CFIARegistrationNumbers.AddNew();
			existingCFIARegNumber.CY_Code = "002";
			existingCFIARegNumber.CY_Data = "???";
			var existingSittNumber = invoiceLine.SITTCertificationNumbers.AddNew();
			existingSittNumber.CY_Data = "67890";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			Assert(existingCFIARegNumber.IsDeleted);
			Assert(existingSittNumber.IsDeleted);
			AssertNotNull(invoiceLine.Part);
			AssertEquals("Invoice Units", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("CA_ValueForDutyCode", "13", invoiceLine.CA_ValueForDutyCode);
			AssertEquals("CA_TreatmentCode", "12", invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_99TariffCode", "9901", invoiceLine.CA_99TariffCode);
			AssertEquals("CA_AuthorityNumber", "AUTH#", invoiceLine.CA_AuthorityNumber);
			AssertEquals("CA_TRSNumber", "TRS#", invoiceLine.CA_TRSNumber);
			AssertEquals("JI_OA_ManufacturerAddress", "MANUFACTURER ADDRESS", invoiceLine.ManufacturerAddress.Address1);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Illinois, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RequirementID", "REQID", invoiceLine.CA_RequirementID);
			AssertEquals("CA_RequirementVer", "2", invoiceLine.CA_RequirementVer);
			AssertEquals("CA_AirsCode", "AIRSCD", invoiceLine.CA_AirsCode);
			AssertEquals("CA_DestinationProvince", CanadianProvinceList.Codes.Manitoba, invoiceLine.CA_DestinationProvince);
			AssertEquals("CA_EndUse", "111", invoiceLine.CA_EndUse);
			AssertEquals("CA_MiscID", "222", invoiceLine.CA_MiscID);
			AssertEquals("CA_RN_NKCFIAOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKCFIAOrigin);
			AssertEquals("CA_CFIAUSStateOfOrigin", USStatesList.Codes.NewYork, invoiceLine.CA_CFIAUSStateOfOrigin);
			AssertEquals("2 CFIA numbers", 2, invoiceLine.CFIARegistrationNumbers.Count);
			AssertEquals("1st CFIA number CY_Code", "001", invoiceLine.CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("1st CFIA number CY_Date", "1234", invoiceLine.CFIARegistrationNumbers[0].CY_Data);
			AssertEquals("2nd CFIA number CY_Code", "002", invoiceLine.CFIARegistrationNumbers[1].CY_Code);
			AssertEquals("2nd CFIA number CY_Date", "5678", invoiceLine.CFIARegistrationNumbers[1].CY_Data);
			AssertEquals("CA_ImportReasonCode", "01", invoiceLine.CA_ImportReasonCode);
			AssertEquals("CA_Model", "MDL", invoiceLine.CA_Model);
			AssertEquals("CA_ModelNumber", "MDLN111", invoiceLine.CA_ModelNumber);
			AssertEquals("JI_BrandName", "BNAME", invoiceLine.JI_BrandName);
			AssertEquals("2 SITT numbers", 2, invoiceLine.SITTCertificationNumbers.Count);
			AssertEquals("1st SITT number", "12345", invoiceLine.SITTCertificationNumbers[0].CY_Data);
			AssertEquals("2nd SITT number", "67890", invoiceLine.SITTCertificationNumbers[1].CY_Data);
			AssertEquals("CA_TypeSize", "TYPSIZ", invoiceLine.CA_TypeSize);
			AssertEquals("CA_TIIN", "666", invoiceLine.CA_TIIN);
			Assert("CA_CompliantCompletion", invoiceLine.CA_CompliantCompletion);
			Assert("CA_CompliantImportDate", invoiceLine.CA_CompliantImportDate);

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = USStatesList.Codes.Illinois;
			invoiceLine.JI_CountryOfOrigin = "";
			invoiceLine.JI_StateOrRegionOfOrigin = "";
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("JI_CountryOfOrigin", "US", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", "IL", invoiceLine.JI_StateOrRegionOfOrigin);

			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.JZ_RW_NKOriginState = USStatesList.Codes.Alabama;
			invoiceLine.JI_CountryOfOrigin = "";
			invoiceLine.JI_StateOrRegionOfOrigin = "";
			invoiceLine.JI_PartNo = "";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Illinois, invoiceLine.JI_StateOrRegionOfOrigin);
		}

		public void TestSettingDefaultsFromLVSPivot()
		{
			var part = GetPartWithImportPivot();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var existingCFIARegNumber = invoiceLine.CFIARegistrationNumbers.AddNew();
			existingCFIARegNumber.CY_Code = "002";
			existingCFIARegNumber.CY_Data = "???";
			var existingSittNumber = invoiceLine.SITTCertificationNumbers.AddNew();
			existingSittNumber.CY_Data = "67890";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			Assert(existingCFIARegNumber.IsDeleted);
			Assert(existingSittNumber.IsDeleted);
			AssertNotNull(invoiceLine.Part);
			AssertEquals("Invoice Units", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("CA_ValueForDutyCode", "13", invoiceLine.CA_ValueForDutyCode);
			AssertEquals("CA_TreatmentCode", "12", invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_99TariffCode", "9901", invoiceLine.CA_99TariffCode);
			AssertEquals("CA_AuthorityNumber", "AUTH#", invoiceLine.CA_AuthorityNumber);
			AssertEquals("CA_TRSNumber", "TRS#", invoiceLine.CA_TRSNumber);
			AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", USStatesList.Codes.Illinois, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RN_NKExport", ZString.Empty, invoiceLine.CA_RN_NKExport);
			AssertEquals("CA_USStateOfExport", ZString.Empty, invoiceLine.CA_USStateOfExport);
			AssertEquals("CA_RequirementID", "REQID", invoiceLine.CA_RequirementID);
			AssertEquals("CA_RequirementVer", "2", invoiceLine.CA_RequirementVer);
			AssertEquals("CA_AirsCode", "AIRSCD", invoiceLine.CA_AirsCode);
			AssertEquals("CA_DestinationProvince", CanadianProvinceList.Codes.Manitoba, invoiceLine.CA_DestinationProvince);
			AssertEquals("CA_EndUse", "111", invoiceLine.CA_EndUse);
			AssertEquals("CA_MiscID", "222", invoiceLine.CA_MiscID);
			AssertEquals("CA_RN_NKCFIAOrigin", Core.Constants.CountryCodes.UnitedStates, invoiceLine.CA_RN_NKCFIAOrigin);
			AssertEquals("CA_CFIAUSStateOfOrigin", USStatesList.Codes.NewYork, invoiceLine.CA_CFIAUSStateOfOrigin);
			AssertEquals("2 CFIA numbers", 2, invoiceLine.CFIARegistrationNumbers.Count);
			AssertEquals("1st CFIA number CY_Code", "001", invoiceLine.CFIARegistrationNumbers[0].CY_Code);
			AssertEquals("1st CFIA number CY_Date", "1234", invoiceLine.CFIARegistrationNumbers[0].CY_Data);
			AssertEquals("2nd CFIA number CY_Code", "002", invoiceLine.CFIARegistrationNumbers[1].CY_Code);
			AssertEquals("2nd CFIA number CY_Date", "5678", invoiceLine.CFIARegistrationNumbers[1].CY_Data);
			AssertEquals("CA_ImportReasonCode", "01", invoiceLine.CA_ImportReasonCode);
			AssertEquals("CA_Model", "MDL", invoiceLine.CA_Model);
			AssertEquals("CA_ModelNumber", "MDLN111", invoiceLine.CA_ModelNumber);
			AssertEquals("JI_BrandName", "BNAME", invoiceLine.JI_BrandName);
			AssertEquals("2 SITT numbers", 2, invoiceLine.SITTCertificationNumbers.Count);
			AssertEquals("1st SITT number", "12345", invoiceLine.SITTCertificationNumbers[0].CY_Data);
			AssertEquals("2nd SITT number", "67890", invoiceLine.SITTCertificationNumbers[1].CY_Data);
			AssertEquals("CA_TypeSize", "TYPSIZ", invoiceLine.CA_TypeSize);
			AssertEquals("CA_TIIN", "666", invoiceLine.CA_TIIN);
			Assert("CA_CompliantCompletion", invoiceLine.CA_CompliantCompletion);
			Assert("CA_CompliantImportDate", invoiceLine.CA_CompliantImportDate);
		}

		public void TestSettingDefaultsFromImportPivotOnB2()
		{
			var part = GetPartWithImportPivot();
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CI_TariffNum = "1234567890";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;

			Factory.Save();

			AssertNotNull(invoiceLine.Part);
			AssertEquals("HS code from pivot", "1234567890", invoiceLine.JI_Tariff);
			AssertEquals("Invoice Units", "KG", invoiceLine.JI_InvoiceUQ);
			AssertEquals("CA_ValueForDutyCode", "13", invoiceLine.CA_ValueForDutyCode);
			AssertEquals("CA_99TariffCode", "9901", invoiceLine.CA_99TariffCode);
			AssertEquals("CA_AuthorityNumber", "AUTH#", invoiceLine.CA_AuthorityNumber);
			AssertEquals("CA_TRSNumber", ZString.Empty, invoiceLine.CA_TRSNumber);
			AssertEquals("JI_CountryOfOrigin", ZString.Empty, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("CA_RequirementID", ZString.Empty, invoiceLine.CA_RequirementID);
		}

		public void TestUpdateDetailsOnPartChange_BrandName()
		{
			var part = GetPartWithImportPivot();
			importPivot.CCA_TCIndicator = YesNoList.Codes.Yes;
			importPivot.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
			importPivot.Part.OP_Brand = "Test Brand";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertNotNull(invoiceLine.Part);
			AssertEquals("Test Brand", invoiceLine.JI_BrandName);
		}

		public void TestCheckProerptiesWrappedJI_BrandName()
		{
			var invoiceLine = GetNewBusinessObject() as JobComInvoiceLine;
			CombineAssertions(() =>
			{
				var brandNameMaxLength = invoiceLine.JI_BrandNameInfo.MaxLength;
				Assert("CA_BrandNameAPI", invoiceLine.CA_BrandNameAPIInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameBBC", invoiceLine.CA_BrandNameBBCInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameCPR", invoiceLine.CA_BrandNameCPRInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameHDR", invoiceLine.CA_BrandNameHDRInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameOCS", invoiceLine.CA_BrandNameOCSInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameMDE", invoiceLine.CA_BrandNameMDEInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameNHP", invoiceLine.CA_BrandNameNHPInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNamePES", invoiceLine.CA_BrandNamePESInfo.MaxLength == brandNameMaxLength);
				Assert("CA_BrandNameVET", invoiceLine.CA_BrandNameVETInfo.MaxLength == brandNameMaxLength);
			});
		}

		public void TestConvertUQToCustomsUnitsWhenModifyingProductCode()
		{
			var part = GetPartWithImportPivot();
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CI_TariffNum = "1234567890";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			Factory.Save();

			AssertEquals(CustomsUnitOfMeasureList.Codes.Kilogram, invoiceLine.JI_InvoiceUQ);
		}

		OrgSupplierPart GetPart(string partNum = "APART", CusClassification classification = null)
		{
			if (importer == null)
			{
				importer = Factory.NewWithValidTestData<OrgHeader>();
			}

			if (supplier == null)
			{
				supplier = Factory.NewWithValidTestData<OrgHeader>();
			}

			manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress = Factory.NewWithValidTestData<OrgAddress>();
			manufacturerAddress = manufacturer.MainAddress;
			manufacturerAddress.Address1 = "MANUFACTURER ADDRESS";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.OP_Desc = "A typical part";
			part.OP_StockKeepingUnit = "KG";
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);

			if (classification == null)
			{
				classification = Factory.New<CusClassification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			}
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;
			exportPivot = part.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			Factory.Save();
			return part;
		}

		OrgHeader importer;
		OrgHeader supplier;
		OrgHeader manufacturer;
		CusClassPartPivot importPivot;
		CusClassPartPivot exportPivot;

		#endregion

		#region Test Header Properties

		public void TestFallbackToHeaderProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("CA_TreatmentCode", ZString.Empty, invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", ZString.Empty, invoiceLine.CA_ValueForDutyCode);
			AssertEquals("JI_CountryOfOrigin", ZString.Empty, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", ZString.Empty, invoiceLine.JI_StateOrRegionOfOrigin);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.CA_TreatmentCode = "02";
			invoice.CA_ValueForDutyCode = "14";
			invoice.JZ_RN_NKDefaultOrigin = "US";
			invoice.JZ_RW_NKOriginState = "AL";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Fallback to Header's TreatmentCode", "02", invoiceLine.CA_TreatmentCode);
			AssertEquals("Fallback to Header's ValueForDutyCode", "14", invoiceLine.CA_ValueForDutyCode);
			AssertEquals("Fallback to Header's CountryOfOrigin", "US", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("Fallback to Header's StateOfOrigin", "AL", invoiceLine.JI_StateOrRegionOfOrigin);

			invoiceLine.JI_StateOrRegionOfOrigin = "AK";
			invoice.JZ_RN_NKDefaultOrigin = "GB";
			AssertEquals("JI_CountryOfOrigin", "GB", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", "", invoiceLine.JI_StateOrRegionOfOrigin);

			invoiceLine.CA_TreatmentCode = "03";
			invoiceLine.CA_ValueForDutyCode = "15";
			invoiceLine.JI_CountryOfOrigin = "CA";

			AssertEquals("CA_TreatmentCode", "03", invoiceLine.CA_TreatmentCode);
			AssertEquals("CA_ValueForDutyCode", "15", invoiceLine.CA_ValueForDutyCode);
			AssertEquals("JI_CountryOfOrigin", "CA", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_StateOrRegionOfOrigin", "", invoiceLine.JI_StateOrRegionOfOrigin);
		}

		#endregion

		#region Test Bonded Warehouse Interface

		public void TestSettingBondedWarehouseFields()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			declaration.JE_OH_Importer = helper.Importer.PK;

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "123456000001-1";
			AssertEquals("JI_PreviousEntryNumber", "123456000001", invoiceLine.JI_PreviousEntryNumber);
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)1, invoiceLine.JI_PreviousEntryLineNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)2, invoiceLine.JI_PreviousEntryLineNumber);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			invoiceLine.JI_PreviousEntryNumber = "123456000002";
			AssertEquals("JI_PreviousEntryNumber", "123456000002", invoiceLine.JI_PreviousEntryNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)3, invoiceLine.JI_PreviousEntryLineNumber);
		}

		public void TestSettingBondedWarehouseFieldsForCAD()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			declaration.JE_OH_Importer = helper.Importer.PK;

			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PreviousEntryNumber = "123456000001-1";
			AssertEquals("JI_PreviousEntryNumber", "123456000001", invoiceLine.JI_PreviousEntryNumber);
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)1, invoiceLine.JI_PreviousEntryLineNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)2, invoiceLine.JI_PreviousEntryLineNumber);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			invoiceLine.JI_PreviousEntryNumber = "123456000002";
			AssertEquals("JI_PreviousEntryNumber", "123456000002", invoiceLine.JI_PreviousEntryNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)3, invoiceLine.JI_PreviousEntryLineNumber);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			invoiceLine.JI_PreviousEntryNumber = "123456000003";
			AssertEquals("JI_PreviousEntryNumber", "123456000003", invoiceLine.JI_PreviousEntryNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 4;
			AssertEquals("JI_PreviousEntryLineNumber", (ZShort)4, invoiceLine.JI_PreviousEntryLineNumber);
		}

		public void TestIsGoingIntoBondedWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			Assert("IsGoingIntoBondedWarehouse", invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			var invoiceLine2 = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine2.IsGoingIntoBondedWarehouse);
			var invoiceLine3 = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.CA_IsAutoDummyHSCodeCasualImportLine = true;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine3.IsGoingIntoBondedWarehouse);

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Assert("IsGoingIntoBondedWarehouse", invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			Assert("IsGoingIntoBondedWarehouse", invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine1.IsGoingIntoBondedWarehouse);
			var invoiceLine4 = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.CA_CalculationMethod = CalculationMethods.Codes.RepairsRemission;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine4.IsGoingIntoBondedWarehouse);
			var invoiceLine5 = (JobComInvoiceLine)declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine3.CA_IsAutoDummyHSCodeCasualImportLine = true;
			Assert("IsGoingIntoBondedWarehouse", !invoiceLine5.IsGoingIntoBondedWarehouse);
		}

		#endregion

		#region Test PGA Header AddInfo Data

		public void TestPGAHeaderAddInfoData()
		{
			CheckPGAHeaderAddInfoData(PGACodes.Codes.CFIA);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.CNSC);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.DFO);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.ECCC);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.GAC);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.HC);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.NRCan);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.PHAC);
			CheckPGAHeaderAddInfoData(PGACodes.Codes.TC);
		}

		void CheckPGAHeaderAddInfoData(ZString agencyCode)
		{
			var pgaProvider = new PGARequirementProvider(InvoiceLine);
			var pgaRequirement = new PGARequirement(Factory, agencyCode, pgaProvider);
			pgaRequirement.ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
			Factory.Save();
			var pgaHeader = pgaProvider.GetProgramRequirementProvider(agencyCode) as CusAddInfo;
			AssertEquals(true, pgaHeader.IsInDatabase);
			AssertEquals(true, pgaHeader.B7_AddInfoData.Contains("ProgramInd"));

			pgaRequirement.ProgramCodeRequirements[0].Indicator = string.Empty;
			Factory.Save();
			AssertEquals(false, pgaHeader.IsInDatabase);
		}

		#endregion

		#region Test SIMA Dumping Number

		public void TestSIMADumpingNumber()
		{
			SetUpUniversalTariff();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.OnRefreshSIMAMeasureEvent = null;
			invoiceLine.JI_Tariff = "0123456789";
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.CA_SIMADumpingNum);
			invoiceLine.JI_Tariff = "0789456123";
			AssertEquals(ZString.Empty, invoiceLine.CA_SIMADumpingNum);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedInvoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			loadedInvoiceLine.OnRefreshSIMAMeasureEvent += () =>
			{
				return loadedInvoiceLine.SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault(x => x.CA_DumpingNumber == "AD1409");
			};
			loadedInvoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(ZString.Empty, loadedInvoiceLine.CA_SIMADumpingNum);
			loadedInvoiceLine.JI_Tariff = "0789456123";
			AssertEquals("AD1409", loadedInvoiceLine.CA_SIMADumpingNum);
			loadedInvoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals(ZString.Empty, loadedInvoiceLine.CA_SIMADumpingNum);
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			loadedInvoiceLine.JI_Tariff = "0789456123";
			AssertEquals(ZString.Empty, loadedInvoiceLine.CA_SIMADumpingNum);
			ErrorReporter.Clear();
		}

		public void TestRefreshSIMAOnProductImport()
		{
			SetUpUniversalTariff();
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

			var partWithSIMA = GetPart("PART1", classification);
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "0123456789";
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals("AD1407", importPivot.CCA_SIMADumpingNumber);
			AssertEquals(1, importPivot.DutiesAndTaxes.Count);
			AssertEquals(DutyAndTaxTypes.Codes.ADD, importPivot.DutiesAndTaxes[0].C1_TaxType);

			var partWithoutSIMA = GetPart("PART2", classification);
			importPivot.CI_CC = ZGuid.Empty;
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Taiwan;
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "0123456789";
			AssertEquals("AD1407", importPivot.CCA_SIMADumpingNumber);
			AssertEquals(1, importPivot.DutiesAndTaxes.Count);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.Taiwan;
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Taiwan;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_PartNo = partWithSIMA.OP_PartNum;
			AssertEquals("Contr Of Origin = CN", "CN", invoiceLine.CountryOfOrigin.Code);
			AssertEquals("SIMA should be populated from Invoice Line Details", "AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals("A SIMA duty should be populated", 1, invoiceLine.DutiesAndTaxes.Count);
			AssertEquals("The rate should be the one for CN", 100.5m, invoiceLine.DutiesAndTaxes[0].C1_Rate);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_PartNo = partWithoutSIMA.OP_PartNum;
			AssertEquals("Contr Of Origin = TW", "TW", invoiceLine.CountryOfOrigin.Code);
			AssertEquals("SIMA should be populated from Product", "AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals("A SIMA duty should be populated", 1, invoiceLine.DutiesAndTaxes.Count);
			AssertEquals("The rate should be the one for TW", 200.5m, invoiceLine.DutiesAndTaxes[0].C1_Rate);
		}

		public void TestCreatingProductWithSIMAFromInvoiceLine_NoDuplicateDutiesOnInvoiceLine()
		{
			SetUpUniversalTariff();
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			importer = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_PartNo = "PART1";

			var partWithSIMA = GetPart("PART1", classification);
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "0123456789";
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;

			invoiceLine.PartSyncManager.Refresh();

			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
		}

		void SetUpUniversalTariff()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
			var taiwanTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(taiwanTradeGroup, Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingRateTW = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingApplicabilityTW = universalHelper.CreateCusApplicability(antiDumpingRateTW, taiwanTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456123", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(countervailingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var countervailingTariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123");
			var countervailingRelationShip1 = universalHelper.CreateTariffRelationship(countervailingTariff1.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate1 = universalHelper.CreateRate(countervailingTariff1, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability1 = universalHelper.CreateCusApplicability(countervailingRate1, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingTariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1409", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123");
			var countervailingRelationShip2 = universalHelper.CreateTariffRelationship(countervailingTariff2.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate2 = universalHelper.CreateRate(countervailingTariff2, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "120 * [KGM]");
			var countervailingApplicability2 = universalHelper.CreateCusApplicability(countervailingRate2, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		#endregion

		public void TestPGADetailsNotClearedWhenCreateProduct()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			var importer = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.PartSyncManager.Enabled = false;
			invoiceLine.JI_Tariff = "2507000000";
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode = "ABC123";
			invoiceLine.CFIAPGAHeader.AIRSRegistrationNumbers.AddNew("893", "12345A");

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "INCT1";
			var supplierOrg = part.RelatedOrganisations.AddNew();
			supplierOrg.OU_Relationship = "SUP";
			supplierOrg.OU_OH = supplier.PK;
			var ownerOrg = part.RelatedOrganisations.AddNew();
			ownerOrg.OU_Relationship = "OWN";
			ownerOrg.OU_OH = importer.PK;
			part.JustUpdatedByDataRefresh = true;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "2507000000";
			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			pivot.CFIAPGAHeader.CA_AIRSExtensionCode = "EFG456";
			invoiceLine.JI_PartNo = "INCT1";
			((IDataRefreshBusSubscriber)invoiceLine.PartSyncManager).UpdatedByDataRefresh(new[] { part });
			AssertEquals(YesNoList.Codes.Yes, invoiceLine.CA_CFIAInd);
			AssertEquals("ABC123", invoiceLine.CFIAPGAHeader.CA_AIRSExtensionCode);
			AssertEquals(1, invoiceLine.CFIAPGAHeader.AIRSRegistrationNumbers.Count);
			AssertEquals("893", invoiceLine.CFIAPGAHeader.AIRSRegistrationNumbers[0].CY_Code);
			AssertEquals("12345A", invoiceLine.CFIAPGAHeader.AIRSRegistrationNumbers[0].CY_Data);
		}

		public void TestPackagesForInvoiceLinesForBindingOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MB123";

			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Container;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 5;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 1;
			package3.CW_PackType = Core.Constants.PkgUnit.Box;
			package3.CW_CW_Parent = package1.PK;

			var package4 = declaration.Packages.AddNew();
			package4.CW_PackQty = 25;
			package4.CW_PackType = Core.Constants.PkgUnit.Basket;
			package4.CW_CW_Parent = package2.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(2, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);

			declaration.Packages.Delete(package3);
			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);
		}

		public void TestTemplateCopy_InvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_PreviousLineNo = 1;
			invoiceLine.JI_LinePrice = 100m;

			AssertEquals(1, invoiceLine.CA_PreviousLineNo);
			AssertEquals(100m, invoiceLine.JI_LinePrice);

			var invoiceLineClone = (JobComInvoiceLine)((ITemplateCopyable)invoiceLine).TemplateCopy();
			AssertEquals(1, invoiceLineClone.CA_PreviousLineNo);
			AssertEquals(100m, invoiceLineClone.JI_LinePrice);

			invoiceLine.JI_LinePrice = 200m;
			var invoiceLineClone2 = (JobComInvoiceLine)((ITemplateCopyable)invoiceLine).TemplateCopy();

			AssertEquals(1, invoiceLineClone2.CA_PreviousLineNo);
			AssertEquals(200m, invoiceLineClone2.JI_LinePrice);
		}

		public void TestCopySIMADutiesFromProductToInvoiceLine()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
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
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_TariffNum = "1234567890";
			importPivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			AssertEquals("AD1407", importPivot.CCA_SIMADumpingNumber);
			AssertEquals(3, importPivot.DutiesAndTaxes.Count);
			foreach (var dutyAndTax in importPivot.DutiesAndTaxes)
			{
				dutyAndTax.C1_ExemptCode = SIMACodes.Codes.C31;
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			var surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C31, false, 10m, "", 0m, "", 0m, "");
			var addTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C31, false, 100.5m, "NMB", 0m, "", 0m, "");
			var cvdTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C31, false, 200m, "KGM", 0m, "", 0m, "");

			var surTaxInPivot = importPivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			surTaxInPivot.C1_Override = true;
			surTaxInPivot.C1_Rate = 20m;
			surTaxInPivot.C1_ExemptCode = SIMACodes.Codes.C51;
			var addTaxInPivot = importPivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			addTaxInPivot.C1_Override = true;
			addTaxInPivot.C1_NormalValuePerUnit = 10m;
			addTaxInPivot.C1_NormalValueCurrency = "CNY";
			addTaxInPivot.C1_ExemptCode = SIMACodes.Codes.C51;
			var cvdTaxInPivot = importPivot.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			cvdTaxInPivot.C1_Override = true;
			cvdTaxInPivot.C1_ForeignRate = 15m;
			cvdTaxInPivot.C1_ForeignCurrency = "CNY";
			Factory.Save();

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C51, true, 20m, "", 0m, "", 0m, "");
			addTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C51, true, 100.5m, "NMB", 10m, "CNY", 0m, "");
			cvdTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C51, true, 200m, "KGM", 0m, "", 15m, "CNY");

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.CC_TariffNum = "1234567890";
			AssertEquals(3, classification.DutiesAndTaxes.Count);
			foreach (var dutyAndTax in classification.DutiesAndTaxes)
			{
				dutyAndTax.C1_ExemptCode = SIMACodes.Codes.C52;
			}

			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			supRelation = part.RelatedOrganisations.AddSupplier(supplier);
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;
			AssertEquals("AD1407", importPivot.CCA_SIMADumpingNumber);
			AssertEquals(0, importPivot.DutiesAndTaxes.Count);
			AssertEquals(3, importPivot.DutiesAndTaxesForCC.Count);
			Factory.Save();

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 3;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C52, false, 10m, "", 0m, "", 0m, "");
			addTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C52, false, 100.5m, "NMB", 0m, "", 0m, "");
			AssertEquals(0m, addTaxInvoiceLine.C1_Amount);
			cvdTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C52, false, 200m, "KGM", 0m, "", 0m, "");
			AssertEquals(600m, cvdTaxInvoiceLine.C1_Amount);
		}

		public void TestCopySIMADutiesFromCusClassificationToInvoiceLine()
		{
			#region Setup Universal Tariff

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var chinaTradeGroup = universalHelper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddNewOrExistingCountry(chinaTradeGroup, Core.Constants.CountryCodes.China);
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
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "1234567890");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "1234567890");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var countervailingRate = universalHelper.CreateRate(antiDumpingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			#endregion

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "IMPLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.China;
			classification.CC_TariffNum = "1234567890";
			AssertEquals("AD1407", classification.CCA_SIMADumpingNumber);
			AssertEquals(3, classification.DutiesAndTaxes.Count);
			foreach (var dutyAndTax in classification.DutiesAndTaxes)
			{
				dutyAndTax.C1_ExemptCode = SIMACodes.Codes.C31;
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			var surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C31, false, 10m, "", 0m, "", 0m, "");
			var addTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C31, false, 100.5m, "NMB", 0m, "", 0m, "");
			var cvdTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C31, false, 200m, "KGM", 0m, "", 0m, "");

			var surTaxInPivot = classification.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			surTaxInPivot.C1_Override = true;
			surTaxInPivot.C1_Rate = 20m;
			var addTaxInPivot = classification.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			addTaxInPivot.C1_Override = true;
			addTaxInPivot.C1_NormalValuePerUnit = 10m;
			addTaxInPivot.C1_NormalValueCurrency = "CNY";
			var cvdTaxInPivot = classification.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			cvdTaxInPivot.C1_Override = true;
			cvdTaxInPivot.C1_ForeignRate = 15m;
			cvdTaxInPivot.C1_ForeignCurrency = "CNY";
			Factory.Save();

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Kilogram;
			invoiceLine.JI_CC = classification.PK;
			AssertEquals("AD1407", invoiceLine.CA_SIMADumpingNum);
			AssertEquals(3, invoiceLine.DutiesAndTaxes.Count);
			surTaxInInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.SUR);
			AssertDutyProperties(surTaxInInvoiceLine, SIMACodes.Codes.C31, true, 20m, "", 0m, "", 0m, "");
			addTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.ADD);
			AssertDutyProperties(addTaxInvoiceLine, SIMACodes.Codes.C31, true, 100.5m, "NMB", 10m, "CNY", 0m, "");
			AssertEquals(0m, addTaxInvoiceLine.C1_Amount);
			cvdTaxInvoiceLine = invoiceLine.DutiesAndTaxes.First(x => x.C1_TaxType == DutyAndTaxTypes.Codes.CVD);
			AssertDutyProperties(cvdTaxInvoiceLine, SIMACodes.Codes.C31, true, 200m, "KGM", 0m, "", 15m, "CNY");
			AssertEquals(400m, cvdTaxInvoiceLine.C1_Amount);
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

		public override void TestWipeNKTaxType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			Assert(!invoiceLine.ShouldWipeNKTaxType);
		}

		public void TestPopulateCNSCUNDGCode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			dec.JE_OH_Importer = importer.PK;
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "TestProd";
			var relatedOrganisation = product.RelatedOrganisations.AddNew();
			relatedOrganisation.OU_OH = importer.PK;
			relatedOrganisation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "TEST";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var uNDGDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			uNDGDataItem.DI_DG = subs.PK;
			uNDGDataItem.LinkDefault(subs);
			product.UNDGs.Add(uNDGDataItem);
			invoiceLine.DangerousGoodsDGSubs = subs.PK;
			Factory.Save();

			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertEquals(uNDGDataItem.DI_DG, invoiceLine.DangerousGoodsDGSubs);

			invoiceLine.CA_CNSCInd = "Y";
			AssertEquals(uNDGDataItem.DI_DG, invoiceLine.CNSCPGAHeader.DangerousGoodsDGSubs);
		}

		public void TestUpdateAMMV()
		{
			#region SetUp Data
			var declaration = GetImportDeclarationWithBuyerSupplier();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 20m;
			invoiceLine.JI_LinePrice = 2000m;

			var product = GetPartWithRelationship(declaration.Importer);
			product.OP_PartNum = "ProductForTesting";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CCA_AMMVPercentage = 20m;
			pivot.CCA_AMMVPerUnit = ZDecimal.Zero;

			var product2 = GetPartWithRelationship(declaration.Importer);
			product2.OP_PartNum = "ProductForTesting2";
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CCA_AMMVPercentage = ZDecimal.Zero;
			pivot2.CCA_AMMVPerUnit = 20m;

			var product3 = GetPartWithRelationship(declaration.Importer);
			product3.OP_PartNum = "ProductForTesting3";
			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CCA_AMMVPercentage = ZDecimal.Zero;
			pivot3.CCA_AMMVPerUnit = ZDecimal.Zero;

			var product4 = GetPartWithRelationship(declaration.Importer);
			product4.OP_PartNum = "ProductForTesting4";
			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CCA_AMMVPercentage = ZDecimal.Zero;
			pivot4.CCA_AMMVPerUnit = 10m;
			pivot4.CCA_AMMVPerUnitCurrency = "USD";
			#endregion

			CombineAssertions(() =>
			{
				invoiceLine.JI_PartNo = product.OP_PartNum;
				AssertEquals("There should be a new ADD Charge in invoice line if AMMVPercentage was specified in product", 1, invoiceLine.ApportionedCharges.Count);
				var charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(20m, charge.J7_Percentage);
				AssertEquals(400m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.Japan, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product2.OP_PartNum;
				AssertEquals("AMMVPerUnit from the product should be copied across to the invoice line", 20m, invoiceLine.CA_AMMVPerUnit);
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(true, charge.J7_Percentage.IsEmpty);
				AssertEquals(400m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.Canada, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product4.OP_PartNum;
				AssertEquals("AMMVPerUnit from the product should be copied across to the invoice line", 10m, invoiceLine.CA_AMMVPerUnit);
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);
				charge = invoiceLine.ApportionedCharges[0];
				AssertEquals(true, charge.J7_Percentage.IsEmpty);
				AssertEquals(200m, charge.J7_Amount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, charge.J7_RX_NKCurrency);
				AssertEquals(true, charge.IsAMMV());

				invoiceLine.JI_PartNo = product.OP_PartNum;
				invoiceLine.CA_AMMVPercentage = 0m;
				invoiceLine.CA_AMMVPerUnit = 30m;
				AssertEquals("Charge should be created upon unit value input", 600m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_InvoiceQuantity = 30m;
				AssertEquals("Charge should be recalculated upon line quantity change", 900m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.CA_AMMVPerUnit = 0m;
				AssertEquals("Charge should be none if no amount", 0, invoiceLine.ApportionedCharges.Count);
				invoiceLine.CA_AMMVPercentage = 10m;
				AssertEquals("Charge should be created upon percentage input", 1, invoiceLine.ApportionedCharges.Count);
				AssertEquals("Currency should change to line currency if calculate by percentage", Core.Constants.CurrencyCodes.Japan, invoiceLine.ApportionedCharges[0].J7_RX_NKCurrency);
				AssertEquals("Charge should be calculated by percentage and line price", 200m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_LinePrice = 3000m;
				AssertEquals("Charge should be recalculated upon line price change", 300m, invoiceLine.ApportionedCharges[0].J7_Amount);
				invoiceLine.JI_LinePrice = 0m;
				AssertEquals("Charge should be none if no amount", 0, invoiceLine.ApportionedCharges.Count);

				invoiceLine.JI_PartNo = product3.OP_PartNum;
				AssertEquals(0, invoiceLine.ApportionedCharges.Count);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				invoiceLine.CA_AMMVPerUnit = 30m;
				AssertEquals("AMMV only works for import", 0, invoiceLine.ApportionedCharges.Count);
				invoiceLine.CA_AMMVPercentage = 30m;
				AssertEquals("AMMV only works for import", 0, invoiceLine.ApportionedCharges.Count);
			});
		}

		public void TestAMMVReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				AssertEquals(false, invoiceLine.CA_AMMVPercentageInfo.ReadOnly);
				AssertEquals(false, invoiceLine.CA_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.CA_AMMVPerUnit = 10m;
				AssertEquals(true, invoiceLine.CA_AMMVPercentageInfo.ReadOnly);
				invoiceLine.CA_AMMVPerUnit = 0m;
				AssertEquals(false, invoiceLine.CA_AMMVPercentageInfo.ReadOnly);
				invoiceLine.CA_AMMVPercentage = 10m;
				AssertEquals(true, invoiceLine.CA_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.CA_AMMVPercentage = 0m;
				AssertEquals(false, invoiceLine.CA_AMMVPerUnitInfo.ReadOnly);
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPerUnit = 10m;
				AssertEquals(false, invoiceLine.CA_AMMVPerUnitInfo.ReadOnly);
				AssertEquals(false, invoiceLine.CA_AMMVPercentageInfo.ReadOnly);
			});
		}

		public void TestOnCA_AMMVPercentageChangedCA_AMMVPerUnitIsClearedOut()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.CA_AMMVPerUnit);
				AssertEquals(10m, invoiceLine.CA_AMMVPercentage);

				invoiceLine.CA_AMMVPercentage = 10m;
				AssertEquals(10m, invoiceLine.CA_AMMVPerUnit);

				invoiceLine.CA_AMMVPercentage = 0m;
				AssertEquals(10m, invoiceLine.CA_AMMVPerUnit);

				invoiceLine.CA_AMMVPercentage = 20m;
				AssertEquals(ZDecimal.Zero, invoiceLine.CA_AMMVPerUnit);

				invoiceLine.CA_AMMVPercentage = 30m;
				AssertEquals(ZDecimal.Zero, invoiceLine.CA_AMMVPerUnit);
			});
		}

		public void TestOnCA_AMMVPerUnitChangedCA_AMMVPercentageIsClearedOut()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPercentage = 10m;
				invoiceLine.AddInfoLookups.Parent.CA_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.CA_AMMVPerUnit);
				AssertEquals(10m, invoiceLine.CA_AMMVPercentage);

				invoiceLine.CA_AMMVPerUnit = 10m;
				AssertEquals(10m, invoiceLine.CA_AMMVPercentage);

				invoiceLine.CA_AMMVPerUnit = 0m;
				AssertEquals(10m, invoiceLine.CA_AMMVPercentage);

				invoiceLine.CA_AMMVPerUnit = 20m;
				AssertEquals(ZDecimal.Zero, invoiceLine.CA_AMMVPercentage);

				invoiceLine.CA_AMMVPerUnit = 30m;
				AssertEquals(ZDecimal.Zero, invoiceLine.CA_AMMVPercentage);
			});
		}

		public void TestProductLoadedCustomsUnitAfterPartCreatedInCA()
		{
			ZString tariffCode = "4415103000";
			ZString customsUQ = "";

			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;

			var invoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PART1";
			AssertEquals("invoiceLine.JI_OP", ZGuid.Empty, invoiceLine.JI_OP);
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.UpdatePartSyncManagerAndRefresh(true);
			invoiceLine.JI_InvoiceUQ = "PCE";
			Factory.Save();

			AssertEquals("PART1", invoiceLine.JI_PartNo);
			AssertEquals("", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.JI_CustomsUnitQty);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART1";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.OP_StockKeepingUnit = "NO";
			part.RelatedOrganisations.AddOwner(consignee);

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "4415103000", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, "Default Customs UQ");
			universalHelper.CreateTariffUOM(tariff, "CU1", "NMB");
			Factory.Save();

			var classification = Factory.NewWithValidTestData<CusClassification>();
			var aPivot = Factory.New<CusClassPartPivot>();
			aPivot.CI_CC = classification.PK;
			aPivot.CI_OP = part.PK;
			aPivot.CI_PartPivotUOM = "NO";
			classification.CC_TariffNum = "4415103000";
			classification.CC_LookupCode = "GRANNY";
			classification.CC_ClassificationType = "BTH";

			Factory.Save();

			var invoiceLoaded = new BusinessObjectFactory().Load<JobComInvoiceHeader>(invoice.PK);
			new FakeDeclarationCreatorForInvoice(invoiceLoaded);
			var invoiceLineLoaded = invoiceLoaded.InvoiceLines[0];
			Assert("Precondition:PartSyncManager.Enabled", invoiceLineLoaded.PartSyncManager.Enabled);
			AssertEquals("Precondition:JI_OP", part.PK, invoiceLineLoaded.JI_OP);

			AssertEquals("4415103000", invoiceLineLoaded.JI_Tariff);
			AssertEquals("NMB", invoiceLineLoaded.JI_CustomsUnitQty);
		}

		public void TestGetNewValidation()
		{
			var invoiceLine = (JobComInvoiceLine)GetNewBusinessObject();
			var declaration = invoiceLine.Declaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Misc Validation", typeof(JobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Validation", typeof(ImportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("IM2 Validation", typeof(ImportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Validation", typeof(B2JobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Validation", typeof(B2JobComInvoiceLineValidation), invoiceLine.Validation.GetType());
		}

		public void TestCustomsQuantity_DecimalPlaces()
		{
			var invoiceLine = (JobComInvoiceLine)GetNewBusinessObject();
			AssertEquals(3, Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.JI_CustomsQuantityInfo).DecimalPlaces);
			AssertEquals(3, Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.JI_CustomsSecondQuantityInfo).DecimalPlaces);
			AssertEquals(3, Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.JI_CustomsThirdQuantityInfo).DecimalPlaces);
		}

		public void TestNoExceptionThrownWhenAccessPartInOnloaded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine2.JI_ParentTableCode = invoiceLine.TablePrefix;
			invoiceLine2.JI_Tariff = JobComInvoiceLine.LuxuryTaxTariffCode;
			invoiceLine2.JI_InvoiceQuantity = 1000m;

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				var factory = new BusinessObjectFactory();
				var declarationLoaded = factory.Load<JobDeclaration>(declaration.PK);
				_ = declarationLoaded.InvoiceLines;
			});
		}

		protected override bool UseUniversalTariff => false;

		JobDeclaration GetImportDeclarationWithBuyerSupplier()
		{
			JobDeclaration result = Factory.NewWithValidTestData<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			return result;
		}

		OrgSupplierPart GetPartWithRelationship(OrgHeader importer)
		{
			var result = Factory.New<OrgSupplierPart>();
			result.RelatedOrganisations.AddOwner(importer);
			return result;
		}

		#region LightValidationTester

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new JobComInvoiceLineLightValidationTester(bizObjToTest);
		}

		class JobComInvoiceLineLightValidationTester : LightValidationTester
		{
			public JobComInvoiceLineLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != JobDeclaration.Schema.JE_SystemCreateTimeUtc;
			}
		}

		#endregion

		#region Implementation

		class ExpectedClassificationLine2ForTesting : IClassificationLine2
		{
			public ZInt B3LineNumber { get; set; }
			public ZString UnitOfMeasureCode { get; set; }
			public ZDecimal ClassificationLineQuantity { get; set; }
			public ZDecimal WeightInKGM { get; set; }
			public ZDecimal CustomsDutyRate { get; set; }
			public ZString CustomsDutyRateType { get; set; }
			public ZDecimal CustomsDutyAmount { get; set; }
			public ZString PreviousTransactionNumber { get; set; }
			public ZInt PreviousLineNumber { get; set; }
			public ZString InvoiceUnitOfMeasureCode { get; set; }
			public ZDecimal ClassificationLineInvoiceQuantity { get; set; }
		}

		public class ExpectedClassificationLine1ForTesting : IClassificationLine1
		{
			public ZDecimal Deposit { get; set; }
			public ZShort B3LineNumber { get; set; }
			public ZShort SequenceNumber { get; set; }
			public ZString RecordIdentifier { get; set; }
			public ZInt B3SubHeaderNumber { get; set; }
			public ZInt B3SubHeaderNumberForLVX { get; set; }
			public ZString ClassificationNumber { get; set; }
			public ZString ValueForDutyCode { get; set; }
			public ZString TariffCode { get; set; }
			public ZDecimal ValueForCurrency { get; set; }
			public ZDecimal ValueForDuty { get; set; }
			public ZDecimal ValueForTax { get; set; }
			public ZString AuthorityNumber { get; set; }
			public ZString TRSNumber { get; set; }
			public ZString[] PartNumberDescriptions { get; set; }
			public IEnumerable<IInvoiceCrossReference> InvoiceCrossReferences { get; set; }
			public ZDecimal CustomsQuantity { get; set; }
			public ZString CustomsUnitQty { get; set; }
			public ZDecimal InvoiceQuantity { get; set; }
			public ZString InvoiceUQ { get; set; }
			public ZDecimal CountOfInvoice { get; set; }
			public Money TotalLinePrice { get; set; }
			public Money CustomsValue { get; set; }
			public Money FOB { get; set; }
			public ZDecimal SalesTaxAmount { get; set; }
			public ZBool SalesIsOverride { get; set; }
			public ZDecimal CTAAmount { get; set; }
			public ZBool CTAIsOverride { get; set; }
			public (ZDecimal Amount, RefCurrency Currency) DeductionChargeAmountAndCurrency { get; set; }
			public ZString CustomsDutyCode { get; set; }
			public ZDecimal SurtaxQuantity { get; set; }
			public ZDecimal SurtaxAmount { get; set; }
			public ZString SurtaxUnitOfMeasure { get; set; }
			public ZString SurtaxStatementCode { get; set; }
			public ZString SurtaxCode { get; set; }
			public ZBool SurtaxIsOverride { get; set; }
			public ZBool HasSurtax { get; set; }
			public ZDecimal ADDAmount { get; set; }
			public ZDecimal ADDQuantity { get; set; }
			public ZString ADDUnitOfMeasure { get; set; }
			public ZString ADDCode { get; set; }
			public ZBool ADDIsOverride { get; set; }
			public ZBool HasADD { get; set; }
			public ZDecimal CVDAmount { get; set; }
			public ZDecimal CVDQuantity { get; set; }
			public ZString CVDUnitOfMeasure { get; set; }
			public ZString CVDCode { get; set; }
			public ZBool CVDIsOverride { get; set; }
			public ZBool HasCVD { get; set; }
			public ZDecimal SafeguardAmount { get; set; }
			public ZString SafeguardCode { get; set; }
			public ZBool SafeguardIsOverride { get; set; }
			public ZBool HasSafeguard { get; set; }
			public ZString SafeguardStatementCode { get; set; }
			public ZString SIMACode { get; set; }
			public ZString SIMAStatementCode { get; set; }
			public ZDecimal SIMAAssessment { get; set; }
			public ZDecimal ExciseDutyAmount { get; set; }
			public ZString ExciseExemptionCode { get; set; }
			public ZString ExciseCode { get; set; }
			public ZDecimal ExciseTaxRate { get; set; }
			public ZDecimal ExciseTaxRateToPrint { get; set; }
			public ZString ExciseTaxRateType { get; set; }
			public ZDecimal ExciseTaxAmount { get; set; }

			public bool IsDummyExciseTaxRate
			{
				get { return false; }
			}

			public ZBool HasExcise { get; set; }
			public ZString GSTExemptionCode { get; set; }
			public ZString GSTCode { get; set; }
			public ZDecimal RateOfGST { get; set; }
			public ZString GSTRateType { get; set; }
			public ZDecimal GSTAmount { get; set; }
			public ZBool GSTIsOverride { get; set; }
			public bool HasGSTDetails { get; set; }
			public ZDecimal CUDAmount { get; set; }
			public IEnumerable<IClassificationLine2> ClassificationLines { get; set; }
			public ZInt CountOfConsolidatedLines { get; set; }

			public BusinessObjectFactory Factory { get; set; }

			public BusinessObject RelevantLine { get; set; }
			public MessageSubTypes MessageSubType { get; set; }
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var header = declaration.Invoices.AddNew();
			return header.InvoiceLines.AddNew();
		}

		protected override Type ExpectedMetadataType
		{
			get { return typeof(Metadata.Business.CAJobComInvoiceLine); }
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		void UseCustomsTariffList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.InvalidateCachedProperties();
		}

		void UseExportTarifList()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.InvalidateCachedProperties();
		}

		const string CustomsTariffCode = "1234567890";
		const string CustomsTariffDescription = "CUSTOMS TARIFF DESCRIPTION.";
		const string CustomsTariffUnits = "KGM";
		const string ExportTariffCode = "1234567800";
		const string ExportTariffDescription = "EXPORT TARIFF DESCRIPTION.";
		const string ExportTariffUnits = "KGM";

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

			var helper = new CACExportTariffTestCase(Factory);
			helper.CreateNewTariffIfNotExists(ExportTariffCode, ExportTariffDescription, ExportTariffUnits);
			Factory.Save();
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		protected override bool RatesAreReciprocal => true;

		#endregion

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
