using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentCombinationsProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When previousDocument is null", () => new PreviousDocumentCombinationsProvider(previousDocument: null));
	}

	public void TestGetAllowedPreviousDocumentCombinationsForImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertEquals("Number of Combinations", 37, previousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations().Count);
	}

	public void TestGetAllowedPreviousDocumentCombinationsForNonImport()
	{
		declaration.JE_MessageType = "EXP";
		AssertEquals("Number of Combinations", 58, previousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations().Count);
	}

	public void TestGetAllowedPreviousDocumentCombinationsFilteredByProcedure()
	{
		declaration.JE_MessageType = "IMP";
		previousDocument.CSI_Procedure = "MRN";
		AssertEquals("Number of Combinations", 3, previousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations().Count);
	}

	public void TestCombinationsAreCached()
	{
		declaration.JE_MessageType = "IMP";

		var combinations1 = previousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations();
		var combinations2 = previousDocumentCombinationsProvider.GetAllowedPreviousDocumentCombinations();
		AssertSame("Cached", combinations1, combinations2);
	}

	public void TestGetNewSettingsForImport()
	{
		declaration.JE_MessageType = "IMP";
		AssertType<ImportPreviousDocumentFieldsInfo>("Type", previousDocumentCombinationsProvider.GetNewSettings());
	}

	public void TestGetNewSettingsForExport()
	{
		declaration.JE_MessageType = "EXP";
		AssertType<PreviousDocumentFieldsInfo>("Type", previousDocumentCombinationsProvider.GetNewSettings());
	}

	public void TestGetNewSettingsForUcc6Export()
	{
		declaration.JE_MessageType = "EXP";
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertType<Ucc6ExportPreviousDocumentFieldsInfo>("Type", previousDocumentCombinationsProvider.GetNewSettings());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration
			.CustomsEntryInstructions.AddNew()
			.PreviousDocuments.AddNew();
		previousDocumentCombinationsProvider = new PreviousDocumentCombinationsProvider(previousDocument);
	}

	JobDeclaration declaration;
	PreviousDocument previousDocument;
	IPreviousDocumentCombinationsProvider previousDocumentCombinationsProvider;
}
