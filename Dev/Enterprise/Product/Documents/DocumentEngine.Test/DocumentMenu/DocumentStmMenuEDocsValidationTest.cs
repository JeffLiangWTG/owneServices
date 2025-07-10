using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class DocumentStmMenuEDocsValidationTest : TestCaseWithFactory
	{
		public void TestValidateSX_Filter()
		{
			const string stringExpressionFormat = "The following expression {0} is incorrect. Please make sure:\r\n\u2022 you are using a True/False expression\r\n\u2022 you are not mixing legacy filters(e.g.CTY = AU) with other filters(e.g. \"<PropertyName>\" == \"My value\")\r\n\u2022 if you use a legacy filter, they cannot be combined.";
			const string filterExpressionMissingMacroError = "Filter must contain macros";

			var eDocs = Factory.New<DocumentStmMenuEDocs>();
			AssertEquals("HasErrors", false, eDocs.SX_FilterInfo.HasErrors());

			eDocs.SX_Filter = "HBL=ABC && \"<BowTies>\" == \"Cool\"";
			var filterExpressionFormatError = string.Format(stringExpressionFormat, eDocs.SX_Filter);
			AssertHasError(eDocs.SX_FilterInfo, filterExpressionFormatError);

			eDocs.SX_Filter = "<BowTies> == Cool";
			filterExpressionFormatError = string.Format(stringExpressionFormat, eDocs.SX_Filter);
			AssertHasError(eDocs.SX_FilterInfo, filterExpressionFormatError);

			eDocs.SX_Filter = "\"BowTies\" == \"Cool\"";
			AssertHasError(eDocs.SX_FilterInfo, filterExpressionMissingMacroError);

			eDocs.SX_Filter = "\"<BowTies>\" == \"Cool\"";
			AssertNoErrors(eDocs.SX_FilterInfo);
		}
	}
}
