using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class JobComInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestICusStorageDocPivotTypeSupporter_ReloadCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			_ = invHeader.EDocPivotCollection;
			var provider = (ICusStorageDocPivotTypeSupporter)invHeader;
			var pivot = Factory.New<CusStorageDocPivot>();
			pivot.CSD_ParentID = invHeader.PK;
			pivot.CSD_ParentTableCode = invHeader.TablePrefix;
			provider.ReloadCollection();
			AssertEquals(1, invHeader.EDocPivotCollection.Count);
		}

		public void TestICusStorageDocPivotTypeSupporter_HumanReadableName()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "INV 123";
			var provider = (ICusStorageDocPivotTypeSupporter)invHeader;
			AssertEquals("invoice INV 123", provider.HumanReadableName);
		}

		public void TestCanDeleteAndReasonForNotAbleToDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			Assert("The quarantine header has not sent any messages, therefore we should be able to delete the invoice header", invoiceHeader.CanDelete);
			AssertEquals(ZString.Empty, invoiceHeader.ReasonForNotAbleToDelete);

			quarantineExDocHeader.Messages.AddNew();

			Assert("The quarantine header has messages, therefore we shouldn't be able to delete the invoice header", !invoiceHeader.CanDelete);
			AssertEquals("Messages exist against the job so this cannot be deleted.", invoiceHeader.ReasonForNotAbleToDelete);
		}

		public void TestIsNEXDOCSActiveDoesntCreateNewQuarantineIfNotFound()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();

			var filter = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invHeader.PK);
			var quarantine = Factory.LoadTop1<QuarantineExDocHeader>(filter);
			AssertNull("Precondition:", quarantine);

			var nexdocsActive = invHeader.IsNEXDOCSActive;
			quarantine = Factory.LoadTop1<QuarantineExDocHeader>(filter);
			AssertNull("The quarantine should not have been created to check if nexdocs is active", quarantine);
			Assert("NEXDOCS should be considered inactive, as there is no Quarantine", !nexdocsActive);
		}

		public void TestCanDeleteDoesntCreateNewQuarantineIfNotFound()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();

			var filter = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, invHeader.PK);
			var quarantine = Factory.LoadTop1<QuarantineExDocHeader>(filter);
			AssertNull("Precondition:", quarantine);

			var canDelete = invHeader.CanDelete;
			quarantine = Factory.LoadTop1<QuarantineExDocHeader>(filter);
			AssertNull("The quarantine should not have been created to check if the invoice header can be deleted", quarantine);
			Assert("The inv header should be able to be deleted, as there is no quarantine to check for messaging against", canDelete);
		}

		[TestDate(2006, 5, 12)]
		public void TestEffectiveDutyDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 05, 05);
			AssertEquals("Effective Duty Date", ZDateTime.Today, invoice.EffectiveDutyDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2006, 05, 06);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Effective Duty Date", new ZDateTime(2006, 05, 06), invoice.EffectiveDutyDate);

			declaration.JE_DateOfFirstArrival = ZDateTime.Today.AddDays(1);
			AssertEquals("PreCondition:DateOfFirstArrival is in the future", true, declaration.JE_DateOfFirstArrival.IsInTheFutureDatePartOnly);
			AssertEquals("Effective duty date is today when first arrival date is in the future", ZDateTime.Today, invoice.EffectiveDutyDate);

			invoice.AddInfo.ZA_EFD = "120506";//ddMMyy
			AssertEquals("effective duty date is overriden in AddInfo", new ZDateTime(2006, 05, 12), invoice.EffectiveDutyDate);
		}

		public void TestEffectiveDutyDateForExWarehouse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			var invoice = declaration.Invoices.AddNew();
			AssertEquals(ZDateTime.Today, invoice.EffectiveDutyDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2015, 04, 08);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals(declaration.JE_EntrySubmittedDate, invoice.EffectiveDutyDate);

			invoice.AddInfo.ZA_EFD = "120415";
			AssertEquals(new ZDateTime(2015, 04, 12), invoice.EffectiveDutyDate);
		}

		public void TestZeroOFTONSValidationForEffectiveCIF()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.RunPreSaveValidation();
			AssertEquals("Warning for zero OFT & ONS", true, invoice.EffectiveCIFAmountInfo.GetWarnings().Contains(InvoiceHeaderValidation.ZeroFreightInsuranceWarning));
		}

		public void TestGSTExemptCollections()
		{
			var testTreatmentRate1 = CMRTreatmentRatePeriodCharacteristic.New(Factory);
			testTreatmentRate1.TR_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);

			var testTreatmentRate2 = CMRTreatmentRatePeriodCharacteristic.New(Factory);
			testTreatmentRate2.TR_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.LuxuryCarTaxMayBeApplicable);

			var testDec = JobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();

			AssertEquals("GST Exempt Treatment Rates should have testTreatmentRate1", true, invoice.GSTExemptTreatmentRateCharacteristics.Contains(testTreatmentRate1));
			AssertEquals("GST Exempt Treatment Rates should not have testTreatmentRate2", false, invoice.GSTExemptTreatmentRateCharacteristics.Contains(testTreatmentRate2));

			var testTariffRate1 = CMRTariffRatePeriodCharacteristic.New(Factory);
			testTariffRate1.TH_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);

			var testTariffRate2 = CMRTariffRatePeriodCharacteristic.New(Factory);
			testTariffRate2.TH_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.LuxuryCarTaxMayBeApplicable);

			AssertEquals("GST Exempt Tariff Rates should have testTariffRate1", true, invoice.GSTExemptTariffRateCharacteristics.Contains(testTariffRate1));
			AssertEquals("GST Exempt Tariff Rates should not have testTariffRate2", false, invoice.GSTExemptTariffRateCharacteristics.Contains(testTariffRate2));
		}

		public void TestEffectiveFOBAmountEffectiveCIFAmount()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;

			var oNS = invoice.Charges.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 10m;
			oNS.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;

			var fIFT = invoice.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 50m;
			fIFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			fIFT.J7_IsDutiable = false;

			AssertEquals("Effective FOB Amount", 9950m, invoice.EffectiveFOBAmount);
			AssertEquals("Effective CIF Amount", 10060m, invoice.EffectiveCIFAmount);
			AssertEquals("T&I: This is calculated by JZ_Calc_CIF - JZ_Calc_FOB", 160m, invoice.JZ_Calc_TNI);

			invoice.JZ_OverrideFOB = true;
			invoice.JZ_FOBValue = 10000m;

			AssertEquals("Effective FOB Amount", 10000m, invoice.EffectiveFOBAmount);
			AssertEquals("Effective CIF Amount", 10110m, invoice.EffectiveCIFAmount);
			AssertEquals("T&I: still the same", 160m, invoice.JZ_Calc_TNI);
		}

		public void TestReadOnlyOfEffectiveFOBAmount()
		{
			var testDec = JobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();

			invoice.JZ_OverrideFOB = true;
			AssertEquals("JZ_FOBValue should be open for editing", false, invoice.EffectiveFOBAmountInfo.ReadOnly);

			invoice.JZ_OverrideFOB = false;
			AssertEquals("JZ_FOBValue should be locked down", true, invoice.EffectiveFOBAmountInfo.ReadOnly);
		}

		public void TestCopyCalc_FOBValueToFOBWhenOverriden()
		{
			var testDec = JobDeclaration.New(Factory);
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = "FOB";
			AssertEquals("JZ_Calc_FOB", 10000m, invoice.JZ_Calc_FOBAmount);

			invoice.JZ_OverrideFOB = true;
			AssertEquals("JZ_FOBValue has JZ_Calc_FOB", 10000m, invoice.JZ_FOBValue);

			invoice.JZ_FOBValue = 12000m;
			AssertEquals("JZ_FOBValue is modified by users", 12000m, invoice.JZ_FOBValue);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var invoiceLoaded = factory2.Load<JobComInvoiceHeader>(invoice.PK);
			AssertEquals("JZ_FOBValue field still has the value", 12000m, invoiceLoaded.JZ_FOBValue);
		}

		[TestDate(2016, 6, 15, 1, 1, 1)]
		public void TestEffectiveDutyDateException()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			declaration.JE_DateOfFirstArrival = new ZDateTime(1916, 03, 06);

			line.JI_Tariff = "70099200 25";
			AssertNoExceptionThrown(() => line.Validation.ValidateJI_Tariff());
			AssertHasErrors("Date of first arrival is invalid", declaration.JE_DateOfFirstArrivalInfo);
			AssertEquals("Effective duty date is empty", ZDateTime.Today, header.EffectiveDutyDate);

			declaration.JE_DateOfFirstArrival = new ZDateTime(2016, 03, 06);
			declaration.Validation.ValidateJE_DateOfFirstArrival();
			AssertEquals("Date of first arrival is valid", true, declaration.JE_DateOfFirstArrival.IsValid);
			AssertEquals("Effective duty date is still today", ZDateTime.Today, header.EffectiveDutyDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2016, 05, 06);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Effective duty date is lodged date", declaration.JE_EntrySubmittedDate, header.EffectiveDutyDate);

			header.AddInfo.ZA_EFD = "030616";
			AssertEquals("Effective duty date is not empty", new ZDateTime(2016, 06, 03), header.EffectiveDutyDate);
		}

		public void TestWUVGetsUpdatedWhenInvoiceCurrencyChanges()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 1, 1), new ZDateTime(2050, 1, 1), 0.5m, helper.USDCurrency);

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2049, 1, 1);
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			var header = declaration.Invoices.AddNew();
			header.JZ_JZ_GroupInvoiceFK = declaration.JobComInvoiceGroupHeaders[0].PK;
			header.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line = header.JobComInvoiceLines.AddNew();

			line.JI_IsPackToBondForLine = true;
			line.JI_InvoiceQuantity = 10m;
			line.JI_LinePrice = 1000m;
			AssertEquals("WUV", 100m, line.WUV);

			header.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			AssertEquals("WUV", 200m, line.WUV);
		}

		public void TestSetDefaultForCMR()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);

			var invHeader = declaration.Invoices.AddNew();
			AssertEquals("Default for Valuation Basis", CMRValuationBasisList.Codes._1stPref_TransactionValue, invHeader.AddInfo.ZA_VALB_Hidden);
		}

		public void TestValidationObject()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;

			AssertEquals("Is SAC", true, testDec.IsSACWithoutLines);

			var invoice = testDec.Invoices.AddNew();
			AssertEquals("Validation Object for SAC Invoice", typeof(SACJobComInvoiceHeaderValidation), invoice.Validation.GetType());

			testDec.JE_MessageSubType = "FRM";
			AssertEquals("Is SAC", false, testDec.IsSACWithoutLines);
			AssertEquals("Validation Object for non-SAC Invoice", typeof(ImportJobComInvoiceHeaderValidation), invoice.Validation.GetType());
		}

		public void TestDontSendPackingCostIfNotMandatory()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			charge.J7_IsIncludedInITOT = true;
			invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 900;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.PackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("Packing is not allowed to send", 0m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is allowed to send", 100m, invoiceLine.JI_ForeignInlandFreight.Amount);

			charge.J7_IsIncludedInITOT = false;
			invoiceLine.JI_LinePrice = 800;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.UnpackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("Packing is not allowed to send", 100m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is allowed to send", 100m, invoiceLine.JI_ForeignInlandFreight.Amount);
		}

		public void TestDontSendFIFTIfNotMandatory()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			fIFT.J7_IsIncludedInITOT = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000;

			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.FreeOnBoard, invoice.ITOTIncoTerm);
			AssertEquals("FIFT is not allowed to send", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);

			fIFT.J7_IsIncludedInITOT = false;
			invoiceLine.JI_LinePrice = 900;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.PackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("FIFT is allowed to send", 100m, invoiceLine.JI_ForeignInlandFreight.Amount);
		}

		public void TestDontSendLandingChargeIfNotMandatory()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);
			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge oNS = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100);
			BaseJobComInvHeaderCharge lCH = invoice.Charges.AddNew(AUChargeCodeList.Codes.LandingCharges, 100);
			lCH.J7_IsIncludedInITOT = true;
			oFT.J7_IsIncludedInITOT = true;
			oNS.J7_IsIncludedInITOT = true;
			fIFT.J7_IsIncludedInITOT = true;
			pC.J7_IsIncludedInITOT = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.LandedIntoStore, invoice.ITOTIncoTerm);
			AssertEquals("Packing is not allowed to send", 0m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is not allowed to send", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("LCH is allowed to send", 100m, invoiceLine.JI_LandingCharges.Amount);
			AssertEquals("OFT is allowed to send", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("ONS is allowed to send", 100m, invoiceLine.JI_OverseasInsurance.Amount);

			lCH.J7_IsIncludedInITOT = false;
			invoiceLine.JI_LinePrice = 900;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.CostInsuranceAndFreight, invoice.ITOTIncoTerm);
			AssertEquals("Packing is not allowed to send", 0m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is not allowed to send", 0m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("LCH is not allowed to send", 0m, invoiceLine.JI_LandingCharges.Amount);
			AssertEquals("OFT is allowed to send", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("ONS is allowed to send", 100m, invoiceLine.JI_OverseasInsurance.Amount);

			oFT.J7_IsIncludedInITOT = false;
			oNS.J7_IsIncludedInITOT = false;
			fIFT.J7_IsIncludedInITOT = false;
			invoiceLine.JI_LinePrice = 600;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.PackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("Packing is not allowed to send", 0m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is allowed to send", 100m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("LCH is not allowed to send", 0m, invoiceLine.JI_LandingCharges.Amount);
			AssertEquals("OFT is allowed to send", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("ONS is allowed to send", 100m, invoiceLine.JI_OverseasInsurance.Amount);

			pC.J7_IsIncludedInITOT = false;
			invoiceLine.JI_LinePrice = 500;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: ITOT incoterm", Core.Constants.IncoTerms.UnpackedAtFactory, invoice.ITOTIncoTerm);
			AssertEquals("Packing is allowed to send", 100m, invoiceLine.JI_PackingCosts.Amount);
			AssertEquals("FIFT is allowed to send", 100m, invoiceLine.JI_ForeignInlandFreight.Amount);
			AssertEquals("LCH is not allowed to send", 0m, invoiceLine.JI_LandingCharges.Amount);
			AssertEquals("OFT is allowed to send", 100m, invoiceLine.JI_OverseasFreight.Amount);
			AssertEquals("ONS is allowed to send", 100m, invoiceLine.JI_OverseasInsurance.Amount);
		}

		public void TestExWorksApportion()
		{
			AssertApportion(CustomsChargeTypeList.Codes.ExWorks);
		}

		public void TestJZ_Discount()
		{
			AssertApportion(CustomsChargeTypeList.Codes.Discount);
		}

		public void TestJZ_ForeignInlandFreight()
		{
			AssertApportion(CustomsChargeTypeList.Codes.ForeignInlandFreight);
		}

		public void TestJZ_PackingCosts()
		{
			AssertApportion(CustomsChargeTypeList.Codes.PackingCost);
		}

		public void TestJZ_OverseasFreight()
		{
			AssertApportion(CustomsChargeTypeList.Codes.OverseasFreight);
		}

		public void TestJZ_OverseasInsurance()
		{
			AssertApportion(CustomsChargeTypeList.Codes.OverseasInsurance);
		}

		public void TestJZ_OtherCharge()
		{
			AssertApportion(CustomsChargeTypeList.Codes.OtherCharges);
		}

		public void TestJZ_Commission()
		{
			AssertApportion(CustomsChargeTypeList.Codes.Commission);
		}

		protected void AssertApportion(string chargeName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(chargeName, 100, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = false;

			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 5000;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			BaseJobComInvHeaderCharge invoiceChargeForInvoice1 = invoice1.Charges.AddNew(chargeName, 60);
			invoiceChargeForInvoice1.J7_Calc_IsIncludedInInvoiceAmount = false;

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 5000;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			declaration.ResumeApportionment();
			AssertEquals("No apportion", 0, invoice1.GroupCharges.Count);
			AssertEquals("Apportioned Charge", 40m, invoice2.GroupCharges[0].J7_Amount);

			invoiceChargeForInvoice1.J7_Amount = 0;
			invoiceChargeForInvoice1.J7_RX_NKCurrency = ZString.Empty;
			declaration.ResumeApportionment();
			AssertEquals("One apportioned Charge", 1, invoice1.GroupCharges.Count);
			AssertEquals("One Apportioned Charge", 1, invoice2.GroupCharges.Count);
			AssertEquals("Apportioned", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Apportioned Charge", 50m, invoice2.GroupCharges[0].J7_Amount);

			BaseJobComInvHeaderCharge invoiceChargeForInvoice2 = invoice2.Charges.AddNew(chargeName, 40m, JobDeclaration.LocalCurrencyConstantCode);
			invoiceChargeForInvoice2.J7_Calc_IsIncludedInInvoiceAmount = false;
			declaration.ResumeApportionment();
			AssertEquals("Apportioned", 60m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("No Apportion", 0, invoice2.GroupCharges.Count);

			invoice2.Charges.RemoveAndDeleteAll();
			declaration.ResumeApportionment();
			AssertEquals("One apportioned Charge", 1, invoice1.GroupCharges.Count);
			AssertEquals("One Apportioned Charge", 1, invoice2.GroupCharges.Count);
			AssertEquals("Apportioned", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Apportioned Charge", 50m, invoice2.GroupCharges[0].J7_Amount);
		}

		public void TestJZ_InvoiceAmountChangeLeadToReApportion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100, JobDeclaration.LocalCurrencyConstantCode);

			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 2000;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 2000;
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			declaration.ResumeApportionment();
			AssertEquals("Apportioned", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Apportioned", 50m, invoice2.GroupCharges[0].J7_Amount);

			invoice2.JZ_InvoiceAmount = 6000;
			declaration.ResumeApportionment();
			AssertEquals("JZ_Commission reapportioned", 25m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("ReApportioned", 75m, invoice2.GroupCharges[0].J7_Amount);
		}

		public void TestCalculateFOBValueWithDDP()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);

			header.JZ_InvoiceAmount = 10600;
			ZDecimal expectedFOB = 10600 - 10 - 100 - 200;
			AssertEquals("FOB Value / DTD ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateFOBValueWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);

			header.JZ_InvoiceAmount = 10600;

			var expectedFOB = new ZDecimal(10600 - 10 - 100);
			AssertEquals("FOB Value / CIF ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateFOBValueWithCFR()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			header.JZ_InvoiceAmount = 10600;
			var expectedFOB = new ZDecimal(10600 - 100);
			AssertEquals("FOB Value / CFR ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateBalanceWithInvalidCurrencyOnInlandFreight()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			var line1 = header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 10000;
			declaration.ResumeApportionment();
			AssertEquals("Initial Balance", new ZDecimal(10000), header.JZ_Calc_Balance);

			BaseJobComInvHeaderCharge fIFT = header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 1000);
			declaration.ResumeApportionment();
			AssertEquals("Balance after adding Inland Freight (defaulted currency)", 9000m, header.JZ_Calc_Balance);

			fIFT.J7_IsIncludedInITOT = true;
			declaration.ResumeApportionment();
			AssertEquals("Balance after adding Inland Freight (defaulted currency)", 10000m, header.JZ_Calc_Balance);
		}

		public void TestCalculateFOBValueWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			Assert("No FOB", header.JZ_Calc_FOBAmount == 0);
			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			var expectedFOB = new ZDecimal(10600);
			AssertEquals("FOB Value / FOB ", expectedFOB, header.JZ_Calc_FOBAmount);
		}

		public void TestCalculateRealInvoiceTotal()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			BaseJobComInvHeaderCharge oNS = header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);

			//FOB Incoterm
			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var expected = new ZDecimal(10600 - 200 - 300 + 1 - 5);
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);

			//LIS Incoterm
			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			expected = new ZDecimal(10600 - 200 - 300 - 100 - 10 - 200 + 1 - 5);
			AssertEquals("Real Invoice / DDP ", expected, header.InvoiceLineTotal);

			//CIF Incoterm
			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			expected = new ZDecimal(10600 - 200 - 300 - 100 - 10 + 1 - 5);
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);

			//C&R Incoterm
			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;
			header.Charges.RemoveAndDelete(oNS);
			expected = 10600 - 200 - 300 - 100 + 1 - 5;
			AssertEquals("Real Invoice / C&F ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateRealInvoiceTotalWithUFB()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 20);

			ZDecimal expected = 10600m - 300m + 5m - 20m;
			AssertEquals("Real Invoice / UFB ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUFB()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 5);
			header.Charges.AddNew(AUChargeCodeList.Codes.OtherCommission, 10);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge packingCost = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 90, JobDeclaration.LocalCurrencyConstantCode);
			packingCost.J7_IsIncludedInITOT = false;

			ZDecimal expected = 10600m + 90;
			declaration.ResumeApportionment();
			AssertEquals("CIF value / UFB ", expected, header.JZ_Calc_CIFAmount);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 240, JobDeclaration.LocalCurrencyConstantCode);
			oFT.J7_IsIncludedInITOT = false;

			expected = 10600m + 90 + 240;
			declaration.ResumeApportionment();
			AssertEquals("CIF value / UFB ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithEXW()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			var expected = new ZDecimal(10600 - 200 - 5 + 1);
			AssertEquals("Real Invoice / EXW ", expected, header.InvoiceLineTotal);

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300, JobDeclaration.LocalCurrencyConstantCode);

			expected = new ZDecimal(10600 - 200 - 5 + 1);
			AssertEquals("Real Invoice / EXW ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateRealInvoiceTotalWithPAF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			var expected = new ZDecimal(10600 - 200 - 5 + 1);
			AssertEquals("Real Invoice / PAF ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithPAF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			ZDecimal expected = 10600m;
			AssertEquals("CIF value / PAF ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = false;
			expected = 10600 + 100m;
			declaration.ResumeApportionment();
			AssertEquals("CIF value / PAF ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithUAF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			ZDecimal expected = 10600 - 5 + 1;
			AssertEquals("Real Invoice / UAF ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUAF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(AUChargeCodeList.Codes.BuyingCommission, 200);
			header.Charges.AddNew(AUChargeCodeList.Codes.Discount, 1);
			header.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 5);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = false;

			ZDecimal expected = 10600 + 40m;
			declaration.ResumeApportionment();
			AssertEquals("CIF Value / UAF ", expected, header.JZ_Calc_CIFAmount);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			oFT.J7_IsIncludedInITOT = false;
			expected = 10600 + 40 + 100m;
			declaration.ResumeApportionment();
			AssertEquals("CIF Value / UAF ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 1000);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			ZDecimal expected = 10600m - 100 - 200 - 5 + 1;
			AssertEquals("Real Invoice / FOB ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithFOB()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			ZDecimal expected = 10600m;
			AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge charge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, aUD.RX_Code);
			charge.J7_IsIncludedInITOT = false;

			expected = 10600m + 100;
			declaration.ResumeApportionment();
			AssertEquals("CIF / FOB ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			ZDecimal expected = 10600m - 100 - 200 - 300 - 5 + 1;
			AssertEquals("Real Invoice / CIP ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithCIP()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
			ZDecimal expected = 10600m;
			AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, aUD.RX_Code);
			declaration.ResumeApportionment();
			header.GroupCharges[0].J7_IsIncludedInITOT = false;

			expected = 10600m + 100;
			AssertEquals("CIF value / CIP ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithCFR()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			ZDecimal expected = 10600m - 100 - 200 - 400 - 5 + 1;
			AssertEquals("Real Invoice / CFR ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithCFR()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			ZDecimal expected = 10600m;
			AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, aUD.RX_Code);
			oNS.J7_IsIncludedInITOT = false;

			expected = 10600m + 100;
			declaration.ResumeApportionment();
			AssertEquals("CIF value / CFR ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithUCI()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			ZDecimal expected = 10600m - 200 - 300 - 400 - 5 + 1;
			AssertEquals("Real Invoice / UCI ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUCI()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			ZDecimal expected = 10600m;
			AssertEquals("CIF value / UCI ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge packingCost = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, aUD.RX_Code);
			packingCost.J7_IsIncludedInITOT = false;

			expected = header.JZ_Calc_FOBAmount + 400 + 300;
			AssertEquals("CIF value / UCI ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithUCF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			ZDecimal expected = 10600m - 100 - 200 - 400 - 5 + 1;
			AssertEquals("Real Invoice / UCI ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithUCF()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			ZDecimal expected = 10600m;
			AssertEquals("CIF value / UCI ", expected, header.JZ_Calc_CIFAmount);

			var groupHeader = header.Master as JobComInvoiceGroupHeader;
			BaseJobComInvHeaderCharge packingCost = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, aUD.RX_Code);
			packingCost.J7_IsIncludedInITOT = false;

			expected = header.JZ_Calc_FOBAmount + 400;
			AssertEquals("CIF value / UCI ", expected, header.JZ_Calc_CIFAmount);
		}

		public void TestCalculateRealInvoiceTotalWithCIF()
		{
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 1000);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			ZDecimal expected = 10600m - 100 - 200 - 300 - 400 - 40 - 5 + 1;
			AssertEquals("Real Invoice / CIF ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateRealInvoiceTotalWithLIS()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600);
			nonDutiableOtherCharge.J7_IsDutiable = false;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 500);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			ZDecimal expected = 10600m - 100 - 200 - 300 - 400 - 500 - 600 - 40 - 5 + 1;
			AssertEquals("Real Invoice / LIS ", expected, header.InvoiceLineTotal);
		}

		public void TestCalculateCIFWithLIS()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 10600;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 1);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 5);
			BaseJobComInvHeaderCharge nonDutiableOtherCharge = header.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 600);
			nonDutiableOtherCharge.J7_IsDutiable = false;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 200);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 40);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 300);
			header.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 1000);

			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			ZDecimal expected = 10600m - 1000 - 600 - 400 - 300;
			AssertEquals("FOB Value", expected, header.JZ_Calc_FOBAmount);

			expected = 10600 - 1000;
			AssertEquals("CIF value / LIS ", expected, header.JZ_Calc_CIFAmount);

			header.JZ_OverrideFOB = true;
			header.JZ_FOBValue = 8900m;

			AssertEquals("Effective FOB amount", 8900m, header.EffectiveFOBAmount);
			AssertEquals("Effective FOB amount", 9600m, header.EffectiveCIFAmount);
			AssertEquals("T&I", 1300m, header.JZ_Calc_TNI);
		}

		public void TestLinesEntered()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 0);

			line1.JI_LinePrice = 1000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 1000);

			line2.JI_LinePrice = 4000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 5000);

			line3.JI_LinePrice = 5000;
			Assert("Lines Entered", header.JZ_Calc_LinesEntered == 10000);
		}

		public void TestBalance()
		{
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var line3 = header.JobComInvoiceLines.AddNew();
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			header.JZ_InvoiceAmount = 10000;
			declaration.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 10000);

			line1.JI_LinePrice = 1000;
			declaration.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 9000);

			line2.JI_LinePrice = 4000;
			declaration.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 5000);

			line3.JI_LinePrice = 5000;
			declaration.ResumeApportionment();
			Assert("Lines Entered", header.JZ_Calc_Balance == 0);
		}

		public void TestBalanceGroupHeaderChargeAndHeaderCharge()
		{
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, aUD.RX_Code);

			header.JZ_InvoiceAmount = 1000;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200, aUD.RX_Code);

			ZDecimal expected = header.JZ_InvoiceAmount - 200;
			declaration.ResumeApportionment();
			AssertEquals("The balance should be Invoice Total less than cost of its own", expected, header.JZ_Calc_Balance);
		}

		public void TestCommissionTypeAfterLoaded()
		{
			header.JZ_CommissionType = JobComInvoiceHeader.CommissionType.Buying;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedHeader = factory2.Load<JobComInvoiceHeader>(header.PK);
			AssertEquals("Commission Type", JobComInvoiceHeader.CommissionType.Buying, loadedHeader.JZ_CommissionType);
		}

		public void TestAddInfoHasCommissionTypeAfterLoadedButIsHidden()
		{
			header.JZ_CommissionType = JobComInvoiceHeader.CommissionType.Buying;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadedHeader = factory2.Load<JobComInvoiceHeader>(header.PK);
			string commissionType = loadedHeader.AddInfo.ZA_CommissionType_Hidden;
			AssertEquals("Commission type should be in addinfo", "1", commissionType);
			AssertEquals("Add Info Line should be blank", "", loadedHeader.AddInfo.AddInfoLine);
		}

		public void TestValidateJZ_InvoiceAmount()
		{
			header.JZ_InvoiceAmount = 0.0m;
			header.Validation.ValidateJZ_InvoiceAmount();
			AssertEquals("Invoice with zero amount and no invoice number should not be an error", false, header.JZ_InvoiceAmountInfo.HasErrors());
			header.JZ_InvoiceNumber = "Test Invoice";
			header.Validation.ValidateJZ_InvoiceAmount();
			AssertEquals("Invoice with zero amount and invoice number should be an error", true, header.JZ_InvoiceAmountInfo.HasMessageErrors());
			header.JZ_InvoiceAmount = 0.01m;
			AssertEquals("Invoice Total greater than zero should not be an error", false, header.JZ_InvoiceAmountInfo.HasErrors());
		}

		public void TestValidateJZ_InvoiceNumber()
		{
			header.JZ_InvoiceNumber = "";
			header.Validation.ValidateJZ_InvoiceNumber();
			AssertEquals("Invoice Number is Required", true, header.JZ_InvoiceNumberInfo.HasMessageErrors());
			header.JZ_InvoiceNumber = "Test";
			AssertEquals("Invoice Number set, no message error expected", false, header.JZ_InvoiceNumberInfo.HasNotifications());
		}

		public void TestApportionChargesInGroupChargeCurrency()
		{
			declaration.JE_ExportDate = new ZDateTime(2003, 12, 2);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			new ZTestHelper(Factory).SetExchangeRate(new ZDateTime(2003, 12, 1), new ZDateTime(2003, 12, 3), 0.7123m, uSD);
			var allInvoicesGroup = declaration.JobComInvoiceGroupHeaders[0];

			var header = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceAmount = 300m;
			header.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			var header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			header2.JZ_InvoiceAmount = 700m;
			header2.JZ_RX_NKInvoice_Currency = uSD.RX_Code;

			allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 1000, JobDeclaration.LocalCurrencyConstantCode);

			ZDecimal expectedHeader1 = 1000m * (300m / 1000m);
			ZDecimal expectedHeader2 = 1000m * (700m / 1000m);
			declaration.ResumeApportionment();
			AssertEquals(expectedHeader1, header.GroupCharges[0].J7_Amount, 0.05m);
			AssertEquals(expectedHeader2, header2.GroupCharges[0].J7_Amount, 0.05m);
		}

		public void TestOtherChargesAffectFOBValue()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_InvoiceAmount = 4698.00m;

			BaseJobComInvHeaderCharge nonDutiable = header.Charges.AddNew(AUChargeCodeList.Codes.OtherCharges, 110, JobDeclaration.LocalCurrencyConstantCode);
			nonDutiable.J7_IsDutiable = false;

			header.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;

			ZDecimal expected = header.JZ_InvoiceAmount - 110;
			AssertEquals("NonDutiable Other Charges should affect the FOB value for LIS invoice", expected, header.JZ_Calc_FOBAmount);
		}

		public void TestMultipleApportions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today;

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var g1HeaderEXW = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var g1HeaderDDP = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1];
			var g1HeaderCIF = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[2];
			var g2HeaderUAF = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			var g2HeaderDDU = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[1];
			var g2HeaderFOB = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[2];
			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var group2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0];

			g1HeaderEXW.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			g1HeaderDDP.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			g1HeaderCIF.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			g2HeaderUAF.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			g2HeaderDDU.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			g2HeaderFOB.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			g1HeaderEXW.JZ_InvoiceAmount = 3201.57m;
			g1HeaderEXW.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			g1HeaderDDP.JZ_InvoiceAmount = 5746.75m;
			g1HeaderDDP.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			g1HeaderCIF.JZ_InvoiceAmount = 8000.00m;
			g1HeaderCIF.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			g2HeaderUAF.JZ_InvoiceAmount = 6120.04m;
			g2HeaderUAF.JZ_RX_NKInvoice_Currency = gBP.RX_Code;
			g2HeaderDDU.JZ_InvoiceAmount = 41251.73m;
			g2HeaderDDU.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			g2HeaderFOB.JZ_InvoiceAmount = 4999.99m;
			g2HeaderFOB.JZ_RX_NKInvoice_Currency = uSD.RX_Code;

			BaseJobComInvHeaderCharge lCH = group1.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 2320.44m, JobDeclaration.LocalCurrencyConstantCode);
			declaration.ResumeApportionment();
			Assert("G1HeaderEXW JZ_LandingCharges should not be apportioned a value", g1HeaderEXW.GroupCharges.Count == 0);
			Assert("G1HeaderDDP JZ_LandingCharges should be apportioned a value", g1HeaderDDP.GroupCharges[0].J7_Amount != 0.0m);
			Assert("G1HeaderCIF JZ_LandingCharges should not be apportioned a value", g1HeaderCIF.GroupCharges.Count == 0);
			Assert("G2HeaderUAF JZ_LandingCharges should not be apportioned a value", g2HeaderUAF.GroupCharges.Count == 0);
			Assert("G2HeaderDDU JZ_LandingCharges should be apportioned a value", g2HeaderDDU.GroupCharges[0].J7_Amount != 0.0m);
			Assert("G2HeaderFOB JZ_LandingCharges should not be apportioned a value", g2HeaderFOB.GroupCharges.Count == 0);

			AssertEquals("For this test G1HeaderDDP and G2HeaderDDU should have the same Currency", g1HeaderDDP.JZ_RX_NKInvoice_Currency, g2HeaderDDU.JZ_RX_NKInvoice_Currency);
			ZDecimal invoiceTotal = g1HeaderDDP.JZ_InvoiceAmountInLocalCurrency + g2HeaderDDU.JZ_InvoiceAmountInLocalCurrency;

			ZDecimal invoiceDDPPercent = g1HeaderDDP.JZ_InvoiceAmountInLocalCurrency / invoiceTotal;
			ZDecimal expectedDDPLandingChargesAmount = ZArchitecture.Core.Utilities.Round(lCH.J7_Amount * invoiceDDPPercent, 2);
			declaration.ResumeApportionment();
			AssertEquals("Invoice Landing Charges", expectedDDPLandingChargesAmount, g1HeaderDDP.GroupCharges[0].J7_Amount);

			ZDecimal invoiceDDUPercent = g2HeaderDDU.JZ_InvoiceAmountInLocalCurrency / invoiceTotal;
			ZDecimal expectedDDULandingChargesAmount = ZArchitecture.Core.Utilities.Round(lCH.J7_Amount * invoiceDDUPercent, 2);
			declaration.ResumeApportionment();
			AssertEquals(expectedDDULandingChargesAmount, g2HeaderDDU.GroupCharges[0].J7_Amount, 0.01m);//"Invoice Landing Charges",

			group1.Charges.AddNew(AUChargeCodeList.Codes.Discount, 2000.34m, JobDeclaration.LocalCurrencyConstantCode);
			group2.Charges.AddNew(AUChargeCodeList.Codes.Discount, 123.65m, JobDeclaration.LocalCurrencyConstantCode);

			var incoTermAndChargeFactory = g1HeaderEXW.IncoTermAndChargeFactory;
			declaration.ResumeApportionment();
			var discountChargeCodeChargeKey = incoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.Discount).ChargeCodeChargeKey;
			Assert("G1HeaderEXW JZ_Discount should be apportioned a value", g1HeaderEXW.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
			Assert("G1HeaderDDP JZ_Discount should be apportioned a value", g1HeaderDDP.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
			Assert("G1HeaderCIF JZ_Discount should be apportioned a value", g1HeaderCIF.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
			Assert("G2HeaderUAF JZ_Discount should be apportioned a value", g2HeaderUAF.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
			Assert("G2HeaderDDU JZ_Discount should be apportioned a value", g2HeaderDDU.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
			Assert("G2HeaderFOB JZ_Discount should be apportioned a value", g2HeaderFOB.GroupCharges.HasChargeWithCurrency(discountChargeCodeChargeKey));
		}

		public void TestJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			group1.JobComInvoiceGroupHeaders.AddNew();
			group1.JobComInvoiceHeaders.AddNew();
			AssertSame(declaration, group1.JobComInvoiceHeaders[0].JobDeclaration);
		}

		public void TestGetNature10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = group1.JobComInvoiceHeaders.AddNew();
			invoice.JZ_Nature10PackCount = 100;

			AssertEquals("Header is nature 10", JobComInvoiceHeader.NatureString.Nature10, invoice.Nature);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Nature 10 cannot be determined from Header level in CMR", JobComInvoiceHeader.NatureString.NotDetermined, invoice.Nature);
		}

		public void TestGetNature20()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = group1.JobComInvoiceHeaders.AddNew();
			invoice.JZ_BondPackCount = 200;

			AssertEquals("Header is nature 20", JobComInvoiceHeader.NatureString.Nature20, invoice.Nature);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Nature 10 cannot be determined from Header level in CMR", JobComInvoiceHeader.NatureString.NotDetermined, invoice.Nature);
		}

		public void TestGetNature30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;

			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoice = group1.JobComInvoiceHeaders.AddNew();

			AssertEquals("Header is nature 30", JobComInvoiceHeader.NatureString.Nature30, invoice.Nature);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Header is nature 30", JobComInvoiceHeader.NatureString.Nature30, invoice.Nature);
		}

		public void TestAddInfo()
		{
			var testAddInfo = "DTY=23.24*AMB=DVTQPOC*LCTE=404*ICN=C23000H";
			header.JZ_AddInfo = testAddInfo;
			AssertEquals("Declaration AddInfo Object Field - Duty", 23.24m, header.AddInfo.ZA_DTY);
			AssertEquals("Declaration AddInfo Object Field - Amber Processing", "DVTQPOC", header.AddInfo.ZA_AMB);
			AssertEquals("Declaration AddInfo Object Field - Luxury Car Tax Exemption", "404", header.AddInfo.ZA_LCTE);
			AssertEquals("Declaration AddInfo Object Field - Import Credit Number", "C23000H", header.AddInfo.ZA_ICN);
			AssertEquals("Declaration AddInfo Object Field - Origin", "", header.AddInfo.ZA_ORG);
			AssertEquals("Declaration AddInfo Object Field - Invoice Spirit Strength", 0m, header.AddInfo.ZA_ISS);
		}

		public void TestBalanceIsShownInInvoiceCurrency()
		{
			header.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			header.JZ_InvoiceAmount = 1000;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			declaration.ResumeApportionment();
			AssertEquals("Outstanding Balance", header.JZ_InvoiceAmount, header.JZ_Calc_Balance);
		}

		public void TestUpdateApportionedCharge()
		{
			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var group2 = group1.JobComInvoiceGroupHeaders.AddNew();
			var header = group2.JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceAmount = 1000m;
			header.JZ_RX_NKInvoice_Currency = "AUD";

			group1.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 200, JobDeclaration.LocalCurrencyConstantCode);
			group2.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, JobDeclaration.LocalCurrencyConstantCode);

			declaration.ResumeApportionment();
			ZDecimal real = decimal.Round(header.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.PackingCost, JobDeclaration.GetLocalCurrency()), 2);
			AssertEquals("Packing Costs in header", 100m, real);

			group2.Charges.RemoveAndDeleteAll();

			declaration.ResumeApportionment();
			real = decimal.Round(header.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.PackingCost, JobDeclaration.GetLocalCurrency()), 2);
			AssertEquals("Packing Costs in header", 200m, real);

			group1.Charges.RemoveAndDeleteAll();
			declaration.ResumeApportionment();
			real = decimal.Round(header.GroupCharges.GetCharge(CustomsChargeTypeList.Codes.PackingCost, JobDeclaration.GetLocalCurrency()), 2);
			AssertEquals("Packing Costs in header", 0m, real);
		}

		public void TestApportionAcrossExistingCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var group2 = group1.JobComInvoiceGroupHeaders.AddNew();
			var header1 = group2.JobComInvoiceHeaders.AddNew();
			var header2 = group2.JobComInvoiceHeaders.AddNew();
			var header3 = group2.JobComInvoiceHeaders.AddNew();

			header1.JZ_InvoiceAmount = 1000.0m;
			header1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			header2.JZ_InvoiceAmount = 1000.0m;
			header2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			header3.JZ_InvoiceAmount = 1000.0m;
			header3.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			header1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			BaseJobComInvHeaderCharge charge = group2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 150, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsIncludedInITOT = false;
			declaration.ResumeApportionment();
			AssertEquals("Header 1 Not apportioned", 0, header1.GroupCharges.Count);
			AssertEquals("Apportioned amount Header 2", 25.0m, header2.GroupCharges[0].J7_Amount);
			AssertEquals("Apportioned amount Header 3", 25.0m, header3.GroupCharges[0].J7_Amount);
		}

		public void TestChargesCurrencyIsInvoiceCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var group1 = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = group1.JobComInvoiceHeaders.AddNew();

			invoiceHeader.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = 1000m;

			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);

			AssertEquals("Charge currency should be an invoice currency", invoiceHeader.Invoice_Currency.RX_Code, invoiceHeader.Charges[0].J7_RX_NKCurrency);
		}

		public void TestApportionRoundingAddingUpToOrgCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			SetUpChargeAndCurrencyForHeader(groupHeader);//OverseasFreight : 100 USD
			var invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceHeader3 = groupHeader.JobComInvoiceHeaders.AddNew();
			SetUpChargeAndCurrencyForHeader(invoiceHeader1, 50m);
			SetUpChargeAndCurrencyForHeader(invoiceHeader2, 25m);
			SetUpChargeAndCurrencyForHeader(invoiceHeader3, 25m);
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header 2 Apportioned Charge", 25m, invoiceHeader2.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice Header 3 Apportioned Charge", 25m, invoiceHeader3.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice Header 1 Apportioned Charge", 50m, invoiceHeader1.GroupCharges[0].J7_Amount);

			ZDecimal realTotal = invoiceHeader1.GroupCharges[0].J7_Amount + invoiceHeader3.GroupCharges[0].J7_Amount + invoiceHeader2.GroupCharges[0].J7_Amount;
			declaration.ResumeApportionment();
			AssertEquals("Invoice Header 1 Apportioned Charge", 100m, realTotal);
		}

		public void TestApportionRoundingAddingUpToOrgCharge2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.Today;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			SetUpChargeAndCurrencyForHeader(groupHeader);//OverseasFreight : 100 USD
			var invoiceHeader1 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceHeader3 = groupHeader.JobComInvoiceHeaders.AddNew();
			declaration.ResumeApportionment();

			SetUpChargeAndCurrencyForHeader(invoiceHeader1, 1m);
			SetUpChargeAndCurrencyForHeader(invoiceHeader2, 1m);
			SetUpChargeAndCurrencyForHeader(invoiceHeader3, 1m);
			declaration.ResumeApportionment();
			ZDecimal realTotal =
				invoiceHeader1.GroupCharges[0].J7_Amount
				+ invoiceHeader2.GroupCharges[0].J7_Amount
				+ invoiceHeader3.GroupCharges[0].J7_Amount;

			AssertEquals("Invoice Header 1 Apportioned Charge", 100m, realTotal);
		}

		public void TestFOBWithGroupFreightCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var header1 = groupHeader.JobComInvoiceHeaders.AddNew();

			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, aUD.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			declaration.ResumeApportionment();
			AssertEquals("FOB value", 900m, header1.JZ_Calc_FOBAmount);
			AssertEquals("Balance", 1000m, header1.JZ_Calc_Balance);
		}

		public void TestFOBWithGroupDiscountCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var header1 = groupHeader.JobComInvoiceHeaders.AddNew();

			header1.JZ_InvoiceAmount = 1000m;
			header1.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100, aUD.RX_Code);
			declaration.ResumeApportionment();
			AssertEquals("FOB value", 900m, header1.JZ_Calc_FOBAmount);
			AssertEquals("Balance", 1000m, header1.JZ_Calc_Balance);
		}

		public void TestComplexFOBCustomsFactor()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_ExportDate = new ZDateTime(2003, 11, 12);
			testJobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2003, 11, 16);
			var groupHeader = testJobDeclaration.JobComInvoiceGroupHeaders[0];
			SetValidExchangeRate(uSD, testJobDeclaration.JE_ExportDate);
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_InvoiceAmount = 262.53m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = uSD.RX_Code;

			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 46.33m, uSD.RX_Code);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 60, uSD.RX_Code);
			oFT.J7_IsIncludedInITOT = false;
			invoiceHeader.JobDeclaration.ResumeApportionment();

			AssertEquals("FOB Amount", 262.53m, invoiceHeader.JZ_Calc_FOBAmount);
			AssertEquals("CIF Amount", 322.53m, invoiceHeader.JZ_Calc_CIFAmount);
			AssertEquals("Customs Factor", 0.84999676m, invoiceHeader.ValuationFactor);
		}

		public void TestFOBCalculationForPAFIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2001, 01, 08), new ZDateTime(2001, 01, 28), 0.5706m);
			helper.CreateEntryFromDeliveranceN10S_PAF();
			var declaration = helper.Declaration;
			declaration.DoMerge();

			AssertEquals("FOB value for PAF incoterm", 47.32m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("Invoice line total", 42.10m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestFOBCalculationForUFBIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 12, 07), new ZDateTime(2001, 02, 02), 1190.23m);
			helper.CreateEntryFromDeliveranceN10S_UFB();
			var declaration = helper.Declaration;
			declaration.DoMerge();

			AssertEquals("FOB value for UFB incoterm", 69608940.0m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("Invoice line total", 68601910m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestFOBCalculationForUAFIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2001, 01, 15), new ZDateTime(2001, 01, 18), 1.1404m, helper.EURCurrency);
			helper.CreateEntryFromDeliveranceN10A_UAF();
			var declaration = helper.Declaration;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.DoMerge();

			AssertEquals("FOB value for UAF incoterm", 9684.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("Invoice line total", 9390.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestFOBCalculationForLISIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2001, 10, 13), new ZDateTime(2001, 10, 24), 1m);
			helper.CreateEntryFromDeliveranceN20S();
			var declaration = helper.Declaration;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.DoMerge();

			AssertEquals("FOB value for LIS incoterm", 10881.15m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("CIF value for LIS incoterm", 11451.15m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_CIFAmount);
			AssertEquals("Invoice line total", 12051.15m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestFOBCalculationForUCFIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 10, 08), new ZDateTime(2000, 10, 16), 0.5494m);
			helper.CreateEntryFromDeliveranceN10_UCF();
			var declaration = helper.Declaration;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.DoMerge();

			AssertEquals("FOB value for UCF incoterm", 64720.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("CIF value for UCF incoterm", 67320.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_CIFAmount);
			AssertEquals("Invoice line total", 66420.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestFOBCalculationForUCIIncoTerms()
		{
			var helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(new ZDateTime(2000, 10, 08), new ZDateTime(2000, 10, 16), 0.5494m);
			helper.CreateEntryFromDeliveranceN10_UCI();
			var declaration = helper.Declaration;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.DoMerge();

			AssertEquals("FOB value for UCF incoterm", 64720.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBAmount);
			AssertEquals("CIF value for UCF incoterm", 67320.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_CIFAmount);
			AssertEquals("Invoice line total", 66420.00m, declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].InvoiceLineTotal);
		}

		public void TestDefaultValuationBasisIfPossible1()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USLAX";
			var supplier = Factory.New<OrgHeader>();
			var link1 = supplier.BuyerLinks.AddNew(importer);
			link1.OL_RelatedParty = "Y";
			link1.OL_ValuationBasis = "IG";
			var link2 = supplier.BuyerLinks.AddNew(importer);
			link2.OL_RelatedParty = "N";
			link2.OL_RN_NKImporterCountry = "AU";
			link2.OL_ValuationBasis = "CV";

			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "IG", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);
			header.JZ_OH_Supplier = ZGuid.Empty;
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "CV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
		}

		public void TestDefaultValuationBasisIfPossible2()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";
			var supplier = Factory.New<OrgHeader>();
			var link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = importer.PK;
			link.OL_ValuationBasis = "DV";
			link.OL_RelatedParty = "N";

			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Valuation Basis", "DV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);

			var supplier2 = Factory.New<OrgHeader>();
			link.OL_ValuationBasis = "FB";
			link.OL_RelatedParty = "Y";
			header.JZ_OH_Supplier = supplier2.PK;
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "FB", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);

			header.JZ_OH_Supplier = supplier2.PK;
			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
		}

		public void TestDefaultValuationBasisWithInvalidBuyerLinkValue()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";
			var supplier = Factory.New<OrgHeader>();
			var link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = importer.PK;
			link.OL_ValuationBasis = "UT";

			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
		}

		public void TestDefaultValuationBasisWithNonPersistentDeclaration()
		{
			var nonPersistentDeclaration = Factory.New<JobDeclaration>();
			nonPersistentDeclaration.MakeNonPersistent();
			header.JZ_JE = nonPersistentDeclaration.PK;
			var supplier = Factory.New<OrgHeader>();
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "AUSYD";
			var link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_ValuationBasis = "DV";
			link.OL_RelatedParty = "Y";
			link.OL_RX_NKDefaultCurrency = "USD";

			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("Currency", ZString.Empty, header.JZ_RX_NKInvoice_Currency);
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("Valuation Basis", "TV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "N", header.AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("Currency", ZString.Empty, header.JZ_RX_NKInvoice_Currency);
			header.JZ_OH_Buyer = buyer.PK;
			AssertEquals("Valuation Basis", "DV", header.AddInfo.ZA_VALB_Hidden);
			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);
			AssertEquals("Currency", "USD", header.JZ_RX_NKInvoice_Currency);
		}

		public void TestDefaultSupplierAddressIfPossible()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.Addresses.AddNewMainAddress();
			supplier.Addresses.AddNew(OrgAddressType.Miscellaneous, false);
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("JZ_OA_SupplierAddress default to supplier's main address", supplier.MainAddress.PK, header.JZ_OA_SupplierAddress);
		}

		public void TestJZ_OH_Supplier()
		{
			var property = DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.JZ_OH_Supplier));
			AssertEquals("Caption", "Supplier", property.Caption);
			AssertEquals("FullDescription", JobComInvoiceHeader.SupplierFullDescription, property.FullDescription);
		}

		public void TestJZ_OA_SupplierAddress()
		{
			var property = DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.JZ_OA_SupplierAddress));
			AssertEquals("Caption", "Supplier Address", property.Caption);
			AssertEquals("FullDescription", JobComInvoiceHeader.SupplierFullDescription, property.FullDescription);
		}

		public void TestRelatedPartyUnchangedOnImportInvoice()
		{
			var helper = new ZTestHelper(Factory);
			helper.CreateEntryFromDeliveranceN10_UCF();
			var declaration = helper.Declaration;
			header.AddInfo.ZA_HeaderREL_Hidden = "Y";
			header.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;

			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);
			declaration.Invoices.Add(header);
			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);
			declaration.Factory.Save();
			AssertEquals("Related Party", "Y", header.AddInfo.ZA_HeaderREL_Hidden);
		}

		public void TestDefaultOrgFromConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "USLAX";
			var testDec = Factory.New<JobDeclaration>();
			var testHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("ORG", "", testHeader.AddInfo.ZA_ORG);
			testHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("ORG", "US", testHeader.AddInfo.ZA_ORG);
		}

		public void TestTILVHeader()
		{
			header.JZ_InvoiceAmount = 1100m;
			header.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			header.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			header.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100);

			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 500m;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 500m;

			line1.AddInfo.ZA_TILV = "80AUD";
			line2.AddInfo.ZA_TILV = "20AUD";

			AssertEquals("TILV aggregate", "100.00 AUD", header.AggregatedTILV.ToString());
		}

		public void TestHasAPiecesCountHasBeenEnteredForThisInvoice()
		{
			Assert("!HasAPiecesCountHasBeenEnteredForThisInvoice", !header.HasAPiecesCountHasBeenEnteredForThisInvoice);
			header.AddInfo.ZA_PiecesForRelease_Hidden = 10;
			Assert("HasAPiecesCountHasBeenEnteredForThisInvoice", header.HasAPiecesCountHasBeenEnteredForThisInvoice);
			header.AddInfo.ZA_PiecesForRelease_Hidden = 0;
			Assert("!HasAPiecesCountHasBeenEnteredForThisInvoice", !header.HasAPiecesCountHasBeenEnteredForThisInvoice);
			header.AddInfo.ZA_PiecesForBond_Hidden = 10;
			Assert("HasAPiecesCountHasBeenEnteredForThisInvoice", header.HasAPiecesCountHasBeenEnteredForThisInvoice);
		}

		public void TestDefaultCommodityCodeForInvoiceLines()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = testDec.Invoices.AddNew();
			var supplier = Factory.New<OrgHeader>();
			var commodity1 = Factory.LoadTop1<RefCommodityCode>(new ZQuery());
			var commodity2 = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, commodity1.PK));

			supplier.MiscServ.OM_RH_NKCMMainImportCmdty = commodity1.RH_Code;
			supplier.MiscServ.OM_RH_NKCMMainExportCmdty = commodity2.RH_Code;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("InvoiceLine's commodity code remains as empty as this is Import Job", ZString.Empty, invoiceLine.JI_RH_NKCommodity_Code);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = supplier.PK;
			AssertEquals("InvoiceLine's commodity code should default from Supplier's Export Code", commodity2.RH_Code, invoiceLine.JI_RH_NKCommodity_Code);
		}

		public void TestJZ_Calc_TNIWhenThereIsTILV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew("OFT", 120m, "AUD");
			AssertEquals("JZ_Calc_TNI", 120m, invoice.JZ_Calc_TNI);
			AssertEquals(JobDeclaration.GetLocalCurrency().PK, invoice.JZ_RX_Calc_TNICurrency);

			invoice.AddInfo.ZA_TILV = "30NZD";
			AssertEquals("JZ_Calc_TNI", 30m, invoice.JZ_Calc_TNI);
			AssertEquals(Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD").PK, invoice.JZ_RX_Calc_TNICurrency);
		}

		public void TestJZ_RX_Calc_TNICurrency()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_RL_NKClosestPort = "HKHKG";

			var orgMiscServ = supplier.MiscServ;
			orgMiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.HongKong;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Precondition: port of loading should be 'HKHKG'", "HKHKG", declaration.JE_RL_NKPortOfLoading);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			AssertEquals("Precondition: currency should be defaulted from country of Supplier", Core.Constants.CurrencyCodes.HongKong, invoice.JZ_RX_NKInvoice_Currency);

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120m, Core.Constants.CurrencyCodes.HongKong);
			AssertEquals("JZ_Calc_TNI", 120m, invoice.JZ_Calc_TNI);

			var hongKongDollars = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.HongKong);

			AssertEquals("TNI Currency should be Hong Kong dollars", hongKongDollars.PK, invoice.JZ_RX_Calc_TNICurrency);

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("TNI Currency should be Hong Kong dollars", hongKongDollars.PK, invoice.JZ_RX_Calc_TNICurrency);
		}

		public void TestAggregatedTILV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.Charges.AddNew("OFT", 120m, "AUD");

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 5000m;
			invoiceLine1.AddInfo.ZA_TILV = "20AUD";

			AssertEquals("AggregatedTILV", 20m, invoice.AggregatedTILV.Amount);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;
			declaration.ResumeApportionment();
			AssertEquals("AggregatedTILV including apportioned TILV", 120m, invoice.AggregatedTILV.Amount);
		}

		#region AQIS Documents

		public void TestAQISDocuments()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISDocuments.Count);

			var document = invoiceHeader.AQISDocuments.AddNew();
			document.Number = "1234";
			document.Type = "Type";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Type/1234", invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden);
		}

		public void TestAQISDocumentsWithOneValueInAddInfo()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISDocuments.Count);

			var document = invoiceHeader.AQISDocuments.AddNew();
			document.Type = "TT2";
			document.Number = "Num2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
		}

		public void TestAQISDocumentsWithMultipleValuesInAddInfo()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden = "TT1/Num1,TT2/Num2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISDocuments.Count);

			var document = invoiceHeader.AQISDocuments.AddNew();
			document.Type = "TT3";
			document.Number = "Num3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISDocuments.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT1/Num1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT3/Num3"));
		}

		#endregion

		#region AQIS Premises Id And Processing Types

		public void TestAQISPremisesIdAndProcessingTypes()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			var premisesIdAndProcessingType = invoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem1";
			premisesIdAndProcessingType.ProcessingType = "Process";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Prem1/Process", invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden);
		}

		public void TestAQISPremisesIdAndProcessingTypesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			var premisesIdAndProcessingType = invoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem2";
			premisesIdAndProcessingType.ProcessingType = "Process2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
		}

		public void TestAQISPremisesIdAndProcessingTypesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisPremises.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden = "Prem1/Process1,Prem2/Process2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			var premisesIdAndProcessingType = invoiceHeader.AQISPremisesIdAndProcessingTypes.AddNew();
			premisesIdAndProcessingType.PremisesId = "Prem3";
			premisesIdAndProcessingType.ProcessingType = "Process3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISPremisesIdAndProcessingTypes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem1/Process1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem2/Process2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPremIdProcessType_Hidden.Contains("Prem3/Process3"));
		}

		#endregion

		#region AQIS Commodity Codes

		public void TestAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISCommodityCodes.Count);

			var commodityCode = invoiceHeader.AQISCommodityCodes.AddNew();
			commodityCode.Code = "1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "1", invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		public void TestAQISCommodityCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden = "1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISCommodityCodes.Count);

			var commodityCode = invoiceHeader.AQISCommodityCodes.AddNew();
			commodityCode.Code = "2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
		}

		public void TestAQISCommodityCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISCommodityCodes.Count);

			var commodityCode = invoiceHeader.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISCommodityCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden.Contains("1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden.Contains("2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden.Contains("3"));
		}

		public void TestReBuildAQISCommodityCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisCommodity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden = "1,2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISCommodityCodes.Count);

			var commodityCode = invoiceHeader.AQISCommodityCodes.AddNew();
			commodityCode.Code = "3";
			invoiceHeader.AddInfo.ReBuildAQISCommodityCodes();
			AssertEquals("Add Info Value", "1,2,3", invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden);
		}

		#endregion

		#region AQIS Entity Ids

		public void TestAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISEntityIds.Count);

			var entityId = invoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		public void TestAQISEntityIdsWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISEntityIds.Count);

			var entityId = invoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
		}

		public void TestAQISEntityIdsWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISEntityIds.Count);

			var entityId = invoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISEntityIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISEntityIds()
		{
			TestCaseHelper.ClearTable(CMRAqisEntity.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISEntityIds.Count);

			var entityId = invoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = "Code3";
			invoiceHeader.AddInfo.ReBuildAQISEntityIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden);
		}

		#endregion

		#region AQIS Permit Ids

		public void TestAQISPermitIds()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISPermitIds.Count);

			var permitId = invoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		public void TestAQISPermitIdsWithOneValueInAddInfo()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISPermitIds.Count);

			var permitId = invoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
		}

		public void TestAQISPermitIdsWithMultipleValuesInAddInfo()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISPermitIds.Count);

			var permitId = invoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISPermitIds.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISPermitIds()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISPermitIds.Count);

			var permitId = invoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = "Code3";
			invoiceHeader.AddInfo.ReBuildAQISPermitIds();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceHeader.AddInfo.ZA_AQISPermitIds_Hidden);
		}

		#endregion

		#region AQIS Producer Codes

		public void TestAQISProducerCodes()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("No values in collection", 0, invoiceHeader.AQISProducerCodes.Count);

			var producerCode = invoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", "Code1", invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		public void TestAQISProducerCodesWithOneValueInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1";
			AssertEquals("One value in the collection", 1, invoiceHeader.AQISProducerCodes.Count);

			var producerCode = invoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = "Code2";
			AssertEquals("Two values in the collection", 2, invoiceHeader.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
		}

		public void TestAQISProducerCodesWithMultipleValuesInAddInfo()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISProducerCodes.Count);

			var producerCode = invoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			AssertEquals("Two values in the collection", 3, invoiceHeader.AQISProducerCodes.Count);

			Factory.Save();
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code1"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code2"));
			AssertEquals("Add Info Value", true, invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden.Contains("Code3"));
		}

		public void TestReBuildAQISProcducerCode()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden = "Code1,Code2";
			AssertEquals("One value in the collection", 2, invoiceHeader.AQISProducerCodes.Count);

			var producerCode = invoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = "Code3";
			invoiceHeader.AddInfo.ReBuildAQISProducerCodes();
			AssertEquals("Add Info Value", "Code1,Code2,Code3", invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden);
		}

		#endregion

		public void TestAQISLoadingEstablishmentLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();

			AssertEquals("DocAddressType", MasterFiles.Integration.DocAddressType.AQISLoadingEstablishment, invoiceHeader.AQISLoadingEstablishmentLocation.DocAddressType);
		}

		public void TestCleanUnnecessaryQuarantineValuesOnSaving()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;

			var loadingEstablishment = invoiceHeader.AQISLoadingEstablishmentLocation;
			loadingEstablishment.E2_AddressOverride = true;
			loadingEstablishment.E2_GovRegNum = "123456";

			quarantineExDocHeader.QH_LoadingDate = ZDate.BrettsBirthday;
			declaration.OnSaving();

			quarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			declaration.OnSaving();
			CombineAssertions("ProduceType is not SkinsAndHides", () =>
			{
				Assert("E2_AddressOverride", !loadingEstablishment.E2_AddressOverride);
				Assert("E2_OA_Address", loadingEstablishment.E2_OA_Address.IsEmpty);
				Assert("E2_GovRegNum", loadingEstablishment.E2_GovRegNum.IsEmpty);
				Assert("QH_LoadingDate", quarantineExDocHeader.QH_LoadingDate.IsEmpty);
			});
		}

		public void TestQuarantineExDocHeaderCreation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertNull("Not a Quarantine Declaration", invoiceHeader.QuarantineExDocHeader);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var quarantineExDocHeader = invoiceHeader.QuarantineExDocHeader;
			AssertNotNull("Is Quarantine Declaration so create", invoiceHeader.QuarantineExDocHeader);
			Assert("Is registered editable child", invoiceHeader.IsRegisteredEditableChildObject(quarantineExDocHeader));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);
			AssertEquals(quarantineExDocHeader.PK, headerReloaded.QuarantineExDocHeader.PK);
		}

		public void TestGetDocAddressRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var docAddress = (IDocAddresses)invoiceHeader;
			AssertType<AQISLoadingEstablishmentAddressRequirement>(docAddress.GetDocAddressRequirement(MasterFiles.Integration.DocAddressType.AQISLoadingEstablishment));
		}

		public void TestIfMergeIsRequiredWhenLandedCostingExchangeRateChanged()
		{
			header.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			var headerLoaded = declarationLoaded.Invoices[0];
			headerLoaded.JZ_InvoiceCurrLandedCostExRate = 0.9526m;
			Assert("Merge is not required", !declarationLoaded.MergeManager.RequiresMerge);
		}

		#region Implementation

		RefCurrency aUD;
		RefCurrency uSD;
		RefCurrency jPY;
		RefCurrency nZD;
		RefCurrency gBP;
		JobDeclaration declaration;
		JobComInvoiceHeader header;
		JobComInvoiceGroupHeader groupHeader;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			header = groupHeader.JobComInvoiceHeaders.AddNew();

			aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			jPY = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			nZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			gBP = RefCurrency.LoadFromCurrencyCode(Factory, "GBP");
			SetValidExchangeRate(jPY);
			SetValidExchangeRate(uSD);
			SetValidExchangeRate(nZD);
			SetValidExchangeRate(gBP);
			//SetValidExchangeRate(EUR);
			distributeByForExport = CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		protected void SetUpChargeAndCurrencyForHeader(JobComInvoiceHeader invoiceHeader, ZDecimal amount)
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = amount;
		}

		protected void SetUpChargeAndCurrencyForHeader(JobComInvoiceGroupHeader groupHeader)
		{
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, uSD.RX_Code);
		}

		//		protected void AssertIncoTermCurrencyReadOnly(JobComInvoiceHeader Header,
		//			string IncoTerm,
		//			bool OverseasFreight,
		//			bool OverseasInsurance,
		//			bool ExWorks,
		//			bool ForeignInlandFreight,
		//			bool PackingCosts,
		//			bool LandingCharges,
		//			bool OtherCharges1,
		//			bool OtherCharges2,
		//			bool Discount,
		//			bool FOBAmount,
		//			bool CIFAmount,
		//			bool TAndI)
		//		{
		//			Header.JZ_IncoTerm = IncoTerm;
		//			AssertEquals("JZ_OverseasFreightInfo.ReadOnly on IncoTerm : " + IncoTerm, OverseasFreight, Header.JZ_OverseasFreightInfo.ReadOnly);
		//			AssertEquals("JZ_OverseasInsuranceInfo.ReadOnly on IncoTerm : " + IncoTerm, OverseasInsurance, Header.JZ_OverseasInsuranceInfo.ReadOnly);
		//			AssertEquals("JZ_ExWorksInfo.ReadOnly on IncoTerm : " + IncoTerm, ExWorks, Header.JZ_ExWorksAmountInfo.ReadOnly);
		//			AssertEquals("JZ_ForeignInlandFreightInfo.ReadOnly on IncoTerm : " + IncoTerm, ForeignInlandFreight, Header.JZ_ForeignInlandFreightInfo.ReadOnly);
		//			AssertEquals("JZ_PackingCostsInfo.ReadOnly on IncoTerm : " + IncoTerm, PackingCosts, Header.JZ_PackingCostsInfo.ReadOnly);
		//			AssertEquals("JZ_LandingChargesInfo.ReadOnly on IncoTerm : " + IncoTerm, LandingCharges, Header.JZ_LandingChargesInfo.ReadOnly);
		//			AssertEquals("JZ_OtherCharges1Info.ReadOnly on IncoTerm : " + IncoTerm, OtherCharges1, Header.JZ_OtherCharges1Info.ReadOnly);
		//			AssertEquals("JZ_OtherCharges2Info.ReadOnly on IncoTerm : " + IncoTerm, OtherCharges2, Header.JZ_OtherCharges2Info.ReadOnly);
		//			AssertEquals("JZ_DiscountInfo.ReadOnly on IncoTerm : " + IncoTerm, Discount, Header.JZ_DiscountInfo.ReadOnly);
		//			AssertEquals("JZ_FOBAmountInfo.ReadOnly on IncoTerm : " + IncoTerm, FOBAmount, Header.JZ_Calc_FOBAmountInfo.ReadOnly);
		//			AssertEquals("JZ_CIFAmountInfo.ReadOnly on IncoTerm : " + IncoTerm, CIFAmount, Header.JZ_CIFAmountInfo.ReadOnly);
		//			AssertEquals("JZ_Calc_TNIInfo.ReadOnly on IncoTerm : " + IncoTerm, TAndI, Header.JZ_Calc_TNIInfo.ReadOnly);
		//		}

		protected ZGuid CurrencyFromCode(string code)
		{
			return Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, code).PK;
		}

		protected OrgAddress GetValidDepotAddress()
		{
			var filter = new ZQuery(OrgHeaderSchema.OH_IsPackDepot, true);
			var header = Factory.New<OrgHeader>();
			header.OH_IsPackDepot = true;
			return header.Addresses.AddNew();
		}

		protected void SetValidExchangeRate(RefCurrency currency)
		{
			SetValidExchangeRate(currency, ZDateTime.Today);
		}

		protected void SetValidExchangeRate(RefCurrency currency, ZDateTime dateForExchange)
		{
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, currency.RX_Code);
			var rate = Factory.LoadTop1<RefExchangeRate>(filter);
			if (rate == null)
			{
				rate = Factory.New<RefExchangeRate>();
				rate.RE_RX_NKExCurrency = currency.RX_Code;
				rate.RE_SellRate = 1.0m;
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			}
			rate.RE_StartDate = dateForExchange;
			rate.RE_ExpiryDate = dateForExchange;
		}

		#endregion

	}
}
