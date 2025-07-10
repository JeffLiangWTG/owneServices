using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DeductionChargeImportLineTest : TestCaseWithFactory
	{
		[TestDate(2012, 1, 1)]
		public void TestUnitConvertWhenNetAndGrossWeightAreEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "4001100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();

				var product = Factory.New<AUOrgSupplierPart>();
				product.OP_PartNum = "Test";
				var relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = importer.PK;
				relation.OU_Relationship = "OWN";

				product.OP_StockKeepingUnit = "UNT";
				product.OP_Weight = 0.52m;
				product.OP_WeightUQ = "KG";
				product.OP_NetWeight = 0.51m;

				var classification = Factory.New<Classification>();
				classification.CC_ClassificationType = "IMP";
				classification.CC_TariffNum = "4001100001";

				product.AddNewImportPivotWithClassification(classification.PK);

				var productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 0.13m;
				productUnitConversion.OF_PackType = "KG";
				productUnitConversion.OF_ParentPackType = "NO";

				productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 4m;
				productUnitConversion.OF_PackType = "NO";
				productUnitConversion.OF_ParentPackType = "UNT";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = "IMP";

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "Test";
				invoiceLine.JI_InvoiceQuantity = 10m;
				AssertEquals("Net weight is calculated", 5.1m, invoiceLine.JI_NetWeight);
				AssertEquals("Net weight UQ", "KG", invoiceLine.JI_NetWeightUQ);

				AssertEquals("Customs UQ", "KG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Customs Quantity", 5.1m, invoiceLine.JI_CustomsQuantity);
			}
		}

		[TestDate(2012, 1, 1)]
		public void TestUnitConvertWhenNetAndGrossWeightAreEntered_AUCClass()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";

			product.OP_StockKeepingUnit = "UNT";
			product.OP_Weight = 0.52m;
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 0.51m;

			var classification = Factory.New<Classification>();
			classification.CC_ClassificationType = "IMP";
			classification.CC_TariffNum = "4001100001";

			product.AddNewImportPivotWithClassification(classification.PK);

			var productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 0.13m;
			productUnitConversion.OF_PackType = "KG";
			productUnitConversion.OF_ParentPackType = "NO";

			productUnitConversion = product.PartUnits.AddNew();
			productUnitConversion.OF_QuantityInParent = 4m;
			productUnitConversion.OF_PackType = "NO";
			productUnitConversion.OF_ParentPackType = "UNT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = "IMP";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "Test";
			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Net weight is calculated", 5.1m, invoiceLine.JI_NetWeight);
			AssertEquals("Net weight UQ", "KG", invoiceLine.JI_NetWeightUQ);

			AssertEquals("Customs UQ", "KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Customs Quantity", 5.1m, invoiceLine.JI_CustomsQuantity);
		}

		[TestDate(2012, 1, 1)]
		public void TestUnitConvertWhenNOWeightsAreEntered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "4001100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();

				var product = Factory.New<AUOrgSupplierPart>();
				product.OP_PartNum = "Test";
				var relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = importer.PK;
				relation.OU_Relationship = "OWN";

				product.OP_StockKeepingUnit = "UNT";

				var classification = Factory.New<Classification>();
				classification.CC_ClassificationType = "IMP";
				classification.CC_TariffNum = "4001100001";

				product.AddNewImportPivotWithClassification(classification.PK);

				var productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 0.13m;
				productUnitConversion.OF_PackType = "KG";
				productUnitConversion.OF_ParentPackType = "NO";

				productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 4m;
				productUnitConversion.OF_PackType = "NO";
				productUnitConversion.OF_ParentPackType = "UNT";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "Test";

				invoiceLine.JI_InvoiceQuantity = 10m;
				AssertEquals("Customs UQ", "KG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Customs Quantity", 5.2m, invoiceLine.JI_CustomsQuantity);
			}
		}

		[TestDate(2012, 1, 1)]
		public void TestUnitConvertWhenNOWeightsAreEntered_AUCClass()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();

				var product = Factory.New<AUOrgSupplierPart>();
				product.OP_PartNum = "Test";
				var relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = importer.PK;
				relation.OU_Relationship = "OWN";

				product.OP_StockKeepingUnit = "UNT";

				var classification = Factory.New<Classification>();
				classification.CC_ClassificationType = "IMP";
				classification.CC_TariffNum = "4001100001";

				product.AddNewImportPivotWithClassification(classification.PK);

				var productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 0.13m;
				productUnitConversion.OF_PackType = "KG";
				productUnitConversion.OF_ParentPackType = "NO";

				productUnitConversion = product.PartUnits.AddNew();
				productUnitConversion.OF_QuantityInParent = 4m;
				productUnitConversion.OF_PackType = "NO";
				productUnitConversion.OF_ParentPackType = "UNT";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "Test";

				invoiceLine.JI_InvoiceQuantity = 10m;
				AssertEquals("Customs UQ", "KG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Customs Quantity", 5.2m, invoiceLine.JI_CustomsQuantity);
			}
		}

		public void TestIssue00721603()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNoExceptionThrown(delegate
			{
				var accessedAmount = invoiceLine.JI_Calc_TNI;
				var accessedCurr = invoiceLine.JI_RX_Calc_TNICurrency;
			});
		}

		public void TestCostInLocalCurrency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.5m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.AddInfo.ZA_ADJ = "20KRW";
			AssertEquals(1000m, ((IUltimateDistributee)invoiceLine).LinePriceInInvoiceCurrency);
			AssertEquals(2000m, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);

			invoice.AddInfo.ZA_IncADJ_Hidden = true;
			AssertEquals(1020m, ((IUltimateDistributee)invoiceLine).LinePriceInInvoiceCurrency);
			AssertEquals(2040m, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);

			JobComInvoiceLine standAloneInvoiceLine = Factory.New<JobComInvoiceLine>();
			standAloneInvoiceLine.JI_LinePrice = 500m;
			standAloneInvoiceLine.AddInfo.ZA_ADJ = "20KRW";
			AssertEquals("Expect no exception", 500m, ((IUltimateDistributee)standAloneInvoiceLine).LinePriceInInvoiceCurrency);
			AssertEquals("Expect no exception - cannot be calculated without a header", (ZDecimal)0, ((IUltimateDistributee)standAloneInvoiceLine).CostInLocalCurrency);
		}

		public void TestPrice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.5m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			AssertEquals((ZDecimal)1000, invoiceLine.Price.Amount);
			AssertEquals(invoice.JZ_RX_NKInvoice_Currency, invoiceLine.Price.Currency.Code);

			JobComInvoiceLine standAloneInvoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Price should default to empty if there is no parent header", Money.Empty, standAloneInvoiceLine.Price);
		}

		public void TestAggreatedValueForPreferences()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_POC = "US";
			invoice.AddInfo.ZA_PST = "US";
			invoice.AddInfo.ZA_PRT = "WO";

			CMRTariffRatePeriodSnapshot tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_PreferenceSchemeType = "GEN";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("IsGeneralRate", true, invoiceLine.IsGeneralRate);

			AssertEquals("AggregatedValue for POC", "", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_POC.Name));
			AssertEquals("AggregatedValue for PST", "GEN", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_PST.Name));
			AssertEquals("AggregatedValue for PRT", "", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_PRT.Name));

			CMRTariffRatePeriodSnapshot tariffRate2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate2.TT_TariffClassificationNumber = "00000001";
			tariffRate2.TT_PreferenceSchemeType = "US";
			tariffRate2.TT_StartDate = new ZDateTime(2005, 1, 1);

			CMRPreferenceSchemePeriodCountry schemeCountryUS = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountryUS.PC_PreferenceSchemePeriodSnapshotSchemeType = "US";
			schemeCountryUS.PC_CountryCode = "US";

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000001 00";
			AssertEquals("IsGeneralRate", false, invoiceLine.IsGeneralRate);
			AssertEquals("AggregatedValue for POC", "US", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_POC.Name));
			AssertEquals("AggregatedValue for PST", "US", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_PST.Name));
			AssertEquals("AggregatedValue for PRT", "WO", invoiceLine.AggregatedValue(AUAddInfoSchema.ZA_PRT.Name));
		}

		public void TestDeductionChargesForExWorks()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");

			BaseJobComInvHeaderCharge nonDutiableFIFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - EXW", Core.Constants.IncoTerms.ExWorks, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight should not have Non-dutiable portion", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("JI_AdditionCharge", 0m, invoiceLine.JI_OtherCharges1.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 150m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should not include this amount as non-dutiable is part of OFT and OFT is not part of CV calculation as incoterm is EXW", 0m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForUnpackedFactory()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");

			BaseJobComInvHeaderCharge nonDutiableFIFT = groupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - UCF", Core.Constants.IncoTerms.UnpackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight should not have Non-dutiable portion", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("JI_AdditionCharge", 0m, invoiceLine.JI_OtherCharges1.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 150m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should not include this amount as non-dutiable is part of OFT and OFT is not part of CV calculation as incoterm is UAF", 0m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForUCF()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			BaseJobComInvHeaderCharge overseasFreight = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			overseasFreight.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsIncludedInITOT = true;
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - UCF", Core.Constants.IncoTerms.UnpackedCostAndFreight, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight with Non-dutiable portion", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 150m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should not include this amount as non-dutiable is part of OFT and will be deducted from ITOT to get to CV", 50m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForUCI()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			BaseJobComInvHeaderCharge overseasFreight = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			overseasFreight.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge overseasInsurance = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "AUD");
			overseasInsurance.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsIncludedInITOT = true;
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - UCI", Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight with Non-dutiable portion", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 160m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("Deduction should include this amount and will be deducted from ITOT to get to CV", 50m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForITOTIncotermWithCAndI()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");

			BaseJobComInvHeaderCharge overseasInsurance = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, "AUD");
			overseasInsurance.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsIncludedInITOT = true;
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - C&I", Core.Constants.IncoTerms.CostAndInsurance, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight with Non-dutiable portion", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 160m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should include this amount as non-dutiable should be deducted from C&I - ONS to get to CV", 50m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForITOTIncotermAboveCFRWithNonDutiableGSTible()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			BaseJobComInvHeaderCharge overseasFreight = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");
			overseasFreight.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsIncludedInITOT = true;
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - CFR", Core.Constants.IncoTerms.CostAndFreight, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 150m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should be non-dutiable FIFT to reduce Customs Value as it is included in ITOT", 50m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestDeductionChargesForITOTIncotermBelowFOBWithNonDutiableGSTible()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 50m, "AUD");
			nonDutiableFIFT.J7_IsIncludedInITOT = true;
			nonDutiableFIFT.J7_IsDutiable = false;

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			testDec.ResumeApportionment();
			AssertEquals("ITOT incoterm - FOB", Core.Constants.IncoTerms.FreeOnBoard, invoice.ITOTIncoTerm);
			AssertEquals("JI_OverseasFreight", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("JI_ForeignInlandFreight", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("TransportAndInsurance should have non-dutiable portion", 150m, invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("Deduction should include non-dutiable FIFT to reduce Customs Value as it is included in ITOT", 50m, invoiceLine.JI_OtherCharges2.Amount);
		}

		public void TestIsDrawbackClaimAmountUnusual()
		{
			JobDeclaration referenceDec;
			CusEntryHeader referenceDecEntryHeader1;
			CusEntryHeader referenceDecEntryHeader2;
			CusEntryLine referenceDecEntryLine1;
			CusEntryLine referenceDecEntryLine2;
			OrgHeader importer1;
			OrgHeader importer2;

			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "ORG1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "ORG2";
			AUOrgSupplierPart part1 = AUOrgSupplierPart.New(Factory);
			part1.OP_PartNum = "PART1";
			part1.OP_Desc = "PART1";
			part1.OP_StockKeepingUnit = "NO";
			OrgPartRelation relation1 = part1.RelatedOrganisations.AddNew();
			relation1.OU_OH = importer1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AUOrgSupplierPart part2 = AUOrgSupplierPart.New(Factory);
			part2.OP_PartNum = "PART2";
			part2.OP_Desc = "PART2";
			part2.OP_StockKeepingUnit = "NO";
			OrgPartRelation relation2 = part2.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_OH_Importer = importer1.PK;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_PartNo = "PART1";

			referenceDec = JobDeclaration.New(Factory);
			referenceDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDec.JE_OH_Importer = importer1.PK;

			referenceDecEntryHeader1 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader1.EntryNumber = "ENTRY2";
			referenceDecEntryLine1 = referenceDecEntryHeader1.MergedLines.AddNew();
			referenceDecEntryLine1.CL_LineNumber = 5;
			referenceDecEntryLine1.CL_CustomsValue = 4000m;
			referenceDecEntryLine1.CL_DutyPercent = 5m;
			referenceDecEntryLine1.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			JobComInvoiceHeader referenceDecInvoiceHeader = referenceDec.Invoices.AddNew();
			referenceDecInvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDecInvoiceHeader.JZ_InvoiceAmount = 14000m;
			referenceDecInvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine referenceDecEntryLine1InvoiceLine = referenceDecInvoiceHeader.JobComInvoiceLines.AddNew();

			referenceDecEntryLine1InvoiceLine.JI_CL = referenceDecEntryLine1.PK;
			referenceDecEntryLine1InvoiceLine.JI_PartNo = "PART1";
			referenceDecEntryLine1InvoiceLine.JI_LinePrice = 4000m;
			referenceDecEntryLine1InvoiceLine.JI_InvoiceQuantity = 20m;
			referenceDecEntryLine1InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecEntryLine1InvoiceLine.JI_CustomsQuantity = 20m;
			referenceDecEntryLine1InvoiceLine.JI_CustomsUnitQty = "NO";
			referenceDecEntryHeader2 = referenceDec.CustomsEntryHeaders.AddNew();
			referenceDecEntryHeader2.EntryNumber = "ENTRY1";
			referenceDecEntryLine2 = referenceDecEntryHeader2.MergedLines.AddNew();
			referenceDecEntryLine2.CL_LineNumber = 1;
			referenceDecEntryLine2.CL_CustomsValue = 10000m;
			referenceDecEntryLine2.CL_DutyPercent = 5m;
			referenceDecEntryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1000m);
			JobComInvoiceLine referenceDecEntryLine2InvoiceLine = referenceDecInvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecEntryLine2InvoiceLine.JI_CL = referenceDecEntryLine2.PK;
			referenceDecEntryLine2InvoiceLine.JI_PartNo = "PART1";
			referenceDecEntryLine2InvoiceLine.JI_LinePrice = 10000m;
			referenceDecEntryLine2InvoiceLine.JI_InvoiceQuantity = 30m;
			referenceDecEntryLine2InvoiceLine.JI_InvoiceUQ = "NO";
			referenceDecEntryLine2InvoiceLine.JI_CustomsQuantity = 30m;
			referenceDecEntryLine2InvoiceLine.JI_CustomsUnitQty = "NO";
			var referenceDecEntryLine2OtherLine = referenceDecInvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDecEntryLine2OtherLine.JI_CL = referenceDecEntryLine2.PK;
			referenceDecEntryLine2OtherLine.JI_LinePrice = 10000m;
			referenceDecEntryLine2OtherLine.JI_InvoiceQuantity = 1m;
			referenceDecEntryLine2OtherLine.JI_InvoiceUQ = "NO";
			referenceDecEntryLine2OtherLine.JI_CustomsQuantity = 1m;
			referenceDecEntryLine2OtherLine.JI_CustomsUnitQty = "NO";

			referenceDec.ManualClearanceDate = new ZDateTime(2007, 6, 30);
			Factory.Save();

			invoice.JZ_InvoiceDate = new ZDateTime(2007, 7, 1);
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Method B Claim Customs Value", 2000m, invoiceLine.DrawbackCustomsValue);
			AssertEquals("Method B Claim Duty Amount", 100m, invoiceLine.DrawbackDutyAmount);
			AssertEquals("Average Customs Value", 280m, invoiceLine.AverageCustomsValue);
			Assert("Is Unusual", invoiceLine.IsDrawbackClaimAmountUnusual);
			invoiceLine.JI_CustomsQuantity = 7m;
			invoiceLine.IsDrawbackLineValueOverriden = true;
			invoiceLine.AddInfo.ZA_DDT_Hidden = 100m;
			invoiceLine.AddInfo.ZA_DCV_Hidden = 2000m;
			AssertEquals("Average Customs Value Amt", 280m, invoiceLine.AverageCustomsValue);
			Assert("Is not Unusual", !invoiceLine.IsDrawbackClaimAmountUnusual);

			JobDeclaration referenceDec2 = JobDeclaration.New(Factory);
			referenceDec2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			referenceDec2.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			referenceDec2.JE_OH_Importer = importer1.PK;
			referenceDec2.ManualClearanceDate = new ZDateTime(2006, 7, 1);
			CusEntryHeader referenceDec2EntryHeader = referenceDec2.CustomsEntryHeaders.AddNew();
			referenceDec2EntryHeader.EntryNumber = "ENTRY3";
			CusEntryLine referenceDec2EntryLine = referenceDec2EntryHeader.MergedLines.AddNew();
			referenceDec2EntryLine.CL_LineNumber = 2;
			referenceDec2EntryLine.CL_CustomsValue = 12000m;
			referenceDec2EntryLine.CL_DutyPercent = 0m;
			referenceDec2EntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 600m);
			JobComInvoiceHeader referenceDec2InvoiceHeader = referenceDec2.Invoices.AddNew();
			referenceDec2InvoiceHeader.JZ_IncoTerm = "FOB";
			referenceDec2InvoiceHeader.JZ_InvoiceAmount = 15000m;
			referenceDec2InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine referenceDec2InvoiceLine = referenceDec2InvoiceHeader.JobComInvoiceLines.AddNew();
			referenceDec2InvoiceLine.JI_CL = referenceDec2EntryLine.PK;
			referenceDec2InvoiceLine.JI_PartNo = "PART1";
			referenceDec2InvoiceLine.JI_InvoiceQuantity = 30m;
			referenceDec2InvoiceLine.JI_InvoiceUQ = "XX";
			referenceDec2InvoiceLine.JI_CustomsUnitQty = "NO";
			referenceDec2InvoiceLine.JI_CustomsQuantity = 40m;
			Factory.Save();

			invoiceLine.IsDrawbackLineValueOverriden = false;
			invoiceLine.JI_CustomsQuantity = 8m;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";

			AssertEquals("Method B Claim Duty Amount", 120m, invoiceLine.DrawbackDutyAmount);
			AssertEquals("Method B Claim Customs Value", 2400m, invoiceLine.DrawbackCustomsValue);
			AssertEquals("Average Customs Value", 288.889m, invoiceLine.AverageCustomsValue);
			Assert("Is not unusual", !invoiceLine.IsDrawbackClaimAmountUnusual);

			referenceDec2.ManualClearanceDate = new ZDateTime(2005, 12, 31);
			Factory.Save();

			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Method B Claim Duty Amount", 80m, invoiceLine.DrawbackDutyAmount);
			AssertEquals("Method B Claim Customs Value", 1600m, invoiceLine.DrawbackCustomsValue);
			AssertEquals("Average Customs Value", 280m, invoiceLine.AverageCustomsValue);

			referenceDec2.ManualClearanceDate = new ZDateTime(2006, 1, 1);
			Factory.Save();

			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Average Customs Value", 288.889m, invoiceLine.AverageCustomsValue);

			referenceDec2InvoiceLine.JI_PartNo = "PART2";
			Factory.Save();

			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Average Custms Value", 280m, invoiceLine.AverageCustomsValue);

			referenceDec2InvoiceLine.JI_PartNo = "PART1";
			referenceDec2InvoiceLine.JI_CustomsQuantity = 45m;
			Factory.Save();

			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Average Customs Value", 273.684m, invoiceLine.AverageCustomsValue);

			referenceDec2.JE_OH_Importer = importer2.PK;
			Factory.Save();

			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Average Customs Value", 280m, invoiceLine.AverageCustomsValue);

			invoiceLine.JI_PartNo = "PART2";
			invoiceLine.AddInfo.ZA_DAM_Hidden = "";
			invoiceLine.AddInfo.ZA_DDN_Hidden = "";
			invoiceLine.AddInfo.ZA_DDL_Hidden = 0;
			invoiceLine.AddInfo.ZA_DAM_Hidden = "B";
			AssertEquals("Average Customs Value", 0m, invoiceLine.AverageCustomsValue);
			Assert("Is not unusual", !invoiceLine.IsDrawbackClaimAmountUnusual);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;
		}

		#endregion
	}
}
