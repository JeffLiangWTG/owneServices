using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(TransportDetailsLayout))]
sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestTransportationTypeUserControl_TransportModeAndMessageType()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
		AssertEquals("Visibility when TransportMode<>ROAD", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VehicleTypeDropEdit, Declaration));

		Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
		AssertEquals("Visibility when TransportMode=ROAD and declaration is IMPORT", true, Layout.IsVisible(TransportDetailsControlBag.Instance.VehicleTypeDropEdit, Declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("Visibility when MessageType<>Import", false, Layout.IsVisible(TransportDetailsControlBag.Instance.VehicleTypeDropEdit, Declaration));
	}

	public void TestTransportIDAndNationalityUserControl_TransportMode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_TransportMode = "AIR";
			AssertEquals("TransportIDAndNationalityUserControl Not Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, Declaration));

			Declaration.JE_TransportMode = "ROA";
			AssertEquals("TransportIDAndNationalityUserControl Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, Declaration));
		});
	}

	public void TestVoyageAndNationalityUserControl_TransportMode()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_TransportMode = "AIR";
			AssertEquals("VoyageAndNationalityUserControl Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, Declaration));

			Declaration.JE_TransportMode = "ROA";
			AssertEquals("VoyageAndNationalityUserControl Not Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, Declaration));
		});
	}

	public void TestDispatchCountryUserControl()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("DispatchCountryUserControl Visible", true, Layout.IsVisible(TransportDetailsControlBag.Instance.DispatchCountryUserControl, Declaration));

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("DispatchCountryUserControl Visible", false, Layout.IsVisible(TransportDetailsControlBag.Instance.DispatchCountryUserControl, Declaration));
	}

	public void TestGoodsDestinationCountryCodeFindBox()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("GoodsDestinationCountryCodeFindBox Visible", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.GoodsDestinationCountryCodeFindBox, Declaration));

			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("GoodsDestinationCountryCodeFindBox Visible", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.GoodsDestinationCountryCodeFindBox, Declaration));
		});
	}

	public void TestTypeOfIdDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Export / Air", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, Declaration));

			Declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
			AssertEquals("Export / Own", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, Declaration));

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("ExportDeclarationActivation / Own", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, Declaration));

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("Import / Own", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, Declaration));
		});
	}

	public void TestSpecificCircumstanceIndicatorDropEditVisiblity()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("Specific Circumstance Indicator not Visible IMP", false, Layout.IsVisible(TransportDetailsControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, Declaration));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("Specific Circumstance Indicator Visible EDA", true, Layout.IsVisible(TransportDetailsControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, Declaration));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Specific Circumstance Indicator Visible EXP", true, Layout.IsVisible(TransportDetailsControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, Declaration));
	}

	public void TestUcrVisibility()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("UCR not Visible IMP", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.UCRTextBox, Declaration));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("UCR Visible EDA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.UCRTextBox, Declaration));

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("UCR Visible EXP", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.UCRTextBox, Declaration));
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	PanelLayout Layout => layout ?? (layout = new TransportDetailsLayout().Layout);
	PanelLayout layout;

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportMeansDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.VehicleTypeDropEdit, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.DispatchCountryUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.GoodsDestinationCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.UCRTextBox, ControlWidthClass.Auto);
			yield return (TransportDetailsControlBag.Instance.SpecificCircumstanceIndicatorDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 2;

	public JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
			}
			return declaration;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();

	JobDeclaration declaration;
}
