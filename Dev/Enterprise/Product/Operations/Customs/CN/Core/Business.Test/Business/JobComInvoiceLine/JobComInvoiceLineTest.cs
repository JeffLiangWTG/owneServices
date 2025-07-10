using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCodeCore should be CN", Core.Constants.CountryCodes.China, InvoiceLine.CustomsCountryCode);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
				AssertEquals(Core.Constants.CountryCodes.China, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestRateSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "01";
			invoiceLine.JI_ConcessionOrder = "TestOrder";

			var rateSelectionCriteria = invoiceLine.ExportDutyRateSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCode = JI_SecondaryPreference", rateSelectionCriteria.AdditionalCodes.Contains("01"));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", Universal.Constants.RateTypes.ExportDuty, rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);

			rateSelectionCriteria = invoiceLine.ExciseRateSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCode = JI_SecondaryPreference", rateSelectionCriteria.AdditionalCodes.Contains("01"));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", Universal.Constants.RateTypes.Excise, rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);

			rateSelectionCriteria = invoiceLine.GetSpecificRateSelectionCriteria(Universal.Constants.RateTypes.Duty, Constants.UniversalReferenceConstants.RefCusRateCodes.CustomsDuty, Constants.PrimaryPreferenceCodes.MostFavouredNations, ZString.Empty);
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", Constants.PrimaryPreferenceCodes.MostFavouredNations, rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCode is empty", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", Universal.Constants.RateTypes.Duty, rateSelectionCriteria.RateType);
			AssertEquals("RateCode", Constants.UniversalReferenceConstants.RefCusRateCodes.CustomsDuty, rateSelectionCriteria.RateCode);

			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.CertificateOfOriginCountry = "ZA";
			rateSelectionCriteria = invoiceLine.GetSpecificRateSelectionCriteria(Universal.Constants.RateTypes.Duty, Constants.UniversalReferenceConstants.RefCusRateCodes.CustomsDuty, Constants.PrimaryPreferenceCodes.MostFavouredNations, ZString.Empty);
			AssertEquals("TradeGroupCountry should return CertificateOfOriginCountry", "ZA", rateSelectionCriteria.TradeGroupCountry);
		}

		public void TestIsDangerousChemical()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("CN", RefCusCodeListTypes.Codes.CNDangerousChemical, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateCusCodeListAttribute(codeList.PK, "Alias", "液氨");
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "China Customs Tariff Additional Elements");
			helper.CreateNewOrGetExistingCusCodeList("CN", RefCusCodeListTypes.Codes.CNAdditionalElements, "00005", "CAS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute("AdditionalInfo1", "00005", tariff);
			anotherFactory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.XC_GoodsSpecModel = "7664-41-7";
			Assert(invoiceLine.GoodsIsDangerousChemical);

			invoiceLine.XC_GoodsSpecModel = "";
			invoiceLine.JI_NameOfGoods = "氨";
			Assert(invoiceLine.GoodsIsDangerousChemical);

			invoiceLine.JI_NameOfGoods = "test";
			Assert(!invoiceLine.GoodsIsDangerousChemical);

			invoiceLine.JI_NameOfGoods = "液氨";
			Assert(invoiceLine.GoodsIsDangerousChemical);

			invoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "2008", "c", "IMO").First().PK;
			Assert(!invoiceLine.JI_NonDangerousChemicalFlag);
		}

		public override void TestCustomsQtyCalculatedByNetWeightOfProductWhenInvoiceQtySet()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
				Factory.Save();

				var tariff = helper.CreateTariff("CN", hsnTariffType.PK, "0000000090", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				helper.CreateTariffUOM(tariff.PK, "CU1", "035");
				Factory.Save();

				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var supplier = Factory.New<OrgHeader>();
				InvoiceHeader.JZ_OH_Supplier = supplier.PK;

				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "TESTTEST1";
				product1.OP_StockKeepingUnit = "NO";
				product1.OP_NetWeight = 3m;
				product1.OP_WeightUQ = "KG";//3 kg per NO

				var relation1 = product1.RelatedOrganisations.AddNew();
				relation1.OU_OH = supplier.PK;
				relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				product1.PivotsForBinding.AddNew();

				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "TESTTEST2";
				product2.OP_StockKeepingUnit = "NO";
				product2.OP_NetWeight = 4m;
				product2.OP_WeightUQ = "KG";//4 kg per NO

				var relation2 = product2.RelatedOrganisations.AddNew();
				relation2.OU_OH = supplier.PK;
				relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				product2.PivotsForBinding.AddNew();

				InvoiceLine.JI_Tariff = "0000000090";
				InvoiceLine.JI_CustomsUnitQty = "KG";
				InvoiceLine.JI_PartNo = product1.OP_PartNum;
				InvoiceLine.JI_InvoiceUQ = "NO";
				InvoiceLine.JI_InvoiceQuantity = 18m;

				var invoiceline2 = InvoiceLine.InvoiceHeader.InvoiceLines.AddNew();
				invoiceline2.JI_Tariff = "0000000090";
				invoiceline2.JI_CustomsUnitQty = "KG";
				invoiceline2.JI_PartNo = product2.OP_PartNum;
				invoiceline2.JI_InvoiceUQ = "NO";
				invoiceline2.JI_InvoiceQuantity = 18m;

				AssertEquals(54m, InvoiceLine.JI_NetWeight);
				AssertEquals(72m, invoiceline2.JI_NetWeight);
				AssertEquals(54m, InvoiceLine.JI_CustomsQuantity);
				AssertEquals(72m, invoiceline2.JI_CustomsQuantity);
			}
		}

		public void TestNameOfGoodsSeemsTobeUsedProduct()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertEquals("NameOfGoodsSeemsTobeUsedProduct should be false for JI_NameOfGoods empty ", false, invoiceLine.NameOfGoodsSeemsTobeUsedProduct);

			invoiceLine.JI_NameOfGoods = "液氨";
			AssertEquals("NameOfGoodsSeemsTobeUsedProduct should be false for JI_NameOfGoods don't contain '旧'", false, invoiceLine.NameOfGoodsSeemsTobeUsedProduct);

			invoiceLine.JI_NameOfGoods = "旧轮胎";
			AssertEquals("NameOfGoodsSeemsTobeUsedProduct should be true for JI_NameOfGoods 旧轮胎 ", true, invoiceLine.NameOfGoodsSeemsTobeUsedProduct);
		}

		[TestDate(2018, 12, 12)]
		public void TestDefaultOriginState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var today = ZDateTime.Today;
			helper.CreateCusMapType(RefCusCodeListTypes.Codes.CNCIQStates, "OUT", "CIQ State Mapping", true);
			helper.CreateCusMap(RefCusCodeListTypes.Codes.CNCIQStates, "TN22", "788067", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 30), Core.Constants.CountryCodes.China);
			helper.CreateCusMap(RefCusCodeListTypes.Codes.CNCIQStates, "UYAR", "858001", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 30), Core.Constants.CountryCodes.China);
			Factory.Save();

			InvoiceLine.JI_CountryOfOrigin = "TN";
			InvoiceLine.JI_StateOrRegionOfOrigin = "22";
			AssertEquals("Origin State is defaulted.", "788067", InvoiceLine.JI_CIQOriginState);

			InvoiceLine.JI_CountryOfOrigin = "UY";
			InvoiceLine.JI_StateOrRegionOfOrigin = "AR";
			AssertEquals("Origin State is defaulted.", "858001", InvoiceLine.JI_CIQOriginState);

			InvoiceLine.JI_StateOrRegionOfOrigin = "AU";
			AssertEquals("Origin State is defaulted with the ISO code of Goods Origin.", InvoiceLine.CountryOfOrigin.RN_IsoNumericUNM49Code, InvoiceLine.JI_CIQOriginState);
		}

		public void TestDefaultJI_SecondaryPreference()
		{
			Helper.CreateNewOrGetExistingCusCodeType("CNPTA", "CN Prefential Trade Agreement");
			var cnpta_01 = Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "01", "01", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var cnpta_02 = Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "02", "02", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_01.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.Australia);
			Helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_02.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = dec.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = "US";
			invoiceLine1.JI_PrimaryPreference = ZString.Empty;
			invoiceLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			AssertEquals("EffectiveCountryOfOrigin US, No matching CNPTA code, JI_SecondaryPreference should not be defaulted.", ZString.Empty, invoiceLine1.JI_SecondaryPreference);

			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine1.CertificateOfOriginCountry = "US";
			invoiceLine1.JI_PrimaryPreference = ZString.Empty;
			invoiceLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			AssertEquals("EffectiveCountryOfOrigin AU, JI_SecondaryPreference should be defaulted.", "01", invoiceLine1.JI_SecondaryPreference);

			invoiceLine1.JI_SecondaryPreference = "";
			invoiceLine1.CertificateOfOrigin = "C001";
			AssertEquals("JI_SecondaryPreference should have been defaulted when CertificateOfOriginCountry is set.", "01", invoiceLine1.JI_SecondaryPreference);
		}

		public void TestJI_CustomsValue()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice1 = dec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_RX_NKInvoice_Currency = "CNY";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			var charge = invoiceLine1.Charges.AddNew();
			charge.J7_ChargeType = "OTH";
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = false;
			charge.J7_Amount = 10m;

			AssertEquals(10010m, invoiceLine1.JI_CustomsValue);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(10000m, invoiceLine1.JI_CustomsValue);
		}

		public void TestFormulaPricingRecordNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			AssertEquals("FormulaPricingRecordNumber should be empty", ZString.Empty, invoiceLine.FormulaPricingRecordNumber);
			var jobComInvLineRefs = invoiceLine.InvoiceLineRefs.AddNew();
			var jobComInvLineRefsPk = jobComInvLineRefs.PK;
			jobComInvLineRefs.JG_ReferenceNumber = "999";
			jobComInvLineRefs.JG_ReferenceType = ZString.Empty;
			var jobComInvLineRefsLoaded = Factory.Load<JobComInvLineRefs>(jobComInvLineRefsPk);
			AssertNotNull("JobComInvLineRefs record should not be null", jobComInvLineRefsLoaded);
			AssertEquals("FormulaPricingRecordNumber should be empty", ZString.Empty, invoiceLine.FormulaPricingRecordNumber);
			jobComInvLineRefs.JG_ReferenceType = Constants.JobComInvLineRefType.FormulaPricingRecordNumber;
			AssertEquals("FormulaPricingRecordNumber should be 999", "999", invoiceLine.FormulaPricingRecordNumber);
			invoiceLine.FormulaPricingRecordNumber = ZString.Empty;
			jobComInvLineRefsLoaded = Factory.Load<JobComInvLineRefs>(jobComInvLineRefsPk);
			AssertEquals("FormulaPricingRecordNumber should be empty", ZString.Empty, invoiceLine.FormulaPricingRecordNumber);
			AssertNull("JobComInvLineRefs record should be deleted", jobComInvLineRefsLoaded);
			invoiceLine.FormulaPricingRecordNumber = "999";
			AssertEquals("FormulaPricingRecordNumber should be 999", "999", invoiceLine.FormulaPricingRecordNumber);
		}

		public void TestDefaultCusSupportingDocumentsAfterSetTariff()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var condType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.China, "CTRL", "CNDOC");
			var condValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.China, "TSTVT");
			var testCondCtrlImport = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.China, condType.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var testCondCtrlExport = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.China, condType.PK, tariff.PK, "Direction:Export", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(condValueType.PK, testCondCtrlImport.PK, "0001");
			helper.CreateOrGetExistingRefCusConditionValue(condValueType.PK, testCondCtrlExport.PK, "0002");
			helper.CreateOrGetExistingRefCusConditionValue(condValueType.PK, testCondCtrlExport.PK, "0003");

			anotherFactory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var inst = dec.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = CNRefCusProcedure.Codes._0110;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 100m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = inst.PK;
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_Tariff = "8476900000";

			AssertEquals("UniversalTariff not null", tariff.PK, invoiceLine.UniversalTariff.PK);

			var supportingDocuments = invoiceLine.CusSupportingDocuments.Cast<CusSupportingDocument>();
			AssertEquals("Default from the only CusConditionValue", 1, supportingDocuments.Count());
			AssertEquals("Default CSI_Code", "0001", supportingDocuments.First().CSI_Code);
			AssertEquals("CSI_ReferenceNumber", ZString.Empty, supportingDocuments.First().CSI_ReferenceNumber);
			supportingDocuments.First().CSI_ReferenceNumber = "D0100000001";

			JobComInvoiceLine invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = inst.PK;
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_Tariff = "8476900000";

			var supportingDocuments2 = invoiceLine2.CusSupportingDocuments.Cast<CusSupportingDocument>();
			AssertEquals("Default from the only CusConditionValue", 1, supportingDocuments2.Count());
			AssertEquals("Default CSI_Code", "0001", supportingDocuments2.First().CSI_Code);
			AssertEquals("Default CSI_ReferenceNumber", "D0100000001", supportingDocuments2.First().CSI_ReferenceNumber);

			dec.JE_MessageType = "EXP";
			var inst2 = dec.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = CNRefCusProcedure.Codes._3339;
			var invoiceHeader2 = dec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine3 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = inst2.PK;
			invoiceLine3.JI_LinePrice = 100m;

			var supportingDocument = invoiceLine3.CusSupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "0003";
			supportingDocument.CSI_ReferenceNumber = "D0100000003";

			JobComInvoiceLine invoiceLine4 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine4.JI_CEI = inst2.PK;
			invoiceLine4.JI_LinePrice = 100m;
			invoiceLine4.JI_Tariff = "8476900000";

			var supportingDocuments4 = invoiceLine4.CusSupportingDocuments.Cast<CusSupportingDocument>();
			AssertEquals("Default from the only CusConditionValue", 1, supportingDocuments4.Count());
			AssertEquals("Default CSI_Code", "0003", supportingDocuments4.First().CSI_Code);
			AssertEquals("Default CSI_ReferenceNumber", "D0100000003", supportingDocuments4.First().CSI_ReferenceNumber);

			var inst3 = dec.CustomsEntryInstructions.AddNew();
			inst3.CEI_Style = CNRefCusProcedure.Codes._2025;

			JobComInvoiceLine invoiceLine5 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine5.JI_CEI = inst3.PK;
			invoiceLine5.JI_LinePrice = 100m;
			invoiceLine5.JI_Tariff = "8476900000";

			var supportingDocument2 = invoiceLine5.CusSupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "0003";
			supportingDocument2.CSI_ReferenceNumber = "D0100000003";

			JobComInvoiceLine invoiceLine6 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine6.JI_CEI = inst3.PK;
			invoiceLine6.JI_LinePrice = 100m;
			invoiceLine6.JI_Tariff = "8476900000";

			var supportingDocuments5 = invoiceLine6.CusSupportingDocuments.Cast<CusSupportingDocument>();
			AssertEquals("Will not default when CEI_Style is not 0110, 3339", 0, supportingDocuments5.Count());
		}

		public void TestCalculateAmountBasedOnPercentage()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				var dec = Factory.New<JobDeclaration>();
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 100m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";
				invoiceLine.JI_LinePrice = 100m;

				var charge = invoiceHeader.Charges.AddNew();
				charge.J7_ChargeType = "OFT";
				charge.J7_Amount = 10m;
				charge.J7_RX_NKCurrency = "CNY";
				charge = invoiceHeader.Charges.AddNew();
				charge.J7_ChargeType = "ONS";
				charge.J7_Percentage = 0.3m;

				Factory.Save();

				AssertEquals(0.33m, charge.J7_Amount);
				var apportionedCharge = invoiceLine.ApportionedCharges.Cast<BaseInvoiceLineApportionedCharge>().First(x => x.J7_ChargeType == "ONS");
				AssertNotNull(apportionedCharge);
				AssertEquals(0.33m, apportionedCharge.J7_Amount);
				AssertEquals(0.33m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);
				AssertEquals(110.33m, invoiceLine.JI_Calc_CIF);

				invoiceHeader.JZ_IncoTerm = "CIF";
				Factory.Save();
				AssertEquals(0.33m, charge.J7_Amount);

				charge.J7_ChargeType = "OFT";
				Factory.Save();
				AssertEquals(0.3m, charge.J7_Amount);
			}
		}

		public void TestCusSupportingDocumentCollection()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertEquals(0, invoiceLine.CusSupportingDocuments.Count);

			invoiceLine.CusSupportingDocuments.AddNew();
			AssertEquals(1, invoiceLine.CusSupportingDocuments.Count);

			invoiceLine.CusSupportingDocuments.RemoveAndDeleteAll();
			AssertEquals(0, invoiceLine.CusSupportingDocuments.Count);
			AssertEquals(0, invoiceLine.CusSupportingDocuments.Count);
		}

		public void TestFilteredCusSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.CusSupportingDocuments.AddNew().CSI_Code = "1Y";
			invoiceLine.CusSupportingDocuments.AddNew().CSI_Code = "1A";
			invoiceLine.CusSupportingDocuments.AddNew().CSI_Code = "01";
			AssertEquals(3, invoiceLine.CusSupportingDocuments.Count);
			AssertEquals(2, invoiceLine.FilteredCusSupportingDocuments.Count);

			var difference = invoiceLine.CusSupportingDocuments.Except(invoiceLine.FilteredCusSupportingDocuments);
			AssertEquals("1Y", difference.Cast<CusSupportingDocument>().First().CSI_Code);
		}

		public void TestCusSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = InvoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var collection = invoiceLine.CusSupportingDocuments;
			AssertEquals(0, collection.Count);
			var newDoc = collection.AddNew();
			AssertEquals(1, collection.Count);
			AssertEquals(Constants.CusSupportingInfoTypes.CusSupportingDocument, newDoc.CSI_Type);
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("EffectiveCountryOfOrigin falls back to JI_CountryOfOrigin", "US", invoiceLine.EffectiveCountryOfOrigin);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.CertificateOfOriginCountry = "ZA";
			AssertEquals("EffectiveCountryOfOrigin returns CertificateOfOriginCountry", "ZA", invoiceLine.EffectiveCountryOfOrigin);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.MostFavouredNations;
			AssertEquals("EffectiveCountryOfOrigin returns CertificateOfOriginCountry", "US", invoiceLine.EffectiveCountryOfOrigin);
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			MasterFiles.Business.OrgSupplierPart product = (MasterFiles.Business.OrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			GlbCompany cNCompany = Factory.New<GlbCompany>();
			cNCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			GlbBranch cNBranch = cNCompany.Branches.AddNew();
			cNBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.China)).RL_Code;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = cNBranch.PK;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			JobDeclaration declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the type", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseJobComInvoiceLine to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestJI_FormattedTariff()
		{
			ZString tariff = "12345678";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78", InvoiceLine.JI_FormattedTariff);
			tariff = "9876 .54 .32 10";
			InvoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", "9876.54", InvoiceLine.JI_FormattedTariff);
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;

		RefCusTariffType TrfType => trfType ?? (trfType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN"));
		RefCusTariffType trfType;
		CusRefPreferenceView ftaCode;
		CusRefPreferenceView ldcCode;
		CusRefPreferenceView mfnCode;
		CusRefPreferenceView normalCode;

		public void TestJI_TariffCalculateCustomsQtyFromTradeQty()
		{
			var tariffType = TrfType;
			Factory.Save();

			var tariff1 = helper.CreateTariff("CN", tariffType.PK, "0000000010", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1.PK, "CU1", "030");

			var tariff2 = helper.CreateTariff("CN", tariffType.PK, "0000000020", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff2.PK, "CU1", "001");
			helper.CreateTariffUOM(tariff2.PK, "CU2", "035");

			var tariff3 = helper.CreateTariff("CN", tariffType.PK, "0000000030", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff3.PK, "CU1", "035");
			helper.CreateTariffUOM(tariff3.PK, "CU2", "001");

			var tariff4 = helper.CreateTariff("CN", tariffType.PK, "0000000040", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff4.PK, "CU1", "036");
			helper.CreateTariffUOM(tariff4.PK, "CU2", "035");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_InvoiceUQ = string.Empty;
			invoiceLine.JI_CustomsUnitQty = string.Empty;
			invoiceLine.JI_CustomsSecondUnitQty = string.Empty;

			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals(ZString.Empty, invoiceLine.JI_InvoiceUQ);
			AssertEquals("030", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("030", invoiceLine.JI_TradeUnitQty);
			AssertEquals("", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals("001", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("035", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			AssertEquals("035", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("001", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_Tariff = tariff4.ZZ1_TariffCode;
			AssertEquals("036", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("035", invoiceLine.JI_CustomsSecondUnitQty);

			invoiceLine.JI_InvoiceUQ = "BAG";
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			AssertEquals("001", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("035", invoiceLine.JI_CustomsSecondUnitQty);
		}

		public void TestTypeCustomsUnitDefaultingStrategy()
		{
			var invoiceline = Factory.New<JobComInvoiceLineForTest>();
			var strategy = invoiceline.GetCustomsUnitDefaultingStrategyExposed();

			AssertType<TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>>("GetCustomsUnitDefaultingStrategy should return TariffCustomsUnitDefaultingStrategy", strategy);
		}

		public void TestJI_CIQCountryOfOrigin()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_ExportDate = new ZDateTime(2018, 7, 19);

			testItems.InvoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("USA", testItems.InvoiceLine.JI_CIQCountryOfOrigin);

			testItems.InvoiceLine.JI_CountryOfOrigin = "GB";
			AssertEquals("GBR", testItems.InvoiceLine.JI_CIQCountryOfOrigin);
		}

		public void TestUpdateFromPivot_JI_TradeUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("1010101011", "00000", "00423", "00352", "99999");
			var typeCUSUQ = helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			var unit1 = helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "001", "Unit1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			var unit2 = helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "002", "Unit2", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			var unit125 = helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "125", "Unit125", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CNCusEntryHeaderHelper.CreateNewProduct(Factory, supplier, "NEWPROD10", "PRODUCT10");
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OH = supplier.PK;
			pivot.CI_TariffNum = "1010101011";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_TradeUnitQty = "001";
			invoiceLine.JI_PartNo = part.OP_PartNum;

			AssertEquals("Empty value should not be copied from Pivot", "001", invoiceLine.JI_TradeUnitQty);

			invoiceLine.JI_PartNo = ZString.Empty;
			pivot.CNC_TradeUnitQty = "002";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should have copied from Pivot if JI_TradeUnitQty has value", "002", invoiceLine.JI_TradeUnitQty);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_TradeUnitQty = "";
			invoiceLine.JI_InvoiceUQ = invoiceLine.PartStockTakeUnit;
			pivot.CNC_TradeUnitQty = "";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should default tradeUQ from JI_InvoiceUQ if  pivot.CNC_TradeUnitQty has no value", "125", invoiceLine.JI_TradeUnitQty);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_TradeUnitQty = "";
			invoiceLine.JI_InvoiceUQ = "";
			invoiceLine.JI_CustomsUnitQty = "004";
			pivot.CNC_TradeUnitQty = "";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should default JI_InvoiceUQ when setting partNo", "125", invoiceLine.JI_TradeUnitQty);
			AssertEquals("Should default tradeUQ JI_InvoiceUQ if  pivot.CNC_TradeUnitQty has no value", "125", invoiceLine.JI_TradeUnitQty);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_TradeUnitQty = "";
			invoiceLine.JI_InvoiceUQ = "003";
			invoiceLine.JI_CustomsUnitQty = "004";
			pivot.CNC_TradeUnitQty = "002";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should have copied from Pivot as long as  pivot.CNC_TradeUnitQty has value", "002", invoiceLine.JI_TradeUnitQty);

			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			pivot.CNC_RX_NKTradeUnitPriceCurrency = "CNY";
			pivot.CNC_TradeUnitPrice = 9.99m;
			invoiceLine.JI_LinePrice = 1.23m;
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("CNC_TradeUnitPrice 0, JI_LinePrice not 0, Currency not equal, should not copy TradeUnitPrice.", 0m, invoiceLine.TradeUnitPrice);
			AssertEquals("CNC_TradeUnitPrice 0, JI_LinePrice not 0, Currency not equal, should not calculate JI_LinePrice.", 1.23m, invoiceLine.JI_LinePrice);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("JI_LinePrice not 0, Currency not equal, should not copy TradeUnitPrice.", 0m, invoiceLine.TradeUnitPrice);
			AssertEquals("JI_LinePrice not 0, Currency not equal, should not calculate JI_LinePrice.", 1.23m, invoiceLine.JI_LinePrice);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Currency not equal, should not copy TradeUnitPrice.", 0m, invoiceLine.TradeUnitPrice);
			AssertEquals("Currency not equal, should not calculate JI_LinePrice.", 0m, invoiceLine.JI_LinePrice);

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";
			invoiceLine.JI_TradeQuantity = 2.00m;
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertEquals("Should copy TradeUnitPrice.", 9.99m, invoiceLine.TradeUnitPrice);
			AssertEquals("Should calculate JI_LinePrice.", 19.98m, invoiceLine.JI_LinePrice);
		}

		public void TestUpdateTotalPriceOrUnitpriceWhenUpdatingQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_LinePrice = 0;
			invoiceLine1.TradeUnitPrice = 1;
			invoiceLine1.JI_TradeQuantity = 10;
			AssertEquals("Unit price should not be updated because total price is zero.", 1m, invoiceLine1.TradeUnitPrice);
			AssertEquals("Should calculate total price when Unit Price not zero.", 10m, invoiceLine1.JI_LinePrice);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_LinePrice = 100;
			invoiceLine2.JI_TradeQuantity = 20;
			AssertEquals("Total Price should not be updated.", 100m, invoiceLine2.JI_LinePrice);
			AssertEquals("Unit Price should be cauculated.", 5m, invoiceLine2.TradeUnitPrice);
		}

		public void TestUpdateFromPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("1010101011", "00000", "00423", "00352", "99999");

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CNCusEntryHeaderHelper.CreateNewProduct(Factory, supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OH = supplier.PK;
			pivot.CI_TariffNum = "1010101011";
			var manufacturer1 = Factory.New<OrgHeader>();
			manufacturer1.OH_Code = "M1";
			var manufacturerAddr1 = manufacturer1.Addresses.AddNew();
			manufacturerAddr1.OA_Address1 = "M1 Addr1";
			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_Code = "M2";
			var manufacturerAddr2 = manufacturer2.Addresses.AddNew();
			manufacturerAddr2.OA_Address1 = "M2 Addr1";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoiceHeader = declaration.Invoices.AddNew();

			#region Original value for Invoice Line

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			invoiceLine.JI_StateOrRegionOfOrigin = "CNX";
			invoiceLine.JI_CIQOriginState = "200001";
			invoiceLine.JI_OriginDistrict = "10001";

			invoiceLine.JI_OriginRegion = "20001";
			invoiceLine.JI_DestinationDistrict = "10002";
			invoiceLine.JI_DestinationRegion = "20002";
			invoiceLine.JI_CIQTariff = "100000010";
			var collectionParent = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes);
			collectionParent.OptionCollection.SelectedCodes = new List<ZString>
			{
				"18",
				"23",
				"25",
				"30"
			};
			collectionParent.OptionCollection.RefreshSelectionCollection();

			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddr1.PK;
			invoiceLine.JI_NDescription = "SPEC";
			invoiceLine.JI_Model = "MODEL";
			invoiceLine.JI_BrandName = "BRAND";
			invoiceLine.JI_CIQEndUse = "23";

			invoiceLine.CIQIngredient = "IngredientNote";

			invoiceLine.JI_PackageTypeOfUNDG = UNDGPackageTypeList.Codes._1A1;
			invoiceLine.JI_NonDangerousChemicalFlag = false;
			invoiceLine.JI_CIQQualityGuaranteePeriod = 99;

			#endregion

			invoiceLine.JI_PartNo = part.OP_PartNum;

			#region Assert not copy from empty Pivot

			AssertEquals("Empty value should not be copied from Pivot", "CN", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("Empty value should not be copied from Pivot", "US", invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Empty value should not be copied from Pivot", "CNX", invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("Empty value should not be copied from Pivot", "200001", invoiceLine.JI_CIQOriginState);
			AssertEquals("Empty value should not be copied from Pivot", "10001", invoiceLine.JI_OriginDistrict);

			AssertEquals("Empty value should not be copied from Pivot", "20001", invoiceLine.JI_OriginRegion);
			AssertEquals("Empty value should not be copied from Pivot", "10002", invoiceLine.JI_DestinationDistrict);
			AssertEquals("Empty value should not be copied from Pivot", "20002", invoiceLine.JI_DestinationRegion);
			AssertEquals("Empty value should not be copied from Pivot", "100000010", invoiceLine.JI_CIQTariff);
			AssertSequencesEqual("Empty value should not be copied from Pivot", new[] { "18", "23", "25", "30" }, invoiceLine.CargoAttributes.Cast<CargoAttribute>().Select(code => code.CY_Code.ToString()));

			AssertEquals("Empty value should not be copied from Pivot", manufacturerAddr1.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("Empty value should not be copied from Pivot", "SPEC", invoiceLine.JI_NDescription);
			AssertEquals("Empty value should not be copied from Pivot", "MODEL", invoiceLine.JI_Model);
			AssertEquals("Empty value should not be copied from Pivot", "BRAND", invoiceLine.JI_BrandName);
			AssertEquals("Empty value should not be copied from Pivot", "23", invoiceLine.JI_CIQEndUse);

			AssertEquals("Empty value should not be copied from Pivot", "IngredientNote", invoiceLine.CIQIngredient);

			AssertEquals("Empty value should not be copied from Pivot", UNDGPackageTypeList.Codes._1A1, invoiceLine.JI_PackageTypeOfUNDG);
			AssertEquals("Empty value should not be copied from Pivot", false, invoiceLine.JI_NonDangerousChemicalFlag);
			AssertEquals("Empty value should not be copied from Pivot", 99, invoiceLine.JI_CIQQualityGuaranteePeriod);

			#endregion

			invoiceLine.JI_PartNo = ZString.Empty;

			#region Give value to Pivot

			pivot.CI_RN_NKCountryOfOrigin = "GB";
			pivot.CI_RN_NKCountryOfExport = "DE";
			pivot.CI_RW_NKOriginState = "CNY";
			pivot.CNC_OriginState = "210001";
			pivot.CNC_OriginDistrict = "11001";

			pivot.CNC_OriginRegion = "21001";
			pivot.CNC_DestinationDistrict = "11002";
			pivot.CNC_DestinationRegion = "21002";
			pivot.CNC_CIQTariff = "110000010";
			var pivotParent = new CodeDescriptionOptionCollectionParent(Factory, pivot.CargoAttributes);
			pivotParent.OptionCollection.SelectedCodes = new List<ZString>
			{
				"19",
				"24",
				"26",
				"30"
			};
			pivotParent.OptionCollection.RefreshSelectionCollection();

			pivot.CNC_OA_ManufacturerAddress = manufacturerAddr2.PK;
			pivot.CI_NDescription = "SPEC2";
			pivot.CNC_Model = "MODEL2";
			pivot.CNC_Brand = "BRAND2";
			pivot.CNC_EndUse = "24";

			pivot.CIQIngredient = "IngredientNote2";

			pivot.CNC_UNPackageMarking = UNDGPackageTypeList.Codes._1B1;
			pivot.CNC_NonDangerousChemicalFlag = true;
			pivot.CNC_QualityGuaranteePeriod = 100;

			part.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			#endregion

			invoiceLine.JI_PartNo = part.OP_PartNum;

			#region Assert copy from Pivot

			AssertEquals("Should have copied from Pivot", "GB", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("Should have copied from Pivot", "DE", invoiceLine.JI_RN_NKCountryOfExport);
			AssertEquals("Should have copied from Pivot", "CNY", invoiceLine.JI_StateOrRegionOfOrigin);
			AssertEquals("Should have copied from Pivot", "210001", invoiceLine.JI_CIQOriginState);
			AssertEquals("Should have copied from Pivot", "11001", invoiceLine.JI_OriginDistrict);

			AssertEquals("Should have copied from Pivot", "21001", invoiceLine.JI_OriginRegion);
			AssertEquals("Should have copied from Pivot", "11002", invoiceLine.JI_DestinationDistrict);
			AssertEquals("Should have copied from Pivot", "21002", invoiceLine.JI_DestinationRegion);
			AssertEquals("Should have copied from Pivot", "110000010", invoiceLine.JI_CIQTariff);
			AssertSequencesEqual("Should have copied from Pivot", new[] { "19", "24", "26", "30" }, invoiceLine.CargoAttributes.Cast<CargoAttribute>().Select(code => code.CY_Code.ToString()).OrderBy(c => c));

			AssertEquals("Should have copied from Pivot", manufacturerAddr2.PK, invoiceLine.JI_OA_ManufacturerAddress);
			AssertEquals("Should have copied from Pivot", "SPEC2", invoiceLine.JI_NDescription);
			AssertEquals("Should have copied from Pivot", "MODEL2", invoiceLine.JI_Model);
			AssertEquals("Should have copied from Pivot", "BRAND2", invoiceLine.JI_BrandName);
			AssertEquals("Should have copied from Pivot", "24", invoiceLine.JI_CIQEndUse);

			AssertEquals("Should have copied from Pivot", "IngredientNote2", invoiceLine.CIQIngredient);

			AssertEquals("Should have copied from Pivot", UNDGPackageTypeList.Codes._1B1, invoiceLine.JI_PackageTypeOfUNDG);
			AssertEquals("Should have copied from Pivot", true, invoiceLine.JI_NonDangerousChemicalFlag);
			AssertEquals("Should have copied from Pivot", 100, invoiceLine.JI_CIQQualityGuaranteePeriod);

			AssertEquals("Should have copied from Pivot", "0004a", invoiceLine.DangerousGoods.UNDGSubstance.DG_Code);

			#endregion

			invoiceLine.JI_PartNo = ZString.Empty;
			declaration.JE_MessageSubType = "CUS";
			pivot.CNC_OriginDistrict = "12001";
			pivot.CNC_OriginRegion = "22001";

			invoiceLine.JI_PartNo = part.OP_PartNum;
			Assert("Should NOT copy JI_OriginDistrict when WillGenerateExitingEntry is false", invoiceLine.JI_OriginDistrict != "11001");
			Assert("Should NOT copy JI_OriginRegion when WillGenerateExitingEntry is false", invoiceLine.JI_OriginRegion != "22001");

			invoiceLine.JI_PartNo = ZString.Empty;
			declaration.JE_MessageType = "EXP";
			pivot.CI_RW_NKOriginState = "CNZ";
			pivot.CNC_OriginState = "220001";
			pivot.CNC_DestinationDistrict = "12002";
			pivot.CNC_DestinationRegion = "22002";

			invoiceLine.JI_PartNo = part.OP_PartNum;
			Assert("Should NOT copy JI_StateOrRegionOfOrigin when WillGenerateEnteringEntry is false", invoiceLine.JI_StateOrRegionOfOrigin != "CNZ");
			Assert("Should NOT copy JI_CIQOriginState when WillGenerateEnteringEntry is false", invoiceLine.JI_CIQOriginState != "220001");
			Assert("Should NOT copy JI_DestinationDistrict when WillGenerateEnteringEntry is false", invoiceLine.JI_DestinationDistrict != "12002");
			Assert("Should NOT copy JI_DestinationRegion when WillGenerateEnteringEntry is false", invoiceLine.JI_DestinationRegion != "22002");
		}

		public void TestUpdateAdditionalInfomationsFromPivot()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("1010101011", "00000", "00423", "00352", "99999");

			var additionalElement1 = helper.CreateAdditionalElement("00000", "品名");
			var additionalElement2 = helper.CreateAdditionalElement("00423", "针入度");
			var additionalElement3 = helper.CreateAdditionalElement("00352", "加工方法");
			var additionalElement6 = helper.CreateAdditionalElement("99999", "其他");

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CNCusEntryHeaderHelper.CreateNewProduct(Factory, supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.JE_OH_Supplier = supplier.PK;
			var header = declaration.Invoices.AddNew();

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			pivot.CI_OH = supplier.PK;
			pivot.CI_TariffNum = "1010101011";
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pivot.CI_RW_NKOriginState = "11";
			pivot.CI_RN_NKCountryOfExport = Core.Constants.CountryCodes.China;

			pivot.CNC_NameOfGoods = "品名";
			pivot.CNC_GoodsSpecModel = "X|Y|Z";

			var line = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line.Part);
			AssertEquals("JI_Tariff", "1010101011", line.JI_Tariff);
			AssertEquals("JI_NameOfGoods", "品名", line.JI_NameOfGoods);
			AssertEquals("XC_GoodsSpecModel", "X|Y|Z", line.XC_GoodsSpecModel);
			AssertEquals("JI_NameOfGoods2", "品名", line.JI_NameOfGoods2);
			AssertEquals("XC_GoodsSpecModel2", "X|Y|Z", line.XC_GoodsSpecModel2);

			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			line = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line.Part);
			AssertEquals("JI_Tariff", "1010101011", line.JI_Tariff);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			line = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line.Part);
			AssertEquals("JI_Tariff", "1010101011", line.JI_Tariff);
			AssertEquals("JI_NameOfGoods", "品名", line.JI_NameOfGoods);
			AssertEquals("XC_GoodsSpecModel", "X|Y|Z", line.XC_GoodsSpecModel);
			AssertEquals("JI_NameOfGoods2", ZString.Empty, line.JI_NameOfGoods2);
			AssertEquals("XC_GoodsSpecModel2", ZString.Empty, line.XC_GoodsSpecModel2);
		}

		public void TestUpdateAdditionalInfomationsFromPivot_CorrectGoodsSpecModel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("1010101011", "00000", "00422", "00069", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("99999", "其他");

			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CNCusEntryHeaderHelper.CreateNewProduct(Factory, supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.JE_OH_Supplier = supplier.PK;
			var header = declaration.Invoices.AddNew();

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_OH = supplier.PK;
			pivot.CI_TariffNum = "1010101011";
			pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pivot.CI_RW_NKOriginState = "11";
			pivot.CI_RN_NKCountryOfExport = Core.Constants.CountryCodes.China;

			pivot.CNC_NameOfGoods = "品名";
			pivot.CNC_GoodsSpecModel = "1|2|无其他";

			var line1 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line1.Part);
			AssertEquals("JI_Tariff", "1010101011", line1.JI_Tariff);
			AssertEquals("XC_GoodsSpecModel", "1|3|无其他", line1.XC_GoodsSpecModel);
			AssertEquals("XC_GoodsSpecModel2", "1|2|无其他", line1.XC_GoodsSpecModel2);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var line2 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line2.Part);
			AssertEquals("JI_Tariff", "1010101011", line2.JI_Tariff);
			AssertEquals("XC_GoodsSpecModel", "1|3|无其他", line2.XC_GoodsSpecModel);
			AssertEquals("XC_GoodsSpecModel2", "1|2|无其他", line2.XC_GoodsSpecModel2);

			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var line3 = header.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.JI_PartNo = part.OP_PartNum;

			AssertNotNull(line3.Part);
			AssertEquals("JI_Tariff", "1010101011", line3.JI_Tariff);
			AssertEquals("XC_GoodsSpecModel", "1|2|无其他", line3.XC_GoodsSpecModel);
			AssertEquals("XC_GoodsSpecModel2", "", line3.XC_GoodsSpecModel2);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			ICusCodeDataTypeSupporter supporter = Factory.New<JobComInvoiceLine>();
			AssertEquals(typeof(ProductionBatch), supporter.GetCusCodeDataTypes()[Constants.CusCodeDataTypes.Codes.CIQ]);
			AssertEquals(typeof(CargoAttribute), supporter.GetCusCodeDataTypes()[Constants.CusCodeDataTypes.Codes.CargoAttribute]);
		}

		public void TestUniveralTariffAndCIQTariff()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			anotherFactory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900090", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, ciqTariffType.PK, "8476900000999", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.China, ciqTariffType.PK, "8476900090999", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900080", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			anotherFactory.Save();
			var relationship1 = helper.CreateTariffRelationship(tariff3.PK, hsnTariffType.PK, "8476900000");
			anotherFactory.Save();

			InvoiceLine.JI_Tariff = "8476900000";
			AssertEquals("UniversalTariff", tariff1.PK, InvoiceLine.UniversalTariff.PK);
			InvoiceLine.JI_Tariff = "8476900090";
			AssertEquals("UniversalTariff", tariff2.PK, InvoiceLine.UniversalTariff.PK);
			InvoiceLine.JI_CIQTariff = "8476900000999";
			AssertEquals("CIQTariff", tariff3.PK, InvoiceLine.CIQTariff.PK);
			InvoiceLine.JI_CIQTariff = "8476900090999";
			AssertEquals("CIQTariff", tariff4.PK, InvoiceLine.CIQTariff.PK);

			var instruction = Declaration.CustomsEntryInstructions.AddNew();
			InvoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_CIQRequires = true;
			InvoiceLine.JI_Tariff = "8476900000";
			AssertEquals("8476900000999", InvoiceLine.JI_CIQTariff);
			InvoiceLine.JI_Tariff = "8476900001";
			AssertEquals("8476900000999", InvoiceLine.JI_CIQTariff);
			InvoiceLine.JI_Tariff = "8476900080";
			AssertEquals(ZString.Empty, InvoiceLine.JI_CIQTariff);
		}

		public void TestCleanupVINDataOnSaving()
		{
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var vin1 = InvoiceLine.VINDataCollection.AddNew();
			Factory.Save();
			AssertEquals("VIN Datas should NOT be deleted", 1, InvoiceLine.VINDataCollection.Count);
			Assert("VIN Data should NOT be deleted", !vin1.IsDeleted);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("VIN Datas should be deleted", 0, InvoiceLine.VINDataCollection.Count);
			Assert("VIN Data should be deleted", vin1.IsDeleted);
		}

		public void TestCleanUpNameOfGoodsAndSpecModel()
		{
			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			InvoiceLine.JI_NameOfGoods2 = "X";
			InvoiceLine.XC_GoodsSpecModel2 = "Y";
			Factory.Save();
			AssertEquals("JI_NameOfGoods2 should NOT be cleared", "X", InvoiceLine.JI_NameOfGoods2);
			AssertEquals("JI_NameOfGoods2 should NOT be cleared", "Y", InvoiceLine.XC_GoodsSpecModel2);

			Declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Factory.Save();
			AssertEquals("JI_NameOfGoods2 should be cleared", "", InvoiceLine.JI_NameOfGoods2);
			AssertEquals("JI_NameOfGoods2 should be cleared", "", InvoiceLine.XC_GoodsSpecModel2);
		}

		public void TestBatchNumberAndManufactureDates()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();

			var addInfo11 = invoiceLine.ProductionBatch.AddNew();
			addInfo11.CY_Data = "11111";
			addInfo11.CY_Date = ZDateTime.Today;
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addInfo11.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addInfo11.CY_Code);

			var addInfo12 = invoiceLine.ProductionBatch.AddNew();
			addInfo12.CY_Data = "22222";
			addInfo12.CY_Date = ZDateTime.Today.AddDays(2);
			var addInfo13 = invoiceLine.ProductionBatch.AddNew();
			addInfo13.CY_Data = "33333";
			addInfo13.CY_Date = ZDateTime.Today.AddDays(3);

			var addInfo14 = invoiceLine.ProductionBatch.AddNew();
			addInfo14.CY_Data = "44444";
			addInfo14.CY_Date = ZDateTime.Today.AddDays(4);
			addInfo14.CY_Type = "*";
			AssertEquals(4, invoiceLine.ProductionBatch.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(3, invoiceLine.ProductionBatch.Count);

			var addinfo1 = invoiceLine.ProductionBatch[0];
			AssertEquals(addInfo11.PK, addinfo1.PK);
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo1.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo1.CY_Code);
			AssertEquals("11111", addinfo1.CY_Data);
			AssertEquals(true, addinfo1.CY_Date == addInfo11.CY_Date);

			invoiceLine.ProductionBatch.RemoveAndDelete(addinfo1);
			AssertEquals(true, addinfo1.IsDeleted);
			AssertEquals(2, invoiceLine.ProductionBatch.Count);

			newFactory.Save();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			addinfo1 = newFactory.Load<ProductionBatch>(addinfo1.PK);
			AssertEquals(null, addinfo1);
			AssertEquals(2, invoiceLine.ProductionBatch.Count);

			var addinfo2 = invoiceLine.ProductionBatch[0];
			AssertEquals(addInfo12.PK, addinfo2.PK);
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo2.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo2.CY_Code);
			AssertEquals("22222", addinfo2.CY_Data);
			AssertEquals(false, invoiceLine.HasChanges);
			addinfo2.CY_Data = "11111";
			AssertEquals(true, invoiceLine.HasChanges);

			newFactory.Save();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(2, invoiceLine.ProductionBatch.Count);

			addinfo2 = invoiceLine.ProductionBatch[0];
			AssertEquals(addInfo12.PK, addinfo2.PK);
			AssertEquals(Constants.CusCodeDataTypes.Codes.CIQ, addinfo2.CY_Type);
			AssertEquals(Constants.CusCodeDataCode.BatchNumber, addinfo2.CY_Code);
			AssertEquals("11111", addinfo2.CY_Data);

			invoiceLine.ProductionBatch.RemoveAndDeleteAll();
			newFactory.Save();
			AssertEquals(0, invoiceLine.ProductionBatch.Count);

			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(0, invoiceLine.ProductionBatch.Count);
		}

		public void TestIngredientNote()
		{
			AssertHiddenTextNote(PredefinedNoteTypes.Instance.CustomsQuarantineIngredient.Description, (JobComInvoiceLine line, ZString value) => { line.CIQIngredient = value; return line.CIQIngredient; });
		}

		public void TestXC_GoodsSpecModel()
		{
			AssertHiddenTextNote("Goods Specification and Model", (JobComInvoiceLine line, ZString value) => { line.XC_GoodsSpecModel = value; return line.XC_GoodsSpecModel; });
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.XC_GoodsSpecModel), false, attr => attr.MaxLengthMember == nameof(JobComInvoiceLine.GoodsSpecModelMaxLength));
		}

		public void TestXC_GoodsSpecModel2()
		{
			AssertHiddenTextNote("Goods Specification and Model 2", (JobComInvoiceLine line, ZString value) => { line.XC_GoodsSpecModel2 = value; return line.XC_GoodsSpecModel2; });
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.XC_GoodsSpecModel2), false, attr => attr.MaxLengthMember == nameof(JobComInvoiceLine.GoodsSpecModelMaxLength));
		}

		public void TestGoodsSpecModelMaxLength()
		{
			AssertMaxLength(true, 1300);
			AssertMaxLength(false, 255);

			void AssertMaxLength(bool enableGoLiveDate, int expectedMaxLength)
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CNDecMessageV2020GoLiveDate, Core.Constants.CountryCodes.China, ZDateTime.Today, enableGoLiveDate))
				{
					AssertEquals(expectedMaxLength, Factory.New<JobComInvoiceLine>().GoodsSpecModelMaxLength);
				}
			}
		}

		void AssertHiddenTextNote(string expectedDescription, Func<JobComInvoiceLine, ZString, ZString> setPropertyValueThenGet)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var getterValue = setPropertyValueThenGet(invoiceLine, "4|0|A|B|C");
			AssertEquals("4|0|A|B|C", getterValue);
			AssertEquals("invoiceLine has Changes", true, invoiceLine.HasChanges);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(StmNoteSchema.ST_ParentID, invoiceLine.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, JobComInvoiceLineSchema.Constants.TableName);
			var stmNotes = newFactory.Load<StmNote>(query);
			AssertEquals(1, stmNotes.Length);
			var stmNote = stmNotes[0];
			AssertEquals(expectedDescription, stmNote.ST_Description);
			AssertEquals("DOC", stmNote.ST_NoteType);
			AssertEquals("AAA", stmNote.ST_NoteContext);

			invoiceLine.Delete();
			Factory.Save();
			AssertEquals("Test deltion of invoice line also deleted the hidden text note", true, stmNote.IsDeleted);

			newFactory = new BusinessObjectFactory();
			stmNote = newFactory.Load<StmNote>(stmNote.PK);
			AssertNull("Test reloading after deletion of invoice Line", stmNote);
		}

		public void TestCargoAttributesAsString()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();

			var code1 = Factory.New<CargoAttribute>();
			code1.CY_ParentTableCode = invoiceLine.TablePrefix;
			code1.CY_Code = "11";
			code1.CY_Type = "CAT";
			code1.CY_ParentID = invoiceLine.PK;

			var code2 = Factory.New<CargoAttribute>();
			code2.CY_ParentTableCode = invoiceLine.TablePrefix;
			code2.CY_Code = "14";
			code2.CY_Type = "CAT";
			code2.CY_ParentID = invoiceLine.PK;

			Factory.Save();

			AssertEquals("3C目录内,预包装", invoiceLine.CargoAttributesAsString);
		}

		public void TestEffectiveAssessmentDate()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 2, 1);
			AssertEquals("EffectiveAssessmentDate", new ZDateTime(2019, 2, 1), testItems.InvoiceLine.EffectiveAssessmentDate);

			testItems.EntryInstruction.CEI_DateForDuty = new ZDateTime(2019, 2, 25);
			AssertEquals("EffectiveAssessmentDate", new ZDateTime(2019, 2, 25), testItems.InvoiceLine.EffectiveAssessmentDate);
		}

		public void TestCIQRequires()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;

			AssertEquals(3, instruction.InvoiceLines.Length);
			AssertEquals(false, instruction.CEI_CIQRequires);
			AssertEquals(false, invoiceLine1.CIQRequires);
			AssertEquals(false, invoiceLine2.CIQRequires);
			AssertEquals(false, invoiceLine3.CIQRequires);

			invoiceLine1.JI_CIQTariff = "1111";
			instruction.CEI_CIQRequires = true;
			AssertEquals(true, instruction.CEI_CIQRequires);
			AssertEquals(true, invoiceLine1.CIQRequires);
			AssertEquals(true, invoiceLine2.CIQRequires);
			AssertEquals(true, invoiceLine3.CIQRequires);

			invoiceLine2.JI_CIQTariff = "2222";
			invoiceLine3.JI_CIQTariff = "3333";
			AssertEquals(true, instruction.CEI_CIQRequires);
			AssertEquals(true, invoiceLine1.CIQRequires);
			AssertEquals(true, invoiceLine2.CIQRequires);
			AssertEquals(true, invoiceLine3.CIQRequires);
		}

		public void TestIsEnteringOrExiting()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals(EnteringOrExiting.Entering, invoiceLine.IsEnteringOrExiting());
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals(EnteringOrExiting.Entering, invoiceLine.IsEnteringOrExiting());
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals(EnteringOrExiting.Entering, invoiceLine.IsEnteringOrExiting());
			AssertEquals(EnteringOrExiting.Exiting, invoiceLine.IsEnteringOrExiting(true));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals(EnteringOrExiting.Exiting, invoiceLine.IsEnteringOrExiting());
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals(EnteringOrExiting.Exiting, invoiceLine.IsEnteringOrExiting());
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals(EnteringOrExiting.Entering, invoiceLine.IsEnteringOrExiting());
			AssertEquals(EnteringOrExiting.Exiting, invoiceLine.IsEnteringOrExiting(true));
		}

		public void TestGetPartPivotTypeCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			AssertEquals(invoiceLine.GetPartPivotType(), "HTI");

			declaration.JE_MessageType = "EXP";
			AssertEquals(invoiceLine.GetPartPivotType(), "HTE");

			declaration.JE_MessageSubType = "BTH";
			AssertEquals(invoiceLine.GetPartPivotType(), "HTB");
		}

		public void TestGetCusAddInfoTypes()
		{
			var testItem = (ICusAddInfoTypeSupporter)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals(typeof(VINData), testItem.GetCusAddInfoTypes()["VID"]);
		}

		public void TestUniversalDutyRateFormula()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "EXP");
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			var expRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "EXP", expRateType.PK);
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountry("STANDARD", "STANDARD", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TARIFFA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "VAT");
			var aMFNRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.2", mfnPreference.PK, "20");
			helper.CreateCusApplicability(aMFNRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aSTDRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.9", stdPreference.PK, "90");
			helper.CreateCusApplicability(aSTDRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var expRate = helper.CreateRate(tariffA, expRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROUND(VFD / (1 + 0.4), 0) * 0.4", rateFormulaDeriveFrom: "40");
			helper.CreateCusApplicability(expRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Tariff = "TARIFFA";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			AssertEquals("UniversalDutyRateFormula", "90", invoiceLine.UniversalDutyRateFormula);
			invoiceLine.JI_PrimaryPreference = "MFN";
			AssertEquals("UniversalDutyRateFormula", "20", invoiceLine.UniversalDutyRateFormula);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("UniversalDutyRateFormula", ZString.Empty, invoiceLine.UniversalDutyRateFormula);
		}

		public void TestUniversalDutyRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			var tariffTypePK2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);

			var ldcPreference = helper.CreatePreferenceForCountry("LDC", "LDC", Core.Constants.CountryCodes.China);

			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "LDC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "LDC2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TARIFFA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "VAT");
			var aLDCRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.35", ldcPreference.PK, "35");
			helper.CreateCusApplicability(aLDCRate, tradeGroup1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aLDCRate2 = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.43", ldcPreference.PK, "43");
			helper.CreateCusApplicability(aLDCRate2, tradeGroup2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffB = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK2, "TARIFFB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "VAT");
			var aLDCRateB = helper.CreateRate(tariffB, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.5", ldcPreference.PK, "50");
			helper.CreateCusApplicability(aLDCRateB, tradeGroup2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_Tariff = "TARIFFA";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			invoiceLine.JI_PrimaryPreference = "LDC";

			declaration.DoMerge();

			AssertEquals("When there are more than one rates, choose the lower one", aLDCRate.PK, invoiceLine.UniversalDutyRate.PK);

			invoiceLine.JI_Tariff = "TARIFFB";
			AssertEquals("When there is only one rate, just choose it", aLDCRateB.PK, invoiceLine.UniversalDutyRate.PK);

			invoiceLine.JI_Tariff = "TARIFFC";
			AssertNull(invoiceLine.UniversalDutyRate);
		}

		public void TestTradeAgreementCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_SecondaryPreference = "10";

			invoiceLine.JI_PrimaryPreference = ftaCode.ZZS_Preference;
			AssertEquals("For FTA, TradeAgreementCode should be set to JI_SecondaryPreference", invoiceLine.JI_SecondaryPreference, invoiceLine.TradeAgreementCode);

			invoiceLine.JI_PrimaryPreference = ldcCode.ZZS_Preference;
			AssertEquals("For LDC, TradeAgreementCode should be set to 13", Constants.TradeAgreementCodes.Codes.LDC, invoiceLine.TradeAgreementCode);

			invoiceLine.JI_PrimaryPreference = mfnCode.ZZS_Preference;
			AssertEquals("TradeAgreementCode should be empty", ZString.Empty, invoiceLine.TradeAgreementCode);
		}

		public void TestCertificateOfOriginFields()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItem.JobDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = testItem.InvoiceLine;
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;

			AssertEquals("CertificateOfOrigin", ZString.Empty, invoiceLine.CertificateOfOrigin);
			AssertEquals("CertificateOfOriginCountry", ZString.Empty, invoiceLine.CertificateOfOriginCountry);
			AssertEquals("CertificateOfOriginType", ZString.Empty, invoiceLine.CertificateOfOriginType);
			AssertEquals("ItemNoOnCertOfOrigin", 0, invoiceLine.ItemNoOnCertOfOrigin);

			var supportingDocument1 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "0y";
			supportingDocument1.CSI_ReferenceNumber = "123456";
			supportingDocument1.CSI_RN_NKCountryCode = "DE";
			supportingDocument1.CSI_LineNo = 1;
			supportingDocument1.CSI_SubType = "D";

			var supportingDocument2 = invoiceLine.CusSupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "1Y";
			supportingDocument2.CSI_ReferenceNumber = "174401A1/000262";
			supportingDocument2.CSI_RN_NKCountryCode = "FR";
			supportingDocument2.CSI_LineNo = 2;
			supportingDocument2.CSI_SubType = "C";

			AssertEquals("CertificateOfOrigin", "174401A1/000262", invoiceLine.CertificateOfOrigin);
			AssertEquals("CertificateOfOriginCountry", "FR", invoiceLine.CertificateOfOriginCountry);
			AssertEquals("CertificateOfOriginType", "C", invoiceLine.CertificateOfOriginType);
			AssertEquals("ItemNoOnCertOfOrigin", 2, invoiceLine.ItemNoOnCertOfOrigin);

			invoiceLine.CusSupportingDocuments.RemoveAndDeleteAll();

			invoiceLine.CertificateOfOrigin = ZString.Empty;
			invoiceLine.CertificateOfOriginCountry = ZString.Empty;
			invoiceLine.CertificateOfOriginType = ZString.Empty;
			invoiceLine.ItemNoOnCertOfOrigin = 0;
			AssertNull("Should not create CertificateOfOriginDocument for empty value", invoiceLine.CertificateOfOriginDocument);

			invoiceLine.CertificateOfOrigin = "174401A1/000288";
			invoiceLine.CertificateOfOriginCountry = "US";
			invoiceLine.CertificateOfOriginType = "D";
			invoiceLine.ItemNoOnCertOfOrigin = 1;
			AssertNotNull("Should create CertificateOfOriginDocument", invoiceLine.CertificateOfOriginDocument);

			var certificateOfOriginDocument = invoiceLine.CertificateOfOriginDocument;
			AssertEquals("CertificateOfOrigin setting", "174401A1/000288", certificateOfOriginDocument.CSI_ReferenceNumber);
			AssertEquals("CertificateOfOriginCountry setting", "US", certificateOfOriginDocument.CSI_RN_NKCountryCode);
			AssertEquals("CertificateOfOriginType setting", "D", certificateOfOriginDocument.CSI_SubType);
			AssertEquals("ItemNoOnCertOfOrigin setting", 1, certificateOfOriginDocument.CSI_LineNo);

			invoiceLine.JI_CountryOfOrigin = "ZA";
			invoiceLine.CusSupportingDocuments.RemoveAndDeleteAll();
			AssertEquals("CertificateOfOriginCountry falls back to JI_CountryOfOrigin", "ZA", invoiceLine.CertificateOfOriginCountry);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.MostFavouredNations;
			invoiceLine.CertificateOfOrigin = "174401A1/000288";
			AssertNotNull("JI_PrimaryPreference MFN i.e. IsCertificateOfOriginApplicable false, CertificateOfOriginCountry should be deleted but not yet, but after saving.", invoiceLine.CertificateOfOriginCountry);
			Factory.Save();
			AssertNull("CertificateOfOriginDocument should have been deleted.", invoiceLine.CertificateOfOriginDocument);

			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.CertificateOfOriginType = "X";
			Factory.Save();
			var cusSupportingInfo = new BusinessObjectFactory { NameForDebugging = "TestCertificateOfOriginFields" }.LoadTop1<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.PK, invoiceLine.CertificateOfOriginDocument.PK)) as INeedRow;
			AssertEquals("CSI_ReferenceNumer should have been cleared when CertificateOfOriginType set X", ZString.Empty, cusSupportingInfo.Row["CSI_ReferenceNumber"]);
			AssertEquals("CSI_ItemNumber should have been cleared when CertificateOfOriginType set X", ZInt.Zero, cusSupportingInfo.Row["CSI_ItemNumber"]);
			AssertEquals("CertificateOfOrigin should return constant XJE00000 when CertificateOfOriginType set X", "XJE00000", invoiceLine.CertificateOfOrigin);
			AssertEquals("ItemNoOnCertOfOrigin should return EntryLine No. when CertificateOfOriginType set X", testItem.EntryLine.EntryLineNo, invoiceLine.ItemNoOnCertOfOrigin);
		}

		public void TestCertificateOfOriginFieldsReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceLine.JI_PrimaryPreference = "NORMAL";
			AssertEquals("CertificateOfOriginInfo", true, invoiceLine.CertificateOfOriginInfo.ReadOnly);
			AssertEquals("CertificateOfOriginCountryInfo", true, invoiceLine.CertificateOfOriginCountryInfo.ReadOnly);
			AssertEquals("CertificateOfOriginTypeInfo", true, invoiceLine.CertificateOfOriginTypeInfo.ReadOnly);
			AssertEquals("ItemNoOnCertOfOriginInfo", true, invoiceLine.ItemNoOnCertOfOriginInfo.ReadOnly);

			invoiceLine.JI_PrimaryPreference = "FTA";
			AssertEquals("CertificateOfOriginInfo", false, invoiceLine.CertificateOfOriginInfo.ReadOnly);
			AssertEquals("CertificateOfOriginCountryInfo", false, invoiceLine.CertificateOfOriginCountryInfo.ReadOnly);
			AssertEquals("CertificateOfOriginTypeInfo", false, invoiceLine.CertificateOfOriginTypeInfo.ReadOnly);
			AssertEquals("ItemNoOnCertOfOriginInfo", false, invoiceLine.ItemNoOnCertOfOriginInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PrimaryPreference = "NORMAL";
			AssertEquals("CertificateOfOrigin should be writable for EXP.", false, invoiceLine.CertificateOfOriginInfo.ReadOnly);
			AssertEquals("CertificateOfOriginCountry should be writable for EXP.", false, invoiceLine.CertificateOfOriginCountryInfo.ReadOnly);
			AssertEquals("CertificateOfOriginType should be writable for EXP.", false, invoiceLine.CertificateOfOriginTypeInfo.ReadOnly);
			AssertEquals("ItemNoOnCertOfOrigin should be writable for EXP.", false, invoiceLine.ItemNoOnCertOfOriginInfo.ReadOnly);

			invoiceLine.CertificateOfOriginType = "X";
			AssertEquals("CertificateOfOriginInfo should be readonly when CertificateOfOriginType set to X", true, invoiceLine.CertificateOfOriginInfo.ReadOnly);
			AssertEquals("ItemNoOnCertOfOriginInfo should be readonly when CertificateOfOriginType set to X", true, invoiceLine.ItemNoOnCertOfOriginInfo.ReadOnly);
		}

		public void TestCertificateOfOriginBooleanFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceLine.JI_PrimaryPreference = "FTA";
			AssertEquals("IsCertificateOfOriginApplicable should return true for IMP FTA.", true, invoiceLine.IsCertificateOfOriginApplicable);
			AssertEquals("IsCertificateOfOriginRequired should return true for IMP FTA.", true, invoiceLine.IsCertificateOfOriginRequired);

			invoiceLine.JI_PrimaryPreference = "NORMAL";
			AssertEquals("IsCertificateOfOriginApplicable should return false for IMP NORMAL.", false, invoiceLine.IsCertificateOfOriginApplicable);
			AssertEquals("IsCertificateOfOriginRequired should return false for IMP NORMAL.", false, invoiceLine.IsCertificateOfOriginRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsCertificateOfOriginApplicable should return true for EXP NORMAL.", true, invoiceLine.IsCertificateOfOriginApplicable);
			AssertEquals("IsCertificateOfOriginRequired should return false for EXP NORMAL.", false, invoiceLine.IsCertificateOfOriginRequired);

			invoiceLine.JI_PrimaryPreference = "FTA";
			AssertEquals("IsCertificateOfOriginRequired should return false for EXP FTA.", false, invoiceLine.IsCertificateOfOriginRequired);
		}

		public void TestDefaultingPreference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			var inst1 = declaration.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = CNRefCusProcedure.Codes._0110;

			var inst2 = declaration.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = CNRefCusProcedure.Codes._0420;

			invoiceLine1.JI_CEI = inst1.PK;
			invoiceLine1.JI_SecondaryPreference = "1";
			invoiceLine2.JI_CEI = inst1.PK;
			invoiceLine2.JI_SecondaryPreference = "2";
			invoiceLine3.JI_CEI = inst2.PK;
			invoiceLine3.JI_SecondaryPreference = "3";

			AssertEquals("JI_PrimaryPreference should be defaulted to MFN", mfnCode.ZZS_Preference, invoiceLine1.JI_PrimaryPreference);

			var supportingDocument1 = invoiceLine2.CusSupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "0y";
			supportingDocument1.CSI_ReferenceNumber = "654321";

			var supportingDocument2 = invoiceLine2.CusSupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "1Y";
			supportingDocument2.CSI_ReferenceNumber = "123456";

			var supportingDocument3 = invoiceLine3.CusSupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "1Y";
			supportingDocument3.CSI_ReferenceNumber = "000000";

			invoiceLine1.JI_PrimaryPreference = normalCode.ZZS_Preference;
			AssertEquals("No document should copied for NORMAL reference", 0, invoiceLine1.CusSupportingDocuments.Count);
			AssertEquals("JI_SecondaryPreference should cleared", ZString.Empty, invoiceLine1.JI_SecondaryPreference);

			invoiceLine1.JI_SecondaryPreference = "1";

			invoiceLine1.JI_PrimaryPreference = mfnCode.ZZS_Preference;
			AssertEquals("No document should copied for MFN reference", 0, invoiceLine1.CusSupportingDocuments.Count);
			AssertEquals("JI_SecondaryPreference should cleared", ZString.Empty, invoiceLine1.JI_SecondaryPreference);

			invoiceLine1.JI_PrimaryPreference = ftaCode.ZZS_Preference;
			AssertEquals("A document should be copied from invoiceLine2", 1, invoiceLine1.CusSupportingDocuments.Count);
		}

		public void TestDefaultCertificateOfOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CNPTA", "CN Prefential Trade Agreement");
			var cnpta_01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "01", "01", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_01.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			var inst1 = declaration.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = CNRefCusProcedure.Codes._0110;
			invoiceLine1.JI_CEI = inst1.PK;

			invoiceLine1.JI_PrimaryPreference = ftaCode.ZZS_Preference;
			invoiceLine1.JI_SecondaryPreference = "01";
			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine1.CertificateOfOriginCountry = "US";
			invoiceLine1.CertificateOfOriginType = "C";

			invoiceLine2.JI_CEI = inst1.PK;
			invoiceLine2.JI_CountryOfOrigin = "US";
			invoiceLine2.JI_PrimaryPreference = mfnCode.ZZS_Preference;
			invoiceLine2.JI_PrimaryPreference = ftaCode.ZZS_Preference;

			CombineAssertions("Copy CertificateOfOrigin values.", () =>
			{
				AssertEquals("Copy JI_SecondaryPreference", "01", invoiceLine2.JI_SecondaryPreference);
				AssertEquals("Copy CertificateOfOrigin", "111", invoiceLine2.CertificateOfOrigin);
				AssertEquals("Copy CertificateOfOriginCountry.", "US", invoiceLine2.CertificateOfOriginCountry);
				AssertEquals("Copy CertificateOfOriginType.", "C", invoiceLine2.CertificateOfOriginType);
			});

			invoiceLine2.JI_PrimaryPreference = mfnCode.ZZS_Preference;
			AssertEquals("CertificateOfOrigin cleared", "", invoiceLine2.CertificateOfOrigin);
			AssertEquals("Certificate Of Origin document deleted", 0, invoiceLine2.CusSupportingDocuments.Count);
		}

		public void TestJI_TradeUnitQty_DefaultFromInvoiceUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("035", invoiceLine.JI_TradeUnitQty);

			invoiceLine.JI_InvoiceUQ = "";
			AssertEquals("035", invoiceLine.JI_TradeUnitQty);
			invoiceLine.JI_TradeUnitQty = "036";

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("036", invoiceLine.JI_TradeUnitQty);
		}

		public void TestJI_TradeQuantity_ConvertingFromInvoiceQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceLine.JI_InvoiceUQ = "T";
			invoiceLine.JI_InvoiceQuantity = 1010m;
			AssertEquals("JI_TradeQuantity should have been converted when editing JI_InvoiceQuantity", 1010m, invoiceLine.JI_TradeQuantity);

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("JI_TradeQuantity should have been converted when editing JI_InvoiceUQ", 1.01m, invoiceLine.JI_TradeQuantity);
		}

		public void TestJI_TradeQuantity_ConvertToCustomsQuantities()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_TradeUnitQty = "035";
			invoiceLine.JI_CustomsUnitQty = "035";
			invoiceLine.JI_CustomsSecondUnitQty = "070";

			invoiceLine.JI_TradeQuantity = 1001m;
			AssertEquals("JI_CustomsUnitQty should have been converted when setting JI_TradeUnitQty", 1001m, invoiceLine.JI_CustomsQuantity);
			AssertEquals("JI_CustomsSecondQuantity should have been converted when setting JI_TradeUnitQty", 1.001m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestPrices()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "CNY";

			InvoiceLine.JI_TradeQuantity = 2;
			InvoiceLine.JI_InvoiceQuantity = 3;

			InvoiceLine.JI_LinePrice = 12;
			AssertEquals(new ZDecimal(6), InvoiceLine.TradeUnitPrice);

			InvoiceLine.TradeUnitPrice = 9;
			AssertEquals(new ZDecimal(18), InvoiceLine.JI_LinePrice);
			AssertEquals(new ZDecimal(6), InvoiceLine.UnitPrice);

			InvoiceLine.JI_TradeQuantity = 11;
			AssertEquals(new ZDecimal(1.6364), InvoiceLine.TradeUnitPrice);
		}

		public override void TestWipeNKTaxType()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			Assert(!testItem.InvoiceLine.ShouldWipeNKTaxType);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNotNullOrEmpty("ChargeTypeList has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList = commonInvoice.ChargeTypeList;
			AssertNullOrEmpty("ChargeTypeList not has 'RYT'", chargeTypeList.GetDescriptionFromCode("RYT"));
		}

		public void TestManufacturerChineseName()
		{
			var manufacturer = CNCusEntryHeaderHelper.CreateNewAddress(Factory, "Manufacturer 1", "", "", "");
			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.PK;

			AssertEquals("Should return a correct company name.", "Manufacturer 1", InvoiceLine.ManufacturerChineseName);
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			invoiceLine.VINDataCollection.AddNew();
			invoiceLine.ProductionBatch.AddNew();
			invoiceLine.CusSupportingDocuments.AddNew();
			invoiceLine.CIQProductQualifications.AddNew();

			var attachmentLink = invoiceLine.AttachmentLinks.Cast<AttachmentInvoiceLineLink>().FirstOrDefault();
			attachmentLink.IsLinked = true;

			CombineAssertions("To make sure the items exists before Deleting InvoiceLine.", () =>
			{
				AssertEquals("CusAddInfo rows", 1, Factory.Load<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, invoiceLine.PK)).Length);
				AssertEquals("CusCodeData rows", 2, Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, invoiceLine.PK)).Length);
				AssertEquals("CusSupportingInfo rows", 2, Factory.Load<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK)).Length);
				AssertEquals("GenPivot rows", 1, Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, invoiceLine.PK)).Length);
			});

			invoiceLine.Delete();

			CombineAssertions("Items should have been deleted when InvoiceLine Delete.", () =>
			{
				AssertEquals("CusAddInfo rows deleted", 0, Factory.Load<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, invoiceLine.PK)).Length);
				AssertEquals("CusCodeData rows deleted", 0, Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, invoiceLine.PK)).Length);
				AssertEquals("CusSupportingInfo rows deleted", 0, Factory.Load<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK)).Length);
				AssertEquals("GenPivot rows deleted", 0, Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, invoiceLine.PK)).Length);
				AssertEquals("AttachmentLinks deleted", true, attachmentLink.IsDeleted);
			});
		}

		public void TestAttachmentLinks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = CSDDocTypeList.Codes._80000001;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("AttachmentLinks", 1, invoiceLine.AttachmentLinks.Count);
		}

		public void TestLinkedAttachmentTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.CargoAttributes.AddNew(CargoAttributeList.Codes._31);

			var storageDoc1 = instruction.CusStorageDocPivots.AddNew();
			storageDoc1.CSD_DocType = CSDDocTypeList.Codes._80000001;
			storageDoc1.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine;
			AssertContainsExactElementsInAnyOrder(new[] { CSDDocTypeList.Codes._80000001 }, invoiceLine.LinkedAttachmentTypes);

			var storageDoc2 = instruction.CusStorageDocPivots.AddNew();
			storageDoc2.CSD_DocType = CSDDocTypeList.Codes._80000002;
			storageDoc2.InvoiceLineLinks.AddNew().Relation2Object = invoiceLine;
			AssertContainsExactElementsInAnyOrder(new[] { CSDDocTypeList.Codes._80000001, CSDDocTypeList.Codes._80000002 }, invoiceLine.LinkedAttachmentTypes);

			invoiceLine.CargoAttributes.RemoveAndDeleteAll();
			AssertEquals(0, invoiceLine.LinkedAttachmentTypes.Length);
		}

		public void TestDangerousGoodsDGSubs()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "2008", "c", "IMO").First().PK;
			AssertEquals(invoiceLine.DangerousGoods.UNDGSubstance.DG_Class, invoiceLine.DangerousGoods.DI_IMOClass);
			AssertNoErrors(invoiceLine.DangerousGoods.DI_IMOClassInfo);

			invoiceLine.DangerousGoodsDGSubs = ZGuid.Empty;
			AssertEquals(ZString.Empty, invoiceLine.DangerousGoods.DI_IMOClass);
			AssertNoErrors(invoiceLine.DangerousGoods.DI_IMOClassInfo);

			invoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "1438", "", "IMO").First().PK;
			AssertEquals(invoiceLine.DangerousGoods.UNDGSubstance.DG_Class, invoiceLine.DangerousGoods.DI_IMOClass);
			AssertNoErrors(invoiceLine.DangerousGoods.DI_IMOClassInfo);

			invoiceLine.DangerousGoodsDGSubs = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, invoiceLine.DangerousGoods.DI_IMOClass);
			AssertNoErrors(invoiceLine.DangerousGoods.DI_IMOClassInfo);

			invoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "2008", "c", "IMO").First().PK;
			AssertEquals(invoiceLine.DangerousGoods.UNDGSubstance.DG_Class, invoiceLine.DangerousGoods.DI_IMOClass);
			AssertNoErrors(invoiceLine.DangerousGoods.DI_IMOClassInfo);
		}

		public void TestDangerousChemicalByCargoAttributes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTDGC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "DGC", tariff);
			Factory.Save();

			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			var collection = invoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			collection.AddNew(CargoAttributeList.Codes._21);
			CombineAssertions("MarkedAsDangerousChemical and EDocAttachmentsAvailable", () =>
			{
				AssertEquals("None of 31,32,33 selected, MarkedAsDangerousChemical should be false.", false, invoiceLine.HasDangerousGoodsAttribute);
				AssertEquals("None of 31,32 selected, EDocAttachmentsAvailable should be false.", false, invoiceLine.CanLinkToAttachment);

				collection.AddNew(CargoAttributeList.Codes._31);
				AssertEquals("31 selected, MarkedAsDangerousChemical should be true.", true, invoiceLine.HasDangerousGoodsAttribute);
				AssertEquals("31 selected, EDocAttachmentsAvailable should be true.", true, invoiceLine.CanLinkToAttachment);

				collection.RemoveAndDelete(collection.FindByCode(CargoAttributeList.Codes._31));
				collection.AddNew(CargoAttributeList.Codes._32);
				AssertEquals("32 selected, MarkedAsDangerousChemical should be true.", true, invoiceLine.HasDangerousGoodsAttribute);
				AssertEquals("32 selected, EDocAttachmentsAvailable should be true.", true, invoiceLine.CanLinkToAttachment);

				collection.RemoveAndDelete(collection.FindByCode(CargoAttributeList.Codes._32));
				collection.AddNew(CargoAttributeList.Codes._33);
				AssertEquals("33 selected, MarkedAsDangerousChemical should be true.", true, invoiceLine.HasDangerousGoodsAttribute);
				AssertEquals("33 selected, EDocAttachmentsAvailable should be false.", false, invoiceLine.CanLinkToAttachment);

				AssertEquals("No Tariff input, TariffIsDangerousChemical should return false.", false, invoiceLine.TariffIsDangerousChemical);
				invoiceLine.JI_Tariff = "TESTDGC";
				AssertEquals("DGC Tariff input, TariffIsDangerousChemical should return true.", true, invoiceLine.TariffIsDangerousChemical);

				invoiceLine.JI_Tariff = "TESTUMF";
				AssertEquals("UMF Tariff input, TariffIsDangerousChemical should return false.", false, invoiceLine.TariffIsDangerousChemical);
			});
		}

		public void TestAdditionalInformationHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;

			invoiceLine.JI_NameOfGoods = "NameOfGoods1";
			invoiceLine.XC_GoodsSpecModel = "GoodsSpecModel1";
			AssertEquals("AdditionalInformationHelper.NameOfGoods", "NameOfGoods1", invoiceLine.AdditionalInformationHelper.NameOfGoods);
			AssertEquals("AdditionalInformationHelper.GoodsSpecModel", "GoodsSpecModel1", invoiceLine.AdditionalInformationHelper.GoodsSpecModel);
			AssertEquals("AdditionalInformationHelper.IsEnteringOrExiting", EnteringOrExiting.Entering, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);

			invoiceLine.JI_NameOfGoods2 = "NameOfGoods2";
			invoiceLine.XC_GoodsSpecModel2 = "GoodsSpecModel2";
			AssertEquals("AdditionalInformatio2nHelper.NameOfGoods", "NameOfGoods2", invoiceLine.AdditionalInformation2Helper.NameOfGoods);
			AssertEquals("AdditionalInformation2Helper.GoodsSpecModel", "GoodsSpecModel2", invoiceLine.AdditionalInformation2Helper.GoodsSpecModel);
			AssertEquals("AdditionalInformation2Helper.IsEnteringOrExiting", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformation2Helper.IsEnteringOrExiting);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("AdditionalInformationHelper.IsEnteringOrExiting", EnteringOrExiting.Entering, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);
			AssertEquals("AdditionalInformation2Helper.IsEnteringOrExiting", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformation2Helper.IsEnteringOrExiting);

			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("AdditionalInformationHelper.IsEnteringOrExiting", EnteringOrExiting.Exiting, invoiceLine.AdditionalInformationHelper.IsEnteringOrExiting);
		}

		public void TestValidationModeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CEI = instruction.PK;

			ValidationExtensionsTest.AssertValidationModeProvider(declaration, invoiceLine.AdditionalInformationHelper.ValidationModeProvider);
		}

		public void TestSyncCIQDetails()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "720c28d9e797c0d8408d34da1b0ab981", "baeb884cae1c682fd9c48a9bbc20b849", "99998", "ef374a392b7934885636cee2f907884a");
				helper.CreateAdditionalElement("720c28d9e797c0d8408d34da1b0ab981", "面料成分含量");
				helper.CreateAdditionalElement("baeb884cae1c682fd9c48a9bbc20b849", "品牌(厂商)");
				helper.CreateAdditionalElement("99998", "规格型号");
				helper.CreateAdditionalElement("ef374a392b7934885636cee2f907884a", "生产日期");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "50|B|规格：333、型号：444|20220201;20220202";
			var batches = invoiceLine.ProductionBatch;
			var batch1 = batches.AddNew();
			batch1.CY_Data = "1";
			batch1.CY_Date = new ZDateTime(2022, 02, 01);
			var batch2 = batches.AddNew();
			batch2.CY_Date = new ZDateTime(2022, 02, 03);

			invoiceLine.SyncCIQDetails();

			CombineAssertions(() =>
			{
				AssertEquals("Ingredient", "50", invoiceLine.CIQIngredient);
				AssertEquals("Specification", "333", invoiceLine.JI_NDescription);
				AssertEquals("Brand", "B", invoiceLine.JI_BrandName);
				AssertEquals("Model", "444", invoiceLine.JI_Model);
				AssertEquals("ProductionBatch Count", 2, batches.Count);
				AssertEquals("1st Manufacture Date", new ZDateTime(2022, 02, 01), batches[0].CY_Date);
				AssertEquals("2nd Manufacture Date", new ZDateTime(2022, 02, 02), batches[1].CY_Date);
			});
		}

		public void TestBatchNumbersAndManufactureDates()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var invoiceLine = item.InvoiceLine;

			var batch1 = invoiceLine.ProductionBatch.AddNew();
			var batch2 = invoiceLine.ProductionBatch.AddNew();
			var batch3 = invoiceLine.ProductionBatch.AddNew();
			batch1.CY_Data = "1";
			batch1.CY_Date = new ZDateTime(2022, 02, 11);
			batch2.CY_Data = "2";
			batch2.CY_Date = new ZDateTime(2022, 02, 11);
			batch3.CY_Data = "2";
			batch3.CY_Date = new ZDateTime(2022, 02, 14);

			CombineAssertions(() =>
			{
				AssertEquals("BatchNumbersAsString", "1;2", invoiceLine.BatchNumbersAsString);
				AssertEquals("ManufactureDatesAsString", "20220211;20220214", invoiceLine.ManufactureDatesAsString);
			});
		}

		public void TestConditionSelectionCriteria()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DateForDuty = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_PrimaryPreference = "MFN";
			invoiceLine.JI_SecondaryPreference = "01";

			var conditionSelectionCriteria = invoiceLine.ConditionSelectionCriterias.First();
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), conditionSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", conditionSelectionCriteria.TradeGroupCountry);
			AssertEquals("PrimaryPreference", "MFN", conditionSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, conditionSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", conditionSelectionCriteria.AdditionalCodes.Contains("01"));
			AssertEquals("ConcessionOrder", "", conditionSelectionCriteria.ConcessionOrder);
			AssertEquals("DataGrouping", "CN", conditionSelectionCriteria.DataGrouping);
			AssertEquals("Direction", ConditionChecker.ConditionDirection.Import, conditionSelectionCriteria.Direction);
			AssertNotNull("EvaluateConditionValue", invoiceLine.EvaluateConditionValue);
			AssertNotNull("GetFriendlyConditionValue", invoiceLine.GetFriendlyConditionValue);
			AssertType<ConditionCalcDataForInvoiceLine>("CalcDataForConditionFormula", invoiceLine.CalcDataForConditionFormula);
		}

		public void TestCloneShouldCloneCorrespondingAddInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_AddInfo = "";
			invoiceLine.JI_NAddInfo = "NameOfGoods=产品名";

			var clonedDec = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
			AssertEquals("", clonedDec.InvoiceLines[0].JI_AddInfo);
			AssertEquals("NameOfGoods=产品名", clonedDec.InvoiceLines[0].JI_NAddInfo);

			var clonedDec2 = (JobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy, Factory).Clone();
			AssertEquals("", clonedDec2.InvoiceLines[0].JI_AddInfo);
			AssertEquals("", clonedDec2.InvoiceLines[0].JI_NAddInfo);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceLine,
				"CNJobComInvoiceLine");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);

			ftaCode = helper.CreatePreferenceForCountryAndGrouping(Constants.PrimaryPreferenceCodes.FreeTradeAgreement, "Free Trade Agreement", "CN", "CN");
			ldcCode = helper.CreatePreferenceForCountryAndGrouping(Constants.PrimaryPreferenceCodes.LeastDevelopedCountries, "Least Developed Countries", "CN", "CN");
			mfnCode = helper.CreatePreferenceForCountryAndGrouping(Constants.PrimaryPreferenceCodes.MostFavouredNations, "Most Favoured Nations", "CN", "CN");
			normalCode = helper.CreatePreferenceForCountryAndGrouping(Constants.PrimaryPreferenceCodes.Normal, "Normal", "CN", "CN");

			Factory.Save();
		}

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => InvoiceLine;

		protected override bool RatesAreReciprocal => true;

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			base.DoMerge(declaration);
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = CNRefCusProcedure.Codes._0110;
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = instruction.PK;
			}
		}

		public class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{ }

			public ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategyExposed() => GetCustomsUnitDefaultingStrategy();
		}
	}
}
