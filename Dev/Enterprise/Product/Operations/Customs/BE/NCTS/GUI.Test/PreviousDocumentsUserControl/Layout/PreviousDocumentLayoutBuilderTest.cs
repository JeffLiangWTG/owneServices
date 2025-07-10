using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.BE.NCTS.Business.NctsHeader;
using NctsPreviousDocument = Enterprise.Customs.BE.NCTS.Business.NctsPreviousDocument;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

[TestedType(typeof(PreviousDocumentLayoutBuilder<NctsPreviousDocument>))]
sealed class PreviousDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<PreviousDocumentLayoutBuilder<NctsPreviousDocument>, NctsPreviousDocument, PreviousDocumentControlBag>
{
	public void TestSetDefaultVisibilities()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = header.Bills.AddNew();
		var goodItem = bill.GoodsItems.AddNew();
		var previousDocument = goodItem.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			var layout = ((IPanelLayoutProvider)new Phase5GoodsItemPreviousDocumentLayoutWithGrid()).Layout;
			AssertEquals("Specific user control: CSI_code is empty", false, layout.IsVisible(PreviousDocumentControlBag.Instance.ReferenceNumberN785UserControl, previousDocument));
			AssertEquals("Standard reference number: CSI_code is empty", true, layout.IsVisible(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, previousDocument));

			previousDocument.CSI_Code = Constants.PreviousDocumentTypes.CargoManifest;
			AssertEquals("Specific user control: CSI_Code is 'N785'", true, layout.IsVisible(PreviousDocumentControlBag.Instance.ReferenceNumberN785UserControl, previousDocument));
			AssertEquals("Standard reference number: CSI_code is 'N785'", false, layout.IsVisible(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, previousDocument));
		});
	}

	protected override int ExpectedMaxColumns => 1;

	protected override PreviousDocumentLayoutBuilder<NctsPreviousDocument> GetColumnLayoutBuilderForTesting() => new PreviousDocumentLayoutBuilder<NctsPreviousDocument>();
}
