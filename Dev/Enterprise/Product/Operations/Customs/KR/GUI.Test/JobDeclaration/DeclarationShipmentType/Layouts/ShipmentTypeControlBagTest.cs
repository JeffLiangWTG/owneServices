using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.Instance.TransactionTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.ExporterTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.TransactionDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.PaymentTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.PlanTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.ImporterTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}
