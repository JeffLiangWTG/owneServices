using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDetailsLayoutWithGrid))]
	sealed class HouseConsignmentDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HouseConsignmentDetailsLayoutBuilder<Business.NctsBill>();

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentsGridUserControl);

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (HouseConsignmentDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (HouseConsignmentDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (HouseConsignmentDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (HouseConsignmentDetailsControlBag.Instance.ReferenceNumberUCRTextBox, ControlWidthClass.Long);
				yield return (HouseConsignmentDetailsControlBag.Instance.TransportMoPDropEdit, ControlWidthClass.Long);
				yield return (HouseConsignmentDetailsControlBag.Instance.LinePriceCurrencyDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (HouseConsignmentDetailsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (HouseConsignmentDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.Long);
			}
		}
	}
}
