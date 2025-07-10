using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ExportJobComInvoiceLineCountryDestinationValidationTest : TestCaseWithFactory
{
	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_WhenBothHeaderAndLineAreEmpty()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				declaration.JE_GoodsDestination = "DE";
				invoiceLine.ZG_CountryOfDestination = "";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);

				declaration.JE_GoodsDestination = "";
				invoiceLine.ZG_CountryOfDestination = "DE";
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);

				declaration.JE_GoodsDestination = "";
				invoiceLine.ZG_CountryOfDestination = "";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_WhenNoEntryInstructionIsLinked()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);

				invoiceLine.JI_CEI = ZGuid.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_ForMultipleEntryInstruction()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			CombineAssertions("Two invoice lines attached to the same Entry. One of them is not empty", () =>
			{
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.ZG_CountryOfDestination = "ES";
				declaration.JE_GoodsDestination = "";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});

			CombineAssertions("Two invoice lines attached to different Entries. One of them is not empty", () =>
			{
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_WhenIsSecurityDeclaration()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);

				declaration.ZG_IsSecurityDeclaration = true;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_WhenSubStyleBDeclaration()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("When EXP UCC6", () =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);

				entryInstruction.CEI_SubStyle = "B";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeDeclareAtHeaderOrLineLevel_ForNonUCC6()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, CountryOfDestinationExpectedMessageError);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeEntered()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			CombineAssertions("Two invoice lines attached to the same Entry. Both are empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "ES";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Two invoice lines attached to different Entries. One of them is not empty", () =>
			{
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeEntered_WhenNonUCC6()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("EXP UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "ES";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			CombineAssertions("EXP NON UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "ES";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeEntered_WhenSecurityDeclaration()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("EXP UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "ES";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_IsSecurityDeclaration = true;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestZG_CountryDestination_MustBeEntered_WhenSubStyleBDeclaration()
	{
		var countryOfDestinationInfo = invoiceLine.ZG_CountryOfDestinationInfo;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			CombineAssertions("EXP UCC6, Two invoice lines attached to the same Entry. One of them are not empty", () =>
			{
				invoiceLine2.ZG_CountryOfDestination = "ES";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertHasMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = "B";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfDestination();
				AssertNoMessageErrorContaining(countryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
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

	const string CountryOfDestinationExpectedMessageError = "Country of Destination must be declared at header or line level.";

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
