using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(TransportDetailsLayoutBuilder))]
sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	public void TestFlightAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["FlightNumberTextBox"], "Flt.", "Flight", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");
			AssertCaptions(captionData["TransportNationalityFindBox"], "", "[21] Nationality", "");
		});
	}

	public void TestTransportDetailsPortOfLoadingWithIATAUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfLoadingFindBox"], "Load Port", "Load Port", "");
			AssertCaptions(captionData["ExportDateEdit"], "Dep.", "Departure", "");
			AssertCaptions(captionData["IATALoadPortCodeFindBox"], "", "IATA", "");
		});
	}

	public void TestPortOfDischargeUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfDischargeFindBox"], "Discharge", "Discharge Port", "");
		});
	}

	public void TestOceanBillTextBoxResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration, out var captionData));
			AssertCaptions(captionData["OceanBillTextBox"], "", "Ocean Bill", "Master Bill of the consignment");
		});
	}

	public void TestVoyageAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["VoyageNumberTextBox"], "Voy.", "Voyage", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");
		});
	}

	public void TestPortOfLoadingUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfLoadingFindBox"], "Load Port", "Load Port", "");
			AssertCaptions(captionData["ExportDateEdit"], "Dep.", "Departure", "");
		});
	}

	public void TestTransportIDAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["TransportIDTextBox"], "", "[21] Transport ID", "");
		});
	}

	public void TestTransportInlandRailUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, declaration, out var captionData));
			AssertCaptions(captionData["TrainNationalityCodeFindBox"], "", "[18] Nationality", "");
			AssertCaptions(captionData["WagonNationalityCodeFindBox"], "", "[18] Nationality", "");
		});
	}

	public void TestTransportInlandIDAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["TransportIDTextBox"], "", "Trans. ID (Inland)", "");
			AssertCaptions(captionData["TransportNationalityFindBox"], "", "[18] Nationality", "");
		});
	}

	public void TestTransportNationalityCodeFindBoxResourceStringData()
	{
		AssertEquals("Are captions set?", true, layout.TryGetCaptionData(EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, declaration, out var captionData));
		AssertCaptions(captionData["TransportNationalityCodeFindBox"], "", "[21] Nationality", "");
	}

	protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new TransportDetailsLayoutBuilder();

	protected override int ExpectedMaxColumns => 2;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		layout = ((IPanelLayoutProvider)new TransportDetailsLayout()).Layout;
	}

	JobDeclaration declaration;
	PanelLayout layout;

	static void AssertCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedCaption, string expectedFullDescription)
	{
		AssertEquals("CaptionData ShortCaption", expectedShortCaption, captionData.ShortCaption);
		AssertEquals("CaptionData Caption", expectedCaption, captionData.Caption);
		AssertEquals("CaptionData FullDescription", expectedFullDescription, captionData.FullDescription);
	}
}
