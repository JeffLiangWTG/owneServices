using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CarnetDetailsControlBag))]
	sealed class CarnetDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CarnetDetailsControlBag.Instance.HouseBillSplitDropEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.CarnetUseDropEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.CarnetCertificateNoTextBox);
				yield return nameof(CarnetDetailsControlBag.Instance.EffectiveToDateDateEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.RepresentativeProductNameLongTextControl);
				yield return nameof(CarnetDetailsControlBag.Instance.CargoManagementNoTextBox);
				yield return nameof(CarnetDetailsControlBag.Instance.HouseBillTextBox);
				yield return nameof(CarnetDetailsControlBag.Instance.TotalWeightCalcDropEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.TotalQtyCalcEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.NoPackagesCalcDropEdit);
				yield return nameof(CarnetDetailsControlBag.Instance.TotalAmountConvertToLocalCurrencyControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CarnetDetailsControlBag.Instance;
	}
}
