using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(UCC5TemporaryStorageLayoutBuilder))]
	sealed class UCC5TemporaryStorageLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UCC5TemporaryStorageLayoutBuilder, TemporaryStorageHeader, EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag>
	{
		public void TestArrivalTransportMeansCodeTextBoxCaption()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new UCC5TemporaryStorageLayout()).Layout;
			var controlReference = EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox;

			header.TransportType = "40";
			layout.TryGetCaption(controlReference, header, out var resourceStringData);
			CombineAssertions("When TransportType is 40", () =>
			{
				AssertEquals("Caption", "Flight Number", resourceStringData.Caption);
				AssertEquals("FullDescription", "[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);
			});

			header.TransportType = "11";
			layout.TryGetCaption(controlReference, header, out resourceStringData);
			CombineAssertions("When TransportType is 11", () =>
			{
				AssertEquals("Caption", "Vessel Name", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);
			});

			header.TransportType = "81";
			layout.TryGetCaption(controlReference, header, out resourceStringData);
			CombineAssertions("When TransportType is 81", () =>
			{
				AssertEquals("Caption", "Vessel Name", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);
			});
		}

		protected override UCC5TemporaryStorageLayoutBuilder GetColumnLayoutBuilderForTesting() => new UCC5TemporaryStorageLayoutBuilder();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
