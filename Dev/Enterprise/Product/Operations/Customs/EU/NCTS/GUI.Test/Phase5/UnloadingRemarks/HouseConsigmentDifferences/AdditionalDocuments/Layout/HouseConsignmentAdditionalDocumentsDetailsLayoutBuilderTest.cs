using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument>))]
	class HouseConsignmentAdditionalDocumentsDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument>, NctsBillAdditionalDocument, HouseConsignmentAdditionalDocumentsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument> GetColumnLayoutBuilderForTesting() => new HouseConsignmentAdditionalDocumentsLayoutBuilder<NctsBillAdditionalDocument>();

		public void TestReferenceNumberVisibility()
		{
			CombineAssertions(() =>
			{
				supportingInfo.CSI_SubType = "REF";
				AssertEquals("ReferenceNumberTextBox Visible, if Unloaded State = REF", true, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.ReferenceNumberTextBox, supportingInfo));

				supportingInfo.CSI_SubType = "INF";
				AssertEquals("ReferenceNumberTextBox Visible, if Unloaded State = INF", false, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.ReferenceNumberTextBox, supportingInfo));

				supportingInfo.CSI_SubType = "TRA";
				AssertEquals("ReferenceNumberTextBox Visible, if Unloaded State = TRA", true, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.ReferenceNumberTextBox, supportingInfo));
			});
		}

		public void TestTextBoxVisibility()
		{
			CombineAssertions(() =>
			{
				supportingInfo.CSI_SubType = "REF";
				AssertEquals("TextBox Visible, if Unloaded State = REF", false, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.TextTextBox, supportingInfo));

				supportingInfo.CSI_SubType = "INF";
				AssertEquals("TextBox Visible, if Unloaded State = INF", true, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.TextTextBox, supportingInfo));

				supportingInfo.CSI_SubType = "TRA";
				AssertEquals("TextBox Visible, if Unloaded State = TRA", false, Layout.IsVisible(HouseConsignmentAdditionalDocumentsControlBag.Instance.TextTextBox, supportingInfo));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			supportingInfo = bill.AdditionalDocuments.AddNew();
		}
		NctsBillAdditionalDocument supportingInfo;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new HouseConsignmentAdditionalDocumentsLayout()).Layout);
		PanelLayout layout;
	}
}
