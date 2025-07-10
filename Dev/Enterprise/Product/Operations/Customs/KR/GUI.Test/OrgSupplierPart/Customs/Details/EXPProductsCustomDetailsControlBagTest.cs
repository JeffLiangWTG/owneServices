using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EXPProductsCustomDetailsControlBag))]
	sealed class EXPProductsCustomDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EXPProductsCustomDetailsControlBag.BrandNameTextBox);
				yield return nameof(EXPProductsCustomDetailsControlBag.ModelTradeNameTextBox);
				yield return nameof(EXPProductsCustomDetailsControlBag.IngredientTextBox);
				yield return nameof(EXPProductsCustomDetailsControlBag.UsageCommentTextBox);
				yield return nameof(EXPProductsCustomDetailsControlBag.ClassificationDescriptionTextBox);

				yield return nameof(EXPProductsCustomDetailsControlBag.GoodsOriginCodeFindBox);
				yield return nameof(EXPProductsCustomDetailsControlBag.COOLabelLocationDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EXPProductsCustomDetailsControlBag.Instance;
	}
}
