using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(TraderDetailsControlBag))]
	sealed class TraderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TraderDetailsControlBag.BrokerCodeFindBox);
				yield return nameof(TraderDetailsControlBag.CertificateDropEdit);
				yield return nameof(TraderDetailsControlBag.TrainingCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TraderDetailsControlBag.Instance;
	}
}
