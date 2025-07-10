using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDifferencesControlBag))]
	sealed class HouseConsignmentDifferencesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HouseConsignmentDifferencesControlBag.SequenceNumberTextBox);
				yield return nameof(HouseConsignmentDifferencesControlBag.SecurityCheckBox);
				yield return nameof(HouseConsignmentDifferencesControlBag.HouseConsignmentTextBox);
				yield return nameof(HouseConsignmentDifferencesControlBag.UnloadedStateDropEdit);
				yield return nameof(HouseConsignmentDifferencesControlBag.DeclaredValueLabel);
				yield return nameof(HouseConsignmentDifferencesControlBag.UnloadedValueLabel);
				yield return nameof(HouseConsignmentDifferencesControlBag.GrossWeightCalcDropEdit);
				yield return nameof(HouseConsignmentDifferencesControlBag.GrossWeightUnloadedCalcDropEdit);

				yield return nameof(HouseConsignmentDifferencesControlBag.ConsigneeDocAddressControl);
				yield return nameof(HouseConsignmentDifferencesControlBag.ConsignorDocAddressControl);

				yield return nameof(HouseConsignmentDifferencesControlBag.ArrivalTransportInfosUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HouseConsignmentDifferencesControlBag.Instance;
	}
}
