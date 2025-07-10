using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ExportUcc6AdditionalProcedureCodeValidationTest : BusinessObjectLookupsTestCase
{
	public void TestAdditionalProcedure_WhenEnterACodeNotInList()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "222", "Two", "EXP", group: "IFD");
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

			additionalProcedureCode.CY_Code = "XXX";
			AssertHasMessageError(additionalProcedureCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			additionalProcedureCode.CY_Code = "1111222";
			AssertNoMessageErrorContaining(additionalProcedureCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}

	public void TestAdditionalProcedure_WhenIsInTransitionPeriod()
	{
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			const string expectedMessageError = "During the transition period, which is active now, Additional Procedures may not be provided.";

			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				additionalProcedureCode.CY_Code = "6020";
				AssertHasMessageErrorContaining("When In Transition Period", additionalProcedureCode.CY_CodeInfo, expectedMessageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				additionalProcedureCode.CY_Code = "6020";
				AssertNoMessageErrorContaining("When not in transition period", additionalProcedureCode.CY_CodeInfo, expectedMessageError);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);
}
