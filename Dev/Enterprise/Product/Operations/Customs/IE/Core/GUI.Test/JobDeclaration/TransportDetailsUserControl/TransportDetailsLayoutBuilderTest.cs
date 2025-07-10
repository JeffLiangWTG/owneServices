using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayoutBuilder))]
	sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
	{
		public void TestTransportInlandModeAndTypeOfIdUserControlCaptions_When_ImportUCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var userControl = Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertAllCaptions(captionData[Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit], "[7/5] Trans. Mode", "[7/5] Transport Mode", "[7/5] Inland Transport Mode", "[7/5] Inland mode of transport");
				AssertAllCaptions(captionData[Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.TypeOfIDDropEdit], "[7/9] ID", "[7/9] Type of ID", "[7/9] Type of Identification", "[7/9] Identity of means of transport on arrival > Type of Identification");
			});
		}

		public void TestTransportInlandModeAndTypeOfIdUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			AssertTransportInlandModeAndTypeOfIdUserControlCaptions(declaration);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertTransportInlandModeAndTypeOfIdUserControlCaptions(declaration);
		}

		void AssertTransportInlandModeAndTypeOfIdUserControlCaptions(JobDeclaration declaration)
		{
			var userControl = Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl;
			CombineAssertions($"MessageType: {declaration.JE_MessageType}; ApplicationCode: {declaration.JE_ApplicationCode}", () =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration));
				AssertEquals("SetCaptions for Control", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit], "Trans. Mode", "[19 04 001 000] Inland Mode of Transport");
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_NameOfTheSeaGoingVessel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Vessel Name", "[19 05 017 000] Vessel Name");
				var nationalityCodeFindBocCaptionData = captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox];
				AssertCaptions(nationalityCodeFindBocCaptionData, "Nationality", "[19 08 062 000] Nationality");
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_ImoShipIdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Lloyds No.", "[19 05 017 000] Lloyds Number");
				var nationalityCodeFindBocCaptionData = captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox];
				AssertCaptions(nationalityCodeFindBocCaptionData, "Nationality", "[19 08 062 000] Nationality");
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_CaptionChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("SetCaptions for Control", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Vessel Name", "[19 05 017 000] Vessel Name");

				declaration.JE_TransportMeans = Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber;
				AssertEquals("SetCaptions for Control", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out captionData));
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Lloyds No.", "[19 05 017 000] Lloyds Number");
			});
		}

		public void TestFlightAndNationalityUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var userControl = EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.FlightNumberTextBox], "Flight No.", "[19 08 017 000] Flight Number");

				var nationalityCodeFindBocCaptionData = captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox];
				AssertCaptions(nationalityCodeFindBocCaptionData, "Nationality", "[19 08 062 000] Nationality");
			});

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.FlightNumberTextBox], "Flight No.", "[19 08 017 000] Flight Number");
				AssertAllCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "[7/15] Nat.", "[7/15] Nationality", "[7/15] Nationality at the border", "[7/15] Nationality of active means of transport crossing the border");
			});
		}

		public void TestTransportIDAndNationalityRailUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			var userControl = EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityRailUserControl.ControlNames.TransportIDTextBox], "Wagon Number", "[19 08 017 000] Wagon Number");
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityRailUserControl.ControlNames.TransportNationalityFindBox], "Nationality", "[19 08 062 000] Nationality");
			});

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityRailUserControl.ControlNames.TransportIDTextBox], "Wagon Number", "[19 08 017 000] Wagon Number");
				AssertAllCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "[7/15] Nat.", "[7/15] Nationality", "[7/15] Nationality at the border", "[7/15] Nationality of active means of transport crossing the border");
			});
		}

		public void TestTransportIDAndNationalityUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			var userControl = EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Vehicle and/or trailer registration number");
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "Nationality", "[19 08 062 000] Nationality");

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				Layout.TryGetCaptionData(userControl, declaration, out var captionDataMail);
				AssertCaptions(captionDataMail[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Transport ID at Border");

				declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				Layout.TryGetCaptionData(userControl, declaration, out var captionDataOwn);
				AssertCaptions(captionDataOwn[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Transport ID at Border");
			});

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Vehicle and/or trailer registration number");
				AssertAllCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "[7/15] Nat.", "[7/15] Nationality", "[7/15] Nationality at the border", "[7/15] Nationality of active means of transport crossing the border");

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				Layout.TryGetCaptionData(userControl, declaration, out var captionDataMail);
				AssertCaptions(captionDataMail[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Transport ID at Border");

				declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
				Layout.TryGetCaptionData(userControl, declaration, out var captionDataOwn);
				AssertCaptions(captionDataOwn[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 08 017 000] Transport ID at Border");
			});
		}

		public void TestTransportIDAndNationalityInlandWaterwayENIUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			var userControl = EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "ENI Code", "[19 08 017 000] European Vessel Identification Number (ENI)");
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "Nationality", "[19 08 062 000] Nationality");
			});

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			CombineAssertions(() =>
			{
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));
				AssertEquals("data.Length", 2, captionData.Count);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "ENI Code", "[19 08 017 000] European Vessel Identification Number (ENI)");
				AssertAllCaptions(captionData[EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox], "[7/15] Nat.", "[7/15] Nationality", "[7/15] Nationality at the border", "[7/15] Nationality of active means of transport crossing the border");
			});
		}

		public void TestTransportInlandIDAndNationalityUserControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			var userControl = Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));

				declaration.JE_TransportMode = "";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "");

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 06 017 000] Arrival transport means < Identification number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				declaration.JE_TransportMeans = "10";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Lloyds No.", "[19 05 017 000] Lloyds Number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
				declaration.JE_TransportMeans = "30";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Registration No.", "[19 05 017 000] Vehicle Registration Number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
				declaration.JE_TransportMeans = "40";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Flight No.", "[19 05 017 000] Flight No.");

				declaration.JE_TransportModeInland = "";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "Transport ID", "[19 06 017 000] Arrival transport means < Identification number");
			});
		}

		public void TestJE_TransportIDInlandCaption_ImportUCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var userControl = Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(userControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(userControl, declaration, out var captionData));

				declaration.JE_TransportMode = "";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Transport ID", "[7/9] Arrival transport means < Identification number");

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Transport ID", "[7/9] Arrival transport means < Identification number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
				declaration.JE_TransportMeans = "10";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Lloyds No.", "[7/9] Lloyds Number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
				declaration.JE_TransportMeans = "30";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Reg. No.", "[7/9] Vehicle Registration Number");

				declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
				declaration.JE_TransportMeans = "40";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Flight No.", "");

				declaration.JE_TransportModeInland = "";
				Layout.TryGetCaptionData(userControl, declaration, out captionData);
				AssertCaptions(captionData[EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox], "[7/9] Transport ID", "[7/9] Arrival transport means < Identification number");
			});
		}

		public void TestFlightAndNationalityUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Should not be visible when transport mode not Air", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
			});
		}

		public void TestTransportIDAndNationalityUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Should not be visible when transport mode not Road", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
			});
		}

		public void TestTransportIDAndNationalityRailUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Should not be visible when transport mode not Rail", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityRailUserControl, declaration));
			});
		}

		public void TestTransportIDAndNationalityInlandWaterwayUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._81;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Should not be visible when transport mode not Inland Waterway", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayUserControl, declaration));
			});
		}

		public void TestTransportIDAndNationalityInlandWaterwayENIUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._80;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Should not be visible when transport mode not Inland Waterway", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityInlandWaterwayENIUserControl, declaration));
			});
		}

		public void TestVoyageAndNationalityUserControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Should not be visible when transport mode not Sea", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
			});
		}

		public void TestAircraftRegistrationNumberTextBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._41;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertEquals("UCC5 Import", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertEquals("UCC6 Import", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("Export (always UCC6)", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
				AssertEquals("Should not be visible when border transport means not 41", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("Should not be visible when transport mode not Air", false, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.AircraftRegistrationNumberTextBox, declaration));
			});
		}

		public void TestTransportInlandSeparatorUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, x => true);
		}

		public void TestTransportInlandModeAndTypeOfIdUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, x => true);
		}

		public void TestTransportInlandIDAndNationalityUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => x.JE_TransportModeInland.IsEmpty || (!x.IsRoadInland && !x.IsSeaInland));
		}

		public void TestTransportDetailsPortOfLoadingWithIATAUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir);
		}

		public void TestTransportInlandRoadUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, x => x.JE_TransportModeInland == TransportTypeList.Codes.Road);
		}

		public void TestTransportInlandSeaUserControlVisible()
		{
			AssertInlandTransportControlVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, x => x.JE_TransportModeInland == TransportTypeList.Codes.Sea);
		}

		protected override int ExpectedMaxColumns => 1;

		protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new TransportDetailsLayoutBuilder();

		static void AssertCaptions(ResourceStringData captionData, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("captionData.Caption", expectedCaption, captionData.Caption);
			AssertEquals("captionData.FullDescription", expectedFullDescription, captionData.FullDescription);
		}

		static void AssertAllCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedMediumCaption, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("captionData.ShortCaption", expectedShortCaption, captionData.ShortCaption);
			AssertEquals("captionData.Caption", expectedCaption, captionData.Caption);
			AssertEquals("captionCata.MediumCaption", expectedMediumCaption, captionData.MediumCaption);
			AssertEquals("captionData.FullDescription", expectedFullDescription, captionData.FullDescription);
		}

		void AssertInlandTransportControlVisible(ControlReference controlReference, Func<JobDeclaration, bool> visible)
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertControlVisibleForInlandTransportModes(controlReference, declaration, visible);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				AssertControlVisibleForInlandTransportModes(controlReference, declaration, visible);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertControlVisibleForInlandTransportModes(controlReference, declaration, visible);
			});
		}

		void AssertControlVisibleForInlandTransportModes(ControlReference controlReference, JobDeclaration declaration, Func<JobDeclaration, bool> visible)
		{
			foreach (var transportMode in allTransportModes)
			{
				declaration.JE_TransportModeInland = transportMode;
				AssertEquals($"MessageType: {declaration.JE_MessageType}; ApplicationCode: {declaration.JE_ApplicationCode}; TransportModeInland: {transportMode}", visible(declaration), Layout.IsVisible(controlReference, declaration));
			}
		}

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

		PanelLayout Layout => layout ??= new TransportDetailsLayout().Layout;
		PanelLayout layout;
	}
}
