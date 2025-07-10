using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EXPProductsCustomDetailsCommonLayout))]
	sealed class EXPProductsCustomDetailsCommonLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EXPProductsCustomDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EXPProductsCustomDetailsControlBag.Instance.BrandNameTextBox, ControlWidthClass.Long);
				yield return (EXPProductsCustomDetailsControlBag.Instance.ModelTradeNameTextBox, ControlWidthClass.Long);
				yield return (EXPProductsCustomDetailsControlBag.Instance.IngredientTextBox, ControlWidthClass.Long);
				yield return (EXPProductsCustomDetailsControlBag.Instance.UsageCommentTextBox, ControlWidthClass.Long);
				yield return (EXPProductsCustomDetailsControlBag.Instance.ClassificationDescriptionTextBox, ControlWidthClass.Long);
			}
		}
	}
}
