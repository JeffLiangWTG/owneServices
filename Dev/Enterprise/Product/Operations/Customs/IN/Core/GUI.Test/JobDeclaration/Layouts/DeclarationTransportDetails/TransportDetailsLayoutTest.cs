using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(TransportDetailsLayouts))]
sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestLoadingAndDestinationInformationSeparatorUserControlVisibility()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Export", true, Layout.IsVisible(InBag.LoadingAndDestinationInformationSeparatorUserControl, Declaration));

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import", false, Layout.IsVisible(InBag.LoadingAndDestinationInformationSeparatorUserControl, Declaration));
	}

	public void TestCustomsLoadPortCodeFindBoxVisibility()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("Export and TrasportMode empty", false, Layout.IsVisible(CommonBag.CustomsLoadPortCodeFindBox, Declaration));

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
		AssertEquals("Export and TrasportMode Road", false, Layout.IsVisible(CommonBag.CustomsLoadPortCodeFindBox, Declaration));

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		AssertEquals("Export and TransportMode Sea", true, Layout.IsVisible(CommonBag.CustomsLoadPortCodeFindBox, Declaration));

		Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		AssertEquals("Export and TransportMode Air", true, Layout.IsVisible(CommonBag.CustomsLoadPortCodeFindBox, Declaration));

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import", false, Layout.IsVisible(CommonBag.CustomsLoadPortCodeFindBox, Declaration));
	}

	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => Builder;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBag.OverrideValuesCheckBox, ControlWidthClass.Long);
			yield return (CommonBag.MasterBillTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.OceanBillTextBox, ControlWidthClass.Auto);
			yield return (CommonBag.VehicleRegistrationNumberTextBox, ControlWidthClass.Auto);
			yield return (CommonBag.VesselCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBag.FlightUserControl, ControlWidthClass.Auto);
			yield return (CommonBag.VoyageNumberTextBox, ControlWidthClass.Auto);
			yield return (InBag.LoadingAndDestinationInformationSeparatorUserControl, ControlWidthClass.LongControl);
			yield return (CommonBag.PortOfLoadingUserControl, ControlWidthClass.Auto);
			yield return (CommonBag.CustomsLoadPortCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBag.PortOfDischargeUserControl, ControlWidthClass.Auto);
		}
	}

	PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new TransportDetailsLayouts()).Layout;
	PanelLayout layout;

	Customs.GUI.TransportDetailsControlBag CommonBag => commonBag ??= Builder.CommonBag;
	Customs.GUI.TransportDetailsControlBag commonBag;

	TransportDetailsControlBag InBag => inBag ??= TransportDetailsControlBag.Instance;
	TransportDetailsControlBag inBag;

	TransportDetailsLayoutBuilder<BaseJobDeclaration> Builder => builder ??= new TransportDetailsLayoutBuilder<BaseJobDeclaration>();
	TransportDetailsLayoutBuilder<BaseJobDeclaration> builder;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
