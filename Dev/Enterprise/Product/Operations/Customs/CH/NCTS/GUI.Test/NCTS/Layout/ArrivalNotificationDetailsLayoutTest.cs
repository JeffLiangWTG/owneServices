using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalNotificationDetailsLayout))]
class ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			yield return (ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			yield return (ArrivalNotificationDetailsControlBag.Instance.CommunicationLanguageDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
			yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.StateOfSealsDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.AdditionalTextTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.TransportMeansLabel, ControlWidthClass.LongNoCaption);
			yield return (ArrivalNotificationDetailsControlBag.Instance.TransportAtArrivalTypeDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.TransportAtArrivalIDTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.TransportNationalityCodeFindBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
		}
	}

	public void TestMultipleMRNIndicator_True()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		movementHeader.MultipleMRNIndicator = ZBool.True;
		Factory.Save();

		var layout = ((IPanelLayoutProvider)new ArrivalNotificationDetailsLayout()).Layout;

		CombineAssertions(() =>
		{
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, MrnTextBox should not be visible", false, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, nctsHeader));
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, StateOfSealsDropEdit should not be visible", false, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.StateOfSealsDropEdit, nctsHeader));
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, AdditionalTextTextBox should not be visible", false, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.AdditionalTextTextBox, nctsHeader));
		});
	}

	public void TestMultipleMRNIndicator_False()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		movementHeader.MultipleMRNIndicator = ZBool.False;
		Factory.Save();

		var layout = ((IPanelLayoutProvider)new ArrivalNotificationDetailsLayout()).Layout;

		CombineAssertions(() =>
		{
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, MrnTextBox should be visible", true, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, nctsHeader));
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, StateOfSealsDropEdit should be visible", true, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.StateOfSealsDropEdit, nctsHeader));
			AssertEquals($"When {movementHeader.MultipleMRNIndicator}, AdditionalTextTextBox should be visible", true, layout.IsVisible(ArrivalNotificationDetailsControlBag.Instance.AdditionalTextTextBox, nctsHeader));
		});
	}
}
