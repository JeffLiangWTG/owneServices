using System.Collections.Generic;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(ManifestLayouts))]
sealed class ManifestLayoutsTest : LayoutsAbstractTest
{
	public void TestManifestNumberFromMasterBillTextBoxCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, header, out var resourceStringData);

		CombineAssertions(() =>
		{
			AssertEquals("Caption Before", "BOL", resourceStringData.Caption);

			header.AMA_AgentType = "CLD";
			LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, header, out var resData);
			AssertEquals("Caption After", "Co-Load MBL", resData.Caption);
		});
	}

	public void TestCarrierAddressControlCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.CarrierAddressControl, header, out var resourceStringData);

		CombineAssertions(() =>
		{
			AssertEquals("Caption Before", "Carrier", resourceStringData.Caption);

			header.AMA_AgentType = "CLD";
			LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.CarrierAddressControl, header, out var resData);
			AssertEquals("Caption After", "Co-Loader", resData.Caption);
		});
	}

	public void TestShippingAgentCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.ShippingAgentAddressControl, header, out var resourceStringData);

		CombineAssertions(() =>
		{
			AssertEquals("Short Caption Before", "Default Frt. Forwarder", resourceStringData.ShortCaption);
			AssertEquals("Full Caption Before", "Default Freight Forwarder", resourceStringData.Caption);

			header.AMA_AgentType = "CLD";
			LayoutForTesting.TryGetCaption(CommonManifestControlBag.Instance.ShippingAgentAddressControl, header, out var resData);
			AssertEquals("Caption After", "Sub Co-Loader", resData.Caption);
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
			yield return (CommonManifestControlBag.Instance.CustomsOriginPortCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
			yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
			yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
			yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
			yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (ManifestControlBag.Instance.CarrierMPCITextBox, ControlWidthClass.Medium);
			yield return (ManifestControlBag.Instance.ShippingAgentMPCITextBox, ControlWidthClass.Medium);
		}
	}
}
