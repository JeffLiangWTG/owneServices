using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemPackageLayout))]
	sealed class Phase5GoodsItemPackageLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new GoodsItemPackageLayoutBuilder<Business.NctsPackage>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (GoodsItemPackageControlBag.Instance.PackageTypeDropEdit, ControlWidthClass.Long);
				yield return (GoodsItemPackageControlBag.Instance.NumberOfPackagesCalcEdit, ControlWidthClass.Medium);
				yield return (GoodsItemPackageControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
			}
		}
	}
}
