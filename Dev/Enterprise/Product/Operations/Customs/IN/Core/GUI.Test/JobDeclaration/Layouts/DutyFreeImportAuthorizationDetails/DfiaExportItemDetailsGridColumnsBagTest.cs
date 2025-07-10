using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DfiaExportItemDetailsGridColumnsBag))]
sealed class DfiaExportItemDetailsGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = DfiaExportItemDetailsGridColumnsBag.Instance;
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.SerialNoTextBoxColumn, DfiaExportItemDetail.Schema.CSI_LineNo, 50);
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.LicenseNoTextBoxColumn, DfiaExportItemDetail.Schema.CSI_ReferenceNumber, 175);
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.LicenseDateEditColumn, DfiaExportItemDetail.Schema.CSI_DateOfIssue, 80, info =>
		{
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, info.DateTimeFormat);
		});
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.LicenseExportItemSerialNoTextBoxColumn, DfiaExportItemDetail.Schema.CSI_ReferenceNumber2, 80);
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.LicenseExportQuantityCalcEditColumn, DfiaExportItemDetail.Schema.CSI_Quantity, 100, info =>
		{
			AssertEquals("MaxValue", 9999999999.999m, info.MaxValue);
		});
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.LicenseExportQuantityUnitDropEditColumn, DfiaExportItemDetail.Schema.CSI_UnitOfQuantity, 40);
	}
}
