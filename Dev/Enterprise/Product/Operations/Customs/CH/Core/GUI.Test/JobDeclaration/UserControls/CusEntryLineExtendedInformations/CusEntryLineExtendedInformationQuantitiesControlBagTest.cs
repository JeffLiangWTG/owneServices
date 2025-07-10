using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusEntryLineExtendedInformationQuantitiesControlBag))]
sealed class CusEntryLineExtendedInformationQuantitiesControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return (nameof(CusEntryLineExtendedInformationQuantitiesControlBag.CalcAdditionalQuantityDropEdit));
			yield return (nameof(CusEntryLineExtendedInformationQuantitiesControlBag.CalcCustomsNetWeightDropEdit));
			yield return (nameof(CusEntryLineExtendedInformationQuantitiesControlBag.CalcGrossWeightDropEdit));
			yield return (nameof(CusEntryLineExtendedInformationQuantitiesControlBag.CalcNetWeightDropEdit));
		}
	}

	protected override ControlBag GetControlBagForTesting() => CusEntryLineExtendedInformationQuantitiesControlBag.Instance;
}
