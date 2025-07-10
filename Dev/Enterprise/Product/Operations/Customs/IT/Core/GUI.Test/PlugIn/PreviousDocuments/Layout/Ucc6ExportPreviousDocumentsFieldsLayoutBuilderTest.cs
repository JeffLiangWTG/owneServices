using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument>))]
sealed class Ucc6ExportPreviousDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument>, PreviousDocument, EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag>
{
	public void TestSetCodeDropEditCaption()
	{
		CombineAssertions("CodeDropEdit", () =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.CodeDropEdit, previousDocument, out var captionData));
			AssertCaptions(captionData, "", "Type", "[12 01 002 000] Type");
		});
	}

	public void TestSetReferenceTextBoxCaption()
	{
		CombineAssertions("ReferenceTextBox", () =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.ReferenceTextBox, previousDocument, out var captionData));
			AssertCaptions(captionData, "", "Reference Number", "[12 01 001 000] Reference Number");
		});
	}

	public void TestSetPackageQuantityCalcDropEditCaption()
	{
		CombineAssertions("PackageQuantityCalcDropEdit", () =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaption(EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance.PackageQuantityCalcDropEdit, previousDocument, out var captionData));
			AssertCaptions(captionData, "Package No.", "Number of Packages", "[12 01 004 000] Number of Packages");
		});
	}

	public void TestSetItemNumberCalcEditCaption()
	{
		CombineAssertions("ItemNumberCalcEdit", () =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaption(PreviousDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, previousDocument, out var captionData));
			AssertCaptions(captionData, "Item No.", "Goods Item Identifier", "[12 01 007 000] Goods Item Identifier");
		});
	}

	protected override Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument> GetColumnLayoutBuilderForTesting() => new Ucc6ExportPreviousDocumentsFieldsLayoutBuilder<PreviousDocument>();

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew()
			.PreviousDocuments.AddNew();
		layout = new Ucc6ExportPreviousDocumentFieldsLayout().Layout;
	}

	void AssertCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedCaption, string expectedFullDescription)
	{
		AssertEquals("CaptionData ShortCaption", expectedShortCaption, captionData.ShortCaption);
		AssertEquals("CaptionData Caption", expectedCaption, captionData.Caption);
		AssertEquals("CaptionData FullDescription", expectedFullDescription, captionData.FullDescription);
	}

	PreviousDocument previousDocument;
	PanelLayout layout;
}
