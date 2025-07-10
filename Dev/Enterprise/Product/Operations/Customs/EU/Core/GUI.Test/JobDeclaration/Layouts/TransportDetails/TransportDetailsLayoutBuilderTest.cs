using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.GUI.Declaration.Testing
{
	[TestedType(typeof(TransportDetailsLayoutBuilder<JobDeclaration>))]
	sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder<JobDeclaration>, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
	{
		public void TestWarehouseAdjustmentVisibilities()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			CombineAssertions(() =>
			{
				AssertEquals("MasterBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, declaration));
				AssertEquals("OceanBillTextBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, declaration));
				AssertEquals("VesselCodeFindBox", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, declaration));
				AssertEquals("FlightAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
				AssertEquals("VoyageAndNationalityUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
				AssertEquals("TransportIDAndNationalityUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
				AssertEquals("PortOfLoadingUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, declaration));
				AssertEquals("TransportDetailsPortOfLoadingWithIATAUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, declaration));
				AssertEquals("PortOfFirstArrivalUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
				AssertEquals("PortOfDischargeUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, declaration));
				AssertEquals("TransportInlandSeparatorUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
				AssertEquals("InlandTransportDetailsUserControl", true, Layout.IsVisible(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, declaration));
				AssertEquals("InlandModeOfTransportDropEdit", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, declaration));
				AssertEquals("TransportInlandRoadUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, declaration));
				AssertEquals("TransportInlandAirUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandAirUserControl, declaration));
				AssertEquals("TransportInlandInlandWaterwaysUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandInlandWaterwaysUserControl, declaration));
				AssertEquals("TransportInlandOwnPropulsionUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandOwnPropulsionUserControl, declaration));
				AssertEquals("TransportInlandRailUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, declaration));
				AssertEquals("TransportInlandSeaUserControl", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("AdditionalWagonNumbersUserControl", false, Layout.IsVisible(TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl, declaration));
			});
		}

		public void TestOverrideValuesCheckBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipmentGuid = ZGuid.NewZGuid();

			CombineAssertions(() =>
			{
				foreach (var messageType in new[] { DEJobMessageTypeList.Codes.Import, DEJobMessageTypeList.Codes.Export, DEJobMessageTypeList.Codes.MiscellaneousCustoms, DEJobMessageTypeList.Codes.WarehouseAdjustment })
				{
					declaration.JE_JS = ZGuid.Empty;
					declaration.JE_MessageType = messageType;
					AssertEquals($"{messageType}: JE_JS empty", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));

					declaration.JE_JS = shipmentGuid;
					AssertEquals($"{messageType}: JE_JS not empty", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, declaration));
				}
			});
		}

		public void TestMasterBillTextBoxVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, x => x.IsAir);
		}

		public void TestOceanBillTextBoxVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, x => x.IsSea);
		}

		public void TestVesselCodeFindBoxVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, x => x.IsSea);
		}

		public void TestFlightAndNationalityUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, x => x.IsAir);
		}

		public void TestVoyageAndNationalityUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, x => x.IsSea);
		}

		public void TestTransportIDAndNationalityUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, x => !x.IsAir && !x.IsSea);
		}

		public void TestPortOfLoadingUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, x => !(x.IsImport && x.IsAir));
		}

		public void TestTransportDetailsPortOfLoadingWithIATAUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir);
		}

		public void TestPortOfFirstArrivalUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, x => true);
		}

		public void TestTransportDetailsPortOfDischargeUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, x => true);
		}

		public void TestTransportInlandSeparatorUserControlVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, x => true);
		}

		public void TestInlandTransportDetailsUserControlVisible()
		{
			AssertUCC6ControlVisibility(EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, x => x.JE_TransportModeInland.IsEmpty, true);
		}

		public void TestAdditionalWagonNumbersUserControlVisible()
		{
			AssertUCC6ControlVisibility(TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl, x => x.JE_TransportModeInland == TransportModes.Rail && !x.IsTransitionPeriodAES30);
		}

		public void TestAdditionalWagonNumbersUserControlVisible_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, CountryCodes.Germany, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportModeInland = TransportModes.Rail;
				AssertEquals(false, Layout.IsVisible(TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl, declaration));
			}
		}

		public void TestInlandModeOfTransportDropEditVisible()
		{
			AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, x => !x.IsExport);
		}

		public void TestTransportInlandRoadUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, x => x.JE_TransportModeInland == TransportModes.Road);
		}

		public void TestTransportInlandAirUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandAirUserControl, x => x.JE_TransportModeInland == TransportModes.Air);
		}

		public void TestTransportInlandWaterwaysUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandInlandWaterwaysUserControl, x => x.JE_TransportModeInland == TransportModes.InlandWaterwayTransport);
		}

		public void TestTransportInlandOwnPropulsionUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandOwnPropulsionUserControl, x => x.JE_TransportModeInland == TransportModes.OwnPropulsion || x.IsFixedInstallationInland || x.IsMailInland);
		}

		public void TestTransportInlandRailUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, x => x.JE_TransportModeInland == TransportModes.Rail);
		}

		public void TestTransportInlandSeaUserControlVisible()
		{
			AssertUCC6ControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, x => x.JE_TransportModeInland == TransportModes.Sea);
		}

		public void TestExportTransportInlandSeaUserControlCaptions_NameOfTheSeaGoingVessel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true);
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "Vessel Name", string.Empty);
			});
		}

		public void TestExportTransportInlandSeaUserControlCaptions_ImoShipIdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
			Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true);
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration));
				AssertEquals("SetCaptions for Control1", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, declaration, out var captionData));
				AssertCaptions(captionData[Customs.GUI.TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox], "IMO No.", "Lloyds / IMO Number");
			});
		}

		static void AssertCaptions(ResourceStringData captionData, string expectedCaption, string expectedFullDescription)
		{
			AssertEquals("captionData.Caption", expectedCaption, captionData.Caption);
			AssertEquals("captionData.FullDescription", expectedFullDescription, captionData.FullDescription);
		}

		protected override int ExpectedMaxColumns => 1;

		protected override TransportDetailsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting()
		{
			var builder = new TransportDetailsLayoutBuilder<JobDeclaration>();
			builder.AddControlBag(TransportDetailsControlBag.Instance);
			builder.AddControlBag(EU.GUI.TransportDetailsControlBag.Instance);
			return builder;
		}

		void AssertControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible)
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

		void AssertUCC6ControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible, bool visibleWhenNotUCC6 = false)
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				AssertEquals("Is Non UCC6", false, declaration.Configuration.IsUCC6(declaration));
				AssertControlVisibility("Non UCC6", (x) => visibleWhenNotUCC6);

				Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true);
				AssertEquals("Is UCC6", true, declaration.Configuration.IsUCC6(declaration));
				AssertControlVisibility("UCC6", visible);
			});

			void AssertControlVisibility(string description, Func<JobDeclaration, bool> isVisible)
			{
				foreach (var transportMode in allTransportModes)
				{
					declaration.JE_TransportMode = transportMode;
					foreach (var inlandTransportMode in allTransportModes)
					{
						declaration.JE_TransportModeInland = inlandTransportMode;
						AssertEquals($"{description}: TransportMode: {transportMode}; InlandTransportMode: {inlandTransportMode}", isVisible(declaration), Layout.IsVisible(controlReference, declaration));
					}
				}
			}
		}

		string[] messageTypesWithoutWAD => new[] { DEJobMessageTypeList.Codes.Import, DEJobMessageTypeList.Codes.Export, DEJobMessageTypeList.Codes.MiscellaneousCustoms };

		string[] allTransportModes => new[]
		{
			TransportModes.Air,
			TransportModes.FixedTransportInstallations,
			TransportModes.InlandWaterwayTransport,
			TransportModes.OwnPropulsion,
			TransportModes.Mail,
			TransportModes.Rail,
			TransportModes.Road,
			TransportModes.Sea,
			string.Empty
		};

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new TransportDetailsLayout()).Layout);
		PanelLayout layout;
	}
}
