using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefundMethodOfCalculation = Enterprise.Customs.IE.Business.Constants.CusEntryLineFeeRefundDutyMethodOfCalculation;

namespace Enterprise.Customs.IE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	public void TestAdditionalInfos()
	{
		UniversalReferenceTestDataHelper universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		RefDataGrouping eun = universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		universalReferenceTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
		universalReferenceTestDataHelper.CreateNewOrGetExistingCusCodeType("ADDIN", "Additional Information", "ZZ", 0);
		universalReferenceTestDataHelper.CreateCusCodeList("EUN", "ADDIN", "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		universalReferenceTestDataHelper.CreateCusCodeList("EUN", "ADDIN", "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var inv = dec.Invoices.AddNew();
		var invLine1 = inv.JobComInvoiceLines.AddNew();
		invLine1.JI_Tariff = "2203001010";

		var addInfo1 = invLine1.AdditionalInfos.AddNew();
		addInfo1.CSI_Code = "9001";
		addInfo1.CSI_Description = "9001 Desc";

		var addInfo2 = invLine1.AdditionalInfos.AddNew();
		addInfo2.CSI_Code = "9001";
		addInfo2.CSI_Description = "9001 Desc";

		var addInfo3 = invLine1.AdditionalInfos.AddNew();
		addInfo3.CSI_Code = "9003";
		addInfo3.CSI_Description = "9003 Desc";

		DoMerge(dec);

		AssertEquals("ActiveEntryHeaders", 1, dec.ActiveEntryHeaders.Count);
		AssertEquals("MergedLines", 1, dec.ActiveEntryHeaders[0].MergedLines.Count);
		AssertEquals("AdditionalInfos", 2, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());

		addInfo2.CSI_Code = "12345";

		dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals("AdditionalInfos", 3, ((CusEntryLine)dec.ActiveEntryHeaders[0].MergedLines[0]).AdditionalInfos.Count());
	}

	public void TestHeader()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		AssertType<CusEntryHeader>(entryLine.Header);
	}

	public void TestRandomLine()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		AssertType<JobComInvoiceLine>(entryLine.RandomLine);
	}

	public override void TestDutyRateDescription()
	{
		var cusEntryLine = Factory.New<CusEntryLine>();
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
		a30Fee.CF_ChargeAmount = 202m;
		a30Fee.CF_BaseValue = 404m;
		a30Fee.CF_Rate = 50;
		var a00Fee = cusEntryLine.Fees.AddNew();
		a00Fee.CF_ChargeAmount = 33m;
		a00Fee.CF_BaseValue = 100m;
		a00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		a00Fee.CF_Rate = 33;
		AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
		AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
		AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);
	}

	public void TestPreviousDocuments()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();

		var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];

		var invLinePrevDoc1 = invLine.PreviousDocuments.AddNew();
		invLinePrevDoc1.CSI_Code = "111";
		invLinePrevDoc1.CSI_ReferenceNumber = "111";

		var invLinePrevDoc2 = invLine.PreviousDocuments.AddNew();
		invLinePrevDoc2.CSI_Code = "222";
		invLinePrevDoc2.CSI_ReferenceNumber = "222";

		var invHeaderPrevDoc = invLine.InvoiceHeader.PreviousDocuments.AddNew();
		invHeaderPrevDoc.CSI_Code = "333";
		invHeaderPrevDoc.CSI_ReferenceNumber = "333";

		var declarationPrevDoc = invLine.Declaration.PreviousDocuments.AddNew();
		declarationPrevDoc.CSI_Code = "444";
		declarationPrevDoc.CSI_ReferenceNumber = "444";

		AssertEquals("Entry Line Prev Doc Count", 2, entryLine.PreviousDocuments.Count());
		var aPrevDocToTest = entryLine.PreviousDocuments.GetEnumerator();
		aPrevDocToTest.MoveNext();
		AssertEquals("aPrevDocOnInvLine.CSI_Code = 111", "111", aPrevDocToTest.Current.CSI_Code);
		aPrevDocToTest.MoveNext();
		AssertEquals("aPrevDocOnInvLine.CSI_Code = 222", "222", aPrevDocToTest.Current.CSI_Code);
	}

	public void TestPreviousDocuments_Base()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		entryLine.Declaration.JE_MessageType = MessageTypeList.Codes.Import;

		var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];

		var invLinePrevDoc1 = invLine.PreviousDocuments.AddNew();
		invLinePrevDoc1.CSI_Code = "111";
		invLinePrevDoc1.CSI_ReferenceNumber = "111";

		var invLinePrevDoc2 = invLine.PreviousDocuments.AddNew();
		invLinePrevDoc2.CSI_Code = "222";
		invLinePrevDoc2.CSI_ReferenceNumber = "222";

		var invHeaderPrevDoc = invLine.InvoiceHeader.PreviousDocuments.AddNew();
		invHeaderPrevDoc.CSI_Code = "333";
		invHeaderPrevDoc.CSI_ReferenceNumber = "333";

		var declarationPrevDoc = invLine.Declaration.PreviousDocuments.AddNew();
		declarationPrevDoc.CSI_Code = "444";
		declarationPrevDoc.CSI_ReferenceNumber = "444";

		AssertEquals("Base - Line Prev Doc Count", 2, entryLine.PreviousDocuments.Count());
		var aPrevDocToTest = entryLine.PreviousDocuments.GetEnumerator();
		aPrevDocToTest.MoveNext();
		AssertEquals("Base - aPrevDocOnInvLine.CSI_Code = 111", "111", aPrevDocToTest.Current.CSI_Code);
		aPrevDocToTest.MoveNext();
		AssertEquals("Base - aPrevDocOnInvLine.CSI_Code = 222", "222", aPrevDocToTest.Current.CSI_Code);
	}

	public void TestGetNewValidation()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		entryLine.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertType<EU.Business.Declaration.CusEntryLineValidation>("Base EU CusEntryLineValidation for entry lines under IMP jobs.", entryLine.Validation);

		entryLine.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
		AssertType<ExportCusEntryLineValidation>("IE ExportCusEntryLineValidation for entry lines under EXP jobs.", entryLine.Validation);
	}

	public void TestCountryOfExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		AssertEquals("CountryOfExport", ZString.Empty, entryLine.CountryOfExport);

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_RN_NKCountryOfExport = "IE";
		entryLine.RefreshInvoiceLines();
		AssertEquals("CountryOfExport", "IE", entryLine.CountryOfExport);
	}

	public void TestFiscalReferences()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions[0].PK;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLineA = invoice.JobComInvoiceLines.AddNew();
		var invoiceLineB = invoice.JobComInvoiceLines.AddNew();
		var invoiceLineC = invoice.JobComInvoiceLines.AddNew();

		var fr1 = invoiceLineB.FiscalReferences.AddNew();
		fr1.CFR_Code = "FR1";
		fr1.CFR_Reference = "GB11111111";

		var fr2 = invoiceLineB.FiscalReferences.AddNew();
		fr2.CFR_Code = "FR2";
		fr2.CFR_Reference = "GB22222222";

		var fr3 = invoiceLineC.FiscalReferences.AddNew();
		fr3.CFR_Code = "FR1";
		fr3.CFR_Reference = "GB11111111";

		var fr4 = invoiceLineC.FiscalReferences.AddNew();
		fr4.CFR_Code = "FR2";
		fr4.CFR_Reference = "GB33333333";

		var fr5 = invoiceLineC.FiscalReferences.AddNew();
		fr5.CFR_Code = "FR4";
		fr5.CFR_Reference = "GB44444444";

		var fr6 = declaration.CustomsEntryInstructions[0].FiscalReferences.AddNew();
		fr6.CFR_Code = "FR6";
		fr6.CFR_Reference = "GB66666666";

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

		AssertEquals(3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

		var entryLineA = declaration.CustomsEntryHeaders[0].MergedLines[0];
		AssertEquals(0, entryLineA.FiscalReferences.Count());

		var entryLineB = declaration.CustomsEntryHeaders[0].MergedLines[1];
		AssertEquals(2, entryLineB.FiscalReferences.Count());
		AssertNotNull(entryLineB.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR1" && x.CFR_Reference == "GB11111111"));
		AssertNotNull(entryLineB.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR2" && x.CFR_Reference == "GB22222222"));

		var entryLineC = declaration.CustomsEntryHeaders[0].MergedLines[2];
		AssertEquals(3, entryLineC.FiscalReferences.Count());
		AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR1" && x.CFR_Reference == "GB11111111"));
		AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR2" && x.CFR_Reference == "GB33333333"));
		AssertNotNull(entryLineC.FiscalReferences.FirstOrDefault(x => x.CFR_Code == "FR4" && x.CFR_Reference == "GB44444444"));
	}

	public void TestRefundDuties()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		var refundDuty = Factory.New<CusEntryLineFee>();
		refundDuty.CF_CL = entryLine.PK;
		refundDuty.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		refundDuty.CF_MethodOfCalculation = RefundMethodOfCalculation.ToBeRepaid;

		AssertEquals("RefundDuties should have loaded.", 1, entryLine.RefundDuties.Count);
	}

	public void TestVATValues()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();

		var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];
		if (invLine.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction != null)
		{
			invLine.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var supportingDoc = entryInstruction.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "1A06";
			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 100m);
			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryLine.GSTVATDeferred", 100m, entryLine.GSTVATDeferred);
				AssertEquals("CustomsEntryLine.GSTVATAmount", 0m, entryLine.GSTVATAmount);
			});
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			CombineAssertions(() =>
			{
				AssertEquals("CustomsEntryLine.GSTVATDeferred", 0m, entryLine.GSTVATDeferred);
				AssertEquals("CustomsEntryLine.GSTVATAmount", 100m, entryLine.GSTVATAmount);
			});
		}
	}

	public void TestVATNotAccessedInMerge()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();

		var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];
		if (invLine.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction != null)
		{
			var declaration = entryInstruction.EntryHeader.Declaration;
			var isMergeInProgress = declaration.GetType().GetProperty("IsMergeInProgress", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (isMergeInProgress != null)
			{
				isMergeInProgress.SetValue(declaration, true);
			}

			invLine.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;

			AssertEquals("CustomsEntryLine.GSTVATDeferred", 0m, entryLine.GSTVATDeferred);
		}
	}

	public void TestIsSecuritiesForEndUse()
	{
		var entryLine = (CusEntryLine)GetNewBusinessObject();
		var invoiceLine = entryLine.RandomLine;
		var invoiceHeader = invoiceLine.InvoiceHeader;
		var instruction = invoiceLine.EntryInstruction;
		instruction.CEI_Style = string.Empty;
		invoiceLine.JI_Procedure = string.Empty;
		instruction.AdditionalInfos.RemoveAndDeleteAll();

		invoiceHeader.AdditionalInfos.RemoveAndDeleteAll();
		invoiceLine.AdditionalInfos.RemoveAndDeleteAll();

		CombineAssertions("IsSecuritiesForEndUse", () =>
		{
			AssertEquals("False by default.", false, entryLine.IsSecuritiesForEndUse);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			invoiceLine.JI_Procedure = "44";
			AssertEquals("H1-44, no 00100, expect false.", false, entryLine.IsSecuritiesForEndUse);

			instruction.AdditionalInfos.AddNew("00100", "").CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("H1-44, 00100 present, expect true.", true, entryLine.IsSecuritiesForEndUse);
			instruction.AdditionalInfos.RemoveAndDeleteAll();
			AssertEquals("To make sure 00100 removed.", false, entryLine.IsSecuritiesForEndUse);

			invoiceHeader.AdditionalInfos.AddNew("00100", "").CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("H1-44, 00100 on InvoiceHeader, expect true.", true, entryLine.IsSecuritiesForEndUse);
			invoiceHeader.AdditionalInfos.RemoveAndDeleteAll();
			AssertEquals("To make sure InvoiceHeader 00100 removed.", false, entryLine.IsSecuritiesForEndUse);

			invoiceLine.AdditionalInfos.AddNew("00100", "").CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("H1-44, 00100 on InvoiceLine, expect true.", true, entryLine.IsSecuritiesForEndUse);

			invoiceLine.JI_Procedure = "53";
			AssertEquals("H1-53, 00100 present, expect false.", false, entryLine.IsSecuritiesForEndUse);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			AssertEquals("H3-53, 00100 present, expect true.", true, entryLine.IsSecuritiesForEndUse);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			AssertEquals("H4-53, 00100 present, expect false.", false, entryLine.IsSecuritiesForEndUse);
			invoiceLine.JI_Procedure = "51";
			AssertEquals("H4-51, 00100 present, expect true.", true, entryLine.IsSecuritiesForEndUse);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		var line = entry.MergedLines.AddNew();
		invoiceLine.JI_CL = line.PK;
		line.Fees.AddNew();
		line.Header.PivotsToContainers.RemoveAndDeleteAll();
		line.Header.PivotsToContainers.GetOrCreatePivotFor(line.Header.Declaration.CusContainers.AddNew());
		return line;
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);
}
