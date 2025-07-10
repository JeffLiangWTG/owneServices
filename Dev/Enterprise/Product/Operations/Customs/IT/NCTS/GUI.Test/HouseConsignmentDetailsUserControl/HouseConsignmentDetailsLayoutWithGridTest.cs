using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDetailsLayoutWithGrid))]
sealed class HouseConsignmentDetailsLayoutWithGridTest : LayoutsAbstractTest
{
	public void TestTransportMoPDropEditControlVisibility()
	{
		var header = Factory.New<NctsHeader>();
		var bill = header.Bills.AddNew();
		var layout = ((IPanelLayoutProvider)new HouseConsignmentDetailsLayoutWithGrid()).Layout;
		var euBag = EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance;
		CombineAssertions(() =>
		{
			using (TemporarilySetTransitionPeriod(false))
			{
				AssertEquals("When TP is OFF then TransportMoPDropEdit is visible.", true, layout.IsVisible(euBag.TransportMoPDropEdit, bill));
			}

			using (TemporarilySetTransitionPeriod(true))
			{
				AssertEquals("When TP is ON then TransportMoPDropEdit is hidden.", false, layout.IsVisible(euBag.TransportMoPDropEdit, bill));
			}
		});
	}

	public void TestCountryOfDestinationDropEditControlVisibility()
	{
		var header = Factory.New<NctsHeader>();
		var bill = header.Bills.AddNew();
		var layout = ((IPanelLayoutProvider)new HouseConsignmentDetailsLayoutWithGrid()).Layout;
		var euBag = EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance;

		CombineAssertions(() =>
		{
			using (TemporarilySetTransitionPeriod(false))
			{
				AssertEquals("When TP is OFF then CountryOfDestinationDropEdit is visible.", true, layout.IsVisible(euBag.CountryOfDestinationDropEdit, bill));
			}

			using (TemporarilySetTransitionPeriod(true))
			{
				AssertEquals("When TP is ON then CountryOfDestinationDropEdit is hidden.", false, layout.IsVisible(euBag.CountryOfDestinationDropEdit, bill));
			}
		});
	}

	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentDetailsLayoutBuilder<NctsBill>();

	protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentsGridUserControl);

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
			yield return (HouseConsignmentDetailsControlBag.Instance.CustomsStatusUserControl, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.ReferenceNumberUCRTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.TransportMoPDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.Long);
		}
	}

	IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);
}
