using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExitSummaryMainPanelControlBag))]
	public class ExitSummaryMainPanelControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExitSummaryMainPanelControlBag.BrokerCodeFindBox);
				yield return nameof(ExitSummaryMainPanelControlBag.CertificateDropEdit);
				yield return nameof(ExitSummaryMainPanelControlBag.DeclEmailAddrTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExitSummaryMainPanelControlBag.Instance;
	}
}
