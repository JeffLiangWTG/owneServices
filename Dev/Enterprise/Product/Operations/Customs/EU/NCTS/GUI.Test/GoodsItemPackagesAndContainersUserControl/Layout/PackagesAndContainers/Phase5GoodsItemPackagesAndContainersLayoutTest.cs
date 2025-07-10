using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemPackagesAndContainersLayout))]
	sealed class Phase5GoodsItemPackagesAndContainersLayoutTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5GoodsItemPackagesAndContainersLayoutBuilder<Business.NctsPackage>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Phase5GoodsItemPackagesAndContainersControlBag.Instance.DynamicPackagesUserControl, ControlWidthClass.LongControl);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (Phase5GoodsItemPackagesAndContainersControlBag.Instance.ContainersUserControl, ControlWidthClass.Auto);
			}
		}
	}
}
