using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ExportJobComInvoiceLineCountryExportValidationTest : TestCaseWithFactory
{
	public void TestCheckJI_RN_NKCountryOfExport_ListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: grouping);

		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "Export country/territory for entry style EX");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				invoiceLine.JI_RN_NKCountryOfExport = "ZZ";
				AssertNoMessageErrorContaining("When JI_RN_NKCountryOfExport is valid", invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError.ToString());

				invoiceLine.JI_RN_NKCountryOfExport = "AA";
				AssertHasMessageErrorContaining("When JI_RN_NKCountryOfExport is not valid", invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			invoiceLine.JI_RN_NKCountryOfExport = "AA";
			AssertNoMessageErrorContaining("When Declaration is not UCC6 and JI_RN_NKCountryOfExport is not valid", invoiceLine.JI_RN_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError.ToString());
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeDeclareAtHeaderOrLineLevel_WhenBothHeaderAndLineAreEmpty()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsOrigin = "DE";
				invoiceLine.JI_RN_NKCountryOfExport = "";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);

				declaration.JE_GoodsOrigin = "";
				invoiceLine.JI_RN_NKCountryOfExport = "DE";
				AssertNoMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);

				declaration.JE_GoodsOrigin = "";
				invoiceLine.JI_RN_NKCountryOfExport = "";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeDeclareAtHeaderOrLineLevel_WhenNoEntryInstructionIsLinked()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);

				invoiceLine.JI_CEI = ZGuid.Empty;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeDeclareAtHeaderOrLineLevel_ForMultipleEntryInstruction()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			CombineAssertions("Two invoice lines attached to the same Entry. One of them is not empty", () =>
			{
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				declaration.JE_GoodsOrigin = "";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});

			CombineAssertions("Two invoice lines attached to different Entries. One of them is not empty", () =>
			{
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeDeclareAtHeaderOrLineLevel_ForNonUCC6()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, CountryOfExportExpectedMessageError);
			});
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeEntered()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			CombineAssertions("Two invoice lines attached to the same Entry. Both are empty", () =>
			{
				invoiceLine2.JI_RN_NKCountryOfExport = "";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Two invoice lines attached to different Entries. One of them is not empty", () =>
			{
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCheckJI_RN_NKCountryOfExport_MustBeEntered_WhenNonUCC6()
	{
		var countryOfExportInfo = invoiceLine.JI_RN_NKCountryOfExportInfo;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("EXP UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertHasMessageErrorContaining(countryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("EXP NON UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.JI_RN_NKCountryOfExport = "ES";
				invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
				AssertNoMessageErrorContaining(countryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction entryInstruction;

	const string CountryOfExportExpectedMessageError = "Country/Region of Export must be declared at header or line level.";

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
