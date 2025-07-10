using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MiscOptionsControlBag))]
sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(MiscOptionsControlBag.PreClearingCheckBox);
			yield return nameof(MiscOptionsControlBag.BadgeCodeDropEdit);
			yield return nameof(MiscOptionsControlBag.SubscriberDropEdit);
			yield return nameof(MiscOptionsControlBag.DefermentAccountNumberDropEdit);
			yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
}
