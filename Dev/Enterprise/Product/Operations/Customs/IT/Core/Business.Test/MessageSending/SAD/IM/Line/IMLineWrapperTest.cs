using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class IMLineWrapperTest : SADLineCommonWrapperTest<IMLineWrapper>
{
	public abstract void TestPreferences();
	public abstract void TestQuotas();
	public abstract void TestItemPriceInEuro();
	public abstract void TestAdjustmentInEuro();

	public override void TestCountryOfOrigin()
	{
		var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = "US";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals(nameof(sadLineWrapper.CountryOfOrigin), "US", sadLineWrapper.CountryOfOrigin);

		invoiceLine.JI_CountryOfOrigin = "";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals(nameof(sadLineWrapper.CountryOfOrigin), "", sadLineWrapper.CountryOfOrigin);
	}

	public void TestEvaluationMethod()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_ValuationCode = "1";
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_ValuationCode = "2";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Evaluation Method should be", "1", sadLineWrapper.EvaluationMethod);
	}

	public void TestPackage()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("Package should not be null", sadLineWrapper.Package);
		AssertType<SADLinePackageWrapper>("Package type", sadLineWrapper.Package);
	}

	public void TestSpecialMentionGroup()
	{
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNotNull("SpecialMentionGroup should not be null", sadLineWrapper.SpecialMentionGroup);
		AssertType<IMLineSpecialMentionGroupWrapper>("SpecialMentionGroup type", sadLineWrapper.SpecialMentionGroup);
	}

	public void TestGoodsDescriptionCleanedOfBlackListChars()
	{
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_Description = "Tubi lanciamissili;\tlanciafiamme;\r\nlanciagranate;lanciasiluri e dispositivi di lancio simili";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Invalid chars have been removed", "Tubi lanciamissili; lanciafiamme; lanciagranate;lanciasiluri e dispositivi di lancio simili", sadLineWrapper.GoodsDescription);
	}

	public void TestNotesWithMarksAndNumbers()
	{
		var invoiceLine1 = entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = entryLine.InvoiceLines.AddNew();
		var declarationBill = jobDeclaration.Bills.AddNew();
		var billPackingGroup = declarationBill.PackingGroups.AddNew();
		var package1 = jobDeclaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = billPackingGroup.PK;
		var package2 = jobDeclaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = billPackingGroup.PK;
		var packageInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
		packageInvoiceLine1.CHC_CW = package1.PK;
		var packageInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		packageInvoiceLine2.CHC_CW = package2.PK;

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "";
		package2.CW_MarksAndNos = "";
		AssertEquals("", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 1";
		AssertEquals("", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 2";
		AssertEquals("", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "THIS IS 20 CHAR LENG";
		package2.CW_MarksAndNos = "THIS IS 21 CHAR LENGX";
		AssertEquals("", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "MARK AND NOS PACKAGE 1";
		package2.CW_MarksAndNos = "MARK AND NOS PACKAGE 2";
		AssertEquals("Other Marks: MARK AND NOS PACKAGE 2", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "THIS IS 53 STRING LENGTH THERE ARE NO BLOCK TO REMOVE";
		package2.CW_MarksAndNos = "";
		AssertEquals("Other Marks: THIS IS 53 STRING LENGTH THERE ARE NO BLOCK TO REMOVE", sadLineWrapper.Notes);

		package1.CW_MarksAndNos = "THIS IS 30 CHAR LENGTH STRINGG";
		package2.CW_MarksAndNos = "THIS IS 29 CHAR LENGTH STRING";
		AssertEquals("Other Marks: THIS IS 30 CHAR LENGTH STRINGG;THIS IS 29 CHAR LENGTH STRING", sadLineWrapper.Notes);

		var package3 = jobDeclaration.Packages.AddNew();
		package3.CW_CR_HouseContainer = billPackingGroup.PK;
		var package3InvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		package3InvoiceLine2.CHC_CW = package3.PK;

		var package4 = jobDeclaration.Packages.AddNew();
		package4.CW_CR_HouseContainer = billPackingGroup.PK;
		var package4InvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
		package4InvoiceLine2.CHC_CW = package4.PK;

		package1.CW_MarksAndNos = "MARK AND NOS 1";
		package2.CW_MarksAndNos = "MARK AND NOS 2";
		package3.CW_MarksAndNos = "MARK AND NOS 3";
		package4.CW_MarksAndNos = "MARK AND NOS 4";
		AssertEquals("Other Marks: MARK AND NOS 3;MARK AND NOS 4", sadLineWrapper.Notes);
	}

	public void TestNotesCleanedOfBlackListChars()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.Remarks = "additional\tinfo\r\ndescription";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Invalid chars have been removed", "additional info description", sadLineWrapper.Notes);
	}

	public void TestNotesMaximumLength()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.Remarks = new ZString('1', 300);
		invoiceLine2.Remarks = new ZString('2', 300);
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Notes have been truncated to maximum length", 500, sadLineWrapper.Notes.Length);
	}

	protected void SetUpRefData()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "71", "11", "111", "One", "IMP", group: "IFD", intoWarehouse: true);
		helper.CreateRefCusProcedure(currentCountry, "A", "40", "22", "222", "Two", "IMP", group: "IFD", intoWarehouse: false);
		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration.JE_MessageType = "IMP";
	}

	protected override IReadOnlyList<ZString> ExpectedNationalProcedures => System.Array.Empty<ZString>();
}
