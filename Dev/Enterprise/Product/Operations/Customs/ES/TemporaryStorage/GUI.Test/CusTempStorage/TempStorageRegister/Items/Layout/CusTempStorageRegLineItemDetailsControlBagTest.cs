using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(CusTempStorageRegLineItemDetailsControlBag))]
	public class CusTempStorageRegLineItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusTempStorageRegLineItemDetailsControlBag.GoodsItemNumberCalcEdit);
				yield return nameof(CusTempStorageRegLineItemDetailsControlBag.TariffTextBox);
				yield return nameof(CusTempStorageRegLineItemDetailsControlBag.CusC4NumberTextBox);
				yield return nameof(CusTempStorageRegLineItemDetailsControlBag.GoodsDescriptionTextBox);
				yield return nameof(CusTempStorageRegLineItemDetailsControlBag.GrossWeightCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CusTempStorageRegLineItemDetailsControlBag.Instance;
	}
}
