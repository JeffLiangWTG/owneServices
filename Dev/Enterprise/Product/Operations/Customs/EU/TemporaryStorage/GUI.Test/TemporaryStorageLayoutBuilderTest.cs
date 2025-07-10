using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageLayoutBuilder<TemporaryStorageHeader>))]
	sealed class TemporaryStorageLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TemporaryStorageLayoutBuilder<TemporaryStorageHeader>, TemporaryStorageHeader, TemporaryStorageUserControlBag>
	{
		protected override TemporaryStorageLayoutBuilder<TemporaryStorageHeader> GetColumnLayoutBuilderForTesting() => new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		public void TestArrivalTransportMeansCodeTextBoxCaption()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageLayout()).Layout;

			CombineAssertions(() =>
			{
				header.TransportType = "10";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out var resourceStringData);
				AssertEquals("TransportType = 10", "IMO Number", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "20";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 20", "Wagon Number", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "21";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 21", "Train Number", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "31";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 31", "Road Trailer Reg. No.", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "30";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 30", "Road Vehicle Reg. No.", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "41";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 41", "Aircraft Reg. No.", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "80";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 80", "ENI Code", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "49";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType = 49", "Arrival Transport Means", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);

				header.TransportType = "";
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, header, out resourceStringData);
				AssertEquals("TransportType is empty", "Arrival Transport Means", resourceStringData.Caption);
				AssertEquals("[19 06 017 000] Arrival Transport Means > Identification Number", resourceStringData.FullDescription);
			});
		}

		public void TestPersonPresentingTheGoodsAddressControl_Caption()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageLayout()).Layout;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;

			CombineAssertions(() =>
			{
				layout.TryGetCaption(TemporaryStorageUserControlBag.Instance.PersonPresentingTheGoodsAddressControl, header, out var resourceStringData);
				AssertEquals("Full Description PersonPresentingTheGoodsAddressControl", "Person notifying the arrival after movement", resourceStringData.FullDescription);
				AssertEquals("Medium Caption PersonPresentingTheGoodsAddressControl", "Person notifying the arrival", resourceStringData.MediumCaption);
				AssertEquals("Short Caption PersonPresentingTheGoodsAddressControl", "Person notifying the arrival", resourceStringData.ShortCaption);
			});
		}
	}
}
