using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayout))]
class ShipmentTypeLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstAndOnlyColumnControls;
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstAndOnlyColumnControls
	{
		get
		{
			yield return (ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
		}
	}

	public void TestMessageSubTypeDropEdit_Visibility()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("IMP MessageSubTypeDropEdit Visible", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, Declaration));

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("EXP MessageSubTypeDropEdit Visible", false, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, Declaration));

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("EDA MessageSubTypeDropEdit Visible", true, Layout.IsVisible(ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, Declaration));
		});
	}

	PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayout().Layout);
	PanelLayout layout;

	JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	JobDeclaration declaration;
}
