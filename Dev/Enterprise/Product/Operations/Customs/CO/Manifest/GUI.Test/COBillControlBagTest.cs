using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(COBillControlBag))]
	sealed class COBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(COBillControlBag.CargoDispositionDropEdit);
				yield return nameof(COBillControlBag.TravelDocumentTypeDropEdit);
				yield return nameof(COBillControlBag.MultimodalCheckBox);
				yield return nameof(COBillControlBag.CarriersLiabilityCheckBox);
				yield return nameof(COBillControlBag.GoodsValueConvertToLocalCurrencyControl);
				yield return nameof(COBillControlBag.BillIssueDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => COBillControlBag.Instance;
	}
}
