using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(AdditionalInformationDetailsControlBag))]
public class AdditionalInformationDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(AdditionalInformationDetailsControlBag.NKCountryCodeFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => AdditionalInformationDetailsControlBag.Instance;
}
