using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(AdditionalInformationFieldsControlBag))]
sealed class AdditionalInformationFieldsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return (nameof(AdditionalInformationFieldsControlBag.CodeDropEdit));
			yield return (nameof(AdditionalInformationFieldsControlBag.ReferenceNumberDropEdit));
			yield return (nameof(AdditionalInformationFieldsControlBag.DescriptionTextBox));
		}
	}

	protected override ControlBag GetControlBagForTesting() => AdditionalInformationFieldsControlBag.Instance;
}
