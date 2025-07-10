using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(MiscOptionsControlBag))]
	sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsControlBag.TrainingCheckBox);
				yield return nameof(MiscOptionsControlBag.ShipmentTypeDropEdit);
				yield return nameof(MiscOptionsControlBag.RouteFRequestedCheckBox);
				yield return nameof(MiscOptionsControlBag.LCPDepartDateEdit);
				yield return nameof(MiscOptionsControlBag.LCPInspectDateEdit);
				yield return nameof(MiscOptionsControlBag.RelatedDeclarationsUserControl);
				yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
				yield return nameof(MiscOptionsControlBag.PaymentMethodDropEdit);
				yield return nameof(MiscOptionsControlBag.DeferralSeparatorUserControl);
				yield return nameof(MiscOptionsControlBag.ItineraryCountriesUserControl);
				yield return nameof(MiscOptionsControlBag.ItineraryCountriesSeparatorUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
	}
}
