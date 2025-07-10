using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPackageControlBag))]
	sealed class GoodsItemPackageControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GoodsItemPackageUserControl.PackageTypeDropEdit);
				yield return nameof(GoodsItemPackageUserControl.NumberOfPackagesCalcEdit);
				yield return nameof(GoodsItemPackageUserControl.MarksAndNumbersTextBox);
				yield return nameof(GoodsItemPackageUserControl.PackageIDTextBox);
				yield return nameof(GoodsItemPackageUserControl.ModelTextBox);
				yield return nameof(GoodsItemPackageUserControl.BrandTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GoodsItemPackageControlBag.Instance;
	}
}
