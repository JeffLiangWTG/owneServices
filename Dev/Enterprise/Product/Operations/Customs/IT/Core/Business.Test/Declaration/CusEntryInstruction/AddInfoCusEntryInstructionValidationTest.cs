using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
{
	[TestDate(2019, 01, 01)]
	public void TestCheckZG_TempProcLimitDateNotRequired()
	{
		SetUpRefData();

		declaration.JE_MessageType = "EXP";
		entryInstruction.CEI_Procedure = "";
		entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2019, 01, 01);
		AssertHasMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, "Temporary Procedure Limit Date is not required for empty Procedure Code");

		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, "Temporary Procedure Limit Date is not required for empty Procedure Code");

		entryInstruction.CEI_Procedure = "40";
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);
		entryInstruction.CEI_Procedure = "21";
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertHasMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);
	}

	[TestDate(2019, 01, 01)]
	public void TestCheckZG_TempProcLimitDateRequired()
	{
		SetUpRefData();

		declaration.JE_MessageType = "EXP";
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);

		CombineAssertions("Temporary procedure", () =>
		{
			AssertTemporaryProcedureLimitDate(entryInstruction, "21");
			AssertTemporaryProcedureLimitDate(entryInstruction, "22");
			AssertTemporaryProcedureLimitDate(entryInstruction, "23");

			AssertTemporaryProcedureLimitDate(entryInstruction, "51");
			AssertTemporaryProcedureLimitDate(entryInstruction, "53");

			entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2020, 01, 01);
			AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, "Temporary Procedure Limit Date should be greater than today.");
			entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2018, 12, 31);
			AssertHasMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, "Temporary Procedure Limit Date should be greater than today.");

			entryInstruction.CEI_Procedure = "60";
			AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);

			entryInstruction.CEI_Procedure = ZString.Empty;
			entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);
			entryInstruction.ZG_TempProcLimitDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);

			entryInstruction.CEI_Procedure = "40";
			entryInstruction.ZG_TempProcLimitDate = new ZDateTime(2019, 01, 01);
			AssertHasMessageErrorContaining(entryInstruction.ZG_TempProcLimitDateInfo, "Temporary Procedure Limit Date is not required for Procedure Code 40");
		});
	}

	public void TestCheckZG_ParticipantType()
	{
		CombineAssertions("JE_MessageType = EXP", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			entryInstruction.ZG_ParticipantType = ZString.Empty;
			AssertHasErrorContaining("Empty", entryInstruction.ZG_ParticipantTypeInfo, MandatoryValidation.MustBeEntered);

			entryInstruction.ZG_ParticipantType = "XXX";
			AssertHasErrorContaining("Invalid", entryInstruction.ZG_ParticipantTypeInfo, ListValidation.InvalidCodeError);

			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			AssertNoErrors("Valid", entryInstruction.ZG_ParticipantTypeInfo);
		});

		CombineAssertions("JE_MessageType IS NOT EXP", () =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction.ZG_ParticipantType = ZString.Empty;
			AssertNoErrors("Empty", entryInstruction.ZG_ParticipantTypeInfo);

			entryInstruction.ZG_ParticipantType = "XXX";
			AssertNoErrors("Invalid", entryInstruction.ZG_ParticipantTypeInfo);

			entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
			AssertNoErrors("Valid", entryInstruction.ZG_ParticipantTypeInfo);
		});
	}

	public void TestCheckZG_PreviousInvoiceAmountMandatoryValidation()
	{
		entryInstruction.ZG_ParticipantType = ZString.Empty;
		entryInstruction.ZG_PreviousInvoiceAmount = 0m;
		AssertNoMessageErrorContaining("Empty ZG_ParticipantType", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction.ZG_PreviousInvoiceAmount = 0m;
		AssertNoMessageErrorContaining("ZG_ParticipantType = BUY", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		entryInstruction.ZG_PreviousInvoiceAmount = 0m;
		AssertHasMessageErrorContaining("ZG_ParticipantType = TRG, empty value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction.ZG_PreviousInvoiceAmount = 0m;
		AssertHasMessageErrorContaining("ZG_ParticipantType = JTD, empty value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);

		entryInstruction.ZG_PreviousInvoiceAmount = 1m;
		AssertNoMessageErrorContaining("ZG_ParticipantType = JTD, filled value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);
	}

	public void TestCheckZG_PreviousInvoiceAmountNegativeValidation()
	{
		entryInstruction.ZG_ParticipantType = ZString.Empty;
		entryInstruction.ZG_PreviousInvoiceAmount = -1m;
		AssertNoMessageErrorContaining("Empty ZG_ParticipantType", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction.ZG_PreviousInvoiceAmount = -2m;
		AssertNoMessageErrorContaining("ZG_ParticipantType = BUY", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		entryInstruction.ZG_PreviousInvoiceAmount = -1m;
		AssertHasMessageErrorContaining("ZG_ParticipantType = TRG, negative value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction.ZG_PreviousInvoiceAmount = -2m;
		AssertHasMessageErrorContaining("ZG_ParticipantType = JTD, negative value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

		entryInstruction.ZG_PreviousInvoiceAmount = 1m;
		AssertNoMessageErrorContaining("ZG_ParticipantType = JTD, positive value", entryInstruction.ZG_PreviousInvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);
	}

	public void TestCheckZG_PreviousInvoiceCurrencyMandatoryValidation()
	{
		entryInstruction.ZG_PreviousInvoiceCurrency = ZString.Empty;
		AssertNoMessageErrorContaining("Empty ZG_ParticipantType", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction.ZG_PreviousInvoiceCurrency = ZString.Empty;
		AssertNoMessageErrorContaining("ZG_ParticipantType = BUY", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		entryInstruction.ZG_PreviousInvoiceCurrency = ZString.Empty;
		AssertHasMessageErrorContaining("ZG_ParticipantType = TRG, empty value", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction.ZG_PreviousInvoiceCurrency = ZString.Empty;
		AssertHasMessageErrorContaining("ZG_ParticipantType = JTD, empty value", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, MandatoryValidation.YouHaveNotEntered);

		entryInstruction.ZG_PreviousInvoiceCurrency = "EUR";
		AssertNoMessageErrorContaining("ZG_ParticipantType = JTD, filled value", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckZG_PreviousInvoiceCurrencyListValidation()
	{
		const string expectedMessageError = "The code you have selected is not in the list";

		entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";
		AssertNoMessageErrorContaining("Empty ZG_ParticipantType", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedMessageError);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";
		AssertNoMessageErrorContaining("ZG_ParticipantType = BUY, invalid currency", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedMessageError);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";
		AssertHasMessageErrorContaining("ZG_ParticipantType = TRG, invalid currency", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedMessageError);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction.ZG_PreviousInvoiceCurrency = "XXX";
		AssertHasMessageErrorContaining("ZG_ParticipantType = JTD, invalid currency", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedMessageError);

		entryInstruction.ZG_PreviousInvoiceCurrency = "EUR";
		AssertNoMessageErrorContaining("ZG_ParticipantType = JTD, valid currency", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedMessageError);
	}

	public void TestCheckZG_PreviousInvoiceCurrencyMissingExchangeRate()
	{
		var expectedWarning = "There is no exchange rate in the database for the valuation date";

		var foreignCurrencyWithoutValidCustomsRate = RefCurrency.New(Factory);
		foreignCurrencyWithoutValidCustomsRate.RX_Code = "INV";
		foreignCurrencyWithoutValidCustomsRate.SetCustomsRate(ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-4), 0.19m);
		var foreignCurrencyWithValidCustomsRate = RefCurrency.New(Factory);
		foreignCurrencyWithValidCustomsRate.RX_Code = "VAL";
		foreignCurrencyWithValidCustomsRate.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8119m);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.Triangulation;
		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithoutValidCustomsRate.RX_Code;
		AssertHasWarningContaining("ZG_ParticipantType = TRG, currency without valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);

		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithValidCustomsRate.RX_Code;
		AssertNoWarningContaining("ZG_ParticipantType = TRG, currency with valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.JointDeclarationManySuppliersForTheSameEntryLine;
		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithoutValidCustomsRate.RX_Code;
		AssertHasWarningContaining("ZG_ParticipantType = JTD, currency without valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);

		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithValidCustomsRate.RX_Code;
		AssertNoWarningContaining("ZG_ParticipantType = JTD, currency with valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);

		entryInstruction.ZG_ParticipantType = ParticipantTypeList.Codes.BuyersConsolManySuppliersOneImporter;
		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithoutValidCustomsRate.RX_Code;
		AssertNoWarningContaining("ZG_ParticipantType = BUY, currency without valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);

		entryInstruction.ZG_PreviousInvoiceCurrency = foreignCurrencyWithValidCustomsRate.RX_Code;
		AssertNoWarningContaining("ZG_ParticipantType = BUY, currency with valid customs rate", entryInstruction.ZG_PreviousInvoiceCurrencyInfo, expectedWarning);
	}

	public void TestZG_SimplifiedDecAcceptanceDate_EmptyForSubStylesOtherThanXYZ()
	{
		SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(ITEntrySubStyleList.Codes.StandardDeclarationA, entryInstruction);
		AssertNoNotifications(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo);

		SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD, entryInstruction);
		AssertNoNotifications(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo);
	}

	public void TestZG_SimplifiedDecAcceptanceDate_DateRequiredForSubStylesXYZ()
	{
		SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(ITEntrySubStyleList.Codes.SupplementaryDeclarationX, entryInstruction);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, MandatoryValidation.YouHaveNotEntered);

		SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(ITEntrySubStyleList.Codes.SupplementaryDeclarationY, entryInstruction);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, MandatoryValidation.YouHaveNotEntered);

		SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(ITEntrySubStyleList.Codes.SupplementaryDeclarationZ, entryInstruction);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, MandatoryValidation.YouHaveNotEntered);
	}

	[TestDate(2020, 03, 01)]
	public void TestZG_SimplifiedDecAcceptanceDate_DateShouldNotBeGreaterThanTodayForSubStylesXYZ()
	{
		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationX;

		entryInstruction.ZG_SimplifiedDecAcceptanceDate = new ZDateTime(2020, 04, 01);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, "greater than today's date");

		entryInstruction.ZG_SimplifiedDecAcceptanceDate = new ZDateTime(2020, 03, 01);
		AssertNoNotifications(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo);

		entryInstruction.ZG_SimplifiedDecAcceptanceDate = new ZDateTime(2020, 01, 01);
		AssertNoNotifications(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo);

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationY;
		entryInstruction.ZG_SimplifiedDecAcceptanceDate = new ZDateTime(2020, 03, 02);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, "greater than today's date");

		entryInstruction.CEI_SubStyle = ITEntrySubStyleList.Codes.SupplementaryDeclarationZ;
		entryInstruction.ZG_SimplifiedDecAcceptanceDate = new ZDateTime(2020, 03, 02);
		AssertHasMessageErrorContaining(entryInstruction.ZG_SimplifiedDecAcceptanceDateInfo, "greater than today's date");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	void SetSubStyleAndRunValidationForSimplifiedDecAcceptanceDate(string value, CusEntryInstruction entryInstruction)
	{
		entryInstruction.CEI_SubStyle = value;
		entryInstruction.AddInfoValidation.ValidateZG_SimplifiedDecAcceptanceDate();
	}

	void AssertTemporaryProcedureLimitDate(CusEntryInstruction entryInstruction, ZString procedureCode)
	{
		entryInstruction.CEI_Procedure = procedureCode;
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Empty;
		AssertHasMessageErrorContaining(string.Format("CP = {0} AND ZG_TempProcLimitDate Empty", procedureCode), entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);
		entryInstruction.ZG_TempProcLimitDate = ZDateTime.Now;
		AssertNoMessageErrorContaining("No message error if ZG_TempProcLimitDate provided", entryInstruction.ZG_TempProcLimitDateInfo, ValidationCaptions.EntryInstruction.TemporaryProcedureLimitDateRequired);
	}

	void SetUpRefData()
	{
		var dataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		dataHelper.CreateRefCusProcedure40And71ForCurrentCountry();
		dataHelper.CreateRefCusProcedure23ForCurrentCountry();
		dataHelper.CreateRefCusProcedure53ForCurrentCountry();
		dataHelper.CreateRefCusProcedure21And22ForCurrentCountry();
		dataHelper.CreateRefCusProcedure51ForCurrentCountry();
	}
}
