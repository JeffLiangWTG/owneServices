using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuMenuPivotBaseValidationTest : TestCaseWithFactory
	{
		public void TestValidateSF_Filter_ShouldValidateUsingExpressionEvaluator()
		{
			const string stringExpressionFormat = "The following expression {0} is incorrect. Please make sure:\r\n\u2022 you are using a True/False expression\r\n\u2022 you are not mixing legacy filters(e.g.CTY = AU) with other filters(e.g. \"<PropertyName>\" == \"My value\")\r\n\u2022 if you use a legacy filter, they cannot be combined.";
			const string filterExpressionMissingMacroError = "Filter must contain macros";
			string filterExpressionFormatError;

			var pivot = Factory.New<StmMenuMenuPivotBase>();
			AssertEquals("HasErrors", false, pivot.SF_FilterInfo.HasErrors());

			pivot.SF_Filter = "HBL=ABC && \"<BowTies>\" == \"Cool\"";
			filterExpressionFormatError = string.Format(stringExpressionFormat, pivot.SF_Filter);
			AssertHasError(pivot.SF_FilterInfo, filterExpressionFormatError);

			pivot.SF_Filter = "<BowTies> == Cool";
			filterExpressionFormatError = string.Format(stringExpressionFormat, pivot.SF_Filter);
			AssertHasError(pivot.SF_FilterInfo, filterExpressionFormatError);

			pivot.SF_Filter = "\"BowTies\" == \"Cool\"";
			AssertHasError(pivot.SF_FilterInfo, filterExpressionMissingMacroError);

			pivot.SF_Filter = "\"<BowTies>\" == \"Cool\"";
			AssertNoErrors(pivot.SF_FilterInfo);
		}

		public void TestValidateSF_SU_Inward()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItem>();
			var menu2 = Factory.NewWithValidTestData<StmMenuItem>();

			var pivot = Factory.New<StmMenuMenuPivotBase>();
			pivot.SF_SU_Outward = menu2.PK;
			pivot.SF_OverriddenBusinessContext = "";
			pivot.SF_SU_Inward = menu.PK;

			AssertNoError(pivot.SF_SU_InwardInfo, "Each document can only be used once");

			var pivotFail = Factory.New<StmMenuMenuPivotBase>();
			pivotFail.SF_SU_Outward = menu2.PK;
			pivotFail.SF_OverriddenBusinessContext = "";
			pivotFail.SF_SU_Inward = menu.PK;

			AssertHasError(pivotFail.SF_SU_InwardInfo, "Each document can only be used once");
		}
	}
}
