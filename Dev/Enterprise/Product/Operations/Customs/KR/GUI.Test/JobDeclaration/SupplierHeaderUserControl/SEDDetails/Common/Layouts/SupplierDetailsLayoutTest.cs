using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SupplierDetailsLayout))]
	sealed class SupplierDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SEDDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SEDDetailsControlBag.Instance.SupplierGuidFindBox, ControlWidthClass.Auto);
				yield return (SEDDetailsControlBag.Instance.SupplierUnipassIDTextBox, ControlWidthClass.Auto);
			}
		}
	}
}
