using Enterprise.Core.Forms;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentsGridColumnsBag))]
sealed class SupportingDocumentsGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = SupportingDocumentsGridColumnsBag.Instance;
		AssertColumn<ZTextBoxColumnStyleInfo>(controlBag.LineNoTextBoxColumn, SupportingDocument.Schema.CSI_LineNo, 50);
		AssertColumn<ZTextBoxColumnStyleInfo>(controlBag.ImageReferenceNumberTextBoxColumn, SupportingDocument.Schema.CSI_ReferenceNumber2, 100);
		AssertColumn<ZCodeFindBoxColumnStyleInfo>(controlBag.DocumentTypeCodeFindBoxColumn, SupportingDocument.Schema.CSI_Code, 100);
		AssertColumn<ZGuidFindBoxColumnStyleInfo>(controlBag.OrganisationGuidFindBoxColumn, SupportingDocument.Schema.OrganizationPK, 100);
		AssertColumn<ZDropEditColumnStyleInfo>(controlBag.CodeDropEditColumn, SupportingDocument.Schema.CSI_IssuerType, 100);
	}

	void AssertColumn<T>(IGridColumnReference column, string columnName, int width) where T : ZGridColumnInfo
	{
		var columnInfo = column.CreateGridColumnInfo() as T;
		AssertNotNull(columnInfo);
		AssertEquals(columnName, columnInfo.ColumnName);
		AssertEquals(width, columnInfo.Width);
	}
}
