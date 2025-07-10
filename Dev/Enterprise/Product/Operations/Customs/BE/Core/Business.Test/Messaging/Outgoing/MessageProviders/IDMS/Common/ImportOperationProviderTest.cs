using System;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class ImportOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportOperationProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("null", () => new IE415BDataProvider(null));
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		AssertExceptionThrown<ArgumentNullException>("no JobDeclaration", () => new IE415BDataProvider(null));
		var jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("no CusEntryInstruction", () => new IE415BDataProvider(null));
	});

	public void TestLRN()
	{
		entryHeader.CH_BGMReference = "lrn";
		AssertEquals("lrn", provider.LRN);
	}

	public void TestMRN()
	{
		entryHeader.MovementReferenceNumberSetter("mrn");
		AssertEquals("mrn", provider.MRN);
	}

	public void TestDeclarationType()
	{
		jobDeclaration.JE_EntryStyle = "EX";
		AssertEquals("EX", provider.DeclarationType);
	}

	public void TestAdditionalDeclarationType()
	{
		entryInstruction.CEI_SubStyle = "D";
		AssertEquals("D", provider.AdditionalDeclarationType);
	}

	public void TestPresentationDateAndTime()
	{
		var dateTime = ZDateTime.Now;
		jobDeclaration.ZG_PresentationStartDate = dateTime;
		AssertEquals(dateTime.ToDateTime(), provider.PresentationDateAndTime);
	}

	public void TestDeclarationAcceptanceDate()
	{
		var dateTime = ZDateTime.Now;
		var entryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, jobDeclaration.CountryCode);
		entryNum.CE_IssueDate = dateTime;
		AssertEquals(dateTime.ToDateTime(), provider.DeclarationAcceptanceDate);
	}

	public void TestLanguageCode()
	{
		jobDeclaration.JE_DeclarationLanguage = "EN";
		AssertEquals("EN", provider.LanguageCode);
	}

	protected override ImportOperationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryInstruction = Factory.New<CusEntryInstruction>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		provider = new ImportOperationProvider(entryHeader);
	}
	JobDeclaration jobDeclaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	ImportOperationProvider provider;
}
