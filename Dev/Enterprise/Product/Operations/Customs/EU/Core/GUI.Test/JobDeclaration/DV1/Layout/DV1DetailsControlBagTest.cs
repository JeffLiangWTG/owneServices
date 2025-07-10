using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(DV1DetailsControlBag))]
	sealed class DV1DetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DV1DetailsControlBag.RelationshipDropEdit);
				yield return nameof(DV1DetailsControlBag.RelationDetailsTextBox);
				yield return nameof(DV1DetailsControlBag.PriceInfluenceDropEdit);
				yield return nameof(DV1DetailsControlBag.CloseApproximationDropEdit);
				yield return nameof(DV1DetailsControlBag.RestrictionsDropEdit);
				yield return nameof(DV1DetailsControlBag.ConsiderationDropEdit);
				yield return nameof(DV1DetailsControlBag.RestrictionsConsiderationTextBox);
				yield return nameof(DV1DetailsControlBag.RoyalitiesLicenceDropEdit);
				yield return nameof(DV1DetailsControlBag.RoyalitiesLicenceDetailsTextBox);
				yield return nameof(DV1DetailsControlBag.ResaleDropEdit);
				yield return nameof(DV1DetailsControlBag.ResaleDetailsTextBox);
				yield return nameof(DV1DetailsControlBag.CustomsDecisionNumberTextBox);
				yield return nameof(DV1DetailsControlBag.ContractNumberTextBox);
				yield return nameof(DV1DetailsControlBag.ContractDateDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DV1DetailsControlBag.Instance;
	}
}
