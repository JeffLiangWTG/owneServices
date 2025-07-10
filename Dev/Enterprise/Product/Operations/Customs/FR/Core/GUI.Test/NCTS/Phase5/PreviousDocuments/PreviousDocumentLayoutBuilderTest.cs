using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;
using NctsPreviousDocument = Enterprise.Customs.FR.Business.NCTS.NctsPreviousDocument;
using UniversalReferenceConstants = Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.GUI.Testing
{
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

			var layout = ((IPanelLayoutProvider)new Phase5GoodsItemPreviousDocumentLayoutWithGrid()).Layout;
			AssertEquals("ReferenceNumberCodeFindBox: should not be visible when CSI_Code is empty.", false, layout.IsVisible(PreviousDocumentControlBag.Instance.ReferenceNumberCodeFindBox, previousDocument));
			AssertEquals("ReferenceNumberTextBox: should be visible when CSI_Code is empty.", true, layout.IsVisible(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, previousDocument));

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("ReferenceNumberCodeFindBox: should be visible when CSI_Code is 'N337'.", true, layout.IsVisible(PreviousDocumentControlBag.Instance.ReferenceNumberCodeFindBox, previousDocument));
			AssertEquals("ReferenceNumberTextBox: should not be visible when CSI_Code is 'N337'.", false, layout.IsVisible(EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, previousDocument));
		}

		protected override int ExpectedMaxColumns => 1;

		protected override PreviousDocumentLayoutBuilder<NctsPreviousDocument> GetColumnLayoutBuilderForTesting() => new PreviousDocumentLayoutBuilder<NctsPreviousDocument>();
	}
}
