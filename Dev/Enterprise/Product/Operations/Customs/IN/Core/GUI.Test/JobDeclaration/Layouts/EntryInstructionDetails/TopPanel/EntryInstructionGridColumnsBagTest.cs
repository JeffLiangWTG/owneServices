using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionGridColumnsBag))]
sealed class EntryInstructionGridColumnsBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = EntryInstructionGridColumnsBag.Instance;

		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.LocalReferenceNumberTextBoxColumn, CusEntryInstruction.Schema.LocalReferenceNumber, 80);
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.LocalReferenceNumberDateEditColumn, CusEntryInstruction.Schema.LocalReferenceNumberDate, 80);
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.CEI_StyleDropEditColumn, CusEntryInstruction.Schema.CEI_Style, 80);
		LayoutTestHelper.AssertGridColumn<ZDropEditColumnStyleInfo>(controlBag.CEI_SubStyleDropEditColumn, CusEntryInstruction.Schema.CEI_SubStyle, 80);
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.CEI_DescriptionTextBoxColumn, CusEntryInstruction.Schema.CEI_Description, 250);
		LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.ShippingBillNumberTextBoxColumn, CusEntryInstruction.Schema.ShippingBillNumber, 80);
		LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.ShippingBillDateDateEditColumn, CusEntryInstruction.Schema.ShippingBillDate, 80);
	}
}
