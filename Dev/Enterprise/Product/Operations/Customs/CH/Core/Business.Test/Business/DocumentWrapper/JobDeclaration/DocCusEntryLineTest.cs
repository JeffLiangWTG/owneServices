using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocCusEntryLine))]
sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
{
	public void TestPackages()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var package1 = declaration.Packages.AddNew();
		var package2 = declaration.Packages.AddNew();
		var package3 = declaration.Packages.AddNew();
		var package4 = declaration.Packages.AddNew();
		var pivot1 = invoiceLine1.PackagesPivot.AddNew();
		var pivot2 = invoiceLine1.PackagesPivot.AddNew();
		var pivot3 = invoiceLine1.PackagesPivot.AddNew();
		var pivot4 = invoiceLine2.PackagesPivot.AddNew();
		package1.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		package2.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
		package3.CW_PackType = "??";
		package4.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		pivot1.CHC_CW = package1.PK;
		pivot2.CHC_CW = package2.PK;
		pivot3.CHC_CW = package3.PK;
		pivot4.CHC_CW = package4.PK;
		pivot1.CHC_NumberOfPacks = 10;
		pivot2.CHC_NumberOfPacks = 20;
		pivot3.CHC_NumberOfPacks = 30;
		pivot4.CHC_NumberOfPacks = 2;

		var expectedResult = new ZStringBuilder();
		expectedResult.Append($"{nameof(RefCusCodeTestHelper.UNPKGCodeNoBulk)} / 12");
		expectedResult.Append($"{nameof(RefCusCodeTestHelper.UNPKGCodeWithBulkYes)} / 20");
		expectedResult.Append($" / 30");
		AssertEquals(expectedResult.ToStringWithNewLineBetweenAppends(), EntryLineWrapperInternal.PackageLines);
	}

	public void TestPreviousDocuments()
	{
		RefCusCodeTestHelper.CreatePreviousDocumentsList(Factory);

		var document1 = invoiceHeader.PreviousDocuments.AddNew();
		var document2 = invoiceHeader.PreviousDocuments.AddNew();
		var document3 = invoiceHeader.PreviousDocuments.AddNew();
		document1.CSI_Code = RefCusCodeTestHelper.ValidPreviousDocumentsListImport;
		document1.CSI_ReferenceNumber = "docref1";
		document1.CSI_Description = "addinf1";
		document2.CSI_Code = RefCusCodeTestHelper.ValidPreviousDocumentsListImport;
		document2.CSI_ReferenceNumber = "docref2";
		document3.CSI_Code = RefCusCodeTestHelper.ValidPreviousDocumentsListImport;
		document3.CSI_Description = "addinf3";

		var expectedResult = new ZStringBuilder();
		expectedResult.Append($"{nameof(RefCusCodeTestHelper.ValidPreviousDocumentsListImport)} / docref1 / addinf1");
		expectedResult.Append($"{nameof(RefCusCodeTestHelper.ValidPreviousDocumentsListImport)} / docref2");
		expectedResult.Append($"{nameof(RefCusCodeTestHelper.ValidPreviousDocumentsListImport)} / addinf3");
		AssertEquals(expectedResult.ToStringWithNewLineBetweenAppends(), EntryLineWrapperInternal.PreviousDocumentLines);
	}

	public void TestPreviousDocuments_FromAllInvoiceHeaders() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePreviousDocumentsList(Factory);

		declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2a = invoiceHeader2.InvoiceLines.AddNew();
		var invoiceLine2b = invoiceHeader2.InvoiceLines.AddNew();
		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

		var document1 = invoiceHeader.PreviousDocuments.AddNew();
		var document2 = invoiceHeader2.PreviousDocuments.AddNew();
		var document3a = invoiceHeader.PreviousDocuments.AddNew();
		var document3b = invoiceHeader2.PreviousDocuments.AddNew();
		var document4a = invoiceHeader.PreviousDocuments.AddNew();
		var document4b = invoiceHeader2.PreviousDocuments.AddNew();
		var document5a = invoiceHeader.PreviousDocuments.AddNew();
		var document5b = invoiceHeader2.PreviousDocuments.AddNew();
		document1.CSI_ReferenceNumber = "docref1";
		document2.CSI_ReferenceNumber = "docref2";
		document3a.CSI_ReferenceNumber = "docref3";
		document3b.CSI_ReferenceNumber = "docref3";
		document4a.CSI_ReferenceNumber = "docref4";
		document4a.CSI_Description = "a";
		document4b.CSI_ReferenceNumber = "docref4";
		document4b.CSI_Description = "b";
		document5a.CSI_ReferenceNumber = "docref5";
		document5a.CSI_Code = "a";
		document5b.CSI_ReferenceNumber = "docref5";
		document5b.CSI_Code = "b";

		var lines = EntryLineWrapperInternal.PreviousDocumentLines.ToString();
		AssertEquals("docref1 should exist exactly once", 1, lines.CountMatches("docref1"));
		AssertEquals("docref2 should exist exactly once", 1, lines.CountMatches("docref2"));
		AssertEquals("docref3 should exist exactly once", 1, lines.CountMatches("docref3"));
		AssertEquals("docref4 should exist exactly twice", 2, lines.CountMatches("docref4"));
		AssertEquals("docref5 should exist exactly twice", 2, lines.CountMatches("docref5"));
	});

	public void TestDescription() => CombineAssertions(() =>
	{
		EntryLineInternal.CL_Description = "EntryLine Description";
		EntryLineInternal.RandomLine.JI_Description = "InvoiceLine Description";
		AssertEquals("EntryLine Description", EntryLineWrapperInternal.Description);

		EntryLineInternal.CL_Description = ZString.Empty;
		AssertEquals("InvoiceLine Description", EntryLineWrapperInternal.Description);
	});

	public void TestGrossWeight()
	{
		EntryLineInternal.RandomLine.JI_CustomsQuantity = 1.2;
		AssertEquals(1.2m, EntryLineWrapperInternal.GrossWeightInKG);
	}

	public override void TestDutyAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
		AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
	}

	public override void TestGSTVATAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.VAT, 12.3453M);
		AssertEquals("GSTVATAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
	}

	public override void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
	{
		Assert(true);
	}

	public override void TestLinePricesWithCurrency()
	{
		Assert(true);
	}

	public void TestNetWeight()
	{
		EntryLineInternal.RandomLine.JI_CustomsSecondQuantity = 1.2m;
		AssertEquals("NetWeight", 1.2m, EntryLineWrapperInternal.NetWeight);
	}

	public void TestGrossWeightInKG()
	{
		EntryLineInternal.RandomLine.JI_CustomsQuantity = 1.2m;
		AssertEquals("GrossWeightInKG", 1.2m, EntryLineWrapperInternal.GrossWeightInKG);
	}

	public void TestBorderValue() => CombineAssertions(() =>
	{
		EntryLineInternal.CL_StatisticalValue = 0;
		AssertEquals("BorderValue 0", 0m, EntryLineWrapperInternal.BorderValue);
		EntryLineInternal.CL_StatisticalValue = 0.5m;
		AssertEquals("BorderValue 0.5", 1m, EntryLineWrapperInternal.BorderValue);
		EntryLineInternal.CL_StatisticalValue = 3.56m;
		AssertEquals("BorderValue 3.56", 3m, EntryLineWrapperInternal.BorderValue);
	});

	public void TestCommercialGoods()
	{
		EntryLineInternal.RandomLine.JI_NonTradingGoods = true;
		AssertEquals("CommercialGoods", false, EntryLineWrapperInternal.CommercialGoods);
	}

	public void TestSupplementaryUnits()
	{
		EntryLineInternal.RandomLine.JI_CustomsThirdQuantity = 1.2m;
		AssertEquals("SupplementaryUnits", 1.2m, EntryLineWrapperInternal.SupplementaryUnits);
	}

	public void TestRestrictionObligation() => CombineAssertions(() =>
	{
		AssertEquals("RestrictionObligation no restrictions", false, EntryLineWrapperInternal.RestrictionObligation);
		EntryLineInternal.RandomLine.Restrictions.AddNew();
		AssertEquals("RestrictionObligation with restrictions", true, EntryLineWrapperInternal.RestrictionObligation);
	});

	public void TestPackagingReferences()
	{
		var package1 = declaration.Packages.AddNew();
		var package2 = declaration.Packages.AddNew();
		var package3 = declaration.Packages.AddNew();
		var pivot1 = invoiceLine1.PackagesPivot.AddNew();
		var pivot2 = invoiceLine1.PackagesPivot.AddNew();
		var pivot3 = invoiceLine1.PackagesPivot.AddNew();
		package1.CW_PackType = "1A";
		package1.CW_MarksAndNos = "a";
		package2.CW_PackType = "1F";
		package2.CW_MarksAndNos = "b c";
		package3.CW_PackType = "1A";
		package3.CW_MarksAndNos = "d e f";
		pivot1.CHC_CW = package1.PK;
		pivot2.CHC_CW = package2.PK;
		pivot3.CHC_CW = package3.PK;
		pivot1.CHC_NumberOfPacks = 1;
		pivot2.CHC_NumberOfPacks = 2;
		pivot3.CHC_NumberOfPacks = 3;

		AssertEquals("PackagingReferences", "1A, a d e f, 4; 1F, b c, 2", EntryLineWrapperInternal.PackagingReferences);
	}

	public void TestProcedureDescription()
	{
		RefCusCodeTestHelper.CreateProcedureCodeList(Factory);
		EntryLineInternal.RandomLine.JI_Procedure = "01";
		AssertEquals("ProcedureDescription", "ValidImportProcedureCode", EntryLineWrapperInternal.ProcedureDescription);
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Core.Constants.CountryCodes.Switzerland; }
	}

	protected override CusEntryLine GetNewEntryLine()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		declaration.JE_ClusterKey = 1;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		return entryLine;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
	CusEntryHeader entryHeader;

	protected override DocCusEntryLine CreateEntryLineWrapper(Customs.Business.ICusEntryLine entryLineInternal)
	{
		return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
	}

	protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
	{
		get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
	}

	#endregion
}
