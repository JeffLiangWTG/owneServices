using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EXPProductsCustomDetailsCertificateOfOriginLayout))]
	sealed class EXPProductsCustomDetailsCertificateOfOriginLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EXPProductsCustomDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EXPProductsCustomDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (EXPProductsCustomDetailsControlBag.Instance.COOLabelLocationDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
