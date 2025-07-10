using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(TransportDetailsLayoutBuilder))]
sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	public void TestJE_IATALoadPortCodeFindBox_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Sea", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.IATALoadPortCodeFindBox, declaration));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Air", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.IATALoadPortCodeFindBox, declaration));
		});
	}

	public void TestTransportInlandSeparatorUserControlVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			declaration.JE_TransportModeInland = TransportModes.Mail;
			AssertEquals($"EXP - InlandTransportMode: MAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.FixedTransportInstallations;
			AssertEquals($"EXP - InlandTransportMode: FIX", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Road;
			AssertEquals($"EXP - InlandTransportMode: ROA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.InlandWaterwayTransport;
			AssertEquals($"EXP - InlandTransportMode: IWT", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Rail;
			AssertEquals($"EXP - InlandTransportMode: RAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Sea;
			AssertEquals($"EXP - InlandTransportMode: SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.OwnPropulsion;
			AssertEquals($"EXP - InlandTransportMode: OWN", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Air;
			AssertEquals($"EXP - InlandTransportMode: AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			declaration.JE_TransportModeInland = TransportModes.Mail;
			AssertEquals($"IMP - InlandTransportMode: MAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.FixedTransportInstallations;
			AssertEquals($"IMP - InlandTransportMode: FIX", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Road;
			AssertEquals($"IMP - InlandTransportMode: ROA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.InlandWaterwayTransport;
			AssertEquals($"IMP - InlandTransportMode: IWT", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Rail;
			AssertEquals($"IMP - InlandTransportMode: RAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Sea;
			AssertEquals($"IMP - InlandTransportMode: SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.OwnPropulsion;
			AssertEquals($"IMP - InlandTransportMode: OWN", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Air;
			AssertEquals($"IMP - InlandTransportMode: AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
		});
	}

	public void TestTransportDetailsPortOfLoadingWithIATAUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfLoadingFindBox"], "Load Port", "Load Port", "");
			AssertCaptions(captionData["ExportDateEdit"], "Dep.", "Departure", "");
			AssertCaptions(captionData["IATALoadPortCodeFindBox"], "", "IATA", "");
		});
	}

	public void TestPortOfDischargeUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfDischargeFindBox"], "Discharge", "Discharge Port", "");
		});
	}

	public void TestVoyageAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["VoyageNumberTextBox"], "Voy.", "[UCC 7/7] Voyage", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");
		});
	}

	public void TestPortOfLoadingUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration, out var captionData));
			AssertCaptions(captionData["PortOfLoadingFindBox"], "Load Port", "Load Port", "");
			AssertCaptions(captionData["ExportDateEdit"], "Dep.", "Departure", "");
		});
	}

	public void TestTransportIDAndNationalityUserControlResourceStringData()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration, out var captionData));
			AssertCaptions(captionData["TransportIDTextBox"], "", "Transport ID", "[UCC 7/7] Transport ID");
		});
	}

	public void TestOverrideValuesCheckBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_JS = ZGuid.Empty;
			AssertEquals("When declaration is standalone", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));

			declaration.JE_JS = ZGuid.NewZGuid();
			AssertEquals("When declaration is linked to a shipment", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
		});
	}

	public void TestMasterBillTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not AIR", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));

			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Transport Mode is AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
		});
	}

	public void TestOceanBillTextBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = "";
			AssertEquals("When Transport Mode is not SEA", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));

			declaration.JE_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
		});
	}

	public void TestVesselCodeFindBoxVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = ''", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = ''", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
		});
	}

	public void TestFlightAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = ''", expected: false, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = SEA", expected: false, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = AIR", expected: true, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = ''", expected: false, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = SEA", expected: false, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = true, JE_MessageType = EXP, JE_TransportMode = AIR", expected: true, Layout.IsVisible(TransportDetailsUserControlBag.Instance.FlightAndNationalityUserControl, declaration));
		});
	}

	public void TestVoyageAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = ''", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = SEA", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = ''", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = true, JE_MessageType = EXP, JE_TransportMode = SEA", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
		});
	}

	public void TestTransportIDAndNationalityUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = SEA", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = RAI", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = ''", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = EXP, JE_TransportMode = SEA", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("IsVisible = true, JE_MessageType = EXP, JE_TransportMode = RAI", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = true, JE_MessageType = EXP, JE_TransportMode = ''", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
		});
	}

	public void TestPortOfLoadingUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is IMP and Transport Mode is AIR", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));

			declaration.JE_TransportMode = "";
			AssertEquals("When Message Type is IMP and Transport Mode is not AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));

			declaration.JE_MessageType = "";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is not IMP and Transport Mode is AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
		});
	}

	public void TestTransportDetailsPortOfLoadingWithIATAUserControlVisibility()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is IMP and Transport Mode is AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));

			declaration.JE_TransportMode = "";
			AssertEquals("When Message Type is IMP and Transport Mode is not AIR", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));

			declaration.JE_MessageType = "";
			declaration.JE_TransportMode = "AIR";
			AssertEquals("When Message Type is not IMP and Transport Mode is AIR", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));
		});
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int ExpectedMaxColumns => 1;

	protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new TransportDetailsLayoutBuilder();
		builder.AddControlBag(EU.GUI.TransportDetailsControlBag.Instance);
		return builder;
	}

	static void AssertCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedCaption, string expectedFullDescription)
	{
		AssertEquals("CaptionData ShortCaption", expectedShortCaption, captionData.ShortCaption);
		AssertEquals("CaptionData Caption", expectedCaption, captionData.Caption);
		AssertEquals("CaptionData FullDescription", expectedFullDescription, captionData.FullDescription);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
	PanelLayout layout;
}
