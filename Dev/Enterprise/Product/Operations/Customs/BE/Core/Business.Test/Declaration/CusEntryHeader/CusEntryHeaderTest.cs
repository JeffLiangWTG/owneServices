using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
{
	public void TestDeclaration()
	{
		AssertType<JobDeclaration>(CreateEntryHeader().Declaration);
	}

	public override void TestOfficeOfEntry()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
		{
			var declaration = (JobDeclaration)GetNewDeclaration();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");

			CombineAssertions(() =>
			{
				AssertEquals("Office of Entry of declaration", "LV002000", declaration.OfficeOfEntry);
				AssertEquals("Office of Entry of entry", "LV002000", entry.OfficeOfEntry);
			});
		}
	}

	public void TestDeclarantRegistrationCode()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("123456789");
		AssertEquals("123456789", entryHeader.DeclarantEoriOfMainOfficeRegistrationNumber);
		SetCompanyEoriNumber("BE123456789");
		AssertEquals("123456789", entryHeader.DeclarantEoriOfMainOfficeRegistrationNumber);
	}

	public void TestLocalReferenceNumberGenerated()
	{
		entryHeader = CreateEntryHeader();
		AssertNotNullOrEmpty(GetLocalReferenceNumberAfterSave());
	}

	public void TestLocalReferenceNumberCurrentYearPart()
	{
		entryHeader = CreateEntryHeader();
		var firstTwoCharactersForCurrentYear = ZDateTime.Now.Year.ToString().Substring(2);
		AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the current year.", firstTwoCharactersForCurrentYear, GetLocalReferenceNumberAfterSave());
	}

	[TestDate(2071, 01, 01)]
	public void TestLocalReferenceNumberDifferentYearPart()
	{
		entryHeader = CreateEntryHeader();
		var firstTwoCharactersForMockedCurrentYear = "71";
		AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the Mocked \"current\" year.", firstTwoCharactersForMockedCurrentYear, GetLocalReferenceNumberAfterSave());
	}

	public void TestLocalReferenceNumberEoriNumberPart()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("123456789");
		var partThatShouldStartWithEORI = GetLocalReferenceNumberAfterSave().Substring(2);

		AssertStartsWith("Expected Local Reference Number, after the first 2 characters, to contain the EORI of the Declarant.", "123456789", partThatShouldStartWithEORI);
	}

	public void TestLocalReferenceNumberNumberFountainPart()
	{
		entryHeader = CreateEntryHeader();
		var partThatShouldBe9RandomDigits = GetLocalReferenceNumberAfterSave().Substring(2 + 9);
		AssertMatch("Expected Local Reference Number, after the first 11 characters, to be 9 random digits.", new Regex(@"\d{9}"), partThatShouldBe9RandomDigits);

		entryHeader.CH_BGMReference = null;
		var partThatShouldBe9DifferentRandomDigits = GetLocalReferenceNumberAfterSave().Substring(2 + 9);
		AssertNotEquals("Expected Local Reference Number, after the first 11 characters, to be 9 random digits. Yet they were the same after generating the LRN twice.", partThatShouldBe9DifferentRandomDigits, partThatShouldBe9RandomDigits);
	}

	public void TestLocalReferenceNumberAlreadySet()
	{
		entryHeader = CreateEntryHeader();
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberDeclarantChange()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("123456789");
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		SetCompanyEoriNumber("987654321");
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertNotEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberAfterDeclarationUCRChange()
	{
		entryHeader = CreateEntryHeader();
		var localReferenceNumberAtStart = GetLocalReferenceNumberAfterSave();
		entryHeader.Declaration.JE_UCR = "123456789";
		var localReferenceNumberAfterAnotherSave = GetLocalReferenceNumberAfterSave();

		AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
	}

	public void TestLocalReferenceNumberSubmitted()
	{
		TestLocalReferenceNumberSubmitted(false, false, true);
		TestLocalReferenceNumberSubmitted(true, false, false);
		TestLocalReferenceNumberSubmitted(false, true, false);
		TestLocalReferenceNumberSubmitted(true, true, false);
	}

	public void TestManualDeclarationProperty()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		AssertEquals(entryInstruction.ZG_ManualDeclaration, entryHeader.ZG_ManualDeclaration);

		entryInstruction.ZG_ManualDeclaration = true;
		AssertEquals(entryInstruction.ZG_ManualDeclaration, entryHeader.ZG_ManualDeclaration);

		entryInstruction.ZG_ManualDeclaration = false;
		AssertEquals(entryInstruction.ZG_ManualDeclaration, entryHeader.ZG_ManualDeclaration);
	}

	public void TestDefaultStatusDescription()
	{
		AssertEquals(ZString.Empty, CreateEntryHeader().DefaultStatusDescription);
	}

	public void TestSetAllEntryLinesReadOnly_MovementReferenceNumber()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.ZG_ManualDeclaration = false;

		CombineAssertions(() =>
		{
			entryHeader.MovementReferenceNumberSetter("TESTMRN123");
			entryHeader.SetAllEntryLinesReadOnly();
			AssertEquals("MRN Set", true, ((Customs.Business.AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).ReadOnly);
			entryHeader.MovementReferenceNumberSetter(string.Empty);
			entryHeader.SetAllEntryLinesReadOnly();
			AssertEquals("MRN Empty", false, ((Customs.Business.AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).ReadOnly);
		});
	}

	public void TestSetAllEntryLinesReadOnly_ManualDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("TESTMRN123");

		CombineAssertions(() =>
		{
			entryInstruction.ZG_ManualDeclaration = false;
			AssertEquals("Not manual", true, ((Customs.Business.AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).ReadOnly);
			entryInstruction.ZG_ManualDeclaration = true;
			AssertEquals("Manual", false, ((Customs.Business.AllCusEntryLineCollection<CusEntryLine>)entryHeader.AllEntryLines).ReadOnly);
		});
	}

	protected override ZBool IgnoreTestsThatRequireEntryShouldSetUCRinBGMReferenceNumber => true;

	protected override Customs.Business.BaseJobDeclaration GetNewDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		return dec;
	}

	protected override Customs.Business.BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}
	}

	protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new CurrencyTestSetup();

	CusEntryHeader entryHeader;
	CusEntryHeader CreateEntryHeader()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.SetDeclarationForTesting(declaration);

		var declarant = entryHeader.DeclarantOrganisation;
		declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123987465", "BE");

		return entryHeader;
	}

	void SetCompanyEoriNumber(string value)
	{
		var company = entryHeader.Declaration.Branch.OrgProxy;
		company.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, value, "BE");
		Factory.Save();
	}

	ZString GetLocalReferenceNumberAfterSave()
	{
		Factory.Save();
		return entryHeader.CH_BGMReference;
	}

	void TestLocalReferenceNumberSubmitted(bool isWaitingForResponse, bool hasBeenLodgedAtCustoms, bool changeExpected)
	{
		var declaration = Factory.New<JobDeclaration>();
		
		var entryHeader = Factory.New<DummyCusEntryHeader>();
		entryHeader.SetDeclarationForTesting(declaration);

		var declarant = entryHeader.DeclarantOrganisation;
		entryHeader.IsWaitingForResponseReturns = isWaitingForResponse;
		entryHeader.HasBeenLodgedAtCustomsReturns = hasBeenLodgedAtCustoms;

		declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "BE");
		Factory.Save();
		var localReferenceNumberAtStart = entryHeader.CH_BGMReference;

		declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", "BE");
		Factory.Save();
		var localReferenceNumberAfterAnotherSave = entryHeader.CH_BGMReference;

		if (changeExpected)
		{
			AssertNotEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
		}
		else
		{
			AssertEquals(localReferenceNumberAtStart, localReferenceNumberAfterAnotherSave);
		}
	}

	public void TestLocalReferenceNumberLength()
	{
		entryHeader = CreateEntryHeader();
		SetCompanyEoriNumber("BEDE9668822669288");
		var localReferenceNumber = GetLocalReferenceNumberAfterSave();
		CombineAssertions(() =>
		{
			AssertEquals("Length of LocalReferenceNumber should be 22 characters, long EORI-nr", 22, localReferenceNumber.Length);
			SetCompanyEoriNumber("BE123");
			localReferenceNumber = GetLocalReferenceNumberAfterSave();
			AssertEquals("Length of LocalReferenceNumber should be 22 characters, short EORI-nr", 22, localReferenceNumber.Length);
			SetCompanyEoriNumber("BEDE966882266928812345678901234567");
			localReferenceNumber = GetLocalReferenceNumberAfterSave();
			AssertEquals("Length of LocalReferenceNumber should be 0 characters, Invalid (very long) EORI-nr", 0, localReferenceNumber.Length);
		});
	}

	sealed class DummyCusEntryHeader : CusEntryHeader
	{
		public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsWaitingForResponseReturns { get; set; }
		public bool HasBeenLodgedAtCustomsReturns { get; set; }

		public override bool IsWaitingForResponse => IsWaitingForResponseReturns;
		public override bool HasBeenLodgedAtCustoms => HasBeenLodgedAtCustomsReturns;
	}

	sealed class CurrencyTestSetup : IChargesCurrencyTestSetup
	{
		void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(Customs.Business.BaseJobDeclaration declaration, ZString currencyCode)
		{
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			var nonDutiableCharge = invoiceHeader.Charges.AddNew();
			nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			nonDutiableCharge.J7_Amount = 200m;
			nonDutiableCharge.J7_IsDutiable = false;
			nonDutiableCharge.J7_IsIncludedInITOT = true;

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = BECustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 500m;
			oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
		}

		ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10500m;
		ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 11000m;
	}

	public override void TestShouldCompletelyReassignNumbers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		AssertEquals(true, entryHeader.ShouldCompletelyReassignNumbers);
		entryHeader.EntryNumber = "EN001";
		entryHeader.MovementReferenceNumberSetter("TESTMRN123");

		AssertEquals(false, entryHeader.ShouldCompletelyReassignNumbers);
	}
}
