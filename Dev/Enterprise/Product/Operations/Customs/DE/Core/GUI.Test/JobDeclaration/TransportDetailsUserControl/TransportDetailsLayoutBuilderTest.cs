using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayoutBuilder))]
	sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, TransportDetailsControlBag>
	{
		public void TestExportTransportInlandModeAndTypeOfIdUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 1, captionData.Count);
				AssertCaptions(captionData[TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit], "Trans. Mode", "[19 04 001 000] Inland Mode of Transport");
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_NameOfTheSeaGoingVessel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;

			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Vessel Name", "[19 05 017 000] Vessel Name");
				AssertCaptions(captionData[TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox], "Nationality", "[19 08 062 000] Nationality");
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_ImoShipIdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;

			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Lloyds No.", "[19 05 017 000] Lloyds Number");
				AssertCaptions(captionData[TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox], "Nationality", "[19 08 062 000] Nationality");
			});
		}

		public void TestExportTransportInlandRoadUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;

			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[TransportInlandRoadUserControl.ControlNames.TransportIDTextBox], "Registration No.", "[19 05 017 000] Vehicle Registration Number");
				AssertCaptions(captionData[TransportInlandRoadUserControl.ControlNames.TransportNationalityCodeFindBox], "Nationality", "[19 08 062 000] Nationality");
			});
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_Default()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				string.Empty,
				string.Empty,
				"Transport ID",
				string.Empty);
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_Air_IataFlightNumber()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.Air,
				TransportMeansList.Codes.IataFlightNumber,
				"Flight No.",
				"[19 05 017 000] Flight No.");
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_Air_RegistrationNumberOfTheAircraft()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.Air,
				TransportMeansList.Codes.RegistrationNumberOfTheAircraft,
				"Registration No.",
				"[19 05 017 000] Aircraft Registration Number");
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_InlandWaterwayTransport_NameOfTheInlandWaterwaysVessel()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.InlandWaterwayTransport,
				TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel,
				"Vessel Name",
				"[19 05 017 000] Vessel Name");
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_InlandWaterwayTransport_EuropeanVesselIdentificationNumberEniCode()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.InlandWaterwayTransport,
				TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode,
				"ENI Code",
				"[19 05 017 000] European Vessel Identification Number");
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_Rail_WagonNumber()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.Rail,
				TransportMeansList.Codes.WagonNumber,
				"Wagon No.",
				"[19 05 017 000] Wagon Number");
		}

		public void TestExportTransportInlandIDAndNationalityUserControlCaptions_Rail_TrainNumber()
		{
			AssertExportTransportInlandIDAndNationalityUserControlCaptions(
				TransportTypeList.Codes.Rail,
				TransportMeansList.Codes.TrainNumber,
				"Train No.",
				"[19 05 017 000] Train Number");
		}

		public void TestTransportInlandSeparatorUserControlVisible()
		{
			AssertControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, x => true);
		}
		public void TestTransportInlandModeAndTypeOfIdUserControlVisible()
		{
			AssertControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, x => x.IsUCC6);
		}

		public void TestTransportInlandIDAndNationalityUserControlVisible()
		{
			AssertControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => x.IsUCC6);
		}

		public void TestTransportInlandRoadUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, x => x.JE_TransportModeInland == TransportTypeList.Codes.Road);
		}

		public void TestTransportInlandSeaUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, x => x.JE_TransportModeInland == TransportTypeList.Codes.Sea);
		}

		protected override int ExpectedMaxColumns => 1;

		protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new TransportDetailsLayoutBuilder();

		void AssertControlVisible(ControlReference controlReference, Func<JobDeclaration, bool> visible)
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				foreach (var messageType in messageTypesWithoutWAD)
				{
					declaration.JE_MessageType = messageType;
					foreach (var transportMode in allTransportModes)
					{
						declaration.JE_TransportMode = transportMode;
						AssertEquals($"MessageType: {messageType}; TransportMode: {transportMode}", visible(declaration), Layout.IsVisible(controlReference, declaration));
					}
				}
			});
		}

		void AssertUCC6ControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible)
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				foreach (var transportMode in allTransportModes)
				{
					declaration.JE_TransportMode = transportMode;
					foreach (var inlandTransportMode in allTransportModes)
					{
						declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
						AssertEquals("Is UCC6 (Export)", true, declaration.Configuration.IsUCC6(declaration));
						declaration.JE_TransportModeInland = inlandTransportMode;
						AssertEquals($"Export: TransportMode: {transportMode}; InlandTransportMode: {inlandTransportMode}", visible(declaration), Layout.IsVisible(controlReference, declaration));

						declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
						AssertEquals("Is Non UCC6 (MiscellaneousCustoms)", false, declaration.Configuration.IsUCC6(declaration));
						AssertEquals($"MiscellaneousCustoms: TransportMode: {transportMode}; InlandTransportMode: {inlandTransportMode}", false, Layout.IsVisible(controlReference, declaration));
					}
				}
			});
		}

		static void AssertCaptions(ResourceStringData captionData, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("captionData.Caption", expectedCaption, captionData.Caption);
			AssertEquals("captionData.FullDescription", expectedFullDescription, captionData.FullDescription);
		}

		public void AssertExportTransportInlandIDAndNationalityUserControlCaptions(string transportModeInland, string transportMeans, string expectedCaption, string expectedDescription)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.JE_TransportMeans = transportMeans;

			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox], expectedCaption, expectedDescription);
				AssertCaptions(captionData[TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "Nationality", "[19 08 062 000] Nationality");
			});
		}

		string[] messageTypesWithoutWAD => new[] { EUJobMessageTypeList.Codes.Import, EUJobMessageTypeList.Codes.Export };

		string[] allTransportModes => new[]
		{
			TransportTypeList.Codes.Air,
			TransportTypeList.Codes.FixedTransportInstallations,
			TransportTypeList.Codes.InlandWaterwayTransport,
			TransportTypeList.Codes.OwnPropulsion,
			TransportTypeList.Codes.Mail,
			TransportTypeList.Codes.Rail,
			TransportTypeList.Codes.Road,
			TransportTypeList.Codes.Sea
		};

		PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
		PanelLayout layout;
	}
}
