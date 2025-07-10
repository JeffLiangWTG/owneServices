using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TobaccoLookups))]
sealed class TobaccoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListExport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertList((ICodeDescriptionPairList)lookups.CodeList, "1", "2", "3", "4", "5");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertList((ICodeDescriptionPairList)lookups.CodeList, "1", "2", "3", "4", "5");
	});

	public void TestCodeListImport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertList((ICodeDescriptionPairList)lookups.CodeList, "1", "2", "3", "4", "5");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertList((ICodeDescriptionPairList)lookups.CodeList, "2", "3", "4", "5");
	});

	public void TestAdditionalDescriptionListExport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertList(lookups.AdditionalDescriptionList, "1", "2", "3", "4", "5");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertList(lookups.AdditionalDescriptionList, "1", "2", "3", "4", "5");
	});

	public void TestAdditionalDescriptionListImport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertList(lookups.AdditionalDescriptionList, "1", "2", "3", "4", "5");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertList(lookups.AdditionalDescriptionList, "2", "3", "4", "5");
	});

	public void TestSubType_Cigars_Export() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigars, "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigars, "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11");
	});

	public void TestSubType_Cigars_Import() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigars, "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigars, "02", "03", "04", "05", "06", "07", "08", "09", "10", "11");
	});

	public void TestSubType_Cigarettes_Export() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigarettes, "01", "02", "03", "04", "05", "06", "07");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigarettes, "01", "02", "03", "04", "05", "06", "07");
	});

	public void TestSubType_Cigarettes_Import() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigarettes, "01", "02", "03", "04", "05", "06", "07");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Cigarettes, "02", "03", "04", "05", "06", "07");
	});

	public void TestSubType_CutTobacco_Export() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.CutTobacco, "01", "02", "03", "04", "05", "06");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.CutTobacco, "01", "02", "03", "04", "05", "06");
	});

	public void TestSubType_CutTobacco_Import() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.CutTobacco, "01", "02", "03", "04", "05", "06");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.CutTobacco, "02", "03", "04", "05", "06");
	});

	public void TestSubType_Assortment_Export() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Assortment, "01", "02", "03");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Assortment, "01", "02", "03");
	});

	public void TestSubType_Assortment_Import() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Assortment, "01", "02", "03");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.Assortment, "02", "03");
	});

	public void TestSubType_ECigarettes_Export() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.ECigarettes, "01", "02");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.ECigarettes, "01", "02");
	});

	public void TestSubType_ECigarettes_Import() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertSubTypeCodesList(TobaccoMainGroupCodes.ECigarettes, "01", "02");

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertSubTypeCodesList(TobaccoMainGroupCodes.ECigarettes, "02");
	});

	public void TestSpecialUnitOfMeasureList()
	{
		var codeList = lookups.SpecialUnitOfMeasureList;

		CombineAssertions(() =>
		{
			AssertType<TobaccoSpecialUnitOfMeasureList>(lookups.SpecialUnitOfMeasureList);
			AssertSame("Cached", codeList, lookups.SpecialUnitOfMeasureList);
			AssertEquals("TransportMeansList", "NAR, KG", codeList.CodesAsString);
		});
	}

	void AssertSubTypeCodesList(string code, params string[] expectedCodes)
	{
		tobacco.CSI_Code = code;
		var list = lookups.SubTypeList;
		AssertEquals("count", expectedCodes.Length, list.Count);
		foreach (var expectedCode in expectedCodes)
		{
			AssertEquals($"Code {expectedCode}", true, list.ContainsCode(expectedCode));
		}
	}

	void AssertList(ICodeDescriptionPairList list, params string[] expectedCodes)
	{
		AssertEquals("count", expectedCodes.Length, list.Count);
		foreach (var expectedCode in expectedCodes)
		{
			AssertEquals($"Code {expectedCode}", true, list.ContainsCode(expectedCode));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		RefCusCodeTestHelper.CreateTobaccoLists(Factory);

		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		tobacco = invoiceLine.Tobaccos.AddNew();
		lookups = tobacco.Lookups;
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction entryInstruction;
	Tobacco tobacco;
	TobaccoLookups lookups;
}
