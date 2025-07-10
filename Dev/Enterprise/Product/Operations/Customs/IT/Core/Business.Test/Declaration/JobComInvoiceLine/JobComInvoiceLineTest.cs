using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	public void TestJI_ZZF_NKTaxTypeReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertEquals("JI_ZZF_NKTaxType ReadOnly", false, invoiceLine.JI_ZZF_NKTaxTypeInfo.ReadOnly);
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be IT", Core.Constants.CountryCodes.Italy, InvoiceLine.CustomsCountryCode);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Italy, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestPreviousDocumentsCorrectType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<PreviousDocumentCollection>(line.PreviousDocuments);
	}

	public void TestUniversalTariffType()
	{
		var dec = Factory.New<JobDeclaration>();
		var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("UniversalTariffType", () =>
		{
			dec.JE_MessageType = "IMP";
			AssertEquals("When declaration is IMP, IMP is expected ", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, invLine.UniversalTariffType);

			dec.JE_MessageType = "EXP";
			AssertEquals("When declaration is EXP, EXP is expected ", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ExportTariff, invLine.UniversalTariffType);
		});
	}

	public void TestUniversalTariffTypeWithNullDeclaration()
	{
		AssertNoExceptionThrown("When declaration is null no exception is expected", () => { var tariff = Factory.New<JobComInvoiceLine>().UniversalTariffType; });
	}

	public void TestValidation()
	{
		var dec = Factory.New<JobDeclaration>();
		var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions("Validation Type", () =>
		{
			dec.JE_MessageType = "";
			AssertType<CommonJobComInvoiceLineValidation>("JE_MessageType = ''", invLine.Validation);

			dec.JE_MessageType = "IMP";
			AssertType<ImportJobComInvoiceLineValidation>("JE_MessageType = 'IMP'", invLine.Validation);

			dec.JE_MessageType = "EXP";
			AssertType<ExportJobComInvoiceLineValidation>("JE_MessageType = 'EXP'", invLine.Validation);
		});
	}

	public void TestLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<JobComInvoiceLineLookups>(line.Lookups);
	}

	public void TestSetDefaultValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals(SteelTypeList.Codes._0, line.ZG_SteelType);
	}

	public void TestValuationCode_Defaulting()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals("When Message type = IMP", "1", invLine.JI_ValuationCode);

		declaration.JE_MessageType = "EXP";
		invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertEquals("When Message type = EXP", "", invLine.JI_ValuationCode);
	}

	public void TestValuationCode_DeclarationMissing()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		AssertEquals("When Declaration Missing", "", invoiceLine.JI_ValuationCode);
	}

	public void TestSupportingDocumentsCorrectType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<SupportingDocumentCollection>(line.SupportingDocuments);
	}

	public void TestPackagesPivot()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertType<InvoiceLinePackagePivotCollection>("Expected type is InvoiceLinePackagePivotCollection", invoiceLine.PackagesPivot);
	}

	public void TestMaxNumberOfAdditionalProcedureCode()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertEquals("MaxNumberOfAdditionalProcedureCode", 99, invoiceLine.MaxNumberOfAdditionalProcedureCode);
	}

	public void TestProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage()
	{
		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		AssertEquals("To select additional procedure codes the procedure/CPC must be filled in.", invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage);
	}

	public void TestIsTemporaryProcedure()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "";
		AssertEquals("It is not a temporary Proc", false, invoiceLine.IsTemporaryProcedure);

		testDataHelper.CreateNewRefCusProcedure(procedureCode: "400000", shipmentType: "IMP");
		invoiceLine.JI_Procedure = "400000";
		AssertEquals("It is not a temporary Proc", false, invoiceLine.IsTemporaryProcedure);

		invoiceLine.JI_Procedure = "X2";
		AssertEquals("It is not a temporary Proc", false, invoiceLine.IsTemporaryProcedure);

		testDataHelper.CreateNewRefCusProcedure(procedureCode: "200010", shipmentType: "IMP", configurationAction: x => x.ZZ6_IntoTemporaryImport = "Y");
		invoiceLine.JI_Procedure = "200010";
		AssertEquals("It is a temporary Proc", true, invoiceLine.IsTemporaryProcedure);

		testDataHelper.CreateNewRefCusProcedure(procedureCode: "500010", shipmentType: "EXP", configurationAction: x => x.ZZ6_IntoTemporaryExport = "Y");
		declaration.JE_MessageType = "EXP";
		invoiceLine.JI_Procedure = "500010";
		AssertEquals("It is a temporary Proc", true, invoiceLine.IsTemporaryProcedure);
	}

	public void TestEntryInstructionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		line.JI_CEI = entryInstruction.PK;
		AssertType<CusEntryInstruction>(line.EntryInstruction);
	}

	public override void TestProcedureLookupsAndValidation()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
		var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
		var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
		var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "IFD";

		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_RN_NKCountryOfExport = GlbCompany.CurrentCompany.Country.Code;
		AssertEquals("JI_CEI automatically set when assigning JE_DeclarationType", entryInstruction.PK, invoiceLine1.JI_CEI);
		AssertEquals(null, invoiceLine1.CusProcedure);
		var cpcs = invoiceLine1.Lookups.CPCList;
		AssertEquals("IT lookups has all CPCs filters by shipmentType=IMP & procedureCode", 2, cpcs.Count);
		entryInstruction.CEI_Procedure = "11";
		cpcs = invoiceLine1.Lookups.CPCList;
		AssertEquals("IT lookups has all CPCs filters by shipmentType=IMP & procedureCode=11", 1, cpcs.Count);
		Assert("IT lookups has all CPCs filters by shipmentType=IMP & procedureCode=11", cpcs.Contains(procedure1));

		entryInstruction.CEI_Procedure = "11";
		invoiceLine1.JI_Procedure = "1111111";
		AssertNoMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "list");
		AssertEquals(procedure1, invoiceLine1.CusProcedure);
		entryInstruction.CEI_Procedure = "22";
		invoiceLine1.JI_Procedure = "2222222";
		AssertNoMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "list");
		AssertEquals(procedure2, invoiceLine1.CusProcedure);
		invoiceLine1.JI_Procedure = "3333333";
		AssertHasMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "list");
		AssertEquals(procedure3, invoiceLine1.CusProcedure);
		invoiceLine1.JI_Procedure = "xxxxxxx";
		AssertHasMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "list");
		AssertEquals(null, invoiceLine1.CusProcedure);

		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertNoMessageErrorContaining(invoiceLine1.JI_ProcedureInfo, "series");
		invoiceLine1.JI_Procedure = "1111111";
		invoiceLine2.JI_Procedure = "3333333";
		AssertHasMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, "series");

		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Style = "EFD";
		invoiceLine2.JI_CEI = entryInstruction.PK;
		cpcs = invoiceLine2.Lookups.CPCList;
		AssertEquals("IT lookups has all CPCs filters by shipmentType=EXP & procedureCode", 0, cpcs.Count);
		entryInstruction.CEI_Procedure = "44";
		cpcs = invoiceLine2.Lookups.CPCList;
		AssertEquals("IT lookups has all CPCs filters by shipmentType=EXP & procedureCode=44", 1, cpcs.Count);
		Assert("IT lookups has all CPCs filters by shipmentType=EXP & procedureCode=44", cpcs.Contains(procedure4));
		invoiceLine2.JI_Procedure = "4444444";
		AssertNoMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, "list");
		AssertEquals(procedure4, invoiceLine2.CusProcedure);
	}

	public void TestInvoiceLinePackageValidationType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var supporter = (ICusLinkPackageSupporter)invoiceLine;
		var basePackage = new BaseCusLinkPackageCollection(invoiceLine).AddNew();
		AssertType<InvoiceLinePackageValidation>(supporter.GetNewLinkPackValidation(basePackage));
	}

	public void TestCheckJI_CEI()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_CEI = ZGuid.Empty;
		AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		invoiceLine.JI_CEI = entryInstruction.PK;
		AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestDefaultSupplementaryQuantityUOMFromTariff()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var tariffWithMultipleUnits = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariffUOM(tariffWithMultipleUnits, UnitOfMeasureTypes.AdditionalUOMType, "XXX");

		helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "3333333333", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var tariffWithOneUnit = helper.CreateTariff(Core.Constants.CountryCodes.Italy, tariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(tariffWithOneUnit, UnitOfMeasureTypes.AdditionalUOMType, "SSS");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		invoiceLine.JI_CustomsSecondUnitQty = "";
		invoiceLine.JI_Tariff = "1111111111";
		AssertEquals("When setting a tariff with multiple UOMs, CustomsSecondUnitQty is not defaulted, but set to empty", "", invoiceLine.JI_CustomsSecondUnitQty);

		invoiceLine.JI_CustomsSecondUnitQty = "";
		invoiceLine.JI_Tariff = "3333333333";
		AssertEquals("When setting a tariff with no UOMs, CustomsSecondUnitQty is not defaulted, but set to empty", "", invoiceLine.JI_CustomsSecondUnitQty);

		invoiceLine.JI_CustomsSecondUnitQty = "";
		invoiceLine.JI_Tariff = "2222222222";
		AssertEquals("When setting a tariff with one UOM, CustomsSecondUnitQty is defaulted", "SSS", invoiceLine.JI_CustomsSecondUnitQty);

		invoiceLine.JI_Tariff = "";
		AssertEquals("When changing a tariff (selecting one without UOMs), CustomsSecondUnitQty is set to empty", "", invoiceLine.JI_CustomsSecondUnitQty);

		invoiceLine.JI_CustomsSecondUnitQty = "PPP";
		invoiceLine.JI_Tariff = "2222222222";
		AssertEquals("When setting a tariff with one UOMs, and CustomsSecondUnitQty is set, CustomsSecondUnitQty is replaced", "SSS", invoiceLine.JI_CustomsSecondUnitQty);

		invoiceLine.JI_CustomsSecondUnitQty = "PPP";
		invoiceLine.JI_Tariff = "3333333333";
		AssertEquals("When setting a tariff with no UOMs, CustomsSecondUnitQty is set to empty", "", invoiceLine.JI_CustomsSecondUnitQty);
	}

	public void TestDefaultThridQuantityUOMFromTariff()
	{
		UniversalReferenceHelperTest.SetupTariffForTestintQuantityUOMs(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

		invoiceLine.JI_CustomsThirdUnitQty = "DTNE";
		invoiceLine.JI_Tariff = "1";
		AssertEquals("When selected tariff is 1, JI_CustomsThirdUnitQty should be set to empty", ZString.Empty, invoiceLine.JI_CustomsThirdUnitQty);

		invoiceLine.JI_Tariff = "";
		invoiceLine.JI_Tariff = "0710400090";
		AssertEquals("When selected tariff has a formula: VFD * 0.051 + 9.400 * [DTNE], then JI_CustomsThirdUnitQty should be set", "DTNE", invoiceLine.JI_CustomsThirdUnitQty);

		invoiceLine.JI_Tariff = "DTNZ";
		invoiceLine.JI_Tariff = "0710400090";
		AssertEquals("When selected tariff has a formula: VFD * 0.051 + 9.400 * [DTNE], then JI_CustomsThirdUnitQty should be set", "DTNE", invoiceLine.JI_CustomsThirdUnitQty);
	}

	public void TestSuggestedCustomsThirdQuantity()
	{
		UniversalReferenceHelperTest.SetupTariffForTestintQuantityUOMs(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

		invoiceLine.JI_Tariff = "";
		AssertEquals("When selected tariff is empty, SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "1";
		AssertEquals("When selected tariff is 1, SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "1602321900";
		AssertEquals("When selected tariff has a formula: VFD * 0.080, then SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "8501538190";
		AssertEquals("When selected tariff has a formula: 787.81 * [TNE], then SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "2009893579";
		AssertEquals("When selected tariff has a formula: VFD * 0.336 + 20.600 * [DTN], then SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "9102290000";
		AssertEquals("When selected tariff has a formula: MIN(MAX(VFD * 0.045, 0.300 * [NAR]), 0.800 * [NAR]), then SuggestedCustomsThirdQuantityUOM should be", ZString.Empty, invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "0710400090";
		AssertEquals("When selected tariff has a formula: VFD * 0.051 + 9.400 * [DTNE], then SuggestedCustomsThirdQuantityUOM should be", "DTNE", invoiceLine.SuggestedCustomsThirdQuantityUOM);

		invoiceLine.JI_Tariff = "1702907100";
		AssertEquals("When selected tariff has a formula: 0.400 * [DTNZ], then SuggestedCustomsThirdQuantityUOM should be", "DTNZ", invoiceLine.SuggestedCustomsThirdQuantityUOM);
	}

	public void TestSetSecondQuantityFromEdiTariffsOwnRecord()
	{
		var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
		AssertEquals("SetSecondQuantityFromEdiTariffsOwnRecord is false for IT invoice line", false, invoiceLine.SetSecondQuantityFromEdiTariffsOwnRecordExposed);
	}

	public void TestRemarks()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.Remarks = "this is a description";
		AssertEquals("this is a description", invoiceLine.Remarks);

		invoiceLine.Remarks = ZString.Empty;
		AssertEquals(ZString.Empty, invoiceLine.Remarks);
	}

	public void TestRemarksMaxLength()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertEquals(300, invoiceLine.RemarksInfo.MaxLength);
	}

	public void TestEffectiveRemarks()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();

		CombineAssertions(() =>
		{
			invoiceLine.Remarks = "";
			AssertEquals("EffectiveRemarks", "", invoiceLine.EffectiveRemarks);

			invoiceLine.Remarks = "\r\n";
			AssertEquals("EffectiveRemarks", "", invoiceLine.EffectiveRemarks);

			invoiceLine.Remarks = "\r\nRemarks1";
			AssertEquals("EffectiveRemarks", "Remarks1", invoiceLine.EffectiveRemarks);

			invoiceLine.Remarks = "Remarks2\r\n";
			AssertEquals("EffectiveRemarks", "Remarks2", invoiceLine.EffectiveRemarks);

			invoiceLine.Remarks = "Remarks3\r\nRemarks4";
			AssertEquals("EffectiveRemarks", "Remarks3 Remarks4", invoiceLine.EffectiveRemarks);
		});
	}

	public void TestAdditionalInfosType()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<AdditionalInfoCollection>(invoiceLine.AdditionalInfos);
	}

	public void TestEffectiveAdditionalInfos_WhenHasParentInvoice()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceAddInfo1 = invoice.AdditionalInfos.AddNew();
		var invoiceAddInfo2 = invoice.AdditionalInfos.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var invoiceLineAddInfo1 = invoiceLine.AdditionalInfos.AddNew();
		var invoiceLineAddInfo2 = invoiceLine.AdditionalInfos.AddNew();

		var effectiveAdditionalInfos = invoiceLine.EffectiveAdditionalInfos();
		AssertContainsExactElementsInAnyOrder(
			"EffectiveAdditionalInfos contains Invoice Line and Invoice AdditionalInfo records (regardless the presence of RefCusCodeListAttribute)",
			new EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo[] { invoiceAddInfo1, invoiceAddInfo2, invoiceLineAddInfo1, invoiceLineAddInfo2 },
			effectiveAdditionalInfos);
	}

	public void TestEffectiveAdditionalInfos_WhenHasNoParentInvoice()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var invoiceLineAddInfo1 = invoiceLine.AdditionalInfos.AddNew();
		var invoiceLineAddInfo2 = invoiceLine.AdditionalInfos.AddNew();

		var effectiveAdditionalInfos = invoiceLine.EffectiveAdditionalInfos();
		AssertContainsExactElementsInAnyOrder(
			"EffectiveAdditionalInfos only contains Invoice Line AdditionalInfo records",
			new EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo[] { invoiceLineAddInfo1, invoiceLineAddInfo2 },
			effectiveAdditionalInfos);
	}

	public void TestAddInfoLookups()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNotNull("AddInfoLookups", invoiceLine.AddInfoLookups);
		AssertType<AddInfoJobComInvoiceLineLookups>("AddInfoLookups type", invoiceLine.AddInfoLookups);
	}

	public void TestAddInfoValidation()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNotNull("AddInfoValidation", invoiceLine.AddInfoValidation);
		AssertType<AddInfoJobComInvoiceLineValidation>("AddInfoValidation type", invoiceLine.AddInfoValidation);
	}

	public void TestPortTaxRate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		invoiceLine.ZG_PortTaxRate = "A1";

		declaration.JE_TransportMode = "AIR";
		AssertEquals("ZG_PortTaxRate", "", invoiceLine.ZG_PortTaxRate);

		declaration.JE_TransportMode = "SEA";
		invoiceLine.ZG_PortTaxRate = "A1";
		declaration.JE_RL_NKPortOfArrival = "ITTRS";
		AssertEquals("ZG_PortTaxRate", "", invoiceLine.ZG_PortTaxRate);

		invoiceLine.ZG_PortTaxRate = "A1";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		AssertEquals("ZG_PortTaxRate", "A1", invoiceLine.ZG_PortTaxRate);

		declaration.JE_RL_NKPortOfArrival = "";
		declaration.JE_MessageType = "EXP";
		invoiceLine.ZG_PortTaxRate = "A1";
		declaration.JE_RL_NKPortOfLoading = "ITTRS";
		AssertEquals("ZG_PortTaxRate", "", invoiceLine.ZG_PortTaxRate);
	}

	void AssertPortTaxRateReadOnly(bool generatePortTaxes)
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITCEJ";
		AssertEquals("PortTaxRateReadOnly", !generatePortTaxes, invoiceLine.PortTaxRateReadOnly);

		declaration.JE_TransportMode = "AIR";
		AssertEquals("PortTaxRateReadOnly", true, invoiceLine.PortTaxRateReadOnly);

		declaration.JE_MessageType = "EXP";
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfLoading = "ITTRS";
		AssertEquals("PortTaxRateReadOnly", true, invoiceLine.PortTaxRateReadOnly);

		declaration.JE_RL_NKPortOfLoading = "ITCEJ";
		AssertEquals("PortTaxRateReadOnly", !generatePortTaxes, invoiceLine.PortTaxRateReadOnly);
	}

	public void TestPortTaxRateReadOnlyWhenGeneratePortTaxIsTrue()
	{
		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		AssertPortTaxRateReadOnly(true);
	}

	public void TestPortTaxRateReadOnlyWhenGeneratePortTaxIsFalse()
	{
		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		AssertPortTaxRateReadOnly(false);
	}

	public void TestSetDefaultPortCommodityRateSettingPartNo()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "PortTaxProduct";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		var customsPivot = product.PivotsForBinding.AddNew();
		customsPivot.CI_ChildType = "IMP";
		var productTax = customsPivot.Taxes.AddNew().Data;
		productTax.G4_Type = "9AA";
		productTax.G4_PortTaxRate = "A1";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Supplier = org.PK;
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";
		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		Factory.Save();

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		invoiceLine.JI_PartNo = product.OP_PartNum;
		AssertEquals("ZG_PortTaxRate", "A1", invoiceLine.ZG_PortTaxRate);

		invoiceLine.ZG_PortTaxRate = "A3";
		invoiceLine.JI_PartNo = product.OP_PartNum;
		AssertEquals("ZG_PortTaxRate", "A3", invoiceLine.ZG_PortTaxRate);

		invoiceLine.JI_PartNo = "";
		invoiceLine.ZG_PortTaxRate = "";
		declaration.JE_RL_NKPortOfArrival = "ITTRS";
		invoiceLine.JI_PartNo = product.OP_PartNum;
		AssertEquals("ZG_PortTaxRate", "", invoiceLine.ZG_PortTaxRate);

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		invoiceLine.ZG_PortTaxRate = "";
		invoiceLine.JI_PartNo = "";
		invoiceLine.JI_PartNo = product.OP_PartNum;
		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		AssertEquals("ZG_PortTaxRate", "", invoiceLine.ZG_PortTaxRate);
	}

	public void TestVatRateDescription()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFeeWithRelatedVatApplicability("IMP", "99999999", ("ORD", 0.21000000m, ""), ("RID", 0.04500000m, ""));

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "99999999";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		AssertEquals("VatRateDescription", "VAT Rate 21%", invoiceLine.VatRateDescription);

		invoiceLine.JI_ZZF_NKTaxType = "RID";
		AssertEquals("VatRateDescription", "VAT Rate 4.5%", invoiceLine.VatRateDescription);

		invoiceLine.JI_ZZF_NKTaxType = "";
		AssertEquals("[No VAT Selected] VatRateDescription", "", invoiceLine.VatRateDescription);
	}

	public override void TestGetNewPackagesPivotCollection()
	{
		SetUpPackageTypes();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = "BLT";

		declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var pack = declaration.Packages.AddNew();
		pack.CW_PackQty = 1;
		pack.CW_PackType = "CT";
		pack.CW_MarksAndNos = "IND";

		Assert("Precondition Pack is IT Package class", pack is Package);
		Assert("Precondition InvoiceLine is IT InvoiceLine class", invoiceLine1 is JobComInvoiceLine);
		Assert("Precondition SupportsChcPivot", invoiceLine1.SupportsChcPivotBetweenInvoiceLineAndPacking);

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine1, pack, 0);

		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertNoMessageErrorContaining(packagePivot.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);

		declaration.DoMerge();

		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertHasMessageErrorContaining(packagePivot.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);

		pack.CW_PackType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;

		packagePivot.Validation.ValidateAll();
		AssertNoMessageErrorContaining(packagePivot.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
	}

	public override void TestIsSupportEmptyPackTypeAndValidation()
	{
		SetUpPackageTypes();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = "BLT";

		declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var pack = declaration.Packages.AddNew();
		pack.CW_PackQty = 1;
		pack.CW_PackType = "CT";
		pack.CW_MarksAndNos = "IND";

		Assert("Precondition Pack is EU Package class", pack is Package);
		Assert("Precondition InvoiceLine is EU InvoiceLine class", invoiceLine1 is JobComInvoiceLine);
		Assert("Precondition SupportsChcPivot", invoiceLine1.SupportsChcPivotBetweenInvoiceLineAndPacking);

		var packagePivot = MergeTestHelper.GetNewPackagePivot(invoiceLine1, pack, 0);

		declaration.DoMerge();

		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertHasMessageErrorContaining(packagePivot.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);

		pack.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertNoMessageErrorContaining(packagePivot.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
	}

	public void TestGetEffectiveSupplementaryCodesRelatedToVATApplicabilities()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFeeWithRelatedVatApplicability("IMP", "99999999", ("ORD", 21m, "Q001"), ("RID", 4.5m, "Q002"));

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		AssertArrayEqualsByElements("When Tariff is empty, GetEffectiveSupplementaryCodesRelatedToVATApplicabilities()", Array.Empty<ZString>(), invoiceLine.GetEffectiveSupplementaryCodesRelatedToVATApplicabilities().ToArray());

		invoiceLine.JI_Tariff = "99999999";
		AssertContainsExactElementsInAnyOrder("GetEffectiveSupplementaryCodesRelatedToVATApplicabilities()", new ZString[] { "Q001", "Q002" }, invoiceLine.GetEffectiveSupplementaryCodesRelatedToVATApplicabilities().ToArray());
	}

	public void TestAddMissingSupportingDocuments()
	{
		MissingSupportingDocumentParentTest.SetupRefCusConditionValueForMissingSupportingDocumentImportTest(Factory, true);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertNoExceptionThrown("No exception expected when missingSupportingDocuments is null", () => invoiceLine.AddMissingSupportingDocuments(null));

		invoiceLine.JI_Tariff = "800900";
		var missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(invoiceLine);

		var allMissingSupportingDocuments = missingSupportingDocumentParent.MissingSupportingDocumentGrouped.SelectMany(x => x.MissingSupportingDocuments);
		allMissingSupportingDocuments.Where(x => x.Code == "XA").ToList().ForEach(x => x.ShouldImport = true);
		allMissingSupportingDocuments.SingleOrDefault(x => x.Code == "YA").ShouldImport = true;

		invoiceLine.AddMissingSupportingDocuments(missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport());

		AssertEquals("SupportingDocuments count", 2, invoiceLine.SupportingDocuments.Count);

		var supportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>();
		void AssertMissingSupportingDocumentsAreImportedCorrectly()
		{
			var supportingDocumentCodes = supportingDocuments.Select(x => x.CSI_Code).ToArray();
			CombineAssertions("Check Invoice Line Supporting Documents", () =>
			{
				AssertCollectionContains("XA", supportingDocumentCodes);
				AssertEquals("Contains only one XA Document", 1, supportingDocumentCodes.Where(x => x == "XA").Count());
				AssertCollectionContains("YA", supportingDocumentCodes);
			});
		}
		AssertMissingSupportingDocumentsAreImportedCorrectly();
		supportingDocuments.Single(x => x.CSI_Code == "XA").Delete();

		invoiceLine.AddMissingSupportingDocuments(missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport());
		AssertEquals("SupportingDocuments count", 2, invoiceLine.SupportingDocuments.Count);
		AssertMissingSupportingDocumentsAreImportedCorrectly();

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var xaDeclarationAtDeclarationLevel = declaration.SupportingDocuments.AddNew();
		xaDeclarationAtDeclarationLevel.CSI_Code = "XA";

		invoiceLine.AddMissingSupportingDocuments(missingSupportingDocumentParent.GetMissingSupportingDocumentsToImport());
		AssertEquals("SupportingDocuments count", 1, invoiceLine.SupportingDocuments.Count);
		AssertEquals("Imported Supporting Document Code", "YA", supportingDocuments.Single().CSI_Code);
	}

	public void TestActualSupportingDocumentCodeCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "C100";

		AssertEquals("Actual Supporting Document Code Collection count", 1, invoiceLine.ActualSupportingDocumentCodeCollection.Count());
		AssertEquals("Actual Supporting Document Code Collection", "C100", invoiceLine.ActualSupportingDocumentCodeCollection.Single().CSI_Code);

		invoice.SupportingDocuments.AddNew().CSI_Code = "Y900";

		AssertEquals("AActual Supporting Document Code Collection count", 2, invoiceLine.ActualSupportingDocumentCodeCollection.Count());
		CombineAssertions("Assert Actual Supporting Document Code Collection inherit elements from InvoiceHeader Supporting Documents", () =>
		{
			var actualSupportingDocumentCodes = invoiceLine.ActualSupportingDocumentCodeCollection.Select(x => x.CSI_Code).ToArray();

			AssertCollectionContains("Y900", actualSupportingDocumentCodes);
			AssertCollectionContains("C100", actualSupportingDocumentCodes);
		});

		declaration.SupportingDocuments.AddNew().CSI_Code = "D200";

		AssertEquals("Actual Supporting Document Code Collection count", 3, invoiceLine.ActualSupportingDocumentCodeCollection.Count());

		CombineAssertions("Assert Actual Supporting Document Code Collection inherit elements from InvoiceHeader and Declaration Supporting Documents", () =>
		{
			var actualSupportingDocumentCodes = invoiceLine.ActualSupportingDocumentCodeCollection.Select(x => x.CSI_Code).ToArray();

			AssertCollectionContains("D200", actualSupportingDocumentCodes);
			AssertCollectionContains("Y900", actualSupportingDocumentCodes);
			AssertCollectionContains("C100", actualSupportingDocumentCodes);
		});

		entryInstruction.SupportingDocuments.AddNew().CSI_Code = "N018";
		CombineAssertions("Assert Actual Supporting Document Code Collection inherit elements from InvoiceHeader, Entry Instruction and Declaration Supporting Documents", () =>
		{
			var actualSupportingDocumentCodes = invoiceLine.ActualSupportingDocumentCodeCollection.Select(x => x.CSI_Code).ToArray();

			AssertCollectionContains("D200", actualSupportingDocumentCodes);
			AssertCollectionContains("Y900", actualSupportingDocumentCodes);
			AssertCollectionContains("C100", actualSupportingDocumentCodes);
			AssertCollectionContains("N018", actualSupportingDocumentCodes);
		});
	}

	public void TestHasSameConditionSelectionCriteria()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		invoiceLine1.JI_PrimaryPreference = "100";
		invoiceLine2.JI_PrimaryPreference = "120";
		AssertEquals("Invoice lines with different PrimaryPreference, HasSameConditionSelectionCriteria", false, invoiceLine1.HasSameConditionSelectionCriteria(invoiceLine2.ConditionSelectionCriterias[0]));

		invoiceLine2.JI_PrimaryPreference = "100";
		invoiceLine1.JI_CountryOfOrigin = "CN";
		invoiceLine2.JI_CountryOfOrigin = "US";
		AssertEquals("Invoice lines with different CountryOfOrigin, HasSameConditionSelectionCriteria", false, invoiceLine1.HasSameConditionSelectionCriteria(invoiceLine2.ConditionSelectionCriterias[0]));

		invoiceLine2.JI_CountryOfOrigin = "CN";
		invoiceLine1.JI_ConcessionOrder = "1";
		invoiceLine2.JI_ConcessionOrder = "2";
		AssertEquals("Invoice lines with different ConcessionOrder, HasSameConditionSelectionCriteria", false, invoiceLine1.HasSameConditionSelectionCriteria(invoiceLine2.ConditionSelectionCriterias[0]));

		invoiceLine2.JI_ConcessionOrder = "1";
		invoiceLine1.JI_SupplementaryCode1 = "Q001";
		invoiceLine2.JI_SupplementaryCode1 = "Q002";
		AssertEquals("Invoice lines with different SupplementaryCode, HasSameConditionSelectionCriteria", false, invoiceLine1.HasSameConditionSelectionCriteria(invoiceLine2.ConditionSelectionCriterias[0]));

		invoiceLine1.JI_SupplementaryCode2 = "Q002";
		invoiceLine2.JI_SupplementaryCode2 = "Q001";
		AssertEquals("Invoice lines with same ConditionSelectionCriteria, HasSameConditionSelectionCriteria", true, invoiceLine1.HasSameConditionSelectionCriteria(invoiceLine2.ConditionSelectionCriterias[0]));
	}

	public void TestIsTaxTypeDefaultingSuspended()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		using (invoiceLine.SuspendTaxTypeDefaulting())
		{
			using (invoiceLine.SuspendTaxTypeDefaulting())
			{
				AssertEquals("IsTaxTypeDefaultingSuspended", true, invoiceLine.IsTaxTypeDefaultingSuspended);
			}
			AssertEquals("IsTaxTypeDefaultingSuspended", true, invoiceLine.IsTaxTypeDefaultingSuspended);
		}
		AssertEquals("IsTaxTypeDefaultingSuspended", false, invoiceLine.IsTaxTypeDefaultingSuspended);
	}

	public void TestGetValuationCalculator()
	{
		var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
		AssertType<ITCustomsValuationCalculator>(invoiceLine.GetValuationCalculatorCoreExposed());
	}

	public void TestJI_Calc_ExtraEUFreightChargesAmount()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		AssertEquals("No charges", 0m, invoiceLine.JI_Calc_ExtraEUFreightChargesAmount);

		var invoiceLineCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge.J7_IsDutiable = true;
		invoiceLineCharge.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge.J7_IsGSTApplicable = true;
		AssertEquals($"Expected refreshed value. In-depth test cases in {nameof(ITCustomsValuationCalculatorTest)}", 100m, invoiceLine.JI_Calc_ExtraEUFreightChargesAmount);
	}

	public void TestJI_Calc_EUFreightChargesAmount()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		AssertEquals("No charges", 0m, invoiceLine.JI_Calc_EUFreightChargesAmount);

		var invoiceLineCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge.J7_IsDutiable = false;
		invoiceLineCharge.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge.J7_IsGSTApplicable = true;
		AssertEquals($"Expected refreshed value. In-depth test cases in {nameof(ITCustomsValuationCalculatorTest)}", 100m, invoiceLine.JI_Calc_EUFreightChargesAmount);
	}

	public void TestJI_Calc_DomesticFreightChargesAmount()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		AssertEquals("No charges", 0m, invoiceLine.JI_Calc_DomesticFreightChargesAmount);

		var invoiceLineCharge = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 100m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge.J7_IsDutiable = false;
		invoiceLineCharge.J7_IsStatisticalValueApplicable = false;
		invoiceLineCharge.J7_IsGSTApplicable = true;
		AssertEquals($"Expected refreshed value. In-depth test cases in {nameof(ITCustomsValuationCalculatorTest)}", 100m, invoiceLine.JI_Calc_DomesticFreightChargesAmount);
	}

	public void TestSupportingDocumentsManager()
	{
		var supportingDocumentsManager = InvoiceLine.SupportingDocumentsManager;
		AssertNotNull("SupportingDocumentsManager", supportingDocumentsManager);
		AssertEquals("SupportingDocumentsManager should be cached", supportingDocumentsManager, InvoiceLine.SupportingDocumentsManager);
	}

	public void TestAddCustomsDecisionSupportingDocumentChangingProcedureCode()
	{
		var (_, importer, authorisation, _) = InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.SetupAndGetDataForDefaultingSupportingDocument(Factory, "AUTH_NUMBER", "IPO");
		InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AddDocRuleToAuthorisation(authorisation, "C122");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		declaration.JE_OH_Importer = importer.PK;
		entryInstruction.CEI_Procedure = "51";
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine.JI_Procedure = "5100";

		InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine, "C122", authorisation.CPH_Number);
	}

	public void TestAddCustomsDecisionSupportingDocumentChangingEntryInstruction()
	{
		var (customsWarehouseOrganization, importer, authorisation, _) = InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.SetupAndGetDataForDefaultingSupportingDocument(Factory, "AUTH_NUMBER", "CWP");
		InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AddDocRuleToAuthorisation(authorisation, "C122");

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		entryInstruction.CEI_OA_Warehouse2 = customsWarehouseOrganization.MainAddress.PK;
		declaration.JE_OH_Importer = importer.PK;
		entryInstruction.CEI_Procedure = "71";
		invoiceLine.JI_Procedure = "7100";

		invoiceLine.JI_CEI = entryInstruction.PK;
		InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest.AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine, "C122", authorisation.CPH_Number);
	}

	public void TestShouldCheckMissingPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("For Normal Declaration - A, ShouldCheckMissingPreviousDocuments", true, invoiceLine.ShouldCheckMissingPreviousDocuments);

		entryInstruction.CEI_SubStyle = "D";
		AssertEquals("For Preliminary Declaration - D, ShouldCheckMissingPreviousDocuments", false, invoiceLine.ShouldCheckMissingPreviousDocuments);
	}

	public void TestOriginStateSetDefaultValue()
	{
		var orgHeaderIT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderIT.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Italy;

		var declaration = Factory.New<JobDeclaration>();
		declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeaderIT.PK;
		declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
		declaration.SupplierDocumentaryAddress.State = "AN";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		AssertContains(CountryCodes.Italy, invoiceLine.JI_CountryOfOrigin);
		AssertContains("AN", invoiceLine.JI_StateOrRegionOfOrigin);
	}

	public void TestOriginStateDontSetDefaultValue()
	{
		var orgHeaderIT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderIT.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Belgium;

		var declaration = Factory.New<JobDeclaration>();
		declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeaderIT.PK;
		declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
		declaration.SupplierDocumentaryAddress.State = "AN";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		AssertContains(CountryCodes.Belgium, invoiceLine.JI_CountryOfOrigin);
		AssertNullOrEmpty(invoiceLine.JI_StateOrRegionOfOrigin);
	}

	public void TestJI_ZZF_NKTaxType_ResourceData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ZZF_NKTaxTypeInfo);
		AssertEquals("Caption", "IVA", resourceStringData.Caption);
	}

	public void TestJI_CustomsQuantityInExportDeclarationWhenTransitionPeriodIsON()
	{
		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "IsUCC6Core", true, null))
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CustomsQuantity = 12.239238m;
			AssertEquals("JI_CustomsQuantity rounded to 3 decimals", 12.239m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_CustomsQuantity = 13.129876m;
			AssertEquals("JI_CustomsQuantity rounded to 3 decimals", 13.130m, invoiceLine.JI_CustomsQuantity);
		}
	}

	public void TestJI_CustomsQuantityInExportDeclarationWhenTransitionPeriodIsOFF()
	{
		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "IsUCC6Core", true, null))
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CustomsQuantity = 12.239238m;
			AssertEquals("JI_CustomsQuantity", 12.239238m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_CustomsQuantity = 13.129876m;
			AssertEquals("JI_CustomsQuantity", 13.129876m, invoiceLine.JI_CustomsQuantity);
		}
	}

	public void TestJI_CustomsQuantityInImportDeclarationWhenTransitionPeriodIsON()
	{
		using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "IsUCC6Core", true, null))
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_CustomsQuantity = 12.239238m;
			AssertEquals("JI_CustomsQuantity", 12.239238m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_CustomsQuantity = 13.129876m;
			AssertEquals("JI_CustomsQuantity", 13.129876m, invoiceLine.JI_CustomsQuantity);
		}
	}

	public void TestJI_CustomsThirdQuantity_ResourceData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CustomsThirdQuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "[44] Third Qty (10YY)", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Third Qty (10YY)", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "Third Qty", resourceStringData.ShortCaption);
		});
	}

	public void TestJI_CustomsThirdUnitQty()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CustomsThirdUnitQtyInfo);
		CombineAssertions("JI_CustomsThirdUnitQty", () =>
		{
			AssertEquals("Caption", "Third Quantity Unit", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Third Qty Unit", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "UQ", resourceStringData.ShortCaption);
		});
	}

	public void TestJI_CustomsFourthQuantity_ResourceData()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_CustomsFourthQuantityInfo);

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Fourth Qty", resourceStringData.Caption);
			AssertEquals("FullDescription", "Fourth Quantity", resourceStringData.FullDescription);
		});
	}

	public void TestCusSupplyChainActorReferences()
	{
		var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nameof(jobComInvoiceLine.CusSupplyChainActorReferences), jobComInvoiceLine.CusSupplyChainActorReferences);
	}

	public override void TestWipeNKTaxType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var procedureNotRequiringVat = helper.CreateRefCusProcedure("IT", "IM", "40", "00", "", "4000 desc", "IMP");
		procedureNotRequiringVat.ZZ6_CalculateVAT = false;
		var procedureRequiringVat = helper.CreateRefCusProcedure("IT", "IM", "99", "00", "", "9900 desc", "IMP");
		procedureRequiringVat.ZZ6_CalculateVAT = true;
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		CombineAssertions("When the procedure does not require the VAT", () =>
		{
			invoiceLine.JI_ZZF_NKTaxType = "ORD";
			invoiceLine.JI_Procedure = "4000";
			AssertEquals("ShouldWipeNKTaxType", true, invoiceLine.ShouldWipeNKTaxType);
			AssertEquals("JI_ZZF_NKTaxType", "", invoiceLine.JI_ZZF_NKTaxType);
		});

		CombineAssertions("When the procedure is not a valid one", () =>
		{
			invoiceLine.JI_ZZF_NKTaxType = "ORD";
			invoiceLine.JI_Procedure = "1000";
			AssertEquals("ShouldWipeNKTaxType", false, invoiceLine.ShouldWipeNKTaxType);
			AssertEquals("JI_ZZF_NKTaxType", "ORD", invoiceLine.JI_ZZF_NKTaxType);
		});

		CombineAssertions("When the procedure requires the VAT", () =>
		{
			invoiceLine.JI_ZZF_NKTaxType = "ORD";
			invoiceLine.JI_Procedure = "9900";
			AssertEquals("ShouldWipeNKTaxType", false, invoiceLine.ShouldWipeNKTaxType);
			AssertEquals("JI_ZZF_NKTaxType", "ORD", invoiceLine.JI_ZZF_NKTaxType);
		});
	}

	public void TestBuyerJobDocAddressAdditionalValidation()
	{
		var orphanInvoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNull("When declaration is null for invoice line, additional validation for buyer is null", orphanInvoiceLine.BuyerDocAddress.AdditionalValidation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<InvoiceLineTraderJobDocAddressValidation>("When declaration is IMP, InvoiceLineTraderJobDocAddressValidation expected", invoiceLine1.BuyerDocAddress.AdditionalValidation);

		declaration.JE_MessageType = "EXP";
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertNull("When declaration is EXP, null expected", invoiceLine2.BuyerDocAddress.AdditionalValidation);
	}

	public void TestSellerJobDocAddressAdditionalValidation()
	{
		var orphanInvoiceLine = Factory.New<JobComInvoiceLine>();
		AssertNull("When declaration is null for invoice line, additional validation for seller is null", orphanInvoiceLine.SellerDocAddress.AdditionalValidation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<InvoiceLineTraderJobDocAddressValidation>("When declaration is IMP, InvoiceLineTraderJobDocAddressValidation expected", invoiceLine1.SellerDocAddress.AdditionalValidation);

		declaration.JE_MessageType = "EXP";
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertNull("When declaration is EXP, null expected", invoiceLine2.SellerDocAddress.AdditionalValidation);
	}

	public void TestIsNonTurkishImportWithTurkishDispatch()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		CombineAssertions("IsNonTurkishImportWithTurkishDispatch", () =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_GoodsOrigin = "TR";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			AssertEquals("When MessageType=IMP, GoodsOrigin=TR, CountryOfOrigin=ZA",
				expected: true,
				invoiceLine.IsNonTurkishImportWithTurkishDispatch);

			declaration.JE_MessageType = "EXP";
			AssertEquals("When MessageType=EXP (Not IMP)",
				expected: false,
				invoiceLine.IsNonTurkishImportWithTurkishDispatch);

			declaration.JE_MessageType = "IMP";
			declaration.JE_GoodsOrigin = "AU";
			AssertEquals("When MessageType=IMP, GoodsOrigin=AU (Not TR)",
				expected: false,
				invoiceLine.IsNonTurkishImportWithTurkishDispatch);

			declaration.JE_GoodsOrigin = "TR";
			invoiceLine.JI_CountryOfOrigin = "TR";
			AssertEquals("When MessageType=IMP, GoodsOrigin=TR, CountryOfOrigin=TR",
				expected: false,
				invoiceLine.IsNonTurkishImportWithTurkishDispatch);
		});
	}

	public void TestIsEligibleForTurkeyCustomsDutyExempt()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();

		CombineAssertions("IsEligibleForTurkeyCustomsDutyExempt", () =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_GoodsOrigin = "TR";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			invoiceLine.JI_PrimaryPreference = "400";

			AssertEquals("When IsNonTurkishImportJobWithTurkishDispatch, PrimaryPreference is 400",
				expected: true,
				invoiceLine.IsEligibleForTurkeyCustomsDutyExempt);

			invoiceLine.JI_PrimaryPreference = "100";
			AssertEquals("When PrimaryPreference is not 400",
				expected: false,
				invoiceLine.IsEligibleForTurkeyCustomsDutyExempt);

			invoiceLine.JI_PrimaryPreference = "400";
			declaration.JE_GoodsOrigin = "AU";
			AssertEquals("When IsNonTurkishImportJobWithTurkishDispatch=false",
				expected: false,
				invoiceLine.IsEligibleForTurkeyCustomsDutyExempt);
		});
	}

	public override void TestGetNewLinkPackValidation()
	{
		AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
	}

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
	{
		var customsChargeTypeList = new UCCCustomsChargeTypeList();
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AdjustmentCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
		customsChargeTypeList.RemoveCode(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
		customsChargeTypeList.AddPair(ChargeTypeList.Codes.StatisticalValue, ChargeTypeList.Descriptions.StatisticalValue);
		customsChargeTypeList.Sort();
		return customsChargeTypeList;
	}

	protected override Type GetExpectedCusEntryLineType() => typeof(CusEntryLine);

	protected override Type GetExpectedPartType() => typeof(OrgSupplierPart);

	protected override string GetLocalPortCode() => "ITMIL";

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

	protected override Type ExpectedAdditionalProcedureCode => typeof(AdditionalProcedureCode);

	protected override Type GetExpectedEntryInstructionType() => typeof(CusEntryInstruction);

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	new JobComInvoiceLine InvoiceLine => base.InvoiceLine;

	void SetUpPackageTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
	}
}

class JobComInvoiceLineForTest : JobComInvoiceLine
{
	public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public bool SetSecondQuantityFromEdiTariffsOwnRecordExposed => SetSecondQuantityFromEdiTariffsOwnRecord;

	public ICustomsValuationCalculator GetValuationCalculatorCoreExposed() => GetValuationCalculatorCore();
}

sealed class AdditionalSupplementaryCodeSetterTest : TestCaseWithFactory
{
	public void TestDefaultAdditionalSupplementaryCodeEnteringVat()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFeeWithRelatedVatApplicability("IMP", "99999999", ("ORD", 21m, "Q001"), ("MIN", 4m, ""));

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "99999999";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_SupplementaryCode1 = "";

		CombineAssertions("[PRE-CONDITION] Check Additional Supplementary Codes are empty and clear", () =>
		{
			AssertEquals("JI_SupplementaryCode1", "", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("JI_SupplementaryCode2", "", invoiceLine.JI_SupplementaryCode2);
			AssertEquals("AdditionalSupplementaryCodes count", 0, invoiceLine.AdditionalSupplementaryCodes.Count);
		});

		TestSupplementaryCodeFillFirstEmptyPosition(invoiceLine);
		TestSupplementaryCodeOverrideFirstQCode(invoiceLine);
		TestSupplementaryCodeEmptyFirstQCode(invoiceLine);

		TestSupplementaryCodeAddedOverLimit(invoiceLine);
	}

	void TestSupplementaryCodeFillFirstEmptyPosition(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.JI_ZZF_NKTaxType = "ORD";
		AssertEquals("JI_SupplementaryCode1", "Q001", invoiceLine.JI_SupplementaryCode1);

		invoiceLine.JI_SupplementaryCode1 = "C001";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";
		CombineAssertions("Check Additional Supplementary Codes", () =>
		{
			AssertEquals("JI_SupplementaryCode1", "C001", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("JI_SupplementaryCode2", "Q001", invoiceLine.JI_SupplementaryCode2);
		});

		invoiceLine.JI_SupplementaryCode2 = "C002";
		var thirdSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		thirdSupplementaryCode.CY_Code = "C003";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		CombineAssertions("Check Additional Code is automatically set in first available position [4th AdditionalSupplementaryCodes]", () =>
		{
			AssertEquals("JI_SupplementaryCode1", "C001", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("JI_SupplementaryCode2", "C002", invoiceLine.JI_SupplementaryCode2);
			AssertEquals("AdditionalSupplementaryCodes count", 2, invoiceLine.AdditionalSupplementaryCodes.Count);
		});
		AssertEquals("Q Additional Code in fourth position", "Q001", invoiceLine.AdditionalSupplementaryCodes[1].CY_Code);
	}

	void TestSupplementaryCodeOverrideFirstQCode(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine.JI_SupplementaryCode2 = "Q099";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		AssertEquals("Q Additional Code should override first Q Code previous entered, JI_SupplementaryCode2", "Q001", invoiceLine.JI_SupplementaryCode2);

		invoiceLine.JI_SupplementaryCode2 = "C002";
		var thirdSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		thirdSupplementaryCode.CY_Code = "Q009";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		AssertEquals("AdditionalSupplementaryCodes count", 1, invoiceLine.AdditionalSupplementaryCodes.Count);
		AssertEquals("Additional Code should override the Q Code previous entered, AdditionalSupplementaryCodes[0]", "Q001", invoiceLine.AdditionalSupplementaryCodes[0].CY_Code);

		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine.JI_SupplementaryCode1 = "";
		invoiceLine.JI_SupplementaryCode2 = "Q999";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";
		CombineAssertions("Check Additional Code is automatically set in first available position [JI_SupplementaryCode2]", () =>
		{
			AssertEquals("JI_SupplementaryCode1", "", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("JI_SupplementaryCode2", "Q001", invoiceLine.JI_SupplementaryCode2);
		});
	}

	void TestSupplementaryCodeEmptyFirstQCode(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine.JI_SupplementaryCode1 = "S001";
		invoiceLine.JI_SupplementaryCode2 = "Q099";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "MIN";

		CombineAssertions("Selecting a VAT with no Q code related, the first Q Code should be Empty", () =>
		{
			AssertEquals("Supplementary Code 1", "S001", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("Supplementary Code 2", "", invoiceLine.JI_SupplementaryCode2);
			AssertEquals("JI_ZZF_NKTaxType", "MIN", invoiceLine.JI_ZZF_NKTaxType);
		});
	}

	void TestSupplementaryCodeAddedOverLimit(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		var maxAdditionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.MaxCount;
		for (int i = 0; i < maxAdditionalSupplementaryCode; i++)
		{
			var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = $"C{i + 3:#00}";
		}
		AssertEquals("AdditionalSupplementaryCodes should not allow to add new codes manually", false, invoiceLine.AdditionalSupplementaryCodes.AllowNew);
		invoiceLine.JI_SupplementaryCode1 = "C001";
		invoiceLine.JI_SupplementaryCode2 = "C002";
		invoiceLine.JI_ZZF_NKTaxType = "";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";
		AssertEquals("AdditionalSupplementaryCodes count", maxAdditionalSupplementaryCode + 1, invoiceLine.AdditionalSupplementaryCodes.Count);
		AssertEquals("99th AdditionalSupplementaryCode", "Q001", invoiceLine.AdditionalSupplementaryCodes[maxAdditionalSupplementaryCode].CY_Code);
	}

	public void TestGetNationalRateSelectionCriteria()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("IT");
		helper.CreateNewOrGetExistingDataGrouping("FR");
		helper.CreateCusRateType("IT", "EXC");
		helper.CreateCusRateType("IT", "LEV");
		helper.CreateCusRateType("IT", "MOE");
		helper.CreateCusRateType("FR", "MOE");

		Factory.Save();

		var invoiceLine = Factory.New<JobComInvoiceLineForTest>();

		var nationalRateSelectionCriteraCollection = invoiceLine.GetNationalRateSelectionCriteriaExposed();
		AssertEquals("GetNationalRateSelectionCriteria count", 3, nationalRateSelectionCriteraCollection.Count());
		AssertArrayEqualsByElements("NationalRateSelectionCriteria", new ZString[] { "EXC", "LEV", "MOE" }, nationalRateSelectionCriteraCollection.Select(x => x.RateType).ToArray());
	}

	public void TestSetDefaultTaxOrFeeCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		var countryCode = CountryCodes.Italy;
		helper.CreateTaxOrFee("IVA", 0.21, countryCode);
		var expTariffType = helper.CreateTariffType(countryCode, "EXP");
		var impTariffType = helper.CreateTariffType(countryCode, "IMP");

		var tariffExp = helper.LoadOrCreateNewTariff(countryCode, expTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		var tariffImp = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "22223333", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingVATApplicability(tariffExp, countryCode, "IVA");
		helper.CreateNewOrGetExistingVATApplicability(tariffImp, countryCode, "IVA");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "11112222";

		AssertEquals(nameof(invoiceLine.JI_ZZF_NKTaxType), "", invoiceLine.JI_ZZF_NKTaxType);

		declaration.JE_MessageType = "IMP";
		invoiceLine.JI_Tariff = "22223333";

		AssertEquals(nameof(invoiceLine.JI_ZZF_NKTaxType), "IVA", invoiceLine.JI_ZZF_NKTaxType);
	}

	public void TestEnableOrDisableAdditionalInfosMaxCountValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		AssertAdditionalInfoLinesCount("EXP", invoiceLine, 1, "Only 1 line of Additional Info is allowed.");
		AssertAdditionalInfoLinesCount("IMP", invoiceLine, 99, "Only 99 lines of Additional Info are allowed.");

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			AssertEquals("Max AdditionalInfo Count, UCC6 + EXP", -1, invoiceLine.AdditionalInfos.MaxCount);
		}

		void AssertAdditionalInfoLinesCount(string messageType, JobComInvoiceLine invLine, int linesMaxCount, string expectedErrorMessage)
		{
			declaration.JE_MessageType = messageType;
			for (int i = 0; i < linesMaxCount; i++)
			{
				var additionalInfo = invLine.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = "ABC" + i;
				AssertNoRowErrorContaining(additionalInfo, expectedErrorMessage);
			}
			AssertEquals($"For {messageType}, Number of Additional Info lines before error", linesMaxCount, invLine.AdditionalInfos.Count);

			var additionalInfo1 = invLine.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "XYZ";
			AssertHasRowErrorContaining(additionalInfo1, expectedErrorMessage);

			AssertEquals($"For {messageType}, Number of Additional Info lines after error", linesMaxCount + 1, invLine.AdditionalInfos.Count);
			invLine.AdditionalInfos.RemoveAndDeleteAll();
		}
	}

	public void TestCIFValueCalculationForInterfacedDeclaration()
	{
		var usDollar = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
		AddExchangeRate(new ZDateTime(2022, 01, 01), 1.1345m);
		AddExchangeRate(ZDateTime.Today, 1.083m);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "ITF";
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_LinePrice = 882.41m;
		AddCharge("OFT", 70.91m);
		AddCharge("ONS", 0.7m);
		AddCharge("ADD", 8.21m);

		CombineAssertions("When the invoice lines is not part of a registered entry, 'Today' exchange rate is used", () =>
		{
			AssertEquals("Exchange Rate", 1.083m, invoiceLine.CurrencyConverter.GetExchangeRate(usDollar));
			AssertEquals("JI_CustomsValue", 888.48m, invoiceLine.JI_CustomsValue);
			AssertEquals("JI_Calc_CIF", 962.22m, invoiceLine.JI_Calc_CIF);
		});

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.NewCusEntryNumber(entryHeader, "IMP", "4-12345", new ZDateTime(2022, 01, 03));
		invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;

		CombineAssertions("When the invoice lines is part of a registered entry, the closest exchange rate to the entry registration date", () =>
		{
			AssertEquals("Exchange Rate", 1.1345m, invoiceLine.CurrencyConverter.GetExchangeRate(usDollar));
			AssertEquals("JI_CustomsValue", 848.16m, invoiceLine.JI_CustomsValue);
			AssertEquals("JI_Calc_CIF", 962.24m, invoiceLine.JI_Calc_CIF);
		});

		void AddExchangeRate(ZDateTime startDate, ZDecimal sellRate)
		{
			var exchangeRate = usDollar.ExchangeRates.AddNew();
			exchangeRate.RE_StartDate = startDate;
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_SellRate = sellRate;
		}

		void AddCharge(ZString chargeType, ZDecimal chargeAmount)
		{
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = chargeType;
			charge.J7_Amount = chargeAmount;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_IsDutiable = charge.J7_IsStatisticalValueApplicable = charge.J7_IsGSTApplicable = true;
		}
	}

	public void TestCusAuthorizationUsages()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>("CusAuthorizationUsages Type", invoiceLine.CusAuthorizationUsages);
	}

	public void TestAdditionalProcedureCodesType()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<AdditionalProcedureCodeCollection>("AdditionalProcedureCodes", invoiceLine.AdditionalProcedureCodes);
	}

	class JobComInvoiceLineForTest : JobComInvoiceLine
	{
		public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCoreExposed() => base.GetAllApplicableRatesSelectionCriteriaCore();

		public IEnumerable<IZZRateSelectionCriteria> GetNationalRateSelectionCriteriaExposed() => GetNationalRateSelectionCriteriaCore();

		public IZZConditionSelectionCriteria[] GetConditionSelectionCriteriasExposed() => base.GetConditionSelectionCriterias();
	}
}
