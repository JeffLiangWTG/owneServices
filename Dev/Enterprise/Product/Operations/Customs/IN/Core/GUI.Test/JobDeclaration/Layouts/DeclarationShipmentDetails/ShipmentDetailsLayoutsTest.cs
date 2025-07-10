using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ShipmentDetailsLayouts))]
sealed class ShipmentDetailsLayoutsTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	public void TestControlsVisibility()
	{
		var layout = GetLayout();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			AssertEquals(false, layout.IsVisible(commonBag.ContainerCountCalcEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, layout.IsVisible(commonBag.ContainerCountCalcEdit, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(true, layout.IsVisible(commonBag.ContainerCountCalcEdit, declaration));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<BaseJobDeclaration>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			var builder = new ShipmentDetailsLayoutBuilder<BaseJobDeclaration>();
			yield return (commonBag.HouseBillParcelPostTextBox, ControlWidthClass.Auto);
			yield return (commonBag.ShipmentDetailsOriginUserControl, ControlWidthClass.Auto);
			yield return (commonBag.ShipmentDetailsFinalDestinationUserControl, ControlWidthClass.Auto);
			yield return (commonBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			yield return (commonBag.OwnersReferenceTextBox, ControlWidthClass.Auto);
			yield return (commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.TotalNoOfPiecesCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
			yield return (commonBag.TotalNoOfPacksCalcDropEdit, ControlWidthClass.Auto);
			yield return (commonBag.ShipmentDetailsIncoTermsUserControl, ControlWidthClass.Auto);
			yield return (commonBag.ShipmentDetailsScreeningUserControl, ControlWidthClass.Auto);
		}
	}

	ShipmentDetailsControlBag commonBag => ShipmentDetailsControlBag.Instance;

	PanelLayout GetLayout() => GetPanelLayoutProvider().Layout;

	IPanelLayoutProvider GetPanelLayoutProvider() => new ShipmentDetailsLayouts();
}
