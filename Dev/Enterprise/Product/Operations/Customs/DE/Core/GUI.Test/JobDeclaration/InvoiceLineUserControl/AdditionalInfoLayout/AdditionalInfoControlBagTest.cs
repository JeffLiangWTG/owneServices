using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(AdditionalInfoControlBag))]
	class AdditionalInfoControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AdditionalInfoControlBag.ContentInfoTypeSeparatorUserControl);
				yield return nameof(AdditionalInfoControlBag.ContentInfoTypeGrid);
				yield return nameof(AdditionalInfoControlBag.AdditionalTariffsSeparatorUserControl);
				yield return nameof(AdditionalInfoControlBag.AdditionalTariffsGrid);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AdditionalInfoControlBag.Instance;
	}
}
