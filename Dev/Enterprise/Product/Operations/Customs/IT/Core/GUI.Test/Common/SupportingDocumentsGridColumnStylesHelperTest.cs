using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class SupportingDocumentsGridColumnStylesHelperTest : TestCaseWithFactory
{
	public void TestGetColumnStylesIfDeclarationIsNull()
	{
		var columnStyles = columnStylesHelper.GetColumnStyles(declaration: null);
		AssertArrayEqualsByElements(expectedNonUcc6ColumnsStyles, columnStyles);
	}

	public void TestGetColumnStylesIfDeclarationIsImport()
	{
		declaration.JE_MessageType = "IMP";
		var columnStyles = columnStylesHelper.GetColumnStyles(declaration);
		AssertArrayEqualsByElements(expectedImportColumnStyles, columnStyles);
	}

	public void TestGetColumnStylesIfDeclarationIsExportUcc6()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			var columnStyles = columnStylesHelper.GetColumnStyles(declaration);
			AssertArrayEqualsByElements(expectedExportUcc6ColumnStyles, columnStyles);
		}
	}

	public void TestGetColumnStylesIfDeclarationIsNotImportNorExportUcc6()
	{
		declaration.JE_MessageType = "";
		var columnStyles = columnStylesHelper.GetColumnStyles(declaration);
		AssertArrayEqualsByElements(expectedNonUcc6ColumnsStyles, columnStyles);
	}

	public void TestGetColumnStylesForPartPivot()
	{
		var columnStyles = columnStylesHelper.GetColumnStylesForPartPivot();
		AssertArrayEqualsByElements(expectedPartPivotColumnStyles, columnStyles);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		columnStylesHelper = new SupportingDocumentsGridColumnStylesHelper();
	}

	JobDeclaration declaration;
	SupportingDocumentsGridColumnStylesHelper columnStylesHelper;

	readonly string[] expectedNonUcc6ColumnsStyles = new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_Status,
	};

	readonly string[] expectedImportColumnStyles = new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_DateOfExpiry,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency
	};

	readonly string[] expectedExportUcc6ColumnStyles = new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_DateOfExpiry,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_LineNo
	};

	readonly string[] expectedPartPivotColumnStyles = new string[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_YearOfIssue,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_DateOfExpiry,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_LineNo,
		SupportingDocument.Schema.CSI_Status,
	};
}
