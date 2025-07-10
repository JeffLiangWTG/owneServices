using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DfiaImportItemDetailsGridColumnsBag))]
sealed class DfiaImportItemDetailsGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = DfiaImportItemDetailsGridColumnsBag.Instance;
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.SerialNoTextBoxColumn, DfiaImportItemDetail.Schema.CSI_LineNo, 50);
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.LicenseImportItemSerialNoTextBoxColumn, DfiaImportItemDetail.Schema.CSI_ReferenceNumber, 100);
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.LicenseImportQuantityCalcEditColumn, DfiaImportItemDetail.Schema.CSI_Quantity, 130, info =>
		{
			AssertEquals("MaxValue", 9999999999.999m, info.MaxValue);
		});
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.LicenseImportQuantityUnitDropEditColumn, DfiaImportItemDetail.Schema.CSI_UnitOfQuantity, 50);
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.ItemTypeDropEditColumn, DfiaImportItemDetail.Schema.CSI_IssuerType, 50);
	}
}
