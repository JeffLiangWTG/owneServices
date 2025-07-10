using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class OrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentsTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<OrgSupplierPartFormCustomsControl>("previousDocsTabPage", "PreviousDocumentsUserControl", "Previous Documents", typeof(PreviousDocumentsUserControl));
		}

		public void TestAdditionalInfosTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<OrgSupplierPartFormCustomsControl>("additionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
		}

		public void TestSupportingDocumentsTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<OrgSupplierPartFormCustomsControl>("supportingDocsTabPage", "SupportingDocumentsUserControl", "Supporting Docs", typeof(EU.GUI.PlugIn.PartPivotLayoutSupportingDocumentsUserControl));
		}
	}
}
