using System;
using System.Collections.Generic;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDifferencesLayout))]
sealed class HouseConsignmentDifferencesLayoutTest : LayoutsAbstractTest
{
	protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentDifferencesGridUserControl);

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.SequenceNumberTextBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.SecurityCheckBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.HouseConsignmentTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.UnloadedStateDropEdit, ControlWidthClass.Long);
			yield return (HouseConsignmentDifferencesControlBag.Instance.UnloadingRemarkCodeDropEdit, ControlWidthClass.Long);
			yield return (HouseConsignmentDifferencesControlBag.Instance.UnloadingRemarkTextTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.DeclaredValueLabel, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.ArrivalTransportInfosUserControl, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.UnloadedValueLabel, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.GrossWeightUnloadedCalcDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongControl);
			yield return (EU.NCTS.GUI.HouseConsignmentDifferencesControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentDifferencesLayoutBuilder<NctsBill>();
}
