using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPackagesAndContainersControlBag))]
	sealed class GoodsItemPackagesAndContainersControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GoodsItemPackagesAndContainersUserControl.PackagesUserControl);
				yield return nameof(GoodsItemPackagesAndContainersUserControl.ContainersUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GoodsItemPackagesAndContainersControlBag.Instance;
	}
}
