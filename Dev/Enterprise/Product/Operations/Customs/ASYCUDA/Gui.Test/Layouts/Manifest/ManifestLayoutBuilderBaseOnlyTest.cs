using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;
using NUnit.Framework;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ManifestLayoutBuilder<AsycudaManifestHeader>))]
	sealed class ManifestLayoutBuilderBaseOnlyTest : ManifestLayoutBuilderAbstractTest<ManifestLayoutBuilder<AsycudaManifestHeader>, AsycudaManifestHeader>
	{
		public void TestMasterBOLTextBoxCaption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
				AssertEquals("SetCaption for MasterBOLTextBox Coload", true, layout.TryGetCaption(ColumnLayoutBuilderForTesting.CommonBag.MasterBOLTextBox, header, out var resourceStringData));
				AssertEquals("caption should be Parent Bill", "Parent Bill", resourceStringData.Caption);
				header.AMA_AgentType = Core.Constants.AgentType.Other;
				AssertEquals("SetCaption for MasterBOLTextBox Other", true, layout.TryGetCaption(ColumnLayoutBuilderForTesting.CommonBag.MasterBOLTextBox, header, out resourceStringData));
				AssertEquals("caption should be Master BOL", "Master BOL", resourceStringData.Caption);
			});
		}

		public void TestVoyageFlightTextBoxCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("SetCaption for VoyageFlightTextBox", true, layout.TryGetCaption(ColumnLayoutBuilderForTesting.CommonBag.VoyageFlightTextBox, header, out var caption));
				AssertEquals("caption should be VoyageFlightNoLabel", header.VoyageFlightNoLabel, caption);
			});
		}

		public void TestManifestNumberFromMasterBillTextBoxCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Courier;
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("SetCaption for ManifestNumberFromMasterBillTextBox", true, layout.TryGetCaption(ColumnLayoutBuilderForTesting.CommonBag.ManifestNumberFromMasterBillTextBox, header, out var caption));
				AssertEquals("caption should be MasterBillLabel", header.MasterBillLabel, caption);
			});
		}

		public void TestContainerModeDropEditVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				foreach (var transportMode in AsycudaDefaultTransportModes)
				{
					header.AMA_TransportMode = transportMode;
					AssertEquals($"Invisible only for Air. Mode: {transportMode}", !header.IsAir, layout.IsVisible(CommonManifestControlBag.Instance.ContainerModeDropEdit, header));
				}
			});
		}

		public void TestVesselCodeFindBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.VesselCodeFindBox, Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.Sea);
		}

		public void TestMastersNameTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.MastersNameTextBox, Core.Constants.TransportModes.Truck, Core.Constants.TransportModes.Sea);
		}

		public void TestConveyanceCountryCodeFindBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, Core.Constants.TransportModes.WarehouseHandling, Core.Constants.TransportModes.Sea);
		}

		public void TestConveyanceCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.CookIslands);
			var er = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.CookIslands, Core.Constants.CountryCodes.CookIslands, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(er.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.CookIslands, parent: wcoDataGrouping);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.CookIslands;
			header.AMA_ManifestType = "ASY";
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("TransportMode - Road", false, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("TransportMode - Sea", true, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("TransportMode - Air for ASYCUDA countries is required", true, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
			});

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = "IAM";
			layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("AirAMS / TransportMode - Road", false, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("AirAMS / TransportMode - Sea", true, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AirAMS / TransportMode - Air", false, layout.IsVisible(CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, header));
			});
		}

		public void TestMessageStatusTextBoxVisibility()
		{
			var mockHeader = Factory.NewMoq<AsycudaManifestHeader>();
			mockHeader
				.Protected()
				.SetupSequence<bool>("AMA_MessageStatus_ReadOnly")
				.Returns(false)
				.Returns(true);
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Not Readonly and not visible", false, layout.IsVisible(CommonManifestControlBag.Instance.MessageStatusTextBox, mockHeader.Object));
				AssertEquals("Readonly and visible", true, layout.IsVisible(CommonManifestControlBag.Instance.MessageStatusTextBox, mockHeader.Object));
			});
			mockHeader.VerifyAll();
		}

		public void TestMessageStatusDropEditVisibility()
		{
			var mockHeader = Factory.NewMoq<AsycudaManifestHeader>();
			mockHeader
				.Protected()
				.SetupSequence<bool>("AMA_MessageStatus_ReadOnly")
				.Returns(true)
				.Returns(false);
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Readonly and not visible", false, layout.IsVisible(CommonManifestControlBag.Instance.MessageStatusDropEdit, mockHeader.Object));
				AssertEquals("Not Readonly and visible", true, layout.IsVisible(CommonManifestControlBag.Instance.MessageStatusDropEdit, mockHeader.Object));
			});
			mockHeader.VerifyAll();
		}

		public void TestJobReferenceTextBoxVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Stand Alone", false, layout.IsVisible(CommonManifestControlBag.Instance.JobReferenceTextBox, header));
				header.SetParent(Factory.New<ForwardingConsol>());
				AssertEquals("Parent Consol", true, layout.IsVisible(CommonManifestControlBag.Instance.JobReferenceTextBox, header));
			});
		}

		public void TestRegistrationDateEditVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			AssertEquals("always visible by default", true, layout.IsVisible(CommonManifestControlBag.Instance.RegistrationDateEdit, header));
		}

		public void TestCustomsStatusDropEditVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			AssertEquals("always visible by default", true, layout.IsVisible(CommonManifestControlBag.Instance.CustomsStatusDropEdit, header));
		}

		public void TestRegistrationNumberTextBoxVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			AssertEquals("always visible by default", true, layout.IsVisible(CommonManifestControlBag.Instance.RegistrationNumberTextBox, header));
		}

		public void TestVehicleRegistrationTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.VehicleRegistrationTextBox, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road);
		}

		public void TestVoyageFlightTextBoxVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				foreach (var transportMode in AsycudaDefaultTransportModes)
				{
					header.AMA_TransportMode = transportMode;
					AssertEquals($"Invisible only for Road. Mode: {transportMode}", !header.IsRoad, layout.IsVisible(CommonManifestControlBag.Instance.VoyageFlightTextBox, header));
				}
			});
		}

		public void TestRadioCallSignTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.RadioCallSignTextBox, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.Sea);
		}

		public void TestAMA_LloydsNumberTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.LloydsNumberTextBox, Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.Sea);
		}

		public void TestAMA_Trailer1RegNoTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.Trailer1RegNoTextBox, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road);
		}

		public void TestAMA_Trailer2RegNoTextBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.Trailer2RegNoTextBox, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road);
		}

		public void TestAMA_RN_NKTrailer1RegCountryFindBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, Core.Constants.TransportModes.SeaAir, Core.Constants.TransportModes.Road);
		}

		public void TestAMA_RN_NKTrailer2RegCountryFindBoxVisibility()
		{
			AssertVisibilityOnTransportMode(CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, Core.Constants.TransportModes.SeaAir, Core.Constants.TransportModes.Road);
		}

		public void TestMasterBOLTextBoxVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.MasterBOL, "MasterBOL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, Core.Constants.AgentType.CoLoad);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.COH));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			var controlVisibilityDependencies = layout.GetVisibilityDependencies(CommonManifestControlBag.Instance.MasterBOLTextBox, header);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("PropertyInfo's that change visibility", new ZPropertyInfo[] { header.AMA_AgentTypeInfo, header.AMA_RN_NKCountryInfo, header.AMA_ManifestTypeInfo }, controlVisibilityDependencies);
				header.AMA_AgentType = Core.Constants.AgentType.Charter;
				AssertEquals("Invisible", false, layout.IsVisible(CommonManifestControlBag.Instance.MasterBOLTextBox, header));
				header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
				header.AMA_ManifestType = nameof(Universal.Messaging.CUSCAR.ManifestDocumentType.COH);
				AssertEquals("Visible", true, layout.IsVisible(CommonManifestControlBag.Instance.MasterBOLTextBox, header));
			});
		}

		public void TestBuyersConsolidationCheckBoxVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				header.AMA_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				AssertEquals("Not conainerised", false, layout.IsVisible(CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, header));
				header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("Conainerised", true, layout.IsVisible(CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, header));
			});
		}

		protected override ManifestLayoutBuilder<AsycudaManifestHeader> GetColumnLayoutBuilderForTesting() => new ManifestLayoutBuilder<AsycudaManifestHeader>();

		void AssertVisibilityOnTransportMode(ControlReference controlToTest, string invisibleTransportMode, string visibleTransportMode)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new DefaultManifestLayouts()).Layout;
			CombineAssertions(() =>
			{
				header.AMA_TransportMode = invisibleTransportMode;
				AssertEquals("Invisible TransportMode", false, layout.IsVisible(controlToTest, header));
				header.AMA_TransportMode = visibleTransportMode;
				AssertEquals("Visible TransportMode", true, layout.IsVisible(controlToTest, header));
			});
		}

		string[] AsycudaDefaultTransportModes => new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Mail, TransportTypeList.Codes.Road, TransportTypeList.Codes.Rail,
			TransportTypeList.Codes.InlandWaterwayTransport, TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.OwnPropulsion };
	}
}
