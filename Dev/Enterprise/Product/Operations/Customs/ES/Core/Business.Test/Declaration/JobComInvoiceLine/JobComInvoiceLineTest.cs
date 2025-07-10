using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLine))]
class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	public void TestGetWarningBeforeBeingDeleted_HasBeenLodgedAtCustoms()
	{
		const string warningMessage = "The selected Invoice Line has been already declared. Entry lines cannot be deleted from a declared or canceled Entry.";

		var entryInstuction = dec.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstuction.PK;
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstuction.PK;

		CombineAssertions(() =>
		{
			foreach (var entryStatus in entryHeader.LodgedAtCustomsEntryStatus)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				AssertEquals($"EntryStatus {entryStatus}", warningMessage, invoiceLine.GetWarningBeforeBeingDeleted());
			}
		});
	}

	public void TestGetWarningBeforeBeingDeleted_IsWaitingForResponse()
	{
		const string warningMessage = "The selected Invoice Line has been already declared. Entry lines cannot be deleted from a declared or canceled Entry.";

		var entryInstuction = dec.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstuction.PK;
		var entryHeader = dec.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstuction.PK;

		entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
		AssertEquals(warningMessage, invoiceLine.GetWarningBeforeBeingDeleted());
	}

	public void TestVATIGICTypeCaption()
	{
		CombineAssertions(() =>
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.ZG_DestinationState = "zz";
			AssertEquals("VAT Type when no Canary Island", "VAT Type", invoiceLine.VATIGICTypeCaption.Caption);

			dec.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertEquals("IGIC Type when Canary Island", "IGIC Type", invoiceLine.VATIGICTypeCaption.Caption);

			invoiceLine.JI_JZ = ZGuid.Missing;
			AssertEquals("VAT Type when no Invoice Header", "VAT Type", invoiceLine.VATIGICTypeCaption.Caption);
		});
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be ES", Core.Constants.CountryCodes.Spain, InvoiceLine.CustomsCountryCode);
	}

	public void TestVehicleRelationship()
	{
		AssertEquals("VehicleRelationship should be Many", VehicleRelationshipType.Many, InvoiceLine.VehicleRelationship);
	}

	public void TestIsSupportEmptyPackType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders.AddNew();
		var entryLine1 = entryHeader.AllEntryLines.AddNew();
		entryLine1.CL_LineNumber = 1;

		var entryLine2 = entryHeader.AllEntryLines.AddNew();
		entryLine2.CL_LineNumber = 2;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var pack = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		pack.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
		pack.CW_PackQty = 15;

		var npbos = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
		var npbo = npbos.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault();
		npbo.IsLinked = true;
		npbo.PackQty = 0;
		npbo.Validation.ValidatePackQty();
		AssertHasMessageErrorContaining("Message error when first entry line and PackQty = 0", npbo.PackQtyInfo, "have not entered");

		npbo.PackQty = 14;
		npbo.Validation.ValidatePackQty();
		AssertNoMessageErrorContaining("No message error when PackQty > 0", npbo.PackQtyInfo, "have not entered");

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		var npbos2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly;
		var npbo2 = npbos2.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault();
		npbo2.IsLinked = true;
		npbo2.PackQty = 0;
		npbo2.Validation.ValidatePackQty();
		AssertNoMessageErrorContaining("No message error when CL_LineNumber != 1 and PackQty = 0", npbo2.PackQtyInfo, "have not entered");
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Spain, partDetails.CustomsCountryCode);
			AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestAutoFillExciseBox()
	{
		#region Setup

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

		var countryCode = Core.Constants.CountryCodes.Spain;
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var expTariffType = helper.CreateTariffType(countryCode, "EXP");
		var esexcType = helper.CreateTariffType(countryCode, "ESEXC");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();

		var tariffWithMultipleExcises = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11113333", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff2 = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11114444", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var exporttariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11116666", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(tariffExcise, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "4.5*[MIL]");
		var tariffExcise2 = helper.LoadOrCreateNewTariff(countryCode, esexcType.PK, "0A1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "0A1", rateType.PK);
		helper.CreateRate(tariffExcise2, rateCode2.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "5*[MIL]");
		Factory.Save();

		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariffWithMultipleExcises.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise2.PK, impTariffType.PK, tariffWithMultipleExcises.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise2.PK, impTariffType.PK, tariff2.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffExcise.PK, expTariffType.PK, exporttariff.ZZ1_TariffCode);
		Factory.Save();

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_FormattedTariff = tariffWithMultipleExcises.ZZ1_TariffCode;
			AssertEquals("Not Auto-Filled ZG_ExciseCode, tariff contains more than 1 ExciseCode", ZString.Empty, invoiceLine.ZG_ExciseCode);

			invoiceLine.JI_FormattedTariff = tariff.ZZ1_TariffCode;
			AssertEquals("Auto-Filled ZG_ExciseCode if tariff contains only 1 ExciseCode", "0A0", invoiceLine.ZG_ExciseCode);

			invoiceLine.JI_FormattedTariff = tariffWithMultipleExcises.ZZ1_TariffCode;
			AssertEquals("ZG_ExciseCode not changed if new tariff contains the actual code as an ExciseCode", "0A0", invoiceLine.ZG_ExciseCode);

			invoiceLine.JI_FormattedTariff = tariff2.ZZ1_TariffCode;
			AssertEquals("Auto-Filled ZG_ExciseCode if new tariff contains only 1 ExciseCode", "0A1", invoiceLine.ZG_ExciseCode);

			invoiceLine.JI_FormattedTariff = "11115555";
			AssertEquals("ZG_ExciseCode is empty if new tariff doesn't contain ExciseCodes", ZString.Empty, invoiceLine.ZG_ExciseCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedTariff = exporttariff.ZZ1_TariffCode;
			AssertEquals("ZG_ExciseCode is empty if declaration is export even though the tariff has 1 ExciseCode", ZString.Empty, invoiceLine.ZG_ExciseCode);
		});
	}

	public void TestAutoFillVATBox()
	{
		#region Setup

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateTaxOrFee("IV1", 0.21, countryCode);
		helper.CreateTaxOrFee("IV2", 0.21, countryCode);
		helper.CreateTaxOrFee("IV3", 0.21, countryCode);
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var expTariffType = helper.CreateTariffType(countryCode, "EXP");
		Factory.Save();

		var tariffWithMultipleVAT = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11113333", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff2 = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11114444", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var exporttariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11116666", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV1");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IV1");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IV2");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IV3");
		helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "IV3");
		helper.CreateNewOrGetExistingVATApplicability(exporttariff, countryCode, "IV2");
		Factory.Save();

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("For VAT", () =>
		{
			declaration.ZG_DestinationState = "01";

			invoiceLine.JI_FormattedTariff = tariffWithMultipleVAT.ZZ1_TariffCode;
			AssertEquals("Not Auto-Filled JI_ZZF_NKTaxType, tariff contains more than 1 VAT Type", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariff.ZZ1_TariffCode;
			AssertEquals("Auto-Filled JI_ZZF_NKTaxType if tariff contains only 1 VAT Type", "IV1", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariffWithMultipleVAT.ZZ1_TariffCode;
			AssertEquals("JI_ZZF_NKTaxType not changed if new tariff contains the actual code as an VAT Type", "IV1", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariff2.ZZ1_TariffCode;
			AssertEquals("Auto-Filled JI_ZZF_NKTaxType if new tariff contains only 1 VAT Type", "IV3", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = "11115555";
			AssertEquals("JI_ZZF_NKTaxType is empty if new tariff doesn't contain VAT Types", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedTariff = exporttariff.ZZ1_TariffCode;
			AssertEquals("JI_ZZF_NKTaxType is empty if declaration is export even though the tariff has 1 VAT Type", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
		});
	}

	public void TestAutoFillIGICBox()
	{
		#region Setup

		var helper = new ESUniversalReferenceTestDataHelper(Factory);

		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateTaxOrFee("IG1", 0.21, countryCode);
		helper.CreateTaxOrFee("IG2", 0.21, countryCode);
		helper.CreateTaxOrFee("IG3", 0.21, countryCode);
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var expTariffType = helper.CreateTariffType(countryCode, "EXP");
		Factory.Save();

		var tariffWithMultipleVAT = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11113333", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariff2 = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11114444", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var exporttariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11116666", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IG1");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IG1");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IG2");
		helper.CreateNewOrGetExistingVATApplicability(tariffWithMultipleVAT, countryCode, "IG3");
		helper.CreateNewOrGetExistingVATApplicability(tariff2, countryCode, "IG3");
		helper.CreateNewOrGetExistingVATApplicability(exporttariff, countryCode, "IG2");
		Factory.Save();

		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("For IGIC", () =>
		{
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			invoiceLine.JI_FormattedTariff = tariffWithMultipleVAT.ZZ1_TariffCode;
			AssertEquals("Not Auto-Filled JI_ZZF_NKTaxType, tariff contains more than 1 IGIC Type", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariff.ZZ1_TariffCode;
			AssertEquals("Auto-Filled JI_ZZF_NKTaxType if tariff contains only 1 IGIC Type", "IG1", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariffWithMultipleVAT.ZZ1_TariffCode;
			AssertEquals("JI_ZZF_NKTaxType not changed if new tariff contains the actual code as an IGIC Type", "IG1", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = tariff2.ZZ1_TariffCode;
			AssertEquals("Auto-Filled JI_ZZF_NKTaxType if new tariff contains only 1 IGIC Type", "IG3", invoiceLine.JI_ZZF_NKTaxType);

			invoiceLine.JI_FormattedTariff = "11115555";
			AssertEquals("JI_ZZF_NKTaxType is empty if new tariff doesn't contain IGIC Types", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedTariff = exporttariff.ZZ1_TariffCode;
			AssertEquals("JI_ZZF_NKTaxType is empty if declaration is export even though the tariff has 1 IGIC Type", ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
		});
	}

	public void TestAutoFillAIEMBox()
	{
		#region Setup

		var helper = new ESUniversalReferenceTestDataHelper(Factory);

		var countryCode = Core.Constants.CountryCodes.Spain;
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var expTariffType = helper.CreateTariffType(countryCode, "EXP");
		var aiemTariffType = helper.CreateTariffType(countryCode, "AIEM");
		Factory.Save();
		var tariffWithMultipleAIEMs = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11113333", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffAIEM = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11113333_AIEM01", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var tariffAIEM2 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11113333_AIEM02", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var tariffAIEM3 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11113333_AIEM03", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc3");

		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffAIEM4 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM03", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc3");

		var tariff2 = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11114444", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffAIEM5 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11114444_AIEM05", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc5");

		var exporttariff = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11116666", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffAIEM6 = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11116666_AIEM06", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc6");

		helper.CreateTariffRelationship(tariffAIEM.PK, impTariffType.PK, tariffWithMultipleAIEMs.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM2.PK, impTariffType.PK, tariffWithMultipleAIEMs.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM3.PK, impTariffType.PK, tariffWithMultipleAIEMs.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM4.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM5.PK, impTariffType.PK, tariff2.ZZ1_TariffCode);
		helper.CreateTariffRelationship(tariffAIEM6.PK, expTariffType.PK, exporttariff.ZZ1_TariffCode);
		Factory.Save();

		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

		#endregion

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

		CombineAssertions(() =>
		{
			invoiceLine.JI_FormattedTariff = tariffWithMultipleAIEMs.ZZ1_TariffCode;
			AssertEquals("Not Auto-Filled ZG_AIEMType, tariff contains more than 1 AIEM Type", ZString.Empty, invoiceLine.ZG_AIEMType);

			invoiceLine.JI_FormattedTariff = tariff.ZZ1_TariffCode;
			AssertEquals("Auto-Filled ZG_AIEMType if tariff contains only 1 AIEM Type", "AIEM03", invoiceLine.ZG_AIEMType);

			invoiceLine.JI_FormattedTariff = tariffWithMultipleAIEMs.ZZ1_TariffCode;
			AssertEquals("ZG_AIEMType not changed if new tariff contains the actual code as an AIEM Type", "AIEM03", invoiceLine.ZG_AIEMType);

			invoiceLine.JI_FormattedTariff = tariff2.ZZ1_TariffCode;
			AssertEquals("Auto-Filled ZG_AIEMType if new tariff contains only 1 AIEM Type", "AIEM05", invoiceLine.ZG_AIEMType);

			invoiceLine.JI_FormattedTariff = "11115555";
			AssertEquals("ZG_AIEMType is empty if new tariff doesn't contain AIEM Types", ZString.Empty, invoiceLine.ZG_AIEMType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_FormattedTariff = exporttariff.ZZ1_TariffCode;
			AssertEquals("ZG_AIEMType is empty if declaration is export even though the tariff has 1 AIEM Type", ZString.Empty, invoiceLine.ZG_AIEMType);
		});
	}

	public void TestIsPCAApplicable()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCodePCA = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(tariffExcise, rateCodePCA.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PCA");
		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffExciseNoPCA = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A1", rateType.PK);
		helper.CreateRate(tariffExciseNoPCA, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*ZZZ");
		helper.CreateTariffRelationship(tariffExciseNoPCA.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			InvoiceLine.JI_Tariff = "11112222";
			InvoiceLine.ZG_ExciseCode = "0A1";
			AssertEquals("IsPCAApplicable equals to false when the formula does not contain PCA", false, InvoiceLine.IsPCAApplicable);

			InvoiceLine.ZG_ExciseCode = "0A0";
			AssertEquals("IsPCAApplicable equals to true when the formula does contain PCA", true, InvoiceLine.IsPCAApplicable);

			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("IsPCAApplicable equals to false when no import declaration", false, InvoiceLine.IsPCAApplicable);
		});
	}

	public void TestIsPVPApplicable()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(tariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
		helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffExciseNoPVP = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "0A1", rateType.PK);
		helper.CreateRate(tariffExciseNoPVP, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*ZZZ");
		helper.CreateTariffRelationship(tariffExciseNoPVP.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			InvoiceLine.JI_Tariff = "11112222";
			InvoiceLine.ZG_ExciseCode = "0A1";
			AssertEquals("IsPVPApplicable equals to false when the formula does not contain PVP", false, InvoiceLine.IsPVPApplicable);

			InvoiceLine.ZG_ExciseCode = "0A0";
			AssertEquals("IsPVPApplicable equals to true when the formula does contain PVP", true, InvoiceLine.IsPVPApplicable);

			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("IsPVPApplicable equals to false when no import declaration", false, InvoiceLine.IsPVPApplicable);
		});
	}

	public void TestCurrentSpecialExciseRate()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		var tariffExcise0 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0E0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc0");
		var rateCodePVP0 = helper.LoadOrCreateNewCusRateCode(Factory, "0E0", rateType.PK);
		helper.CreateRate(tariffExcise0, rateCodePVP0.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
		helper.CreateTariffRelationship(tariffExcise0.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffExcise5 = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "5E0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc5");
		var rateCodePVP5 = helper.LoadOrCreateNewCusRateCode(Factory, "5E0", rateType.PK);
		helper.CreateRate(tariffExcise5, rateCodePVP5.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
		helper.CreateTariffRelationship(tariffExcise5.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		var tariffExciseO = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "AE0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "descO");
		var rateCodePVPO = helper.LoadOrCreateNewCusRateCode(Factory, "AE0", rateType.PK);
		helper.CreateRate(tariffExciseO, rateCodePVPO.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
		helper.CreateTariffRelationship(tariffExciseO.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "11112222";
			InvoiceLine.ZG_ExciseCode = "0E0";
			AssertEquals("The 5E0 code was expected and we have received 0E0", "5E0", InvoiceLine.CurrentSpecialExciseRate.RateCode);

			InvoiceLine.ZG_ExciseCode = "5E0";
			AssertEquals("The 0E0 code was expected and we have received 5E0", "0E0", InvoiceLine.CurrentSpecialExciseRate.RateCode);

			InvoiceLine.ZG_ExciseCode = "AE0";
			AssertEquals("The result must be empty when it does not start with 0 or 5.", null, InvoiceLine.CurrentSpecialExciseRate);
		});
	}

	public void TestCurrentExciseRate()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");
		var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
		var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
		var rateType = helper.CreateCusRateType(countryCode, "EXC");
		Factory.Save();
		var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var canTariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
		var esTariffExcise = helper.LoadOrCreateNewTariff(countryCode, esexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
		var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
		helper.CreateRate(canTariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
		helper.CreateRate(esTariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.3*PVP");
		helper.CreateTariffRelationship(canTariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
		helper.CreateTariffRelationship(esTariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "11112222";
			AssertNull("Is null when no ZG_ExciseCode", InvoiceLine.CurrentExciseRate);

			InvoiceLine.ZG_ExciseCode = "ZZ";
			AssertNull("Is null when invalid ZG_ExciseCode", InvoiceLine.CurrentExciseRate);

			InvoiceLine.ZG_ExciseCode = "0A0";
			AssertNotNull("Is not null when valid Tariff and ExciseCode", InvoiceLine.CurrentExciseRate);
			AssertEquals("ES ExciseCode", "0.3*PVP", InvoiceLine.CurrentExciseRate.ZZ2_RateFormula);

			InvoiceLine.Declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			AssertEquals("Canary Island ExciseCode", "0.2*PVP", InvoiceLine.CurrentExciseRate.ZZ2_RateFormula);

			InvoiceLine.JI_Tariff = ZString.Empty;
			AssertNull("Is null when no Tariff", InvoiceLine.CurrentExciseRate);

			InvoiceLine.JI_Tariff = "123";
			AssertNull("Is null when invalid Tariff", InvoiceLine.CurrentExciseRate);
		});
	}

	public void TestSpecialExciseCode()
	{
		CombineAssertions(() =>
		{
			InvoiceLine.ZG_ExciseCode = "0E0";
			AssertEquals("The 5E0 code was expected and we have received 0E0", "5E0", InvoiceLine.SpecialExciseCode);

			InvoiceLine.ZG_ExciseCode = "5E0";
			AssertEquals("The 0E0 code was expected and we have received 5E0", "0E0", InvoiceLine.SpecialExciseCode);

			InvoiceLine.ZG_ExciseCode = "AE0";
			AssertEquals("The result must be empty when it does not start with 0 or 5.", ZString.Empty, InvoiceLine.SpecialExciseCode);
		});
	}

	public void TestPVPCurrency()
	{
		AssertEquals("Currency for PVP is EUR", Core.Constants.CurrencyCodes.EuropeanUnion, InvoiceLine.TotalRetailPriceCurrency);
	}

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		var customsChargeTypeList = new EU.Business.UCCCustomsChargeTypeList();
		customsChargeTypeList.AddPair(EU.Business.ChargeTypeList.Codes.StatisticalValue, EU.Business.ChargeTypeList.Descriptions.StatisticalValue);
		if (dec.IsImport)
		{
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreight);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.UnloadingOfGoods);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.PortTransitFee);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TerminalHandlingCharge);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation);
		}
		else
		{
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InternationalFreightExp);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherInternationalPayments);
			customsChargeTypeList.AddPair(ESCustomsChargeTypeList.Codes.OtherNationalPayments);
		}
		customsChargeTypeList.Sort();
		return customsChargeTypeList;
	}

	public void TestDeleteAdditionalProcedureCodesWhenJI_ProcedureFirst4CharactersChanged()
	{
		InvoiceLine.JI_FormattedProcedure = "2222.01";
		CombineAssertions(() =>
		{
			ChangeProcedureCodeAndAssertAdditionalCodeCount("2222.22", expectDeleteAdditionalCodes: false);
			ChangeProcedureCodeAndAssertAdditionalCodeCount("3222.01", expectDeleteAdditionalCodes: true);
			ChangeProcedureCodeAndAssertAdditionalCodeCount("3322.01", expectDeleteAdditionalCodes: true);
			ChangeProcedureCodeAndAssertAdditionalCodeCount("3332.01", expectDeleteAdditionalCodes: true);
			ChangeProcedureCodeAndAssertAdditionalCodeCount("3333.01", expectDeleteAdditionalCodes: true);
			ChangeProcedureCodeAndAssertAdditionalCodeCount(ZString.Empty, expectDeleteAdditionalCodes: true);
		});
	}
	void ChangeProcedureCodeAndAssertAdditionalCodeCount(string newCode, bool expectDeleteAdditionalCodes)
	{
		InvoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
		InvoiceLine.AdditionalProcedureCodes.AddNew("222201");
		InvoiceLine.AdditionalProcedureCodes.AddNew("222204");
		AssertEquals("Additional Code Count (after adding new ones)", 2, InvoiceLine.AdditionalProcedureCodes.Count);
		var previousCode = InvoiceLine.JI_FormattedProcedure;
		InvoiceLine.JI_FormattedProcedure = newCode;
		AssertEquals($"Procedure Code changed from '{previousCode}' to '{newCode}' => Additional Code Count", expectDeleteAdditionalCodes ? 0 : 2, InvoiceLine.AdditionalProcedureCodes.Count);
	}

	public void TestAdditionalInfos()
	{
		var line = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<AdditionalInfoCollection>(line.AdditionalInfos);
	}

	public new void TestSupportingDocuments()
	{
		var line = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<SupportingDocumentCollection>(line.SupportingDocuments);
	}

	public void TestDestinationStateIsCanaryIsland() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, "61", "Test 61");
		var noDecInvLine = Factory.New<JobComInvoiceLine>();
		var line = Declaration.Invoices.AddNew().InvoiceLines.AddNew();

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("InvoiceLine without parent declaration return false", false, noDecInvLine.DestinationStateIsCanaryIsland);

			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			Declaration.ZG_DestinationState = "61";
			AssertEquals("Destination State is canary island for declaration, ZG_DestinationState is canary island, Import and not UCC6", true, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is canary island for invLine, ZG_DestinationState is canary island, Import and not UCC6", true, line.DestinationStateIsCanaryIsland);

			Declaration.ZG_DestinationState = "12";
			AssertEquals("Destination State is not canary island for declaration, ZG_DestinationState is not canary island, Import and not UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, ZG_DestinationState is not canary island, Import and not UCC6", false, line.DestinationStateIsCanaryIsland);

			Declaration.JE_CustomsOffice = "ES003861";
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Import and not UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, JE_CustomsOffice is canary island, Import and not UCC6", false, line.DestinationStateIsCanaryIsland);

			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			Declaration.ZG_DestinationState = "61";
			AssertEquals("Destination State is not canary island for declaration, ZG_DestinationState is canary island, Export and not UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, ZG_DestinationState is canary island, Export and not UCC6", false, line.DestinationStateIsCanaryIsland);

			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Export and not UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, JE_CustomsOffice is canary island, Export and not UCC6", false, line.DestinationStateIsCanaryIsland);
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is canary island, Export and UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, JE_CustomsOffice is canary island, Export and UCC6", false, line.DestinationStateIsCanaryIsland);

			Declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is canary island for invLine, JE_CustomsOffice is canary island, Import and UCC6", true, line.DestinationStateIsCanaryIsland);

			Declaration.JE_CustomsOffice = ZString.Empty;
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is not canary island, Import and UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, JE_CustomsOffice is not canary island, Import and UCC6", false, line.DestinationStateIsCanaryIsland);

			Declaration.JE_CustomsOffice = "ES003541";
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is canary island for invLine, JE_CustomsOffice is canary island, Import and UCC6", true, line.DestinationStateIsCanaryIsland);

			Declaration.JE_CustomsOffice = "ES003712";
			AssertEquals("Destination State is not canary island for declaration, JE_CustomsOffice is not canary island, Import and UCC6", false, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is not canary island for invLine, JE_CustomsOffice is not canary island, Import and UCC6", false, line.DestinationStateIsCanaryIsland);

			Declaration.JE_CustomsOffice = "ES009998";
			AssertEquals("Destination State is canary island for declaration, JE_CustomsOffice is canary island, Import and UCC6", true, Declaration.DestinationStateIsCanaryIsland);
			AssertEquals("Destination State is canary island for invLine, JE_CustomsOffice is canary island, Import and UCC6", true, line.DestinationStateIsCanaryIsland);
		}
	});

	public override void TestGetSupportingDocumentsMaxCountReduction()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("0 documents have been added to the dec or invoice, the reduction is 0", 0, invLine.GetSupportingDocumentsMaxCountReduction());

			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = "doc";
			AssertEquals("A new document has been added to the dec, the reduction is 1", 1, invoice.GetSupportingDocumentsMaxCountReduction());

			var newInvoice = Factory.New<JobComInvoiceHeader>();
			var newInvLine = Factory.New<JobComInvoiceLine>();
			newInvLine.JI_JZ = newInvoice.PK;
			AssertEquals("invLine hasn't Declaration", null, newInvLine.Declaration);
			AssertNotNull(newInvLine.InvoiceHeader);
			AssertEquals("When the invLine has no declaration the reduction is 0", 0, newInvLine.GetSupportingDocumentsMaxCountReduction());

			var newInvoiceDoc = newInvoice.SupportingDocuments.AddNew();
			newInvoiceDoc.CSI_Code = "doc2";
			AssertEquals("A new document has been added to the header, the reduction is 1", 1, newInvLine.GetSupportingDocumentsMaxCountReduction());

			var newInvoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("invLine hasn't Declaration", null, newInvoiceLine.Declaration);
			AssertEquals("invLine hasn't Header", null, newInvoiceLine.InvoiceHeader);
			AssertEquals("When the invLine has no declaration and header the reduction is 0", 0, newInvoiceLine.GetSupportingDocumentsMaxCountReduction());
		});
	}

	public override void TestMaxSupportingDocuments()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("EnableMaxCountValidationWithMessageError is disabled", 99, invLine.MaxSupportingDocuments);

			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			invLine.JI_CEI = entryInstruction.PK;
			AssertEquals("EnableMaxCountValidationWithMessageError is disabled for EXS", 10, invLine.MaxSupportingDocuments);
		});
	}

	public void TestEnableMaxCountValidationWithMessageError()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		for (int i = 0; i < 20; i++)
		{
			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = $"doc{i}";
		}
		for (int i = 0; i < 20; i++)
		{
			var doc = invoice.SupportingDocuments.AddNew();
			doc.CSI_Code = $"dc{i}";
		}
		for (int i = 0; i < 59; i++)
		{
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = $"do{i}";
		}
		AssertEquals(20, dec.SupportingDocuments.Count);
		AssertEquals(20, invoice.SupportingDocuments.Count);
		AssertEquals(59, invLine.SupportingDocuments.Count);
		AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
		AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
		AssertNoRowMessageErrorContaining(invLine.SupportingDocuments[58], "Customs will not accept a declaration with more than 99 documents per line");

		var newDoc = invLine.SupportingDocuments.AddNew();
		newDoc.CSI_Code = "mydoc";
		AssertEquals(20, dec.SupportingDocuments.Count);
		AssertEquals(20, invoice.SupportingDocuments.Count);
		AssertEquals(60, invLine.SupportingDocuments.Count);
		AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
		AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
		AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[59], "Customs will not accept a declaration with more than 99 documents per line");
	}

	public void TestEnableMaxCountValidationWithMessageErrorEXS()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		invLine.JI_CEI = entryInstruction.PK;

		for (int i = 0; i < 3; i++)
		{
			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = $"doc{i}";
		}
		for (int i = 0; i < 3; i++)
		{
			var doc = invoice.SupportingDocuments.AddNew();
			doc.CSI_Code = $"dc{i}";
		}
		for (int i = 0; i < 4; i++)
		{
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = $"do{i}";
		}
		CombineAssertions(() =>
		{
			AssertEquals("PreReq", 3, dec.SupportingDocuments.Count);
			AssertEquals("PreReq", 3, invoice.SupportingDocuments.Count);
			AssertEquals("PreReq", 4, invLine.SupportingDocuments.Count);
			AssertNoRowMessageErrorContaining(invLine.SupportingDocuments[3], "Customs will not accept a declaration with more than 10 documents per line");

			var newDoc = invLine.SupportingDocuments.AddNew();
			newDoc.CSI_Code = "mydoc";
			AssertEquals("PreReq", 5, invLine.SupportingDocuments.Count);
			AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[4], "Customs will not accept a declaration with more than 10 documents per line");
		});
	}

	public void TestEnableMaxCountValidationWithMessageErrorDynamicCounting()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		for (int i = 0; i < 20; i++)
		{
			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = $"doc{i}";
		}
		for (int i = 0; i < 20; i++)
		{
			var doc = invoice.SupportingDocuments.AddNew();
			doc.CSI_Code = $"dc{i}";
		}
		for (int i = 0; i < 59; i++)
		{
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = $"do{i}";
		}

		CombineAssertions(() =>
		{
			AssertEquals(20, dec.SupportingDocuments.Count);
			AssertEquals(20, invoice.SupportingDocuments.Count);
			AssertEquals(59, invLine.SupportingDocuments.Count);
			AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
			AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
			AssertNoRowMessageErrorContaining(invLine.SupportingDocuments[58], "Customs will not accept a declaration with more than 99 documents per line");

			var newDecDoc = dec.SupportingDocuments.AddNew();
			newDecDoc.CSI_Code = "mydoc";
			AssertEquals(21, dec.SupportingDocuments.Count);
			AssertEquals(20, invoice.SupportingDocuments.Count);
			AssertEquals(59, invLine.SupportingDocuments.Count);
			invLine.SupportingDocuments.RunPreSaveValidation();
			AssertNoRowMessageErrorContaining(dec.SupportingDocuments[20], "Customs will not accept a declaration with more than 99 documents per line");
			AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
			AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[58], "Customs will not accept a declaration with more than 99 documents per line");

			newDecDoc.Delete();
			var newHeaderDoc = invoice.SupportingDocuments.AddNew();
			newHeaderDoc.CSI_Code = "mydoc";
			AssertEquals(20, dec.SupportingDocuments.Count);
			AssertEquals(21, invoice.SupportingDocuments.Count);
			AssertEquals(59, invLine.SupportingDocuments.Count);
			invLine.SupportingDocuments.RunPreSaveValidation();
			AssertNoRowMessageErrorContaining(dec.SupportingDocuments[19], "Customs will not accept a declaration with more than 99 documents per line");
			AssertNoRowMessageErrorContaining(invoice.SupportingDocuments[20], "Customs will not accept a declaration with more than 99 documents per line");
			AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[58], "Customs will not accept a declaration with more than 99 documents per line");
		});
	}

	public void TestEnableMaxCountValidationWithMessageErrorDynamicCountingEXS()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

		var invoice = dec.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		invLine.JI_CEI = entryInstruction.PK;

		for (int i = 0; i < 3; i++)
		{
			var doc = dec.SupportingDocuments.AddNew();
			doc.CSI_Code = $"doc{i}";
		}
		for (int i = 0; i < 3; i++)
		{
			var doc = invoice.SupportingDocuments.AddNew();
			doc.CSI_Code = $"dc{i}";
		}
		for (int i = 0; i < 4; i++)
		{
			var doc = invLine.SupportingDocuments.AddNew();
			doc.CSI_Code = $"do{i}";
		}

		CombineAssertions(() =>
		{
			AssertEquals("PreReq", 3, dec.SupportingDocuments.Count);
			AssertEquals("PreReq", 3, invoice.SupportingDocuments.Count);
			AssertEquals("PreReq", 4, invLine.SupportingDocuments.Count);
			AssertNoRowMessageErrorContaining(invLine.SupportingDocuments[3], "Customs will not accept a declaration with more than 10 documents per line");

			var newDecDoc = dec.SupportingDocuments.AddNew();
			newDecDoc.CSI_Code = "mydoc";
			AssertEquals("PreReq", 4, dec.SupportingDocuments.Count);
			AssertEquals("PreReq", 3, invoice.SupportingDocuments.Count);
			AssertEquals("PreReq", 4, invLine.SupportingDocuments.Count);
			invLine.SupportingDocuments.RunPreSaveValidation();
			AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[3], "Customs will not accept a declaration with more than 10 documents per line");

			newDecDoc.Delete();
			var newHeaderDoc = invoice.SupportingDocuments.AddNew();
			newHeaderDoc.CSI_Code = "mydoc";
			AssertEquals("PreReq", 3, dec.SupportingDocuments.Count);
			AssertEquals("PreReq", 4, invoice.SupportingDocuments.Count);
			AssertEquals("PreReq", 4, invLine.SupportingDocuments.Count);
			invLine.SupportingDocuments.RunPreSaveValidation();
			AssertHasRowMessageErrorContaining(invLine.SupportingDocuments[3], "Customs will not accept a declaration with more than 10 documents per line");
		});
	}

	public void TestMaxNumberOfAdditionalProcedureCode()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		AssertEquals(2, invoiceLine.MaxNumberOfAdditionalProcedureCode);

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		AssertEquals(2, invoiceLine.MaxNumberOfAdditionalProcedureCode);

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals(2, invoiceLine.MaxNumberOfAdditionalProcedureCode);
	}

	public void TestIsAdditionalProcedureCodesApplicable()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		AssertEquals(true, invoiceLine.IsAdditionalProcedureCodesApplicable);

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		AssertEquals(true, invoiceLine.IsAdditionalProcedureCodesApplicable);

		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals(true, invoiceLine.IsAdditionalProcedureCodesApplicable);
	}

	public void TestJI_FormattedProcedureHasCorrectFormat()
	{
		var dec = Factory.New<JobDeclaration>();
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "1000";

		CombineAssertions(() =>
		{
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("JI_FormattedProcedure is the same as JI_Procedure for Import", "1000", invoiceLine.JI_FormattedProcedure);

			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				AssertEquals("JI_FormattedProcedure is the same as JI_Procedure for Export AES", "1000", invoiceLine.JI_FormattedProcedure);
			}

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				AssertEquals("JI_FormattedProcedure is the same as JI_Procedure for Export AES1.1", "1000", invoiceLine.JI_FormattedProcedure);
			}
		});
	}

	protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);

	protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	protected new JobComInvoiceLine InvoiceLine => base.InvoiceLine;

	public void TestGetPreviousDocument()
	{
		var dec1 = Factory.New<JobDeclaration>();
		var invoice = dec1.Invoices.AddNew();
		var invLine1 = invoice.InvoiceLines.AddNew();

		var dec2 = Factory.New<JobDeclaration>();
		var invLine2 = dec2.Invoices.AddNew().InvoiceLines.AddNew();

		Declaration.PreviousDocuments.AddNew();
		invoice.PreviousDocuments.AddNew();
		invLine2.PreviousDocuments.AddNew();

		Declaration.PreviousDocuments[0].CSI_Code = "CD1";
		invoice.PreviousDocuments[0].CSI_Code = "CD1";
		invLine2.PreviousDocuments[0].CSI_Code = "CD1";

		CombineAssertions(() =>
		{
			AssertEquals("Method returns Declaration Previous Document", Declaration.PreviousDocuments, InvoiceLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly());
			AssertEquals("Method returns Header Previous Document", invoice.PreviousDocuments, invLine1.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly());
			AssertEquals("Method returns InvLine Previous Document", invLine2.PreviousDocuments, invLine2.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly());
		});
	}

	public void TestSynchroniseVINNumberAndVINPartAttribute()
	{
		for (int i = 1; i <= 3; i++)
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = $"OH{i}";
			owner.MiscServ[$"OM_IMPartAttrib{i}Type"] = PartAttributeTypeList.Codes.VIN;
			owner.MiscServ[$"OM_IMPartAttrib{i}Name"] = "VIN number attribute";

			var part = Factory.New<MasterFiles.OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";

			var ownRelation = part.RelatedOrganisations.AddNew();
			ownRelation.OU_OH = owner.PK;
			ownRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			ownRelation[$"OU_UsePartAttrib{i}"] = true;

			Factory.Save();

			Declaration.JE_OH_Importer = ownRelation.OU_OH;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var vehicle = invoiceLine.Vehicles.AddNew();
			invoiceLine.JI_PartNo = "PARTNUM";

			var partAttribName = $"JI_PartAttrib{i}";

			invoiceLine[partAttribName] = "VIN3456";
			AssertEquals("VIN field should be populated from VIN number attribute", "VIN3456", vehicle.CVH_VehicleIdentificationNumber);
			vehicle.CVH_VehicleIdentificationNumber = "VIN1234";
			AssertEquals("VIN number attribute should be populated from VIN field", "VIN1234", invoiceLine[partAttribName]);

			invoiceLine.JI_PartNo = "";

			vehicle.CVH_VehicleIdentificationNumber = "VIN1234";
			AssertEquals("VIN number attribute should NOT be populated from VIN field", "", invoiceLine[partAttribName]);
			invoiceLine[partAttribName] = "VIN3456";
			AssertEquals("VIN field should NOT be populated from VIN number attribute", "VIN1234", vehicle.CVH_VehicleIdentificationNumber);

			invoiceLine.JI_PartNo = "PARTNUM";

			AssertEquals("VIN field should NOT be populated from VIN number attribute", "VIN1234", vehicle.CVH_VehicleIdentificationNumber);
			AssertEquals("VIN number attribute should be populated from VIN field", "VIN1234", invoiceLine[partAttribName]);
		}
	}

	public void TestAdjValues()
	{
		var line1 = InvoiceHeader.InvoiceLines.AddNew();
		InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
		var groupHeader = InvoiceHeader.GroupHeader;

		AddChargeToGroupHeader(EU.Business.UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 30m, true, false, Declaration.LocalCurrencyCode);
		AddChargeToInvoiceLine(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 20m, false, true);
		AddChargeToInvoiceLine(EU.Business.UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 3m, true, false);
		AddChargeToInvoiceLine(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2m, false, true);

		CombineAssertions(() =>
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_InvoiceAmount = 100m;
			line1.JI_LinePrice = 100m;
			Declaration.ResumeApportionment();
			AssertEquals("Negative adjustment", -22m, line1.JI_NegAdj);
			AssertEquals("Positive adjustment", 33m, line1.JI_PosAdj);

			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, 2m);

			AddChargeToGroupHeader(EU.Business.UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 100m, true, false, "MDD");
			Declaration.ResumeApportionment();
			AssertEquals("Positive adjustment", 83m, line1.JI_PosAdj);

			line1.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20m, Declaration.LocalCurrencyCode);
			Declaration.ResumeApportionment();
			AssertEquals("Positive adjustment with EGV charge added", 103m, line1.JI_PosAdj);

			AddChargeToGroupHeader(EU.Business.UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 100m, false, true, "MDD");
			Declaration.ResumeApportionment();
			AssertEquals("Negative adjustment", -72m, line1.JI_NegAdj);

			AddChargeToInvoiceLine(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 20m, false, true);
			Declaration.ResumeApportionment();
			AssertEquals("Negative adjustment with EGP charge not added", -72m, line1.JI_NegAdj);
		});

		void AddChargeToGroupHeader(string chargeCode, decimal amount, bool isDutiable, bool isIncludedInITOT, string currency)
		{
			var charge = groupHeader.Charges.AddNew(chargeCode, amount, currency);
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
		}

		void AddChargeToInvoiceLine(string chargeCode, decimal amount, bool isDutiable, bool isIncludedInITOT)
		{
			var charge = line1.Charges.AddNew(chargeCode, amount, Declaration.LocalCurrencyCode);
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
		}
	}

	public void TestVATAdditions()
	{
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoice.JZ_InvoiceAmount = 1000m;
		invoiceLine.JI_LinePrice = 1000m;

		var insuranceCharge = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 500m, Declaration.LocalCurrencyCode);
		insuranceCharge.J7_IsDutiable = false;

		var overseasFreight = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, Declaration.LocalCurrencyCode);

		invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, 25m, Declaration.LocalCurrencyCode);

		CombineAssertions(() =>
		{
			AssertEquals("Customs value = 1000+100 = $1100", 1100m, invoiceLine.JI_CustomsValue);
			AssertEquals("VAT Base value = 1000+100+475 = $1575", 1575m, invoiceLine.JI_Calc_ValueForVat);
			AssertEquals("VAT Additions = VAT Base (1600) - Customs Value (1100) - Duties Amount(0) - REA Aid Amount(25) = $475", 475m, invoiceLine.JI_VAT_Additions, 0.01m);
		});
	}

	public new void TestVatValue()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var invoice = dec.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoice.JZ_InvoiceAmount = 1000m;
		invoiceLine.JI_LinePrice = 1000m;

		var insuranceCharge = invoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100m, dec.LocalCurrencyCode);
		CombineAssertions(() =>
		{
			Assert("Pre-Req - insurance is VAT-able", insuranceCharge.J7_IsGSTApplicable);

			var nonStatCharge = invoiceLine.Charges.AddNew("ABC", 50m, dec.LocalCurrencyCode);
			nonStatCharge.J7_IsGSTApplicable = false;

			AssertEquals("VAT value  = Customs Value (1100) + VAT Additions (0) = $1100", 1100m, invoiceLine.JI_Calc_ValueForVat);

			nonStatCharge.J7_IsDutiable = false;
			nonStatCharge.J7_IsGSTApplicable = true;
			AssertEquals("VAT value  = Customs Value (1100) + VAT Additions (50) = $1150", 1150m, invoiceLine.JI_Calc_ValueForVat);

			var supDoc1 = invoiceLine.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "7003";
			supDoc1.CSI_ReferenceNumber = "25";
			AssertEquals("VAT value  = Customs Value (1100) + VAT Additions (50) = $1150, supporting document 7003 is not used in the calculation", 1150m, invoiceLine.JI_Calc_ValueForVat);

			var supDoc2 = invoiceLine.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = "7009";
			supDoc2.CSI_ReferenceNumber = "24,5";
			AssertEquals("VAT value  = Customs Value (1100) + VAT Additions (50) = $1150, supporting document 7009 is not used in the calculation", 1150m, invoiceLine.JI_Calc_ValueForVat);
		});
	}

	public void TestJI_Calc_ESCustomsValue()
	{
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoice.JZ_InvoiceAmount = 1000m;
		invoiceLine.JI_LinePrice = 1000m;

		var insuranceCharge = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 500m, Declaration.LocalCurrencyCode);
		insuranceCharge.J7_IsDutiable = false;

		var overseasFreight = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, Declaration.LocalCurrencyCode);

		CombineAssertions(() =>
		{
			Declaration.ResumeApportionment();
			AssertEquals("Customs value = 1000+100 = $1100", 1100m, invoiceLine.JI_CustomsValue);
			AssertEquals("ES Customs value = 1000+100 = $1100", 1100m, invoiceLine.JI_Calc_ESCustomsValue);

			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, 20m, Declaration.LocalCurrencyCode);
			Declaration.ResumeApportionment();
			AssertEquals("Customs value = 1000+100 = $1100 when EGV charge declared", 1100m, invoiceLine.JI_CustomsValue);
			AssertEquals("ES Customs value = 1000+100+20 = $1100 when EGV charge declared", 1120m, invoiceLine.JI_Calc_ESCustomsValue);

			invoiceLine.Charges.AddNew(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, 30m, Declaration.LocalCurrencyCode);
			Declaration.ResumeApportionment();
			AssertEquals("Customs value = 1000+100-30 = $1100 when EGV and EGP charges declared", 1070m, invoiceLine.JI_CustomsValue);
			AssertEquals("ES Customs value = 1000+100-30+20+30 = $1100 when EGV and EGP charges declared", 1120m, invoiceLine.JI_Calc_ESCustomsValue);
		});
	}

	public void TestAddNewSupportingDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("No supporting documents in invocieLine", false, invoiceLine.SupportingDocuments.Any());

			invoiceLine.AddNewSupportingDocument("AAA", "reference");
			AssertEquals("1 supporting documents in invocieLine after calling method", 1, invoiceLine.SupportingDocuments.Count);
			AssertEquals("SupportingDocument CSI_Code", "AAA", invoiceLine.SupportingDocuments[0].CSI_Code);
			AssertEquals("SupportingDocument CSI_ReferenceNumber", "reference", invoiceLine.SupportingDocuments[0].CSI_ReferenceNumber);
		});
	}

	public void TestOnFactorySaving_AdjustImportSupportingDocuments_EGVCharge()
	{
		AssertImportSupDocsAdjustment("EGV Charge", ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing);
	}

	public void TestOnFactorySaving_AdjustImportSupportingDocuments_EGPCharge()
	{
		AssertImportSupDocsAdjustment("EGP Charge", ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing);
	}

	public void TestOnFactorySaving_AdjustImportSupportingDocuments_EGVAndEGPCharge()
	{
		AssertImportSupDocsAdjustment("EGV Charge and EGP Charge", ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing);
	}

	void AssertImportSupDocsAdjustment(string chargeText, string chargeCode, string secondChargeCode = "")
	{
		var hasSecondChargeCode = !string.IsNullOrEmpty(secondChargeCode);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		invoice.JZ_IncoTerm = "DDP";
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoice.JZ_InvoiceAmount = 1000m;

		var tDM = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 30m, declaration.LocalCurrencyCode);
		tDM.J7_IsNotIncludedInInvoice = false;
		tDM.J7_IsDutiable = false;
		tDM.J7_IsGSTApplicable = true;
		invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 20m, declaration.LocalCurrencyCode);

		invoiceLine.JI_LinePrice = 100m;
		declaration.ResumeApportionment();

		CombineAssertions(() =>
		{
			AssertEquals("Vat_Additions is not empty", 30m, invoiceLine.JI_VAT_Additions);

			Factory.Save();

			AssertEquals("When no " + chargeText + " declared there are no supporting documents created automatically", false, invoiceLine.SupportingDocuments.Any());

			var charge1 = invoiceLine.Charges.AddNew(chargeCode, 40m, Declaration.LocalCurrencyCode);
			BaseInvoiceLineCharge secondCharge1 = null;
			if (hasSecondChargeCode)
			{
				secondCharge1 = invoiceLine.Charges.AddNew(secondChargeCode, 20m, Declaration.LocalCurrencyCode);
			}
			declaration.ResumeApportionment();

			AssertEquals("Vat_Additions is not changed after declaring " + chargeText, 30m, invoiceLine.JI_VAT_Additions);

			Factory.Save();

			AssertEquals("When " + chargeText + " declared a 7009 sup doc is created, CSI_Code", "7009", invoiceLine.SupportingDocuments[0].CSI_Code);
			AssertEquals("When " + chargeText + " declared a 7009 sup doc is created, CSI_ReferenceNumber", hasSecondChargeCode ? "60,00" : "40,00", invoiceLine.SupportingDocuments[0].CSI_ReferenceNumber);

			var tDM2 = invoiceLine.Charges.AddNew(EU.Business.UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 50m, declaration.LocalCurrencyCode);
			tDM2.J7_IsNotIncludedInInvoice = false;
			tDM2.J7_IsDutiable = false;
			tDM2.J7_IsGSTApplicable = true;

			var charge2 = invoiceLine.Charges.AddNew(chargeCode, 60m, Declaration.LocalCurrencyCode);
			BaseInvoiceLineCharge secondCharge2 = null;
			if (hasSecondChargeCode)
			{
				secondCharge2 = invoiceLine.Charges.AddNew(secondChargeCode, 50m, Declaration.LocalCurrencyCode);
			}
			declaration.ResumeApportionment();

			AssertEquals("Vat_Additions has been updated", 80m, invoiceLine.JI_VAT_Additions);

			Factory.Save();

			AssertEquals("When second " + chargeText + " declared the 7009 sup doc is updated, CSI_Code", "7009", invoiceLine.SupportingDocuments[0].CSI_Code);
			AssertEquals("When second " + chargeText + " declared the 7009 sup doc is updated, CSI_ReferenceNumber", hasSecondChargeCode ? "170,00" : "100,00", invoiceLine.SupportingDocuments[0].CSI_ReferenceNumber);

			invoiceLine.Charges.RemoveAndDelete(charge1);
			invoiceLine.Charges.RemoveAndDelete(charge2);
			if (hasSecondChargeCode)
			{
				invoiceLine.Charges.RemoveAndDelete(secondCharge1);
				invoiceLine.Charges.RemoveAndDelete(secondCharge2);
			}

			declaration.ResumeApportionment();

			AssertEquals("Vat_Additions is not empty after removing all " + chargeText, 80m, invoiceLine.JI_VAT_Additions);

			Factory.Save();

			AssertEquals("There are no documents when no " + chargeText + " declared", false, invoiceLine.SupportingDocuments.Any());
		});
	}

	public void TestIsVehicleDeclared()
	{
		CombineAssertions("Expected Supported DataContexts", () =>
		{
			AssertEquals("IsVehicleDeclared should be false not vehicles declared", false, invoiceLine.IsVehicleDeclared);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "vin_number";
			AssertEquals("IsVehicleDeclared should be true", true, invoiceLine.IsVehicleDeclared);

			vehicle.CVH_VehicleIdentificationNumber = ZString.Empty;
			vehicle.CVH_BrandName = "brand";
			AssertEquals("IsVehicleDeclared should be false when only brand is declared", true, invoiceLine.IsVehicleDeclared);

			vehicle.CVH_VehicleIdentificationNumber = ZString.Empty;
			vehicle.CVH_BrandName = ZString.Empty;
			vehicle.CVH_ModelName = "model";
			AssertEquals("IsVehicleDeclared should be false when only model is declared", true, invoiceLine.IsVehicleDeclared);

			vehicle.CVH_VehicleIdentificationNumber = ZString.Empty;
			vehicle.CVH_BrandName = ZString.Empty;
			vehicle.CVH_ModelName = ZString.Empty;
			AssertEquals("IsVehicleDeclared should be false when all vehicle fields are empty", true, invoiceLine.IsVehicleDeclared);
		});
	}

	public void TestIsExs()
	{
		var dec = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			AssertEquals("IsExs should be false when not define", false, invLine.IsEXS);

			var entryInstruction = dec.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			invLine.JI_CEI = entryInstruction.PK;
			AssertEquals("IsExs should be true when define EXS", true, invLine.IsEXS);

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			AssertEquals("IsExs should be false when define A", false, invLine.IsEXS);
		});
	}

	public void TestREARateCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invLine.ZG_IsREADirectConsumption = true; //AYD
			AssertEquals("REARateCode is AYD", UniversalReferenceConstants.RateCodeCodeList.AYD, invLine.REARateCode);

			invLine.ZG_IsREADirectConsumption = false; //AYT
			AssertEquals("REARateCode is AYT", UniversalReferenceConstants.RateCodeCodeList.AYT, invLine.REARateCode);
		});
	}

	public override void TestChargeTypeList()
	{
		dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
		var chargeTypeList1 = commonInvoice.ChargeTypeList;
		var chargeTypeList2 = commonInvoice.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = GetExpectedCustomsChargeTypeList();
			AssertEquals("Import Expected Charge Code List", customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			customsChargeTypeList = GetExpectedCustomsChargeTypeList();
			AssertEquals("Export Expected Charge Code List", customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		});
	}

	public void TestEntryHeaderValidationModeIsImportNoneOrPDS()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();

		CombineAssertions(() =>
		{
			AssertEquals("True for empty invoice line", true, invoiceLine.EntryHeaderValidationModeIsImportNoneOrPDS);

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.ValidationMode = ValidationModes.PDI;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("False for invoice line associated to an entry line with PDI entry validation mode", false, invoiceLine.EntryHeaderValidationModeIsImportNoneOrPDS);

			entryHeader.ValidationMode = ValidationModes.PDS;
			AssertEquals("True for invoice line associated to an entry line with non PDI entry validation mode", true, invoiceLine.EntryHeaderValidationModeIsImportNoneOrPDS);
		});
	}

	public void TestEntryHeaderValidationModeIsImportNone()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();

		CombineAssertions(() =>
		{
			AssertEquals("True for empty invoice line", true, invoiceLine.EntryHeaderValidationModeIsImportNone);

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.ValidationMode = ValidationModes.PDI;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("False for invoice line associated to an entry line with PDI entry validation mode", false, invoiceLine.EntryHeaderValidationModeIsImportNone);

			entryHeader.ValidationMode = ValidationModes.None;
			AssertEquals("True for invoice line associated to an entry line with non PDI or PDS entry validation mode", true, invoiceLine.EntryHeaderValidationModeIsImportNone);

			entryHeader.ValidationMode = ValidationModes.PDS;
			AssertEquals("False for invoice line associated to an entry line with PDS entry validation mode", false, invoiceLine.EntryHeaderValidationModeIsImportNone);
		});
	}
	public void TestGetNewValidationComInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceLineValidation>(invoiceLine.Validation);
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceLineValidation>(invoiceLine.Validation);
		declaration.JE_MessageType = "";
		AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceLineValidation>(invoiceLine.Validation);
	}

	public void TestShouldCheckMissingPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			AssertEquals("For Import not t2l/t2c, ShouldCheckMissingPreviousDocuments", false, invoiceLine.ShouldCheckMissingPreviousDocuments);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("For export not t2l/t2c, ShouldCheckMissingPreviousDocuments", true, invoiceLine.ShouldCheckMissingPreviousDocuments);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("For t2l expedition, ShouldCheckMissingPreviousDocuments", false, invoiceLine.ShouldCheckMissingPreviousDocuments);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("For export t2c, ShouldCheckMissingPreviousDocuments", true, invoiceLine.ShouldCheckMissingPreviousDocuments);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("For t2l reception, ShouldCheckMissingPreviousDocuments", false, invoiceLine.ShouldCheckMissingPreviousDocuments);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("For import t2c, ShouldCheckMissingPreviousDocuments", true, invoiceLine.ShouldCheckMissingPreviousDocuments);
		});
	}

	public void TestTariffCaptions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("JI_FormattedTariff Caption", "[33] Tariff", invoiceLine.JI_FormattedTariffInfo.Description);
			AssertEquals("JI_Tariff Caption", "[33] Tariff", invoiceLine.JI_TariffInfo.Description);
		});
	}

	public void TestJI_CustomsThirdQuantityCaption()
	{
		AssertEquals("[31] Third Qty", invoiceLine.JI_CustomsThirdQuantityInfo.Description);
	}

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		return declaration;
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	public void TestSetDefaultTaxOrFeeCode()
	{
		var (tariff, expectedTaxType) = SetupTariffAndFeeDetails();
		var declaration = Factory.New<BaseJobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
		AssertEquals(invoiceLine.UseUniversalTariff ? expectedTaxType : ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
	}

	(TariffView tariff, ZString expectedTaxType) SetupTariffAndFeeDetails()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateTaxOrFee("IV1", 0.21, countryCode);
		var tariffType = helper.CreateTariffType(countryCode, "EXP");
		Factory.Save();

		var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "IV1");
		Factory.Save();
		return (tariff, ZString.Empty);
	}

	public void TestCusSupplyChainActorReferences()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nameof(jobComInvoiceLine.CusSupplyChainActorReferences), jobComInvoiceLine.CusSupplyChainActorReferences);
	}

	public void TestGetTariffNomenclatureSelectionModes()
	{
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var selectionModes = invoiceLine.GetTariffNomenclatureSelectionModes();
		AssertSelectionModes(selectionModes, new[] { SelectionStyle.Tariff }, ZString.Empty);

		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
		selectionModes = invoiceLine.GetTariffNomenclatureSelectionModes();
		AssertSelectionModes(selectionModes, new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff }, ExsEntrySubStyleList.Codes.EXS);

		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
		selectionModes = invoiceLine.GetTariffNomenclatureSelectionModes();
		AssertSelectionModes(selectionModes, new[] { SelectionStyle.Tariff }, ExsEntrySubStyleList.Codes.A);

		void AssertSelectionModes(IReadOnlyCollection<SelectionStyle> actualModes, SelectionStyle[] expectedModes, ZString entrySubStyle)
		{
			var entrySubStyleText = entrySubStyle.IsEmpty ? "EMPTY" : entrySubStyle.ToString();
			AssertNotNull($"Selection Modes for {entrySubStyleText}", actualModes);
			AssertEquals($"Selection Mode for {entrySubStyleText} Count", expectedModes.Length, actualModes.Count);
			AssertArrayEqualsByElements($"Selection Modes for {entrySubStyleText}", expectedModes, actualModes.ToArray());
		}
	}

	public void TestGetAdditionalProcedureCodesList()
	{
		var additionalProcedures = invoiceLine.GetAdditionalProcedureCodesList();
		AssertEquals("Expected empty GetAdditionalProcedureCodesList when no data declared", 0, additionalProcedures.Length);

		invoiceLine.JI_Procedure = "1049123";
		additionalProcedures = invoiceLine.GetAdditionalProcedureCodesList();
		AssertList("Only one element with only JI_Procedure declared", new ZString[] { "123" }, additionalProcedures);

		invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
		invoiceLine.AdditionalProcedureCodes.AddNew("789100");
		additionalProcedures = invoiceLine.GetAdditionalProcedureCodesList();
		AssertList("Only one element with JI_Procedure and AdditionalProcedureCodes declared, all codes are returned", new ZString[] { "123", "F89", "100" }, additionalProcedures);

		void AssertList(ZString messageText, ZString[] expectedList, ZString[] actualList)
		{
			AssertEquals(messageText + " Count", expectedList.Length, actualList.Length);
			AssertArrayEqualsByElements(messageText + " elements", expectedList, actualList);
		}
	}

	public void TestGetAdditionalProcedureCodesListForExportUccMessage()
	{
		var additionalProcedures = invoiceLine.GetAdditionalProcedureCodesListForExportUccMessage();
		AssertEquals("Expected empty GetAdditionalProcedureCodesList when no data declared", 0, additionalProcedures.Length);

		invoiceLine.JI_Procedure = "1049123";
		additionalProcedures = invoiceLine.GetAdditionalProcedureCodesListForExportUccMessage();
		AssertList("Only one element with only JI_Procedure declared", new ZString[] { "123" }, additionalProcedures);

		invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
		invoiceLine.AdditionalProcedureCodes.AddNew("789100");
		additionalProcedures = invoiceLine.GetAdditionalProcedureCodesListForExportUccMessage();
		AssertList("Only one element with JI_Procedure and AdditionalProcedureCodes declared, only first 2 codes are returned", new ZString[] { "F89", "123" }, additionalProcedures);

		void AssertList(ZString messageText, ZString[] expectedList, ZString[] actualList)
		{
			AssertEquals(messageText + " Count", expectedList.Length, actualList.Length);
			AssertArrayEqualsByElements(messageText + " elements", expectedList, actualList);
		}
	}

	public void TestZG_HasNonRecycledPlasticsCaption()
	{
		var resourceString = DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_HasNonRecycledPlastics));
		AssertEquals("Caption", "Contains non-recycled plastics?", resourceString.Caption);
	}

	public void TestGetGuidedDecisionMakingSource()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var source = invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource();
		AssertEquals("GetGuidedDecisionMakingSingleInvoiceLineSource return type", typeof(GuidedDecisionMakingSingleInvoiceLineSource), source.GetType());

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var source2 = invoiceLine2.GetGuidedDecisionMakingMultiInvoiceLinesSource();
		AssertEquals("GetGuidedDecisionMakingMultiInvoiceLinesSource return type", typeof(GuidedDecisionMakingMultiInvoiceLinesSource), source2.GetType());
	}

	public void TestGetGuidedDecisionMakingTarget()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var source = invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineTarget();
		AssertEquals("GetGuidedDecisionMakingSingleInvoiceLineTarget return type", typeof(GuidedDecisionMakingSingleInvoiceLineTarget), source.GetType());

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		var source2 = invoiceLine2.GetGuidedDecisionMakingMultiInvoiceLinesTarget(new List<EU.Business.Declaration.JobComInvoiceLine> { invoiceLine, invoiceLine2 });
		AssertEquals("GetGuidedDecisionMakingMultiInvoiceLinesTarget return type", typeof(GuidedDecisionMakingMultiInvoiceLinesTarget), source2.GetType());
	}

	public void TestZG_CountryOfDispatch()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(InvoiceLine.ZG_CountryOfDispatchInfo, "Country of Dispatch", "Ctry. Dispatch", "Ctry. Disp.", "Country of Dispatch of the goods");
		});
	}

	public void TestZG_RegionOfDestination()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(InvoiceLine.ZG_RegionOfDestinationInfo, "Region of Destination", "Reg. Destination", "Reg. Dest.", "State or Region of Destination");
		});
	}

	public void TestIsCountryOfDestinationESOrXCOrXLOrEmpty()
	{
		InvoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Spain;
		CombineAssertions(() =>
		{
			AssertEquals("JE_GoodsDestination is ES, Expected true", true, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
			AssertEquals("JE_GoodsDestination is FR, Expected false", false, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XC;
			AssertEquals("JE_GoodsDestination is XC, Expected true", true, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Germany;
			AssertEquals("JE_GoodsDestination is DE, Expected false", false, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = Core.Constants.NonStandardCountryCodes.Codes.XL;
			AssertEquals("JE_GoodsDestination is XL, Expected true", true, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Italy;
			AssertEquals("JE_GoodsDestination is IT, Expected false", false, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
			InvoiceLine.ZG_CountryOfDestination = ZString.Empty;
			AssertEquals("JE_GoodsDestination is Empty, Expected true", true, InvoiceLine.IsCountryOfDestinationESOrXCOrXLOrEmpty);
		});
	}

	public void TestJI_ValuationCode()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(InvoiceLine.JI_ValuationCodeInfo, "Valuation Method", "Val. Method", "Val. Method", "Valuation Method Code");
		});
	}

	public override void TestGetNewLinkPackValidation()
	{
		AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
	}

	protected override Type ExpectedGuidedDecisionMakingBasicType => typeof(GuidedDecisionMakingBasic);

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dec = Factory.New<JobDeclaration>();
		var invoiceHeader = dec.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
	}

	JobDeclaration dec;
	JobComInvoiceLine invoiceLine;

	protected override ZString? NoVariableDefaultDataGroupingCodeCountry => Core.Constants.CountryCodes.Spain;

	protected override ZString OFTChargeDescription => "TRANSPORT COSTS, LOADING AND HANDLING CHARGES  from Entry";

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

	protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);
}
