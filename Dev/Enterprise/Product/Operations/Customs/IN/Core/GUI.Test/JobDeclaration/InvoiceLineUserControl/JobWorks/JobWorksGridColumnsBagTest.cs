using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobWorksGridColumnsBag))]
sealed class JobWorksGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = JobWorksGridColumnsBag.Instance;
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.LineNoCalcEditColumn, JobWork.Schema.CSI_LineNo, 50, info => { AssertEquals("IsMandatory", true, info.IsMandatory); });
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.ReferenceNumberTextBoxColumn, JobWork.Schema.CSI_ReferenceNumber, 100, info => { AssertEquals("IsMandatory", true, info.IsMandatory); });
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.DateOfIssueDateEditColumn, JobWork.Schema.CSI_DateOfIssue, 100, info =>
		{
			AssertEquals("IsMandatory", true, info.IsMandatory);
			AssertEquals("DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, info.DateTimeFormat);
		});
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.CustomsOfficeTextBoxColumn, JobWork.Schema.CSI_CustomsOffice, 100, info => { AssertEquals("IsMandatory", true, info.IsMandatory); });
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.ReferenceNumber2TextBoxColumn, JobWork.Schema.CSI_ReferenceNumber2, 100, info => { AssertEquals("IsMandatory", true, info.IsMandatory); });
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.ItemNumberCalcEditColumn, JobWork.Schema.CSI_ItemNumber, 100, info =>
		{
			AssertEquals("IsMandatory", true, info.IsMandatory);
			AssertEquals("MaxValue", 9999m, info.MaxValue);
		});
		LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.QuantityCalcEditColumn, JobWork.Schema.CSI_Quantity, 100, info =>
		{
			AssertEquals("IsMandatory", true, info.IsMandatory);
			AssertEquals("MaxValue", 99999999m, info.MaxValue);
		});
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.UnitOfQuantityTextBoxColumn, JobWork.Schema.CSI_UnitOfQuantity, 100, info => { AssertEquals("IsMandatory", true, info.IsMandatory); });
	}
}

