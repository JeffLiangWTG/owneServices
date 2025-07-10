using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsControlBag))]
	sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsControlBag.PaymentSeparatorUserControl);
				yield return nameof(MiscOptionsControlBag.BankAccountGuidFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
	}
}
