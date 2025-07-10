using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder))]
sealed class Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder, NctsPreviousDocument, EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag>
{
	public void TestSetReferenceTextBoxCaption()
	{
		CombineAssertions("Reference2TextBox", () =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.Reference2TextBox, previousDocument, out var captionData));
			AssertCaptions(captionData, "", "Complement of Information", "[12 01 002 000] Complement of Information");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeader();

		previousDocument = nctsHeader
			.MovementHeader.GoodsItems.AddNew()
			.PreviousDocuments.AddNew();
		layout = new Phase5GoodsItemPreviousDocumentLayoutWithGrid().Layout;
	}

	NctsPreviousDocument previousDocument;
	PanelLayout layout;

	protected override Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		return new Phase5GoodsItemPreviousDocumentsFieldsLayoutBuilder();
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	void AssertCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedCaption, string expectedFullDescription)
	{
		AssertEquals("CaptionData ShortCaption", expectedShortCaption, captionData.ShortCaption);
		AssertEquals("CaptionData Caption", expectedCaption, captionData.Caption);
		AssertEquals("CaptionData FullDescription", expectedFullDescription, captionData.FullDescription);
	}
}
