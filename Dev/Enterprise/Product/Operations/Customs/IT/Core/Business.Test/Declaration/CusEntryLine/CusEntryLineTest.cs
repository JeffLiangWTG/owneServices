using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Resources;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	public void TestValidationType()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertType<CusEntryLineValidation>(entryLine.Validation);
	}

	public void TestDutyDetailsIncludingMethodOfPaymentR()
	{
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
		fee1.CF_ChargeAmount = 300m;

		CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
		{
			AssertEquals("Agricultural Component (EA) fee", 300m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
			AssertEquals("DutyDetails", 300m, entryLine.DutyDetails);
		});

		var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
		fee2.CF_ChargeAmount = 100m;

		CombineAssertions("EntryLine with 3 DTY type fees: A00, EA, A35", () =>
		{
			AssertEquals("A35 fee", 100m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty));
			AssertEquals("DutyDetails", 400m, entryLine.DutyDetails);
		});

		var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty);
		fee3.CF_ChargeAmount = 70m;

		CombineAssertions("EntryLine with 4 fees: A00, EA, A35, A45", () =>
		{
			AssertEquals("Provisional Countervailing Duty (A45) fee", 70m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty));
			AssertEquals("DutyDetails", 470m, entryLine.DutyDetails);
		});
	}

	public void TestDutyDetailsForVATNotIncludingMethodOfPaymentR()
	{
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
		CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
		fee1.CF_ChargeAmount = 300m;

		CombineAssertions("EntryLine with 2 DTY type fees: A00, EA.", () =>
		{
			AssertEquals("Agricultural Component (EA) fee", 300m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent));
			AssertEquals("DutyDetails", 300m, entryLine.DutyDetailsForVAT);
		});

		var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents);
		fee2.CF_ChargeAmount = 100m;

		CombineAssertions("EntryLine with 3 DTY type fees: A00, EA, ADSZ", () =>
		{
			AssertEquals("Additional duty on sugar contents", 100m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents));
			AssertEquals("DutyDetails", 400m, entryLine.DutyDetailsForVAT);
		});

		var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
		fee3.CF_ChargeAmount = 70m;

		CombineAssertions("EntryLine with 4 fees: A00, EA, ADSZ, A35", () =>
		{
			AssertEquals("Additional duty on flour contents", 70m, entryLine.Fees.GetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty));
			AssertEquals("DutyDetails", 400m, entryLine.DutyDetailsForVAT);
		});
	}

	public void TestNumberOfPackages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = declaration.Packages.AddNew();
		var entryLine1 = Factory.New<CusEntryLine>();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		package1.CW_PackQty = 100;
		package1.CW_PackType = "VG";
		var package2 = declaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = billPackingGroup.PK;
		package2.CW_PackQty = 200;
		package2.CW_PackType = "VG";
		var package3 = declaration.Packages.AddNew();
		package3.CW_CR_HouseContainer = billPackingGroup.PK;
		package3.CW_PackQty = 15;
		package3.CW_PackType = "VG";

		var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
		var packageInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;
		packageInvoiceLine1.CHC_NumberOfPacks = 99;

		var invoiceLine2 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
		var packageInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package3.PK;
		packageInvoiceLine2.CHC_NumberOfPacks = 15;
		AssertEquals("Number Of Packs should be", 114, entryLine1.NumberOfPackages);

		packageInvoiceLine1.CHC_NumberOfPacks = 1;
		packageInvoiceLine2.CHC_NumberOfPacks = 1;
		AssertEquals("Number Of Packs should be", 2, entryLine1.NumberOfPackages);

		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.NumberOfPackages)
			.WithCaption("Packages");
	}

	public void TestPackageType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package = declaration.Packages.AddNew();
		package.CW_CR_HouseContainer = billPackingGroup.PK;
		package.CW_PackType = "1A";

		var entryLine1 = Factory.New<CusEntryLine>();
		var invoiceLine1 = (JobComInvoiceLine)entryLine1.InvoiceLines.AddNew();
		var packageInvoiceLine = invoiceLine1.PackagesPivot.AddNew();
		packageInvoiceLine.CHC_CW = package.PK;
		AssertEquals("PackageType should be", "1A", entryLine1.PackageType);
		package.CW_PackType = "XX";
		AssertEquals("PackageType should be", "XX", entryLine1.PackageType);
	}

	public void TestMarksAndNumbers()
	{
		(var entryLine, var package1, var package2, var packingGroup, var declaration, _, var invoiceLine2) = SetupEntryLineWithPackages();
		AssertEquals("", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "";
		package2.CW_MarksAndNos = "";
		AssertEquals("", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 1";
		AssertEquals("MARK AND NOS 1", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 2";
		AssertEquals("MARK AND NOS 1;MARK AND NOS 2", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "THIS IS 20 CHAR LENG";
		package2.CW_MarksAndNos = "THIS IS 21 CHAR LENGX";
		AssertEquals("THIS IS 20 CHAR LENG;THIS IS 21 CHAR LENGX", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "MARK AND NOS PACKAGE 1";
		package2.CW_MarksAndNos = "MARK AND NOS PACKAGE 2";
		AssertEquals("MARK AND NOS PACKAGE 1... see notes", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "THIS IS 53 STRING LENGTH THERE ARE NO BLOCK TO REMOVE";
		package2.CW_MarksAndNos = "";
		AssertEquals("... see notes", entryLine.MarksAndNumbers);

		package1.CW_MarksAndNos = "THIS IS 30 CHAR LENGTH STRINGG";
		package2.CW_MarksAndNos = "THIS IS 29 CHAR LENGTH STRING";
		AssertEquals("... see notes", entryLine.MarksAndNumbers);

		var package3 = declaration.Packages.AddNew();
		package3.CW_CR_HouseContainer = packingGroup.PK;
		var package3InvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		package3InvoiceLine2.CHC_CW = package3.PK;

		var package4 = declaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = packingGroup.PK;
		var package4InvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		package4InvoiceLine2.CHC_CW = package4.PK;

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 2";
		package3.CW_MarksAndNos = "MARK AND NOS 3";
		package4.CW_MarksAndNos = "MARK AND NOS 4";
		AssertEquals("MARK AND NOS 1;MARK AND NOS 2... see notes", entryLine.MarksAndNumbers);
	}

	public void TestNotesWithMarksAndNumbers()
	{
		(var entryLine, var package1, var package2, var packingGroup, var declaration, _, var invoiceLine2) = SetupEntryLineWithPackages();
		AssertEquals("", entryLine.LineNotes);

		package1.CW_MarksAndNos = "";
		package2.CW_MarksAndNos = "";
		AssertEquals("", entryLine.LineNotes);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 1";
		AssertEquals("", entryLine.LineNotes);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 2";
		AssertEquals("", entryLine.LineNotes);

		package1.CW_MarksAndNos = "THIS IS 20 CHAR LENG";
		package2.CW_MarksAndNos = "THIS IS 21 CHAR LENGX";
		AssertEquals("", entryLine.LineNotes);

		package1.CW_MarksAndNos = "MARK AND NOS PACKAGE 1";
		package2.CW_MarksAndNos = "MARK AND NOS PACKAGE 2";
		AssertEquals("Other Marks: MARK AND NOS PACKAGE 2", entryLine.LineNotes);

		package1.CW_MarksAndNos = "THIS IS 53 STRING LENGTH THERE ARE NO BLOCK TO REMOVE";
		package2.CW_MarksAndNos = "";
		AssertEquals("Other Marks: THIS IS 53 STRING LENGTH THERE ARE NO BLOCK TO REMOVE", entryLine.LineNotes);

		package1.CW_MarksAndNos = "THIS IS 30 CHAR LENGTH STRINGG";
		package2.CW_MarksAndNos = "THIS IS 29 CHAR LENGTH STRING";
		AssertEquals("Other Marks: THIS IS 30 CHAR LENGTH STRINGG;THIS IS 29 CHAR LENGTH STRING", entryLine.LineNotes);
	}

	public void TestNotesWithAdditionalInfoOrderedByLineNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		AssertEquals("", entryLine.LineNotes);

		invoiceLine1.JI_LineNo = 1;
		invoiceLine2.JI_LineNo = 2;
		invoiceLine1.Remarks = "this is a description";
		invoiceLine2.Remarks = "this is another description";
		AssertEquals("this is a description|this is another description", entryLine.LineNotes);

		invoiceLine1.JI_LineNo = 2;
		invoiceLine2.JI_LineNo = 1;
		AssertEquals("this is another description|this is a description", entryLine.LineNotes);
	}

	public void TestNotesWithAdditionalInfoDistinct()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_LineNo = 1;
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_LineNo = 2;
		invoiceLine2.JI_CL = entryLine.PK;
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_LineNo = 3;
		invoiceLine3.JI_CL = entryLine.PK;
		AssertEquals("", entryLine.LineNotes);

		invoiceLine1.Remarks = "this is a description";
		invoiceLine2.Remarks = "this is another description";
		invoiceLine3.Remarks = "this is a description";
		AssertEquals("this is a description|this is another description", entryLine.LineNotes);

		invoiceLine2.Remarks = "";
		AssertEquals("this is a description", entryLine.LineNotes);
	}

	public void TestNotesWithMarksAndNumbersAndAdditionalInfo()
	{
		(var entryLine, var package1, var package2, _, _, var invoiceLine1, var invoiceLine2) = SetupEntryLineWithPackages();
		AssertEquals("", entryLine.LineNotes);

		invoiceLine1.JI_LineNo = 1;
		invoiceLine2.JI_LineNo = 2;
		invoiceLine1.Remarks = "this is a description";
		invoiceLine2.Remarks = "this is another description";
		AssertEquals("this is a description|this is another description", entryLine.LineNotes);

		package1.CW_MarksAndNos = "MARK AND NOS PACKAGE 1";
		package2.CW_MarksAndNos = "MARK AND NOS PACKAGE 2";
		AssertEquals("Other Marks: MARK AND NOS PACKAGE 2.this is a description|this is another description", entryLine.LineNotes);
	}

	public void TestSteelType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine1.ZG_SteelType = "X";
		AssertEquals("X", entryLine.SteelType);
	}

	public void TestAdditionalCodes()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_SupplementaryCode1 = "XXX";
		invoiceLine1.JI_SupplementaryCode2 = "YYY";
		var additionalCode1 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode1.CY_Code = "S001";
		var additionalCode2 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode2.CY_Code = "S002";
		var additionalCode3 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode3.CY_Code = "S003";
		var additionalCode4 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode4.CY_Code = "S004";
		var additionalCode5 = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCode5.CY_Code = "S005";
		var additionalCodeEmpty = invoiceLine1.AdditionalSupplementaryCodes.AddNew();
		additionalCodeEmpty.CY_Code = "";

		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_SupplementaryCode1 = "XXX";
		invoiceLine2.JI_SupplementaryCode2 = "YYY";
		var additionalCode6 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode6.CY_Code = "S001";
		var additionalCode7 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode7.CY_Code = "S002";
		var additionalCode8 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode8.CY_Code = "S003";
		var additionalCode9 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode9.CY_Code = "S004";
		var additionalCode10 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
		additionalCode10.CY_Code = "S005";

		AssertArrayEqualsByElements(new ZString[] { "S001", "S002", "S003", "S004", "S005", "XXX", "YYY" }, entryLine.AdditionalCodes.ToArray());
	}

	public void TestNationalProcedures()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();

		invoiceLine1.JI_Procedure = "4071C33";
		AssertEquals("National Procedure should be", new ZString("C33"), entryLine.NationalProcedureCode);

		invoiceLine1.JI_Procedure = "4071C3";
		AssertEquals("National Procedure should be", "C3", entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "4000  A";
		AssertEquals("National Procedure should be", "A", entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "4000 AB";
		AssertEquals("National Procedure should be", "AB", entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "4000AB ";
		AssertEquals("National Procedure should be", "AB", entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "4071C33456";
		AssertEquals("National Procedure should be", "C33", entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "4071";
		AssertEquals("National Procedure should be", string.Empty, entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "407";
		AssertEquals("National Procedure should be", string.Empty, entryLine.NationalProcedureCode.ToString());

		invoiceLine1.JI_Procedure = "";
		AssertEquals("National Procedure should be", string.Empty, entryLine.NationalProcedureCode.ToString());
	}

	public void TestSupplementaryUnit()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_CustomsSecondQuantity = 200m;
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_CustomsSecondQuantity = 500m;
		AssertEquals(700m, entryLine.SupplementaryUnit);

		invoiceLine1.JI_CustomsSecondQuantity = 0m;
		AssertEquals(500m, entryLine.SupplementaryUnit);

		invoiceLine2.JI_CustomsSecondQuantity = 0m;
		AssertNull(entryLine.SupplementaryUnit);
	}

	public void TestEffectiveDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "40";
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_IncoTerm = "FOB";
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_Tariff = "1806905000";
		invoiceLine1.JI_Description = "";
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_Tariff = "1806905000";
		invoiceLine2.JI_Description = "Added Sugar";
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction.PK;
		invoiceLine3.JI_Tariff = "1806905000";
		invoiceLine3.JI_Description = "Sweet Sugar";
		declaration.DoMerge();

		AssertEquals("Declaration should have one EntryHeader", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("EntryHeader should have one EntryLine", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines[0];
		AssertEquals("EntryHeader.EffectiveDescription should be", "Added Sugar", entryLine.EffectiveDescription);

		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
		declaration.DoMerge();

		AssertEquals("Declaration should have one EntryHeader", 1, declaration.CustomsEntryHeaders.Count);
		entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("EntryHeader should have one EntryLine", 3, entryHeader.MergedLines.Count);
		var orderedMergegLines = entryHeader.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.EffectiveDescription).ToArray();
		CombineAssertions("Entry Lines effective description should be:", () =>
		{
			AssertEquals("Effective Description 0 should be", "", orderedMergegLines[0].EffectiveDescription);
			AssertEquals("Effective Description 1 should be", "Added Sugar", orderedMergegLines[1].EffectiveDescription);
			AssertEquals("Effective Description 2 should be", "Sweet Sugar", orderedMergegLines[2].EffectiveDescription);
		});
	}

	public void TestEffectiveGrossWeightIsApplicable()
	{
		var orphanEntryLine = Factory.New<CusEntryLine>();
		Assert("Effective Gross Weight Is Applicable should be true also when entry line is not linked to a declaration", orphanEntryLine.EffectiveGrossWeightIsApplicable);

		var declaration = Factory.New<JobDeclaration>();
		var entryLineLinkedToDeclaration = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		Assert("Effective Gross Weight Is Applicable should be true also when entry line is linked to a declaration", entryLineLinkedToDeclaration.EffectiveGrossWeightIsApplicable);
	}

	public void TestPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_Weight = 100m;
		invoiceLine1.JI_NetWeight = 200m;
		invoiceLine1.JI_CL = entryLine1.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Weight = 200m;
		invoiceLine2.JI_NetWeight = 400m;
		invoiceLine2.JI_CL = entryLine1.PK;

		var declarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 100m, 100m, 100m, 100);
		declaration.PreviousDocuments.Add(declarationPreviousDocumentProcedure2);
		var anotherDeclarationPreviousDocumentProcedure2 = PreviousDocumentTestHelper.CreateDocument(Factory, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 75m, 75m, 75m, 75);
		declaration.PreviousDocuments.Add(anotherDeclarationPreviousDocumentProcedure2);
		var invoiceLinePreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);
		invoiceLine2.PreviousDocuments.Add(invoiceLinePreviousDocumentProcedureA3);

		var mergedPreviousDocuments = entryLine1.MergedPreviousDocuments.ToArray();
		AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Length);
		var previousDocumentWithProcedure2 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "2");
		CheckDocument(previousDocumentWithProcedure2, "CO", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 1, "2", "", "", "", 175m, 175, 175, 175);
		var previousDocumentWithProcedureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3");
		CheckDocument(previousDocumentWithProcedureA3, "ZZZ", new ZDate(2020, 01, 01), "1", "Z", "", "IT137100", 0, "A3", "", "", "", 100m, 100m, 100m, 100);

		var invoiceLine1PreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedureA3);
		mergedPreviousDocuments = entryLine1.MergedPreviousDocuments.ToArray();
		AssertEquals("Previous Documents count", 2, mergedPreviousDocuments.Length);
		declaration.DoMerge();
		mergedPreviousDocuments = entryLine1.MergedPreviousDocuments.ToArray();
		AssertEquals("Previous Documents count", 3, mergedPreviousDocuments.Length);
		var newInvoiceLinePreviousDocumentWithProcdureA3 = mergedPreviousDocuments.SingleOrDefault(x => x.CSI_Procedure == "A3" && x.CSI_ReferenceNumber == "999");
		CheckDocument(newInvoiceLinePreviousDocumentWithProcdureA3, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 12m, 12m, 12m, 12);
	}

	public void TestGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();

		AssertNotNull("Grouped Previous Documents", entryLine.GroupedPreviousDocuments);

		var invoiceLine1PreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedureA3);
		declaration.DoMerge();
		AssertEquals("Grouped Previous Documents count", 0, entryLine.GroupedPreviousDocuments.Count);

		var invoiceLine1PreviousDocumentProcedure2S = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "MRN", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedure2S);
		declaration.DoMerge();
		AssertEquals("Grouped Previous Documents count", 2, entryLine.GroupedPreviousDocuments.Count);
	}

	public void TestResetGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;

		AssertEquals("Grouped Previous Documents count", 0, entryLine.GroupedPreviousDocuments.Count);

		var invoiceLine1PreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedureA3);
		var invoiceLine1PreviousDocumentProcedure2S = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "MRN", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedure2S);

		AssertEquals("Grouped Previous Documents count", 0, entryLine.GroupedPreviousDocuments.Count);
		declaration.ResetApportionedPreviousDocuments();
		AssertEquals("Grouped Previous Documents count", 2, entryLine.GroupedPreviousDocuments.Count);
	}

	public void TestSadAttachmentPrintingSupporter()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertNotNull(entryLine.AttachmentPrintingSupporter);
		AssertType<CusEntryLineSadAttachmentPrintingSupporter>(entryLine.AttachmentPrintingSupporter);
	}

	public void TestEadAttachmentPrintingSupporter()
	{
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.UseEadAttachmentPrintingSupporter = true;
		AssertNotNull(entryLine.AttachmentPrintingSupporter);
		AssertType<CusEntryLineEadAttachmentPrintingSupporter>(entryLine.AttachmentPrintingSupporter);
	}

	public void TestUseEadAttachmentPrintingSupporter()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertNotNull(entryLine.AttachmentPrintingSupporter);
		AssertType<CusEntryLineSadAttachmentPrintingSupporter>(entryLine.AttachmentPrintingSupporter);

		entryLine.UseEadAttachmentPrintingSupporter = true;
		AssertNotNull(entryLine.AttachmentPrintingSupporter);
		AssertType<CusEntryLineEadAttachmentPrintingSupporter>(entryLine.AttachmentPrintingSupporter);
	}

	public void TestRemarks()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine.PK;

		invoiceLine1.Remarks = "Invoice Line1\r\nsecond row";
		invoiceLine2.Remarks = "\r\n";
		invoiceLine3.Remarks = "Invoice Line 2\r\n";

		var expectedRemarks = new ZString[]
		{
			"Invoice Line1 second row",
			"Invoice Line 2",
		};

		AssertArrayEqualsByElements("Remarks", expectedRemarks, entryLine.Remarks.ToArray());
	}

	public void TestAdditionalInfos()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var additionalInfo1 = AddAdditionalInfo(invoice.AdditionalInfos, "INF", "00100", "", "INV INF");
		var additionalInfo2 = AddAdditionalInfo(invoice.AdditionalInfos, "REF", "Y001", "INV REF", "");
		var additionalInfo3 = AddAdditionalInfo(invoice.AdditionalInfos, "TRA", "C613", "INV TRA", "");

		var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var additionalInfo4 = AddAdditionalInfo(invoiceLine1.AdditionalInfos, "INF", "00100", "", "INV LINE INF");
		var additionalInfo5 = AddAdditionalInfo(invoiceLine1.AdditionalInfos, "REF", "Y001", "INV LINE REF", "");

		var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		var additionalInfo6 = AddAdditionalInfo(invoiceLine2.AdditionalInfos, "TRA", "C613", "INV LINE TRA", "");

		AssertContainsExactElementsInAnyOrder(
			"AdditionalInfos contains elements from the Invoice and from the 2 Invoice Lines distinct by Code, Reference and Description",
			new object[] { additionalInfo1, additionalInfo2, additionalInfo3, additionalInfo4, additionalInfo5, additionalInfo6 },
			entryLine.AdditionalInfos);

		EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo AddAdditionalInfo(
			EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection documentCollection,
			string subType,
			string code,
			string referenceNumber,
			string description)
		{
			var additionalDocument = documentCollection.AddNew();
			additionalDocument.CSI_SubType = subType;
			additionalDocument.CSI_Code = code;
			additionalDocument.CSI_ReferenceNumber = referenceNumber;
			additionalDocument.CSI_Description = description;
			return additionalDocument;
		}
	}

	public void TestGetTaxBoxSupporterListOrder()
	{
		var entryLine = Factory.New<CusEntryLine>();
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");
		AssertEquals("entryLine.Fees count", 13, entryLine.Fees.Count);

		var taxBoxSupporterList = entryLine.GetTaxBoxSupporterList();
		AssertNotNull("taxBoxSupporterList not null", taxBoxSupporterList);
		AssertEquals("taxBoxSupporterList count", 13, taxBoxSupporterList.Count());
		AssertArrayEqualsByElements("taxBoxSupporterList order", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, taxBoxSupporterList.Select(x => x.Type).ToArray());
	}

	public void TestGetTaxBoxSupporterListContainsNoExcludedItem()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var fee = entryLine.Fees.AddNew();
		fee.CF_ChargeType = "A00";
		fee.CF_RateOverrideReasonCode = "ADD";

		var excludedFee = entryLine.Fees.AddNew();
		excludedFee.CF_RateOverrideReasonCode = "EXC";

		AssertEquals("entryLine.Fees count", 2, entryLine.Fees.Count);

		var taxBoxSupporterList = entryLine.GetTaxBoxSupporterList().ToArray();
		AssertNotNull("taxBoxSupporterList not null", taxBoxSupporterList);
		AssertEquals("taxBoxSupporterList count", 1, taxBoxSupporterList.Length);
		AssertArrayEqualsByElements("taxBoxSupporterList order", new ZString[] { "A00" }, taxBoxSupporterList.Select(x => x.Type).ToArray());
	}

	public void TestPreferenceCodeStartWith2()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoiceLine.JI_PrimaryPreference = "300";
		AssertEquals("When PrimaryPreference is 300, PreferenceCodeStartWith2", false, entryLine.PreferenceCodeStartWith2);

		invoiceLine.JI_PrimaryPreference = "220";
		AssertEquals("When PrimaryPreference is 220, PreferenceCodeStartWith2", true, entryLine.PreferenceCodeStartWith2);
	}

	public void TestGetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceB = declaration.Invoices.AddNew();
		invoiceB.JZ_InvoiceNumber = "B";
		var invoiceLineB1 = invoiceB.InvoiceLines.AddNew();
		invoiceLineB1.JI_LineNo = 1;
		invoiceLineB1.JI_PrimaryPreference = "200";

		var invoiceA = declaration.Invoices.AddNew();
		invoiceA.JZ_InvoiceNumber = "A";
		var invoiceLineA2 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA2.JI_LineNo = 2;
		invoiceLineA2.JI_PrimaryPreference = "200";
		var invoiceLineA1 = invoiceA.InvoiceLines.AddNew();
		invoiceLineA1.JI_LineNo = 1;
		invoiceLineA1.JI_PrimaryPreference = "200";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLineB1.JI_CL = entryLine.PK;
		invoiceLineA2.JI_CL = entryLine.PK;
		invoiceLineA1.JI_CL = entryLine.PK;

		AssertEquals("[PRE-CONDITION] linked InvoiceLines count", 3, entryLine.InvoiceLines.Count);
		AssertEquals("GetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo", invoiceLineA1.PK, entryLine.GetFirstInvoiceLineOrderedByInvoiceNumberAndLineNo().PK);
	}

	public void TestIsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		entryLine.CL_CustomsValue = 5000m;
		AssertEquals("When CustomsValueInEuro < to 6000€, IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration", true, entryLine.IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration);

		entryLine.CL_CustomsValue = 6000m;
		AssertEquals("When CustomsValueInEuro = to 6000€, IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration", true, entryLine.IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration);

		entryLine.CL_CustomsValue = 7000m;
		AssertEquals("When CustomsValueInEuro > to 6000€, IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration", false, entryLine.IsCustomsInEuroValueLessOrEqualThanCustomsValueInEuroTresholdForOriginDeclaration);
	}

	public void TestExtraEUFreightChargesAmount()
	{
		var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
		var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();

		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		AssertEquals("No charges", 0m, entryLine.ExtraEUFreightChargesAmount);

		var invoiceLineCharge1 = invoiceLine1.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 50m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge1.J7_IsDutiable = true;
		invoiceLineCharge1.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge1.J7_IsGSTApplicable = true;
		var invoiceLineCharge2 = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 25m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge2.J7_IsDutiable = true;
		invoiceLineCharge2.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge2.J7_IsGSTApplicable = true;
		AssertEquals("ExtraEUFreightChargesAmount", 75m, entryLine.ExtraEUFreightChargesAmount);
	}

	public void TestEUFreightChargesAmount()
	{
		var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
		var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();

		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		AssertEquals("No charges", 0m, entryLine.EUFreightChargesAmount);

		var invoiceLineCharge1 = invoiceLine1.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 50m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge1.J7_IsDutiable = false;
		invoiceLineCharge1.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge1.J7_IsGSTApplicable = true;
		var invoiceLineCharge2 = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 25m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge2.J7_IsDutiable = false;
		invoiceLineCharge2.J7_IsStatisticalValueApplicable = true;
		invoiceLineCharge2.J7_IsGSTApplicable = true;
		AssertEquals("EUFreightChargesAmount", 75m, entryLine.EUFreightChargesAmount);
	}

	public void TestDomesticFreightChargesAmount()
	{
		var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
		var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();

		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		var entryLine = Factory.New<CusEntryLine>();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		AssertEquals("No charges", 0m, entryLine.DomesticFreightChargesAmount);

		var invoiceLineCharge1 = invoiceLine1.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 50m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge1.J7_IsDutiable = false;
		invoiceLineCharge1.J7_IsStatisticalValueApplicable = false;
		invoiceLineCharge1.J7_IsGSTApplicable = true;
		var invoiceLineCharge2 = invoiceLine2.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 25m, CurrencyCodes.EuropeanUnion);
		invoiceLineCharge2.J7_IsDutiable = false;
		invoiceLineCharge2.J7_IsStatisticalValueApplicable = false;
		invoiceLineCharge2.J7_IsGSTApplicable = true;
		AssertEquals("DomesticFreightChargesAmount", 75m, entryLine.DomesticFreightChargesAmount);
	}

	public void TestRequiresVATExemption()
	{
		var entryLineWithoutDeclaration = Factory.New<CusEntryLine>();
		Assert("VAT Exemption not required (declaration is null)", !entryLineWithoutDeclaration.RequiresVATExemption);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		Assert("VAT Exemption not required", !entryLine.RequiresVATExemption);

		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		Assert("VAT Exemption required", entryLine.RequiresVATExemption);
	}

	public void TestAddInfo_IsConnectedtoAutoClass()
	{
		AssertEquals(true, typeof(CusEntryLine).IsSubclassOf(typeof(AutoCusEntryLine)));
	}

	public void TestAddInfo_IsAutoGenerated()
	{
		AssertNotNull(typeof(AutoCusEntryLine).GetCustomAttribute<AutoGeneratedSourceCodeAttribute>(inherit: false));
	}

	public void TestAddInfo_HasUseAddInfoPropertyDescriptorsTrue()
	{
		AssertNotNull(typeof(AutoCusEntryLine).GetCustomAttribute<PropertyDescriptorCollectionAttribute>(inherit: false));
	}

	public void TestAddInfoType()
	{
		AssertEquals(typeof(AddInfoCusEntryLine), CusEntryLine.AddInfoType);
	}
	public void TestZG_LinesValueCaption() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.ZG_LinesValue)
			.WithCaption("Lines Value in EUR")
	);

	public void TestZG_LinesValue()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertEquals(true, entryLine.ZG_LinesValueInfo.ReadOnly);
	}

	public void TestZG_AdjustmentAmount()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertEquals(true, entryLine.ZG_AdjustmentAmountInfo.ReadOnly);
	}

	public void TestLinesValueInInvoiceCurrencyCaption() => CombineAssertions(() =>
		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.LinesValueInInvoiceCurrency)
			.WithCaption("Lines Value")
	);

	public void TestLinesValueInInvoiceCurrency()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "USD";
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_LinePrice = 1000m;
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 3000m;
		invoiceLine2.JI_CL = entryLine.PK;

		AssertEquals("LinesValueInInvoiceCurrency Should not be converted to EUR", 4000m, entryLine.LinesValueInInvoiceCurrency);
		AssertEquals("LinesValueInInvoiceCurrency should be same as TotalLinePrice", entryLine.TotalLinePrice.Amount, entryLine.LinesValueInInvoiceCurrency);
	}

	public void TestResetTotalsAndCachedValues_AddInfoProperties()
	{
		CombineAssertions(() =>
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.ZG_AdjustmentAmount = 12.3m;
			entryLine.ZG_LinesValue = 9.3m;
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("ZG_AdjustmentAmount", 0m, entryLine.ZG_AdjustmentAmount);
			AssertEquals("ZG_LinesValue", 0m, entryLine.ZG_LinesValue);
		});
	}

	public void TestIPackageProviderMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package = declaration.Packages.AddNew();
		package.CW_CR_HouseContainer = billPackingGroup.PK;
		package.CW_PackType = "PT";
		package.CW_MarksAndNos = "MARKS AND NOS";
		package.CW_PackQty = 100;

		var entryLine = Factory.New<CusEntryLine>();
		var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var packageInvoiceLine = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine.CHC_CW = package.PK;
		packageInvoiceLine.CHC_NumberOfPacks = 100;

		CombineAssertions(() =>
		{
			var packageProvider = (IPackageProvider)entryLine;
			AssertEquals(nameof(packageProvider.MarksAndNumbers), "MARKS AND NOS", packageProvider.MarksAndNumbers);
			AssertEquals(nameof(packageProvider.NumberOfPackages), 100, packageProvider.NumberOfPackages);
			AssertEquals(nameof(packageProvider.PackageType), "PT", packageProvider.PackageType);
		});
	}

	public void TestIMergedPreviousDocumentsProviderMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.CL_LineNumber = 2;
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		entryLine.ZG_NBStatus = "NBS";
		declaration.ResetApportionedPreviousDocuments();

		var mergedPreviousDocumentsProvider = entryLine as IMergedPreviousDocumentsProvider;
		AssertNotNull($"{nameof(CusEntryLine)} must implement {nameof(IMergedPreviousDocumentsProvider)}", mergedPreviousDocumentsProvider);

		CombineAssertions($"Assert {nameof(IMergedPreviousDocumentsProvider)} properties", () =>
		{
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.LineNumber), 2, mergedPreviousDocumentsProvider.LineNumber);
			AssertEquals($"{nameof(IMergedPreviousDocumentsProvider.MergedPreviousDocuments)} Count()", 2, mergedPreviousDocumentsProvider.MergedPreviousDocuments.Count());
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.NBStatus), "NBS", mergedPreviousDocumentsProvider.NBStatus);
			AssertEquals(nameof(IMergedPreviousDocumentsProvider.IsExport), true, mergedPreviousDocumentsProvider.IsExport);
		});

		declaration.JE_MessageType = "IMP";
		AssertEquals("When MessageType: IMP, IsExport", false, mergedPreviousDocumentsProvider.IsExport);
	}

	public void TestGetUnloadingDataPreviousProcedureDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLne = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLne.PK;

		AssertNull($"When entryLine has no Previous Documents, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());

		var paDocument = invoiceLine.PreviousDocuments.AddNew();
		paDocument.CSI_Procedure = "A3";
		var rpDocument = invoiceLine.PreviousDocuments.AddNew();
		rpDocument.CSI_Procedure = "2";
		rpDocument.CSI_Quantity = 200m;
		rpDocument.CSI_Quantity2 = 100m;
		rpDocument.CSI_Tariff = "1234568790";
		declaration.ResetApportionedPreviousDocuments();

		var previousProcedureDocument = entryLne.GetUnloadingDataPreviousProcedureDocument();
		AssertNotNull($"When entryLine has one PA and one RP Previous Document, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());

		CombineAssertions("1PA and 1RP document -> RP document quantities in UnloadingData", () =>
		{
			AssertEquals("1234568790", previousProcedureDocument.CSI_Tariff);
			AssertEquals(200m, previousProcedureDocument.NetMass);
			AssertEquals(100m, previousProcedureDocument.SupplementaryQuantity);
		});

		invoiceLine.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		declaration.ResetApportionedPreviousDocuments();
		AssertNull($"When entryLine has more than two Previous Documents, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());
	}

	public void TestGetUnloadingDataPreviousProcedureDocumentOnlyOneGenericDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLne = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLne.PK;

		AssertNull($"When entryLine has no Previous Documents, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());

		var paDocument = invoiceLine.PreviousDocuments.AddNew();
		paDocument.CSI_Procedure = "A3";
		declaration.ResetApportionedPreviousDocuments();

		var previousProcedureDocument = entryLne.GetUnloadingDataPreviousProcedureDocument();
		AssertNull($"When entryLine has one Previous Document which is not RP, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());
	}

	public void TestGetUnloadingDataPreviousProcedureDocumentOnlyOneRPDocument()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLne = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLne.PK;

		AssertNull($"When entryLine has no Previous Documents, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());

		var rpDocument = invoiceLine.PreviousDocuments.AddNew();
		rpDocument.CSI_Procedure = "2";
		rpDocument.CSI_Quantity = 200m;
		rpDocument.CSI_Quantity2 = 100m;
		rpDocument.CSI_Tariff = "1234568790";
		declaration.ResetApportionedPreviousDocuments();

		var previousProcedureDocument = entryLne.GetUnloadingDataPreviousProcedureDocument();
		AssertNotNull($"When entryLine has one RP Previous Document, {nameof(CusEntryLine.GetUnloadingDataPreviousProcedureDocument)}", entryLne.GetUnloadingDataPreviousProcedureDocument());

		CombineAssertions("Only 1RP document -> RP document quantities in UnloadingData", () =>
		{
			AssertEquals("1234568790", previousProcedureDocument.CSI_Tariff);
			AssertEquals(200m, previousProcedureDocument.NetMass);
			AssertEquals(100m, previousProcedureDocument.SupplementaryQuantity);
		});
	}

	public override void TestDutyRateDescription()
	{
		var cusEntryLine = Factory.New<CusEntryLineForTest>();
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_CL = cusEntryLine.PK;
		AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
		AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
		AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
		var b00Fee = cusEntryLine.Fees.AddNew();
		b00Fee.CF_Rate = 17.5;
		b00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		var a30Fee = cusEntryLine.Fees.AddNew();
		a30Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
		a30Fee.CF_BaseValue = 404m;
		a30Fee.CF_Rate = 50;
		a30Fee.CF_ChargeAmount = 202m;
		var a00Fee = cusEntryLine.Fees.AddNew();
		a00Fee.CF_BaseValue = 100m;
		a00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		a00Fee.CF_Rate = 33;
		a00Fee.CF_ChargeAmount = 33m;
		AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
		AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
		AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);
	}

	[TestDate(2005, 6, 2)]
	public override void TestMoneyInLocalCurrency()
	{
		var newCurrency = RefCurrency.New(Factory);
		newCurrency.RX_Code = "MDD";
		var from = new ZDateTime(2005, 6, 1);
		var to = new ZDateTime(2005, 6, 5);
		newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);

		var declaration = ImportJobDeclaration;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

		var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
		line1.JI_Tariff = "2203.10.10 10";
		line2.JI_Tariff = "2203.10.10 10";
		line1.JI_LinePrice = 100.0m;
		line2.JI_LinePrice = 200.0m;

		var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line1ONS.J7_Amount = 5.0m;
		line1ONS.J7_IsDutiable = true;
		var line1OFT = line1.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		line1OFT.J7_Amount = 10.0m;
		line1OFT.J7_IsDutiable = true;
		var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
		line2ONS.J7_Amount = 10.0m;
		line2ONS.J7_IsDutiable = true;
		var line2OFT = line2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		line2OFT.J7_IsDutiable = true;
		line2OFT.J7_Amount = 20.0m;

		CombineAssertions(() =>
		{
			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_CIF.Amount);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_CIF.Amount);
			AssertEquals("PreReq, line 1 CIF currency is invoice currency", newCurrency.Code, line1.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 2 CIF currency is invoice currency", newCurrency.Code, line2.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 1 CIF is 115", 115.0m, line1.JI_Calc_CIF);
			AssertEquals("PreReq, line 2 CIF is 230", 230.0m, line2.JI_Calc_CIF);
			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong.
			AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
		});
	}

	protected override ZString ExpectedClassificationDescription => "LINE";

	protected override ZString ExpectedFallbackEntrylineDescription => "LINE";

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		base.DoMerge(declaration);
	}

	public void TestIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var nBWrappableBusinessObject = entryLine as INBWrappableBusinessObject;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(INBWrappableBusinessObject)}", nBWrappableBusinessObject);

		declaration.JE_MessageType = "EXP";
		AssertEquals("isImport is false", false, nBWrappableBusinessObject.IsImport);

		declaration.JE_MessageType = "IMP";
		AssertEquals("isImport is true", true, nBWrappableBusinessObject.IsImport);
	}

	public void TestIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var nBWrappableBusinessObject = entryLine as INBWrappableBusinessObject;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(INBWrappableBusinessObject)}", nBWrappableBusinessObject);

		declaration.JE_MessageType = "EXP";
		AssertEquals("isExport is true", true, nBWrappableBusinessObject.IsExport);

		declaration.JE_MessageType = "IMP";
		AssertEquals("isExport is false", false, nBWrappableBusinessObject.IsExport);
	}

	public void TestLineNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var nBWrappableBusinessObject = entryLine as INBWrappableBusinessObject;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(INBWrappableBusinessObject)}", nBWrappableBusinessObject);

		entryLine.CL_LineNumber = ZShort.Zero;
		AssertEquals("LineNumber has the value zero", ZShort.Zero, nBWrappableBusinessObject.LineNumber);

		entryLine.CL_LineNumber = 1234;
		AssertEquals("LineNumber has the value 1234", 1234, nBWrappableBusinessObject.LineNumber);
	}

	public void TestEntryNumberWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var nBWrappableBusinessObject = entryLine as INBWrappableBusinessObject;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(INBWrappableBusinessObject)}", nBWrappableBusinessObject);

		CombineAssertions("EntryLine's EntryNumberWrapper is empty", () =>
		{
			var registrationInfoWrapper = nBWrappableBusinessObject.EntryNumberWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), ZString.Empty, registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), ZString.Empty, registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), ZDate.Empty, registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), ZString.Empty, registrationInfoWrapper.Series);
		});

		Factory.NewCusEntryNumber(entryHeader, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "4 T-2343G", issueDate: new ZDateTime(2020, 01, 01));
		CombineAssertions("EntryLine's EntryNumberWrapper is present", () =>
		{
			var registrationInfoWrapper = nBWrappableBusinessObject.EntryNumberWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), "4", registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), "2343G", registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), new ZDateTime(2020, 01, 01), registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), "T", registrationInfoWrapper.Series);
		});
	}

	public void TestNBGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();

		var nBWrappableBusinessObject = entryLine as INBWrappableBusinessObject;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(INBWrappableBusinessObject)}", nBWrappableBusinessObject);

		AssertNotNull("NBGroupedPreviousDocuments is not null", nBWrappableBusinessObject.NBGroupedPreviousDocuments);

		var invoiceLine1PreviousDocumentProcedureA3 = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "A3", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedureA3);
		declaration.DoMerge();
		AssertEquals("NBGroupedPreviousDocuments count", 0, nBWrappableBusinessObject.NBGroupedPreviousDocuments.Count());

		var invoiceLine1PreviousDocumentProcedure2S = PreviousDocumentTestHelper.CreateDocument(Factory, "ZZZ", new ZDate(2020, 01, 01), "999", "Z", "", "IT137100", 0, "MRN", "", "", "", 12m, 12m, 12m, 12);
		invoiceLine1.PreviousDocuments.Add(invoiceLine1PreviousDocumentProcedure2S);
		declaration.DoMerge();
		AssertEquals("Grouped Previous Documents count", 2, nBWrappableBusinessObject.NBGroupedPreviousDocuments.Count());
	}

	public void TestCusEntryLine_ICanBeImportOrExport_IsExport_withMessageTypeChange()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		var canBeImportOrExportMember = entryLine as ICanBeImportOrExport;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(ICanBeImportOrExport)}", canBeImportOrExportMember);

		declaration.JE_MessageType = "IMP";
		AssertEquals("When Message type = IMP, IsExport", false, canBeImportOrExportMember.IsExport);

		declaration.JE_MessageType = "EXP";
		AssertEquals("When Message type = EXP, IsExport", true, canBeImportOrExportMember.IsExport);
	}

	public void TestCusEntryLine_ICanBeImportOrExport_IsExport_without_Declaration()
	{
		var entryLine = Factory.New<CusEntryLine>();
		AssertNull(entryLine.Declaration);

		var canBeImportOrExportMember = entryLine as ICanBeImportOrExport;
		AssertNotNull($"{nameof(CusEntryLine)} as {nameof(ICanBeImportOrExport)}", canBeImportOrExportMember);
		AssertEquals("When CusEntryLine created without Declaration", false, canBeImportOrExportMember.IsExport);
	}

	public void TestReleaseCodeCaption()
	{
		AssertEquals("Caption", "Release Code", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryLine), CusEntryLine.Schema.ReleaseCode).Caption);
	}

	public void TestReleaseDateCaption()
	{
		AssertEquals("Caption", "Release Date", DataBoundResourceStrings.GetDataForProperty(typeof(CusEntryLine), CusEntryLine.Schema.ReleaseDate).Caption);
	}

	public void TestReleaseCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		AssertEquals("When CusEntryNum missing for EntryLine", "", entryLine1.ReleaseCode);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		Factory.NewCusEntryNumber(entryLine2, CusEntryNumberConstants.EntryTypes.ClereanceCode, entryNum: null, issueDate: new ZDateTime(2020, 01, 01));
		AssertEquals("When CusEntryNum added without Entry num for EntryLine", "", entryLine2.ReleaseCode);

		var entryLine3 = entryHeader.MergedLines.AddNew();
		Factory.NewCusEntryNumber(entryLine3, CusEntryNumberConstants.EntryTypes.ClereanceCode, entryNum: "WE32T6", issueDate: new ZDateTime(2020, 01, 01));
		AssertEquals("When CusEntryNum added with Entry num for EntryLine", "WE32T6", entryLine3.ReleaseCode);
	}

	public void TestReleaseDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		AssertEquals("When CusEntryNum missing for EntryLine", "", entryLine1.ReleaseDate.ToString());

		var entryLine2 = entryHeader.MergedLines.AddNew();
		Factory.NewCusEntryNumber(entryLine2, CusEntryNumberConstants.EntryTypes.ClereanceCode, entryNum: "WE32T6", issueDate: null);
		AssertEquals("When CusEntryNum added without Issue Date for EntryLine", "", entryLine2.ReleaseDate.ToString());

		var entryLine3 = entryHeader.MergedLines.AddNew();
		Factory.NewCusEntryNumber(entryLine3, CusEntryNumberConstants.EntryTypes.ClereanceCode, entryNum: "WE32T6", issueDate: new ZDateTime(2020, 01, 01));
		AssertEquals("When CusEntryNum added with Issue Date for EntryLine", new ZDate(2020, 01, 01), entryLine3.ReleaseDate);
	}

	public void TestNatureOfTransaction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("NatureOfTransaction", "", entryLine.NatureOfTransaction);

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoice.JZ_ValuationCode = "11";
		entryLine.RefreshInvoiceLines();
		AssertEquals("NatureOfTransaction", "11", entryLine.NatureOfTransaction);
	}

	public void TestCountryOfExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("CountryOfExport", "", entryLine.CountryOfExport);

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_RN_NKCountryOfExport = "US";
		entryLine.RefreshInvoiceLines();
		AssertEquals("CountryOfExport", "US", entryLine.CountryOfExport);
	}

	public void TestEffectiveGrossWeightKg()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("Gross Weight Sum", ZDecimal.Zero, entryLine.EffectiveGrossWeightKg);

		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.EffectiveGrossWeightKg)
			.WithCaption("Gross Weight (KG)")
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces.Equals(6), because: "Gross Weight should have 6 decimal places");

		var invoice1 = declaration.Invoices.AddNew();
		var invoice1Line1 = CreateInvoiceLine(invoice1, entryLine);
		var invoice1Line2 = CreateInvoiceLine(invoice1, entryLine);
		var invoice2 = declaration.Invoices.AddNew();
		var invoice2Line1 = CreateInvoiceLine(invoice2, entryLine);
		var invoice2Line2 = CreateInvoiceLine(invoice2, entryLine);

		invoice1Line1.JI_Weight = 123;
		invoice1Line1.JI_WeightUQ = "KG";
		invoice1Line2.JI_Weight = 123;
		invoice1Line2.JI_WeightUQ = "G";

		invoice2Line1.JI_Weight = 123;
		invoice2Line1.JI_WeightUQ = "KG";
		invoice2Line2.JI_Weight = 123;
		invoice2Line2.JI_WeightUQ = "G";

		entryLine.RefreshInvoiceLines();
		AssertEquals("Gross Weight Sum", new ZDecimal(246.246), entryLine.EffectiveGrossWeightKg);
	}

	public void TestEffectiveNetWeightKg()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("Net Weight Sum", ZDecimal.Zero, entryLine.EffectiveNetWeightKg);

		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.EffectiveNetWeightKg)
			.WithCaption("Net Weight (KG)")
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces.Equals(6), because: "Net Weight should have 6 decimal places");

		var invoice1 = declaration.Invoices.AddNew();
		var invoice1Line1 = CreateInvoiceLine(invoice1, entryLine);
		var invoice1Line2 = CreateInvoiceLine(invoice1, entryLine);
		var invoice2 = declaration.Invoices.AddNew();
		var invoice2Line1 = CreateInvoiceLine(invoice2, entryLine);
		var invoice2Line2 = CreateInvoiceLine(invoice2, entryLine);

		invoice1Line1.JI_NetWeight = 123;
		invoice1Line1.JI_NetWeightUQ = "KG";
		invoice1Line2.JI_NetWeight = 123;
		invoice1Line2.JI_NetWeightUQ = "G";

		invoice2Line1.JI_NetWeight = 123;
		invoice2Line1.JI_NetWeightUQ = "KG";
		invoice2Line2.JI_NetWeight = 123;
		invoice2Line2.JI_NetWeightUQ = "G";

		entryLine.RefreshInvoiceLines();
		AssertEquals("Net Weight Sum", new ZDecimal(246.246), entryLine.EffectiveNetWeightKg);
	}

	public void TestEffectiveCustomsWeightKg()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("Customs Quantity Sum", ZDecimal.Zero, entryLine.EffectiveCustomsWeightKg);

		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.EffectiveCustomsWeightKg)
			.WithCaption("Customs Qty (KG)")
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces.Equals(6), because: "Customs Quantity should have 6 decimal places");

		var invoice1 = declaration.Invoices.AddNew();
		var invoice1Line1 = CreateInvoiceLine(invoice1, entryLine);
		var invoice1Line2 = CreateInvoiceLine(invoice1, entryLine);
		var invoice2 = declaration.Invoices.AddNew();
		var invoice2Line1 = CreateInvoiceLine(invoice2, entryLine);
		var invoice2Line2 = CreateInvoiceLine(invoice2, entryLine);

		invoice1Line1.JI_CustomsQuantity = 102;
		invoice1Line2.JI_CustomsQuantity = 21;

		invoice2Line1.JI_CustomsQuantity = 6;
		invoice2Line2.JI_CustomsQuantity = 303;

		entryLine.RefreshInvoiceLines();
		AssertEquals("Customs Quantity Sum", new ZDecimal(432), entryLine.EffectiveCustomsWeightKg);
	}

	public void TestEffectiveSupplementaryQuantity()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("EffectiveSupplementaryQuantity", ZDecimal.Zero, entryLine.EffectiveSupplementaryQuantity);

		AssertEntity<CusEntryLine>()
			.HasProperty(x => x.EffectiveSupplementaryQuantity)
			.WithCaption("Suppl. Qty")
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces.Equals(6), because: "EffectiveSupplementaryQuantity should have 6 decimal places");

		var invoice1 = declaration.Invoices.AddNew();
		var invoice1Line1 = CreateInvoiceLine(invoice1, entryLine);
		var invoice1Line2 = CreateInvoiceLine(invoice1, entryLine);
		var invoice2 = declaration.Invoices.AddNew();
		var invoice2Line1 = CreateInvoiceLine(invoice2, entryLine);
		var invoice2Line2 = CreateInvoiceLine(invoice2, entryLine);

		invoice1Line1.JI_CustomsSecondQuantity = 102;
		invoice1Line2.JI_CustomsSecondQuantity = 21;

		invoice2Line1.JI_CustomsSecondQuantity = 6;
		invoice2Line2.JI_CustomsSecondQuantity = 303;

		entryLine.RefreshInvoiceLines();
		AssertEquals("EffectiveSupplementaryQuantity sum", new ZDecimal(432), entryLine.EffectiveSupplementaryQuantity);
	}

	JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoiceHeader, CusEntryLine entryLine)
	{
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return invoiceLine;
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

	void SetUpFees(CusEntryLineFeeCollection lineFeeCollection, params ZString[] rateCodesToAdd)
	{
		foreach (var rateCode in rateCodesToAdd)
		{
			lineFeeCollection.AddOrUpdate(rateCode, 0m);
		}
	}

	void CheckDocument(
		MergedPreviousDocument mergedPreviousDocument
		, ZString csiCode
		, ZDate dateOfIssue
		, ZString referenceNumber
		, ZString subType
		, ZString status
		, ZString customsOffice
		, ZShort lineNo
		, ZString procedure
		, ZString referenceNumber2
		, ZString tariff
		, ZString unitOfQuantity2
		, ZDecimal quantity
		, ZDecimal quantity2
		, ZDecimal grossMass
		, ZInt packageQuantity)
	{
		AssertEquals("CSI_Code should be", csiCode, mergedPreviousDocument.CSI_Code);
		AssertEquals("CSI_DateOfIssue should be", dateOfIssue, mergedPreviousDocument.CSI_DateOfIssue);
		AssertEquals("CSI_ReferenceNumber should be", referenceNumber, mergedPreviousDocument.CSI_ReferenceNumber);
		AssertEquals("CSI_SubType should be", subType, mergedPreviousDocument.CSI_SubType);
		AssertEquals("CSI_Status should be", status, mergedPreviousDocument.CSI_Status);
		AssertEquals("CSI_CustomsOffice should be", customsOffice, mergedPreviousDocument.CSI_CustomsOffice);
		AssertEquals("CSI_LineNo should be", lineNo, mergedPreviousDocument.CSI_LineNo);
		AssertEquals("CSI_Procedure should be", procedure, mergedPreviousDocument.CSI_Procedure);
		AssertEquals("CSI_ReferenceNumber2 should be", referenceNumber2, mergedPreviousDocument.CSI_ReferenceNumber2);
		AssertEquals("CSI_Tariff should be", tariff, mergedPreviousDocument.CSI_Tariff);
		AssertEquals("CSI_UnitOfQuantity2 should be", unitOfQuantity2, mergedPreviousDocument.CSI_UnitOfQuantity2);
		AssertEquals("NetMass should be", quantity, mergedPreviousDocument.NetMass);
		AssertEquals("SupplementaryQuantity should be", quantity2, mergedPreviousDocument.SupplementaryQuantity);
		AssertEquals("GrossMass should be", grossMass, mergedPreviousDocument.GrossMass);
		AssertEquals("PackageQuantity should be", packageQuantity, mergedPreviousDocument.PackageQuantity);
	}

	(CusEntryLine entryLine, BasePackage package1, BasePackage package2, BasePackingGroup packingGroup, JobDeclaration declaration, JobComInvoiceLine invoiceLine1, JobComInvoiceLine invoiceLine2) SetupEntryLineWithPackages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var declarationBill = declaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = declaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		var package2 = declaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = billPackingGroup.PK;
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		var packageInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		var packageInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package2.PK;
		return (entryLine, package1, package2, billPackingGroup, declaration, invoiceLine1, invoiceLine2);
	}
}

class CusEntryLineForTest : CusEntryLine
{
	public CusEntryLineForTest(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	public ZString GetDutyRateDescriptionForTest() => GetDutyRateDescription();

	public ZDecimal GetGstRateForTest() => GetGSTRate();
}
