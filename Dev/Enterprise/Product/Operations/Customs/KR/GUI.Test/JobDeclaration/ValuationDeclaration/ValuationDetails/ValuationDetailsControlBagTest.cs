using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDetailsControlBag))]
	sealed class ValuationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ValuationDetailsControlBag.ProductCodeCodeFindBox);
				yield return nameof(ValuationDetailsControlBag.TariffFindBox);
				yield return nameof(ValuationDetailsControlBag.DescriptionLongTextControl);
				yield return nameof(ValuationDetailsControlBag.ModelTradeNameTextBox);
				yield return nameof(ValuationDetailsControlBag.BrandNameTextBox);
				yield return nameof(ValuationDetailsControlBag.IngredientLongTextControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ValuationDetailsControlBag.Instance;
	}
}
