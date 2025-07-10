using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(ExitSummaryMainPanelControlBag))]
	public class ExitSummaryMainPanelControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ExitSummaryMainPanelControlBag.ReferenceNumberTextBox);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderCustomsOfficeCodeFindBox);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderArrivalNotificationDateDateEdit);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderArrivalNotificationPlaceTextBox);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderExitDateDateEdit);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderTransportIdTextBox);
				yield return nameof(ExitSummaryMainPanelControlBag.AgentOrgAddressControl);
				yield return nameof(ExitSummaryMainPanelControlBag.HeaderCarrierOrgAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExitSummaryMainPanelControlBag.Instance;
	}
}
