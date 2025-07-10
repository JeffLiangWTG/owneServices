using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing;

[TestedType(typeof(MiscOptionsControlBag))]
sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(MiscOptionsControlBag.DefermentAccountNumberDropEdit);
			yield return nameof(MiscOptionsControlBag.VATDeferTypeDropEdit);
			yield return nameof(MiscOptionsControlBag.VatCanaDropEdit);
			yield return nameof(MiscOptionsControlBag.VATDeferNumberTextBox);
			yield return nameof(MiscOptionsControlBag.ChargePaymentOrDestinationIDsDropEdit);
			yield return nameof(MiscOptionsControlBag.CustomsGuaranteeNumberDropEdit);
			yield return nameof(MiscOptionsControlBag.PaymentSeparatorUserControl);
			yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
}
