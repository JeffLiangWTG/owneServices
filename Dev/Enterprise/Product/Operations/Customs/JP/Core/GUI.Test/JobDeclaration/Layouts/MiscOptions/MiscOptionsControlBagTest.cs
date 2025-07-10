using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MiscOptionsControlBag))]
	sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(MiscOptionsControlBag.NACCSCredentialGuidDropEdit),
					nameof(MiscOptionsControlBag.PaymentOptionsSeparatorUserControl),
					nameof(MiscOptionsControlBag.PaymentPartyDropEdit),
					nameof(MiscOptionsControlBag.PaymentDeadlineExtensionDropEdit),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
	}
}
