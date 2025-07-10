using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Test;

[TestedType(typeof(TobaccoFieldsControlBag))]
class TobaccoFieldsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TobaccoFieldsControlBag.Instance.MainGroupDropEdit);
			yield return nameof(TobaccoFieldsControlBag.Instance.SubGroupDropEdit);
			yield return nameof(TobaccoFieldsControlBag.Instance.DesignationTextBox);
			yield return nameof(TobaccoFieldsControlBag.Instance.SequentialNumberIntEdit);
			yield return nameof(TobaccoFieldsControlBag.Instance.RetailPriceCalcEdit);
			yield return nameof(TobaccoFieldsControlBag.Instance.ReverseNumberTextBox);
			yield return nameof(TobaccoFieldsControlBag.Instance.SpecialUnitOfMeasureDropEdit);
			yield return nameof(TobaccoFieldsControlBag.Instance.TobaccoBrandDropEdit);
		}
	}
	protected override ControlBag GetControlBagForTesting() => TobaccoFieldsControlBag.Instance;
}
