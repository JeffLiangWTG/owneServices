using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsRulesProviderTest : TestCaseWithFactory
{
	public void TestIncotermAllowItalianAgreedPlaceCode()
	{
		Assert("Empty incoterm does not allow Agreed Place Code 1", CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode(""));
		Assert("FOB does not allow Agreed Place Code 1", !CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode("FOB"));
		Assert("EXW does not allow Agreed Place Code 1", !CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode("EXW"));
		Assert("FAS does not allow Agreed Place Code 1", !CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode("FAS"));
		Assert("FCA does not allow Agreed Place Code 1", !CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode("FCA"));
		Assert("CIF allow Agreed Place Code 1", CustomsRulesProvider.IncotermAllowItalianAgreedPlaceCode("CIF"));
	}

	public void TestIsSummaryDeclarationDocument()
	{
		var summaryDeclarationDocumentList = new string[] { "A3", "PF", "MRN", "NN", "CIM", "A44", "A45", "A6", "AWB", "BCTR", "LC", "LTA", "MTA", "T2F", "TIR", "TIEX", "T1", "T2" };
		CombineAssertions(() =>
		{
			foreach (var summaryDeclarationDocument in summaryDeclarationDocumentList)
			{
				Assert($"{summaryDeclarationDocument} is a summary declaration document", CustomsRulesProvider.IsSummaryDeclarationDocument(summaryDeclarationDocument));
			}
		});

		Assert("Empty is not a summary declaration document", !CustomsRulesProvider.IsSummaryDeclarationDocument(""));
		Assert("2 is not a summary declaration document", !CustomsRulesProvider.IsSummaryDeclarationDocument("2"));
	}

	public void TestSummaryDeclarationDocumentIsNotAlsoPreviousProcedureDocument()
	{
		foreach (var previousDocumentProcedureCode in new PreviousDocumentProcedureList().GetAllCodes())
		{
			var isSummaryDeclarationDocument = CustomsRulesProvider.IsSummaryDeclarationDocument(previousDocumentProcedureCode);
			if (isSummaryDeclarationDocument)
			{
				AssertEquals("When a register code is a Summary Declaration Document, IsPreviousProcedureDocument()", !isSummaryDeclarationDocument, CustomsRulesProvider.IsPreviousProcedureDocument(previousDocumentProcedureCode));
			}
		}
	}

	public void TestIsPreviousProcedureDocument()
	{
		var previousProcedureDocumentList = new string[] { "2", "2S", "2T", "5", "5S", "5T", "7", "7S", "7T", "1", "1S", "1T" };
		CombineAssertions(() =>
		{
			foreach (var previousProcedureDocument in previousProcedureDocumentList)
			{
				Assert($"{previousProcedureDocument} is a previous procedure document", CustomsRulesProvider.IsPreviousProcedureDocument(previousProcedureDocument));
			}
		});

		Assert("Empty is not a previous procedure document", !CustomsRulesProvider.IsPreviousProcedureDocument(""));
		Assert("MRN is not a previous procedure document", !CustomsRulesProvider.IsPreviousProcedureDocument("MRN"));
	}

	public void TestPreviousProcedureDocumentIsNotAlsoSummaryDeclarationDocument()
	{
		foreach (var previousDocumentProcedureCode in new PreviousDocumentProcedureList().GetAllCodes())
		{
			var isPreviousProcedureDocument = CustomsRulesProvider.IsPreviousProcedureDocument(previousDocumentProcedureCode);
			if (isPreviousProcedureDocument)
			{
				AssertEquals("When a register code is a Previous Procedure Document, IsSummaryDeclarationDocument()", !isPreviousProcedureDocument, CustomsRulesProvider.IsSummaryDeclarationDocument(previousDocumentProcedureCode));
			}
		}
	}

	public void TestConvertContainerModeFromCargoWiseToIT()
	{
		AssertEquals("When containerMode is empty", false, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(ZString.Empty));
		AssertEquals("When containerMode is FCL", true, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(Core.Constants.ContainerModes.FCL));
		AssertEquals("When containerMode is LCL", true, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(Core.Constants.ContainerModes.LCL));
		AssertEquals("When containerMode is ULD", true, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(Core.Constants.ContainerModes.ULD));
		AssertEquals("When containerMode is Containerised", true, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(Core.Constants.ContainerModes.Containerised));
		AssertEquals("When containerMode is Loose", false, CustomsRulesProvider.ConvertContainerModeFromCargoWiseToIT(Core.Constants.ContainerModes.Loose));
	}

	public void TestConvertRepresentativeTypeToItalianCustomsFormat()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Self", "1", CustomsRulesProvider.ConvertRepresentativeTypeToItalianCustomsFormat("SEL"));
			AssertEquals("Indirect", "3", CustomsRulesProvider.ConvertRepresentativeTypeToItalianCustomsFormat("IND"));
			AssertEquals("Invalid", "", CustomsRulesProvider.ConvertRepresentativeTypeToItalianCustomsFormat("XXX"));
		});
	}

	public void TestGetRegistrationCodeFromUcc6Mrn()
	{
		AssertEquals(nameof(CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn), ZString.Empty, CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn(ZString.Empty));
		AssertEquals(nameof(CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn), ZString.Empty, CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn("ABC"));
		AssertEquals(nameof(CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn), "4 T-D591055", CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn("22ITQTG4TD591055R8"));
		AssertEquals(nameof(CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn), "4 -CE98155", CustomsRulesProvider.GetRegistrationCodeFromUcc6Mrn("22ITQXT04CE98155R2"));
	}
}
