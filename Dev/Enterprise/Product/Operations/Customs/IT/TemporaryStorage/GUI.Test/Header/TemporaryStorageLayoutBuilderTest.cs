using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TemporaryStorageLayoutBuilder))]
sealed class TemporaryStorageLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TemporaryStorageLayoutBuilder<TemporaryStorageHeader>, TemporaryStorageHeader, EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag>
{
	protected override TemporaryStorageLayoutBuilder<TemporaryStorageHeader> GetColumnLayoutBuilderForTesting() => new TemporaryStorageLayoutBuilder();

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

	public void TestArrivalTransportMeansCodeTextBoxCaption()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageLayout()).Layout;

		layout.TryGetCaption(EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out var resourceStringData);
		CombineAssertions(() =>
		{
			AssertNotNull(resourceStringData);
			AssertEquals("Caption", "Transport ID", resourceStringData.Caption);
		});
	}
}
