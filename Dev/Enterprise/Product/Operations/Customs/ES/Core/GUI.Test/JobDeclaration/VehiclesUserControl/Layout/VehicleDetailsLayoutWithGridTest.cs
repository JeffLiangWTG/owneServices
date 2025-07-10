using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(VehicleDetailsLayoutWithGrid))]
	internal class VehicleDetailsLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (VehicleDetailsControlBag.Instance.VinTextBox, ControlWidthClass.Long);
				yield return (VehicleDetailsControlBag.Instance.BrandTextBox, ControlWidthClass.Long);
				yield return (VehicleDetailsControlBag.Instance.ModelTextBox, ControlWidthClass.Long);
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(VehiclesGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new VehicleDetailsLayoutBuilder();
	}
}
