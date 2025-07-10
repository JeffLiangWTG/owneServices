using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayoutBuilder))]
sealed class EntryInstructionDetailsBasicUserControlLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsBasicUserControlLayoutBuilder, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
{
	protected override EntryInstructionDetailsBasicUserControlLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsBasicUserControlLayoutBuilder();

	public void TestLocationOfGoodsUserControl_Caption() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		Layout.TryGetCaptionData(Customs.EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction, out var captionData);
		AssertEquals("GoodLocationDescription Caption", "Location of Goods", captionData["LocationOfGoodsUserControl"].Caption);
		AssertEquals("GoodLocationDescription FullDescription", "[UCC 5/23] Location of Goods", captionData["LocationOfGoodsUserControl"].FullDescription);
	});

	protected override int ExpectedMaxColumns => 3;

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new EntryInstructionDetailsBasicUserControlLayout()).Layout);
	PanelLayout layout;
}
